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

    // #region Send
    //$scope.sendItemApplicable = function () {
    //    for (var i = 0; i < $scope.MaterialArticleList.length; i++) {
    //        $scope.MaterialArticleList[i].MachineApplicable = $scope.MachineApplicable;
    //        $scope.MaterialArticleList[i].WorkCenterApplicable = $scope.WorkCenterApplicable;
    //        $scope.MaterialArticleList[i].OrderLevel = $scope.OrderLevel;
    //        //$scope.SelectedOrderLevel=$scope.OrderLevel;
    //    }
    //    if ($scope.ModelNew.MaterialLevel == "Material") {
    //        var gridObj = $("#GridEdit").data("ejGrid");
    //        gridObj.refreshContent(true);
    //        gridObj.refreshTemplate();
    //    }
    //    else {
    //        var gridObj = $("#GridEditB").data("ejGrid");
    //        gridObj.refreshContent(true);
    //        gridObj.refreshTemplate();
    //    }
       
    //}
     // #endregion Send

    // #region checkbox all for Material
    
    //$scope.refreshMaterialArticle = function (args) {
    //    $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllMaterialArticle });
    //};

    // function CheckBoxSelectAllMaterialArticle(e) {
    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;

    //    }

    //    var filtered = $("#GridEdit").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.MaterialArticleList.length; i++) {
    //            $scope.MaterialArticleList[i].chk = ChkOrUnchk;
    //            $scope.MaterialArticleList[i].MachineApplicable = $scope.MachineApplicable;
    //            $scope.MaterialArticleList[i].WorkCenterApplicable = $scope.WorkCenterApplicable;
    //            $scope.MaterialArticleList[i].OrderLevel = $scope.OrderLevel;


    //        }
    //    }
    //    else {
    //        for (var j = 0; j < filtered.length; j++) {

    //            filtered[j].CheckBoxSelect = ChkOrUnchk;
    //       }

    //    }
    //    var gridObj = $("#GridEdit").data("ejGrid");
    //    gridObj.refreshContent();
    //};

   // #endregion checkbox all

    // #region checkbox all for Article

    //$scope.refreshArticle = function (args) {
    //    $("#headchkB").ejCheckBox({ "change": CheckBoxSelectAllArticle });
    //};

    //function CheckBoxSelectAllArticle (e) {
    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;
    //    }

    //    var filtered = $("#GridEditB").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.MaterialArticleList.length; i++) {
    //            $scope.MaterialArticleList[i].chk = ChkOrUnchk;
    //            $scope.MaterialArticleList[i].MachineApplicable = $scope.MachineApplicable;
    //            $scope.MaterialArticleList[i].WorkCenterApplicable = $scope.WorkCenterApplicable;
    //            $scope.MaterialArticleList[i].OrderLevel = $scope.OrderLevel;
    //        }
    //    }
    //    else {
    //        for (var j = 0; j < filtered.length; j++) {

    //            filtered[j].CheckBoxSelect = ChkOrUnchk;
    //        }
    //    }
    //    var gridObj = $("#GridEditB").data("ejGrid");
    //    gridObj.refreshContent();
    //};



   // #endregion checkbox all for Article

    // #region
    $scope.changeDropDown = function () {
        $scope.MAObject.OrderLevelText = $("#OrderLevelIdPOP option:selected").text();
    }

    function checkExistTempList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    //$scope.tempList = [];
    //$scope.OrderLevelText = $("#OrderLevelId option:selected").text();
    //$scope.selectChValueId = function (event, data) {
    //    try {
    //        if (event.currentTarget.checked) {
    //            if (checkExistTempList($scope.tempList, data.MaterialMasterId) === false) {
                    
    //                data.WorkCenterApplicable = $scope.WorkCenterApplicable;
    //                data.MachineApplicable = $scope.MachineApplicable;
    //                data.OrderLevel = $scope.OrderLevel;
    //                data.OrderLevelText = $("#OrderLevelId option:selected").text();
    //                $scope.tempList.push(data);
    //            }
    //        }
    //        else {
    //            for (var i = 0; i < $scope.tempList.length; i++) {
    //                if ($scope.tempList[i].Id === data.MaterialMasterId) {
    //                    $scope.tempList.splice(i, 1);
    //                    break;
    //                }
    //            }
    //        }
    //    } catch (e) {
    //        event.currentTarget.checked = false;
    //        ShowResult(e, "failure");
    //    }
    //};


    //$scope.CheckAll = function (event) {
    //    var _isselected = event.target.checked;


    //    for (var i = 0; i < $scope.MaterialArticleList.length; i++) {
    //        $scope.MaterialArticleList[i].chk = _isselected;
    //    }

    //    for (var i = 0; i < baseService.arrayLength($scope.MaterialArticleList); i++) {
    //        if (_isselected)
    //            $scope.tempList.push($scope.MaterialArticleList[i]);
    //        else
    //            for (var j = 0; j < $scope.tempList.length; j++) {
    //                if ($scope.tempList[j].Id === $scope.MaterialArticleList[i].MaterialMasterId) {
    //                    $scope.tempList.splice(j, 1);
    //                    break;
    //                }
    //            }
    //    }
    //};
    // #endregion

    // #region SAVE CHILD

    // #region Save Item Applicable
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

    $scope.SaveIssueControlChild = function () {
        if ($scope.ModelNew.MaterialLevel == 'Material') {
            var materialMasterIds = "''";
            for (var m = 0; m < $scope.MaterialLevelList.length; m++) {
                materialMasterIds += ",'" + $scope.MaterialLevelList[m].MaterialMasterId + "'";
            }
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + 'UpdateMaterialMasterForIssueControl',
                data: {
                    'data': $scope.MaterialLevelList,
                    'materiallevel': $scope.ModelNew.MaterialLevel,
                    'materialMasterIds': materialMasterIds
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
            var materialMasterIds = "''";
            for (var m = 0; m < $scope.MaterialArticleList.length; m++) {
                materialMasterIds += ",'" + $scope.MaterialArticleList[m].Id + "'";
            }
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + 'UpdateMaterialMasterForIssueControl',
                data: {
                    'data': $scope.MaterialArticleList,
                    'materiallevel': $scope.ModelNew.MaterialLevel,
                    'materialMasterIds': materialMasterIds
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
    // #endregion SAVE CHILD

    // #region Get Issue save Data
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
    // #endregion Get Issue save Data

    // #region Double tab on row
    $scope.Get = function (args) {
        $scope.ModelNew.Id = args.data.Id;
        $scope.ModelNew = Object.assign({}, args.data);

        $scope.Action = 'Update';

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };
     // #endregion Double tab on row

    // #region Clear

    $scope.Clear = function () {
        ClearFields();
        return true;
    };


    function ClearFields() {
        
        $scope.userMaterialArticleList = [];
        
        

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
    // #endregion Clear

    // #region Hide & show Material/Article
    $scope.hideshow = function () {
        var id = document.getElementById("ArticleId");
       
        var mid = document.getElementById("MaterialId");
       
        if ($scope.ModelNew.MaterialLevel == "Article") {
            id.style.display = "block";
            
            mid.style.display = "none";
        }
        else if ($scope.ModelNew.MaterialLevel == "Material") {
            id.style.display = "none";
            
            mid.style.display = "block";

        }
    }
    // #endregion Hide & show Material/Article

    // #region MAObject
    $scope.MAObject = {
        Id: null,
        WorkCenterApplicable: false,
        MachineApplicable: false,
        OrderLevel: null,
        OrderLevelText:null
        
    }
     // #endregion MAObject

    // #region Item Applicable Pop Up
    $scope.openItemApplicablePopUp = function (obj) {
        $scope.MAObject = obj.x;
       
        angular.element(document.querySelector('#itemApplicablePopUpid')).modal('show');
        
    }

    $scope.closeItemApplicablePopUp = function () {
       
        angular.element(document.querySelector('#itemApplicablePopUpid')).modal('hide');
        //if ($scope.ModelNew.MaterialLevel == "Material") {
        //    var gridObj = $("#GridEdit").data("ejGrid");
        //    gridObj.refreshContent(true);
        //    gridObj.refreshTemplate();
        //}
        //else {
        //    var gridObj = $("#GridEditB").data("ejGrid");
        //    gridObj.refreshContent(true);
        //    gridObj.refreshTemplate();
        //}
    }
    // #endregion Item Applicable Pop Up

    
}
