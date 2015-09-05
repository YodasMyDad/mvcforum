tinymce.PluginManager.add("forumimageinsert", function (editor, url) {
    editor.addButton("forumimageinsert", {
        icon: "image",
        tooltip: buttonTitle,
        onclick: function () {
            editor.windowManager.open({
                title: buttonTitle,
                url: app_base + "file/imageuploadtinymce/",
                width: 500,
                height: 170,
                buttons: [
                    {
                        text: buttonOk,
                        classes: 'widget btn btn-default first abs-layout-item',
                        onclick: function () {
                            var b = editor.windowManager.getWindows()[0];
                            var uploadFile = b.getContentWindow().document.getElementById('content');
                            var imageDesc = b.getContentWindow().document.getElementById('desc');
                            var externalUrl = b.getContentWindow().document.getElementById('external');
                            var externalRow = b.getContentWindow().document.getElementById('externalrow');
                            var uploadrow = b.getContentWindow().document.getElementById('uploadrow');
                            var waitNotice = b.getContentWindow().document.getElementById('waiting');;
                            if (externalUrl.value != '') {
                                // We have an external url so use that
                                // Check for http
                                if (!externalUrl.value.startsWith('http') ) {
                                    alert(enterValidUrl);
                                    return false;
                                } else {
                                    var imageAlt = imageDesc.value;
                                    var imageSrc = externalUrl.value;
                                    var imageTag = '<img src="' + imageSrc + '?width=690" alt="' + imageAlt + '" />';
                                    editor.insertContent(imageTag), b.close();
                                }
                            } else {

                                // No external url
                                if (uploadFile.value == '') {
                                    alert(selectFile);
                                    return false;
                                }

                                if (uploadFile.files[0].size > 2000 * 1024) {
                                    alert(maxImageFileSize);
                                    return false;
                                }

                                if (uploadFile.files[0].type != "image/jpeg" && uploadFile.files[0].type != "image/jpg" &&
                                    uploadFile.files[0].type != "image/png" && uploadFile.files[0].type != "image/gif") {
                                    alert(onlyImages);
                                    return false;
                                }

                                // Show wait notice
                                waitNotice.style.display = 'block';

                                var data;

                                data = new FormData();
                                data.append('file', uploadFile.files[0]);

                                $.ajax({
                                    url: app_base + 'api/TinyMce/UploadImage',
                                    data: data,
                                    processData: false,
                                    contentType: false,
                                    async: true,
                                    type: 'POST',
                                }).done(function (msg) {
                                    if (msg != '') {
                                        var imageAlt = imageDesc.value;
                                        var imageSrc = msg;
                                        var imageTag = '<img src="' + imageSrc + '?width=690&upscale=false" alt="' + imageAlt + '" />';
                                        editor.insertContent(imageTag), b.close();
                                    } else {
                                        alert(generalError);
                                        waitNotice.style.display = 'none';
                                        externalRow.style.display = 'block';
                                    }

                                }).fail(function (jqXHR, textStatus) {
                                    alert("Request failed: " + jqXHR.responseText + " --- " + textStatus);
                                    waitNotice.style.display = 'none';
                                    externalRow.style.display = 'block';
                                });
                            }
                        }
                    }, {
                        text: buttonClose,
                        onclick: "close"
                    }
                ]
            })
        }
    });
});