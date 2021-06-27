'use strict';
serviceGroupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function serviceGroupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Service Group";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.serviceGroups = [];
    $scope.path = 'Setups/serviceGroup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
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
            'name': 'HSN Code',
            'value': 'HSNCodeName'
        }
    ];
    $scope.getAllData = function () {
        debugger;
        baseService.init($scope.getListUrl + '?serviceTypeId=' + $scope.serviceGroupNew.ServiceTypeId, null, null, null, "Sequence", "UserName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.serviceGroups = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        $scope.GetSequence();
    }

    $scope.serviceGroup = {
        Id: null
        , ServiceTypeId: null
        , HSNCodeId: null
        , HSNCodeName: null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.serviceGroupNew = Object.assign({}, $scope.serviceGroup);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl + '?serviceTypeId=' + $scope.serviceGroupNew.ServiceTypeId)
            .then(function (response) {
                $scope.serviceGroupNew.Sequence = response.data;
            });
    }

    $http.get('Setups/ServiceType/GetCbo')
        .then(function (response) {
            $scope.serviceTypeList = response.data;
        });


    cboService.getHNSCbo(function (response) {
        $scope.hsnCodeList = response;
    });

   
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.serviceGroup = $scope.serviceGroups[$scope.index];
        $scope.serviceGroupNew = Object.assign({}, $scope.serviceGroup);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.serviceGroupForm.$valid) {
            angular.copy($scope.serviceGroupNew, $scope.serviceGroup);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.serviceGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.serviceGroupNew = response.data.ServiceGroup;
                        $scope.serviceGroupNew.HSNCodeName = angular.element("#hsnCodeId :selected").text();
                        $scope.serviceGroups.push($scope.serviceGroupNew);
                        $scope.serviceGroups = $filter('orderBy')($scope.serviceGroups, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                debugger;
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.serviceGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.serviceGroupNew.HSNCodeName = angular.element("#hsnCodeId :selected").text();
                            $scope.serviceGroups[$scope.index] = $scope.serviceGroupNew;
                            $scope.serviceGroups = $filter('orderBy')($scope.serviceGroups, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.serviceGroupNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.serviceGroupNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.serviceGroups.splice($scope.index, 1);
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
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.serviceGroup = {};
        $scope.serviceGroupNew = { ServiceTypeId: $scope.serviceGroupNew.ServiceTypeId, Sequence: seq, Active: true };
    }
}