using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Reflection;
using System.Windows.Media.Media3D;

namespace GrainGrowthUI
{
    class Simulation
    {

        private string number;
        private string myFilePath;
        private SimulationListItem item;

        private int size;
        private int gapSize;

        private MyColors myColors;

        private Label wallsLabel;
        private List<string[,]> walls;
        private Canvas canvas;
        private Image image;

        private MCube3D cube;
        private ScrollViewer scrollViewer;

        public Simulation(string counter, string fileName, string sizeX, string sizeY, string sizeZ, 
                          string neighbourhood, string bc, string numberOfNucleons, string simulation,
                          string numberOfIterations, string kt, string j, 
                          StackPanel simulationPanel, ListView listView) {

            this.myColors = new MyColors();

            this.number = counter;

            int maxSize = Math.Max(Int32.Parse(sizeX), Math.Max(Int32.Parse(sizeY), Int32.Parse(sizeZ)));

            if (maxSize < 25)
            {
                this.size = 20;
                this.gapSize = 1;
            }
            else if (maxSize < 50)
            {
                this.size = 10;
                this.gapSize = 2;
            }
            else if (maxSize < 100)
            {
                this.size = 5;
                this.gapSize = 4;
            }
            else if (maxSize < 250)
            {
                this.size = 2;
                this.gapSize = 6;
            }
            else
            {
                this.size = 1;
                this.gapSize = 10;
            }

            myColors.InitializeCellColors(Int32.Parse(numberOfNucleons) + 2);

            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();


            this.myFilePath = Config.FILE_PATH + fileName + "_" + milliseconds + ".xml";

            this.item = new SimulationListItem
            {
                ID = counter,
                SizeX = sizeX,
                SizeY = sizeY,
                SizeZ = sizeZ,
                Neighbourhood = neighbourhood,
                BC = bc,
                Nucleons = numberOfNucleons,
                Simulation = simulation,
                NumberOfIterations = numberOfIterations,
                KT = kt,
                J = j,
                ProgressValue = 0,
                ProgressBool = true
            };

            listView.Items.Add(item);


            using (XmlWriter writer = XmlWriter.Create(this.myFilePath))
            {

                writer.WriteStartElement("CG_config");
                writer.WriteElementString("FilePath", fileName + ".txt");
                writer.WriteElementString("SizeX", sizeX);
                writer.WriteElementString("SizeY", sizeY);
                writer.WriteElementString("SizeZ", sizeZ);
                writer.WriteElementString("Neighbourhood", neighbourhood);
                writer.WriteElementString("BC", bc);
                writer.WriteElementString("Nucleons", numberOfNucleons);
                writer.WriteElementString("Simulation", simulation);
                writer.WriteElementString("NumberOfIterations", numberOfIterations);
                writer.WriteElementString("KT", kt);
                writer.WriteElementString("J", j);
                writer.WriteEndElement();
                writer.Flush();
            }
        }


        public void Run(string fileName, TabControl tabControl, ListView listView, string isParallel = "")
        {
            Console.WriteLine(fileName);

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;

            image = new Image();

            TabItem ti = new TabItem();

            Viewbox viewbox = new Viewbox();

            Grid grid = new Grid();
            grid.Width = 1200;
            grid.Height = 800;


            ColumnDefinition c1 = new ColumnDefinition();
            ColumnDefinition c2 = new ColumnDefinition();

            c1.Width = new GridLength(4, GridUnitType.Star);
            c2.Width = new GridLength(1, GridUnitType.Star);
            
            grid.ColumnDefinitions.Add(c1);
            grid.ColumnDefinitions.Add(c2);

            scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            Grid.SetColumn(scrollViewer, 0);

            canvas = new Canvas();
            canvas.Margin = new Thickness(10);
            canvas.Background = new SolidColorBrush(Colors.Black);
            canvas.HorizontalAlignment = HorizontalAlignment.Left;
            canvas.VerticalAlignment = VerticalAlignment.Top;

            scrollViewer.Content = canvas;


            Grid rightGrid = new Grid();
            RowDefinition r1 = new RowDefinition();
            RowDefinition r2 = new RowDefinition();

            r1.Height = new GridLength(2, GridUnitType.Star);
            r2.Height = new GridLength(1, GridUnitType.Star);

            rightGrid.RowDefinitions.Add(r1);
            rightGrid.RowDefinitions.Add(r2);

            TextBlock textBlock = new TextBlock();
            textBlock.Margin = new Thickness(10);

            StackPanel containerPanel = new StackPanel();

            StackPanel labelPanel = new StackPanel();
            labelPanel.HorizontalAlignment = HorizontalAlignment.Left;
            Button button3D = new Button();
            button3D.Margin = new Thickness(5);
            button3D.Width = 50;
            button3D.Height = 25;
            button3D.Content = "3D View";
            button3D.Click += new RoutedEventHandler(show3D_Click);

            Button button = new Button();
            button.Margin = new Thickness(5);
            button.Width = 50;
            button.Height = 25;
            button.Content = "Show all";
            button.Click += new RoutedEventHandler(showAllButton_Click);


            labelPanel.Children.Add(button3D);
            labelPanel.Children.Add(button);

            StackPanel buttonPanel = new StackPanel();
            buttonPanel.Orientation = Orientation.Horizontal;

            containerPanel.Children.Add(labelPanel);
            containerPanel.Children.Add(buttonPanel);


            wallsLabel = new Label();
            wallsLabel.Margin = new Thickness(5);
            wallsLabel.Content = "Wall: Left";
            wallsLabel.Visibility = Visibility.Hidden;



            Button bLeft = new Button();
            bLeft.Margin = new Thickness(5);
            bLeft.Width = 50;
            bLeft.Height = 25;
            bLeft.Content = "<-";
            bLeft.Click += new RoutedEventHandler(leftButton_Click);

            Button bRight = new Button();
            bRight.Margin = new Thickness(5);
            bRight.Width = 50;
            bRight.Height = 25;
            bRight.Content = "->";
            bRight.Click += new RoutedEventHandler(rightButton_Click);

            buttonPanel.Children.Add(bLeft);
            buttonPanel.Children.Add(bRight);

            buttonPanel.Children.Add(wallsLabel);

            Grid.SetRow(textBlock, 0);
            Grid.SetRow(containerPanel, 1);


            rightGrid.Children.Add(textBlock);
            rightGrid.Children.Add(containerPanel);

            Grid.SetColumn(rightGrid, 1);

            grid.Children.Add(scrollViewer);
            grid.Children.Add(rightGrid);

            viewbox.Child = grid;

            // Cube3D ----------------------------------
            

            if (Int32.Parse(this.item.SizeX) > 1 && Int32.Parse(this.item.SizeY) > 1 && Int32.Parse(this.item.SizeZ) > 1)
            {
                cube = new MCube3D();
                Grid.SetColumn(cube.mainDock, 0);

                grid.Children.Add(cube.mainDock);

                scrollViewer.Visibility = Visibility.Hidden;
                float x = 1.0f;
                float y = float.Parse(this.item.SizeY) / float.Parse(this.item.SizeX);
                float z = float.Parse(this.item.SizeZ) / float.Parse(this.item.SizeX);

                float biggest = Math.Max(x, Math.Max(y, z));

                if (biggest == y)
                {
                    x = 1.0f / y;
                    z = z / y;
                    y = 1.0f;
                }
                else if (biggest == z)
                {
                    x = 1.0f / z;
                    y = y / z;
                    z = 1.0f;
                }

                var topGeometry3D = cube.Top.Geometry3D as MeshGeometry3D;
                var bottomGeometry3D = cube.Bottom.Geometry3D as MeshGeometry3D;

                var leftpGeometry3D = cube.Left.Geometry3D as MeshGeometry3D;
                var rightGeometry3D = cube.Right.Geometry3D as MeshGeometry3D;

                var frontGeometry3D = cube.Front.Geometry3D as MeshGeometry3D;
                var backGeometry3D = cube.Back.Geometry3D as MeshGeometry3D;

                Point3DCollection positionsTop = topGeometry3D.Positions;
                Point3DCollection positionsBottom = bottomGeometry3D.Positions;

                Point3DCollection positionsLeft = leftpGeometry3D.Positions;
                Point3DCollection positionsRight = rightGeometry3D.Positions;

                Point3DCollection positionsFront = frontGeometry3D.Positions;
                Point3DCollection positionsBack = backGeometry3D.Positions;

                for (var i = 0; i < positionsTop.Count; i++)
                {
                    Point3D positionTop = positionsTop[i];
                    Point3D positionBottom = positionsBottom[i];

                    Point3D positionLeft = positionsLeft[i];
                    Point3D positionRight = positionsRight[i];

                    Point3D positionFront = positionsFront[i];
                    Point3D positionBack = positionsBack[i];


                    positionTop.Z *= z;
                    positionTop.Y = y;
                    positionTop.X *= x;

                    positionBottom.Y = -y;
                    positionBottom.Z *= z;
                    positionBottom.X *= x;


                    positionLeft.Y *= y;
                    positionLeft.Z *= z;
                    positionLeft.X *= x;

                    positionRight.Y *= y;
                    positionRight.Z *= z;
                    positionRight.X *= x;


                    positionFront.Y *= y;
                    positionFront.Z = z;
                    positionFront.X *= x;

                    positionBack.Y *= y;
                    positionBack.Z = -z;
                    positionBack.X *= x;


                    positionsTop[i] = positionTop;
                    positionsBottom[i] = positionBottom;

                    positionsLeft[i] = positionLeft;
                    positionsRight[i] = positionRight;

                    positionsFront[i] = positionFront;
                    positionsBack[i] = positionBack;
                }

            }

            // --------------------------------------------------

            ti.Content = viewbox;
            ti.Header = "Simulation " + this.number;
            ti.IsEnabled = false;

            canvas.Children.Add(image);

            tabControl.Items.Insert(tabControl.Items.Count, ti);

            backgroundWorker.DoWork += new DoWorkEventHandler((state, arg) =>
            {

            string returnvalue = string.Empty;

            ProcessStartInfo info = new ProcessStartInfo(fileName);
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = true;
            info.Arguments = this.myFilePath + " " + isParallel;

                using (Process process = Process.Start(info))
            {
                returnvalue = process.StandardOutput.ReadLine();

                string preparingTime = process.StandardOutput.ReadLine();
                string simulationTime = process.StandardOutput.ReadLine();
                string writingToFileTime = process.StandardOutput.ReadLine();

                this.item.PreparationTime = preparingTime;
                this.item.SimulationTime = simulationTime;
                this.item.WriteToFileTime = writingToFileTime;

            }

            walls = Deserialize(returnvalue);

            int sizeX = 0;
            int sizeY = 0;


            for (int i = 0; i < walls.Count; i += 2)
            {
                int mSizeX = (walls[i].GetLength(0) + 1) * size;
                if (i + 1 < walls.Count)
                    mSizeX += (walls[i + 1].GetLength(0) + 1) * size;


                    if (mSizeX > sizeX)
                        sizeX = mSizeX;

                    sizeY += (walls[i].GetLength(1) + 1) * size;
             }

            WriteableBitmap simulationBitmap = BitmapFactory.New(sizeX, sizeY);
            simulationBitmap.Clear(Colors.White);
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                canvas.Width = sizeX;
                canvas.Height = sizeY;


                PropertyInfo[] properties = typeof(SimulationListItem).GetProperties();
                int pIndex = 0;

                foreach (PropertyInfo property in properties)
                {
                    if (pIndex >= 1 && pIndex <= 13)
                    {
                        var bold = new Bold(new Run(property.Name + ": "));
                        var normal = new Run(property.GetValue(item, null).ToString() + "\n");

                        textBlock.Inlines.Add(bold);
                        textBlock.Inlines.Add(normal);
                    }
                    pIndex++;
                }

                if (walls.Count == 1)
                {
                    wallsLabel.Visibility = Visibility.Hidden;
                    button.Visibility = Visibility.Hidden;
                    bLeft.Visibility = Visibility.Hidden;
                    button3D.Visibility = Visibility.Hidden;
                    bRight.Visibility = Visibility.Hidden;

                }
            }));

            Draw(walls, simulationBitmap, size);
            simulationBitmap.Freeze();

            WriteableBitmap frontWall = null;
            WriteableBitmap backWall = null;
            WriteableBitmap leftWall = null;
            WriteableBitmap rightWall = null;
            WriteableBitmap topWall = null;
            WriteableBitmap bottomWall = null;

            if (walls.Count != 1)
            {
                backWall = BitmapFactory.New(walls[0].GetLength(0), walls[0].GetLength(1));
                frontWall = BitmapFactory.New(walls[1].GetLength(0), walls[1].GetLength(1));

                leftWall = BitmapFactory.New(walls[2].GetLength(1), walls[2].GetLength(0));
                rightWall = BitmapFactory.New(walls[3].GetLength(1), walls[3].GetLength(0));

                topWall = BitmapFactory.New(walls[4].GetLength(1), walls[4].GetLength(0));
                bottomWall = BitmapFactory.New(walls[5].GetLength(1), walls[5].GetLength(0));

                Draw(walls[1], frontWall, 1, 0);

                Draw(walls[2], leftWall, 1, 1);
                Draw(walls[4], topWall, 1, 1);

                Draw(walls[0], backWall, 1, 2);

                Draw(walls[3], rightWall, 1, 3);

                Draw(walls[5], bottomWall, 1, 4);

                backWall.Freeze();
                frontWall.Freeze();
                leftWall.Freeze();
                rightWall.Freeze();
                topWall.Freeze();
                bottomWall.Freeze();
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (walls.Count != 1)
                {
                    cube.Back.Material.Brush = new ImageBrush(backWall);
                    cube.Front.Material.Brush = new ImageBrush(frontWall);
                    cube.Left.Material.Brush = new ImageBrush(leftWall);
                    cube.Right.Material.Brush = new ImageBrush(rightWall);
                    cube.Top.Material.Brush = new ImageBrush(topWall);
                    cube.Bottom.Material.Brush = new ImageBrush(bottomWall);
                }

                image.Source = simulationBitmap;

                ti.IsEnabled = true;
                this.item.ProgressBool = false;
                this.item.ProgressValue = 100;
            }));

            });

            backgroundWorker.RunWorkerAsync();

        }

        // 21XLH70r

        private List<string [,]> Deserialize(string path)
        {
            
            System.IO.StreamReader file =
                new System.IO.StreamReader(path);

            string currentLine = file.ReadLine();
            int index = 0;

            List<string[,]> walls = new List<string[,]>();

            string[,] array = null;

            while (String.IsNullOrEmpty(currentLine) == false)
            {
                if (currentLine.Contains("dim"))
                {
                    if (index != 0)
                        walls.Add(array);

                    string[] sizes = currentLine.Split(' ');
                    array = new string[Int32.Parse(sizes[1]), Int32.Parse(sizes[2])];
                    index = 0;
                }
                else
                {
                    string[] splited = currentLine.Split(' ');

                    for (int i = 0; i < splited.Length; i++)
                    {
                        if(splited[i] != " ")
                        array[index, i] = splited[i];
                    }
                    index++;
                }


                currentLine = file.ReadLine();
            }

            walls.Add(array);


            return walls;
        }

        private void Draw(string[,] wall, WriteableBitmap bitmap, int size, int turn)
        {
            if (turn == 1)
            {
                for (int i = 0; i < wall.GetLength(1); i++)
                {
                    for (int j = 0; j < wall.GetLength(0); j++)
                    {
                        DrawRectangle(bitmap, i,
                                        j, size, myColors.Cell[Int32.Parse(wall[j, i])]);
                    }
                }
            } else if (turn == 2)
            {
                for (int i = wall.GetLength(0) - 1, indexI = 0; i >= 0; i--, indexI++)
                {
                    for (int j = 0; j < wall.GetLength(1); j++)
                    {
                        DrawRectangle(bitmap, indexI,
                                        j, size, myColors.Cell[Int32.Parse(wall[i, j])]);
                    }
                }
            }
            else if (turn == 3)
            {
                for (int i = wall.GetLength(1) - 1, indexI = 0; i >= 0; i--, indexI++)
                {
                    for (int j = 0; j < wall.GetLength(0); j++)
                    {
                        DrawRectangle(bitmap, indexI,
                                        j, size, myColors.Cell[Int32.Parse(wall[j, i])]);
                    }
                }
            }
            else if (turn == 4)
            {
                for (int i = wall.GetLength(1) - 1; i >= 0; i--)
                {
                    for (int j = wall.GetLength(0) - 1, indexJ = 0; j >= 0; j--, indexJ++)
                    {
                        DrawRectangle(bitmap, i,
                                        indexJ, size, myColors.Cell[Int32.Parse(wall[j, i])]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < wall.GetLength(0); i++)
                {
                    for (int j = 0; j < wall.GetLength(1); j++)
                    {
                        DrawRectangle(bitmap, i,
                                        j, size, myColors.Cell[Int32.Parse(wall[i, j])]);
                    }
                }
            }
        }

        private void Draw(List<string[,]> walls, WriteableBitmap bitmap, int size)
        {

            int oX = 0;
            int oY = 0;

            for (var k = 0; k < walls.Count; k++)
            {
                if (k != 0 && k % 2 == 0)
                    oX = 0;
                if (k != 0 && k % 2 == 0)
                    oY++;

                int sizeX = k - 1 >= 0 && k % 2 != 0 ? walls[k - 1].GetLength(0) + this.gapSize :  0;
                int sizeY = k - 4 >= 0 ? (walls[k - 4].GetLength(1) + this.gapSize + walls[k - 2].GetLength(1) + this.gapSize) : 
                                         k - 2 >= 0 ? walls[k - 2].GetLength(1) + this.gapSize : 0;


                for (int i = 0; i < walls[k].GetLength(0); i++)
                {
                    for (int j = 0; j < walls[k].GetLength(1); j++)
                    {
                        DrawRectangle(bitmap, i +  sizeX,
                                      j + sizeY, size, myColors.Cell[Int32.Parse(walls[k][i, j])]);
                    }
                }

                oX++;

            }
        }

        private static void DrawRectangle(WriteableBitmap bitmap,
                                            int left, int top, int size, Color color)
        {
            var x1 = left * size;
            var y1 = top * size;
            var x2 = x1 + size;
            var y2 = y1 + size;

            bitmap.FillRectangle(x1, y1, x2, y2, color);


        }



        void showAllButton_Click(Object sender, RoutedEventArgs e)
        {
            cube.mainDock.Visibility = Visibility.Hidden;
            scrollViewer.Visibility = Visibility.Visible;

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;

            wallsLabel.Visibility = Visibility.Hidden;

            backgroundWorker.DoWork += new DoWorkEventHandler((state, arg) =>
            {
                int sizeX = 0;
                int sizeY = 0;
                for (int i = 0; i < walls.Count; i += 2)
                {
                    int mSizeX = (walls[i].GetLength(0) + 1) * size;
                    if (i + 1 < walls.Count)
                        mSizeX += (walls[i + 1].GetLength(0) + 1) * size;


                    if (mSizeX > sizeX)
                        sizeX = mSizeX;

                    sizeY += (walls[i].GetLength(1) + 1) * size;
                }

                WriteableBitmap simulationBitmap = BitmapFactory.New(sizeX, sizeY);
                simulationBitmap.Clear(Colors.White);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    canvas.Width = sizeX;
                    canvas.Height = sizeY;
                }));

                Draw(walls, simulationBitmap, size);

                simulationBitmap.Freeze();

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    image.Source = simulationBitmap;
                }));
            });

            backgroundWorker.RunWorkerAsync();
        }


        private int wallsCounter = 0;

        string CheckWall()
        {
            switch (wallsCounter)
            {
                case 0:
                    return "Back";
                case 1:
                    return "Front";
                case 2:
                    return "Left";
                case 3:
                    return "Right";
                case 4:
                    return "Top";
                default:
                    return "Bottom";
            }
        }

        void leftButton_Click(Object sender, RoutedEventArgs e)
        {
            cube.mainDock.Visibility = Visibility.Hidden;
            scrollViewer.Visibility = Visibility.Visible;

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            wallsLabel.Visibility = Visibility.Visible;

            wallsCounter--;

            if (wallsCounter < 0)
                wallsCounter = this.walls.Count - 1;

            wallsLabel.Content = CheckWall();

            backgroundWorker.DoWork += new DoWorkEventHandler((state, arg) =>
            {
               

                List<string[,]> walls = new List<string[,]>();
                walls.Add(this.walls[wallsCounter]);

                int sizeX = 0;
                int sizeY = 0;
                for (int i = 0; i < walls.Count; i += 2)
                {
                    int mSizeX = (walls[i].GetLength(0) + 1) * size;
                    if (i + 1 < walls.Count)
                        mSizeX += (walls[i + 1].GetLength(0) + 1) * size;


                    if (mSizeX > sizeX)
                        sizeX = mSizeX;

                    sizeY += (walls[i].GetLength(1) + 1) * size;
                }
                WriteableBitmap simulationBitmap = BitmapFactory.New(sizeX, sizeY);
                simulationBitmap.Clear(Colors.White);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    canvas.Width = sizeX;
                    canvas.Height = sizeY;
                }));

                Draw(walls, simulationBitmap, size);

                simulationBitmap.Freeze();

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {

                    image.Source = simulationBitmap;
                }));
            });

            backgroundWorker.RunWorkerAsync();
        }

        void show3D_Click(object sender, RoutedEventArgs e)
        {
            cube.mainDock.Visibility = Visibility.Visible;
            scrollViewer.Visibility = Visibility.Hidden;
        }

        void rightButton_Click(Object sender, RoutedEventArgs e)
        {
            cube.mainDock.Visibility = Visibility.Hidden;
            scrollViewer.Visibility = Visibility.Visible;

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            wallsLabel.Visibility = Visibility.Visible;

            wallsCounter++;

            if (wallsCounter >= walls.Count)
                wallsCounter = 0;

            wallsLabel.Content = CheckWall();

            backgroundWorker.DoWork += new DoWorkEventHandler((state, arg) =>
            {


                List<string[,]> walls = new List<string[,]>();
                walls.Add(this.walls[wallsCounter]);

                int sizeX = 0;
                int sizeY = 0;
                for (int i = 0; i < walls.Count; i += 2)
                {
                    int mSizeX = (walls[i].GetLength(0) + 1) * size;
                    if (i + 1 < walls.Count)
                        mSizeX += (walls[i + 1].GetLength(0) + 1) * size;


                    if (mSizeX > sizeX)
                        sizeX = mSizeX;

                    sizeY += (walls[i].GetLength(1) + 1) * size;
                }
                WriteableBitmap simulationBitmap = BitmapFactory.New(sizeX, sizeY);
                simulationBitmap.Clear(Colors.White);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    canvas.Width = sizeX;
                    canvas.Height = sizeY;
                }));

                Draw(walls, simulationBitmap, size);

                simulationBitmap.Freeze();

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {

                    image.Source = simulationBitmap;
                }));
            });

            backgroundWorker.RunWorkerAsync();
        }
    }
}
