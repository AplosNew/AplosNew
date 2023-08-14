'use strict';
fixedAssetMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function fixedAssetMasterController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = 'FixedAsset Master';
    $scope.Action = 'Save';
    $scope.ActionItem = 'Save';
    $scope.ActionAIItem = 'Save';
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
            Value: 'Tangible',
            Text: 'Tangible'
        },
        {
            Value: 'Intangible',
            Text: 'Intangible'
        }
        //,
        //{
        //    Value: 'Equipment',
        //    Text: 'Equipment'
        //},
        //{
        //    Value: 'Land&SiteDevelopment',
        //    Text: 'Land&SiteDevelopment'
        //},
        //{
        //    Value: 'Plant',
        //    Text: 'Plant'
        //},
        //{
        //    Value: 'Vehicle',
        //    Text: 'Vehicle'
        //},
        //{
        //    Value: 'Other',
        //    Text: 'Other'
        //}
    ];

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
        //$scope.refreshAccGroup(0);
        //$scope.refreshAccGroup(1);
        //$scope.refreshAccGroup(2);
    }

    $scope.fixedAssetMasterReport = function () {
        location.href = 'fixedassets/fixedassetmaster/fixedassetmasterreport';
    };

    $scope.FixedAssetMasterReport = function () {
        //var dataList = [];
        //var g = $("#GridMasterLC").data("ejGrid");
        //dataList = g.getFilteredRecords();

        //for (var i = 0; i < $scope.MasterLCList.length; i++) {
        //    if ($scope.MasterLCList[i].isSelected == true) {
        //        dataList.push($scope.MasterLCList[i]);
        //    }
        //}

        //if (dataList.length == 0) {
        //    dataList = $scope.MasterLCList;
        //}
        $scope.fileName = 'Fixed Asset Master Report.xlsx';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

        $http({
            method: 'POST',
            url: $scope.path + "FixedAssetMasterXls",
            data: { 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.FixedAssetMasterIndividualReport = function (data) {

        $scope.fileName = 'Fixed Asset Master Report.xlsx';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

        $http({
            method: 'POST',
            url: $scope.path + "FixedAssetMasterIndividualXls",
            data: { 'FAMId': data, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }


    //--------******Fixed Asset Master Item Start*****-----------//

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

    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });


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

    $scope.ClearFAMI = function () {
        $scope.AdditionalInfoItemList = [];
        $scope.ModelChildNew = Object.assign({}, $scope.fixedAssetMasterItem);
        $scope.ActionItem = 'Save';
    }

    $scope.FixedAssetMasterItemList = [];
    $scope.getFAMIListUrl = $scope.path + 'getFAMIlist';
    $scope.getFAMIData = function () {
        $scope.FixedAssetMasterItemList = [];
        $http.get($scope.getFAMIListUrl)
            .then(function (response) {
                $scope.FixedAssetMasterItemList = response.data.Rows;
            });
    };
    $scope.getFAMIData();

    $scope.GetFAMI = function (args) {
        $scope.ModelChildNew = Object.assign({}, args.data);
        $scope.getAdditionalData();
        $scope.getFAMIData();
        $scope.ActionItem = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.FixedAssetMasterItemReport = function () {
        $scope.fileName = 'Fixed Asset Master Item Report.xlsx';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

        $http({
            method: 'POST',
            url: $scope.path + "FixedAssetMasterItemXls",
            data: { 'data': $scope.FixedAssetMasterItemList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    //--------******Fixed Asset Master Item End*****-------------//

    //--------******Additional Info Item Start*****-------------//
    $scope.AdditionalInfoItem = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        UOMId: null,
        UOM: null,
        Remarks: null,
        IsMandatory: false,
        Active: true
    };
    $scope.AIItemModelNew = Object.assign({}, $scope.AdditionalInfoItem);

    $scope.GetSequence = function () {
        cboService.getSequence('fixedassets/FixedAssetRegister/GetAutoSequence', function (data) {
            $scope.AIItemModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.SaveAIItem = function () {
        $http({
            method: 'POST',
            url: 'fixedassets/FixedAssetRegister/CreateAdditionalInfoItem',
            data: { 'data': $scope.AIItemModelNew },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ClearAIItem();
                $scope.getAIItemData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.DeleteAIItem = function () {
        if (!baseService.isUndefinedOrNull($scope.AIItemModelNew.Id)) {
            $http({
                method: 'POST',
                url: 'fixedassets/FixedAssetRegister/DeleteAdditionalInfoItemData',
                data: { 'Id': $scope.AIItemModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getAIItemData();
                    $scope.ClearAIItem();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.ClearAIItem = function () {
        $scope.AIItemModelNew = Object.assign({}, $scope.AdditionalInfoItem);
        $scope.GetSequence();
        $scope.ActionAIItem = 'Save';
    }

    $scope.DoubleClickAIItem = function (args) {
        $scope.AIItemModelNew = Object.assign({}, args.data);
        $scope.getAIItemData();
        $scope.ActionItem = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
     
    $scope.searchEdit = 'Sequence'; $scope.searchEditValue = '';
    $scope.searchByEdit = [{ name: 'Sequence', value: 'Sequence' },
    { name: 'Code', value: 'Code' },
    { name: 'Standard Name', value: 'StandardName' },
    { name: 'Short Name', value: 'ShortName' },
    { name: 'User Name', value: 'UserName' },
    { name: 'Category', value: 'Category' },
    { name: 'Sub-Category', value: 'SubCategory' },
    { name: 'Remarks', value: 'Remarks' }];

    $scope.AIItemList = [];
    $scope.getAIItemData = function () {
        $http({
            method: "POST",
            url: 'fixedassets/FixedAssetRegister/GetAdditionalInfoItemList',
            data: { column: $scope.searchEdit, value: $scope.searchEditValue },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.AIItemList = response.data;
        });
    };
    $scope.getAIItemData();

    //--------******Additional Info Item End*****-------------//

    $scope.searchdata = [];
    $scope.GetAdditionalInfoItemData = function () {
        $scope.searchdata = [];
        $http({
            method: "POST",
            url: 'fixedassets/FixedAssetRegister/GetAdditionalInfoItemList',
            data: { column: $scope.searchEdit, value: $scope.searchEditValue },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.searchdata = response.data;
        });
        angular.element(document.querySelector('#AdditionalPopUp')).modal('show');
    }
    $scope.CloseAdditionalPopUp = function () {
        MakeData();
        angular.element(document.querySelector('#AdditionalPopUp')).modal('hide');
    }

    // #region checkbox all

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GriAdditional").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.searchdata.length; i++) {
                $scope.searchdata[i].State = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GriAdditional").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.AdditionalInfoItemList = [];
    function MakeData() {

        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].State == true) {
                if (checkExists($scope.AdditionalInfoItemList, $scope.searchdata[i].Id) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.FixedAssetItemId = $scope.ModelChildNew.Id;
                    ob.AdditionalInfoItemId = $scope.searchdata[i].Id;
                    ob.Sequence = $scope.searchdata[i].Sequence;
                    ob.Code = $scope.searchdata[i].Code;
                    ob.ShortName = $scope.searchdata[i].ShortName;
                    ob.StandardName = $scope.searchdata[i].StandardName;
                    ob.UoM = $scope.searchdata[i].UoM;
                    ob.Remarks = $scope.searchdata[i].Remarks;
                    ob.Man = $scope.searchdata[i].Man;
                    ob.Act = $scope.searchdata[i].Act;

                    $scope.AdditionalInfoItemList.push(ob);
                }
                
            }
        }
        $scope.SaveAdditional();
    }

    function checkExists(list, AdditionalInfoItemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].AdditionalInfoItemId === AdditionalInfoItemId) {
                return true;
            }
        }
        return false;
    }
    $scope.SaveAdditional = function () {
        try {
            
            $http({
                method: 'POST',
                url: 'FixedAssets/FixedAssetMaster/CreateAdditional',
                data: { 'AdditionalList': $scope.AdditionalInfoItemList, 'masterId': $scope.ModelChildNew.Id },
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
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getAdditionalData = function () {
        $http({
            method: 'GET',
            url: 'FixedAssets/FixedAssetMaster/getAdditionalData?masterId=' + $scope.ModelChildNew.Id
        }).then(function successCallback(response) {
            $scope.AdditionalInfoItemList = response.data;
        });
    }

}