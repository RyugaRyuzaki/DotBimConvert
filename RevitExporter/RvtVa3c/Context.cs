#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.DB;
using static RvtVa3c.Container;
using DB = dotbim;
using Autodesk.Revit.UI;
using RvtVa3c.Model;
using System.Windows.Controls;
#endregion // Namespaces

namespace RvtVa3c
{
    public class Context : IExportContext
    {

        /// <summary>
        /// Scale entire top level BIM object node in JSON
        /// output. A scale of 1.0 will output the model in 
        /// millimetres. Currently we scale it to decimetres
        /// so that a typical model has a chance of fitting 
        /// into a cube with side length 100, i.e. 10 metres.
        /// </summary>

        ///// <summary>
        ///// Scale applied to each vertex in each individual 
        ///// BIM element. This can be used to scale the model 
        ///// down from millimetres to metres, e.g.
        ///// Currently we stick with millimetres after all
        ///// at this level.
        ///// </summary>
        //double _scale_vertex = 1.0;

        /// <summary>
        /// If true, switch Y and Z coordinate 
        /// and flip X to negative to convert from
        /// Revit coordinate system to standard 3d
        /// computer graphics coordinate system with
        /// Z pointing out of screen, X towards right,
        /// Y up.
        /// </summary>
        bool _switch_coordinates = true;

        #region VertexLookupXyz
        /// <summary>
        /// A vertex lookup class to eliminate 
        /// duplicate vertex definitions.
        /// </summary>
        class VertexLookupXyz : Dictionary<XYZ, int>
        {
            #region XyzEqualityComparer
            /// <summary>
            /// Define equality for Revit XYZ points.
            /// Very rough tolerance, as used by Revit itself.
            /// </summary>
            class XyzEqualityComparer : IEqualityComparer<XYZ>
            {
                const double _sixteenthInchInFeet
                  = 1.0 / (16.0 * 12.0);

                public bool Equals(XYZ p, XYZ q)
                {
                    return p.IsAlmostEqualTo(q,
                      _sixteenthInchInFeet);
                }

                public int GetHashCode(XYZ p)
                {
                    return Utils.PointString(p).GetHashCode();
                }
            }
            #endregion // XyzEqualityComparer

            public VertexLookupXyz()
              : base(new XyzEqualityComparer())
            {
            }

            /// <summary>
            /// Return the index of the given vertex,
            /// adding a new entry if required.
            /// </summary>
            public int AddVertex(XYZ p)
            {
                return ContainsKey(p)
                  ? this[p]
                  : this[p] = Count;
            }
        }
        #endregion // VertexLookupXyz

        #region VertexLookupInt
        /// <summary>
        /// An integer-based 3D point class.
        /// </summary>
        class PointInt : IComparable<PointInt>
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            //public PointInt( int x, int y, int z )
            //{
            //  X = x;
            //  Y = y;
            //  Z = z;
            //}

            /// <summary>
            /// Consider a Revit length zero 
            /// if is smaller than this.
            /// </summary>
            const double _eps = 1.0e-9;

            /// <summary>
            /// Conversion factor from feet to millimetres.
            /// </summary>
            const double _feet_to_m = 25.4 * 12 / 1000;

            /// <summary>
            /// Conversion a given length value 
            /// from feet to millimetre.
            /// </summary>
            public static double ConvertFeetToMetres(double d)
            {
                if (0 < d)
                {
                    return _eps > d
                      ? 0
                      : _feet_to_m * d + 0.0005;

                }
                else
                {
                    return _eps > -d
                      ? 0
                      : _feet_to_m * d - 0.0005;

                }
            }

            public PointInt(XYZ p, bool switch_coordinates)
            {
                X = ConvertFeetToMetres(p.X);
                Y = ConvertFeetToMetres(p.Y);
                Z = ConvertFeetToMetres(p.Z);

                if (switch_coordinates)
                {
                    X = -X;
                    double tmp = Y;
                    Y = Z;
                    Z = tmp;
                }
            }

            public int CompareTo(PointInt a)
            {
                double d = X - a.X;

                if (0 == d)
                {
                    d = Y - a.Y;

                    if (0 == d)
                    {
                        d = Z - a.Z;
                    }
                }
                return (0 == d) ? 0 : ((0 < d) ? 1 : -1);
            }
        }

        /// <summary>
        /// A vertex lookup class to eliminate 
        /// duplicate vertex definitions.
        /// </summary>
        class VertexLookupInt : Dictionary<PointInt, int>
        {
            #region PointIntEqualityComparer
            /// <summary>
            /// Define equality for integer-based PointInt.
            /// </summary>
            class PointIntEqualityComparer : IEqualityComparer<PointInt>
            {
                public bool Equals(PointInt p, PointInt q)
                {
                    return 0 == p.CompareTo(q);
                }

                public int GetHashCode(PointInt p)
                {
                    return (p.X.ToString()
                      + "," + p.Y.ToString()
                      + "," + p.Z.ToString())
                      .GetHashCode();
                }
            }
            #endregion // PointIntEqualityComparer

            public VertexLookupInt()
              : base(new PointIntEqualityComparer())
            {
            }

            /// <summary>
            /// Return the index of the given vertex,
            /// adding a new entry if required.
            /// </summary>
            public int AddVertex(PointInt p)
            {
                return ContainsKey(p)
                  ? this[p]
                  : this[p] = Count;
            }
        }
        #endregion // VertexLookupInt

        Document _doc;
        string _fileName;
        Dictionary<string, Va3cMaterial> _materials;
        Dictionary<string, Va3cObject> _objects;
        Dictionary<string, Va3cGeometry> _geometries;
        Va3cObject _currentElement;

        // Keyed on material uid to handle several materials per element:

        Dictionary<string, Va3cObject> _currentObject;
        Dictionary<string, Va3cGeometry> _currentGeometry;
        Dictionary<string, VertexLookupInt> _vertices;

        Stack<ElementId> _elementStack = new Stack<ElementId>();
        Stack<Transform> _transformationStack = new Stack<Transform>();

        string _currentMaterialUid;

        public string myjs = null;

        Va3cObject CurrentObjectPerMaterial
        {
            get
            {
                return _currentObject[_currentMaterialUid];
            }
        }

        Va3cGeometry CurrentGeometryPerMaterial
        {
            get
            {
                return _currentGeometry[_currentMaterialUid];
            }
        }

        VertexLookupInt CurrentVerticesPerMaterial
        {
            get
            {
                return _vertices[_currentMaterialUid];
            }
        }

        Transform CurrentTransform
        {
            get
            {
                return _transformationStack.Peek();
            }
        }

        public override string ToString()
        {
            return myjs;
        }

        /// <summary>
        /// Set the current material
        /// </summary>
        void SetCurrentMaterial(string uidMaterial)
        {
            if (!_materials.ContainsKey(uidMaterial))
            {
                Material material = _doc.GetElement(
                  uidMaterial) as Material;

                Va3cMaterial m
                  = new Va3cMaterial();

                m.uuid = uidMaterial;
                m.name = material.Name;
                m.color = Utils.ColorToInt(material.Color);
                m.opacity = 0.01 * (double)(100 - material.Transparency); // Revit has material.Transparency in [0,100], three.js expects opacity in [0.0,1.0]
                m.dotBimColor = new DB.Color { A = (int)(m.opacity * 255), R = material.Color.Red, G = material.Color.Green, B = material.Color.Blue };
                _materials.Add(uidMaterial, m);
            }
            _currentMaterialUid = uidMaterial;

            string uid_per_material = _currentElement.uuid + "-" + uidMaterial;

            if (!_currentObject.ContainsKey(uidMaterial))
            {
                Debug.Assert(!_currentGeometry.ContainsKey(uidMaterial), "expected same keys in both");

                _currentObject.Add(uidMaterial, new Va3cObject());
                CurrentObjectPerMaterial.name = _currentElement.name;
                CurrentObjectPerMaterial.geometry = uid_per_material;
                CurrentObjectPerMaterial.material = _currentMaterialUid;
                CurrentObjectPerMaterial.uuid = uid_per_material;
            }

            if (!_currentGeometry.ContainsKey(uidMaterial))
            {
                _currentGeometry.Add(uidMaterial, new Va3cGeometry());
                CurrentGeometryPerMaterial.uuid = uid_per_material;
                CurrentGeometryPerMaterial.type = "BufferGeometry";
                CurrentGeometryPerMaterial.data = new Va3cGeometryData();
                var attributes = new Attributes();
                var position = new Position();
                position.itemSize = 3;
                position.type = "Float32Array";
                position.array = new List<double>();

                attributes.position = position;
                CurrentGeometryPerMaterial.data.attributes = attributes;
            }

            if (!_vertices.ContainsKey(uidMaterial))
            {
                _vertices.Add(uidMaterial, new VertexLookupInt());
            }
        }
        public List<Category> Categories { get; set; }
        public bool MergeFile { get; set; }
        public Context(Document document, string fileName, List<Category> categories, bool mergeFile=false)
        {
            _doc = document;
            _fileName = fileName;
            Categories = categories;
            MergeFile = mergeFile;
        }

        public bool Start()
        {
            _materials = new Dictionary<string, Va3cMaterial>();
            _geometries = new Dictionary<string, Va3cGeometry>();
            _objects = new Dictionary<string, Va3cObject>();
            _transformationStack.Push(Transform.Identity);
            return true;
        }

        public void Finish()
        {

            Dictionary<string, (List<DB.Mesh>, List<DB.Element>)> keyValuePairs = GetDotBimCategory();
            List<DB.Mesh> meshes = new List<DB.Mesh>();
            List<DB.Element> elements = new List<DB.Element>();

            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Value.Item1.Count == 0) continue;
                DB.File dotBim = new DB.File
                {
                    SchemaVersion = "1.0.0",
                    Meshes = kvp.Value.Item1,
                    Elements = kvp.Value.Item2,
                    Info = GetDefaultInfo(kvp.Key)
                };
                dotBim.Save(_fileName + "-" + kvp.Key + ".bim");
                if(MergeFile)
                {
                    meshes.AddRange(dotBim.Meshes);
                    elements.AddRange(dotBim.Elements);
                }
            }

            if(meshes.Count>0&&elements.Count>0)
            {
                DB.File dotBim = new DB.File
                {
                    SchemaVersion = "1.0.0",
                    Meshes = meshes,
                    Elements = elements,
                    Info = GetDefaultInfo()
                };
                dotBim.Save(_fileName+"-" + _doc.ProjectInformation.Name + ".bim");
            }
        }

        public void OnPolymesh(PolymeshTopology polymesh)
        {

            IList<XYZ> pts = polymesh.GetPoints();
            Transform t = CurrentTransform;

            pts = pts.Select(p => t.OfPoint(p)).ToList();



            int count = 0;

            foreach (PolymeshFacet facet in polymesh.GetFacets())
            {
                var p1 = new PointInt(pts[facet.V1], _switch_coordinates);
                var p2 = new PointInt(pts[facet.V2], _switch_coordinates);
                var p3 = new PointInt(pts[facet.V3], _switch_coordinates);


                CurrentGeometryPerMaterial.data.attributes.position.array.Add(Math.Round(p1.X, 5));
                CurrentGeometryPerMaterial.data.attributes.position.array.Add(Math.Round(p1.Y, 5));
                CurrentGeometryPerMaterial.data.attributes.position.array.Add(Math.Round(p1.Z, 5));
                CurrentGeometryPerMaterial.data.attributes.position.array.Add(Math.Round(p2.X, 5));
                CurrentGeometryPerMaterial.data.attributes.position.array.Add(Math.Round(p2.Y, 5));
                CurrentGeometryPerMaterial.data.attributes.position.array.Add(Math.Round(p2.Z, 5));
                CurrentGeometryPerMaterial.data.attributes.position.array.Add(Math.Round(p3.X, 5));
                CurrentGeometryPerMaterial.data.attributes.position.array.Add(Math.Round(p3.Y, 5));
                CurrentGeometryPerMaterial.data.attributes.position.array.Add(Math.Round(p3.Z, 5));

                count++;
            }
        }

        public void OnMaterial(MaterialNode node)
        {
            //Debug.WriteLine( "     --> On Material: " 
            //  + node.MaterialId + ": " + node.NodeName );

            // OnMaterial method can be invoked for every 
            // single out-coming mesh even when the material 
            // has not actually changed. Thus it is usually
            // beneficial to store the current material and 
            // only get its attributes when the material 
            // actually changes.

            ElementId id = node.MaterialId;
            if (ElementId.InvalidElementId != id)
            {
                Element m = _doc.GetElement(node.MaterialId);
                SetCurrentMaterial(m.UniqueId);
            }
            else
            {
                //string uid = Guid.NewGuid().ToString();

                // Generate a GUID based on colour, 
                // transparency, etc. to avoid duplicating
                // non-element material definitions.

                string iColor = Utils.ColorToInt(node.Color);

                string uid = string.Format("MaterialNode_{0}_{1}",
                  iColor, Utils.RealString(node.Transparency * 100));

                if (!_materials.ContainsKey(uid))
                {
                    Va3cMaterial m
                      = new Va3cMaterial();

                    m.uuid = uid;
                    m.color = iColor;
                    m.opacity = 1.0 - node.Transparency; // Revit MaterialNode has double Transparency in ?range?, three.js expects opacity in [0.0,1.0]
                    m.dotBimColor = new DB.Color { A = (int)(m.opacity * 255), R = node.Color.Red, G = node.Color.Green, B = node.Color.Blue };
                    _materials.Add(uid, m);
                }
                SetCurrentMaterial(uid);
            }
        }
        public bool IsCanceled()
        {
            return false;
        }

        public void OnRPC(RPCNode node)
        {
            Debug.WriteLine("OnRPC: " + node.NodeName);
            //Asset asset = node.GetAsset();
            //Debug.WriteLine( "OnRPC: Asset:"
            //  + ( ( asset != null ) ? asset.Name : "Null" ) );
        }

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            Debug.WriteLine("OnViewBegin: "
              + node.NodeName + "(" + node.ViewId.IntegerValue
              + "): LOD: " + node.LevelOfDetail);

            return RenderNodeAction.Proceed;
        }

        public void OnViewEnd(ElementId elementId)
        {
            Debug.WriteLine("OnViewEnd: Id: " + elementId.IntegerValue);
            // Note: This method is invoked even for a view that was skipped.
        }

        public RenderNodeAction OnElementBegin(
          ElementId elementId)
        {
            Element e = _doc.GetElement(elementId);
            string uid = e.UniqueId;

            Debug.WriteLine(string.Format(
              "OnElementBegin: id {0} category {1} name {2}",
              elementId.IntegerValue, e.Category.Name, e.Name));

            if (_objects.ContainsKey(uid))
            {
                Debug.WriteLine("\r\n*** Duplicate element!\r\n");
                return RenderNodeAction.Skip;
            }

            if (null == e.Category)
            {
                Debug.WriteLine("\r\n*** Non-category element!\r\n");
                return RenderNodeAction.Skip;
            }

            _elementStack.Push(elementId);

            ICollection<ElementId> idsMaterialGeometry = e.GetMaterialIds(false);
            ICollection<ElementId> idsMaterialPaint = e.GetMaterialIds(true);

            int n = idsMaterialGeometry.Count;

            if (1 < n)
            {
                Debug.Print("{0} has {1} materials: {2}",
                  Utils.ElementDescription(e), n,
                  string.Join(", ", idsMaterialGeometry.Select(
                    id => _doc.GetElement(id).Name)));
            }


            _currentElement = new Va3cObject();

            _currentElement.name = Utils.ElementDescription(e);
            _currentElement.material = _currentMaterialUid;
            _currentElement.uuid = uid;

            _currentObject = new Dictionary<string, Va3cObject>();
            _currentGeometry = new Dictionary<string, Va3cGeometry>();
            _vertices = new Dictionary<string, VertexLookupInt>();

            if (null != e.Category
              && null != e.Category.Material)
            {
                SetCurrentMaterial(e.Category.Material.UniqueId);
            }

            return RenderNodeAction.Proceed;
        }

        public void OnElementEnd(
          ElementId id)
        {
            // Note: this method is invoked even for 
            // elements that were skipped.

            Element e = _doc.GetElement(id);
            string uid = e.UniqueId;

            Debug.WriteLine(string.Format(
              "OnElementEnd: id {0} category {1} name {2}",
              id.IntegerValue, e.Category.Name, e.Name));

            if (_objects.ContainsKey(uid))
            {
                Debug.WriteLine("\r\n*** Duplicate element!\r\n");
                return;
            }

            if (null == e.Category)
            {
                Debug.WriteLine("\r\n*** Non-category element!\r\n");
                return;
            }

            List<string> materials = _vertices.Keys.ToList();

            int n = materials.Count;

            _currentElement.children = new List<Va3cObject>(n);

            foreach (string material in materials)
            {
                Va3cObject obj = _currentObject[material];
                Va3cGeometry geo = _currentGeometry[material];

                obj.geometry = geo.uuid;
                _geometries.Add(geo.uuid, geo);
                _currentElement.children.Add(obj);
            }


            _currentElement.userData = Utils.GetElementProperties(e);

            // Add Revit element unique id to user data dict.

            _objects.Add(_currentElement.uuid, _currentElement);

            _elementStack.Pop();
        }

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            // This method is invoked only if the 
            // custom exporter was set to include faces.

            //Debug.Assert( false, "we set exporter.IncludeFaces false" ); // removed in Revit 2017

            Debug.WriteLine("  OnFaceBegin: " + node.NodeName);
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
            // This method is invoked only if the 
            // custom exporter was set to include faces.

            //Debug.Assert( false, "we set exporter.IncludeFaces false" ); // removed in Revit 2017

            Debug.WriteLine("  OnFaceEnd: " + node.NodeName);

            // Note: This method is invoked even for faces that were skipped.
        }

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {


            _transformationStack.Push(CurrentTransform.Multiply(
              node.GetTransform()));

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        public void OnInstanceEnd(InstanceNode node)
        {

            _transformationStack.Pop();
        }

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {

            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
            Debug.WriteLine("  OnLinkEnd: " + node.NodeName);
            // Note: This method is invoked even for instances that were skipped.
            _transformationStack.Pop();
        }

        public void OnLight(LightNode node)
        {
            Debug.WriteLine("OnLight: " + node.NodeName);
            //Asset asset = node.GetAsset();
            //Debug.WriteLine( "OnLight: Asset:" + ( ( asset != null ) ? asset.Name : "Null" ) );
        }

        private static Dictionary<string, string> GetDefaultInfo(string category=null)
        {
            Dictionary<string, string> info = new Dictionary<string, string>();
            info.Add("Description", "");
            info.Add("Author", "https://github.com/RyugaRyuzaki");
            info.Add("OriginAuthor", "https://github.com/paireks");
            if (null != category)
            {
                info.Add("Category", category);
            }
            else
            {
                info.Add("All Project", category);
            }
            return info;
        }


        private DB.Mesh MergeMesh(List<Va3cObject> children, int ElementID, List<int> face_colors)
        {
            List<double> coordinates = new List<double>();
            List<int> indices = new List<int>();
            int index = 0;
            for (int i = 0; i < children.Count; i++)
            {
                Va3cObject child = children[i];
                Va3cMaterial va3CMaterial = _materials[child.material];

                Va3cGeometry va3CGeometry = _geometries[child.geometry];
                DB.Color dotBimColor = va3CMaterial.dotBimColor;
                int itemSize = va3CGeometry.data.attributes.position.itemSize;
                List<double> array = va3CGeometry.data.attributes.position.array;
                for (int j = 0; j < array.Count; j += itemSize)
                {
                    coordinates.Add(array[j]);
                    coordinates.Add(-array[j + 2]);
                    coordinates.Add(array[j + 1]);
                  
                    if(j%9==0)
                    {
                        face_colors.Add(dotBimColor.R);
                        face_colors.Add(dotBimColor.G);
                        face_colors.Add(dotBimColor.B);
                        face_colors.Add(dotBimColor.A);
                    }

                    indices.Add(index);
                    index++;
                }
            }
            if (coordinates.Count == 0 || indices.Count == 0) return null;
            return new DB.Mesh { Coordinates = coordinates, Indices = indices, MeshId = ElementID };
        }
        private Dictionary<string, (List<DB.Mesh>, List<DB.Element>)> GetDotBimCategory()
        {
            Dictionary<string, (List<DB.Mesh>, List<DB.Element>)> categories = new Dictionary<string, (List<DB.Mesh>, List<DB.Element>)>();
            List<Va3cObject> list = _objects.Values.ToList();
            List<string> cats = Categories.Select(c => c.BuiltInCategory.ToString()).ToList();

            for (int i = 0; i < list.Count; i++)
            {

                Va3cObject element = list[i];
                if (element.userData == null) continue;
                string BuiltInCategory = element.userData["BuiltInCategory"];
                string Category = element.userData["Category"];
                string ElementID = element.userData["ElementID"];
                // ignore if Category is null and ElementID
                if (Category == null || ElementID == null || BuiltInCategory == null) continue;
                if (!cats.Contains(BuiltInCategory)) continue;
                List<int> face_colors = new List<int>();

                if (!categories.ContainsKey(Category)) categories.Add(Category, (new List<DB.Mesh>(), new List<DB.Element>()));

                List<DB.Mesh> meshes = categories[Category].Item1;
                List<DB.Element> elements = categories[Category].Item2;
                if (meshes == null || elements == null) continue;
                DB.Mesh mesh = MergeMesh(element.children, int.Parse(ElementID), face_colors);
                if (mesh == null) continue;


                categories[Category].Item1.Add(mesh);
                DB.Element dotBimElement = new DB.Element
                {
                    Type = Category,
                    Info = element.userData,
                    Vector = new DB.Vector
                    {
                        X = 0,
                        Y = 0,
                        Z = 0
                    },
                    Rotation = new DB.Rotation
                    {
                        Qw = 1,
                        Qx = 0,
                        Qy = 0,
                        Qz = 0
                    },
                    Guid = System.Guid.NewGuid().ToString(),
                    MeshId = int.Parse(ElementID),
                    FaceColors = face_colors,

                };

                categories[Category].Item2.Add(dotBimElement);
            }

            return categories;
        }

      


    }
}
