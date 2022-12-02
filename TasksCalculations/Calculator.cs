using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using TspAlgorithms;

namespace TasksCalculations
{
    public class Calculator
    {
        private int _numberOfTasks;
        private int _pmxTime;
        private int _threeOptTime;
        private TspGraph _tspGraph;
        private NamedPipeClientStream pipeRequests;
        private NamedPipeServerStream pipeData;

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

            TspGraph firstParent = new TspGraph(_tspGraph);
            TspGraph secondParent = new TspGraph(_tspGraph);
            firstParent.PermutateNodes();
            secondParent.PermutateNodes();
            while (true)
            {
                secondParent.PermutateNodes();
                Pmx pmx = new Pmx(firstParent, secondParent, firstParent.Nodes.Count / 2);
                pmx.Start();
                TspGraph resultPmx = pmx.ResultingGraph;

                ThreeOpt threeOpt = new ThreeOpt(resultPmx);
                threeOpt.Start();
                TspGraph resultThreeOpt = threeOpt.BestGraph;

                ss.WriteString(resultThreeOpt.GetFormattedGraph());
                firstParent = new TspGraph(resultThreeOpt);
                secondParent = new TspGraph(resultThreeOpt);
            }

            pipeData.Close();
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
                _tspGraph = new TspGraph(ss.ReadString(), true);
            }
            catch (FormatException e)
            {
                pipeRequests.Close();
                return;
            }
        }
    }
}
