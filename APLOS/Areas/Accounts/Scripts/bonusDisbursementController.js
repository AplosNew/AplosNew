'use strict';
bonusDisbursementController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function bonusDisbursementController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {

    $scope.path = 'Accounts/SalaryDisbursement/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.SaveSalaryDisbursementUrl = $scope.path + 'SaveBonus';
    $scope.Action = 'Bonus Disbursement';
    $scope.paymentMode = null;
    $scope.sheetType = false;
    $scope.cboSalaryProcessIdList = [];
    $scope.month = "";
    $scope.year = "";
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;
    $scope.isActive = true;
    $scope.isSeperated = false;
    $scope.isMaternity = false;
    $scope.isManualFilter = false;
    $scope.empGrid = false;
    
    $scope.disbursementAdvice = {
        Id: null,
        Remarks: null,
        PaymentMode: null,
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
    };

    $scope.paymentModeList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetEMPPaymentModeEnumCbo/'
    }).then(function successCallback(response) {
        $scope.paymentModeList = response.data;
    });

    $scope.selectedPaymentMode = $("#paymentMode option:selected").text();
    $scope.selectedEmployeeCategory = $("#employeeCategoryId option:selected").text();
    $scope.payGroupListSelected = [];
    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];
    $scope.EmpNetPayment = [];

    $scope.GetEmployeeInformation = function () {
        $scope.EmployeeList = [];
        $scope.EmployeeListDefault = [];
        $scope.EmployeeListTemp = [];
        if (angular.isUndefinedOrNull($scope.disbursementAdvice.PaymentMode)) {
            ShowResult("Select Payment Mode", 'failure');
        }
        else if (baseService.isUndefinedOrNull($scope.disbursementAdvice.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.disbursementAdvice.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.disbursementAdvice.FromDate) > new Date($scope.disbursementAdvice.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.disbursementAdvice.ToDate) < new Date($scope.disbursementAdvice.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {

            var parameters = {
                'fromDate': $scope.disbursementAdvice.FromDate, 'toDate': $scope.disbursementAdvice.ToDate, 'salaryProcessId': $scope.salaryProcessId, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity, 'paymentMode': $scope.disbursementAdvice.PaymentMode

            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Accounts/SalaryDisbursement/GetEmpInfoBonusDisbursement',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.empdata.length > 0) {
                    $scope.EmployeeListTemp = response.data.empdata;

                }

                $scope.GetSalaryUnDisbursed();
                var gridObj = $("#empInfoGrid").data("ejGrid");
                gridObj.windowonresize();
                gridObj.refreshContent(true);
            });
        }
    };


    function daysInMonth(month, year) {
        return new Date(year, month, 0).getDate();
    }

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#empInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
                $scope.EmployeeListTemp[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#empInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.ModelNew = { FileName: null };
    $scope.ImportData = function () {
        try {
            $scope.msg = "";

            var picData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.picdata)) {
                $scope.ModelNew.FileName = $scope.picdata.name;
            } else {
                throw "Please select File.";
            }


            $http({
                method: 'POST',
                url: 'Accounts/SalaryDisbursement/ImportBonusData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    picData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                        picData.append('file', data.file);
                    }
                    return picData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {

                    for (var i = 0; i < $scope.BonusUnDisburseList.length; i++) {
                        $scope.BonusUnDisburseList[i].CheckBoxSelect = getActive(response.data, $scope.BonusUnDisburseList[i].Id);
                    }
                    $scope.ShowSaveBtn = true;
                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    function getActive(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    $scope.saveBtnDisable = false;
    $scope.SalaryDisbursement = function () {
        try {
            var EmployeeListNew = [];
            for (var i = 0; i < $scope.BonusUnDisburseList.length; i++) {
                if ($scope.BonusUnDisburseList[i].CheckBoxSelect) {
                    EmployeeListNew.push($scope.BonusUnDisburseList[i]);
                }
            }

            if (EmployeeListNew.length == 0) {
                throw "Please Select Employee.";
            }
            
            $scope.$broadcast('show-errors-check-validity');
            $scope.saveBtnDisable = true;
            $http({
                method: 'POST',
                url: $scope.SaveSalaryDisbursementUrl,
                data: { 'DisbursementAdvice': $scope.disbursementAdvice, 'EmployeeList': EmployeeListNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.saveBtnDisable = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.disbursementAdvice.Remarks = null;

                    $scope.GetEmployeeInformation();

                    var gridObj = $("#empInfoGrid").data("ejGrid");
                    gridObj.refreshContent();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

   
    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    $scope.BonusUnDisburseList = [];
    $scope.GetSalaryUnDisbursed = function () {
        $scope.BonusUnDisburseList = [];
        if (angular.isUndefinedOrNull($scope.disbursementAdvice.PaymentMode)) {
            ShowResult("Select Payment Mode", 'failure');
        }
        else if (baseService.isUndefinedOrNull($scope.disbursementAdvice.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.disbursementAdvice.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.disbursementAdvice.FromDate) > new Date($scope.disbursementAdvice.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.disbursementAdvice.ToDate) < new Date($scope.disbursementAdvice.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {

            var parameters = {
                'fromDate': $scope.disbursementAdvice.FromDate, 'toDate': $scope.disbursementAdvice.ToDate, 'salaryProcessId': $scope.salaryProcessId, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity, 'paymentMode': $scope.disbursementAdvice.PaymentMode

            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Accounts/SalaryDisbursement/GetBonusUnDisbursed',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.BonusUnDisburseList = response.data;
                    $scope.saveBtnDisable = false;
                    $scope.empGrid = true;
                }
                else {
                    $scope.empGrid = false;
                    ShowResult("No Data Found", 'failure');
                   
                }
                var gridObj = $("#empInfoGrid").data("ejGrid");
                gridObj.windowonresize();
                gridObj.refreshContent(true);
            });
        }
    };

    $scope.refreshTemplateSalaryUnDisbursed = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectSalaryUnDisbursed });
    };

    function CheckBoxSelectSalaryUnDisbursed(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#empInfoGridSalaryUnDisbursed").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.BonusUnDisburseList.length; i++) {
                $scope.BonusUnDisburseList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#empInfoGridSalaryUnDisbursed").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.BonusUnDisbursedReportExcel = function () {
        var dataListUnDisbursed = [];
        var newDataListUnDisbursed = [];
        var gUnDisbursed = $("#empInfoGridSalaryUnDisbursed").data("ejGrid");
        dataListUnDisbursed = gUnDisbursed.getFilteredRecords();
        var obj = {};

        if (dataListUnDisbursed.length == 0) {

            dataListUnDisbursed = $scope.BonusUnDisburseList;
        }

        for (let i = 0; i < dataListUnDisbursed.length; i++) {
            obj.YearNo = dataListUnDisbursed[i].YearNo;
            obj.MonthName = dataListUnDisbursed[i].MonthName;
            obj.Id = dataListUnDisbursed[i].Id;
            obj.SalaryProcId = dataListUnDisbursed[i].SalaryProcId;
            obj.EmployeeCode = dataListUnDisbursed[i].EmployeeCode;
            obj.EmployeeName = dataListUnDisbursed[i].EmployeeName;
            obj.Designation = dataListUnDisbursed[i].Designation;
            obj.Department = dataListUnDisbursed[i].Department;
            obj.Division = dataListUnDisbursed[i].Division;
            obj.EmployeeCategory = dataListUnDisbursed[i].EmployeeCategory;
            obj.Plant = dataListUnDisbursed[i].Plant;
            obj.Section = dataListUnDisbursed[i].Section;
            obj.SubSection = dataListUnDisbursed[i].SubSection;
            obj.Unit = dataListUnDisbursed[i].Unit;
            obj.DOJ = dataListUnDisbursed[i].DOJ;
            obj.DOS = dataListUnDisbursed[i].DOS;
            obj.CurrentMonthEmployeeStatus = dataListUnDisbursed[i].CurrentMonthEmployeeStatus;
            obj.EmployeeStatus = dataListUnDisbursed[i].EmployeeStatus;
            obj.AccountsGroup = dataListUnDisbursed[i].AccountsGroup;
            obj.SalaryProcFlag = dataListUnDisbursed[i].SalaryProcFlag;
            obj.PayRollGroup = dataListUnDisbursed[i].PayRollGroup;
            obj.JobLocation = dataListUnDisbursed[i].JobLocation;
            obj.PaymentMode = dataListUnDisbursed[i].PaymentMode;
            obj.BankName = dataListUnDisbursed[i].BankName;
            obj.VoucherNo = dataListUnDisbursed[i].VoucherNo;
            obj.PayableVoucherNo = dataListUnDisbursed[i].PayableVoucherNo;
            obj.DisbursementVoucherNo = dataListUnDisbursed[i].DisbursementVoucherNo;
            obj.IsLock = dataListUnDisbursed[i].IsLock;
            obj.IsDisburse = dataListUnDisbursed[i].IsDisburse;
            obj.AddedBy = dataListUnDisbursed[i].AddedBy;
            obj.NetPayment = dataListUnDisbursed[i].NetPayment;
            newDataListUnDisbursed.push(obj);
            obj = {};
        }
        $scope.fileName = 'BonusUnDisbursed';
        $http({
            method: "POST",
            url: $scope.exportgriddataUrl,
            data: {
                'data': newDataListUnDisbursed,
                'reportFileName': $scope.fileName,

            },

            dataType: 'JSON',

        })
            .then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
    $scope.BonusDisbursementReportExcel = function () {
        var dataList = [];
        var newDataList = [];
        var g = $("#empInfoGrid").data("ejGrid");
        dataList = g.getFilteredRecords();
        var obj = {};

        if (dataList.length == 0) {

            dataList = $scope.EmployeeListTemp;
        }

        for (let i = 0; i < dataList.length; i++) {
            obj.YearNo = dataList[i].YearNo;
            obj.MonthName = dataList[i].MonthName;
            obj.BonusDisbursementAdviceId = dataList[i].BonusDisbursementAdviceId;
            obj.Remarks = dataList[i].Remarks;
            obj.Id = dataList[i].Id;
            obj.SalaryProcId = dataList[i].SalaryProcId;
            obj.AddedBy = dataList[i].AddedBy;
            obj.EmployeeCode = dataList[i].EmployeeCode;
            obj.EmployeeName = dataList[i].EmployeeName;
            obj.Designation = dataList[i].Designation;
            obj.Department = dataList[i].Department;
            obj.Division = dataList[i].Division;
            obj.EmployeeCategory = dataList[i].EmployeeCategory;
            obj.Plant = dataList[i].Plant;
            obj.Section = dataList[i].Section;
            obj.SubSection = dataList[i].SubSection;
            obj.Unit = dataList[i].Unit;
            obj.DOJ = dataList[i].DOJ;
            obj.DOS = dataList[i].DOS;
            obj.CurrentMonthEmployeeStatus = dataList[i].CurrentMonthEmployeeStatus;
            obj.EmployeeStatus = dataList[i].EmployeeStatus;
            obj.AccountsGroup = dataList[i].AccountsGroup;
            obj.SalaryProcFlag = dataList[i].SalaryProcFlag;
            obj.PayRollGroup = dataList[i].PayRollGroup;
            obj.JobLocation = dataList[i].JobLocation;
            obj.PaymentMode = dataList[i].PaymentMode;
            obj.BankName = dataList[i].BankName;
            obj.VoucherNo = dataList[i].VoucherNo;
            obj.PayableVoucherNo = dataList[i].PayableVoucherNo;
            obj.DisbursementVoucherNo = dataList[i].DisbursementVoucherNo;
            obj.IsLock = dataList[i].IsLock;
            obj.IsDisburse = dataList[i].IsDisburse;
            obj.NetPayment = dataList[i].NetPayment;
            newDataList.push(obj);
            obj = {};
        }
        $scope.fileName = 'BonusDisbursement';
        $http({
            method: "POST",
            url: $scope.exportgriddataUrl,
            data: {
                'data': newDataList,
                'reportFileName': $scope.fileName,

            },

            dataType: 'JSON',

        })
            .then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.BonusDisbursementSummaryReportExcel = function () {
        $scope.summaryfileName = "Bonus Disbursement Summary.xlsx"
        var parameters = {
            'fromDate': $scope.disbursementAdvice.FromDate, 'toDate': $scope.disbursementAdvice.ToDate, 'salaryProcessId': $scope.salaryProcessId, 'isActive': $scope.isActive,
            'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity, 'paymentMode': $scope.disbursementAdvice.PaymentMode
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Accounts/SalaryDisbursement/GetEmployeeBonusDisbursementSummary',
            data: parameters
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.summaryfileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
    };
    $scope.BonusUnDisbursementSummaryReportExcel = function () {
        $scope.summaryfileName = "Bonus UnDisbursement Summary.xlsx"
        var parameters = {
            'fromDate': $scope.disbursementAdvice.FromDate, 'toDate': $scope.disbursementAdvice.ToDate, 'salaryProcessId': $scope.salaryProcessId, 'isActive': $scope.isActive,
            'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity, 'paymentMode': $scope.disbursementAdvice.PaymentMode
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Accounts/SalaryDisbursement/GetEmployeeBonusUnDisbursementSummary',
            data: parameters
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.summaryfileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
    };
}