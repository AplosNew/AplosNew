'use strict';
salaryDisbursementController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function salaryDisbursementController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {

    $scope.path = 'Accounts/SalaryDisbursement/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.SaveSalaryDisbursementUrl = $scope.path + 'Save';
    $scope.Action = 'Salary Disbursement';
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
    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();
    $scope.disbursementAdvice = {
        Id: null, Remarks: null, PaymentMode: null
    };

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.paymentModeList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetEMPPaymentModeEnumCbo/'
    }).then(function successCallback(response) {
        $scope.paymentModeList = response.data;
    });

    $scope.SelectDefaultValue = function (args) {
        var x = new Date();
        x.setDate(10);
        x.setMonth(x.getMonth() - 1);

        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === x.getFullYear().toString()) {
                $scope.year = $scope.yearList[i].Text;
                $scope.month = (x.getMonth() + 1).toString();
                continue;
            }
        }
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };


    $scope.payGroupList = [];
    $scope.payGroupListSelected = [];

    cboService.getPayRollGroupCbo(function (result) {
        $scope.payGroupList = result;
    });

    $scope.create = function (args) {
        $("#checkBox").ejCheckBox({
            change: function (args) {
                var obj = $("#ddlPayRollGroupList").ejDropDownList("instance");
                if (args.isChecked) obj.checkAll();
                else obj.uncheckAll();
            },
            text: "Select All",
            cssClass: "temp"
        });

    };


    $scope.getSalaryProcessIdList = function (args) {
        $scope.isCompletedMonth = 1;

        var DropDownListMonth = $("#ddlMonthList").data("ejDropDownList");
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");

        $scope.month = DropDownListMonth.getSelectedValue();
        $scope.year = DropDownListYear.getSelectedValue();
        if (angular.isUndefinedOrNull($scope.year)) {
            ShowResult("Select Year", 'failure');
        }
        else {
            cboService.getSalaryProcessIdCboByYearMonth($scope.month, $scope.year, $scope.isCompletedMonth, function (result) {
                $scope.cboSalaryProcessIdList = result;
            });
        }


    };

    $scope.selectedPaymentMode = $("#paymentMode option:selected").text();
    $scope.selectedEmployeeCategory = $("#employeeCategoryId option:selected").text();
    $scope.payGroupListSelected = [];
    $scope.EmployeeListTemp = [];
    $scope.EmpNetPayment = [];

    $scope.GetEmployeeInformation = function () {
        $scope.EmployeeListTemp = [];
        var monthName = $scope.monthList.filter(function (mnth) {
            return mnth.Value == $scope.month;
        });
        $scope.effectiveDate = daysInMonth($scope.month, $scope.year) + '-' + monthName[0].Text + '-' + $scope.year;

        if (angular.isUndefinedOrNull($scope.month)) {
            ShowResult("Select Month", 'failure');
        }
        if (angular.isUndefinedOrNull($scope.year)) {
            ShowResult("Select Year", 'failure');
        }
        if (angular.isUndefinedOrNull($scope.disbursementAdvice.PaymentMode)) {
            ShowResult("Select Payment Mode", 'failure');
        }
        else {

            var parameters = {
                'effectiveDate': $scope.effectiveDate, 'salaryProcessId': $scope.salaryProcessId, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity, 'paymentMode': $scope.disbursementAdvice.PaymentMode
                
            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Accounts/SalaryDisbursement/GetEmpInfo',
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
                url: 'Accounts/SalaryDisbursement/ImportData',
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
                    $scope.saveBtnDisable = true;
                    for (var i = 0; i < $scope.SalaryUnDisburseList.length; i++) {
                        $scope.SalaryUnDisburseList[i].CheckBoxSelect = getActive(response.data, $scope.SalaryUnDisburseList[i].EmployeeCode);
                    }
                    $scope.saveBtnDisable = false;
                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    function getActive(list, EmployeeCode) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === EmployeeCode) {
                return true;
            }
        }
        return false;
    }
    $scope.saveBtnDisable = false;
    $scope.SalaryDisbursement = function () {
        try {
            var EmployeeListNew = [];
            for (var i = 0; i < $scope.SalaryUnDisburseList.length; i++) {
                if ($scope.SalaryUnDisburseList[i].CheckBoxSelect) {
                    EmployeeListNew.push($scope.SalaryUnDisburseList[i]);
                }
            }

            if (EmployeeListNew.length == 0) {
                throw "Please Select Employee.";
            }
            //var data = ej.DataManager(EmployeeListNew).executeLocal(ej.Query().select(["EmpSystemId", "PayableVoucherId", "DisbursementVoucherId", "Id", "MonthNo", "YearNo", "Lock", "CheckBoxSelect"]));

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

    // Written By Nitesh
    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    $scope.SalaryUnDisburseList = [];
    $scope.GetSalaryUnDisbursed = function () {
        $scope.SalaryUnDisburseList = [];
        var monthName = $scope.monthList.filter(function (mnth) {
            return mnth.Value == $scope.month;
        });
        $scope.effectiveDate = daysInMonth($scope.month, $scope.year) + '-' + monthName[0].Text + '-' + $scope.year;

        if (angular.isUndefinedOrNull($scope.month)) {
            ShowResult("Select Month", 'failure');
        }
        if (angular.isUndefinedOrNull($scope.year)) {
            ShowResult("Select Year", 'failure');
        }
        else {

            var parameters = {
                'effectiveDate': $scope.effectiveDate, 'salaryProcessId': $scope.salaryProcessId, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity, 'paymentMode': $scope.disbursementAdvice.PaymentMode
            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Accounts/SalaryDisbursement/GetSalaryUnDisbursed',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.empGrid = true;
                    $scope.SalaryUnDisburseList = response.data;
                    $scope.saveBtnDisable = false;
                }
                else {
                    ShowResult("No Data Found", 'failure');
                    $scope.empGrid = false;
                }
                var gridObj = $("#empInfoGrid").data("ejGrid");
                gridObj.windowonresize();
                gridObj.refreshContent(true);
            });
        }
    };

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.summaryfileName = "Salary UnDisbursed.xlsx"
    $scope.XlsSalaryUnDisburseReport = function () {
        var monthName = $scope.monthList.filter(function (mnth) {
            return mnth.Value == $scope.month;
        });
        $scope.effectiveDate = daysInMonth($scope.month, $scope.year) + '-' + monthName[0].Text + '-' + $scope.year;

        if (angular.isUndefinedOrNull($scope.month)) {
            ShowResult("Select Month", 'failure');
        }
        if (angular.isUndefinedOrNull($scope.year)) {
            ShowResult("Select Year", 'failure');
        }
        else {

            var parameters = {
                'effectiveDate': $scope.effectiveDate, 'salaryProcessId': $scope.salaryProcessId, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity, 'paymentMode': $scope.disbursementAdvice.PaymentMode
            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Accounts/SalaryDisbursement/GetEmployeeSalaryUnDisbursed',
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
            for (var i = 0; i < $scope.SalaryUnDisburseList.length; i++) {
                $scope.SalaryUnDisburseList[i].CheckBoxSelect = ChkOrUnchk;
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
    
    $scope.SalaryUnDisbursement = function () {
        try {
            var EmployeeListSalaryUndisbursedNew = [];
            for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
                if ($scope.EmployeeListTemp[i].CheckBoxSelect) {
                    EmployeeListSalaryUndisbursedNew.push($scope.EmployeeListTemp[i]);
                }
            }

            if (EmployeeListSalaryUndisbursedNew.length == 0) {
                throw "Please Select Employee.";
            }

            //if (baseService.arrayLength($scope.SalaryUnDisburseList) > 0) {
            //    angular.forEach($scope.SalaryUnDisburseList, function (a) {

            //        if (a.CheckBoxSelect) {
            //            var ob = {};
            //            ob.Id = null;
            //            ob.EmpSystemId = a.EmpSystemId;
            //            ob.PayableVoucherId = a.PayableVoucherId;
            //            ob.DisbursementVoucherId = a.DisbursementVoucherId;
            //            ob.MonthNo = a.MonthNo;
            //            ob.YearNo = a.YearNo;
            //            ob.Lock = a.Lock;
            //            ob.CheckBoxSelect = a.CheckBoxSelect;
            //            $scope.SalaryUndisbursedTemp.push(ob);
            //            // EmployeeListSalaryUndisbursedNew = {};

            //        }

            //    });
            //}

            //var data = ej.DataManager(EmployeeListSalaryUndisbursedNew).executeLocal(ej.Query().select(["EmpSystemId", "PayableVoucherId", "DisbursementVoucherId", "Id", "MonthNo", "YearNo", "Lock", "CheckBoxSelect"]));

            $scope.$broadcast('show-errors-check-validity');
            $scope.saveBtnDisable = true;
            $http({
                method: 'POST',
                url: $scope.path + 'SaveSalaryUnDisbursed',
                data: { 'EmployeeList': EmployeeListSalaryUndisbursedNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.saveBtnDisable = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetEmployeeInformation();
                    $scope.saveBtnDisable = false;
                    //var gridObj = $("#empInfoGridSalaryUnDisbursed").data("ejGrid");
                    //gridObj.refreshContent();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.XlsSalaryDisbursement = function () {
        var dataList = [];
        var newDataList = [];
        var g = $("#empInfoGrid").data("ejGrid");
        dataList = g.getFilteredRecords();
        var obj = {};

        if (dataList.length == 0) {

            dataList = $scope.EmployeeListTemp;
        }

        for (let i = 0; i < dataList.length; i++) {
            obj.DisbursementAdviceId = dataList[i].DisbursementAdviceId;
            obj.Remarks = dataList[i].Remarks;
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
        $scope.fileName = 'SalaryDisbursement';
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
}



