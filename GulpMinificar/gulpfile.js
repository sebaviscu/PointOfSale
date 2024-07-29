const gulp = require('gulp');
const uglify = require('gulp-uglify');
const concat = require('gulp-concat');
const ts = require('gulp-typescript');
const path = require('path');

// Crear un proyecto TypeScript
const tsProject = ts.createProject('tsconfig.json');

// Ruta a los archivos JS y TS en el proyecto web
const jsPath = path.join(__dirname, '../PointOfSale/wwwroot/js/**/*.{js,ts}');

// Tarea para compilar TypeScript y minificar archivos JS
gulp.task('scripts', function () {
    return gulp.src(jsPath)
        .pipe(tsProject()) // Compilar TypeScript
        .pipe(concat('app.min.js')) // Nombre del archivo de salida
        .pipe(uglify()) // Minificar
        .pipe(gulp.dest(path.join(__dirname, '../PointOfSale/wwwroot/js/dist'))); // Carpeta de destino
});

// Tarea para observar cambios en los archivos JS y TS
//gulp.task('watch', function () {
//    gulp.watch(jsPath, gulp.series('scripts'));
//});

// Tarea por defecto
gulp.task('default', gulp.series('scripts', 'watch'));
