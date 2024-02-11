using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RvtVa3c
{
    [DataContract]
    public class DotBimModel


    {
        [DataContract]
        public class DotBimColor
        {
            [DataMember]
            public int r { get; set; }

            [DataMember]
            public int g { get; set; }

            [DataMember]
            public int b { get; set; }

            [DataMember]
            public int a { get; set; } // 1 
            public DotBimColor(Color color, double opacity)
            {
                r = (int)color.Red; g = (int)color.Green; b = (int)color.Blue;
                a = (int)(opacity * 255);
            }
        }
        public class DotBimMaterial
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public DotBimColor color { get; set; } // 16777215
        }

        public class Position
        {
            [DataMember]
            public int itemSize { get; set; }
            [DataMember]
            public List<double> array { get; set; }
        }
        public class DotBimGeometry
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public Position position { get; set; }
        }




        [DataContract]
        public class DotBimMesh
        {
            [DataMember]
            public int mesh_id { get; set; }
            [DataMember]
            public List<double> coordinates { get; set; }
            [DataMember]
            public List<int> indices { get; set; }
        }
        [DataContract]
        public class DotBimVector
        {
            [DataMember]
            public double x { get; set; }
            [DataMember]
            public double y { get; set; }
            [DataMember]

            public double z { get; set; }

            public DotBimVector()
            {
                x = 0;
                y = 0;
                z = 0;
            }

        }
        [DataContract]
        public class DotBimRotation
        {
            [DataMember]
            public double qx { get; set; }
            [DataMember]
            public double qy { get; set; }
            [DataMember]
            public double qz { get; set; }
            [DataMember]
            public double qw { get; set; }
            public DotBimRotation()
            {
                qx = 0;
                qy = 0;
                qz = 0;
                qw = 1;
            }

        }
        [DataContract]
        public class DotBimElement
        {
            [DataMember]

            public string type { get; set; }

            [DataMember]
            public Dictionary<string, object> info { get; set; }


            [DataMember]
            public DotBimColor color { get; set; }

            [DataMember]
            public List<int> face_colors { get; set; } 

            [DataMember]
            public string guid { get; set; }

            [DataMember]
            public DotBimRotation rotation { get; set; } = new DotBimRotation();
            [DataMember]
            public DotBimVector vector { get; set; } = new DotBimVector();
            [DataMember]
            public int mesh_id { get; set; }
        }
        [DataContract]
        public class DotBimObject
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string name { get; set; } // BIM <document name>
            [DataMember]
            public string type { get; set; } // Object3D
            [DataMember]
            public List<DotBimObject> children { get; set; }
            [DataMember]
            public string geometry { get; set; }
            [DataMember]
            public string material { get; set; }
            [DataMember]
            public Dictionary<string, object> info { get; set; }
        }
    }
}
