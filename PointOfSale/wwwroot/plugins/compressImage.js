function compressImage(file, quality, maxWidth, maxHeight) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = event => {
            const img = new Image();
            img.src = event.target.result;
            img.onload = () => {
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');

                const ratio = Math.min(maxWidth / img.width, maxHeight / img.height);
                canvas.width = img.width * ratio;
                canvas.height = img.height * ratio;

                ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

                const mimeType = file.type === 'image/png' ? 'image/png' :
                    file.type === 'image/gif' ? 'image/gif' :
                        'image/jpeg';

                canvas.toBlob(blob => {
                    resolve(blob);
                }, mimeType, quality);
            };
            img.onerror = reject;
        };
        reader.onerror = reject;
    });
}
