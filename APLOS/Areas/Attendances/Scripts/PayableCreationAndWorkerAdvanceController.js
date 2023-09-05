'use strict';
PayableCreationAndWorkerAdvanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function PayableCreationAndWorkerAdvanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Payable Creation & Multiple Employee advance';
    $rootScope.titleTab1 = 'Payable Creation';
    $rootScope.titleTab2 = 'Multiple Employee advance';
    $scope.WorkerAdvanceList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.saveUrl = $scope.path + 'CreateWorkerAdvance';
    $scope.savePCUrl = $scope.path + 'CreatePayableCreationWorkerAdvance';
    $scope.UpdateUrl = $scope.path + 'UpdateGoodWorkDetailEdit';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    baseService.init($scope.getListUrl);
    //$scope.LoadEmpListUrl = $scope.path + 'LoadPCAACEmployeelist';
    $scope.Action = 'Save';
    $scope.passwordShow = true;
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //***********************************Worker Advance Start ********************************************************//
    $scope.ModelTemp = {
        Id: null,
        Year: null,
        YearNo: null,
        Month: null,
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
        RoundOff: null,
        PayDaysType: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelWADTemp = {
        Id: null,
        WorkerAdvanceId: null,
        EmpSystemId: null,
        EmployeeCode: null,
        EmployeeName: null,
        PayDays: null,
        RatePerDay: null,
        RatePerHour: null,
        Amount: null,
        AdvanceGiven: 0,
        NetPayable: null,
    };
    $scope.ModelWADNew = Object.assign({}, $scope.ModelWADTemp);


    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

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
    //$scope.DisabledDates = [];

    $scope.CalenderFunc = function () {
        $scope._firstDay = null;
        $scope._lastDay = null;
        //$scope.ModelNew.FromDate = null;
        //$scope.ModelNew.ToDate = null;

        $scope._firstDay = $filter('dateFiltering')(new Date($scope.ModelNew.YearNo, $scope.ModelNew.MonthNo - 1, 1), 'dd-MM-yyyy');
        $scope._lastDay = $filter('dateFiltering')(new Date($scope.ModelNew.YearNo, $scope.ModelNew.MonthNo, 0), 'dd-MM-yyyy');
        //InitializeDate();

        $('.datepic').datepicker({
            startDate: $scope._firstDay,
            endDate: $scope._lastDay,
            //datesDisabled: $scope.DisabledDates,
            format: 'dd-MM-yyyy',
            todayHighlight: true,
            autoclose: true,
            inline: true,
            changeMonth: true
        }); 
        /* $("#GFG").datepicker("refresh");*/
    };
    //function InitializeDate() {
    //    $(".datepic").datepicker();
    //}

    //$('.datepicker').datepicker({
    //    startDate: '-1d',
    //    endDate: '31d',
    //    datesDisabled: $scope.DisabledDates,
    //    format: 'dd-M-yyyy',
    //    todayHighlight: true,
    //    autoclose: true,
    //    inline: true,
    //    changeMonth: true
    //});

    $scope.popUpDataList = [];
    $scope.showByWhomEmployeeListPopUp = function (index) {
        try {
            $scope.tempIndex = index;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'Attendances/GoodWork/GetAllActiveEmployeeData'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedById = arg.data.SystemId;
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedByCode = arg.data.EmployeeCode;
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedByName = arg.data.EmployeeName;
        $scope.closePopUp();
    }

    $scope.clearEmp = function () {
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedById = null;
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedByCode = null;
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedByName = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

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
            $scope.FD = $filter('dateFiltering')(new Date($scope.ModelNew.FromDate), 'dd-MM-yyyy');
            $scope.TD = $filter('dateFiltering')(new Date($scope.ModelNew.ToDate), 'dd-MM-yyyy');

            $scope.$broadcast('show-errors-check-validity');
            if ($scope._firstDay == $scope.FD || $scope._lastDay == $scope.TD) {
                ShowResult('You can not select 1st Date & Last Date of the Month!', 'failure');
                return false;
            }
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


    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.employeeDataList = [];
    $scope.employeeUrl = 'OrderManagements/masterorder/GetEmployeeListResponsible';
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeDataList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeDataList[$scope.employeeIndex];
            if ($scope.Name === 'PB') {
                $scope.ModelNew.PreparedById = employee.SystemId;
                $scope.ModelNew.PreparedBy = employee.EmployeeName;
            } else if ($scope.Name === 'AB') {
                $scope.ModelNew.ApprovedById = employee.SystemId;
                $scope.ModelNew.ApprovedBy = employee.EmployeeName;
            }
            else if ($scope.Name === 'CB') {
                $scope.ModelNew.CheckedById = employee.SystemId;
                $scope.ModelNew.CheckedBy = employee.EmployeeName;
            }
            else {
                $scope.ModelPCNew.ByWhomId = employee.SystemId;
                $scope.ModelPCNew.ByWhom = employee.EmployeeName;
            }
        }
        $scope.hideEmployeePopUp();
    };


    $scope.EmployeeList = [];
    $scope.EmployeeMainList = [];
    $scope.getEmploymeeList = function () {
        if ($scope.ModelNew.ToDate === "" || $scope.ModelNew.ToDate === null || $scope.ModelNew.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        if ($scope.ModelNew.FromDate === "" || $scope.ModelNew.FromDate === null || $scope.ModelNew.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.ModelNew.PayDaysType === "" || $scope.ModelNew.PayDaysType === null || $scope.ModelNew.PayDaysType === undefined) {
            ShowResult('Select From Pay Days', 'failure');
            return false;
        }
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
        UserName: null,
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
    $scope.GetLoadEmployeeInformation = function () {
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
            data: { 'fromDate': $scope.ModelPCNew.FromDate, 'toDate': $scope.ModelPCNew.ToDate },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCEmployeeList = response.data;
        });
    }

    $scope.PayableCreationSave = function () {
        try {
            $scope.FD = $filter('dateFiltering')(new Date($scope.ModelNew.FromDate), 'dd-MM-yyyy');
            $scope.TD = $filter('dateFiltering')(new Date($scope.ModelNew.ToDate), 'dd-MM-yyyy');

            $scope.$broadcast('show-errors-check-validity');

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
                    //$scope.getData();
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
            url: $scope.path + "GetGoodWorkPaymentList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GoodWorkPaymentList = response.data;
        });
    }
    $scope.GetGoodWorkPaymentData();


    $scope.GetGWPDblClick = function (args) {
        $scope.ModelPCNew = Object.assign({}, args.data);
        $scope.GetLoadEmployeeInformation();
        $scope.Action = 'Update';
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

    //***********************************Payable Creation End********************************************************//
}