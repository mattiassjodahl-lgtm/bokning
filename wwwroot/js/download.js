// Tunn helper for att trigga en filnedladdning i webblasaren fran en base64-encoded byte-array.
// Anvands fran AdminReports.razor for Excel-export.
window.fileDownload = {
    fromBase64: function (base64, filename, mimeType) {
        try {
            const byteString = atob(base64);
            const byteArray = new Uint8Array(byteString.length);
            for (let i = 0; i < byteString.length; i++) {
                byteArray[i] = byteString.charCodeAt(i);
            }
            const blob = new Blob([byteArray], {
                type: mimeType || "application/octet-stream"
            });
            const url = URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = filename || "download";
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            // Frigor URL:en efter en kort delay sa nedladdningen hinner starta
            setTimeout(() => URL.revokeObjectURL(url), 1000);
            return true;
        } catch (err) {
            console.error("fileDownload.fromBase64 failed:", err);
            return false;
        }
    }
};
