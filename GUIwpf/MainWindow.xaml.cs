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

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimeUnitComboBox(comboBoxPmxUnit);
            InitializeTimeUnitComboBox(comboBox3optUnit);
            UiGraphManager = new UiGraphManager(canvasTsp);
            ProcessesManager = new ProcessesManager();
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TspGraph != null)
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
            int pmxTime = Int32.Parse(TextBoxPmxTime.Text);
            if (comboBoxPmxUnit.Text == "min")
                pmxTime *= 60;
            int threeOptTime = Int32.Parse(TextBox3optTime.Text);
            if (comboBox3optUnit.Text == "min")
                threeOptTime *= 60;

            if (TspGraph == null)
            {
                MessageBox.Show("You need to load a graph first");
                return;
            }

            if (radioButtonTasks.IsChecked == true)
            {
                int numberOfTasks = Int32.Parse(TextBoxTasksNumber.Text);
                ProcessesManager.StartTasks(numberOfTasks, pmxTime, threeOptTime, TspGraph);
            }
            else
            {
                int numberOfThreads = Int32.Parse(TextBoxTasksNumber.Text);
                ProcessesManager.StartThreads(numberOfThreads, pmxTime, threeOptTime, TspGraph);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {

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
    }
}