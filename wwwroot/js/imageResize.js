// Skalar ner och komprimerar bilder i webbläsaren innan Blazors <InputFile> läser dem.
//
// Aktiveras av data-resize på <input type="file">. Skriptet fångar change-eventet,
// byter ut filen i input.files mot den bearbetade och skickar eventet vidare, så
// serverkoden (OnChange + SaveImageAsync) är densamma oavsett om skriptet körs.
// Går något fel skickas originalfilen vidare – servern kontrollerar alltid igen.
//
//   data-max-width  längsta bredd i px (standard 1600). Mindre bilder förstoras aldrig.
//   data-max-kb     målstorlek; kvaliteten sänks stegvis tills bilden ryms.
//   data-format     "webp" (standard) eller "original" (logotyper behåller sitt format).
//
// GIF (kan vara animerad) och SVG (vektor) rörs aldrig.
(function () {
    'use strict';

    var RASTER = ['image/jpeg', 'image/png', 'image/webp'];
    var QUALITIES = [0.82, 0.72, 0.62];
    var EXT = { 'image/webp': '.webp', 'image/jpeg': '.jpg', 'image/png': '.png' };

    document.addEventListener('change', async function (e) {
        var input = e.target;
        if (!(input instanceof HTMLInputElement) || input.type !== 'file' || !('resize' in input.dataset)) return;

        // Vårt eget vidareskickade event: släpp igenom till Blazor.
        if (input.__resizePassthrough) { input.__resizePassthrough = false; return; }

        var file = input.files && input.files[0];
        if (!file || RASTER.indexOf(file.type) === -1) return;

        e.stopImmediatePropagation();

        var result = file;
        try {
            result = (await process(file, input.dataset)) || file;
        } catch (err) {
            console.warn('Bildskalning misslyckades, skickar originalet.', err);
        }

        var dt = new DataTransfer();
        dt.items.add(result);
        input.files = dt.files;
        input.__resizePassthrough = true;
        input.dispatchEvent(new Event('change', { bubbles: true }));
    }, true);

    async function process(file, ds) {
        var maxWidth = parseInt(ds.maxWidth || '1600', 10);
        var maxBytes = parseInt(ds.maxKb || '0', 10) * 1024;
        var wanted = ds.format === 'original' ? file.type : 'image/webp';

        // from-image tar hänsyn till EXIF-rotation, så mobilfoton hamnar rätt vänt.
        var bmp = await createImageBitmap(file, { imageOrientation: 'from-image' });
        try {
            var scale = Math.min(1, maxWidth / bmp.width);
            var w = Math.round(bmp.width * scale);
            var h = Math.round(bmp.height * scale);

            var best = await encode(bmp, w, h, wanted, maxBytes);
            if (!best) return file;

            // Redan liten och oskalad: behåll originalet om det är mindre.
            if (scale === 1 && best.type === file.type && file.size <= best.size) return file;

            return new File([best], rename(file.name, best.type), { type: best.type, lastModified: Date.now() });
        } finally {
            bmp.close();
        }
    }

    async function encode(bmp, w, h, type, maxBytes) {
        var canvas = draw(bmp, w, h, null);
        var qualities = type === 'image/png' ? [undefined] : QUALITIES;
        var best = null;

        for (var i = 0; i < qualities.length; i++) {
            var blob = await toBlob(canvas, type, qualities[i]);

            // Webbläsaren kan inte koda formatet och föll tillbaka på PNG: ta JPEG på vit botten.
            if (blob && blob.type !== type && type === 'image/webp') {
                type = 'image/jpeg';
                canvas = draw(bmp, w, h, '#ffffff');
                blob = await toBlob(canvas, type, qualities[i]);
            }
            if (!blob) return best;
            best = blob;
            if (!maxBytes || blob.size <= maxBytes) break;
        }
        return best;
    }

    function draw(bmp, w, h, background) {
        var canvas = document.createElement('canvas');
        canvas.width = w;
        canvas.height = h;
        var ctx = canvas.getContext('2d');
        ctx.imageSmoothingQuality = 'high';
        if (background) { ctx.fillStyle = background; ctx.fillRect(0, 0, w, h); }
        ctx.drawImage(bmp, 0, 0, w, h);
        return canvas;
    }

    function toBlob(canvas, type, quality) {
        return new Promise(function (resolve) { canvas.toBlob(resolve, type, quality); });
    }

    function rename(name, type) {
        var dot = name.lastIndexOf('.');
        return (dot > 0 ? name.substring(0, dot) : name) + (EXT[type] || '');
    }
})();
