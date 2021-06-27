'use strict';
EmployeeServicesRateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeServicesRateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Service Rate';
    $scope.EmployeeServicesRateList = [];
   
    $scope.path = 'EmployeeServices/EmployeeServicesRate/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';
 
    $scope.deleteUrl = $scope.path + 'delete/';
 
    baseService.init($scope.getListUrl);

    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeServicesRateList = response.data;
            ClearFields();
        });
    }
        $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        ServiceNameId: null,
        EffectiveDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        UOMId: null,
        EmployeeServiceCategoryId: null,
        Rate: null,
        Remarks: null,
        UOM: null,
        EmpServiceTypeId: null,
        NonChargeableGLCode: null
};
    $scope.EmployeeServicesRate = Object.assign({}, $scope.ModelTemp);

    $scope.Action = 'Save';

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeServicesRateList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.EmployeeServicesRate.EmployeeServiceCategoryId = $scope.CategoryIdTab;
        if ($scope.EMPSerRateForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.EmployeeServicesRate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.EmployeeServicesRate = response.data.Data;
                    $scope.LoadAllSelectedServiceRateTab();
                    $scope.getData();
      
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Clear = function () {
        ClearFields();
       
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.EmployeeServicesRate = Object.assign({}, $scope.ModelTemp); 
    }

    // ********* Employee Service Rate Tab ************************8

    $scope.EmpServiceRateList = [];
    $scope.ConfirmDetailsPopUp = function (Id) {
        $scope.CategoryIdTab = Id;
        angular.element(document.querySelector('#EMPSerRatePoUp')).modal('show');
        $scope.LoadAllSelectedServiceRateTab();
    }

    $scope.ClosePopUp = function () {
        angular.element(document.querySelector('#EMPSerRatePoUp')).modal('hide');
        $scope.getData();
    }

    $scope.LoadAllSelectedServiceRateTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedServiceRateTab?CategoryId=' + $scope.CategoryIdTab
        }).then(function successCallback(response) {
            $scope.EmpServiceRateList = response.data;
        });
    }

    $scope.DeleteEmpSerRate = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteEmpSerRate?Id=' + $scope.EmpSerRateId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedServiceRateTab();
                $scope.getData();
            }
        });
    }

    $scope.ConfirmDeleteEmpSerRateTab = function (ESRId) {
        $scope.EmpSerRateId = ESRId;
        angular.element(document.querySelector("#DeleteEmpSerRatePopUp")).modal("show");
    }

}