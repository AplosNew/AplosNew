'use strict';
TalukController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TalukController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Taluk';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Farming/Taluk/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'StateId', name: "State" }, { value: 'DistrictId', name: "District" }, { value: 'UserName', name: "User Name" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" } ];
    $scope.DistrictIdList = [];
    $scope.StateList = [];
    $scope.CountryList = [];

    $http({
        method: 'GET',
        url: 'Farming/Taluk/getcountrylist/',
    }).then(function successCallback(response) {
        $scope.CountryList = response.data;
    });

    $scope.GetState = function () {
        $scope.StateList = [];
        $http({
            method: 'GET',
            url: 'Farming/Taluk/getState?CountryId=' + $scope.Taluk.CountryId
        }).then(function successCallback(response) {
            $scope.StateList = response.data;
        });
    }

    $scope.GetDistrict = function () {
        $scope.DistrictIdList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getdistrictlist?StateId=' + $scope.Taluk.StateId
        }).then(function successCallback(response) {
            $scope.DistrictIdList = response.data;
        });
    }



    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        DistrictId: null,
        StateId: null,
        CountryId: null,
        UserName: null,    
        Active: true,
        ShortName: null,
        StandardName: null,
        Description: null,
        Remarks: null,
    };
    $scope.Taluk = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.Taluk.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.Taluk = Object.assign({}, args.data);
        $scope.GetState();
        $scope.GetDistrict();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.TalukForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.Taluk },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.Taluk.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.Taluk.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
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
        $scope.Action = 'Save';
        $scope.Taluk = Object.assign({}, $scope.ModelTemp);
        $scope.Taluk.Sequence = seq;
    }
}