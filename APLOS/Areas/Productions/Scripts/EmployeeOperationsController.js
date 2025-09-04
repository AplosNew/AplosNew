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
    $scope.EntityId = null;

    $scope.PODBuyerRef = null;
    $scope.PODOwnRef = null;
    $scope.PODArticle = null;

    $scope.responsiblePerson = null;
    $scope.responsiblePersonId = null;

    var show = document.getElementById("ShowForm");

    //Arrays
    $scope.EntityList = [];
    $scope.workCenterList = [];
    $scope.ProcessList = [];
    $scope.ShiftList = [];
    $scope.POList = [];
    $scope.PeriodList = [];
    $scope.ModelList = [];
    $scope.EmployeeList = [];

    let wipNos = {};

    // The Tab Switching Code

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        if (newTab == 3) {
            show.style.display = "none";
        }
        else {
            show.style.display = "block";
        }
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
      
        return $scope.tab === tabNum;
    };


    //Get Operations
    $scope.getStartUp = function () {

        $http({
            method: 'POST',
            url: $scope.path + 'GetEntity'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetPeriod',
        }).then(function succ(resp) {
            $scope.PeriodList = resp.data.Data;
            $scope.periodId = resp.data.Current;
        });

        //$http({
        //    method: 'GET',
        //    url: $scope.path + 'GetShift',
        //}).then(function succ(resp) {
        //    $scope.ShiftList = resp.data;
        //});

        $http({
            method: 'GET',
            url: $scope.path + 'GetEmps',
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });

    }

    $scope.getStartUp();
    //Getting the Process
    $scope.getProcess = function () {

        $http({
            method: 'POST',
            url: $scope.path + 'GetProcess',
            data: {'EId':$scope.EntityId}
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;
        });
    }

    $scope.GetShiftList = function () {
        $http.get('Productions/Productionsummary/GetShiftList?processId=' + $scope.ProcessId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.ShiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.shiftId = $scope.ShiftList[0].Value;
                    }
                }
            });
    }

    //Getting the WorkCenters
    $scope.getWkC = function () {
        $scope.workCenterList = [];
        $http({
            method: 'POST',
            url: $scope.path + 'GetWorkCenter',
            data: { 'PId': $scope.ProcessId, 'entityId': $scope.EntityId }
        }).then(function succ(resp) {
            $scope.workCenterList = resp.data;
        });
    }

    //Getting the Responsible Persons
    $scope.getResponsiblePerson = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'GetResp',
            params: {'WKId' : $scope.workCenterId},
        }).then(function succ(resp) {
            if (resp.data.length > 0) {
                $scope.responsiblePerson = resp.data[0].EmployeeName;
                $scope.responsiblePersonId = resp.data[0].ResponsiblePersonId;
            }
            else {
                $scope.responsiblePerson =null;
                $scope.responsiblePersonId =null;
            }
        });
    }

    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'Costings/QuickCostingMaster/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
    }
    $scope.getPopUpData();

    $scope.setEmpData = function (obj) {
        $scope.responsiblePersonId = obj.data.SystemID;
        $scope.responsiblePerson = obj.data.EmployeeName;

        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    // Getting the POs
    $scope.getPo = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPOs',
            data: { 'entityId': $scope.EntityId},
        }).then(function succ(resp) {
            $scope.POList = resp.data;
        });
    }

    //Getting the PO Details
    $scope.getPODetails = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getPODetails',
            data: { 'POId': $scope.POId },
        }).then(function succ(resp) {
            $scope.PODBuyerRef = resp.data[0].BuyerReferenceNo;
            $scope.PODOwnRef = resp.data[0].OwnReferenceNo;
            $scope.PODArticle = resp.data[0].Article;
        });
    }

    //Getting the Responsible Person
    $scope.selectResp = function () {
        angular.element(document.querySelector('#employeesModal')).modal('show');
    }

    $scope.doubleResp = function (e) {
        $scope.responsiblePerson = e.data.EmployeeName;
        $scope.responsiblePersonId = e.data.SystemId;
        angular.element(document.querySelector('#employeesModal')).modal('hide');
    }

    // Refreshing the serials
    function refreshSerial() {
        for (var j = 0; j < $scope.ModelList.length ; j++)
        {
            $scope.ModelList[j].Serial = j;
        }
    }

    // Add Tiles
    $scope.AddTile = function (e) {
        let ob = {};
        Object.assign(ob, e);
        ob.Id = null;
        ob.EmployeeCode = null;
        ob.EmpName = null;
        ob.EmployeeId = null;
        ob.PeriodId = e.PeriodId;
        ob.Qty = 0;
       
        ob.Remarks = null;
        ob.isChanged = 0;
        $scope.ModelList.splice(e.Serial+1, 0, ob);
        refreshSerial();
    }
    
    //Getting All the Data For the Saving
    $scope.PrevAllList = [];
    $scope.getAllData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetOperationsData',
            data: { 'PId': $scope.POId, 'Period': $scope.periodId, 'ProcessId': $scope.ProcessId, 'WorkCenterId': $scope.workCenterId},
        }).then(function succ(resp) {
            $scope.ModelList = resp.data;
            for (var i = 0; i < $scope.ModelList.length; i++) {
                Object.assign($scope.ModelList[i], {'Serial': parseInt(i) ,'isChanged': 0 , 'Remarks':null });
                //$scope.refreshPage();
                if ($scope.ModelList[i].Sequence in wipNos) {
                    continue;
                }
                else {
                    wipNos[$scope.ModelList[i].Sequence] = 0;
                }
            }

            $scope.PrevAllList = $scope.ModelList;
        });
    }

    function refresh() {
        var gridObj = $("#EmpEditsGrid").data("ejGrid");
        gridObj.dataSource($scope.ModelList);
    }

   // While Changing the Places
    $scope.changeInData = function (e, col) {
        e.isChanged = 1;

        if (col == 'emp') {
            //const results = $scope.EmployeeList.filter(object => Object.values($scope.EmployeeList).some(i => i.includes(e.EmployeeCode)));
            //console.log(results);

            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                if ($scope.EmployeeList[i].EmployeeCode == e.EmployeeCode) {
                    e.EmpName = $scope.EmployeeList[i].EmployeeName;
                }
            }
        }

        if (col === 'qty' && e.Sequence != 1) {
            let prevQty = 0;
            for (var i = 0; i < $scope.ModelList.length; i++) {
                if ($scope.ModelList[i].Sequence == e.Sequence - 1) {
                    prevQty = prevQty + parseFloat($scope.ModelList[i].Qty);
                }
            }
            let currQty = 0;
            for (var i = 0; i < $scope.ModelList.length; i++) {
                if ($scope.ModelList[i].Sequence == e.Sequence) {
                    currQty = currQty + parseFloat($scope.ModelList[i].Qty);
                }
            }

            if (currQty > prevQty) {
                e.Qty = 0;
                ShowResult('Value Exceeds than WIP in ' + e.OperationCode + '!!', 'failure');
            }
            
        }
       // refresh();

    }

    //Check WIP
    //$scope.checkWIP = function () {
    //    for (var i = 0; i < $scope.ModelList.length; i++) {
    //        wipNos[$scope.ModelList[i].Sequence] += $scope.ModelList[i].Qty;
    //        let ind = Object.keys(wipNos)
    //        let index = ind.indexOf($scope.ModelList[i].Sequence.toString());
    //        let prevVal = Object.values(wipNos)[index - 1];
    //        if ($scope.ModelList[i].Sequence != 0 && wipNos[$scope.ModelList[i].Sequence] > (prevVal + $scope.ModelList[i].WIP)) {
    //            wipNos[$scope.ModelList[i].Sequence] -= $scope.ModelList[i].Qty;
    //            ShowResult('Value Exceeds than WIP in ' + $scope.ModelList[i].OperationCode+ '!!', 'failure');
    //        }
    //    }
       
    //}

    $scope.isSaveBtnDisable = false;
    //Saving of the Data
    $scope.saveData = function () {

        $scope.NewList = [];

       /* $scope.checkWIP();*/

        for (var i = 0; i < $scope.ModelList.length; i++) {
            if ($scope.ModelList[i].isChanged == true && $scope.ModelList[i].Qty > 0) {
                $scope.NewList.push($scope.ModelList[i]);
            }
        }

        $scope.ModelList = $scope.PrevAllList;

        $http({
            method: 'POST',
            url: $scope.path + 'saveData',
            data: {
                'data': $scope.NewList, 'WorkCenter': $scope.workCenterId,
                'ProcessId': $scope.ProcessId,
                'ShiftId': $scope.shiftId,
                'POId': $scope.POId ,
                'Date': $scope.Date, 'PeriodId': $scope.periodId,
                'ResponsiblePersonId': $scope.responsiblePersonId,
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
    $scope.saveRowItemData = function (data) {
        $scope.isSaveBtnDisable = true;
        for (var i = 0; i < $scope.ModelList.length; i++) {
            if ($scope.ModelList[i].Sequence == data.Sequence + 1) {
                var NextoperationVariationId = $scope.ModelList[i].OperationId;
            }
        } 

        $scope.NewList = [];
        for (var j = 0; j < $scope.ModelList.length; j++) {
            if ($scope.ModelList[j].Sequence == data.Sequence) {
                $scope.NewList.push($scope.ModelList[j]);
            }
        }
        
        $scope.ModelList = $scope.PrevAllList;

        $http({
            method: 'POST',
            url: $scope.path + 'saveRowItemData',
            data: {
                'data': $scope.NewList, 'WorkCenter': $scope.workCenterId,
                'ProcessId': $scope.ProcessId,
                'ShiftId': $scope.shiftId,
                'POId': $scope.POId,
                'Date': $scope.Date, 'PeriodId': $scope.periodId,
                'ResponsiblePersonId': $scope.responsiblePersonId,
                'NxtOPVariationId': NextoperationVariationId,
            },
        }).then(function succ(resp) {

            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                ShowResult(resp.data.Message, 'success');
                $scope.getAllData();
                //$scope.ClearGrid();
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
            data: { 'Date': $scope.Date, 'Wkc': $scope.workCenterId},
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
    $scope.FromDate = null;
    $scope.ToDate = null;
    $scope.getReportDownload = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getReportDownload",
            data: { 'Date': $scope.Date, 'Wkc': $scope.workCenterId},
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
                'FromDate': $scope.FromDate , 'ToDate':$scope.ToDate,
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
                'FromDate': $scope.FromDate, 'ToDate': $scope.ToDate,
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
    $scope.ProcessDate = null;
    $scope.processAll = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'processAll',
            data: {
                'Date': $scope.ProcessDate
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