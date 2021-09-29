using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace GrainGrowthUI
{
    interface WallI
    {

        MeshGeometry3D Geometry3D { get; set; }
        DiffuseMaterial Material { get; set; }
    }
}
