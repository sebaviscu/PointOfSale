const gulp = require('gulp');
const fs = require('fs');
const path = require('path');
const javascriptObfuscator = require('gulp-javascript-obfuscator');
const rename = require('gulp-rename');

// Ruta a los archivos JS en el proyecto web
const jsPath = path.join(__dirname, '../wwwroot/js/views/**/*.js');
const obfuscatedFilesPath = path.join(__dirname, '../wwwroot/js/views/');

// Tarea para limpiar archivos .min.js o .obf.js antes de generar nuevos
gulp.task('clean', function (done) {
    fs.readdir(obfuscatedFilesPath, (err, files) => {
        if (err) throw err;
        for (const file of files) {
            if (file.endsWith('.obf.js')) {
                fs.unlink(path.join(obfuscatedFilesPath, file), err => {
                    if (err) throw err;
                });
            }
        }
        done();
    });
});

// Tarea para ofuscar archivos JS individualmente usando JavaScript Obfuscator
gulp.task('scripts', gulp.series('clean', function () {
    return gulp.src(jsPath)
        .pipe(javascriptObfuscator({
            compact: true,
            controlFlowFlattening: true,
            controlFlowFlatteningThreshold: 0.75,
            deadCodeInjection: true,
            deadCodeInjectionThreshold: 0.4,
            debugProtection: false,
            selfDefending: false,
            disableConsoleOutput: true,
            identifierNamesGenerator: 'hexadecimal',
            log: false,
            renameGlobals: false,
            rotateStringArray: true,
            stringArray: true,
            stringArrayEncoding: ['base64'],
            stringArrayThreshold: 0.75,
            transformObjectKeys: true,
            unicodeEscapeSequence: false
        }))
        .pipe(rename({ extname: '.obf.js' })) // Renombrar con .obf.js
        .pipe(gulp.dest(path.join(__dirname, '../wwwroot/js/views'))); // Guardar en la misma carpeta
}));

// Tarea para observar cambios en los archivos JS
gulp.task('watch', function () {
    gulp.watch(jsPath, gulp.series('scripts'));
});

// Tarea por defecto
gulp.task('default', gulp.series('scripts', 'watch'));
