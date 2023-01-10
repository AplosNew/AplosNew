'use strict';
skillManagementController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function skillManagementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "SkillManagement";
    $scope.CriticalLevelLists = [];
    $scope.GroupList = [];
    $scope.Action = 'Save';
    $scope.path = 'Machines/SkillManagement/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlEntity = $scope.path + 'createEntity';
    $scope.saveUrlPositionCode = $scope.path + 'createPositionCode';
    $scope.saveUrlLevel = $scope.path + 'createLevel';
    $scope.saveUrlItem = $scope.path + 'createItem';
    $scope.saveUrlParameter = $scope.path + 'createParameter';
    $scope.saveUrlBudgetCode = $scope.path + 'createBudgetCode';
    $scope.saveUrlTeamDefinition = $scope.path + 'createTeamDefinition';
    $scope.saveUrlGrading = $scope.path + 'createGrading';
    
    $scope.CriticalLevelLists = [
        {
            'Value': 'Normal',
            'Text': 'Normal'
        },
        {
            'Value': 'Important',
            'Text': 'Important'
        },
        {
            'Value': 'Critical',
            'Text': 'Critical'
        }
    ];
    
    $scope.PerformanceGroupList = [];
    $scope.GetPerformanceGroupList = function (pid) {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagement/GetPerformanceGroupList?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.PerformanceGroupList = response.data;
        });
    }
    $scope.GetPerformanceGroupList();

    $scope.GroupList = [
        {
            'Value': '1',
            'Text': '1'
        },
        {
            'Value': '2',
            'Text': '2'
        },
        {
            'Value': '3',
            'Text': '3'
        },
        {
            'Value': '4',
            'Text': '4'
        }
    ];
    $scope.schedule = {
        Id: null
        , ScheduleCode: null
        , ProcessId: null
        , SubProcessId: null
        , StandaredName: null
        , ScheduleDays: null
        , MinScheduleMinutes: null
        , ResponsiblePersoneBgtCodeId: null
        , ResponsiblePersoneBgtCode: null
        , UserName:null
        , MinScheduleDays: null
        , MaxScheduleMinutes: null
        , MaxScheduleDays: null
        , StandardScheduleMinutes: null
        , IsActive: true
        , Department: null
        , DepartmentId: null
        , TrainingGroup: null
        , AdvancePlanningDays: null

    };
    $scope.scheduleNew = Object.assign({}, $scope.schedule);

    $scope.SkillLevel = {
        Id: null
        , SMID: null
        , SNO: null
        , PerformanceGroup: null
        , PerformanceDetails: null
        , PerformancePoints: null
        , Remarks: null
    }
    $scope.SkillLevelNew = Object.assign({}, $scope.SkillLevel);

    $scope.Item = {
        Id: null
        , SNO: null
        , ItemName: null
        , CriticalLevel: null
        , IsAuditable: null
        , ByWhomId:null
        , ByWhom:null
        , Remarks: null
        , SMID: null
        , PerformanceGroupId:null
        , ItemMinutes: null
        , ExceptionDays: null
        , ReportApplicable: true
        , MaximumPoints: null
        , MinimumPoints: null
    };
    $scope.ItemNew = Object.assign({}, $scope.Item);

    $scope.Parameter = {
        Id: null
        , SNO: null
        , CheckPoints: null
        , Remarks: null
        , ItemId:null
    }
    $scope.ParameterNew = Object.assign({}, $scope.Parameter);

    $scope.PersonBudget = {
        Id: null
        , SNO: null
        , PersonBudgetCodeId: null
        , PersonBudgetCode: null
        , Group: null
        , SMID: null
    }
    $scope.PersonBudgetNew = Object.assign({}, $scope.PersonBudget);

    $scope.TeamDefinition = {
        Id: null
        , SNO: null
        , TeamDefinitionId: null
        , TeamDefinition: null
        , SMID: null
    }
    $scope.TeamDefinitionNew = Object.assign({}, $scope.TeamDefinition);

    $scope.Grade = {
        Id: null
        , SMID: null
        , PerformanceGroup: null
        , Grade1: null
        , Grade2: null
        , Grade3: null
        , Grade4: null
    }
    $scope.GradeNew = Object.assign({}, $scope.Grade);

    $scope.ProcessList = [];
    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagement/GetProcessList'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    $scope.GetProcessList();

    $scope.SubProcessList = [];
    $scope.GetSubProcessList = function (id) {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagement/GetSubProcessList?Pid=' + id
        }).then(function successCallback(response) {
            $scope.SubProcessList = response.data;
        });
    }

    $scope.SkillManagementMasterList = [];
    $scope.LoadSkillManagementMasterList = function () {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagement/LoadSkillManagementMasterList'
        }).then(function successCallback(response) {
            $scope.SkillManagementMasterList = response.data;
            var gridObj = $("#GridSkillManagementSchedulingMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadSkillManagementMasterList();

    $scope.GetDetails = function (args) {
        $scope.ScheduleMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'Machines/SkillManagement/LoadScheduleEditData?ScheduleID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.scheduleNew = response.data.schedule[0];
            $scope.scheduleNew.ResponsiblePersoneBgtCode = response.data.schedule[0].ResponsiblePersoneBgtCode;
            $scope.GetSubProcessList($scope.scheduleNew.ProcessId);
            $scope.GetPerformanceGroupList($scope.ScheduleMasterId);
            $scope.LoadEntityDetails($scope.ScheduleMasterId);
            $scope.LoadPositionCodeDetails($scope.ScheduleMasterId);
            $scope.LoadSkillLevelDetails($scope.ScheduleMasterId);
            $scope.GeneratSkillLevelSequenceNo($scope.ScheduleMasterId);
            $scope.LoadItemDetails($scope.ScheduleMasterId);
            $scope.GeneratItemSequenceNo($scope.ScheduleMasterId);
            $scope.LoadBudgetCodeDetails($scope.ScheduleMasterId);
            $scope.GeneratPersonBudgetSequenceNo($scope.ScheduleMasterId);
            $scope.LoadTeamDefinitionDetails($scope.ScheduleMasterId);
            $scope.LoadGradingDetails($scope.ScheduleMasterId);
            $scope.GeneratTeamDefinitionSequenceNo($scope.ScheduleMasterId);

            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.SkillManagementEntityList = [];
    $scope.LoadEntityDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagement/LoadEntityDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.SkillManagementEntityList = response.data;
        }
        )
    }

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
            for (var i = 0; i < $scope.SkillManagementEntityList.length; i++) {
                $scope.SkillManagementEntityList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEntity").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.EntitySave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SkillManagementEntityList.length; i++) {
                if ($scope.SkillManagementEntityList[i].Flag == true) {
                    $scope.SkillManagementEntityList[i].SMID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.SkillManagementEntityList[i]);
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
                    $scope.LoadEntityDetails($scope.scheduleNew.Id);
                    $scope.LoadPositionCodeDetails($scope.scheduleNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.refreshTemplatePositionCode = function (args) {
        $("#PCheadchk").ejCheckBox({ "change": CheckBoxSelectAllPositionCode });
    };
    function CheckBoxSelectAllPositionCode(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPositionCode").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SchedulePositionCodeList.length; i++) {
                $scope.SchedulePositionCodeList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPositionCode").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SchedulePositionCodeList = [];
    $scope.LoadPositionCodeDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagement/LoadPositionCodeDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.SchedulePositionCodeList = response.data;
        }
        )
    }

    $scope.PositionCodeSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SchedulePositionCodeList.length; i++) {
                if ($scope.SchedulePositionCodeList[i].Flag == true) {
                    $scope.SchedulePositionCodeList[i].SMID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.SchedulePositionCodeList[i]);
                }
            }


            $http({
                method: 'POST',
                url: $scope.saveUrlPositionCode,
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
                    $scope.LoadPositionCodeDetails($scope.scheduleNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };


    // #region For AutoSequenceNo
    $scope.GeneratSkillLevelSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagement/GetSkillLevelAutoSequence?scheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.SkillLevelNew.SNO = response.data;
        });
    }
    $scope.GeneratSkillLevelSequenceNo();

    $scope.GeneratItemSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagement/GetItemAutoSequence?scheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ItemNew.SNO = response.data;
        });
    }
    $scope.GeneratItemSequenceNo();

    $scope.GeneratPersonBudgetSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagement/GetPersonBudgetAutoSequence?scheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.PersonBudgetNew.SNO = response.data;
        });
    }
    $scope.GeneratPersonBudgetSequenceNo();

    $scope.GeneratTeamDefinitionSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/SkillManagement/GetTeamDefinitionAutoSequence?scheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.TeamDefinitionNew.SNO = response.data;
        });
    }
    $scope.GeneratTeamDefinitionSequenceNo();
   
    
    $scope.ScheduleSkillLevelList = [];
    $scope.LoadSkillLevelDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagement/LoadSkillLevelDetails?ScheduleId='+$scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleSkillLevelList = response.data;
        }
        )
    }

    $scope.ScheduleItemList = [];
    $scope.LoadItemDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagement/LoadItemDetails?ScheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleItemList = response.data;
        }
        )
    }

   
    $scope.ScheduleBudgetCodeList = [];
    $scope.LoadBudgetCodeDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagement/LoadBudgetCodeDetails?ScheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleBudgetCodeList = response.data;
        }
        )
    }
    $scope.ScheduleTeamDefinitionList = [];
    $scope.LoadTeamDefinitionDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagement/LoadTeamDefinitionDetails?ScheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleTeamDefinitionList = response.data;
        }
        )
    }

    $scope.ScheduleGradingList = [];
    $scope.LoadGradingDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagement/LoadGradingDetails?ScheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleGradingList = response.data;
        }
        )
    }
    
    $scope.selectBudgetCode = function () {
        $scope.getBudgetCode();
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('show');
    }

    $scope.BudgetCodeList = [];
    $scope.getBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetBudgetCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.BudgetCodeList = resp.data;
        });
    }

    $scope.doubleBudgetCode = function (e) {
        $scope.scheduleNew.ResponsiblePersoneBgtCodeId = e.data.ManPowerBudgetId;
        $scope.scheduleNew.ResponsiblePersoneBgtCode = e.data.Code;
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('hide');
    }

    $scope.closeBudgetCodePopUp = function () {
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('hide');
    }

    $scope.selectByWhomeBudgetCode = function () {
        $scope.ByWhomeBudgetCode();
        angular.element(document.querySelector('#ByWhomPop')).modal('show');
    }

    $scope.ByWhomeBudgetCodeList = [];
    $scope.ByWhomeBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetByWhomeBudgetCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ByWhomeBudgetCodeList = resp.data;
        });
    }

    $scope.doubleByWhomeBudgetCode = function (e) {
        $scope.ItemNew.ByWhomId = e.data.ManPowerBudgetId;
        $scope.ItemNew.ByWhom = e.data.Code;
        angular.element(document.querySelector('#ByWhomPop')).modal('hide');
    }

    $scope.closeByWhomPopUp = function () {
        angular.element(document.querySelector('#ByWhomPop')).modal('hide');
    }

    $scope.selectPersonBudgetCode = function () {
        $scope.getPersonBudgetCode();
        angular.element(document.querySelector('#PersonBudgetCodePopUp')).modal('show');
    }

    $scope.PersonBudgetCodeList = [];
    $scope.getPersonBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPersonBudgetCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PersonBudgetCodeList = resp.data;
        });
    }

    $scope.doublePersonBudgetCode = function (e) {
        $scope.PersonBudgetNew.PersonBudgetCodeId = e.data.ManPowerBudgetId;
        $scope.PersonBudgetNew.PersonBudgetCode = e.data.Code;
        angular.element(document.querySelector('#PersonBudgetCodePopUp')).modal('hide');
    }

    $scope.closePersonBudgetCodePopUp = function () {
        angular.element(document.querySelector('#PersonBudgetCodePopUp')).modal('hide');
    }

    $scope.selectTeamDefinition = function () {
        $scope.getTeamDefinition();
        angular.element(document.querySelector('#TeamDefinitionPopUp')).modal('show');
    }

    $scope.TeamDefinitionList = [];
    $scope.getTeamDefinition = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetTeamDefinition',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.TeamDefinitionList = resp.data;
        });
    }

    $scope.doubleTeamDefinition = function (e) {
        $scope.TeamDefinitionNew.TeamDefinitionId = e.data.Id;
        $scope.TeamDefinitionNew.TeamDefinition = e.data.UserName;
        angular.element(document.querySelector('#TeamDefinitionPopUp')).modal('hide');
    }

    $scope.closeTeamDefinitionPopUp = function () {
        angular.element(document.querySelector('#TeamDefinitionPopUp')).modal('hide');
    }

    $scope.selectDepartment = function () {
        $scope.getDepartment();
        angular.element(document.querySelector('#DepartmentPopUp')).modal('show');
    }

    $scope.DepartmentList = [];
    $scope.getDepartment = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetDepartment',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.DepartmentList = resp.data;
        });
    }

    $scope.doubleDepartment = function (e) {
        $scope.scheduleNew.DepartmentId = e.data.DepartmentId;
        $scope.scheduleNew.Department = e.data.Department;
        angular.element(document.querySelector('#DepartmentPopUp')).modal('hide');
    }

    $scope.closeDepartmentPopUp = function () {
        angular.element(document.querySelector('#DepartmentPopUp')).modal('hide');
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MaintenanceScheduleForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'ScheduleData': $scope.scheduleNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadSkillManagementMasterList();
                    ScheduleClearFields();
                 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    $scope.SkillLevelSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SkillManagementLevelForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlLevel,
                data: {
                    'LevelData': $scope.SkillLevelNew,
                    'Pid': $scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadSkillLevelDetails($scope.scheduleNew.Id);
                    SkillLevelClearFields($scope.GeneratSkillLevelSequenceNo($scope.scheduleNew.Id));
                    $scope.GetPerformanceGroupList($scope.ScheduleMasterId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.ItemSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SkillManagementItemForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlItem,
                data: {
                    'ItemData': $scope.ItemNew,
                    'Pid':$scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadItemDetails($scope.scheduleNew.Id);
                    ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.BudgetCodeSave = function () {
            $http({
                method: 'POST',
                url: $scope.saveUrlBudgetCode,
                data: {
                    'BudgetCodeData': $scope.PersonBudgetNew,
                    'Pid': $scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadBudgetCodeDetails($scope.scheduleNew.Id);
                    BudgetCodeClearFields($scope.GeneratPersonBudgetSequenceNo($scope.scheduleNew.Id));

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    };

    $scope.TeamDefinitionSave = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlTeamDefinition,
            data: {
                'TeamDefinitionData': $scope.TeamDefinitionNew,
                'Pid': $scope.scheduleNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadTeamDefinitionDetails($scope.scheduleNew.Id);
                TeamDefinitionClearFields($scope.GeneratTeamDefinitionSequenceNo($scope.scheduleNew.Id));

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.GradingSave = function () {
        $http({
            method: 'POST',
            url:$scope.saveUrlGrading,
            data: {
                'GradingData': $scope.GradeNew,
                'Pid': $scope.scheduleNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadGradingDetails($scope.scheduleNew.Id);
                GradingClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.ParameterLists = [];
    $scope.getParameterPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.ItemNew.Id;
        $scope.ItemNew.Id = ItemId;
        try {
            $http.get('Machines/SkillManagement/getParameterData?ItemId=' + $scope.NewObject.Id)
                .then(
                    function successCallback(response) {
                        $scope.ParameterLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ParameterPoUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.getParameter = function (data) {
        try {
            $http.get('Machines/SkillManagement/getParameterData?ItemId=' + data)
                .then(
                    function successCallback(response) {
                        $scope.ParameterLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.GetParameterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/SkillManagement/LoadParameterEditData?ParameterId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ParameterNew = response.data.Parameter[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.SaveParameterData = function () {
            $http({
                method: 'POST',
                url: $scope.saveUrlParameter,
                data: {
                    'ParameterData': $scope.ParameterNew,
                    'Pid': $scope.ItemNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getParameter($scope.ItemNew.Id);
                    ParameterClearFields();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    };


    $scope.tab = 0;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.GetSkillLevelDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/SkillManagement/LoadSkillLevelEditData?LevelId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.SkillLevelNew = response.data.skilllevel[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.GetItemDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/SkillManagement/LoadItemEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ItemNew = response.data.item[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    
    $scope.GetBudgetCodeDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/SkillManagement/LoadBudgetCodeEditData?BudgetCodeId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.PersonBudgetNew = response.data.PersonBudget[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.GetTeamDefinitionDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/SkillManagement/LoadTeamDefinitionEditData?TeamDefinitionId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.TeamDefinitionNew = response.data.TeamDefinition[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetGradingDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/SkillManagement/LoadGradingEditData?GradeId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.GradeNew = response.data.Grade[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.Clear = function () {
        ScheduleClearFields();
    };
    $scope.SkillLevelClear = function () {
        SkillLevelClearFields($scope.GeneratSkillLevelSequenceNo($scope.scheduleNew.Id));
    };
    $scope.ItemClear = function () {
        ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));
    };
    $scope.SaveParameterClear = function () {
        ParameterClearFields();
    };
    $scope.BudgetCodeClear = function () {
        BudgetCodeClearFields($scope.GeneratPersonBudgetSequenceNo($scope.scheduleNew.Id));
    };

    $scope.TeamDefinitionClear = function () {
        TeamDefinitionClearFields($scope.GeneratTeamDefinitionSequenceNo($scope.scheduleNew.Id));
    };
    $scope.GradingClear = function () {
        GradingClearFields();
    };
    function ScheduleClearFields() {
        $scope.Action = "Save";
        $scope.scheduleNew = Object.assign({}, $scope.schedule);
        $scope.ScheduleMachineList = [];
    }

    function SkillLevelClearFields(seq) {
        $scope.Action = "Save";
        $scope.SkillLevelNew = Object.assign({}, $scope.SkillLevel);
        $scope.SkillLevelNew.SNO = seq;
    }

    function ItemClearFields(seq) {
        $scope.Action = "Save";
        $scope.ItemNew = Object.assign({}, $scope.Item);
        $scope.ItemNew.SNO = seq;
    }

    function BudgetCodeClearFields(seq) {
        $scope.Action = "Save";
        $scope.PersonBudgetNew = Object.assign({}, $scope.PersonBudget);
        $scope.PersonBudgetNew.SNO = seq;
    }

    function TeamDefinitionClearFields(seq) {
        $scope.Action = "Save";
        $scope.TeamDefinitionNew = Object.assign({}, $scope.TeamDefinition);
        $scope.TeamDefinitionNew.SNO = seq;
    }

    function ParameterClearFields() {
        $scope.Action = "Save";
        $scope.ParameterNew = Object.assign({}, $scope.Parameter);
    }
    function GradingClearFields() {
        $scope.Action = "Save";
        $scope.GradeNew = Object.assign({}, $scope.Grade);
    }

    $scope.removeLevelModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempLevelId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveLevel')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeRowModal = function (index,data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveItem')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    
    
    $scope.removeBudgetCodeModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempbudgetId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveBudgetCode')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeTeamDefinitionModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempTeamId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveTeamDefinition')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeGradingModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempGradeId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveGrading')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeItemRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/SkillManagement/ItemDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadItemDetails($scope.scheduleNew.Id);
                ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.removeLevelRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/SkillManagement/LevelDelete?id=' + $scope.tempLevelId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadSkillLevelDetails($scope.scheduleNew.Id);
                SkillLevelClearFields($scope.GeneratSkillLevelSequenceNo($scope.scheduleNew.Id));
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    
    $scope.removeBudgetCodeRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/SkillManagement/BudgetCodeDelete?id=' + $scope.tempbudgetId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadBudgetCodeDetails($scope.scheduleNew.Id);
                BudgetCodeClearFields($scope.GeneratPersonBudgetSequenceNo($scope.scheduleNew.Id));
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.removeTeamDefinitionRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/SkillManagement/TeamDefinitionDelete?id=' + $scope.tempTeamId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadTeamDefinitionDetails($scope.scheduleNew.Id);
                TeamDefinitionClearFields($scope.GeneratTeamDefinitionSequenceNo($scope.scheduleNew.Id));
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.removeGradeRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/SkillManagement/GradingDelete?id=' + $scope.tempGradeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadGradingDetails($scope.scheduleNew.Id);
                GradingClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Machines/SkillManagement/ScheduleDelete?id=' + $scope.scheduleNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadSkillManagementMasterList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
}