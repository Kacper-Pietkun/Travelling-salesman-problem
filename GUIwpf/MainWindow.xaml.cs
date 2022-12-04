using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Drawing;
using Color = System.Windows.Media.Color;
using System.Xml.Linq;
using Size = System.Windows.Size;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.ComponentModel;
using TspAlgorithms;
using System.IO.Pipes;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace GUIwpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public TspGraph? TspGraph { get; set; }
        public UiGraphManager UiGraphManager { get; set; }
        public ProcessesManager ProcessesManager { get; set; }
        private Task canvasTask;
        private CommandResource CommandResource { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimeUnitComboBox(comboBoxPmxUnit);
            InitializeTimeUnitComboBox(comboBox3optUnit);
            UiGraphManager = new UiGraphManager(canvasTsp);
            CommandResource = new CommandResource();
            ProcessesManager = new ProcessesManager();
            this.DataContext = this;
            ProcessesManager.Epoch = "...";
            ProcessesManager.BestGraphThreadId = "...";
            ProcessesManager.BestScore = "...";
            ProcessesManager.ShouldStartButtonBeEnabled = true;
            ProcessesManager.ShouldOpenButtonBeEnabled = true;
            ProcessesManager.ShouldExitButtonBeEnabled = true;
            ProcessesManager.ButtonStartContent = "Start";
            canvasTask = Task.Factory.StartNew(() => UpdateCanvas());
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UiGraphManager.Draw(TspGraph);
        }

        private void OpenTspFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TspGraph = new TspGraph(openFileDialog.FileName);
                UiGraphManager.Draw(TspGraph);
            }
        }

        private void StartCalculations_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessesManager.ButtonStartContent == "Start")
            {
                if (TspGraph == null)
                {
                    MessageBox.Show("You need to load a graph first");
                    return;
                }
                ProcessesManager.ShouldStartButtonBeEnabled = false;
                ProcessesManager.ShouldOpenButtonBeEnabled = false;
                ProcessesManager.ShouldExitButtonBeEnabled = false;
                ProcessesManager.ButtonStartContent = "Pause";
                ProcessesManager.PhaseCounter = 0;

                string strPmxTime = TextBoxPmxTime.Text;
                strPmxTime = strPmxTime.Replace('.', ',');
                float pmxTime = float.Parse(strPmxTime);
                if (comboBoxPmxUnit.Text == "min")
                    pmxTime *= 60;
                string strThreeOptTime = TextBox3optTime.Text;
                strThreeOptTime = strThreeOptTime.Replace('.', ',');
                float threeOptTime = float.Parse(strThreeOptTime);
                if (comboBox3optUnit.Text == "min")
                    threeOptTime *= 60;
                int maxEpochs = Int32.Parse(TextBoxMaxEpochs.Text);
                
                if (radioButtonTasks.IsChecked == true)
                {
                    int numberOfTasks = Int32.Parse(TextBoxTasksNumber.Text);
                    ProcessesManager.StartTasks(numberOfTasks, pmxTime, threeOptTime, maxEpochs, TspGraph, CommandResource);
                }
                else
                {
                    int numberOfThreads = Int32.Parse(TextBoxTasksNumber.Text);
                    ProcessesManager.StartThreads(numberOfThreads, pmxTime, threeOptTime, maxEpochs, TspGraph, CommandResource);
                }
                
            }
            else if (ProcessesManager.ButtonStartContent == "Pause")
            {
                ProcessesManager.ShouldStartButtonBeEnabled = false;
                ProcessesManager.ButtonStartContent = "Resume";
                CommandResource.SetCommand("Pause");
            }
            else if (ProcessesManager.ButtonStartContent == "Resume")
            {
                ProcessesManager.ShouldStartButtonBeEnabled = false;
                ProcessesManager.ButtonStartContent = "Pause";
                CommandResource.SetCommand("Resume");
            }

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void InitializeTimeUnitComboBox(ComboBox comboBox)
        {
            comboBox.ItemsSource = new BindingList<String>(new string[]
            {
                "s",
                "min"
            });
            comboBox.SelectedIndex = 0;
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void UpdateCanvas()
        {
            while (true)
            {
                if (ProcessesManager.ButtonStartContent != "Start")
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate {
                        UiGraphManager.Draw(ProcessesManager.BestGraph);
                    });
                }

                Thread.Sleep(50);
            }
        }

    }
}