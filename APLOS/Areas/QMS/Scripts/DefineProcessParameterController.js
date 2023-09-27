'use strict';
DefineProcessParameterController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function DefineProcessParameterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "DefineProcessParameter";
    $scope.Action = 'Save';
    $scope.path = 'QMS/DefineProcessParameter/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlArticle = $scope.path + 'createDefineProcessParameterArticle';
    $scope.saveUrlDefineProcess = $scope.path + 'createDefineProcessParameterProcess';
    $scope.saveUrlDefineProcessParameter = $scope.path + 'createDefineProcessParameterItem';
    $scope.saveUrlDefineProcessParameterReason = $scope.path + 'createDefineProcessParameterReason';
    $scope.saveUrlDefineProcessParameterCheckPoints = $scope.path + 'createDefineProcessParameterCheckPoints';
    $scope.saveUrlDefineWorkCenter = $scope.path + 'createDefineWorkCenter';

    $scope.tabPRM = 1;
    $scope.setTabPRM = function (newTab) {
        $scope.tabPRM = newTab;
    };

    $scope.isSetPRM = function (tabNum) {
        return $scope.tabPRM === tabNum;
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    //DefineProcessPara Start
    $scope.DefineProcessPara = {
        Id: null
        , Group: null
        , SubGroup: null
        , UserName: null
        , MasterId: null
        , ByWhomId: null
        , ByWhom: null
        , UserCode: null
        , DefineName: null
        , Remarks: null
    };
    $scope.DefineProcessParameterNew = Object.assign({}, $scope.DefineProcessPara);

    $scope.GroupList = [];
    $scope.GetGroupList  = function () {
        $http({
            method: 'GET',
            url: 'QMS/DefineProcessParameter/GetGroupList'
        }).then(function successCallback(response) {
            $scope.GroupList = response.data;
        });
    }
    $scope.GetGroupList();

    $scope.SubGroupList = [];
    $scope.getPPMSubGroup = function (group) {
        $http({
            method: 'GET',
            url: 'QMS/DefineProcessParameter/GetSubGroupList?Group=' + group
        }).then(function successCallback(response) {
            $scope.SubGroupList = response.data;
        });
    }

    $scope.UserNameList = [];
    $scope.getPPMUserName = function (Subgroup) {
        $http({
            method: 'GET',
            url: 'QMS/DefineProcessParameter/GetUserNameList?Subgroup=' + Subgroup
        }).then(function successCallback(response) {
            $scope.UserNameList = response.data;
        });
    }
    $scope.DefineProcessParameterList = [];
    $scope.LoadDefineProcessParameter = function () {
        try {
            $scope.DefineProcessParameterList = [];
            $http.get('QMS/DefineProcessParameter/LoadDefineProcessParameter')
                .then(function (response) {
                    $scope.DefineProcessParameterList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.LoadDefineProcessParameter();

    $scope.selectByWhom = function () {
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.DefineProcessParameterNew.ByWhomId = e.data.SystemId;
        $scope.DefineProcessParameterNew.ByWhom = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.GetDetails = function (args) {
        $scope.DefineProcessMasterId = args.data.Id;
        $scope.GroupName = args.data.Group;
        $scope.SubGroupName = args.data.SubGroup;
        $http({
            method: 'Get',
            url: 'QMS/DefineProcessParameter/LoadDefineProcessParameterEditData?DefineProcessParameterID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.DefineProcessParameterNew = response.data.defineprocessparameter[0];
            $scope.getPPMSubGroup($scope.GroupName);
            $scope.getPPMUserName($scope.SubGroupName);
            $scope.LoadArticleDetails($scope.DefineProcessMasterId);
            $scope.LoadDefineProcessParaProcessDetails($scope.DefineProcessMasterId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
       
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.DefineProcessParameterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'DefineProcessParameterData': $scope.DefineProcessParameterNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.DefineProcessParameterNew.Id = response.data.Id;
                    $scope.LoadDefineProcessParameter();
                    //$scope.LoadEntityDetails($scope.ProcessMasterId);
                    //$scope.LoadProcessParaActivityGroupDetails($scope.ProcessMasterId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'QMS/DefineProcessParameter/DefineProcessParameterDelete?id=' + $scope.DefineProcessParameterNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadDefineProcessParameter();
                DefineProcessParaClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Clear = function () {
        DefineProcessParaClearFields();
    };

    function DefineProcessParaClearFields() {
        $scope.Action = "Save";
        $scope.DefineProcessPara = {
            Id: null
            , Group: null
            , SubGroup: null
            , UserName: null
            , MasterId: null
            , ByWhomId: null
            , ByWhom: null
            , UserCode: null
            , DefineName: null
            , Remarks: null
        };
        $scope.DefineProcessParameterNew = Object.assign({}, $scope.DefineProcessPara);
    }

    $scope.DefineProcessParameterArticleList = [];
    $scope.LoadArticleDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/DefineProcessParameter/LoadDefineProcessParameterArticleDetails?MasterId=' + pid
        }).then(function successCallback(response) {
            $scope.DefineProcessParameterArticleList = response.data;
        }
        )
    }
    $scope.LoadArticleDetails();

    $scope.refreshTemplateArticle = function (args) {
        $("#Aheadchk").ejCheckBox({ "change": CheckBoxSelectAllArticle });
    };
    function CheckBoxSelectAllArticle(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridDefineProcessParameterArticle").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DefineProcessParameterArticleList.length; i++) {
                $scope.DefineProcessParameterArticleList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDefineProcessParameterArticle").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ArticleSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.DefineProcessParameterArticleList.length; i++) {
                if ($scope.DefineProcessParameterArticleList[i].Flag == true) {
                    $scope.DefineProcessParameterArticleList[i].DPPID = $scope.DefineProcessParameterNew.Id;
                    $scope.SaveList.push($scope.DefineProcessParameterArticleList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlArticle,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.DefineProcessParameterNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadArticleDetails($scope.DefineProcessParameterNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.DefineProcessParaProcessList = [];
    $scope.LoadDefineProcessParaProcessDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/DefineProcessParameter/LoadDefineProcessParaProcessDetails?MasterId=' + pid
        }).then(function successCallback(response) {
            $scope.DefineProcessParaProcessList = response.data;
        }
        )
    }
    $scope.LoadDefineProcessParaProcessDetails();

    $scope.refreshTemplateDefineProcess = function (args) {
        $("#DPheadchk").ejCheckBox({ "change": CheckBoxSelectAllDefineProcess });
    };
    function CheckBoxSelectAllDefineProcess(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridDefineProcessParaProcess").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DefineProcessParaProcessList.length; i++) {
                $scope.DefineProcessParaProcessList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDefineProcessParaProcess").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ProcessSave = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.DefineProcessParaProcessList.length; i++) {
                if ($scope.DefineProcessParaProcessList[i].Flag == true) {
                    $scope.DefineProcessParaProcessList[i].DPPID = $scope.DefineProcessParameterNew.Id;
                    $scope.SaveList.push($scope.DefineProcessParaProcessList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlDefineProcess,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.DefineProcessParameterNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadDefineProcessParaProcessDetails($scope.DefineProcessParameterNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.ProcessParameterLists = [];
    $scope.ProcessParameterId = null;
    $scope.ProcessParameterProcessId = null;
    $scope.ProcessParameterMastreId = null;
    $scope.getProcessParameterPopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ProcessParameterId = $scope.NewObject.Id;
        $scope.ProcessParameterProcessId = $scope.NewObject.ProcessId;
        $scope.ProcessParameterMastreId = $scope.NewObject.MasterId;
        
        try {
            $http.get('QMS/DefineProcessParameter/getDefineProcessParameterData?ProcessParameterId=' + $scope.NewObject.Id + '&ProcessId=' + $scope.NewObject.ProcessId + '&MasterId='+ $scope.NewObject.MasterId)
                .then(
                    function successCallback(response) {
                        $scope.ProcessParameterLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridProcessParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ProcessParameterPoUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.LoadDefineProcessParameterItemDetails = function () {
        $http({
            method: 'Get',
            url: 'QMS/DefineProcessParameter/LoadDefineProcessParameterItemDetails?ProcessParameterId=' + $scope.ProcessParameterId + '&ProcessId=' + $scope.ProcessParameterProcessId + '&MasterId=' + $scope.ProcessParameterMastreId
        }).then(function successCallback(response) {
            $scope.ProcessParameterLists = response.data;
        }
        )
    }

    $scope.refreshTemplateDefineProcessParameter = function (args) {
        $("#DPPheadchk").ejCheckBox({ "change": CheckBoxSelectAllDefineProcessParameter });
    };
    function CheckBoxSelectAllDefineProcessParameter(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridProcessParameter").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProcessParameterLists.length; i++) {
                $scope.ProcessParameterLists[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridProcessParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ProcessParameterSave = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProcessParameterLists.length; i++) {
                if ($scope.ProcessParameterLists[i].Flag == true) {
                    $scope.ProcessParameterLists[i].ProcessParameterId = $scope.ProcessParameterId;
                    $scope.SaveList.push($scope.ProcessParameterLists[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlDefineProcessParameter,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.ProcessParameterId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadDefineProcessParameterItemDetails($scope.ProcessParameterId, $scope.ProcessParameterProcessId, $scope.ProcessParameterMastreId);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.ReasonLists = [];
    $scope.ParameterId = null;
    $scope.ParameterMasterId = null;
    $scope.getReasonPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.NewObject.Id;
        $scope.ParameterId = ItemId;
        $scope.ParameterMasterId = $scope.NewObject.MasterItemId;

        try {
            $http.get('QMS/DefineProcessParameter/getDefineProcessParameterReasonData?ParameterId=' + $scope.NewObject.Id + '&MasterParameterId=' + $scope.NewObject.MasterItemId)
                .then(
                    function successCallback(response) {
                        $scope.ReasonLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridReason").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ReasonPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.LoadDefineProcessParameterReasonDetails = function () {
        $http({
            method: 'Get',
            url: 'QMS/DefineProcessParameter/LoadDefineProcessParameterReasonDetails?ParameterId=' + $scope.ParameterId + '&MasterParameterId=' + $scope.ParameterMasterId
        }).then(function successCallback(response) {
            $scope.ReasonLists = response.data;
        }
        )
    }

    $scope.refreshTemplateDefineReason = function (args) {
        $("#DRheadchk").ejCheckBox({ "change": CheckBoxSelectAllDefineReason });
    };
    function CheckBoxSelectAllDefineReason(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridReason").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ReasonLists.length; i++) {
                $scope.ReasonLists[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridReason").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SaveReasonData = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.ReasonLists.length; i++) {
                if ($scope.ReasonLists[i].Flag == true) {
                    $scope.ReasonLists[i].ParameterId = $scope.ParameterId;
                    $scope.SaveList.push($scope.ReasonLists[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlDefineProcessParameterReason,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.ParameterId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadDefineProcessParameterReasonDetails($scope.ParameterId,$scope.ParameterMastreId);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.ParameterCheckPointsLists = [];
    $scope.getParameterCheckPointsPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.NewObject.Id;
        $scope.ParameterId = ItemId;
        $scope.ParameterMasterId = $scope.NewObject.MasterItemId;

        try {
            $http.get('QMS/DefineProcessParameter/getDefineParameterCheckPointsData?ParameterId=' + $scope.NewObject.Id + '&MasterParameterId=' + $scope.NewObject.MasterItemId)
                .then(
                    function successCallback(response) {
                        $scope.ParameterCheckPointsLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameterCheckPoints").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ParameterCheckPointsPoUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.LoadDefineProcessParameterCheckPointsDetails = function () {
        $http({
            method: 'Get',
            url: 'QMS/DefineProcessParameter/LoadDefineProcessParameterCheckPointsDetails?ParameterId=' + $scope.ParameterId + '&MasterParameterId=' + $scope.ParameterMasterId
        }).then(function successCallback(response) {
            $scope.ParameterCheckPointsLists = response.data;
        }
        )
    }

    $scope.refreshTemplateDefineCheckPoints = function (args) {
        $("#CPheadchk").ejCheckBox({ "change": CheckBoxSelectAllDefineCheckPoints });
    };
    function CheckBoxSelectAllDefineCheckPoints(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridParameterCheckPoints").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ParameterCheckPointsLists.length; i++) {
                $scope.ParameterCheckPointsLists[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridParameterCheckPoints").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SaveCheckPointsData = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.ParameterCheckPointsLists.length; i++) {
                if ($scope.ParameterCheckPointsLists[i].Flag == true) {
                    $scope.ParameterCheckPointsLists[i].ParameterId = $scope.ParameterId;
                    $scope.SaveList.push($scope.ParameterCheckPointsLists[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlDefineProcessParameterCheckPoints,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.ParameterId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadDefineProcessParameterCheckPointsDetails($scope.ParameterId, $scope.ParameterMastreId);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.DefineProcessParameterWorkCenterList = [];
    $scope.getWorkCenterPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.NewObject.Id;
        $scope.ParameterId = ItemId;
        $scope.ParameterMasterId = $scope.NewObject.MasterItemId;
        try {
            $http.get('QMS/DefineProcessParameter/LoadDefineParameterWorkCenterDetails?ParameterId=' + $scope.ParameterId + '&MasterParameterId=' + $scope.ParameterMasterId)
                .then(
                    function successCallback(response) {
                        $scope.DefineProcessParameterWorkCenterList = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridDefineParameterWorkCenter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#DefineParameterWorkCenterPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.LoadDefineParameterWorkCenterDetails = function () {
        $http({

            method: 'Get',
            url: 'QMS/DefineProcessParameter/LoadDefineParameterWorkCenterDetails?ParameterId=' + $scope.ParameterId + '&MasterParameterId=' + $scope.ParameterMasterId
        }).then(function successCallback(response) {
            $scope.DefineProcessParameterWorkCenterList = response.data;
        }
        )
    }

    $scope.refreshTemplateDefineWorkCenter = function (args) {
        $("#DWheadchk").ejCheckBox({ "change": CheckBoxSelectAllDefineWorkCenter });
    };
    function CheckBoxSelectAllDefineWorkCenter(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridDefineParameterWorkCenter").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DefineProcessParameterWorkCenterList.length; i++) {
                $scope.DefineProcessParameterWorkCenterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDefineParameterWorkCenter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.DefineWorkCenterSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.DefineProcessParameterWorkCenterList.length; i++) {
                if ($scope.DefineProcessParameterWorkCenterList[i].Flag == true) {
                    $scope.DefineProcessParameterWorkCenterList[i].ParameterId = $scope.ParameterId;
                    $scope.SaveList.push($scope.DefineProcessParameterWorkCenterList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlDefineWorkCenter,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.ParameterId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadDefineParameterWorkCenterDetails($scope.ParameterId, $scope.ParameterMastreId);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
}

