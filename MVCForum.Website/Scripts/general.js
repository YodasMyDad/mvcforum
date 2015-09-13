//---------------- On Ready ------------------------

$(function () {

    PostattachmentFancybox();

    //ChangeLanguage();

    // Attach files click handler
    ShowFileUploadClickHandler();

    DisplayWaitForPostUploadClickHandler();

    // Sort the date of the member
    SortWhosOnline();

    ResponsiveTable();

    // We add the post click events like this, so we can reattach when we do the show more posts
    AddPostClickEvents();

    var mobilenavbutton = $('.showmobilenavbar');
    if (mobilenavbutton.length > 0) {
        mobilenavbutton.click(function (e) {
            e.preventDefault();
            $('.mobilenavbar-inner ul.nav').slideToggle();
        });
    }   

    // Topic Methods
    ShowPostOptions();
    TopicShowMorePosts();

    // Private Message Methods

    showPrivateMessagesPanel();
    deletePrivateMessages();
    blockMember();
    PmShowMorePosts();

    // Subscription Methods

    emailsubscription();
    emailunsubscription();

    // Some manual ajax badge checks
    var postBadgeQs = $.QueryString["postbadges"];
    if (typeof postBadgeQs != 'undefined') {
        if (postBadgeQs == "true") {
            // Do a post badge check
            UserPost();
        }
    }

    // Poll vote radio button click
    $(".pollanswerselect").click(function () {
        //Firstly Show the submit poll button
        $('.pollvotebuttonholder').show();
        // set the value of the hidden input to the answer value
        var answerId = $(this).data("answerid");
        $('.selectedpollanswer').val(answerId);
    });


    $(".pollvotebutton").click(function (e) {
        e.preventDefault();

        var pollId = $('#Poll_Id').val();
        var answerId = $('.selectedpollanswer').val();

        var UpdatePollViewModel = new Object();
        UpdatePollViewModel.PollId = pollId;
        UpdatePollViewModel.AnswerId = answerId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(UpdatePollViewModel);

        $.ajax({
            url: app_base + 'Poll/UpdatePoll',
            type: 'POST',
            dataType: 'html',
            data: strung,
            cache: false,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $(".pollcontainer").html(data);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });

    });

    $('.btn-file :file').on('fileselect', function (event, numFiles, label) {

        var input = $(this).parents('.input-group').find(':text'),
            log = numFiles > 1 ? numFiles + ' files selected' : label;

        if (input.length) {
            input.val(log);
        } else {
            if (log) alert(log);
        }

    });

    hideSlideOutPanel();

    PostGetAllLikes();

});

var ShowPostOptions = function() {
    var postOptionButton = $(".postoptions");
    if (postOptionButton.length > 0) {
        postOptionButton.click(function(e) {
            var thisButton = $(this);
            var postadmin = thisButton.closest(".postadmin");
            var postAdminList = postadmin.find(".postadminlist");
            postAdminList.slideToggle("fast");
        });
    }
};

var TopicShowMorePosts = function() {
    var smp = $(".showmoreposts");
    if (smp.length > 0) {
        smp.click(function (e) {
            e.preventDefault();

            var topicId = $('#topicId').val();
            var pageIndex = $('#pageIndex');
            var totalPages = parseInt($('#totalPages').val());
            var activeText = $('span.smpactive');
            var loadingText = $('span.smploading');
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

var PostGetAllLikes = function() {
    var othersliked = $('.othersliked a');
    if (othersliked.length > 0) {
        othersliked.click(function (e) {
            e.preventDefault();
            var othersLink = $(this).parent();
            var postId = othersLink.data("postid");
            var holdingDiv = othersLink.closest(".postlikedby");

            $.ajax({
                url: app_base + 'Post/GetAllPostLikes',
                type: 'POST',
                dataType: 'html',
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

//

/*--------------- PANEL SLIDER -----------------*/

//var quickcart = function () {
//    var quickcartLink = $(".quickcart");
//    if (quickcartLink.length > 0) {
//        quickcartLink.click(function (e) {
//            e.preventDefault();
//            var thisButton = $(this);
//            var title = thisButton.data("title");
//            slideOutPanel(title);

//            // Get the url for the page to call via ajax
//            var url = "/umbraco/surface/cart/quickcart";

//            $.get(url, function (data) {
//                slideOutPanel(title, data);
//            });

//        });
//    }
//}
/*------------------ General Ajax Form Class ----------------------*/
var submitAjaxForm = function () {
    var submitFormElement = $(".ajaxform form");
    submitFormData(submitFormElement);
};

/*------------------ GENERAL SUBMIT FORMS METHOD -------------------*/

var submitFormData = function (formElement) {
    formElement.submit(function (e) {
        e.preventDefault();
        $(this).validate();
        if ($(this).valid()) {
            $.ajax({
                url: this.action,
                type: this.method,
                data: $(this).serialize(),
                dataType: "json",
                cache: false,
                success: function (result) {
                    if (result.Success) {
                        closeSlideOutPanel();
                        ShowUserMessage(result.ReturnMessage);
                    } else {
                        ShowUserMessage(result.ReturnMessage);
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });
        }
        return false;
    });
};

/*------------ SUBSCRIPTION METHODS --------------------*/

var emailsubscription = function () {
    var esub = $(".emailsubscription");
    if (esub.length > 0) {
        esub.click(function (e) {
            e.preventDefault();
            var entityId = $(this).data('id');
            var subscriptionType = $(this).data('type');

            $(this).hide();

            var subscribeEmailViewModel = new Object();
            subscribeEmailViewModel.Id = entityId;
            subscribeEmailViewModel.SubscriptionType = subscriptionType;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(subscribeEmailViewModel);

            $.ajax({
                url: app_base + 'Email/Subscribe',
                type: 'POST',
                cache: false,
                data: strung,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    $(".emailunsubscription").fadeIn();
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });
        }); 
    }
};

var emailunsubscription = function () {
    var eunsub = $(".emailunsubscription");
    if (eunsub.length > 0) {
        eunsub.click(function (e) {
            e.preventDefault();
            var entityId = $(this).data('id');
            var subscriptionType = $(this).data('type');

            $(this).hide();

            var unSubscribeEmailViewModel = new Object();
            unSubscribeEmailViewModel.Id = entityId;
            unSubscribeEmailViewModel.SubscriptionType = subscriptionType;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(unSubscribeEmailViewModel);

            $.ajax({
                url: app_base + 'Email/UnSubscribe',
                type: 'POST',
                cache: false,
                data: strung,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    $(".emailsubscription").fadeIn();
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });
        });
    }
};


/*------------ PRIVATE MESSAGE METHODS --------------------*/

var PmShowMorePosts = function () {
    var smp = $(".showmorepms");
    if (smp.length > 0) {
        smp.click(function (e) {
            e.preventDefault();

            var userId = $('#userId').val();
            var pageIndex = $('#pageIndex');
            var totalPages = parseInt($('#totalPages').val());
            var activeText = $('span.smpmactive');
            var loadingText = $('span.smpmloading');
            var showMoreLink = $(this);

            activeText.hide();
            loadingText.show();

            var getMoreViewModel = new Object();
            getMoreViewModel.UserId = userId;
            getMoreViewModel.PageIndex = pageIndex.val();

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(getMoreViewModel);

            $.ajax({
                url: app_base + 'PrivateMessage/AjaxMore',
                type: 'POST',
                dataType: 'html',
                cache: false,
                data: strung,
                contentType: 'application/json; charset=utf-8',
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

var blockMember = function () {
    var blockMemberButton = $(".pm-block");
    if (blockMemberButton.length > 0) {
        blockMemberButton.click(function(e) {
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
                url: app_base + 'Block/BlockOrUnBlock',
                type: 'POST',
                data: strung,
                cache: false,
                contentType: 'application/json; charset=utf-8',
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
                type: 'GET',
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
            if (confirm(closeconfirmationText)) {
                event.preventDefault();
                panel.removeClass("is-visible");
                //Clear fields
                var contentDiv = panel.find(".cd-panel-content");
                contentDiv.html(largeSpinnerBlock);
                var titleDiv = panel.find(".cd-panel-header h6");
                titleDiv.html("");
                $("body").css("overflow", "").css("height", "");
                $("html").css("overflow", "").css("height", "");
            }
            return false;
        }
    });
};
var closeSlideOutPanel = function () {
    var panel = $(".cd-panel");
    panel.trigger("click");
};

/*--------------- END PANEL SLIDER -----------------*/

$(document).on('change', '.btn-file :file', function () {
    var input = $(this),
        numFiles = input.get(0).files ? input.get(0).files.length : 1,
        label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
    input.trigger('fileselect', [numFiles, label]);
});

function ResponsiveTable() {
    var adaptiveTable = $('.table-adaptive');
    if (adaptiveTable.length > 0) {
        var headertext = [],
        headers = document.querySelectorAll(".table-adaptive th"),
        tablerows = document.querySelectorAll(".table-adaptive th"),
        tablebody = document.querySelector(".table-adaptive tbody");

        for (var i = 0; i < headers.length; i++) {
            var current = headers[i];
            headertext.push(current.textContent.replace(/\r?\n|\r/, ""));
        }
        if (tablebody.rows != null) {
            for (var i = 0, row; row = tablebody.rows[i]; i++) {
                for (var j = 0, col; col = row.cells[j]; j++) {
                    col.setAttribute("data-th", headertext[j]);
                }
            }
        }
    }
}

function PostattachmentFancybox() {
    var uploadImages = $('a.fileupload[href$=".gif"], a.fileupload[href$=".jpg"], a.fileupload[href$=".png"], a.fileupload[href$=".bmp"], a.fileupload[href$=".jpeg"]');
    if (uploadImages.length > 0) {
        uploadImages.fancybox({
            openEffect: 'elastic',
            closeEffect: 'elastic'
        });
    }
}

function ChangeLanguage() {
    var languageSelect = $(".languageselector select");
    if (languageSelect.length > 0) {
        languageSelect.change(function () {
            var langVal = this.value;
            $.ajax({
                url: '/Language/ChangeLanguage',
                type: 'POST',
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

function DisplayWaitForPostUploadClickHandler() {
    var postUploadButton = $('.postuploadbutton');
    if (postUploadButton.length > 0) {
        postUploadButton.click(function (e) {
            var uploadHolder = $(this).closest("div.postuploadholder");
            var ajaxSpinner = uploadHolder.find("span.ajaxspinner");
            ajaxSpinner.show();
            $(this).fadeOut("fast");
        });
    }
}

function ShowFileUploadClickHandler() {
    var attachButton = $('.postshowattach');
    if (attachButton.length > 0) {
        attachButton.click(function (e) {
            e.preventDefault();
            var postHolder = $(this).closest("div.post");
            var uploadHolder = postHolder.find("div.postuploadholder");
            uploadHolder.toggle();
        });
    }
}

function SortWhosOnline() {
    $.getJSON(app_base + 'Members/LastActiveCheck');
}

function AddPostClickEvents() {

    ShowExpandedVotes();

    $(".solutionlink").click(function (e) {
        var solutionHolder = $(this);
        var postId = solutionHolder.data('id');

        var markAsSolutionViewModel = new Object();
        markAsSolutionViewModel.Post = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(markAsSolutionViewModel);

        $.ajax({
            url: app_base + 'Vote/MarkAsSolution',
            type: 'POST',
            cache: false,
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

    $(".votelink").click(function (e) {
        e.preventDefault();
        var voteLink = $(this);
        var postId = voteLink.data('id');
        var voteType = voteLink.data('votetype');
        var holdingLi = voteLink.closest("li");
        var holdingUl = holdingLi.closest("ul");
        var votePoints = holdingLi.find(".count");
        var voteText = voteLink.data('votetext');
        var votedText = voteLink.data('votedtext');
        var hasVoted = voteLink.data('hasvoted');
       
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
            type: 'POST',
            cache: false,
            data: strung,
            contentType: 'application/json; charset=utf-8',
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
        var postId = favLink.data('id');
        var addremove = favLink.data('addremove');

        // Holding li
        var holdingLi = favLink.closest('.postsocial li');

        // count and star
        var amountOfFavsHolder = holdingLi.find('span.count');
        var currentFavCount = parseInt(amountOfFavsHolder.text());
        var favStar = holdingLi.find('.glyphicon');

        // Are we on the my favourites page
        var myFavsHolder = $('div.myfavourites');

        var voteViewModel = new Object();
        voteViewModel.PostId = postId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(voteViewModel);

        var ajaxUrl = "Favourite/FavouritePost";

        $.ajax({
            url: app_base + ajaxUrl,
            type: 'POST',
            cache: false,
            data: strung,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (data) {

                // Update the text
                favLink.text(data.Message);

                // Update the count and change the link type
                if (addremove == "add") {
                    amountOfFavsHolder.text(currentFavCount + 1);
                    favStar.removeClass("glyphicon-star-empty").addClass("glyphicon-star");
                    favLink.data('addremove', 'remove');

                    BadgeFavourite(data.Id);
                } else {
                    amountOfFavsHolder.text(currentFavCount - 1);
                    favStar.removeClass("glyphicon-star").addClass("glyphicon-star-empty");
                    favLink.data('addremove', 'add');
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

function AddNewPosts(showMoreLink, posts) {
    showMoreLink.before(posts);
}

function ShowExpandedVotes() {
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
                url: app_base + 'Vote/GetVotes',
                type: 'POST',
                cache: false,
                dataType: 'html',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    votesSpan.html(data);
                    // remove the css class to remove the click pointer and show multple calls to this
                    thisExpandVotes.removeClass('expandvotes');
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }
}

// Unused now
//function AddShowVoters() {
//    var showVoters = $(".showvoters");
//    if (showVoters.length > 0) {
//        // Container/Parent
//        showVoters.click(function (e) {
//            e.preventDefault();
//            // This the child box
//            var voterBox = $(this).find('.showvotersbox');

//            // firstly set the left position
//            voterBox.css("left", voterBox.parent().width() + 2);

//            // Now show it
//            voterBox.toggle();

//            if (voterBox.is(":visible")) {
//                // Is being shown so do the Ajax call

//                var GetVotersViewModel = new Object();
//                GetVotersViewModel.Post = voterBox.attr("id");

//                // Ajax call to post the view model to the controller
//                var strung = JSON.stringify(GetVotersViewModel);

//                $.ajax({
//                    url: app_base + 'Vote/GetVoters',
//                    type: 'POST',
//                    dataType: 'html',
//                    data: strung,
//                    contentType: 'application/json; charset=utf-8',
//                    success: function (data) {
//                        voterBox.html(data);
//                    },
//                    error: function (xhr, ajaxOptions, thrownError) {
//                        ShowUserMessage("Error: " + xhr.status + " " + thrownError);
//                    }
//                });

//            }
//        });
//    }
//}


//function ShowHideRemovePollAnswerButton(counter) {
//    var removeButton = $('.removeanswer');
//    if(counter > 1) {
//        removeButton.show();
//    } else {
//        removeButton.hide();
//    }
//}

//---------------- Functions------------------------

function BadgeMarkAsSolution(postId) {

    // Ajax call to post the view model to the controller
    var markAsSolutionBadgeViewModel = new Object();
    markAsSolutionBadgeViewModel.PostId = postId;

    // Ajax call to post the view model to the controller
    var strung = JSON.stringify(markAsSolutionBadgeViewModel);

    $.ajax({
        url: app_base + 'Badge/MarkAsSolution',
        type: 'POST',
        cache: false,
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

function BadgeFavourite(favouriteId) {

    // Ajax call to post the view model to the controller
    var favouriteViewModel = new Object();
    favouriteViewModel.FavouriteId = favouriteId;

    // Ajax call to post the view model to the controller
    var strung = JSON.stringify(favouriteViewModel);

    $.ajax({
        url: app_base + 'Badge/Favourite',
        type: 'POST',
        cache: false,
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
        url: app_base + 'Badge/VoteUpPost',
        type: 'POST',
        cache: false,
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

function BadgeVoteDown(postId) {

    // Ajax call to post the view model to the controller
    var voteUpBadgeViewModel = new Object();
    voteUpBadgeViewModel.PostId = postId;

    // Ajax call to post the view model to the controller
    var strung = JSON.stringify(voteUpBadgeViewModel);

    $.ajax({
        url: app_base + 'Badge/VoteDownPost',
        type: 'POST',
        cache: false,
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
        url: app_base + 'Badge/Post',
        type: 'POST',
        cache: false,
        success: function (data) {
            // No need to do anything
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowUserMessage("Error: " + xhr.status + " " + thrownError);
        }
    });
}

function MarkAsSolution(solutionHolder) {
    var solvedClass = "glyphicon-ok green-colour";
    var notSolvedClass = "glyphicon-question-sign";
    solutionHolder.unbind("click");
    var icon = solutionHolder.closest("li").find("." + notSolvedClass);
    icon.removeClass(notSolvedClass).addClass(solvedClass);
    $('.solutionlink').hide();
    $('.' + notSolvedClass).hide();
}

function SuccessfulThumbDown(karmascore) {
    var currentKarma = parseInt($(karmascore).text());
    $(karmascore).text((currentKarma - 1));
}

function ShowUserMessage(message) {
    if (message != null) {
        var jsMessage = $('#jsquickmessage');
        var toInject = "<div class=\"alert alert-info fade in\" role=\"alert\"><button type=\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;<\/span><\/button>" + message + "<\/div>";
        jsMessage.html(toInject);
        jsMessage.show();
        $('div.alert').delay(2200).fadeOut();
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
    $('.rte').val('');
    if ($(".bbeditorholder textarea").length > 0) {
        $(".bbeditorholder textarea").data("sceditor").val('');
    }
    if ($('.wmd-input').length > 0) {
        $(".wmd-input").val('');
        $(".wmd-preview").html('');
    }
    if (typeof tinyMCE != "undefined") {
        tinyMCE.activeEditor.setContent('');
    }

    // Re-enable the button
    AjaxPostFinish();

    // Finally do an async badge check
    UserPost();

    // Attached the upload click events
    ShowFileUploadClickHandler();
    DisplayWaitForPostUploadClickHandler();
}

function AjaxPrivateMessageSuccess() {
    // Grab the span the newly added post is in
    var postHolder = $('#newpmmarker');

    // Now add a new span after with the key class
    // In case the user wants to add another ajax post straight after
    postHolder.before('<span id="newpmmarker"></span>');

    // Finally chnage the name of this element so it doesn't insert it into the same one again
    postHolder.attr('id', 'imtonystark');

    // And more finally clear the post box
    $('.createpm').val('');
    if ($(".bbeditorholder textarea").length > 0) {
        $(".bbeditorholder textarea").data("sceditor").val('');
    }
    if ($('.wmd-input').length > 0) {
        $(".wmd-input").val('');
        $(".wmd-preview").html('');
    }
    if (typeof tinyMCE != "undefined") {
        tinyMCE.activeEditor.setContent('');
    }

    // Re-enable the button
    AjaxPostFinish();

    // Finally do an async badge check
    UserPost();
}

function AjaxPostBegin() {
    var createButton = $('#createpostbutton');
    if (createButton.length > 0) {
        createButton.attr("disabled", true);
    }
    var pmButton = $('#createpmbutton');
    if (pmButton.length > 0) {
        pmButton.attr("disabled", true);
    }
}

function AjaxPostFinish() {
    var createButton = $('#createpostbutton');
    if (createButton.length > 0) {
        createButton.attr("disabled", false);
    }
    var pmButton = $('#createpmbutton');
    if (pmButton.length > 0) {
        pmButton.attr("disabled", false);
    }
}

function AjaxPostError(message) {
    ShowUserMessage(message);
    AjaxPostFinish();
}

(function ($) {
    $.QueryString = (function (a) {
        if (a == "") return {};
        var b = {};
        for (var i = 0; i < a.length; ++i) {
            var p = a[i].split('=');
            if (p.length != 2) continue;
            b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
        }
        return b;
    })(window.location.search.substr(1).split('&'));
})(jQuery);