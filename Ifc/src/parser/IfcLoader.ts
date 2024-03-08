import * as WebIFC from "web-ifc"
import { GeometryReader } from "./GeometryLoader";
import { DotBim } from "./DotBim";

export class IfcLoader {
  /**
   *
   */
  private api: WebIFC.IfcAPI = new WebIFC.IfcAPI()
  private readonly wasmPath = "/";
  private readonly webIfcSettings = {
    COORDINATE_TO_ORIGIN: true,
    USE_FAST_BOOLS: true,
    OPTIMIZE_PROFILES: true,
    CIRCLE_SEGMENTS_LOW: 12,
    CIRCLE_SEGMENTS_MEDIUM: 24,
    CIRCLE_SEGMENTS_HIGH: 48,
    CIRCLE_SEGMENTS: 48,
    BOOL_ABORT_THRESHOLD: 10,
  };
  optionalCategories: number[] = [WebIFC.IFCSPACE];
  private geometryReader!: GeometryReader;
  private dotBim!: DotBim;
  private schema_version = "1.0.0"
  onSuccessAll!: ( dotBim: any ) => void
  onSuccessCategories!: ( dotBim: any ) => void
  onError!: () => void
  constructor( private allOption = true ) {
    this.api.SetWasmPath( this.wasmPath )
    this.geometryReader = new GeometryReader( this.api )
    this.dotBim = new DotBim( this.api, this.schema_version, allOption )
  }
  async parse( data: Uint8Array ) {
    try {
      await this.api.Init()
      this.api.SetLogLevel( WebIFC.LogLevel.LOG_LEVEL_OFF );
      const modelID = this.api.OpenModel( data, this.webIfcSettings )
      await this.readAllGeometries( modelID )
      await this.dotBim.getIfcMetadata( modelID )
      await this.dotBim.getMeshes( modelID, this.geometryReader.items )
      if ( this.allOption ) {
        if ( this.onSuccessAll ) this.onSuccessAll( this.dotBim.converterAll() )
      } else {
        if ( this.onSuccessCategories ) this.onSuccessCategories( this.dotBim.converterCategories() )
      }
    } catch ( error ) {
      console.log( error );

    }
  }
  private async readAllGeometries( modelID: number ) {

    // Some categories (like IfcSpace) need to be created explicitly
    const optionals = [...this.optionalCategories];

    // Force IFC space to be transparent
    if ( optionals.includes( WebIFC.IFCSPACE ) ) {
      const index = optionals.indexOf( WebIFC.IFCSPACE );
      optionals.splice( index, 1 );
      this.api.StreamAllMeshesWithTypes( modelID, [WebIFC.IFCSPACE], ( mesh: WebIFC.FlatMesh ) => {
        this.geometryReader.streamMesh( modelID, mesh )
      } );
    }

    // Load rest of optional categories (if any)
    if ( optionals.length ) {
      this.api.StreamAllMeshesWithTypes( modelID, optionals, ( mesh: WebIFC.FlatMesh ) => {
        this.geometryReader.streamMesh( modelID, mesh )
      } );
    }

    // Load common categories
    this.api.StreamAllMeshes( modelID, ( mesh: WebIFC.FlatMesh ) => {
      this.geometryReader.streamMesh( modelID, mesh )
    } );

  }


}