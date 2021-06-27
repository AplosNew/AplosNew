'use strict';
AlternativeCOAController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function AlternativeCOAController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "AlternativeCOA";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.alternativeCOAs = [];
    $scope.getListUrl = "accounts/alternativecoa/getalternativecoalist/";
    baseService.init($scope.getListUrl, null, null, null, 'Code', 'Code');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.alternativeCOAs = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList.push(
        {
            'name': 'LengthOfGL',
            'value': 'LengthOfGL'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    );

    Array.prototype.multisplice = function () {
        var args = Array.apply(null, arguments);
        args.sort(function (a, b) {
            return a - b;
        });
        for (var i = 0; i < args.length; i++) {
            var index = args[i] - i;
            this.splice(index, 1);
        }
    };

    $rootScope.searchByList.multisplice(0, 2, 4);

    $scope.alternativeCOA = {
        Id: null,
        Code: null,
        UserName: null,
        LengthOfGL: null,
        Description: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd')
    };
    $scope.checkLgth = function (update) {
        $scope.ac = update;
        $scope.lengthField = false;
        if ($scope.ac == "Update" && $scope.alternativeCOA.LengthOfGL < $scope.g && $scope.checkAlternativeGlId == true) {
            $scope.lengthField = true;
            if ($scope.lengthField) {
                $scope.lengthFieldMsg = "Length Of GL must greater then Or Equal to " + $scope.g;
            }
        }
    };
    $scope.checkAlternativeGl = function (id) {
        $http.get('accounts/alternativecoa/getaglalternativecoa?acoaid=' + id)
            .then(function (response) {
                $scope.checkAlternativeGlId = response.data;
            });
    };
    $scope.Get = function (id, index) {
        $scope.checkAlternativeGl(id);
        $scope.LengthOfAlternativeCoaDisable = true;
        $scope.index = index;
        $scope.alternativeCOA = $scope.alternativeCOAs[$scope.index];
        $scope.g = $scope.alternativeCOAs[$scope.index]['LengthOfGL'];
        $scope.alternativeCOA.AddedDate = $filter('dateFilter')($scope.alternativeCOA.AddedDate);
        $scope.alternativeCOA.UpdatedDate = $filter('dateFilter')($scope.alternativeCOA.UpdatedDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.alternativeCOAForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: "accounts/alternativecoa/create",
                    data: $scope.alternativeCOA,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.alternativeCOAs.push(response.data.AlternativeCOA);
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action == 'Update' && !$scope.lengthField) {
                $http({
                    method: 'POST',
                    url: "accounts/alternativecoa/edit",
                    data: $scope.alternativeCOA,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.alternativeCOAs[$scope.index] = $scope.alternativeCOA;
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
        if (!baseService.isUndefinedOrNull($scope.alternativeCOA.Id)) {
            $http({
                method: 'POST',
                url: "accounts/alternativecoa/delete/" + $scope.alternativeCOA.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    var msg = ParseError(response.data.Message);
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.alternativeCOAs.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.alternativeCOA = {};
        $scope.alternativeCOA.Active = true;
    }
}
