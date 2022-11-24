'use strict';
materialStorageController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function materialStorageController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Material Storage";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerStyles = [];
    $scope.showTbl = false;
    $scope.path = 'Materials/MaterialStorage/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.buyerStyle = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        BudgetId: null,
        BudgetCode: null,
        Active: true
    };
    $scope.buyerStyleNew = Object.assign({}, $scope.buyerStyle);

    $scope.getDataList = function (buyerId) {
        baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.buyerStyleNew.CompanyId;
            $rootScope.parameters.plantId = $scope.buyerStyleNew.PlantId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerStyles = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        $scope.GetSequence();
    }
    $rootScope.searchByList = [
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
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.buyerStyleNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl = $scope.path + 'getautosequence?companyId=' + $scope.buyerStyleNew.CompanyId + '&plantId=' + $scope.buyerStyleNew.PlantId)
            .then(function (response) {
                $scope.buyerStyleNew.Sequence = response.data;
            });
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerStyle = $scope.buyerStyles[$scope.index];
        $scope.buyerStyleNew = $scope.buyerStyle;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    //#region Responsible ManPower

    $scope.name = null;
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpDataList = [];
    $scope.budgetpopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $http({
            method: 'GET',
            url: 'Materials/MaterialStorage/getbudgetcodelist?companyId=' + $scope.buyerStyleNew.CompanyId + '&plantId=' + $scope.buyerStyleNew.PlantId
        }).then(function successCallback(response) {
            $scope.popUpDataList = response.data.Rows;
        });

        angular.element(document.querySelector('#popUpId')).modal('show');
    };

    $scope.selectDoubleClick = function (data) {
        $scope.buyerStyleNew.BudgetId = data.data.Id;
        $scope.buyerStyleNew.BudgetCode = data.data.Code;

        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.clearCode = function () {
        $scope.buyerStyleNew.BudgetCode = null;
        $scope.buyerStyleNew.BudgetId = null;
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };


    //#endregion Responsible ManPower

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.buyerStyleForm.$valid) {
            $scope.buyerStyle = Object.assign({}, $scope.buyerStyleNew);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.buyerStyle,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.buyerStyles.push($scope.buyerStyle);
                        $scope.buyerStyles = $filter('orderBy')($scope.buyerStyles, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.buyerStyle,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.buyerStyles[$scope.index] = $scope.buyerStyle;
                            $scope.buyerStyles = $filter('orderBy')($scope.buyerStyles, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.buyerStyleNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.buyerStyleNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.buyerStyles.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        $scope.buyerStyleNew = {};
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.buyerStyle = {};
        $scope.buyerStyleNew = {
            CompanyId: $scope.buyerStyleNew.CompanyId
            , PlantId: $scope.buyerStyleNew.PlantId
            , Sequence: seq
            , Active: true
        };
    }
};