$(function () {

    AddModerateClickEvents();
    AddModerateAjaxCalls();
    ResponsiveTable();

    var showNavButton = $(".btn-show-admin-nav");
    var mainNavHolder = $(".admin-options");
    showNavButton.click(function (e) {
        e.preventDefault();
        mainNavHolder.slideToggle('fast',
            function () {
                if (mainNavHolder.is(":hidden")) {
                    showNavButton.text("Show Main Navigation");
                } else {
                    showNavButton.text("Hide Main Navigation");
                }
            }
        );
    });

    // Someone has clicked one of the roles checkboxes
    // On the list page so update
    var listrolecbholder = 'span.listrolecbholder';
    $(listrolecbholder).click(function () {
        var checkedRoles = [];
        $(this).find('input[type=radio]:checked').each(function () {
            checkedRoles.push($(this).val());
        });

        var userId = $(this).find('#userId').val();

        // Make a view model instance
        var ajaxRoleUpdateViewModel = new Object();
        ajaxRoleUpdateViewModel.Id = userId;
        ajaxRoleUpdateViewModel.Roles = checkedRoles;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(ajaxRoleUpdateViewModel);

        $.ajax({
            url: app_base + 'Admin/Account/UpdateUserRoles',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                ShowUserMessage("Roles updated");
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
            }
        });
    });

    // Permissions table
    $('span.permissioncheckbox').click(function () {

        $('table.permissiontable input[type=checkbox]').attr('disabled', true);
        $('span.ajaxsuccessshow').hide();
        $('img.editpermissionsspinner').show();
        var checkBox = $(this).find('input[type=checkbox]');

        var isChecked = checkBox.is(':checked');
        var permission = checkBox.data('permisssion');
        var category = checkBox.data('category');
        var role = checkBox.data('role');

        // Ajax call here
        // Make a view model instance
        var ajaxEditPermissionViewModel = new Object();
        ajaxEditPermissionViewModel.HasPermission = isChecked;
        ajaxEditPermissionViewModel.Permission = permission;
        ajaxEditPermissionViewModel.Category = category;
        ajaxEditPermissionViewModel.MembershipRole = role;

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(ajaxEditPermissionViewModel);

        $.ajax({
            url: app_base + 'Admin/Permissions/UpdatePermission',
            type: 'POST',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                ResetTableAfterAjaxCall();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                ResetTableAfterAjaxCall();
            }
        });
    });

    // Find the encompassing element
    function getParentElement(element, elementType) {
        var target = element.parent();
        var found = false;

        while (target != undefined && !found) {
            var tagName = target.get(0).tagName;

            if (tagName == undefined) {
                return undefined;
            }

            if (tagName.toLowerCase() != elementType) {
                target = target.parent();
            } else {
                found = true;
            }
        }

        return target;
    }

    function tableContext(element) {

        this.Cell = getParentElement(element, "td");
        this.Row = getParentElement(element, "tr");
    }

    // Handler for click on the resource edit button
    $('span.editresource').click(function (e) {
        e.preventDefault();
        var tableInfo = new tableContext($(this));

        tableInfo.Cell.find(".saveresource").show();
        tableInfo.Row.find(".resourcevalueedit").show();
        tableInfo.Row.find(".resourcevaluedisplay").hide();
        tableInfo.Cell.find(".editresource").hide();
    });

    // Handler for click on the resource save button
    $('span.saveresource').click(function (e) {
        e.preventDefault();
        var tableInfo = new tableContext($(this));

        var inputfield = tableInfo.Row.find(".resourcevalueedit textarea");
        var displayfield = tableInfo.Row.find(".resourcevaluedisplay");

        // Ajax call setup
        var languageid = inputfield.data('languageid');
        var resourcekey = inputfield.data('resourcekey');
        var oldvalue = inputfield.data('oldvalue');
        var newvalue = inputfield.val();

        // Don't allow a null/empty string value, and don't update if nothing changed
        if ((newvalue != null && newvalue != "") && (newvalue != oldvalue)) {
            // Make a view model instance
            var ajaxEditLanguageValueViewModel = new Object();
            ajaxEditLanguageValueViewModel.LanguageId = languageid;
            ajaxEditLanguageValueViewModel.ResourceKey = resourcekey;
            ajaxEditLanguageValueViewModel.NewValue = newvalue;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(ajaxEditLanguageValueViewModel);

            $.ajax({
                url: app_base + 'Admin/AdminLanguage/UpdateResourceValue',
                type: 'POST',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

            displayfield.text(newvalue);
        }

        tableInfo.Cell.find(".editresource").show();
        tableInfo.Row.find(".resourcevaluedisplay").show();
        tableInfo.Cell.find(".saveresource").hide();
        tableInfo.Row.find(".resourcevalueedit").hide();
    });


    // Handler for click on the resource key edit button
    $('span.editresourcekey').click(function (e) {
        e.preventDefault();
        var tableInfo = new tableContext($(this));

        tableInfo.Cell.find(".saveresourcekey").show();
        tableInfo.Row.find(".resourcekeyvalueedit").show();
        tableInfo.Row.find(".resourcekeyvaluedisplay").hide();
        tableInfo.Cell.find(".editresourcekey").hide();
    });
    
    // Handler for click on the resource key save button
    $('span.saveresourcekey').click(function (e) {
        e.preventDefault();
        var tableInfo = new tableContext($(this));

        var inputfield = tableInfo.Row.find(".resourcekeyvalueedit input");
        var displayfield = tableInfo.Row.find(".resourcekeyvaluedisplay");

        // Ajax call setup
        var resourcekeyid = inputfield.data('resourcekeyid');
        var oldvalue = inputfield.data('oldvalue');
        var newvalue = inputfield.val();

        // Don't allow a null/empty string value, and don't update if nothing changed
        if ((newvalue != null && newvalue != "") && (newvalue != oldvalue)) {
            // Make a view model instance
            var ajaxEditLanguageKeyViewModel = new Object();
            ajaxEditLanguageKeyViewModel.ResourceKeyId = resourcekeyid;
            ajaxEditLanguageKeyViewModel.NewName = newvalue;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(ajaxEditLanguageKeyViewModel);

            $.ajax({
                url: app_base + 'Admin/AdminLanguage/UpdateResourceKey',
                type: 'POST',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

            displayfield.text(newvalue);
        }

        tableInfo.Cell.find(".editresourcekey").show();
        tableInfo.Row.find(".resourcekeyvaluedisplay").show();
        tableInfo.Cell.find(".saveresourcekey").hide();
        tableInfo.Row.find(".resourcekeyvalueedit").hide();
    });
    
    $('span.editbannedword').click(function (e) {
        e.preventDefault();
        var tableInfo = new tableContext($(this));

        tableInfo.Cell.find(".savebannedword").show();
        tableInfo.Row.find(".bannedwordvalueedit").show();
        tableInfo.Row.find(".bannedwordvaluedisplay").hide();
        tableInfo.Cell.find(".editbannedword").hide();
    });

    // Handler for click on the resource key save button
    $('span.savebannedword').click(function (e) {
        e.preventDefault();
        var tableInfo = new tableContext($(this));

        var inputfield = tableInfo.Row.find(".bannedwordvalueedit input");
        var displayfield = tableInfo.Row.find(".bannedwordvaluedisplay");

        // Ajax call setup
        var wordid = inputfield.data('wordid');
        var oldvalue = inputfield.data('oldvalue');
        var newvalue = inputfield.val();

        // Don't allow a null/empty string value, and don't update if nothing changed
        if ((newvalue != null && newvalue != "") && (newvalue != oldvalue)) {
            // Make a view model instance
            var AjaxEditWordViewModel = new Object();
            AjaxEditWordViewModel.WordId = wordid;
            AjaxEditWordViewModel.NewName = newvalue;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(AjaxEditWordViewModel);

            $.ajax({
                url: app_base + 'Admin/BannedWord/AjaxUpdateWord',
                type: 'POST',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

            displayfield.text(newvalue);
        }

        tableInfo.Cell.find(".editbannedword").show();
        tableInfo.Row.find(".bannedwordvaluedisplay").show();
        tableInfo.Cell.find(".savebannedword").hide();
        tableInfo.Row.find(".bannedwordvalueedit").hide();
    });
    
    $('span.editbannedemail').click(function (e) {
        e.preventDefault();
        var tableInfo = new tableContext($(this));

        tableInfo.Cell.find(".savebannedemail").show();
        tableInfo.Row.find(".bannedemailvalueedit").show();
        tableInfo.Row.find(".bannedemailvaluedisplay").hide();
        tableInfo.Cell.find(".editbannedemail").hide();
    });

    // Handler for click on the resource key save button
    $('span.savebannedemail').click(function (e) {
        e.preventDefault();
        var tableInfo = new tableContext($(this));

        var inputfield = tableInfo.Row.find(".bannedemailvalueedit input");
        var displayfield = tableInfo.Row.find(".bannedemailvaluedisplay");

        // Ajax call setup
        var emailid = inputfield.data('emailid');
        var oldvalue = inputfield.data('oldvalue');
        var newvalue = inputfield.val();

        // Don't allow a null/empty string value, and don't update if nothing changed
        if ((newvalue != null && newvalue != "") && (newvalue != oldvalue)) {
            // Make a view model instance
            var AjaxEditEmailViewModel = new Object();
            AjaxEditEmailViewModel.EmailId = emailid;
            AjaxEditEmailViewModel.NewName = newvalue;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(AjaxEditEmailViewModel);

            $.ajax({
                url: app_base + 'Admin/BannedEmail/AjaxUpdateEmail',
                type: 'POST',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

            displayfield.text(newvalue);
        }

        tableInfo.Cell.find(".editbannedemail").show();
        tableInfo.Row.find(".bannedemailvaluedisplay").show();
        tableInfo.Cell.find(".savebannedemail").hide();
        tableInfo.Row.find(".bannedemailvalueedit").hide();
    });


    // Handler for click on the tag edit button
    $('span.edittag').click(function (e) {
        e.preventDefault();
        var tableInfo = new tableContext($(this));
        tableInfo.Row.find(".savetag").show();
        tableInfo.Row.find(".tagvalueedit").show();
        tableInfo.Row.find(".edittag").hide();
        tableInfo.Row.find(".tagvaluedisplay").hide();
    });

    // Handler for click on the resource key save button
    $('span.savetag').click(function (e) {
        
        e.preventDefault();
        var tableInfo = new tableContext($(this));

        var inputfield = tableInfo.Row.find(".tagvalueedit input");
        var displayfield = tableInfo.Row.find(".tagvaluedisplay");

        // Ajax call setup
        var oldvalue = inputfield.data('oldvalue');
        var newvalue = inputfield.val();

        // Don't allow a null/empty string value, and don't update if nothing changed
        if ((newvalue != null && newvalue != "") && (newvalue != oldvalue)) {
            
            // Make a view model instance
            var ajaxEditTagViewModel = new Object();
            ajaxEditTagViewModel.OldName = oldvalue;
            ajaxEditTagViewModel.NewName = newvalue;

            // Ajax call to post the view model to the controller
            var strung = JSON.stringify(ajaxEditTagViewModel);

            $.ajax({
                url: app_base + 'Admin/AdminTag/UpdateTag',
                type: 'POST',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

            displayfield.text(newvalue);
        }

        tableInfo.Row.find(".edittag").show();
        tableInfo.Row.find(".tagvaluedisplay").show();
        tableInfo.Row.find(".tagvalueedit").hide();
        tableInfo.Row.find(".savetag").hide();
    });

    $('.btn-file :file').on('fileselect', function (event, numFiles, label) {

        var input = $(this).parents('.input-group').find(':text'),
            log = numFiles > 1 ? numFiles + ' files selected' : label;

        if (input.length) {
            input.val(log);
        } else {
            if (log) alert(log);
        }

    });

});

$(document).on('change', '.btn-file :file', function () {
    var input = $(this),
        numFiles = input.get(0).files ? input.get(0).files.length : 1,
        label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
    input.trigger('fileselect', [numFiles, label]);
});

function AddModerateAjaxCalls() {
    // Moderate section

    $(".showmoretopics").click(function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();

        var pageIndex = $('#topicPageIndex');
        var totalPages = parseInt($('#topicTotalPages').val());
        var activeText = $(this).find('span.smpactive');
        var loadingText = $(this).find('span.smploading');
        var showMoreLink = $(this);

        activeText.hide();
        loadingText.show();

        var AjaxPagingViewModel = new Object();
        AjaxPagingViewModel.PageIndex = pageIndex.val();

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(AjaxPagingViewModel);

        $.ajax({
            url: app_base + 'Admin/Moderate/GetMoreTopics',
            type: 'POST',
            dataType: 'html',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                // Add new data to page
                showMoreLink.before(data);

                // Update the page index value
                var newPageIdex = (parseInt(pageIndex.val()) + parseInt(1));
                pageIndex.val(newPageIdex);

                // If the new pageindex is greater than the total pages, then hide the show more button
                if (newPageIdex > totalPages) {
                    showMoreLink.hide();
                }

                // Lastly reattch the click events
                AddModerateClickEvents();
                activeText.show();
                loadingText.hide();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                activeText.show();
                loadingText.hide();
            }
        });
    });


    $(".showmoreposts").click(function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();

        var pageIndex = $('#postsPageIndex');
        var totalPages = parseInt($('#postsTotalPages').val());
        var activeText = $(this).find('span.smpactive');
        var loadingText = $(this).find('span.smploading');
        var showMoreLink = $(this);

        activeText.hide();
        loadingText.show();

        var AjaxPagingViewModel = new Object();
        AjaxPagingViewModel.PageIndex = pageIndex.val();

        // Ajax call to post the view model to the controller
        var strung = JSON.stringify(AjaxPagingViewModel);

        $.ajax({
            url: app_base + 'Admin/Moderate/GetMorePosts',
            type: 'POST',
            dataType: 'html',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                // Add new data to page
                showMoreLink.before(data);

                // Update the page index value
                var newPageIdex = (parseInt(pageIndex.val()) + parseInt(1));
                pageIndex.val(newPageIdex);

                // If the new pageindex is greater than the total pages, then hide the show more button
                if (newPageIdex > totalPages) {
                    showMoreLink.hide();
                }

                // Lastly reattch the click events
                AddModerateClickEvents();
                activeText.show();
                loadingText.hide();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                activeText.show();
                loadingText.hide();
            }
        });
    });

}

function AddModerateClickEvents() {

    var approvetopic = $('.topicaction');
    if (approvetopic.length > 0) {
        approvetopic.click(function (e) {
            e.preventDefault();
            var id = $(this).data("topicid");
            var action = $(this).data("topicaction");
            var snippetHolder = $('#topic-' + id);
            var approve = true;
            if (action == "delete") {
                if (!confirm('Are you sure you want to delete this?')) {
                    return false;
                }
                approve = false;
            }

            var moderateActionViewModel = new Object();
            moderateActionViewModel.IsApproved = approve;
            moderateActionViewModel.TopicId = id;
            var strung = JSON.stringify(moderateActionViewModel);

            $.ajax({
                url: app_base + 'Admin/Moderate/ModerateTopic',
                type: 'POST',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    if (data != "error") {
                        snippetHolder.fadeOut('fast');
                    } else {
                        ShowUserMessage("There was an error, check the error log");
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }

    var approvepost = $('.postaction');
    if (approvepost.length > 0) {
        approvepost.click(function (e) {
            e.preventDefault();
            var id = $(this).data("postid");
            var action = $(this).data("postaction");
            var snippetHolder = $('#post-' + id);
            var approve = true;
            if (action == "delete") {
                if (!confirm('Are you sure you want to delete this?')) {
                    return false;
                }
                approve = false;
            }

            var moderateActionViewModel = new Object();
            moderateActionViewModel.IsApproved = approve;
            moderateActionViewModel.PostId = id;
            var strung = JSON.stringify(moderateActionViewModel);

            $.ajax({
                url: app_base + 'Admin/Moderate/ModeratePost',
                type: 'POST',
                data: strung,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    if (data != "error") {
                        snippetHolder.fadeOut('fast');
                    } else {
                        ShowUserMessage("There was an error, check the error log");
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    ShowUserMessage("Error: " + xhr.status + " " + thrownError);
                }
            });

        });
    }

}

function HighlightUpdated(clickedElement) {
    $(clickedElement).effect("highlight", {}, 3000);
}
function ShowUserMessage(message) {
    if (message != null) {
        var jsMessage = $('#jsquickmessage');
        var toInject = "<div class=\"alert alert-info fade in\" role=\"alert\"><button type=\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;<\/span><\/button>" + message + "<\/div>";
        jsMessage.html(toInject);
        jsMessage.show();
        $('div.alert').delay(2200).fadeOut();
    }
}
function ResetTableAfterAjaxCall() {
    $('img.editpermissionsspinner').hide();
    $('table.permissiontable input[type=checkbox]').removeAttr('disabled');
}
function ShowSuccessNotification() {
    $('span.ajaxsuccessshow').fadeIn().delay('800').fadeOut();
}

var isFirstLoad = true;

function ImportLanguage() {
    $("#ImportForm").submit();
}

function Import_Complete() {
    // Check to see if this is the first load of the iFrame
    if (isFirstLoad == true) {
        isFirstLoad = false;
        return;
    }

    // Reset the import form so the file won't get uploaded again
    document.getElementById("ImportForm").reset();

    // Grab the content of the textarea we named jsonResult in the hidden iframe
    var importResults = $.parseJSON($("#UploadTarget").contents().find("#jsonResult")[0].innerHTML);
    var displayImportResults = "";

    // Redirect as there are no errors
    if (!importResults.HasErrors && !importResults.HasWarnings) {
        displayImportResults += "<div>The import was successful.</div>";
    }

    if (importResults.HasErrors) {
        displayImportResults += "<div>The import had the following errors:</div>";
        displayImportResults += formatImportResults(importResults.Errors);
    }

    if (importResults.HasWarnings) {
        displayImportResults += "<div>The import had the following warnings:</div>";
        displayImportResults += formatImportResults(importResults.Warnings);
    }

    $("#ImportResults").html(displayImportResults);
}

function formatImportResults(jsonErrorsWarnings) {
    var results = "";
    var errorsWarnings = jQuery.parseJSON(jsonErrorsWarnings);
    for (var i = 0; i < errorsWarnings.length; i++) {
        results += "<div>" + errorsWarnings[i] + "</div>";
    }
    return results;
}

function ResponsiveTable() {
    var adaptiveTable = $('.table-adaptive');
    if (adaptiveTable.length > 0) {
        var headertext = [],
        headers = document.querySelectorAll(".table-adaptive th"),
        tablerows = document.querySelectorAll(".table-adaptive th"),
        tablebody = document.querySelector(".table-adaptive tbody");

        for (var i = 0; i < headers.length; i++) {
            var current = headers[i];
            headertext.push(current.textContent.replace(/\r?\n|\r/, ""));
        }
        if (tablebody.rows != null) {
            for (var i = 0, row; row = tablebody.rows[i]; i++) {
                for (var j = 0, col; col = row.cells[j]; j++) {
                    col.setAttribute("data-th", headertext[j]);
                }
            }
        }
    }
}