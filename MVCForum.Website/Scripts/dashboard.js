$(function () {
    CallHome();
    LatestUsers();
    LowestPointUsers();
    LowestPointPosts();
    HighestViewedTopics();
    LatestNews();
});

function LatestUsers() {
    $.post("/Admin/Dashboard/LatestUsers",
    function (data) {
        $(".dashboardlatestusers").html(data);
    });
}

function LowestPointUsers() {
    $.post("/Admin/Dashboard/LowestPointUsers",
    function (data) {
        $(".dashboardlowestpointusers").html(data);
    });
}

function LowestPointPosts() {
    $.post("/Admin/Dashboard/LowestPointPosts",
    function (data) {
        $(".dashboardlowestpointposts").html(data);
    });
}

function HighestViewedTopics() {
    $.post("/Admin/Dashboard/HighestViewedTopics",
    function (data) {
        $(".dashboardhighestviewedtopics").html(data);
    });
}

function LatestNews() {
    $.post("/Admin/Dashboard/MvcForumLatestNews",
    function (data) {
        $(".mvcforumlatestnews").html(data);
    });
}
function CallHome() {
    var url = document.domain;
    $.post("http://www.mvcforum.com/base/MVCBase/DomainCheck", { "url": url }, function (data) {
        //TODO: Add stuff here if needed
    });
}