'use strict';
voucherParkController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function voucherParkController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'VoucherPark';
    $scope.Action = 'Save';
    $scope.path = 'Accounts/VoucherPark/';
    $scope.url = "Accounts/VoucherPark";
    $scope.parkUrl = $scope.url + "/parkModeVoucher";
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

    $scope.voucher = {
        Id: null,
        VoucherNo: null
    };


    $scope.VoucherDataList = [];
    $scope.getVoucherData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "getVoucherDataList",
                data: { voucherNo: $scope.voucher.VoucherNo},
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.VoucherDataList = response.data.DATA;

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
   // $scope.getVoucherData();

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId,sourceType) {
        $scope.voucherId = voucherId;
        $scope.sourceType = sourceType;
        $scope.message_confirmation = "Are you sure to Park Mode?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };
    $scope.park = function (vId, vSourceType) {
        $http({
            method: "POST",
            url: $scope.parkUrl,
            data: {
                "voucherId": vId,
                "sourceType": vSourceType
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getVoucherData();
                //$scope.Clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };


    //$scope.invoiceId = null;
    //$scope.confirmDelete = function (invoiceId, voucherId) {
    //    $scope.invoiceId = invoiceId;
    //    $scope.voucherId = voucherId;
    //    $scope.message_delete_confirmation = "Are you sure to Delete?";
    //    angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    //};

    //$scope.delete = function (invoiceId, voucherId) {
    //    $http({
    //        method: "POST",
    //        url: $scope.deleteUrl,
    //        data: {
    //            "invoiceId": invoiceId, "voucherId": voucherId
    //        },
    //        dataType: "JSON"
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, "failure");
    //        }
    //        else {
    //            ShowResult(response.data.Message, "success");
    //            $scope.getData();
    //            $scope.Clear();
    //            $scope.invoiceId = null;
    //            $scope.voucherId = null;
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.status.Message, "failure");
    //    });
    //    return true;
    //};



    //$scope.entityList = [];
    //cboService.getCboEntityByPlant(null, null, "", function (result) {
    //    $scope.entityList = result;
    //});

    //$scope.departmentList = [];
    //cboService.getCboDepartmentByCompanyGroup(null, function (result) {
    //    $scope.departmentList = result;
    //});


    //$scope.refreshTemplateEntityandDepartment = function (args) {
    //    $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEntityAndDepartment });
    //};

    //function CheckBoxSelectAllEntityAndDepartment(e) {

    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;

    //    }

    //    var filtered = $("#GridEntityFixedAssetRegister").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.EntityFixedAssetRegisterList.length; i++) {
    //            $scope.EntityFixedAssetRegisterList[i].isSelected = ChkOrUnchk;
    //        }
    //    }
    //    else {

    //        for (var j = 0; j < filtered.length; j++) {

    //            filtered[j].isSelected = ChkOrUnchk;
    //        }


    //    }
    //    var gridObj = $("#GridEntityFixedAssetRegister").data("ejGrid");
    //    gridObj.refreshContent();
    //};

    //$scope.NewEntityFixedAssetRegisterList = [];
    //$scope.validation = function () {
    //    if (baseService.isUndefinedOrNull($scope.fixedAsset.EntityId)) {
    //        ShowResult('Please select Entity', 'failure');
    //        return true;
    //    }
    //    if (baseService.isUndefinedOrNull($scope.fixedAsset.DepartmentId)) {
    //        ShowResult('Please select Department', 'failure');
    //        return true;
    //    }
    //    $scope.NewEntityFixedAssetRegisterList = [];
    //    for (var i = 0; i < $scope.EntityFixedAssetRegisterList.length; i++) {
    //        if ($scope.EntityFixedAssetRegisterList[i].isSelected == true) {
    //            $scope.NewEntityFixedAssetRegisterList.push($scope.EntityFixedAssetRegisterList[i]);
    //        }
    //    }

    //    if ($scope.NewEntityFixedAssetRegisterList.length == 0) {
    //        //(angular.isUndefinedOrNull(NewMasterLCList)) 
    //        ShowResult('Please select at least one Fixed Assets', 'failure');
    //        return true;
    //    }

    //    else {
    //        return false;

    //    }
    //}

    //$scope.Save = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if (!$scope.validation()) {

    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrl,
    //            data: { 'entityId': $scope.fixedAsset.EntityId, 'departmentId': $scope.fixedAsset.DepartmentId, 'entityFixedAssetList': $scope.NewEntityFixedAssetRegisterList },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, "success");
    //                $scope.GetEntityFixedAssetRegisterData();
    //                //$scope.Clear();
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    }
    //   // $scope.GetEntityFixedAssetRegisterData();
    //}


    


};






