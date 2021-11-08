'use strict';
MaterialReconcilationReportController.$inject = ['$window',"addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MaterialReconcilationReportController($window, addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {

    $scope.ContractNoList = [];

    $scope.path = 'Outsourcing/MaterialReconcilationReport/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';

    baseService.init($scope.getListUrl);

    $scope.searchBy = "EmployeeCode"; $scope.search = "";


    $scope.searchByList = [{ value: 'EmployeeCode', name: "Employee Code" }, { value: 'OThour', name: "OT hour" }];

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
            url: $scope.path + 'LoadAllTransConForSelection?Type=' + $scope.MatReconcilation.Type
        }).then(function successCallback(response) {
            $scope.ContractList = response.data;
        });
    }

    $scope.ContractDataClear = function () {
        $scope.MatReconcilation.ContractId = null;
        $scope.MatReconcilation.Party = null;
        $scope.MatReconcilation.ContractDate = null;
        $scope.MatReconcilation.ContractClosingDate = null;
    };

    $scope.closeContractPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setContractData = function (obj) {
        var data = obj.data;
        $scope.MatReconcilation.ContractId = data.Id;
        $scope.MatReconcilation.Party = data.Party;
        $scope.MatReconcilation.ContractDate = data.ContractDate;
        $scope.MatReconcilation.ContractClosingDate = data.ContractCloseDate;
        $scope.disable = true;
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    };
   //  # end region

    $scope.disable = false;

    // #end region

    //var d = new Date();

    //var hh = d.getHours();
    //var mm = d.getMinutes();
    //mm = (mm < 10 ? '0' + mm : mm);
    //var ss = d.getSeconds()

    ////   var _Time = hh + ":" + mm + ":" + ss;
    //var _Time = hh + ":" + mm;

    $scope.MatReconcilationModelTemp = {
        ContractNo: null,
        ContractClosingDate: null,
        ContractDate: null,
        Type: null,   
    };
    $scope.MatReconcilation = Object.assign({}, $scope.MatReconcilationModelTemp);

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() { 
        $scope.MatReconcilation = Object.assign({}, $scope.MatReconcilationModelTemp);
    }

    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.Generate = function () {
        if ($scope.MatReconcilation.Type == "ValueAdded") {
            $http({
                method: 'POST',
                url: $scope.path + 'GetMatReconcilationReport',
                data: {
                    ContractId: $scope.MatReconcilation.ContractId
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

        if ($scope.MatReconcilation.Type == "Transformation") {
            $http({
                method: 'POST',
                url: $scope.path + 'GetMatReconcilationTransformationReport',
                data: {
                    ContractId: $scope.MatReconcilation.ContractId
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

}