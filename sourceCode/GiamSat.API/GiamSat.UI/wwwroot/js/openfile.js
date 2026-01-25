function BlazorDownloadFile(filename, content) {
    // thanks to Geral Barre : https://www.meziantou.net/generating-and-downloading-a-file-in-a-blazor-webassembly-application.htm 

    // Handle both byte array (Uint8Array) and base64 string
    let blob;
    if (content instanceof Uint8Array) {
        // .NET 6+ optimized byte array interop
        blob = new Blob([content], { type: "application/octet-stream" });
    } else if (typeof content === 'string') {
        // Legacy base64 string (for older .NET versions)
        const byteCharacters = atob(content);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        blob = new Blob([byteArray], { type: "application/octet-stream" });
    } else {
        // Fallback: try to create File directly
        blob = new Blob([content], { type: "application/octet-stream" });
    }

    const exportUrl = URL.createObjectURL(blob);

    // Create the <a> element and click on it
    const a = document.createElement("a");
    document.body.appendChild(a);
    a.href = exportUrl;
    a.download = filename;
    a.target = "_self";
    a.click();

    // We don't need to keep the object url, let's release the memory
    // On Safari it seems you need to comment this line... (please let me know if you know why)
    URL.revokeObjectURL(exportUrl);
    a.remove();
}

function BlazorOpenFile(filename, content) {

    // Create the <a> element and click on it
    const a = document.createElement("a");
    document.body.appendChild(a);
    a.href = filename;
    a.download = filename;
    a.target = "_self";
    a.click();
}