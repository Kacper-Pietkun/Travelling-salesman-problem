using System;
using System.IO.Pipes;
using System.IO;
using TspAlgorithms;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace GUIwpf
{
    public class ProcessesManager : INotifyPropertyChanged
    {
        private NamedPipeServerStream pipeRequests;
        private NamedPipeClientStream pipeData;
        private BackgroundWorker workerRequests;
        private BackgroundWorker workerData;
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

        private bool _shouldStartButtonBeEnabled;
        public bool ShouldStartButtonBeEnabled
        {
            get
            {
                return _shouldStartButtonBeEnabled;
            }
            set
            {
                if (_shouldStartButtonBeEnabled != value)
                {
                    _shouldStartButtonBeEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _buttonStartContent;
        public string ButtonStartContent
        {
            get
            {
                return _buttonStartContent;
            }
            set
            {
                if (_buttonStartContent != value)
                {
                    _buttonStartContent = value;
                    OnPropertyChanged();
                }
            }
        }

        private CommandResource _commandResource;


        private void FirstConfigurations()
        {
            workerRequests = new BackgroundWorker();
            //workerRequests.DoWork += SendRequests;
            workerData = new BackgroundWorker();
            workerData.DoWork += GatherGraphData;
            pipeRequests = new NamedPipeServerStream("TspPipeRequests", PipeDirection.InOut, 2);
            pipeData = new NamedPipeClientStream(".", "TspPipeData", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            PhaseCounter = 0;
        }

        public void StartTasks(int numberOfTasks, int pmxTime, int threeOptTime, int maxEpochs, TspGraph tspGraph, CommandResource commandResource)
        {
            FirstConfigurations();
            _numberOfTasks = numberOfTasks;
            _pmxTime = pmxTime;
            _threeOptTime = threeOptTime;
            _tspGraph = tspGraph;
            MaxEpochs = maxEpochs;
            BestGraph = tspGraph;
            _commandResource = commandResource;
            Process.Start("TasksCalculations.exe");
            //workerRequests.RunWorkerAsync();
            workerData.RunWorkerAsync();
            Task task = Task.Run(() => SendRequests());
        }

        public void StartThreads(int numberOfThreads, int pmxTime, int threeOptTime, int maxEpochs, TspGraph tspGraph, CommandResource commandResource)
        {
            FirstConfigurations();
            _numberOfTasks = numberOfThreads;
            _pmxTime = pmxTime;
            _threeOptTime = threeOptTime;
            _tspGraph = tspGraph;
            MaxEpochs = maxEpochs;
            _commandResource = commandResource;
            Process.Start("ThreadsCalculations.exe");
            //workerRequests.RunWorkerAsync();
            workerData.RunWorkerAsync();
            Task task = Task.Run(() => SendRequests());
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
                    case "Paused":
                        _currentPhase = "3-opt";
                        PhaseCounter--;
                        ShouldStartButtonBeEnabled = true;
                        Epoch = _currentPhase + ": " + PhaseCounter;
                        break;
                    case "Resumed":
                        ShouldStartButtonBeEnabled = true;
                        break;
                }
                message = ss.ReadString();
            }
            _commandResource.SetCommand("EOS");
            ButtonStartContent = "Start";

            pipeData.Close();
        }

        public void SendRequests()
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


                // TODO: Fix busy wait with some syncrhonization method
                ShouldStartButtonBeEnabled = true;
                string command = _commandResource.GetCommand();
                while (command != "EOS")
                {
                    switch (command)
                    {
                        case "Pause":
                            ShouldStartButtonBeEnabled = false;
                            ss.WriteString("Pause");
                            break;
                        case "Resume":
                            ShouldStartButtonBeEnabled = false;
                            ss.WriteString("Resume");
                            break;
                    }
                    command = _commandResource.GetCommand();
                }
                ss.WriteString("EOS");

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
