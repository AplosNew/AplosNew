'use strict';
ProcessQualityControlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function ProcessQualityControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Process/Quality Issue Control";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.productionSummaryes = [];
    $scope.IssueTypeList = [];
    $scope.PeriodCategoryList = [];
    $scope.CriticalLevelLists = [];
    $scope.PeriodList = [];
    $scope.FrequencyList = [];
    $scope.DependentDateList = [];
    $scope.EntryLevelList = [];
    $scope.path = 'Productions/ProcessQualityControl/';
    $scope.saveUrlIssue = $scope.path + 'createIssue';
    $scope.saveUrlReason = $scope.path + 'createReason';
    $scope.saveUrlTime = $scope.path + 'createTime';
    $scope.saveUrlQICValue = $scope.path + 'create';
    $scope.saveUrlIssueItem = $scope.path + 'createIssueItem';
    $scope.saveUrlGrade = $scope.path + 'createGrade';
    $scope.saveUrlPOQuality = $scope.path + 'createPOQuality';
    $scope.saveUrlActionToBeTaken = $scope.path + 'createActionToBeTaken';
    $scope.CriticalLevelLists = [
        {
            'Value': 'High',
            'Text': 'High'
        },
        {
            'Value': 'Very High',
            'Text': 'Very High'
        },
        {
            'Value': 'Medium',
            'Text': 'Medium'
        },
        {
            'Value': 'Low',
            'Text': 'Low'
        }
    ];
        
    $scope.PeriodList = [
        {
            'Value': '2hours',
            'Text': '2hours'
        },
        {
            'Value': '4hours',
            'Text': '4hours'
        },
        {
            'Value': '8hours',
            'Text': '8hours'
        },
        {
            'Value': '24hours',
            'Text': '24hours'
        },
        {
            'Value': 'Weekly',
            'Text': 'Weekly'
        },
        {
            'Value': 'Monthly',
            'Text': 'Monthly'
        },
        {
            'Value': 'Quarterly',
            'Text': 'Quarterly'
        },
        {
            'Value': 'Annualy',
            'Text': 'Annualy'
        }
    ];

    $scope.FrequencyList = [
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
        },
        {
            'Value': '5',
            'Text': '5'
        },
        {
            'Value': '6',
            'Text': '6'
        },
        {
            'Value': '7',
            'Text': '7'
        },
        {
            'Value': '8',
            'Text': '8'
        }
        ,
        {
            'Value': '9',
            'Text': '9'
        }
        ,
        {
            'Value': '10',
            'Text': '10'
        }
    ];

    $scope.DependentDateList = [
        {
            'Value': 'ItemDate',
            'Text': 'Item Date'
        },
        {
            'Value': 'ExFactoryDate',
            'Text': 'ExFactory Date'
        },
        {
            'Value': 'PODate',
            'Text': 'PO Date'
        },
        {
            'Value': 'POStartDate',
            'Text': 'PO Start Date'
        },
        {
            'Value': 'POEndDate',
            'Text': 'PO End Date'
        }
    ];

    $scope.EntryLevelList = [
        {
            'Value': 'PO',
            'Text': 'PO'
        },
        {
            'Value': 'LOT',
            'Text': 'LOT'
        }
    ];

    $scope.IssueItem = {
          Id: null
        , SNO: null
        , IssueId: null
        , ItemName: null
        , UOMId: null
        , UOM: null
        , Max: null
        , Min: null
        , PositionCodeId: null
        , PositionCode: null
        , CriticalLevel: null
        , Remarks: null
        , ParameterId: null
        , Parameter: null
        , CheckingInterval: null
    };
    $scope.IssueItemNew = Object.assign({}, $scope.IssueItem);

    $scope.Grade = {
        Id: null
        , SNO: null
        , GradeName: null
        , ShortName: null
        , GradeValue: null
        , Remarks: null
    };
    $scope.GradeNew = Object.assign({}, $scope.Grade);

    $scope.ActionToBeTaken = {
        Id: null
        , SNO: null
        , ActionToBeTakenName: null
    };
    $scope.ActionToBeTakenNew = Object.assign({}, $scope.ActionToBeTaken);


    $scope.GeneratItemSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetItemAutoSequence'
        }).then(function successCallback(response) {
            $scope.IssueItemNew.SNO = response.data;
        });
    }
    $scope.GeneratItemSequenceNo();

    $scope.GeneratGradeSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetGradeAutoSequence'
        }).then(function successCallback(response) {
            $scope.GradeNew.SNO = response.data;
        });
    }
    $scope.GeneratGradeSequenceNo();

    $scope.GeneratActionToBeTakenSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetActionToBeTakenAutoSequence'
        }).then(function successCallback(response) {
            $scope.ActionToBeTakenNew.SNO = response.data;
        });
    }
    $scope.GeneratActionToBeTakenSequenceNo();
   
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.IssueTypeList = [
        {
            'Value': 'Order',
            'Text': 'Order'
        },
        {
            'Value': 'General',
            'Text': 'General'
        },
    ];

    $scope.PeriodCategoryList = [
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

    $scope.productionSummary = {
        Id: null,
        PlantId: null,
        EntityId: null,
        ProcessId: null,
        ProductionInChargeId: null,
        ProductionInCharge: null,
        ProductionOrderId: null,
        WorkCenterMasterId: null,
        ProductionDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),
        ProductionShiftId: null,
        Value: 0,
        UOM: null,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        InCharge: null,
        InChargeId: null,
        Remarks: null,
        LotNumber: null,
        PeriodId: null,
        IssueId: null,
        GradeId: null
    };
    $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);

    // Refreshing the serials
    function refreshSerial() {
        for (var j = 0; j < $scope.wcList.length; j++) {
            $scope.wcList[j].Serial = j;
        }
    }
    // Add Tiles
    $scope.AddTile = function (e) {
        console.log(e);
        let ob = {};
        Object.assign(ob, e);
        ob.Flag = 0;
        ob.Id = null;
        ob.WorkCenterMasterId = e.WorkCenterMasterId;
        ob.ProductionOrderId = null;
        ob.LotNumber = null;
        ob.Quantity = 0;
        ob.DetentionSum = 0;
        ob.SumMin = 0;
        ob.Value = null;
        ob.ResponsiblePersonId = e.ResponsiblePersonId;
        ob.InChargeId = e.InChargeId;
        $scope.wcList.splice(e.Serial + 1, 0, ob);
        refreshSerial();
    }

    $scope.Issue = {
        Id: null,
        EntityId: null,
        ProcessId: null,
        IssueNameId: null, 
        IssueName: null,
        DepartmentId: null,
        Department: null,
        IssueType: null,
        IssueCategory: null,
        PositionCodeId: null,
        PositionCode: null,
        Remarks: null,
        CheckingInterval: null,
        IsMandatory: true,
        IsWorkCenter: true,
        Period: null,
        Frequency: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null
    };
    $scope.IssueNew = Object.assign({}, $scope.Issue);
   /* console.log($scope.IssueNew);*/
    $scope.Reason = {
        Id: null,
        IssueId: null,
        ReasonName: null,
    };
    $scope.ReasonNew = Object.assign({}, $scope.Reason);

    $scope.Time = {
        Id: null,
        PeriodName: null,
        PeriodCategory: null,
        IssueId: null,
        FromTime: null,
        ToTime: null,
    };
    $scope.TimeNew = Object.assign({}, $scope.Time);

    $scope.POQuality = {
        Id: null,
        IssueId: null,
        ProcessId:null,
        SequenceNo: null,
        Category: null,
        DependentDate: null,
        Legdays: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        PositionCodeId: null,
        PositionCode: null,
        Remarks: null,
        EntryLevel: null
    };
    $scope.POQualityNew = Object.assign({}, $scope.POQuality);

    $scope.ProcessIssueList = [];
    $scope.GetProcessIssueList = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetProcessIssueList'
        }).then(function successCallback(response) {
            $scope.ProcessIssueList = response.data;
        });
    }
    $scope.GetProcessIssueList();

    $scope.POIssueType = null;
    $scope.GetIssueType = function (QId) {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetIssueType?IssueId=' + QId
        }).then(function successCallback(response) {
            $scope.POIssueType = response.data[0].POIssueType;
        });
    }

    $scope.GetChkInterval = function (QId) {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetChkInterval?IssueId=' + QId
        }).then(function successCallback(response) {
            $scope.IssueItemNew.CheckingInterval = response.data[0].CheckingInterval;
        });
    }

    $scope.ReasonList = [];
    $scope.LoadReasonDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadReasonDetails'
        }).then(function successCallback(response) {
            $scope.ReasonList = response.data;
        }
        )
    }
    $scope.LoadReasonDetails();

    $scope.POQualityList = [];
    $scope.LoadPOQualityDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadPOQualityDetails'
        }).then(function successCallback(response) {
            $scope.POQualityList = response.data;
        }
        )
    }
    $scope.LoadPOQualityDetails();

    $scope.GradeGridList = [];
    $scope.GetGradeGridList = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetGradeGridList'
        }).then(function successCallback(response) {
            $scope.GradeGridList = response.data;
        });
    }
    $scope.GetGradeGridList();

    $scope.TimeList = [];
    $scope.LoadTimeDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadTimeDetails'
        }).then(function successCallback(response) {
            $scope.TimeList = response.data;
        }
        )
    }
    $scope.LoadTimeDetails();

    $scope.GetReasonDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadReasonDetailsEditData?ReasonId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ReasonNew = response.data.Reason[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetPOQualityDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadPOQualityDetailsEditData?PQPId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.POQualityNew = response.data.qualityplan[0];
            $scope.getPOProcess($scope.POQualityNew.IssueId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetTimeDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadTimeDetailsEditData?TimeId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.TimeNew = response.data.Time[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.ReasonSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ReasoningDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlReason,
                data: {
                    'ReasonData': $scope.ReasonNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadReasonDetails();
                    ReasonClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.ReasonClear = function () {
        ReasonClearFields();
    };

    function ReasonClearFields() {
        $scope.Action = "Save";
        $scope.ReasonNew = Object.assign({}, $scope.Reason);
    }

    $scope.POQualitySave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.POQualityDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlPOQuality,
                data: {
                    'POQualityData': $scope.POQualityNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadPOQualityDetails();
                    POQualityClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.POQualityClear = function () {
        POQualityClearFields();
    };

    function POQualityClearFields() {
        $scope.Action = "Save";
        $scope.POQualityNew = Object.assign({}, $scope.POQuality);
    }

    $scope.TimeSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.TimeDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlTime,
                data: {
                    'TimeData': $scope.TimeNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadTimeDetails();
                    TimeClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.TimeClear = function () {
        TimeClearFields();
    };

    function TimeClearFields() {
        $scope.Action = "Save";
        $scope.TimeNew = Object.assign({}, $scope.Time);
    }

    $scope.TimeIssueList = [];
    $scope.LoadTimeIssueListDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadTimeIssueDetails'
        }).then(function successCallback(response) {
            $scope.TimeIssueList = response.data;
        }
        )
    }
    $scope.LoadTimeIssueListDetails();

    $scope.IssueItemIssueList = [];
    $scope.LoadIssueItemIssueListDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadIssueItemIssueDetails'
        }).then(function successCallback(response) {
            $scope.IssueItemIssueList = response.data;
        }
        )
    }
    $scope.LoadIssueItemIssueListDetails();

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
        $scope.IssueItemNew.UOMId = e.data.UOMId;
        $scope.IssueItemNew.UOM = e.data.UOM;
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
    }

    $scope.closeUOMPopUp = function () {
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
    }

    $scope.selectParameter = function () {
        $scope.getParameter();
        angular.element(document.querySelector('#ParameterPopUp')).modal('show');
    }

    $scope.ParameterList = [];
    $scope.getParameter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetParameter',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ParameterList = resp.data;
        });
    }

    $scope.doubleParameter = function (e) {
        $scope.IssueItemNew.ParameterId = e.data.ParameterId;
        $scope.IssueItemNew.Parameter = e.data.Parameter;
        angular.element(document.querySelector('#ParameterPopUp')).modal('hide');
    }

    $scope.closeParameterPopUp = function () {
        angular.element(document.querySelector('#ParameterPopUp')).modal('hide');
    }


    $scope.selectDepartment = function () {
        $scope.getDepartment();
        angular.element(document.querySelector('#DepartmentPop')).modal('show');
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
        $scope.IssueNew.DepartmentId = e.data.DepartmentId;
        $scope.IssueNew.Department = e.data.DepartmentName;
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
    }

    $scope.closeDepartmentPopUp = function () {
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
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
        $scope.IssueNew.PositionCodeId = e.data.Id;
        $scope.IssueNew.PositionCode = e.data.Code;
        angular.element(document.querySelector('#PositionCodePop')).modal('hide');
    }

    $scope.closePositionCodePopUp = function () {
        angular.element(document.querySelector('#PositionCodePop')).modal('hide');
    }



    $scope.selectPOPositionCode = function () {
        $scope.getPOPositionCode();
        angular.element(document.querySelector('#POPositionCodePop')).modal('show');
    }

    $scope.POPositionCodeList = [];
    $scope.getPOPositionCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPositionCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.POPositionCodeList = resp.data;
        });
    }

    $scope.doublePOPositionCode = function (e) {
        $scope.POQualityNew.PositionCodeId = e.data.Id;
        $scope.POQualityNew.PositionCode = e.data.Code;
        angular.element(document.querySelector('#POPositionCodePop')).modal('hide');
    }

    $scope.closePOPositionCodePopUp = function () {
        angular.element(document.querySelector('#POPositionCodePop')).modal('hide');
    }

    $scope.selectItemPositionCode = function (data) {
        $scope.NewObject = data.data;
        $scope.getItemPositionCode();
        angular.element(document.querySelector('#ItemPositionCodePop')).modal('show');
    }

    $scope.ItemPositionCodeList = [];
    $scope.getItemPositionCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPositionCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ItemPositionCodeList = resp.data;
        });
    }

    $scope.doubleItemPositionCode = function (e) {
        //$scope.IssueItemNew.PositionCodeId = e.data.Id;
        //$scope.IssueItemNew.PositionCode = e.data.Code;
        $scope.NewObject.PositionCodeId = e.data.Id;
        $scope.NewObject.PositionCode = e.data.Code;
        angular.element(document.querySelector('#ItemPositionCodePop')).modal('hide');
    }

    $scope.closeIssueItemCodePopUp = function () {
        angular.element(document.querySelector('#ItemPositionCodePop')).modal('hide');
    }

    $scope.IssueNameList = [];
    $scope.GetIssueNameList = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetIssueNameList'
        }).then(function successCallback(response) {
            $scope.IssueNameList = response.data;
        });
    }
    $scope.GetIssueNameList();

    $scope.IssueReasonList = [];
    $scope.GetIssueReasonList = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetIssueReasonList'
        }).then(function successCallback(response) {
            $scope.IssueReasonList = response.data;
        });
    }
    $scope.GetIssueReasonList();

    $scope.IssuePOQualityList = [];
    $scope.GetIssuePOQualityList = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProcessQualityControl/GetIssuePOQualityList'
        }).then(function successCallback(response) {
            $scope.IssuePOQualityList = response.data;
        });
    }
    $scope.GetIssuePOQualityList();

    $scope.IssueList = [];
    $scope.LoadIssueDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadIssueDetails'
        }).then(function successCallback(response) {
            $scope.IssueList = response.data;
        }
        )
    }
    $scope.LoadIssueDetails();

    $scope.GetIssueDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadIssueDetailsEditData?IssueId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.IssueNew = response.data.Issue[0];
            $scope.getQIEntities($scope.IssueNew.IssueNameId);
            $scope.getQIProcess($scope.IssueNew.IssueNameId);
            $scope.loadProcessList($scope.IssueNew.EntityId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.IssueSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.IssueDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlIssue,
                data: {
                    'IssueData': $scope.IssueNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadIssueDetails();
                    $scope.GetIssueReasonList();
                    $scope.LoadTimeIssueListDetails();
                    $scope.LoadIssueItemIssueListDetails();
                    $scope.GetIssuePOQualityList();
                    IssueClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.IssueClear = function () {
        IssueClearFields();
    };

    function IssueClearFields() {
        $scope.Action = "Save";
        $scope.IssueNew = Object.assign({}, $scope.Issue);
    }

    $scope.IssueItemList = [];
    $scope.LoadIssueItemDetails = function (IssueId) {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadIssueItemDetails?IssueId=' + IssueId
        }).then(function successCallback(response) {
            $scope.IssueItemList = response.data;
        }
        )
    }
    //$scope.LoadIssueItemDetails();

    $scope.refreshTemplateIssueItem = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllIssueItem });
    };
    function CheckBoxSelectAllIssueItem(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridIssueItem").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.IssueItemList.length; i++) {
                $scope.IssueItemList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridIssueItem").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.GetIssueItemDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadIssueItemDetailsEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.IssueItemNew = response.data.IssueItem[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    //$scope.IssueItemSave = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.IssueItemDetailsForm.$valid) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrlIssueItem,
    //            data: {
    //                'IssueItemData': $scope.IssueItemNew
    //            },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.LoadIssueItemDetails();
    //                IssueItemClearFields();
    //                $scope.GeneratItemSequenceNo();
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    }
    //};

    $scope.IssueItemSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.IssueItemList.length; i++) {
                if ($scope.IssueItemList[i].Flag == true) {
                    $scope.IssueItemList[i].IssueId = $scope.IssueItemNew.IssueId;
                    $scope.SaveList.push($scope.IssueItemList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlIssueItem,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.IssueItemNew.IssueId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadIssueItemDetails($scope.IssueItemNew.IssueId);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.IssueItemClear = function () {
        IssueItemClearFields();
    };

    function IssueItemClearFields() {
        $scope.Action = "Save";
        $scope.IssueItemNew = Object.assign({}, $scope.IssueItem);
        $scope.GeneratItemSequenceNo();
    }

    $scope.GradeList = [];
    $scope.LoadGradeDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadGradeDetails'
        }).then(function successCallback(response) {
            $scope.GradeList = response.data;
        }
        )
    }
    $scope.LoadGradeDetails();

    $scope.GetGradeDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadGradeDetailsEditData?GradeId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.GradeNew = response.data.Grade[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GradeSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.GradeDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlGrade,
                data: {
                    'GradeData': $scope.GradeNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadGradeDetails();
                    GradeClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.GradeClear = function () {
        GradeClearFields();
    };

    function GradeClearFields() {
        $scope.Action = "Save";
        $scope.GradeNew = Object.assign({}, $scope.Grade);
    }

    $scope.ActionToBeTakenList = [];
    $scope.LoadActionToBeTakenDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadActionToBeTakenDetails'
        }).then(function successCallback(response) {
            $scope.ActionToBeTakenList = response.data;
        }
        )
    }
    $scope.LoadActionToBeTakenDetails();

    $scope.GetActionToBeTakenDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/ProcessQualityControl/LoadActionToBeTakenDetailsEditData?ActionToBeTakenId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ActionToBeTakenNew = response.data.ActionToBeTaken[0];
        }
        )
    }

    $scope.ActionToBeTakenSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ActionToBeTakenDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlActionToBeTaken,
                data: {
                    'ActionToBeTakenData': $scope.ActionToBeTakenNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadActionToBeTakenDetails();
                    ActionToBeTakenClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.ActionToBeTakenClear = function () {
        ActionToBeTakenClearFields();
    };

    function ActionToBeTakenClearFields() {
        $scope.Action = "Save";
        $scope.ActionToBeTakenNew = Object.assign({}, $scope.ActionToBeTaken);
        $scope.GeneratActionToBeTakenSequenceNo();
    }

    $scope.removeIssueModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempIssueId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveIssue')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeIssueRow = function () {
        $http({
            method: 'POST',
            url: 'Productions/ProcessQualityControl/IssueDelete?id=' + $scope.tempIssueId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadIssueDetails();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.removeReasonModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempReasonId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveReason')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeReasonRow = function () {
        $http({
            method: 'POST',
            url: 'Productions/ProcessQualityControl/ReasonDelete?id=' + $scope.tempReasonId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadReasonDetails();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.removePOQualityModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempPOQualityId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemovePOQuality')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removePOQualityRow = function () {
        $http({
            method: 'POST',
            url: 'Productions/ProcessQualityControl/POQualityDelete?id=' + $scope.tempPOQualityId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadPOQualityDetails();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.removeTimeModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempTimeId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveTime')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeTimeRow = function () {
        $http({
            method: 'POST',
            url: 'Productions/ProcessQualityControl/TimeDelete?id=' + $scope.tempTimeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadTimeDetails();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.removeIssueItemModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempIssueItemId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveIssueItem')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeIssueItemRow = function () {
        $http({
            method: 'POST',
            url: 'Productions/ProcessQualityControl/IssueItemDelete?id=' + $scope.tempIssueItemId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadIssueItemDetails();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.removeGradeModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempGradeId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveGrade')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeGradeRow = function () {
        $http({
            method: 'POST',
            url: 'Productions/ProcessQualityControl/GradeDelete?id=' + $scope.tempGradeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadGradeDetails();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.removeActionToBeTakenModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempActionToBeTakenId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveActionToBeTaken')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeActionToBeTakenRow = function () {
        $http({
            method: 'POST',
            url: 'Productions/ProcessQualityControl/ActionToBeTakenDelete?id=' + $scope.tempActionToBeTakenId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadActionToBeTakenDetails();
                ActionToBeTakenClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.SaveQIC = function (data) {
        try {
            if (baseService.isUndefinedOrNull(data.data.Value)) {
                throw "Please enter Value and proceed";
            }
            data.data.ProcessId = $scope.productionSummaryNew.ProcessId;
            data.data.EntityId = $scope.productionSummaryNew.EntityId;
            data.data.ProductionDate = $scope.productionSummaryNew.ProductionDate;
            data.data.ProductionShiftId = $scope.productionSummaryNew.ProductionShiftId;
            data.data.IssueId = $scope.productionSummaryNew.IssueId;
            data.data.PeriodId = $scope.productionSummaryNew.PeriodId;
            data.data.ProductionOrderId = $scope.productionSummaryNew.ProductionOrderId;
            data.data.ProductionInChargeId = $scope.productionSummaryNew.ProductionInChargeId;
            $http({
                method: 'POST',
                url: $scope.saveUrlQICValue,
                data: { 'ProcessQualityControlData': data.data },
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

    $scope.ProductionId = null;
    $scope.ProductionReasonList = [];
    $scope.getReasonValuePopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ProductionId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'Productions/productionSummary/LoadProcessReasonList?ProcessId=' + $scope.productionSummaryNew.ProcessId + '&ProductionId=' + $scope.ProductionId
        }).then(function successCallback(response) {
            $scope.ProductionReasonList = response.data;
            var gridObj = $("#GridReasonValuePopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ReasonValuePopup')).modal('show');
        }
        )
    }

    $scope.closeReasonValuePopup = function () {
        angular.element(document.querySelector('#ReasonValuePopup')).modal('hide');
    }

    $scope.SaveReasonValue = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProductionReasonList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.ProductionReasonList[i].ReasonValue)) {
                    $scope.ProductionReasonList[i].ProductionId = $scope.ProductionId;
                    $scope.SaveList.push($scope.ProductionReasonList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlReasonValue,
                data: { 'ProductionReasonData': $scope.SaveList },
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

    $scope.ProductionSummaryDetail = {
        Id: null,
        ProductionSummaryId: null,
        FCharId: null,
        SCharId: null,
        TCharId: null,
        Characteristics1Id: null,
        Characteristics1ValueId: null,
        Characteristics2Id: null,
        Characteristics2ValueId: null,
        Characteristics3Id: null,
        Characteristics3ValueId: null,
        Qty: 0
    };

    $scope.selectProductionInCharge = function () {
        $scope.getProductionInCharge();
        angular.element(document.querySelector('#ProductionInChargePopup')).modal('show');
    }

    $scope.ProductionInChargeList = [];
    $scope.getProductionInCharge = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProductionInChargeList = resp.data;
        });
    }

    $scope.doubleProductionInCharge = function (e) {
        $scope.productionSummaryNew.ProductionInChargeId = e.data.SystemId;
        $scope.productionSummaryNew.ProductionInCharge = e.data.EmployeeName;
        angular.element(document.querySelector('#ProductionInChargePopup')).modal('hide');
    }

    $scope.closeProductionInChargePopUp = function () {
        angular.element(document.querySelector('#ProductionInChargePopup')).modal('hide');
    }

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.IssueNew.EntityId = $scope.entityList[0].Value;
                //default
                $scope.loadProcessList($scope.IssueNew.EntityId);
            }
        });
    }
    $scope.getAllEntities();

    $scope.QIentityList = [];
    $scope.getQIEntities = function (Id) {
        $http.get('Productions/ProcessQualityControl/GetEntity?IssueNameId=' + Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.QIentityList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.IssueNew.EntityId = $scope.QIentityList[0].Value;
                    }
                }
            });
    }

    $scope.QIprocessList = [];
    $scope.getQIProcess = function (Id) {
        $http.get('Productions/ProcessQualityControl/GetProcess?IssueNameId=' + Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.QIprocessList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.IssueNew.ProcessId = $scope.QIprocessList[0].Value;
                    }
                }
            });
    }

    $scope.POprocessList = [];
    $scope.getPOProcess = function (Id) {
        $http.get('Productions/ProcessQualityControl/GetProcess?IssueNameId=' + Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.POprocessList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.POQualityNew.ProcessId = $scope.POprocessList[0].Value;
                    }
                }
            });
    }
  

    $scope.ArticleList = [];
    $scope.getArticle = function (POId) {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetArticle?POID=" + POId + ""
        }).then(function successCallback(response) {
            $scope.ArticleList = response.data;
        });
    }

    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.IssueNew.ProcessId = $scope.processList[0].Value;
                $scope.getProdLevel();
                //default
                $scope.loadWC($scope.IssueNew.ProcessId, $scope.IssueNew.EntityId);
            }
        });
    };

    $scope.LotNumberList = [];
    $scope.disGo = false;
    $scope.IsVisible = true;
    $scope.PQEnable = true;
    $scope.LotNumberCapture = false;
    $scope.LotNumberMandatory = false;
    $scope.IsSKU1 = false;
    $scope.IsSKU2 = false;
    $scope.IsSKU3 = false;
    $scope.IsFirst = false;
    $scope.IsParameterBased = false;
    $scope.ToCloseAllowed = false;

    $scope.getProdLevel = function () {
        try {
            $scope.PQEnable = false;

            $scope.IsFirst = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsFirst;

            $scope.productionSummaryNew.ProductionBookingLevel = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].ProductionBookingLevel;

            $scope.LotNumberCapture = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].LotNumberCapture;

            $scope.LotNumberMandatory = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].LotNumberMandatory;

            $scope.IsSKU1 = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsSKU1;

            $scope.IsSKU2 = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsSKU2;

            $scope.IsSKU3 = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsSKU3;

            $scope.IsParameterBased = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsParameterBased;

            $scope.ToCloseAllowed = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].ToCloseAllowed;

            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                $scope.ProductionLevel = 'Production Order';
                $scope.disGo = false;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                $scope.ProductionLevel = 'Sales Order';
                $scope.disGo = false;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                $scope.ProductionLevel = 'Master Order Item';
                $scope.disGo = false;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductCode') {
                $scope.ProductionLevel = 'Product Code';
                $scope.disGo = false;
            }
            else {
                $scope.disGo = true;
                $scope.PQEnable = true;
                throw 'Production Booking Level is not defined for selected process.';
            }

            if ($scope.IsSKU1 === true || $scope.IsSKU2 === true || $scope.IsSKU2 === true || $scope.IsParameterBased == true) {
                $scope.PQEnable = true;
                $scope.disGo = false;
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.wcList = [];
    $scope.loadWC = function () {
        try {
            $http.get('Productions/ProcessQualityControl/GetIssueCboQIC?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&shiftId=' + $scope.productionSummaryNew.ProductionShiftId + '&ProductionInChargeId=' + $scope.productionSummaryNew.ProductionInChargeId + '&IssueId=' + $scope.productionSummaryNew.IssueId + '&PeriodId=' + $scope.productionSummaryNew.PeriodId)
                .then(function (response) {
                    $scope.wcList = response.data;
                    for (var i = 0; i < $scope.wcList.length; i++) {
                        Object.assign($scope.wcList[i], { 'Serial': parseInt(i) });
                    }
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.productionSummaryNew.NewLotNumber = true;
    $scope.ShowLotNum = false;
    $scope.SetNewLotNumber = function () {
        if ($scope.productionSummaryNew.NewLotNumber) {
            $scope.ShowLotNum = false;
            $scope.productionSummaryNew.LotNumber = null;
        } else {
            $scope.ShowLotNum = true;
        }
    };
    $scope.ShowNew = false;
    $scope.getLotNumberCbo = function () {
        try {
            $http.get('Productions/Productionsummary/GetLotNumberCbo?SalesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId + '&ProcessId=' + $scope.productionSummaryNew.ProcessId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel)
                .then(function (response) {
                    $scope.LotNumberList = response.data;
                    if (baseService.arrayLength($scope.LotNumberList) > 0) {
                        $scope.ShowLotNum = true;
                        $scope.ShowNew = true;
                        $scope.productionSummaryNew.NewLotNumber = false;
                    } else {
                        $scope.ShowLotNum = false;
                        $scope.productionSummaryNew.NewLotNumber = true;
                    }
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.CheckValidLotNumber = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.LotNumber)) {
                //if (/^[ A-Za-z0-9_@./#&+-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                if (/^[ A-Za-z0-9_./-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                    ///
                } else {
                    throw "You have entered an invalid value for Lot Number.";
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //$scope.productTimeList = [];
    //cboService.getProductionBookingPeriodCbo(function (result) {
    //    $scope.productTimeList = result;
    //});

    $scope.ProdQtyCount = 0;
    $scope.getProdQty = function () {
        try {
            $scope.ProdQtyCount = 0;
            $http.get('Productions/Productionsummary/GetTotalProductionQty?wcid=' + $scope.productionSummaryNew.WorkCenterMasterId + '&workdate=' + $scope.productionSummaryNew.ProductionDate)
                .then(function (response) {
                    $scope.ProdQtyCount = 0;
                    if (!baseService.isUndefinedOrNull(response.data[0].TotalProductionQty)) {
                        $scope.ProdQtyCount = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
                    }
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.TotalSalesOrderQty = 0;
    $scope.TotalProductionBookingQty = 0;
    $scope.RemainQty = 0;
    $scope.TotalActualPlannedQty = 0;
    $scope.TotalProcessPlanPercentage = 0;
    $scope.TotalPOQty = 0;
    $scope.TotalProcessPlanQty = 0;
    $scope.GetTotalProductionBookingQty = function () {
        try {
            $scope.TotalSalesOrderQty = 0;
            $scope.TotalProductionBookingQty = 0;
            $scope.RemainQty = 0;
            $scope.TotalActualPlannedQty = 0;
            $scope.TotalProcessPlanPercentage = 0;
            $scope.TotalPOQty = 0;
            $scope.TotalProcessPlanQty = 0;
            if ($scope.NewObject.BookingLevel === 'ProductionOrder') {
                if (baseService.isUndefinedOrNull($scope.NewObject.ProductionOrderId)) {
                    $scope.NewObject.ProductionOrderId = $scope.ProductionOrderId;
                }
                $http.get('Productions/Productionsummary/GetPOQty?productionOrderId=' + $scope.NewObject.ProductionOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
                            $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(2);
                            $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
                            $scope.TotalPOQty = parseFloat(response.data[0].POQty).toFixed(2);
                            $scope.TotalProcessPlanQty = parseFloat(response.data[0].ProcessPlanQty).toFixed(2);
                            $scope.NewObject.RemainingQty = $scope.RemainQty;
                            $scope.NewObject.OrderQty = $scope.TotalSalesOrderQty;
                            $scope.NewObject.BookedQty = $scope.TotalProductionBookingQty;
                            $scope.NewObject.ActualPlannedQty = $scope.TotalActualPlannedQty;
                            $scope.NewObject.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
                            $scope.NewObject.POQty = $scope.TotalPOQty;
                            $scope.NewObject.ProcessPlanQty = $scope.TotalProcessPlanQty;
                        }
                    });
            }
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
            //    if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
            //        $scope.productionSummaryNew.SalesOrderId = $scope.SalesOrderId;
            //    }
            //    $http.get('Productions/Productionsummary/GetTotalSOQty?salesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
            //        .then(function (response) {
            //            if (baseService.arrayLength(response.data) > 0) {
            //                $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
            //                $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
            //                $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
            //                $scope.NewObject.RemainingQty = $scope.RemainQty;
            //                $scope.NewObject.OrderQty = $scope.TotalSalesOrderQty;
            //                $scope.NewObject.BookedQty = $scope.TotalProductionBookingQty;
            //            }
            //        });
            //}
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem' || $scope.productionSummaryNew.ProductionBookingLevel === 'ProductionCode') {
            //    if (baseService.isUndefinedOrNull($scope.NewObject.MasterOrderItemId)) {
            //        $scope.NewObject.MasterOrderItemId = $scope.MasterOrderItemId;
            //    }
            //    $http.get('Productions/Productionsummary/GetTotalMOIQty?MasterOrderItemId=' + $scope.NewObject.MasterOrderItemId + '&processId=' + $scope.productionSummaryNew.ProcessId)
            //        .then(function (response) {
            //            if (baseService.arrayLength(response.data) > 0) {
            //                $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
            //                $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
            //                $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
            //                $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(2);
            //                $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
            //                $scope.NewObject.RemainingQty = $scope.RemainQty;
            //                $scope.NewObject.OrderQty = $scope.TotalSalesOrderQty;
            //                $scope.NewObject.BookedQty = $scope.TotalProductionBookingQty;
            //                $scope.NewObject.ActualPlannedQty = $scope.TotalActualPlannedQty;
            //                $scope.NewObject.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
            //            }
            //        });
            //}
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.GetMasterOrderItemQty = function () {
        try {
            $scope.TotalSalesOrderQty = 0;
            $scope.TotalProductionBookingQty = 0;
            $scope.RemainQty = 0;
            $scope.TotalActualPlannedQty = 0;
            $scope.TotalProcessPlanPercentage = 0;
            $scope.TotalPOQty = 0;
            $scope.TotalProcessPlanQty = 0;
            if ($scope.NewobjectMOI.BookingLevel === 'MasterOrderItem' || $scope.NewobjectMOI.BookingLevel === 'ProductionCode') {
                if (baseService.isUndefinedOrNull($scope.NewobjectMOI.MasterOrderItemId)) {
                    $scope.NewobjectMOI.MasterOrderItemId = $scope.MasterOrderItemId;
                }
                $http.get('Productions/Productionsummary/GetTotalMOIQty?POId=' + $scope.NewobjectMOI.ProductionOrderId + '&MasterOrderItemId=' + $scope.NewobjectMOI.MasterOrderItemId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
                            $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(2);
                            $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
                            $scope.TotalPOQty = parseFloat(response.data[0].POQty).toFixed(2);
                            $scope.TotalProcessPlanQty = parseFloat(response.data[0].ProcessPlanQty).toFixed(2);
                            $scope.NewobjectMOI.RemainingQty = $scope.RemainQty;
                            $scope.NewobjectMOI.OrderQty = $scope.TotalSalesOrderQty;
                            $scope.NewobjectMOI.BookedQty = $scope.TotalProductionBookingQty;
                            $scope.NewobjectMOI.ActualPlannedQty = $scope.TotalActualPlannedQty;
                            $scope.NewobjectMOI.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
                            $scope.NewobjectMOI.POQty = $scope.TotalPOQty;
                            $scope.NewobjectMOI.ProcessPlanQty = $scope.TotalProcessPlanQty;
                        }
                    });
            }
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.GetProductCodeItemQty = function () {
        try {
            $scope.TotalSalesOrderQty = 0;
            $scope.TotalProductionBookingQty = 0;
            $scope.RemainQty = 0;
            $scope.TotalActualPlannedQty = 0;
            $scope.TotalProcessPlanPercentage = 0;
            $scope.TotalPOQty = 0;
            $scope.TotalProcessPlanQty = 0;
            if ($scope.NewobjectPC.BookingLevel === 'ProductCode') {
                if (baseService.isUndefinedOrNull($scope.NewobjectPC.MasterOrderItemId)) {
                    $scope.NewobjectPC.MasterOrderItemId = $scope.MasterOrderItemId;
                }
                $http.get('Productions/Productionsummary/GetTotalPCQty?POId=' + $scope.NewobjectPC.ProductionOrderId + '&MasterOrderItemId=' + $scope.NewobjectPC.MasterOrderItemId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
                            $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(2);
                            $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
                            $scope.TotalPOQty = parseFloat(response.data[0].POQty).toFixed(2);
                            $scope.TotalProcessPlanQty = parseFloat(response.data[0].ProcessPlanQty).toFixed(2);
                            $scope.NewobjectPC.RemainingQty = $scope.RemainQty;
                            $scope.NewobjectPC.OrderQty = $scope.TotalSalesOrderQty;
                            $scope.NewobjectPC.BookedQty = $scope.TotalProductionBookingQty;
                            $scope.NewobjectPC.ActualPlannedQty = $scope.TotalActualPlannedQty;
                            $scope.NewobjectPC.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
                            $scope.NewobjectPC.POQty = $scope.TotalPOQty;
                            $scope.NewobjectPC.ProcessPlanQty = $scope.TotalProcessPlanQty;
                        }
                    });
            }
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.GetSalesOrderItemQty = function () {
        try {
            $scope.TotalSalesOrderQty = 0;
            $scope.TotalProductionBookingQty = 0;
            $scope.RemainQty = 0;
            $scope.TotalActualPlannedQty = 0;
            $scope.TotalProcessPlanPercentage = 0;
            $scope.TotalPOQty = 0;
            $scope.TotalProcessPlanQty = 0;
            if ($scope.NewobjectSO.BookingLevel === 'SalesOrder') {
                if (baseService.isUndefinedOrNull($scope.NewobjectSO.SalesOrderId)) {
                    $scope.NewobjectSO.SalesOrderId = $scope.SalesOrderId;
                }
                $http.get('Productions/Productionsummary/GetTotalSO?POId=' + $scope.NewobjectSO.ProductionOrderId + '&salesOrderId=' + $scope.NewobjectSO.SalesOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
                            $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(2);
                            $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
                            $scope.TotalPOQty = parseFloat(response.data[0].POQty).toFixed(2);
                            $scope.TotalProcessPlanQty = parseFloat(response.data[0].ProcessPlanQty).toFixed(2);
                            $scope.NewobjectSO.RemainingQty = $scope.RemainQty;
                            $scope.NewobjectSO.OrderQty = $scope.TotalSalesOrderQty;
                            $scope.NewobjectSO.BookedQty = $scope.TotalProductionBookingQty;
                            $scope.NewobjectSO.ActualPlannedQty = $scope.TotalActualPlannedQty;
                            $scope.NewobjectSO.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
                            $scope.NewobjectSO.POQty = $scope.TotalPOQty;
                            $scope.NewobjectSO.ProcessPlanQty = $scope.TotalProcessPlanQty;
                        }
                    });
            }
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //$scope.shiftList = [];
    //cboService.GetProductionShiftCbo(function (result) {
    //    $scope.shiftList = result;
    //    if (baseService.arrayLength(result) === 1) {
    //        $scope.productionSummaryNew.ProductionShiftId = $scope.shiftList[0].Value;
    //    }
    //});

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }


    $scope.DateValidation = function (ProductionDate) {
        try {
            if (new Date(ProductionDate) > new Date()) {
                throw "Production Date must be below or equal to current Date!";
            }

        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    };
    $scope.RemainingQtyValue = null;
    $scope.ValidateProdQty = function (ProcessId, POId) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProdQtyValidate?Processid=' + ProcessId + '&POId=' + POId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.RemainingQtyValue = resp.data[0].RemainingQty;
        });
    }

    function ValidationMaster() {
        try {
            //CheckField("Work Center Master", $scope.productionSummaryNew.WorkCenterMasterId);

            if ($scope.LotNumberCapture && $scope.LotNumberMandatory) {
                CheckField("Lot Number", $scope.NewObject.LotNumber);
            }

            /*  if ($scope.productionSummaryNew.ProductionBookingLevel === "ProductionOrder") {*/
            if ($scope.productionSummaryNew.ProductionOrderId == null) {
                CheckField("Production Order", $scope.productionSummaryNew.ProductionOrderId);
                /*CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);*/
                //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            }
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === "SalesOrder") {
            //    CheckField("Sales Order", $scope.productionSummaryNew.SalesOrderId);
            //    CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
            //    CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
            //    CheckField("Article", $scope.productionSummaryNew.ArticleId);
            //    CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
            //    //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            //}
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === "MasterOrderItem") {
            //    CheckField("Master Order Item", $scope.productionSummaryNew.MasterOrderItemId);
            //    CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
            //    CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
            //    CheckField("Article", $scope.productionSummaryNew.ArticleId);
            //    CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
            //    //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            //}
            //else {
            //    CheckField("Product Code", $scope.productionSummaryNew.ProductLibraryId);
            //    CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
            //    CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
            //    CheckField("Article", $scope.productionSummaryNew.ArticleId);
            //    CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
            //    //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            //}
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationPreMaster() {
        try {
            CheckField("Entity", $scope.productionSummaryNew.EntityId);
            CheckField("Process", $scope.productionSummaryNew.ProcessId);
            CheckField("Production Date", $scope.productionSummaryNew.ProductionDate);
            CheckField("Shift", $scope.productionSummaryNew.ProductionShiftId);
            CheckField("Issue", $scope.productionSummaryNew.IssueId);
            CheckField("Period", $scope.productionSummaryNew.PeriodId);
            if ($scope.POIssueType === 'Order') {
            CheckField("PONo", $scope.productionSummaryNew.ProductionOrderId);
            }
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationDetail(master) {
        try {
            CheckField("Production Summary Id", master.Id);
            CheckField("Sales Order", master.SalesOrderId);
            CheckField("MaterialMaster", master.MaterialMasterId);
            CheckField("Production Date", $scope.productionSummaryNew.ProductionDate);
        } catch (ex) {
            throw ex;
        }
    }

    $scope.IsGo = false;
    $scope.masterGo = function (isdisabled) {
        try {
            ValidationPreMaster();
            $scope.SetGo(isdisabled);
            if ($scope.IsParameterBased == true) {
                $scope.IsVisible = false;
            }
            else {
                $scope.IsVisible = true;
            }
            $scope.loadWC($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.EntityId);
            //$scope.getLineGrid();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SetGo = function (isdisabled) {
        $scope.IsGo = isdisabled;
    };

    $scope.SetBack = function (isdisabled) {
        $scope.IsGo = isdisabled;
        $scope.ClearMasterPart();
        $scope.ProductionSummaryDetail = [];
        $scope.LineGridList = [];
    };

    $scope.SOItemList = [];
    $scope.getMaterialMasterbyTypePopUp = function (flag) {
        if (baseService.isUndefinedOrNull($scope.productionSummaryNew.WorkCenterMasterId)) {
            return ShowResult('Please Work Center.', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
            return ShowResult('Please Production Order.', 'failure');
        }
        $scope.SOItemList = [];
        $http.get('Productions/ProductionSummary/GetItemsData?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.productionSummaryNew.WorkCenterMasterId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
            angular.element(document.querySelector('#POItemPopup')).modal('show');
        }
        else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
            angular.element(document.querySelector('#SOItemPopup')).modal('show');
        }
        else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
            angular.element(document.querySelector('#MasterOrderItemPopup')).modal('show');
        }
        else {
            angular.element(document.querySelector('#ProductCodePopup')).modal('show');
        }
    };

    $scope.selectSOItem = function ($event) {
        try {
            var soitem = $event.data;
            //if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
            //    $scope.productionSummaryNew.ProductionOrderId = soitem.POId;
            //}
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
            //    $scope.productionSummaryNew.SalesOrderId = soitem.SOId;
            //}
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
            //    $scope.productionSummaryNew.MasterOrderItemId = soitem.MasterOrderItemId;
            //}
            //else {
            //    $scope.productionSummaryNew.ProductLibraryId = soitem.ProductLibraryId;
            //}

            $scope.productionSummaryNew.ProductLibraryId = soitem.ProductLibraryId;
            $scope.productionSummaryNew.ProductCode = soitem.ProductCode;
            $scope.productionSummaryNew.MasterOrderItemId = soitem.MasterOrderItemId;
            $scope.productionSummaryNew.SalesOrderId = soitem.SOId;


            $scope.productionSummaryNew.MaterialMasterId = soitem.MaterialMasterId;
            $scope.productionSummaryNew.MaterialMaster = soitem.MaterialMaster;
            $scope.productionSummaryNew.ArticleId = soitem.ArticleId;
            $scope.productionSummaryNew.Article = soitem.Article;
            $scope.productionSummaryNew.Customer = soitem.Customer;
            $scope.productionSummaryNew.UOM = soitem.UOM;
            $scope.productionSummaryNew.MOQty = soitem.MOQty;
            $scope.productionSummaryNew.ExtraP = soitem.ExtraP;
            $scope.productionSummaryNew.WastageP = soitem.WastageP;
            $scope.productionSummaryNew.MasterOrderNo = soitem.MasterOrderNo;
            $scope.productionSummaryNew.CharCount = soitem.CharCount;
            $scope.productionSummaryNew.PONumber = soitem.PONumber;

            $scope.productionSummaryNew.BuyerOrder = soitem.BuyerOrder;
            $scope.productionSummaryNew.OwnOrder = soitem.OwnOrder;

            $scope.productionSummaryNew.BuyerItem = soitem.BuyerItem;
            $scope.productionSummaryNew.OwnItem = soitem.OwnItem;

            if (!baseService.isUndefinedOrNull(soitem.RemainingQty)) {
                $scope.RemainQty = parseFloat(soitem.RemainingQty.toFixed(2));
            }
            if (!baseService.isUndefinedOrNull(soitem.PlannedQty)) {
                $scope.TotalSalesOrderQty = parseFloat(soitem.PlannedQty.toFixed(2));
            }
            if (!baseService.isUndefinedOrNull(soitem.TotalProductionQty)) {
                $scope.TotalProductionBookingQty = parseFloat(soitem.TotalProductionQty.toFixed(2));
            }


            angular.element(document.querySelector('#SOItemPopup')).modal('hide');
            angular.element(document.querySelector('#ProductCodePopup')).modal('hide');
            angular.element(document.querySelector('#MasterOrderItemPopup')).modal('hide');


            $scope.GetTotalProductionBookingQty();
            $scope.getLotNumberCbo();
        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function () { 
        $scope.ProductionOrderList = [];
        $http.get('Productions/ProcessQualityControl/GetQualityProductionOrderList?entityid=' + $scope.productionSummaryNew.EntityId  + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ToCloseAllowed=' + $scope.ToCloseAllowed)
            .then(
                function successCallback(response) {
                    $scope.ProductionOrderList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

        angular.element(document.querySelector('#POItemPopup')).modal('show');

    };


    $scope.SetPrOData = function ($event) {
        $scope.productionSummaryNew.ProductionOrderId = $event.data.POId;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }

    $scope.psdList = [];
    $scope.char1Save = function () {
        try {
            angular.element(document.querySelector('#firstPopup')).modal('hide');
        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.SalesOrderId = null;
    /*$scope.ProductionOrderId = null;*/
    $scope.ClearMasterPart = function () {
        $scope.ProductionOrderId = $scope.productionSummaryNew.ProductionOrderId;
        $scope.SalesOrderId = $scope.productionSummaryNew.SalesOrderId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var processid = $scope.productionSummaryNew.ProcessId;
        var workdate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var wcid = $scope.productionSummaryNew.WorkCenterMasterId;
        var piid = $scope.productionSummaryNew.ProductionInChargeId;
        $scope.productionSummaryNew.Id = null;
        $scope.productionSummaryNew.SalesOrderId = null;
        $scope.productionSummaryNew.ProductionOrderId = null;
        $scope.productionSummaryNew.MaterialMasterId = null;
        $scope.productionSummaryNew.MaterialMaster = null;
        $scope.productionSummaryNew.ArticleId = null;
        $scope.productionSummaryNew.Article = null;
        $scope.productionSummaryNew.UOM = null;
        $scope.productionSummaryNew.MOQty = null;
        $scope.productionSummaryNew.ExtraP = null;
        $scope.productionSummaryNew.WastageP = null;
        $scope.productionSummaryNew.MasterOrderNo = null;
        $scope.productionSummaryNew.CharCount = null;
        $scope.productionSummaryNew.ProductionGrade = null;

        $scope.productionSummaryNew.Quantity = null;
        $scope.productionSummaryNew.QtyWithoutScan = 0;
        $scope.productionSummaryNew.ScanQty = 0;
        $scope.productionSummaryNew.Customer = null;
        $scope.productionSummaryNew.ResponsiblePersonId = null;
        $scope.productionSummaryNew.ResponsiblePersonName = null;
        $scope.productionSummaryNew.InChargeId = null;
        $scope.productionSummaryNew.InCharge = null;
        $scope.productionSummaryNew.MentorId = null;
        $scope.productionSummaryNew.MentorName = null;
        $scope.productionSummaryNew.PONumber = null;
        $scope.productionSummaryNew.InTime = null;
        $scope.productionSummaryNew.OutTime = null;
        $scope.productionSummaryNew.ConsumeHour = 0;
        $scope.productionSummaryNew.ManPower = 0;
        $scope.productionSummaryNew.CheckedBy = null;
        $scope.productionSummaryNew.CheckedByName = null;
        $scope.productionSummaryNew.Remarks = null;
        $scope.productionSummaryNew.LotNumber = null;

        $scope.productionSummaryNew.BuyerOrder = null;
        $scope.productionSummaryNew.OwnOrder = null;
        $scope.productionSummaryNew.BuyerItem = null;
        $scope.productionSummaryNew.OwnItem = null;
        $scope.productionSummaryNew.NewLotNumber = true;
        $scope.ShowLotNum = false;
        $scope.ShowNew = false;
    }

    $scope.selectLineItem = function (soitem) {
        try {
            $scope.productionSummaryNew.Id = soitem.Id;
            $scope.productionSummaryNew.SalesOrderId = soitem.SalesOrderId;
            $scope.productionSummaryNew.ProductionOrderId = soitem.ProductionOrderId;
            $scope.productionSummaryNew.MaterialMasterId = soitem.MaterialMasterId;
            $scope.productionSummaryNew.MaterialMaster = soitem.MaterialMaster;
            $scope.productionSummaryNew.ArticleId = soitem.ArticleId;
            $scope.productionSummaryNew.Article = soitem.Article;
            $scope.productionSummaryNew.Customer = soitem.Customer;
            $scope.productionSummaryNew.UOM = soitem.UOM;
            $scope.productionSummaryNew.MOQty = soitem.MOQty;
            $scope.productionSummaryNew.ExtraP = soitem.ExtraP;
            $scope.productionSummaryNew.WastageP = soitem.WastageP;
            $scope.productionSummaryNew.MasterOrderNo = soitem.MasterOrderNo;
            $scope.productionSummaryNew.CharCount = soitem.CharCount;
            $scope.productionSummaryNew.Quantity = soitem.Quantity;
            $scope.productionSummaryNew.ProductionGrade = soitem.ProductionGrade;
            $scope.productionSummaryNew.LotNumber = soitem.LotNumber;
            $scope.productionSummaryNew.BuyerItem = soitem.BuyerItem;
            $scope.productionSummaryNew.OwnItem = soitem.OwnItem;
            $scope.productionSummaryNew.BuyerOrder = soitem.BuyerOrder;
            $scope.productionSummaryNew.OwnOrder = soitem.OwnOrder;
            angular.element(document.querySelector('#SOItemPopup')).modal('hide');

        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.CharacteristicsValueId = null;
    $scope.characteristicsValueList = [];
    $scope.showFirstPopup = function (master) {
        try {
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                    throw "Production Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
                    throw "Sales Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.MasterOrderItemId)) {
                    throw "Master Order Item is required.";
                }
            }
            else {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductLibraryId)) {
                    throw "Product Code is required.";
                }
            }
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionGrade)) {
                throw "Grade is required.";
            }
            $scope.productionSummaryNew.Id = master.Id;
            $scope.productionSummaryNew.MaterialMasterId = master.MaterialMasterId;
            $scope.productionSummaryNew.SalesOrderId = master.SalesOrderId;
            $scope.productionSummaryNew.ProductionOrderId = master.ProductionOrderId;
            $scope.productionSummaryNew.MasterOrderItemId = master.MasterOrderItemId;
            $scope.productionSummaryNew.ProductLibraryId = master.ProductLibraryId;
            $scope.productionSummaryNew.ArticleId = master.ArticleId;
            $scope.productionSummaryNew.CharCount = master.CharCount;

            $scope.GetcharacteristicsValueList(master.ProductionOrderId);

            angular.element(document.querySelector('#firstPopup')).modal('show');

        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    }

    $scope.showSecondPopup = function (master) {
        try {
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                    throw "Production Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
                    throw "Sales Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.MasterOrderItemId)) {
                    throw "Master Order Item is required.";
                }
            }
            else {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductLibraryId)) {
                    throw "Product Code is required.";
                }
            }
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionGrade)) {
                throw "Grade is required.";
            }
            $scope.productionSummaryNew.Id = master.Id;
            $scope.productionSummaryNew.MaterialMasterId = master.MaterialMasterId;
            $scope.productionSummaryNew.SalesOrderId = master.SalesOrderId;
            $scope.productionSummaryNew.ProductionOrderId = master.ProductionOrderId;
            $scope.productionSummaryNew.MasterOrderItemId = master.MasterOrderItemId;
            $scope.productionSummaryNew.ProductLibraryId = master.ProductLibraryId;
            $scope.productionSummaryNew.ArticleId = master.ArticleId;
            $scope.productionSummaryNew.CharCount = master.CharCount;

            $scope.GetcharacteristicsValueList(master.ProductionOrderId);

            angular.element(document.querySelector('#firstPopup')).modal('show');

        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    }

    $scope.showBothPopup = function (master) {
        try {
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                    throw "Production Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
                    throw "Sales Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.MasterOrderItemId)) {
                    throw "Master Order Item is required.";
                }
            }
            else {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductLibraryId)) {
                    throw "Product Code is required.";
                }
            }
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionGrade)) {
                throw "Grade is required.";
            }
            $scope.productionSummaryNew.Id = master.Id;
            $scope.productionSummaryNew.MaterialMasterId = master.MaterialMasterId;
            $scope.productionSummaryNew.SalesOrderId = master.SalesOrderId;
            $scope.productionSummaryNew.ProductionOrderId = master.ProductionOrderId;
            $scope.productionSummaryNew.MasterOrderItemId = master.MasterOrderItemId;
            $scope.productionSummaryNew.ProductLibraryId = master.ProductLibraryId;
            $scope.productionSummaryNew.ArticleId = master.ArticleId;
            $scope.productionSummaryNew.CharCount = master.CharCount;

            $scope.GetBothcharacteristicsValueList(master.SalesOrderId);

            angular.element(document.querySelector('#secondPopup')).modal('show');

        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    }

    $scope.GetcharacteristicsValueList = function (soId) {
        cboService.getCharacteristicsValueByPrCbo(soId, function (result) {
            $scope.characteristicsValueList = result;
            if (baseService.arrayLength($scope.characteristicsValueList) > 0) {
                $scope.CharacteristicsValueId = $scope.characteristicsValueList[0].Value;
            }
            $scope.getCharInfo();
        });
    }

    $scope.GetBothcharacteristicsValueList = function (soId) {
        cboService.getCharacteristicsValueCbo(soId, function (result) {
            $scope.characteristicsValueList = result;
            if (baseService.arrayLength($scope.characteristicsValueList) > 0) {
                $scope.CharacteristicsValueId = $scope.characteristicsValueList[0].Value;
            }
            $scope.getChar2Info();
        });
    }

    $scope.ProductionSummaryDetail = [];
    $scope.getChar1Info = function () {
        $scope.ProductionSummaryDetail = [];
        $http.get('Productions/Productionsummary/GetChar1Info?id=' + $scope.productionSummaryNew.Id + '&soid=' + $scope.productionSummaryNew.SalesOrderId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
        //CharCount 1
    };

    $scope.getChar1 = function (masterid, soid) {
        $scope.ProductionSummaryDetail = [];
        $http.get('Productions/Productionsummary/GetChar1Info?id=' + masterid + '&soid=' + soid)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
        //CharCount 1
    };

    $scope.mentorandresperson = [];
    $scope.getMentorAndRespPersonByWCM = function () {
        $http.get('productions/productionsummary/getmentorandresppersonbywcm?wcmId=' + $scope.productionSummaryNew.WorkCenterMasterId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.productionSummaryNew.MentorId = response.data[0].MentorId;
                    $scope.productionSummaryNew.MentorName = response.data[0].MentorName;
                    $scope.productionSummaryNew.ResponsiblePersonId = response.data[0].ResponsiblePersonId;
                    $scope.productionSummaryNew.ResponsiblePersonName = response.data[0].ResponsiblePersonName;
                }
            })
    }

    $scope.getCharInfo = function () {
        $scope.ProductionSummaryDetail = [];

        $http.get('Productions/Productionsummary/GetChar1InfobyPrO?masterid=' + $scope.productionSummaryNew.Id + '&soid=' + $scope.productionSummaryNew.ProductionOrderId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
    };

    $scope.getChar2Info = function () {
        $scope.ProductionSummaryDetail = [];

        $http.get('Productions/Productionsummary/GetCharInfoByPrO?masterid=' + $scope.productionSummaryNew.Id + '&workdate=' + $scope.productionSummaryNew.ProductionDate + '&mmid=' + $scope.productionSummaryNew.MaterialMasterId + '&soid=' + $scope.productionSummaryNew.ProductionOrderId + '&artid=' + $scope.productionSummaryNew.ArticleId + '&CharCount=' + $scope.productionSummaryNew.CharCount + '&CharacteristicsValueId=' + $scope.CharacteristicsValueId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
    };

    $scope.closeCharPopUp = function () {
        angular.element(document.querySelector('#firstPopup')).modal('hide');
        angular.element(document.querySelector('#secondPopup')).modal('hide');
        angular.element(document.querySelector('#thirdPopup')).modal('hide');
    }

    function clearMaster() {
        $scope.productionSummaryNew.Id = null;
        $scope.productionSummaryNew.ProductionGrade = null;
        $scope.productionSummaryNew.Quantity = 0;
        $scope.productionSummaryNew.QtyWithoutScan = 0;
        $scope.productionSummaryNew.ScanQty = 0;
        $scope.productionSummaryNew.UOM = null;
        //$scope.productionSummaryNew.ProductionHour = null;
        $scope.productionSummaryNew.MOQty = null;
        $scope.productionSummaryNew.ExtraP = null;
        $scope.productionSummaryNew.WastageP = null;
        $scope.productionSummaryNew.CharCount = null;

        $scope.LineGridList = [];
    }

    $scope.SearchLineGridList = [];
    $scope.LineGridList = [];
    $scope.getLineGrid = function () {
        try {
            //$scope.ClearMasterPart();
            var entityid = $scope.productionSummaryNew.EntityId;
            var processid = $scope.productionSummaryNew.ProcessId;
            var workdate = $scope.productionSummaryNew.ProductionDate;
            var shiftid = $scope.productionSummaryNew.ProductionShiftId;
            var wcid = $scope.productionSummaryNew.WorkCenterMasterId;

            $scope.LineGridList = [];
            $http.get('Productions/Productionsummary/GetLineItemGrid?entityid=' + entityid + '&processid=' + processid + '&workdate=' + workdate + '&shiftid=' + shiftid + '&wcid=' + wcid + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel)
                .then(function (response) {
                    $scope.LineGridList = [];
                    $scope.LineGridList = response.data;
                    if (baseService.arrayLength($scope.SearchLineGridList) === 0) {
                        baseService.getDDLSearchColumn(response.data, $scope.SearchLineGridList);
                    }
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Get = function (id, index) {
        var entityid = $scope.productionSummaryNew.EntityId;
        var processid = $scope.productionSummaryNew.ProcessId;
        var workdate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var wcid = $scope.productionSummaryNew.WorkCenterMasterId;
        var piid = $scope.productionSummaryNew.ProductionInChargeId;
        var ProductionBookingLevel = $scope.productionSummaryNew.ProductionBookingLevel;

        $scope.index = index;
        $scope.productionSummary = $scope.LineGridList[$scope.index];
        $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);

        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.ProductionDate = workdate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        $scope.productionSummaryNew.WorkCenterMasterId = wcid;
        $scope.productionSummaryNew.ProductionInChargeId = piid;
        $scope.productionSummaryNew.ProductionBookingLevel = ProductionBookingLevel;
        $scope.Action = 'Update';
        if ($scope.IsSKU1 == true && $scope.IsSKU2 == false && $scope.IsSKU3 == false) {
            $scope.GetcharacteristicsValueList($scope.productionSummaryNew.ProductionOrderId);
            //$scope.GetcharacteristicsValueList($scope.productionSummaryNew.SalesOrderId);
            //$scope.getChar1($scope.productionSummaryNew.Id, $scope.productionSummaryNew.SalesOrderId);
        }
        if ($scope.IsSKU1 == false && $scope.IsSKU2 == true && $scope.IsSKU3 == false) {
            $scope.GetcharacteristicsValueList($scope.productionSummaryNew.ProductionOrderId);
            //$scope.getChar1($scope.productionSummaryNew.Id, $scope.productionSummaryNew.SalesOrderId);
        }
        if ($scope.IsSKU1 == true && $scope.IsSKU2 == true) {
            $scope.GetBothcharacteristicsValueList($scope.productionSummaryNew.SalesOrderId);
            //$scope.getCharInfo($scope.productionSummaryNew.Id, $scope.productionSummaryNew.SalesOrderId);
        }
        $scope.productionSummaryNew.InTime = new Date($scope.productionSummaryNew.InTime);
        if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.OutTime)) {
            $scope.productionSummaryNew.OutTime = new Date($scope.productionSummaryNew.OutTime);
        } else {
            $scope.productionSummaryNew.OutTime = null;
        }

        $scope.GetTotalProductionBookingQty();

        $scope.getLotNumberCbo();
    };

    // #region Employee Mentor

    $scope.employeeFilterList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'Middle Name',
            'value': 'MiddleName'
        },
        {
            'name': 'Last Name',
            'value': 'LastName'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Employment Type',
            'value': 'EmploymentType'
        },
        {
            'name': 'Status',
            'value': 'EmployeeStatus'
        }
    ];

    $scope.employeeParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'EmployeeCode, FirstName, MiddleName, LastName '
        , searchBy: 'EmployeeCode'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.showEmployeeListPopUp = function (data, flag) {
        $scope.respOrMentor = flag;
        $scope.NewObject = data.data;
        if ($scope.respOrMentor === 'ResponsiblePerson') { $scope.popUpTitle = 'Responsible Person'; }
        else if ($scope.respOrMentor === 'Mentor') { $scope.popUpTitle = 'Mentor'; }
        else if ($scope.respOrMentor === 'CheckedBy') { $scope.popUpTitle = 'CheckedBy'; }
        baseService.setCurrentPage('employeeList');
        $scope.searchEmployeeByList = [];
        $scope.getEmployeeData = function (pageno) {
            $scope.employeeParameters.plantId = $window.plantId;
            baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.employeeUrl = 'WorkCenters/workcentermaster/GetEmployeeListByPlant';

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            if ($scope.respOrMentor === 'ResponsiblePerson') {
                $scope.NewObject.ResponsiblePersonId = employee.SystemId;
                $scope.NewObject.ResponsiblePerson = employee.EmployeeName;
            }
            else if ($scope.respOrMentor === 'Mentor') {
                $scope.NewObject.MentorId = employee.SystemId;
                $scope.NewObject.Mentor = employee.EmployeeName;
            }
            else if ($scope.respOrMentor === 'CheckedBy') {
                $scope.NewObject.CheckedBy = employee.SystemId;
                $scope.NewObject.CheckedByName = employee.EmployeeName;
            }

        }
        $scope.hideEmployeePopUp();
        var gridObj = $("#ProductionSummaryWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.ClearEmployee = function () {
        if ($scope.respOrMentor === 'ResponsiblePerson') {
            $scope.productionSummaryNew.ResponsiblePersonId = null;
            $scope.productionSummaryNew.ResponsiblePersonName = null;
        }
        else if ($scope.respOrMentor === 'Mentor') {
            $scope.productionSummaryNew.MentorId = null;
            $scope.productionSummaryNew.MentorName = null;
        }
        else if ($scope.respOrMentor === 'CheckedBy') {
            $scope.productionSummaryNew.CheckedBy = null;
            $scope.productionSummaryNew.CheckedByName = null;
        }
    };


    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    // #endregion Employee Mentor

    $scope.countProductQty = function () {
        $scope.ProdQty = 0;
        for (var i = 0; i < $scope.ProductionSummaryDetail.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.ProductionSummaryDetail[i].Qty)) {
                $scope.ProdQty = $scope.ProdQty + $scope.ProductionSummaryDetail[i].Qty;
            }
        }
        $scope.productionSummaryNew.Quantity = $scope.ProdQty;
    }

    $scope.CompareMaxValue = 0;
    $scope.SaveMaster = function () {
        try {
            $scope.getProdLevel();
            ValidationMaster();
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                $scope.productionSummaryNew.MasterOrderItemId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }

            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.MasterOrderItemId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.SalesOrderId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }
            else {
                $scope.productionSummaryNew.SalesOrderId = null;
            }

            if ($scope.BookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
            }

            if ($scope.BookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.SalesOrderId = $scope.SOId;
            }

            if ($scope.BookingLevel === 'ProductCode') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
            }



            if (new Date($scope.productionSummaryNew.ProductionDate) > new Date()) {
                throw "Future Date not allowed for Production Booking.";
            }
            $scope.productionSummaryNew.Quantity = $scope.productionSummaryNew.QtyWithoutScan;
            CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            ValidationMaster();
            if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.LotNumber)) {
                if (/^[ A-Za-z0-9_./-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                    ///
                } else {
                    throw "You have entered an invalid value for Lot Number.";
                }
            }
            $scope.ProdQty = 0;

            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                for (var i = 0; i < $scope.ProductionSummaryDetail.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.ProductionSummaryDetail[i].Qty)) {
                        $scope.ProdQty = $scope.ProdQty + $scope.ProductionSummaryDetail[i].Qty;
                    }
                }
                $scope.productionSummaryNew.Quantity = $scope.ProdQty;
                $scope.productionSummaryNew.QtyWithoutScan = $scope.ProdQty;
            }
            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                if ($scope.ProdQty === 0) {
                    throw "SKU Qty is required.";
                }
            }

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.RemainQty) < 0) {
            //        throw "Order Quantity dosen't available.";
            //    }
            //}

            //if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.TotalPreviousProcessQty) && baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId!=202028)
            //{
            //    throw "If Current Produced Qty is greater than Previous Process Booked Qty then Please enter remarks and inform to departmental head without fail!";
            //}

            //$scope.CompareMaxValue = Math.max(parseFloat($scope.TotalProcessPlanQty), parseFloat($scope.TotalPreviousProcessQty))
            //if (parseFloat($scope.TotalProductionBookingQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue && baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks)) {
            //    throw "You cannot booked greater than Current Process Plan Qty or Previous Process Booked Qty.";
            //}
            //else
            //{
            //    $scope.productionSummaryNew.PPQFlag = true;
            //}
            if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.POPreviousProdQty) && baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
                throw "If Current Produced Qty is greater than Previous Process Booked Qty then Please enter remarks and inform to departmental head without fail!";
            }

            $scope.CompareMaxValue = Math.max(parseFloat($scope.NewObject.ProcessPlanQty), parseFloat($scope.NewObject.POPreviousProdQty))
            if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue) {
                if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue && !baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
                    $scope.productionSummaryNew.PPQFlag = true;
                }
                else {
                    throw "You cannot booked greater than Current Process Plan Qty or Previous Process Booked Qty.";
                }
            }
            else {
                $scope.productionSummaryNew.PPQFlag = false;
            }

            if (parseFloat($scope.productionSummaryNew.Quantity) < 0) {
                throw "Quantity should not be less than 0.";
            }


            //if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.RemainingQtyValue)) {
            //    throw "Produced Quantity should not be greater than RemainingQtyValue.";
            //}

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.NewObject.RemainingQty) < 0 && $scope.productionSummaryNew.Quantity > 0) {
            //        throw "Produced Quantity should less than Order Quantity.";
            //    }
            //}

            //if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.RemainingQty) && $scope.productionSummaryNew.Quantity > 0) {
            //    throw "Produced Quantity should not be greater than Balance Quantity.";
            //}

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    "ps": $scope.productionSummaryNew,
                    "psd": $scope.ProductionSummaryDetail,
                    "ProcessParaList": $scope.ProcessParaList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.NewObject.Id = response.data.ProductionSummary.Id;
                    $scope.ValidateProdQty($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.ProductionOrderId);
                    var gridObj = $("#ProductionSummaryWC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                    //$scope.loadWC();
                    $scope.Action = 'Save';
                }
                angular.element(document.querySelector('#ProcessParaPopup')).modal('hide');
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.refreshTemplateProductionSummaryWC = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllWorkCenter });
    };
    function CheckBoxSelectAllWorkCenter(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#ProductionSummaryWC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.wcList.length; i++) {
                $scope.wcList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#ProductionSummaryWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };
    $scope.refreshTemplateProductionSummaryDetentionWC = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllWorkCenterDetention });
    };
    function CheckBoxSelectAllWorkCenterDetention(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#ProductionSummaryDetentionWC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                $scope.ProcessDetentionLists[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SaveMasterWC = function (data) {
        $scope.NewObject = data.data;
        var processid = $scope.productionSummaryNew.ProcessId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var productiondate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var PInChargId = $scope.productionSummaryNew.ProductionInChargeId;
        var PInCharg = $scope.productionSummaryNew.ProductionInCharge;
        $scope.productionSummaryNew = data.data;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProductionDate = productiondate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        $scope.productionSummaryNew.ProductionInChargeId = PInChargId;
        $scope.productionSummaryNew.ProductionInCharge = PInCharg;
        try {
            $scope.getProdLevel();
            ValidationMaster();
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                $scope.productionSummaryNew.MasterOrderItemId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }

            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.MasterOrderItemId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.SalesOrderId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }
            else {
                $scope.productionSummaryNew.SalesOrderId = null;
            }

            if ($scope.BookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
            }

            if ($scope.BookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.SalesOrderId = $scope.SOId;
            }

            if ($scope.BookingLevel === 'ProductCode') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
            }

            if (new Date($scope.productionSummaryNew.ProductionDate) > new Date()) {
                throw "Future Date not allowed for Production Booking.";
            }
            $scope.productionSummaryNew.QtyWithoutScan = $scope.productionSummaryNew.Quantity;
            CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            ValidationMaster();

            if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.LotNumber)) {
                if (/^[ A-Za-z0-9_./-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                    ///
                } else {
                    throw "You have entered an invalid value for Lot Number.";
                }
            }
            $scope.ProdQty = 0;

            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                for (var i = 0; i < $scope.ProductionSummaryDetail.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.ProductionSummaryDetail[i].Qty)) {
                        $scope.ProdQty = $scope.ProdQty + $scope.ProductionSummaryDetail[i].Qty;
                    }
                }
                $scope.productionSummaryNew.Quantity = $scope.ProdQty;
                $scope.productionSummaryNew.QtyWithoutScan = $scope.ProdQty;
            }
            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                if ($scope.ProdQty === 0) {
                    throw "SKU Qty is required.";
                }
            }

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.RemainQty) < 0) {
            //        throw "Order Quantity dosen't available.";
            //    }
            //}

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.TotalSalesOrderQty) <= parseFloat($scope.TotalProductionBookingQty) + parseFloat($scope.productionSummaryNew.Quantity)) {
            //        throw " less than Order Quantity.";
            //    }
            //}

            if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.POPreviousProdQty) && baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
                throw "If Current Produced Qty is greater than Previous Process Booked Qty then Please enter remarks and inform to departmental head without fail!";
            }

            $scope.CompareMaxValue = Math.max(parseFloat($scope.NewObject.ProcessPlanQty), parseFloat($scope.NewObject.POPreviousProdQty))
            if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue) {
                if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue && !baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
                    $scope.productionSummaryNew.PPQFlag = true;
                }
                else {
                    throw "You cannot booked greater than Current Process Plan Qty or Previous Process Booked Qty.";
                }
            }
            else {
                $scope.productionSummaryNew.PPQFlag = false;
            }
            //if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.RemainingQtyValue)) {
            //    throw "Produced Quantity should not be greater than RemainingQtyValue.";
            //}

            //if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.RemainingQty) && $scope.productionSummaryNew.Quantity > 0) {
            //    throw "Produced Quantity should not be greater than Balance Quantity.";
            //}

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.NewObject.RemainingQty) < 0 && $scope.productionSummaryNew.Quantity > 0) {
            //        throw "Produced Quantity should less than Order Quantity.";
            //    }
            //}


            $http({
                method: 'POST',
                url: $scope.saveUrlWC,
                data: {
                    "ps": $scope.productionSummaryNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.NewObject.Id = response.data.ProductionSummary.Id;
                    $scope.ValidateProdQty($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.ProductionOrderId);
                    var gridObj = $("#ProductionSummaryWC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                    //$scope.loadWC();
                    $scope.Action = 'Save';
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.UpdateMasterWC = function (data) {
        $scope.NewObject = data.data;
        var processid = $scope.productionSummaryNew.ProcessId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var productiondate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var PInChargId = $scope.productionSummaryNew.ProductionInChargeId;
        var PInCharg = $scope.productionSummaryNew.ProductionInCharge;
        $scope.productionSummaryNew = data.data;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProductionDate = productiondate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        $scope.productionSummaryNew.ProductionInChargeId = PInChargId;
        $scope.productionSummaryNew.ProductionInCharge = PInCharg;
        try {
            var date = new Date();
            date.setDate(date.getDate() - 1);
            $scope.YDate = $filter('dateFiltering')(date);
            $scope.getProdLevel();
            if ($scope.productionSummaryNew.ProductionDate < $scope.YDate) {
                throw "Update should be perform only for today's and yestarday's date.";
            }
            ValidationMaster();
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                $scope.productionSummaryNew.MasterOrderItemId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }

            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.MasterOrderItemId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.SalesOrderId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }
            else {
                $scope.productionSummaryNew.SalesOrderId = null;
            }

            if ($scope.BookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
            }

            if ($scope.BookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.SalesOrderId = $scope.SOId;
            }

            if ($scope.BookingLevel === 'ProductCode') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
            }

            if (new Date($scope.productionSummaryNew.ProductionDate) > new Date()) {
                throw "Future Date not allowed for Production Booking.";
            }

            $scope.productionSummaryNew.QtyWithoutScan = $scope.productionSummaryNew.Quantity;
            CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            ValidationMaster();
            if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.LotNumber)) {
                if (/^[ A-Za-z0-9_./-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                    ///
                } else {
                    throw "You have entered an invalid value for Lot Number.";
                }
            }
            $scope.ProdQty = 0;

            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                for (var i = 0; i < $scope.ProductionSummaryDetail.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.ProductionSummaryDetail[i].Qty)) {
                        $scope.ProdQty = $scope.ProdQty + $scope.ProductionSummaryDetail[i].Qty;
                    }
                }
                $scope.productionSummaryNew.Quantity = $scope.ProdQty;
                $scope.productionSummaryNew.QtyWithoutScan = $scope.ProdQty;
            }
            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                if ($scope.ProdQty === 0) {
                    throw "SKU Qty is required.";
                }
            }

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.RemainQty) < 0) {
            //        throw "Order Quantity dosen't available.";
            //    }
            //}

            if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.POPreviousProdQty) && baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
                throw "If Current Produced Qty is greater than Previous Process Booked Qty then Please enter remarks and inform to departmental head without fail!";
            }

            $scope.CompareMaxValue = Math.max(parseFloat($scope.NewObject.ProcessPlanQty), parseFloat($scope.NewObject.POPreviousProdQty))
            if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue) {
                if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue && !baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
                    $scope.productionSummaryNew.PPQFlag = true;
                }
                else {
                    throw "You cannot booked greater than Current Process Plan Qty or Previous Process Booked Qty.";
                }
            }
            else {
                $scope.productionSummaryNew.PPQFlag = false;
            }

            if (parseFloat($scope.productionSummaryNew.Quantity) < 0) {
                throw "Quantity should not be less than 0.";
            }

            //if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.RemainingQty) && $scope.productionSummaryNew.Quantity > 0) {
            //    throw "Produced Quantity should not be greater than Balance Quantity.";
            //}

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.NewObject.RemainingQty) < 0 && $scope.productionSummaryNew.Quantity > 0) {
            //        throw "Produced Quantity should less than Order Quantity.";
            //    }
            //}

            $http({
                method: 'POST',
                url: $scope.UpdateUrlWC,
                data: {
                    "ps": $scope.productionSummaryNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.NewObject.Id = response.data.ProductionSummary.Id;
                    var gridObj = $("#ProductionSummaryWC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                    $scope.Action = 'Save';
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.charSave = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.saveSecondDetailUrl,
                data: {
                    "psd": $scope.ProductionSummaryDetail,
                    "productionSummary": $scope.productionSummaryNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'secondPopup');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'secondPopup');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'secondPopup');
            };
        } catch (e) {
            ShowResult(e, 'failure', 'secondPopup');
        }
    };

    $scope.SaveDetail = function () {
        $http({
            method: 'POST',
            url: $scope.saveDetailUrl,
            data: {
                "psid": $scope.productionSummaryNew.Id,
                "psd": $scope.ProductionSummaryDetail
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.productionSummaryes.push(response.data.ProductionSummary);
                //$scope.productionSummaryes = $filter('orderBy')($scope.productionSummaryes, 'PlanningGroupPriority');
                //baseService.paginationAdd();
                //ClearFields(response.data.PlanningGroupPriority);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

        //}
    };

    $scope.deleteMaster = function (master) {
        if (!baseService.isUndefinedOrNull(master.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + master.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getLineGrid();
                    $scope.getProdQty();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult("Production Summary not found...", 'Info');
        }
    }

    $scope.deleteMasterWC = function (master) {
        if (!baseService.isUndefinedOrNull(master.data.Id)) {
            $http({
                method: 'POST',
                url: 'Productions/ProcessQualityControl/DeleteMasterWC?id=' + master.data.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    // $scope.loadWC();
                    for (var i = 0; i < $scope.wcList.length; i++) {
                        if ($scope.wcList[i].Id == master.data.Id) {
                            $scope.wcList[i].Id = null;
                            break;
                        }
                    }
                    var gridObj = $("#ProductionSummaryWC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult("Production Summary not found...", 'Info');
        }
    }

    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }
    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.SalesOrderListForProductionOrderId = [];
    $scope.getSalesOrderOfProdOrderList = function (prodOrdId) {
        $scope.openPopup('dialogSOItemsForProductionOrder');
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }
    //search
    $scope.TotalPreviousProcessQty = 0;
    $scope.ProcessParaList = [];
    $scope.getProcessParaPopupPoPUp = function (data) {
        $scope.NewObject = data.data;
        var processid = $scope.productionSummaryNew.ProcessId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var productiondate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var PInChargId = $scope.productionSummaryNew.ProductionInChargeId;
        var PInCharg = $scope.productionSummaryNew.ProductionInCharge;
        $scope.productionSummaryNew = data.data;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProductionDate = productiondate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        $scope.productionSummaryNew.ProductionInChargeId = PInChargId;
        $scope.productionSummaryNew.ProductionInCharge = PInCharg;
        $scope.TotalPreviousProcessQty = $scope.NewObject.POPreviousProdQty;
        try {
            $scope.ProcessParaList = [];
            $http.get('Productions/ProductionSummary/GetProcessParaData?processId=' + $scope.productionSummaryNew.ProcessId + '&masterId=' + data.data.Id + '&ProductionOrderId=' + data.data.ProductionOrderId)
                .then(
                    function successCallback(response) {
                        $scope.ProcessParaList = response.data;
                        $scope.GetTotalProductionBookingQty();
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });

            angular.element(document.querySelector('#ProcessParaPopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.Calculate = function () {
        try {
            $scope.productionSummaryNew.QtyWithoutScan = 0;
            $scope.NewObject.Quantity = 0;
            $http({
                method: 'POST',
                url: 'Productions/ProductionSummary/Calculate',
                data: { 'OpenHeadNew': $scope.ProcessParaList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.NewData.length; i++) {
                    for (var j = 0; j < $scope.ProcessParaList.length; j++) {
                        if (response.data.NewData[i].UserName == $scope.ProcessParaList[j].UserName) {
                            $scope.ProcessParaList[j].Value = response.data.NewData[i].Value;
                        }
                    }
                    if (response.data.NewData[i].IsProduction == true) {
                        $scope.NewObject.Quantity += response.data.NewData[i].Value;
                        $scope.productionSummaryNew.QtyWithoutScan = response.data.NewData[i].Value;
                    }
                }
                //$scope.SaveMaster();
                var gridObj = $("#ProductionSummaryWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            }, function errorCallback(response) {
                $scope.ShowResultCustom(response.status.Message, "failure");
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = "Save";
        $scope.productionSummary = {};
        $scope.productionSummaryNew = {};
        $scope.productionSummaryNew.Active = true;
        $scope.productionSummaryNew.ProductionDate = $filter("date")(Date.now(), 'dd-MMM-yyyy');
        $scope.ProdQtyCount = 0;
        $scope.TotalProductionBookingQty = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.RemainQty = 0;
        $scope.SetBack(false);
        $scope.IsGo = false;
        $scope.ProductionSummaryDetail = [];
        $scope.wcList = [];
    }

   
    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $http.get('Productions/Productionsummary/GetShiftList?processId=' + $scope.productionSummaryNew.ProcessId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.productionSummaryNew.ProductionShiftId = $scope.shiftList[0].Value;
                    }
                }
            });
    }

    $scope.IssueHeaderList = [];
    $scope.GetIssueList = function (PId) {
        $http.get('Productions/ProcessQualityControl/GetQualityIssueList?processId=' + PId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.IssueHeaderList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.productionSummaryNew.IssueId = $scope.IssueHeaderList[0].Value;
                    }
                }
            });
    }

    $scope.PeriodHeaderList = [];
    $scope.GetPeriodList = function (PId) {
        $scope.PeriodHeaderList = null;
        $http.get('Productions/ProcessQualityControl/GetQualityPeriodList?IssueId=' + PId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.PeriodHeaderList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.productionSummaryNew.PeriodId = $scope.PeriodHeaderList[0].Value;
                    }
                }
            });
    }

    $scope.selectGridIncharge = function (data) {
        $scope.Newobject = data.data;
        $scope.getsI();
        angular.element(document.querySelector('#InchargeGridPopup')).modal('show');
    }

    $scope.InchargeGridList = [];
    $scope.getsI = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.InchargeGridList = resp.data;
        });
    }

    $scope.doubleInchargeGrid = function (e) {
        $scope.Newobject.InChargeId = e.data.SystemId;
        $scope.Newobject.InCharge = e.data.EmployeeName;
        angular.element(document.querySelector('#InchargeGridPopup')).modal('hide');
    }

    $scope.closeInchargeGridPopup = function () {
        angular.element(document.querySelector('#InchargeGridPopup')).modal('hide');

    }

    $scope.selectGridResponsible = function (data) {
        $scope.Newobject = data.data;
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
        $scope.Newobject.ResponsiblePersonId = e.data.SystemId;
        $scope.Newobject.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.selectResponsible = function () {
        $scope.getResponsible();
        angular.element(document.querySelector('#ResponsiblePopup')).modal('show');
    }

    $scope.ResponsibleList = [];
    $scope.getResponsible = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetResponsiblePerson?IssueId=' + $scope.POQualityNew.IssueId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsibleList = resp.data;
        });
    }

    $scope.doubleResponsible = function (e) {
        $scope.POQualityNew.ResponsiblePersonId = e.data.SystemId;
        $scope.POQualityNew.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePopup')).modal('hide');
    }

    $scope.closeResponsiblePopUp = function () {
        angular.element(document.querySelector('#ResponsiblePopup')).modal('hide');
    }

    $scope.selectIssueResponsible = function () {
        $scope.getIssueResponsible();
        angular.element(document.querySelector('#IssueResponsiblePopup')).modal('show');
    }

    $scope.IssueResponsibleList = [];
    $scope.getIssueResponsible = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetResponsiblePerson?IssueId=' + $scope.IssueNew.IssueNameId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.IssueResponsibleList = resp.data;
        });
    }

    $scope.doubleIssueResponsible = function (e) {
        $scope.IssueNew.ResponsiblePersonId = e.data.SystemId;
        $scope.IssueNew.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#IssueResponsiblePopup')).modal('hide');
    }

    $scope.closeIssueResponsiblePopUp = function () {
        angular.element(document.querySelector('#IssueResponsiblePopup')).modal('hide');
    }

    $scope.getSalesOrderPopUp = function (data) {
        $scope.NewobjectSO = data.data;
        $scope.getSalesOrder();
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('show');
    }

    $scope.SalesOrderItemList = [];
    $scope.getSalesOrder = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSalesOrder?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.NewobjectSO.WorkCenterMasterId + '&productionLevel=' + $scope.NewobjectSO.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.NewobjectSO.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.SalesOrderItemList = resp.data;
        });
    }
    $scope.BookingLevel = null;
    $scope.SOId = null;
    $scope.selectSalesOrderItem = function (e) {
        $scope.NewobjectSO.SalesOrderId = e.data.SOId;
        $scope.NewobjectSO.SOArticle = e.data.Article;
        $scope.BookingLevel = $scope.NewobjectSO.BookingLevel;
        $scope.SOId = $scope.NewobjectSO.SalesOrderId;
        $scope.GetSalesOrderItemQty();
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('hide');
    }

    $scope.getMasterOrderItemPopUp = function (data) {
        $scope.NewobjectMOI = data.data;
        $scope.getMasterOrderItem();
        angular.element(document.querySelector('#MasterOrderItemPopup')).modal('show');
    }

    $scope.MasterOrderItemList = [];
    $scope.getMasterOrderItem = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMasterOrderItem?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.NewobjectMOI.WorkCenterMasterId + '&productionLevel=' + $scope.NewobjectMOI.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.NewobjectMOI.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.MasterOrderItemList = resp.data;
        });
    }

    $scope.ItemId = null;
    $scope.selectMasterOrderItem = function (e) {
        $scope.NewobjectMOI.MasterOrderItemId = e.data.MasterOrderItemId;
        $scope.NewobjectMOI.MOIArticle = e.data.Article;
        $scope.BookingLevel = $scope.NewobjectMOI.BookingLevel;
        $scope.ItemId = $scope.NewobjectMOI.MasterOrderItemId;
        $scope.GetMasterOrderItemQty();
        angular.element(document.querySelector('#MasterOrderItemPopup')).modal('hide');
    }

    $scope.getProductCodePopUp = function (data) {
        $scope.NewobjectPC = data.data;
        $scope.getProductCode();
        angular.element(document.querySelector('#ProductCodePopup')).modal('show');
    }

    $scope.ProductCodeList = [];
    $scope.getProductCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProductCode?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.NewobjectPC.WorkCenterMasterId + '&productionLevel=' + $scope.NewobjectPC.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.NewobjectPC.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProductCodeList = resp.data;
        });
    }

    $scope.selectProductCode = function (e) {
        $scope.NewobjectPC.MasterOrderItemId = e.data.MOIId;
        $scope.NewobjectPC.ProductCodeArticle = e.data.Article;
        $scope.BookingLevel = $scope.NewobjectPC.BookingLevel;
        $scope.ItemId = $scope.NewobjectPC.MasterOrderItemId;
        $scope.GetProductCodeItemQty();
        angular.element(document.querySelector('#ProductCodePopup')).modal('hide');
    }
}