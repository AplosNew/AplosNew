'use strict';
IssueControlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function IssueControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Issue Control';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/IssueControl/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    $scope.ModelTemp = {
        Id: null,
        UserName: null,
        MaterialLevel: null,
        IsMachineApplicable:false,
        IsWorkCenterApplicable:false,
        OrderLevel:null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelTempC = {
        Id: null,
        Machine: null,
        WorkCenter: null,
    };
    $scope.ModelNewC = Object.assign({}, $scope.ModelTempC);

    //  #region All Lists
    $scope.MaterialTypeList = [];
    $scope.MaterialGroupList = [];
    $scope.MaterialList = [];
    $scope.MaterialArticleList = [];
    //  #endregion All Lists

    // #region GET FUN
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

        });
    }
    $scope.getMaterialGroup();

    $scope.getMaterial = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterial",
            data: { 'materialgroupid': $scope.ModelNew.MaterialGroupMasterId, },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialList = response.data;
        });
    }
    $scope.getMaterial();

    // #region get Define Enum
    $scope.EnumList = [];
    $scope.getEnum = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEnum",           
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EnumList = response.data;
        });
    }
    $scope.getEnum();
     // #endregion get Define Enum

    $scope.ItemApplicableList = [];
    $scope.getItemApplicable = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetItemApplicable",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ItemApplicableList = response.data;
        });
    }
    // #region ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//


    $scope.MaterialArticleList = [];
    $scope.MaterialLevelList = [];
    $scope.ViewMaterialAndArticle = function () {
        $scope.MaterialArticleList = [];
        $scope.MaterialLevelList = [];
        $http({
            method: 'POST',
            url: $scope.path + "GetMaterialAndArticle",
            data: {
                'materialTypeId': $scope.ModelNew.MaterialTypeId,
                'materialMasterId': $scope.ModelNew.MaterialMasterId,
                'materialGroupMasterId': $scope.ModelNew.MaterialGroupMasterId,
                'storagelevel': $scope.ModelNew.MaterialLevel,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if ($scope.ModelNew.MaterialLevel =='Article')
                $scope.MaterialArticleList = response.data;
            else
                $scope.MaterialLevelList = response.data;
        })
    }


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModelNew,                
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ModelNew.Id = response.data.Data.Id;
                $scope.ModelNew.MaterialLevel = response.data.Data.MaterialLevel;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };


    $scope.refreshMaterial = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllMaterial });
    };

    function CheckBoxSelectAllMaterial(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridMaterial").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MaterialLevelList.length; i++) {
                $scope.MaterialLevelList[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridMaterial").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.refreshArticle = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllArticle });
    };

    function CheckBoxSelectAllArticle(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridArticle").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MaterialArticleList.length; i++) {
                $scope.MaterialArticleList[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridArticle").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.changeDropDown = function () {
        $scope.MAObject.OrderLevelText = $("#OrderLevelIdPOP option:selected").text();
    }

    $scope.Apply = function () {
        var orderLevelText = $.grep($scope.EnumList, function (item) {
            return item.Value == $scope.ModelNew.OrderLevel;
        })[0].Text;
        if ($scope.ModelNew.MaterialLevel == 'Article') {
            for (var i = 0; i < $scope.MaterialArticleList.length; i++) {
                $scope.MaterialArticleList[i].IsMachineApplicable = $scope.ModelNew.IsMachineApplicable
                $scope.MaterialArticleList[i].IsWorkCenterApplicable = $scope.ModelNew.IsWorkCenterApplicable
                $scope.MaterialArticleList[i].OrderLevelText = orderLevelText
                $scope.MaterialArticleList[i].OrderLevel = $scope.ModelNew.OrderLevel
            }
        }
        if ($scope.ModelNew.MaterialLevel == 'Material') {
            for (var i = 0; i < $scope.MaterialLevelList.length; i++) {
                $scope.MaterialLevelList[i].IsMachineApplicable = $scope.ModelNew.IsMachineApplicable
                $scope.MaterialLevelList[i].IsWorkCenterApplicable = $scope.ModelNew.IsWorkCenterApplicable
                $scope.MaterialLevelList[i].OrderLevelText = orderLevelText
                $scope.MaterialLevelList[i].OrderLevel = $scope.ModelNew.OrderLevel
            }
        }
        
        var gridObj = $("#GridMaterial").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        var gridArticleObj = $("#GridArticle").data("ejGrid");
        gridArticleObj.refreshContent(true);
        gridArticleObj.refreshTemplate();
    }
    //have to update
    $scope.SaveItemApplicable = function () {
        
        $scope.$broadcast('show-errors-check-validity');
       
        $http({
            method: 'POST',
            url: $scope.path + 'SaveItemApplicable',
            data: {
                'headerId': $scope.ModelNew.Id,
                'machineApplicable': $scope.MachineApplicable,
                'worckcenterApplicable': $scope.WorkCenterApplicable,
                'orderlevel': $scope.OrderLevel
                
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMaterialArticleId();
                
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };
     // #endregion Save Item Applicable

    $scope.materialProcess = function() {
        $scope.materialMasterIds = "''";
        $scope.materialMasterTempList = [];
        for (var t = 0; t < $scope.MaterialLevelList.length; t++) {
            if ($scope.MaterialLevelList[t].isSelected == true) {
                $scope.materialMasterIds += ",'" + $scope.MaterialLevelList[t].MaterialMasterId + "'";
                $scope.materialMasterTempList.push($scope.MaterialLevelList[t]);
            }
        }
    }
    $scope.articleProcess = function () {
        $scope.articleIds = "''";
        $scope.materialArticleTempList = [];
        for (var t = 0; t < $scope.MaterialArticleList.length; t++) {
            if ($scope.MaterialArticleList[t].isSelected == true) {
                $scope.articleIds += ",'" + $scope.MaterialArticleList[t].Id + "'";
                $scope.materialArticleTempList.push($scope.MaterialArticleList[t]);
            }
        }
    }
    $scope.SaveIssueControlChild = function () {
        if ($scope.ModelNew.MaterialLevel == 'Material') {
            $scope.materialProcess();
            
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + 'UpdateMaterialMasterForIssueControl',
                data: {
                    'data': $scope.materialMasterTempList,
                    'materiallevel': $scope.ModelNew.MaterialLevel,
                    'materialMasterIds': $scope.materialMasterIds
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.tempList = [];
                    $scope.GetIssue();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        else {
            $scope.articleProcess();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + 'UpdateMaterialMasterForIssueControl',
                data: {
                    'data': $scope.materialArticleTempList,
                    'materiallevel': $scope.ModelNew.MaterialLevel,
                    'materialMasterIds': $scope.articleIds
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.tempList = [];
                    $scope.GetIssue();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };
   
    $scope.GetIssue = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetIssue',           
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data
        });
    }
    $scope.GetIssue();
   
    $scope.Get = function (args) {
        $scope.ModelNew.Id = args.data.Id;
        $scope.ModelNew = Object.assign({}, args.data);

        $scope.Action = 'Update';

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };
   
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.materialArticleTempList = [];
        $scope.MaterialArticleList = [];
        $scope.materialMasterTempList = [];
        $scope.MaterialLevelList = [];
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
   
}
