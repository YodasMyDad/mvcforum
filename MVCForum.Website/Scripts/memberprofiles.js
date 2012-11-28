$(function () {

    // Get the this week earners
    $.get(app_base + "Members/GetMemberDiscussions",
    function (data) {
        $(".thisweekleaderboard").html(data);
    });


});