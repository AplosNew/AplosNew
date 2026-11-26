'use strict';
RPCategoryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function RPCategoryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    
    $scope.Action = 'Save';
    $scope.RPModelList = [];
    $scope.path = 'Machines/MachineandEquipmentHelpDesk/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'CreateRPCategory';
    $scope.deleteUrl = $scope.path + 'DeleteRPCategory/';
    baseService.init($scope.getListUrl);
    $scope.searchByRP = "UserName"; $scope.search = "";
    $scope.searchByRList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getRPData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetRPCategoryList",
            data: { column: $scope.searchByRP, value: $scope.searchRP },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.RPModelList = response.data;
        });
    }
    $scope.getRPData();

    $scope.ModelTemp = {
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
    $scope.RPCategoryNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.RPCategoryNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.GetRP = function (args) {
        $scope.RPCategoryNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveRP = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.RPCategoryNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.RPCategoryNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    RPClearFields(response.data.Sequence);
                    $scope.getRPData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteRP = function () {
        if (!baseService.isUndefinedOrNull($scope.RPCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.RPCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    RPClearFields(response.data.Sequence);
                    $scope.getRPData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearRPCat = function () {
        RPClearFields($scope.GetSequence());
        return true;
    };

    function RPClearFields(seq) {
        $scope.Action = 'Save';
        $scope.RPCategoryNew = Object.assign({}, $scope.ModelTemp);
        $scope.RPCategoryNew.Sequence = seq;
    }
}