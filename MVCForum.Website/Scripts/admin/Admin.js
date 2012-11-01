$(function () {

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
            url: '/Admin/Account/UpdateUserRoles',
            type: 'POST',
            dataType: 'json',
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
            url: '/Admin/Permissions/UpdatePermission',
            type: 'POST',
            dataType: 'json',
            data: strung,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                ResetTableAfterAjaxCall();
                ShowSuccessNotification();
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

        var inputfield = tableInfo.Row.find(".resourcevalueedit input");
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
                url: '/Admin/AdminLanguage/UpdateResourceValue',
                type: 'POST',
                dataType: 'json',
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
                url: '/Admin/AdminLanguage/UpdateResourceKey',
                type: 'POST',
                dataType: 'json',
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


    // TAGS
    
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
                url: '/Admin/AdminTag/UpdateTag',
                type: 'POST',
                dataType: 'json',
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

});

function HighlightUpdated(clickedElement) {
    $(clickedElement).effect("highlight", {}, 3000);
}
function ShowUserMessage(message) {
    if (message != null) {
        var jsMessage = $('#jsquickmessage');
        var toInject = "<div class=\"alert alert-block alert-info fade in\"><a href=\"#\" data-dismiss=\"alert\" class=\"close\">&times;<\/a>" + message + "<\/div>";
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
