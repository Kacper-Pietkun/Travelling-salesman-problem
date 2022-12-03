using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TspAlgorithms;

namespace TasksCalculations
{
    public class Calculator
    {
        private int _numberOfTasks;
        private int _pmxTime;
        private int _threeOptTime;
        private int _maxEpochs;
        private TspGraph _tspGraph;
        private NamedPipeClientStream pipeRequests;
        private NamedPipeServerStream pipeData;
        private object _lockSendBestGraph = new object();
        private TspGraph globalBestGraph = null;
        private Random random = new Random();

        public Calculator()
        {
            pipeRequests = new NamedPipeClientStream(".", "TspPipeRequests", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            pipeData = new NamedPipeServerStream("TspPipeData", PipeDirection.InOut, 2);
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
                for (int i = 0; i < bestGraphs.Count; i++)
                    bestGraphs[i] = new TspGraph(bestGraphs[i]);



                List<TspGraph> bestPmxGraphes = new List<TspGraph>();
                Task[] pmxTasksArray = new Task[_numberOfTasks];

                ss.WriteString("StartPmx");
                for (int i = 0; i < pmxTasksArray.Length; i++)
                {
                    int temp = i;
                    pmxTasksArray[temp] = Task.Factory.StartNew((Object obj) =>
                    {
                        Pmx pmx = obj as Pmx;

                        (TspGraph first, TspGraph second) = PmxThread(pmx, ss, temp);
                        lock (_lockSendBestGraph)
                        {
                            bestPmxGraphes.Add(first);
                            bestPmxGraphes.Add(second);
                        }
                    },
                    new Pmx(bestGraphs[2* temp], bestGraphs[2* temp + 1], bestGraphs[2*temp].Nodes.Count / 2));
                }
                Task.WaitAll(pmxTasksArray);
                bestPmxGraphes.Sort((g1, g2) => g2.PathLength.CompareTo(g1.PathLength));

                bestPmxGraphes.RemoveRange(0, _numberOfTasks);


                List<TspGraph> bestThreeOptGraphes = new List<TspGraph>();
                Task[] threeOptTasksArray = new Task[_numberOfTasks];
                ss.WriteString("StartThreeOpt");
                for (int i = 0; i < threeOptTasksArray.Length; i++)
                {
                    threeOptTasksArray[i] = Task.Factory.StartNew((Object obj) =>
                    {
                        ThreeOpt threeOpt = obj as ThreeOpt;
                        TspGraph best3opt = ThreeOptThread(threeOpt, ss);
                        lock (_lockSendBestGraph)
                        {
                            bestThreeOptGraphes.Add(best3opt);
                        }
                    },
                    new ThreeOpt(bestPmxGraphes[i]));
                }
                Task.WaitAll(threeOptTasksArray);
                bestThreeOptGraphes.Sort((g1, g2) => g2.PathLength.CompareTo(g1.PathLength));
    
                bestThreeOptGraphes.RemoveRange(0, _numberOfTasks/2);


                bestGraphs.Sort((g1, g2) => g2.PathLength.CompareTo(g1.PathLength));

                bestGraphs.RemoveRange(0, _numberOfTasks);
                bestThreeOptGraphes.AddRange(bestGraphs);
                bestGraphs = new List<TspGraph>();
                for (int i = 0; i < 2 * _numberOfTasks; i++)
                    bestGraphs.Add(new TspGraph(bestThreeOptGraphes[i%bestThreeOptGraphes.Count]));
                PermutateGraphList(bestGraphs);
                _maxEpochs--;
            }
            pipeData.Close();
            ss.WriteString("EOS");
        }

        private TspGraph ThreeOptThread(ThreeOpt threeOpt, StreamString ss)
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
            }
            return threeOpt.BestGraph;
        }

        private (TspGraph, TspGraph) PmxThread(Pmx pmx, StreamString ss, int iii)
        {
            TspGraph bestGraph1 = new TspGraph(pmx.FirstGraph);
            TspGraph bestGraph2 = new TspGraph(pmx.SecondGraph);
            int counter = 1000;
            while(counter-- != 0)
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
                    if (IsGraphGood(pmx.ResultingGraph) == false)
                        Console.WriteLine("false");

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
                message = ss.ReadString();
                // TODO: process message from another process
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
            }
            catch (FormatException e)
            {
                pipeRequests.Close();
                return;
            }
        }
    }
}
