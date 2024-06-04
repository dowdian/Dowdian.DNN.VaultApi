var gulp = require('gulp');

function watchFiles() {
    gulp.watch('Modules/DnnVaultApi/PersonaBar/**/*').on('change', function(path) {
        console.log('File ' + path + ' was changed, running tasks...');
        // copy the changed file to "~Website\DesktopModules\Admin\Dnn.PersonaBar\Modules\DnnVaultApi"
        return gulp.src(path, { base: 'Modules/DnnVaultApi/PersonaBar' })
            .pipe(gulp.dest('Website/DesktopModules/Admin/Dnn.PersonaBar/Modules/DnnVaultApi'));
    });
}

exports.watch = watchFiles;
