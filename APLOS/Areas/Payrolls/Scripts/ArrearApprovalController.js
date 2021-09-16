'use strict';
ArrearApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ArrearApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Arrear Approval';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/ArrearApproval/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.FromDate = new Date();
    $scope.ToDate = new Date();

    $scope.ArrearProcessInfo = [];
    $scope.SelectedArrearProcessBatchId = null;
    $http({
        method: "GET",
        dataType: 'JSON',
        url: 'humanresource/PayrollReports/GetAllArrearProcessInfo'
    }).then(function successCallback(response) {
        $scope.ArrearProcessInfo = response.data;
    });

    $scope.EmployeeListApproved = [];
    $scope.EmployeeListUnApproved = [];
    $scope.GetEmployeeInformation = function () {

        try {

            var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
            $scope.SelectedArrearProcessBatchId = DropDownListYear.getSelectedValue();
         

            if (baseService.isUndefinedOrNull($scope.SelectedArrearProcessBatchId)) {
                throw 'Please select batch';
            }

            var parameters = { 'batchId': $scope.SelectedArrearProcessBatchId };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.path + 'GetEmpList',
                data: parameters
            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.length; i++) {

                    if (angular.isUndefinedOrNull(response.data[i].DOJ) == false)
                        response.data[i].DOJ = new Date(response.data[i].DOJ);

                    if (angular.isUndefinedOrNull(response.data[i].DOS) == false)
                        response.data[i].DOS = new Date(response.data[i].DOS);

                    if (angular.isUndefinedOrNull(response.data[i].LastSalaryEffectiveDate) == false)
                        response.data[i].LastSalaryEffectiveDate = new Date(response.data[i].LastSalaryEffectiveDate);

                    if (angular.isUndefinedOrNull(response.data[i].LatestSalaryEffectiveDate) == false)
                        response.data[i].LatestSalaryEffectiveDate = new Date(response.data[i].LatestSalaryEffectiveDate);

                }

                $scope.EmployeeListApproved = ej.DataManager(response.data).executeLocal(ej.Query().where("IsApproved", "equal", true));
                $scope.EmployeeListUnApproved = ej.DataManager(response.data).executeLocal(ej.Query().where("IsApproved", "equal", false));

            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ProcessAll = function (isApprove) {

        var ListModel = $scope.EmployeeListUnApproved;
        if (isApprove == false)
            ListModel = $scope.EmployeeListApproved;

        ListModel = ej.DataManager(ListModel).executeLocal(ej.Query().where("CheckBoxSelect", "equal", true));
        ListModel = ej.DataManager(ListModel).executeLocal(ej.Query().select(["EmpSystemID"]));

        try {

          
            var _selectedBatch = $scope.SelectedArrearProcessBatchId;


            if (baseService.isUndefinedOrNull(_selectedBatch)) {
                throw 'Please select batch';
            }


            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.path + 'ApprovelUnapprove',
                data: { data: ListModel, ArrearProcessBatchId: _selectedBatch, isApprove: isApprove }
            }).then(function successCallback(response) {
                $scope.GetEmployeeInformation();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.clearScreen = function () {
        $scope.EmployeeListApproved = [];
        $scope.EmployeeListUnApproved = [];
        $scope.SelectedArrearProcessBatchId = null;
    }

    $scope.SelectedEmployeeForDeleteion = '';
    $scope.DeleteEmployee = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.SelectedArrearProcessBatchId)) {
                throw 'Please select batch';
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.path + 'DeleteEmployeeArrear',
                data: { EmployeeSystemId: $scope.SelectedEmployeeForDeleteion, ArrearProcessBatchId: $scope.SelectedArrearProcessBatchId }
            }).then(function successCallback(response) {
                $scope.GetEmployeeInformation();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.deleteArrear = function (EmpSystemID) {

        $scope.SelectedEmployeeForDeleteion = EmpSystemID;
        $rootScope.openPopupAngular('confirmDelete');
        

    }

    
    $scope.dataBoundemployeeUnApproved = function (args) {
        if (args.rowIndex == 0) {
            $("#headchkUnApproved").ejCheckBox({"change": headCheckChangeUnApproved });
        }
    }
    function headCheckChangeUnApproved(e) {
        if (!e.isInteraction)
            return;

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#empInfoGridUnApproved").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeListUnApproved.length; i++) {
                $scope.EmployeeListUnApproved[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#empInfoGridUnApproved").data("ejGrid");
        gridObj.refreshContent();

    }

    $scope.dataBoundemployeeApproved = function (args) {
        if (args.rowIndex == 0) {
            $("#headchkApproved").ejCheckBox({ "change": headCheckChangeApproved });
        }
    }
    function headCheckChangeApproved(e) {
        if (!e.isInteraction)
            return;

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#empInfoGridApproved").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeListApproved.length; i++) {
                $scope.EmployeeListApproved[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#empInfoGridApproved").data("ejGrid");
        gridObj.refreshContent();

    }


}