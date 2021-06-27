'use strict';
OrderControlTypesController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function OrderControlTypesController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Order Control Types";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.lsds = [];
    $scope.path = 'OrderManagements/OrderControlTypes/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveMasterUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.model = [];
    $scope.GetSettingsData = function () {
        $http({
            method: 'POST',
            data: {},
            url: $scope.getListUrl
        }).then(function successCallback(response) {
            $scope.model = response.data;
        });

    };
    $scope.GetSettingsData();

    $scope.Save = function () {
        $http({
            method: 'POST',
            data: { 'data': $scope.model },
            url: $scope.path + 'Save'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSettingsData();
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    };



    $scope.EmployeemodelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'EmployeeCode', name: 'Code ' },
        { value: 'EmployeeName', name: 'Name ' },
        { value: 'Department', name: 'Department ' },
        { value: 'Designation', name: 'Designation ' },
        { value: 'Section', name: 'Section ' },
        { value: 'SubSection', name: 'Sub Section ' }
    ];
    $scope.searchCol = "UserName";
    $scope.searchVal = "";
    $scope.EmployeeSearchCol = "EmployeeName";
    $scope.EmployeeSearchVal = "";
    $scope.WhereEmployeeNeeded = {};
    $scope.EmployeeList = [];
    $scope.OpenEmployeeSearchBox = function (WhereEmployeeNeeded) {
        $scope.WhereEmployeeNeeded = WhereEmployeeNeeded;
        var eDialog = $("#dialogSearchEmployee").data("ejDialog");
        eDialog.open();
  
        $scope.getEmployeeData();
    }
    $scope.getEmployeeData = function () {
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'column': $scope.EmployeeSearchCol, 'value': $scope.EmployeeSearchVal },
                url: $scope.path + 'SearchEmployee'

            }).then(function successCallback(response) {
                $scope.EmployeeList = response.data;

            });
        } catch (e) {

        }
    }
    $scope.TagEmployee = function (args) {
        var eDialog = $("#dialogSearchEmployee").data("ejDialog");
        eDialog.close();
        for (var i = 0; i < $scope.model.length; i++) {
            if ($scope.WhereEmployeeNeeded.ControlType == $scope.model[i].ControlType) {

                $scope.model[i].ResponsiblePersonId = args.data.Id;
                $scope.model[i].ResponsiblePerson = args.data.EmployeeName;
                
                break;
            }
        }

        var gridObj = $("#GridSettings").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();

    }

}
