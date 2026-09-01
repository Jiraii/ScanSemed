const fs = require('fs');
let pkg = JSON.parse(fs.readFileSync('package.json', 'utf8'));
pkg.name = 'scan';
pkg.description = 'BDSender SEMED';
pkg.author = 'Jiraii';
pkg.scripts['build:angular'] = 'powershell -Command "cd ../web-frontend-source; npm run build; if ($LASTEXITCODE -eq 0) { Copy-Item -Path \'./dist/web-frontend-source/browser/*\' -Destination \'../electron-app/dist/web-frontend/browser\' -Recurse -Force }"';
pkg.scripts.dist = 'npm run build:angular && electron-builder';
pkg.build.appId = 'com.hospital.scan';
pkg.build.productName = 'Scan';
pkg.build.directories.output = 'D:/Scan';
pkg.build.win.icon = 'icon.png';
if(!pkg.devDependencies) pkg.devDependencies = {};
if(pkg.dependencies && pkg.dependencies.electron) {
    pkg.devDependencies.electron = pkg.dependencies.electron;
    delete pkg.dependencies.electron;
}
fs.writeFileSync('package.json', JSON.stringify(pkg, null, 2));
