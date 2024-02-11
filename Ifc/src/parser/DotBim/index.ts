import * as WebIFC from "web-ifc"
import * as THREE from 'three'
import { v4 as uuidv4 } from 'uuid';
import { IDotBimColor, IDotBimElement, IDotBimInfo, IDotBimMeshes, IDotBimRotation, IDotBimVector, IIfcGeometry } from "../type";
import { mergeBufferGeometries } from 'three-stdlib'


export * from './Serializer'

const dotBimInfo = {
    Description: "",
    Author: "https://github.com/RyugaRyuzaki",
    OriginAuthor: "https://github.com/paireks"
}
const propName = {
    name: WebIFC.IFCRELDEFINESBYPROPERTIES,
    relating: "RelatingPropertyDefinition",
    related: "RelatedObjects",
    key: "IsDefinedBy",
};
interface ICategory {
    meshes: { [expressID: number | string]: IDotBimMeshes }
    elements: { [expressID: number | string]: IDotBimElement }
}

export class DotBim {
    private meshes: { [expressID: number | string]: IDotBimMeshes } = {}
    private info: any = dotBimInfo
    private elements: { [expressID: number | string]: IDotBimElement } = {}
    private allProps: any[] = [];
    private categories: { [type: string]: ICategory } = {}
    /**
     *
     */
    constructor( private api: WebIFC.IfcAPI, private schema_version = "1.0.0", private allOption = true ) {
    }

    converterCategories() {
        const result: any = {}
        result.schema_version = this.schema_version
        result.info = this.info
        result.categories = this.categories
        return result
    }
    converterAll() {
        const result: any = {}
        result.schema_version = this.schema_version
        result.info = this.info
        result.meshes = this.meshes
        result.elements = this.elements
        return result
    }
    async getIfcMetadata( modelID: number ) {
        const { FILE_NAME, FILE_DESCRIPTION } = WebIFC;
        this.info.Name = this.getMetadataEntry( modelID, FILE_NAME );
        this.info.Description = this.getMetadataEntry( modelID, FILE_DESCRIPTION );
        this.info.Schema = this.api.GetModelSchema( modelID ) || "IFC2X3";
        this.info.ProjectID = this.getProjectID( modelID )
        await this.getRel( modelID )
    }
    private async getRel( modelID: number ) {
        const relProps = await this.api.GetLineIDsWithType( modelID, propName.name )
        const size = relProps.size();
        for ( let index = 0; index < size; index++ ) {
            const id = relProps.get( index )
            const rel = await this.api.GetLine( modelID, id )
            if ( !rel[propName.related] || !rel[propName.relating].value ) continue
            let propIds = rel[propName.related];
            if ( !Array.isArray( propIds ) ) propIds = [propIds];
            const relElementIds = propIds.map( ( e: any ) => e.value )
            if ( relElementIds.length > 0 ) {
                const propExpressID = rel[propName.relating]?.value
                if ( !propExpressID ) continue
                const props = await this.api.GetLine( modelID, propExpressID )
                if ( !props.Name ) continue
                const Name = props.Name.value
                if ( !Name ) continue
                const propElement: any = {}
                if ( props.HasProperties ) {
                    if ( !Array.isArray( props.HasProperties ) ) props.HasProperties = [props.HasProperties];
                    for ( let i = 0; i < props.HasProperties.length; i++ ) {
                        const has = props.HasProperties[i];
                        const hasProp = await this.api.GetLine( modelID, has.value )
                        if ( hasProp.Name && hasProp.Name.value ) {
                            const key = `${Name} ${hasProp.Name.value}`
                            const value = hasProp.NominalValue?.value || "None"
                            if ( !propElement[key] ) propElement[key] = value
                        }
                    }
                }
                if ( props.Quantities ) {
                    if ( !Array.isArray( props.Quantities ) ) props.Quantities = [props.Quantities];

                    for ( let i = 0; i < props.Quantities.length; i++ ) {
                        const has = props.Quantities[i];
                        const hasProp = await this.api.GetLine( modelID, has.value )
                        if ( hasProp.Name && hasProp.Name.value ) {
                            const key = `${Name} ${hasProp.Name.value}`
                            const value = DotBim.getQuantityValue( hasProp )
                            if ( !propElement[key] ) propElement[key] = value
                        }
                    }
                }
                if ( Object.keys( propElement ).length > 0 )
                    this.allProps.push( {
                        relElementIds,
                        props: propElement,
                    } );
            }
        }
    }

    private getProjectID( modelID: number ) {
        const projectsIDs = this.api.GetLineIDsWithType( modelID, WebIFC.IFCPROJECT );
        const projectID = projectsIDs.get( modelID );
        const project = this.api.GetLine( modelID, projectID );
        return project.GlobalId.value;
    }
    private getMetadataEntry( modelID: number, type: number ) {
        let description = "";
        const descriptionData = this.api.GetHeaderLine( modelID, type ) || "";
        if ( !descriptionData ) return description;
        for ( const arg of descriptionData.arguments ) {
            if ( arg === null || arg === undefined ) {
                continue;
            }
            if ( Array.isArray( arg ) ) {
                for ( const subArg of arg ) {
                    if ( !subArg ) continue;
                    description += `${subArg.value}|`;
                }
            } else {
                description += `${arg.value}|`;
            }
        }
        return description;
    }

    async getMeshes( modelID: number, items: { [expressID: number | string]: IIfcGeometry } ) {
        for ( const expressID in items ) {
            const geometry = items[expressID]

            const info = await this.getDotBimInfo( modelID, Number( expressID ) )
            const type = info.type
            // if ( type !== "IfcWallStandardCase" ) continue
            const vector = { x: 0, y: 0, z: 0 } as IDotBimVector
            const rotation = { qx: 0, qy: 0, qz: 0, qw: 1 } as IDotBimRotation
            const guid = uuidv4()
            const face_colors: number[] = []
            const geometries: THREE.BufferGeometry[] = []
            //if multiple meshes use average
            let r = 0, g = 0, b = 0, a = 0;
            const geometryLength = Object.keys( geometry ).length
            for ( const colID in geometry ) {
                const { dotBimColor, buffers } = geometry[colID]
                if ( buffers.length === 0 ) continue
                r += dotBimColor.r
                g += dotBimColor.g
                b += dotBimColor.b
                a += dotBimColor.a
                const combined = mergeBufferGeometries( buffers )
                if ( !combined || !combined.index ) continue
                const length = combined.index.array.length
                if ( length % 3 !== 0 ) continue
                for ( let i = 0; i < length; i += 3 ) {
                    face_colors.push( dotBimColor.r )
                    face_colors.push( dotBimColor.g )
                    face_colors.push( dotBimColor.b )
                    face_colors.push( dotBimColor.a )
                }
                geometries.push( combined )
                buffers.forEach( ( buf: THREE.BufferGeometry ) => buf.dispose() )
            }
            r /= geometryLength
            g /= geometryLength
            b /= geometryLength
            a /= geometryLength
            const color = { r, g, b, a } as IDotBimColor
            if ( geometries.length === 0 ) continue
            const newCombined = mergeBufferGeometries( geometries )
            if ( !newCombined || !newCombined.index ) continue

            const indices = Array.from( newCombined.index.array )
            // const pos = newCombined.attributes.position.array
            // const posItemSize = newCombined.attributes.position.itemSize
            // const coordinates: number[] = []
            // for ( let i = 0; i < pos.length; i += posItemSize ) {
            //     coordinates.push( pos[i] )
            //     coordinates.push( pos[i + 2] )
            //     coordinates.push( pos[i + 1] )
            // }

            const coordinates = Array.from( newCombined.attributes.position.array )
            const mesh_id = Number( expressID )
            const mesh = {
                mesh_id,
                coordinates,
                indices,
            } as IDotBimMeshes

            if ( this.allOption ) {
                if ( !this.meshes[expressID] ) this.meshes[expressID] = mesh
                if ( !this.elements[expressID] ) this.elements[expressID] = { type, info, color, face_colors, guid, rotation, vector, mesh_id } as IDotBimElement
            } else {
                if ( !this.categories[type] ) this.categories[type] = {
                    meshes: {},
                    elements: {},
                } as ICategory
                if ( !this.categories[type].meshes[expressID] ) this.categories[type].meshes[expressID] = mesh
                if ( !this.categories[type].elements[expressID] ) this.categories[type].elements[expressID] = { type, info, color, face_colors, guid, rotation, vector, mesh_id } as IDotBimElement
            }
            newCombined.dispose()
        }

    }


    private async getDotBimInfo( modelID: number, expressID: number ): Promise<IDotBimInfo> {
        const attributes = await this.api.GetLine( modelID, expressID );
        const id = attributes.expressID
        const type = this.api.GetNameFromTypeCode( attributes.type )
        const GlobalId = attributes.GlobalId ? attributes.GlobalId.value : "None"
        const Name = attributes.Name ? attributes.Name.value : "None"
        const Description = attributes.Description ? attributes.Description.value : "None"
        const ObjectType = attributes.ObjectType ? attributes.ObjectType.value : "None"
        const Tag = attributes.Tag ? attributes.Tag.value : "None"
        const PredefinedType = attributes.PredefinedType ? attributes.PredefinedType.value : "None"
        const PropertySet = this.getProp( expressID )
        return { id, type, GlobalId, Name, Description, ObjectType, Tag, PredefinedType, ...PropertySet } as IDotBimInfo
    }
    private getProp( expressID: number ) {
        if ( this.allProps.length === 0 ) return []
        const prop = this.allProps.find( ( prop: any ) => prop.relElementIds.find( ( id: string | number ) => id === expressID ) )
        if ( !prop ) return []
        return prop.props
    }
    static getQuantityValue( qua: any ) {
        const quantityLabel = qua.Name?.value;
        if ( !quantityLabel ) return "";
        switch ( quantityLabel ) {
            case "Area":
                return qua.AreaValue?.value.toFixed( 3 );
            case "GrossArea":
                return qua.AreaValue?.value.toFixed( 3 );
            case "GrossFootprintArea":
                return qua.AreaValue?.value.toFixed( 3 );
            case "CrossSectionArea":
                return qua.AreaValue?.value.toFixed( 3 );
            case "GrossSideArea":
                return qua.AreaValue?.value.toFixed( 3 );
            case "OuterSurfaceArea":
                return qua.AreaValue?.value.toFixed( 3 );
            case "Gross SurfaceArea":
                return qua.AreaValue?.value.toFixed( 3 );
            case "GrossVolume":
                return qua.VolumeValue?.value.toFixed( 3 );

            case "Perimeter":
                return qua.LengthValue?.value.toFixed( 3 );

            case "NetArea":
                return qua.AreaValue?.value.toFixed( 3 );

            case "NetSideArea":
                return qua.AreaValue?.value.toFixed( 3 );

            case "NetVolume":
                return qua.VolumeValue?.value.toFixed( 3 );

            case "Height":
                return qua.LengthValue?.value.toFixed( 3 );

            case "Length":
                return qua.LengthValue?.value.toFixed( 3 );

            case "Width":
                return qua.LengthValue?.value.toFixed( 3 );

            default:
                return "";
        }
    }
}