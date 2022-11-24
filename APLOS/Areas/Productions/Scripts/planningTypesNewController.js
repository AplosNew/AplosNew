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
        $scope.GetSavedHolidayData();
        $scope.GetLatestPlanDate();
        $scope.GetSavedDateData();
        $scope.GetPlanCapacityData();
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
        $scope.SavedDateList = [];
        $scope.SavedShiftList = [];
        $scope.SavedHolidayList = [];
        $scope.SavedWeekList = [];
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
                url: 'Productions/PlanningTypesNew/GetWorkCenterList?processId=' + $scope.planningTypesNew.BaseProcessId + '&subprocessId=' + $scope.planningTypesNew.SubProcessId + '&PlantId=' + $scope.planningTypesNew.PlantId + '&PlanningTypesId=' + $scope.planningTypesNew.Id
            }).then(function successCallback(res) {
                $scope.workCenterList = res.data;
            });

            var eDialog = $("#workCenterPopUp").data("ejDialog");
            eDialog.open();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.EditPlanWC = function (obj) {
        $scope.modelWCNew = Object.assign({}, obj.data);
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
        $scope.GetSavedShiftData();
    }


    //#endregion

    //#region Shift

    $scope.modelShift = { Id: null, ShiftId: null, PlanningTypesId: $scope.planningTypesNew.Id, ProductionShiftStartTime: null, ProductionShiftEndTime: null, ProductionTime: null, Remark: null, IsExceptionApplicable: false, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    $scope.modelShiftNew = Object.assign({}, $scope.modelShift);

    $scope.selectedShiftList = [];
    $scope.searchShiftList = [
        {
            'name': 'Shift Name',
            'value': 'ShiftDefinationName'
        },
        {
            'name': 'Description',
            'value': 'ShiftDefinationDescription'
        },
        {
            'name': 'Shift Type',
            'value': 'ShiftType'
        }
    ];
    $scope.ShiftPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'ShiftDefinationName',
        searchBy: 'ShiftDefinationName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ShiftPopUp = function () {
        var wcids = "";
        for (var i = 0; i < $scope.SavedWCList.length; i++) {
            if (wcids == "") {
                wcids = "'','" + $scope.SavedWCList[i].WorkCenterMasterId + "'";
            }
            else {
                wcids += ",'" + $scope.SavedWCList[i].WorkCenterMasterId + "'";
            }
        }


        $scope.ShiftPopUpList = [];
        $scope.ShiftPopUpParameters.sort = 'ShiftDefinationName';
        $scope.ShiftPopUpParameters.searchBy = 'ShiftDefinationName';
        $scope.getShiftPopUpData = function (pageno) {

            baseService.paginationBase('Productions/PlanningTypesNew/GetShiftList?ShiftDefinationIDs=' + isShiftDefinationIDExistGrid($scope.selectedShiftList) + '&plantId=' + $scope.planningTypesNew.PlantId + '&wcids=' + wcids, pageno, $scope.ShiftPopUpParameters)
                .then(function (result) {
                    $scope.ShiftPopUpDataList = result.Rows;
                    $scope.ShiftPopUpParameters.total_count = result.Total;

                    for (var t = 0; t < baseService.arrayLength($scope.ShiftPopUpDataList); t++) {
                        $scope.ShiftPopUpDataList[t].Flag = baseService.valueCheckInList($scope.tempList, 'ShiftDefinationID', $scope.ShiftPopUpDataList[t].ShiftDefinationID);
                    }

                    if (baseService.arrayLength($scope.ShiftPopUpList) == 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.ShiftPopUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'ShiftPopUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#ShiftPopUpId')).modal('show');
        $scope.getShiftPopUpData();
    }

    function isShiftDefinationIDExistGrid(list) {
        $scope.ShiftDefinationIDs = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                $scope.ShiftDefinationIDs.push(list[i]['ShiftDefinationID']);
            }
        }
        return JSON.stringify($scope.ShiftDefinationIDs);
    }

    $scope.tempList = [];
    $scope.pushInTempList = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.ShiftDefinationID) === false) {
                    $scope.tempList.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                        if ($scope.tempList[i].ShiftDefinationID === data.ShiftDefinationID) {
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }

                    $scope.tempList.push(data);
                }
            }
            else {
                for (var t = 0; t < baseService.arrayLength($scope.tempList); t++) {
                    if ($scope.tempList[t].ShiftDefinationID === data.ShiftDefinationID) {
                        $scope.tempList.splice(t, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    $scope.SetShift = function (data) {
        $scope.modelShiftNew.Shift = data.ShiftDefinationName;
        $scope.modelShiftNew.ShiftId = data.ShiftDefinationID;
        $scope.modelShiftNew.PlanningTypesId = $scope.planningTypesNew.Id;
        $scope.CloseShift();
    }

    $scope.CloseShift = function () {
        angular.element(document.querySelector('#ShiftPopUpId')).modal('hide');
    }

    $scope.getMinute = function () {
        try {
            $scope.MinuteUrl = 'Productions/PlanningTypesNew/GetMinute/'
            $http({
                method: 'POST',
                url: $scope.MinuteUrl,
                data: { 'data': $scope.modelShiftNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.modelShiftNew.ProductionTime = response.data;
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.SaveShift = function () {
        try {
            $scope.modelShiftNew.PlanningTypesId = $scope.planningTypesNew.Id;
            if (baseService.isUndefinedOrNull($scope.modelShiftNew.PlanningTypesId)) {
                throw "PlanningType is required.";
            }
            $http({
                method: 'POST',
                url: '/Productions/PlanningTypesNew/CreateShift',
                data: { 'data': $scope.modelShiftNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearShift();
                    $scope.GetSavedShiftData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ClearShift = function () {
        $scope.modelShift = { Id: null, ShiftId: null, PlanningTypesId: $scope.planningTypesNew.Id, ProductionShiftStartFrom: null, ProductionShiftStartTo: null, ProductionTime: null, Remark: null, IsExceptionApplicable: false, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.modelShiftNew = Object.assign({}, $scope.modelShift);
    }

    $scope.SavedShiftList = [];
    $scope.GetSavedShiftData = function () {
        $http.get('Productions/PlanningTypesNew/GetSavedShiftData?PlanningTypesId=' + $scope.planningTypesNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SavedShiftList = response.data;
                }
            });
        $scope.GetSavedWeekData();
    }

    $scope.EditPlanShift = function (obj) {
        $scope.modelShiftNew = Object.assign({}, obj.data);
    }

    //#endregion

    //#region Week
    $scope.modelWeek = { Id: null, WeekDays: null, PlanningTypesId: $scope.planningTypesNew.Id, IsWorkingDays: false, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    $scope.modelWeekNew = Object.assign({}, $scope.modelWeek);

    $scope.WeekDaysList = [
        {
            'Value': 'Saturday',
            'Text': 'Saturday'
        },
        {
            'Value': 'Sunday',
            'Text': 'Sunday'
        },
        {
            'Value': 'Monday',
            'Text': 'Monday'
        }
        , {
            'Value': 'Tuesday',
            'Text': 'Tuesday'
        },
        {
            'Value': 'Wednesday',
            'Text': 'Wednesday'
        },
        {
            'Value': 'Thursday',
            'Text': 'Thursday'
        },
        {
            'Value': 'Friday',
            'Text': 'Friday'
        }
    ];

    $scope.SaveWeek = function () {
        try {
            $scope.modelWeekNew.PlanningTypesId = $scope.planningTypesNew.Id;
            if (baseService.isUndefinedOrNull($scope.modelWeekNew.PlanningTypesId)) {
                throw "PlanningType is required.";
            }
            $http({
                method: 'POST',
                url: '/Productions/PlanningTypesNew/CreateWeek',
                data: { 'data': $scope.modelWeekNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearWeek();
                    $scope.GetSavedWeekData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ClearWeek = function () {
        $scope.modelWeek = { Id: null, ShiftId: null, PlanningTypesId: $scope.planningTypesNew.Id, ProductionShiftStartFrom: null, ProductionShiftStartTo: null, ProductionTime: null, Remark: null, IsExceptionApplicable: false, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.modelWeekNew = Object.assign({}, $scope.modelShift);
    }

    $scope.SavedWeekList = [];
    $scope.GetSavedWeekData = function () {
        $http.get('Productions/PlanningTypesNew/GetSavedWeekData?PlanningTypesId=' + $scope.planningTypesNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SavedWeekList = response.data;
                }
            });
    }

    $scope.EditPlanWK = function (obj) {
        $scope.modelWeekNew = Object.assign({}, obj.data);
    }

    //#endregion

    //#region Holiday
    $scope.modelHoliday = { Id: null, PlanningTypesId: $scope.planningTypesNew.Id, HolidayDate: null, HolidayName: null, Remark: null, IsWorking: false, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    $scope.modelHolidayNew = Object.assign({}, $scope.modelHoliday);

    $scope.SaveHoliday = function () {
        try {
            $scope.modelHolidayNew.PlanningTypesId = $scope.planningTypesNew.Id;
            if (baseService.isUndefinedOrNull($scope.modelHolidayNew.PlanningTypesId)) {
                throw "PlanningType is required.";
            }
            $http({
                method: 'POST',
                url: '/Productions/PlanningTypesNew/CreateHoliday',
                data: { 'data': $scope.modelHolidayNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearHoliday();
                    $scope.GetSavedHolidayData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ClearHoliday = function () {
        $scope.modelHoliday = { Id: null, PlanningTypesId: $scope.planningTypesNew.Id, HolidayDate: null, HolidayName: null, Remark: null, IsWorking: false, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.modelHolidayNew = Object.assign({}, $scope.modelHoliday);
    }

    $scope.SavedHolidayList = [];
    $scope.GetSavedHolidayData = function () {
        $http.get('Productions/PlanningTypesNew/GetSavedHolidayData?PlanningTypesId=' + $scope.planningTypesNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SavedHolidayList = response.data;
                }
            });
    }

    $scope.EditPlanHoliday = function (obj) {
        $scope.modelHolidayNew = Object.assign({}, obj.data);
    }

    //#endregion

    //#region Date
    $scope.modelDate = { Id: null, PlanningTypesId: $scope.planningTypesNew.Id, FromDate: null, ToDate: null, PlanningDate: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    $scope.modelDateNew = Object.assign({}, $scope.modelDate);
    $scope.LatestDate = null;

    $scope.GetLatestPlanDate = function () {
        $http.get('Productions/PlanningTypesNew/GetLatestPlanDate?PlanningTypesId=' + $scope.planningTypesNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.modelDateNew.FromDate = response.data[0].FromDate;
                    $scope.LatestDate = response.data[0].FromDate;

                    if (baseService.isUndefinedOrNull($scope.modelDateNew.FromDate)) {
                        $scope.modelDateNew.FromDate = $filter('dateFiltering')(new Date());
                    }
                } 
            });
    }


    $scope.SaveDate = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.LatestDate)) {
                if (new Date($scope.modelDateNew.FromDate) < new Date($scope.LatestDate)) {
                    throw "From date must be greater than to LatestDate";
                }
            }

            if (new Date($scope.modelDateNew.FromDate) > new Date($scope.modelDateNew.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            if (new Date($scope.modelDateNew.ToDate) < new Date($scope.modelDateNew.FromDate)) {
                throw "To date must be above or equal to From Date.";
            }

            $scope.modelDateNew.PlanningTypesId = $scope.planningTypesNew.Id;
            angular.copy($scope.modelDateNew, $scope.modelDate);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.dateform.$valid) {
                $scope.modelDateNew.PlanningTypesId = $scope.planningTypesNew.Id;
                $scope.modelDate.PlanningTypesId = $scope.planningTypesNew.Id;
                $http({
                    method: 'POST',
                    url: '/Productions/PlanningTypesNew/CreateDate',
                    data: { 'data': $scope.modelDate },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetSavedDateData();
                        $scope.GetLatestPlanDate();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SavedDateList = [];
    $scope.GetSavedDateData = function () {
        $http.get('Productions/PlanningTypesNew/GetSavedDateData?PlanningTypesId=' + $scope.planningTypesNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SavedDateList = response.data;
                }
            });
    }

    //#endregion

    //#region  CapacityPlan   

    $scope.PlanCapacityList = [];
    $scope.GetPlanCapacityData = function () {
        if (!baseService.isUndefinedOrNull($scope.planningTypesNew.Id)) {
    $http.get('Productions/PlanningTypesNew/GetPlanCapacityDataByPlanningType?PlanningTypesId=' + $scope.planningTypesNew.Id)
                .then(function (response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.PlanCapacityList = response.data;
                    }
                });
        }
    }

    $scope.SavePlanCapacity = function () {
        try {
            for (var i = 0; i < $scope.PlanCapacityList.length; i++) {
                $scope.PlanCapacityList[i].PlanningTypesId = $scope.planningTypesNew.Id;
            }

            $http({
                method: 'POST',
                url: 'Productions/PlanningTypesNew/CreateCapacityPlanning',
                data: { "data": $scope.PlanCapacityList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetPlanCapacityData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //#endregion


    document.addEventListener('keydown', function () {
        if (event.keyCode == 123) {
            alert("You Can not Do This!");
            return false;
        } else if (event.ctrlKey && event.shiftKey && event.keyCode == 73) {
            alert("You Can not Do This!");
            event.preventDefault();
            return false;
        } else if (event.ctrlKey && event.keyCode == 85) {
            alert("You Can not Do This!");
            return false;
        }
    }, false);

    if (document.addEventListener) {
        document.addEventListener('contextmenu', function (e) {
            alert("You Can not Do This!");
            e.preventDefault();
        }, false);
    } else {
        document.attachEvent('oncontextmenu', function () {
            alert("You Can not Do This!");
            window.event.returnValue = false;
        });
    }

}