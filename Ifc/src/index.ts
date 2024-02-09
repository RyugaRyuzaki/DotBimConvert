import fs from 'fs';
import path from 'path';
import { parserIfcToDotBim } from './parser'


const filePath = process.argv[2]
const outDir = process.argv[3] || './output';
const optionArgv = process.argv[4]

const errorType = {
    1: 'Missing Arguments,expect 4',
    2: 'Missing input file',
    3: 'File does not existed',
    4: 'File is not .ifc',
    5: 'Wrong output',
    6: 'Missing option',
}
// check option
function check() {
    if ( process.argv.length < 4 ) return 1
    if ( !filePath ) return 2
    if ( !fs.existsSync( filePath ) ) return 3
    if ( !filePath.endsWith( ".ifc" ) ) return 4
    if ( !outDir.startsWith( "./" ) ) return 5
    if ( !fs.existsSync( outDir ) ) fs.mkdirSync( outDir );
    return 10
}

// check option
function getAllOption(): boolean {
    if ( !optionArgv || optionArgv !== "all" ) return false
    return true
}






( async () => {
    const checkInput = check()
    if ( checkInput !== 10 ) {
        console.error( `Error : ${errorType[checkInput]}` );
        return
    }
    const allOption = getAllOption()
    const fileName = path.basename( filePath ).split( '.ifc' )[0];
    parserIfcToDotBim( filePath, outDir, fileName, allOption )
} )()