$(function () {
    showPrivateMessagesPanel();
    deletePrivateMessages();
    blockMember();
    PmShowMorePosts();
});
var showPrivateMessagesPanel = function () {
    var privatemessageButton = $(".pm-panel");
    if (privatemessageButton.length > 0) {
        privatemessageButton.unbind("click");
        privatemessageButton.click(function (e) {
            e.preventDefault();
            var thisButton = $(this);
            var pmUrl = thisButton.attr("href");
            var title = thisButton.data("name");
            slideOutPanel(title);
            $.ajax({
                url: pmUrl,
                type: "GET",
                async: true,
                cache: false,
                success: function (data) {
                    // Load the Html into the side panel
                    slideOutPanel(title, data);

                    // Trigger Validation
                    $.validator.unobtrusive.parse(document);

                    // Delete private messages
                    deletePrivateMessages();
                    blockMember();
                    PmShowMorePosts();
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }
};

var deletePrivateMessages = function () {
    var privateMessageDeleteButton = $(".privatemessagedelete");
    if (privateMessageDeleteButton.length > 0) {
        privateMessageDeleteButton.click(function (e) {
            e.preventDefault();
            var linkClicked = $(this);
            var messageId = linkClicked.data("id");
            var deletePrivateMessageViewModel = new Object();
            deletePrivateMessageViewModel.Id = messageId;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(deletePrivateMessageViewModel);

            // Just fade out anyway
            var pmHolder = linkClicked.closest(".pmblock");
            pmHolder.fadeOut("fast");

            $.ajax({
                url: app_base + "PrivateMessage/Delete",
                type: "POST",
                cache: false,
                data: strung,
                contentType: "application/json; charset=utf-8",
                success: function (data) {

                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });
        });
    }
};

var blockMember = function () {
    var blockMemberButton = $(".pm-block");
    if (blockMemberButton.length > 0) {
        blockMemberButton.click(function (e) {
            e.preventDefault();
            var pmButton = $(this);
            var blockText = pmButton.data("blocktext");
            var blockedText = pmButton.data("blockedtext");
            var isBlocked = pmButton.data("isblocked");
            var userid = pmButton.data("userid");

            if (isBlocked) {
                pmButton.text(blockText);
            } else {
                pmButton.text(blockedText);
            }

            var viewModel = new Object();
            viewModel.MemberToBlockOrUnBlock = userid;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(viewModel);

            $.ajax({
                url: app_base + "Block/BlockOrUnBlock",
                type: "POST",
                data: strung,
                cache: false,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    // Just update attribute to opposite
                    if (isBlocked) {
                        pmButton.data("isblocked", false);
                    } else {
                        pmButton.data("isblocked", true);
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {

                    // Switch back if error
                    if (isBlocked) {
                        pmButton.text(blockedText);
                    } else {
                        pmButton.text(blockText);
                    }

                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }
};

var PmShowMorePosts = function () {
    var smp = $(".showmorepms");
    if (smp.length > 0) {
        smp.click(function (e) {
            e.preventDefault();

            var userId = $("#userId").val();
            var pageIndex = $("#pageIndex");
            var totalPages = parseInt($("#totalPages").val());
            var activeText = $("span.smpmactive");
            var loadingText = $("span.smpmloading");
            var showMoreLink = $(this);

            activeText.hide();
            loadingText.show();

            var getMoreViewModel = new Object();
            getMoreViewModel.UserId = userId;
            getMoreViewModel.PageIndex = pageIndex.val();

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(getMoreViewModel);

            $.ajax({
                url: app_base + "PrivateMessage/AjaxMore",
                type: "POST",
                dataType: "html",
                cache: false,
                data: strung,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    // Now add the new posts
                    showMoreLink.before(data);

                    // Update the page index value
                    var newPageIdex = (parseInt(pageIndex.val()) + parseInt(1));
                    pageIndex.val(newPageIdex);

                    // If the new pageindex is greater than the total pages, then hide the show more button
                    if (newPageIdex > totalPages) {
                        showMoreLink.hide();
                    }

                    // Lastly reattch the click events
                    deletePrivateMessages();
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                    activeText.show();
                    loadingText.hide();
                }
            });

        });
    }
}

var AjaxPrivateMessageSuccess = function () {
    // Grab the span the newly added post is in
    var postHolder = $("#newpmmarker");

    // Now add a new span after with the key class
    // In case the user wants to add another ajax post straight after
    postHolder.before("<span id=\"newpmmarker\"></span>");

    // Finally chnage the name of this element so it doesn't insert it into the same one again
    postHolder.attr("id", "imtonystark");

    // And more finally clear the post box
    $(".rte").val("");
    if ($(".bbeditorholder textarea").length > 0) {
        $(".bbeditorholder textarea").data("sceditor").val("");
    }
    if ($(".wmd-input").length > 0) {
        $(".wmd-input").val("");
        $(".wmd-preview").html("");
    }
    if (typeof tinyMCE != "undefined") {
        tinyMCE.activeEditor.setContent('');
    }

    // Re-enable the button
    AjaxPostFinish();

    // Finally do an async badge check
    TriggerPostBadges();
}