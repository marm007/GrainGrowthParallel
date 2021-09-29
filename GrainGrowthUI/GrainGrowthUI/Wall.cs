using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GrainGrowthUI
{
    class Wall : WallI
    {
        private MeshGeometry3D geometry3D;
        private DiffuseMaterial material;

        public Wall()
        {
            this.geometry3D = new MeshGeometry3D();
            this.material = new DiffuseMaterial();
            this.geometry3D.TriangleIndices = new Int32Collection(new List<int>() { 0, 1, 2, 2, 3, 0 });
            this.geometry3D.TextureCoordinates = new PointCollection(new List<Point>() {
                new Point(0, 1), new Point(1,1), new Point(1,0), new Point(0, 0)});

        }

        public MeshGeometry3D Geometry3D { get => geometry3D; set => geometry3D = value; }
        public DiffuseMaterial Material { get => material; set => material = value; }
    }
}
