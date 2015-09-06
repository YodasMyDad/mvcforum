var enterKeypress = $.Event("keypress", { which: 13 });
$(function () {
    $('.tagstextarea').tagsInput({
        'autocomplete_url': app_base + 'tag/autocompletetags',
        'removeWithBackspace': true,
        'minChars': 2,
        'maxChars': 25,
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

    addFocusOutToTags();
});
function hasWhiteSpace(s) {
    return s.indexOf(' ') >= 0;
}

var addFocusOutToTags = function() {
    $("#Tags_tag").focusout(function () {
        $(this).trigger(enterKeypress);
    });
};
