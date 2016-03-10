$(function () {
    // Hide panel click events
    hideSlideOutPanel();
});


/*------------ Slide Out PAnel --------------------*/

var largeSpinnerBlock = "<div class=\"loaderholder\"><img src=\"" + largeSpinnerBlockImage + "\" alt=\"\"/></div>";
var AddLargeSpinner = function (elementToPlaceInside) {
    elementToPlaceInside.html(largeSpinnerBlock);
};


var slideOutPanel = function (title) {
    slideOutPanel(title, largeSpinnerBlock);
}

var slideOutPanel = function (title, content) {
    // Get panel
    var panel = $(".cd-panel");

    // Get content div and add in content
    var contentDiv = panel.find(".cd-panel-content");
    contentDiv.html(content);
    var titleDiv = panel.find(".cd-panel-header h6");
    titleDiv.html(title);

    // Show Panel
    panel.addClass("is-visible");

    // Add overflow hidden to the body tag
    $("body").css("overflow", "hidden").css("height", "100%");
    $("html").css("overflow", "hidden").css("height", "100%");
};

var hideSlideOutPanel = function () {
    var panel = $(".cd-panel");
    panel.on("click", function (event) {
        if ($(event.target).is(".cd-panel") || $(event.target).is(".cd-panel-close")) {
            // Confirm they want to close it
            var closeconfirmationText = panel.data("confirmationtext");

            if (typeof tinyMCE != "undefined") {
                var pmMessage = tinyMCE.activeEditor.getContent();
                if (pmMessage != "") {
                    if (confirm(closeconfirmationText)) {
                        hardPanelClose(event, panel);
                    }
                } else {
                    hardPanelClose(event, panel);
                }
            } else {
                hardPanelClose(event, panel);
            }
            return false;
        }
    });
};

var hardPanelClose = function (event, panel) {
    event.preventDefault();
    panel.removeClass("is-visible");
    //Clear fields
    var contentDiv = panel.find(".cd-panel-content");
    contentDiv.html(largeSpinnerBlock);
    var titleDiv = panel.find(".cd-panel-header h6");
    titleDiv.html("");
    $("body").css("overflow", "").css("height", "");
    $("html").css("overflow", "").css("height", "");
    //remove onbeforeunload registered by TinyMCE
    window.onbeforeunload = function () { };
};

var closeSlideOutPanel = function () {
    var panel = $(".cd-panel");
    panel.trigger("click");
};

/*--------------- END PANEL SLIDER -----------------*/
