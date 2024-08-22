const gulp = require('gulp');
const uglify = require('gulp-uglify');
const concat = require('gulp-concat');
const path = require('path');

// Ruta a los archivos JS en el proyecto web
const jsPaths = [
    path.join(__dirname, '../wwwroot/js/views/**/*.js'),
    path.join(__dirname, '../wwwroot/plugins/printservice.js')
];

// Tarea para minificar y concatenar archivos JS
gulp.task('scripts', function () {
    return gulp.src(jsPaths)
        .pipe(concat('app.min.js')) // Nombre del archivo de salida
        .pipe(uglify()) // Minificar y uglificar
        .pipe(gulp.dest(path.join(__dirname, '../wwwroot/js/dist'))); // Carpeta de destino
});

// Tarea para observar cambios en los archivos JS
gulp.task('watch', function () {
    gulp.watch(jsPaths, gulp.series('scripts'));
});

// Tarea por defecto
gulp.task('default', gulp.series('scripts', 'watch'));
