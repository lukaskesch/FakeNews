﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace FakeNews
{
    public partial class MainWindow : Window
    {
        private int VertexCount;
        private int EdgeCount;
        private int[][] AdjacencyList;
        private int[] Vertices;
        private bool[] WatchedVertices;
        static private int[] Degrees;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
            Stopwatch Timer = new Stopwatch();
            Timer.Start();
            DetermineWatchedVertecies();
            Timer.Stop();
            MessageBox.Show("Berechnet in: " + Timer.Elapsed);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private bool SaveFile()
        {
            SaveFileDialog Dlg = new SaveFileDialog()
            {
                Filter = "Solution Files (*.sol)|*.sol|All files (*.*)|*.*",
                DefaultExt = ".sol",
                FilterIndex = 0
            };

            if (Dlg.ShowDialog() != true)
            {
                return false;
            }

            string SaveFileName = Dlg.FileName;
            FileStream File = new FileStream(Dlg.FileName, FileMode.OpenOrCreate);
            StreamWriter Writer = new StreamWriter(File);

            try
            {
                string Output = string.Empty;
                int WatchedVerticesCount = 0;
                for (int I = 0; I < VertexCount; I++)
                {
                    if (WatchedVertices[I])
                    {
                        WatchedVerticesCount++;
                        Output += I.ToString() + " ";
                    }
                }

                Writer.WriteLine(WatchedVerticesCount.ToString());
                Writer.WriteLine(Output.TrimEnd());
            }
            catch
            {
                MessageBox.Show("Die Datei konnte nicht gespeichert werden!");
                return false;
            }
            finally
            {
                Writer.Flush();
                Writer.Close();
                File.Close();
            }

            return true;
        }

        private void DetermineWatchedVertecies()
        {
            while(true)
            {
                int Index = GetBestVertex();
                if(Index == -1)
                {
                    break;
                }

                WatchedVertices[Index] = true;
                Degrees[Index] = 0;
                for (int I = 0; I < AdjacencyList[Index].Length; I++)
                {
                    Degrees[AdjacencyList[Index][I]]--;
                }

            }
        }

        private bool OpenFile()
        {
            // Open file dialog
            OpenFileDialog Dlg = new OpenFileDialog()
            {
                Filter = "Graph Files (*.graph)|*.graph|All files (*.*)|*.*",
                FilterIndex = 0
            };

            // Return if user cancels selection
            if (Dlg.ShowDialog() != true)
            {
                return false;
            }

            // Read file contents
            string FileName = Dlg.FileName;
            FileStream File = new FileStream(Dlg.FileName, FileMode.Open);
            string FileString = String.Empty;
            StreamReader Reader = new StreamReader(File);

            try
            {
                FileString = Reader.ReadToEnd();
            }
            catch
            {
                MessageBox.Show("Die Datei konnte nicht eingelesen werden!");
                return false;
            }
            finally
            {
                File.Close();
                Reader.Close();
            }

            // Split file into rows
            string[] Rows = FileString.Split('\n');

            try
            {
                // Parse file
                string[] Substrings = Rows[0].Split(' ');
                VertexCount = int.Parse(Substrings[2]);
                EdgeCount = int.Parse(Substrings[3]);

                List<int>[] _AdjacencyList = new List<int>[VertexCount];
                for (int I = 0; I < VertexCount; I++)
                {
                    _AdjacencyList[I] = new List<int>();
                }

                for (int I = 0; I < EdgeCount; I++)
                {
                    string[] EdgeSubstrings = Rows[I + 1].Split(' ');
                    int U = int.Parse(EdgeSubstrings[0]) - 1;
                    int V = int.Parse(EdgeSubstrings[1]) - 1;
                    _AdjacencyList[U].Add(V);
                    _AdjacencyList[V].Add(U);
                }

                AdjacencyList = new int[VertexCount][];
                Vertices = new int[VertexCount];
                Degrees = new int[VertexCount];
                WatchedVertices = new bool[VertexCount];
                for (int I = 0; I < VertexCount; I++)
                {
                    int Count = _AdjacencyList[I].Count;

                    Vertices[I] = I;
                    Degrees[I] = Count;
                    WatchedVertices[I] = false;

                    AdjacencyList[I] = new int[Count];
                    for (int J = 0; J < Count; J++)
                    {
                        AdjacencyList[I][J] = _AdjacencyList[I][J];
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Die Datei konnte nicht verarbeitet werden!");
                return false;
            }
            return true;
        }

        /*
        private int GetBestVertex()
        {
            int BestDegree = 0;
            for(int I = 0; I < VertexCount; I++)
            {
                if(Degrees[I] > BestDegree)
                {
                    BestDegree = Degrees[I];
                }
            }

            List<int> BestIndices = new List<int>();
            for (int I = 0; I < VertexCount; I++)
            {
                if (Degrees[I] == BestDegree)
                {
                    BestIndices.Add(I);
                }
            }

            if(BestIndices.Count < 1)
            {
                return -1;
            }
            else if(BestIndices.Count == 1)
            {
                return BestIndices[0];
            }

            int[] DegreeSums = new int[BestIndices.Count];
            for(int I = 0; I < BestIndices.Count; I++)
            {
                int Sum = 0;
                for (int J = 0; J < AdjacencyList[BestIndices[I]].Length; J++)
                {
                    Sum += Degrees[AdjacencyList[BestIndices[I]][J]];
                }
                DegreeSums[I] = Sum;
            }

            int BestIndex = -1;
            int BestSum = 0;
            for (int I = 0; I < BestIndices.Count; I++)
            {
                if (DegreeSums[I] > BestSum)
                {
                    BestSum = DegreeSums[I];
                    BestIndex = BestIndices[I];
                }
            }

            return BestIndex;
        }
        */
        
        private int GetBestVertex()
        {
            int BestDegree = 0;
            int BestIndex = -1;
            for (int I = 0; I < VertexCount; I++)
            {
                if (Degrees[I] > BestDegree)
                {
                    BestIndex = I;
                    BestDegree = Degrees[I];
                }
            }

            return BestIndex;
        }
        

        public struct Vertex
        {
            public int Index;
            public int UnwatchedDegree;

            public Vertex(int _Index, int _UnwatchedDegree)
            {
                Index = _Index;
                UnwatchedDegree = _UnwatchedDegree;
            }
        }

        /*
        public class DegreeComparer : IComparer<int>
        {
            int IComparer<int>.Compare(int x, int y)
            {
                if(Degrees[x] > Degrees[y])
                {
                    return -1;
                }
                if (Degrees[x] < Degrees[y])
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        */
        public string GetResult()
        {
            string Line1 = string.Empty;
            string Line2 = string.Empty;
            int NumberWatched = 0;
            
            for (int i = 0; i < WatchedVertices.Length; i++)
            {
                if (WatchedVertices[i])
                {
                    NumberWatched++;
                    Line2 += " " + i;
                }
            }
            Line1 = NumberWatched.ToString() + "/n";
            return Line1 + Line2;
        }
    }
}
