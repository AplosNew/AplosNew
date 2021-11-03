'use strict';
WorkCenterBuyerTagController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function WorkCenterBuyerTagController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = "Work Center Buyer";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.workCenterBuyerTags = [];
    $scope.path = 'WorkCenters/workcenterbuyertag/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Buyer', 'Buyer');

    // $scope.getData();

    $scope.workCenterBuyerTag = {
        Id: null,
        CompanyGroupId: null,
        WorkCenterMasterId: null,
        BuyerId: null,
        DMMId: null,
        PlantId: null,
        UnitId: null,
        MaterialMasterId: null,
        Active: true
    };
    $scope.workCenterBuyerTagNew = Object.assign({}, $scope.workCenterBuyerTag);

    $scope.searchByList = [
        {
            'name': 'WorkCenter Master',
            'value': 'WorkCenterMaster'
        },
        {
            'name': 'Material Master',
            'value': 'MaterialMaster'
        },
        {
            'name': 'DMM',
            'value': 'DMM'
        },
        {
            'name': 'Buyer',
            'value': 'Buyer'
        }];

    $scope.getData = function (pageno) {
        $rootScope.parameters.PlantId = $scope.workCenterBuyerTagNew.PlantId;
        $rootScope.parameters.UnitId = $scope.workCenterBuyerTagNew.UnitId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.workCenterBuyerTags = result.Rows;
                console.log($scope.workCenterBuyerTags);
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.DMMList = [];
    $http({
        method: 'GET',
        url: 'Productions/dmm/getcbo'
    }).then(function (response) {
        $scope.DMMList = response.data;
    });

    $scope.buyerList = [];
    $http({
        method: 'GET',
        url: 'Parties/buyer/getcbo'
    }).then(function (response) {
        $scope.buyerList = response.data;
    });

    $scope.plantList = [];
    $http({
        method: 'GET',
        url: 'Organizations/plant/getcbo'
    }).then(function successCallback(response) {
        $scope.plantList = response.data;
    });

    $scope.unitList = [];
    cboService.getCboUnitByCompany($window.CompanyId, function (result) {
        $scope.unitList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getworkCenterBuyerTag = angular.copy($scope.workCenterBuyerTags[$scope.index]);
        $scope.workCenterBuyerTagNew = $scope.getworkCenterBuyerTag;
        $scope.workCenterBuyerTagNew.WorkCenterMasterName = $scope.workCenterBuyerTagNew.WorkCenterMaster;
        $scope.workCenterBuyerTagNew.MaterialMasterName = $scope.workCenterBuyerTagNew.MaterialMaster;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.workCenterBuyerTagNew, $scope.workCenterBuyerTag);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.workCenterBuyerTagNewForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.workCenterBuyerTag,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        //$scope.workCenterBuyerTags.push(response.data.WorkCenterBuyerTag);
                        $scope.getData();
                        $scope.workCenterBuyerTags = $filter('orderBy')($scope.workCenterBuyerTags, 'Buyer');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.workCenterBuyerTag,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.workCenterBuyerTags[$scope.index] = $scope.workCenterBuyerTag;
                            $scope.getData();
                            $scope.workCenterBuyerTags = $filter('orderBy')($scope.workCenterBuyerTags, 'Buyer');
                        }
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.workCenterBuyerTagNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.workCenterBuyerTagNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.workCenterBuyerTags.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
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
        return true;
    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.workCenterBuyerTag = {};
        $scope.workCenterBuyerTagNew = {};
        $scope.workCenterBuyerTagNew.Active = true;
    }

    $scope.ClearWC = function () {
        ClearWCFields();
        return true;
    }
    function ClearWCFields() {
        $scope.workCenterBuyerTagNew.WorkCenterMasterName = null;
    }

    $scope.ClearMM = function () {
        ClearMMFields();
        return true;
    }
    function ClearMMFields() {
        $scope.workCenterBuyerTagNew.MaterialMasterName = null;
    }
    // #region PopUp-Dynamic
    $scope.popUpList = [];
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: '',
        searchBy: '',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function (name, id) {
        $scope.popUpUrl = '';
        $scope.popUpParameters.sort = '';
        $scope.popUpParameters.searchBy = '';
        if (name == 'MaterialMasterName') {
            $scope.popUpTitle = 'Matrial Master';
            $scope.popUpUrl = 'Materials/materialmaster/materialmastersearch';
            $scope.popUpParameters.sort = 'UserName';
            $scope.popUpParameters.searchBy = 'UserName';
        }
        if (name == 'WorkCenterMasterName') {
            $scope.popUpTitle = 'WorkCenter Master';
            $scope.popUpUrl = 'WorkCenters/workcentermaster/getallworkcenter?=' + $scope.workCenterBuyerTagNew.UnitId;
            $scope.popUpParameters.sort = 'UserName';
            $scope.popUpParameters.searchBy = 'UserName';
        }
        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.popUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (arrayLength($scope.popUpList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.fieldId = id;
        $scope.fieldName = name;
        angular.element(document.querySelector('#popUp')).modal('show');
        $scope.popUpData();
    }
    $scope.selectdblClick = function (data) {
        if ($scope.fieldName == 'MaterialMasterName') {
            $scope.workCenterBuyerTagNew[$scope.fieldId] = data.Id;
            $scope.workCenterBuyerTagNew[$scope.fieldName] = data.UserName;
        }
        if ($scope.fieldName == 'WorkCenterMasterName') {
            $scope.workCenterBuyerTagNew[$scope.fieldId] = data.Id;
            $scope.workCenterBuyerTagNew[$scope.fieldName] = data.UserName;
        }

        $scope.fieldId = '';
        $scope.fieldName = '';
        $scope.fieldId = '';
        $scope.fieldName = '';
        $scope.closePopUp();
    }
    $scope.valueData = '';
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    }
    $scope.SelectByButton = function () {
        if ($scope.valueData == '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick($scope.valueData)
        $scope.valueData = '';
        angular.element(document.querySelector('#popUp')).modal('hide');
    }
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
        $scope.popUpParameters.search = null;
        $scope.popUpParameters.total_count = 0;
        $scope.popUpParameters.offset = 0;
    }

    // #endregion
}