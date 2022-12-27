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
        StorageLocationId: null,
        StorageSubLocation: null,
        MaterialTypeId: null,
        MaterialGroupMasterId: null,
        MaterialMasterId: null,
        MaterialMasterArticleId: null,
        AccessType: null,
        NoOfBin: null,
        Remarks: null,
        StorageLevel: null,
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

    $scope.searchByBinAllocation = "UserName"; $scope.searchBinAllocation = "";
    $scope.searchByBinAllocationList = [{ value: 'UserName', name: "User Name" }, { value: 'StorageBinMaster', name: "StorageBinMaster" }
        , { value: 'MaterialType', name: "MaterialType" }, { value: 'MaterialGroup', name: "Material Group" }, { value: 'MaterialName', name: "Material" }
        , { value: 'AccessType', name: "Access Type" }];

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.getStorageLocation();
            $scope.getStorageSubLocation();
        }
    };

    $scope.binAllocationHeads = [];
    $scope.getbinAllocationHeadDataList = function () {
        $http({
            method: 'POST',
            url: 'Materials/StorageBinAllocation/GetBinAllocationHead',
            data: { column: $scope.searchByBinAllocation, value: $scope.searchBinAllocation },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.binAllocationHeads = response.data;
        });
    };
    $scope.getbinAllocationHeadDataList();

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
            data: {
                'storagesublocation': $scope.ModelNew.StorageSubLocation,
            },
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
            data: {
                'storageLocationId': $scope.ModelNew.StorageLocationId,

            },
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




    // save op

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.path + 'Save',
            data: { 'datas': $scope.ModelNew, },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');

            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.ModelNew.Id = response.data.Data.Id;
                $scope.ModelNew.MaterialMasterId = response.data.Data.MaterialMasterId
                $scope.SaveMaterialAllocation();
                $scope.SaveBinAllocation();

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
        $scope.BinHeadList = [];
        $scope.BinAllocationChildList = [];
        $scope.StorageLocation = null;
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }



    // #region ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//


    $scope.userMaterialList = [];
    $scope.selectIDs = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + "selectIDs",
                data: {
                    'materialType': $scope.ModelNew.MaterialTypeId,
                    'materialGroup': $scope.ModelNew.MaterialGroupMasterId,
                    'material': $scope.ModelNew.MaterialMasterId,
                    'storagelevel': $scope.ModelNew.StorageLevel,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BinHeadList = response.data;
                $scope.selectBinIDs();
            })
        }
    }
    // #endregion ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//
    /*
   
    //  ---------------------------------      BIN ALLOCACTION GRID      -----------------------------------//

    */
    $scope.BinAllocationChildList = [];
    $scope.userBinAllocationList = [];
    $scope.selectBinIDs = function () {
        $http({
            method: 'POST',
            url: $scope.path + "selectBinIDs",
            data: {
                'storagelocation': $scope.ModelNew.StorageLocationId,
                'storagesublocation': $scope.ModelNew.StorageSubLocation,
                'AccessType': $scope.ModelNew.AccessType,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.BinAllocationChildList = response.data;
        })
    }
    $scope.selHeaderBinList = [];
    $scope.headerBinId = [];

    $scope.selheaderBin = function () {

        for (var i = 0; i < $scope.BinHeadList.length; i++) {
            if ($scope.BinHeadList[i].chk == true) {
                $scope.headerBinId.push($scope.BinHeadList[i].Id);
                $scope.selHeaderBinList.push($scope.BinHeadList[i]);
            }
        }
        $scope.selectIDs();
    }

    $scope.SelectedBinAllocationList = [];
    $scope.BinAllocationId = [];
    $scope.selBinAllocation = function () {
        for (var i = 0; i < $scope.BinAllocationChildList.length; i++) {
            if ($scope.BinAllocationChildList[i].isSelected == true) {
                $scope.BinAllocationId.push($scope.BinAllocationChildList[i].Id);
                $scope.SelectedBinAllocationList.push($scope.BinAllocationChildList[i]);
            }
            // $scope.selectBinIDs();
        }
    }

    $scope.SaveMaterialAllocation = function () {
        //$scope.selheaderBin();

        if (baseService.arrayLength($scope.BinHeadList) > 0) {
            angular.forEach($scope.BinHeadList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = null;
                    ob.ArticleName = a.ArticleName;
                    ob.MaterialMasterArticleId = a.MaterialMasterArticleId;
                    ob.MaterialGroupMasterId = a.MaterialGroupMasterId;
                    ob.MaterialMaster = a.MaterialMaster;
                    ob.MaterialMasterId = a.MaterialMasterId;
                    ob.MaterialType = a.MaterialType;
                    ob.MaterialTypeId = a.MaterialTypeId;
                    ob.MaterialgroupName = a.MaterialgroupName;
                    ob.EstimatedCapacity = a.EstimatedCapacity;
                    $scope.userMaterialList.push(ob);
                    ob = {};
                }


            });
        }

        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.path + 'SaveMaterialAllocation',
            data: {
                'headerId': $scope.ModelNew.Id,
                'material': $scope.userMaterialList,
                'storagelevel': $scope.ModelNew.StorageLevel,
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

    // Bin allocation
    $scope.SaveBinAllocation = function () {
        //$scope.selBinAllocation();
        if (baseService.arrayLength($scope.BinAllocationChildList) > 0) {
            angular.forEach($scope.BinAllocationChildList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = null;
                    ob.AccessType = a.AccessType;
                    ob.AreaRackCode = a.AreaRackCode;
                    ob.BinCode = a.BinCode;
                    ob.BinReference = a.BinReference;
                    ob.CapacityValue = a.CapacityValue;
                    ob.ColumnNo = a.ColumnNo;
                    ob.Remarks = a.Remarks;
                    ob.RowNo = a.RowNo;
                    ob.StorageBinMasterId = a.StorageBinMasterId;
                    ob.UserLocationType = a.UserLocationType;

                    $scope.userBinAllocationList.push(ob);
                    ob = {};
                }


            });
        }
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.path + 'SaveBinAllocation',
            data: {
                'headerId': $scope.ModelNew.Id,
                'BinHead': $scope.userBinAllocationList,
                'MaterialId': $scope.ModelNew.MaterialMasterId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Clear();
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

    // SELECT UNSELECT ALL

    // Materiall Allocation
    $scope.chkdMaterialAllocation = [];
    $scope.MaterialAllocationGridAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        for (var i = 0; i < $scope.BinHeadList.length; i++) {
            $scope.BinHeadList[i].chk = ChkOrUnchk;
            $scope.chkdMaterialAllocation = $scope.BinHeadList[i].chk;
        }

        var gridObj = $("#GridEdit").data("ejGrid");
        gridObj.refreshContent();
    };

    // Bin Allocation
    $scope.chkdBinAllocation = [];
    $scope.BinAllocationGridAllCheck = function (args) {
        $("#headchkB").ejCheckBox({ "change": BinAllCheckBoxSelectAll });
    };

    function BinAllCheckBoxSelectAll(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        for (var i = 0; i < $scope.BinAllocationChildList.length; i++) {
            $scope.BinAllocationChildList[i].chk = ChkOrUnchk;
            $scope.chkdBinAllocation = $scope.BinAllocationChildList[i].chk;
        }

        var gridObj = $("#GridEditB").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.ReportFormat ='Excel',
        $scope.GetStorageBinAllocation = function (data) {
        //var z = "#" + x;
        //var gridObj = $(x).data("ejGrid");
        //var Data = gridObj.getSelectedRecords()[0];
        var url = "Materials/StorageBinAllocation/StorageBinAllocationReport?reportFormat=" + $scope.ReportFormat + '&sbaId=' + data.Id;
        $rootScope.report(url);
    };


}

