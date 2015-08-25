$(function () {
    $('.tagstextarea').tagsInput({
        'autocomplete_url': app_base + 'tag/autocompletetags',
        'interactive': true,
        'removeWithBackspace': true,
        'autocomplete': { something: "This", option: "that" },
        'minChars': 2,
        'maxChars': 20,
        onAddTag: function (value) {
            if (hasWhiteSpace(value)) {
                var tag = value;
                tag = tag.replace(/\s+/g, '-').toLowerCase();
                if (!$(this).tagExist(tag)) {
                    $(this).removeTag(value);
                    $(this).addTag(tag);
                }
                else {
                    $(this).removeTag(value);
                }
            }
        }
    });
});
function hasWhiteSpace(s) {
    return s.indexOf(' ') >= 0;
}

//$(selector).tagsInput({
//    'autocomplete_url': app_base + '/tag/autocompletetags',
//    'autocomplete': { option: value, option: value },
//    'height': '100px',
//    'width': '300px',
//    'interactive': true,
//    'defaultText': 'add a tag',
//    'onAddTag': callback_function,
//    'onRemoveTag': callback_function,
//    'onChange': callback_function,
//    'delimiter': [',', ';'],   // Or a string with a single delimiter. Ex: ';'
//    'removeWithBackspace': true,
//    'minChars': 0,
//    'maxChars': 0, // if not provided there is no limit
//    'placeholderColor': '#666666'
//});