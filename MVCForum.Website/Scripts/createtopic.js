
$(function () {

    // Disbale button when form is submitted to stop double posts
    //var form = $(".createtopicholder form");
    //var formButton = $(".submit-holder button");
    //formButton.click(function() {
    //    var button = $(this);
    //    if (form.valid()) {
    //        button.attr("disabled", true);
    //    }
    //});
    
    var topicName = $(".createtopicname");
    if (topicName.length > 0) {
        topicName.focusout(function () {
            var tbValue = $.trim(topicName.val());
            var length = tbValue.length;
            if (length >= 4) {
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

});


function AddNewPollAnswer(counter) {
    var placeHolder = $('#pollanswerplaceholder').val();
    var liHolder = $(document.createElement('li')).attr("id", 'answer' + counter);
    liHolder.html('<input type="text" name="PollAnswers[' + counter + '].Answer" id="PollAnswers_' + counter + '_Answer" class="form-control" value="" placeholder="' + placeHolder + '" />');
    liHolder.appendTo(".pollanswerlist");
}
