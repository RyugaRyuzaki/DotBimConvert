// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace CompressDotBim
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct DotBimBufferRotation : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static DotBimBufferRotation GetRootAsDotBimBufferRotation(ByteBuffer _bb) { return GetRootAsDotBimBufferRotation(_bb, new DotBimBufferRotation()); }
  public static DotBimBufferRotation GetRootAsDotBimBufferRotation(ByteBuffer _bb, DotBimBufferRotation obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public DotBimBufferRotation __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public float Qx { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; } }
  public float Qy { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; } }
  public float Qz { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; } }
  public float Qw { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; } }

  public static Offset<CompressDotBim.DotBimBufferRotation> CreateDotBimBufferRotation(FlatBufferBuilder builder,
      float qx = 0.0f,
      float qy = 0.0f,
      float qz = 0.0f,
      float qw = 0.0f) {
    builder.StartTable(4);
    DotBimBufferRotation.AddQw(builder, qw);
    DotBimBufferRotation.AddQz(builder, qz);
    DotBimBufferRotation.AddQy(builder, qy);
    DotBimBufferRotation.AddQx(builder, qx);
    return DotBimBufferRotation.EndDotBimBufferRotation(builder);
  }

  public static void StartDotBimBufferRotation(FlatBufferBuilder builder) { builder.StartTable(4); }
  public static void AddQx(FlatBufferBuilder builder, float qx) { builder.AddFloat(0, qx, 0.0f); }
  public static void AddQy(FlatBufferBuilder builder, float qy) { builder.AddFloat(1, qy, 0.0f); }
  public static void AddQz(FlatBufferBuilder builder, float qz) { builder.AddFloat(2, qz, 0.0f); }
  public static void AddQw(FlatBufferBuilder builder, float qw) { builder.AddFloat(3, qw, 0.0f); }
  public static Offset<CompressDotBim.DotBimBufferRotation> EndDotBimBufferRotation(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<CompressDotBim.DotBimBufferRotation>(o);
  }
}


static public class DotBimBufferRotationVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Qx*/, 4 /*float*/, 4, false)
      && verifier.VerifyField(tablePos, 6 /*Qy*/, 4 /*float*/, 4, false)
      && verifier.VerifyField(tablePos, 8 /*Qz*/, 4 /*float*/, 4, false)
      && verifier.VerifyField(tablePos, 10 /*Qw*/, 4 /*float*/, 4, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}