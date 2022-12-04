using GUIwpf;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TspAlgorithms;

namespace ThreadsCalculations
{
    public class Calculator
    {
        private int _numberOfTasks;
        private int _pmxTime;
        private bool _isPmxInProgress;
        private int _threeOptTime;
        private bool _isThreeOptProgress;
        private int _maxEpochs;
        private TspGraph _tspGraph;
        private NamedPipeClientStream pipeRequests;
        private NamedPipeServerStream pipeData;
        private object _lockSendBestGraph = new object();
        private object _pauseMonitorLock = new object();
        private TspGraph globalBestGraph = null;
        private Random random = new Random();
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private CommandResource commandResource;
        private bool _threadException;

        public Calculator()
        {
            pipeRequests = new NamedPipeClientStream(".", "TspPipeRequests", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            pipeData = new NamedPipeServerStream("TspPipeData", PipeDirection.InOut, 2);
            commandResource = new CommandResource();
        }

        public void Calculate()
        {
            ConfigurePipelines();
            PrepareDataForCalculations();
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(() => ManageProcessesRequests()));
            tasks.Add(Task.Factory.StartNew(() => DoCalculations()));
            Task.WaitAll(tasks.ToArray());
        }

        public void DoCalculations()
        {
            StreamString ss = new StreamString(pipeData);
            globalBestGraph = new TspGraph(_tspGraph, true);

            List<TspGraph> bestGraphs = new List<TspGraph>();
            for (int i = 0; i < _numberOfTasks; i++)
            {
                bestGraphs.Add(new TspGraph(_tspGraph, true));
                bestGraphs.Add(new TspGraph(_tspGraph, true));
            }
            while (_maxEpochs != 0)
            {
                _threadException = false;
                if (_tokenSource == null)
                {
                    _tokenSource = new CancellationTokenSource();
                    _token = _tokenSource.Token;
                }


                for (int i = 0; i < bestGraphs.Count; i++)
                    bestGraphs[i] = new TspGraph(bestGraphs[i]);

                try
                {
                    List<TspGraph> bestPmxGraphes = new List<TspGraph>();
                    ss.WriteString("StartPmx");
                    _isPmxInProgress = true;
                    CountdownEvent endOfWork = new CountdownEvent(_numberOfTasks);
                    for (int i = 0; i < _numberOfTasks; i++)
                    {
                        int temp = i;
                        ThreadPool.QueueUserWorkItem(obj =>
                        {
                            Pmx pmx = obj as Pmx;
                            try
                            {
                                (TspGraph first, TspGraph second) = PmxThread(pmx, ss, _token);
                                lock (_lockSendBestGraph)
                                {
                                    bestPmxGraphes.Add(first);
                                    bestPmxGraphes.Add(second);
                                }
                            }
                            catch (Exception ex)
                            {
                                _threadException = true;
                            }
                            endOfWork.Signal();
                        }, new Pmx(bestGraphs[2 * temp], bestGraphs[2 * temp + 1], bestGraphs[2 * temp].Nodes.Count / 2));
                    }
                    System.Timers.Timer pmxTimer = new System.Timers.Timer(_pmxTime * 1000);
                    pmxTimer.Elapsed += (Object source, ElapsedEventArgs e) => _isPmxInProgress = false;
                    pmxTimer.AutoReset = false;
                    pmxTimer.Enabled = true;
                    endOfWork.Wait();
                    if (_threadException)
                        throw new AggregateException();
                    _isPmxInProgress = false;
                    bestPmxGraphes.Sort((g1, g2) => g2.PathLength.CompareTo(g1.PathLength));
                    bestPmxGraphes.RemoveRange(0, _numberOfTasks);


                    List<TspGraph> bestThreeOptGraphes = new List<TspGraph>();
                    ss.WriteString("StartThreeOpt");
                    _isThreeOptProgress = true;
                    endOfWork = new CountdownEvent(_numberOfTasks);
                    for (int i = 0; i < _numberOfTasks; i++)
                    {
                        int temp = i;
                        ThreadPool.QueueUserWorkItem(obj =>
                        {
                            try
                            { 
                                ThreeOpt threeOpt = obj as ThreeOpt;
                                TspGraph best3opt = ThreeOptThread(threeOpt, ss, _token);
                                lock (_lockSendBestGraph)
                                {
                                    bestThreeOptGraphes.Add(best3opt);
                                }
                            }
                            catch (Exception ex)
                            {
                                _threadException = true;
                            }

                            endOfWork.Signal();
                        }, new ThreeOpt(bestPmxGraphes[temp]));
                    }
                    System.Timers.Timer threeOptTimer = new System.Timers.Timer(_threeOptTime * 1000);
                    threeOptTimer.Elapsed += (Object source, ElapsedEventArgs e) => _isThreeOptProgress = false;
                    threeOptTimer.AutoReset = false;
                    threeOptTimer.Enabled = true;
                    endOfWork.Wait();
                    if (_threadException)
                        throw new AggregateException();
                    _isThreeOptProgress = false;
                    bestThreeOptGraphes.Sort((g1, g2) => g2.PathLength.CompareTo(g1.PathLength));
                    bestThreeOptGraphes.RemoveRange(0, _numberOfTasks / 2);


                    bestGraphs.Sort((g1, g2) => g2.PathLength.CompareTo(g1.PathLength));
                    bestGraphs.RemoveRange(0, _numberOfTasks);
                    bestThreeOptGraphes.AddRange(bestGraphs);
                    bestGraphs = new List<TspGraph>();
                    for (int i = 0; i < 2 * _numberOfTasks; i++)
                        bestGraphs.Add(new TspGraph(bestThreeOptGraphes[i % bestThreeOptGraphes.Count]));
                    PermutateGraphList(bestGraphs);
                    _maxEpochs--;
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException || ex is AggregateException)
                    {
                        _tokenSource = null;
                        ss.WriteString("Paused");
                        commandResource.GetCommand();
                        ss.WriteString("Resumed");
                    }
                }
            }
            ss.WriteString("EOS");
            pipeData.Close();
            if (_tokenSource != null)
                _tokenSource.Dispose();
        }

        private TspGraph ThreeOptThread(ThreeOpt threeOpt, StreamString ss, CancellationToken token)
        {
            TspGraph bestGraph = new TspGraph(threeOpt.BestGraph);
            foreach (int i in threeOpt.Start())
            {
                lock (_lockSendBestGraph)
                {
                    if (globalBestGraph.PathLength > threeOpt.BestGraph.PathLength)
                    {
                        globalBestGraph = new TspGraph(threeOpt.BestGraph);
                        ss.WriteString("Graph");
                        ss.WriteString(globalBestGraph.GetFormattedGraph());
                        ss.WriteString("ThreadId");
                        ss.WriteString(Thread.CurrentThread.ManagedThreadId.ToString());
                    }
                }
                if (_isThreeOptProgress == false)
                    break;
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
            }
            return threeOpt.BestGraph;
        }

        private (TspGraph, TspGraph) PmxThread(Pmx pmx, StreamString ss, CancellationToken token)
        {
            TspGraph bestGraph1 = new TspGraph(pmx.FirstGraph);
            TspGraph bestGraph2 = new TspGraph(pmx.SecondGraph);
            while (_isPmxInProgress == true)
            {
                for (int i = 0; i < 1; i++)
                {
                    pmx.SwapParents();
                    pmx.Start();

                    if (bestGraph1.PathLength > pmx.ResultingGraph.PathLength && IsGraphGood(pmx.ResultingGraph))
                    {
                        bestGraph2.Nodes = bestGraph1.Nodes;
                        bestGraph2.PathLength = bestGraph1.PathLength;
                        bestGraph1 = new TspGraph(pmx.ResultingGraph);
                    }
                    else if (bestGraph2.PathLength > pmx.ResultingGraph.PathLength && IsGraphGood(pmx.ResultingGraph))
                        bestGraph2 = new TspGraph(pmx.ResultingGraph);

                    lock (_lockSendBestGraph)
                    {

                        if (globalBestGraph.PathLength > bestGraph1.PathLength)
                        {
                            globalBestGraph = new TspGraph(bestGraph1);
                            ss.WriteString("Graph");
                            ss.WriteString(globalBestGraph.GetFormattedGraph());
                            ss.WriteString("ThreadId");
                            ss.WriteString(Thread.CurrentThread.ManagedThreadId.ToString());
                        }
                        else if (globalBestGraph.PathLength > bestGraph2.PathLength)
                        {
                            globalBestGraph = new TspGraph(bestGraph2);
                            ss.WriteString("Graph");
                            ss.WriteString(globalBestGraph.GetFormattedGraph());
                            ss.WriteString("ThreadId");
                            ss.WriteString(Thread.CurrentThread.ManagedThreadId.ToString());
                        }
                    }
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();
                }
            }
            return (bestGraph1, bestGraph2);
        }

        private bool IsGraphGood(TspGraph graph)
        {
            Dictionary<int, bool> dict = new Dictionary<int, bool>();
            for (int i = 1; i <= 29; i++)
                dict.Add(i, false);
            foreach (var el in graph.Nodes)
            {
                dict[el.Item1] = true;
            }
            for (int i = 1; i <= 29; i++)
                if (dict[i] == false)
                    return false;
            return true;
        }

        private void PermutateGraphList(List<TspGraph> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int index = random.Next(list.Count);
                TspGraph temp = list[index];
                list[index] = list[i];
                list[i] = temp;
            }
        }

        private void ManageProcessesRequests()
        {
            StreamString ss = new StreamString(pipeRequests);
            string message = ss.ReadString();
            while (message != "EOS")
            {
                switch (message)
                {
                    case "Pause":
                        if (_tokenSource != null)
                            _tokenSource.Cancel();
                        break;
                    case "Resume":
                        commandResource.SetCommand("Resume");
                        break;
                }
                message = ss.ReadString();
            }
            pipeRequests.Close();
        }

        private void ConfigurePipelines()
        {
            pipeRequests.Connect();
            pipeData.WaitForConnection();
        }

        private void PrepareDataForCalculations()
        {
            StreamString ss = new StreamString(pipeRequests);
            try
            {
                _numberOfTasks = int.Parse(ss.ReadString());
                _pmxTime = int.Parse(ss.ReadString());
                _threeOptTime = int.Parse(ss.ReadString());
                _maxEpochs = int.Parse(ss.ReadString());
                _tspGraph = new TspGraph(ss.ReadString(), true);
                _tspGraph.PermutateNodes();
                _isPmxInProgress = false;
                _isThreeOptProgress = false;
            }
            catch (FormatException e)
            {
                pipeRequests.Close();
                return;
            }
        }
    }
}
