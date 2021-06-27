'use strict';
function FGComponentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "fgcomponent";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.fgcomponents = [];
    $scope.path = 'Materials/fgcomponent/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "Sequence");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
                   .then(function (result) {
                       $scope.fgcomponents = result.Rows;
                   }, function () {
                       ShowResult(commonMessage.NetworkError, 'failure');
                   }).finally(function () {
                   });
    };
    $scope.getData();

    $scope.fgcomponent = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.fgcomponentNew = angular.copy($scope.fgcomponent);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
          .then(function (response) {
              $scope.fgcomponentNew.Sequence = response.data;
          });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.fgcomponent = $scope.fgcomponents[$scope.index];
        $scope.fgcomponentNew = angular.copy($scope.fgcomponent);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fgcomponentNewForm.$valid) {
            angular.copy($scope.fgcomponentNew, $scope.fgcomponent);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.fgcomponent,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.fgcomponents.push(response.data.FGComponent);
                        $scope.fgcomponents = $filter('orderBy')($scope.fgcomponents, 'Sequence');
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
                    data: $scope.fgcomponent,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.fgcomponents[$scope.index] = $scope.fgcomponent;
                            $scope.fgcomponents = $filter('orderBy')($scope.fgcomponents, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.fgcomponent.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.fgcomponent.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.fgcomponents.splice($scope.index, 1);
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
        $scope.fgcomponent = {};
        $scope.fgcomponentNew = {};
        $scope.fgcomponentNew.Sequence = seq;
        $scope.fgcomponentNew.Active = true;
    }
};
FGComponentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
