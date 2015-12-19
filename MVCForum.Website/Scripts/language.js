$(function () {
    ChangeLanguage();
});

function ChangeLanguage() {
    var languageSelect = $(".languageselector select");
    if (languageSelect.length > 0) {
        languageSelect.change(function () {
            var langVal = this.value;
            $.ajax({
                url: "/Language/ChangeLanguage",
                type: "POST",
                cache: false,
                data: { lang: langVal },
                success: function (data) {
                    location.reload();
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });
        });
    }
}
