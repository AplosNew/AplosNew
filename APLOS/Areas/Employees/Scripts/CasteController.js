'use strict';
CasteController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CasteController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Add Info';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Employees/Caste/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.getListUrl = $scope.path + 'GetChildList';
    $scope.getChildSeqUrl = $scope.path + '';
    $scope.saveChildUrl = $scope.path + 'CreateChild';
    $scope.deleteUrl = $scope.path + 'DeleteChild/';
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


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
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
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
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
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
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }

    $scope.GetDetailData = function (data) {
        $scope.ModelNew = Object.assign({}, data);
        $scope.getChildData();
        $scope.CGetSequence();
        angular.element(document.querySelector('#DetailPopUp')).modal('show');
    }
    $scope.ModelChildList = [];
    $scope.getChildData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetChildList",
            data: { column: $scope.searchBy, value: $scope.search, masterId: $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelChildList = response.data;
        });
    }

    $scope.CModelTemp = {
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
    $scope.CModelNew = Object.assign({}, $scope.CModelTemp);

    $scope.CGetSequence = function () {
        cboService.getSequence($scope.path + 'getchildautosequence?masterId=' + $scope.ModelNew.Id, function (data) {
            $scope.CModelTemp.Sequence = data;
            $scope.CModelNew.Sequence = data;
        });
    };

    $scope.GetChild = function (args) {

        $scope.CModelNew = Object.assign({}, args.data);
       
    };

    $scope.SaveChild = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.CModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveChildUrl,
                data: { 'data': $scope.CModelNew, 'masterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    CClearFields(response.data.Sequence);
                    $scope.getChildData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteChilld = function () {
        if (!baseService.isUndefinedOrNull($scope.CModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteChildUrl + $scope.CModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    CClearFields(response.data.Sequence);
                    $scope.getChildData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.CClear = function () {
        CClearFields($scope.CGetSequence());
        return true;
    };

    function CClearFields(seq) {
        $scope.Action = 'Save';
        $scope.CModelNew = Object.assign({}, $scope.CModelTemp);
        $scope.CModelNew.Sequence = seq;
    }

}