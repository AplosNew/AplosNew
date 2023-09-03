'use strict';
QualityActionConfirmationController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function QualityActionConfirmationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "QualityActionConfirmation";
    $scope.Action = 'Save';
    $scope.path = 'Productions/QualityActionConfirmation/';
    $scope.saveUrlActionTaken = $scope.path + 'createActionTaken';
    $scope.ParameterStatusLists = [];
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate() - 10);
    $scope.ParameterStatusLists = [
        {
            'Value': 'InProgress',
            'Text': 'InProgress'
        },
        {
            'Value': 'Close',
            'Text': 'Close'
        },
        {
            'Value': 'Complete',
            'Text': 'Complete'
        }
    ];

    $scope.ReasonNameLists = [];
    $scope.GetReasonNameLists = function () {
        $http({
            method: 'GET',
            url: 'Productions/QualityActionConfirmation/GetReasonNameLists'
        }).then(function successCallback(response) {
            $scope.ReasonNameLists = response.data;
        });
    }
    //$scope.GetReasonNameLists();

    $scope.AddTile = function (e) {
        console.log(e);
        let ob = {};
        Object.assign(ob, e);
        ob.Flag = 0;
        ob.Id = null;
        ob.SNO = null;
        ob.ReasonId = null;
        ob.ReasonName = null;
        ob.ActionTaken = null;
        ob.ActionById = null;
        ob.ActionBy = null;
        ob.Remarks = null;
        ob.ConfirmRemarks = null;
        $scope.QualityActionTakenDetailsList.splice(e.Serial + 1, 0, ob);
        refreshSerial();
    }

    $scope.status = {
        Id: null,
        FromDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        ToDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        ActResponsiblePerson: $window.employeeName,
        ActResponsiblePersonId: $window.employeeId
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.selectActResponsiblePerson = function () {
        $scope.getActResponsiblePerson();
        angular.element(document.querySelector('#ActResponsiblePersonPopup')).modal('show');
    }

    $scope.ActResponsiblePersonList = [];
    $scope.getActResponsiblePerson = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ActResponsiblePersonList = resp.data;
        });
    }

    $scope.doubleActResponsiblePerson = function (e) {
        $scope.statusNew.ActResponsiblePersonId = e.data.SystemId;
        $scope.statusNew.ActResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ActResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeActResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ActResponsiblePersonPopup')).modal('hide');
    }

    $scope.selectActionBy = function (data) {
        $scope.NewObject = data.data;
        $scope.getActionBy();
        angular.element(document.querySelector('#ActionByPopup')).modal('show');
    }

    $scope.ActionByList = [];
    $scope.getActionBy = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetActionBy',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ActionByList = resp.data;
        });
    }

    $scope.doubleActionBy = function (e) {
        $scope.NewObject.ActionById = e.data.SystemId;
        $scope.NewObject.ActionBy = e.data.EmployeeName;
        angular.element(document.querySelector('#ActionByPopup')).modal('hide');
    }

    $scope.closeActionByPopUp = function () {
        angular.element(document.querySelector('#ActionByPopup')).modal('hide');
    }
   
    $scope.QualityActionUpdateHeaderList = [];
    $scope.View = function () {
        try {
            $scope.QCCompleteList = [];
            $http.get('Productions/QualityActionConfirmation/LoadQualityActionUpdateHeader?FromDate=' + $scope.statusNew.FromDate + '&ToDate=' + $scope.statusNew.ToDate + '&ResponsiblePersonId=' + $scope.statusNew.ActResponsiblePersonId)
                .then(function (response) {
                    $scope.QualityActionUpdateHeaderList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
   /* $scope.View();*/

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.status = {
            Id: null,
            FromDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
            ToDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
            ActResponsiblePerson: null,
            ActResponsiblePerson: null
        };
        $scope.statusNew = Object.assign({}, $scope.status);
    }
    $scope.rowDataBound = function rowDataBound(e) {

        if (e.data.Date == $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')) {
            e.row.css("background-color", '#FFFFFF');
        }
        else {
            e.row.css("background-color", '#FFD580');

        }
    }
    $scope.QCHeaderId = null;
    $scope.QualityActionUpdateParameterDetailsList = [];
    $scope.GetDetails = function (args) {
        $scope.QCHeaderId = args.data.HeaderId;
        $http({
            method: 'Get',
            url: 'Productions/QualityActionConfirmation/LoadQualityActionUpdateParameterListGetDetails?HeaderId=' + $scope.QCHeaderId
        }).then(function successCallback(response) {
            $scope.QualityActionUpdateParameterDetailsList = response.data;
            var gridObj = $("#GridQualityActionUpdate").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#QualityActionUpdatePop')).modal('show');
        }
        )
    }

    $scope.ActionTakenUpdate = {
        Id: null
        , SNO: null
        , ReasonId: null
        , ReasonName: null
        , ActionTaken: null
        , ActionById: null
        , ActionBy: null
        , Remarks: null
        , ConfirmRemarks: null
        , ParameterId: null
        , ParameterStatus: null
        , ConfirmationRemarks: null
    }
    $scope.ActionTakenUpdateNew = Object.assign({}, $scope.ActionTakenUpdate);

    $scope.QAUParameterId = null;
    $scope.QAUItemId = null;
    $scope.QAUStatus = null;
    $scope.QualityActionTakenDetailsList = [];
    $scope.GetActionTakenPopUp = function (args) {
        $scope.QAUParameterId = args.data.ParameterId;
        $scope.QAUItemId = args.data.ItemId;
        $scope.ActionTakenUpdateNew.ParameterStatus = args.data.Status;
        $http({
            method: 'Get',
            url: 'Productions/QualityActionConfirmation/LoadQualityActionTakenListGetDetails?ParameterId=' + $scope.QAUParameterId + '&ItemId=' + $scope.QAUItemId
        }).then(function successCallback(response) {
            $scope.QualityActionTakenDetailsList = response.data;
            for (var i = 0; i < $scope.QualityActionTakenDetailsList.length; i++) {
                Object.assign($scope.QualityActionTakenDetailsList[i], { 'Serial': parseInt(i) });
            }
        var gridObj = $("#GridQualityActionTaken").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#QualityActionTakenPop')).modal('show');
        }
        )
    }

    //$scope.ActionTakenSave = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.saveUrlActionTaken,
    //        data: { 
    //            'ActionTakenData': $scope.ActionTakenUpdateNew,
    //            'Pid': $scope.QAUParameterId
    //        },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            $scope.getActionTaken($scope.QAUParameterId);
    //            ActionTakenClearFields();

    //        }
    //    }), function errorCallBack(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    }
    //};

    $scope.ActionTakenSave = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ActionTakenUpdateNew.ConfirmationRemarks) && $scope.ActionTakenUpdateNew.ParameterStatus == 'InProgress')
            {
                throw 'Please Update Confirmation Remarks and Proceed!'
            }
            $scope.SaveList = [];
            for (var i = 0; i < $scope.QualityActionTakenDetailsList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.QualityActionTakenDetailsList[i].ActionTaken)) {
                    $scope.SaveList.push($scope.QualityActionTakenDetailsList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlActionTaken,
                data: {
                    'DataList': $scope.SaveList,
                    'PId': $scope.QAUParameterId,
                    'Status': $scope.ActionTakenUpdateNew.ParameterStatus,
                    'ConfirmationRemarks': $scope.ActionTakenUpdateNew.ConfirmationRemarks
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector('#QualityActionUpdatePop')).modal('hide');
                    angular.element(document.querySelector('#QualityActionTakenPop')).modal('hide');
                    //$scope.getActionTaken($scope.QAUParameterId, $scope.QAUItemId);
                    ActionTakenClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.ActionTakenClear = function () {
        ActionTakenClearFields();
    };

    function ActionTakenClearFields() {
        $scope.Action = "Save";
        $scope.ActionTakenUpdateNew = Object.assign({}, $scope.ActionTakenUpdate);
    }

    $scope.getActionTaken = function (data) {
        try {
            $http.get('Productions/QualityActionConfirmation/LoadQualityActionTakenListGetDetails?ParameterId=' + data)
                .then(
                    function successCallback(response) {
                        $scope.QualityActionTakenDetailsList = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridQualityActionTaken").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }
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

    $scope.PlannedId = null;
    $scope.ReponsiblePersonList = [];
    $scope.GetReponsiblePersonPopUp = function (data) {
        $scope.NewObject = data.data;
        var PlannedId = data.data.PlannedId;
        $scope.PlannedId = PlannedId;
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadReponsiblePersonList?Id=' + $scope.PlannedId + '&MaintenanceId='+ data.data.Id
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

    $scope.refreshTemplateMachineAsset = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllMachineAsset });
    };
    function CheckBoxSelectAllMachineAsset(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPlannedMachineAsset").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MaintenanceStatusPlannedDetailsList.length; i++) {
                $scope.MaintenanceStatusPlannedDetailsList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.Asset = null;
    $scope.MaintenanceStatusPlannedDetailsList = [];
    $scope.GetAssetPopUp = function (data) {
        $scope.PlannedId = data.data.PlannedId;
        //$scope.Asset = data.data.AssetId;
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenancePendingdScheduleList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + data.data.PlannedId
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }
    $scope.GetAssetDetails = function () {

        $http({
            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenancePendingdScheduleList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.PlannedId
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }
    $scope.closeMachinePopUp = function () {
        angular.element(document.querySelector('#MachineAssetPop')).modal('hide');
    }
    $scope.SavePlannedDetails = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.MaintenanceStatusPlannedDetailsList.length; i++) {
                if ($scope.MaintenanceStatusPlannedDetailsList[i].Flag == true) {
                    $scope.SaveList.push($scope.MaintenanceStatusPlannedDetailsList[i]);
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
                    $scope.GetAssetDetails();
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
    $scope.uploadUrl = "Productions/QualityActionConfirmation/SaveDefault";
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
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenancePendingdScheduleList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.PlannedId 
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }



    //#endregion
}

