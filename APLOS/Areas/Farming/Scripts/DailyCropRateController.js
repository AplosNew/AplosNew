'use strict';
DailyCropRateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DailyCropRateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Daily Crop Rate';
    $scope.LocationList = [];
    $scope.DailyCropRateList = [];
    
    $scope.CropList = [];
    $scope.CropTypeList = [];
    $scope.PlanMaximumRateList = [];
   
    $scope.path = 'Farming/DailyCropRate/';

    $scope.getListUrl = $scope.path + 'getlist';
  

    $scope.saveUrl = $scope.path + 'create';
    
 
    $scope.deleteUrl = $scope.path + 'delete/';
  
  

    baseService.init($scope.getListUrl);


    $scope.searchBy = "Crop"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Location', name: "Location" }, { value: 'Crop', name: "Crop" }, { value: 'CropType', name: "CropType" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'Farming/DailyCropRate/getlocation/',
    }).then(function successCallback(response) {
        $scope.LocationList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Farming/DailyCropRate/getcrop/',
    }).then(function successCallback(response) {
        $scope.CropList = response.data;
    });

    $scope.GetCropType = function () {
        $scope.CropTypeList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getcroptype?CropId=' + $scope.DailyCropRate.CropId
        }).then(function successCallback(response) {
            $scope.CropTypeList = response.data;
        });
    }

    $scope.GetPlanMaximumRate = function () {
        //$scope.DailyCropRate.PlanMaximumRate = null;
        //$scope.DailyCropRate.PlanMaximumRateId = null;
        $scope.PlanMaximumRateList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getplanmaximumrate?CropTypeId=' + $scope.DailyCropRate.CropTypeId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                //$scope.DailyCropRate.PlanMaximumRate = response.data[0].MaximumRate;
                //$scope.DailyCropRate.PlanMaximumRateId = response.data[0].Id;
                $scope.PlanMaximumRateList = response.data;
            }
        });
    }

  

    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DailyCropRateList = response.data;
            ClearFields();
           
        });
    }
    $scope.getData();


    $scope.ModelTemp = {
        Id: null,
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        LocationId: null,
        CropId: null,
        CropTypeId: null,
        TaregtRate: null,
        MaximumRate: null,
        ApproveById: null,
        Remarks: null,
        EmployeeStatus: null,
        PlanMaximumRateId: null,
        PlanMaximumRate: null,
};
    $scope.DailyCropRate = Object.assign({}, $scope.ModelTemp);

    //$scope.getDateListData = function () {
    //    $scope.DailyCropRateList2 = [];
    //    $http({
    //        method: 'POST',
    //        data: { Date: $scope.DailyCropRate.Date },
    //        url: $scope.path + 'getDateListData'
    //    }).then(function successCallback(response) {
    //        $scope.DailyCropRateList2 = response.data;
    //    });
    //}
    //$scope.DailyCropRateList2 = [];
   
    

    $scope.Get = function (args) {

        $scope.DailyCropRate = Object.assign({}, args.data);
        $scope.GetCropType();
        $scope.GetPlanMaximumRate();
 //       $scope.getDateListData($scope.DailyCropRate.Date);
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
            $scope.DailyCropRateList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.DailyCropRate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.DailyCropRate = response.data.Data;
                  
                    $scope.Action = 'Update';
                    $scope.getData();
                    //$scope.getCropTypeData($scope.CropMaster.Id);
                    //$scope.getCropProcessData($scope.CropMaster.Id);
                    //$scope.LoadAllSelectedMonthsTab(); 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.DailyCropRate.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.DailyCropRate.Id,
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
        $scope.DailyCropRate = Object.assign({}, $scope.ModelTemp);
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
            data: { Id: $scope.DailyCropRate.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.DailyCropRate.ApproveById = null;
        $scope.DailyCropRate.ResponsiblePerson = null;
        $scope.DailyCropRate.EmployeeCode = null;
        $scope.DailyCropRate.EmployeeStatus = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.DailyCropRate.EmployeeCode = data.Code;
        $scope.DailyCropRate.ApproveById = data.Id;
        $scope.DailyCropRate.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region ResPerson

    ///////////////////////////////////  Responsible Person Pop Up End ////////////////////////////////////////
  

}