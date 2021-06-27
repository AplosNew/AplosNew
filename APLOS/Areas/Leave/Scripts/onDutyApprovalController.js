'use strict';
onDutyApprovalController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function onDutyApprovalController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'On Duty Approval';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'Leave/OnDutyApproval/';
    $scope.processUrl = $scope.path + 'Process';
    $scope.Action = 'Approve';

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#OnDutyApprovalGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ondutyapprovalList.length; i++) {
                $scope.ondutyapprovalList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#OnDutyApprovalGrid").data("ejGrid");
        gridObj.refreshContent();
    };
    
    $scope.Process = function () {
        try {
            $scope.EmployeeListNew = [];
            for (var i = 0; i < $scope.ondutyapprovalList.length; i++) {
                if ($scope.ondutyapprovalList[i].CheckBoxSelect == true) {
                    if ($scope.EmployeeListNew, $scope.ondutyapprovalList[i].EmpSystemId)  {
                        $scope.EmployeeListNew.push($scope.ondutyapprovalList[i]);
                    }
                }                             
            }
            
            var data = ej.DataManager($scope.EmployeeListNew).executeLocal(ej.Query().select(["FromDate", "ToDate","EmpSystemId"]));


            if ($scope.EmployeeListNew.length == 0) {
                throw "Please Select Employee....";
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.OnDutyApprovalForm.$valid) {
                if ($scope.Action === 'Approve') {
                    $http({
                        method: 'POST',
                        url: $scope.processUrl,
                        data: { 'EmpList': data},
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            var gridObj = $("#OnDutyApprovalGrid").data("ejGrid");
                            gridObj.refreshContent(true);
                            $scope.getListData();
                        }

                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {
       
    } 
    
    $scope.ondutyapprovalList = [];
    $scope.getListData = function () {
        $http.get('Leave/OnDutyApproval/getlist')
            .then(
                function successCallback(response) {
                   // $scope.ondutyapprovalList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        if (!baseService.isUndefinedOrNull(response.data)) {
                            $scope.ondutyapprovalList = response.data;
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.getListData();

    
}