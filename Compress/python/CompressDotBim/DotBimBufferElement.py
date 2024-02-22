# automatically generated by the FlatBuffers compiler, do not modify

# namespace: CompressDotBim

import flatbuffers
from flatbuffers.compat import import_numpy
np = import_numpy()

class DotBimBufferElement(object):
    __slots__ = ['_tab']

    @classmethod
    def GetRootAs(cls, buf, offset=0):
        n = flatbuffers.encode.Get(flatbuffers.packer.uoffset, buf, offset)
        x = DotBimBufferElement()
        x.Init(buf, n + offset)
        return x

    @classmethod
    def GetRootAsDotBimBufferElement(cls, buf, offset=0):
        """This method is deprecated. Please switch to GetRootAs."""
        return cls.GetRootAs(buf, offset)
    # DotBimBufferElement
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # DotBimBufferElement
    def Type(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(4))
        if o != 0:
            return self._tab.String(o + self._tab.Pos)
        return None

    # DotBimBufferElement
    def Info(self, j):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(6))
        if o != 0:
            x = self._tab.Vector(o)
            x += flatbuffers.number_types.UOffsetTFlags.py_type(j) * 4
            x = self._tab.Indirect(x)
            from CompressDotBim.DotBimBufferInfo import DotBimBufferInfo
            obj = DotBimBufferInfo()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # DotBimBufferElement
    def InfoLength(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(6))
        if o != 0:
            return self._tab.VectorLen(o)
        return 0

    # DotBimBufferElement
    def InfoIsNone(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(6))
        return o == 0

    # DotBimBufferElement
    def Color(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(8))
        if o != 0:
            x = self._tab.Indirect(o + self._tab.Pos)
            from CompressDotBim.DotBimBufferColor import DotBimBufferColor
            obj = DotBimBufferColor()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # DotBimBufferElement
    def Facecolors(self, j):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(10))
        if o != 0:
            a = self._tab.Vector(o)
            return self._tab.Get(flatbuffers.number_types.Int32Flags, a + flatbuffers.number_types.UOffsetTFlags.py_type(j * 4))
        return 0

    # DotBimBufferElement
    def FacecolorsAsNumpy(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(10))
        if o != 0:
            return self._tab.GetVectorAsNumpy(flatbuffers.number_types.Int32Flags, o)
        return 0

    # DotBimBufferElement
    def FacecolorsLength(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(10))
        if o != 0:
            return self._tab.VectorLen(o)
        return 0

    # DotBimBufferElement
    def FacecolorsIsNone(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(10))
        return o == 0

    # DotBimBufferElement
    def Guid(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(12))
        if o != 0:
            return self._tab.String(o + self._tab.Pos)
        return None

    # DotBimBufferElement
    def Rotation(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(14))
        if o != 0:
            x = self._tab.Indirect(o + self._tab.Pos)
            from CompressDotBim.DotBimBufferRotation import DotBimBufferRotation
            obj = DotBimBufferRotation()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # DotBimBufferElement
    def Vector(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(16))
        if o != 0:
            x = self._tab.Indirect(o + self._tab.Pos)
            from CompressDotBim.DotBimBufferVector import DotBimBufferVector
            obj = DotBimBufferVector()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # DotBimBufferElement
    def Meshid(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(18))
        if o != 0:
            return self._tab.Get(flatbuffers.number_types.Int32Flags, o + self._tab.Pos)
        return 0

def DotBimBufferElementStart(builder):
    builder.StartObject(8)

def Start(builder):
    DotBimBufferElementStart(builder)

def DotBimBufferElementAddType(builder, type):
    builder.PrependUOffsetTRelativeSlot(0, flatbuffers.number_types.UOffsetTFlags.py_type(type), 0)

def AddType(builder, type):
    DotBimBufferElementAddType(builder, type)

def DotBimBufferElementAddInfo(builder, info):
    builder.PrependUOffsetTRelativeSlot(1, flatbuffers.number_types.UOffsetTFlags.py_type(info), 0)

def AddInfo(builder, info):
    DotBimBufferElementAddInfo(builder, info)

def DotBimBufferElementStartInfoVector(builder, numElems):
    return builder.StartVector(4, numElems, 4)

def StartInfoVector(builder, numElems: int) -> int:
    return DotBimBufferElementStartInfoVector(builder, numElems)

def DotBimBufferElementAddColor(builder, color):
    builder.PrependUOffsetTRelativeSlot(2, flatbuffers.number_types.UOffsetTFlags.py_type(color), 0)

def AddColor(builder, color):
    DotBimBufferElementAddColor(builder, color)

def DotBimBufferElementAddFacecolors(builder, facecolors):
    builder.PrependUOffsetTRelativeSlot(3, flatbuffers.number_types.UOffsetTFlags.py_type(facecolors), 0)

def AddFacecolors(builder, facecolors):
    DotBimBufferElementAddFacecolors(builder, facecolors)

def DotBimBufferElementStartFacecolorsVector(builder, numElems):
    return builder.StartVector(4, numElems, 4)

def StartFacecolorsVector(builder, numElems: int) -> int:
    return DotBimBufferElementStartFacecolorsVector(builder, numElems)

def DotBimBufferElementAddGuid(builder, guid):
    builder.PrependUOffsetTRelativeSlot(4, flatbuffers.number_types.UOffsetTFlags.py_type(guid), 0)

def AddGuid(builder, guid):
    DotBimBufferElementAddGuid(builder, guid)

def DotBimBufferElementAddRotation(builder, rotation):
    builder.PrependUOffsetTRelativeSlot(5, flatbuffers.number_types.UOffsetTFlags.py_type(rotation), 0)

def AddRotation(builder, rotation):
    DotBimBufferElementAddRotation(builder, rotation)

def DotBimBufferElementAddVector(builder, vector):
    builder.PrependUOffsetTRelativeSlot(6, flatbuffers.number_types.UOffsetTFlags.py_type(vector), 0)

def AddVector(builder, vector):
    DotBimBufferElementAddVector(builder, vector)

def DotBimBufferElementAddMeshid(builder, meshid):
    builder.PrependInt32Slot(7, meshid, 0)

def AddMeshid(builder, meshid):
    DotBimBufferElementAddMeshid(builder, meshid)

def DotBimBufferElementEnd(builder):
    return builder.EndObject()

def End(builder):
    return DotBimBufferElementEnd(builder)