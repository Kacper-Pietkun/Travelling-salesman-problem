using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TspAlgorithms;
using System.ComponentModel;
using System.Windows.Controls;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Principal;

namespace GUIwpf
{
    public class ProcessesManager
    {
        private NamedPipeServerStream pipeRequests;
        private NamedPipeClientStream pipeData;
        private readonly BackgroundWorker workerRequests;
        private readonly BackgroundWorker workerData;
        private int _numberOfTasks;
        private int _pmxTime;
        private int _threeOptTime;
        private TspGraph _tspGraph;

        public ProcessesManager()
        {
            workerRequests = new BackgroundWorker();
            workerRequests.DoWork += SendRequests;
            workerData = new BackgroundWorker();
            workerData.DoWork += GatherGraphData;
            pipeRequests = new NamedPipeServerStream("TspPipeRequests", PipeDirection.InOut, 2);
            pipeData = new NamedPipeClientStream(".", "TspPipeData", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
        }

        public void StartTasks(int numberOfTasks, int pmxTime, int threeOptTime, TspGraph tspGraph)
        {
            _numberOfTasks = numberOfTasks;
            _pmxTime = pmxTime;
            _threeOptTime = threeOptTime;
            _tspGraph = tspGraph;
            Process.Start("TasksCalculations.exe");
            workerRequests.RunWorkerAsync();
            workerData.RunWorkerAsync();
        }

        public void StartThreads(int numberOfThreads, int pmxTime, int threeOptTime, TspGraph tspGraph)
        {
            _numberOfTasks = numberOfThreads;
            _pmxTime = pmxTime;
            _threeOptTime = threeOptTime;
            _tspGraph = tspGraph;
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
                message = ss.ReadString();
                // TODO: process message from another process
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

                byte[] tspGraphBytes;
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream())
                {
                    bf.Serialize(stream, _tspGraph);
                    tspGraphBytes = stream.ToArray();
                }
                string msg = Encoding.GetEncoding("ISO-8859-1").GetString(tspGraphBytes);
                ss.WriteString(msg);

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
    }
}
