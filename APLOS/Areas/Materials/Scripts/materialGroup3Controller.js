'use strict';
function MaterialGroup3Controller(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Material Group3";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.materialGroup3s = [];
    $scope.path = 'Materials/materialgroup3/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "Sequence");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
                   .then(function (result) {
                       $scope.materialGroup3s = result.Rows;
                   }, function () {
                       ShowResult(commonMessage.NetworkError, 'failure');
                   }).finally(function () {
                   });
    };
    $scope.getData();

    $scope.materialGroup3 = {
        Id: null,
        Sequence: 0,
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


    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.materialGroup3.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.materialGroup3 = $scope.materialGroup3s[$scope.index];
        $scope.materialGroup3.AddedDate = $filter('dateFilter')($scope.materialGroup3.AddedDate);
        $scope.materialGroup3.UpdatedDate = $filter('dateFilter')($scope.materialGroup3.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.materialGroup3Form.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.materialGroup3,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.materialGroup3s.push(response.data.MaterialGroup3);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.materialGroup3,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -3) {
                            $scope.materialGroup3s[$scope.index] = $scope.materialGroup3;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.materialGroup3.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.materialGroup3.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialGroup3s.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.materialGroup3 = {};
        $scope.materialGroup3.Sequence = seq;
        $scope.materialGroup3.Active = true;
    }
}
MaterialGroup3Controller.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
