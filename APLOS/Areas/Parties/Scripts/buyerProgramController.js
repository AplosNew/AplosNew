'use strict';
buyerProgramController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "cboService"];
function buyerProgramController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerPrograms = [];
    $scope.getListUrl = 'Parties/buyerProgram/getbuyerProgramlist';
    baseService.init($scope.getListUrl, null, 10, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        $rootScope.parameters.buyerId = $scope.buyerProgramNew.BuyerId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.buyerPrograms = result.Rows;
                $scope.buyerProgramNew = { BuyerId: $scope.buyerProgramNew.BuyerId, Active: $scope.buyerProgramNew.Active };
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.buyerProgram = {
        Id: null,
        BuyerId: null,
        Sequence: 1,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Archive: false
    };

    $scope.buyerProgramNew = Object.assign({}, $scope.buyerProgram);

    $scope.buyerList = [];
    cboService.getCboBuyer(function (result) {
        $scope.buyerList = result;
    });

    $scope.GetSequence = function () {
        $http.get("Parties/buyerProgram/getautosequence?buyerId=" + $scope.buyerProgramNew.BuyerId)
            .then(function (response) {
                $scope.buyerProgramNew.Sequence = response.data;
            });
    };
    
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerProgram = $scope.buyerPrograms[$scope.index];
        $scope.buyerProgramNew = Object.assign({}, $scope.buyerProgram);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.buyerProgramNewForm.$valid) {
            angular.copy($scope.buyerProgramNew, $scope.buyerProgram);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "Parties/buyerProgram/create",
                    data: $scope.buyerProgram,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.buyerPrograms.push(response.data.buyerProgram);
                        $scope.buyerPrograms = $filter('orderBy')($scope.buyerPrograms, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: "Parties/buyerProgram/edit",
                    data: $scope.buyerProgram,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.buyerPrograms[$scope.index] = $scope.buyerProgram;
                            $scope.buyerPrograms = $filter('orderBy')($scope.buyerPrograms, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.buyerProgramNew.Id)) {
            $http({
                method: 'POST',
                url: "Parties/buyerProgram/delete/" + $scope.buyerProgramNew.Id + "&buyerId=" + $scope.buyerProgramNew.BuyerId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.buyerPrograms.splice($scope.index, 1);
                    ClearFields(response.data.Sequence);
                    $scope.getData();
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
        ClearFields($scope.GetSequence($scope.buyerProgramNew.BuyerId));
        return true;
    };
    function ClearFields(seq) {
        $scope.BuyerId = $scope.buyerProgramNew.BuyerId;
        $scope.Action = "Save";
        $scope.buyerProgram = {
            Id: null,
            BuyerId: null,
            Sequence: 1,
            Code: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Description: null,
            Remarks: null,
            Active: true,
            Archive: false
        };

        $scope.buyerProgramNew = Object.assign({}, $scope.buyerProgram);
        $scope.buyerProgramNew.BuyerId = $scope.BuyerId;
    }
}