'use strict';
PayableCreationAndWorkerAdvanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function PayableCreationAndWorkerAdvanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
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
            $scope.getCalulationAmount();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkItemExist(list, SystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemId === SystemId) {
                return true;
            }
        }
        return false;
    }

    $scope.getCalulationAmount = function () {
        for (var i = 0; i < $scope.EmployeeMainList.length; i++) {
            $scope.EmployeeMainList[i].Amount = Math.floor($scope.EmployeeMainList[i].Basic / 26 * $scope.EmployeeMainList[i].PayDays * $scope.ModelNew.Percentage / 100);
            $scope.EmployeeMainList[i].NetPayable = $scope.EmployeeMainList[i].Amount - $scope.EmployeeMainList[i].AdvanceGiven;

        }
    }
    //*********************************** Worker Advance End********************************************************//

    //***********************************Payable Creation Start*******************************************************//

    $scope.ModelPCTemp = {
        Id: null,
        FromDate: null,
        ToDate: null,
        UserRef: null,
        PaymentDate: null,
        ByWhom: null,
        ByWhomId: null,
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
        $scope.TabName = obj;
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
    $scope.GetGoodWorkPaymentData = function () {
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

    $scope.GetGWPDblClick = function (args) {
        $scope.ModelPCNew = Object.assign({}, args.data);
        $scope.GetGoodWorkPaymentAdvisedetail();
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

                $scope.FD = $filter('dateFiltering')(new Date($scope.ModelPCNew.FromDate), 'dd-MM-yyyy');
                $scope.TD = $filter('dateFiltering')(new Date($scope.ModelPCNew.ToDate), 'dd-MM-yyyy');
                $scope.ModelPCNew.PaymentSource = obj;
                $http({
                    method: 'POST',
                    url: $scope.path + 'CreateGoodWorkPayableCreation',
                    data: { 'data': $scope.ModelPCNew, 'goodWorkPaymentAdviseDetail': $scope.PCEmployeeList,'tabName': $scope.SaveTabName},
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
            }

            else {
                $scope.FD = $filter('dateFiltering')(new Date($scope.ModelOTNew.FromDate), 'dd-MM-yyyy');
                $scope.TD = $filter('dateFiltering')(new Date($scope.ModelOTNew.ToDate), 'dd-MM-yyyy');
                $scope.ModelOTNew.PaymentSource = obj;
                $http({
                    method: 'POST',
                    url: $scope.path + 'CreateGoodWorkPayableCreation',
                    data: { 'data': $scope.ModelOTNew, 'goodWorkPaymentAdviseDetail': $scope.PCOTEmployeeList,'tabName': $scope.SaveTabName},
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ClearOTPayableCreation();
                        $scope.GetGoodWorkOTPaymentData();
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
        ByWhom: null,
        ByWhomId: null,
        Remarks: null
    };
    $scope.ModelOTNew = Object.assign({}, $scope.ModelOTemp);

    $scope.ClearOTPayableCreation = function () {
        $scope.Action = 'Save';
        $scope.ModelOTNew = Object.assign({}, $scope.ModelOTemp);
        $scope.PCOTEmployeeList = [];
        return true;
    };

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
        $scope.GetGoodWorkPaymentOTAdvisedetail();
        $scope.PCOTAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    //***********************************Payable Creation Extra OT End********************************************************//


    //***********************************Payable Creation End********************************************************//
}