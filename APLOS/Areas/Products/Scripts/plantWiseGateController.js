'use strict';
plantWiseGateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function plantWiseGateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Plant Wise Gate';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.plantWiseGates = [];
    $scope.path = 'Products/plantWiseGate/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        $rootScope.parameters.PlantId = $scope.plantWiseGateNew.PlantId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.plantWiseGates = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.plantWiseGate = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        PlantId: null,
        CompanyId: $window.companyId,
        PreFix:null
    };

    $scope.plantWiseGateNew = Object.assign({}, $scope.plantWiseGate);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.plantWiseGateNew.Sequence = data;
        })
    };
    $scope.GetSequence();

    $scope.PlantList = [];
    cboService.getCboPlantByCompany($scope.plantWiseGateNew.CompanyId, function (result) {
        $scope.PlantList = result;
    });


    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.plantWiseGate = $scope.plantWiseGates[$scope.index];
        $scope.plantWiseGateNew = Object.assign({}, $scope.plantWiseGate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {

            angular.copy($scope.plantWiseGateNew, $scope.plantWiseGate);
            $scope.$broadcast('show-errors-check-validity');

            if (isNaN($scope.plantWiseGate.PreFix)) {
                throw "Enter only number for PreFix.";
            }

            if ($scope.plantWiseGate.PreFix.length != 2) {
                throw "PreFix must be 2 character.";
            }

            var res = $scope.plantWiseGate.PreFix.substr(0, 1);
            if (res === '0') {
                throw "0 is not allowed for PreFix.";
            }

            if ($scope.plantWiseGateNewForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.plantWiseGate,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.plantWiseGates.push(response.data.PlantWiseGate);
                            $scope.plantWiseGates = $filter('orderBy')($scope.plantWiseGates, 'Sequence');
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                            $scope.getData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.plantWiseGate,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.plantWiseGates[$scope.index] = $scope.plantWiseGate;
                                $scope.plantWiseGates = $filter('orderBy')($scope.plantWiseGates, 'Sequence');
                            }
                            ClearFields(response.data.Sequence);
                            $scope.getData();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        try {

            if (!baseService.isUndefinedOrNull($scope.plantWiseGateNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl + $scope.plantWiseGateNew.Id,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.plantWiseGates.splice($scope.index, 1);
                        baseService.paginationRemove();
                        ClearFields(response.data.Sequence);
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.plantWiseGate = {};
        $scope.plantWiseGateNew = { PlantId:$scope.plantWiseGateNew.PlantId};
        $scope.plantWiseGateNew.Sequence = seq;
        $scope.plantWiseGateNew.Active = true;
    }
}