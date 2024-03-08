import * as THREE from 'three'

export interface IBufferGeometry {
    buffer: THREE.BufferGeometry;
    matrix: THREE.Matrix4
}

export interface IIfcGeometry {
    [colID: string]: {
        dotBimColor: IDotBimColor;
        buffers: IBufferGeometry[]
    }
}
// mesh
export interface IDotBimMeshes {
    mesh_id: number,
    coordinates: number[],
    indices: number[],
}

// element items
export interface IDotBimInfo {
    id: number,
    type: string,
    GlobalId: string,
    Name: string | "None",
    Description: string | "None",
    ObjectType: string | "None",
    Tag: string | "None",
    PredefinedType: string | "None",
}
export interface IDotBimColor {
    r: number,
    g: number,
    b: number,
    a: number,
}
export interface IDotBimVector {
    x: number,
    y: number,
    z: number,
}

export interface IDotBimRotation {
    qx: number,
    qy: number,
    qz: number,
    qw: number,
}
export interface IDotBimElement {
    type: string,
    info: IDotBimInfo,
    color?: IDotBimColor,
    face_colors?: number[],
    guid: string,
    rotation: IDotBimRotation,
    vector: IDotBimVector,
    mesh_id: number,
}