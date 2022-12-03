using System;
using System.IO.Pipes;
using System.IO;
using TspAlgorithms;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace GUIwpf
{
    public class ProcessesManager : INotifyPropertyChanged
    {
        private NamedPipeServerStream pipeRequests;
        private NamedPipeClientStream pipeData;
        private readonly BackgroundWorker workerRequests;
        private readonly BackgroundWorker workerData;
        private int _numberOfTasks;
        private int _pmxTime;
        private int _threeOptTime;
        private TspGraph _tspGraph;
        public TspGraph BestGraph { get; set; }

        private string _bestScore;
        public string BestScore
        {
            get
            {
                return _bestScore;
            }
            set
            {
                if (_bestScore != value)
                {
                    _bestScore = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _bestGraphThreadId;
        public string BestGraphThreadId
        {
            get
            {
                return _bestGraphThreadId;
            }
            set
            {
                if (_bestGraphThreadId != value)
                {
                    _bestGraphThreadId = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _phaseCounter;
        public int PhaseCounter
        {
            get
            {
                return _phaseCounter;
            }
            set
            {
                if (_phaseCounter != value)
                {
                    _phaseCounter = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _currentPhase;
        private string _epoch;
        public string Epoch
        {
            get
            {
                return _epoch;
            }
            set
            {
                if (_epoch != value)
                {
                    _epoch = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _maxEpochs;
        public int MaxEpochs
        {
            get
            {
                return _maxEpochs;
            }
            set
            {
                if (_maxEpochs != value)
                {
                    _maxEpochs = value;
                    OnPropertyChanged();
                }
            }
        }


        public ProcessesManager()
        {
            workerRequests = new BackgroundWorker();
            workerRequests.DoWork += SendRequests;
            workerData = new BackgroundWorker();
            workerData.DoWork += GatherGraphData;
            pipeRequests = new NamedPipeServerStream("TspPipeRequests", PipeDirection.InOut, 2);
            pipeData = new NamedPipeClientStream(".", "TspPipeData", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            PhaseCounter = 0;
        }

        public void StartTasks(int numberOfTasks, int pmxTime, int threeOptTime, int maxEpochs, TspGraph tspGraph)
        {
            _numberOfTasks = numberOfTasks;
            _pmxTime = pmxTime;
            _threeOptTime = threeOptTime;
            _tspGraph = tspGraph;
            MaxEpochs = maxEpochs;
            BestGraph = tspGraph;
            Process.Start("TasksCalculations.exe");
            workerRequests.RunWorkerAsync();
            workerData.RunWorkerAsync();
        }

        public void StartThreads(int numberOfThreads, int pmxTime, int threeOptTime, int maxEpochs, TspGraph tspGraph)
        {
            _numberOfTasks = numberOfThreads;
            _pmxTime = pmxTime;
            _threeOptTime = threeOptTime;
            _tspGraph = tspGraph;
            MaxEpochs = maxEpochs;
            Process.Start("ThreadsCalculations.exe");
            workerRequests.RunWorkerAsync();
            workerData.RunWorkerAsync();
        }

        public void GatherGraphData(object? sender, DoWorkEventArgs e)
        {
            pipeData.Connect();
            StreamString ss = new StreamString(pipeData);
            string message = ss.ReadString();
            while (message != "EOS")
            {
                switch (message)
                {
                    case "StartPmx":
                        PhaseCounter++;
                        _currentPhase = "Pmx";
                        Epoch = _currentPhase + ": " + PhaseCounter;
                        break;
                    case "StartThreeOpt":
                        _currentPhase = "3-opt";
                        Epoch = _currentPhase + ": " + PhaseCounter;
                        break;
                    case "Graph":
                        BestGraph = new TspGraph(ss.ReadString(), true);
                        BestScore = BestGraph.PathLength.ToString();
                        break;
                    case "ThreadId":
                        BestGraphThreadId = ss.ReadString();
                        break;
                }
                message = ss.ReadString();
            }

            pipeData.Close();
        }

        public void SendRequests(object? sender, DoWorkEventArgs e)
        {
            pipeRequests.WaitForConnection();

            try
            {
                StreamString ss = new StreamString(pipeRequests);
                ss.WriteString(_numberOfTasks.ToString());
                ss.WriteString(_pmxTime.ToString());
                ss.WriteString(_threeOptTime.ToString());
                ss.WriteString(MaxEpochs.ToString());
                ss.WriteString(_tspGraph.GetFormattedGraph());

                // TODO: delete this busy wait - here we are going to implement user ability to cancel calculations done by another process
                while (true)
                {

                }

            }
            catch (IOException ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }
            pipeRequests.Close();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
