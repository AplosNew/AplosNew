'use strict';
PayableCreationAndWorkerAdvanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller", "$window"];
function PayableCreationAndWorkerAdvanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $window) {
    $rootScope.title = 'Payable Creation & Multiple Employee advance';
    $rootScope.titleTab1 = 'Payable Creation';
    $rootScope.titleTab2 = 'Multiple Employee Payment';
    $rootScope.titleTab3 = 'Good Work';
    $rootScope.titleTab4 = 'Extra OT';
    $scope.WorkerAdvanceList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.saveUrl = $scope.path + 'CreateWorkerAdvance';
    $scope.savePCUrl = $scope.path + 'PayableCreationSave';
    $scope.UpdateUrl = $scope.path + 'UpdateGoodWorkDetailEdit';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    baseService.init($scope.getListUrl);
    //$scope.LoadEmpListUrl = $scope.path + 'LoadPCAACEmployeelist';
    $scope.Action = 'Save';
    $scope.PCAction = 'Save';
    $scope.PCOTAction = 'Save';
    $scope.passwordShow = true;
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $scope.getVoucherListUrl = $scope.path + "GetGoodWorkPaymentAdviseDisbursementVoucherList";
    baseService.init($scope.getVoucherListUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.goodWorkAdviseDisbursementVoucherList = [];
    $scope.getVoucherData = function (pageno) {
        baseService.pagination(pageno, $scope.parameters)
            .then(function (result) {
                $scope.goodWorkAdviseDisbursementVoucherList = result.Rows;
                $scope.parameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getVoucherData();

    $scope.searchByList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        },
        {
            "name": "Payment Advise Id",
            "value": "PaymentAdviseId"
        },
        {
            "name": "Payment Bank",
            "value": "PaymentBank"
        },
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab2) {
        $scope.tab2 = newTab2;
    };

    $scope.isSet2 = function (tabNum2) {
        return $scope.tab2 === tabNum2;
    };
    //***********************************Worker Advance Start ********************************************************//
    $scope.ModelTemp = {
        Id: null,
        YearNo: null,
        MonthNo: null,
        FromDate: null,
        ToDate: null,
        UserRef: null,
        NoOfDays: null,
        Percentage: 0,
        CheckedBy: null,
        CheckedById: null,
        ApprovedBy: null,
        ApprovedById: null,
        PreparedBy: null,
        PreparedById: null,
        Remarks: null,
        PayDaysType: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelWADTemp = {
        Id: null,
        WorkerAdvanceId: null,
        EmpSystemId: null,
        EmployeeCode: null,
        EmployeeName: null,
        PayDays: 0,
        RatePerDay: null,
        RatePerHour: null,
        Amount: null,
        AdvanceGiven: 0,
        NetPayable: null,
    };
    $scope.ModelWADNew = Object.assign({}, $scope.ModelWADTemp);


    $scope.yearlist = [];
    $scope.GetCbo = function () {
        $http.get('Attendances/AttendanceProcessUI/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.yearlist = [];
                        $scope.yearlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();

    $scope.ModelNew.YearNo = new Date().getFullYear().toString();

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
    $scope.ModelNew.MonthNo = (new Date().getMonth() + 1).toString();

    $scope.CalenderFunc = function () {
        //$scope._firstDay = null;
        //$scope._lastDay = null;

        $scope._firstDay = $filter('dateFiltering')(new Date($scope.ModelNew.YearNo, $scope.ModelNew.MonthNo - 1, 1), 'dd-MM-yyyy');
        $scope._lastDay = $filter('dateFiltering')(new Date($scope.ModelNew.YearNo, $scope.ModelNew.MonthNo, 0), 'dd-MM-yyyy');

        $('.datepic').datepicker({
            startDate: $scope._firstDay,
            endDate: $scope._lastDay,
            datesDisabled: $scope.DisabledDates,
            format: 'dd-MM-yyyy',
            todayHighlight: true,
            autoclose: true,
            inline: true,
            changeMonth: true
        });

    };
    $scope.CalenderFunc();



    $scope.removeRow = function (data) {
        $scope.empSystemId = data.SystemId;
        $scope.Id = data.Id;
        if (baseService.isUndefinedOrNull(data.EmployeeName))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.EmployeeName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteRow = function () {
        if ($scope.Id == "") {
            var tempData = $scope.EmployeeMainList;
            for (var i = 0; i < tempData.length; i++) {
                if (tempData[i].SystemId === $scope.empSystemId) {
                    $scope.EmployeeMainList.splice(i, 1);
                }
            }
            $scope.Id = null;
            tempData = [];
        }
        else {
            $http({
                method: 'GET',
                url: 'Attendances/GoodWork/DeleteWorkerAdvanceChildUrl?Id=' + $scope.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetWorkerAdvanceDetailCenter();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.Save = function () {
        try {
            //$scope.FD = $filter('dateFiltering')(new Date($scope.ModelNew.FromDate), 'dd-MM-yyyy');
            //$scope.TD = $filter('dateFiltering')(new Date($scope.ModelNew.ToDate), 'dd-MM-yyyy');

            //$scope.$broadcast('show-errors-check-validity');
            //if ($scope._firstDay == $scope.FD || $scope._lastDay == $scope.TD) {
            //    ShowResult('You can not select 1st Date & Last Date of the Month!', 'failure');
            //    return false;
            //}
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, 'workerAdvanceDetail': $scope.EmployeeMainList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.EmployeeMainList = [];
        $scope.ModelNew.YearNo = new Date().getFullYear().toString();
        $scope.ModelNew.MonthNo = (new Date().getMonth() + 1).toString();
        return true;
    };


    $scope.getData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetWorkerAdvanceList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.WorkerAdvanceList = response.data;
        });
    }
    $scope.getData();


    $scope.GetDblClick = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.YearNo = $scope.ModelNew.YearNo.toString();
        $scope.ModelNew.MonthNo = $scope.ModelNew.MonthNo.toString();

        $scope.GetWorkerAdvanceDetailCenter();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.EmployeeMainList = [];
    $scope.GetWorkerAdvanceDetailCenter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetWorkerAdvanceDetailCenter?workAdvanceId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeMainList = resp.data;
        });
    }

    $scope.popUpDataList = [];
    $scope.showByWhomEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/leaveApplication/getemployeelist'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        var data = arg.data;
        if ($scope.Name === 'PB') {
            $scope.ModelNew.PreparedById = data.SystemID;
            $scope.ModelNew.PreparedBy = data.EmployeeName;
        }
        else if ($scope.Name === 'OT') {
            $scope.ModelOTNew.ByWhomId = data.SystemID;
            $scope.ModelOTNew.ByWhom = data.EmployeeName;
        }
        else {
            $scope.ModelPCNew.ByWhomId = data.SystemID;
            $scope.ModelPCNew.ByWhom = data.EmployeeName;
        }
        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.EmployeeList = [];
    $scope.EmployeeMainList = [];
    $scope.getEmploymeeList = function () {

        if ($scope.ModelNew.FromDate === "" || $scope.ModelNew.FromDate === null || $scope.ModelNew.FromDate === undefined) {
            ShowResult('Select Work Date', 'failure');
            return false;
        }
        if ($scope.ModelNew.PayDaysType === "" || $scope.ModelNew.PayDaysType === null || $scope.ModelNew.PayDaysType === undefined) {
            ShowResult('Select From Pay Days', 'failure');
            return false;
        }
        $scope.FD = $filter('dateFiltering')(new Date($scope.ModelNew.FromDate), 'dd-MM-yyyy');
        $scope.TD = $filter('dateFiltering')(new Date($scope.ModelNew.ToDate), 'dd-MM-yyyy');

        //$scope.$broadcast('show-errors-check-validity');
        //if ($scope._firstDay == $scope.FD) {
        //    ShowResult('You can not select 1st Date!', 'failure');
        //    return false;
        //}
        //if ($scope._lastDay == $scope.TD) {
        //    ShowResult('You can not select Last Date of the Month!', 'failure');
        //    return false;
        //}

        $http({
            method: 'POST',
            url: $scope.path + "LoadPCAACEmployeelist",
            data: { 'fromDate': $scope.ModelNew.FromDate, 'toDate': $scope.ModelNew.ToDate, 'payDaysType': $scope.ModelNew.PayDaysType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;

            angular.element(document.querySelector("#dialogEmployeeInfo")).modal("show");
        });
    }

    $scope.getPayDaysAmount = function () {
        $http({
            method: 'POST',
            url: $scope.path + "LoadPCAACEmployeelist",
            data: { 'fromDate': $scope.ModelNew.FromDate, 'toDate': $scope.ModelNew.ToDate, 'payDaysType': $scope.ModelNew.PayDaysType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                for (var j = 0; j < $scope.EmployeeMainList.length; j++) {
                    if ($scope.EmployeeMainList[j].SystemId == $scope.EmployeeList[i].SystemId) {
                        $scope.EmployeeMainList[j].PayDays = $scope.EmployeeList[i].PayDays;
                    }
                }
            }
        });
    }


    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmployeeInfoList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                $scope.EmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.GetSelectedEmployeeList = function () {
        try {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                if (checkItemExist($scope.EmployeeMainList, $scope.EmployeeList[i].SystemId) === false) {
                    if ($scope.EmployeeList[i].CheckBoxSelect === true) {
                        $scope.EmployeeMainList.push($scope.EmployeeList[i]);
                    }
                }
            }
            angular.element(document.querySelector("#dialogEmployeeInfo")).modal("hide");
            //$scope.getCalulationAmount();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkItemExist(list, SystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === SystemId) {
                return true;
            }
        }
        return false;
    }

    //$scope.getCalulationAmount = function () {
    //    for (var i = 0; i < $scope.EmployeeMainList.length; i++) {
    //        $scope.EmployeeMainList[i].Amount = Math.floor($scope.EmployeeMainList[i].Basic / 26 * $scope.EmployeeMainList[i].PayDays * $scope.ModelNew.Percentage / 100);
    //        $scope.EmployeeMainList[i].NetPayable = $scope.EmployeeMainList[i].Amount - $scope.EmployeeMainList[i].AdvanceGiven;

    //    }
    //}
    //*********************************** Worker Advance End********************************************************//

    //***********************************Payable Creation Start*******************************************************//

    $scope.ModelPCTemp = {
        Id: null,
        FromDate: null,
        ToDate: null,
        UserRef: null,
        PaymentDate: null,
        ByWhom: $window.employeeName,
        ByWhomId: $window.employeeId,
        Remarks: null
    };
    $scope.ModelPCNew = Object.assign({}, $scope.ModelPCTemp);

    $scope.ModelPCemp = {
        Id: null,
        GoodWorkPayableCreationId: null,
        EmpSystemId: null,
        EmployeeCode: null,
        EmployeeName: null,
        Amount: null,
        OTHour: null,
        Rate: null,
        Payment: null,
        RatePerDay: null,
        RatePerHour: null,
        AdvanceGiven: 0,
        NetPayable: null,
        PaymentChildId: null
    };
    $scope.ModelPCEmpNew = Object.assign({}, $scope.ModelPCemp);

    $scope.approvedByList = [];
    $scope.GetApprovedByCboList = function () {
        $http({
            method: 'GET',
            url: 'Attendances/GoodWork/GetGoodWorkPaymentApproveByCboList'
        }).then(function successCallback(response) {
            $scope.approvedByList = response.data;
            if (baseService.arrayLength($scope.approvedByList) == 1) {
                $scope.ModelNew.ApprovedById = $scope.approvedByList[0].Value;
            }
        });
    }
    $scope.GetApprovedByCboList();

    $scope.PCEmployeeList = [];
    $scope.PCOTEmployeeList = [];
    $scope.GetLoadEmployeeInformation = function (obj) {
        if (baseService.isUndefinedOrNull(obj)) {
            $scope.TabName = $scope.SaveTabName;
        } else {
            $scope.TabName = obj;
        }
        if ($scope.TabName == "GoodWork") {
            if ($scope.ModelPCNew.ToDate === "" || $scope.ModelPCNew.ToDate === null || $scope.ModelPCNew.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            if ($scope.ModelPCNew.FromDate === "" || $scope.ModelPCNew.FromDate === null || $scope.ModelPCNew.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }

            $http({
                method: 'POST',
                // url: $scope.path + "LoadPCEmployeelist",
                url: $scope.path + "GetGoodWorkEmployeelist",
                data: { 'fromDate': $scope.ModelPCNew.FromDate, 'toDate': $scope.ModelPCNew.ToDate, 'tabName': $scope.TabName, 'saveUpdate': $scope.PCAction},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.PCEmployeeList = response.data;
                for (var i = 0; i < $scope.PCEmployeeList.length; i++) {
                    $scope.PCEmployeeList[i].Remarks = $scope.ModelPCNew.Remarks;
                }
            });
        }
        else {
            if ($scope.ModelOTNew.ToDate === "" || $scope.ModelOTNew.ToDate === null || $scope.ModelOTNew.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            if ($scope.ModelOTNew.FromDate === "" || $scope.ModelOTNew.FromDate === null || $scope.ModelOTNew.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }

            $http({
                method: 'POST',
                url: $scope.path + "GetGoodWorkEmployeelist",
                data: { 'fromDate': $scope.ModelOTNew.FromDate, 'toDate': $scope.ModelOTNew.ToDate, 'tabName': $scope.TabName, 'saveUpdate': $scope.PCOTAction },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.PCOTEmployeeList = response.data;
                for (var i = 0; i < $scope.PCOTEmployeeList.length; i++) {
                    $scope.PCOTEmployeeList[i].Remarks = $scope.ModelOTNew.Remarks;
                }
            });
        }
    }

    $scope.PayableCreationSave = function () {
        try {
            $scope.FD = $filter('dateFiltering')(new Date($scope.ModelNew.FromDate), 'dd-MM-yyyy');
            $scope.TD = $filter('dateFiltering')(new Date($scope.ModelNew.ToDate), 'dd-MM-yyyy');
            $http({
                method: 'POST',
                url: $scope.savePCUrl,
                data: { 'data': $scope.ModelPCNew, 'goodWorkPaymentDetail': $scope.PCEmployeeList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearPayableCreation();
                    $scope.GetGoodWorkPaymentData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.GoodWorkPaymentList = [];
    $scope.GetGoodWorkPaymentData = function (obj) {
        $scope.TabGWName = obj;
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentList?paymentSource=" + 'GoodWork',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GoodWorkPaymentList = response.data;
        });
    }
    $scope.GetGoodWorkPaymentData();

    $scope.GetGoodWorkPaymentAdvisedetail = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdviseDetailList?paymentAdviseId=" + $scope.ModelPCNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCEmployeeList = response.data;
        });
    }
    $scope.TabGWName = "GoodWork";
    $scope.GetGWPDblClick = function (args) {
        $scope.ModelPCNew = Object.assign({}, args.data);
        //$scope.GetGoodWorkPaymentAdvisedetail();
        $scope.PCAction = 'Update';
        $scope.TabGWName = "GoodWork";
        $scope.GetLoadEmployeeInformation($scope.TabGWName);
        
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ClearPayableCreation = function () {
        $scope.Action = 'Save';
        $scope.ModelPCNew = Object.assign({}, $scope.ModelPCTemp);
        $scope.PCEmployeeList = [];
        return true;
    };


    $scope.ClearPayableCreationOT = function () {
        $scope.Action = 'Save';
        $scope.ModelOTNew = Object.assign({}, $scope.ModelOTemp);
        $scope.PCOTEmployeeList = [];
        return true;
    };


    $scope.GoodWorkPayableCreationSave = function (obj) {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $scope.SaveTabName = obj;
            if ($scope.SaveTabName == "GoodWork") {
                if ($scope.ModelNewForm.$valid) {
                    $scope.GetEmployeeGWListItem();
                    $scope.FD = $filter('dateFiltering')(new Date($scope.ModelPCNew.FromDate), 'dd-MM-yyyy');
                    $scope.TD = $filter('dateFiltering')(new Date($scope.ModelPCNew.ToDate), 'dd-MM-yyyy');
                    $scope.ModelPCNew.PaymentSource = obj;
                    $http({
                        method: 'POST',
                        url: $scope.path + 'CreateGoodWorkPayableCreation',
                        data: { 'data': $scope.ModelPCNew, 'goodWorkPaymentAdviseDetail': $scope.EmployeeGWList, 'tabName': $scope.SaveTabName },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.ModelPCNew.Id = response.data.Data.Id;
                            //$scope.ClearPayableCreation();
                            $scope.GetGoodWorkPaymentData();
                            $scope.GetLoadEmployeeInformation($scope.SaveTabName);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }

            else {
                if ($scope.ModelNew2Form.$valid) {
                    $scope.GetEmployeeListItem();
                    $scope.FD = $filter('dateFiltering')(new Date($scope.ModelOTNew.FromDate), 'dd-MM-yyyy');
                    $scope.TD = $filter('dateFiltering')(new Date($scope.ModelOTNew.ToDate), 'dd-MM-yyyy');
                    $scope.ModelOTNew.PaymentSource = obj;
                    $http({
                        method: 'POST',
                        url: $scope.path + 'CreateGoodWorkPayableCreation',
                        data: { 'data': $scope.ModelOTNew, 'goodWorkPaymentAdviseDetail': $scope.EmployeeEOTList, 'tabName': $scope.SaveTabName },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.ModelOTNew.Id = response.data.Data.Id;
                            $scope.GetGoodWorkOTPaymentData();
                            //$scope.ClearOTPayableCreation();
                            $scope.GetLoadEmployeeInformation($scope.SaveTabName);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //***********************************Payable Creation Extra OT Start********************************************************//
    $scope.ModelOTemp = {
        Id: null,
        FromDate: null,
        ToDate: null,
        UserRef: null,
        PaymentDate: null,
        ByWhom: $window.employeeName,
        ByWhomId: $window.employeeId,
        Remarks: null
    };
    $scope.ModelOTNew = Object.assign({}, $scope.ModelOTemp);

    $scope.ClearOTPayableCreation = function () {
        $scope.Action = 'Save';
        $scope.ModelOTNew = Object.assign({}, $scope.ModelOTemp);
        $scope.PCOTEmployeeList = [];
        return true;
    };

    $scope.TabsName = "ExtraOT";
    $scope.GoodWorkOTPaymentList = [];
    $scope.GetGoodWorkOTPaymentData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentList?paymentSource=" + 'Attendance',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GoodWorkOTPaymentList = response.data;
        });
    }
    $scope.GetGoodWorkOTPaymentData();

    $scope.GetGoodWorkPaymentOTAdvisedetail = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdviseOTDetailList?paymentAdviseId=" + $scope.ModelOTNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCOTEmployeeList = response.data;
        });
    }

    $scope.GetGWPOTDblClick = function (args) {
        $scope.ModelOTNew = Object.assign({}, args.data);
        //$scope.GetGoodWorkPaymentOTAdvisedetail();
        $scope.PCOTAction = 'Update';
        $scope.GetLoadEmployeeInformation($scope.TabsName);
        
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    //***********************************Payable Creation Extra OT End********************************************************//

    $scope.refreshTemplateemployees = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyees });
    };

    function CheckBoxSelectAllEmolyees(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridChildEdit").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PCOTEmployeeList.length; i++) {
                $scope.PCOTEmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridChildEdit").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.EmployeeEOTList = [];
    $scope.GetEmployeeListItem = function () {
        $scope.EmployeeEOTList = [];
        try {
            for (var i = 0; i < $scope.PCOTEmployeeList.length; i++) {
                if (checkItemsExist($scope.EmployeeEOTList, $scope.PCOTEmployeeList[i].EmpSystemId) === false) {
                    if ($scope.PCOTEmployeeList[i].CheckBoxSelect === true) {
                        $scope.EmployeeEOTList.push($scope.PCOTEmployeeList[i]);
                    }
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    function checkItemsExist(list, EmpSystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === EmpSystemId) {
                return true;
            }
        }
        return false;
    }

    //***********************************Payable Creation End********************************************************//
    $scope.refreshTemplateGWemployees = function (args) {
        $("#GWheadchk").ejCheckBox({ "change": CheckBoxSelectGWAllEmolyees });
    };

    function CheckBoxSelectGWAllEmolyees(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridGWChildEdit").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PCEmployeeList.length; i++) {
                $scope.PCEmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridGWChildEdit").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.EmployeeGWList = [];
    $scope.GetEmployeeGWListItem = function () {
        $scope.EmployeeGWList = [];
        try {
            for (var i = 0; i < $scope.PCEmployeeList.length; i++) {
                if (checkItemsExist($scope.EmployeeGWList, $scope.PCEmployeeList[i].EmpSystemId) === false) {
                    if ($scope.PCEmployeeList[i].CheckBoxSelect === true) {
                        $scope.EmployeeGWList.push($scope.PCEmployeeList[i]);
                    }
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkItemsExist(list, EmpSystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === EmpSystemId) {
                return true;
            }
        }
        return false;
    }
    //Pending for Approval WorkerAdvance Start
    $scope.WorkerAdvancePendingforApprovalList = [];
    $scope.getPendingforApprovalData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetWorkerAdvancePendingforApprovalList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.WorkerAdvancePendingforApprovalList = response.data;
        });
    }
    $scope.getPendingforApprovalData();


    $scope.GetDblClickPendingforApproval = function (args) {
        $scope.ModelNewPendingforApproval = Object.assign({}, args.data);
        $scope.ModelNewPendingforApproval.YearNo = $scope.ModelNewPendingforApproval.YearNo.toString();
        $scope.ModelNewPendingforApproval.MonthNo = $scope.ModelNewPendingforApproval.MonthNo.toString();

        $scope.GetWorkerAdvanceDetailPendingforApproval();

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.EmployeeMainListWorkerAdvanceDetail = [];
    $scope.GetWorkerAdvanceDetailPendingforApproval = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetWorkerAdvanceDetailCenter?workAdvanceId=' + $scope.ModelNewPendingforApproval.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeMainListWorkerAdvanceDetail = resp.data;
        });
    }
    $scope.workerAdvanceId = null;
    $scope.ApproveWorkerAdvanceConfirm = function (data) {
        $scope.workerAdvanceId = data.Id;
        $scope.message_approve_confirmation = "Are you sure to Approve?";
        angular.element(document.querySelector("#confirmWorkerAdvanceApprovePopUp")).modal("show");
    };

    $scope.approveUrl = "Attendances/GoodWork/ApproveWorkerAdvance";
    $scope.approveWorkerAdvance = function (workerAdvanceId) {
        $http({
            method: "POST",
            url: $scope.approveUrl,
            data: {
                "workerAdvanceId": workerAdvanceId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getPendingforApprovalData();

                $scope.workerAdvanceId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    //Pending for Approval WorkerAdvance End

    //Pending for Payment GoodWorkPaymentAdvise Pending Payment Start
    $scope.ModelPCTempPendingPayment = {
        Id: null,
        FromDate: null,
        ToDate: null,
        UserRef: null,
        PaymentDate: null,
        ByWhom: null,
        ByWhomId: null,
        PaymentSource: null,
        Remarks: null
    };
    $scope.ModelPCNewPendingPayment = Object.assign({}, $scope.ModelPCTempPendingPayment);

    $scope.GoodWorkPaymentAdvisePendingPaymentList = [];
    $scope.GetGoodWorkPaymentAdvisePendingPaymentData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdvisePendingPaymentList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GoodWorkPaymentAdvisePendingPaymentList = response.data;
        });
    }
    $scope.GetGoodWorkPaymentAdvisePendingPaymentData();
    $scope.PCEmployeeListPendingPayment = [];
    $scope.GetGoodWorkPaymentAdvisePendingPaymentdetail = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdviseDetailForApproveList?paymentAdviseId=" + $scope.ModelPCNewPendingPayment.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCEmployeeListPendingPayment = response.data;
        });
    }

    $scope.GetGWPendingPaymentPDblClick = function (args) {
        $scope.ModelPCNewPendingPayment = Object.assign({}, args.data);
        $scope.GetGoodWorkPaymentAdvisePendingPaymentdetail();

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ClearGoodWorkPaymentAdvisePendingPayment = function () {
        $scope.Action = 'Save';
        $scope.ModelPCNewPendingPayment = Object.assign({}, $scope.ModelPCTempPendingPayment);
        $scope.PCEmployeeListPendingPayment = [];
        return true;
    };

    $scope.GoodWorkPaymentAdvisePendingPaymentSave = function (obj) {
        try {
            $scope.GetEmployeeGWListPendingPaymentItem();
            if ($scope.EmployeeGWListPendingPayment.length === 0) {
                ShowResult("Please select Employee!", "failure");
                return true;
            }
            $http({
                method: 'POST',
                url: $scope.path + 'SaveGoodWorkPaymentAdvisePendingPayment',
                data: { 'data': $scope.ModelPCNewPendingPayment, 'goodWorkPaymentAdviseDetail': $scope.EmployeeGWListPendingPayment },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.ClearGoodWorkPaymentAdvisePendingPayment();
                    $scope.GetGoodWorkPaymentAdvisePendingPaymentData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.refreshTemplateGWemployeesPendingPayment = function (args) {
        $("#GWheadchkPendingPayment").ejCheckBox({ "change": CheckBoxSelectGWAllEmolyeesPendingPayment });
    };

    function CheckBoxSelectGWAllEmolyeesPendingPayment(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridGWChildEditPendingPayment").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PCEmployeeListPendingPayment.length; i++) {
                $scope.PCEmployeeListPendingPayment[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridGWChildEditPendingPayment").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.EmployeeGWListPendingPayment = [];
    $scope.GetEmployeeGWListPendingPaymentItem = function () {
        $scope.EmployeeGWListPendingPayment = [];
        try {
            for (var i = 0; i < $scope.PCEmployeeListPendingPayment.length; i++) {
                if (checkItemsExistPendingPayment($scope.EmployeeGWListPendingPayment, $scope.PCEmployeeListPendingPayment[i].Id) === false) {
                    if ($scope.PCEmployeeListPendingPayment[i].CheckBoxSelect === true) {
                        $scope.EmployeeGWListPendingPayment.push($scope.PCEmployeeListPendingPayment[i]);
                    }
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkItemsExistPendingPayment(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    //Pending for Payment GoodWorkPaymentAdvise Pending Payment End

    //Pending for Payment GoodWorkPaymentAdvise Pending Approval Start
    $scope.ModelPCTempPendingPaymentApproval = {
        Id: null,
        FromDate: null,
        ToDate: null,
        UserRef: null,
        PaymentDate: null,
        ByWhom: null,
        ByWhomId: null,
        PaymentSource: null,
        Remarks: null
    };
    $scope.ModelPCNewPendingPaymentApproval = Object.assign({}, $scope.ModelPCTempPendingPaymentApproval);

    $scope.GoodWorkPaymentAdvisePendingApprovalList = [];
    $scope.GetGoodWorkPaymentAdvisePendingApprovalData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdvisePendingApprovalList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GoodWorkPaymentAdvisePendingApprovalList = response.data;
        });
    }
    $scope.GetGoodWorkPaymentAdvisePendingApprovalData();
    $scope.PCEmployeeListPendingPaymentApproval = [];
    $scope.GetGoodWorkPaymentAdvisePendingApprovaldetail = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdviseDetailCheckedList?paymentAdviseId=" + $scope.ModelPCNewPendingPaymentApproval.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCEmployeeListPendingPaymentApproval = response.data;
        });
    }

    $scope.GetGWPendingPaymentApprovalPDblClick = function (args) {
        $scope.ModelPCNewPendingPaymentApproval = Object.assign({}, args.data);
        $scope.GetGoodWorkPaymentAdvisePendingApprovaldetail();

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ClearGoodWorkPaymentAdvisePendingApproval = function () {
        $scope.Action = 'Save';
        $scope.ModelPCNewPendingPaymentApproval = Object.assign({}, $scope.ModelPCTempPendingPaymentApproval);
        $scope.PCEmployeeListPendingPaymentApproval = [];
        return true;
    };

    $scope.GoodWorkPaymentAdvisePendingApprovalSave = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveGoodWorkPaymentAdvisePendingApproval',
                data: { 'data': $scope.ModelPCNewPendingPaymentApproval },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.ClearGoodWorkPaymentAdvisePendingApproval();
                    $scope.GetGoodWorkPaymentAdvisePendingApprovalData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //Pending for Payment GoodWorkPaymentAdvise Pending Payment End

    //Payments GoodWorkPaymentAdvise Payments Start
    $scope.ModelPCTempPayments = {
        Id: null,
        FromDate: null,
        ToDate: null,
        UserRef: null,
        PaymentDate: null,
        ByWhom: null,
        ByWhomId: null,
        PaymentSource: null,
        Remarks: null
    };
    $scope.ModelPCNewPayments = Object.assign({}, $scope.ModelPCTempPayments);

    $scope.GoodWorkPaymentAdvisePaymentsList = [];
    $scope.GetGoodWorkPaymentAdvisePaymentsData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdviseApprovedList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GoodWorkPaymentAdvisePaymentsList = response.data;
        });
    }
    $scope.GetGoodWorkPaymentAdvisePaymentsData();
    $scope.PCEmployeeListPayments = [];
    $scope.GetGoodWorkPaymentAdvisePaymentsdetail = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdviseDetailCheckedList?paymentAdviseId=" + $scope.ModelPCNewPayments.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCEmployeeListPayments = response.data;
        });
    }

    $scope.GetGWPaymentsPDblClick = function (args) {
        $scope.ModelPCNewPayments = Object.assign({}, args.data);
        $scope.GetGoodWorkPaymentAdvisePaymentsdetail();

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ClearGoodWorkPaymentAdvisePayments = function () {
        $scope.Action = 'Save';
        $scope.ModelPCNewPayments = Object.assign({}, $scope.ModelPCTempPayments);
        $scope.PCEmployeeListPayments = [];
        return true;
    };

    $scope.GoodWorkPaymentAdvisePaymentsSave = function (obj) {
        try {
            $scope.GetEmployeeGWListPaymentsItem();
            if ($scope.EmployeeGWListPayments.length === 0) {
                ShowResult("Please select Employee!", "failure");
                return true;
            }
            $http({
                method: 'POST',
                url: $scope.path + 'SaveGoodWorkPaymentAdvisePayments',
                data: { 'data': $scope.ModelPCNewPayments, 'goodWorkPaymentAdviseDetail': $scope.EmployeeGWListPayments },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.ClearGoodWorkPaymentAdvisePayments();
                    $scope.GetGoodWorkPaymentAdvisePaymentsData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.refreshTemplateGWemployeesPayments = function (args) {
        $("#GWheadchkPayments").ejCheckBox({ "change": CheckBoxSelectGWAllEmolyeesPayments });
    };

    function CheckBoxSelectGWAllEmolyeesPayments(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridGWChildEditPayments").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PCEmployeeListPayments.length; i++) {
                if ($scope.PCEmployeeListPayments[i].IsDisburse === false) {
                    $scope.PCEmployeeListPayments[i].CheckBoxSelect = ChkOrUnchk;
                }

            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                if (filtered[j].IsDisburse === false) {
                    filtered[j].CheckBoxSelect = ChkOrUnchk;
                }

            }
        }
        var gridObj = $("#GridGWChildEditPayments").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.EmployeeGWListPayments = [];
    $scope.GetEmployeeGWListPaymentsItem = function () {
        $scope.EmployeeGWListPayments = [];
        try {
            for (var i = 0; i < $scope.PCEmployeeListPayments.length; i++) {
                if (checkItemsExistPayments($scope.EmployeeGWListPayments, $scope.PCEmployeeListPayments[i].Id) === false) {
                    if ($scope.PCEmployeeListPayments[i].CheckBoxSelect === true && $scope.PCEmployeeListPayments[i].IsDisburse === false) {
                        $scope.EmployeeGWListPayments.push($scope.PCEmployeeListPayments[i]);
                    }
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkItemsExistPayments(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.goodWorkPaymentAdvisePaymentsDownload = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.PaymentsStatus)) return ShowResult('No Payments found', 'failure');
        $window.open('Attendances/GoodWork/GoodWorkPaymentAdvisePaymentsReports?' + '&reportFormat=' + reportFormat + '&goodWorkPaymentAdviseId=' + data.Id);
    };

    //Payments GoodWorkPaymentAdvise Payments End

    //Payments GoodWorkPaymentAdvise Payments Posting Start
    $scope.tabVoucher = 1;
    $scope.setTabVoucher = function (newTabVoucher) {
        $scope.tabVoucher = newTabVoucher;
    };

    $scope.isSetVoucher = function (tabNumVoucher) {
        return $scope.tabVoucher === tabNumVoucher;
    };
    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        ExpenseBookingId: null,
        EmployeeId: null,
        EmployeeName: null,
        EmployeeCodeName: null,
        PartyGLGeneralInfoId: null,
        EmployeeTransactionTypeId: null,
        GLGeneralInfoId: null,
        CurrencyId: null,
        EntityId: null,
        PlantId: null,
        CurrencyCode: null,
        VoucherTypeId: null,
        PartyType: "Employee",
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        DocDate: $filter("dateFiltering")(Date.now()),
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        VoucherDetailId: null,
        Amount: null,
        BaseOnDueDate: $filter("dateFiltering")(Date.now()),
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankAccountNumber: null,
        BankGL: null,
        BankGLGeneralInfoId: null,
        PaymentMode: '',
        IsActive: true,
        IsSeperated: false,
        IsMaternity: false
    };
    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
                $scope.voucherBonus.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };

    $scope.getCboVoucherTypeGoodWorkDisbursementList = function () {
        cboService.getCboVoucherTypeGoodWorkDisbursementList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
            }
        });
    }
    $scope.getCboVoucherTypeGoodWorkDisbursementList();
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });

    $scope.getFiscalYearPeriod = function (date) {
        if (!baseService.isUndefinedOrNull(date)) {
            $http({
                method: "get",
                url: "accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=" + $filter("dateFiltering")(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, "failure");
                            $scope.voucher.PostingDate = "";
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, "failure");
                            $scope.voucher.PostingDate = "";
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else {
                            $scope.voucher.FiscalYearId = result.FiscalYearId;
                            $scope.voucher.FiscalYearName = result.FiscalYearName;
                            $scope.voucher.FiscalYearPeriodId = result.FiscalYearPeriodId;
                            $scope.voucher.FiscalYearPeriodName = result.PeriodName;
                        }
                    }
                },
                function errorCallback(response) {
                });
        }
    };

    $scope.salaryLockPayableGLData = [];
    $scope.EmployeeListNew = [];
    $scope.getSalaryLockPayableGL = function () {
        //$scope.GetEmployeeDisbursementItem();
        $scope.salaryLockPayableGLData = [];
        $http({
            method: "POST",
            url: "Attendances/GoodWork/GetGoodWorkPaymentAdviseDisbursementJVDataList",
            data: { 'disbursementAdviceId': $scope.voucher.DisbursementAdviceId, 'goodWorkPaymentAdviseDetail': $scope.EmployeeListNew },
            dataType: 'JSON'
            , contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            $scope.salaryLockPayableGLData = response.data;
        });
    };

    $scope.employeeDisbursementDataList = [];
    $scope.GetemployeeDisbursement = function () {
        $scope.employeeDisbursementDataList = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.path + "GetGoodWorkPaymentAdviseDetailCheckedList?paymentAdviseId=" + $scope.voucher.DisbursementAdviceId,
        }).then(function successCallback(response) {
            if (response.data.length > 0) {

                $scope.employeeDisbursementDataList = response.data;
                $scope.EmployeeListNew = response.data;
                $scope.getSalaryLockPayableGL();
            }
            else {
                ShowResult("No Data Found", 'failure');

            }
        });
    };

    $scope.saveGoodWorkPaymentAdviseDisbursementUrl = $scope.path + "ParkGoodWorkPaymentAdviseDisbursement";
    $scope.postGoodWorkPaymentAdviseDisbursementUrl = $scope.path + "PostGoodWorkPaymentAdviseDisbursement";
    $scope.saveBtnDisable = false;
    $scope.SaveGoodWorkPaymentAdviseDisbursement = function () {
        //$scope.GetEmployeeDisbursementItem();
        if ($scope.EmployeeListNew.length === 0) {
            ShowResult("Please select Employee!", "failure");
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.voucher.PaymentMode)) {
            ShowResult("Please select Payment Mode!", "failure");
            return true;
        }
        if ($scope.voucher.PaymentMode === "Bank") {
            if ($scope.voucher.BankName === "" || baseService.isUndefinedOrNull($scope.voucher.BankMasterId)) {
                ShowResult("Please select Bank!", "failure");
                return true;
            }
        }
        if ($scope.voucher.PaymentMode === "Cash") {
            if ($scope.voucher.CashName === "" || baseService.isUndefinedOrNull($scope.voucher.CashMasterId)) {
                ShowResult("Please select Cash!", "failure");
                return true;
            }
        }

        $scope.$broadcast("show-errors-check-validity");
        $scope.saveBtnDisable = true;
        try {
            if ($scope.form0.$valid) {
                $http({
                    method: "POST",
                    url: $scope.saveGoodWorkPaymentAdviseDisbursementUrl,
                    data: {
                        "voucherVM": $scope.voucher,
                        "directJVList": $scope.salaryLockPayableGLData,
                        "disbursementAdviceId": $scope.voucher.DisbursementAdviceId,
                        "goodWorkPaymentAdviseDetail": $scope.EmployeeListNew
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.saveBtnDisable = false;
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getVoucherData();
                        $scope.ClearGoodWorkPaymentAdviseDisbursement();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;

            }
        } catch (e) {
            throw ShowResult(e, "failure");
        }
        return true;
    };

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };
    $scope.postGoodWorkPaymentAdviseDisbursementUrl = $scope.path + "PostGoodWorkPaymentAdviseDisbursement";
    $scope.post = function (voucherId) {
        $http({
            method: "POST",
            url: $scope.postGoodWorkPaymentAdviseDisbursementUrl,
            data: {
                "voucherId": voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getVoucherData();
                $scope.ClearGoodWorkPaymentAdviseDisbursement();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.ClearGoodWorkPaymentAdviseDisbursement = function () {
        ClearGoodWorkPaymentAdviseDisbursementFields();
    };

    function ClearGoodWorkPaymentAdviseDisbursementFields() {
        $scope.voucher = {};
        $scope.voucher.PaymentMode = '';
        $scope.voucher.EmployeeId = null;
        $scope.getCboVoucherTypeGoodWorkDisbursementList();
        $scope.voucher.Active = true;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.DocRefNo = null;
        $scope.employeeDisbursementDataList = [];
        $scope.salaryLockPayableGLData = [];
        $scope.EmployeeListNew = [];
        $scope.saveBtnDisable = false;

    }

    $scope.paymentModeList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetEMPPaymentModeEnumCbo/'
    }).then(function successCallback(response) {
        $scope.paymentModeList = response.data;
    });


    $scope.changePaymentMode = function () {
        if ($scope.voucher.PaymentMode == 'Bank') {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
            $scope.voucher.CashMasterId = null;
            $scope.voucher.CashCurrencyId = null;
            $scope.voucher.CashName = null;
            //$scope.getBank();
        }
        else {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
            $scope.voucher.BankId = null;
            $scope.voucher.BankMasterId = null;
            $scope.voucher.BankCurrencyId = null;
            $scope.voucher.AccountTitle = null;
            $scope.voucher.BankName = null;
        }
    }
    $scope.changeBank = function () {
        if ($scope.voucher.PaymentMode == 'Bank') {
            $scope.voucher.PaymentSource = $scope.voucher.PaymentMode;
        }
    }
    $scope.bankSearchByList = [
        {
            "name": "Bank",
            "value": "BankName"
        },
        {
            "name": "Bank Branch",
            "value": "BankBranchName"
        },
        {
            "name": "Account Type",
            "value": "BankAccountTypeName"
        },
        {
            "name": "Account Number",
            "value": "AccountNumber"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        }
    ];
    $scope.bankmasterList = [];
    $scope.bankParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "BankName, BankBranchName, AccountTitle",
        searchBy: "AccountNumber",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showBankPopUp = function () {
        $scope.getBankList = function (pageno) {

            $scope.url = "Accounts/SalaryDisbursement/GetBankMasterList?bankACType=HouseBank&&bankId=" + $scope.voucher.BankId;
            baseService.paginationBase($scope.url, pageno, $scope.bankParameters)
                .then(function (result) {
                    $scope.bankmasterList = result.Rows;
                    $scope.bankParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getBankList();
        angular.element(document.querySelector("#bankPopUp")).modal("show");
    };

    $scope.closeCashPopUp = function () {
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult("Cash GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult("Cash Budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult("Cash Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.voucher.CashMasterId = cash.Id;
                $scope.voucher.CashCurrencyId = cash.CurrencyId;
                $scope.voucher.CashName = cash.CashName;
                $scope.voucher.GLGeneralInfoId = cash.GLGeneralInfoId;
                $scope.voucher.GLGeneralInfoName = cash.GLItem;
                $scope.voucher.BudgetName = cash.BudgetName;
                $scope.voucher.BudgetMasterId = cash.BudgetMasterId;
                $scope.voucher.ActivityId = cash.ActivityId;
                $scope.voucher.ActivityName = cash.ActivityName;
                $scope.checkCashAmount();
            }
        }
        $scope.hideCashPopUp();
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankmasterList[$scope.bankIndex];

            $scope.voucher.AccountTitle = bank.AccountTitle;
            $scope.voucher.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
            $scope.voucher.BankMasterId = bank.BankMasterId;
            $scope.voucher.BankCurrencyId = bank.CurrencyId;

            $scope.voucher.GLGeneralInfoId = bank.GLGeneralInfoId;
            $scope.voucher.GLGeneralInfoName = bank.GLGeneralInfoName;
            $scope.voucher.BudgetMasterId = bank.BudgetMasterId;
            $scope.voucher.BudgetName = bank.BudgetName;
            $scope.voucher.ActivityId = bank.ActivityId;
            $scope.voucher.ActivityName = bank.ActivityName;
            $scope.checkBankAmount();
        }
        $scope.hideBankPopUp();
    };
    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
    };

    $scope.hideBankPopUp = function () {
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
        $scope.bankIndex = -1;
        $scope.bankSelected = null;
    };
    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.BankCurrencyId)) {
            if ($scope.voucher.BankCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.voucher.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucher.BankAmount = 0;
            }
        }
    };

    $scope.checkCashAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.CashCurrencyId)) {
            if ($scope.voucher.CashCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.voucher.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.voucher.BankAmount = 0;
            }
        }
    };

    $scope.deleteGoodWorkPaymentAdviseDisbursementUrl = "Attendances/GoodWork/DeleteGoodWorkPaymentAdviseDisbursement";

    $scope.deleteSalaryDisbursement = function (voucherId, monthNo, yearNo) {
        $http({
            method: "POST",
            url: $scope.deleteGoodWorkPaymentAdviseDisbursementUrl,
            data: {
                "voucherId": voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getVoucherData();
                $scope.ClearGoodWorkPaymentAdviseDisbursement();
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.confirmDelete = function (data) {
        $scope.voucherId = data.PayableVoucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.masterList = [];
    $scope.getMasterData = function () {
        $scope.masterList = [];
        $http.get("Attendances/GoodWork/GetGoodWorkPaymentAdviseApprovedList")
            .then(
                function successCallback(response) {
                    $scope.masterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#DisbursementAdvicepopUp')).modal('show');
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#DisbursementAdvicepopUp')).modal('hide');
    }

    $scope.SelectMaster = function (x) {
        var data = x.data;
        $scope.voucher.DisbursementAdviceId = data.Id;
        $scope.voucher.FromDate = data.FromDate;
        $scope.voucher.ToDate = data.ToDate;
        $scope.voucher.UserRef = data.UserRef;
        $scope.voucher.Remarks = data.Remarks;
        $scope.voucher.AdvicePaymentSource = data.PaymentSource;

        $scope.GetemployeeDisbursement();


        angular.element(document.querySelector('#DisbursementAdvicepopUp')).modal('hide');
    };
    $scope.EmployeeListNew = [];
    $scope.pushInTempListforProcess = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListforConfirm($scope.EmployeeListNew, data.EmpSystemId) === false) {
                    $scope.EmployeeListNew.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.EmployeeListNew); i++) {
                        if ($scope.EmployeeListNew[i].EmpSystemId === data.EmpSystemId) {
                            $scope.EmployeeListNew.splice(i, 1);
                            break;
                        }
                    }

                    $scope.EmployeeListNew.push(data);
                }
            }
            else {
                for (var t = 0; t < baseService.arrayLength($scope.EmployeeListNew); t++) {
                    if ($scope.EmployeeListNew[t].EmpSystemId === data.EmpSystemId) {
                        $scope.EmployeeListNew.splice(t, 1);
                        break;
                    }
                }
            }

            $scope.getSalaryLockPayableGL();

        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }

    }

    //function checkExistTempListforConfirm(list, empSystemId) {
    //    for (var i = 0; i < baseService.arrayLength(list); i++) {
    //        if (list[i].EmpSystemId === empSystemId) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    $scope.GetEmployeeDisbursementItem = function () {
        $scope.EmployeeListNew = [];
        try {
            for (var i = 0; i < $scope.employeeDisbursementDataList.length; i++) {
                if ($scope.employeeDisbursementDataList[i].isSelected) {
                    $scope.EmployeeListNew.push($scope.employeeDisbursementDataList[i]);
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.refreshTemplateEmployeeDisbursement = function (args) {
        $("#headchkDisbursement").ejCheckBox({ "change": CheckBoxSelectAllEmployeeDisbursement });
    };

    function CheckBoxSelectAllEmployeeDisbursement(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#empInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.employeeDisbursementDataList.length; i++) {
                $scope.employeeDisbursementDataList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#empInfoGrid").data("ejGrid");
        gridObj.refreshContent();
        $scope.EmployeeListNew = [];
        for (var i = 0; i < $scope.employeeDisbursementDataList.length; i++) {
            if ($scope.employeeDisbursementDataList[i].isSelected) {
                $scope.EmployeeListNew.push($scope.employeeDisbursementDataList[i]);
            }
        }
        $scope.getSalaryLockPayableGL();
    };

    //Payments GoodWorkPaymentAdvise Payments Posting End
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.XlsGoodWorkExtraOTEmployee = function () {
        var dataList = [];
        var newDataList = [];
        var g = $("#GridGWChildEditPendingPayment").data("ejGrid");
        dataList = g.getFilteredRecords();
        var obj = {};

        if (dataList.length == 0) {

            dataList = $scope.PCEmployeeListPendingPayment;
        }

        for (let i = 0; i < dataList.length; i++) {
            obj.EmployeeCode = dataList[i].EmployeeCode;
            obj.EmployeeName = dataList[i].EmployeeName;
            obj.Minute = dataList[i].Minute;
            obj.Hour = dataList[i].Hour;
            obj.Rate = dataList[i].Rate;
            obj.Amount = dataList[i].Amount;
            obj.Remarks = dataList[i].Remarks;
            
            newDataList.push(obj);
            obj = {};
        }
        $scope.fileName = 'GoodWorkExtraOTEmployee';
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