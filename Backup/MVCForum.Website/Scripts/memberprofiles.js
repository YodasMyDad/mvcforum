$(function () {

    // Get the this week earners
    $.get("/Members/GetMemberDiscussions",
    function (data) {
        $(".thisweekleaderboard").html(data);
    });


});