'use strict';
DestinationController.$inject = ["addressService", 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function DestinationController(addressService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Destination";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.destinations = [];
    $scope.path = 'OrderManagements/destination/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.destinations = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.destination = {
        Id: null,
        CountryId: null,
        CountryName: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.destinationNew = Object.assign({}, $scope.destination);

    $rootScope.searchDestinationByList = [

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
        }
    ];

    addressService.getCountryCbo(function (result) {
        $scope.companyList = result;
    });

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.destinationNew.Sequence = response.data;
            });
    };

    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.destination = $scope.destinations[$scope.index];
        $scope.destinationNew = Object.assign({}, $scope.destination);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.destinationNewForm.$valid) {
            $scope.countryName = document.getElementById("countryId").options[document.getElementById('countryId').selectedIndex].text;
            angular.copy($scope.destinationNew, $scope.destination);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.destination,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.destination = response.data.Destination;
                        $scope.destination.CountryName = $scope.countryName;
                        $scope.destinations.push($scope.destination);
                        $scope.destinations = $filter('orderBy')($scope.destinations, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.destination,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.destination.CountryName = $scope.countryName;
                            $scope.destinations[$scope.index] = $scope.destination;
                            $scope.destinations = $filter('orderBy')($scope.destinations, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.destinationNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.destinationNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.destinations.splice($scope.index, 1);
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
        $scope.destination = {};
        $scope.destinationNew = {};
        $scope.destinationNew.Sequence = seq;
        $scope.destinationNew.Active = true;
    }
}