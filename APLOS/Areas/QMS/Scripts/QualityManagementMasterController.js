'use strict';
QualityManagementMasterController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function QualityManagementMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "QualityManagementMaster";
    $scope.CriticalLevelLists = [];
    $scope.CategoryLists = [];
    $scope.ModelList = [];
    $scope.ReasonCategoryLists = [];
    $scope.TypeLists = [];
    $scope.Action = 'Save';
    $scope.path = 'QMS/QualityManagementMaster/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrlParaMaster = $scope.path + 'createParaMaster';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveUrlEntity = $scope.path + 'createEntity';
    $scope.saveUrlActivityGroup = $scope.path + 'createActivityGroup';
    $scope.saveUrlProcess = $scope.path + 'createProcess';
    $scope.saveUrlItem = $scope.path + 'createItem';
    $scope.saveUrlParameter = $scope.path + 'createParameter';
    $scope.saveUrlFrequency = $scope.path + 'createFrequency';
    $scope.saveUrlFrequencyValue = $scope.path + 'createFrequencyValue';
    $scope.saveUrlMachine = $scope.path + 'createMachine';
    $scope.saveUrlProduct = $scope.path + 'createProduct';
    $scope.saveUrlWorkCenter = $scope.path + 'createWorkCenter';
    $scope.saveUrlPositionCode = $scope.path + 'createPositionCode';
    $scope.saveUrlReason = $scope.path + 'createReason';
    $scope.saveUrlReasonMaster = $scope.path + 'createReasonMaster';
    $scope.saveUrlParameterResponsiblePerson = $scope.path + 'createParameterResponsiblePerson';
    $scope.saveUrlParameterApprovalResponsiblePerson = $scope.path + 'createParameterApprovalResponsiblePerson';
    $scope.saveUrlQualityActionResponsiblePerson = $scope.path + 'createQualityActionResponsiblePerson';
    $scope.saveUrlAuthorizedPerson = $scope.path + 'createAuthorizedPerson';
    $scope.saveUrlCPS = $scope.path + 'createCPSequence';

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
    
    $scope.ParameterProcessList = [];
    $scope.GetParameterProcessList = function (pid) {
        $http({
            method: 'GET',
            url: 'QMS/QualityManagementMaster/GetParameterProcessList?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.ParameterProcessList = response.data;
        });
    }
    $scope.GetParameterProcessList();

    $scope.FrequencyList = [];
    $scope.getFrequency = function () {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/getFrequency'
        }).then(function successCallback(response) {
            $scope.FrequencyList = response.data;
        }
        )
    }
    $scope.getFrequency();

    $scope.ReasonMasterList = [];
    $scope.getReasonMaster = function () {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/getReasonMaster'
        }).then(function successCallback(response) {
            $scope.ReasonMasterList = response.data;
        }
        )
    }
    $scope.getReasonMaster();

    $scope.GetFrequencyDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/getFrequencyData?FrequencyId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.FrequencyNew = response.data.frequency[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetReasonMasterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/getReasonMasterData?ReasonId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ReasonMasterNew = response.data.ReasonMaster[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetParameterProcessAGList = function (pid) {
        $http({
            method: 'GET',
            url: 'QMS/QualityManagementMaster/GetParameterProcessAGList?ScheduleId=' + $scope.ScheduleMasterId + '&ProcessId=' + pid
        }).then(function successCallback(response) {
            $scope.ItemNew.ActivityGroup = response.data[0].ActivityGroupName;
        });
    }

    $scope.schedule = {
        Id: null
        , ScheduleCode: null
        , StandaredName: null
        , UserName: null
        , ScheduleDays: null
        , ResponsiblePersoneBgtCodeId: null
        , ResponsiblePersoneBgtCode: null
        , Remarks: null
        , StandardTime: null
        , MaximumTime: null
    };
    $scope.scheduleNew = Object.assign({}, $scope.schedule);

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

    $scope.OpeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('show');
        $scope.GetResponsiblePerson();
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');

    }

    $scope.ResponsiblePersonList = [];
    $scope.GetResponsiblePerson = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetResponsiblePerson',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsiblePersonList = resp.data;
        });

    }

    $scope.doubleEmploye = function (e) {
        $scope.ModelNew.EmpSystemId = e.data.EmpSystemId;
        $scope.ModelNew.EmployeeName = e.data.EmployeeName;
        $scope.ModelNew.EmployeeCode = e.data.EmployeeCode;

        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

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
        $scope.ModelNew.PositionCodeId = e.data.Id;
        $scope.ModelNew.PositionCode = e.data.Code;
        angular.element(document.querySelector('#PositionCodePop')).modal('hide');
    }

    $scope.closePositionCodePopUp = function () {
        angular.element(document.querySelector('#PositionCodePop')).modal('hide');
    }

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.getData = function () {
        $http.get('Productions/ParameterMaster/GetList')
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
        $scope.EmployeeId = args.data.ResponsiblePerson;
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

    $scope.removeFrequency = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempFId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveFrequency')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

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

    $scope.removeAuthorizedPerson = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempAPId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveAuthorizedPerson')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteFrequency = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityManagementMaster/FrequencyDelete?id=' + $scope.tempFId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getFrequency();
                FrequencyClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.DeleteReasonMaster = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityManagementMaster/ReasonMasterDelete?id=' + $scope.tempRMId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getReasonMaster();
                ReasonMasterClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.DeleteAuthorizedPerson = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityManagementMaster/AuthorizedPersonDelete?id=' + $scope.tempAPId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getAuthorizedPerson();
                AuthorizedPersonClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
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
        , ParameterId: null
        , ParameterName: null
        , CriticalLevel: null
        , Category: null
        , IsAuditable: null
        , ByWhomId:null
        , ByWhom:null
        , Remarks: null
        , QMID: null
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
        , CustomerParameter: false
        , FinalReport: false
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

    $scope.Frequency = {
        Id: null
        , SNO: null
        , UserName: null
        , Remarks: null
    }
    $scope.FrequencyNew = Object.assign({}, $scope.Frequency);

    $scope.ReasonMaster = {
        Id: null
        , SNO: null
        , UserName: null
        , ReasonCategory: null
        , Type: null
        , Remarks: null
    }
    $scope.ReasonMasterNew = Object.assign({}, $scope.ReasonMaster);

    $scope.Reason = {
        Id: null
        , SNO: null
        , ReasonId: null
        , Remarks: null
        , ParameterId: null
        , IsActive: true
    }
    $scope.ReasonNew = Object.assign({}, $scope.Reason);

    $scope.AuthorizedPerson = {
        Id: null
        , SNO: null
        , PositionName: null
        , AuthorizedResPerson: null
        , AuthorizedResPersonId: null
    }
    $scope.AuthorizedPersonNew = Object.assign({}, $scope.AuthorizedPerson);

    $scope.ReasonNameLists = [];
    $scope.GetReasonNameLists = function () {
        $http({
            method: 'GET',
            url: 'QMS/QualityManagementMaster/GetReasonNameLists'
        }).then(function successCallback(response) {
            $scope.ReasonNameLists = response.data;
        });
    }
    $scope.GetReasonNameLists();

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
            $scope.GetParameterProcessList($scope.ScheduleMasterId);
            $scope.LoadItemDetails($scope.ScheduleMasterId);
            //$scope.GeneratItemSequenceNo($scope.ScheduleMasterId);
            $scope.LoadMachineDetails($scope.ScheduleMasterId);
            $scope.LoadProductDetails($scope.ScheduleMasterId);
            $scope.LoadWorkCenterDetails($scope.ScheduleMasterId);
            $scope.LoadPositionCodeDetails($scope.ScheduleMasterId);
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
                    $scope.scheduleNew.Id = response.data.Id;
                    $scope.LoadQualityManagementMasterList();
                    $scope.LoadEntityDetails($scope.scheduleNew.Id);
                    $scope.LoadQMActivityGroupDetails($scope.scheduleNew.Id);
                    $scope.LoadProcessDetails($scope.scheduleNew.Id);
                    $scope.GetParameterProcessList($scope.scheduleNew.Id);
                    $scope.LoadItemDetails($scope.scheduleNew.Id);
                    //$scope.GeneratItemSequenceNo($scope.scheduleNew.Id);
                    $scope.LoadMachineDetails($scope.scheduleNew.Id);
                    $scope.LoadProductDetails($scope.scheduleNew.Id);
                    $scope.LoadWorkCenterDetails($scope.scheduleNew.Id);
                    $scope.LoadPositionCodeDetails($scope.scheduleNew.Id);
                    //ScheduleClearFields();

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

    $scope.CheckPointDelete = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityManagementMaster/CheckPointDelete?id=' + $scope.ParameterNew.Id,
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
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.ReasonDelete = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityManagementMaster/ReasonDelete?id=' + $scope.ReasonNew.Id,
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
                    "Pid": $scope.scheduleNew.Id
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

    $scope.QualityManagementMasterPositionCodeList = [];
    $scope.LoadPositionCodeDetails = function (pid) {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadPositionCodeDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.QualityManagementMasterPositionCodeList = response.data;
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
            for (var i = 0; i < $scope.QualityManagementMasterPositionCodeList.length; i++) {
                $scope.QualityManagementMasterPositionCodeList[i].Flag = ChkOrUnchk;
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
            for (var i = 0; i < $scope.QualityManagementMasterPositionCodeList.length; i++) {
                if ($scope.QualityManagementMasterPositionCodeList[i].Flag == true) {
                    $scope.QualityManagementMasterPositionCodeList[i].QMID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.QualityManagementMasterPositionCodeList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlPositionCode,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.scheduleNew.Id
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



    $scope.QualityManagementActivityGroupList = [];
    $scope.LoadQMActivityGroupDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadQMActivityGroupDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.QualityManagementActivityGroupList = response.data;
        }
        )
    }

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
                    "Pid": $scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadProcessDetails($scope.scheduleNew.Id);
                    $scope.GetParameterProcessList($scope.scheduleNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.QualityManagementMasterMachineList = [];
    $scope.LoadMachineDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadMachineDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.QualityManagementMasterMachineList = response.data;
        }
        )
    }

    $scope.refreshTemplateMachine = function (args) {
        $("#Mheadchk").ejCheckBox({ "change": CheckBoxSelectAllMachine });
    };
    function CheckBoxSelectAllMachine(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridMachine").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.QualityManagementMasterMachineList.length; i++) {
                $scope.QualityManagementMasterMachineList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridMachine").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.MachineSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.QualityManagementMasterMachineList.length; i++) {
                if ($scope.QualityManagementMasterMachineList[i].Flag == true) {
                    $scope.QualityManagementMasterMachineList[i].QMID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.QualityManagementMasterMachineList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlMachine,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadMachineDetails($scope.scheduleNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.QualityManagementMasterProductList = [];
    $scope.LoadProductDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadProductDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.QualityManagementMasterProductList = response.data;
        }
        )
    }

    $scope.refreshTemplateProduct = function (args) {
        $("#Pdheadchk").ejCheckBox({ "change": CheckBoxSelectAllProduct });
    };
    function CheckBoxSelectAllProduct(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridProduct").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.QualityManagementMasterProductList.length; i++) {
                $scope.QualityManagementMasterProductList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridProduct").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ProductSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.QualityManagementMasterProductList.length; i++) {
                if ($scope.QualityManagementMasterProductList[i].Flag == true) {
                    $scope.QualityManagementMasterProductList[i].QMID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.QualityManagementMasterProductList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlProduct,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadProductDetails($scope.scheduleNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.QualityManagementMasterWorkCenterList = [];
    $scope.LoadWorkCenterDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadWorkCenterDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.QualityManagementMasterWorkCenterList = response.data;
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

        var filtered = $("#GridWorkCenter").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.QualityManagementMasterWorkCenterList.length; i++) {
                $scope.QualityManagementMasterWorkCenterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridWorkCenter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.WorkCenterSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.QualityManagementMasterWorkCenterList.length; i++) {
                if ($scope.QualityManagementMasterWorkCenterList[i].Flag == true) {
                    $scope.QualityManagementMasterWorkCenterList[i].QMID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.QualityManagementMasterWorkCenterList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlWorkCenter,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadWorkCenterDetails($scope.scheduleNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //$scope.ParameterItemList = [];
    //$scope.GetParameterItemList = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'QMS/QualityManagementMaster/GetParameterItemList'
    //    }).then(function successCallback(response) {
    //        $scope.ParameterItemList = response.data;
    //    });
    //}
    //$scope.GetParameterItemList();

    //$scope.GeneratItemSequenceNo = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'QMS/QualityManagementMaster/GetItemAutoSequence?scheduleId=' + $scope.scheduleNew.Id
    //    }).then(function successCallback(response) {
    //        $scope.ItemNew.SNO = response.data;
    //    });
    //}
    //$scope.GeneratItemSequenceNo();

   
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
        $scope.ItemNew.UOMId = e.data.UOMId;
        $scope.ItemNew.UOM = e.data.UOM;
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
    }

    $scope.closeUOMPopUp = function () {
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
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
        $scope.ItemNew.ParameterId = e.data.Id;
        $scope.ItemNew.ParameterName = e.data.UserName;
        angular.element(document.querySelector('#ParameterPopUp')).modal('hide');
    }

    $scope.closeParameterPopUp = function () {
        angular.element(document.querySelector('#ParameterPopUp')).modal('hide');
    }

   
    $scope.ItemSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.QualityManagementParameterItemForm.$valid) {
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
                    //ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));
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

    
    $scope.ParameterLists = [];
    $scope.getParameterPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.ItemNew.Id;
        $scope.ItemNew.Id = ItemId;
        try {
            $http.get('QMS/QualityManagementMaster/getParameterData?ParameterId=' + $scope.NewObject.Id)
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

    $scope.ReasonLists = [];
    $scope.ParameterId = null;
    $scope.getReasonPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.NewObject.Id;
        $scope.ParameterId = ItemId;
        try {
            $http.get('QMS/QualityManagementMaster/getReasonData?ParameterId=' + $scope.NewObject.Id)
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

    $scope.getParameter = function (data) {
        try {
            $http.get('QMS/QualityManagementMaster/getParameterData?ParameterId=' + data)
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

    $scope.getReason = function (data) {
        try {
            $http.get('QMS/QualityManagementMaster/getReasonData?ParameterId=' + data)
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

    $scope.GetReasonDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadReasonEditData?ReasonId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ReasonNew = response.data.Reason[0];
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

    $scope.FrequencySave = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlFrequency,
            data: {
                'FrequencyData': $scope.FrequencyNew
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getFrequency();
                FrequencyClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

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
                $scope.getReasonMaster();
                ReasonMasterClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

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



    $scope.ParameterId = null;
    $scope.ParameterFrequencyList = [];
    $scope.getFrequencyPopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ParameterId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadFrequencyList?ParameterId=' + $scope.ParameterId
        }).then(function successCallback(response) {
            $scope.ParameterFrequencyList = response.data;
            var gridObj = $("#GridFrequencyValuePopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#FrequencyValuePopup')).modal('show');
        }
        )
    }

    $scope.closeFrequencyValuePopup = function () {
        angular.element(document.querySelector('#FrequencyValuePopup')).modal('hide');
    }

    $scope.SaveFrequencyValue = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.ParameterFrequencyList.length; i++) {
                if ($scope.ParameterFrequencyList[i].QA == true || $scope.ParameterFrequencyList[i].Quality == true || $scope.ParameterFrequencyList[i].Management == true) {
                    $scope.ParameterFrequencyList[i].ParameterId = $scope.ParameterId;
                    $scope.SaveList.push($scope.ParameterFrequencyList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlFrequencyValue,
                data: {
                    'ParameterFrequencyData': $scope.SaveList,
                    'ParameterId': $scope.ParameterId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

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

    $scope.GetItemDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadItemEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ItemNew = response.data.item[0];
            $scope.GetParameterProcessList($scope.ItemNew.QMID);
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

    $scope.ItemClear = function () {
        //ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));
        ItemClearFields();
    };
    $scope.SaveParameterClear = function () {
        ParameterClearFields();
    };
    $scope.ReasonClear = function () {
        ReasonClearFields();
    };
    $scope.FrequencyClear = function () {
        FrequencyClearFields();
    };
    $scope.ReasonMasterClear = function () {
        ReasonMasterClearFields();
    };
    $scope.AuthorizedPersonClear = function () {
        AuthorizedPersonClearFields();
    };
   
    function ScheduleClearFields() {
        $scope.Action = "Save";
        $scope.scheduleNew = Object.assign({}, $scope.schedule);
    }
    function ActivityGroupClearFields() {
        $scope.Action = "Save";
        $scope.ActivityGroupNew = Object.assign({}, $scope.ActivityGroup);
    }

    function ItemClearFields() {
        $scope.Action = "Save";
        $scope.ItemNew = Object.assign({}, $scope.Item);
        //$scope.ItemNew.SNO = seq;
    }

   
    function ParameterClearFields() {
        $scope.Action = "Save";
        $scope.ParameterNew = Object.assign({}, $scope.Parameter);
    }

    function FrequencyClearFields() {
        $scope.Action = "Save";
        $scope.FrequencyNew = Object.assign({}, $scope.Frequency);
    }

    function ReasonMasterClearFields() {
        $scope.Action = "Save";
        $scope.ReasonMasterNew = Object.assign({}, $scope.ReasonMaster);
    }

    function AuthorizedPersonClearFields() {
        $scope.Action = "Save";
        $scope.AuthorizedPersonNew = Object.assign({}, $scope.AuthorizedPerson);
    }

    function ReasonClearFields() {
        $scope.Action = "Save";
        $scope.ReasonNew = Object.assign({}, $scope.Reason);
    }
  
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
                //ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));
                ItemClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.ParameterResponsiblePersonList = [];
    $scope.LoadParameterResponsiblePersonList  = function () {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadParameterResponsiblePersonDetails'
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
            url: 'QMS/QualityManagementMaster/LoadParameterApprovalResponsiblePersonDetails'
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

    $scope.QualityActionResponsiblePersonList = [];
    $scope.LoadQualityActionResponsiblePersonList = function () {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadQualityActionResponsiblePersonDetails'
        }).then(function successCallback(response) {
            $scope.QualityActionResponsiblePersonList = response.data;
        }
        )
    }
    $scope.LoadQualityActionResponsiblePersonList();

    $scope.refreshTemplateQualityActionResponsiblePerson = function (args) {
        $("#QACheadchk").ejCheckBox({ "change": CheckBoxSelectAllQualityActionResponsiblePerson });
    };
    function CheckBoxSelectAllQualityActionResponsiblePerson(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridQualityActionResponsiblePerson").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.QualityActionResponsiblePersonList.length; i++) {
                $scope.QualityActionResponsiblePersonList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridQualityActionResponsiblePerson").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.QualityActionResponsiblePersonSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.QualityActionResponsiblePersonList.length; i++) {
                if ($scope.QualityActionResponsiblePersonList[i].Flag == true) {
                    $scope.SaveList.push($scope.QualityActionResponsiblePersonList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlQualityActionResponsiblePerson,
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
                    $scope.LoadQualityActionResponsiblePersonList();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.AuthorizedPersonSave = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlAuthorizedPerson,
            data: {
                'AuthorizedPersonData': $scope.AuthorizedPersonNew
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getAuthorizedPerson();
                AuthorizedPersonClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.AuthorizedPersonList = [];
    $scope.getAuthorizedPerson = function () {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/getAuthorizedPerson'
        }).then(function successCallback(response) {
            $scope.AuthorizedPersonList = response.data;
        }
        )
    }
    $scope.getAuthorizedPerson();

    $scope.GetAuthorizedPersonDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/getAuthorizedPersonData?AuthorizedId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.AuthorizedPersonNew = response.data.AuthorizedPerson[0];
        }
        )
    }

    $scope.selectAuthorizedPerson = function () {
        $scope.getAuthorizedResPerson();
        angular.element(document.querySelector('#AuthorizedPersonPopUp')).modal('show');
    }

    $scope.AuthorizedResPersonList = [];
    $scope.getAuthorizedResPerson = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetAuthorizedPerson',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.AuthorizedResPersonList = resp.data;
        });
    }

    $scope.doubleAuthorizedPerson = function (e) {
        $scope.AuthorizedPersonNew.AuthorizedResPersonId = e.data.SystemId;
        $scope.AuthorizedPersonNew.AuthorizedResPerson = e.data.EmployeeName;
        angular.element(document.querySelector('#AuthorizedPersonPopUp')).modal('hide');
    }

    $scope.closeAuthorizedPersonPopUp = function () {
        angular.element(document.querySelector('#AuthorizedPersonPopUp')).modal('hide');
    }

    $scope.QualityManagementMasterCPSList = [];
    $scope.LoadCPSDetails = function () {
        $http({
            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadCPSDetails'
        }).then(function successCallback(response) {
            $scope.QualityManagementMasterCPSList = response.data;
        }
        )
    }
    $scope.LoadCPSDetails();

    $scope.CPSequenceSave = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.QualityManagementMasterCPSList.length; i++) {
                if ($scope.QualityManagementMasterCPSList[i].Sequence !== null) {
                    $scope.SaveList.push($scope.QualityManagementMasterCPSList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlCPS,
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
                    $scope.LoadCPSDetails();
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