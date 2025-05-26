window.beautifyJson = function (elementId) {
    try {
        let el = document.getElementById(elementId);
        let json = JSON.parse(el.value);
        el.value = JSON.stringify(json, null, 4);
    } catch (e) {
        alert("Invalid JSON");
    }
};
