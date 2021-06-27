'use strict';
EntitySFGInventoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', 'cboService'];
function EntitySFGInventoryController(commonMessage, $scope, $rootScope, baseService, $http, cboService, ) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.EntitySFGInventoryList = [];
    $scope.SFGInventoryList = [];
    $scope.path = 'Products/EntitySFGInventory/';
    $scope.getEntitySFGInventoryListUrl = $scope.path + 'getlist?entityId=';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteGraphUrl = $scope.path + 'deleteGraph?entityId';
    $scope.getData = function (pageno) {
        $rootScope.tempList = [];
        $scope.EntitySFGInventoryList = [];
        $http.get($scope.getEntitySFGInventoryListUrl + $scope.EntitySFGInventory.EntityId)
            .then(function (response) {
                $scope.EntitySFGInventoryList = response.data.Rows;
            });
    };

    $scope.EntitySFGInventory = {
        Id: null
        , CompanyId: null
        , EntityId: null
        , ProcessId: null
        , PlantId: null
        , SFGInventoryId: null
        , LotNumberCapture: false
        , LotNumberMandatory: false
    };

    // #region DDL
    $scope.UncheckMandatory = function (data) {

        if (data.LotNumberCapture == false) {
            data.LotNumberMandatory = false;
        }
    }
    $scope.productionBookingLevelList = [];
    cboService.getEnumCbo("enum/GetEnumProductionBookingLevelCbo", function (result) {
        $scope.productionBookingLevelList = result;
    });
    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.EntitySFGInventory.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };


    $scope.entityList = [];
    $scope.getEntity = function () {
        $scope.entities = [];
        $scope.EntitySFGInventoryList = [];
       
        $http({
            method: 'POST',
            url: "Processes/EntityProcessTag/GetEntity?plantId=" + $scope.EntitySFGInventory.PlantId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    };

    // #endregion
  
    // #region POP UP

    //$scope.processParameters = {
    //    limit: 10
    //    , offset: 0
    //    , order: 'asc'
    //    , sort: 'UserName'
    //    , searchBy: "UserName"
    //    , pageSize: 10
    //    , total_count: 0
    //    , search: null
    //    , serverPagination: true
    //};
    //$scope.SFGInventoryPopUp = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if (!$scope.modelForm.$valid) return;
    //    $rootScope.tempList = [];
    //    angular.forEach($scope.EntitySFGInventoryList, function (a) {
    //        $rootScope.tempList.push({
    //            Id: a.SFGInventoryId
    //            , Sequence: a.Sequence
    //            , Code: a.Code
    //            , ShortName: a.ShortName
    //            , StandardName: a.StandardName
    //            , UserName: a.UserName
    //        });
    //    });
    //    baseService.setCurrentPage('SFGInventoryList');
    //    $scope.getSFGData = function (pageno) {
    //        $scope.getProcessUrl = 'Products/SFGInventory/GetList';
    //        baseService.paginationBase($scope.getProcessUrl, pageno, $scope.processParameters)
    //            .then(function (result) {
    //                $scope.SFGInventoryList = result.Rows;
    //                $scope.processParameters.total_count = result.Total;
    //                for (var t = 0; t < baseService.arrayLength($scope.SFGInventoryList); t++) {
    //                    $scope.SFGInventoryList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.SFGInventoryList[t].Id);
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#SFGPopUp')).modal('show');
    //    $scope.getSFGData();
    //};
    //$scope.CloseSFGPopUp = function () {
    //    $rootScope.tempList = [];
    //    angular.element(document.querySelector('#SFGPopUp')).modal('hide');
    //};
    //$rootScope.searchProcessByList = [
    //    {
    //        'name': 'Sequence',
    //        'value': 'Sequence'
    //    },
    //    {
    //        'name': 'Code',
    //        'value': 'Code'
    //    },
    //    {
    //        'name': 'ShortName',
    //        'value': 'ShortName'
    //    },
    //    {
    //        'name': 'Standard Name',
    //        'value': 'StandardName'
    //    },
    //    {
    //        'name': 'UserName',
    //        'value': 'UserName'
    //    },
    //    {
    //        'name': 'Material Type',
    //        'value': 'MaterialType'
    //    }
    //];

    //$scope.addSFG = function () {
    //    if (baseService.arrayLength($scope.SFGInventoryList) === 0)
    //        return ShowResult('Please select at least one row!', 'failure', 'SFGPopUp');
    //    if (baseService.arrayLength($rootScope.tempList) > 0) {
    //        angular.forEach($rootScope.tempList, function (a) {
    //            if (!baseService.valueCheckInList($scope.EntitySFGInventoryList, 'SFGInventoryId', a.Id)) {
    //                $scope.EntitySFGInventoryList.push({
    //                    Id: null
    //                    , EntityId: $scope.EntitySFGInventory.EntityId
    //                    , SFGInventoryId: a.Id
    //                    , Sequence: a.Sequence
    //                    , Code: a.Code
    //                    , ShortName: a.ShortName
    //                    , StandardName: a.StandardName
    //                    , UserName: a.UserName
    //                });
    //            }
    //        });
    //    }
    //    else
    //        $scope.EntitySFGInventoryList = [];
    //    angular.forEach($scope.EntitySFGInventoryList, function (a) {
    //        if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.SFGInventoryId))
    //            $scope.EntitySFGInventoryList.splice(a, 1);
    //    });
    //    $scope.CloseSFGPopUp();
    //};

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to permanently delete [" + name + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope[$scope.listName][$scope.popUpIndex].Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope[$scope.listName][$scope.popUpIndex].Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true)
                    ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };



    $scope.userSFGInventoryList = [];
    $scope.SFGInventoryPopUp = function () {
        $scope.SFGInventoryDataList = [];
        $scope.SFGInventorySearchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.SFGInventoryPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.SFGInventoryUrl = 'Products/SFGInventory/GetList';

        baseService.setCurrentPage('SFGInventoryDataList');
        $scope.getSFGInventoryDataList = function (pageno) {
            baseService.paginationBase($scope.SFGInventoryUrl, pageno, $scope.SFGInventoryPopUpParameters)
                .then(function (result) {
                    $scope.SFGInventoryDataList = result.Rows;
                    $scope.SFGInventoryPopUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.EntitySFGInventoryList) > 0) {
                        for (var i = 0; i < $scope.EntitySFGInventoryList.length; i++) {
                            for (var j = 0; j < $scope.SFGInventoryDataList.length; j++) {
                                if ($scope.EntitySFGInventoryList[i].SFGInventoryId === $scope.SFGInventoryDataList[j].Id) {
                                    $scope.SFGInventoryDataList[j].Flag = true;
                                }
                            }
                        }
                    }


                    if (baseService.arrayLength($scope.SFGInventorySearchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.SFGInventorySearchList);
                    angular.element(document.querySelector('#SFGPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'SFGPopUp');
                }).finally(function () {
                });
        };
        $scope.getSFGInventoryDataList();
    };

    $scope.addSFG = function () {
        if (baseService.arrayLength($scope.SFGInventoryDataList) > 0) {
            angular.forEach($scope.SFGInventoryDataList, function (a) {
                // if (!baseService.valueCheckInList($scope.userSFGInventoryList, 'SFGInventoryId', a.Id)) {
                if (checkSFGInventoryExist($scope.EntitySFGInventoryList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.EntitySFGInventoryList.push({
                            Id: null
                            , EntityId: $scope.EntitySFGInventory.EntityId
                            , SFGInventoryId: a.Id
                            , Code: a.Code
                            , Sequence: a.Sequence
                            , ShortName: a.ShortName
                            , UserName: a.UserName
                            , StandardName: a.StandardName
                            , ProductionBookingLevel:null
                        });
                    }
                }
            });
        }
        else
            $scope.EntitySFGInventoryList = [];
        angular.forEach($scope.EntitySFGInventoryList, function (a) {
            if (!baseService.valueCheckInList($scope.SFGInventoryDataList, 'Id', a.SFGInventoryId))
                $scope.EntitySFGInventoryList.splice(a, 1);
        });
        $scope.CloseSFGPopUp();
    };

    $scope.CloseSFGPopUp = function () {
        $scope.SFGInventoryUpUrl = null;
        $scope.SFGInventoryDataList = [];
        $scope.SFGInventorySearchList = [];
        angular.element(document.querySelector('#SFGPopUp')).modal('hide');
    };

    function checkSFGInventoryExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SFGInventoryId === Id) {
                return true;
            }
        }
        return false;
    }



    // #endregion
  
    $scope.Save = function () {
        try {

            if (baseService.arrayLength($scope.EntitySFGInventoryList) === 0) {
                throw 'No data found.';
            } 
            $http({
                method: 'POST'
                , url: 'Products/EntitySFGInventory/create'
                , data: $scope.EntitySFGInventoryList
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        $http({
            method: 'POST'
            , url: $scope.deleteGraphUrl + $scope.EntitySFGInventory.EntityId
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true)
                ShowResult(response.data.Message, "failure");
            else {
                ShowResult(response.data.Message, "success");
                $scope.Clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
    }

    $scope.Clear = function () {
        $scope.tableShow = false;
        $scope.EntitySFGInventory = {};
        $scope.entities = [];
        $scope.entityValue = [];
        $scope.EntitySFGInventoryList = [];
    }
}