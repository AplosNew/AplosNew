'use strict';
voucherParkController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function voucherParkController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'VoucherPark';
    $scope.Action = 'Save';
    $scope.path = 'Accounts/VoucherPark/';
    $scope.saveUrl = $scope.path + 'create';
    var dt = new Date();

    //$scope.reportParameters = {
    //    FromDate: $filter("dateFiltering")(new Date(dt.setDate(dt.getDate() - 10))), //$filter("dateFiltering")(Date.now()) - 10,
    //    ToDate: $filter("dateFiltering")(Date.now()),
    //    TransactionType: 'LoanTaken',
    //    ReportFormat: 'Excel'
    //    VoucherId: null
    //    IsOrderSpecific: true,
    //   FromDate: $filter('dateFiltering')(Date.now()),
    //};

    $scope.fixedAsset = {
        EntityId: null,
        DepartmentId: null
    };


    $scope.EntityFixedAssetRegisterList = [];
    $scope.GetEntityFixedAssetRegisterData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetEntityFixedAssetRegisterDataList",
                //data: { FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {

                //if (response.data.Error == false) {
                //    for (var i = 0; i < response.data.DATA.length; i++) {
                //      response.data.DATA[i].MasterLCDate = new Date(response.data.DATA[i].MasterLCDate);
                //    }
                //    $scope.EntityFixedAssetRegister = response.data.DATA;
                //}
                //else {
                //    ShowResult(response.data.Message, 'failure');
                //}


                $scope.EntityFixedAssetRegisterList = response.data.DATA;

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    $scope.GetEntityFixedAssetRegisterData();

    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });


    $scope.refreshTemplateEntityandDepartment = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEntityAndDepartment });
    };

    function CheckBoxSelectAllEntityAndDepartment(e) {

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridEntityFixedAssetRegister").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EntityFixedAssetRegisterList.length; i++) {
                $scope.EntityFixedAssetRegisterList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridEntityFixedAssetRegister").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.NewEntityFixedAssetRegisterList = [];
    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.fixedAsset.EntityId)) {
            ShowResult('Please select Entity', 'failure');
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.fixedAsset.DepartmentId)) {
            ShowResult('Please select Department', 'failure');
            return true;
        }
        $scope.NewEntityFixedAssetRegisterList = [];
        for (var i = 0; i < $scope.EntityFixedAssetRegisterList.length; i++) {
            if ($scope.EntityFixedAssetRegisterList[i].isSelected == true) {
                $scope.NewEntityFixedAssetRegisterList.push($scope.EntityFixedAssetRegisterList[i]);
            }
        }

        if ($scope.NewEntityFixedAssetRegisterList.length == 0) {
            //(angular.isUndefinedOrNull(NewMasterLCList)) 
            ShowResult('Please select at least one Fixed Assets', 'failure');
            return true;
        }

        else {
            return false;

        }
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if (!$scope.validation()) {

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'entityId': $scope.fixedAsset.EntityId, 'departmentId': $scope.fixedAsset.DepartmentId, 'entityFixedAssetList': $scope.NewEntityFixedAssetRegisterList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.GetEntityFixedAssetRegisterData();
                    //$scope.Clear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
       // $scope.GetEntityFixedAssetRegisterData();
    }


    //$scope.InvoiceSummaryReport = function () {

    //    try {
    //        var NewMasterLCList = [];
    //        for (var i = 0; i < $scope.MasterLCList.length; i++) {
    //            if ($scope.MasterLCList[i].isSelected == true) {

    //                if (NewMasterLCList, $scope.MasterLCList[i].PartyId) {
    //                    NewMasterLCList.push($scope.MasterLCList[i].PartyId);
    //                }
    //            }
    //        }
    //        if (NewMasterLCList.length == 0) {
    //            //(angular.isUndefinedOrNull(NewMasterLCList)) 
    //            ShowResult('Please select at least one Party', 'failure');
    //            //throw 'Please enter to date';

    //        } else {
    //            var file_src = $scope.path + "PartyPaymentStatusReport?MasterLCList=" + NewMasterLCList;
    //            $rootScope.report(file_src);
    //        }


    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}


};






