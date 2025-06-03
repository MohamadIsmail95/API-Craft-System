window.getSelectedValues = function (selectElement) {
    var selected = [];
    for (var i = 0; i < selectElement.options.length; i++) {
        if (selectElement.options[i].selected) {
            selected.push(selectElement.options[i].value);
        }
    }
    return selected;
};
