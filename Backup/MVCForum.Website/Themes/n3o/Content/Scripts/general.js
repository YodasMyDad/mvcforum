$(function () {
    SortSideScroller();
});

$(window).resize(function () {
    SortSideScroller();
});

function SortSideScroller() {
    var $linksbar = $('.main-side-box');
    var $top = ($linksbar.offset().top - 50);
    var $left = $linksbar.offset().left;

    $(window).scroll(function () {
        if ($(window).scrollTop() > $top) {
            $linksbar.addClass('floater');
            $linksbar.css("left", $left);
        }
        else {
            $linksbar.removeClass('floater');
        }
    });   
}