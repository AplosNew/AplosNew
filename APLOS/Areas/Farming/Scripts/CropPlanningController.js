'use strict';
CropPlanningController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CropPlanningController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Crop Planning';
    $scope.CropPlanningList = [];
    $scope.SelectedCropPlanningChildTabList = [];
  
    $scope.ICSMasterList = [];
   
    $scope.path = 'Farming/CropPlanning/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlCropPlanningChild = $scope.path + 'SaveCropPlanningChild';
 
    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl);


    $scope.searchBy = "UserName"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Year', name: "Year" }, { value: 'Season', name: "Season" }, { value: 'UserName', name: "User Name" }, { value: 'Name', name: "ICS Master" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'Farming/CropPlanning/geticsmaster/',
    }).then(function successCallback(response) {
        $scope.ICSMasterList = response.data;
    });


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CropPlanningList = response.data;
            ClearFields();
            
        });
    }
        $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Year: null,
        Season: null,
        StartDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        UserName: null,
        CloseDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ICSMasterID: null,
        Active: true,
};
    $scope.CropPlanning = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.CropPlanning = Object.assign({}, args.data);
        $scope.CropPlanning.StartDate = $scope.CropPlanning.CPStartDate;
        $scope.CropPlanning.CloseDate = $scope.CropPlanning.CPCloseDate;
        $scope.getCropPlanningChildData($scope.CropPlanning.Id);
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
            $scope.CropPlanningList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.CropPlanning },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.CropPlanning = response.data.Data;
                  
                    $scope.Action = 'Update';
                    $scope.Getgrid();
                    $scope.getCropPlanningChildData($scope.CropPlanning.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.CropPlanning.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.CropPlanning.Id,
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
        $scope.CropPlanning = Object.assign({}, $scope.ModelTemp);
        $scope.getCropPlanningChildData($scope.CropPlanning.Id);
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

 // *************** Crop Planning Child Tab *******************

    $scope.CropList = [];
    $scope.CropTypeList = [];
    $scope.FarmerList = [];
    $scope.FarmerPlotList = [];
    $scope.VillageList = [];
    $scope.FarmerCategoryList = [];

    $http({
        method: 'GET',
        url: 'Farming/CropPlanning/getcrop/',
    }).then(function successCallback(response) {
        $scope.CropList = response.data;
        });

    $scope.GetCropType = function () {
        $scope.CropTypeList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getcroptype?CropId=' + $scope.cropplanningchild.CropId
        }).then(function successCallback(response) {
            $scope.CropTypeList = response.data;
        });
    }
    
    $scope.GetLandStatus = function () {
        $scope.cropplanningchild.LandStatus = null;
        $scope.cropplanningchild.LandStatusId = null;
        $http({
            method: 'GET',
            url: $scope.path + 'getlandstatus?CropTypeId=' + $scope.cropplanningchild.CropTypeId + '&CropId=' + $scope.cropplanningchild.CropId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.cropplanningchild.LandStatus = response.data[0].LandStatus;
                $scope.cropplanningchild.LandStatusId = response.data[0].Id;
                $scope.cropplanningchild.AverageOutput = response.data[0].AverageOutput;
                $scope.GetVillage();
                $scope.GetFarmer();
            }
            });
    }
   

    $scope.GetFarmer = function () {
        $scope.FarmerList = [];
        $http({
            method: 'POST',
            data: { ICSMasterID: $scope.CropPlanning.ICSMasterID, LandStatusId: $scope.cropplanningchild.LandStatusId, VillageId: $scope.cropplanningchild.VillageId },
            url: $scope.path + 'getfarmer'
        }).then(function successCallback(response) {
            $scope.FarmerList = response.data;
        });
    }

    $scope.GetFarmerFather = function () {
        $scope.FarmerFatherHusbandNameList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getfarmerfather?FarmerID=' + $scope.cropplanningchild.FarmerId
        }).then(function successCallback(response) {
            $scope.FarmerFatherHusbandNameList = response.data;
        });
    }
    $scope.GetFarmerRegistrationId = function () {
        $scope.FarmerRegistrationList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getfarmerregistrationid?FarmerFatherID=' + $scope.cropplanningchild.FarmerFatherHusbandNameId
        }).then(function successCallback(response) {
            $scope.FarmerRegistrationList = response.data;
            $scope.cropplanningchild.TotalArea = response.data[0].TotalArea;
        });
    }

    $scope.GetVillage = function () {
        $scope.VillageList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getvillage?ICSMasterID=' + $scope.CropPlanning.ICSMasterID
        }).then(function successCallback(response) {
            $scope.VillageList = response.data;
        });
    }
   

    $scope.GetFarmerPlot = function () {
        $scope.FarmerPlotList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getfarmerplot?FarmerMasterId=' + $scope.cropplanningchild.FarmerId
        }).then(function successCallback(response) {
            $scope.FarmerPlotList = response.data;
        });
    }

    $scope.GetPlotStatus = function () {
        $scope.cropplanningchild.PlotStatus = null;
        $scope.cropplanningchild.PlotStatusId = null;
        $http({
            method: 'GET',
            url: $scope.path + 'getplotstatus?FarmerPlotId=' + $scope.cropplanningchild.FarmerPlotId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.cropplanningchild.PlotStatus = response.data[0].PlotStatus;
                $scope.cropplanningchild.PlotStatusId = response.data[0].Id;
                $scope.cropplanningchild.PlotArea = response.data[0].PlotArea;
            }
        });
    }

    $scope.GetValue = function () {
        $scope.cropplanningchild.Value = null;
        $http({
            method: 'POST',
            data: { AverageOutput: $scope.cropplanningchild.AverageOutput, CropArea: $scope.cropplanningchild.CropArea, ProductivityIndex: $scope.cropplanningchild.ProductivityIndex },
            url: $scope.path + 'getplanquantity'
        }).then(function successCallback(response) {
            $scope.cropplanningchild.PlanQuantity = response.data;
        });
    }

    $http({
        method: 'GET',
        url: 'Farming/CropPlanning/getfarmingcategory/',
    }).then(function successCallback(response) {
        $scope.FarmerCategoryList = response.data;
    });

    $scope.cropplanningchildModelTemp = {
        Id: null,
        CropPlanningMasterId: null,
        CropId: null,
        CropTypeId: null,
        FarmerId: null,
        FarmerPlotId: null,
        CropArea: null,
        FarmerCategoryId: null,
        Active: true,
        Remarks: null,
        LandStatus: null,
        LandStatusId: null,
        ProductivityIndex: 1,
        VillageId: null,
        PlotStatus: null,
        PlotStatusId: null,
        AverageOutput: null,
        AverageOutputId: null,
        PlanQuantity: null,
        FarmerFatherHusbandNameId: null,
        FarmerRegId: null,
        TotalArea: null,
        PlotArea: null

    };
    $scope.cropplanningchild = Object.assign({}, $scope.cropplanningchildModelTemp);

    $scope.GetCropPlanningChild = function (args) {
        $scope.cropplanningchild = Object.assign({}, args.data);
        $scope.GetCropType();
        $scope.GetLandStatus();
        $scope.GetFarmerPlot();
        $scope.GetPlotStatus();
        $scope.GetValue();
      //  $scope.GetVillage($scope.CropPlanning.ICSMasterID);
    };

    $scope.SaveCropPlanningChild = function () {
        $scope.cropplanningchild.CropPlanningMasterId = $scope.CropPlanning.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.cropplanningchildForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlCropPlanningChild,
                data: { 'data': $scope.cropplanningchild },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.cropplanningchild = response.data.Data;
                    ClearFieldsCropPlanningChild();
                    $scope.getCropPlanningChildData($scope.CropPlanning.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.ClearCropPlanningChild = function () {
        ClearFieldsCropPlanningChild();
        return true;
    };


    function ClearFieldsCropPlanningChild() {
      
        $scope.cropplanningchild = Object.assign({}, $scope.cropplanningchildModelTemp);
        $scope.CropTypeList = [];
        $scope.FarmerList = [];
        $scope.FarmerPlotList = [];
        $scope.VillageList = [];
        $scope.cropplanningchild.LandStatus = null;
        $scope.cropplanningchild.LandStatusId = null;
        $scope.cropplanningchild.PlotStatus = null;
        $scope.cropplanningchild.PlotStatusId = null;
        $scope.cropplanningchild.AverageOutput = null;
        $scope.cropplanningchild.AverageOutputId = null;
        $scope.cropplanningchild.Value = null;
        $scope.getCropPlanningChildData($scope.CropPlanning.Id);
    }

    $scope.getCropPlanningChildData = function (CropPlanningMasterId) {

        $http({
            method: 'GET',
            url: $scope.path + 'GetListCropPlanningChild?CropPlanningMasterId=' + CropPlanningMasterId
        }).then(function successCallback(response) {
            $scope.SelectedCropPlanningChildTabList = response.data;

            var gridObj = $("#GridCropPlanningChildTab").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        
        });
    }


    $scope.DeleteCropPlanningChild = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteCropPlanningChild?Id=' + $scope.CropPlanningChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getCropPlanningChildData($scope.CropPlanning.Id);
                ClearFieldsCropPlanningChild();
            }

        });
    }

    $scope.ConfirmDeleteCropPlanningChildTab = function (Id) {
        $scope.CropPlanningChildTabId = Id;
        angular.element(document.querySelector("#DeleteCropPlanningChildTabPopUp")).modal("show");
    }

    ////********** Tab end ***************

}