$(function () {
    HelperDropDowns();
    HelperCascadingDropDowns();
    InitDialogs();
    InitButtons();
    DeleteItem();
    HelperCascadingDropDownsWithTwoParam();
});

$.ajaxSetup({
    async: false
});

function ShowInterfaceFieldLable(interfaceData) {
    $.each(interfaceData, function (i, item) {
        $('#' + item.FieldCode).html(item.FieldName);
    });
}

function ShowInterfaceLable(interfaceData, url, action) {
    if (interfaceData !== null) {
        var html = '<ul class="breadcrumb"><li><i class="ace-icon fa fa-home home-icon"></i>' + interfaceData['Frame'] + '</li>';
        if (interfaceData['Group'] != null) {
            html += '<li class="#">' + interfaceData['Group'] + '</li>';
        }
        if (interfaceData['SubGroup'] !== null) {
            html += '<li class="#">' + interfaceData['SubGroup'] + '</li>';
        }
        if (url === undefined) {
            html += '<li>' + interfaceData['Item'] + '</li>';
        }
        else {
            html += '<li><a href="' + url + '">' + interfaceData['Item'] + '</a></li>';
        }
        if (action !== null) {
            html += '<li class="active">' + action + '</li>';
        }
        html += '</ul>';
        $("#breadcrumbs").html(html);
        action = action === undefined ? "" : action;
        $("#ifName").html(interfaceData['Item'] + " " + action);
    }
}

function ParseDate(input) {
    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    var parts = input.split('-');
    return new Date(parts[2], months.indexOf(parts[1]), parts[0]);
}

function HelperDropDowns() {
    var dropdownElements = $('select.Dropdown:not(.DropdownInited)');
    $.each(dropdownElements, function (index, element) {
        var dropdownEl = $(element);
        var url = dropdownEl.attr('data-url');
        var selected = dropdownEl.attr('data-selected');
        var dataCache = dropdownEl.attr('data-cache') ? true : false;
        $.ajax({
            url: url,
            type: 'GET',
            cache: dataCache,
            success: function (jsonData, textStatus, XMLHttpRequest) {
                var Listitems = '<option value="">--select--</option>';
                $.each(jsonData, function (i, item) {
                    if (selected && selected === item.Value) {
                        Listitems += "<option selected='selected' value='" + item.Value + "'>" + item.Text + "</option>";
                    }
                    else {
                        Listitems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
                    }
                });
                dropdownEl.html(Listitems).addClass("DropdownInited");
            }
        });
    });
}

function HelperCascadingDropDowns() {
    var dependentElements = $('select.Cascading:not(.DropdownInited)');
    $.each(dependentElements, function (index, element) {
        var dependentEl = $(element);
        var parentEl = $('#' + dependentEl.attr('data-parent'));
        var url = dependentEl.attr('data-url');
        var selected = dependentEl.attr('data-selected');
        var dataCache = dependentEl.attr('data-cache') ? true : false;
        var loadDropDownItems = function () {
            if (!parentEl.val()) {
                if (selected) {
                    setTimeout(loadDropDownItems, 300);
                }
                return;
            }
            $.ajax({
                url: url + parentEl.val(),
                type: 'GET',
                cache: dataCache,
                success: function (jsonData, textStatus, XMLHttpRequest) {
                    var Listitems = '<option value="">--select--</option>';
                    $.each(jsonData, function (i, item) {
                        if (selected && selected == item.Value) {
                            Listitems += "<option selected='selected' value='" + item.Value + "'>" + item.Text + "</option>";
                        }
                        else {
                            Listitems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
                        }
                    });
                    dependentEl.html(Listitems).addClass("DropdownInited");
                }
            });
        };
        parentEl.change(loadDropDownItems);
        if (selected) {
            loadDropDownItems();
        }
    });
}

function HelperCascadingDropDownsWithTwoParam() {
    var dependentElements = $('select.CascadingTwoParam:not(.DropdownInited)');
    $.each(dependentElements, function (index, element) {
        var dependentEl = $(element);
        var param1 = $('#' + dependentEl.attr('data-param1'));
        var param2 = $('#' + dependentEl.attr('data-param2'));
        var url = dependentEl.attr('data-url');
        var selected = dependentEl.attr('data-selected');
        var dataCache = dependentEl.attr('data-cache') ? true : false;
        var loadDropDownItems = function () {
            if (!param1.val()) {
                if (selected) {
                    setTimeout(loadDropDownItems, 300);
                }
                return;
            }
            //alert(param2.html());
            $.ajax({
                url: url + "?param1=" + param1.val() + "&&param2=" + param2.val(),
                type: 'GET',
                cache: dataCache,
                success: function (jsonData, textStatus, XMLHttpRequest) {
                    var Listitems = '<option></option>';
                    $.each(jsonData, function (i, item) {
                        if (selected && selected == item.Value) {
                            Listitems += "<option selected='selected' value='" + item.Value + "'>" + item.Text + "</option>";
                        }
                        else {
                            Listitems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
                        }
                    });
                    dependentEl.html(Listitems).addClass("DropdownInited");
                }
            });
        };
        param1.change(loadDropDownItems);
        param2.change(loadDropDownItems);
        if (selected) {
            loadDropDownItems();
        }
    });
}

function InitDialogs() {
    $('a.Dialog:not(.DialogInited)').on("click", function () {
        var url = $(this).attr('href');
        $.ajax({
            url: url,
            type: 'GET',
            cache: false,
            success: function (responseText, textStatus, XMLHttpRequest) {
                var html = '' +
                    '<div class="modal fade" id="addNewForm" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">' +
                    '<div class="modal-dialog">' +
                    '<div class="modal-content">' +
                    '<div class="modal-body">' +
                    responseText +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>';
                var dialogWindow = $(html).appendTo('body');
                dialogWindow.modal({ backdrop: 'static' });
            }
        });
        return false;
    }).addClass("DialogInited");
}

function ShowDialogs(url) {
    $.ajax({
        url: url,
        type: 'GET',
        cache: false,
        success: function (responseText, textStatus, XMLHttpRequest) {
            var html = '' +
                '<div class="modal fade" id="addNewForm" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">' +
                '<div class="modal-dialog">' +
                '<div class="modal-content">' +
                '<div class="modal-body">' +
                responseText +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
            var dialogWindow = $(html).appendTo('body');
            dialogWindow.modal({ backdrop: 'static' });
        }
    });
}

function CloseModal(el, dataAction, dataUrl) {
    if (dataAction == "formsubmit") {
        $("#" + dataUrl).submit();
    }
    else if (dataAction == "refreshparent") {
        window.parent.location = window.parent.location;
    }
    else if (dataAction == "refreshself") {
        window.location = window.location;
    }
    else if (dataAction == "redirect") {
        window.location = dataUrl;
    }
    else if (dataAction == "function") {
        $eval(dataUrl);
    }
    var win = $(el).closest(".modal");
    win.modal("hide");
    setTimeout(function () {
        win.next(".modal-backdrop").remove();
        win.remove();
    }, 500);
}

function ShowResult(msg, status, position, dataUrl) {
    var html = '';
    if (status == "success") { html += '<div class="alert alert-success fade alert-site alert-site-ok">'; }
    else html += '<div class="alert alert-danger fade alert-site" data-dismiss="alert">';
    html += '' + msg + '</div>';
    $("div.alert").remove();
    if (undefined !== position) {
        var id = $("#" + position);
        $("#" + position + " .modal-header").after(html);
        $('.alert-site').css({ 'width': id.width, 'left': $('.sidebar').width() });
    }
    else {
        $("#main").after(html);
        $('.alert-site').css({ 'width': $('.navbar-site').css('width'), 'left': $('.navbar-site').css('margin-left') });
    }
    // hide alert message
    setTimeout(function () { $(".alert-site-ok").fadeOut(); }, 5000);
}

function ShowAsk(msg, dataAction, dataUrl) {
    var html = '' +
        '<div id="userConfirm" class="modal fade">' +
        '<div class="modal-dialog">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<h4 class="modal-title">APLOS DSS</h4>' +
        '</div>' +
        '<div class="modal-body">' +
        msg +
        '</div>' +
        '<div class="modal-footer">' +
        '<button type="button" class="btn btn-primary" onclick="CloseModal(this, \'' + dataAction + '\',\'' + dataUrl + '\')">YES</button>' +
        //'<button type="button" class="btn btn-primary" ng-click="$root.yes()">YES</button>' +
        '<button type="button" class="btn btn-primary btn-close" onclick="CloseModal(this)">NO</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>';
    var dialogWindow = $(html).appendTo('body');
    dialogWindow.modal({ backdrop: 'static' });
}

function DeleteItem() {
    $(".btnRemove").on('click', function () {
        var url = $(this).attr("data-url");
        ShowAsk('Are you sure to delete?', 'function', url);
    });
}

function InitButtons() {
    $('.AddRow:not(.AddRowInited)').on("click", function () {
        var url = $(this).attr('data-url');
        var container = $(this).attr('data-container');
        $.ajax({
            url: url,
            type: 'POST',
            cache: false,
            success: function (html) {
                $("#" + container).append(html);
            }
        });
        return false;
    }).addClass("AddRowInited");

    $('.RemoveRow:not(.RemoveRowInited)').on("click", function () {
        $(this).parents("div.row:first").remove();
        return false;
    }).addClass("RemoveRowInited");
}

function DeleteRow(lnk) {
    var row = lnk.parentNode.parentNode;
    var rowIndex = row.rowIndex - 1;
    var myTableee = document.getElementById('tbl').tBodies[0];
    var tabRowCount = myTableee.Rows.length;
    if (tabRowCount > 1) {
        var myTable = document.getElementById('tbl');
        myTable.deleteRow(rowIndex + 1);
    }
    else {
        ShowResult("At least one row needed!", "failure");
    }
}