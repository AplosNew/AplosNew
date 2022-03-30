'use strict';
EmployeeOperationsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeOperationsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Operations';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/EmployeeOperations/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    

    //Variables
    $scope.workCenterId = null;
    $scope.processId = null;
    $scope.shiftId = null;
    $scope.POId = null;
    $scope.Date = null;
    $scope.periodId = null;
    $scope.change = null;
    $scope.ReportId = null;
    $scope.selEo = null;

    //Arrays
    $scope.workCenterList = [];
    $scope.ProcessList = [];
    $scope.ShiftList = [];
    $scope.POList = [];
    $scope.PeriodList = [];
    $scope.ModelList = [];


    // The Tab Switching Code

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    //Get Operations
    $scope.getStartUp = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetWorkCenter',
        }).then(function succ(resp) {
            $scope.workCenterList = resp.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetProcess',
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetPeriod',
        }).then(function succ(resp) {
            $scope.PeriodList = resp.data.Data;
            $scope.periodId = resp.data.Current;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetShift',
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });

       
        
    }

    $scope.getStartUp();

    // Getting the POs
    $scope.getPo = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPOs',
            data: { 'wk': $scope.workCenterId},
        }).then(function succ(resp) {
            $scope.POList = resp.data;
        });
    }


    // Add Tiles
    $scope.AddTile = function (e) {
        console.log(e);
        let ob = {};
        Object.assign(ob, e);
        ob.Id = null;
        ob.EmployeeCode = null;
        ob.EmployeeId = null;
        ob.PeriodId = e.PeriodId;
        ob.Qty = 0;
        //ob.Period2 = null;
        //ob.Period3 = null;
        //ob.Period4 = null;
        //ob.Period5 = null;
        //ob.Period6 = null;
        ob.Remarks = null;
        ob.isChanged = 0;
        ob.Serial = parseInt(e.Serial) + 1;
        $scope.ModelList.splice(e.Serial, 0, ob);
    }
    
    //Getting All the Data For the Saving
    $scope.getAllData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetOperationsData',
            data: { 'PId': $scope.POId, 'Period': $scope.periodId, 'ProcessId': $scope.ProcessId},
        }).then(function succ(resp) {
            $scope.ModelList = resp.data;
            for (var i = 0; i < $scope.ModelList.length; i++) {
                Object.assign($scope.ModelList[i], {'Serial': parseInt(i+1) ,'isChanged': 0 , 'Remarks':null });
                //$scope.refreshPage();
            }
        });
    }

   // While Changing the Places
    $scope.changeInData = function (e, col) {
        e.isChanged = 1;
    }

    //Saving of the Data
    $scope.saveData = function () {

        $scope.NewList = [];

        for (var i = 0; i < $scope.ModelList.length; i++) {
            if ($scope.ModelList[i].isChanged == true || $scope.ModelList[i].Qty > 0) {
                $scope.NewList.push($scope.ModelList[i]);
            }
        }

        $http({
            method: 'POST',
            url: $scope.path + 'saveData',
            data: {
                'data': $scope.NewList, 'WorkCenter': $scope.workCenterId,
                'ProcessId': $scope.processId,
                'ShiftId': $scope.shiftId,
                'POId': $scope.POId ,
                'Date': $scope.Date, 'PeriodId': $scope.periodId,
                  },
        }).then(function succ(resp) {

            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                ShowResult(resp.data.Message, 'success');
                $scope.ClearGrid();
            }

        });
    }

    //Clearing the grid
    $scope.ClearGrid = function(){
        $scope.ModelList = [];
    }

    // Getting the report
    $scope.getReportView = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getReportView',
        }).then(function succ(response) {
            console.log(response.data.Data);
            console.log(response.data.Cols);

            var ColumnList = [
                { field: 'OperationCode', width: 80, headerText: "Operation Code" },
                { field: 'OperationName', width: 80, headerText: "Operation" },
                { field: 'WorkCenter', width: 80, headerText: "WorkCenter" },
                { field: 'ProductionOrderId', width: 80, headerText: "PO" },
                { field: 'Process', width: 80, headerText: "Process" },
                { field: 'EmployeeCode', width: 80, headerText: "Employee Code" },
                { field: 'EmployeeName', width: 80, headerText: "Employee Name" },
                { field: 'Date', width: 80, headerText: "Date" },
            ];


            for (var i = 0; i < response.data.Cols.length; i++) {
                ColumnList.push({ field: response.data.Cols[i], width: 50, headerText: response.data.Cols[i], type: "number" });// format: "{0:N2}",
            }

            $("#summaryGrid").ejGrid({
                dataSource: response.data.Data,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: ColumnList
                //queryCellInfo: $scope.cellColorChange
            });

            var gridObj = $("#summaryGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        });
    }
    // Download Button Functionality
    $scope.getReportDownload = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getReportDownload",
            
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.getProcessDownload = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getProcessDownload",
            data: {
                'Date': $scope.Date
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

   $scope.getEmployeeWorkDurationReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployeeWorkDurationReport",
            data: {
                'Date': $scope.Date
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    

    //Processing Button
    $scope.processAll = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'processAll',
            data: {
                'Date': $scope.Date
            },
        }).then(function succ(resp) {

            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                ShowResult(resp.data.Message, 'success');
               
            }

        });
    }
}