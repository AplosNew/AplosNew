'use strict';
SalaryStructureUnApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SalaryStructureUnApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Salary Structure Un-Approval';
    $scope.index = -1;

    $scope.EmpSalaryInfoDefine = [];
    $scope.path = 'Payrolls/SalaryStructureUnApproval/';   
    $scope.getEmpListUrl = $scope.path + 'GetEmployeeListForSalaryStrcUnApproval';
    $scope.SaveSalaryStructureApprovalDataUrl = $scope.path + 'SaveSalaryStructureApprovalData';
    $scope.SaveSalaryStructureUnApprovalDataUrl = $scope.path + 'SaveSalaryStructureUnApprovalData';
    $scope.IsApprovedEmpList = false;
    $scope.employees = [];




    $scope.LoadEmployeeDataForGrid = function (url) {
        try {
            $http.get(url)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        $scope.employees = null;
                        $scope.employees = response.data;


                    }
                    function errorCallBack(response) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };


    $scope.LoadEmployeeDataForGrid($scope.getEmpListUrl);

    $scope.messageText = "";

    $scope.ShowResultCustom = function () {
        $("#dialogMessage").ejDialog("setTitle", "Success");
        $scope.messageText = message;
        $scope.messageTitle = "Message";

        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };

    


    function onClickUnApproval(arg) {
       
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.message_confirmation = null;
      
    }

    $scope.message_confirmation = null;
    $scope.remove = function (obj) {
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.employeeInformation = data;
        if (!baseService.isUndefinedOrNull($scope.employeeInformation.SystemId))
            $scope.message_confirmation = 'Are you sure to Unapprove This Employee  [ ' + $scope.employeeInformation.EmployeeCode + ' ] Salary Structure ?';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    };

    $scope.Delete = function () {
      
        try {
            $.ajax({
                type: "POST",
                url: $scope.SaveSalaryStructureUnApprovalDataUrl,
                data:
                {

                    'EmpSystemId': $scope.employeeInformation.SystemId, 'SalaryStructureId': $scope.employeeInformation.SalaryStructureId
                },
                dataType: "json",
                success: function (response) {
                    if (response.Error) {
                        ShowResult(response.Message, 'error');
                    } else {
                        //$scope.ShowResult(data.Message, "success");
                        ShowResult(response.Message, 'success');
                        $scope.LoadEmployeeDataForGrid($scope.getEmpListUrl);

                    }
                 
                }

            });
        } catch (e) {
            ShowResult(e, 'error');
        }
    };

























///ck all
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === "";
    };
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employees, { 'SystemId': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.employees.length; i++) {
                    $scope.employees[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.employees.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employees[i].SystemId === filtered[j].SystemId)
                            $scope.employees[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.employees.length; i++) {
                    $scope.employees[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employees[i].SystemId == filtered[j].SystemId)
                            $scope.employees[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Grid .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.employees, { 'SystemId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckBoxSelect == true)
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    };

          
    

    

}