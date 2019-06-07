require("@syncfusion/ej2-staging");

const gulp = require('gulp');
const shelljs = require('shelljs');

gulp.task('copy-webconfig', function() {
    shelljs.cp('-r', './web.config', './src/deploy')
});