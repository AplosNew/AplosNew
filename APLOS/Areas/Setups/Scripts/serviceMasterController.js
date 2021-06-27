'use strict';
ServiceMasterController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];
function ServiceMasterController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Service Master";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.serviceMasters = [];
    $scope.path = 'Setups/serviceMaster/';
    $scope.getListUrl = $scope.path + 'getlist?ids=null';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.searchByServiceMasterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Service Group',
            'value': 'ServiceGroup'
        }
    ];
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.serviceMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.serviceMaster = {
        Id: null
        , ServiceGroupId: null
        , ServiceGroupName: null
        , HSNCodeId:null
        , Sequence: null
        , Code: null
        , UserName: null
        , StandardName: null
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.serviceMasterNew = Object.assign({}, $scope.serviceMaster);

    $scope.serviceGroupList = [];
    $http.get('Setups/ServiceGroup/GetCbo')
        .then(function (response) {
            $scope.serviceGroupList = response.data;
        });

    $scope.hsnCodeList = [];
    cboService.getHNSCbo(function (response) {
        $scope.hsnCodeList = response;
    });

    $scope.GetHSNCodeByServiceGroupId = function () {
        $scope.serviceMasterNew.HSNCodeId = null;
        $http.get('Setups/ServiceMaster/GetHSNCodeByServiceGroupId?groupId=' + $scope.serviceMasterNew.ServiceGroupId)
            .then(function (response) {
                
                if (baseService.arrayLength($scope.hsnCodeList)>0) {
                    for (var i = 0; i < $scope.hsnCodeList.length; i++) {
                        if ($scope.hsnCodeList[i].Text === response.data[0].Code) {
                            $scope.serviceMasterNew.HSNCodeId = $scope.hsnCodeList[i].Value;
                        }
                    }
                }
            });
    };


    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.serviceMasterNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.serviceMaster = $scope.serviceMasters[$scope.index];
        $scope.serviceMasterNew = Object.assign({}, $scope.serviceMaster);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed)
            $rootScope.toggle();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.serviceMasterForm.$valid) {
            angular.copy($scope.serviceMasterNew, $scope.serviceMaster);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.serviceMaster,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.serviceMasters = $filter('orderBy')($scope.serviceMasters, 'Sequence');
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
                    data: $scope.serviceMaster,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.serviceMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.serviceMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.serviceMasters.splice($scope.index, 1);
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
        $scope.serviceMaster = {};
        $scope.serviceMasterNew = { Sequence: seq, Active: true };
    }
}