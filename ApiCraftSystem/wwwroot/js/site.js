
window.downloadFile = async function (fileName, dotNetStreamReference) {
    const arrayBuffer = await dotNetStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? 'downloaded-file';
    anchorElement.click();

    URL.revokeObjectURL(url);
};
