using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GrainGrowthUI
{
    class MCube3D
    {
        public DockPanel mainDock;
        private DockPanel outerDock;

        public Wall Top = new Wall();
        public Wall Bottom = new Wall();
        public Wall Right = new Wall();
        public Wall Left = new Wall();
        public Wall Front = new Wall();
        public Wall Back = new Wall();

        class MPoint : INotifyPropertyChanged
        {
            public MPoint(double x, double y)
            {
                this.x = y;
                this.y = y;
            }

            private double x;
            private double y;

            public double X
            {
                get
                {
                    return x;
                }
                set
                {
                    x = value;
                    OnPropertyChanged("X");
                }
            }

            public double Y
            {
                get
                {
                    return y;
                }
                set
                {
                    y = value;
                    OnPropertyChanged("Y");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        private MPoint rotation; 
        private bool isRotating = false;
        private AxisAngleRotation3D axisAngleRotation3DH;
        private AxisAngleRotation3D axisAngleRotation3DV;
        private ScrollBar scrollBarH;
        private ScrollBar scrollBarV;

        public static double ConvertRange(
                    double originalStart, double originalEnd, // original range
                    double newStart, double newEnd, // desired range
                    double value) // value to convert
        {
            double scale = (newEnd - newStart) / (originalEnd - originalStart);
            return (newStart + ((value - originalStart) * scale));
        }

        private void outerDock_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!isRotating)
                return;

            var pos = e.GetPosition(this.mainDock);

            var x = pos.X ;
            var y = pos.Y;

            x = ConvertRange(0, mainDock.ActualWidth, -180, 180, x);
            y = ConvertRange(0, mainDock.ActualHeight, -180, 180, y);

            this.rotation.X = x;
            scrollBarH.Value = x;

            this.rotation.Y = y;
            scrollBarV.Value = y;
        }

        private void outerDock_LeftButtonDown(object sender, MouseEventArgs args)
        {
            isRotating = true;
        }

        private void outerDock_LeftButtonUp(object sender, MouseEventArgs args)
        {
            isRotating = false;
        }

        private void scrollBarH_ValueChanged(object sender, EventArgs args)
        {
            this.rotation.X = scrollBarH.Value;
        }

        private void scrollBarV_ValueChanged(object sender, EventArgs args)
        {
            this.rotation.Y = scrollBarV.Value;
        }

        private void outerDock_MouseLeave(object sender, EventArgs args)
        {
            isRotating = false;
        }
        private PerspectiveCamera perspectiveCamera;
        private int fieldOfView = 70;

        private void outerDock_MouseWhel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta < 0 )
            {
                fieldOfView-=2;
            } else
            {
                fieldOfView+=2;
            }

            perspectiveCamera.FieldOfView = fieldOfView;
        }

        public MCube3D()
        {
            mainDock = new DockPanel();
            mainDock.Margin = new System.Windows.Thickness(0);

            rotation = new MPoint(0, 0);

            scrollBarH = new ScrollBar();
            scrollBarH.Name = "Cube3DScrollBarHorizontal";
            scrollBarH.Orientation = Orientation.Horizontal;
            scrollBarH.Minimum = -180;
            scrollBarH.Maximum = 180;
            scrollBarH.LargeChange = 10;
            scrollBarH.SmallChange = 1;
            scrollBarH.Value = 0;
            scrollBarH.ValueChanged += new RoutedPropertyChangedEventHandler<double>(scrollBarH_ValueChanged);
            DockPanel.SetDock(scrollBarH, Dock.Bottom);

            scrollBarV = new ScrollBar();
            scrollBarV.Name = "Cube3DScrollBarVerticle";
            scrollBarV.Orientation = Orientation.Vertical;
            scrollBarV.Minimum = -180;
            scrollBarV.Maximum = 180;
            scrollBarV.LargeChange = 10;
            scrollBarV.SmallChange = 1;
            scrollBarV.Value = 0;
            scrollBarV.ValueChanged += new RoutedPropertyChangedEventHandler<double>(scrollBarV_ValueChanged);
            DockPanel.SetDock(scrollBarV, Dock.Right);

            mainDock.Children.Add(scrollBarH);
            mainDock.Children.Add(scrollBarV);

            outerDock = new DockPanel();
            outerDock.Margin = new System.Windows.Thickness(0);
            outerDock.Name = "Cube3DOuterDock";
            outerDock.Background = Brushes.White;
            outerDock.PreviewMouseMove += new MouseEventHandler(outerDock_PreviewMouseMove);
            outerDock.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(outerDock_LeftButtonDown);
            outerDock.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(outerDock_LeftButtonUp);
            outerDock.MouseLeave += new MouseEventHandler(outerDock_MouseLeave);
            outerDock.PreviewMouseWheel += new MouseWheelEventHandler(outerDock_MouseWhel);

            mainDock.Children.Add(outerDock);

            DockPanel dockCube = new DockPanel();
            dockCube.Margin = new System.Windows.Thickness(0);
            dockCube.Name = "Cube3DDock";
            dockCube.Background = Brushes.White;

            outerDock.Children.Add(dockCube);

            Viewport3D cubeView = new Viewport3D();
            cubeView.Margin = new System.Windows.Thickness(0);

            dockCube.Children.Add(cubeView);

            ModelVisual3D modelVisual3D = new ModelVisual3D();

            cubeView.Children.Add(modelVisual3D);

            Model3DGroup model3DGroup = new Model3DGroup();
            
            modelVisual3D.Content = model3DGroup;

            AmbientLight ambientLight = new AmbientLight(Colors.Gray);
            DirectionalLight directionalLight1 = new DirectionalLight(Colors.Gray, new Vector3D(1, -2, -3));
            DirectionalLight directionalLight2 = new DirectionalLight(Colors.Gray, new Vector3D(-1, 2, 3));

            model3DGroup.Children.Add(ambientLight);
            model3DGroup.Children.Add(directionalLight1);
            model3DGroup.Children.Add(directionalLight2);

            this.Top.Geometry3D.Positions = new Point3DCollection(new List<Point3D>() {
                new Point3D(-1, 1, 1), new Point3D(1,1,1), new Point3D(1,1,-1), new Point3D(-1, 1, -1)});

            this.Bottom.Geometry3D.Positions = new Point3DCollection(new List<Point3D>() {
                new Point3D(-1, -1, -1), new Point3D(1,-1,-1), new Point3D(1,-1,1), new Point3D(-1, -1, 1)});

            this.Right.Geometry3D.Positions = new Point3DCollection(new List<Point3D>() {
                new Point3D(1, -1, 1), new Point3D(1,-1,-1), new Point3D(1,1,-1), new Point3D(1, 1, 1)});

            this.Left.Geometry3D.Positions = new Point3DCollection(new List<Point3D>() {
                new Point3D(-1, -1, -1), new Point3D(-1,-1,1), new Point3D(-1,1,1), new Point3D(-1, 1, -1)});

            this.Front.Geometry3D.Positions = new Point3DCollection(new List<Point3D>() {
                new Point3D(-1, -1, 1), new Point3D(1,-1,1), new Point3D(1,1,1), new Point3D(-1, 1, 1)});

            this.Back.Geometry3D.Positions = new Point3DCollection(new List<Point3D>() {
                new Point3D(1, -1, -1), new Point3D(-1,-1,-1), new Point3D(-1,1,-1), new Point3D(1, 1, -1)});

            GeometryModel3D topModel = new GeometryModel3D(this.Top.Geometry3D, this.Top.Material); 
            GeometryModel3D bottomModel = new GeometryModel3D(this.Bottom.Geometry3D, this.Bottom.Material); 
            GeometryModel3D rightModel = new GeometryModel3D(this.Right.Geometry3D, this.Right.Material); 
            GeometryModel3D leftModel = new GeometryModel3D(this.Left.Geometry3D, this.Left.Material); 
            GeometryModel3D frontModel = new GeometryModel3D(this.Front.Geometry3D, this.Front.Material); 
            GeometryModel3D backModel = new GeometryModel3D(this.Back.Geometry3D, this.Back.Material);

            model3DGroup.Children.Add(topModel);
            model3DGroup.Children.Add(bottomModel);
            model3DGroup.Children.Add(rightModel);
            model3DGroup.Children.Add(leftModel);
            model3DGroup.Children.Add(frontModel);
            model3DGroup.Children.Add(backModel);

            perspectiveCamera = new PerspectiveCamera(new Point3D(1.75, 2.75, 2.75),
                new Vector3D(-1.75, -2.75, -2.75), new Vector3D(0, 1, 0), fieldOfView);

            cubeView.Camera = perspectiveCamera;

            Transform3DGroup transform3DGroup = new Transform3DGroup();

            Binding myBindingH = new Binding("X");
            myBindingH.Source = rotation;

            Binding myBindingV = new Binding("Y");
            myBindingV.Source = rotation;

            RotateTransform3D rotateTransform3DH = new RotateTransform3D();
            axisAngleRotation3DH = new AxisAngleRotation3D();
            axisAngleRotation3DH.Axis = new Vector3D(0, 1, 0);
            BindingOperations.SetBinding(axisAngleRotation3DH, AxisAngleRotation3D.AngleProperty, myBindingH);
            rotateTransform3DH.Rotation = axisAngleRotation3DH;

            RotateTransform3D rotateTransform3DV = new RotateTransform3D();
            axisAngleRotation3DV = new AxisAngleRotation3D();
            axisAngleRotation3DV.Axis = new Vector3D(1, 0, 0);
            BindingOperations.SetBinding(axisAngleRotation3DV, AxisAngleRotation3D.AngleProperty, myBindingV);
            rotateTransform3DV.Rotation = axisAngleRotation3DV;

            transform3DGroup.Children.Add(rotateTransform3DH);
            transform3DGroup.Children.Add(rotateTransform3DV);

            cubeView.Camera.Transform = transform3DGroup;

        }
    }
}
