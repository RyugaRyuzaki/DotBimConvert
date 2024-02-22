// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace CompressDotBim
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct DotBimBufferMeshes : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static DotBimBufferMeshes GetRootAsDotBimBufferMeshes(ByteBuffer _bb) { return GetRootAsDotBimBufferMeshes(_bb, new DotBimBufferMeshes()); }
  public static DotBimBufferMeshes GetRootAsDotBimBufferMeshes(ByteBuffer _bb, DotBimBufferMeshes obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public DotBimBufferMeshes __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Meshid { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public float Coordinates(int j) { int o = __p.__offset(6); return o != 0 ? __p.bb.GetFloat(__p.__vector(o) + j * 4) : (float)0; }
  public int CoordinatesLength { get { int o = __p.__offset(6); return o != 0 ? __p.__vector_len(o) : 0; } }
#if ENABLE_SPAN_T
  public Span<float> GetCoordinatesBytes() { return __p.__vector_as_span<float>(6, 4); }
#else
  public ArraySegment<byte>? GetCoordinatesBytes() { return __p.__vector_as_arraysegment(6); }
#endif
  public float[] GetCoordinatesArray() { return __p.__vector_as_array<float>(6); }
  public int Indices(int j) { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(__p.__vector(o) + j * 4) : (int)0; }
  public int IndicesLength { get { int o = __p.__offset(8); return o != 0 ? __p.__vector_len(o) : 0; } }
#if ENABLE_SPAN_T
  public Span<int> GetIndicesBytes() { return __p.__vector_as_span<int>(8, 4); }
#else
  public ArraySegment<byte>? GetIndicesBytes() { return __p.__vector_as_arraysegment(8); }
#endif
  public int[] GetIndicesArray() { return __p.__vector_as_array<int>(8); }

  public static Offset<CompressDotBim.DotBimBufferMeshes> CreateDotBimBufferMeshes(FlatBufferBuilder builder,
      int meshid = 0,
      VectorOffset coordinatesOffset = default(VectorOffset),
      VectorOffset indicesOffset = default(VectorOffset)) {
    builder.StartTable(3);
    DotBimBufferMeshes.AddIndices(builder, indicesOffset);
    DotBimBufferMeshes.AddCoordinates(builder, coordinatesOffset);
    DotBimBufferMeshes.AddMeshid(builder, meshid);
    return DotBimBufferMeshes.EndDotBimBufferMeshes(builder);
  }

  public static void StartDotBimBufferMeshes(FlatBufferBuilder builder) { builder.StartTable(3); }
  public static void AddMeshid(FlatBufferBuilder builder, int meshid) { builder.AddInt(0, meshid, 0); }
  public static void AddCoordinates(FlatBufferBuilder builder, VectorOffset coordinatesOffset) { builder.AddOffset(1, coordinatesOffset.Value, 0); }
  public static VectorOffset CreateCoordinatesVector(FlatBufferBuilder builder, float[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddFloat(data[i]); return builder.EndVector(); }
  public static VectorOffset CreateCoordinatesVectorBlock(FlatBufferBuilder builder, float[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateCoordinatesVectorBlock(FlatBufferBuilder builder, ArraySegment<float> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateCoordinatesVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<float>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartCoordinatesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddIndices(FlatBufferBuilder builder, VectorOffset indicesOffset) { builder.AddOffset(2, indicesOffset.Value, 0); }
  public static VectorOffset CreateIndicesVector(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddInt(data[i]); return builder.EndVector(); }
  public static VectorOffset CreateIndicesVectorBlock(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateIndicesVectorBlock(FlatBufferBuilder builder, ArraySegment<int> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateIndicesVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<int>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartIndicesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<CompressDotBim.DotBimBufferMeshes> EndDotBimBufferMeshes(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<CompressDotBim.DotBimBufferMeshes>(o);
  }
}


static public class DotBimBufferMeshesVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Meshid*/, 4 /*int*/, 4, false)
      && verifier.VerifyVectorOfData(tablePos, 6 /*Coordinates*/, 4 /*float*/, false)
      && verifier.VerifyVectorOfData(tablePos, 8 /*Indices*/, 4 /*int*/, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}