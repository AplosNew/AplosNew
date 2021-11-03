'use strict';
SandwichAbsentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function SandwichAbsentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Sandwich Absent';
    $scope.Action = 'Save';   
    $scope.path = 'Attendances/SandwichAbsent/';
    $scope.getListUrl = $scope.path + 'GetEmployeeList';
    $scope.getAttdnBalanceUrl = $scope.path + 'GetAttdnDetails';
    $scope.getAssignedListUrl = $scope.path + 'GetAssignedEmployeeList';

    $scope.saveAbsentUrl = $scope.path + 'SaveAbsent';
    $scope.deleteAbsentUrl = $scope.path + 'DeleteAbsent';
  

    $scope.FromDate = null;
    $scope.ToDate = null;
    $scope.EmployeeList = [];
    $scope.AssignedEmployeeList = [];
    $scope.AttdnBalanceList = [];
    
    $scope.GetEmployeeList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Please Enter From Date.";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Please Enter To Date.";
            }
            $scope.EmployeeList = [];
            $http.get($scope.getListUrl + '?FromDate=' + $scope.FromDate + '&ToDate=' + $scope.ToDate)
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.EmployeeList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    $scope.GetAssignedEmployeeList = function () {
        try {
            $scope.AssignedEmployeeList = [];
            $http.get($scope.getAssignedListUrl )
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.AssignedEmployeeList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetAssignedEmployeeList();


    $scope.SelectAttdnDetails = function () {


            
       
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];

        //$("#dialogAttdnDetails").ejDialog("setTitle", "Attendance information of " + data.EmployeeName.toString());
        var eDialog = $("#dialogAttdnDetails").data("ejDialog");
        eDialog.open(); 

        $scope.AttdnBalanceList = [];
        $http.get($scope.getAttdnBalanceUrl + '?EmpsystemId=' + data.EmpSystemID + '&FromDate=' + data.BeforeWorkDate + '&ToDate=' + data.AfterWorkDate)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.AttdnBalanceList  = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
       
    };
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                $scope.EmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    };






    

    $scope.SaveAbsent = function () {
        //$scope.AvailedLvDetails = [];
        try {
            var obj = {};
            var AbsentList = [];
            for (var i = 0; i < $scope.EmployeeList.length; i++) {

                if ($scope.EmployeeList[i].CheckBoxSelect === true) {
                    obj = {};
                    obj.EmpSystemID = $scope.EmployeeList[i].EmpSystemID;
                    obj.WorkingDate = $scope.EmployeeList[i].AttdnProcDate;
                    obj.DayStatus = $scope.EmployeeList[i].DayStatus;

                    AbsentList.push(obj);
                }

            }
            if (AbsentList.length == 0) {
                throw "Please Select Employees.";
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: {'AbsentList': AbsentList, 'FromDate':  $scope.FromDate ,'ToDate': $scope.ToDate },
                url: $scope.saveAbsentUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                 
                    ShowResult(response.data.Message, "success");
                    $scope.GetAssignedEmployeeList();
                    $scope.GetEmployeeList();
                  
                }
            }, function errorCallback(response) {
                ShowResult(response.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };


    $scope.deleteId = null;
    $scope.DayStatusType = null;
    $scope.message_confirmation = null;
    $scope.remove = function (obj) {
        var gridObj = $("#GridAssigned").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.deleteId = data.id;
        $scope.DayStatusType = data.DayStatus;
        if (!baseService.isUndefinedOrNull($scope.deleteId))
            $scope.message_confirmation = 'Are you sure to Delete This Employee  [ ' + data.EmployeeCode + ' ] Sandwich Absenteeism Assignment ?';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    };

    $scope.Delete = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.deleteId)) {
                throw "Please Select Employees.";
            }
            $.ajax({
                type: "POST",
                url: $scope.deleteAbsentUrl,
                data:
                {

                    'Id': $scope.deleteId, 'DayStatus': $scope.DayStatusType
                },
                dataType: "json",
                success: function (response) {
                    //$scope.ShowResult(data.Message, "success");
                    ShowResult(response.Message, 'success');
                    $scope.GetAssignedEmployeeList();

                }

            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


}