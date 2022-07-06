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
        StorageBinmasterId: null,
        StorageSubLocation: null,
        MaterialTypeId: null,
        MaterialGroupMasterId: null,
       
        MaterialMasterId: null,
        MaterialMasterArticleId:null,
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
    $scope.BinHeadList = [];

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
            data: { 'MaterialTypeId': $scope.ModelNew.MaterialTypeId, },
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
            data: { 'materialgroupid': $scope.ModelNew.MaterialGroupMasterId, },
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
            data: { 'storageLocationId': $scope.ModelNew.StorageBinmasterId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.StorageSubLocationList = response.data;
        })
    }
   // $scope.getStorageSubLocation();

    $scope.getMaterialArticle = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterialArticle",
            data: { 'materialmasterId': $scope.ModelNew.MaterialMasterId, },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialArticleList = response.data;
        })
    }

    // View Bin Head
    $scope.viewBinHead = function (s) {
        $http({
            method: 'POST',
            url: $scope.path + "viewBinHead",
            data: {
                'materialType': $scope.ModelNew.MaterialTypeId,
                'materialGroup': $scope.ModelNew.MaterialGroupMasterId,
                'material': $scope.ModelNew.MaterialMasterId,
                'materialArticle': $scope.ModelNew.MaterialMasterArticleId,
                
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.isUndefinedOrNull($scope.ModelNew.MaterialTypeId)) {
                ShowResult('Material Type Id is Required.', 'failure');
                throw 'Invalid Request';
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.MaterialGroupMasterId)) {
                ShowResult('Material Group Master Id is Required.', 'failure');
                throw 'Invalid Request';
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.MaterialMasterId)) {
                ShowResult('Material Master Id is Required.', 'failure');
                throw 'Invalid Request';
            }
                $scope.BinHeadList = response.data;
                //$scope.viewBinAllocation();
        })
    }

    // View Bin Allocation
    $scope.BinAllocationList = [];
    $scope.viewBinAllocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + "viewBinAllocation",
            data: {
                'storagelocation': $scope.ModelNew.StorageBinmasterId,
                'storagesublocation': $scope.ModelNew.StorageSubLocation,
                'AccessType': $scope.ModelNew.AccessType,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.isUndefinedOrNull($scope.ModelNew.StorageBinmasterId)) {
                ShowResult('StorageBinmasterId is Required.', 'failure');
                throw 'Invalid Request';
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.StorageSubLocation)) {
                ShowResult('StorageSubLocation is Required.', 'failure');
                throw 'Invalid Request';
            }

            $scope.BinAllocationList = response.data;
        })
    }
    

    // save op
    
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
                
                $scope.ModelNew.Id = response.data.Data.Id;
                $scope.viewBinHead();
                $scope.viewBinAllocation();
                
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

   /* 
    
    // ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//

    */
    $scope.selectIDs = function () {
        $http({
            method: 'POST',
            url: $scope.path + "selectIDs",
            data: {
                'materialType': $scope.ModelNew.MaterialTypeId,
                'materialGroup': $scope.ModelNew.MaterialGroupMasterId,
                'material': $scope.ModelNew.MaterialMasterId,
                'materialArticle': $scope.ModelNew.MaterialMasterArticleId,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BinHeadList = response.data;
        })
    }

    $scope.selectBinIDs = function () {
        $http({
            method: 'POST',
            url: $scope.path + "selectBinIDs",
            data: {
                'storagelocation': $scope.ModelNew.StorageBinmasterId,
                'storagesublocation': $scope.ModelNew.StorageSubLocation,
                'AccessType': $scope.ModelNew.AccessType,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.selectedBinAllocationList = response.data;
        })
    }

    $scope.selHeaderBinList = [];
    $scope.headerBinId = [];

    $scope.selheaderBin = function () {
        
        for (var i = 0; i < $scope.BinHeadList.length; i++) {
            if ($scope.BinHeadList[i].isSelected == true) {
                $scope.headerBinId.push($scope.BinHeadList[i].Id);
                $scope.selHeaderBinList.push($scope.BinHeadList[i]);
            }
            else {
                throw 'Your selection is empty please select atleast 1';
            }
        }
        $scope.selectIDs();
       }

    $scope.selectedBinAllocationList = [];
    $scope.BinAllocationId = [];
    $scope.selBinAllocation = function () {
        for (var i = 0; i < $scope.BinAllocationList.length; i++) {
            if ($scope.BinAllocationList[i].isSelected == true) {
                $scope.BinAllocationId.push($scope.BinAllocationList[i].Id);
                $scope.selectedBinAllocationList.push($scope.BinAllocationList[i]);
            }
            $scope.selectBinIDs();
        }
    }

    $scope.SaveMaterialAllocation = function () {
        
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.path + 'SaveMaterialAllocation',
            data: {
                'headerId': $scope.ModelNew.Id,
                'BinHead': $scope.selHeaderBinList
                
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    $scope.SaveBinAllocation = function () {
        
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.path + 'SaveBinAllocation',
            data: {
                'headerId': $scope.ModelNew.Id,
                'BinHead': $scope.selectedBinAllocationList

            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

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

    // Enable Disable
    $scope.EnableDisable = function () {
        $scope.selected = $scope.ModelNew.StorageLevel;
        if ($scope.selected == "Article") {
            if (document.getElementById("MaterialMasterArticleId").disabled == true) {
                document.getElementById("MaterialMasterArticleId").disabled = false;
            }

        } else {
            if (document.getElementById("MaterialMasterArticleId").disabled == false) {
                document.getElementById("MaterialMasterArticleId").disabled = true;
            }
        }


    }


}

