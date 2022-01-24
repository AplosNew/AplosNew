'use strict';
BusinessProcessController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function BusinessProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "BusinessProcess";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.brands = [];
    $scope.path = 'Setups/businessprocess/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');

    $scope.brand = {
        Id: null
        , CompanyGroupId: null
        , BusinessProcessName: null
        , UserName: null
        , Type: null
    };
    angular.copy($scope.brand, $scope.brandNew);
    $rootScope.searchByList = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Business Process',
            'value': 'BusinessProcessName'
        },
        {
            'name': 'User Define Name',
            'value': 'UserName'
        }
    ];
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyGroupId = $scope.brandNew.CompanyGroupId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.brands = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });
    cboService.getEnumCbo("enum/getbusinessprocess", function (result) {
        $scope.bProcessList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.brands[$scope.index], $scope.brand);
        angular.copy($scope.brand, $scope.brandNew);
        $scope.Action = 'Update';
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.brandForm.$valid) {
            angular.copy($scope.brandNew, $scope.brand);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.brand,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.brand,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.brandNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.brandNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.brands.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                } function errorCallback(response) {
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
        $scope.brand = {};
        $scope.brandNew = { CompanyGroupId: $scope.brandNew.CompanyGroupId };
    }

    $scope.model = {
        Id: null,
        BusinessProcessId: null,
        Column1: null,
        Column2: null,
        Column3: null,
        Column4: null,
        Column5: null,
        Column6: null,
        Column7: null,
        Column8: null,
        Column9: null,
        Column10: null
    }

    $scope.extraBPList = [];
    $scope.GetBPPopUp = function () {
        var obj = {};

        $scope.extraBPList = [];
        $http({
            method: 'GET',
            url: 'Setups/BusinessProcess/GetDynamicColList?businessProcessId=' + $scope.brandNew.Id,
        }).then(function successCallback(response) {
            $scope.extraBPList = response.data;
            if (baseService.arrayLength($scope.extraBPList) == 0) {
                obj.Id = null;
                obj.BusinessProcessId = null;
                obj.Column1 = null;
                obj.Column2 = null;
                obj.Column3 = null;
                obj.Column4 = null;
                obj.Column5 = null;
                obj.Column6 = null;
                obj.Column7 = null;
                obj.Column8 = null;
                obj.Column9 = null;
                obj.Column10 = null
                $scope.extraBPList.push(obj);
            }
        })

        angular.element(document.querySelector('#BPPopUp')).modal('show');
    }

    function getBPSData(BusinessProcessId) {
        $scope.extraBPList = [];
        $http({
            method: 'GET',
            url: 'Setups/BusinessProcess/GetDynamicColList?businessProcessId=' + BusinessProcessId,
        }).then(function successCallback(response) {
            $scope.extraBPList = response.data;
        })
    }

    $scope.SaveBPS = function () {
        try {
            $http({
                method: 'post',
                url: 'Setups/BusinessProcess/SaveBPSatting',
                data: { 'funds': $scope.extraBPList, 'BusinessProcessId': $scope.brandNew.Id },
                dataType: 'json'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    getBPSData($scope.brandNew.Id);
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    // #region Tab

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion

}