$(function () {
    $('.tagstextarea').tagsInput({
        'interactive': true,
        'defaultText': 'add a tag',
        'removeWithBackspace': true,
        'minChars': 3,
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