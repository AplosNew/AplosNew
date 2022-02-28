'use strict';
PrePackDefinitionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PrePackDefinitionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Pre Pack Definition';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/PrePackDefinition/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = null; $scope.search = null;
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

   
    $scope.skuOneList = [];
    $scope.skuTwoList = [];
   

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields();
        });
    }
    $scope.getData();


    $scope.getSKU = function () {

        $http({
            method: 'GET',
            url: $scope.path + "getSKU1",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.skuOneList = response.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + "getSKU2",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.skuTwoList = response.data;
        });

    }

    $scope.getSKU();



    $scope.ModelTemp = {
        Id: null,
        SKU1Id: null,
        SKU2Id: null,
        UserName: null,
        ShortName: null,
        Remarks: null,
        StandardQty: 0.0,
        MinQty: 0.0,
        MaxQty: 0.0,
        NetWeightPerPiece: 0.0,
        PacketWt: 0.0,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    $scope.Get = function (args) {

        var AllData = [];
        $http({
            method: 'POST',
            url: $scope.path + "Get",
            data: { 'Id': args.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(resp) {

            AllData = resp.data.master;
            $scope.ModelNew = Object.assign({}, AllData[0]);

        });

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        //if (angular.isUndefinedOrNull($scope.ModelNew.BudgetId)) {
        //    ShowResult('No Budget Code Selected!!' , 'failure');
        //    throw ("Invalid");
        //}

      


        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'datas': $scope.ModelNew},
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
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelTemp = {
            Id: null,
            SKU1Id: null,
            SKU2Id: null,
            UserName: null,
            ShortName: null,
            Remarks: null,
            StandardQty: 0.0,
            MinQty: 0.0,
            MaxQty: 0.0,
            NetWeightPerPiece: 0.0,
            PacketWt: 0.0,
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
}