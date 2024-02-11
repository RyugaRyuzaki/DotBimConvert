using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static RvtVa3c.Container;

namespace RvtVa3c
{
    [DataContract]
    public class DotBim

    {

        public class DotBimColor
        {
            [DataMember]
            public int r { get; set; }
            [DataMember]
            public int g { get; set; }
            [DataMember]
            public int b { get; set; }
            [DataMember]

            public int a { get; set; }

            public DotBimColor(Color color, double opacity)
            {
                r = (int)color.Red; g = (int)color.Green; b = (int)color.Blue; a = (int)opacity * 255;
            }
        }
        public class DotBimVector
        {
            [DataMember]
            public int x { get; set; }
            [DataMember]
            public int y { get; set; }
            [DataMember]
            public int z { get; set; }
            public DotBimVector()
            {
                x = 0;y = 0;z = 0;    
            }
        }
        public class DotBimRotation
        {
            [DataMember]
            public int qx { get; set; }
            [DataMember]
            public int qy { get; set; }
            [DataMember]
            public int qz { get; set; }
            [DataMember]
            public int qw { get; set; }
            public DotBimRotation()
            {
                qx = 0;qy = 0;qz = 0;qw = 0;
            }
        }
        public class DotBimMeshes
        {
            [DataMember]
            public int mesh_id { get; set; }
            [DataMember]
            public List<double> coordinates { get; set; }
            [DataMember]
            public List<int> indices { get; set; }
        }
        public class DotBimElement
        {
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public Dictionary<string, string> info { get; set; }
            [DataMember]
            public DotBimColor color { get; set; }
            [DataMember]
            public List<double> face_colors { get; set; }
            [DataMember]
            public string guid { get; set; }
            [DataMember]
            public DotBimRotation rotation { get; set; }
            [DataMember]
            public DotBimVector vector { get; set; }
            [DataMember]
            public int mesh_id { get; set; }
        } 
        public class DotBimCategory
        {
            [DataMember]
            public List<DotBimMeshes> meshes { get; set; }
            [DataMember]
            public List<DotBimElement> elements { get; set; }
        }  
        

        

        [DataMember]
        public string schema_version { get; set; } = "1.0.0";
        [DataMember]
        public Dictionary<string, string> info { get; set; }
        [DataMember]
        public List<DotBimMeshes> meshes { get; set; }
        [DataMember]
        public List<DotBimElement> elements { get; set; }
    }
}
