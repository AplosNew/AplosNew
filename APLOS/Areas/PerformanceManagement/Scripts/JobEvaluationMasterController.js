'use strict';
JobEvaluationMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function JobEvaluationMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Job Evaluation Master';
    $scope.JobEvaluationList = [];
    $scope.SelectedJobEvaluationChildTabList = [];
    $scope.JobEvaluationChild2TabList = [];
  
    $scope.EmployeeCategoryList = [];
    $scope.PerformanceAttributeList = [];
   
    $scope.path = 'PerformanceManagement/JobEvaluationMaster/';

    $scope.getListUrl = $scope.path + 'getlist';
   
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlDimensionData = $scope.path + 'SaveDimensionDetails';
    $scope.saveUrlDimensionData2 = $scope.path + 'CreateDimDetails';
 
    $scope.deleteUrl = $scope.path + 'delete/';
  
  

    baseService.init($scope.getListUrl);


    $scope.searchBy = "AttributeStandardName"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'AttributeStandardName', name: "Attribute Standard Name" }, { value: 'AttributeUserName', name: "Attribute UserName" }, { value: 'DimApplicable', name: "Dimension Applicable" }, { value: 'PerformanceAttribute', name: "Performance Attribute" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'PerformanceManagement/JobEvaluationMaster/getemployeecategorylist/',
    }).then(function successCallback(response) {
        $scope.EmployeeCategoryList = response.data;
    });

    $http({
        method: 'GET',
        url: 'PerformanceManagement/JobEvaluationMaster/getperformanceattributelist/',
    }).then(function successCallback(response) {
        $scope.PerformanceAttributeList = response.data;
    });

//    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.JobEvaluationList = response.data;
            ClearFields();
          
        });
    }
        $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        AttributeStandardName: null,
        AttributeUserName: null,
        DimensionApplicable: false,
        PerformanceAttributeId: null,
};
    $scope.JobEvaluation = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.JobEvaluation = Object.assign({}, args.data);
        $scope.EnableDisable();
        if ($scope.JobEvaluation.DimensionApplicable == true) {
            $scope.getJEMChildData();
        }   
        $scope.getJEMChildData2();
        $scope.LoadAllSelectedEmpCatTab();
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
            $scope.JobEvaluationList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.GeneralForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.JobEvaluation },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.JobEvaluation = response.data.Data;
                  
                    $scope.Action = 'Update';
                    $scope.Getgrid();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.JobEvaluation.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.JobEvaluation.Id,
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
        $scope.JobEvaluation = Object.assign({}, $scope.ModelTemp);
        $scope.getJEMChildData();
        $scope.getJEMChildData2();
        $scope.LoadAllSelectedEmpCatTab();
        $scope.EnableDisable();
    }

    // Enable Disable
    $scope.enable = true;
    $scope.disableChild = false;
    $scope.EnableDisable = function () {
        if ($scope.JobEvaluation.DimensionApplicable == true) {
            $scope.enable = false;
            $scope.disableChild = true;
        }

        else {
            $scope.enable = true;
            $scope.disableChild = false;
        }
       
    }

    ///////*********************Tabs*******************************
    // #region Tab
   
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.subtab = 1;
    $scope.settheTab = function (newsubTab) {
        $scope.subtab = newsubTab;
    };
    $scope.Set = function (tabsubNum) {
        return $scope.subtab === tabsubNum;
    };

//    // #endregion

    // Job Evaluation Child 

    $scope.JobEvaluationChildModelTemp = {
        Id: null,
        JobEvaluationMasterId: null,
        Dimension1ControlName: null,
        Dimension1ControlLevel: null,
        Dimension1ControlCode: null,
        Dimension2ControlName: null,
        Dimension2ControlLevel: null,
        Dimension2ControlCode: null,
        Points: null,
        Remarks: null,
     
    };
    $scope.JobEvaluationChild = Object.assign({}, $scope.JobEvaluationChildModelTemp);

    $scope.SaveDimensionData = function () {

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.GeneralForm.$valid) {
            if ($scope.DimChildForm.$valid) {

                $http({
                    method: 'POST',
                    url: $scope.saveUrlDimensionData,
                    data: { 'data': $scope.JobEvaluation, 'JEChildData': $scope.JobEvaluationChild },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.JobEvaluation = response.data.Data;
                        $scope.JobEvaluationChild = response.data.CData;
                        $scope.Getgrid();
                        $scope.getJEMChildData();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        }
       
    };


    function ClearFieldsJEMChildData() {
       
        $scope.JobEvaluationChild = Object.assign({}, $scope.JobEvaluationChildModelTemp);
        

    }

    $scope.getJEMChildData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getJEMChildData?JobEvaluationMasterId=' + $scope.JobEvaluation.Id
        }).then(function successCallback(response) {
            $scope.SelectedJobEvaluationChildTabList = response.data;
            ClearFieldsJEMChildData();
        });
    }


    $scope.DeleteJEMChild = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteJEMChild?Id=' + $scope.JobEvaluationChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getJEMChildData();
            }

        });
    }

    $scope.ConfirmDeleteJobEvaluationChildTab = function (Id) {
        $scope.JobEvaluationChildTabId = Id;
        angular.element(document.querySelector("#DeleteJobEvaluationChildTabPopUp")).modal("show");
    }
    //********** Tab end ***************

    // Job Evaluation Child 2

    $scope.JobEvaluationChild2ModelTemp = {
        Id: null,
        JobEvaluationMasterId: null,
        Category: null,
        Criteria: null,
        Code: null,
        Points: null,
        Remarks: null,

    };
    $scope.JobEvaluationChild2 = Object.assign({}, $scope.JobEvaluationChild2ModelTemp);

    $scope.CreateDimensionData2 = function () {

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.GeneralForm.$valid) {
            if ($scope.DimForm.$valid) {

                $http({
                    method: 'POST',
                    url: $scope.saveUrlDimensionData2,
                    data: { 'data': $scope.JobEvaluation, 'JEMChildDetails': $scope.JobEvaluationChild2 },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.JobEvaluation = response.data.Data;
                        $scope.JobEvaluationChild2 = response.data.CData;
                        $scope.Getgrid();
                        $scope.getJEMChildData2();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        }
      
    };


    function ClearFieldsJEMChildData2() {

        $scope.JobEvaluationChild2 = Object.assign({}, $scope.JobEvaluationChild2ModelTemp);
    }

    $scope.getJEMChildData2 = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getJEMChildDetails?JobEvaluationMasterId=' + $scope.JobEvaluation.Id
        }).then(function successCallback(response) {
            $scope.JobEvaluationChild2TabList = response.data;
            ClearFieldsJEMChildData2();
        });
    }


    $scope.DelJEMChild2 = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelJEMChild2?Id=' + $scope.JEMChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getJEMChildData2();
            }

        });
    }

    $scope.ConfirmDeleteJobEChild2Tab = function (Id) {
        $scope.JEMChildTabId = Id;
        angular.element(document.querySelector("#DelJEMChildTabPopUp")).modal("show");
    }
    //********** Tab end ***************

    // Employee Category Tab

    $scope.EmpCatList = [];
    $scope.showEmpCategoryTabPopUp = function () {
        angular.element(document.querySelector("#EmpCatTabPopUp")).modal("show");
        $scope.getempcatTabData();

    }
    $scope.getempcatTabData = function () {
        $scope.EmpCatList = [];

        $http({
            method: 'POST',
            data: { JobEvaluationMasterId: $scope.JobEvaluation.Id },
            url: $scope.path + 'LoadAllEmpCatForSelection'
        }).then(function successCallback(response) {
            $scope.EmpCatList = response.data;
        });
    }

    $scope.SelectedEmpCategoryTabList = [];
    $scope.LoadAllSelectedEmpCatTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedEmpCatTab?JobEvaluationMasterId=' + $scope.JobEvaluation.Id
        }).then(function successCallback(response) {
            $scope.SelectedEmpCategoryTabList = response.data;
        });
    }


    //Save Function 
    $scope.EmpCategoryTabId = '';
    $scope.SaveEmpCatTab = function () {

        var checkedData = [];
        for (var i = 0; i < $scope.EmpCatList.length; i++) {
            if ($scope.EmpCatList[i].isSelected == true)
                checkedData.push($scope.EmpCatList[i]);
        }

        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one Employee Category';
            }

            $http({
                method: 'POST',
                data: { JobEvaluationMasterId: $scope.JobEvaluation.Id, EmpCatTabData: checkedData },
                url: $scope.path + 'SaveEmpCatTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedEmpCatTab();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }
    $scope.DelECat = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelECat?Id=' + $scope.EmpCategoryTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedEmpCatTab();
            }

        });
    }

    $scope.ConfirmDeleteEmpCategoryTab = function (Id) {
        $scope.EmpCategoryTabId = Id;
        angular.element(document.querySelector("#confirmDelEmpCatPopUp")).modal("show");
    }

    $scope.closeEmpCatTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    // # end region Department Tab

}