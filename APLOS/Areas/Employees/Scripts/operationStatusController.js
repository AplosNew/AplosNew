"use strict";
operationStatusController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function operationStatusController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = "Operation Status ";
    $scope.Action = "Save";


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
    $scope.deleteUrl = $scope.path + "DeleteOperationStatusSetting/";
    $scope.getListUrl = $scope.path + "getdatalist";
    $scope.getMasterListUrl = $scope.path + "GetOS1MasterDataList";
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
            data: { column: $rootScope.parameters.searchMasterBy, value: $rootScope.parameters.searchMaster, fromDate: $scope.master.Date },
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
            data: { column: $rootScope.parameters.searchMasterBy, value: $rootScope.parameters.searchMaster, fromDate: $scope.OS2OneDate },
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
            data: { column: $rootScope.parameters.searchMasterBy, value: $rootScope.parameters.searchMaster, fromDate: $scope.OS2SecondDate },
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
            url: $scope.getOS2SecondListUrl,
            data: { column: $rootScope.parameters.searchMasterBy, value: $rootScope.parameters.searchMaster, fromDate: $scope.ProductionDate },
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
}
