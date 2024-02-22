import flatbuffers
import numpy as np
import json
from dotbimpy import File,Color,Vector,Rotation,Mesh,Element;
import CompressDotBim.DotBimBufferColor as DotBimBufferColor;
import CompressDotBim.DotBimBufferVector as DotBimBufferVector;
import CompressDotBim.DotBimBufferRotation as DotBimBufferRotation;
import CompressDotBim.DotBimBufferInfo as DotBimBufferInfo;
import CompressDotBim.DotBimBufferMeshes as DotBimBufferMeshes;
import CompressDotBim.DotBimBufferElement as DotBimBufferElement;
import CompressDotBim.DotBimSchemaBuffer as SchemaBuffer;
from CompressDotBim.DotBimSchemaBuffer import DotBimSchemaBuffer

class CompressDotBimFile:
    def __init__(self,file):
        if not isinstance(file, File):
            raise TypeError("'File' must be of type 'File'.")
        self.schema_version=file.schema_version
        self.info=file.info
        self.meshes=file.meshes
        self.elements=file.elements
    def compress(self):
        builder = flatbuffers.Builder(2048)
        schema_version_vector=builder.CreateString(self.schema_version)
        info_vector=builder.CreateString( json.dumps(self.info))
        # meshes
        meshes_count=[]
        for i in range(len(self.meshes)):
            meshes_count.append(self.compress_mesh(self.meshes[i],builder))
        meshes_count.reverse()
            
        SchemaBuffer.StartMeshesVector(builder, len(meshes_count))
        for mesh in meshes_count:
            builder.PrependUOffsetTRelative(mesh)
        meshes_offset = builder.EndVector( len(meshes_count))

        #elements
        elements_count=[]
        for i in range(len(self.elements)):
            elements_count.append(self.compress_element(self.elements[i],builder))
   
        elements_count.reverse()   

        SchemaBuffer.StartElementsVector(builder, len(elements_count))
        for element in elements_count:
            builder.PrependUOffsetTRelative(element)

        elements_offset = builder.EndVector(len(elements_count))

        SchemaBuffer.Start(builder)
        SchemaBuffer.AddSchemaversion(builder,schema_version_vector)
        SchemaBuffer.AddInfo(builder,info_vector)
        SchemaBuffer.AddMeshes(builder,meshes_offset)
        SchemaBuffer.AddElements(builder,elements_offset)
        schema_buffer = SchemaBuffer.End(builder)
        builder.Finish(schema_buffer)
        return builder.Output()
        
    def compress_color(self,color,builder):
        if not isinstance(color, Color):
            raise TypeError("'Color' must be of type 'Color'.")
        DotBimBufferColor.Start(builder)
        DotBimBufferColor.AddR(builder,color.r)
        DotBimBufferColor.AddG(builder,color.g)
        DotBimBufferColor.AddB(builder,color.b)
        DotBimBufferColor.AddA(builder,color.a)
        return DotBimBufferColor.End(builder)
    
    
    def compress_vector(self,vector,builder):
        if not isinstance(vector, Vector):
            raise TypeError("'Vector' must be of type 'Vector'.")
        DotBimBufferVector.Start(builder)
        DotBimBufferVector.AddX(builder,vector.x)
        DotBimBufferVector.AddY(builder,vector.y)
        DotBimBufferVector.AddZ(builder,vector.z)
        return DotBimBufferVector.End(builder)
    
    
    def compress_rotation(self,rotation,builder):
        if not isinstance(rotation, Rotation):
            raise TypeError("'rotation' must be of type 'Rotation'.")
        DotBimBufferRotation.Start(builder)
        DotBimBufferRotation.AddQx(builder,rotation.qx)
        DotBimBufferRotation.AddQy(builder,rotation.qy)
        DotBimBufferRotation.AddQz(builder,rotation.qz)
        DotBimBufferRotation.AddQw(builder,rotation.qw)
        return DotBimBufferRotation.End(builder)

            
    def compress_mesh(self,mesh,builder):
        if not isinstance(mesh, Mesh):
            raise TypeError("'mesh' must be of type 'Mesh'.")
        coordinates_offset = builder.CreateNumpyVector(np.array(mesh.coordinates, dtype=np.float32))
        indices_offset = builder.CreateNumpyVector(np.array(mesh.indices, dtype=np.int32))
        DotBimBufferMeshes.Start(builder)
        DotBimBufferMeshes.AddMeshid(builder,mesh.mesh_id)
        DotBimBufferMeshes.AddCoordinates(builder,coordinates_offset)
        DotBimBufferMeshes.AddIndices(builder,indices_offset)
        return DotBimBufferMeshes.End(builder)
    
    
    
    def compress_element(self,element,builder):
        if not isinstance(element, Element):
            raise TypeError("'element' must be of type 'Element'.")
        type_vector=builder.CreateString(element.type)
        guid_vector=builder.CreateString(element.guid)
        info_count=[]
        for key, value in element.info.items():
            key_vector=builder.CreateString(key)
            value_vector=builder.CreateString(value)
            DotBimBufferInfo.Start(builder)
            DotBimBufferInfo.AddKey(builder,key_vector)
            DotBimBufferInfo.AddValue(builder,value_vector)
            info_count.append(DotBimBufferInfo.End(builder))

        info_count.reverse()    
        DotBimBufferElement.StartInfoVector(builder, len(info_count))
        for info in info_count:
            builder.PrependUOffsetTRelative(info)

        info_vector_offset = builder.EndVector(len(info_count))

        face_color_offset = builder.CreateNumpyVector(np.array(element.face_colors, dtype=np.int32))
       
        
        rotation_offset = self.compress_rotation(element.rotation,builder)
        color_offset = self.compress_color(element.color,builder)
        vector_offset = self.compress_vector(element.vector,builder)
        
        DotBimBufferElement.Start(builder)
        DotBimBufferElement.AddType(builder,type_vector)
        DotBimBufferElement.AddInfo(builder,info_vector_offset)
        DotBimBufferElement.AddColor(builder,color_offset)
        DotBimBufferElement.AddFacecolors(builder,face_color_offset)
        DotBimBufferElement.AddGuid(builder,guid_vector)
        DotBimBufferElement.AddRotation(builder,rotation_offset)
        DotBimBufferElement.AddVector(builder,vector_offset)
        DotBimBufferElement.AddMeshid(builder,element.mesh_id)
        return DotBimBufferElement.End(builder)
        
        
class DeCompressDotBimFile:
    def __init__(self,compressed_bytes):
        if not isinstance(compressed_bytes, bytes):
            raise TypeError("input must be of type 'bytes'.") 
        self.compressed_bytes=compressed_bytes     
         
    def de_compress(self):
        buffer= DotBimSchemaBuffer.GetRootAs(self.compressed_bytes)
        schema_version=buffer.Schemaversion().decode('utf-8')
        info=json.loads(buffer.Info().decode('utf-8'))
        meshes = []
        for i in range(buffer.MeshesLength()):
            mesh_buffer = buffer.Meshes(i)
            mesh_id = mesh_buffer.Meshid()
            coordinates=[] 
            for i in range(mesh_buffer.CoordinatesLength()):
                coordinates.append(mesh_buffer.Coordinates(i))
            indices=[]
            for i in range(mesh_buffer.IndicesLength()):
                indices.append(mesh_buffer.Indices(i))
            mesh = Mesh(mesh_id,coordinates,indices)
            meshes.append(mesh)

        elements = []
        for i in range(buffer.ElementsLength()):
            element_buffer = buffer.Elements(i)
            type = element_buffer.Type().decode('utf-8')
            guid = element_buffer.Guid().decode('utf-8')
            info = {}
            for i in range(element_buffer.InfoLength()):
                info_entry = element_buffer.Info(i)
                key = info_entry.Key().decode('utf-8')
                value = info_entry.Value().decode('utf-8')
                info[key] = value
            color_buffer = element_buffer.Color()
            color = Color(r=color_buffer.R(), g=color_buffer.G(), b=color_buffer.B(), a=color_buffer.A())
            face_colors=[]
            for i in range(element_buffer.FacecolorsLength()):
                face_colors.append(element_buffer.Facecolors(i))
            
            rotation_buffer = element_buffer.Rotation()
            rotation = Rotation(qx=rotation_buffer.Qx(), qy=rotation_buffer.Qy(), qz=rotation_buffer.Qz(), qw=rotation_buffer.Qw())
            vector_buffer = element_buffer.Vector()
            vector = Vector(x=vector_buffer.X(), y=vector_buffer.Y(), z=vector_buffer.Z())
            mesh_id = element_buffer.Meshid()
      
            element = Element(
                mesh_id=mesh_id,
                vector=vector,
                rotation=rotation,
                guid=guid,
                type=type,
                info=info,
                face_colors=face_colors,color=color
                )
            elements.append(element)

        file = File(schema_version,meshes,elements,info)
        return file
        
        

