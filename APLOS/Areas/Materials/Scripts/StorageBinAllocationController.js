'use strict';
StorageBinAllocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function StorageBinAllocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Storage Bin Allocation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/StorageBinAllocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.searchbyMaterialMasterDatalist = [
        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
       
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];

    $scope.ModelTemp = {
        Id: null,
        UserName: null,
        StorageLocation: null,
        StorageSubLocation: null,
        MaterialType: null,
        MaterialGroup: null,
       
        MaterialMaster: null,
        MaterialArticle:null,
        AccessType:null,
        NoOfBin: null,
        Remarks: null,
        StorageLevel:null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    // All Lists are here
    $scope.StorageLevelList = [];
    $scope.MaterialList = [];
    $scope.MaterialGroupList = [];
    $scope.MaterialTypeList = [];
    $scope.StorageLocationList = [];
    $scope.StorageSubLocationList = [];
    $scope.MaterialArticleList = [];
    $scope.AccessTypeList = [];

    $scope.getStorageLevel = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getStorageLevel",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.StorageLevelList = response.data;
        })
    }
    $scope.getStorageLevel();

    $scope.getAccessType = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getAccessType",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.AccessTypeList = response.data;
        })
    }
    $scope.getAccessType();


    $scope.getMaterialType = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterialType",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialTypeList = response.data;
        })
    }
    $scope.getMaterialType();

    $scope.getMaterialGroup = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterialGroup",
            data: { 'MaterialTypeId': $scope.ModelNew.MaterialType, },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialGroupList = response.data;
           
        })
    }
    //$scope.getMaterialGroup();

    $scope.getMaterial = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterial",
            data: { 'materialgroupid': $scope.ModelNew.MaterialGroup, },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialList = response.data;
        })
    }   

    $scope.getStorageLocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getStorageLocation",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.StorageLocationList = response.data;
        })
    }
    $scope.getStorageLocation();

    $scope.getStorageSubLocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getStorageSubLocation",
           
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.StorageSubLocationList = response.data;
        })
    }
    $scope.getStorageSubLocation();

    $scope.getMaterialArticle = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterialArticle",
            data: { 'materialmasterId': $scope.ModelNew.MaterialMaster, },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialArticleList = response.data;
        })
    }
    
    // save op
    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ResponsiblePersonId = args.data.ResponsiblePersonId;
        $scope.StorageLocationId = args.data.StorageLocationId;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.getResponsiblePersonId();
            $scope.getStorageLocationId();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.path + 'Save',
            data: {
                'datas': $scope.ModelNew,
                
            },
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

    };

    // clear Data
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';

        $scope.ResponsiblePerson = null,
            $scope.ModelTemp = {
            Id: null,
            UserName: null,
            StorageLocation: null,
            StorageSubLocation: null,
            MaterialType: null,
            MaterialGroup: null,
            MaterialMaster: null,
            MaterialArticle: null,
            AccessType: null,
            NoOfBin: null,
            Remarks: null,
            StorageLevel: null,


            };
        $scope.StorageLocation = null;
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    // TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.redirectTab = function () {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.tabForm3.$invalid) {
            $scope.setTab(3);
        }
        else if ($scope.tabForm4.$invalid) {
            $scope.setTab(4);
        }
    };
}

