$(function () {
    $.ajax({
        url: '/babywise/themes/template/footer',
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html'
    })
    .success(function (result) {
        $("#footer_container").html(result);
    });
});

