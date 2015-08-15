//---------------- On Ready ------------------------

$(function () {

    PostattachmentFancybox();

    ChangeLanguage();

    // Attach files click handler
    ShowFileUploadClickHandler();

    DisplayWaitForPostUploadClickHandler();

    // make code pretty
    window.prettyPrint && prettyPrint();

    $('input, textarea').placeholder();

    // Sort the date of the member
    SortWhosOnline();

    ResponsiveTable();

    //---------------- On Click------------------------

    // We add the post click events like this, so we can reattach when we do the show more posts
    AddPostClickEvents();

    var mobilenavbutton = $('.showmobilenavbar');
    if (mobilenavbutton.length > 0) {
        mobilenavbutton.click(function (e) {
            e.preventDefault();
            $('.mobilenavbar-inner ul.nav').slideToggle();
        });
    }

    var topicName = $(".createtopicname");
    if (topicName.length > 0) {
        topicName.focusout(function () {
            var tbValue = $.trim(topicName.val());
            var length = tbValue.length;
            if (length > 5) {
                // Someone has entered some text more than 5 charactors and now clicked
                // out of the textbox, so search
                $.ajax({
                    url: app_base + 'Topic/GetSimilarTopics',
                    type: 'POST',
                    dataType: 'html',
                    data: { 'searchTerm': tbValue },
                    success: function (data) {
                        if (data != '') {
                            $('.relatedtopicskey').html(data);
                            $('.relatedtopicsholder').show();
                        }
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                    }
                });
            }
        });
    }

    var createTopicCategoryDropdown = $('.createtopicholder #Category');
    if (createTopicCategoryDropdown.length > 0) {
        // This is purely for the UI - All the below permissions are 
        // checked server side so it doesn't matter if they submit
        // something they are not allowed to do. It won't get through

        // Divs to show and hide
        var stickyholder = $('.createtopicholder .createsticky');
        var lockedholder = $('.createtopicholder .createlocked');
        var uploadholder = $('.createtopicholder .createuploadfiles');
        var pollButtonholder = $('.createtopicholder .pollcreatebuttonholder');

        // Fire when the dropdown changes
        createTopicCategoryDropdown.change(function (e) {
            e.preventDefault();
            var catId = $(this).val();
            if (catId != "") {

                // Go and get the ajax model
                $.ajax({
                    url: app_base + 'Topic/CheckTopicCreatePermissions',
                    type: 'POST',
                    dataType: 'json',
                    data: { catId: catId },
                    success: function (data) {
                        if (data.CanStickyTopic) {
                            stickyholder.show();
                        } else {
                            stickyholder.hide();
                        }

                        if (data.CanLockTopic) {
                            lockedholder.show();
                        } else {
                            lockedholder.hide();
                        }

                        if (data.CanUploadFiles) {
                            uploadholder.show();
                        } else {
                            uploadholder.hide();
                        }

                        if (data.CanCreatePolls) {
                            pollButtonholder.show();
                        } else {
                            pollButtonholder.hide();
                        }
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                    }
                });

            }
        });
    }

    $(".showmoreposts").click(function (e) {
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

    $(".privatemessagedelete").click(function (e) {
        e.preventDefault();
        var linkClicked = $(this);
        var messageId = linkClicked.data('id');
        var deletePrivateMessageViewModel = new Object();
        deletePrivateMessageViewModel.Id = messageId;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(deletePrivateMessageViewModel);

        $.ajax({
            url: app_base + 'PrivateMessage/Delete',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                var pmHolder = linkClicked.closest(".pmblock");
                pmHolder.fadeOut("fast");
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });

    });

    $(".emailsubscription").click(function (e) {
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
    //var counter = 0;

    // This check is for the edit page
    //if (typeof counter === 'undefined') {
    //    // variable is undefined
    //} else {
    //    ShowHideRemovePollAnswerButton(counter);
    //}    

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
        counter = 0;
    });

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

    // Add a new answer
    $(".addanswer").click(function (e) {
        e.preventDefault();
        AddNewPollAnswer(counter);
        counter++;
        //ShowHideRemovePollAnswerButton(counter);
    });

    // Remove a poll answer
    $(".removeanswer").click(function (e) {
        e.preventDefault();
        if (counter > 0) {
            counter--;
            $("#answer" + counter).remove();
            //ShowHideRemovePollAnswerButton(counter);
        }
    });

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

});

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
        var postId = $(this).data('id');
        var voteType = $(this).data('votetype');
        var holdingLi = $(this).closest("li");
        var holdingUl = holdingLi.closest("ul");
        var votePoints = holdingLi.find(".count");

        // Remove all vote links
        holdingUl.find(".votelink").fadeOut("fast");

        var voteUpViewModel = new Object();
        voteUpViewModel.Post = postId;

        var voteUrl = "Vote/VoteDownPost";
        if (voteType == "up") {
            voteUrl = "Vote/VoteUpPost";
        }

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(voteUpViewModel);


        $.ajax({
            url: app_base + voteUrl,
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {

                var currentPoints = parseInt($(votePoints).text());
                votePoints.text((currentPoints + 1));

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
            data: strung,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (data) {

                // Update the text
                favLink.text(data);

                // Update the count and change the link type
                if (addremove == "add") {
                    amountOfFavsHolder.text(currentFavCount + 1);
                    favStar.removeClass("glyphicon-star-empty").addClass("glyphicon-star");
                    favLink.data('addremove', 'remove');
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

    //$(".post a.favourite").click(function (e) {
    //    e.preventDefault();
    //    var favLink = $(this);
    //    var postId = favLink.data('postid');

    //    var ajaxUrl = "Favourite/FavouritePost";

    //    var viewModel = new Object();
    //    viewModel.PostId = postId;

    //    // Ajax call to post the view model to the controller
    //    var strung = JSON.stringify(viewModel);

    //    $.ajax({
    //        url: app_base + ajaxUrl,
    //        type: 'POST',
    //        data: strung,
    //        contentType: 'application/json; charset=utf-8',
    //        dataType: 'html',
    //        success: function (data) {
    //            favLink.closest('.post').remove();
    //        },
    //        error: function (xhr, ajaxOptions, thrownError) {
    //            ShowUserMessage("Error: " + xhr.status + " " + thrownError);
    //        }
    //    });
    //});
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
function AddShowVoters() {
    var showVoters = $(".showvoters");
    if (showVoters.length > 0) {
        // Container/Parent
        showVoters.click(function (e) {
            e.preventDefault();
            // This the child box
            var voterBox = $(this).find('.showvotersbox');

            // firstly set the left position
            voterBox.css("left", voterBox.parent().width() + 2);

            // Now show it
            voterBox.toggle();

            if (voterBox.is(":visible")) {
                // Is being shown so do the Ajax call

                var GetVotersViewModel = new Object();
                GetVotersViewModel.Post = voterBox.attr("id");

                // Ajax call to post the view model to the controller
                var strung = JSON.stringify(GetVotersViewModel);

                $.ajax({
                    url: app_base + 'Vote/GetVoters',
                    type: 'POST',
                    dataType: 'html',
                    data: strung,
                    contentType: 'application/json; charset=utf-8',
                    success: function (data) {
                        voterBox.html(data);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                    }
                });

            }
        });
    }
}

function AddNewPollAnswer(counter) {
    var placeHolder = $('#pollanswerplaceholder').val();
    var liHolder = $(document.createElement('li')).attr("id", 'answer' + counter);
    liHolder.html('<input type="text" name="PollAnswers[' + counter + '].Answer" id="PollAnswers_' + counter + '_Answer" class="form-control" value="" placeholder="' + placeHolder + '" />');
    liHolder.appendTo(".pollanswerlist");
}

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
    $('.createpost').val('');
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
    $('#createpostbutton').attr("disabled", false);

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
    $('#createpmbutton').attr("disabled", false);

    // Finally do an async badge check
    UserPost();
}

function AjaxPostBegin() {
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
    AjaxPostBegin();
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