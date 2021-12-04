let { clean, restore, build, pack, push } = require('gulp-dotnet-cli');
const path = require('path');
let gulp = require('gulp');
let nupkgPath = path.resolve(process.cwd(), 'Nuget');
var common = require('./common.js');
var shelljs = global.shelljs = global.shelljs || require('shelljs');
var regexp = common.regexp;
var fs = global.fs = global.fs || require('fs');
var isReleaseBranch = /^((release\/|hotfix\/))/g.test(process.env.BRANCH_NAME);


gulp.task('clean', () => {
    return gulp.src(['./EJ2ASPCoreFileProvider.csproj'], { read: false })
        .pipe(clean());
});

gulp.task('restore',gulp.series('clean'), () => {
    return gulp.src(['./EJ2ASPCoreFileProvider.csproj'], { read: false })
        .pipe(restore({ echo: true }));
});

gulp.task('build', gulp.series('restore'), () => {
    return gulp.src(['./EJ2ASPCoreFileProvider.csproj'], { read: false })
        .pipe(build({ configuration: 'Release', version: '1.0.0', echo: true }));
});

gulp.task('ls-log', gulp.series('build'), () => {
return null;
});

gulp.task('ci-report',gulp.series('ls-log'), () => {
    return null;
    });

   

gulp.task('generate-nuget', function(done) {
    var version = fs.readFileSync('./version.txt', 'utf8');
    console.log(version);
    //ASP Core Nuget
    shelljs.exec('dotnet restore "./EJ2ASPCoreFileProvider.csproj"  --configfile "NuGet.config"');
    shelljs.exec('dotnet build "./EJ2ASPCoreFileProvider.csproj" --verbosity "m" --configuration  "Release-XML"');
    shelljs.exec(`dotnet pack "./EJ2ASPCoreFileProvider.csproj" -c Release --output NuGet${version}`);
    shelljs.exec('gulp clean');
    done();
});

console.log(nupkgPath);
gulp.task('local-pack', () => {
    var packJson = JSON.parse(fs.readFileSync('./package.json', 'utf-8'));
    var nugetVersion = packJson.version;
    return gulp.src('./EJ2ASPCoreFileProvider.csproj')
        .pipe(restore())
        .pipe(build())
        .pipe(pack({
            output: nupkgPath,
            noBuild: true,
            version: nugetVersion,
            noRestore: true,
            echo: true
        }));
});

//publish nuget packages to a nexus
gulp.task('publish-nuget', () => {
    if (process.env.BRANCH_NAME === 'master' || process.env.BRANCH_NAME === 'development' || isReleaseBranch ) {
        return gulp.src('./Nuget/*.nupkg', { read: false })
            .pipe(push({
                apiKey: process.env.EJ2_NEXUS_NUGET,
                source: 'http://nexus.syncfusion.com/repository/ej2-nuget/'
            }));
    }
});

gulp.task('updatepack-version', function() {
    var pack = JSON.parse(fs.readFileSync('./package.json', 'utf-8'));
    var version = pack.version;
    var modifiedVersion = version.split('.');
    if ((parseInt(modifiedVersion[2]) + 1) > 99) {
        version = modifiedVersion[0] + '.' + (parseInt(modifiedVersion[2]) + 1) + '.' + 0;
    } else {
        version = modifiedVersion[0] + '.' + modifiedVersion[1] + '.' + (parseInt(modifiedVersion[2]) + 1);
    }
    pack.version = version;
    fs.writeFileSync('./package.json', JSON.stringify(pack, null, '\t'));
});

function shellDone(exitCode) {
    if (exitCode !== 0) {
        process.exit(1);
    }
}
exports.shellDone = shellDone;

gulp.task('ci-skip', function(done) {
    var simpleGit = require('simple-git')();
    simpleGit.log(function(err, log) {
        var stagingBranch = common.stagingBranch;
        if (process.env.BRANCH_NAME === stagingBranch) {
            var user = process.env.GITLAB_USER;
            var token = process.env.GITLAB_TOKEN;
            var origin = 'http://' + user + ':' + token + '@gitlab.syncfusion.com/essential-studio/' + common.currentRepo + '.git';
            shelljs.exec('git remote set-url origin ' + origin + ' && git pull origin ' + stagingBranch);
            shelljs.exec('git add -f package.json');
            shelljs.exec('git commit -m \"ci-skip(EJ2-000): Branch merged and package is published [ci skip]\" --no-verify');
            shelljs.exec('git branch -f ' + stagingBranch + ' HEAD && git checkout ' + stagingBranch, shellDone);
            shelljs.exec('git push -f --set-upstream origin ' + stagingBranch + ' --no-verify', { silent: true }, function(exitCode) {
                done();
                shellDone(exitCode);
            });
        } else {
            done();
        }
    });
});

gulp.task('clean', function(done) {
    shelljs.rm('-rf', './src/bin/');
    shelljs.rm('-rf', './src/obj/');
    shelljs.rm('-rf', './bin/');
    done();
});
