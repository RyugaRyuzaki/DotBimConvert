import * as flatbuffers from "flatbuffers";

import * as FBS from "./Schema";
import { IDotBimColor, IDotBimElement, IDotBimMeshes, IDotBimRotation, IDotBimVector } from "../type";

export class Serializer {
  static deCompress( buffer: Uint8Array ) {
    const bytes = new flatbuffers.ByteBuffer( buffer );
    const data = FBS.DotBimSchemaBuffer.getRootAsDotBimSchemaBuffer( bytes );
    const schema_version = data.schemaversion()
    const info = JSON.parse( data.info()! )
    const meshes: IDotBimMeshes[] = []
    for ( let i = 0; i < data.meshesLength(); i++ ) {
      meshes.push( Serializer.deCompressDotBimBufferMeshes( data.meshes( i )! ) )
    }
    const elements: IDotBimElement[] = []
    for ( let i = 0; i < data.elementsLength(); i++ ) {
      elements.push( Serializer.deCompressDotBimBufferElement( data.elements( i )! ) )
    }
    return JSON.stringify( { schema_version, info, meshes, elements } )
  }
  static compress( {
    schema_version,
    info,
    meshes,
    elements,
  }:
    {
      schema_version: string,
      info: any,
      meshes: IDotBimMeshes[],
      elements: IDotBimElement[],
    } ) {
    const builder = new flatbuffers.Builder( 1024 );
    const schemaversionVector = builder.createString( schema_version )
    const infoVector = builder.createString( JSON.stringify( info ) )
    const meshesCount = meshes.map( ( mesh: IDotBimMeshes ) => ( Serializer.compressDotBimBufferMeshes( builder, mesh ) ) )
    const meshesVector = FBS.DotBimSchemaBuffer.createMeshesVector( builder, meshesCount )
    const elementsCount = elements.map( ( element: IDotBimElement ) => ( Serializer.compressDotBimBufferElement( builder, element ) ) )
    const elementsVector = FBS.DotBimSchemaBuffer.createElementsVector( builder, elementsCount )
    const result = FBS.DotBimSchemaBuffer.createDotBimSchemaBuffer( builder, schemaversionVector, infoVector, meshesVector, elementsVector )
    builder.finish( result )
    return builder.asUint8Array();
  }

  private static compressDotBimBufferMeshes( builder: flatbuffers.Builder, mesh: IDotBimMeshes ) {
    const { mesh_id, coordinates, indices } = mesh
    const coordinatesVector = FBS.DotBimBufferMeshes.createCoordinatesVector( builder, coordinates )
    const indicesVector = FBS.DotBimBufferMeshes.createCoordinatesVector( builder, indices )
    return FBS.DotBimBufferMeshes.createDotBimBufferMeshes( builder, mesh_id, coordinatesVector, indicesVector )
  }
  private static deCompressDotBimBufferMeshes( meshBuffer: FBS.DotBimBufferMeshes ) {
    const mesh_id = meshBuffer.meshid()!
    const coordinates = Array.from( meshBuffer.coordinatesArray()! )
    const indices = Array.from( meshBuffer.indicesArray()! )
    return { mesh_id, coordinates, indices } as IDotBimMeshes
  }
  private static compressDotBimBufferColor( builder: flatbuffers.Builder, color: IDotBimColor ) {
    const { r, g, b, a } = color
    return FBS.DotBimBufferColor.createDotBimBufferColor( builder, r, g, b, a )
  }
  private static deCompressDotBimBufferColor( colorBuffer: FBS.DotBimBufferColor ) {
    const r = colorBuffer.r()
    const g = colorBuffer.g()
    const b = colorBuffer.b()
    const a = colorBuffer.a()
    return { r, g, b, a } as IDotBimColor
  }
  private static compressDotBimBufferVector( builder: flatbuffers.Builder, vector: IDotBimVector ) {
    const { x, y, z } = vector
    return FBS.DotBimBufferVector.createDotBimBufferVector( builder, x, y, z )
  }
  private static deCompressDotBimBufferVector( vectorBuffer: FBS.DotBimBufferVector ) {
    const x = vectorBuffer.x()
    const y = vectorBuffer.y()
    const z = vectorBuffer.z()
    return { x, y, z } as IDotBimVector
  }
  private static compressDotBimBufferRotation( builder: flatbuffers.Builder, rotation: IDotBimRotation ) {
    const { qx, qy, qz, qw } = rotation
    return FBS.DotBimBufferRotation.createDotBimBufferRotation( builder, qx, qy, qz, qw )
  }
  private static deCompressDotBimBufferRotation( rotationBuffer: FBS.DotBimBufferRotation ) {
    const qx = rotationBuffer.qx()
    const qy = rotationBuffer.qy()
    const qz = rotationBuffer.qz()
    const qw = rotationBuffer.qw()
    return { qx, qy, qz, qw } as IDotBimRotation
  }
  private static compressDotBimBufferInfo( builder: flatbuffers.Builder, item: { key: string, value: string } ) {
    const keyVector = builder.createString( item.key )
    const valueVector = builder.createString( item.value )
    return FBS.DotBimBufferInfo.createDotBimBufferInfo( builder, keyVector, valueVector )
  }
  private static deCompressDotBimBufferInfo( itemBuffer: FBS.DotBimBufferInfo ) {
    const key = itemBuffer.key()
    const value = itemBuffer.value()
    return { key, value }
  }
  private static compressDotBimBufferElement( builder: flatbuffers.Builder, element: IDotBimElement ) {
    const { type, info, color, face_colors, guid, rotation, vector, mesh_id } = element
    const typeVector = builder.createString( type )
    const guidVector = builder.createString( guid )
    const colorVector = Serializer.compressDotBimBufferColor( builder, color )
    const rotationVector = Serializer.compressDotBimBufferRotation( builder, rotation )
    const vectorVector = Serializer.compressDotBimBufferVector( builder, vector )
    const infoCount = Object.keys( info ).map( ( key: string ) => {
      //@ts-ignore
      const value = info[key]
      return Serializer.compressDotBimBufferInfo( builder, { key, value } )
    } )
    const infoVector = FBS.DotBimBufferElement.createInfoVector( builder, infoCount )
    const facecolorsVector = FBS.DotBimBufferElement.createFacecolorsVector( builder, face_colors )
    FBS.DotBimBufferElement.startDotBimBufferElement( builder )
    FBS.DotBimBufferElement.addType( builder, typeVector )
    FBS.DotBimBufferElement.addInfo( builder, infoVector )
    FBS.DotBimBufferElement.addColor( builder, colorVector )
    FBS.DotBimBufferElement.addFacecolors( builder, facecolorsVector )
    FBS.DotBimBufferElement.addGuid( builder, guidVector )
    FBS.DotBimBufferElement.addRotation( builder, rotationVector )
    FBS.DotBimBufferElement.addVector( builder, vectorVector )
    FBS.DotBimBufferElement.addMeshid( builder, mesh_id )
    return FBS.DotBimBufferElement.endDotBimBufferElement( builder )
  }
  private static deCompressDotBimBufferElement( elementBuffer: FBS.DotBimBufferElement ) {
    const type = elementBuffer.type()
    const info: any = {}
    for ( let i = 0; i < elementBuffer.infoLength(); i++ ) {
      const item = elementBuffer.info( i )
      if ( !item ) continue
      const key = item.key()!
      const value = item.value()!
      if ( !info[key] ) info[key] = value
    }
    const color = Serializer.deCompressDotBimBufferColor( elementBuffer.color()! )
    const face_colors = Array.from( elementBuffer.facecolorsArray()! )
    const guid = elementBuffer.guid()
    const rotation = Serializer.deCompressDotBimBufferRotation( elementBuffer.rotation()! )
    const vector = Serializer.deCompressDotBimBufferVector( elementBuffer.vector()! )
    const mesh_id = elementBuffer.meshid()!
    return { type, info, color, face_colors, guid, rotation, vector, mesh_id } as IDotBimElement
  }


}