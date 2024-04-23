'use strict';
LeaveTransectionController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function LeaveTransectionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "LeaveTransection";
    $scope.CriticalLevelLists = [];
    $scope.CategoryLists = [];
    $scope.ModelList = [];
    $scope.ReasonCategoryLists = [];
   // $scope.DepartmentList = [];
    $scope.TypeLists = [];
    $scope.Action = 'Save';
    $scope.path = 'HumanResource/LeaveTransection/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.getSeqUrl = $scope.path + 'GetParameterMasterSequence';
    $scope.saveUrlParaMaster = $scope.path + 'createParaMaster';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveUrlReasonMaster = $scope.path + 'createParaReasonMaster';
    $scope.saveUrlEntity = $scope.path + 'createProcessParameterEntity';
    $scope.saveUrlActivityGroup = $scope.path + 'createProcessParaActivityGroup';
    $scope.saveUrlProcess = $scope.path + 'createProcessParameterProcess';
    $scope.saveUrlItem = $scope.path + 'createProcessParameter';
    $scope.saveUrlReason = $scope.path + 'createProcessParameterReason';
    $scope.saveUrlCheckPoints = $scope.path + 'createCheckPoints';
    $scope.saveUrlWorkCenter = $scope.path + 'createWorkCenter';
    $scope.saveUrlPositionCode = $scope.path + 'createPositionCode';
    $scope.saveUrlApprovalPerson = $scope.path + 'createApprovalPerson';
    $scope.saveUrlParameterResponsiblePerson = $scope.path + 'createProcessParameterResponsible';
    $scope.saveUrlParameterApprovalResponsiblePerson = $scope.path + 'createProcessParameterApproval';

    $scope.tabPRM = 1;
    $scope.setTabPRM = function (newTab) {
        $scope.tabPRM = newTab;
    };

    $scope.isSetPRM = function (tabNum) {
        return $scope.tabPRM === tabNum;
    };

    $scope.tab = 0;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    //ProcessParaMaster Start
    $scope.ProcessParaMaster = {
        Id: null
        , Code: null
        , StandardName: null
        , UserName: null
        , Group: null
        , SubGroup: null
        , Remarks: null
    };
    $scope.ProcessParaMasterNew = Object.assign({}, $scope.ProcessParaMaster);

    $scope.ProcessParaMasterList = [];
    $scope.LoadProcessParaMasterList = function () {
        $http({

            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParaMasterList'
        }).then(function successCallback(response) {
            $scope.ProcessParaMasterList = response.data;
            var gridObj = $("#GridProcessParaMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadProcessParaMasterList();

    $scope.GetDetails = function (args) {
        $scope.ProcessMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParameterEditData?ProcessParameterID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ProcessParaMasterNew = response.data.processparameter[0];
            $scope.LoadEntityDetails($scope.ProcessMasterId);
            $scope.LoadProcessParaActivityGroupDetails($scope.ProcessMasterId);
            $scope.LoadProcessParaProcessDetails($scope.ProcessMasterId);
            $scope.LoadPositionCodeDetails($scope.ProcessMasterId);
            $scope.LoadAprovalPersonDetails($scope.ProcessMasterId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ProcessParameterMasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'ProcessParaMasterData': $scope.ProcessParaMasterNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ProcessParaMasterNew.Id = response.data.Id;
                    $scope.LoadProcessParaMasterList();
                    $scope.LoadEntityDetails($scope.ProcessMasterId);
                    $scope.LoadProcessParaActivityGroupDetails($scope.ProcessMasterId);
                    $scope.LoadProcessParaProcessDetails($scope.ProcessMasterId);
                    $scope.LoadPositionCodeDetails($scope.ProcessMasterId);
                    $scope.LoadAprovalPersonDetails($scope.ProcessMasterId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'QMS/ProcessParameterMaster/ProcessParaMasterDelete?id=' + $scope.ProcessParaMasterNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadProcessParaMasterList();
                ProcessParaMasterClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Clear = function () {
        ProcessParaMasterClearFields();
    };

    function ProcessParaMasterClearFields() {
        $scope.Action = "Save";
        $scope.ProcessParaMasterNew = Object.assign({}, $scope.ProcessParaMaster);
    }

    //ProcessParaMaster End

    //ParameterMaster Start

    cboService.getCboEntityByPlant(null, null, " ", function (result) {
        $scope.entityList = result;
    });

    $scope.GetDepartmentList = function () {
        $http.get('HumanResource/LeaveTransection/LoadProcessParaMasterList')
            .then(function (response) {
                $scope.DepartmentList = response.data;
            });
    }
    $scope.GetDepartmentList();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        UserName: null,
        StandardName: null,
        ShortName: null,
        IsActive: true,
        Remarks: null,
        PositionCodeId: null,
        PositionCode: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.selectPositionCode = function () {
        $scope.getPositionCode();
        angular.element(document.querySelector('#PositionCodePop')).modal('show');
    }

    $scope.PositionCodeList = [];
    $scope.getPositionCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPositionCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PositionCodeList = resp.data;
        });
    }

    $scope.doublePositionCode = function (e) {
        $scope.ModelNew.BudgetCodeId = e.data.Id;
        $scope.ModelNew.BudgetCode = e.data.BudgetCode;
        angular.element(document.querySelector('#PositionCodePop')).modal('hide');
    }

    $scope.closePositionCodePopUp = function () {
        angular.element(document.querySelector('#PositionCodePop')).modal('hide');
    }


    $scope.getData = function () {
        $http.get('Productions/ParameterMaster/GetProcessParameterList')
            .then(
                function successCallback(response) {
                    $scope.ModelList = response.data;
                    ClearFields(response.data.Sequence);
                    $scope.GetSequence();
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    $scope.getData();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };

    $scope.SaveParaMaster = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveUrlParaMaster,
            data: {
                'data': $scope.ModelNew,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                ClearFields(response.data.Sequence);
                $scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.ClearParameter = function () {
        ClearFields();
        $scope.GetSequence();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelTemp = {
            Id: null,
            Sequence: 0,
            Code: null,
            UserName: null,
            StandardName: null,
            ShortName: null,
            IsActive: true,
            Remarks: null,
            EmployeeName: null,
            EmployeeCode: null,
            EmpSystemId: null,
            PositionCodeId: null,
            PositionCode: null
        };

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.removeParameter = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempPId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveParameter')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteParameter = function () {
        if (!baseService.isUndefinedOrNull($scope.tempPId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.tempPId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

   //ParameterMaster End

   //ReasonMaster Start

   $scope.ReasonMaster = {
        Id: null
        , SNO: null
        , UserName: null
        , ReasonCategory: null
        , Type: null
        , Remarks: null
    }
    $scope.ReasonMasterNew = Object.assign({}, $scope.ReasonMaster);

    $scope.ReasonCategoryLists = [
        {
            'Value': 'Man',
            'Text': 'Man'
        },
        {
            'Value': 'Machine',
            'Text': 'Machine'
        },
        {
            'Value': 'Material',
            'Text': 'Material'
        },
        {
            'Value': 'Method',
            'Text': 'Method'
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

    $scope.TypeLists = [
        {
            'Value': 'HighlyCritical',
            'Text': 'Highly Critical'
        },
        {
            'Value': 'Critical',
            'Text': 'Critical'
        },
        {
            'Value': 'SemiCritical',
            'Text': 'Semi Critical'
        },
        {
            'Value': 'Important',
            'Text': 'Important'
        },
        {
            'Value': 'Normal',
            'Text': 'Normal'
        }
    ];

    $scope.ReasonMasterSave = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlReasonMaster,
            data: {
                'ReasonMasterData': $scope.ReasonMasterNew
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getProcessParameterReasonMaster();
                ReasonMasterClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.ReasonMasterList = [];
    $scope.getProcessParameterReasonMaster = function () {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/getProcessParameterReasonMaster'
        }).then(function successCallback(response) {
            $scope.ReasonMasterList = response.data;
        }
        )
    }
    $scope.getProcessParameterReasonMaster();

    $scope.GetProcessParaReasonMasterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/getProcessParaReasonMasterData?ReasonId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ReasonMasterNew = response.data.ProcessParaReasonMaster[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.removeReasonMaster = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempRMId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveReasonMaster')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteProcessParaReasonMaster = function () {
        $http({
            method: 'POST',
            url: 'QMS/ProcessParameterMaster/ProcessParaReasonMasterDelete?id=' + $scope.tempRMId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getProcessParameterReasonMaster();
                ReasonMasterClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.ReasonMasterClear = function () {
        ReasonMasterClearFields();
    };

    function ReasonMasterClearFields() {
        $scope.Action = "Save";
        $scope.ReasonMasterNew = Object.assign({}, $scope.ReasonMaster);
    }

    //ReasonMaster End

    //ProcessParameter Entity Start

    $scope.ProcessParameterEntityList = [];
    $scope.LoadEntityDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParameterEntityDetails?MasterId=' + pid
        }).then(function successCallback(response) {
            $scope.ProcessParameterEntityList = response.data;
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

        var filtered = $("#GridProcessParameterEntity").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProcessParameterEntityList.length; i++) {
                $scope.ProcessParameterEntityList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridProcessParameterEntity").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.EntitySave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProcessParameterEntityList.length; i++) {
                if ($scope.ProcessParameterEntityList[i].Flag == true) {
                    $scope.ProcessParameterEntityList[i].PPID = $scope.ProcessParaMasterNew.Id;
                    $scope.SaveList.push($scope.ProcessParameterEntityList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlEntity,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.ProcessParaMasterNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadEntityDetails($scope.ProcessParaMasterNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //Process Parameter Entity End

    // Activity Group Start

    $scope.ActivityGroup = {
        Id: null
        , PPID: null
        , ActivityGroupName: null
        , Remarks: null
    };
    $scope.ActivityGroupNew = Object.assign({}, $scope.ActivityGroup);

    $scope.ProcessParaActivityGroupList = [];
    $scope.LoadProcessParaActivityGroupDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParaActivityGroupDetails?MasterId=' + pid
        }).then(function successCallback(response) {
            $scope.ProcessParaActivityGroupList = response.data;
        }
        )
    }

    $scope.ActivityGroupSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ProcessParameterAGForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlActivityGroup,
                data: {
                    'ActivityGroupData': $scope.ActivityGroupNew,
                    'Pid': $scope.ProcessParaMasterNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadProcessParaActivityGroupDetails($scope.ProcessParaMasterNew.Id);
                    ActivityGroupClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.ActivityGroupClear = function () {
        ActivityGroupClearFields();
    };

    function ActivityGroupClearFields() {
        $scope.Action = "Save";
        $scope.ActivityGroupNew = Object.assign({}, $scope.ActivityGroup);
    }

    $scope.GetProcessParaActivityGroupDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParaActivityGroupEditData?AGId=' + args.data.Id
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
            url: 'QMS/ProcessParameterMaster/ProcessParaActivityGroupDelete?id=' + $scope.tempAGId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadProcessParaActivityGroupDetails($scope.ProcessParaMasterNew.Id);
                ActivityGroupClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    //Activity Group End

    //Process Start

    $scope.ProcessParameterProcessList = [];
    $scope.LoadProcessParaProcessDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParaProcessDetails?MasterId=' + pid
        }).then(function successCallback(response) {
            $scope.ProcessParameterProcessList = response.data;
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

        var filtered = $("#GridParameterProcess").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProcessParameterProcessList.length; i++) {
                $scope.ProcessParameterProcessList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridParameterProcess").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ActivityGroupList = [];
    $scope.GetActivityGroupList = function () {
        $http({
            method: 'GET',
            url: 'QMS/ProcessParameterMaster/GetPPActivityGroupList'
        }).then(function successCallback(response) {
            $scope.ActivityGroupList = response.data;
        });
    }
    $scope.GetActivityGroupList();

    $scope.ProcessSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProcessParameterProcessList.length; i++) {
                if ($scope.ProcessParameterProcessList[i].Flag == true) {
                    $scope.ProcessParameterProcessList[i].PPID = $scope.ProcessParaMasterNew.Id;
                    $scope.SaveList.push($scope.ProcessParameterProcessList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlProcess,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.ProcessParaMasterNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadProcessParaProcessDetails($scope.ProcessParaMasterNew.Id);
                    //$scope.GetParameterProcessList($scope.ProcessParaMasterNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //Process End

    //Process Parameter Start
   
    $scope.ProcessParameter = {
        Id: null
        , SNO: null
        , ParameterId: null
        , ParameterName: null
        , CriticalLevel: null
        , Category: null
        , IsAuditable: null
        , ByWhomId: null
        , ByWhom: null
        , Remarks: null
        , ProcessParameterId: null
        , ActivityGroup: null
        , ProcessId: null
        , ExceptionDays: null
        , ReportApplicable: true
        , IsStdApplicable: true
        , IsActive: true
        , OrderSpecific: false
        , General: true
        , UOMId: null
        , UOM: null
        , Max: null
        , Min: null
        , IsWorkCenter: true
    };
    $scope.ProcessParameterNew = Object.assign({}, $scope.ProcessParameter);

    $scope.ProcessParameterLists = [];
    $scope.ProcessParameterId = null;
    $scope.getProcessParameterPopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ProcessParameterId = $scope.NewObject.Id;
        $scope.ProcessParameterNew.ProcessId = $scope.NewObject.ProcessId;
        $scope.ProcessParameterNew.Process = $scope.NewObject.Process;
        $scope.ProcessParameterNew.ActivityGroup = $scope.NewObject.ActivityGroupName;
        try {
            $http.get('QMS/ProcessParameterMaster/getProcessParameterData?ProcessParameterId=' + $scope.NewObject.Id)
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

    //$scope.ProcessParameterItemList = [];
    $scope.LoadProcessParameterItemDetails = function () {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParameterItemDetails?ProcessParameterId=' + $scope.ProcessParameterId
        }).then(function successCallback(response) {
            $scope.ProcessParameterLists = response.data;
        }
        )
    }

    $scope.GetItemDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParameterItemEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ProcessParameterNew = response.data.ProcessParameter[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.selectParameter = function () {
        $scope.getParameterName();
        angular.element(document.querySelector('#ParameterPopUp')).modal('show');
    }

    $scope.ParameterList = [];
    $scope.getParameterName = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetParameterItemList',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ParameterList = resp.data;
        });
    }

    $scope.doubleParameter = function (e) {
        $scope.ProcessParameterNew.ParameterId = e.data.Id;
        $scope.ProcessParameterNew.ParameterName = e.data.UserName;
        angular.element(document.querySelector('#ParameterPopUp')).modal('hide');
    }

    $scope.closeParameterPopUp = function () {
        angular.element(document.querySelector('#ParameterPopUp')).modal('hide');
    }

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
        $scope.ProcessParameterNew.ByWhomId = e.data.ManPowerBudgetId;
        $scope.ProcessParameterNew.ByWhom = e.data.Code;
        angular.element(document.querySelector('#ByWhomPop')).modal('hide');
    }

    $scope.closeByWhomPopUp = function () {
        angular.element(document.querySelector('#ByWhomPop')).modal('hide');
    }

    $scope.selectUOM = function () {
        $scope.getUOM();
        angular.element(document.querySelector('#UOMPopUp')).modal('show');
    }

    $scope.UOMList = [];
    $scope.getUOM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetUOM',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.UOMList = resp.data;
        });
    }

    $scope.doubleUOM = function (e) {
        $scope.ProcessParameterNew.UOMId = e.data.UOMId;
        $scope.ProcessParameterNew.UOM = e.data.UOM;
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
    }

    $scope.closeUOMPopUp = function () {
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
    }

    $scope.ProcessParameterSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ProcessParameterItemForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlItem,
                data: {
                    'ItemData': $scope.ProcessParameterNew,
                    'Pid': $scope.ProcessParameterId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadProcessParameterItemDetails($scope.ProcessParameterId);
                    ItemClearFields();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.removeRowModal = function (index, data) {
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

    $scope.removeItemRow = function () {
        $http({
            method: 'POST',
            url: 'QMS/ProcessParameterMaster/ProcessParameterItemDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadProcessParameterItemDetails($scope.ProcessParameterNew.ProcessParameterId);
                ItemClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.ItemClear = function () {
        ItemClearFields();
    };

    function ItemClearFields() {
        $scope.Action = "Save";
        $scope.ProcessParameter = {
            Id: null
            , SNO: null
            , ParameterId: null
            , ParameterName: null
            , CriticalLevel: null
            , Category: null
            , IsAuditable: null
            , ByWhomId: null
            , ByWhom: null
            , Remarks: null
            , ProcessParameterId: null
            , ActivityGroup: $scope.ProcessParameterNew.ActivityGroup
            , ProcessId: $scope.ProcessParameterNew.ProcessId
            , Process: $scope.ProcessParameterNew.Process
            , ExceptionDays: null
            , ReportApplicable: true
            , IsStdApplicable: true
            , IsActive: true
            , OrderSpecific: false
            , General: true
            , UOMId: null
            , UOM: null
            , Max: null
            , Min: null
            , IsWorkCenter: true
        };
        $scope.ProcessParameterNew = Object.assign({}, $scope.ProcessParameter);
    }

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

    //Process Parameter End

    //Process Parameter Reason Start

    $scope.ReasonLists = [];
    $scope.ParameterId = null;
    $scope.getReasonPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.NewObject.Id;
        $scope.ParameterId = ItemId;
        try {
            $http.get('QMS/ProcessParameterMaster/getProcessParameterReasonData?ParameterId=' + $scope.NewObject.Id)
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

    $scope.Reason = {
        Id: null
        , SNO: null
        , ReasonId: null
        , Remarks: null
        , ParameterId: null
        , IsActive: true
    }
    $scope.ReasonNew = Object.assign({}, $scope.Reason);

    $scope.ReasonNameLists = [];
    $scope.GetReasonNameLists = function () {
        $http({
            method: 'GET',
            url: 'QMS/ProcessParameterMaster/GetReasonNameLists'
        }).then(function successCallback(response) {
            $scope.ReasonNameLists = response.data;
        });
    }
    $scope.GetReasonNameLists();

    $scope.getReason = function (data) {
        try {
            $http.get('QMS/ProcessParameterMaster/getProcessParameterReasonData?ParameterId=' + data)
                .then(
                    function successCallback(response) {
                        $scope.ReasonLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridReason").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetReasonDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParameterReasonEditData?ReasonId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ReasonNew = response.data.Reason[0];
        }
        )
    }

    $scope.SaveReasonData = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlReason,
            data: {
                'ReasonData': $scope.ReasonNew,
                'Pid': $scope.ParameterId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getReason($scope.ParameterId);
                ReasonClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.ReasonClear = function () {
        ReasonClearFields();
    };

    function ReasonClearFields() {
        $scope.Action = "Save";
        $scope.ReasonNew = Object.assign({}, $scope.Reason);
    }

    $scope.ReasonDelete = function () {
        $http({
            method: 'POST',
            url: 'QMS/ProcessParameterMaster/ParameterReasonDelete?id=' + $scope.ReasonNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getReason($scope.ParameterId);
                ReasonClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    //Process Parameter Reason End

    //Check Points Start

    $scope.Parameter = {
        Id: null
        , SNO: null
        , CheckPoints: null
        , Remarks: null
        , ItemId: null
    }
    $scope.ParameterNew = Object.assign({}, $scope.Parameter);

    $scope.ParameterCheckPointsLists = [];
    $scope.getParameterCheckPointsPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.NewObject.Id;
        $scope.ParameterId = ItemId;
        try {
            $http.get('QMS/ProcessParameterMaster/getParameterCheckPointsData?ParameterId=' + $scope.NewObject.Id)
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

    $scope.getParameterCheckPoints = function (data) {
        try {
            $http.get('QMS/ProcessParameterMaster/getParameterCheckPointsData?ParameterId=' + data)
                .then(
                    function successCallback(response) {
                        $scope.ParameterCheckPointsLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameterCheckPoints").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetParameterCheckPointsDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadParameterCheckPointsEditData?ParameterId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ParameterNew = response.data.Parameter[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.CheckPointsSave = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlCheckPoints,
            data: {
                'ParameterData': $scope.ParameterNew,
                'Pid': $scope.ParameterId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getParameterCheckPoints($scope.ParameterId);
                CheckPointsClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.CheckPointsClear = function () {
        CheckPointsClearFields();
    };

    function CheckPointsClearFields() {
        $scope.Action = "Save";
        $scope.ParameterNew = Object.assign({}, $scope.Parameter);
    }

    $scope.CheckPointDelete = function () {
        $http({
            method: 'POST',
            url: 'QMS/ProcessParameterMaster/CheckPointDelete?id=' + $scope.ParameterNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getParameterCheckPoints($scope.ParameterId);
                CheckPointsClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    //Check Points End

    //Parameter Work Center Start

    $scope.ProcessParameterWorkCenterList = [];
    $scope.getWorkCenterPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.NewObject.Id;
        $scope.ParameterId = ItemId;
        try {
            $http.get('QMS/ProcessParameterMaster/LoadParameterWorkCenterDetails?ParameterId=' + $scope.NewObject.Id)
                .then(
                    function successCallback(response) {
                        $scope.ProcessParameterWorkCenterList = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameterWorkCenter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ParameterWorkCenterPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.LoadParameterWorkCenterDetails = function () {
        $http({

            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadParameterWorkCenterDetails?ParameterId=' + $scope.ParameterId
        }).then(function successCallback(response) {
            $scope.ProcessParameterWorkCenterList = response.data;
        }
        )
    }

    $scope.refreshTemplateWorkCenter = function (args) {
        $("#Wheadchk").ejCheckBox({ "change": CheckBoxSelectAllWorkCenter });
    };
    function CheckBoxSelectAllWorkCenter(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridParameterWorkCenter").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProcessParameterWorkCenterList.length; i++) {
                $scope.ProcessParameterWorkCenterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridParameterWorkCenter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.WorkCenterSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProcessParameterWorkCenterList.length; i++) {
                if ($scope.ProcessParameterWorkCenterList[i].Flag == true) {
                    $scope.ProcessParameterWorkCenterList[i].ParameterId = $scope.ParameterId;
                    $scope.SaveList.push($scope.ProcessParameterWorkCenterList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlWorkCenter,
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
                    $scope.LoadParameterWorkCenterDetails($scope.ParameterId);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //Parameter Work Center End

    //Parameter Position Code Start

    $scope.ProcessParameterPositionCodeList = [];
    $scope.LoadPositionCodeDetails = function (pid) {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadParameterPositionCodeDetails?MasterId=' + pid
        }).then(function successCallback(response) {
            $scope.ProcessParameterPositionCodeList = response.data;
        }
        )
    }

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
            for (var i = 0; i < $scope.ProcessParameterPositionCodeList.length; i++) {
                $scope.ProcessParameterPositionCodeList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPositionCode").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.PositionCodeSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProcessParameterPositionCodeList.length; i++) {
                if ($scope.ProcessParameterPositionCodeList[i].Flag == true) {
                    $scope.ProcessParameterPositionCodeList[i].PPID = $scope.ProcessParaMasterNew.Id;
                    $scope.SaveList.push($scope.ProcessParameterPositionCodeList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlPositionCode,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.ProcessParaMasterNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadPositionCodeDetails($scope.ProcessParaMasterNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //Parameter Position Code End

    //Approval Person Start

    $scope.ProcessParameterAprovalPersonList = [];
    $scope.LoadAprovalPersonDetails = function (pid) {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadParameterAprovalPersonDetails?MasterId=' + pid
        }).then(function successCallback(response) {
            $scope.ProcessParameterAprovalPersonList = response.data;
        }
        )
    }

    $scope.refreshTemplateAprovalPerson = function (args) {
        $("#APCheadchk").ejCheckBox({ "change": CheckBoxSelectAllAprovalPerson });
    };
    function CheckBoxSelectAllAprovalPerson(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridApprovalPerson").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProcessParameterAprovalPersonList.length; i++) {
                $scope.ProcessParameterAprovalPersonList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridApprovalPerson").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.AprovalPersonSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProcessParameterAprovalPersonList.length; i++) {
                if ($scope.ProcessParameterAprovalPersonList[i].Flag == true) {
                    $scope.ProcessParameterAprovalPersonList[i].PPID = $scope.ProcessParaMasterNew.Id;
                    $scope.SaveList.push($scope.ProcessParameterAprovalPersonList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlApprovalPerson,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.ProcessParaMasterNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadAprovalPersonDetails($scope.ProcessParaMasterNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //Approval Person End

    $scope.ParameterResponsiblePersonList = [];
    $scope.LoadParameterResponsiblePersonList = function () {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParameterResponsiblePersonDetails'
        }).then(function successCallback(response) {
            $scope.ParameterResponsiblePersonList = response.data;
        }
        )
    }
    $scope.LoadParameterResponsiblePersonList();

    $scope.refreshTemplateParameterResponsiblePerson = function (args) {
        $("#PRCheadchk").ejCheckBox({ "change": CheckBoxSelectAllParameterResponsiblePerson });
    };
    function CheckBoxSelectAllParameterResponsiblePerson(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridParameterResponsiblePerson").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ParameterResponsiblePersonList.length; i++) {
                $scope.ParameterResponsiblePersonList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridParameterResponsiblePerson").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ParameterResponsiblePersonSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.ParameterResponsiblePersonList.length; i++) {
                if ($scope.ParameterResponsiblePersonList[i].Flag == true) {
                    $scope.SaveList.push($scope.ParameterResponsiblePersonList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlParameterResponsiblePerson,
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
                    $scope.LoadParameterResponsiblePersonList();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.ParameterApprovalResponsiblePersonList = [];
    $scope.LoadParameterApprovalResponsiblePersonList = function () {
        $http({
            method: 'Get',
            url: 'QMS/ProcessParameterMaster/LoadProcessParameterApprovalResponsiblePersonDetails'
        }).then(function successCallback(response) {
            $scope.ParameterApprovalResponsiblePersonList = response.data;
        }
        )
    }
    $scope.LoadParameterApprovalResponsiblePersonList();

    $scope.refreshTemplateParameterApprovalResponsiblePerson = function (args) {
        $("#ARCheadchk").ejCheckBox({ "change": CheckBoxSelectAllParameterApprovalResponsiblePerson });
    };
    function CheckBoxSelectAllParameterApprovalResponsiblePerson(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridParameterApprovalResponsiblePerson").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ParameterApprovalResponsiblePersonList.length; i++) {
                $scope.ParameterApprovalResponsiblePersonList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridParameterApprovalResponsiblePerson").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ParameterApprovalResponsiblePersonSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.ParameterApprovalResponsiblePersonList.length; i++) {
                if ($scope.ParameterApprovalResponsiblePersonList[i].Flag == true) {
                    $scope.SaveList.push($scope.ParameterApprovalResponsiblePersonList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlParameterApprovalResponsiblePerson,
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
                    $scope.LoadParameterApprovalResponsiblePersonList();
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