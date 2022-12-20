'use strict';
skillManagementDetailsController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function skillManagementDetailsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "SkillManagementDetails";
    $scope.Action = 'Save';
    $scope.path = 'Machines/SkillManagementDetails/';
    $scope.savePlannedUrl = $scope.path + 'createPlanned';
    $scope.saveResponsibleUrl = $scope.path + 'createResponsible';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate() + 7);
    /*var firstDay = new Date(y, m, 1);*/
   
    
   
    $scope.status = {
        Id: null,
        FromDate: null,
        ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        FromDateMD: null,
        ToDateMD: $filter('dateFiltering')(date, 'dd-MM-yyyy')
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.GetFromDateList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagementDetails/GetFromDateList'
        }).then(function successCallback(response) {
            $scope.statusNew.FromDate = response.data[0].FromDate;
            $scope.statusNew.FromDateMD = response.data[0].FromDate;
        });
    }
    $scope.GetFromDateList();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    function Validation() {
        try {
            CheckField("To Date", $scope.statusNew.ToDate);
        } catch (ex) {
            throw ex;
        }
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }

    $scope.SkillManagementStatusDetailsList = [];
    $scope.View = function () {
        try {
            Validation();
            $http({

                method: 'Get',
                url: 'Machines/SkillManagementDetails/LoadSkillManagementStatusDetailsList?ToDate=' + $scope.statusNew.ToDateMD + '&FromDate=' + $scope.statusNew.FromDateMD
            }).then(function successCallback(response) {
                $scope.SkillManagementStatusDetailsList = response.data;
                var gridObj = $("#GridSkillManagementStatusDetails").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            }
            )
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SkillManagementStatusSummaryList = [];
    $scope.ViewSummary = function () {
        try {
            Validation();
            $http({

                method: 'Get',
                url: 'Machines/SkillManagementDetails/LoadSkillManagementStatusSummaryList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate
            }).then(function successCallback(response) {
                $scope.SkillManagementStatusSummaryList = response.data;
                var gridObj = $("#GridSkillManagementStatusSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            }
            )
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.refreshTemplateEmployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmployee });
    };
    function CheckBoxSelectAllEmployee(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPlannedEmployee").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SkillManagementStatusPlannedDetailsList.length; i++) {
                $scope.SkillManagementStatusPlannedDetailsList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPlannedEmployee").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.refreshTemplateMachineAssetSummary = function (args) {
        $("#headchkSummary").ejCheckBox({ "change": CheckBoxSelectAllMachineAssetSummary });
    };
    function CheckBoxSelectAllMachineAssetSummary(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPlannedMachineAssetSummary").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MaintenanceStatusPlannedSummaryList.length; i++) {
                $scope.MaintenanceStatusPlannedSummaryList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPlannedMachineAssetSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.refreshTemplateResponsiblePerson = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllResponsiblePerson });
    };
    function CheckBoxSelectAllResponsiblePerson(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridResponsiblePopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ReponsiblePersonList.length; i++) {
                $scope.ReponsiblePersonList[i].IsActive = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].IsActive = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridResponsiblePopUp").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };
    $scope.Test = null;
    $scope.MachineId = null;
    $scope.MaintenanceId = null;
    $scope.EntityId = null;
    $scope.SkillManagementStatusPlannedDetailsList = [];
    $scope.GetAssetPopUp = function (data, sample) {
        $scope.Test = sample;
        if ($scope.Test != 0) {
            $scope.Test = 1;
        }
        $scope.MachineId = data.data.MachineId;
        $scope.MaintenanceId = data.data.Id;
        $scope.EntityId = data.data.EntityId;
        $http({
            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadMaintenanceStatusPlannedList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.MaintenanceId + '&MachineId=' + $scope.MachineId + '&EntityId=' + $scope.EntityId + '&Value=' + $scope.Test
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAssetSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetSummaryPop')).modal('show');
        }
        )
    }


    $scope.SD = null;
    $scope.GetDetails = function (args) {
        $scope.EmployeeId = args.data.EmployeeId;
        $scope.SMId = args.data.Id;
        $scope.EntityId = args.data.EntityId;
        $scope.PositionCodeId = args.data.PositionId;
        $scope.SD = args.data.SD;
        $http({
            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadSkillManagementStatusPlannedListGetDetails?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&SMId=' + $scope.SMId + '&EntityId=' + $scope.EntityId + '&PositionId=' + $scope.PositionCodeId + '&EmployeeId=' + $scope.EmployeeId
        }).then(function successCallback(response) {
            $scope.SkillManagementStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedEmployee").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#EmployeePopUp')).modal('show');
        }
        )
    }
    
    $scope.GetAssetPopUpDetails = function () {
        $http({
            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadMaintenanceStatusPlannedListDetails?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.MaintenanceId + '&MachineId=' + $scope.MachineId 
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAssetSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetSummaryPop')).modal('show');
        }
        )
    }
    $scope.ResMaintenanceId = null;
    $scope.GetEmployeePopUpGetDetails = function () {
        $http({
            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadSkillManagementStatusPlannedListGetPlanDetails?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&SMId=' + $scope.SMId + '&EntityId=' + $scope.EntityId + '&PositionId=' + $scope.PositionCodeId + '&EmployeeId=' + $scope.EmployeeId
        }).then(function successCallback(response) {
            $scope.SkillManagementStatusPlannedDetailsList = response.data;
            $scope.PlannedId = response.data[0].PlannedId;
            $scope.ResMaintenanceId = response.data[0].SMId;
            var gridObj = $("#GridPlannedEmployee").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#EmployeePopUp')).modal('show');
            $scope.GetReponsiblePersonPopUp($scope.PlannedId, $scope.ResMaintenanceId);
        }
        )
    }

   
    $scope.ReponsiblePersonList = [];
    $scope.GetReponsiblePersonPopUp = function (data) {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadReponsiblePersonList?Id=' + data + '&MaintenanceId=' + data.data.MaintenanceSchedulingId
        }).then(function successCallback(response) {
            $scope.ReponsiblePersonList = response.data;
            var gridObj = $("#GridResponsiblePopUp").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
        }
        )
    }

     $scope.PlannedId = null;
    $scope.ReponsiblePersonList = [];
    $scope.GetSummaryReponsiblePersonPopUp = function (data) {
        $scope.NewObject = data.data;
        var PlannedId = data.data.Id;
        $scope.PlannedId = PlannedId;
        $http({

            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadReponsiblePersonList?Id=' + $scope.PlannedId + '&MaintenanceId=' + data.data.MaintenanceSchedulingId
        }).then(function successCallback(response) {
            $scope.ReponsiblePersonList = response.data;
            var gridObj = $("#GridResponsiblePopUp").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
        }
        )
    }

    $scope.closeMachineSummaryPopUp = function () {
        angular.element(document.querySelector('#MachineAssetSummaryPop')).modal('hide');
    }

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePopUp')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.SavePlannedDetails = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.SkillManagementStatusPlannedDetailsList.length; i++) {
                if ($scope.SkillManagementStatusPlannedDetailsList[i].Flag == true) {
                    $scope.SaveList.push($scope.SkillManagementStatusPlannedDetailsList[i]);
                }
            }


            $http({
                method: 'POST',
                url: $scope.savePlannedUrl,
                data: {
                    "DataList": $scope.SaveList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    if ($scope.SD == 'Status Details')
                    {
                        $scope.GetEmployeePopUpGetDetails();
                        $scope.SD = null;
                    }
                    else
                    {
                        $scope.GetAssetPopUpDetails();
                        $scope.SD = null;
                    }
                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveResponsiblePerson = function () {
        try {

            $scope.SaveResponsibleList = [];
            for (var i = 0; i < $scope.ReponsiblePersonList.length; i++) {
                if ($scope.ReponsiblePersonList[i].IsActive == true) {
                    $scope.SaveResponsibleList.push($scope.ReponsiblePersonList[i]);
                }
            }


            $http({
                method: 'POST',
                url: $scope.saveResponsibleUrl,
                data: {
                    "DataList": $scope.SaveResponsibleList,
                    "PId": $scope.PlannedId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SkillManagementStatusSummaryReport = function () {
        var dataList = [];
        var g = $("#GridSkillManagementStatusSummary").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.SkillManagementStatusSummaryList;
        }

        $scope.fileName = "Skill Management Status Summary";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

   
    $scope.SkillManagementStatusDetailsReport = function () {
        var dataList = [];
        var g = $("#GridSkillManagementStatusDetails").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.SkillManagementStatusDetailsList;
        }

        $scope.fileName = "Skill Managemenet Status Details";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.rowDataBound = function rowDataBound(e) {

        if (e.data.OverDue > 0) {
            e.row.css("background-color", '#FFA500');
        }
        else if (e.data.OverDue === 0 && e.data.DueToday > 0) {

            e.row.css("background-color", '#d1e5ff');
        }

        else {
            e.row.css("background-color", '#FFFFFF');

        }


    }

    $scope.rowDataBoundDetails = function rowDataBoundDetails(e) {

        if (e.data.OverDue > 0) {
            e.row.css("background-color", '#FFA500');
        }
        else if (e.data.OverDue === 0 && e.data.DueToday > 0) {

            e.row.css("background-color", '#d1e5ff');
        }

        else {
            e.row.css("background-color", '#FFFFFF');

        }


    }
}

