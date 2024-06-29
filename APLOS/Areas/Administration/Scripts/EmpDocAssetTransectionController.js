'use strict';
EmpDocAssetTransectionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function EmpDocAssetTransectionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Emp Doc & Asset Transaction";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.orderCategories = [];
    $scope.path = 'Administration/EmpDocAssetTransection/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ModelTemp = {
        Id: null, Sequence: null, Code: null, CategoryId: null, SubCategoryId: null, CriticltylevelId: null, TypeId: null, EstimatedValueId: null, ItemId: null, GivenById: null, Remarks: null, ReturnDate: null, Active: true, FileName: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelList = [];
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        }
    ];

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.ModelNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    $scope.Get = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        $scope.Action = 'Update';
        //if (!$rootScope.isCollapsed) {
        //    $rootScope.toggle();
        //}
    };


    $scope.CategoryList = [];
    $scope.GetCategoryByMaster = function () {
        $http({
            method: 'GET',
            url: 'Administration/EmpDocAssetTransection/GetCategory'
        }).then(function successCallback(response) {
            $scope.CategoryList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.CategoryList = response.data;
            }
        });
    };
    $scope.GetCategoryByMaster();

    $scope.SubCategoryList = [];
    $scope.GetSubCategoryByMaster = function () {
        $http({
            method: 'GET',
            url: 'Administration/EmpDocAssetTransection/GetSubCategory'
        }).then(function successCallback(response) {
            $scope.SubCategoryList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.SubCategoryList = response.data;
            }
        });
    };
    $scope.GetSubCategoryByMaster();

    $scope.ItemList = [];
    $scope.GetItemByMaster = function () {
        $http({
            method: 'GET',
            url: 'Administration/EmpDocAssetTransection/GetItem'
        }).then(function successCallback(response) {
            $scope.ItemList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.ItemList = response.data;
            }
        });
    };
    $scope.GetItemByMaster();

    $scope.TypeList = [];
    $scope.GetTypeListByMaster = function () {
        $http({
            method: 'GET',
            url: 'Administration/EmpDocAssetTransection/GetType'
        }).then(function successCallback(response) {
            $scope.TypeList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.TypeList = response.data;
            }
        });
    };
    $scope.GetTypeListByMaster();

    $scope.EstimatedValueList = [];
    $scope.GetEstimatedValueListByMaster = function () {
        $http({
            method: 'GET',
            url: 'Administration/EmpDocAssetTransection/GetEstimatedValue'
        }).then(function successCallback(response) {
            $scope.EstimatedValueList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.EstimatedValueList = response.data;
            }
        });
    };
    $scope.GetEstimatedValueListByMaster();

    $scope.CriticltylevelList = [];
    $scope.GetCriticltylevelListByMaster = function () {
        $http({
            method: 'GET',
            url: 'Administration/EmpDocAssetTransection/GetCriticltylevel'
        }).then(function successCallback(response) {
            $scope.CriticltylevelList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.CriticltylevelList = response.data;
            }
        });
    };
    $scope.GetCriticltylevelListByMaster();

    $scope.popUpDataList = [];
    $scope.showEmployeeListPopUp = function () {
        try {

            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/leaveApplication/getemployeelist'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        var data = arg.data;
        $scope.ModelNew.GivenById = data.SystemID;
        $scope.ModelNew.GivenBy = data.EmployeeCode + '-' + data.EmployeeName;
        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.documentationNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                    $scope.GetSequence()
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.documentationNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.documentationNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                    $scope.GetSequence()
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };
    $scope.Clear = function () {
        ClearFields();
        $scope.GetSequence();
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.ModelTemp = {
            Id: null, Sequence: null, Code: null, CategoryId: null, SubCategoryId: null, CriticltylevelId: null, TypeId: null, EstimatedValueId: null, ItemId: null, GivenBy: null, GivenById: null, Remarks: null, ReturnDate: null, Active: true, FileName: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.ModelNew.Id))
                throw 'Please select/save the data first'

            args.data = $scope.ModelNew.Id;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "Administration/EmpDocAssetTransection/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ModelNew.Id))
            ShowResult('Please select/save the production order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }



}