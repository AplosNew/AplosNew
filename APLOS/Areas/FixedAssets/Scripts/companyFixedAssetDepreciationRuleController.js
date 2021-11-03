'use strict';
companyFixedAssetDepreciationRuleController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function companyFixedAssetDepreciationRuleController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Company Depreciation Rule';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.companyDepreciationRules = [];
    $scope.CompanyDepreciaitonRuleWithCombineList = [];
    $scope.path = 'fixedassets/companyFixedAssetDepreciationRule/';
    $scope.getListUrl = 'fixedassets/companyFixedAssetDepreciationRule/';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    if ($scope.CompanyDepreciaitonRuleWithCombineList.length > 0) {
        $scope.tableShow = true;
    } else {
        $scope.tableShow = false;
    }

    $scope.companyDepreciationRule = {
        Id: null,
        DepreciationRuleId: null,
        FixedAssetMasterId: null,
        CompanyId: null
    
    };

    $scope.CompanyList = [];
    //cboService.getCboCompanyByCompanyGroup(' ', function (result) {
    //    $scope.CompanyList = result;
    //});
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.CompanyList = result;
    });

   
    $scope.fixedAssetMasterList = [];
    $scope.GetAssetMasterData = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.path + "GetListAssetMaster",
                data: {
                    'companyId': $scope.companyDepreciationRule.CompanyId
                },
                dataType: 'JSON'

            }).then(function successCallback(response) {

                $scope.fixedAssetMasterList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        catch (e) {

        }
    }
    //$scope.GetAssetMasterData();

    $scope.fixedAssetDepRuleList = [];
    $http({
        method: 'GET',
        url: 'fixedassets/companyFixedAssetDepreciationRule/GetFixedAssetDepRuleList',
    }).then(function successCallback(response) {
        $scope.fixedAssetDepRuleList = response.data;
    });

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.companyDepreciationRuleForm.$valid) {
           // if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.fixedAssetMasterList, 'CompanyId': $scope.companyDepreciationRule.CompanyId},
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        //$scope.depreciationRules.push(response.data.DepreciationRule);
                        //$scope.depreciationRules = $filter('orderBy')($scope.depreciationRules, 'Sequence');
                        //baseService.paginationAdd();
                       // ClearFields();
                        $scope.GetAssetMasterData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
           //}
            
        }
    };

    //$scope.Save = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    //if ($scope.ModelNewForm.$valid) {
    //    $http({
    //        method: 'POST',
    //        url: $scope.saveUrl,
    //        data: { 'data': $scope.ModelNew },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            ClearFields(response.data.Sequence);
    //            $scope.getData();

    //        }
    //    }), function errorCallBack(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    }

    //    //}
    //};


    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.depreciationRule.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrl + $scope.depreciationRule.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.depreciationRules.splice($scope.index, 1);
    //                baseService.paginationRemove();
    //                ClearFields();
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.status.Message, 'failure');
    //        });
    //    }
    //    else {
    //        ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
    //    }
    //    return true;
    //};

    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrl + $scope.ModelNew.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                ClearFields(response.data.Sequence);
    //                $scope.getData();
    //            }
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        });
    //    }
    //};


    //$scope.Clear = function () {
    //    ClearFields();
    //    return true;
    //};

    //function ClearFields() {
    //    $scope.Action = 'Save';
    //    $scope.depreciationRule = {
    //        Id: null,
    //        Code: null,
    //        Description: null,
    //        Factor: null,
    //        LifeTime: null,
    //        SalvageValue: null,
    //        DepreciationRules: null,
    //        DepreciationCharge: null,
    //        DepreciationPurchase: null,
    //        DepreciationDisposal: null,
    //        UniformAcross: true,
    //        Active: true
    //    };

    //}



    /***/
    // #region ******DepreciationType GL******
    //$scope.searchDepreciationTypeByList = [
    //    {
    //        'name': 'Code',
    //        'value': 'Code'
    //    },
    //    {
    //        'name': 'Description',
    //        'value': 'Description'
    //    },
    //    {
    //        'name': 'DepreciationRules',
    //        'value': 'DepreciationRules'
    //    },
    //    {
    //        'name': 'DepreciationCharge',
    //        'value': 'DepreciationCharge'
    //    }
    //];
    //$scope.depreciationListParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: 'asc',
    //    sort: 'Code',
    //    searchBy: "Code",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};
    //$scope.GetDepreciationTypeList = function () {
    //    $scope.Url = 'fixedassets/fixedAssetdepreciationrule/GetList';
    //    $scope.GetDepreciationTypeListData = function (pageno) {
    //        baseService.paginationBase($scope.Url, pageno, $scope.depreciationListParameters)
    //            .then(function (data) {
    //                $scope.DepreciationTypeGLList = data.Rows;
    //                $scope.depreciationListParameters.total_count = data.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#DepreciationTypeListPopUp')).modal('show');
    //    $scope.modalShow = true;
    //    $scope.GetDepreciationTypeListData();
    //};
    //$scope.setDepreciationGLSelected = function (x) {
    //    $scope.rowSelected = x.Id;
    //    $scope.DepreciationRuleId = x.Id;
    //    $scope.DepreciationRuleCode = x.Code;
    //    $scope.DepreciationRuleText = x.Code + ' - ' + x.Description;
    //};
    //$scope.closeDepreciationTypeListPopUpSelected = function () {
    //    if ($scope.rowSelected !== null) {
    //        angular.element(document.querySelector('#DepreciationTypeListPopUp')).modal('hide');
    //    }
    //};
    //$scope.clearDepreciationRule = function () {
    //    $scope.DepreciationRuleId = null;
    //    $scope.DepreciationRuleCode = null;
    //    $scope.DepreciationRuleText = null;
    //};

    // #endregion
    //$scope.getD = function (list) {
    //    angular.forEach($scope.DepreciationRuleList, function (item) {
    //        if (item.Value === list) {
    //            $scope.depcode = item.Code;
    //        }
    //    });
    //};

    //$scope.Get = function (id, index) {
    //    $scope.index = index;
    //    $scope.getdepreciationRule = angular.copy($scope.companyDepreciationRules[$scope.index]);
    //    $scope.companyDepreciationRule = $scope.getdepreciationRule;
    //    $scope.companyDepreciationRule.AddedDate = $filter('dateFilter')($scope.companyDepreciationRule.AddedDate);
    //    $scope.companyDepreciationRule.UpdatedDate = $filter('dateFilter')($scope.companyDepreciationRule.UpdatedDate);
    //    $scope.Action = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};
    //$scope.addDepreciationRuleForSelectble = function () {
    //    if ($scope.tempList.length === 0) {
    //        throw ShowResult("Please select any Rows", 'failure');
    //    }
    //    $scope.CompanyDepreciaitonRuleWithCombineListForSave = [];
    //    angular.forEach($scope.tempList, function (item) {
    //        if (item.Flag) {
    //            if (!baseService.isUndefinedOrNull($scope.DepreciationRuleId)) {
    //                item.DepreciationRuleId = $scope.DepreciationRuleId;
    //                item.CompanyId = $scope.companyDepreciationRule.CompanyId;
    //                $scope.CompanyDepreciaitonRuleWithCombineListForSave.push(item);
    //            } else {
    //                throw ShowResult("Please select Depreciation Rule", 'failure');
    //            }
    //        }
    //    });
    //};

    //$scope.Save = function () {
    //    $scope.addDepreciationRuleForSelectble();
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.companyDepreciationRuleForm.$valid) {
    //        if ($scope.Action === 'Save') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.saveUrl,
    //                data: { 'CompanyDepreciationRule': $scope.CompanyDepreciaitonRuleWithCombineListForSave },
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    $scope.getFixedAssetMasterWithCoa("all");
    //                    $scope.companyDepreciationRules = $filter('orderBy')($scope.companyDepreciationRules, 'DepreciationRuleId');
    //                    baseService.paginationAdd();
    //                    ClearFields();
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //        else if ($scope.Action === 'Update') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.updateUrl,
    //                data: $scope.companyDepreciationRule,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    if ($scope.index > -1) {
    //                        $scope.companyDepreciationRules[$scope.index] = $scope.companyDepreciationRule;
    //                        $scope.companyDepreciationRules = $filter('orderBy')($scope.companyDepreciationRules, 'DepreciationRuleId');
    //                    }
    //                    ClearFields();
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //    }
    //};
    //GL Untagging
    //$scope.glUntagId = null;
    //$scope.glUntagIndex = -1;
    //$scope.valuePassInDelModal = function (data, index, event) {
    //    $scope.glUntagId = data.Id;
    //    $scope.glUntagIndex = index;
    //    $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.FixedAssetMasterName + ' ]?';
    //    angular.element(document.querySelector('#glUntag')).modal('show');
    //};
    //$scope.removeRow = function () {
    //    for (var i = 0; i < $scope.CompanyDepreciaitonRuleWithCombineList.length; i++) {
    //        if ($scope.glUntagId !== null) {
    //            if ($scope.CompanyDepreciaitonRuleWithCombineList[i].Id == $scope.glUntagId) {
    //                $scope.unTagGL($scope.glUntagId, $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetMasterId, i);
    //                break;
    //            }
    //        } else {
    //            unTagFromList($scope.glUntagIndex);
    //            $scope.glUntagIndex = -1;
    //            break;
    //        }
    //    }
    //    $scope.mauid = null;
    //    $scope.mauindex = -1;
    //};
    //function unTagFromList(i) {
    //    $scope.CompanyDepreciaitonRuleWithCombineList[i] = {
    //        FixedAssetMasterName: $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetMasterName,
    //        FixedAssetMasterId: $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetMasterId,
    //        FixedAssetCategoryName: $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetCategoryName,
    //        FixedAssetSubCategoryName: $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetSubCategoryName
    //    };
    //}
    //$scope.unTagGL = function (id, FixedAssetMasterId, index) {
    //    try {
    //        $http({
    //            method: 'POST',
    //            url: $scope.path + '/Delete',
    //            dataType: 'JSON',
    //            data: { 'id': id }
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                for (var i = 0; i < $scope.tempList.length; i++) {
    //                    if ($scope.tempList[i].FixedAssetMasterId === FixedAssetMasterId) {
    //                        document.getElementById($scope.tempList[i].FixedAssetMasterId).checked = false;
    //                        $scope.tempList.splice(i, 1);
    //                        break;
    //                    }
    //                }
    //                unTagFromList(index);
    //                $scope.glUntagIndex = -1;
    //                //angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.status.Message, 'failure');
    //        });
    //        return true;
    //    } catch (e) {
    //        ShowResult(e, 'Error');
    //    }
    //};

    //$scope.clearSearchDropDown = function () {
    //    $scope.fixedassetCategoryIds = [];
    //    $scope.fixedassetSubCategoryIds = [];
    //    getListForm($scope.CompanyDepreciaitonRuleWithCombineListForDdl);
    //};

    //$scope.Clear = function () {
    //    $scope.clearSearchDropDown();
    //    ClearFields();
    //    return true;
    //};

    //function ClearFields() {
    //    $scope.Action = 'Save';
    //    $scope.companyDepreciationRule = { CompanyId: $scope.companyDepreciationRule.CompanyId };
    //    $scope.tempList = [];
    //    $scope.getFixedAssetMasterWithCoa("all");
    //    $scope.clearDepreciationRule();
    //}
}