﻿using System;
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

namespace GUIwpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public TspGraph TspGraph { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadGraph();
        }

        private void LoadGraph()
        {
            
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            TspGraph?.Draw(canvasTsp);
        }

        private void OpenTspFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TspGraph = new TspGraph(openFileDialog.FileName);
                TspGraph.Draw(canvasTsp);
            }
        }
    }
}