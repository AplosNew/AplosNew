'use strict';
VillageController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function VillageController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Village';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Farming/Village/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'StateId', name: "State" }, { value: 'DistrictId', name: "District" }, { value: 'TalukId', name: "Taluk" }, { value: 'UserName', name: "User Name" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" } ];
    $scope.DistrictList = [];
    $scope.StateList = [];
    $scope.TalukList = [];
    $scope.CountryList = [];

    $http({
        method: 'GET',
        url: 'Farming/Village/getcountrylist/',
    }).then(function successCallback(response) {
        $scope.CountryList = response.data;
    });

    $scope.GetState = function () {
        $scope.StateList = [];
        $http({
            method: 'GET',
            url: 'Farming/Village/getState?CountryId=' + $scope.Village.CountryId
        }).then(function successCallback(response) {
            $scope.StateList = response.data;
        });
    }

    $scope.GetDistrict = function () {
        $scope.DistrictList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getdistrictlist?StateId=' + $scope.Village.StateId
        }).then(function successCallback(response) {
            $scope.DistrictList = response.data;
        });
    }

    $scope.GetTaluk = function () {
        $scope.TalukList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'gettaluklist?DistrictId=' + $scope.Village.DistrictId
        }).then(function successCallback(response) {
            $scope.TalukList = response.data;
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
        TalukId: null,
        CountryId: null,
        UserName: null,    
        Active: true,
        ShortName: null,
        StandardName: null,
        Description: null,
        Remarks: null,
    };
    $scope.Village = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.Village.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.Village = Object.assign({}, args.data);
        $scope.GetState();
        $scope.GetDistrict();
        $scope.GetTaluk();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.VillageForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.Village },
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
        if (!baseService.isUndefinedOrNull($scope.Village.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.Village.Id,
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
        $scope.Village = Object.assign({}, $scope.ModelTemp);
        $scope.Village.Sequence = seq;
    }
}