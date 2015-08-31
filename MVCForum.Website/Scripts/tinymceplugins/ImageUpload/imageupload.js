tinymce.PluginManager.add("forumimageinsert", function (editor, url) {
    editor.addButton("forumimageinsert", {
        icon: "image",
        tooltip: "Insert image",
        onclick: function () {
            editor.windowManager.open({
                title: "Insert image",
                url: app_base + "scripts/tinymceplugins/ImageUpload/imageupload.html",
                width: 500,
                height: 170,
                buttons: [
                    {
                        text: "Ok",
                        classes: 'widget btn btn-default first abs-layout-item',
                        onclick: function () {
                            var b = editor.windowManager.getWindows()[0];
                            var uploadFile = b.getContentWindow().document.getElementById('content');
                            var imageDesc = b.getContentWindow().document.getElementById('desc');
                            var externalUrl = b.getContentWindow().document.getElementById('external');
                            var externalRow = b.getContentWindow().document.getElementById('externalrow');
                            var uploadrow = b.getContentWindow().document.getElementById('uploadrow');
                            var waitNotice = b.getContentWindow().document.getElementById('waiting');

                            if (externalUrl.value != '') {
                                // We have an external url so use that
                                // Check for http
                                if (!externalUrl.value.startsWith('http') ) {
                                    alert('Please enter a valid url');
                                }

                                var imageAlt = imageDesc.value;
                                var imageSrc = externalUrl.value;
                                var imageTag = '<img src="' + imageSrc + '?width=690" alt="' + imageAlt + '" />';
                                editor.insertContent(imageTag), b.close();

                            } else {

                                // No external url
                                if (uploadFile.value == '') {
                                    alert('Please select a file');
                                    return false;
                                }

                                if (uploadFile.files[0].size > 2000 * 1024) {
                                    alert('Max image file size is 2MB');
                                    return false;
                                }

                                if (uploadFile.files[0].type != "image/jpeg" && uploadFile.files[0].type != "image/jpg" &&
                                    uploadFile.files[0].type != "image/png" && uploadFile.files[0].type != "image/gif") {
                                    alert('Only image file format can be uploaded');
                                    return false;
                                }

                                // Show wait notice
                                waitNotice.style.display = 'block';
                                externalRow.style.display = 'none';

                                var data;

                                data = new FormData();
                                data.append('file', uploadFile.files[0]);
                                data.append('topicId', topicId);

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
                                        var imageTag = '<img src="' + imageSrc + '?width=650&mode=max" alt="' + imageAlt + '" />';
                                        editor.insertContent(imageTag), b.close();
                                    }
                                    alert('Error uploading file');
                                    waitNotice.style.display = 'none';
                                    externalRow.style.display = 'block';

                                }).fail(function (jqXHR, textStatus) {
                                    alert("Request failed: " + jqXHR.responseText + " --- " + textStatus);
                                    waitNotice.style.display = 'none';
                                    externalRow.style.display = 'block';
                                });
                            }
                        }
                    }, {
                        text: "Close",
                        onclick: "close"
                    }
                ]
            })
        }
    });
});