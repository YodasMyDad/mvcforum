$(function () {
    // Some manual ajax badge checks
    BadgeQueryString();
});


var BadgeQueryString = function () {
    var postBadgeQs = $.QueryString["postbadges"];
    if (typeof postBadgeQs != "undefined") {
        if (postBadgeQs == "true") {
            // Do a post badge check
            TriggerPostBadges();
        }
    }
};

function BadgeMarkAsSolution(postId) {

    // Ajax call to post the view model to the controller
    var markAsSolutionBadgeViewModel = new Object();
    markAsSolutionBadgeViewModel.PostId = postId;

    // Ajax call to post the view model to the controller
    var strung = JSON.stringify(markAsSolutionBadgeViewModel);

    $.ajax({
        url: app_base + "Badge/MarkAsSolution",
        type: "POST",
        cache: false,
        data: strung,
        contentType: "application/json; charset=utf-8",
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
        url: app_base + "Badge/Favourite",
        type: "POST",
        cache: false,
        data: strung,
        contentType: "application/json; charset=utf-8",
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
        url: app_base + "Badge/VoteUpPost",
        type: "POST",
        cache: false,
        data: strung,
        contentType: "application/json; charset=utf-8",
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
        url: app_base + "Badge/VoteDownPost",
        type: "POST",
        cache: false,
        data: strung,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            // No need to do anything
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowUserMessage("Error: " + xhr.status + " " + thrownError);
        }
    });
}

function TriggerPostBadges() {
    $.ajax({
        url: app_base + "Badge/Post",
        type: "POST",
        cache: false,
        success: function (data) {
            // No need to do anything
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowUserMessage("Error: " + xhr.status + " " + thrownError);
        }
    });
}
