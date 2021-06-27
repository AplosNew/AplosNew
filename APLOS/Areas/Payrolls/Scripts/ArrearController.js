'use strict';
ArrearController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ArrearController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Arrear';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/Arrear/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';

    $scope.FromDate = new Date();
    $scope.ToDate = new Date();


    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];
    $scope.AllDataset = {
        dtActive: [],
        dtNewlyJoined: [],
        dtSND: [],
        dtSNA: [],
        dtEXemp: [],
        dtAttNotProcessed: [],
        dtPresetZero: [],
        dtApprovedSalary: [],
        dtMaternityReturn: [],
        dtSeparated: [],
        dtDifferentStatus: []
    };
    function GetShortColumns(plist) {
        var list = [];
        if (plist != null) {
            for (var i = 0; i < plist.length; i++) {
                if (plist[i].CheckBoxSelect == false)
                    continue;

                var obj = {
                    EmpSystemID: null,
                    IsSelectSlrProc: null
                };
                obj.EmpSystemID = plist[i].EmpSystemID
                obj.IsSelectSlrProc = plist[i].CheckBoxSelect
                list.push(obj);
            }
        }//null
        return list;
    }
    $scope.ProcessAll = function () {
        var data_dtActive = GetShortColumns($scope.EmployeeListTemp);
        $scope.AllDataset.dtActive = data_dtActive;


        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.path + 'ProcessAll',
            data: { FromDate: $scope.FromDate, ToDate: $scope.ToDate, pDescription: $scope.Description, palldataset: $scope.AllDataset }
        }).then(function successCallback(response) {
            $scope.msg = "Successfully Completed !!!";
            ShowResult(response.data.Message, "success");
        }, function errorCallback(response) {
            $scope.btnProcess = true;
            ShowResult(response.status.Message, 'failure');
        });//http
    }
    $scope.GetEmployeeInformation = function () {


        if (baseService.isUndefinedOrNull($scope.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.ToDate)) {
            manualValidation('div_ToDate', true, "To Date is required.");
        }
        else if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
            manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
            manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
        }
        else {
            $scope.searchbyonRoleEmpList = [];
            var parameters = { 'FromDate': $scope.FromDate, 'ToDate': $scope.ToDate };
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

                }
                $scope.EmployeeList = response.data;
                $scope.EmployeeListTemp = response.data;


            });
        }

    };
    $scope.dataBoundemployee = function (args) {
        if (args.rowIndex == 0)
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    };
    $scope.dataBoundemployeed = function (args) {
        if (args.rowIndex == 0)
            $("#headchkd").ejCheckBox({ "change": headCheckChangeemployeed });

    };
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            var filtered = $("#empInfoGrid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {

                    $scope.EmployeeListTemp[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeListTemp[i].EmployeeId == filtered[j].EmployeeId)
                            $scope.EmployeeListTemp[i].CheckBoxSelect = true;
                    }

                }
            }
        }
        else {
            var filtered = $("#empInfoGrid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
                    $scope.EmployeeListTemp[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeListTemp[i].Id == filtered[j].Id)
                            $scope.EmployeeListTemp[i].CheckBoxSelect = false;
                    }

                }
            }
        }

        //var gridObj = $("#empInfoGrid").data("ejGrid");
        //gridObj.refreshContent(true);
        //gridObj.refreshTemplate();
    }
    function headCheckChangeemployeed(e) {
        if (e.model.checkState == "check") {

            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {

                    $scope.EmployeeList[i].isToBeSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].EmployeeId == filtered[j].EmployeeId)
                            $scope.EmployeeList[i].isToBeSelect = true;
                    }

                }
            }
        }
        else {
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    $scope.EmployeeList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].Id == filtered[j].Id)
                            $scope.EmployeeList[i].isToBeSelect = false;
                    }

                }
            }
        }

        //var gridObj = $("#Gridemployee").data("ejGrid");
        //gridObj.refreshContent(true);
        //gridObj.refreshTemplate();
    }

    $scope.showEmployeeFilterScreen = function () {
        try {

            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#empfilterPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.clearManualFilter = function () {
        $scope.isManualFilter = false;
        $scope.EmployeeListTemp = $scope.EmployeeList;
    };
    $scope.Back = function () {
        angular.element(document.querySelector('#empfilterPopUp')).modal('hide');
    };

    $scope.saveemployeedata = function () {
        $scope.EmployeeListTemp = [];
        var row = $filter('filter')($scope.EmployeeList, { 'isToBeSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.EmployeeListTemp = row;
            $scope.isManualFilter = true;
        }
        $scope.Back();
    };


}