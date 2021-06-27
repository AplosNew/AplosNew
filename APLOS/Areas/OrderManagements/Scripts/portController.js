'use strict';
PortController.$inject = ["addressService", 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function PortController(addressService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Port";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.ports = [];
    $scope.path = 'OrderManagements/port/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.ports = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.port = {
        Id: null,
        CountryId: null,
        CountryName: null,
        ShipModeId: null,
        ShipModeName: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.portNew = Object.assign({}, $scope.port);
    $rootScope.searchPortByList = [

        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        },
        {
            'name': 'Country',
            'value': 'CountryName'
        },
        {
            'name': 'ShipMode',
            'value': 'ShipModeName'
        }
    ];

    addressService.getCountryCbo(function (result) {
        $scope.companyList = result;
    });

    cboService.getShipModeCbo(function (result) {
        $scope.shipModeList = result;
    });

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.portNew.Sequence = response.data;
            });
    };

    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.port = $scope.ports[$scope.index];
        $scope.portNew = Object.assign({}, $scope.port);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.portNewForm.$valid) {
            $scope.countryName = document.getElementById("countryId").options[document.getElementById('countryId').selectedIndex].text;
            $scope.shipModeName = document.getElementById("shipModeId").options[document.getElementById('shipModeId').selectedIndex].text;
            angular.copy($scope.portNew, $scope.port);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.port,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.port = response.data.Port;
                        $scope.port.CountryName = $scope.countryName;
                        $scope.port.ShipModeName = $scope.shipModeName;
                        $scope.ports.push($scope.port);
                        $scope.ports = $filter('orderBy')($scope.ports, 'Sequence');
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
                    data: $scope.port,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.port.CountryName = $scope.countryName;
                            $scope.port.ShipModeName = $scope.shipModeName;
                            $scope.ports[$scope.index] = $scope.port;
                            $scope.ports = $filter('orderBy')($scope.ports, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.portNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.portNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ports.splice($scope.index, 1);
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
        $scope.port = {};
        $scope.portNew = {};
        $scope.portNew.Sequence = seq;
        $scope.portNew.Active = true;
    }
}