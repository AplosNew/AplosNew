'use strict';
FurniturePolicyController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FurniturePolicyController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Furniture Policy';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/FurniturePolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.redirectTab = function () {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.tabForm3.$invalid) {
            $scope.setTab(3);
        }
        else if ($scope.tabForm4.$invalid) {
            $scope.setTab(4);
        }
    };

    //All Lists Are Here
    $scope.FurnitureMasterList = [];
    $scope.FurnitureGridList = [];
    $scope.DesignationMasterList = [];
    $scope.DesignationGridList = [];
    $scope.SelectedList = [];
    $scope.dateNow = new Date().toLocaleString('en-US', { timeZone: 'UTC' });

    $scope.ModelTemp = {
        Id: null,
        ShortName:null,
        StandardName: null,
        UserName: null,
        EffectiveDate: $scope.dateNow ,
        ResponsiblePerson: null,
        ActiveInactive: true,
        EmployeeCategory: null,
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    
    $scope.getFurnitureMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getFurnitureMaster",
            dataType: 'JSON',

        }).then(function successCallback(response) {
            $scope.FurnitureMasterList = response.data;
        })
    }
    $scope.getFurnitureMaster();

    $scope.getDesignationMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getDesignationMaster",
            dataType: 'JSON',

        }).then(function successCallback(response) {
            $scope.DesignationMasterList = response.data;
        })
    }
    $scope.getDesignationMaster();

    $scope.EmployeeCategoryList = [];
    $scope.getEmployeeCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployeeCategory",
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
           // $scope.viewFurniturePolicyGrids();
        })
        
    }
    $scope.getEmployeeCategory();

    $scope.getFurnitureGridView = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getFurnitureGridView",
            
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.FurnitureGridList = response.data;
        })
    }

    $scope.getDesignationGridView = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getDesignationGridView",
            data: {
                'employeeCategoryId': $scope.ModelNew.EmployeeCategory
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.DesignationGridList = response.data;
        })
    }

    $scope.viewFurniturePolicyGrids = function () {
        if (baseService.isUndefinedOrNull($scope.ModelNew.StandardName)) {
            ShowResult('Standard Name is Required.', 'failure');
            throw 'Invalid Request';
        }

        if (baseService.isUndefinedOrNull($scope.ModelNew.UserName)) {
            ShowResult('User Name is Required.', 'failure');
        }

        if (baseService.isUndefinedOrNull($scope.ModelNew.EffectiveDate)) {
            ShowResult('Effective Date is Required.', 'failure');
        }

        $scope.getFurnitureGridView();
        $scope.getDesignationGridView();
    }

    

   //=============================================Furniture Master================================

    $scope.FurnitureIdList = [];
    $scope.QuantityList = [];
    $scope.chkdFurnitureList = [];
    $scope.chkFurniture_FilteredData = function () {
        var ob = { FurnitureMasterId: null };
        var qt = { Quantity: null };
        for (var i = 0; i < $scope.FurnitureGridList.length; i++) {
            if ($scope.FurnitureGridList[i].IsSelectSlrProc === true) {
                ob.FurnitureMasterId = $scope.FurnitureGridList[i].Id;
                qt.Quantity = $scope.FurnitureGridList[i].Quantity;
                $scope.chkdFurnitureList.push($scope.FurnitureGridList[i])
                
                $scope.FurnitureIdList.push(ob);
                $scope.QuantityList.push(qt);
                var ob = { FurnitureMasterId: null };
                var qt = { Quantity: null };
            }
        }
    }
   
    //=============================================Furniture Master================================

    //=============================================Designation================================
    $scope.DesignationIdList = [];
    $scope.chkdDesignationList = [];
    $scope.chkDesignation_FilteredData = function () {
        var ob = { DesignationMasterId:null};
        for (var i = 0; i < $scope.DesignationGridList.length; i++) {
            if ($scope.DesignationGridList[i].IsSelectSlrProc === true) {
                ob.DesignationMasterId = $scope.DesignationGridList[i].Id;
                $scope.chkdDesignationList.push($scope.DesignationGridList[i])
                $scope.DesignationIdList.push(ob);
                var ob = { DesignationMasterId: null };
            }
        }
    }
  
    //=============================================Designation================================
    //==============================================SAVE==============================================
    $scope.Save = function () {
        $scope.chkDesignation_FilteredData();
        $scope.chkFurniture_FilteredData();
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.path + "Save",
            data: {
                'data': $scope.ModelNew,
                'responsiblePerson': $scope.EmployeeId,
            },
            dataType: 'JSON',

        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Msg, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ModelNew.Id = response.data.Data.Id;
                $scope.SaveTabA();
                $scope.SaveTabB();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Msg, 'failure');
        }

    };

    $scope.SaveTabA = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.path + "SaveTabA",
            data: {
                'childA': $scope.DesignationGridList,
                'headerId': $scope.ModelNew.Id,
                'designationmasterId': $scope.DesignationIdList,
            },
            dataType: 'JSON',

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Msg, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');


            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Msg, 'failure');
        }

    };

    $scope.SaveTabB = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.path + "SaveTabB",
            data: {
                'childB': $scope.FurnitureGridList,
                'headerId': $scope.ModelNew.Id,
                'furnituremasterId': $scope.FurnitureIdList,
                'quantity': $scope.QuantityList,

            },
            dataType: 'JSON',

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Msg, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getFurnitureGridView();
                $scope.getDesignationGridView();
                ClearFields();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Msg, 'failure');
        }

    };

    //=================================================SAVE===========================================
    //=======================================EMPLOYEE POP UP======================================
    $scope.OpeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('show');
        $scope.getEmployee();
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
       
    }
    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });

    }

    $scope.EmployeeId = null;
    $scope.Employee = null;
    $scope.doubleEmploye = function (e) {
        $scope.EmployeeId = e.data.SystemId;
        $scope.Employee = e.data.EmployeeName;
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
        /*$scope.viewFurniturePolicyGrids();*/
    }
    //=======================================EMPLOYEE POP UP======================================

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields() {
        $scope.Employee = null;

        var ob = { FurnitureMasterId: null };
        var qt = { Quantity: null };
        for (var i = 0; i < $scope.FurnitureGridList.length; i++) {
            if ($scope.FurnitureGridList[i].IsSelectSlrProc === true) {
                ob.FurnitureMasterId = false;
                qt.Quantity = false;
                $scope.chkdFurnitureList.pop($scope.FurnitureGridList[i])

                $scope.FurnitureIdList.push(ob);
                $scope.QuantityList.push(qt);
                var ob = { FurnitureMasterId: null };
                var qt = { Quantity: null };
            }
        }

        $scope.ModelNew = {
            
            Id: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            EffectiveDate: $scope.dateNow,
            ResponsiblePerson: null,
            ActiveInactive: true,
            EmployeeCategory: null,
        };

    }
    
}