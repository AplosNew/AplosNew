'use strict';
fixedAssetMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function fixedAssetMasterController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = 'FixedAsset Master';
    $scope.Action = 'Save';
    $scope.ActionItem = 'Save';
    $scope.index = -1;
    $scope.FixedAssetMasters = [];
    $scope.glTagList = [];
    $scope.ReconAssetTypeGLList = [];
    $scope.AccDepreciationGLTypeList = [];
    $scope.DepreciationTypeGLList = [];
    $scope.AUCGLTypeList = [];
    $scope.path = 'fixedassets/fixedassetmaster/';
    $scope.fixedAssetMasterXLUrl = 'fixedassets/fixedassetmaster/fixedassetmasterreport';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveChildUrl = $scope.path + 'CreateChild'; 

    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Category',
            'value': 'FixedAssetCategory'
        },
        {
            'name': 'Sub Category',
            'value': 'FixedAssetSubCategory'
        },
        {
            'name': 'Asset Type',
            'value': 'AssetType'
        }
    ];

    $scope.fixedAssetMaster = {
        Id: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        CompanyGroupId: null,
        FixedAssetCategoryId: null,
        FixedAssetSubCategoryId: null,
        AssetType: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: new Date()
    };


    $scope.fixedAssetTypes = [
        {
            Value: 'Building',
            Text: 'Building'
        },
        {
            Value: 'Machine',
            Text: 'Machine'
        },
        {
            Value: 'Equipment',
            Text: 'Equipment'
        },
        {
            Value: 'Land&SiteDevelopment',
            Text: 'Land&SiteDevelopment'
        },
        {
            Value: 'Plant',
            Text: 'Plant'
        },
        {
            Value: 'Vehicle',
            Text: 'Vehicle'
        },
        {
            Value: 'Other',
            Text: 'Other'
        }
    ];

    $scope.fixedAssetMasterItem = {
        Id: null,
        Code: null,
        FixedAssetMasterId: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        CapacityUoMId: null,
        CapacityValue: null,
        Active: true
    };
    $scope.ModelChildNew = Object.assign({}, $scope.fixedAssetMasterItem);

    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.FixedAssetMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    //$scope.searchByFAM = "Id"; $scope.searchFAM = "";

    //$scope.FAMparameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: "ASC",
    //    sort: "UserName",
    //    searchByFAM: "UserName",
    //    pageSize: 10,
    //    total_count: 0,
    //    searchFAM: null,
    //    serverPagination: true
    //};

    //$scope.getListUrl = $scope.path + 'getlist';
    //$scope.getData = function (pageno) {
    //    baseService.paginationBase($scope.getListUrl, pageno, $scope.FAMparameters)
    //        .then(function (result) {
    //            $scope.FixedAssetMasters = result.Rows;
    //            $scope.FAMparameters.total_count = result.Total;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getData();


    $scope.searchByFAMIList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Fixed Asset Master Name',
            'value': 'FixedAssetMaster'
        },
        {
            'name': 'Capacity UoM',
            'value': 'CapacityUoM'
        },
        {
            'name': 'Capacity Value',
            'value': 'CapacityValue'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    ];
    $scope.FAMIparameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "UserName",
        searchByFAMI: "UserName",
        pageSize: 10,
        total_count: 0,
        searchFAMI: null,
        serverPagination: true
    };

    $scope.FixedAssetMasterItemList = [];
    $scope.getFAMIListUrl = $scope.path + 'getFAMIlist';
    $scope.getFAMIData = function (pageno) {
        baseService.paginationBase($scope.getFAMIListUrl, pageno, $scope.FAMIparameters)
            .then(function (result) {
                $scope.FixedAssetMasterItemList = result.Rows;
                $scope.FAMIparameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getFAMIData();


    $scope.fixedAssetCategoryList = [];
    cboService.getFixedAssetCategoryList(function (result) {
        $scope.fixedAssetCategoryList = result;
    });
 
    $scope.fixedAssetSubCategoryList = [];
    cboService.getFixedAssetSubCategoryList(function (result) {
        $scope.fixedAssetSubCategoryList = result;
    });
   

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.fixedAssetMaster = $scope.FixedAssetMasters[$scope.index];
        $scope.glTag = {};
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fixedAssetMasterNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'fixedAssetMaster': $scope.fixedAssetMaster },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'fixedAssetMaster': $scope.fixedAssetMaster, 'fixedAssetGL': $scope.glTag, 'fixedAssetVendorReconGL': $scope.accountGroupList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.FixedAssetMasters[$scope.index] = $scope.fixedAssetMaster;
                            ClearFields();
                            $scope.getData();
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.fixedAssetMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.fixedAssetMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.FixedAssetMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.fixedAssetMaster = {};
        $scope.fixedAssetMaster.Active = true;
        $scope.glTag = {};
        $scope.refreshAccGroup(0);
        $scope.refreshAccGroup(1);
        $scope.refreshAccGroup(2);
    }

    $scope.fixedAssetMasterReport = function () {
        location.href = 'fixedassets/fixedassetmaster/fixedassetmasterreport';
    };

    $scope.FixedAssetMasterList = [];
    $scope.selectFixedAssetMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetFixedAssetMaster',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.FixedAssetMasterList = resp.data;
        });
        angular.element(document.querySelector('#FAMPop')).modal('show');
    }
    $scope.doubleFixedAssetMaster = function (e) {
        $scope.ModelChildNew.FixedAssetMasterId = e.data.Id;
        $scope.ModelChildNew.FixedAssetMaster = e.data.UserName;
        angular.element(document.querySelector('#FAMPop')).modal('hide');
    }

    $scope.closeFAMPopUp = function () {
        angular.element(document.querySelector('#FAMPop')).modal('hide');
    }

    $scope.SaveChild = function () { 
            $http({
                method: 'POST',
                url: $scope.saveChildUrl,
                data: { 'data': $scope.ModelChildNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearFAMI(); 
                    $scope.getFAMIData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            } 
    };

    $scope.ClearFAMI = function () {
        $scope.ModelChildNew = Object.assign({}, $scope.fixedAssetMasterItem);
        $scope.ActionItem = 'Save';
    }


    $scope.GetFAMI = function (id, index) {
        $scope.index = index;
        $scope.ModelChildNew = $scope.FixedAssetMasterItemList[$scope.index];
        $scope.ActionItem = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.message_confirmation = "Are you sure want to permanent delete ?";
    $scope.DeleteFAMI = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelChildNew.Id)) {
            $http.get('fixedassets/fixedassetmaster/DeleteFAMI?Id=' + $scope.ModelChildNew.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ClearFAMI();
                        $scope.getFAMIData();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

}