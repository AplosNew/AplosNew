"use strict";
operationStatusController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function operationStatusController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = "Operation Status ";
    $scope.Action = "Save";
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
    $scope.getListUrl = $scope.path + "getdatalist";
    $scope.getMasterListUrl = $scope.path + "GetOSMasterDataList";
    $scope.getOS2ListUrl = $scope.path + "GetOS2DataList";
    $scope.getOS2SecondListUrl = $scope.path + "GetOS2SecondDataList";
    $scope.getProductionListUrl = $scope.path + "GetProductionDataList";
    $scope.getWorkCenterListUrl = $scope.path + "GetWorkCenterDataList";


    $scope.searchMasterByList = [
        { value: "SalesOrderId", name: "SO" },
        { value: "Material", name: "Material" },
        { value: "Article", name: "Article" },
        { value: "ProductCategory", name: "ProductCategory" },
        { value: "Product", name: "Product" },
    ];
    $rootScope.parameters = { searchMasterBy: "SalesOrderId", searchMaster: "" };

    $scope.masterDataList = [];
    $scope.master = {};
    $scope.getMasterData = function () {
        $http({
            method: "POST",
            url: $scope.getMasterListUrl,
            data: { column: $rootScope.parameters.searchMasterBy, value: $rootScope.parameters.searchMaster },
            dataType: "JSON"
        }).then(function (response) {
            $scope.masterDataList = response.data;
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };
   // $scope.getMasterData();


    $scope.OS2OneDataList = [];
    $scope.OS2OneDate = null;
    $scope.getOS2OneData = function () {
        $http({
            method: "POST",
            url: $scope.getOS2ListUrl,
            data: { column: $rootScope.parameters.searchMasterBy, value: $rootScope.parameters.searchMaster },
            dataType: "JSON"
        }).then(function (response) {
            $scope.OS2OneDataList = response.data;
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };


    $scope.OS2SecondDataList = [];
    $scope.OS2SecondDate = null;
    $scope.getOS2SecondData = function () {
        $http({
            method: "POST",
            url: $scope.getOS2SecondListUrl,
            data: { column: $rootScope.parameters.searchMasterBy, value: $rootScope.parameters.searchMaster},
            dataType: "JSON"
        }).then(function (response) {
            $scope.OS2SecondDataList = response.data;
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    $scope.ProductionDataList = [];
    $scope.ProductionDate = null;
    $scope.getProductionData = function () {
        $http({
            method: "POST",
            url: $scope.getProductionListUrl,
            data: { column: $rootScope.parameters.searchMasterBy, value: $rootScope.parameters.searchMaster},
            dataType: "JSON"
        }).then(function (response) {
            $scope.ProductionDataList = response.data;
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };

    $scope.WorkCenterDataList = [];
    $scope.getWorkCenterData = function () {
        $http({
            method: "POST",
            url: $scope.getWorkCenterListUrl,
            data: { column: $rootScope.parameters.searchMasterBy, value: $rootScope.parameters.searchMaster },
            dataType: "JSON"
        }).then(function (response) {
            $scope.WorkCenterDataList = response.data;
        }, function () {
            ShowResult(commonMessage.NetworkError, "failure");
        });
    };
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

    $scope.exportMaster = function () { download('Master', $scope.masterDataList); };
    $scope.exportOS21 = function () { download('OS2.1', $scope.OS2OneDataList); };
    $scope.exportOS2Second = function () { download('OS2.2', $scope.OS2SecondDataList); };
    $scope.exportProduction = function () { download('Production', $scope.ProductionDataList); };
    $scope.exportWorkCenter = function () { download('WorkCenter', $scope.WorkCenterDataList); };


    // ej-grid ফিল্টার (গ্রিডের id বদলান, যেমন #masterGrid)
    function filterGrid(gridId) {
        return function (field, value, type) {
            var grid = $(gridId).data('ejGrid');
            if (!grid) return;
            grid.clearFiltering();
            grid.filterColumn(field, type === 'date' ? 'equal' : 'equal', value);   // তারিখ কলামে মান ফরম্যাট মেলাতে হতে পারে
        };
    }

    $scope.openMasterDashboard = function () { $scope.openDashboard('Master', $scope.masterDataList, filterGrid('#masterGrid')); };
    $scope.openOS21Dashboard = function () { $scope.openDashboard('OS2.1', $scope.OS2OneDataList, filterGrid('#oS2OneGrid')); };
    $scope.openOS2SecondDashboard = function () { $scope.openDashboard('OS2.2', $scope.OS2SecondDataList, filterGrid('#OS2SecondGrid')); };
    $scope.openProductionDashboard = function () { $scope.openDashboard('Production', $scope.ProductionDataList, filterGrid('#productionGrid')); };
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
