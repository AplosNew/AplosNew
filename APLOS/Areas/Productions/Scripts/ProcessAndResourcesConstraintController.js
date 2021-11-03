'use strict';
ProcessAndResourcesConstraintController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProcessAndResourcesConstraintController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Process And Resources Constraint';
  
    $scope.SelectedProcessConstraintsTabList = [];
    $scope.SelectedResourcesConstraintsTabList = [];
  
    $scope.ProcessNameList = [];
    $scope.CapacityUOMList = [];
 
   
    $scope.path = 'Productions/ProcessAndResourcesConstraint/';

    $scope.getListUrl = $scope.path + 'getlist';
   
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlResourcesConstraints = $scope.path + 'SaveResourcesConstraints';
 
    $scope.deleteUrl = $scope.path + 'delete/';
  
  

    baseService.init($scope.getListUrl);


    $scope.searchBy = "UserName"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'ShortName', name: "Short Name" }, { value: 'UserName', name: "User Name" }, { value: 'Code', name: "Code" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'Productions/ProcessAndResourcesConstraint/getprocess/'
    }).then(function successCallback(response) {
        $scope.ProcessNameList = response.data;
    });

    $scope.CapacityUOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.CapacityUOMList = response;
    });


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedProcessConstraintsTabList = response.data;
            ClearFields();
         
        });
    }
        $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        EffectiveDateFrom: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        EffectiveDateTo: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ProcessNameId: null,
        EntityName: null,
        WorkCenterName: null,
        WorkingHoursPerDay: null,
        WorkingDaysPerWeek: null,
        WorkingHoursPerWeek: null,
        WorkingDaysPerMonth: null,
        WorkingHoursPerMonth: null,
        CapacityUOMId: null,
        CapacityPerDay: null,
        CapacityPerWeek: null,
        CapacityPerMonth: null,
        ResponsiblePersonId: null,
        Remarks: null,
        EmployeeStatus: null,
   
};
    $scope.ProcessConstraints = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.ProcessConstraints = Object.assign({}, args.data);
        $scope.setTab(1);
        $scope.Action = 'Update';
        //if (!$rootScope.isCollapsed) {
        //    $rootScope.toggle();
        //}
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
            $scope.SelectedProcessConstraintsTabList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.processconstraintsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ProcessConstraints },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ProcessConstraints = response.data.Data;
                    $scope.getData();
                    //$scope.Action = 'Update';
                    //$scope.Getgrid();
                    //$scope.getCropTypeData($scope.CropMaster.Id);
                    //$scope.getCropProcessData($scope.CropMaster.Id);
                    //$scope.LoadAllSelectedMonthsTab(); 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    $scope.DeleteProcessConstraints = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteProcessConstraints?Id=' + $scope.ProcessConstraintsTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                //$scope.getCropTypeData($scope.CropMaster.Id);
                //ClearFieldsCropType();
                $scope.getData();
            }

        });
    }

    $scope.ConfirmDeleteProcessConstraintsTab = function (Id) {
        $scope.ProcessConstraintsTabId = Id;
        angular.element(document.querySelector("#DeleteProcessConstraintsTabPopUp")).modal("show");
    }

    $scope.Clear = function () {
        ClearFields();
        $scope.getData();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ProcessConstraints = Object.assign({}, $scope.ModelTemp);
      
    }


    ///////////////////////////////////  Responsible Person Pop Up  ////////////////////////////////////////


    // #region ResPerson field

  
    $scope.EmpResPersonList = [];
    $scope.ResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpResPersonList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.ProcessConstraints.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.ProcessConstraints.ResponsiblePersonId = null;
        $scope.ProcessConstraints.ResponsiblePerson = null;
        $scope.ProcessConstraints.EmployeeCode = null;
        $scope.ProcessConstraints.EmployeeStatus = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.ProcessConstraints.EmployeeCode = data.Code;
        $scope.ProcessConstraints.ResponsiblePersonId = data.Id;
        $scope.ProcessConstraints.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region ResPerson

    ///////////////////////////////////  Responsible Person Pop Up End ////////////////////////////////////////

    ///////*********************Tabs*******************************
    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion

 // *************** Resources Constraint Tab *******************
    // #region ddl
    $scope.ProcessList = [];

    $http({
        method: 'GET',
        url: 'Productions/ProcessAndResourcesConstraint/getprocess/',
    }).then(function successCallback(response) {
        $scope.ProcessList = response.data;
    });

    $scope.CapacityList = [];
    cboService.getUoMCbo(function (response) {
        $scope.CapacityList = response;
    });


    // #end region

    $scope.getDataResourceConst = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetListResourceConst",
            //data: { column: $scope.searchBy, value: $scope.search },
            //dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedResourcesConstraintsTabList = response.data;
            ClearFieldsResourceConst();
        });
    }
    $scope.getDataResourceConst();

    $scope.ModelTempResourceConst = {
        Id: null,
        EffectiveDateFrom: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        EffectiveDateTo: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ProcessNameId: null,
        ResourcesName: null,
        EntityName: null,
        WorkCenterName: null,
        WorkingHoursPerDay: null,
        WorkingDaysPerWeek: null,
        WorkingHoursPerWeek: null,
        WorkingDaysPerMonth: null,
        WorkingHoursPerMonth: null,
        CapacityUOMId: null,
        CapacityPerDay: null,
        CapacityPerWeek: null,
        CapacityPerMonth: null,
        ResponsiblePersonId: null,
        Remarks: null,
        EmployeeStatusResCons: null,

    };
    $scope.ResourcesConstraints = Object.assign({}, $scope.ModelTempResourceConst);

    $scope.GetResourcesConstraints = function (args) {

        $scope.ResourcesConstraints = Object.assign({}, args.data);
        $scope.setTab(2);
        $scope.ActionResConst = 'Update';
    };
    $scope.ActionResConst = 'Save';

    $scope.SaveResourcesConstraints = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.resourcesconstraintsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlResourcesConstraints,
                data: { 'data': $scope.ResourcesConstraints },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ResourcesConstraints = response.data.Data;
                    $scope.getDataResourceConst();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    $scope.DeleteResourceConst = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteResourceConst?Id=' + $scope.ResourceConstTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getDataResourceConst();
            }

        });
    }

    $scope.ConfirmDeleteResourcesConstraintsTab = function (Id) {
        $scope.ResourceConstTabId = Id;
        angular.element(document.querySelector("#DeleteResourceConstTabPopUp")).modal("show");
    }

    $scope.ClearResourcesConstraints = function () {
        ClearFieldsResourceConst();
        $scope.getDataResourceConst();
        return true;
    };

    function ClearFieldsResourceConst() {
        $scope.ActionResConst = 'Save';
        $scope.ResourcesConstraints = Object.assign({}, $scope.ModelTemp);

    }


    ///////////////////////////////////  Responsible Person Pop Up  ////////////////////////////////////////


    // #region ResPerson field


    $scope.EmpResPersonListResCons = [];
    $scope.ResponsiblePersonPopUpResCons = function () {
        angular.element(document.querySelector("#EmployeePopUpResPersonResCons")).modal("show");
        $scope.getEmpDetailsDataResCons();

    }
    $scope.getEmpDetailsDataResCons = function () {
        $scope.EmpResPersonListResCons = [];

        $http({
            method: 'POST',
            data: { Id: $scope.ResourcesConstraints.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelectionResCons'
        }).then(function successCallback(response) {
            $scope.EmpResPersonListResCons = response.data;
        });
    }

    $scope.ResponsiblePersonClearResCons = function () {
        $scope.ResourcesConstraints.ResponsiblePersonId = null;
        $scope.ResourcesConstraints.ResponsiblePersonResCons = null;
        $scope.ResourcesConstraints.EmployeeCodeResCons = null;
        $scope.ResourcesConstraints.EmployeeStatusResCons = null;
    };
    $scope.closeEmpResPersonPopUpResCons = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpDataResCons = function (obj) {

        var data = obj.data;
        $scope.ResourcesConstraints.EmployeeCodeResCons = data.Code;
        $scope.ResourcesConstraints.ResponsiblePersonId = data.Id;
        $scope.ResourcesConstraints.ResponsiblePersonResCons = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPersonResCons')).modal('hide');
    };
    // # end region ResPerson
 
    // # end region Resources Constraint
}