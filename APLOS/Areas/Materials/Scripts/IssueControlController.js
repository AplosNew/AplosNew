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
    //  #region Objects
    $scope.ModelTemp = {
        Id: null,
        UserName: null,
        StorageBinmasterId: null,
        
        MaterialTypeId: null,
        MaterialGroupMasterId: null,
        MaterialMasterId: null,
        MaterialMasterArticleId: null,
        
        Remarks: null,
        StorageLevel: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //  #endregion Objects

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

    // #region ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//


    $scope.userMaterialList = [];
    $scope.getMaterialArticleId = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterialArticleId",
            data: {
                'materialTypeId': $scope.ModelNew.MaterialTypeId,
                'materialMasterId': $scope.ModelNew.MaterialMasterId,
                'materialGroupMasterId': $scope.ModelNew.MaterialGroupMasterId,                
                'storagelevel': $scope.ModelNew.StorageLevel,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialArticleList = response.data;
            // #region cmnt
            //
            //for (var i = 0; i < $scope.userMaterialList.length; i++) {
            //    for (var j = 0; j < $scope.BinHeadList.length; j++) {
            //        if ($scope.userMaterialList[i].Id === $scope.BinHeadList[j].Id) {
            //            $scope.BinHeadList[j].chk = true;
            //        }
            //    }
            //}
             // #endregion cmnt
        })
    }
    // #endregion ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//
}      
   // #endregion GET FUN
