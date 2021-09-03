'use strict';
EmployeeServiceTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeServiceTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Service Type';
    $scope.ServiceTypeList = [];
    $scope.SelectedServiceCategoryTabList = [];
  
    $scope.UOMList = [];
    $scope.SalaryHeadList = [];
    $scope.path = 'EmployeeServices/EmployeeServiceType/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlServiceCategory = $scope.path + 'SaveServiceCategory';
 
    $scope.deleteUrl = $scope.path + 'delete/';
 
    baseService.init($scope.getListUrl);

    $scope.searchBy = "Service"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Service', name: "Service" }, { value: 'Form', name: "Form" }, { value: 'UOM', name: "UOM" }, { value: 'SalaryHead', name: "Salary Head" }];
 

    // #region ddl

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.UOMList = response;
    });

    $http({
        method: 'GET',
        url: 'EmployeeServices/EmployeeServiceType/getsalaryhead/'
    }).then(function successCallback(response) {
        $scope.SalaryHeadList = response.data;
    });


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ServiceTypeList = response.data;
            ClearFields();
        });
    }
        $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Service: null,
        Form: null,
        UOMId: null,
        SalaryHeadId: null,
        SelectionMode:null,
};
    $scope.ServiceType = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.ServiceType = Object.assign({}, args.data);
        $scope.ServiceType.SelectionMode = $scope.ServiceType.SelectionMode;
        $scope.getServiceCategoryData($scope.ServiceType.Id);
        $scope.setTab(1);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();            
        }
    };
    $scope.Action = 'Save';

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ServiceTypeList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ServiceType },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ServiceType = response.data.Data;     
                    $scope.Action = 'Update';
                    $scope.Getgrid();
                    $scope.getServiceCategoryData($scope.ServiceType.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ServiceType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ServiceType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
       
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ServiceType = Object.assign({}, $scope.ModelTemp);
        $scope.getServiceCategoryData($scope.ServiceType.Id);
        ClearFieldsServiceCategory();
        $scope.setTab();
      
      
    }

    ///////*********************Tabs*******************************
    // #region Tab
    //  $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion

 // *************** Service Category Tab *******************

    $scope.ServiceCategoryModelTemp = {
        Id: null,
        EmpServiceTypeId: null,
        Category: null,
     
    };
    $scope.ServiceCategory = Object.assign({}, $scope.ServiceCategoryModelTemp);

    $scope.SaveServiceCategory = function () {
        $scope.ServiceCategory.EmpServiceTypeId = $scope.ServiceType.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ServiceCategoryForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlServiceCategory,
                data: { 'data': $scope.ServiceCategory },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ServiceCategory = response.data.Data;                  
                    $scope.getServiceCategoryData($scope.ServiceType.Id);
                    ClearFieldsServiceCategoryFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    function ClearFieldsServiceCategoryFields() {
   
        $scope.ServiceCategory.Category = null;
        $scope.ServiceCategory.Id = null;
    }


    function ClearFieldsServiceCategory() {
       
        $scope.ServiceCategory = Object.assign({}, $scope.ServiceCategoryModelTemp);
    }

    $scope.getServiceCategoryData = function (EmpServiceTypeId) {

        $http({
            method: 'GET',
            url: $scope.path + 'GetListServiceCategory?EmpServiceTypeId=' + EmpServiceTypeId
        }).then(function successCallback(response) {
            $scope.SelectedServiceCategoryTabList = response.data;
        });
    }


    $scope.DeleteServiceCategory = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteServiceCategory?Id=' + $scope.ServiceCategoryTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getServiceCategoryData($scope.ServiceType.Id);
            }

        });
    }

    $scope.ConfirmDeleteServiceCategoryTab = function (Id) {
        $scope.ServiceCategoryTabId = Id;
        angular.element(document.querySelector("#DeleteServiceCategoryTabPopUp")).modal("show");
    }
    //********** Tab end ***************

}