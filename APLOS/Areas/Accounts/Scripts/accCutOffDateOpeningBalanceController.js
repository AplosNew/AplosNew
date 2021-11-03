'use strict';
accCutOffDateOpeningBalanceController.$inject = ['$rootScope', '$scope', 'cboService', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function accCutOffDateOpeningBalanceController($rootScope, $scope, cboService, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Opening Balance CutOff Date';
    $scope.Action = 'Save';
    $scope.ngShowTbl = false;
    $scope.index = -1;
    $scope.comSR = [];
    $scope.openingBalanceCutOffDates = [];

    $scope.openingBalanceCutOffDate = {
        Id: null,
        StandardName: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        IsEntityLevel: null,
        CutOffDate: null,
        Remarks: null
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getCompany = function () {
        cboService.getCboCompanyByCompanyGroup($scope.openingBalanceCutOffDate.CompanyGroupId, function (result) {
            $scope.companyList = result;
        });
    };

    $scope.getData = function (pageno) {
        $rootScope.parameters.companyGroupId = $scope.openingBalanceCutOffDate.CompanyGroupId;
        baseService.init('accounts/OpeningBalance/GetACCCutOffDateList', null, null, null, 'CompanyName');
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.comSR = result.Rows;
                for (var i = 0; i < $scope.comSR.length; i++) {
                    $scope.comSR[i].CutOffDate = $filter('dateFiltering')($scope.comSR[i].CutOffDate);
                    $scope.comSR[i].CompanyGroupId = $scope.openingBalanceCutOffDate.CompanyGroupId;
                }
                $scope.ngShowTbl = true;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $rootScope.searchByList = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Cut Off Date',
            'value': 'CutOffDate'
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.openingBalanceCutOffDate = $scope.comSR[$scope.index];
        $scope.openingBalanceCutOffDate.CutOffDate = $filter('dateFiltering')($scope.openingBalanceCutOffDate.CutOffDate, 'dd-MM-yyyy');
        $scope.openingBalanceCutOffDate.AddedDate = $filter('dateFilter')($scope.openingBalanceCutOffDate.AddedDate);
        $scope.openingBalanceCutOffDate.UpdatedDate = $filter('dateFilter')($scope.openingBalanceCutOffDate.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.comSRForm.$valid) {
            console.log('openingBalanceCutOffDates', $scope.openingBalanceCutOffDate);
            $scope.companyId = $scope.openingBalanceCutOffDate.CompanyId;
            console.log($scope.comSR);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/OpeningBalance/CreateACCCutOffDate',
                    data: {
                        'openingBalanceCutOffDates': $scope.comSR
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.comSR.push(response.data.OpeningBalanceCutOffDate);
                        $scope.openingBalanceCutOffDate.CompanyId = $scope.companyId;
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/OpeningBalance/EditCutOffDate',
                    data: $scope.openingBalanceCutOffDate,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.comSR[$scope.index] = $scope.openingBalanceCutOffDate;
                            $scope.openingBalanceCutOffDate.CompanyId = $scope.companyId;
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.openingBalanceCutOffDate.Id)) {
            $http({
                method: 'POST',
                url: 'accounts/OpeningBalance/DeleteCutOffDate/' + $scope.openingBalanceCutOffDate.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.comSR.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.openingBalanceCutOffDate = {
            CompanyGroupId: $scope.openingBalanceCutOffDate.CompanyGroupId,
            CompanyId: $scope.openingBalanceCutOffDate.CompanyId
        };
        $scope.getData();
    }
}