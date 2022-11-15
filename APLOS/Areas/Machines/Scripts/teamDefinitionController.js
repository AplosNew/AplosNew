'use strict';
teamDefinitionController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function teamDefinitionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "TeamDefinition";
    $scope.Action = 'Save';
    $scope.path = 'Machines/TeamDefinition/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlBudgetCode = $scope.path + 'createBudgetCode';
    $scope.saveUrlEntity = $scope.path + 'createEntity';
    $scope.saveUrlEACategory = $scope.path + 'createEACategory';
    $scope.saveUrlEmployee = $scope.path + 'createEmployee';
    $scope.saveUrlTeamCategory = $scope.path + 'createTeamCategory';
    $scope.saveUrlTeamDefinitionCategory = $scope.path + 'createTeamDefinitionCategory';
    $scope.ResponsibilityLevelList = [];

    $scope.ResponsibilityLevelList = [
        {
            'Value': 1,
            'Text': '1'
        },
        {
            'Value': 2,
            'Text': '2'
        },
        {
            'Value': 3,
            'Text': '3'
        },
        {
            'Value': 4,
            'Text': '4'
        },
        {
            'Value': 5,
            'Text': '5'
        },
        {
            'Value': 6,
            'Text': '6'
        }
    ];

    $scope.EActivityCategoryList = [];
    $scope.GetEActivityCategoryList = function () {
        $http({
            method: 'GET',
            url: 'Machines/TeamDefinition/GetEActivityCategoryList'
        }).then(function successCallback(response) {
            $scope.EActivityCategoryList = response.data;
        });
    }
    $scope.GetEActivityCategoryList();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        TeamLeaderId: null,
        TeamLeader: null,
        StandardHours: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Category = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.EACategory = Object.assign({}, $scope.Category);

    $scope.TeamCategory = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.TeamCategoryNew = Object.assign({}, $scope.TeamCategory);
    
    $scope.GenerateSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/TeamDefinition/GetAutoSequenceNo'
        }).then(function successCallback(response) {
            $scope.ModelNew.Sequence = response.data;
        });
    }
    $scope.GenerateSequenceNo();

    $scope.GenerateCategroySequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/TeamDefinition/GetCategorySequenceNo'
        }).then(function successCallback(response) {
            $scope.EACategory.Sequence = response.data;
        });
    }
    $scope.GenerateCategroySequenceNo();

    $scope.GenerateTeamCategroySequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/TeamDefinition/GetTeamCategorySequenceNo'
        }).then(function successCallback(response) {
            $scope.TeamCategoryNew.Sequence = response.data;
        });
    }
    $scope.GenerateTeamCategroySequenceNo();

    $scope.selectTeamLeader = function () {
        $scope.getEmployee();
         angular.element(document.querySelector('#TeamLeaderPopup')).modal('show');
    }

    $scope.TeamLeaderList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.TeamLeaderList = resp.data;
        });
    }

    $scope.doubleTeamLeader = function (e) {
        $scope.ModelNew.TeamLeaderId = e.data.SystemId;
        $scope.ModelNew.TeamLeader = e.data.EmployeeName;
        angular.element(document.querySelector('#TeamLeaderPopup')).modal('hide');
    }

    $scope.closeTeamLeaderPopUp = function () {
        angular.element(document.querySelector('#TeamLeaderPopup')).modal('hide');
    }

    $scope.TeamLeaderList = [];
    $scope.LoadTeamLeaderList = function () {
        $http({

            method: 'Get',
            url: 'Machines/TeamDefinition/LoadTeamLeaderList'
        }).then(function successCallback(response) {
            $scope.TeamLeaderList = response.data;
            var gridObj = $("#GridTeamLeaderMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadTeamLeaderList();

    $scope.EACategoryList = [];
    $scope.LoadEACategoryDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/TeamDefinition/LoadEACategoryDetails'
        }).then(function successCallback(response) {
            $scope.EACategoryList = response.data;
        }
        )
    }
    $scope.LoadEACategoryDetails();

    $scope.TeamCategoryList = [];
    $scope.LoadTeamCategoryDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/TeamDefinition/LoadTeamCategoryDetails'
        }).then(function successCallback(response) {
            $scope.TeamCategoryList = response.data;
        }
        )
    }
    $scope.LoadTeamCategoryDetails();

    $scope.GetEACategoryDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/TeamDefinition/LoadEACategoryEditData?CategoryId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.EACategory = response.data.category[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetTeamCategoryDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/TeamDefinition/LoadTeamCategoryEditData?TeamCategoryId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.TeamCategoryNew = response.data.teamcategory[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetDetails = function (args) {
        $scope.TeamMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'Machines/TeamDefinition/LoadTeamDefinitionEditData?TeamID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ModelNew = response.data.team[0];
            $scope.LoadBudgetCodeDetails($scope.TeamMasterId);
            $scope.LoadEmployeeDetails($scope.TeamMasterId);
            $scope.LoadEntityDetails($scope.TeamMasterId);
            $scope.LoadTeamDefinitionCategoryDetails($scope.TeamMasterId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.TeamDefinitionForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'TeamData': $scope.ModelNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadTeamLeaderList();
                    TeamClearFields($scope.GenerateSequenceNo());

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    $scope.EACategorySave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.EmployeeActivityCategoryForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlEACategory,
                data: {
                    'EACategoryData': $scope.EACategory,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadEACategoryDetails();
                    EACategoryFields($scope.GenerateCategroySequenceNo());

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.TeamCategorySave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.TeamCategoryForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlTeamCategory,
                data: {
                    'TeamCategoryData': $scope.TeamCategoryNew,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadTeamCategoryDetails();
                    TeamCategoryFields($scope.GenerateTeamCategroySequenceNo());

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.Clear = function () {
        TeamClearFields($scope.GenerateSequenceNo());
    };

    $scope.EACategoryClear = function () {
        EACategoryFields($scope.GenerateCategroySequenceNo());
    };

    $scope.TeamCategoryClear = function () {
        TeamCategoryFields($scope.GenerateTeamCategroySequenceNo());
    };

    function TeamClearFields(seq) {
        $scope.Action = "Save";
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }

    function EACategoryFields(seq) {
        $scope.Action = "Save";
        $scope.EACategory = Object.assign({}, $scope.Category);
        $scope.EACategory.Sequence = seq;
    }

    function TeamCategoryFields(seq) {
        $scope.Action = "Save";
        $scope.TeamCategoryNew = Object.assign({}, $scope.TeamCategory);
        $scope.TeamCategoryNew.Sequence = seq;
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Machines/TeamDefinition/TeamDelete?id=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadTeamLeaderList();
                TeamClearFields($scope.GenerateSequenceNo());
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.EACategoryDelete = function () {
        $http({
            method: 'POST',
            url: 'Machines/TeamDefinition/EACategoryDelete?id=' + $scope.EACategory.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadEACategoryDetails();
                EACategoryFields($scope.GenerateCategroySequenceNo());
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.TeamCategoryDelete = function () {
        $http({
            method: 'POST',
            url: 'Machines/TeamDefinition/TeamCategoryDelete?id=' + $scope.TeamCategoryNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadTeamCategoryDetails();
                TeamCategoryFields($scope.GenerateTeamCategroySequenceNo());
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.TeamBudgetCodeList = [];
    $scope.LoadBudgetCodeDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'Machines/TeamDefinition/LoadBudgetCodeDetails?TeamId=' + pid
        }).then(function successCallback(response) {
            $scope.TeamBudgetCodeList = response.data;
        }
        )
    }

    $scope.TeamEmployeeList = [];
    $scope.LoadEmployeeDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'Machines/TeamDefinition/LoadEmployeeDetails?TeamId=' + pid
        }).then(function successCallback(response) {
            $scope.TeamEmployeeList = response.data;
        }
        )
    }

    $scope.TeamEntityList = [];
    $scope.LoadEntityDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'Machines/TeamDefinition/LoadEntityDetails?TeamId=' + pid
        }).then(function successCallback(response) {
            $scope.TeamEntityList = response.data;
        }
        )
    }

    $scope.TeamDefinitionCategoryList = [];
    $scope.LoadTeamDefinitionCategoryDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'Machines/TeamDefinition/LoadTeamDefinitionCategoryDetails?TeamId=' + pid
        }).then(function successCallback(response) {
            $scope.TeamDefinitionCategoryList = response.data;
        }
        )
    }

    $scope.refreshTemplateBudgetCode = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllBudgetCode });
    };
    function CheckBoxSelectAllBudgetCode(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridBudgetCode").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.TeamBudgetCodeList.length; i++) {
                $scope.TeamBudgetCodeList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridBudgetCode").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.refreshTemplateEmployee = function (args) {
        $("#Empheadchk").ejCheckBox({ "change": CheckBoxSelectAllEmployee });
    };
    function CheckBoxSelectAllEmployee(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmployee").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.TeamEmployeeList.length; i++) {
                $scope.TeamEmployeeList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmployee").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };


    $scope.refreshTemplateEntity = function (args) {
        $("#Eheadchk").ejCheckBox({ "change": CheckBoxSelectAllEntity });
    };
    function CheckBoxSelectAllEntity(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEntity").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.TeamEntityList.length; i++) {
                $scope.TeamEntityList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEntity").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.refreshTemplateTeamDefinitionCategory = function (args) {
        $("#TDCheadchk").ejCheckBox({ "change": CheckBoxSelectAllTeamCategory });
    };
    function CheckBoxSelectAllTeamCategory(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridTeamDefinitionCategory").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.TeamDefinitionCategoryList.length; i++) {
                $scope.TeamDefinitionCategoryList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridTeamDefinitionCategory").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.BudgetCodeSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.TeamBudgetCodeList.length; i++) {
                if ($scope.TeamBudgetCodeList[i].Flag == true) {
                    $scope.TeamBudgetCodeList[i].TeamDefinitionId = $scope.ModelNew.Id;
                    $scope.SaveList.push($scope.TeamBudgetCodeList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlBudgetCode,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadBudgetCodeDetails($scope.ModelNew.Id);
                    $scope.LoadEmployeeDetails($scope.ModelNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.EmployeeSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.TeamEmployeeList.length; i++) {
                if ($scope.TeamEmployeeList[i].Flag == true) {
                    $scope.TeamEmployeeList[i].TeamDefinitionId = $scope.ModelNew.Id;
                    $scope.SaveList.push($scope.TeamEmployeeList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlEmployee,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadEmployeeDetails($scope.ModelNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.EntitySave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.TeamEntityList.length; i++) {
                if ($scope.TeamEntityList[i].Flag == true) {
                    $scope.TeamEntityList[i].TeamDefinitionId = $scope.ModelNew.Id;
                    $scope.SaveList.push($scope.TeamEntityList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlEntity,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadEntityDetails($scope.ModelNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.TeamDefinitionCategorySave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.TeamDefinitionCategoryList.length; i++) {
                if ($scope.TeamDefinitionCategoryList[i].Flag == true) {
                    $scope.TeamDefinitionCategoryList[i].TeamDefinitionId = $scope.ModelNew.Id;
                    $scope.SaveList.push($scope.TeamDefinitionCategoryList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlTeamDefinitionCategory,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadTeamDefinitionCategoryDetails($scope.ModelNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

     $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tabteam = 1;
    $scope.setTabTeam = function (newTab) {
        $scope.tabteam = newTab;
    };

    $scope.isSetteam = function (tabNum) {
        return $scope.tabteam === tabNum;
    };
}