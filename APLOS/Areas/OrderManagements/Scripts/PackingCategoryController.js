'use strict';
PackingCategoryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PackingCategoryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Packing Category';
    $scope.PCAction = 'Save';
    $scope.PCModelList = [];
    $scope.path = 'OrderManagements/ProductionOrder/';
    $scope.getPCSeqUrl = $scope.path + 'GetAutoPCSequence';
    $scope.savePCUrl = $scope.path + 'CreatePC';
    $scope.deletePCUrl = $scope.path + 'DeletePC/';
    $scope.searchPCBy = "UserName"; $scope.search = "";
    $scope.searchPCByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getPCData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPCList",
            data: { column: $scope.searchBy, value: $scope.searchpc },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCModelList = response.data;
        });
    }
    $scope.getPCData();

    $scope.PCModelTemp = {
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
    $scope.PCModelNew = Object.assign({}, $scope.PCModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getPCSeqUrl, function (data) {
            $scope.PCModelTemp.Sequence = data;
            $scope.PCModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.GetPC = function (args) {

        $scope.PCModelNew = Object.assign({}, args.data);
        $scope.PCAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SavePC = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.PCModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.savePCUrl,
                data: { 'data': $scope.PCModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getPCData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.PCModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deletePCUrl + $scope.PCModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getPCData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.PCClear = function () {
        ClearFields($scope.GetSequence());
        return true;
        $scope.PCAction = 'Save';
    };

    function ClearFields(seq) {
        $scope.PCAction = 'Save';
        $scope.PCModelNew = Object.assign({}, $scope.PCModelTemp);
        $scope.PCModelNew.Sequence = seq;
    }
}