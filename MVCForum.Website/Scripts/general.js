//---------------- On Ready ------------------------

$(function () {

    $('input, textarea').placeholder();

    //---------------- On Click------------------------

    $(".thumbuplink").click(function (e) {
        var postId = $(this).attr('rel');
        var karmascore = ".karmascore-" + postId;
        var karmathumbholder = ".postkarmathumbs-" + postId;
        $(karmathumbholder).remove();

        var VoteUpViewModel = new Object();
        VoteUpViewModel.Post = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(VoteUpViewModel);

        $.ajax({
            url: '/Vote/VoteUpPost',
            type: 'POST',
            dataType: 'json',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                SuccessfulThumbUp(karmascore);
                BadgeVoteUp(postId);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
        e.preventDefault();
        e.stopImmediatePropagation();
    });

    $(".thumbdownlink").click(function (e) {
        var postId = $(this).attr('rel');
        var karmascore = ".karmascore-" + postId;
        var karmathumbholder = ".postkarmathumbs-" + postId;
        $(karmathumbholder).remove();

        var VoteDownViewModel = new Object();
        VoteDownViewModel.Post = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(VoteDownViewModel);

        $.ajax({
            url: '/Vote/VoteDownPost',
            type: 'POST',
            dataType: 'json',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                SuccessfulThumbDown(karmascore);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
        e.preventDefault();
        e.stopImmediatePropagation();
    });

    $(".privatemessagedelete").click(function (e) {
        var linkClicked = $(this);
        var messageId = linkClicked.attr('rel');
        var deletePrivateMessageViewModel = new Object();
        deletePrivateMessageViewModel.Id = messageId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(deletePrivateMessageViewModel);

        $.ajax({
            url: '/PrivateMessage/Delete',
            type: 'POST',
            dataType: 'json',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                // deleted, remove table row
                RemovePrivateMessageTableRow(linkClicked);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
        e.preventDefault();
        e.stopImmediatePropagation();
    });

    $(".issolution").click(function (e) {
        var solutionHolder = $(this);
        var postId = solutionHolder.attr('rel');

        var MarkAsSolutionViewModel = new Object();
        MarkAsSolutionViewModel.Post = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(MarkAsSolutionViewModel);

        $.ajax({
            url: '/Vote/MarkAsSolution',
            type: 'POST',
            dataType: 'json',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                MarkAsSolution(solutionHolder);
                BadgeMarkAsSolution(postId);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
        e.preventDefault();
        e.stopImmediatePropagation();
    });

    $(".emailsubscription").click(function (e) {
        var entityId = $(this).attr('rel');
        $(this).hide();
        var subscriptionType = $(this).find('span').attr('rel');

        var subscribeEmailViewModel = new Object();
        subscribeEmailViewModel.Id = entityId;
        subscribeEmailViewModel.SubscriptionType = subscriptionType;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(subscribeEmailViewModel);

        $.ajax({
            url: '/Email/Subscribe',
            type: 'POST',
            dataType: 'json',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $(".emailunsubscription").fadeIn();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });

        e.preventDefault();
        e.stopImmediatePropagation();
    });

    $(".emailunsubscription").click(function (e) {
        var entityId = $(this).attr('rel');
        $(this).hide();
        var subscriptionType = $(this).find('span').attr('rel');

        var unSubscribeEmailViewModel = new Object();
        unSubscribeEmailViewModel.Id = entityId;
        unSubscribeEmailViewModel.SubscriptionType = subscriptionType;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(unSubscribeEmailViewModel);

        $.ajax({
            url: '/Email/UnSubscribe',
            type: 'POST',
            dataType: 'json',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $(".emailsubscription").fadeIn();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });

        e.preventDefault();
        e.stopImmediatePropagation();
    });
    
    // Some manual ajax badge checks
    if ($.QueryString["postbadges"] == "true") {
        // Do a post badge check
        UserPost();
    }

    // Poll Answer counter
    var counter = 0;

    // Create Polls
    $(".createpollbutton").click(function (e) {
        e.preventDefault();
        //Firstly Show the Poll Section
        $('.pollanswerholder').show();
        // Now add in the first row
        AddNewPollAnswer(counter);
        counter++;
        // Hide this button now
        $(this).hide();
        // Show the remove poll button
        $(".removepollbutton").show();
    });
    
    // Remove the polls
    $(".removepollbutton").click(function (e) {
        e.preventDefault();
        //Firstly Show the Poll Section
        $('.pollanswerholder').hide();
        $('.pollanswerlist').html("");
        // Hide this button now
        $(this).hide();
        // Show the add poll button
        $(".createpollbutton").show();
    });
    
    // Add a new answer
    $(".addanswer").click(function (e) {
        e.preventDefault();
        AddNewPollAnswer(counter);
        counter++;
        ShowHideRemovePollAnswerButton(counter);
    });

    // Remove a poll answer
    $(".removeanswer").click(function (e) {
        e.preventDefault();
        if (counter > 1) {
            counter--;
            $("#answer" + counter).remove();
            ShowHideRemovePollAnswerButton(counter);
        }
    });

});

function AddNewPollAnswer(counter) {
    var placeHolder = $('#pollanswerplaceholder').val();
    var liHolder = $(document.createElement('li')).attr("id", 'answer' + counter);
    liHolder.html('<input type="text" name="PollAnswers[' + counter + '].Answer" id="PollAnswers_' + counter + '_Answer" value="" placeholder="' + placeHolder + '" />');
    liHolder.appendTo(".pollanswerlist");
}

function ShowHideRemovePollAnswerButton(counter) {
    var removeButton = $('.removeanswer');
    if(counter > 1) {
        removeButton.show();
    } else {
        removeButton.hide();
    }
}

//---------------- Functions------------------------
function RemovePrivateMessageTableRow(linkClicked) {
    linkClicked.parents('tr').first().fadeOut();
}

function BadgeMarkAsSolution(postId) {

    // Ajax call to post the view model to the controller
    var markAsSolutionBadgeViewModel = new Object();
    markAsSolutionBadgeViewModel.PostId = postId;

    // Ajax call to post the view model to the controller
    var strung = JSON.stringify(markAsSolutionBadgeViewModel);

    $.ajax({
        url: '/Badge/MarkAsSolution',
        type: 'POST',
        dataType: 'json',
        data: strung,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            // No need to do anything
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowUserMessage("Error: " + xhr.status + " " + thrownError);
        }
    });
}

function BadgeVoteUp(postId) {
    
        // Ajax call to post the view model to the controller
        var voteUpBadgeViewModel = new Object();
        voteUpBadgeViewModel.PostId = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(voteUpBadgeViewModel);

        $.ajax({
            url: '/Badge/VoteUpPost',
            type: 'POST',
            dataType: 'json',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                // No need to do anything
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
}

function UserPost() {

    $.ajax({
        url: '/Badge/Post',
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            // No need to do anything
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowUserMessage("Error: " + xhr.status + " " + thrownError);
        }
    });
}

function MarkAsSolution(solutionHolder) {
    $(solutionHolder).removeClass("issolution");
    $(solutionHolder).addClass("issolution-solved");
    $(solutionHolder).attr('title', '');
    $('.issolution').hide();
}

function SuccessfulThumbUp(karmascore) {
    var currentKarma = parseInt($(karmascore).text());
    $(karmascore).text((currentKarma + 1));
}

function SuccessfulThumbDown(karmascore) {
    var currentKarma = parseInt($(karmascore).text());
    $(karmascore).text((currentKarma - 1));
}

function ShowUserMessage(message) {
    if (message != null) {
        var jsMessage = $('#jsquickmessage');
        var toInject = "<div class=\"alert alert-block alert-info fade in\"><a href=\"#\" data-dismiss=\"alert\" class=\"close\">&times;<\/a>" + message + "<\/div>";
        jsMessage.html(toInject);
        jsMessage.show();
    }
}

function AjaxPostSuccess() {
    // Grab the span the newly added post is in
    var postHolder = $('#newpostmarker');

    // Now add a new span after with the key class
    // In case the user wants to add another ajax post straight after
    postHolder.after('<span id="newpostmarker"></span>');

    // Finally chnage the name of this element so it doesn't insert it into the same one again
    postHolder.attr('id', 'tonystarkrules');

    // And more finally clear the post box
    $('.createpost').val('');
    if ($(".bbeditorholder textarea").length > 0) {
        $(".bbeditorholder textarea").data("sceditor").val('');
    }
    if ($('.wmd-input').length > 0) {
        $(".wmd-input").val('');
        $(".wmd-preview").html('');
    }

    // Re-enable the button
    $('#createpostbutton').attr("disabled", false);
    
    // Finally do an async badge check
    UserPost();
}

function AjaxPostBegin() {
    $('#createpostbutton').attr("disabled", true);
}

function AjaxPostError(message) {
    ShowUserMessage(message);
    $('#createpostbutton').attr("disabled", false);
}

(function ($) {
    $.QueryString = (function(a) {
        if (a == "") return { };
        var b = { };
        for (var i = 0; i < a.length; ++i) {
            var p = a[i].split('=');
            if (p.length != 2) continue;
            b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
        }
        return b;
    })(window.location.search.substr(1).split('&'));
})(jQuery);