'use strict';
pendingSkillManagementController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function pendingSkillManagementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "PendingSkillManagement";
    $scope.Action = 'Save';
    $scope.path = 'Machines/PendingSkillManagement/';
    $scope.savePlannedUrl = $scope.path + 'createPlanned';
    $scope.saveResponsibleUrl = $scope.path + 'createResponsible';
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate() + 7);
    /*var firstDay = new Date(y, m, 1);*/
    $scope.status = {
        Id: null,
        FromDate: null,
        ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        Responsible: null,
        WorkCenter: null,
        ActResponsiblePerson:null,
        Status:'Pending',
        Asset: null
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.GetFromDateList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagementDetails/GetFromDateList'
        }).then(function successCallback(response) {
            //$scope.statusNew.FromDate = response.data[0].FromDate;
        });
    }
    $scope.GetFromDateList();

    $scope.ActionablePersonList=[];
    $scope.GetActionablePersonList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagementDetails/GetActionablePersonList'
        }).then(function successCallback(response) {
            $scope.ActionablePersonList = response.data;
        });
    }
    $scope.GetActionablePersonList();
    
    $scope.PendingSkillManagementList = [];
    $scope.View = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.statusNew.ToDate)) {
                throw "To Date is required.";
            }

            $scope.PendingSkillManagementList = [];

            $http({
                method: 'POST',
                url: $scope.path + "LoadPendingSkillMangament",
                data: { 'ActResponsiblePerson': $scope.statusNew.ActResponsiblePerson, 'todate': $scope.statusNew.ToDate, 'fromDate': $scope.statusNew.FromDate, 'Status' : $scope.statusNew.Status},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.PendingSkillManagementList = response.data;
                    var gridObj = $("#GridPendingSkillManagement").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.rowDataBound = function rowDataBound(e) {

        if (new Date(e.data.PlannedDate) < new Date($scope.statusNew.ToDate))
        {
            e.row.css("background-color", '#FFA500');
        }
        else if (new Date(e.data.PlannedDate) > new Date($scope.statusNew.ToDate))
        {
           
            e.row.css("background-color", '#FFFFFF');
        }

        else
        {
            e.row.css("background-color", '#d1e5ff');

        }
    }

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

    $scope.PlannedId = null;
    $scope.ReponsiblePersonList = [];
    $scope.GetReponsiblePersonPopUp = function (data) {
        $scope.NewObject = data.data;
        var PlannedId = data.data.PlannedId;
        $scope.PlannedId = PlannedId;
        $http({

            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadReponsiblePersonList?Id=' + $scope.PlannedId + '&MaintenanceId='+ data.data.Id
        }).then(function successCallback(response) {
            $scope.ReponsiblePersonList = response.data;
            var gridObj = $("#GridResponsiblePopUp").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
        }
        )
    }


    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

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

    $scope.Asset = null;
    $scope.SkillManagementStatusPlannedDetailsList = [];
    $scope.GetEmployeePopUp = function (data) {
        $scope.PlannedId = data.data.PlannedId;
        $http({
            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadSkillManagementPendingdScheduleList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + data.data.PlannedId
        }).then(function successCallback(response) {
            $scope.SkillManagementStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedEmployee").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#EmployeeePopUp')).modal('show');
        }
        )
    }
    $scope.GetEmployeeDetails = function () {

        $http({
            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadSkillManagementPendingdScheduleList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.PlannedId
        }).then(function successCallback(response) {
            $scope.SkillManagementStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedEmployee").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#EmployeeePopUp')).modal('show');
        }
        )
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeeePopUp')).modal('hide');
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

                    ShowResult(response.data.Message, 'success');
                    $scope.GetEmployeeDetails();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //#region MOI File 
    $scope.ItemId = null;
    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull(args.model.Data))
                throw 'Please select/save the order first'
            $scope.ItemId = args.model.Data;
            args.data = args.model.Data;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "Machines/PendingSkillManagement/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ItemId))
            ShowResult('Please select/save the order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    $scope.FileDownload = function (data,test) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        if (test == 'id') {
            $scope.dwonloadUrl = virtualPath.MSAPath + '/' + data.Id + extention;
            test = null;
        }
        else {
            $scope.dwonloadUrl = virtualPath.MSAPath + '/' + data.PlannedId + extention;
            test = null;
        }
    };

    $scope.getFileList = function () {

        $http({
            method: 'Get',
            url: 'Machines/SkillManagementDetails/LoadMaintenancePendingdScheduleList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.PlannedId 
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }



    //#endregion
}

