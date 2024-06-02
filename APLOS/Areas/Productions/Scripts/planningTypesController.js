'use strict';
planningTypesController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function planningTypesController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Planning Types";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.planningTypess = [];
    $scope.path = 'Productions/planningTypes/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'PlanningType', 'PlanningType');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.planningTypeses = result.Rows;
                if (baseService.arrayLength($scope.planningTypeses) > 0) {
                    for (var i = 0; i < $scope.planningTypeses.length; i++) {
                        if ($scope.planningTypeses[i].PlanningType === 'PlanningType1') {
                            $scope.planningTypeses[i].Description = 'WC wise';
                        }
                        else if ($scope.planningTypeses[i].PlanningType === 'PlanningType2') {
                            $scope.planningTypeses[i].Description = 'Batch wise';
                        } else {
                            $scope.planningTypeses[i].Description = $scope.planningTypeses[i].PlanningType;
                        }

                    }
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.planningTypes = {
        Id: null,
        CompanyGroupId: null,
        BaseProcessId: null,
        PlanningType: null,
        Description: null,
        CompanyId: null,
        PlantId: null,
        EntityId:null
    };
    $scope.planningTypesNew = Object.assign({}, $scope.planningTypes);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: 'Productions/PlanningTypes/GetAllEntity?CompanyId=' + $scope.planningTypesNew.CompanyId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (response) {
        $scope.companyList = response;
    });

    $scope.plantList = [];
    $scope.getPlantCbo = function () {
        cboService.getCboPlantByCompany($scope.planningTypesNew.CompanyId, function (response) {
            $scope.plantList = response;
        });
    };

    $scope.planningTypesList = [];
    cboService.getEnumCbo('Enum/GetEnumEnumPlanningTypes/', function (result) {
        $scope.planningTypesList = result;
    });

    $scope.processList = [];
    //cboService.getProductionProcessCbo(function (response) {
    //    $scope.processList = response;
    //});
    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.planningTypesNew.ProcessId = $scope.processList[0].Value;
            }
        });
    };



    $scope.ChangeType = function () {
        if ($scope.planningTypes.PlanningType === 'PlanningType1') {
            $scope.planningTypes.Description = 'WC wise';
        }
        else if ($scope.planningTypes.PlanningType === 'PlanningType2') {
            $scope.planningTypes.Description = 'Batch wise';
        }
        else {
            $scope.planningTypes.Description = $scope.planningTypes.PlanningType;
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        if (!baseService.isUndefinedOrNull($scope.planningTypesNew.CompanyId)) {
            $scope.CompanyId = $scope.planningTypesNew.CompanyId;
        }

        $scope.planningTypes = $scope.planningTypeses[$scope.index];
        $scope.planningTypesNew = Object.assign({}, $scope.planningTypes);

        if (!baseService.isUndefinedOrNull($scope.CompanyId)) {
            $scope.planningTypesNew.CompanyId = $scope.CompanyId;
        }
        $scope.getPlantCbo();
        $scope.getAllEntities();
        $scope.loadProcessList($scope.planningTypesNew.EntityId);
        if ($scope.planningTypes.PlanningType === 'PlanningType1') {
            $scope.planningTypes.Description = 'WC wise';
        }
        else if ($scope.planningTypes.PlanningType === 'PlanningType2') {
            $scope.planningTypes.Description = 'Batch wise';
        }
        else {
            $scope.planningTypes.Description = 'N/A';
        }
        $scope.Action = 'Update';
    };

    $scope.Save = function () {
        angular.copy($scope.planningTypesNew, $scope.planningTypes);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.planningTypeForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.planningTypes,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.planningTypeses.push(response.data.PlanningTypes);
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.planningTypes,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.planningTypeses[$scope.index] = $scope.planningTypes;
                        }
                        ClearFields();
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.valuePass = function (index, data) {
        $scope.Id = data.Id;
        $scope.Index = index;
        if (baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete parmanently [ ' + data.PlanningType + ' ]';
        angular.element(document.querySelector('#removePopUp')).modal('show');
    };

    $scope.Delete = function () {
        if (baseService.isUndefinedOrNull($scope.Id)) {
            $scope.planningTypeses.splice($scope.Index, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Productions/PlanningTypes/Delete?id=' + $scope.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.planningTypesNew = {};
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.planningTypeses.splice($scope.Index, 1);
                    $scope.getData();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.planningTypes = {};
        $scope.planningTypesNew = {};
    };

    $scope.GetSavedPMData = function () {
        $scope.productMasterList = [];
        $http({
            method: 'GET',
            url: 'Productions/PlanningTypes/GetPlanningTypeProductMasterDataList?PlanningTypeId=' + $scope.PlanningTypeId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.productMasterList = response.data;
            }
        });
    }
    $scope.PlanningTypeId = null;
    $scope.productMasterList = [];
    $scope.GetPMData = function (obj) {
        try {
            $scope.PlanningTypeId = null;
            $scope.productMasterList = [];
            $scope.PlanningTypeId = obj.Id;
            $http({
                method: 'GET',
                url: 'Productions/PlanningTypes/GetPlanningTypeProductMasterDataList?PlanningTypeId=' + $scope.PlanningTypeId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.productMasterList = response.data;
                    angular.element(document.querySelector('#PMPopUp')).modal('show');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.refreshTemplate = function (args) {
        $("#headschk").ejCheckBox({ "change": CheckBoxSelectAllItemWise });
    };
    function CheckBoxSelectAllItemWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSM").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.productMasterList.length; i++) {
                $scope.productMasterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSM").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SavedproductMasterList = [];
    $scope.ClosePopUp = function () {
        angular.element(document.querySelector('#PMPopUp')).modal('hide');
    }

    function checkItemExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductMasterId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.PMsaveUrl = 'Productions/PlanningTypes/CreatePTPMMap';
    $scope.SaveData = function () {
        $scope.SavedproductMasterList = [];
        for (var i = 0; i < $scope.productMasterList.length; i++) {
            if ($scope.productMasterList[i].Flag == true || !baseService.isUndefinedOrNull($scope.productMasterList[i].Id)) {
                if (checkItemExist($scope.SavedproductMasterList, $scope.productMasterList[i].ProductMasterId) === false) {
                    var obj = {};
                    obj.Id = $scope.productMasterList[i].Id == null ? null : $scope.productMasterList[i].Id;
                    obj.PlanningTypeId = $scope.PlanningTypeId;
                    obj.ProductMasterId = $scope.productMasterList[i].ProductMasterId;
                    obj.Flag = $scope.productMasterList[i].Flag;

                    $scope.SavedproductMasterList.push(obj);
                    obj = {};
                }
            }
        }


        $http({
            method: 'POST',
            url: $scope.PMsaveUrl,
            data: { 'data': $scope.SavedproductMasterList, 'masterId': $scope.PlanningTypeId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedPMData();
                angular.element(document.querySelector('#EntityPopup')).modal('hide');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };



}