
$(function () {
    // Any images uploaded, fire fancybox
    $('div.fileupload a[href$=".gif"], div.fileupload a[href$=".jpg"], div.fileupload a[href$=".png"], div.fileupload a[href$=".bmp"], div.fileupload a[href$=".jpeg"]').fancybox();
});
addthis.layers({
    'theme': 'transparent',
    'share': {
        'position': 'left',
        'numPreferredServices': 6,
        'services': 'facebook,twitter,google_plusone_share,linkedin,stumbleupon,pinterest,more'
    }
});