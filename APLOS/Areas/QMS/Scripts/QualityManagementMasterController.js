'use strict';
QualityManagementMasterController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function QualityManagementMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "QualityManagementMaster";
    $scope.CriticalLevelLists = [];
    $scope.CategoryLists = [];
    $scope.Action = 'Save';
    $scope.path = 'QMS/QualityManagementMaster/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlEntity = $scope.path + 'createEntity';
    $scope.saveUrlActivityGroup = $scope.path + 'createActivityGroup';
    $scope.saveUrlProcess = $scope.path + 'createProcess';
    //$scope.saveUrlItem = $scope.path + 'createItem';
    //$scope.saveUrlParameter = $scope.path + 'createParameter';
    //$scope.saveUrlBudgetCode = $scope.path + 'createBudgetCode';
    //$scope.saveUrlTeamDefinition = $scope.path + 'createTeamDefinition';
    //$scope.saveUrlGrading = $scope.path + 'createGrading';
    
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

    $scope.CategoryLists = [
        {
            'Value': 'Quality',
            'Text': 'Quality'
        },
        {
            'Value': 'Production',
            'Text': 'Production'
        },
        {
            'Value': 'Process',
            'Text': 'Process'
        },
        {
            'Value': 'Other',
            'Text': 'Other'
        }
    ];
    
    $scope.PerformanceGroupList = [];
    $scope.GetPerformanceGroupList = function (pid) {
        $http({
            method: 'GET',
            url: 'QMS/QualityManagementMaster/GetPerformanceGroupList?ScheduleId=' + pid
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
        , StandaredName: null
        , UserName: null
        , ScheduleDays: null
        , ResponsiblePersoneBgtCodeId: null
        , ResponsiblePersoneBgtCode: null
        , Remarks: null
    };
    $scope.scheduleNew = Object.assign({}, $scope.schedule);

    $scope.ActivityGroup = {
        Id: null
        , QMID: null
        , ActivityGroupName: null
        , Remarks: null
    };
    $scope.ActivityGroupNew = Object.assign({}, $scope.ActivityGroup);

    $scope.Item = {
        Id: null
        , SNO: null
        , ItemName: null
        , CriticalLevel: null
        , Category: null
        , IsAuditable: null
        , ByWhomId:null
        , ByWhom:null
        , Remarks: null
        , QMID: null
        , ActivityGroupId: null
        , ActivityGroup: null
        , ProcessId: null
        , ExceptionDays: null
        , ReportApplicable: true
        , IsStdApplicable: true
        , UOMId: null
        , UOM: null
        , Max: null
        , Min: null
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

    $scope.ProcessList = [];
    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: 'QMS/QualityManagementMaster/GetProcessList'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    $scope.GetProcessList();

    $scope.QualityManagementMasterList = [];
    $scope.LoadQualityManagementMasterList = function () {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadQualityManagementMasterList'
        }).then(function successCallback(response) {
            $scope.QualityManagementMasterList = response.data;
            var gridObj = $("#GridQualityManagementMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadQualityManagementMasterList();

    $scope.GetDetails = function (args) {
        $scope.ScheduleMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadQualityManagementEditData?ScheduleID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.scheduleNew = response.data.schedule[0];
            $scope.scheduleNew.ResponsiblePersoneBgtCode = response.data.schedule[0].ResponsiblePersoneBgtCode;
            $scope.LoadEntityDetails($scope.ScheduleMasterId);
            $scope.LoadQMActivityGroupDetails($scope.ScheduleMasterId);
            $scope.LoadProcessDetails($scope.ScheduleMasterId);
            //$scope.GeneratSkillLevelSequenceNo($scope.ScheduleMasterId);
            //$scope.LoadItemDetails($scope.ScheduleMasterId);
            //$scope.GeneratItemSequenceNo($scope.ScheduleMasterId);
            //$scope.LoadBudgetCodeDetails($scope.ScheduleMasterId);
            //$scope.GeneratPersonBudgetSequenceNo($scope.ScheduleMasterId);
            //$scope.LoadTeamDefinitionDetails($scope.ScheduleMasterId);
            //$scope.LoadGradingDetails($scope.ScheduleMasterId);
            /*$scope.GeneratTeamDefinitionSequenceNo($scope.ScheduleMasterId);*/

            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.QualityManagementMasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'ScheduleData': $scope.scheduleNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadQualityManagementMasterList();
                    ScheduleClearFields();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityManagementMaster/ScheduleDelete?id=' + $scope.scheduleNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadQualityManagementMasterList();
                ScheduleClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.QualityManagementMasterEntityList = [];
    $scope.LoadEntityDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadEntityDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.QualityManagementMasterEntityList = response.data;
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
            for (var i = 0; i < $scope.QualityManagementMasterEntityList.length; i++) {
                $scope.QualityManagementMasterEntityList[i].Flag = ChkOrUnchk;
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
            for (var i = 0; i < $scope.QualityManagementMasterEntityList.length; i++) {
                if ($scope.QualityManagementMasterEntityList[i].Flag == true) {
                    $scope.QualityManagementMasterEntityList[i].QMID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.QualityManagementMasterEntityList[i]);
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
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };



    $scope.QualityManagementActivityGroupList = [];
    $scope.LoadQMActivityGroupDetails = function () {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadQMActivityGroupDetails?ScheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.QualityManagementActivityGroupList = response.data;
        }
        )
    }

    $scope.LoadQMActivityGroupDetails();


    $scope.ActivityGroupSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.QualityManagementAGForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlActivityGroup,
                data: {
                    'ActivityGroupData': $scope.ActivityGroupNew,
                    'Pid': $scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadQMActivityGroupDetails($scope.scheduleNew.Id);
                    ActivityGroupClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.GetQMActivityGroupDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadActivityGroupEditData?AGId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ActivityGroupNew = response.data.activitygroup[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.removeAGModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempAGId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveAG')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeLevelRow = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityManagementMaster/ActivityGroupDelete?id=' + $scope.tempAGId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadQMActivityGroupDetails($scope.scheduleNew.Id);
                ActivityGroupClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.QualityManagementProcessList = [];
    $scope.LoadProcessDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadProcessDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.QualityManagementProcessList = response.data;
        }
        )
    }

    $scope.refreshTemplateProcess = function (args) {
        $("#Pheadchk").ejCheckBox({ "change": CheckBoxSelectAllProcess });
    };
    function CheckBoxSelectAllProcess(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridProcess").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.QualityManagementProcessList.length; i++) {
                $scope.QualityManagementProcessList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridProcess").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ActivityGroupList = [];
    $scope.GetActivityGroupList = function () {
        $http({
            method: 'GET',
            url: 'QMS/QualityManagementMaster/GetActivityGroupList'
        }).then(function successCallback(response) {
            $scope.ActivityGroupList = response.data;
        });
    }
    $scope.GetActivityGroupList();

    $scope.ProcessSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.QualityManagementProcessList.length; i++) {
                if ($scope.QualityManagementProcessList[i].Flag == true) {
                    $scope.QualityManagementProcessList[i].QMID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.QualityManagementProcessList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlProcess,
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
                    $scope.LoadProcessDetails($scope.scheduleNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };


    $scope.GeneratItemSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'QMS/QualityManagementMaster/GetItemAutoSequence?scheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ItemNew.SNO = response.data;
        });
    }
    $scope.GeneratItemSequenceNo();

   
    $scope.ScheduleItemList = [];
    $scope.LoadItemDetails = function () {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadItemDetails?ScheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleItemList = response.data;
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
            $http.get('QMS/QualityManagementMaster/getParameterData?ItemId=' + $scope.NewObject.Id)
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
            $http.get('QMS/QualityManagementMaster/getParameterData?ItemId=' + data)
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
            url: 'QMS/QualityManagementMaster/LoadParameterEditData?ParameterId=' + args.data.Id
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
            url: 'QMS/QualityManagementMaster/LoadSkillLevelEditData?LevelId=' + args.data.Id
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
            url: 'QMS/QualityManagementMaster/LoadItemEditData?ItemId=' + args.data.Id
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
            url: 'QMS/QualityManagementMaster/LoadBudgetCodeEditData?BudgetCodeId=' + args.data.Id
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
            url: 'QMS/QualityManagementMaster/LoadTeamDefinitionEditData?TeamDefinitionId=' + args.data.Id
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
            url: 'QMS/QualityManagementMaster/LoadGradingEditData?GradeId=' + args.data.Id
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

    $scope.ActivityGroupClear = function () {
        ActivityGroupClearFields();
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
    }
    function ActivityGroupClearFields() {
        $scope.Action = "Save";
        $scope.ActivityGroupNew = Object.assign({}, $scope.ActivityGroup);
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
            url: 'QMS/QualityManagementMaster/ItemDelete?id=' + $scope.tempId,
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

    
    
    $scope.removeBudgetCodeRow = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityManagementMaster/BudgetCodeDelete?id=' + $scope.tempbudgetId,
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
            url: 'QMS/QualityManagementMaster/TeamDefinitionDelete?id=' + $scope.tempTeamId,
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
            url: 'QMS/QualityManagementMaster/GradingDelete?id=' + $scope.tempGradeId,
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
    
}