'use strict';
planningTypesNewController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function planningTypesNewController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Planning Types";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.planningTypess = [];
    $scope.path = 'Productions/planningTypesNew/';
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

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.planningTypes = {
        Id: null,
        CompanyGroupId: null,
        BaseProcessId: null,
        PlanningType: null,
        Description: null,
        CompanyId: null,
        PlantId: null,
        SubProcessId: null
    };
    $scope.planningTypesNew = Object.assign({}, $scope.planningTypes);

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
    cboService.getProductionProcessCbo(function (response) {
        $scope.processList = response;
    });

    $scope.subprocessList = [];
    $scope.GetSubprocessCbo = function () {
        $scope.subprocessList = [];
        $http.get('Processes/CompanySubProcess/GetCbobyprocessid?processid=' + $scope.planningTypesNew.BaseProcessId + '&companyId=' + $scope.planningTypesNew.CompanyId)
            .then(function (response) {
                $scope.subprocessList = response.data;
            });
    }

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
        $scope.GetSubprocessCbo();
        if (!baseService.isUndefinedOrNull($scope.CompanyId)) {
            $scope.planningTypesNew.CompanyId = $scope.CompanyId;
        }
        $scope.getPlantCbo();
        if ($scope.planningTypes.PlanningType === 'PlanningType1') {
            $scope.planningTypes.Description = 'WC wise';
        }
        else if ($scope.planningTypes.PlanningType === 'PlanningType2') {
            $scope.planningTypes.Description = 'Batch wise';
        }
        else {
            $scope.planningTypes.Description = 'N/A';
        }
        $scope.GetResponsibleEmployeeData();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
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
        $scope.SelectedEmpList = [];
        $scope.SavedWCList = [];
    };

    // #region  ResponsibleEmployee
    $scope.popUpList = [];
    $scope.SelectedEmpList = [];

    $scope.employeeInformation = {
        PlantId: $scope.planningTypesNew.PlantId
        , EmployeeCode: null
        , EmployeeName: null
        , SystemId: null
    };
    $scope.popUpDataList = [];
    $scope.popUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'Productions/PlanningTypesNew/GetAllActiveEmployeeData?PlanningTypesId=' + $scope.planningTypesNew.Id

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#popUp").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.closePopUp = function () {
        var obj = {};
        for (var i = 0; i < $scope.popUpDataList.length; i++) {
            if ($scope.popUpDataList[i].Flag) {
                obj.Id = null;
                obj.PlanningTypesId = $scope.planningTypesNew.Id;
                obj.EmpSystemId = $scope.popUpDataList[i].SystemId;
                obj.EmployeeCode = $scope.popUpDataList[i].EmployeeCode;
                obj.EmployeeName = $scope.popUpDataList[i].EmployeeName;
                obj.Company = $scope.popUpDataList[i].Company;
                obj.Plant = $scope.popUpDataList[i].Plant;
                obj.LegalDesignation = $scope.popUpDataList[i].LegalDesignation;
                obj.Department = $scope.popUpDataList[i].Department;
                obj.Section = $scope.popUpDataList[i].Section;
                obj.SubSection = $scope.popUpDataList[i].SubSection;
                obj.Line = $scope.popUpDataList[i].Line;

                $scope.SelectedEmpList.push(obj);
                obj = {};
            }
        }
        $scope.SaveSelectedEmpList();
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.onrowdatabound = function (e) {
        if (e.data.EmployeeStatus === 'Separated')
            e.row.css("background-color", "red");
    };


    $scope.SaveSelectedEmpList = function () {
        try {
            $http({
                method: 'POST',
                url: 'Productions/PlanningTypesNew/CreateResponsiblePersion',
                data: { "data": $scope.SelectedEmpList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetResponsibleEmployeeData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.GetResponsibleEmployeeData = function () {

        $http.get('Productions/PlanningTypesNew/GetResponsibleEmployeeData?PlanningTypesId=' + $scope.planningTypesNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SelectedEmpList = response.data;
                }
                $scope.GetSavedWCData();
            });

    }

    $scope.valuePassInRPModal = function (data) {
        $scope.Id = data.data.Id;
        if (baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete parmanently [ ' + data.data.EmployeeCode + ' ]';
        angular.element(document.querySelector('#removeRPPopUp')).modal('show');
    };

    $scope.DeleteResponsibleEmployee = function () {
        $http({
            method: 'POST',
            url: 'Productions/PlanningTypesNew/DeleteResponsibleEmployee?id=' + $scope.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetResponsibleEmployeeData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    // #endregion

    //#region WC       

    $scope.modelWC = {
        Id: null, WorkCenterMasterd: null, PlanningTypesId: $scope.planningTypesNew.Id, PlanCapacity: null, Capacity: null, UOM: null, PlanEfficiency: null, AverageLoadFactor: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    }
    $scope.modelWCNew = Object.assign({}, $scope.modelWC);

    $scope.workCenterList = [];
    $scope.GetpopWCUp = function () {
        try {
            $http({
                method: 'GET',
                url: 'Productions/PlanningTypesNew/GetWorkCenterList?processId=' + $scope.planningTypesNew.BaseProcessId + '&subprocessId=' + $scope.planningTypesNew.SubProcessId
            }).then(function successCallback(res) {
                $scope.workCenterList = res.data;
            });

            var eDialog = $("#workCenterPopUp").data("ejDialog");
            eDialog.open();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //$scope.refreshTemplateWC = function (args) {
    //    $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllWC });
    //};

    //function CheckBoxSelectAllWC(e) {
    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;
    //    }
    //    var filtered = $("#GridPopUp").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.workCenterList.length; i++) {
    //            $scope.workCenterList[i].Flag = ChkOrUnchk;
    //        }
    //    }
    //    else {
    //        for (var j = 0; j < filtered.length; j++) {
    //            filtered[j].Flag = ChkOrUnchk;
    //        }
    //    }
    //    var gridObj = $("#workCenterPopUp").data("ejGrid");
    //    gridObj.refreshContent();
    //};

    $scope.SetworkCenter = function (data) {
        $scope.modelWCNew.WorkCenterMaster = data.data.UserName;
        $scope.modelWCNew.WorkCenterMasterId = data.data.WorkCenterMasterId;
        $scope.modelWCNew.Capacity = data.data.Capacity;
        $scope.modelWCNew.UOM = data.data.UOM;
        $scope.modelWCNew.PlanCapacity = data.data.Capacity;
        $scope.CloseWorkCenter();
    }

    $scope.CloseWorkCenter = function () {
        var eDialog = $("#workCenterPopUp").data("ejDialog");
        eDialog.close();
    }

    $scope.SaveWC = function () {
        try {
            $scope.modelWCNew.PlanningTypesId = $scope.planningTypesNew.Id;
            if (baseService.isUndefinedOrNull($scope.modelWCNew.PlanningTypesId)) {
                throw "PlanningType is required.";
            }
            $http({
                method: 'POST',
                url: '/Productions/PlanningTypesNew/CreateWS',
                data: { 'data': $scope.modelWCNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearWC();
                    $scope.GetSavedWCData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ClearWC = function () {
        $scope.modelWC = {
            Id: null, WorkCenterMasterd: null, PlanningTypesId: $scope.planningTypesNew.Id, PlanCapacity: null, Capacity: null, UOM: null, PlanEfficiency: null, AvgeageLoadFactor: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
        }
        $scope.modelWCNew = Object.assign({}, $scope.modelWC);
    }

    $scope.SavedWCList = [];
    $scope.GetSavedWCData = function () {
        $http.get('Productions/PlanningTypesNew/GetSavedWCData?PlanningTypesId=' + $scope.planningTypesNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SavedWCList = response.data;
                }
            });
    }


    //#endregion


}