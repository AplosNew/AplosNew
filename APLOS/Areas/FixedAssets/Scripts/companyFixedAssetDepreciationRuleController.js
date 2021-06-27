'use strict';
companyFixedAssetDepreciationRuleController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function companyFixedAssetDepreciationRuleController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Company Depreciation Rule';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.companyDepreciationRules = [];
    $scope.CompanyDepreciaitonRuleWithCombineList = [];
    $scope.path = 'fixedassets/companyFixedAssetDepreciationRule/';
    $scope.getListUrl = 'fixedassets/companyFixedAssetDepreciationRule/getlist';
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
        CompanyId: null,
        Active: true
    };

    $scope.searchByList = [
        {
            'name': 'FixedAsset Master',
            'value': 'FixedAssetMasterId'
        },
        {
            'name': 'Depreciation Rule',
            'value': 'Description'
        }
    ];

    $scope.getDataWithCompany = function () {
        baseService.init('fixedassets/companyFixedAssetDepreciationRule/getlist?companyId=' + $scope.companyDepreciationRule.CompanyId, null, null, null, 'Description', 'Description');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.companyDepreciationRules = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.getDataWithCoaChange = function () {
        $http.get('fixedassets/companyFixedAssetDepreciationRule/getlistwithcombine?companyId=' + $scope.companyDepreciationRule.CompanyId)
            .then(function (response) {
                $scope.CompanyDepreciaitonRuleWithCombineListForDdl = response.data.Rows;
                getListForm($scope.CompanyDepreciaitonRuleWithCombineListForDdl);
            });
    };

    //$scope.getDataWithCoaChange();
    //$scope.tempList = [];
    //$scope.selectChValueId = function (event, FixedAssetMasterId) {
    //    if (event.currentTarget.checked)
    //        $scope.tempList.push(FixedAssetMasterId);
    //    else
    //        $scope.tempList.splice($scope.tempList.indexOf(FixedAssetMasterId), 1);
    //}

    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListId($scope.tempList, data.FixedAssetMasterId) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].FixedAssetMasterId === data.FixedAssetMasterId) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempListId(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FixedAssetMasterId === Id) {
                return true;
            }
        }
        return false;
    }

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FixedAssetMasterId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.companyDepreciaitonRuleWithCombineListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'FixedAssetMasterName',
        searchBy: "FixedAssetMasterName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    //function buildUrl(url, params) {
    //    if (!params) return url;
    //    var parts = [];
    //    forEachSorted(params, function (value, key) {
    //        if (value === null || isUndefined(value)) return;
    //        if (!isArray(value)) value = [value];

    //        forEach(value, function (v) {
    //            if (isObject(v)) {
    //                v = toJson(v);
    //            }
    //            parts.push(encodeUriQuery(key) + '=' +
    //                encodeUriQuery(v));
    //        });
    //    });
    //    if (parts.length > 0) {
    //        url += ((url.indexOf('?') == -1) ? '?' : '&') + parts.join('&');
    //    }
    //    return url;
    //};
    $scope.getFixedAssetMasterWithCoa = function (str) {
        IdList();
        $scope.CompanyDepreciaitonRuleWithCombineList = [];
        if (str === 'all')
            $scope.url = 'fixedassets/companyFixedAssetDepreciationRule/getlistwithcombineALL';
        if (str === 'notassing')
            $scope.url = 'fixedassets/companyFixedAssetDepreciationRule/getlistwithcombinenotassing';
        if (str === 'assing')
            $scope.url = 'fixedassets/companyFixedAssetDepreciationRule/getlistwithcombineassing';
        baseService.setCurrentPage('CompanyDepreciaitonRuleWithCombineList');
        $scope.getData = function (pageno) {
            $scope.companyDepreciaitonRuleWithCombineListParameters.companyId = $scope.companyDepreciationRule.CompanyId
            $scope.companyDepreciaitonRuleWithCombineListParameters.FixedAssetCategoryIds = $scope.fixedassetCategoryIdstr
            $scope.companyDepreciaitonRuleWithCombineListParameters.FixedAssetSubCategoryIds = $scope.fixedassetSubCategoryIdstr
            baseService.paginationBase($scope.url, pageno, $scope.companyDepreciaitonRuleWithCombineListParameters)
                .then(function (result) {
                    $scope.CompanyDepreciaitonRuleWithCombineList = result.Rows;
                    $scope.companyDepreciaitonRuleWithCombineListParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.CompanyDepreciaitonRuleWithCombineList.length; i++) {
                        $scope.CompanyDepreciaitonRuleWithCombineList[i].Flag = getActive($scope.tempList, $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetMasterId)
                    }
                    if ($scope.CompanyDepreciaitonRuleWithCombineList.length > 0) {
                        $scope.tableShow = true;
                    } else {
                        $scope.tableShow = false;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    /****getfromLIst******/
    function IdList() {
        $scope.fixedassetCategoryIdstr = createIdList(validListWithStr($scope.fixedAssetCategoryNewList, $scope.fixedassetCategoryIds));
        $scope.fixedassetSubCategoryIdstr = createIdList(validListWithStr($scope.fixedAssetSubCategoryNewList, $scope.fixedassetSubCategoryIds));
    }
    function createIdList(list) {
        var value = "''";
        for (var i = 0; i < list.length; i++) {
            if (value === "''") {
                value = "'" + list[i].Value + "'";
            } else {
                value += ",'" + list[i].Value + "'";
            }
        }
        return value;
    }
    function getListForm(list) {
        $scope.fixedAssetCategoryNewList = createCbo(list, 'FixedAssetCategoryId', 'FixedAssetCategoryName');
        $scope.fixedAssetSubCategoryNewList = createCbo(list, 'FixedAssetSubCategoryId', 'FixedAssetSubCategoryName');
    }
    function createCbo(dblist, value, text) {
        var list = [];
        for (var i = 0; i < dblist.length; i++) {
            if (!ddlFilter(list, dblist[i][value])) {
                list.push({
                    Text: dblist[i][text],
                    Value: dblist[i][value]
                })
            }
        }
        //Sorting with text A-Z
        return list.sort(function (a, b) {
            var nameA = a.Text.toUpperCase(); // ignore upper and lowercase
            var nameB = b.Text.toUpperCase(); // ignore upper and lowercase
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }

            // names must be equal
            return 0;
        });
    }
    function ddlFilter(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Value === id)
                return true;
        }
        return false;
    }
    function newList(oldMainDDlList, values, name) {
        var list = [];
        for (var i = 0; i < oldMainDDlList.length; i++) {
            if (values.length > 0) {
                for (var ii = 0; ii < values.length; ii++) {
                    if (oldMainDDlList[i][name] === values[ii].Value) {
                        list.push({
                            Id: oldMainDDlList[i].Id,
                            FixedAssetMasterName: oldMainDDlList[i].FixedAssetMasterName,
                            CompanyGroupId: oldMainDDlList[i].CompanyGroupId,
                            FixedAssetCategoryId: oldMainDDlList[i].FixedAssetCategoryId,
                            FixedAssetCategoryName: oldMainDDlList[i].FixedAssetCategoryName,
                            FixedAssetSubCategoryId: oldMainDDlList[i].FixedAssetSubCategoryId,
                            FixedAssetSubCategoryName: oldMainDDlList[i].FixedAssetSubCategoryName
                        });
                    }
                }
            }
            else {
                list.push({
                    Id: oldMainDDlList[i].Id,
                    FixedAssetMasterName: oldMainDDlList[i].FixedAssetMasterName,
                    CompanyGroupId: oldMainDDlList[i].CompanyGroupId,
                    FixedAssetCategoryId: oldMainDDlList[i].FixedAssetCategoryId,
                    FixedAssetCategoryName: oldMainDDlList[i].FixedAssetCategoryName,
                    FixedAssetSubCategoryId: oldMainDDlList[i].FixedAssetSubCategoryId,
                    FixedAssetSubCategoryName: oldMainDDlList[i].FixedAssetSubCategoryName
                });
            }
        }
        return list;
    }
    function ddlFilterByDDL(newlist, value, text) {
        var list = [];
        for (var i = 0; i < newlist.length; i++) {
            if (!ddlFilter(list, newlist[i][value])) {
                list.push({
                    Value: newlist[i][value],
                    Text: newlist[i][text]
                });
            }
        }
        return list.sort(function (a, b) {
            var nameA = a.Text.toUpperCase(); // ignore upper and lowercase
            var nameB = b.Text.toUpperCase(); // ignore upper and lowercase
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }

            // names must be equal
            return 0;
        });
    }
    $scope.cboCratetor = function (val, name) {
        $scope.newList = [];
        $scope.newList = newList($scope.CompanyDepreciaitonRuleWithCombineListForDdl, val, name);
        if (name !== 'FixedAssetCategoryId')
            $scope.fixedAssetCategoryNewList = ddlFilterByDDL($scope.newList, 'FixedAssetCategoryId', 'FixedAssetCategoryName');
        if (name !== 'FixedAssetSubCategoryId')
            $scope.fixedAssetSubCategoryNewList = ddlFilterByDDL($scope.newList, 'FixedAssetSubCategoryId', 'FixedAssetSubCategoryName');
    }
    $scope.multiSelectSettings = {
        scrollableHeight: 'auto',
        smartButtonMaxItems: 3,
        scrollable: true,
        showCheckAll: false,
        showUncheckAll: false,
        enableSearch: true,
        dynamicTitle: true
    };
    $scope.fixedassetCategoryIds = [];
    $scope.multi3events = {
        onItemSelect: function (item) {
            //if ($scope.fixedassetCategoryIds.length > 0) {
            $scope.cboCratetor($scope.fixedassetCategoryIds, 'FixedAssetCategoryId');
            //}
        }, onItemDeselect: function (item) {
            //if ($scope.fixedassetCategoryIds.length > 0) {
            $scope.cboCratetor($scope.fixedassetCategoryIds, 'FixedAssetCategoryId');
            //}
        }
    };

    $scope.fixedassetSubCategoryIds = [];
    $scope.multi4events = {
        onItemSelect: function (item) {
            //if ($scope.fixedassetSubCategoryIds.length > 0) {
            $scope.cboCratetor($scope.fixedassetSubCategoryIds, 'FixedAssetSubCategoryId');
            //}
        }, onItemDeselect: function (item) {
            //if ($scope.fixedassetCategoryIds.length > 0) {
            $scope.cboCratetor($scope.fixedassetSubCategoryIds, 'FixedAssetSubCategoryId');
            //}
        }
    };
    function validListWithStr(list, values) {
        var tempValues = [];
        for (var i = 0; i < values.length; i++) {
            for (var j = 0; j < list.length; j++) {
                if (values[i].Value === list[j].Value) {
                    tempValues.push(values[i]);
                }
            }
        }
        return tempValues;
    }

    /***********cbo*************/
    $scope.DepreciationRuleList = [];
    $scope.CompanyList = [];
    cboService.getCboDepreciationRule(function (result) {
        $scope.DepreciationRuleList = result;
    });

    cboService.getCboCompanyByCompanyGroup(' ', function (result) {
        $scope.CompanyList = result;
    });

    /***/
    // #region ******DepreciationType GL******
    $scope.searchDepreciationTypeByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Description',
            'value': 'Description'
        },
        {
            'name': 'DepreciationRules',
            'value': 'DepreciationRules'
        },
        {
            'name': 'DepreciationCharge',
            'value': 'DepreciationCharge'
        }
    ];
    $scope.depreciationListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetDepreciationTypeList = function () {
        $scope.Url = 'fixedassets/fixedAssetdepreciationrule/GetList';
        $scope.GetDepreciationTypeListData = function (pageno) {
            baseService.paginationBase($scope.Url, pageno, $scope.depreciationListParameters)
                .then(function (data) {
                    $scope.DepreciationTypeGLList = data.Rows;
                    $scope.depreciationListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#DepreciationTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetDepreciationTypeListData();
    };
    $scope.setDepreciationGLSelected = function (x) {
        $scope.rowSelected = x.Id;
        $scope.DepreciationRuleId = x.Id;
        $scope.DepreciationRuleCode = x.Code;
        $scope.DepreciationRuleText = x.Code + ' - ' + x.Description;
    };
    $scope.closeDepreciationTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#DepreciationTypeListPopUp')).modal('hide');
        }
    };
    $scope.clearDepreciationRule = function () {
        $scope.DepreciationRuleId = null;
        $scope.DepreciationRuleCode = null;
        $scope.DepreciationRuleText = null;
    };

    // #endregion
    $scope.getD = function (list) {
        angular.forEach($scope.DepreciationRuleList, function (item) {
            if (item.Value === list) {
                $scope.depcode = item.Code;
            }
        });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getdepreciationRule = angular.copy($scope.companyDepreciationRules[$scope.index]);
        $scope.companyDepreciationRule = $scope.getdepreciationRule;
        $scope.companyDepreciationRule.AddedDate = $filter('dateFilter')($scope.companyDepreciationRule.AddedDate);
        $scope.companyDepreciationRule.UpdatedDate = $filter('dateFilter')($scope.companyDepreciationRule.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.addDepreciationRuleForSelectble = function () {
        if ($scope.tempList.length === 0) {
            throw ShowResult("Please select any Rows", 'failure');
        }
        $scope.CompanyDepreciaitonRuleWithCombineListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if (!baseService.isUndefinedOrNull($scope.DepreciationRuleId)) {
                    item.DepreciationRuleId = $scope.DepreciationRuleId;
                    item.CompanyId = $scope.companyDepreciationRule.CompanyId;
                    $scope.CompanyDepreciaitonRuleWithCombineListForSave.push(item);
                } else {
                    throw ShowResult("Please select Depreciation Rule", 'failure');
                }
            }
        });
    };

    $scope.Save = function () {
        $scope.addDepreciationRuleForSelectble();
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.companyDepreciationRuleForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'CompanyDepreciationRule': $scope.CompanyDepreciaitonRuleWithCombineListForSave },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getFixedAssetMasterWithCoa("all");
                        $scope.companyDepreciationRules = $filter('orderBy')($scope.companyDepreciationRules, 'DepreciationRuleId');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.companyDepreciationRule,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.companyDepreciationRules[$scope.index] = $scope.companyDepreciationRule;
                            $scope.companyDepreciationRules = $filter('orderBy')($scope.companyDepreciationRules, 'DepreciationRuleId');
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };
    //GL Untagging
    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.FixedAssetMasterName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.CompanyDepreciaitonRuleWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.CompanyDepreciaitonRuleWithCombineList[i].Id == $scope.glUntagId) {
                    $scope.unTagGL($scope.glUntagId, $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetMasterId, i);
                    break;
                }
            } else {
                unTagFromList($scope.glUntagIndex);
                $scope.glUntagIndex = -1;
                break;
            }
        }
        $scope.mauid = null;
        $scope.mauindex = -1;
    };
    function unTagFromList(i) {
        $scope.CompanyDepreciaitonRuleWithCombineList[i] = {
            FixedAssetMasterName: $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetMasterName,
            FixedAssetMasterId: $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetMasterId,
            FixedAssetCategoryName: $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetCategoryName,
            FixedAssetSubCategoryName: $scope.CompanyDepreciaitonRuleWithCombineList[i].FixedAssetSubCategoryName
        };
    }
    $scope.unTagGL = function (id, FixedAssetMasterId, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + '/Delete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < $scope.tempList.length; i++) {
                        if ($scope.tempList[i].FixedAssetMasterId === FixedAssetMasterId) {
                            document.getElementById($scope.tempList[i].FixedAssetMasterId).checked = false;
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }
                    unTagFromList(index);
                    $scope.glUntagIndex = -1;
                    //angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.clearSearchDropDown = function () {
        $scope.fixedassetCategoryIds = [];
        $scope.fixedassetSubCategoryIds = [];
        getListForm($scope.CompanyDepreciaitonRuleWithCombineListForDdl);
    };

    $scope.Clear = function () {
        $scope.clearSearchDropDown();
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.companyDepreciationRule = { CompanyId: $scope.companyDepreciationRule.CompanyId };
        $scope.tempList = [];
        $scope.getFixedAssetMasterWithCoa("all");
        $scope.clearDepreciationRule();
    }
}