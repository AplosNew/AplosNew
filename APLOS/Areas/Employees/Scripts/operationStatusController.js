"use strict";
operationStatusController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function operationStatusController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = "Operation Status ";
    $scope.Action = "Save";
    $scope.ActionControl = "Save";
    $controller('dashboardBaseController', { $scope: $scope, $http: $http });

    $scope.activeTab = 1;

    $scope.setTab = function (tab) {
        $scope.activeTab = tab;
    };

    $scope.isSet = function (tab) {
        return $scope.activeTab === tab;
    };


    $scope.index = -1;
    $scope.operationStatusSettings = [];
    $scope.path = "Employees/EmployeeInFoReport/";
    $scope.saveUrl = $scope.path + "CreateOperationStatusSetting";
    $scope.updateUrl = $scope.path + "CreateOperationStatusSetting";
    $scope.deleteUrl = $scope.path + "DeleteOperationStatusSetting/";
    $scope.deleteColumnControlUrl = $scope.path + "DeleteOperationStatusColumnControl/";
    $scope.getListUrl = $scope.path + "getdatalist";
    $scope.getColumnControlListUrl = $scope.path + "GetColumnControlDataList";
    $scope.getMasterListUrl = $scope.path + "GetOSMasterDataList";
    $scope.getOS2ListUrl = $scope.path + "GetOS2DataList";
    $scope.getOS2SecondListUrl = $scope.path + "GetOS2SecondDataList";
    $scope.getProductionListUrl = $scope.path + "GetProductionDataList";
    $scope.getWorkCenterListUrl = $scope.path + "GetWorkCenterDataList";
    $scope.saveColumnControlUrl = $scope.path + "CreateOperationStatusColumnControl";



    $scope.searchMasterByList = [
        { value: "SalesOrderId", name: "SO" },
        { value: "Material", name: "Material" },
        { value: "Article", name: "Article" },
        { value: "ProductCategory", name: "ProductCategory" },
        { value: "Product", name: "Product" },
    ];
    $rootScope.parameters = { searchMasterBy: "SalesOrderId", searchMaster: "" };

    $scope.masterDataList = [];
    $scope.master = { userName: "", users: [] };

    // ড্রপডাউনের ইউজার লিস্ট
    $scope.loadMasterUsers = function (data) {
        $http.get('Employees/EmployeeInFoReport/GetDataDisplayUserNames?tabName='+data).then(function (res) {
            $scope.master.users = res.data || [];
            if ($scope.master.users.length && !$scope.master.userName) {
                $scope.master.userName = $scope.master.users[0];
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };
    $scope.loadMasterUsers('Master');

    $scope.getMasterData = function () {
        if (!$scope.master.userName) { ShowResult("Select a user first", "failure"); return; }

        $http({
            method: "POST",
            url: $scope.getMasterListUrl,
            data: { userName: $scope.master.userName },
            dataType: "JSON"
        }).then(function (response) {
            $scope.masterDataList = response.data.data || [];
            $scope.masterColumns = response.data.columns || [];
            $scope.buildMasterGrid();
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    $scope.buildMasterGrid = function () {
        var cols = $scope.masterColumns.map(function (c) {
            return { field: c.field, headerText: c.headerText, width: 120, textAlign: "left" };
        });

        // আগের গ্রিড মুছে নতুন করে বানানো হচ্ছে, কারণ কলাম প্রতিবার বদলাতে পারে
        var host = $("#masterGridHost");
        var old = host.find("#masterGrid");
        if (old.data("ejGrid")) { old.ejGrid("destroy"); }
        host.html('<div id="masterGrid"></div>');

        $("#masterGrid").ejGrid({
            dataSource: $scope.masterDataList,
            columns: cols,
            allowFiltering: true,
            allowPaging: true,
            allowSorting: true,
            allowSelection: true,
            allowTextWrap: true,
            enableTouch: false,
            isResponsive: true,
            filterSettings: { filterType: "excel" },
            queryCellInfo: $scope.PeriodValidation,   // আপনার $scope-এ যদি এই নামে ফাংশন থাকে
            actionComplete: $scope.refreshPage        // একইভাবে
        });
    };

    // C0, C1... কে DisplayName-কীতে রূপান্তর করে। Export ও Dashboard এটাই ব্যবহার করবে।
    $scope.getMasterDisplayRows = function () {
        var cols = $scope.masterColumns || [];
        return ($scope.masterDataList || []).map(function (r) {
            var o = {};
            cols.forEach(function (c) { o[c.headerText] = r[c.field]; });
            return o;
        });
    };

    // কলামের হেডার-তালিকা, গ্রিডের ক্রম অনুযায়ী
    $scope.getMasterHeaders = function () {
        return ($scope.masterColumns || []).map(function (c) { return c.headerText; });
    };

    function buildDynamicGrid(hostId, gridId, columns, rows) {
        if (!columns || !columns.length) {
            $("#" + hostId).html('');
            ShowResult("No Data found।", "failure");
            return;
        }

        var cols = (columns || []).map(function (c) {
            return { field: c.field, headerText: c.headerText, width: 120, textAlign: "left" };
        });

        var host = $("#" + hostId);
        var old = host.find("#" + gridId);
        var oldGrid = old.data("ejGrid");
        if (oldGrid) {
            try { oldGrid.clearFiltering(); } catch (e) { }
            try { oldGrid.clearSorting(); } catch (e) { }
            try { old.ejGrid("destroy"); } catch (e) { }
        }
        host.html('<div id="' + gridId + '"></div>');

        $("#" + gridId).ejGrid({
            dataSource: rows || [],
            columns: cols,
            allowFiltering: true,
            allowPaging: true,
            allowSorting: true,
            allowSelection: true,
            allowTextWrap: true,
            enableTouch: false,
            isResponsive: true,
            filterSettings: { filterType: "excel" }
        });
    }
    // C0, C1... -> DisplayName কী
    function toDisplayRows(columns, rows) {
        return (rows || []).map(function (r) {
            var o = {};
            (columns || []).forEach(function (c) { o[c.headerText] = r[c.field]; });
            return o;
        });
    }

    // ---------- OS2.1 ----------
    $scope.os2One = { userName: "", users: [] };
    $scope.OS2OneDataList = [];
    $scope.OS2OneColumns = [];
    $scope.OS2OneDate = null;

     
    $scope.loadOS2Users = function (data) {
        $http.get('Employees/EmployeeInFoReport/GetDataDisplayUserNames?tabName=' + data).then(function (res) {
            $scope.os2One.users = res.data || [];
            if ($scope.os2One.users.length && !$scope.os2One.userName) {
                $scope.os2One.userName = $scope.os2One.users[0];
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    // $scope.getOS2ListUrl এখন অবশ্যই GetOS2Grid অ্যাকশনের url হতে হবে
    $scope.getOS2OneData = function () {
        if (!$scope.os2One.userName) { ShowResult("Select a user first", "failure"); return; }

        $http({
            method: "POST",
            url: $scope.getOS2ListUrl,
            data: { userName: $scope.os2One.userName },
            dataType: "JSON"
        }).then(function (response) {
            $scope.OS2OneDataList = response.data.data || [];
            $scope.OS2OneColumns = response.data.columns || [];
            buildDynamicGrid("os2OneGridHost", "os2OneGrid", $scope.OS2OneColumns, $scope.OS2OneDataList);
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    // Export (Excel)
    $scope.exportOS21 = function () {
        download('OS2.1', $scope.OS2OneDataList, toExportColumns($scope.OS2OneColumns));
    };

    // Dashboard
    $scope.openOS21Dashboard = function () {
        var fieldOf = {};
        ($scope.OS2OneColumns || []).forEach(function (c) { fieldOf[c.headerText] = c.field; });

        $scope.openDashboard('OS2.1', toDisplayRows($scope.OS2OneColumns, $scope.OS2OneDataList), function (header, value) {
            var field = fieldOf[header];
            var grid = $('#os2OneGrid').data('ejGrid');
            if (!field || !grid) return;
            grid.clearFiltering();
            grid.filterColumn(field, 'equal', value);
        });
    };



    // ---------- OS2.2 ----------
    $scope.os2Second = { userName: "", users: [] };
    $scope.OS2SecondDataList = [];
    $scope.OS2SecondColumns = [];
    $scope.OS2SecondDate = null;


    $scope.loadOS2SecondUsers = function (data) {
        $http.get('Employees/EmployeeInFoReport/GetDataDisplayUserNames?tabName=' + data).then(function (res) {
            $scope.os2Second.users = res.data || [];
            if ($scope.os2Second.users.length && !$scope.os2Second.userName) {
                $scope.os2Second.userName = $scope.os2Second.users[0];
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    // $scope.getOS2ListUrl এখন অবশ্যই GetOS2Grid অ্যাকশনের url হতে হবে
    $scope.getOS2SecondData = function () {
        if (!$scope.os2Second.userName) { ShowResult("Select a user first", "failure"); return; }

        $http({
            method: "POST",
            url: $scope.getOS2SecondListUrl,
            data: { userName: $scope.os2Second.userName },
            dataType: "JSON"
        }).then(function (response) {
            $scope.OS2SecondDataList = response.data.data || [];
            $scope.OS2SecondColumns = response.data.columns || [];
            buildDynamicGrid("os2SecondGridHost", "os2SecondGrid", $scope.OS2SecondColumns, $scope.OS2SecondDataList);
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    // Export (Excel)
    $scope.exportOS2Second = function () {
        download('OS2.2', $scope.OS2SecondDataList, toExportColumns($scope.OS2SecondColumns));
    };

    // Dashboard
    $scope.openOS2SecondDashboard = function () {
        var fieldOf = {};
        ($scope.OS2SecondColumns || []).forEach(function (c) { fieldOf[c.headerText] = c.field; });

        $scope.openDashboard('OS2.2', toDisplayRows($scope.OS2SecondColumns, $scope.OS2SecondDataList), function (header, value) {
            var field = fieldOf[header];
            var grid = $('#os2SecondGrid').data('ejGrid');
            if (!field || !grid) return;
            grid.clearFiltering();
            grid.filterColumn(field, 'equal', value);
        });
    };
    //--------End OS2.2----------------------


    // ---------- Production ----------
    $scope.production = { userName: "", users: [] };
    $scope.productionDataList = [];
    $scope.productionColumns = [];
    $scope.productionDate = null;


    $scope.loadProductionUsers = function (data) {
        $http.get('Employees/EmployeeInFoReport/GetDataDisplayUserNames?tabName=' + data).then(function (res) {
            $scope.production.users = res.data || [];
            if ($scope.production.users.length && !$scope.production.userName) {
                $scope.production.userName = $scope.production.users[0];
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    $scope.getProductionData = function () {
        if (!$scope.production.userName) { ShowResult("Select a user first", "failure"); return; }

        $http({
            method: "POST",
            url: $scope.getProductionListUrl,
            data: { userName: $scope.production.userName },
            dataType: "JSON"
        }).then(function (response) {
            $scope.productionDataList = response.data.data || [];
            $scope.productionColumns = response.data.columns || [];
            buildDynamicGrid("productionGridHost", "productionGrid", $scope.productionColumns, $scope.productionDataList);
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    // Export (Excel)
    $scope.exportproduction = function () {
        download('Production', $scope.productionDataList, toExportColumns($scope.productionColumns));
    };

    // Dashboard
    $scope.openproductionDashboard = function () {
        var fieldOf = {};
        ($scope.productionColumns || []).forEach(function (c) { fieldOf[c.headerText] = c.field; });

        $scope.openDashboard('Production', toDisplayRows($scope.productionColumns, $scope.productionDataList), function (header, value) {
            var field = fieldOf[header];
            var grid = $('#productionGrid').data('ejGrid');
            if (!field || !grid) return;
            grid.clearFiltering();
            grid.filterColumn(field, 'equal', value);
        });
    };
    //--------End production----------------------
   


    // ---------- WorkCenter ----------
    $scope.workCenter = { userName: "", users: [] };
    $scope.workCenterDataList = [];
    $scope.workCenterColumns = [];
    $scope.workCenterDate = null;


    $scope.loadWorkCenterUsers = function (data) {
        $http.get('Employees/EmployeeInFoReport/GetDataDisplayUserNames?tabName=' + data).then(function (res) {
            $scope.workCenter.users = res.data || [];
            if ($scope.workCenter.users.length && !$scope.workCenter.userName) {
                $scope.workCenter.userName = $scope.workCenter.users[0];
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    $scope.getWorkCenterData = function () {
        if (!$scope.workCenter.userName) { ShowResult("Select a user first", "failure"); return; }

        $http({
            method: "POST",
            url: $scope.getWorkCenterListUrl,
            data: { userName: $scope.workCenter.userName },
            dataType: "JSON"
        }).then(function (response) {
            $scope.workCenterDataList = response.data.data || [];
            $scope.workCenterColumns = response.data.columns || [];
            buildDynamicGrid("workCenterGridHost", "workCenterGrid", $scope.workCenterColumns, $scope.workCenterDataList);
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    // Export (Excel)
    $scope.exportworkCenter = function () {
        download('WorkCenter', $scope.workCenterDataList, toExportColumns($scope.workCenterColumns));
    };

    // Dashboard
    $scope.openproductionDashboard = function () {
        var fieldOf = {};
        ($scope.workCenterColumns || []).forEach(function (c) { fieldOf[c.headerText] = c.field; });

        $scope.openDashboard('WorkCenter', toDisplayRows($scope.workCenterColumns, $scope.workCenterDataList), function (header, value) {
            var field = fieldOf[header];
            var grid = $('#workCenterGrid').data('ejGrid');
            if (!field || !grid) return;
            grid.clearFiltering();
            grid.filterColumn(field, 'equal', value);
        });
    };
    //--------End WorkCenter----------------------
    
    $scope.columnList = [];
    $scope.getMasterCbo = function (data) {
        $scope.cboUrl = null;
        if (data == 'Master') {
            $scope.cboUrl = $scope.path + "GetOSMasterCboList";
        }
        else if (data =='OS2.1') {
            $scope.cboUrl = $scope.path + "GetOS2CboList";
        }
        else if (data == 'OS2.2') {
            $scope.cboUrl = $scope.path + "GetOS2SecondCboList";
        }
        else if (data == 'Production') {
            $scope.cboUrl = $scope.path + "GetProductionCboList";
        }
        else {
            $scope.cboUrl = $scope.path + "GetWorkCenterCboList";
        }
        $http.get($scope.cboUrl ).then(function (res) {
            $scope.columnList = res.data;
        });
    }
    


    $scope.searchByList = [
        { value: "DataName", name: "Data Name" },
        { value: "ColumnName", name: "Column Name" },
        { value: "Operator", name: "Operator" },
        { value: "OperatorValue", name: "Operator Value" },
        { value: "RepeatValue", name: "Repeat Value" },
        { value: "TypeText", name: "Type Text" }
    ];
    $rootScope.parameters = { searchBy: "DataName", search: "" };

    $scope.getData = function () {
        $http({
            method: "POST",
            url: $scope.getListUrl,
            data: { column: $rootScope.parameters.searchBy, value: $rootScope.parameters.search },
            dataType: "JSON"
        }).then(function (response) {
            $scope.operationStatusSettings = response.data;
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };
    $scope.getData();

    function newItem() {
        return {
            Id: null,
            DataName: null,
            ColumnName: null,
            Filter: false,
            Type: null,
            Operator: null,
            OperatorValue: null,
            StartDate: null,
            StartTime: null,
            RepeatValue: null
        };
    }
    $scope.operationStatusSetting = newItem();

    //cboService.getEnumCbo("enum/GetTypeCbo", function (result) {
    //    $scope.typeTextList = result;
    //});

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.operationStatusSetting = angular.copy($scope.operationStatusSettings[index]);
        $scope.operationStatusSetting.StartDate = $filter("dateFilter")($scope.operationStatusSetting.StartDate);
        $scope.operationStatusSetting.AddedDate = $filter("dateFilter")($scope.operationStatusSetting.AddedDate);
        $scope.operationStatusSetting.UpdatedDate = $filter("dateFilter")($scope.operationStatusSetting.UpdatedDate);
        $scope.operationStatusSetting.StartTime = $scope.operationStatusSetting.StartTimeText ? $scope.operationStatusSetting.StartTimeText.substring(0, 5) : null;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
       // if ($scope.operationStatusSetting.$valid) {
            var url = $scope.Action === "Save" ? $scope.saveUrl : $scope.updateUrl;
            $http({
                method: "POST",
                url: url,
                data: {
                    'data': $scope.operationStatusSetting
                },
                
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getData();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback() {
                ShowResult(commonMessage.NetworkError, "failure");
            });
        //}
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.operationStatusSetting.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.operationStatusSetting.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getData();
                    ClearFields(response.data.Sequence);

                }
            }, function errorCallback() {
                ShowResult(commonMessage.NetworkError, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.index = -1;
        $scope.operationStatusSetting = newItem();
    }

    $scope.operationStatusColumnControlList = [];
    $scope.getColumnControlData = function () {
        $http({
            method: "POST",
            url: $scope.getColumnControlListUrl,
            data: { column: $rootScope.parameters.searchBy, value: $rootScope.parameters.search },
            dataType: "JSON"
        }).then(function (response) {
            $scope.operationStatusColumnControlList = response.data;
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };
    $scope.getColumnControlData();

    function newControlItem() {
        return {
            Id: null,
            DataName: null,
            ColumnName: null,
            DisplayName: null,
            UserName: null,
            Sequence: null,
        };
    }
    $scope.operationStatusColumnControl = newControlItem();
    $scope.GetColumnControl = function (id, index) {
        $scope.index = index;
        $scope.operationStatusColumnControl = angular.copy($scope.operationStatusColumnControlList[index]);
        $scope.operationStatusColumnControl.AddedDate = $filter("dateFilter")($scope.operationStatusColumnControlList.AddedDate);
        $scope.operationStatusColumnControl.UpdatedDate = $filter("dateFilter")($scope.operationStatusColumnControlList.UpdatedDate);
        $scope.ActionControl = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.SaveColumnControl = function () {
        $scope.$broadcast("show-errors-check-validity");
        // if ($scope.operationStatusSetting.$valid) {
        var url = $scope.ActionControl === "Save" ? $scope.saveColumnControlUrl : $scope.saveColumnControlUrl;
        $http({
            method: "POST",
            url: url,
            data: {
                'data': $scope.operationStatusColumnControl
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getColumnControlData();
                ClearColumnControlFields(response.data.Sequence);
            }
        }, function errorCallback() {
            ShowResult(commonMessage.NetworkError, "failure");
        });
        //}
        return true;
    };


    $scope.DeleteColumnControl = function () {
        if (!baseService.isUndefinedOrNull($scope.operationStatusColumnControl.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteColumnControlUrl + $scope.operationStatusColumnControl.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getData();
                    ClearColumnControlFields(response.data.Sequence);

                }
            }, function errorCallback() {
                ShowResult(commonMessage.NetworkError, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.ClearColumnControl = function () {
        ClearColumnControlFields();
        return true;
    };

    function ClearColumnControlFields(seq) {
        $scope.Action = "SaveColumnControl";
        $scope.index = -1;
        $scope.operationStatusColumnControl = newControlItem();
    }


    $scope.hourList = [];
    $scope.minuteList = [];
    for (var i = 0; i < 24; i++) $scope.hourList.push(('0' + i).slice(-2));
    for (var j = 0; j < 60; j++) $scope.minuteList.push(('0' + j).slice(-2));
    $scope.startTimePart = { Hour: '', Minute: '' };

    // Hour/Minute থেকে StartTime ("HH:mm") বানায়
    $scope.setStartTime = function () {
        var p = $scope.startTimePart;
        if (!p.Hour) {
            p.Minute = '';
            $scope.operationStatusSetting.StartTime = null;
            return;
        }
        if (!p.Minute) p.Minute = '00';
        $scope.operationStatusSetting.StartTime = p.Hour + ':' + p.Minute;
    };

    // সেভ করা StartTime থেকে ড্রপডাউন সেট করে (Get-এর শেষে ডাকবেন)
    $scope.splitStartTime = function () {
        var t = $scope.operationStatusSetting.StartTime;
        if (t && t.indexOf(':') > -1) {
            var a = t.split(':');
            $scope.startTimePart = { Hour: a[0], Minute: a[1] };
        } else {
            $scope.startTimePart = { Hour: '', Minute: '' };
        }
    };


    var EXPORT_URL = 'Employees/EmployeeInFoReport/ExportToExcel';
    var XLSX_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';

    function pad(n) { return n < 10 ? '0' + n : '' + n; }

    function stamp() {
        var d = new Date();
        return d.getFullYear() + pad(d.getMonth() + 1) + pad(d.getDate()) + '_' + pad(d.getHours()) + pad(d.getMinutes());
    }

    function bufferToText(buf) {
        try { return new TextDecoder('utf-8').decode(new Uint8Array(buf)); } catch (e) { return ''; }
    }

    function showError(msg) {
        if (typeof ShowResult === 'function') ShowResult(msg, 'failure');
        else alert(msg);
    }

    function saveBlob(blob, fileName) {
        if (window.navigator && window.navigator.msSaveOrOpenBlob) {
            window.navigator.msSaveOrOpenBlob(blob, fileName);
            return;
        }
        var url = window.URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(function () { window.URL.revokeObjectURL(url); }, 2000);
    }

    // name: ফাইলের নাম, rows: গ্রিডের ডেটা, columns: ঐচ্ছিক [{field,title}], sheetName: ঐচ্ছিক
    function download(name, rows, columns, sheetName) {
        if (!rows || !rows.length) {
            showError('Export করার মতো কোনো ডেটা নেই। আগে ডেটা লোড করুন।');
            return;
        }
        var fileName = (name || 'Export') + '_' + stamp() + '.xlsx';

        return $http({
            method: 'POST',
            url: EXPORT_URL,
            data: { fileName: fileName, sheetName: sheetName || name, columns: columns || null, rows: rows },
            responseType: 'arraybuffer',
            transformResponse: angular.identity
        }).then(function (res) {
            var ct = (res.headers('Content-Type') || '').toLowerCase();
            // সার্ভার এরর: HTTP 200 + JSON { Error:true, Message }
            if (ct.indexOf('json') > -1) {
                var msg = 'Excel ডাউনলোড করা যায়নি।';
                try {
                    var j = JSON.parse(bufferToText(res.data));
                    if (j && j.Message) msg = j.Message;
                } catch (e) { }
                showError(msg);
                return false;
            }
            saveBlob(new Blob([res.data], { type: XLSX_TYPE }), fileName);
            return true;
        }, function () {
            showError('Excel ডাউনলোড করা যায়নি।');
            return false;
        });
    }

    $scope.exportMaster = function () {
        var cols = ($scope.masterColumns || []).map(function (c) {
            return { field: c.field, title: c.headerText };
        });
        download('Master', $scope.masterDataList, cols);
    };


    // ej-grid ফিল্টার (গ্রিডের id বদলান, যেমন #masterGrid)
    function filterGrid(gridId) {
        return function (field, value, type) {
            var grid = $(gridId).data('ejGrid');
            if (!grid) return;
            grid.clearFiltering();
            grid.filterColumn(field, type === 'date' ? 'equal' : 'equal', value);   // তারিখ কলামে মান ফরম্যাট মেলাতে হতে পারে
        };
    }

    $scope.getMasterDisplayRows = function () {
        var cols = $scope.masterColumns || [];
        return ($scope.masterDataList || []).map(function (r) {
            var o = {};
            cols.forEach(function (c) { o[c.headerText] = r[c.field]; });
            return o;
        });
    };

    $scope.openMasterDashboard = function () {
        var fieldOf = {};   // DisplayName -> C0, C1 ...
        ($scope.masterColumns || []).forEach(function (c) { fieldOf[c.headerText] = c.field; });

        $scope.openDashboard('Master', $scope.getMasterDisplayRows(), function (header, value, type) {
            var field = fieldOf[header];
            var grid = $('#masterGrid').data('ejGrid');
            if (!field || !grid) return;
            grid.clearFiltering();
            grid.filterColumn(field, 'equal', value);
        });
    };
    $scope.openWorkCenterDashboard = function () { $scope.openDashboard('WorkCenter', $scope.WorkCenterDataList, filterGrid('#workCenterGrid')); };



    // ---- Applied filter chips ----
    var OP_TEXT = { '=': 'is', '<>': 'is not', '>=': '\u2265', '=>': '\u2265', '<=': '\u2264', '=<': '\u2264', '>': '>', '<': '<', 'LIKE': 'contains', 'IN': 'in' };
    function normOp(op) {
        op = (op || '').trim().toUpperCase();
        return op === '=>' ? '>=' : (op === '=<' ? '<=' : op);
    }
    function humanize(name) {
        return String(name || '').replace(/_/g, ' ').replace(/([a-z])([A-Z])/g, '$1 $2').replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2');
    }
    function startTime(x) {                       // StartDate + StartTime; পার্স না হলে 0 (মানে সক্রিয়)
        var d = new Date(x.StartDate);
        if (isNaN(d)) return 0;
        var m = /^(\d{1,2}):(\d{2})/.exec(x.StartTime || '');
        if (m) d.setHours(+m[1], +m[2], 0, 0);
        return d.getTime();
    }
    function buildChips(list, dataName) {
        var now = Date.now(), latest = {};
        (list || []).forEach(function (x, i) {
            if (x.DataName !== dataName || !x.Filter) return;
            var t = startTime(x);
            if (t > now) return;                                   // এখনো শুরু হয়নি
            var k = x.ColumnName + '|' + normOp(x.Operator) + '|' + x.Type;
            if (!latest[k] || t >= latest[k].t) latest[k] = { t: t, i: i, x: x };
        });
        var byCol = {}, order = [];
        Object.keys(latest).forEach(function (k) {
            var x = latest[k].x, key = x.ColumnName + '|' + x.Type;
            if (!byCol[key]) { byCol[key] = []; order.push(key); }
            byCol[key].push(x);
        });
        var chips = [];
        order.forEach(function (key) {
            var items = byCol[key], first = items[0], exclude = first.Type === 'Excluded';
            var lo = items.filter(function (x) { return /^>/.test(normOp(x.Operator)); })[0];
            var hi = items.filter(function (x) { return /^</.test(normOp(x.Operator)) && normOp(x.Operator) !== '<>'; })[0];
            var rest = items.filter(function (x) { return x !== lo && x !== hi; });
            if (lo && hi) {
                chips.push({ col: humanize(first.ColumnName), raw: first.ColumnName, op: 'between', text: lo.OperatorValue + ' \u2192 ' + hi.OperatorValue, exclude: exclude });
            } else {
                rest = items;
            }
            rest.forEach(function (x) {
                var op = normOp(x.Operator);
                chips.push({ col: humanize(x.ColumnName), raw: x.ColumnName, op: OP_TEXT[op] || op, text: x.OperatorValue, exclude: exclude });
            });
        });
        return chips;
    }
    // ng-repeat-এ ফাংশন সরাসরি কল না করে scope-এ জমা রাখা হচ্ছে (না হলে digest লুপ হয়)
    $scope.activeFilters = {};
    $scope.$watch('operationStatusSettings', function (list) {
        var out = {};
        ['Master', 'OS2.1', 'OS2.2', 'Production', 'WorkCenter'].forEach(function (n) { out[n] = buildChips(list, n); });
        $scope.activeFilters = out;
    }, true);
}
