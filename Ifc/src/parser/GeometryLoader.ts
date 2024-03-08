import * as WebIFC from "web-ifc"
import { IBufferGeometry, IDotBimColor, IIfcGeometry } from "./type";
import * as THREE from 'three'


export class GeometryReader {

    items: { [expressID: number]: { ids: number[], geometry: IIfcGeometry } } = {}
    /**
     *
     */
    constructor( private api: WebIFC.IfcAPI ) {

    }


    async streamMesh( modelID: number, mesh: WebIFC.FlatMesh ) {
        const placedGeometries = mesh.geometries;
        const size = placedGeometries.size();
        const expressID = mesh.expressID
        for ( let i = 0; i < size; i++ ) {
            const placedGeometry = placedGeometries.get( i );
            const color = placedGeometry.color
            const colID = `${color.x}-${color.y}-${color.z}-${color.w}`;
            const dotBimColor = GeometryReader.convertColor( color )
            if ( !this.items[expressID] ) this.items[expressID] = { ids: [], geometry: {} }
            this.items[expressID].ids.push( placedGeometry.geometryExpressID )
            if ( !this.items[expressID].geometry[colID] ) this.items[expressID].geometry[colID] = { dotBimColor, buffers: [] as IBufferGeometry[] }
            const buffer = this.newBufferGeometry( modelID, placedGeometry.geometryExpressID, );
            if ( !buffer ) continue
            const matrix = new THREE.Matrix4().fromArray( placedGeometry.flatTransformation )
            this.items[expressID].geometry[colID].buffers.push( { buffer, matrix } )
        }
    }


    private newBufferGeometry( modelID: number, geometryExpressID: number ) {
        const geometry = this.api.GetGeometry( modelID, geometryExpressID );
        const verts = this.getVertices( geometry );
        if ( !verts.length ) return null;
        const indices = this.getIndices( geometry );
        if ( !indices.length ) return null;
        const buffer = this.constructBuffer( verts, indices );
        // transform geometry to origin coordination
        // const matrix4 = new THREE.Matrix4().fromArray( matrix )
        // buffer.applyMatrix4( matrix4 )
        // @ts-ignore
        geometry.delete();
        return buffer;
    }
    private getIndices( geometryData: WebIFC.IfcGeometry ) {
        const indices = this.api.GetIndexArray(
            geometryData.GetIndexData(),
            geometryData.GetIndexDataSize()
        ) as Uint32Array;
        return indices;
    }

    private getVertices( geometryData: WebIFC.IfcGeometry ) {
        const verts = this.api.GetVertexArray(
            geometryData.GetVertexData(),
            geometryData.GetVertexDataSize()
        ) as Float32Array;
        return verts;
    }

    private constructBuffer( vertexData: Float32Array, indexData: Uint32Array ) {
        const geometry = new THREE.BufferGeometry();

        const posFloats = new Float32Array( vertexData.length / 2 );
        const normFloats = new Float32Array( vertexData.length / 2 );

        for ( let i = 0; i < vertexData.length; i += 6 ) {
            posFloats[i / 2] = vertexData[i];
            posFloats[i / 2 + 1] = vertexData[i + 1];
            posFloats[i / 2 + 2] = vertexData[i + 2];

            normFloats[i / 2] = vertexData[i + 3];
            normFloats[i / 2 + 1] = vertexData[i + 4];
            normFloats[i / 2 + 2] = vertexData[i + 5];
        }

        geometry.setAttribute( "position", new THREE.BufferAttribute( posFloats, 3 ) );
        geometry.setAttribute( "normal", new THREE.BufferAttribute( normFloats, 3 ) );
        geometry.setIndex( new THREE.BufferAttribute( indexData, 1 ) );

        return geometry;
    }

    private static convertColor( color: WebIFC.Color ): IDotBimColor {
        const { x, y, z, w } = color
        const r = Math.ceil( x * 255 )
        const g = Math.ceil( y * 255 )
        const b = Math.ceil( z * 255 )
        const a = Math.ceil( w * 255 )
        return { r, g, b, a } as IDotBimColor
    }
}