var ShowUserMessage = function (message) {
    if (message != null) {
        var jsMessage = $("#jsquickmessage");
        var toInject = "<div class=\"alert alert-info fade in\" role=\"alert\"><button type=\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;<\/span><\/button>" + message + "<\/div>";
        jsMessage.html(toInject);
        jsMessage.show();
        $("div.alert").delay(2200).fadeOut();
    }
}