'use strict';
ToolMasterController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window", "$controller","$timeout"];
function ToolMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, $controller, $timeout) {
    $rootScope.title = "Tool Master";
    $scope.Action = 'Save';

    $scope.path = 'Machines/MachineandEquipmentHelpDesk/';
    $scope.deleteUrl = $scope.path + 'DeleteToolMaster/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region ToolMaster
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.ModelList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetToolMasterList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ToolMasters = {
        Id: null,
        Category: null,
        SubCategory: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Remarks: null
    };
    $scope.ToolMasterNew = Object.assign({}, $scope.ToolMasters);
    $scope.Get = function (data) {
        $scope.ToolMasterNew = Object.assign({}, data.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ToolMasterNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'Machines/MachineandEquipmentHelpDesk/CreateToolMaster',
                data: { 'data': $scope.ToolMasterNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ToolMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ToolMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ToolMasters = {
            Id: null,
            Category: null,
            SubCategory: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Remarks: null
        };

        $scope.ToolMasterNew = Object.assign({}, $scope.ToolMasters);
    }

    // #endregion ToolMaster

    // #region ToolItemMaster

    $scope.TIMModelList = [];
    $scope.getTIMData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetToolItemMasterList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.TIMModelList = response.data;
        });
    }
    $scope.getTIMData();

    $scope.ToolItemMaster = {
        Id: null,
        ToolMasterId: null,
        Remarks: null,
        IsAvailable: true
    };
    $scope.ToolItemMasterNew = Object.assign({}, $scope.ToolItemMaster);
    $scope.TIMAction = 'Save';

    $scope.GetTIM = function (data) {
        $scope.ToolItemMasterNew = Object.assign({}, data.data);
        $scope.TIMAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ToolMasterCboList = [];
    $scope.GetToolMasterCbo = function () {
        $scope.ToolMasterCboList = [];
        $http.get('Machines/MachineandEquipmentHelpDesk/GetToolMasterCbo')
            .then(function (response) {
                $scope.ToolMasterCboList = response.data;
            });
    };
    $scope.GetToolMasterCbo();

    $scope.SaveTIM = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ToolItemMasterNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'Machines/MachineandEquipmentHelpDesk/CreateToolItemMaster',
                data: { 'data': $scope.ToolItemMasterNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    TIMClearFields();
                    $scope.getTIMData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
    $scope.message_confirmation = "";
    $scope.removeTIM = function () {

        if (!baseService.isUndefinedOrNull($scope.ToolItemMasterNew.Id)) {
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.ToolItemMasterNew.Id + ' ]';
            angular.element(document.querySelector('#confirmRunDelPopUp')).modal('show');
        }
    };
    $scope.DeleteTIM = function () {
        if (!baseService.isUndefinedOrNull($scope.ToolItemMasterNew.Id)) {
            $http({
                method: 'POST',
                url: 'Machines/MachineandEquipmentHelpDesk/DeleteToolItemMaster?id=' + $scope.ToolItemMasterNew.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getTIMData();
                    TIMClearFields();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }

    };

    $scope.ClearTIM = function () {
        TIMClearFields();
        return true;
    };

    function TIMClearFields() {
        $scope.ToolItemMaster = {
            Id: null,
            ToolMasterId: null,
            Remarks: null,
            IsAvailable: true
        };
        $scope.ToolItemMasterNew = Object.assign({}, $scope.ToolItemMaster);
        $scope.TIMAction = 'Save';
    }

    // #endregion

    // #region HandToolIssue

    // #endregion

};

