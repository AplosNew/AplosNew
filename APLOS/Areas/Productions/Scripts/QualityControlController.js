'use strict';
QualityControlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function QualityControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Quality Control";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.productionSummaryes = [];
    $scope.IssueTypeList = [];
    $scope.PeriodCategoryList = [];
    $scope.CriticalLevelLists = [];
    $scope.path = 'Productions/QualityControl/';
    $scope.saveUrlIssue = $scope.path + 'createIssue';
    $scope.saveUrlReason = $scope.path + 'createReason';
    $scope.saveUrlTime = $scope.path + 'createTime';
    $scope.saveUrlQICValue = $scope.path + 'create';
    $scope.saveUrlIssueItem = $scope.path + 'createIssueItem';
    $scope.saveUrlGrade = $scope.path + 'createGrade';
    $scope.saveUrl = $scope.path + 'createQC';
    $scope.saveUrlQP = $scope.path + 'createQP';
    $scope.UpdateUrlQP = $scope.path + 'UpdateQP';
    $scope.saveUrlGI = $scope.path + 'createGI';
    $scope.UpdateUrlGI = $scope.path + 'UpdateGI';
    $scope.saveUrlRepeat = $scope.path + 'createRepeatQC';
    $scope.UpdateUrlQICValue = $scope.path + 'UpdateQIC';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate());
    var CurrentDate = new Date();

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

    $scope.POComplete = {
        Id: null,
        //FromDate: $filter('dateFiltering')(CurrentDate, 'dd-MM-yyyy'),
        //ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        FromDate: null,
        ToDate: null,
        POIssueId: null,
        POId: null
    };
    $scope.POCompleteNew = Object.assign({}, $scope.POComplete);

    $scope.GeneralIssue = {
        ActResponsiblePerson: $window.employeeName,
        ActResponsiblePersonId: $window.employeeId
    };
    $scope.GeneralIssueNew = Object.assign({}, $scope.GeneralIssue);

    $scope.selectActResponsiblePerson = function () {
        $scope.getActResponsiblePerson();
        angular.element(document.querySelector('#ActResponsiblePersonPopup')).modal('show');
    }

    $scope.ActResponsiblePersonList = [];
    $scope.getActResponsiblePerson = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetGIEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ActResponsiblePersonList = resp.data;
        });
    }

    $scope.doubleActResponsiblePerson = function (e) {
        $scope.GeneralIssueNew.ActResponsiblePersonId = e.data.SystemId;
        $scope.GeneralIssueNew.ActResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ActResponsiblePersonPopup')).modal('hide');
        //$scope.ProcessGeneralIssue();
    }

    $scope.closeActResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ActResponsiblePersonPopup')).modal('hide');
    }

    $scope.selectPIResponsiblePerson = function () {
        $scope.getPIResponsiblePerson();
        angular.element(document.querySelector('#PIResponsiblePersonPopup')).modal('show');
    }

    $scope.PIResponsiblePersonList = [];
    $scope.getPIResponsiblePerson = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPIEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PIResponsiblePersonList = resp.data;
        });
    }

    $scope.doublePIResponsiblePerson = function (e) {
        $scope.POIssueNew.PIResponsiblePersonId = e.data.SystemId;
        $scope.POIssueNew.PIResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#PIResponsiblePersonPopup')).modal('hide');
        //$scope.ProcessQualityPlan();
    }

    $scope.closePIResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#PIResponsiblePersonPopup')).modal('hide');
    }

    $scope.POIssue = {
        ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        PIResponsiblePersonId: $window.employeeId,
        PIResponsiblePerson: $window.employeeName,
    };
    $scope.POIssueNew = Object.assign({}, $scope.POIssue);

    $scope.POSummary = {
        Id: null,
        //FromDate: $filter('dateFiltering')(CurrentDate, 'dd-MM-yyyy'),
        //ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        FromDate: null,
        ToDate: null,
        POIssueId: null,
        POId: null
    };
    $scope.POSummaryNew = Object.assign({}, $scope.POSummary);

    $scope.QCCompleteList = [];
    $scope.View = function () {
        try {
            $scope.QCCompleteList = [];
            $http.get('Productions/QualityControl/LoadQCComplete?IssueId=' + $scope.POCompleteNew.POIssueId + '&todate=' + $scope.POCompleteNew.ToDate + '&fromDate=' + $scope.POCompleteNew.FromDate + '&POId=' + $scope.POCompleteNew.POId)
                .then(function (response) {
                    $scope.QCCompleteList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.QCSummaryList = [];
    $scope.SummaryView = function () {
        try {
            $scope.QCSummaryList = [];
            $http.get('Productions/QualityControl/LoadQCSummary?IssueId=' + $scope.POSummaryNew.POIssueId + '&todate=' + $scope.POSummaryNew.ToDate + '&fromDate=' + $scope.POSummaryNew.FromDate + '&POId=' + $scope.POSummaryNew.POId)
                .then(function (response) {
                    $scope.QCSummaryList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.QualityPlanList = [];
    $scope.ProcessQualityPlan = function () {
        try {
            $scope.QualityPlanList = [];
            $http.get('Productions/QualityControl/LoadQualityPlan?POIssueDate=' + $scope.POIssueNew.ToDate + '&ResponsiblePersonId=' + $scope.POIssueNew.PIResponsiblePersonId)
                .then(function (response) {
                    $scope.QualityPlanList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.ProcessQualityPlan();

    $scope.GeneralIssueList = [];
    $scope.ProcessGeneralIssue = function () {
        try {
            $scope.GeneralIssueList = [];
            $http.get('Productions/QualityControl/LoadGeneralIssue?ResponsiblePersonId=' + $scope.GeneralIssueNew.ActResponsiblePersonId)
                .then(function (response) {
                    $scope.GeneralIssueList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.ProcessGeneralIssue();

    $scope.GIClear = function () {
        GIClearFields();
    };

    function GIClearFields() {
        $scope.GeneralIssue = {
            ActResponsiblePersonId: null,
            ActResponsiblePerson: null
        };
        $scope.GeneralIssueNew = Object.assign({}, $scope.GeneralIssue);
    }

    $scope.PIClear = function () {
        PIClearFields();
    };

    function PIClearFields() {
        $scope.POIssue = {
            ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
            PIResponsiblePersonId: null,
            PIResponsiblePerson: null
        };
        $scope.POIssueNew = Object.assign({}, $scope.POIssue);
    }

    $scope.GeneratItemSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetItemAutoSequence'
        }).then(function successCallback(response) {
            $scope.IssueItemNew.SNO = response.data;
        });
    }
    $scope.GeneratItemSequenceNo();

    $scope.GeneratGradeSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetGradeAutoSequence'
        }).then(function successCallback(response) {
            $scope.GradeNew.SNO = response.data;
        });
    }
    $scope.GeneratGradeSequenceNo();

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
        ProductionDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),
        ProductionShiftId: null,
        UOM: null,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        InCharge: null,
        InChargeId: null,
        Remarks: null,
        LotNumber: null,
        PeriodId: null,
        IssueId: null,
        GradeId: null,
        MasterOrderItemId: null,
        SalesOrderId: null,
        Article: null,
        SOArticle: null,
        MOIArticle: null,
        ProductCodeArticle: null,
        BookingLevel: null,
        WorkCenterId: null,
        RepeatEntry: null,
    };
    $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);

    $scope.POWise = {
        EntityId: null,
        ProcessId: null,
        IssueId: null,
        POId: null,
        ToDate: null,
        POStatus: null,
        Customer: null
    };
    $scope.POWiseNew = Object.assign({}, $scope.POWise);

    $scope.PODateValidation = function (ToDate) {
        try {
            if (ToDate < $filter("date")(Date.now(), 'dd-MMM-yyyy')) {
                throw "Date must not be allow Back Date!";
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    };

    $scope.QualityControlDetails = {
        Id: null,
        QCId: null,
        ItemId: null,
        Value: 0,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        Remarks: null,
        ActionToBeTaken,
        GradeId: null
    };
    $scope.QualityControlDetailsNew = Object.assign({}, $scope.QualityControlDetails);

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
        ProcessId: null,
        IssueName: null,
        DepartmentId: null,
        Department: null,
        IssueType: null,
        IssueCategory: null,
        PositionCodeId: null,
        PositionCode: null,
        Remarks: null,
        CheckingInterval: null,
    };
    $scope.IssueNew = Object.assign({}, $scope.Issue);

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

    $scope.ProcessIssueList = [];
    $scope.GetProcessIssueList = function () {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetProcessIssueList'
        }).then(function successCallback(response) {
            $scope.ProcessIssueList = response.data;
        });
    }
    $scope.GetProcessIssueList();

    $scope.POIssueType = null;
    $scope.GetIssueType = function (QId) {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetIssueType?IssueId=' + QId
        }).then(function successCallback(response) {
            $scope.POIssueType = response.data[0].POIssueType;
        });
    }
    $scope.lotList = [];
    $scope.GetQBookingLevel = function () {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetQBookingLevel?ProcessId=' + $scope.productionSummaryNew.ProcessId + '&EntityId=' + $scope.productionSummaryNew.EntityId + '&POId=' + $scope.productionSummaryNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.productionSummaryNew.BookingLevel = response.data.pbl[0].BookingLevel;
            $scope.lotList = response.data.lot;
            if ($scope.lotList.length==1) {
                $scope.productionSummaryNew.LotNumber = $scope.lotList[0].Value;
            }

        });
    }
    $scope.GetQBookingLevel();

    $scope.GetChkInterval = function (QId) {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetChkInterval?IssueId=' + QId
        }).then(function successCallback(response) {
            $scope.IssueItemNew.CheckingInterval = response.data[0].CheckingInterval;
        });
    }

    $scope.ReasonList = [];
    $scope.LoadReasonDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadReasonDetails'
        }).then(function successCallback(response) {
            $scope.ReasonList = response.data;
        }
        )
    }
    $scope.LoadReasonDetails();

    $scope.GradeGridList = [];
    $scope.GetGradeGridList = function () {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetGradeGridList'
        }).then(function successCallback(response) {
            $scope.GradeGridList = response.data;
        });
    }
    $scope.GetGradeGridList();

    $scope.ActionToBeTakenList = [];
    $scope.GetActionToBeTakenGridList = function () {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetActionToBeTakenGridList'
        }).then(function successCallback(response) {
            $scope.ActionToBeTakenList = response.data;
        });
    }
    $scope.GetActionToBeTakenGridList();

    $scope.WorkCenterList = [];
    $scope.GetWorkCenterGridList = function (QIssueId, QEntityId, QProcessId) {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetWorkCenterList?IssueId=' + QIssueId + '&EntityId=' + QEntityId + '&ProcessId=' + QProcessId
        }).then(function successCallback(response) {
            $scope.WorkCenterList = response.data;
        });
    }
    $scope.GetWorkCenterGridList();

    $scope.TimeList = [];
    $scope.LoadTimeDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadTimeDetails'
        }).then(function successCallback(response) {
            $scope.TimeList = response.data;
        }
        )
    }
    $scope.LoadTimeDetails();

    $scope.GetReasonDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadReasonDetailsEditData?ReasonId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ReasonNew = response.data.Reason[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetTimeDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadTimeDetailsEditData?TimeId=' + args.data.Id
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
            url: 'Productions/QualityControl/LoadTimeIssueDetails'
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
            url: 'Productions/QualityControl/LoadIssueItemIssueDetails'
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
        angular.element(document.querySelector('#ItemPositionCodePop')).modal('hide');
    }

    $scope.selectItemPositionCode = function () {
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
        $scope.IssueItemNew.PositionCodeId = e.data.Id;
        $scope.IssueItemNew.PositionCode = e.data.Code;
        angular.element(document.querySelector('#ItemPositionCodePop')).modal('hide');
    }

    $scope.closeIssueItemCodePopUp = function () {
        angular.element(document.querySelector('#ItemPositionCodePop')).modal('hide');
    }

    $scope.IssueReasonList = [];
    $scope.GetIssueReasonList = function () {
        $http({
            method: 'GET',
            url: 'Productions/QualityControl/GetIssueReasonList'
        }).then(function successCallback(response) {
            $scope.IssueReasonList = response.data;
        });
    }
    $scope.GetIssueReasonList();

    $scope.IssueList = [];
    $scope.LoadIssueDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadIssueDetails'
        }).then(function successCallback(response) {
            $scope.IssueList = response.data;
        }
        )
    }
    $scope.LoadIssueDetails();

    $scope.GetIssueDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadIssueDetailsEditData?IssueId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.IssueNew = response.data.Issue[0];
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
    $scope.LoadIssueItemDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadIssueItemDetails'
        }).then(function successCallback(response) {
            $scope.IssueItemList = response.data;
        }
        )
    }
    $scope.LoadIssueItemDetails();

    $scope.GetIssueItemDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadIssueItemDetailsEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.IssueItemNew = response.data.IssueItem[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.IssueItemSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.IssueItemDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlIssueItem,
                data: {
                    'IssueItemData': $scope.IssueItemNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadIssueItemDetails();
                    IssueItemClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.IssueItemClear = function () {
        IssueItemClearFields();
    };

    function IssueItemClearFields() {
        $scope.Action = "Save";
        $scope.IssueItemNew = Object.assign({}, $scope.IssueItem);
    }

    $scope.GradeList = [];
    $scope.LoadGradeDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadGradeDetails'
        }).then(function successCallback(response) {
            $scope.GradeList = response.data;
        }
        )
    }
    $scope.LoadGradeDetails();

    $scope.GetGradeDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/QualityControl/LoadGradeDetailsEditData?GradeId=' + args.data.Id
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
            url: 'Productions/QualityControl/IssueDelete?id=' + $scope.tempIssueId,
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
            url: 'Productions/QualityControl/ReasonDelete?id=' + $scope.tempReasonId,
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
            url: 'Productions/QualityControl/TimeDelete?id=' + $scope.tempTimeId,
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
            url: 'Productions/QualityControl/IssueItemDelete?id=' + $scope.tempIssueItemId,
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
            url: 'Productions/QualityControl/GradeDelete?id=' + $scope.tempGradeId,
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

    $scope.QCId = null;
    $scope.SaveQC = function () {
        try {
            //ValidationPreMaster();
            if (baseService.isUndefinedOrNull($scope.QPId)) {
                throw "Please update plan record and proceed";
            }

            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.EntityId)) {
                throw "Entity is Required";
            }

            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProcessId) || $scope.productionSummaryNew.ProcessId === '') {
                throw "Process is Required";
            }

            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionDate)) {
                throw "Production Date is Required";
            }

            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionShiftId)) {
                throw "Shift is Required";
            }

            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.IssueId)) {
                throw "Issue is Required";
            }

            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.PeriodId)) {
                throw "Period is Required";
            }

            if ($scope.POIssueType === 'Order') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                    throw "PONo is Required";
                }
            }

            if ($scope.productionSummaryNew.BookingLevel === 'SalesOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SOArticle)) {
                    throw "Please select SO Article and Proceed.";
                }
            }
            if ($scope.productionSummaryNew.BookingLevel === 'MasterOrderItem') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.MOIArticle)) {
                    throw "Please select MOI Article and Proceed.";
                }
            }
            if ($scope.productionSummaryNew.BookingLevel === 'ProductCode') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductCodeArticle)) {
                    throw "Please select Product Code Article and Proceed.";
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'QualityControlData': $scope.productionSummaryNew,
                    'QualityPlanId': $scope.QPId,
                    'PlanType': $scope.PlanType,
                    'EntryLevel': $scope.ELevel
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //ShowResult(response.data.Message, 'success');
                }
                //$scope.QPId = null;
                //$scope.PlanType = null;
                $scope.QCId = response.data.Data.Id;
                $scope.NewObject.Id = null;
                $scope.loadWC($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.EntityId, $scope.POItemId);
                if ($scope.PlanType == "GeneralIssue") { $scope.ProcessGeneralIssue(); }
                if ($scope.PlanType == "POIssue") { $scope.ProcessQualityPlan(); }
                $scope.SaveGI();
            }), function errorCallBack(response) {
                /*  ShowResult(response.data.Message, 'failure');*/
            }
        }
        catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveRepeatQC = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.wcList.length; i++) {
                if ($scope.wcList[i].Repeat == true) {
                    $scope.SaveList.push($scope.wcList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlRepeat,
                data: {
                    'QualityControlData': $scope.productionSummaryNew,
                    'DataList': $scope.SaveList,
                    'QualityPlanId': $scope.QPId,
                    'PlanType': $scope.PlanType
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    if ($scope.PlanType == "GeneralIssue") { $scope.ProcessGeneralIssue(); }
                    if ($scope.PlanType == "POIssue") { $scope.ProcessQualityPlan(); }
                    ShowResult(response.data.Message, 'success');
                }
                $scope.QCId = response.data.Data.Id;
            }), function errorCallBack(response) {
                //ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.UpdateQIC = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.wcList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.wcList[i].Value) && !baseService.isUndefinedOrNull($scope.wcList[i].GradeId)) {
                    //if ($scope.wcList[i].IsWorkCenter == true && baseService.isUndefinedOrNull($scope.wcList[i].WorkCenterId)) {
                    //    throw "Please Select WorkCenter and Proceed";
                    //}
                    //else
                    //{ 
                    $scope.SaveList.push($scope.wcList[i]);
                    //    }
                    //}
                    //else
                    //{
                    //        throw "Please enter Value and Proceed";
                }
            }
            $http({
                method: 'POST',
                url: $scope.UpdateUrlQICValue,
                data: {
                    'DataList': $scope.SaveList,
                    'PId': $scope.QCId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.loadWC();
                    $scope.ProcessGeneralIssue();
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveQIC = function (data) {
        try {
            if (baseService.isUndefinedOrNull(data.data.Value)) {
                throw "Please enter Value and proceed";
            }
            if (baseService.isUndefinedOrNull(data.data.GradeId)) {
                throw "Please enter Grade and proceed";
            }
            if (data.data.IsWorkCenter == true && baseService.isUndefinedOrNull(data.data.WorkCenterId)) {
                throw "Please select WorkCenter and Proceed";
            }
            data.data.QCId = $scope.QCId;
            $http({
                method: 'POST',
                url: $scope.saveUrlQICValue,
                data: { 'QualityControlDetailsData': data.data },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.loadWC();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.SaveQP = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.QualityPlanList.length; i++) {
                $scope.SaveList.push($scope.QualityPlanList[i]);
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlQP,
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
                    $scope.ProcessQualityPlan();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.UpdateQP = function (data) {
        try {

            $scope.SaveList = [];
            $scope.SaveList.push(data.data);

            $http({
                method: 'POST',
                url: $scope.UpdateUrlQP,
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
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveGI = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.GeneralIssueList.length; i++) {
                $scope.SaveList.push($scope.GeneralIssueList[i]);
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlGI,
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
                    $scope.ProcessGeneralIssue();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.UpdateGI = function (data) {
        $scope.NewObject = data.data;
        try {

            $scope.SaveList = [];
            $scope.SaveList.push(data.data);

            $http({
                method: 'POST',
                url: $scope.UpdateUrlGI,
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
                    $scope.NewObject.Id = response.data.Id;
                    var gridObj = $("#GridGeneralIssue").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                    /*  $scope.ProcessGeneralIssue();*/
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
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
                $scope.productionSummaryNew.EntityId = $scope.entityList[0].Value;
                //default
                $scope.loadProcessList($scope.productionSummaryNew.EntityId);
            }
        });
    }
    $scope.getAllEntities();

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
                $scope.productionSummaryNew.ProcessId = $scope.processList[0].Value;
                $scope.getProdLevel();
                //default
                $scope.loadWC($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.EntityId, $scope.POItemId);
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

    $scope.IsParameterBased = false;
    $scope.ToCloseAllowed = false;

    $scope.getProdLevel = function () {
        try {
            $scope.PQEnable = false;



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
            $http.get('Productions/QualityControl/GetIssueCboQIC?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&shiftId=' + $scope.productionSummaryNew.ProductionShiftId + '&ProductionInChargeId=' + $scope.productionSummaryNew.ProductionInChargeId + '&IssueId=' + $scope.productionSummaryNew.IssueId + '&PeriodId=' + $scope.productionSummaryNew.PeriodId + '&PId=' + $scope.QCId + '&POItemId=' + $scope.POItemId)
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

    $scope.refreshTemplateRepeat = function (args) {
        $("#Rheadchk").ejCheckBox({ "change": CheckBoxSelectAllRepeat });
    };
    function CheckBoxSelectAllRepeat(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#ProductionSummaryWC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.wcList.length; i++) {
                $scope.wcList[i].Repeat = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Repeat = ChkOrUnchk;
            }
        }
        var gridObj = $("#ProductionSummaryWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.POSelectList = [];
    $scope.GetPOWiseData = function () {
        try {
            $http.get('Productions/QualityControl/GetPOWiseData?processId=' + $scope.POWiseNew.ProcessId + '&entityId=' + $scope.POWiseNew.EntityId + '&IssueId=' + $scope.POWiseNew.IssueId + '&POId=' + $scope.POWiseNew.POId + '&Date=' + $scope.POWiseNew.ToDate + '&POStatus=' + $scope.POWiseNew.POStatus + '&CustomerId=' + $scope.POWiseNew.Customer)
                .then(function (response) {
                    $scope.POSelectList = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    /* $scope.GetPOWiseData();*/

    $scope.rowDataBound = function rowDataBound(e) {

        if (e.data.Date == $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')) {
            e.row.css("background-color", '#FFFF00');
        }
        else if (new Date(e.data.Date) >= new Date()) {

            e.row.css("background-color", '#FFA500');
        }

        else {
            e.row.css("background-color", '#FFFFFF');

        }
    }

    $scope.QProwDataBound = function QProwDataBound(e) {

        //if (e.data.Date == $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')) {
        //    e.row.css("background-color", '#FFFFE0');
        //}
        //else if (new Date(e.data.Date) < new Date()) {

        //    e.row.css("background-color", '#FFD580');
        //}

        //else {
        //    e.row.css("background-color", '#FFFFFF');

        //}
        if (e.data.Days == 0) {
            e.row.css("background-color", '#FFFFE0');
        }
        else {
            e.row.css("background-color", '#FFD580');
        }
    }
    $scope.QGIrowDataBound = function QGIrowDataBound(e) {

        if (e.data.QualityIssueDate == $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')) {
            e.row.css("background-color", '#FFFFE0');
        }
        else if (new Date(e.data.QualityIssueDate) < new Date()) {

            e.row.css("background-color", '#FFD580');
        }

        else {
            e.row.css("background-color", '#FFFFFF');

        }
    }

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
            $scope.SaveQC();
            $scope.SetGo(isdisabled);
            if ($scope.IsParameterBased == true) {
                $scope.IsVisible = false;
            }
            else {
                $scope.IsVisible = true;
            }
            //$scope.loadWC($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.EntityId);
            //$scope.getLineGrid();
        } catch (ex) {
            //ShowResult(ex, 'Info');
        }
    };

    $scope.SaveRepeatRecord = function () {
        $scope.SaveRepeatQC();
    }

    $scope.masterQCGo = function (isdisabled) {
        try {
            $scope.GetPOWiseData($scope.POWiseNew.ProcessId, $scope.POWiseNew.EntityId);
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

    $scope.SetQCBack = function (isdisabled) {
        $scope.IsGo = isdisabled;
        $scope.ClearMasterPart();
        $scope.POSelectList = [];
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
        $http.get('Productions/QualityControl/GetQualityProductionOrderList?entityid=' + $scope.productionSummaryNew.EntityId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ToCloseAllowed=' + $scope.ToCloseAllowed)
            .then(
                function successCallback(response) {
                    $scope.ProductionOrderList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

        angular.element(document.querySelector('#POItemPopup')).modal('show');

    };

    $scope.CompletePOList = [];
    $scope.getCompletePOPopUp = function () {
        $scope.CompletePOList = [];
        $http.get('Productions/QualityControl/GetQualityCompletePOList?IssueId=' + $scope.POCompleteNew.POIssueId)
            .then(
                function successCallback(response) {
                    $scope.CompletePOList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

        angular.element(document.querySelector('#CompletePOPopup')).modal('show');

    };


    $scope.SetPrOData = function ($event) {
        $scope.productionSummaryNew.ProductionOrderId = $event.data.POId;
        $scope.productionSummaryNew.Article = $event.data.Article;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
        $scope.GetQBookingLevel();
    }

    $scope.SetCompletePOData = function ($event) {
        $scope.POCompleteNew.POId = $event.data.POId;
        angular.element(document.querySelector('#CompletePOPopup')).modal('hide');
    }

    $scope.POItemId = null;
    $scope.SetPOSelectData = function ($event) {
        $scope.productionSummaryNew.ProductionOrderId = $event.data.POId;
        $scope.productionSummaryNew.EntityId = $event.data.EntityId;
        $scope.productionSummaryNew.ProcessId = $event.data.ProcessId;
        $scope.productionSummaryNew.ProductionShiftId = $event.data.ProductionShiftId;
        $scope.productionSummaryNew.ProductionDate = $event.data.Date;
        $scope.productionSummaryNew.IssueId = $event.data.IssueId;
        $scope.productionSummaryNew.PeriodId = $event.data.PeriodId;
        $scope.POItemId = $event.data.ItemId;
        $scope.setTab(3);
        $scope.getAllEntities();
        $scope.loadProcessList($scope.productionSummaryNew.EntityId);
        $scope.GetIssueList($scope.productionSummaryNew.ProcessId);
        $scope.GetShiftList();
        $scope.GetPeriodList($scope.productionSummaryNew.IssueId);
        $scope.GetIssueType($scope.productionSummaryNew.IssueId);
        $scope.GetQBookingLevel();
        //$scope.productionSummaryNew.Article = $event.data.Article;
    }

    $scope.QPId = null;
    $scope.PlanType = null;
    $scope.ELevel = null;
    $scope.SetQPSelectData = function ($event) {
        $scope.productionSummaryNew.ProductionOrderId = $event.data.POId;
        $scope.productionSummaryNew.EntityId = $event.data.EntityId;
        $scope.productionSummaryNew.ProcessId = $event.data.ProcessId;
        $scope.productionSummaryNew.ProductionShiftId = $event.data.ProductionShiftId;
        if (baseService.isUndefinedOrNull($event.data.QPDate)) { $scope.productionSummaryNew.ProductionDate = $filter("date")(Date.now(), 'dd-MMM-yyyy'); }
        else { $scope.productionSummaryNew.ProductionDate = $event.data.QPDate; }
        $scope.productionSummaryNew.IssueId = $event.data.IssueId;
        $scope.productionSummaryNew.PeriodId = $event.data.PeriodId;
        $scope.productionSummaryNew.LotNumber = $event.data.LotNumber;
        $scope.productionSummaryNew.ProductionInCharge = $event.data.QPEmployee;
        $scope.productionSummaryNew.ProductionInChargeId = $event.data.QPEmployeeId;
        $scope.productionSummaryNew.WorkCenterId = $event.data.WorkCenterId;
        $scope.WorkCenterHeaderList = [];
        $scope.QPId = null;
        $scope.PlanType = null;
        $scope.ELevel = null;
        $scope.ELevel = $event.data.EntryLevel;
        $scope.QPId = $event.data.Id;
        $scope.PlanType = "POIssue"
        $scope.setTab(3);
        $scope.getAllEntities();
        $scope.loadProcessList($scope.productionSummaryNew.EntityId);
        $scope.GetIssueList($scope.productionSummaryNew.ProcessId);
        $scope.GetShiftList();
        $scope.GetPeriodList($scope.productionSummaryNew.IssueId);
        $scope.GetIssueType($scope.productionSummaryNew.IssueId);
        $scope.GetQBookingLevel();
        $scope.GetWorkCenterList($scope.productionSummaryNew.IssueId, $scope.productionSummaryNew.EntityId, $scope.productionSummaryNew.ProcessId);
        $scope.GetWorkCenterGridList($scope.productionSummaryNew.IssueId, $scope.productionSummaryNew.EntityId, $scope.productionSummaryNew.ProcessId);
        //$scope.productionSummaryNew.Article = $event.data.Article;
    }

    $scope.NewObject = { Id: null };
    $scope.SetQGISelectData = function ($event) {
        try {
            if (baseService.isUndefinedOrNull($event.data.Id)) {
                throw "Please save record and proceed";
            }
            $scope.productionSummaryNew.EntityId = $event.data.EntityId;
            $scope.productionSummaryNew.ProcessId = $event.data.ProcessId;
            if (baseService.isUndefinedOrNull($event.data.QualityIssueDate)) { $scope.productionSummaryNew.ProductionDate = $filter("date")(Date.now(), 'dd-MMM-yyyy'); }
            else { $scope.productionSummaryNew.ProductionDate = $event.data.QualityIssueDate; }
            $scope.productionSummaryNew.IssueId = $event.data.IssueId;
            $scope.productionSummaryNew.ProductionInCharge = $event.data.QGIEmployee;
            $scope.productionSummaryNew.ProductionInChargeId = $event.data.QGIEmployeeId;
            $scope.productionSummaryNew.WorkCenterId = $event.data.WorkCenterId;
            $scope.WorkCenterHeaderList = [];
            $scope.QPId = null;
            $scope.PlanType = null;
            //$scope.QPId = $scope.NewObject.Id;
            $scope.QPId = $event.data.Id;
            $scope.PlanType = "GeneralIssue"
            $scope.setTab(3);
            $scope.getAllEntities();
            $scope.loadProcessList($scope.productionSummaryNew.EntityId);
            $scope.GetIssueList($scope.productionSummaryNew.ProcessId);
            $scope.GetShiftList();
            $scope.GetPeriodList($scope.productionSummaryNew.IssueId);
            $scope.GetIssueType($scope.productionSummaryNew.IssueId);
            $scope.GetQBookingLevel();
            $scope.GetWorkCenterList($scope.productionSummaryNew.IssueId, $scope.productionSummaryNew.EntityId, $scope.productionSummaryNew.ProcessId);
            $scope.GetWorkCenterGridList($scope.productionSummaryNew.IssueId, $scope.productionSummaryNew.EntityId, $scope.productionSummaryNew.ProcessId);
            //$scope.productionSummaryNew.Article = $event.data.Article;
        }
        catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.SetQCCompleteSelectData = function ($event) {
        $scope.productionSummaryNew.Id = $event.data.TransactionHeaderId;
        $scope.productionSummaryNew.ProductionOrderId = $event.data.PONo;
        $scope.productionSummaryNew.EntityId = $event.data.EntityId;
        $scope.productionSummaryNew.ProcessId = $event.data.ProcessId;
        $scope.productionSummaryNew.ProductionShiftId = $event.data.ProductionShiftId;
        $scope.productionSummaryNew.ProductionDate = $event.data.ActualDate;
        $scope.productionSummaryNew.IssueId = $event.data.IssueId;
        $scope.productionSummaryNew.PeriodId = $event.data.PeriodId;
        $scope.productionSummaryNew.LotNumber = $event.data.LotNumber;
        $scope.productionSummaryNew.ProductionInCharge = $event.data.CheckedBy;
        $scope.productionSummaryNew.ProductionInChargeId = $event.data.ProductionInchargeId;
        $scope.productionSummaryNew.Remarks = $event.data.IssueRemarks;
        $scope.productionSummaryNew.RepeatEntry = $event.data.RepeatEntry;
        $scope.productionSummaryNew.WorkCenterId = $event.data.WorkCenterId;
        $scope.WorkCenterHeaderList = [];
        $scope.QPId = $event.data.QualityPlanId;
        $scope.PlanType = $event.data.PlanType;
        $scope.QCId = $event.data.TransactionHeaderId;
        $scope.setTab(3);
        $scope.getAllEntities();
        $scope.loadProcessList($scope.productionSummaryNew.EntityId);
        $scope.GetIssueList($scope.productionSummaryNew.ProcessId);
        $scope.GetShiftList();
        $scope.GetPeriodList($scope.productionSummaryNew.IssueId);
        $scope.GetIssueType($scope.productionSummaryNew.IssueId);
        $scope.GetQBookingLevel();
        $scope.GetWorkCenterList($scope.productionSummaryNew.IssueId, $scope.productionSummaryNew.EntityId, $scope.productionSummaryNew.ProcessId);
        $scope.loadWC();
        $scope.GetWorkCenterGridList($scope.productionSummaryNew.IssueId, $scope.productionSummaryNew.EntityId, $scope.productionSummaryNew.ProcessId);
        //$scope.productionSummaryNew.Article = $event.data.Article;
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
        $scope.POWiseNew.POId = null;
        $scope.POWiseNew.ProcessId = null;
        $scope.POWiseNew.EntityId = null;
        $scope.POWiseNew.IssueId = null;
        $scope.POWiseNew.ToDate = null;
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
                url: 'Productions/QualityControl/DeleteMasterWC?id=' + master.data.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < $scope.wcList.length; i++) {
                        if ($scope.wcList[i].Id == master.data.Id) {
                            $scope.wcList[i].Id = null;
                            break;
                        }
                    }
                    $scope.loadWC();
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
        /* $scope.Action = "Save";*/
        //$scope.productionSummary = {};
        //$scope.productionSummaryNew = {};
        //$scope.productionSummaryNew.Active = true;
        $scope.productionSummaryNew.ProductionDate = $filter("date")(Date.now(), 'dd-MMM-yyyy');
        $scope.POItemId = null;
        //$scope.ProdQtyCount = 0;
        //$scope.TotalProductionBookingQty = 0;
        //$scope.TotalSalesOrderQty = 0;
        //$scope.RemainQty = 0;
        $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);
        $scope.SetBack(false);
        $scope.IsGo = false;
        /*        $scope.ProductionSummaryDetail = [];*/
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
        $http.get('Productions/QualityControl/GetQualityIssueList?processId=' + PId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.IssueHeaderList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.productionSummaryNew.IssueId = $scope.IssueHeaderList[0].Value;
                    }
                }
            });
    }

    $scope.WorkCenterHeaderList = [];
    $scope.GetWorkCenterList = function (IId, EId, PId) {
        $http.get('Productions/QualityControl/GetQualityWorkCenterList?IssueId=' + IId + '&EntityId=' + EId + '&ProcessId=' + PId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.WorkCenterHeaderList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.productionSummaryNew.WorkCenterId = $scope.WorkCenterHeaderList[0].Value;
                    }
                }
            });
    }

    $scope.POCompleteIssueList = [];
    $scope.GetPOCompleteIssueList = function () {
        $http.get('Productions/QualityControl/GetPOCompleteIssueList')
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.POCompleteIssueList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.POCompleteNew.POIssueId = $scope.POCompleteIssueList[0].Value;
                    }
                }
            });
    }
    $scope.GetPOCompleteIssueList();

    $scope.POSummaryIssueList = [];
    $scope.GetPOSummaryIssueList = function () {
        $http.get('Productions/QualityControl/GetPOCompleteIssueList')
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.POSummaryIssueList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.POSumamryNew.POIssueId = $scope.POSummaryIssueList[0].Value;
                    }
                }
            });
    }
    $scope.GetPOSummaryIssueList();

    $scope.POList = [];
    $scope.GetPOList = function (IId) {
        $http.get('Productions/QualityControl/GetPOList?IssueId=' + IId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.POList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.POWiseNew.POId = $scope.POList[0].Value;
                    }
                }
            });
    }

    $scope.POCompletePONoList = [];
    $scope.GetPOCompleteList = function (IId) {
        $scope.POCompletePONoList = [];
        $http.get('Productions/QualityControl/GetPOCompleteList?IssueId=' + IId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.POCompletePONoList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.POCompleteNew.POId = $scope.POCompletePONoList[0].Value;
                    }
                }
            });
    }

    $scope.POSummaryPONoList = [];
    $scope.GetPOSummaryList = function (IId) {
        $scope.POSummaryPONoList = [];
        $http.get('Productions/QualityControl/GetPOCompleteList?IssueId=' + IId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.POSummaryPONoList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.POSummaryNew.POId = $scope.POSummaryPONoList[0].Value;
                    }
                }
            });
    }

    $scope.PeriodHeaderList = [];
    $scope.GetPeriodList = function (PId) {
        $scope.PeriodHeaderList = null;
        $http.get('Productions/QualityControl/GetQualityPeriodList?IssueId=' + PId)
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

    $scope.selectQPEmployee = function (data) {
        $scope.Newobject = data.data;
        $scope.getQPEmployee();
        angular.element(document.querySelector('#QualityPlanEmployee')).modal('show');
    }

    $scope.QPEmployeeList = [];
    $scope.getQPEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetQPEmployee?IssueId=' + $scope.Newobject.IssueId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.QPEmployeeList = resp.data;
        });
    }

    $scope.doubleQPEmployee = function (e) {
        $scope.Newobject.QPEmployeeId = e.data.SystemId;
        $scope.Newobject.QPEmployee = e.data.EmployeeName;
        angular.element(document.querySelector('#QualityPlanEmployee')).modal('hide');
    }

    $scope.closeQualityPlanEmployee = function () {
        angular.element(document.querySelector('#QualityPlanEmployee')).modal('hide');
    }

    $scope.selectQGIEmployee = function (data) {
        $scope.Newobject = data.data;
        $scope.getQGIEmployee();
        angular.element(document.querySelector('#QualityGIEmployee')).modal('show');
    }

    $scope.QGIEmployeeList = [];
    $scope.getQGIEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetQPEmployee?IssueId=' + $scope.Newobject.IssueId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.QGIEmployeeList = resp.data;
        });
    }

    $scope.doubleQGIEmployee = function (e) {
        $scope.Newobject.QGIEmployeeId = e.data.SystemId;
        $scope.Newobject.QGIEmployee = e.data.EmployeeName;
        angular.element(document.querySelector('#QualityGIEmployee')).modal('hide');
    }

    $scope.closeQualityGIEmployee = function () {
        angular.element(document.querySelector('#QualityGIEmployee')).modal('hide');
    }

    $scope.getSalesOrderPopUp = function () {
        $scope.getSalesOrder();
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('show');
    }

    $scope.SalesOrderItemList = [];
    $scope.getSalesOrder = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSalesOrder?entityid=' + $scope.productionSummaryNew.EntityId + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.SalesOrderItemList = resp.data;
        });
    }

    $scope.selectSalesOrderItem = function (e) {
        $scope.productionSummaryNew.SalesOrderId = e.data.SOId;
        $scope.productionSummaryNew.SOArticle = e.data.Article;
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('hide');
    }

    $scope.getMasterOrderItemPopUp = function () {
        $scope.getMasterOrderItem();
        angular.element(document.querySelector('#MasterOrderItemPopup')).modal('show');
    }

    $scope.MasterOrderItemList = [];
    $scope.getMasterOrderItem = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMasterOrderItem?entityid=' + $scope.productionSummaryNew.EntityId + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.MasterOrderItemList = resp.data;
        });
    }

    $scope.ItemId = null;
    $scope.selectMasterOrderItem = function (e) {
        $scope.productionSummaryNew.MasterOrderItemId = e.data.MasterOrderItemId;
        $scope.productionSummaryNew.MOIArticle = e.data.Article;
        angular.element(document.querySelector('#MasterOrderItemPopup')).modal('hide');
    }

    $scope.getProductCodePopUp = function () {
        $scope.getProductCode();
        angular.element(document.querySelector('#ProductCodePopup')).modal('show');
    }

    $scope.ProductCodeList = [];
    $scope.getProductCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProductCode?entityid=' + $scope.productionSummaryNew.EntityId + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProductCodeList = resp.data;
        });
    }

    $scope.selectProductCode = function (e) {
        $scope.productionSummaryNew.MasterOrderItemId = e.data.MOIId;
        $scope.productionSummaryNew.ProductCodeArticle = e.data.Article;
        angular.element(document.querySelector('#ProductCodePopup')).modal('hide');
    }

    $scope.QCCompleteReport = function () {
        var dataList = [];
        var g = $("#GridQCComplete").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.QCCompleteList;
        }

        $scope.fileName = "Quality Control Completed Issue";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.QCSummaryReport = function () {
        var dataList = [];
        var g = $("#GridQCSummary").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.QCSummaryList;
        }

        $scope.fileName = "Quality Control Summary";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}