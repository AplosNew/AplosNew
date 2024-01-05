'use strict';
EntityProcessTagController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', 'cboService'];
function EntityProcessTagController(commonMessage, $scope, $rootScope, baseService, $http, cboService, ) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.entityProcessTagList = [];
    $scope.processList = [];
    $scope.path = 'Processes/entityProcessTag/';
    $scope.getEntityProcessTagListUrl = $scope.path + 'getlist?entityId=';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteGraphUrl = $scope.path + 'deleteGraph?entityId';
    $scope.getData = function (pageno) {
        $rootScope.tempList = [];
        $scope.entityProcessTagList = [];
        $http.get($scope.getEntityProcessTagListUrl + $scope.entityProcessTag.EntityId)
            .then(function (response) {
                $scope.entityProcessTagList = response.data.Rows;
                for (var i = 0; i < $scope.entityProcessTagList.length; i++) {
                    $scope.entityProcess.IsFinishGoods = $scope.entityProcessTagList[i].IsFinishGoods;
                    $scope.entityProcess.ProcessNature = $scope.entityProcessTagList[i].ProcessNature;
                    $scope.entityProcess.IsPackingFromContSKU = $scope.entityProcessTagList[i].IsPackingFromContSKU;
                    $scope.entityProcess.IsDispatchGrpApplicable = $scope.entityProcessTagList[i].IsDispatchGrpApplicable;
                    $scope.entityProcess.PackingUoM = $scope.entityProcessTagList[i].PackingUoM;
                    $scope.entityProcess.DispatchUoM = $scope.entityProcessTagList[i].DispatchUoM;
                    $scope.entityProcess.LotNumberCapture = $scope.entityProcessTagList[i].LotNumberCapture;
                    $scope.entityProcess.LotNumberMandatory = $scope.entityProcessTagList[i].LotNumberMandatory;
                    $scope.entityProcess.IsSKU1 = $scope.entityProcessTagList[i].IsSKU1;
                    $scope.entityProcess.IsSKU2 = $scope.entityProcessTagList[i].IsSKU2;
                    $scope.entityProcess.IsSKU3 = $scope.entityProcessTagList[i].IsSKU3;
                    $scope.entityProcess.IsScanApplicable = $scope.entityProcessTagList[i].IsScanApplicable;

                }
            });
    };

    $scope.entityProcessTag = {
        Id: null
        , CompanyId: null
        , EntityId: null
        , ProcessId: null
        , IsFinishGoods: false
        , ProcessNature: null
        , IsPackingFromContSKU: false
        , IsDispatchGrpApplicable: false
        , PackingUoM: null
        , DispatchUoM: null
        , ProductionBookingLevel:null
        , PlantId: null
        , LotNumberCapture: false
        , LotNumberMandatory:false
        , IsSKU1:false
        , IsSKU2:false
        , IsSKU3:false
        , IsScanApplicable:false
    };

    $scope.entityProcess = {
        IsFinishGoods: false
        , ProcessNature: null
        , IsPackingFromContSKU: false
        , IsDispatchGrpApplicable: false
        , PackingUoM: null
        , DispatchUoM: null
        , LotNumberCapture: false
        , LotNumberMandatory: false
        , IsPackingSKURequired: false
        , PackingForm: null
        , IsDispatchSKURequired: false
        , DispatchForm: null
        , DispatchType: null

    };

    $scope.ClearProcessNatureFields = function () {
        if ($scope.entityProcess.ProcessNature === 'Packing') {
            $scope.entityProcess.IsDispatchSKURequired = false;
            $scope.entityProcess.DispatchForm = null;
            $scope.entityProcess.DispatchType = null;

        } else {
            $scope.entityProcess.IsFinishGoods = false;
            $scope.entityProcess.IsPackingSKURequired = false;
            $scope.entityProcess.PackingForm = null;
        }
    }

    // #region DDL

    $scope.processNatureList = [];
    cboService.getEnumCbo('enum/getenumprocessnaturecbo', function (result) {
        $scope.processNatureList = result;
    });

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
        cboService.getCboPlantByCompany($scope.entityProcessTag.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };

    $scope.productionProcessGroupCboList = [];
    cboService.productionProcessGroupCbo(null, function (result) {
        $scope.productionProcessGroupCboList = result.Rows;
    });

    $scope.entityList = [];
    $scope.getEntity = function () {
        $scope.entities = [];
        $scope.entityValue = [];
        $scope.entityProcessTagList = [];
        //cboService.getCboProductionEntityByCompany(null, $scope.entityProcessTag.CompanyId, function (result) {
        //    $scope.entityList = result;
        //});
        $http({
            method: 'POST',
            url: "Processes/EntityProcessTag/GetEntity?plantId=" + $scope.entityProcessTag.PlantId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    };

    
    $scope.UncheckMandatory = function (data) {
   
        if (data.LotNumberCapture==false) {
            data.LotNumberMandatory = false;
        }
    }

    // #endregion

    // #region Entity Map Data

    $scope.entities = [];
    $scope.getEntityMapData = function () {
        $scope.entities = [];
        $scope.entityProcessTagList = [];
        $scope.entityProcessTag.ProductionProcessGroupId = null;
        $http({
            method: 'GET',
            url: 'Organizations/entity/get?id=' + $scope.entityProcessTag.EntityId
        }).then(function successCallback(response) {
            if (baseService.arrayLength($scope.entities) === 0) {
                var localValue = [];
                localValue.push(response.data);
                baseService.getDDLSearchColumn(localValue, $scope.entities);
                $scope.entityValue = localValue;
            }
        });
    };

    // #endregion

    // #region POP UP

    $scope.processParameters = {
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
    $scope.processPopUp = function () {
        $scope.$broadcast('show-errors-check-validity');
        if (!$scope.modelForm.$valid) return;
        $rootScope.tempList = [];
        angular.forEach($scope.entityProcessTagList, function (a) {
            $rootScope.tempList.push({
                Id: a.ProcessId
                , Sequence: a.Sequence
                , Code: a.Code
                , ShortName: a.ShortName
                , StandardName: a.StandardName
                , UserName: a.UserName
                , IsProductionProcess: a.IsProductionProcess
                , Active: a.Active
            });
        });
        baseService.setCurrentPage('processList');
        $scope.getProcessData = function (pageno) {
            $scope.getProcessUrl = 'Processes/CompanyProcess/GetCompanyProcessList/?companyId=' + $scope.entityProcessTag.CompanyId + '&processIds=[]';// + isProcessIdExistGrid($scope.entityProcessTagList);
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.processParameters)
                .then(function (result) {
                    $scope.processList = result.Rows;
                    $scope.processParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.processList); t++) {
                        $scope.processList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.processList[t].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };
    $scope.CloseProcessPopUp = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };
    $rootScope.searchProcessByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        },
        {
            'name': 'Material Type',
            'value': 'MaterialType'
        }
    ];

    $scope.addProcess = function () {
        if (baseService.arrayLength($scope.processList) === 0)
            return ShowResult('Please select at least one row!', 'failure', 'processPopUp');
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.entityProcessTagList, 'ProcessId', a.Id)) {
                    $scope.entityProcessTagList.push({
                        Id: null
                        , EntityId: $scope.entityProcessTag.EntityId
                        , ProcessId: a.Id
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , ShortName: a.ShortName
                        , StandardName: a.StandardName
                        , UserName: a.UserName
                        , MaterialType: a.MaterialType
                        , IsProductionProcess: a.IsProductionProcess
                        , Active: a.Active
                    });
                }
            });
        }
        else
            $scope.entityProcessTagList = [];
        angular.forEach($scope.entityProcessTagList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.ProcessId))
                $scope.entityProcessTagList.splice(a, 1);
        });
        $scope.CloseProcessPopUp();
    };

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

    // #endregion

    $scope.ProcessNature = function (d, index) {
        $scope.entityProcess = d;
        $scope.index_popup = index;
        angular.element(document.querySelector('#NaturePopUp')).modal('show');
    };

    $scope.set_popup = function (list, ProcessId, data) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessId === ProcessId) {
                list[i].IsFinishGoods = data.IsFinishGoods;
                list[i].ProcessNature = data.ProcessNature;
                list[i].IsPackingFromContSKU = data.IsPackingFromContSKU;
                list[i].IsDispatchGrpApplicable = data.IsDispatchGrpApplicable;
                list[i].PackingUoM = data.PackingUoM;
                list[i].DispatchUoM = data.DispatchUoM;

                list[i].IsPackingSKURequired = data.IsPackingSKURequired;
                list[i].PackingForm = data.PackingForm;
                list[i].IsDispatchSKURequired = data.IsDispatchSKURequired;
                list[i].DispatchForm = data.DispatchForm;
                list[i].DispatchType = data.DispatchType;
                break;
            }
        }
    };

    $scope.closePopUp = function () {
        try {
            if ($scope.entityProcess.IsDispatchSKURequired) {
                if (baseService.isUndefinedOrNull($scope.entityProcess.DispatchForm)) {
                    throw "Dispatch Form is required.";
                }
                if (baseService.isUndefinedOrNull($scope.entityProcess.DispatchType)) {
                    throw "Dispatch Form is required.";
                }
            }
            if ($scope.entityProcess.IsPackingSKURequired) {
                if (baseService.isUndefinedOrNull($scope.entityProcess.PackingForm)) {
                    throw "Packing Form is required.";
                }
               
            }

            var id_x = $scope.index_popup;
            var pId = $scope.entityProcessTagList[id_x].ProcessId;
            $scope.set_popup($scope.entityProcessTagList, pId, $scope.entityProcess);
            angular.element(document.querySelector('#NaturePopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'NaturePopUp');
        }
    };


    $scope.Save = function () {
        try {

            if (baseService.arrayLength($scope.entityProcessTagList) === 0) {
                throw 'No data found.';
            } else {
                for (var i = 0; i < $scope.entityProcessTagList.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.entityProcessTagList[i].ProductionBookingLevel) && $scope.entityProcessTagList[i].IsProductionProcess === false) {
                        throw 'This Process : "' + $scope.entityProcessTagList[i].UserName + '" is not Production Process.';
                    }
                }
            }
            
            $http({
                method: 'POST'
                , url: 'Processes/entityProcessTag/create'
                , data: $scope.entityProcessTagList
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
            , url: $scope.deleteGraphUrl + $scope.entityProcessTag.EntityId + '&productionProcessGroupId=' + $scope.entityProcessTag.ProductionProcessGroupId
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
        $scope.entityProcessTag = {};
        $scope.entities = [];
        $scope.entityValue = [];
        $scope.entityProcessTagList = [];
    }
}