using Color = dotbim.Color;
using Vector = dotbim.Vector;
using Rotation = dotbim.Rotation;
using Mesh = dotbim.Mesh;
using Element = dotbim.Element;
using File = dotbim.File;
using Google.FlatBuffers;
using Newtonsoft.Json;
namespace CompressDotBim {
    public class Utils {
        public static byte[]  CompressDotBimFile(File file){
            try
            {
                //2048=limited 2GB if over throw error
                JsonSerializerSettings settings= new JsonSerializerSettings();
                settings.NullValueHandling= NullValueHandling.Ignore;
                FlatBufferBuilder builder = new FlatBufferBuilder(2048);
                var SchemaVersion = builder.CreateString(file.SchemaVersion);
                string info = JsonConvert.SerializeObject(file.Info, Formatting.None, settings);
                var infoVector = builder.CreateString(info);
                Offset<DotBimBufferMeshes>[] meshCount = new Offset<DotBimBufferMeshes>[file.Meshes.Count];
                int i = 0;
                foreach (var mesh in file.Meshes)
                {
                    meshCount[i] = CompressMesh(mesh, builder);
                    i++; 
                }
                var meshVector = DotBimSchemaBuffer.CreateMeshesVector(builder, meshCount);
                i = 0;
                Offset<DotBimBufferElement>[] elementCount = new Offset<DotBimBufferElement>[file.Elements.Count];
                foreach (var element in file.Elements)
                {
                    elementCount[i] = CompressElement(element, builder);
                    i++;
                }
                var elementVector = DotBimSchemaBuffer.CreateElementsVector(builder, elementCount);
                DotBimSchemaBuffer.StartDotBimSchemaBuffer(builder);
                DotBimSchemaBuffer.AddSchemaversion(builder,SchemaVersion);
                DotBimSchemaBuffer.AddInfo(builder,infoVector);
                DotBimSchemaBuffer.AddMeshes(builder,meshVector);
                DotBimSchemaBuffer.AddElements(builder,elementVector);
                var offset = DotBimSchemaBuffer.EndDotBimSchemaBuffer(builder);
                builder.Finish(offset.Value);
                return builder.SizedByteArray();
            }
            catch (Exception ex)
            {
                
                throw  new Exception($"{ex.Message}");
            }
        }
        public static File DeCompressDotBimFile(byte[] compressedData){
            try
            {
                ByteBuffer byteBuffer = new ByteBuffer(compressedData);
                DotBimSchemaBuffer dotBimSchemaBuffer = DotBimSchemaBuffer.GetRootAsDotBimSchemaBuffer(byteBuffer);

                
                string schemaVersion = dotBimSchemaBuffer.Schemaversion;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                Dictionary<string, string> info = JsonConvert.DeserializeObject<Dictionary<string, string>>(dotBimSchemaBuffer.Info);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

               
                List<Mesh> meshes = new List<Mesh>();
                for (int i = 0; i < dotBimSchemaBuffer.MeshesLength; i++)
                {
#pragma warning disable CS8629 // Nullable value type may be null.
                    Mesh mesh = DeCompressMesh((DotBimBufferMeshes)dotBimSchemaBuffer.Meshes(i));
#pragma warning restore CS8629 // Nullable value type may be null.
                    meshes.Add(mesh);
                }

                
                List<Element> elements = new List<Element>();
                for (int i = 0; i < dotBimSchemaBuffer.ElementsLength; i++)
                {
#pragma warning disable CS8629 // Nullable value type may be null.
                    Element element = DeCompressElement((DotBimBufferElement)dotBimSchemaBuffer.Elements(i));
#pragma warning restore CS8629 // Nullable value type may be null.
                    elements.Add(element);
                }

                File decompressedFile = new File
                {
                    SchemaVersion = schemaVersion,
                    Info = info,
                    Meshes = meshes,
                    Elements = elements
                };

                return decompressedFile;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during decompression: {ex.Message}");
            }
        }
        private static Offset<DotBimBufferColor> CompressColor(Color color,FlatBufferBuilder builder){
            return DotBimBufferColor.CreateDotBimBufferColor(builder, color.R, color.G, color.B, color.A);
        } 
        private static Color  DeCompressColor(DotBimBufferColor color){
            return new Color {R=color.R,G=color.G,B=color.B,A=color.A };
        } 
        private static Offset<DotBimBufferVector> CompressVector(Vector vector,FlatBufferBuilder builder){
            return DotBimBufferVector.CreateDotBimBufferVector(builder, (int)vector.X, (int)vector.Y, (int)vector.Z);
        } 
        private static Vector  DeCompressVector(DotBimBufferVector vector){
            return new Vector { X = vector.X, Y = vector.Y, Z = vector.Z };
        } 
        private static Offset<DotBimBufferRotation> CompressRotation(Rotation rotation,FlatBufferBuilder builder){
             return DotBimBufferRotation.CreateDotBimBufferRotation(builder, (float)rotation.Qx,(float)rotation.Qy,(float)rotation.Qz,(float)rotation.Qw);
        } 
        private static Rotation  DeCompressRotation(DotBimBufferRotation rotation){
            return new Rotation { Qx = rotation.Qx, Qy = rotation.Qy, Qz = rotation.Qz, Qw = rotation.Qw };
        } 
        private static Offset<DotBimBufferInfo>[] CompressInfo(Dictionary<string, string> Info,FlatBufferBuilder builder){
              Offset<DotBimBufferInfo>[] infos = new Offset<DotBimBufferInfo>[Info.Values.Count];
                int i = 0;
              foreach (var item in Info.ToList())
              {
                var key = builder.CreateString(item.Key);
                var value = builder.CreateString(item.Value);
                infos[i] = DotBimBufferInfo.CreateDotBimBufferInfo(builder,key,value);
                i++;
              }
            return infos;
        } 
        private static Dictionary<string, string>  DeCompressInfo(DotBimBufferInfo[] infos){
            Dictionary<string, string> Info = new Dictionary<string, string>();
            foreach (var item in infos)
            {
                string key = item.Key;
                string value = item.Value;
                if (!Info.ContainsKey(key)) Info[key] = value;
            }
            return Info;
        } 
        private static Offset<DotBimBufferMeshes> CompressMesh(Mesh mesh,FlatBufferBuilder builder){
            float[] coordinatesArray = mesh.Coordinates.Select(d => (float)d).ToArray();
            var coordinates = DotBimBufferMeshes.CreateCoordinatesVector(builder, coordinatesArray);
            int[] indicesArray = mesh.Indices.ToArray();
            var indices = DotBimBufferMeshes.CreateIndicesVector(builder, indicesArray);
            return DotBimBufferMeshes.CreateDotBimBufferMeshes(builder, mesh.MeshId, coordinates, indices);
        } 
        private static Mesh  DeCompressMesh(DotBimBufferMeshes comMesh){

            List<double> coordinates = new List<double>();
            for (int i = 0; i < comMesh.CoordinatesLength; i++)
            {
                coordinates.Add(comMesh.Coordinates(i));
            }
            List<int> indices = new List<int>();
             for (int i = 0; i < comMesh.IndicesLength; i++)
            {
                indices.Add(comMesh.Indices(i));
            }
            return new Mesh { MeshId = comMesh.Meshid,Coordinates=coordinates,Indices=indices };
        } 
        private static Offset<DotBimBufferElement> CompressElement(Element element,FlatBufferBuilder builder){
            var typeVector = builder.CreateString(element.Type);
            var infoCount = CompressInfo(element.Info, builder);
            var infoVector = DotBimBufferElement.CreateInfoVector(builder,infoCount);
            var colorVector = CompressColor(element.Color, builder);
            var faceColorVector = DotBimBufferElement.CreateFacecolorsVector(builder, element.FaceColors.ToArray());
            var guidVector = builder.CreateString(element.Guid);
            var rotationVector = CompressRotation(element.Rotation, builder);
            var vectorVector = CompressVector(element.Vector, builder);

            return DotBimBufferElement.CreateDotBimBufferElement(
                builder,
                typeVector,
                infoVector,
                colorVector,
                faceColorVector,
                guidVector,
                rotationVector,
                vectorVector,
                element.MeshId
                );
        }
        private static Element DeCompressElement(DotBimBufferElement elementCompress){
            Dictionary<string, string> InfoList = new Dictionary<string, string>();
            for (int i = 0; i < elementCompress.InfoLength; i++)
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                DotBimBufferInfo item = (DotBimBufferInfo)elementCompress.Info(i);
#pragma warning restore CS8629 // Nullable value type may be null.
                string key = item.Key;
                string value = item.Value;
                if (!InfoList.ContainsKey(key)) InfoList[key] = value;
            }
#pragma warning disable CS8629 // Nullable value type may be null.
            Color color = DeCompressColor((DotBimBufferColor)elementCompress.Color);
#pragma warning restore CS8629 // Nullable value type may be null.
            List<int> face_colors = new List<int>();
            for (int i = 0; i < elementCompress.FacecolorsLength; i++)
            {
                face_colors.Add(elementCompress.Facecolors(i));
            }
#pragma warning disable CS8629 // Nullable value type may be null.
            Rotation rotation = DeCompressRotation((DotBimBufferRotation)elementCompress.Rotation);
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            Vector vector = DeCompressVector((DotBimBufferVector)elementCompress.Vector);
#pragma warning restore CS8629 // Nullable value type may be null.
            return new Element
            {
                Type = elementCompress.Type,
                Info = InfoList,
                Color = color,
                FaceColors = face_colors,
                Guid = elementCompress.Guid,
                Rotation=rotation,
                Vector=vector,
                MeshId=elementCompress.Meshid
            };
        }
    }
}