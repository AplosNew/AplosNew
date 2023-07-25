'use strict';
MovementMaterialMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MovementMaterialMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Movement-Material Master";

    $scope.path = 'Productions/MovementMaterialMaster/';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.LoadAllurl = $scope.path + 'LoadAll';
    $scope.saveurl = $scope.path + 'Save';

    $scope.searchBy = ""; $scope.search = "";
    $scope.searchByList = [{ value: 'MovementCategory', name: "Movement Category" }, { value: 'Entity', name: "Entity" }, { value: 'Item', name: "Item" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (response.data.Error == true) {

            }
            else {
                $scope.editdata = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.getData();



    $scope.selectedValues = {
        Id: null,
        CompanyId: null,
        EntityId: null,
        FromStorageLocId: null,
        ToStorageLocId: null,
        PlantId: null,
        FromLocation: null,
        ToLocation: null,
        Prefix: null,
        StartRefNo:null,
        Inventorycheck: false,
        ItemId: null,
        PurposeId: null,
    };

    $scope.Clear = function () {
        $scope.selectedValues = {
            Id: null,
            CompanyId: null,
            EntityId: null,
            FromStorageLocId: null,
            ToStorageLocId: null,
            PlantId: null,
            FromLocation: null,
            ToLocation: null,
            Prefix: null,
            Inventorycheck: false,
            ItemId: null,
            PurposeId: null
        };
        $scope.Action = "Save";
    }

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.selectedValues.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };

    $scope.EntityList = [];
    $scope.getEntityWithChange = function () {
        $scope.EntityList = [];
        $http({
            method: 'POST',
            url: $scope.path + "getEnity",
            data: { 'PlantId': $scope.selectedValues.PlantId, 'CompId': $scope.selectedValues.CompanyId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EntityList = [];
            $scope.EntityList = response.data;
        });
    };
    $scope.ItemList = [];
    $scope.StorageLocList = [];

    $scope.GetItem = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetItem',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.ItemList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.GetItem();


    $scope.CatergoryList = [];
    $scope.getPurposeCategory = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getPurposeCategory',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.CatergoryList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.getPurposeCategory();

    $scope.GetStorageLoc = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetStorageLoc',
            data: { 'PlantId': $scope.selectedValues.PlantId, 'CompId': $scope.selectedValues.CompanyId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.StorageLocList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.GetStorageLoc();

    $scope.Delete = function () {

        if (!baseService.isUndefinedOrNull($scope.selectedValues.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.selectedValues.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }


    $scope.editdata = [];

    $scope.editdoubleclick = function (args) {
        $scope.selectedValues = Object.assign({}, args.data);

        $scope.Action = "Update";

        $scope.getPlant();
        $scope.getEntityWithChange();
        $scope.GetStorageLoc();
        $scope.loadAll();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    $scope.Action = "Save";


    $scope.loadAll = function () {

        $http({
            method: 'POST',
            url: $scope.LoadAllurl,
            data: { 'id': $scope.selectedValues.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.selectedValues = {};
                $scope.selectedValues = response.data.master[0];
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }



    $scope.savedata = function () {
        try {
            $scope.validations();

            $http({
                method: 'POST',
                url: $scope.saveurl,
                data: { 'masterdata': $scope.selectedValues },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
            });
        }

        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.validations = function () {
        try {

            if (angular.isUndefinedOrNull($scope.selectedValues.CompanyId) == true)
                throw "Please select Company";

            if (angular.isUndefinedOrNull($scope.selectedValues.PlantId) == true)
                throw "Please select Plant";

            if (angular.isUndefinedOrNull($scope.selectedValues.EntityId) == true)
                throw "Please select Entity";


            if (angular.isUndefinedOrNull($scope.selectedValues.FromStorageLocId) == false || angular.isUndefinedOrNull($scope.selectedValues.ToStorageLocId) == false) {
                if ($scope.selectedValues.FromStorageLocId == $scope.selectedValues.ToStorageLocId)
                    throw "Both To and From Storage Location Cannot be Same";
            }


            if (angular.isUndefinedOrNull($scope.selectedValues.ItemId) == true)
                throw "Please select Item";



        } catch (e) {
            throw e;
        }
    }

    $scope.PrintReport = function () {

        $http({
            method: 'POST',
            url: $scope.path + 'GetPrintReport',
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    }

    $scope.downloadgriddataUrl = 'GridReports/Download';

}




