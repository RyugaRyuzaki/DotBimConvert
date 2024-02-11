#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.DB;
//using Autodesk.Revit.Utility;
using Newtonsoft.Json;
using Autodesk.Revit.DB.Visual;
using System.Security.Cryptography;
using System.Text;
using static RvtVa3c.Container;
using static RvtVa3c.DotBim;
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
        double _scale_bim = 1.0;

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
        bool all=false;
        Dictionary<string, Container.Va3cMaterial> _materials;
        Dictionary<string, Container.Va3cObject> _objects;
        Dictionary<string, Container.Va3cGeometry> _geometries;
        Container.Va3cObject _currentElement;

        // Keyed on material uid to handle several materials per element:

        Dictionary<string, Container.Va3cObject> _currentObject;
        Dictionary<string, Container.Va3cGeometry> _currentGeometry;
        Dictionary<string, VertexLookupInt> _vertices;

        Stack<ElementId> _elementStack = new Stack<ElementId>();
        Stack<Transform> _transformationStack = new Stack<Transform>();

        string _currentMaterialUid;

        public string myjs = null;

        Container.Va3cObject CurrentObjectPerMaterial
        {
            get
            {
                return _currentObject[_currentMaterialUid];
            }
        }

        Container.Va3cGeometry CurrentGeometryPerMaterial
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

                Container.Va3cMaterial m
                  = new Container.Va3cMaterial();

                //m.metadata = new Va3cContainer.Va3cMaterialMetadata();
                //m.metadata.type = "material";
                //m.metadata.version = 4.2;
                //m.metadata.generator = "RvtVa3c 2015.0.0.0";

                m.uuid = uidMaterial;
                m.name = material.Name;
                m.color = Utils.ColorToInt(material.Color);
                m.opacity = 0.01 * (double)(100 - material.Transparency); // Revit has material.Transparency in [0,100], three.js expects opacity in [0.0,1.0]
                m.dotBimColor = new DotBim.DotBimColor(material.Color, m.opacity);
                _materials.Add(uidMaterial, m);
            }
            _currentMaterialUid = uidMaterial;

            string uid_per_material = _currentElement.uuid + "-" + uidMaterial;

            if (!_currentObject.ContainsKey(uidMaterial))
            {
                Debug.Assert(!_currentGeometry.ContainsKey(uidMaterial), "expected same keys in both");

                _currentObject.Add(uidMaterial, new Container.Va3cObject());
                CurrentObjectPerMaterial.name = _currentElement.name;
                CurrentObjectPerMaterial.geometry = uid_per_material;
                CurrentObjectPerMaterial.material = _currentMaterialUid;
                CurrentObjectPerMaterial.matrix = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
                CurrentObjectPerMaterial.type = "Mesh";
                CurrentObjectPerMaterial.uuid = uid_per_material;
            }

            if (!_currentGeometry.ContainsKey(uidMaterial))
            {
                _currentGeometry.Add(uidMaterial, new Container.Va3cGeometry());
                CurrentGeometryPerMaterial.uuid = uid_per_material;
                CurrentGeometryPerMaterial.type = "BufferGeometry";
                CurrentGeometryPerMaterial.data = new Container.Va3cGeometryData();
                var attributes = new Container.Attributes();
                var position = new Container.Position();
                position.itemSize = 3;
                position.type = "Float32Array";
                position.array = new List<double>();
                var normal = new Container.Normal();
                normal.itemSize = 3;
                normal.type = "Float32Array";
                normal.array = new List<double>();
                var uv = new Container.UV();
                uv.itemSize = 2;
                uv.type = "Float32Array";
                uv.array = new List<double>();
                var index = new Container.Index();
                index.itemSize = 1;
                index.type = "Uint16Array";
                index.array = new List<int>();

                attributes.position = position;
                attributes.normal = normal;
                attributes.uv = uv;
                CurrentGeometryPerMaterial.data.attributes = attributes;
                //CurrentGeometryPerMaterial.data.index = index;

                //CurrentGeometryPerMaterial.data.faces = new List<int>();
                //CurrentGeometryPerMaterial.data.vertices = new List<double>();
                //CurrentGeometryPerMaterial.data.normals = new List<double>();
                //CurrentGeometryPerMaterial.data.uvs = new List<double>();
                CurrentGeometryPerMaterial.data.visible = true;
                CurrentGeometryPerMaterial.data.castShadow = true;
                CurrentGeometryPerMaterial.data.receiveShadow = false;
                CurrentGeometryPerMaterial.data.doubleSided = true;
                CurrentGeometryPerMaterial.data.scale = 1.0;
            }

            if (!_vertices.ContainsKey(uidMaterial))
            {
                _vertices.Add(uidMaterial, new VertexLookupInt());
            }
        }

        public Context(Document document,  string fileName)
        {
            _doc = document;
            _fileName = fileName;
        }

        public bool Start()
        {
            _materials = new Dictionary<string, Container.Va3cMaterial>();
            _geometries = new Dictionary<string, Container.Va3cGeometry>();
            _objects = new Dictionary<string, Container.Va3cObject>();

            _transformationStack.Push(Transform.Identity);


         
            return true;
        }

        public void Finish()
        {

            JsonSerializerSettings settings
               = new JsonSerializerSettings();

            settings.NullValueHandling
              = NullValueHandling.Ignore;
            GetDotBim(settings);

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
                    Container.Va3cMaterial m
                      = new Container.Va3cMaterial();

                    m.uuid = uid;
                    m.color = iColor;
                    m.opacity = 1.0 - node.Transparency; // Revit MaterialNode has double Transparency in ?range?, three.js expects opacity in [0.0,1.0]
                    m.dotBimColor = new DotBim.DotBimColor(node.Color, m.opacity);
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


            _currentElement = new Container.Va3cObject();

            _currentElement.name = Utils.ElementDescription(e);
            _currentElement.material = _currentMaterialUid;
            _currentElement.matrix = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            _currentElement.type = "RevitElement";
            _currentElement.uuid = uid;

            _currentObject = new Dictionary<string, Container.Va3cObject>();
            _currentGeometry = new Dictionary<string, Container.Va3cGeometry>();
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

            _currentElement.children = new List<Container.Va3cObject>(n);

            foreach (string material in materials)
            {
                Container.Va3cObject obj = _currentObject[material];
                Container.Va3cGeometry geo = _currentGeometry[material];

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
            Debug.WriteLine("  OnInstanceBegin: " + node.NodeName
              + " symbol: " + node.GetSymbolId().IntegerValue);

            // This method marks the start of processing a family instance

            _transformationStack.Push(CurrentTransform.Multiply(
              node.GetTransform()));

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        public void OnInstanceEnd(InstanceNode node)
        {
            Debug.WriteLine("  OnInstanceEnd: " + node.NodeName);
            // Note: This method is invoked even for instances that were skipped.
            _transformationStack.Pop();
        }

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            Debug.WriteLine("  OnLinkBegin: " + node.NodeName + " Document: " + node.GetDocument().Title + ": Id: " + node.GetSymbolId().IntegerValue);
            _transformationStack.Push(CurrentTransform.Multiply(node.GetTransform()));
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

        private static Dictionary<string, string> GetDefaultInfo(string category)
        {
            Dictionary<string, string> info = new Dictionary<string, string>();
            info.Add("Description", "");
            info.Add("Author", "https://github.com/RyugaRyuzaki");
            info.Add("OriginAuthor", "https://github.com/paireks");
            info.Add("Category", category);
            return info;
        }

        private bool IsElementHasGeometry(Va3cObject child)
        {
            Va3cMaterial va3CMaterial = _materials[child.material];
            Va3cGeometry va3CGeometry = _geometries[child.geometry];
            if(va3CMaterial==null||va3CGeometry == null || va3CGeometry.data == null|| va3CGeometry.data.attributes == null || va3CGeometry.data.attributes.position == null) { return false; }
            if (va3CGeometry.data.attributes.position.array == null || va3CGeometry.data.attributes.position.array.Count == 0) return false;
            return true;
        }
        private DotBim.DotBimMeshes MergeMesh(List<Va3cObject> children,int ElementID, List<double> face_colors)
        {

            DotBim.DotBimMeshes mesh= new DotBim.DotBimMeshes();
             List<double> coordinates = new List<double>();
             List<int> indices = new List<int>();
            int index = 0;
            for (int i = 0; i < children.Count; i++)
            {
                Va3cObject child = children[i];
                Va3cMaterial va3CMaterial = _materials[child.material];

                Va3cGeometry va3CGeometry = _geometries[child.geometry];

                if (va3CMaterial == null || va3CGeometry == null || va3CGeometry.data == null || va3CGeometry.data.attributes == null || va3CGeometry.data.attributes.position == null) { return null; }

                if (va3CGeometry.data.attributes.position.array == null || va3CGeometry.data.attributes.position.array.Count == 0) return null;

                DotBim.DotBimColor dotBimColor = va3CMaterial.dotBimColor;
                int itemSize = va3CGeometry.data.attributes.position.itemSize;
                List<double> array = va3CGeometry.data.attributes.position.array;
                for (int j = 0; j < array.Count; j+=itemSize)
                {
                    coordinates.Add(array[j]);
                    coordinates.Add(array[j+1]);
                    coordinates.Add(array[j+2]);
                    face_colors.Add(dotBimColor.r);
                    face_colors.Add(dotBimColor.g);
                    face_colors.Add(dotBimColor.b);
                    face_colors.Add(dotBimColor.a);
                    indices.Add(index);
                    index++;
                }
            }
            mesh.mesh_id = ElementID;
            mesh.coordinates = coordinates;
            mesh.indices = indices;
            return mesh;
        }
        private Dictionary<string,DotBimCategory> GetDotBimCategory()
        {
            Dictionary<string,DotBimCategory> categories =new Dictionary<string, DotBimCategory>();
            List<Va3cObject> list = _objects.Values.ToList();
            for (int i = 0; i < list.Count; i++)
            {

                Va3cObject  element = list[i];
                if (element.userData == null) continue;
                string Category = element.userData["Category"];
                string ElementID = element.userData["ElementID"];
                if(Category==null||ElementID==null) continue;
                List<double> face_colors = new List<double>();

                if (!categories.ContainsKey(Category))
                {
                    DotBimCategory dotBimCategory = new DotBimCategory();
                    dotBimCategory.meshes = new List<DotBimMeshes>();    
                    dotBimCategory.elements = new List<DotBimElement>();
                    categories.Add(Category, dotBimCategory);
                }
                if (categories[Category] == null) continue;

                DotBimMeshes mesh = MergeMesh(element.children, int.Parse(ElementID), face_colors);
                if(mesh ==null) continue;
                categories[Category].meshes.Add(mesh);
                DotBimElement dotBimElement = new DotBimElement();
                dotBimElement.type = Category;
                dotBimElement.info = element.userData;
                dotBimElement.vector = new DotBimVector();
                dotBimElement.rotation = new DotBimRotation();
                dotBimElement.guid = element.uuid;
                dotBimElement.mesh_id = int.Parse(ElementID);
                dotBimElement.face_colors = face_colors;
                categories[Category].elements.Add(dotBimElement);
             }

            return categories;
        }   
      
        private void GetDotBim( JsonSerializerSettings settings)
        {
            Dictionary<string, DotBimCategory> keyValuePairs = GetDotBimCategory();
            foreach (var kvp in keyValuePairs)
            {
                DotBim dotBim = new DotBim();
                dotBim.info = GetDefaultInfo(kvp.Key);
                dotBim.meshes= kvp.Value.meshes;
                dotBim.elements= kvp.Value.elements;
                File.WriteAllText( _fileName+"-"+kvp.Key+".bim", JsonConvert.SerializeObject(dotBim, Formatting.Indented, settings));
            }
        }  
         
       
    }
}
