$(function () {

    // Get the this week earners
    $.get(app_base + "Snippets/GetThisWeeksTopEarners",
    function (data) {
        $(".thisweekleaderboard").html(data);
    });

    // Get the this week earners
    $.get(app_base + "Snippets/GetThisYearsTopEarners",
    function (data) {
        $(".alltimeleaderboard").html(data);
    });

});