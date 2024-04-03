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

    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetIssueSlipCheckByCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
            //for (var i = 0; i < $scope.checkedByList.length; i++) {
            //    $scope.ModelNew.CheckedById = $scope.checkedByList[i].Value;
            //    $scope.ModelNew.ApprovedById = $scope.checkedByList[i].Value;
            //}
        });
    }
    $scope.GetSupervisorCboList();

    $scope.approvedByList = [];
    $scope.GetApprovedByList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetApprovedByCbo'
        }).then(function successCallback(response) {
            $scope.approvedByList = response.data;
            //for (var i = 0; i < $scope.checkedByList.length; i++) {
            //    $scope.ModelNew.CheckedById = $scope.checkedByList[i].Value;
            //    $scope.ModelNew.ApprovedById = $scope.checkedByList[i].Value;
            //}
        });
    }
    $scope.GetApprovedByList();

    //$scope.showEmployeeListPopUp = function (name) {
    //    $scope.Name = name;
    //    $scope.employee = [];
    //    $http({
    //        method: 'GET',
    //        url: 'Attendances/GoodWork/GetPayableCreationEmployeeData'
    //    }).then(function successCallback(response) {
    //        $scope.employeeDataList = response.data;
    //    });
    //    angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    //}

    //$scope.setEmpData = function (obj) {
    //    //$scope.Clear();
    //    var data = obj.data;
    //    if ($scope.Name === 'AB') {
    //        $scope.ModelNew.ApprovedById = data.SystemId;
    //        $scope.ModelNew.ApprovedBy = data.EmployeeName;
    //    }
    //    else {
    //        $scope.ModelNew.CheckedById = data.SystemId;
    //        $scope.ModelNew.CheckedBy = data.EmployeeName;
    //    }
    //    angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    //};

    //$scope.closeEmployeePopUp = function () {
    //    angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    //};

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
                url: $scope.path + "LoadPCEmployeelist",
                data: { 'fromDate': $scope.ModelPCNew.FromDate, 'toDate': $scope.ModelPCNew.ToDate, 'tabName': $scope.TabName },
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
                url: $scope.path + "LoadPCEmployeelist",
                data: { 'fromDate': $scope.ModelOTNew.FromDate, 'toDate': $scope.ModelOTNew.ToDate, 'tabName': $scope.TabName },
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
        $scope.GetLoadEmployeeInformation($scope.TabGWName);
        $scope.PCAction = 'Update';
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
            $scope.SaveTabName = obj;
            if ($scope.SaveTabName == "GoodWork") {
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
                        //$scope.GetGoodWorkPaymentData();
                        $scope.GetLoadEmployeeInformation($scope.SaveTabName);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }

            else {
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
                        //$scope.GetGoodWorkOTPaymentData();
                        //$scope.ClearOTPayableCreation();
                        $scope.GetLoadEmployeeInformation($scope.SaveTabName);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
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
        $scope.GetLoadEmployeeInformation($scope.TabsName);
        $scope.PCOTAction = 'Update';
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
            url: $scope.path + "GetGoodWorkPaymentAdviseDetailList?paymentAdviseId=" + $scope.ModelPCNewPendingPayment.Id,
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
        $window.open('Attendances/GoodWork/GoodWorkPaymentAdvisePaymentsReports?' + '&reportFormat=' + reportFormat + '&goodWorkPaymentAdviseId=' + data.Id );
    };

    //Payments GoodWorkPaymentAdvise Payments End

}