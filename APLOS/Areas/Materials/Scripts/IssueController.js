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
        MaterialLevel:null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelTempC = {
        Id: null,
        Machine: null,
        WorkCenter: null,
    };
    $scope.ModelNewC = Object.assign({}, $scope.ModelTempC);
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

    // #region ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//


    $scope.userMaterialArticleList = [];
    $scope.getMaterialArticleId = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMaterialArticleId",
            data: {
                'materialTypeId': $scope.ModelNew.MaterialTypeId,
                'materialMasterId': $scope.ModelNew.MaterialMasterId,
                'materialGroupMasterId': $scope.ModelNew.MaterialGroupMasterId,
                'storagelevel': $scope.ModelNew.MaterialLevel,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialArticleList = response.data;
            // #region cmnt
            $scope.hideshow();
            for (var i = 0; i < $scope.userMaterialArticleList.length; i++) {
                for (var j = 0; j < $scope.MaterialArticleList.length; j++) {
                    if ($scope.userMaterialArticleList[i].Id === $scope.MaterialArticleList[j].Id) {
                        $scope.MaterialArticleList[j].chk = true;
                    }
                }
            }
            // #endregion cmnt
        })
    }
    // #endregion ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//

    // #endregion GET FUN

    // #region save
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
    // #endregion save

    $scope.sendItemApplicable = function () {
        for (var i = 0; i < $scope.MaterialArticleList.length; i++) {
            $scope.MaterialArticleList[i].MachineApplicable = $scope.MachineApplicable;
            $scope.MaterialArticleList[i].WorkcenterApplicable = $scope.WorkcenterApplicable;
            $scope.MaterialArticleList[i].SelectedOrderLevel = $scope.OrderLevel;
        }
        if ($scope.ModelNew.MaterialLevel == "Material") {
            var gridObj = $("#GridEdit").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        }
        else {
            var gridObj = $("#GridEditB").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        }
       
    }

    // #region SAVE CHILD
    $scope.SaveItemApplicable = function () {
        
        $scope.$broadcast('show-errors-check-validity');
       
        $http({
            method: 'POST',
            url: $scope.path + 'SaveItemApplicable',
            data: {
                'headerId': $scope.ModelNew.Id,
                'machineApplicable': $scope.MachineApplicable,
                'worckcenterApplicable': $scope.WorkcenterApplicable,
                'orderlevel': $scope.OrderLevel
                
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
    $scope.SaveIssueControlChild = function () {
        if (baseService.arrayLength($scope.MaterialArticleList) > 0) {
            angular.forEach($scope.MaterialArticleList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = null;
                    ob.ArticleName = a.ArticleName;
                    ob.MaterialGroupMasterId = a.MaterialGroupMasterId;
                    ob.MaterialMaster = a.MaterialMaster;
                    ob.MaterialMasterId = a.MaterialMasterId;
                    ob.MaterialMasterArticleId = a.MaterialMasterArticleId;
                    ob.MaterialType = a.MaterialType;
                    ob.MaterialTypeId = a.MaterialTypeId;
                    ob.MaterialgroupName = a.MaterialgroupName;
                   
                   
                    ob.chk = a.chk;

                    ob.StorageBinMasterId = a.StorageBinMasterId;

                    $scope.userMaterialArticleList.push(ob);
                    ob = {};
                    a.chk = false;
                }
            });
        }
        
        $scope.$broadcast('show-errors-check-validity');
        for (var i = 0; i < $scope.userMaterialArticleList.length; i++) {
            $scope.userMaterialArticleList[i].MachineApplicable = $scope.MachineApplicable;
            $scope.userMaterialArticleList[i].WorkcenterApplicable = $scope.WorkcenterApplicable;
            $scope.userMaterialArticleList[i].OrderLevel = $scope.OrderLevel;
        }
        $http({
            method: 'POST',
            url: $scope.path + 'SaveChild',
            data: {
                'headerId': $scope.ModelNew.Id,
                'data': $scope.userMaterialArticleList,
                'materiallevel': $scope.ModelNew.MaterialLevel
                
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                
                $scope.GetIssue();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };
    // #endregion SAVE CHILD

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
        
        $scope.userMaterialArticleList = [];
        
        

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

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
}
