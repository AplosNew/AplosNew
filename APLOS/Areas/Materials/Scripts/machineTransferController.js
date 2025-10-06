'use strict';
machineTransferController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function machineTransferController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Machine Transfer';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'materials/MachineBudget/';
     

    $scope.model = {
        EntityId :null,
        ToEntityId: null,
        TransferType: null,
        ResponsiblePersonId:null,
        ResponsiblePerson:null,
        ApprovedBy: null,
        Days: null,
        Remarks:null,
        Id:null
    }

    var show = document.getElementById("ShowForm");

    //Arrays
    $scope.EntityList = [];
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
            url:'Productions/EmployeeOperations/GetEntity'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });


        //$http({
        //    method: 'GET',
        //    url: $scope.path + 'GetEmps',
        //}).then(function succ(resp) {
        //    $scope.EmployeeList = resp.data;
        //});

    }

    $scope.getStartUp();
    //Getting the Process
   

    //Getting the Responsible Persons
    $scope.getResponsiblePerson = function () {

        $http({
            method: 'GET',
            url: 'Productions/EmployeeOperationsGetResp',
            params: {'WKId' : $scope.workCenterId},
        }).then(function succ(resp) {
            if (resp.data.length > 0) {
                $scope.model.ResponsiblePerson = resp.data[0].EmployeeName;
                $scope.model.ResponsiblePersonId = resp.data[0].ResponsiblePersonId;
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
        $scope.model.ResponsiblePersonId = obj.data.SystemID;
        $scope.model.ResponsiblePerson = obj.data.EmployeeName;

        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    
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
            url: $scope.path + 'GetMachineBudgetByFromEntity',
            data: { 'EntityId': $scope.model.EntityId},
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
    }

    $scope.isSaveBtnDisable = false;
    //Saving of the Data
    $scope.saveData = function () {

        $scope.NewList = [];

       /* $scope.checkWIP();*/
        $scope.model.ApprovedBy = '1234';
        $scope.model.EntityId = $scope.model.EntityId;
        for (var i = 0; i < $scope.ModelList.length; i++) {
            if ($scope.ModelList[i].isChanged == true && $scope.ModelList[i].Qty > 0) {
                $scope.NewList.push($scope.ModelList[i]);
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + 'CreateMachineTransfer',
            data: {
                'data': $scope.model,
                'childData': $scope.NewList
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
    
}