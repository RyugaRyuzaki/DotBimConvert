// automatically generated by the FlatBuffers compiler, do not modify

/* eslint-disable @typescript-eslint/no-unused-vars, @typescript-eslint/no-explicit-any, @typescript-eslint/no-non-null-assertion */

import * as flatbuffers from 'flatbuffers';

import { DotBimBufferElement } from './dot-bim-buffer-element';
import { DotBimBufferMeshes } from './dot-bim-buffer-meshes';


export class DotBimSchemaBuffer {
  bb: flatbuffers.ByteBuffer | null = null;
  bb_pos = 0;
  __init( i: number, bb: flatbuffers.ByteBuffer ): DotBimSchemaBuffer {
    this.bb_pos = i;
    this.bb = bb;
    return this;
  }

  static getRootAsDotBimSchemaBuffer( bb: flatbuffers.ByteBuffer, obj?: DotBimSchemaBuffer ): DotBimSchemaBuffer {
    return ( obj || new DotBimSchemaBuffer() ).__init( bb.readInt32( bb.position() ) + bb.position(), bb );
  }

  static getSizePrefixedRootAsDotBimSchemaBuffer( bb: flatbuffers.ByteBuffer, obj?: DotBimSchemaBuffer ): DotBimSchemaBuffer {
    bb.setPosition( bb.position() + flatbuffers.SIZE_PREFIX_LENGTH );
    return ( obj || new DotBimSchemaBuffer() ).__init( bb.readInt32( bb.position() ) + bb.position(), bb );
  }

  schemaversion(): string | null
  schemaversion( optionalEncoding: flatbuffers.Encoding ): string | Uint8Array | null
  schemaversion( optionalEncoding?: any ): string | Uint8Array | null {
    const offset = this.bb!.__offset( this.bb_pos, 4 );
    return offset ? this.bb!.__string( this.bb_pos + offset, optionalEncoding ) : null;
  }

  info(): string | null
  info( optionalEncoding: flatbuffers.Encoding ): string | Uint8Array | null
  info( optionalEncoding?: any ): string | Uint8Array | null {
    const offset = this.bb!.__offset( this.bb_pos, 6 );
    return offset ? this.bb!.__string( this.bb_pos + offset, optionalEncoding ) : null;
  }

  meshes( index: number, obj?: DotBimBufferMeshes ): DotBimBufferMeshes | null {
    const offset = this.bb!.__offset( this.bb_pos, 8 );
    return offset ? ( obj || new DotBimBufferMeshes() ).__init( this.bb!.__indirect( this.bb!.__vector( this.bb_pos + offset ) + index * 4 ), this.bb! ) : null;
  }

  meshesLength(): number {
    const offset = this.bb!.__offset( this.bb_pos, 8 );
    return offset ? this.bb!.__vector_len( this.bb_pos + offset ) : 0;
  }

  elements( index: number, obj?: DotBimBufferElement ): DotBimBufferElement | null {
    const offset = this.bb!.__offset( this.bb_pos, 10 );
    return offset ? ( obj || new DotBimBufferElement() ).__init( this.bb!.__indirect( this.bb!.__vector( this.bb_pos + offset ) + index * 4 ), this.bb! ) : null;
  }

  elementsLength(): number {
    const offset = this.bb!.__offset( this.bb_pos, 10 );
    return offset ? this.bb!.__vector_len( this.bb_pos + offset ) : 0;
  }

  static startDotBimSchemaBuffer( builder: flatbuffers.Builder ) {
    builder.startObject( 4 );
  }

  static addSchemaversion( builder: flatbuffers.Builder, schemaversionOffset: flatbuffers.Offset ) {
    builder.addFieldOffset( 0, schemaversionOffset, 0 );
  }

  static addInfo( builder: flatbuffers.Builder, infoOffset: flatbuffers.Offset ) {
    builder.addFieldOffset( 1, infoOffset, 0 );
  }

  static addMeshes( builder: flatbuffers.Builder, meshesOffset: flatbuffers.Offset ) {
    builder.addFieldOffset( 2, meshesOffset, 0 );
  }

  static createMeshesVector( builder: flatbuffers.Builder, data: flatbuffers.Offset[] ): flatbuffers.Offset {
    builder.startVector( 4, data.length, 4 );
    for ( let i = data.length - 1; i >= 0; i-- ) {
      builder.addOffset( data[i]! );
    }
    return builder.endVector();
  }

  static startMeshesVector( builder: flatbuffers.Builder, numElems: number ) {
    builder.startVector( 4, numElems, 4 );
  }

  static addElements( builder: flatbuffers.Builder, elementsOffset: flatbuffers.Offset ) {
    builder.addFieldOffset( 3, elementsOffset, 0 );
  }

  static createElementsVector( builder: flatbuffers.Builder, data: flatbuffers.Offset[] ): flatbuffers.Offset {
    builder.startVector( 4, data.length, 4 );
    for ( let i = data.length - 1; i >= 0; i-- ) {
      builder.addOffset( data[i]! );
    }
    return builder.endVector();
  }

  static startElementsVector( builder: flatbuffers.Builder, numElems: number ) {
    builder.startVector( 4, numElems, 4 );
  }

  static endDotBimSchemaBuffer( builder: flatbuffers.Builder ): flatbuffers.Offset {
    const offset = builder.endObject();
    return offset;
  }

  static finishDotBimSchemaBufferBuffer( builder: flatbuffers.Builder, offset: flatbuffers.Offset ) {
    builder.finish( offset );
  }

  static finishSizePrefixedDotBimSchemaBufferBuffer( builder: flatbuffers.Builder, offset: flatbuffers.Offset ) {
    builder.finish( offset, undefined, true );
  }

  static createDotBimSchemaBuffer( builder: flatbuffers.Builder, schemaversionOffset: flatbuffers.Offset, infoOffset: flatbuffers.Offset, meshesOffset: flatbuffers.Offset, elementsOffset: flatbuffers.Offset ): flatbuffers.Offset {
    DotBimSchemaBuffer.startDotBimSchemaBuffer( builder );
    DotBimSchemaBuffer.addSchemaversion( builder, schemaversionOffset );
    DotBimSchemaBuffer.addInfo( builder, infoOffset );
    DotBimSchemaBuffer.addMeshes( builder, meshesOffset );
    DotBimSchemaBuffer.addElements( builder, elementsOffset );
    return DotBimSchemaBuffer.endDotBimSchemaBuffer( builder );
  }
}
