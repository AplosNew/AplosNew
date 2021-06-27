'use strict';
serviceTypeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function serviceTypeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Service Type";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.serviceCategories = [];
    $scope.path = 'Setups/servicetype/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.serviceCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.servicetype = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.servicetypeNew = Object.assign({}, $scope.servicetype);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.servicetypeNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.servicetype = $scope.serviceCategories[$scope.index];
        $scope.servicetypeNew = Object.assign({}, $scope.servicetype);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.serviceTypeForm.$valid) {
            angular.copy($scope.servicetypeNew, $scope.servicetype);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.servicetype,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.serviceCategories.push(response.data.ServiceType);
                        $scope.serviceCategories = $filter('orderBy')($scope.serviceCategories, 'Sequence');
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
                    data: $scope.servicetype,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.serviceCategories[$scope.index] = $scope.servicetype;
                            $scope.serviceCategories = $filter('orderBy')($scope.serviceCategories, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.servicetypeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.servicetypeNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.serviceCategories.splice($scope.index, 1);
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
        $scope.servicetype = {};
        $scope.servicetypeNew = { Sequence: seq, Active: true };
    }
}