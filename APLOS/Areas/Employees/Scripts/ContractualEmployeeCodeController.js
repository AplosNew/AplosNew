'use strict';
ContractualEmployeeCodeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ContractualEmployeeCodeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Contractual EmployeeCode';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.bloodGroups = [];
    $scope.path = 'employees/ContractualEmployeeCode/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

 
    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        EmployeeCodeLevel: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.dataList = [];

    $scope.LoadData = function () {
        $http.get('employees/ContractualEmployeeCode/GetContractCodeList?Level=' + $scope.ModelNew.EmployeeCodeLevel)
            .then(function (response) {
                $scope.dataList = [];
                $scope.dataList = response.data;
            });
    }



    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        //if ($scope.ModelNewForm.$valid) {
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: { 'data': $scope.dataList, 'Level': $scope.ModelNew.EmployeeCodeLevel},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

        //}
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
        $scope.ModelNew = {
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
        $scope.ModelNew.Sequence = seq;
    }

}