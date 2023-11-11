'use strict';
SQCMasterController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function SQCMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "SQCMaster";
    $scope.CriticalLevelLists = [];
    $scope.CategoryLists = [];
    $scope.ModelList = [];
    $scope.ReasonCategoryLists = [];
    $scope.TypeLists = [];
    $scope.DefectTypeLists = [];
    $scope.Action = 'Save';
    $scope.path = 'QMS/SQCMaster/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.getSeqUrl = $scope.path + 'GetSQParameterMasterSequence';
    $scope.saveUrlParaMaster = $scope.path + 'createSQParaMaster';
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
    $scope.saveUrlDefectMaster = $scope.path + 'createDefectMaster';
    $scope.saveUrlDefectCategory = $scope.path + 'createDefectCategory';
    $scope.saveUrlAQLMaster = $scope.path + 'createAQLMaster';
    $scope.saveUrlDefect = $scope.path + 'createDefect';

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

    $scope.DefectTypeLists = [
        {
            'Value': 'A',
            'Text': 'A'
        },
        {
            'Value': 'B',
            'Text': 'B'
        },
        {
            'Value': 'C',
            'Text': 'C'
        }
    ];
    
    $scope.ParameterProcessList = [];
    $scope.GetParameterProcessList = function (pid) {
        $http({
            method: 'GET',
            url: 'QMS/SQCMaster/GetParameterProcessList?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.ParameterProcessList = response.data;
        });
    }
    $scope.GetParameterProcessList();

    $scope.SQFrequencyList = [];
    $scope.getSQFrequency = function () {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getSQFrequency'
        }).then(function successCallback(response) {
            $scope.SQFrequencyList = response.data;
        }
        )
    }
    $scope.getSQFrequency();

    $scope.SQReasonMasterList = [];
    $scope.getSQReasonMaster = function () {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getSQReasonMaster'
        }).then(function successCallback(response) {
            $scope.SQReasonMasterList = response.data;
        }
        )
    }
    $scope.getSQReasonMaster();

    $scope.DefectMasterList = [];
    $scope.getDefectMaster = function () {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getDefectMaster'
        }).then(function successCallback(response) {
            $scope.DefectMasterList = response.data;
        }
        )
    }
    $scope.getDefectMaster();

    $scope.AQLMasterList = [];
    $scope.getAQLMaster = function () {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getAQLMaster'
        }).then(function successCallback(response) {
            $scope.AQLMasterList = response.data;
        }
        )
    }
    $scope.getAQLMaster();

    $scope.GetSQFrequencyDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getSQFrequencyData?FrequencyId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.FrequencyNew = response.data.frequency[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetSQReasonMasterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getSQReasonMasterData?ReasonId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ReasonMasterNew = response.data.ReasonMaster[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetDefectMasterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getDefectMasterData?DefectId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.DefectMasterNew = response.data.DefectMaster[0];
            $scope.DefectMasterNew.Process = response.data.DefectMaster[0].DefectProcess;
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetAQLMasterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getAQLMasterData?AQLId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.AQLMasterNew = response.data.AQLMaster[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetParameterProcessAGList = function (pid) {
        $http({
            method: 'GET',
            url: 'QMS/SQCMaster/GetParameterProcessAGList?ScheduleId=' + $scope.ScheduleMasterId + '&ProcessId=' + pid
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
        $http.get('Productions/ParameterMaster/GetSQParametertList')
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

    $scope.removeDefectMaster = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempDMId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveDefectMaster')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeAQLMaster = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempAMId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveAQLMaster')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteFrequency = function () {
        $http({
            method: 'POST',
            url: 'QMS/SQCMaster/FrequencyDelete?id=' + $scope.tempFId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getSQFrequency();
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
            url: 'QMS/SQCMaster/ReasonMasterDelete?id=' + $scope.tempRMId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getSQReasonMaster();
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
            url: 'QMS/SQCMaster/AuthorizedPersonDelete?id=' + $scope.tempAPId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getSQAuthorizedPerson();
                AuthorizedPersonClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.DeleteDefectMaster = function () {
        $http({
            method: 'POST',
            url: 'QMS/SQCMaster/DefectMasterDelete?id=' + $scope.tempDMId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDefectMaster();
                DefectMasterClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.DeleteAQLMaster = function () {
        $http({
            method: 'POST',
            url: 'QMS/SQCMaster/AQLMasterDelete?id=' + $scope.tempAMId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getAQLMaster();
                AQLMasterClearFields();
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
        , IsWorkCenter: true
        , CustomerParameter: false
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

    $scope.DefectMaster = {
        Id: null
        , SNO: null
        , ProcessId: null
        , Process: null
        , Defect: null
        , DefectType: null
        , DefectCode: null
        , Minor: null
        , Major: null
        , Critical: null 
        , Remarks: null
    }
    $scope.DefectMasterNew = Object.assign({}, $scope.DefectMaster);

    $scope.AQLMaster = {
        Id: null
        , SNO: null
        , LotSize: null
        , SampleSize: null
        , AQLLevel: null
        , Value: null
        , Minor: null
        , Major: null
        , Critical: null
        , Remarks: null
        , IsApplicable: true
    }
    $scope.AQLMasterNew = Object.assign({}, $scope.AQLMaster);

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

    $scope.SQReasonNameLists = [];
    $scope.GetReasonNameLists = function () {
        $http({
            method: 'GET',
            url: 'QMS/SQCMaster/GetSQReasonNameLists'
        }).then(function successCallback(response) {
            $scope.SQReasonNameLists = response.data;
        });
    }
    $scope.GetReasonNameLists();

    $scope.ProcessList = [];
    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: 'QMS/SQCMaster/GetProcessList'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    $scope.GetProcessList();

    $scope.SQCMasterList = [];
    $scope.LoadSQCMasterList = function () {
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQCMasterList'
        }).then(function successCallback(response) {
            $scope.SQCMasterList = response.data;
            var gridObj = $("#GridSQCMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadSQCMasterList();

    $scope.GetDetails = function (args) {
        $scope.ScheduleMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQCMasterEditData?ScheduleID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.scheduleNew = response.data.schedule[0];
            $scope.scheduleNew.ResponsiblePersoneBgtCode = response.data.schedule[0].ResponsiblePersoneBgtCode;
            $scope.LoadEntityDetails($scope.ScheduleMasterId);
            $scope.LoadSQActivityGroupDetails($scope.ScheduleMasterId);
            $scope.LoadProcessDetails($scope.ScheduleMasterId);
            $scope.GetParameterProcessList($scope.ScheduleMasterId);
            $scope.LoadItemDetails($scope.ScheduleMasterId);
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
        if ($scope.SQCMasterForm.$valid) {
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
                    $scope.LoadSQCMasterList();
                    $scope.LoadEntityDetails($scope.scheduleNew.Id);
                    $scope.LoadSQActivityGroupDetails($scope.scheduleNew.Id);
                    $scope.LoadProcessDetails($scope.scheduleNew.Id);
                    $scope.GetParameterProcessList($scope.scheduleNew.Id);
                    $scope.LoadItemDetails($scope.scheduleNew.Id);
                    $scope.LoadMachineDetails($scope.scheduleNew.Id);
                    $scope.LoadProductDetails($scope.scheduleNew.Id);
                    $scope.LoadWorkCenterDetails($scope.scheduleNew.Id);
                    $scope.LoadPositionCodeDetails($scope.scheduleNew.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'QMS/SQCMaster/ScheduleDelete?id=' + $scope.scheduleNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadSQCMasterList();
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
            url: 'QMS/SQCMaster/CheckPointDelete?id=' + $scope.ParameterNew.Id,
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
            url: 'QMS/SQCMaster/ReasonDelete?id=' + $scope.ReasonNew.Id,
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

    $scope.SQCMasterEntityList = [];
    $scope.LoadEntityDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadEntityDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.SQCMasterEntityList = response.data;
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
            for (var i = 0; i < $scope.SQCMasterEntityList.length; i++) {
                $scope.SQCMasterEntityList[i].Flag = ChkOrUnchk;
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
            for (var i = 0; i < $scope.SQCMasterEntityList.length; i++) {
                if ($scope.SQCMasterEntityList[i].Flag == true) {
                    $scope.SQCMasterEntityList[i].SQCID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.SQCMasterEntityList[i]);
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

    $scope.SQMasterPositionCodeList = [];
    $scope.LoadPositionCodeDetails = function (pid) {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQPositionCodeDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.SQMasterPositionCodeList = response.data;
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

        var filtered = $("#GridSQPositionCode").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SQMasterPositionCodeList.length; i++) {
                $scope.SQMasterPositionCodeList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSQPositionCode").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.PositionCodeSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQMasterPositionCodeList.length; i++) {
                if ($scope.SQMasterPositionCodeList[i].Flag == true) {
                    $scope.SQMasterPositionCodeList[i].SQCID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.SQMasterPositionCodeList[i]);
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



    $scope.SQCActivityGroupList = [];
    $scope.LoadSQActivityGroupDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQActivityGroupDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.SQCActivityGroupList = response.data;
        }
        )
    }

    $scope.ActivityGroupSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SQCAGForm.$valid) {
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
                    $scope.LoadSQActivityGroupDetails($scope.scheduleNew.Id);
                    ActivityGroupClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.GetSQActivityGroupDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/LoadActivityGroupEditData?AGId=' + args.data.Id
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
            url: 'QMS/SQCMaster/ActivityGroupDelete?id=' + $scope.tempAGId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadSQActivityGroupDetails($scope.scheduleNew.Id);
                ActivityGroupClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.SQProcessList = [];
    $scope.LoadProcessDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadProcessDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.SQProcessList = response.data;
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
            for (var i = 0; i < $scope.SQProcessList.length; i++) {
                $scope.SQProcessList[i].Flag = ChkOrUnchk;
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
            url: 'QMS/SQCMaster/GetActivityGroupList'
        }).then(function successCallback(response) {
            $scope.ActivityGroupList = response.data;
        });
    }
    $scope.GetActivityGroupList();

    $scope.ProcessSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQProcessList.length; i++) {
                if ($scope.SQProcessList[i].Flag == true) {
                    $scope.SQProcessList[i].SQCID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.SQProcessList[i]);
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

    $scope.SQMasterMachineList = [];
    $scope.LoadMachineDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQMachineDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.SQMasterMachineList = response.data;
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

        var filtered = $("#GridSQMachine").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SQMasterMachineList.length; i++) {
                $scope.SQMasterMachineList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSQMachine").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.MachineSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQMasterMachineList.length; i++) {
                if ($scope.SQMasterMachineList[i].Flag == true) {
                    $scope.SQMasterMachineList[i].SQCID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.SQMasterMachineList[i]);
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

    $scope.SQMasterProductList = [];
    $scope.LoadProductDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQProductDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.SQMasterProductList = response.data;
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

        var filtered = $("#GridSQProduct").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SQMasterProductList.length; i++) {
                $scope.SQMasterProductList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSQProduct").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ProductSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQMasterProductList.length; i++) {
                if ($scope.SQMasterProductList[i].Flag == true) {
                    $scope.SQMasterProductList[i].SQCID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.SQMasterProductList[i]);
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

    $scope.SQMasterWorkCenterList = [];
    $scope.LoadWorkCenterDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQWorkCenterDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.SQMasterWorkCenterList = response.data;
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

        var filtered = $("#GridSQWorkCenter").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SQMasterWorkCenterList.length; i++) {
                $scope.SQMasterWorkCenterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSQWorkCenter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.WorkCenterSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQMasterWorkCenterList.length; i++) {
                if ($scope.SQMasterWorkCenterList[i].Flag == true) {
                    $scope.SQMasterWorkCenterList[i].SQCID = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.SQMasterWorkCenterList[i]);
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

    $scope.ScheduleItemList = [];
    $scope.LoadItemDetails = function () {
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadItemDetails?ScheduleId=' + $scope.scheduleNew.Id
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

    $scope.selectProcess = function () {
        $scope.getProcess();
        angular.element(document.querySelector('#ProcessPopUp')).modal('show');
    }

    $scope.ProcessList = [];
    $scope.getProcess = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProcess',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;
        });
    }

    $scope.doubleProcess = function (e) {
        $scope.DefectMasterNew.ProcessId = e.data.Id;
        $scope.DefectMasterNew.Process = e.data.Process;
        angular.element(document.querySelector('#ProcessPopUp')).modal('hide');
    }

    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#ProcessPopUp')).modal('hide');
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
        if ($scope.SQParameterItemForm.$valid) {
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

    
    $scope.SQParameterLists = [];
    $scope.getParameterPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.ItemNew.Id;
        $scope.ItemNew.Id = ItemId;
        try {
            $http.get('QMS/SQCMaster/getSQParameterData?ParameterId=' + $scope.NewObject.Id)
                .then(
                    function successCallback(response) {
                        $scope.SQParameterLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridSQParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#SQParameterPoUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.SQReasonLists = [];
    $scope.ParameterId = null;
    $scope.getReasonPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.NewObject.Id;
        $scope.ParameterId = ItemId;
        try {
            $http.get('QMS/SQCMaster/getSQReasonData?ParameterId=' + $scope.NewObject.Id)
                .then(
                    function successCallback(response) {
                        $scope.SQReasonLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridSQReason").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#SQReasonPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.getParameter = function (data) {
        try {
            $http.get('QMS/SQCMaster/getSQParameterData?ParameterId=' + data)
                .then(
                    function successCallback(response) {
                        $scope.SQParameterLists = response.data;
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
            $http.get('QMS/SQCMaster/getSQReasonData?ParameterId=' + data)
                .then(
                    function successCallback(response) {
                        $scope.SQReasonLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridSQReason").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetSQParameterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQParameterEditData?ParameterId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ParameterNew = response.data.Parameter[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetSQReasonDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQReasonEditData?ReasonId=' + args.data.Id
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
                $scope.getSQFrequency();
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
                $scope.getSQReasonMaster();
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
    $scope.SQParameterFrequencyList = [];
    $scope.getFrequencyPopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ParameterId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQFrequencyList?ParameterId=' + $scope.ParameterId
        }).then(function successCallback(response) {
            $scope.SQParameterFrequencyList = response.data;
            var gridObj = $("#GridSQFrequencyValuePopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#SQFrequencyValuePopup')).modal('show');
        }
        )
    }

    $scope.closeFrequencyValuePopup = function () {
        angular.element(document.querySelector('#SQFrequencyValuePopup')).modal('hide');
    }

    $scope.ParameterId = null;
    $scope.SQParameterDefectList = [];
    $scope.getDefectPopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ParameterId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQDefectList?ParameterId=' + $scope.ParameterId + '&ProcessId=' + $scope.NewObject.ProcessId
        }).then(function successCallback(response) {
            $scope.SQParameterDefectList = response.data;
            var gridObj = $("#GridSQDefectPopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#SQDefectPopup')).modal('show');
        }
        )
    }

    $scope.closeDefectPopup = function () {
        angular.element(document.querySelector('#SQDefectPopup')).modal('hide');
    }

    $scope.DefectId = null;
    $scope.DefectCategoryList = [];
    $scope.getDefectCategoryPopup = function (data) {
        $scope.NewObject = data.data;
        $scope.DefectId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'QMS/SQCMaster/LoadDefectCategoryList?DefectId=' + $scope.DefectId
        }).then(function successCallback(response) {
            $scope.DefectCategoryList = response.data;
            var gridObj = $("#GridDefectCategoryPopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#DefectCategoryPopup')).modal('show');
        }
        )
    }

    $scope.closeDefectCategoryPopup = function () {
        angular.element(document.querySelector('#DefectCategoryPopup')).modal('hide');
    }

    $scope.refreshTemplateDefect = function (args) {
        $("#Dheadchk").ejCheckBox({ "change": CheckBoxSelectAllDefect });
    };
    function CheckBoxSelectAllDefect(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSQDefectPopup").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SQParameterDefectList.length; i++) {
                $scope.SQParameterDefectList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSQDefectPopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SaveFrequencyValue = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQParameterFrequencyList.length; i++) {
                if ($scope.SQParameterFrequencyList[i].QA == true || $scope.SQParameterFrequencyList[i].Quality == true || $scope.SQParameterFrequencyList[i].Management == true) {
                    $scope.SQParameterFrequencyList[i].ParameterId = $scope.ParameterId;
                    $scope.SaveList.push($scope.SQParameterFrequencyList[i]);
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

    $scope.SaveDefect = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQParameterDefectList.length; i++) {
                if ($scope.SQParameterDefectList[i].Flag == true) {
                    $scope.SQParameterDefectList[i].ParameterId = $scope.ParameterId;
                    $scope.SaveList.push($scope.SQParameterDefectList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlDefect,
                data: {
                    'ParameterDefectData': $scope.SaveList,
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

    $scope.DefectMasterSave = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlDefectMaster,
            data: {
                'DefectMasterData': $scope.DefectMasterNew
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDefectMaster();
                DefectMasterClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.SaveDefectCategory = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.DefectCategoryList.length; i++) {
                if ($scope.DefectCategoryList[i].Man == true || $scope.DefectCategoryList[i].Machine == true || $scope.DefectCategoryList[i].Material == true || $scope.DefectCategoryList[i].Process == true || $scope.DefectCategoryList[i].Other == true) {
                    $scope.SaveList.push($scope.DefectCategoryList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlDefectCategory,
                data: {
                    'DefectCategoryData': $scope.SaveList
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

    $scope.AQLMasterSave = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlAQLMaster,
            data: {
                'AQLMasterData': $scope.AQLMasterNew
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getAQLMaster();
                AQLMasterClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
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
            url: 'QMS/SQCMaster/LoadItemEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ItemNew = response.data.item[0];
            $scope.GetParameterProcessList($scope.ItemNew.SQCID);
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
    $scope.DefectMasterClear = function () {
       DefectMasterClearFields();
    };
    $scope.AQLMasterClear = function () {
        AQLMasterClearFields();
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

    function DefectMasterClearFields() {
        $scope.Action = "Save";
        $scope.DefectMasterNew = Object.assign({}, $scope.DefectMaster);
    }

    function AQLMasterClearFields() {
        $scope.Action = "Save";
        $scope.AQLMasterNew = Object.assign({}, $scope.AQLMaster);
    }

    function ReasonClearFields() {
        $scope.Action = "Save";
        $scope.ReasonNew = Object.assign({}, $scope.Reason);
    }
  
    $scope.removeItemRow = function () {
        $http({
            method: 'POST',
            url: 'QMS/SQCMaster/ItemDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadItemDetails($scope.scheduleNew.Id);
                ItemClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.SQParameterResponsiblePersonList = [];
    $scope.LoadSQParameterResponsiblePersonList  = function () {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQParameterResponsiblePersonDetails'
        }).then(function successCallback(response) {
            $scope.SQParameterResponsiblePersonList = response.data;
        }
        )
    }
    $scope.LoadSQParameterResponsiblePersonList();

    $scope.refreshTemplateParameterResponsiblePerson = function (args) {
        $("#PRCheadchk").ejCheckBox({ "change": CheckBoxSelectAllParameterResponsiblePerson });
    };
    function CheckBoxSelectAllParameterResponsiblePerson(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSQParameterResponsiblePerson").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SQParameterResponsiblePersonList.length; i++) {
                $scope.SQParameterResponsiblePersonList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSQParameterResponsiblePerson").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ParameterResponsiblePersonSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQParameterResponsiblePersonList.length; i++) {
                if ($scope.SQParameterResponsiblePersonList[i].Flag == true) {
                    $scope.SaveList.push($scope.SQParameterResponsiblePersonList[i]);
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
                    $scope.LoadSQParameterResponsiblePersonList();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SQParameterApprovalResponsiblePersonList = [];
    $scope.LoadSQParameterApprovalResponsiblePersonList = function () {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQParameterApprovalResponsiblePersonDetails'
        }).then(function successCallback(response) {
            $scope.SQParameterApprovalResponsiblePersonList = response.data;
        }
        )
    }
    $scope.LoadSQParameterApprovalResponsiblePersonList();

    $scope.refreshTemplateParameterApprovalResponsiblePerson = function (args) {
        $("#ARCheadchk").ejCheckBox({ "change": CheckBoxSelectAllParameterApprovalResponsiblePerson });
    };
    function CheckBoxSelectAllParameterApprovalResponsiblePerson(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSQParameterApprovalResponsiblePerson").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SQParameterApprovalResponsiblePersonList.length; i++) {
                $scope.SQParameterApprovalResponsiblePersonList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSQParameterApprovalResponsiblePerson").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ParameterApprovalResponsiblePersonSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQParameterApprovalResponsiblePersonList.length; i++) {
                if ($scope.SQParameterApprovalResponsiblePersonList[i].Flag == true) {
                    $scope.SaveList.push($scope.SQParameterApprovalResponsiblePersonList[i]);
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
                    $scope.LoadSQParameterApprovalResponsiblePersonList();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SQQualityActionResponsiblePersonList = [];
    $scope.LoadSQQualityActionResponsiblePersonList = function () {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/LoadSQQualityActionResponsiblePersonDetails'
        }).then(function successCallback(response) {
            $scope.SQQualityActionResponsiblePersonList = response.data;
        }
        )
    }
    $scope.LoadSQQualityActionResponsiblePersonList();

    $scope.refreshTemplateQualityActionResponsiblePerson = function (args) {
        $("#QACheadchk").ejCheckBox({ "change": CheckBoxSelectAllQualityActionResponsiblePerson });
    };
    function CheckBoxSelectAllQualityActionResponsiblePerson(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSQQualityActionResponsiblePerson").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SQQualityActionResponsiblePersonList.length; i++) {
                $scope.SQQualityActionResponsiblePersonList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSQQualityActionResponsiblePerson").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.QualityActionResponsiblePersonSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.SQQualityActionResponsiblePersonList.length; i++) {
                if ($scope.SQQualityActionResponsiblePersonList[i].Flag == true) {
                    $scope.SaveList.push($scope.SQQualityActionResponsiblePersonList[i]);
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
                    $scope.LoadSQQualityActionResponsiblePersonList();
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
                $scope.getSQAuthorizedPerson();
                AuthorizedPersonClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.SQAuthorizedPersonList = [];
    $scope.getSQAuthorizedPerson = function () {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getSQAuthorizedPerson'
        }).then(function successCallback(response) {
            $scope.SQAuthorizedPersonList = response.data;
        }
        )
    }
    $scope.getSQAuthorizedPerson();

    $scope.GetSQAuthorizedPersonDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/SQCMaster/getSQAuthorizedPersonData?AuthorizedId=' + args.data.Id
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
            url: $scope.path + 'GetAuthorizedResPerson',
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

}