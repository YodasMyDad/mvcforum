$(function () {
    doFancyBox();
    PostattachmentFancybox();
    ShowFileUploadClickHandler();
    DisplayWaitForPostUploadClickHandler();
    AddPostClickEvents();
    ShowPostOptions();
    TopicShowMorePosts();
    PostGetAllLikes();
    SelectPollAnswer();
    VoteInPoll();
    ShowPostEditHistory();
    ModerateTopicPosts();
});

var ModerateTopicPosts = function() {
    var moderatepanel = $(".moderatepanelnav");
    if (moderatepanel) {
        moderatepanel.click(function (e) {
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

                    // Add validate click event
                    AddModerateClickEvents();
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });
        });
    }
};

var AddModerateClickEvents = function () {

    var approvetopic = $('.topicaction');
    if (approvetopic.length > 0) {
        approvetopic.click(function (e) {
            e.preventDefault();
            var id = $(this).data("topicid");
            var action = $(this).data("topicaction");
            var snippetHolder = $('#topic-' + id);
            var approve = true;
            if (action === "delete") {
                if (!confirm(areYouSureText)) {
                    return false;
                }
                approve = false;
            }

            var moderateActionViewModel = new Object();
            moderateActionViewModel.IsApproved = approve;
            moderateActionViewModel.TopicId = id;
            var strung = JSON.stringify(moderateActionViewModel);

            $.ajax({
                url: app_base + 'Moderate/ModerateTopic',
                type: 'POST',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    if (data === "allgood") {
                        snippetHolder.fadeOut('fast');
                    } else {
                        ShowUserMessage(data);
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }

    var approvepost = $('.postaction');
    if (approvepost.length > 0) {
        approvepost.click(function (e) {
            e.preventDefault();
            var id = $(this).data("postid");
            var action = $(this).data("postaction");
            var snippetHolder = $('#post-' + id);
            var approve = true;
            if (action === "delete") {
                if (!confirm(areYouSureText)) {
                    return false;
                }
                approve = false;
            }

            var moderateActionViewModel = new Object();
            moderateActionViewModel.IsApproved = approve;
            moderateActionViewModel.PostId = id;
            var strung = JSON.stringify(moderateActionViewModel);

            $.ajax({
                url: app_base + 'Moderate/ModeratePost',
                type: 'POST',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    if (data === "allgood") {
                        snippetHolder.fadeOut('fast');
                    } else {
                        ShowUserMessage(data);
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }

}

var ShowPostEditHistory = function() {
    var showpostedithistory = $(".showpostedithistory");
    if (showpostedithistory) {
        showpostedithistory.click(function(e) {
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
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });
        });
    }
};

var SelectPollAnswer = function () {
    var pollanswerselect = $(".pollanswerselect");
    if (pollanswerselect.length > 0) {
        // Poll vote radio button click
        pollanswerselect.click(function () {
            //Firstly Show the submit poll button
            $(".pollvotebuttonholder").show();
            // set the value of the hidden input to the answer value
            var answerId = $(this).data("answerid");
            $(".selectedpollanswer").val(answerId);
        });
    }
};

var VoteInPoll = function () {
    var pollvotebutton = $(".pollvotebutton");
    if (pollvotebutton.length > 0) {
        pollvotebutton.click(function (e) {
            e.preventDefault();

            var pollId = $("#Poll_Id").val();
            var answerId = $(".selectedpollanswer").val();

            var updatePollViewModel = new Object();
            updatePollViewModel.PollId = pollId;
            updatePollViewModel.AnswerId = answerId;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(updatePollViewModel);

            $.ajax({
                url: app_base + "Poll/UpdatePoll",
                type: "POST",
                dataType: "html",
                data: strung,
                cache: false,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $(".pollcontainer").html(data);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }
};

var TopicShowMorePosts = function () {
    var smp = $(".showmoreposts");
    if (smp.length > 0) {
        smp.click(function (e) {
            e.preventDefault();

            var topicId = $("#topicId").val();
            var pageIndex = $("#pageIndex");
            var totalPages = parseInt($("#totalPages").val());
            var activeText = $("span.smpactive");
            var loadingText = $("span.smploading");
            var showMoreLink = $(this);

            activeText.hide();
            loadingText.show();

            var getMorePostsViewModel = new Object();
            getMorePostsViewModel.TopicId = topicId;
            getMorePostsViewModel.PageIndex = pageIndex.val();
            getMorePostsViewModel.Order = $.QueryString["order"];

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(getMorePostsViewModel);

            $.ajax({
                url: app_base + 'Topic/AjaxMorePosts',
                type: 'POST',
                dataType: 'html',
                data: strung,
                cache: false,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    // Now add the new posts
                    AddNewPosts(showMoreLink, data);

                    // Update the page index value
                    var newPageIdex = (parseInt(pageIndex.val()) + parseInt(1));
                    pageIndex.val(newPageIdex);

                    // If the new pageindex is greater than the total pages, then hide the show more button
                    if (newPageIdex > totalPages) {
                        showMoreLink.hide();
                    }

                    // Lastly reattch the click events
                    AddPostClickEvents();
                    ShowFileUploadClickHandler();
                    activeText.show();
                    loadingText.hide();
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

var ShowPostOptions = function () {
    var postOptionButton = $(".postoptions");
    if (postOptionButton.length > 0) {
        postOptionButton.click(function (e) {
            e.preventDefault();
            var thisButton = $(this);
            var postadmin = thisButton.closest(".postadmin");
            var postAdminList = postadmin.find(".postadminlist");
            postAdminList.slideToggle("fast");
        });
    }
};

var doFancyBox = function() {
    var elements = $("div.fileupload a[href$=\".gif\"], div.fileupload a[href$=\".jpg\"], div.fileupload a[href$=\".png\"], div.fileupload a[href$=\".bmp\"], div.fileupload a[href$=\".jpeg\"]");
    if (elements.length > 0) {
        elements.fancybox({
            openEffect: "elastic",
            closeEffect: "elastic"
        });
    }
};

var PostattachmentFancybox = function() {
    var uploadImages = $("a.fileupload[href$=\".gif\"], a.fileupload[href$=\".jpg\"], a.fileupload[href$=\".png\"], a.fileupload[href$=\".bmp\"], a.fileupload[href$=\".jpeg\"]");
    if (uploadImages.length > 0) {
        uploadImages.fancybox({
            openEffect: "elastic",
            closeEffect: "elastic"
        });
    }
}

var ShowFileUploadClickHandler = function () {
    var attachButton = $(".postshowattach");
    if (attachButton.length > 0) {
        attachButton.click(function (e) {
            e.preventDefault();
            var postHolder = $(this).closest("div.post");
            var uploadHolder = postHolder.find("div.postuploadholder");
            uploadHolder.toggle();
        });
    }
}
var AjaxPostSuccess = function () {
    // Grab the span the newly added post is in
    var postHolder = $("#newpostmarker");

    // Now add a new span after with the key class
    // In case the user wants to add another ajax post straight after
    postHolder.after("<span id=\"newpostmarker\"></span>");

    // Finally chnage the name of this element so it doesn't insert it into the same one again
    postHolder.attr("id", "tonystarkrules");

    // And more finally clear the post box
    $(".rte").val("");
    if ($(".bbeditorholder textarea").length > 0) {
        $(".bbeditorholder textarea").data("sceditor").val('');
    }
    if ($(".wmd-input").length > 0) {
        $(".wmd-input").val("");
        $(".wmd-preview").html("");
    }
    if (typeof tinyMCE != "undefined") {
        tinyMCE.activeEditor.setContent("");
    }

    // Clear the reply div
    var replyToDiv = $(".showreplyto");
    replyToDiv.html("");

    // Re-enable the button
    AjaxPostFinish();

    // Finally do an async badge check
    TriggerPostBadges();

    // Attached the upload click events
    ShowFileUploadClickHandler();
    DisplayWaitForPostUploadClickHandler();
}

var DisplayWaitForPostUploadClickHandler = function () {
    var postUploadButton = $(".postuploadbutton");
    if (postUploadButton.length > 0) {
        postUploadButton.click(function (e) {
            var uploadHolder = $(this).closest("div.postuploadholder");
            var ajaxSpinner = uploadHolder.find("span.ajaxspinner");
            ajaxSpinner.show();
            $(this).fadeOut("fast");
        });
    }
}

var AddPostClickEvents = function () {

    ShowExpandedVotes();

    $(".solutionlink").click(function (e) {
        var solutionHolder = $(this);
        var postId = solutionHolder.data('id');

        var markAsSolutionViewModel = new Object();
        markAsSolutionViewModel.Post = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(markAsSolutionViewModel);

        $.ajax({
            url: app_base + "Vote/MarkAsSolution",
            type: "POST",
            cache: false,
            data: strung,
            contentType: "application/json; charset=utf-8",
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

    $(".votelink").click(function (e) {
        e.preventDefault();
        var voteLink = $(this);
        var postId = voteLink.data("id");
        var voteType = voteLink.data("votetype");
        var holdingLi = voteLink.closest("li");
        var holdingUl = holdingLi.closest("ul");
        var votePoints = holdingLi.find(".count");
        var voteText = voteLink.data("votetext");
        var votedText = voteLink.data("votedtext");
        var hasVoted = voteLink.data("hasvoted");

        // Remove all vote links
        //holdingUl.find(".votelink").fadeOut("fast");

        var voteUpViewModel = new Object();
        voteUpViewModel.Post = postId;

        var voteUrl = "Vote/VoteDownPost";
        var otherVoteLink = holdingUl.find(".votelink[data-votetype='up']");
        if (voteType == "up") {
            voteUrl = "Vote/VoteUpPost";
            otherVoteLink = holdingUl.find(".votelink[data-votetype='down']");
        }

        // We do the show hide/change of votes here for speed in the UI
        // Change the number up or down
        var currentPoints = parseInt(votePoints.text());
        if (hasVoted) {
            // They are removing their vote
            votePoints.text((currentPoints - 1));
            voteLink.text(voteText);

            // So show the other link
            otherVoteLink.show();

            // Change has voted to false
            voteLink.data('hasvoted', false);
        } else {
            // They add adding a vote
            votePoints.text((currentPoints + 1));
            voteLink.text(votedText);

            // Hide the other link
            otherVoteLink.hide();

            // Change has voted to false
            voteLink.data('hasvoted', true);
        }

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(voteUpViewModel);

        $.ajax({
            url: app_base + voteUrl,
            type: "POST",
            cache: false,
            data: strung,
            contentType: "application/json; charset=utf-8",
            success: function (data) {

                if (voteType == "up") {
                    BadgeVoteUp(postId);
                } else {
                    BadgeVoteDown(postId);
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });

    $(".post a.favourite").click(function (e) {
        e.preventDefault();
        var favLink = $(this);

        // Data attributes
        var postId = favLink.data("id");
        var addremove = favLink.data("addremove");

        // Holding li
        var holdingLi = favLink.closest(".postsocial li");

        // count and star
        var amountOfFavsHolder = holdingLi.find("span.count");
        var currentFavCount = parseInt(amountOfFavsHolder.text());
        var favStar = holdingLi.find(".glyphicon");

        // Are we on the my favourites page
        var myFavsHolder = $("div.myfavourites");

        var voteViewModel = new Object();
        voteViewModel.PostId = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(voteViewModel);

        var ajaxUrl = "Favourite/FavouritePost";

        $.ajax({
            url: app_base + ajaxUrl,
            type: "POST",
            cache: false,
            data: strung,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {

                // Update the text
                favLink.text(data.Message);

                // Update the count and change the link type
                if (addremove == "add") {
                    amountOfFavsHolder.text(currentFavCount + 1);
                    favStar.removeClass("glyphicon-star-empty").addClass("glyphicon-star");
                    favLink.data("addremove", "remove");

                    BadgeFavourite(data.Id);
                } else {
                    amountOfFavsHolder.text(currentFavCount - 1);
                    favStar.removeClass("glyphicon-star").addClass("glyphicon-star-empty");
                    favLink.data("addremove", "add");
                    if (myFavsHolder.length > 0) {
                        // We are on the member my favourites page - So find and remove the post
                        var postHolder = favLink.closest("div.post");
                        postHolder.fadeOut("fast");
                    }
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });
}


var PostGetAllLikes = function () {
    var othersliked = $(".othersliked a");
    if (othersliked.length > 0) {
        othersliked.click(function (e) {
            e.preventDefault();
            var othersLink = $(this).parent();
            var postId = othersLink.data("postid");
            var holdingDiv = othersLink.closest(".postlikedby");

            $.ajax({
                url: app_base + "Post/GetAllPostLikes",
                type: "POST",
                dataType: "html",
                cache: false,
                data: { id: postId },
                success: function (data) {
                    holdingDiv.html(data);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }
};


var AddNewPosts =function (showMoreLink, posts) {
    showMoreLink.before(posts);
}

var MarkAsSolution = function (solutionHolder) {
    var solvedClass = "glyphicon-ok green-colour";
    var notSolvedClass = "glyphicon-question-sign";
    solutionHolder.unbind("click");
    var icon = solutionHolder.closest("li").find("." + notSolvedClass);
    icon.removeClass(notSolvedClass).addClass(solvedClass);
    $(".solutionlink").hide();
    $("." + notSolvedClass).hide();
}

var AjaxPostBegin = function () {
    var createButton = $("#createpostbutton");
    if (createButton.length > 0) {
        createButton.attr("disabled", true);
    }
    var pmButton = $("#createpmbutton");
    if (pmButton.length > 0) {
        pmButton.attr("disabled", true);
    }
}

var AjaxPostFinish = function () {
    var createButton = $("#createpostbutton");
    if (createButton.length > 0) {
        createButton.attr("disabled", false);
    }
    var pmButton = $("#createpmbutton");
    if (pmButton.length > 0) {
        pmButton.attr("disabled", false);
    }
}

var AjaxPostError = function(message) {
    ShowUserMessage(message);
    AjaxPostFinish();
}

var SuccessfulThumbDown = function(karmascore) {
    var currentKarma = parseInt($(karmascore).text());
    $(karmascore).text((currentKarma - 1));
}

var ShowExpandedVotes = function () {
    var expandVotes = $(".expandvotes");
    if (expandVotes.length > 0) {
        expandVotes.click(function (e) {
            e.preventDefault();

            var votesSpan = $(this).find("span.votenumber");

            var expandVotesViewModel = new Object();
            expandVotesViewModel.Post = votesSpan.attr("id");

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(expandVotesViewModel);
            var thisExpandVotes = $(this);
            $.ajax({
                url: app_base + "Vote/GetVotes",
                type: "POST",
                cache: false,
                dataType: "html",
                data: strung,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    votesSpan.html(data);
                    // remove the css class to remove the click pointer and show multple calls to this
                    thisExpandVotes.removeClass("expandvotes");
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }
}
