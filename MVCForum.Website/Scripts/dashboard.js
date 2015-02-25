$(function () {
    CallHome();
    LatestUsers();
    LowestPointUsers();
    LowestPointPosts();
    HighestViewedTopics();
    LatestNews();
    TodaysTopics();
});

function LatestUsers() {
    $.post(app_base + "Admin/Dashboard/LatestUsers",
    function (data) {
        $(".dashboardlatestusers").html(data);
    });
}

function LowestPointUsers() {
    $.post(app_base + "Admin/Dashboard/LowestPointUsers",
    function (data) {
        $(".dashboardlowestpointusers").html(data);
    });
}

function LowestPointPosts() {
    $.post(app_base + "Admin/Dashboard/LowestPointPosts",
    function (data) {
        $(".dashboardlowestpointposts").html(data);
    });
}

function TodaysTopics() {
    $.post(app_base + "Admin/Dashboard/TodaysTopics",
    function (data) {
        $(".dashboardtodaystopics").html(data);
    });
}

function HighestViewedTopics() {
    $.post(app_base + "Admin/Dashboard/HighestViewedTopics",
    function (data) {
        $(".dashboardhighestviewedtopics").html(data);
    });
}

function LatestNews() {
    $.post(app_base + "Admin/Dashboard/MvcForumLatestNews",
    function (data) {
        $(".mvcforumlatestnews").html(data);
    });
}
function CallHome() {
    $.post(app_base + "Admin/Admin/Aptitude", function (data) {
    });
}