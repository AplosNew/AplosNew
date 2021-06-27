'use strict';
TaskReplacementController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TaskReplacementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Task Replacement';
    $scope.path = 'TaskManagement/TaskReplacement/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    var _currentDate = new Date();
    var numberOfDaysToAdd = 7;
    _currentDate.setDate(_currentDate.getDate() + numberOfDaysToAdd);


    $scope.TaskList = [];
    $scope.ReplacementModelMain = { FromDate: new Date(), ToDate: _currentDate, FromEmployeeId: null, FromEmployeeCode: null, FromEmployeeName: null, FromEmployeeImage: null, ToEmployeeId: null, ToEmployeeCode: null, ToEmployeeName: null, ToEmployeeImage: null };
    $scope.ReplacementModel = Object.assign({}, $scope.ReplacementModelMain);

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
    $scope.WhereEmployeeNeeded = '';
    $scope.EmployeeList = [];
    $scope.OpenEmployeeSearchBox = function (WhereEmployeeNeeded) {
        $scope.WhereEmployeeNeeded = WhereEmployeeNeeded;
        $scope.searchVal = "";
        $scope.EmployeeSearchVal = "";

        var eDialog = $("#dialogSearchEmployee").data("ejDialog");
        eDialog.open();

        $scope.getEmployeeData();
    }
    $scope.getEmployeeData = function () {

        var path = $scope.path + 'SearchEmployee';
        if ($scope.WhereEmployeeNeeded == 'FromEmployee')
            path = $scope.path + 'SearchEmployeeFrom';
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'column': $scope.EmployeeSearchCol, 'value': $scope.EmployeeSearchVal },
                url: path

            }).then(function successCallback(response) {
                $scope.EmployeeList = response.data;

            });
        } catch (e) {

        }
    }
    $scope.DeleteEmployee = function (flag) {
        if (flag == 'FromEmployee') {
            $scope.ReplacementModel.FromEmployeeId = null;
            $scope.ReplacementModel.FromEmployeeCode = null;
            $scope.ReplacementModel.FromEmployeeName = null;
            $scope.ReplacementModel.FromEmployeeImage = null;
        }
        else {
            $scope.ReplacementModel.ToEmployeeId = null;
            $scope.ReplacementModel.ToEmployeeCode = null;
            $scope.ReplacementModel.ToEmployeeName = null;
            $scope.ReplacementModel.ToEmployeeImage = null;
        }
    }
    $scope.AddEmployee = function (args) {

        var eDialog = $("#dialogSearchEmployee").data("ejDialog");
        eDialog.close();

        if ($scope.WhereEmployeeNeeded == 'FromEmployee') {
            $scope.ReplacementModel.FromEmployeeId = args.data.Id;
            $scope.ReplacementModel.FromEmployeeCode = args.data.EmployeeCode;
            $scope.ReplacementModel.FromEmployeeName = args.data.EmployeeName;
            $scope.ReplacementModel.FromEmployeeImage = args.data.EmpPicPath;
        }
        else {
            $scope.ReplacementModel.ToEmployeeId = args.data.Id;
            $scope.ReplacementModel.ToEmployeeCode = args.data.EmployeeCode;
            $scope.ReplacementModel.ToEmployeeName = args.data.EmployeeName;
            $scope.ReplacementModel.ToEmployeeImage = args.data.EmpPicPath;
        }
    }

    $scope.getTaskList = function () {

        try {
            if (angular.isUndefinedOrNull($scope.ReplacementModel.FromEmployeeId))
                throw 'Please enter From Employee';

            if (angular.isUndefinedOrNull($scope.ReplacementModel.ToEmployeeId))
                throw 'Please enter To Employee';

            if (angular.isUndefinedOrNull($scope.ReplacementModel.FromDate))
                throw 'Please enter From Date';


            if (angular.isUndefinedOrNull($scope.ReplacementModel.ToDate))
                throw 'Please enter To Date';


            $scope.hideTaskDetails = false;
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'data': $scope.ReplacementModel },
                url: $scope.path + 'getTaskList'

            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.TaskList = response.data.DATA;
                }
                else {

                }

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }


    }
    $scope.Update = function () {

        try {
            if (angular.isUndefinedOrNull($scope.ReplacementModel.FromEmployeeId))
                throw 'Please enter From Employee';

            if (angular.isUndefinedOrNull($scope.ReplacementModel.ToEmployeeId))
                throw 'Please enter To Employee';

            if (angular.isUndefinedOrNull($scope.ReplacementModel.FromDate))
                throw 'Please enter From Date';


            if (angular.isUndefinedOrNull($scope.ReplacementModel.ToDate))
                throw 'Please enter To Date';


            var string = "''";
            var collection = [];
            for (var i = 0; i < $scope.TaskList.length; i++) {
                if ($scope.TaskList[i].Checked == true) {
                    if (collection.includes($scope.TaskList[i].Id) == false) {
                        string += ",'" + $scope.TaskList[i].Id + "'";
                        collection.push($scope.TaskList[i].Id);
                    }
                }
            }

            if (string == "''")
                throw 'Select at least one task from the list';

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'data': $scope.ReplacementModel, TaskList: string },
                url: $scope.path + 'Update'

            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.getTaskList();
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.hideTaskDetails = true;
    $scope.Clear = function () {
        $scope.TaskList = [];
        $scope.hideTaskDetails = true;
    }
    function headCheckChangeTaskList(e) {
        if (e.model.checkState == "check") {
            for (var i = 0; i < $scope.TaskList.length; i++) {
                $scope.TaskList[i].Checked = true;
            }

        }
        else {
            for (var i = 0; i < $scope.TaskList.length; i++) {
                $scope.TaskList[i].Checked = false;
            }
        }

        var gridObj = $("#GridEdit").data("ejGrid");
        gridObj.refreshContent();
    }

    $scope.refreshTemplateTaskList = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeTaskList });
        }
    }
    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    }
}