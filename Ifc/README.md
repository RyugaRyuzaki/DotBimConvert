# Install
    - NodeJS>=21
    - npm install
# Build
    - Build command : npm run build
    - Build bundler : npm run build:roll
# Run Test : change your file path
    - Test all categories : npm run test:all    
    - Test  categories : npm run test    
# Convert .ifc to .bim
    - Using command: make sure build before run
        - node dist/index.js <input-path> <output-dir> <option:null || all>
        * option all : 
            - 1 : <output-dir>/<filename>/<filename>.bim
            - 2 : <output-dir>/<filename>/<filename>.frag.bim
        * option null : 
            - 1 : <output-dir>/<filename>/<filename-category-name>.bim
            - 2 : <output-dir>/<filename>/<filename-category-name>.frag.bim
 
# Test Viewer : https://3dviewer.net/