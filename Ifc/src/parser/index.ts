import * as fs from 'fs';
import { IfcLoader } from './IfcLoader';
import { Serializer } from './DotBim';

const extensionDotBim = '.bim';
const extensionDotBimFrag = '.frag.bim';
async function writeFile( outDir: string, fileName: string, schema_version: string, info: any, meshes: any, elements: any ) {
    if ( !fs.existsSync( outDir ) ) fs.mkdirSync( outDir );
    const outputDotBim = `${outDir}/${fileName}${extensionDotBim}`
    const outputDotBimFrag = `${outDir}/${fileName}${extensionDotBimFrag}`
    const newMeshes = Object.keys( meshes ).map( ( key: string ) => meshes[key] )
    const newElements = Object.keys( elements ).map( ( key: string ) => elements[key] )
    const dotBim = { schema_version, info, meshes: newMeshes, elements: newElements }
    await fs.writeFileSync( outputDotBim, JSON.stringify( dotBim, null, 2 ) )
    const compress = Serializer.compress( dotBim )
    await fs.writeFileSync( outputDotBimFrag, compress )
}
export async function parserFragToDotBim() {

}
export async function parserIfcToDotBim( filePath: string, outDir: string, fileName: string, allOption = true ) {
    try {
        const before = performance.now();
        const arrayBuffer = fs.readFileSync( filePath ) as ArrayBuffer
        const ifcLoader = new IfcLoader( allOption )
        const newOutDir = `${outDir}/${fileName}`
        ifcLoader.onSuccessAll = async ( dotBim: any ) => {
            const { schema_version, info, meshes, elements } = dotBim
            await writeFile( newOutDir, fileName, schema_version, info, meshes, elements )
            console.log( `Time total:${( performance.now() - before ) / 1000}s` );
        }
        ifcLoader.onSuccessCategories = async ( dotBim: any ) => {
            const { schema_version, info, categories } = dotBim
            for ( const type in categories ) {
                const newFileName = `${fileName}-${type}`
                const { meshes, elements } = categories[type]
                await writeFile( newOutDir, newFileName, schema_version, info, meshes, elements )
            }
            console.log( `Time total:${( performance.now() - before ) / 1000}s` );
        }
        await ifcLoader.parse( new Uint8Array( arrayBuffer ) )
    } catch ( error ) {
        console.log( error );
    }
}
