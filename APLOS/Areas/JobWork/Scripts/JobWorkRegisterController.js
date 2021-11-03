'use strict';
JobWorkRegisterController.$inject = ['$window',"addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function JobWorkRegisterController($window, addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {

    $scope.ContractNoList = [];

    $scope.path = 'JobWork/JobWorkRegister/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';

    baseService.init($scope.getListUrl);

    $scope.searchBy = "EmployeeCode"; $scope.search = "";


    $scope.searchByList = [{ value: 'EmployeeCode', name: "Employee Code" }, { value: 'OThour', name: "OT hour" }];

 //   $scope.disable = false;

    // #end region

    //var d = new Date();

    //var hh = d.getHours();
    //var mm = d.getMinutes();
    //mm = (mm < 10 ? '0' + mm : mm);
    //var ss = d.getSeconds()

    ////   var _Time = hh + ":" + mm + ":" + ss;
    //var _Time = hh + ":" + mm;

    $scope.RegisterModelTemp = {
        FromDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ToDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        PartyVendorId:null,
        PartyVendorCode: null,
        PartyVendorName: null,
        ContractId: null,
        ContractDate: null,
        POType:null,
    };
    $scope.Register = Object.assign({}, $scope.RegisterModelTemp);

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() { 
        $scope.Register = Object.assign({}, $scope.RegisterModelTemp);
    }

    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.Generate = function () {
        if (baseService.isUndefinedOrNull($scope.Register.POType)) {
            ShowResult("Please Select PO Type");
            return false;
        }

        if ($scope.Register.POType == "Transformation") {
            $http({
                method: 'POST',
                url: $scope.path + 'GetTransformationRegisterReport',
                data: {
                    FromDate: $scope.Register.FromDate,
                    ToDate: $scope.Register.ToDate,
                    PartyVendorId: $scope.Register.PartyVendorId,
                    ContractId: $scope.Register.ContractId
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
        else {
            $http({
                method: 'POST',
                url: $scope.path + 'GetValueAddedRegisterReport',
                data: {
                    FromDate: $scope.Register.FromDate,
                    ToDate: $scope.Register.ToDate,
                    PartyVendorId: $scope.Register.PartyVendorId,
                    ContractId: $scope.Register.ContractId
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

   //   #region Vendor/ Party field

    $scope.PartyVendorList = [];
    $scope.PartyVendorPopUp = function () {
        angular.element(document.querySelector("#PartyVendorPopUp")).modal("show");
        $scope.getPartyVendordata();

    }
    $scope.getPartyVendordata = function () {
        $scope.PartyVendorList = [];
        $http({
            method: 'GET',
         //   data: { Id: $scope.TransConfirmationIssue.Id },
            url: $scope.path + 'LoadAllPartyVendorForSelection'
        }).then(function successCallback(response) {
            $scope.PartyVendorList = response.data;
        });
    }

    $scope.PartyVendorClear = function () {
        $scope.Register.PartyVendorId = null;
        $scope.Register.PartyVendorName = null;
        $scope.Register.PartyVendorCode = null;
    };
    $scope.closePartyVendorPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setPartyVendorData = function (obj) {
        var data = obj.data;
        $scope.Register.PartyVendorCode = data.Code;
        $scope.Register.PartyVendorId = data.Id;
        $scope.Register.PartyVendorName = data.UserName;
        angular.element(document.querySelector('#PartyVendorPopUp')).modal('hide');
    };
   //  # end region

    //   #region Contract field

    $scope.ContractList = [];
    $scope.ContractPopUpShow = function () {
        angular.element(document.querySelector("#ContractPopUp")).modal("show");
        $scope.getCondata();

    }
    $scope.getCondata = function () {
        $scope.ContractList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllPOForSelection?JWPOPartyId=' + $scope.Register.PartyVendorId + '&POType=' + $scope.Register.POType
        }).then(function successCallback(response) {
            $scope.ContractList = response.data;
        });
    }

    $scope.ContractDataClear = function () {
        $scope.Register.ContractId = null;
        $scope.Register.ContractDate = null;
    };

    $scope.closeContractPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setContractData = function (obj) {
        var data = obj.data;
        $scope.Register.ContractId = data.Id;
        $scope.Register.ContractDate = data.ContractDate;
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    };
   //  # end region

}