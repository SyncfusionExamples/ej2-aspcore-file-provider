'use strict';

var fs = require('fs');
var gulp = require('gulp');

gulp.task('version', function() {
    var index = process.argv.indexOf('--option');
    var releaseVersion = process.argv[index + 1];
    console.log('\n RELEASE VERSION: ' + releaseVersion);
    fs.writeFileSync(__dirname + '/../version.txt', releaseVersion);
});
