$(function () {

    // Get the this week earners
    $.get("/Snippets/GetThisWeeksTopEarners",
    function (data) {
        $(".thisweekleaderboard").html(data);
    });

    // Get the this week earners
    $.get("/Snippets/GetThisYearsTopEarners",
    function (data) {
        $(".alltimeleaderboard").html(data);
    });

});