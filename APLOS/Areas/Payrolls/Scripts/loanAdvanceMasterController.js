'use strict';
loanAdvanceMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function loanAdvanceMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Salary Advance";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.masterDataList = [];
    $scope.path = 'payrolls/loanadvancemaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.loanAdvanceMaster = {
        SystemID: null,
        EmpInfoSystemID: null,
        SalaryHeadID: null,
        CurrencyRuleSystemID: null,
        FromMonthNo: null,
        FromYearNo: null,
        EntryCurrencyID: null,
        AdvanceAmount: 0,
        DefineCurrencyID: null,
        DefineAmount: 0,
        DisbustCurrencyID: null,
        PaidAmount: null,
        AmtDefinitionCurrencyID: null,
        AmtDefinitionRate: 0,
        IsFixedAmount: false,
        IsEqualMonthAmount: false,
        IsInterestApplicable: false,
        InterestPercentageAmount: 0,
        InstallmentAmount: 0,
        InstallmentMonth: 0,
        IsDisbusted: false,
        CurrencyRuleSystemID: null,
        PlantId: null,
        BalanceAmount: null
    };
    $scope.loanAdvanceNew = Object.assign({}, $scope.loanAdvanceMaster);
    $scope.LoanAdvanceChild = {
        SystemID: null,
        LoanMstSystemID: null,
        MonthNo: null,
        YearNo: null,
        MonthlyAdjAmount: null,
        PaidAmount: null,
        BalanceAmount: null,
        IsDisbusted: false,
        SequenceNo: 0
    };
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });
    $scope.AddSalaryAdvance = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.EmployeeId)) {
                throw "First select an employee.";
            }
            $scope.Action = 'Save';
            //$scope.loanAdvanceNew.FromYearNo = null;
            //$scope.loanAdvanceNew.FromMonthNo = null;
            $scope.loanAdvanceNew.StartDate = null;
            $scope.loanAdvanceNew.SalaryHeadID = null;
            $scope.loanAdvanceNew.AmtDefinitionRate = null;
            $scope.loanAdvanceNew.AdvanceAmount = null;
            $scope.loanAdvanceNew.PaidAmount = null;
            $scope.loanAdvanceNew.BalanceAmount = null;
            $scope.loanAdvanceNew.InstallmentAmount = null;
            $scope.loanAdvanceNew.InstallmentMonth = null;
            $scope.loanAdvanceNew.IsFixedAmount = null;
            $scope.loanAdvanceNew.IsEqualMonthAmount = null;
            $scope.loanAdvanceNew.SystemID = null;
            angular.element(document.querySelector('#SalaryAdvancePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure')
        }
    };
    $scope.popUpList = [];
    $scope.popUp = function (name) {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: '',
            searchBy: '',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        try {
            if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.PlantId)) {
                throw "First select plant.";
            }
            $scope.popUpUrl = '';
            $scope.popUpParameters.sort = '';
            $scope.popUpParameters.searchBy = '';
            if (name === 'EmployeeInformation') {
                $scope.popUpTitle = 'Employee Information';
                $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatalist?plantId=' + $scope.loanAdvanceNew.PlantId;
                $scope.popUpParameters.sort = 'EmployeeName';
                $scope.popUpParameters.searchBy = 'EmployeeName';
            }
            $scope.popUpData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                    .then(function (result) {
                        $scope.popUpDataList = result.Rows;
                        $scope.popUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.popUpList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };

            $scope.fieldName = name;
            angular.element(document.querySelector('#popUp')).modal('show');
            $scope.popUpData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.loanAdvance = {};
    $scope.selectdblClick = function (data) {
        setPartyName(data);
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    function setPartyName(ob) {
        try {
            if ($scope.fieldName === 'EmployeeInformation') {
                $scope.loanAdvanceNew.EmployeeId = ob.SystemId;
                $scope.loanAdvanceNew.EmpInfoSystemID = $scope.loanAdvanceNew.EmployeeId;
                $scope.loanAdvance.EmployeeName = ob.EmployeeName;
                $scope.loanAdvance.EmployeeCode = ob.EmployeeCode;
                $scope.loanAdvance.EmailId = ob.EmailId;
                $scope.loanAdvance.BudgetCode = ob.BudgetCode;
                $scope.loanAdvance.Designation = ob.Designation;
                $scope.loanAdvance.Department = ob.Department;
                $scope.loanAdvance.EmpPicPath = ob.EmpPicPath;
                $scope.loanAdvance.CurrencyRuleSystemID = ob.CurrencyRuleSystemID;
                $scope.loanAdvance.SalaryRuleName = ob.SalaryRuleName;

                $scope.imageSrc = virtualPath.EmployeePic + '/' + $scope.loanAdvance.EmpPicPath;

                if (baseService.isUndefinedOrNull($scope.loanAdvance.SalaryRuleName)) {
                    throw "Employee Code [" + $scope.loanAdvance.EmployeeCode + "] is not under any Salary Rule.";
                }
                cboService.getSalaryHeadCbo($scope.loanAdvanceNew.CurrencyRuleSystemID, function (result) {
                    console.log('result',result);
                    $scope.salaryHeadList = result;
                    if ($scope.salaryHeadList.lenght > 0) {
                        $scope.loanAdvanceNew.SalaryHeadID = $scope.salaryHeadList[0].Value;
                    }
                });

                $scope.GetLoanMasterByEmployee($scope.loanAdvanceNew.EmployeeId);
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.GetLoanMasterByEmployee = function (employeeId) {
        $http.get('payrolls/loanadvancemaster/getloanmasterbyemployee?employeeId=' + employeeId)
            .then(function (response) {
                $scope.masterDataList = response.data;
                if ($scope.masterDataList.length > 0) {
                    $scope.loanAdvanceNew.FromYearNo = $scope.masterDataList[0].FromYearNo;
                    $scope.loanAdvanceNew.FromMonthNo = $scope.masterDataList[0].FromMonthNo;
                    $scope.loanAdvanceNew.StartDate = $scope.masterDataList[0].StartDate;
                    $scope.loanAdvanceNew.SalaryHeadID = $scope.masterDataList[0].SalaryHeadID;
                    $scope.loanAdvanceNew.AmtDefinitionRate = $scope.masterDataList[0].AmtDefinitionRate;
                    $scope.loanAdvanceNew.AdvanceAmount = $scope.masterDataList[0].AdvanceAmount;
                    $scope.loanAdvanceNew.InstallmentAmount = $scope.masterDataList[0].InstallmentAmount;
                    $scope.loanAdvanceNew.InstallmentMonth = $scope.masterDataList[0].InstallmentMonth;
                    $scope.loanAdvanceNew.IsFixedAmount = $scope.masterDataList[0].IsFixedAmount;
                    $scope.loanAdvanceNew.IsEqualMonthAmount = $scope.masterDataList[0].IsEqualMonthAmount;
                    $scope.loanAdvanceNew.SystemID = $scope.masterDataList[0].SystemID;
                    $scope.loanAdvanceNew.EmpInfoSystemID = $scope.masterDataList[0].EmpInfoSystemID;
                    // $scope.GetLoanChildByMaster($scope.masterDataList[0].SystemID);
                } else {
                    $scope.loanAdvanceNew.FromYearNo = null;
                    $scope.loanAdvanceNew.FromMonthNo = null;
                    $scope.loanAdvanceNew.StartDate = null;
                    $scope.loanAdvanceNew.SalaryHeadID = null;
                    $scope.loanAdvanceNew.AmtDefinitionRate = null;
                    $scope.loanAdvanceNew.AdvanceAmount = null;
                    $scope.loanAdvanceNew.InstallmentAmount = null;
                    $scope.loanAdvanceNew.InstallmentMonth = null;
                    $scope.loanAdvanceNew.IsFixedAmount = null;
                    $scope.loanAdvanceNew.IsEqualMonthAmount = null;
                    $scope.loanAdvanceNew.SystemID = null;
                    $scope.loanAdvanceNew.EmpInfoSystemID = null;
                    $scope.childDataList = [];
                    angular.element(document.querySelector('#SalaryAdvanceDetailsPopUp')).modal('hide');
                }
            });
    };
    $scope.GetLoanChildByMaster = function (masterId) {
        $http.get('payrolls/loanadvancemaster/getloanchildbymaster?masterId=' + masterId)
            .then(function (response) {
                $scope.childDataList = response.data;
                angular.element(document.querySelector('#SalaryAdvanceDetailsPopUp')).modal('show');
            });
    };
    $scope.GetLoanChildByMasterEdit = function (masterId) {
        $http.get('payrolls/loanadvancemaster/getloanchildbymaster?masterId=' + masterId)
            .then(function (response) {
                $scope.childDataList = response.data;
                $scope.Action = 'Update';
                angular.element(document.querySelector('#SalaryAdvancePopUp')).modal('show');
               // angular.element(document.querySelector('#SalaryAdvanceDetailsPopUp')).modal('show');
            });
    };

    $scope.EditSalaryAdvance = function (id, index) {
        $scope.index = index;
        $scope.loanAdvanceMaster = $scope.masterDataList[$scope.index];
        $scope.loanAdvanceNew = Object.assign({}, $scope.loanAdvanceMaster);
        //console.log('$scope.loanAdvanceNew',$scope.loanAdvanceNew);
        //console.log('$scope.loanAdvanceMaster', $scope.loanAdvanceMaster);
        $scope.GetLoanChildByMasterEdit($scope.loanAdvanceNew.SystemID);
        //$scope.Action = 'Update';
        //angular.element(document.querySelector('#SalaryAdvancePopUp')).modal('show');
    };
    $scope.SelectByButton = function () {
        if ($scope.valueData === '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    //$scope.yearList = [];
    //$scope.SetYearOfPassing = function () {
    //    $http.get('payrolls/loanadvancemaster/getyear?plantId=' + $scope.loanAdvanceNew.PlantId)
    //        .then(function (response) {
    //            $scope.yearDataList = response.data;
    //            $scope.startingYear = $scope.yearDataList[0].CutOffDate;
    //            var startyear = new Date($scope.startingYear);
    //            var yeardata = startyear.getFullYear();
    //            var endYear = new Date();
    //            var ey = parseInt(endYear.getFullYear());
    //            for (var i = ey; i > yeardata - 1; i--) {
    //                var ob = {
    //                    Value: i,
    //                    Text: i
    //                };
    //                $scope.yearList.push(ob);
    //            }
    //        });
    //};
    $scope.monthList = [
        {
            'Value': "1",
            'Text': "January"
        },
        {
            'Value': "2",
            'Text': "February"
        },
        {
            'Value': "3",
            'Text': "March"
        },
        {
            'Value': "4",
            'Text': "April"
        },
        {
            'Value': "5",
            'Text': "May"
        },
        {
            'Value': "6",
            'Text': "June"
        },
        {
            'Value': "7",
            'Text': "July"
        },
        {
            'Value': "8",
            'Text': "August"
        },
        {
            'Value': "9",
            'Text': "September"
        },
        {
            'Value': "10",
            'Text': "October"
        },
        {
            'Value': "11",
            'Text': "November"
        },
        {
            'Value': "12",
            'Text': "December"
        }
    ];
    $scope.SetFixedAmountChecked = function (event) {
        $scope.loanAdvanceNew.IsEqualMonthAmount = false;
        $scope.loanAdvanceNew.IsFixedAmount = event.currentTarget.checked;
        $scope.loanAdvanceNew.InstallmentMonth = null;
    };
    $scope.SetEqualMonthAmountChecked = function (event) {
        $scope.loanAdvanceNew.IsFixedAmount = false;
        $scope.loanAdvanceNew.IsEqualMonthAmount = event.currentTarget.checked;
        $scope.loanAdvanceNew.InstallmentAmount = null;
    };
    //var BalanceAdvAmt = 0;
    //var BalanceAdvAmt = $scope.loanAdvanceNew.BalanceAmount;
    var PaindingInsMonth = 0;
    var EntInstallAmt = 0;
    var InstallAmt = 0;
    var PaidAmt = 0;
    var monthNames = ["January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];
    //$scope.childDataList = [];
    var SrNo = 0;
    var datestart = $scope.loanAdvanceNew.StartDate;
    var getDate = new Date(datestart);
    //BalanceAdvAmt = $scope.loanAdvanceNew.AdvanceAmount - PaidAmt;
    //$scope.Define = function () {
    //    $scope.validateDefine();
    //    $scope.getInstallMonth();
    //    $scope.getInstallAmount();
    //};

    $scope.validateDefine = function () {
        if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.StartDate)) {
            throw "Start Date is required.";
        }
        if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.AdvanceAmount)) {
            throw "Advance Amount is required.";
        }
        //if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.PaidAmount)) {
        //    throw "Paid Amount can not be blank.";
        //}
        if (parseInt($scope.loanAdvanceNew.PaidAmount) < 0) {
            throw 'Paid Amount can\'t be less than 0.';
        }
        if (parseInt($scope.loanAdvanceNew.AdvanceAmount) < 0) {
            throw 'Advance Amount can\'t be less than 0.';
        }
        if ((parseInt($scope.loanAdvanceNew.AdvanceAmount) - parseInt($scope.loanAdvanceNew.PaidAmount)) < 0) {
            throw 'Paid Amount can\'t be greater than Advance Amount.';
        }
    }

    $scope.getInstallMonth = function () {
        $scope.childDataList = [];
        try {
            $scope.validateDefine();
            //if (!baseService.isUndefinedOrNull($scope.loanAdvanceNew.InstallmentAmount) && parseInt($scope.loanAdvanceNew.InstallmentAmount) > 0) {
            var BalanceAdvAmt = $scope.loanAdvanceNew.BalanceAmount;
            var pendingMonth = parseInt(BalanceAdvAmt) / parseInt($scope.loanAdvanceNew.InstallmentAmount);
            var PaindingInsMonth = Math.ceil(pendingMonth);
            $scope.loanAdvanceNew.InstallmentMonth = PaindingInsMonth;
            EntInstallAmt = $scope.loanAdvanceNew.InstallmentAmount;
            var InstallAmt = EntInstallAmt;
            var SrNo = 0;
            // $scope.loanAdvanceNew.InstallmentAmount = InstallAmt;
            var PaidAmt = 0;
            for (var i = 0; i < PaindingInsMonth; i++) {
                SrNo = SrNo + 1;
                PaidAmt += parseInt(InstallAmt);
                if (BalanceAdvAmt > InstallAmt) {
                    BalanceAdvAmt -= InstallAmt;
                }
                //if (i === PaindingInsMonth - 1 && BalanceAdvAmt !== 0) {
                //    PaindingInsMonth += 1;
                //}
                var mn = getDate.getMonth();
                var ChildData = {
                    SystemID: null,
                    MonthNo: mn + 1,
                    MonthName: monthNames[getDate.getMonth()],
                    YearNo: getDate.getFullYear(),
                    //DefMonthlyAdjAmount: null,
                    MonthlyAdjAmount: InstallAmt,
                    PaidAmount: PaidAmt,
                    BalanceAmount: BalanceAdvAmt,
                    IsDisbusted: false,
                    SequenceNo: SrNo
                };
                $scope.childDataList.push(ChildData);

                if (BalanceAdvAmt <= InstallAmt) {
                    InstallAmt = BalanceAdvAmt;
                    BalanceAdvAmt = 0;
                }
                getDate.setMonth(getDate.getMonth() + 1);
            }

            // $scope.loanAdvanceNew.InstallmentMonth = PaindingInsMonth;
            //}
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.getInstallAmount = function () {
        $scope.childDataList = [];
        try {
            $scope.validateDefine();
            //if (!baseService.isUndefinedOrNull($scope.loanAdvanceNew.InstallmentMonth) && parseInt($scope.loanAdvanceNew.InstallmentMonth) > 0) {
            var PaindingInsMonth = parseInt($scope.loanAdvanceNew.InstallmentMonth);
            var BalanceAdvAmt = $scope.loanAdvanceNew.BalanceAmount;
            var EntInstallAmt = parseInt($scope.loanAdvanceNew.BalanceAmount) / PaindingInsMonth;
            var InstallAmt = EntInstallAmt;
            var SrNo = 0;
            $scope.loanAdvanceNew.InstallmentAmount = InstallAmt;
            var PaidAmt = 0;
            for (var i = 0; i < PaindingInsMonth; i++) {
                SrNo = SrNo + 1;
                PaidAmt += parseInt(InstallAmt);
                if (BalanceAdvAmt > InstallAmt) {
                    BalanceAdvAmt -= InstallAmt;
                }
                var mn = getDate.getMonth();
                var ChildData = {
                    SystemID: null,
                    MonthNo: mn + 1,
                    MonthName: monthNames[getDate.getMonth()],
                    YearNo: getDate.getFullYear(),
                    //DefMonthlyAdjAmount: null,
                    MonthlyAdjAmount: InstallAmt,
                    PaidAmount: PaidAmt,
                    BalanceAmount: BalanceAdvAmt,
                    IsDisbusted: false,
                    SequenceNo: SrNo
                };
                $scope.childDataList.push(ChildData);

                if (BalanceAdvAmt <= InstallAmt) {
                    InstallAmt = BalanceAdvAmt;
                    BalanceAdvAmt = 0;
                }
                getDate.setMonth(getDate.getMonth() + 1);
            }
            //}
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.showBalanceAmt = function () {
        $scope.loanAdvanceNew.InstallmentMonth = 0;
        $scope.loanAdvanceNew.InstallmentAmount = 0;
        var _paid_amount = 0;
        var _advance_amount = 0;
        if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.PaidAmount) == false) {
            _paid_amount = $scope.loanAdvanceNew.PaidAmount;
        }

        if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.AdvanceAmount) == false) {
            _advance_amount = $scope.loanAdvanceNew.AdvanceAmount;
        }

        var balAmt = parseInt(_advance_amount) - parseInt(_paid_amount);
        $scope.loanAdvanceNew.BalanceAmount = balAmt;
    }

    $scope.Save = function () {
        try {
            //if ($scope.loanAdvanceNew.IsFixedAmount === true && baseService.isUndefinedOrNull($scope.loanAdvanceNew.InstallmentAmount)) {
            //    throw "Installment Amount is required.";
            //}
            //if ($scope.loanAdvanceNew.IsEqualMonthAmount === true && baseService.isUndefinedOrNull($scope.loanAdvanceNew.InstallmentMonth)) {
            //    throw "Installment Month is required.";
            //}

            //if (parseFloat($scope.loanAdvanceNew.AdvanceAmount) < parseFloat($scope.loanAdvanceNew.InstallmentAmount)) {
            //    throw "Installment Amount can\'t be greater then Advance Amount.";
            //}

            //$scope.Define();

            //$scope.$broadcast('show-errors-check-validity');
            //if ($scope.loanAdvanceNewForm2.$valid) {
            angular.copy($scope.loanAdvanceNew, $scope.loanAdvanceMaster);
            console.log('xx', $scope.loanAdvanceNew);
            $scope.loanAdvanceMaster.EmpInfoSystemID = $scope.loanAdvanceNew.EmpInfoSystemID;
                if ($scope.Action === "Save" || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'loanAdvanceMaster': $scope.loanAdvanceMaster, 'loanAdvanceChild': $scope.childDataList },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'SalaryAdvancePopUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            angular.element(document.querySelector('#SalaryAdvancePopUp')).modal('hide');
                            $scope.GetLoanMasterByEmployee(response.data.LoanAdvanceMaster.EmpInfoSystemID);
                            $scope.loanAdvanceNew.PlantId = response.data.LoanAdvanceMaster.PlantId;
                            //$scope.popUp('EmployeeInformation');
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'SalaryAdvancePopUp');
                    };
                }
            //}
        } catch (e) {
            ShowResult(e, 'failure', 'SalaryAdvancePopUp');
        }
    };
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.loanAdvanceMaster = {};
        $scope.loanAdvanceNew = {};
    }

    $scope.commented = function () {
        //$scope.Define = function () {
        //    if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.FromYearNo)) {
        //        throw "Starting Year is required.";
        //    }
        //    if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.FromMonthNo)) {
        //        throw "Starting Month is required.";
        //    }
        //    //if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.AmtDefinitionRate)) {
        //    //    throw "Rate is required.";
        //    //}
        //    if (baseService.isUndefinedOrNull($scope.loanAdvanceNew.AdvanceAmount)) {
        //        throw "Advance Amount is required.";
        //    }
        //    if ($scope.loanAdvanceNew.IsFixedAmount === true && baseService.isUndefinedOrNull($scope.loanAdvanceNew.InstallmentAmount)) {
        //        throw "Installment Amount is required.";
        //    }
        //    if ($scope.loanAdvanceNew.IsEqualMonthAmount === true && baseService.isUndefinedOrNull($scope.loanAdvanceNew.InstallmentMonth)) {
        //        throw "Installment Month is required.";
        //    }

        //    var SrNo = 0;

        //    var datestart = $scope.loanAdvanceNew.FromMonthNo + "-" + "01-" + $scope.loanAdvanceNew.FromYearNo;
        //    var getDate = new Date(datestart);

        //    BalanceAdvAmt = $scope.loanAdvanceNew.AdvanceAmount - PaidAmt;

        //    if ($scope.loanAdvanceNew.IsFixedAmount === true) {
        //        var pendingMonth = parseInt(BalanceAdvAmt) / parseInt($scope.loanAdvanceNew.InstallmentAmount);
        //        PaindingInsMonth = Math.ceil(pendingMonth);
        //        EntInstallAmt = $scope.loanAdvanceNew.InstallmentAmount;

        //        //if (!baseService.isUndefinedOrNull($scope.loanAdvanceNew.DefinitionCurrencyID) === !baseService.isUndefinedOrNull($scope.loanAdvanceNew.AmtDefinitionCurrencyID) && !baseService.isUndefinedOrNull($scope.loanAdvanceNew.AmtDisbusmentCurrency) === !baseService.isUndefinedOrNull($scope.loanAdvanceNew.LocalCurrency)) {
        //        //    InstallAmt = EntInstallAmt * $scope.loanAdvanceNew.AmtDefinitionRate;
        //        //}
        //        //else if (!baseService.isUndefinedOrNull($scope.loanAdvanceNew.DisbustCurrencyID) === !baseService.isUndefinedOrNull($scope.loanAdvanceNew.AmtDefinitionCurrencyID) && !baseService.isUndefinedOrNull($scope.loanAdvanceNew.DefinitionCurrencyID) === !baseService.isUndefinedOrNull($scope.loanAdvanceNew.LocalCurrency)) {
        //        //    InstallAmt = EntInstallAmt / $scope.loanAdvanceNew.AmtDefinitionRate;
        //        //}
        //        //else {
        //        InstallAmt = EntInstallAmt;
        //        // }

        //        $scope.loanAdvanceNew.InstallmentAmount = InstallAmt;

        //        for (var i = 0; i < PaindingInsMonth; i++) {
        //            SrNo = SrNo + 1;
        //            PaidAmt += parseInt(InstallAmt);
        //            if (BalanceAdvAmt > InstallAmt) {
        //                BalanceAdvAmt -= InstallAmt;
        //            }
        //            if (i === PaindingInsMonth - 1 && BalanceAdvAmt !== 0) {
        //                PaindingInsMonth += 1;
        //            }
        //          var  mn = getDate.getMonth();
        //            var ChildData = {
        //                SystemID: null,
        //                MonthNo: mn + 1,
        //                MonthName: monthNames[getDate.getMonth()],
        //                YearNo: getDate.getFullYear(),
        //                //DefMonthlyAdjAmount: null,
        //                MonthlyAdjAmount: InstallAmt,
        //                PaidAmount: PaidAmt,
        //                BalanceAmount: BalanceAdvAmt,
        //                IsDisbusted: false,
        //                SequenceNo: SrNo
        //            };
        //            $scope.childDataList.push(ChildData);

        //            if (BalanceAdvAmt <= InstallAmt) {
        //                InstallAmt = BalanceAdvAmt;
        //                BalanceAdvAmt = 0;
        //            }
        //            getDate.setMonth(getDate.getMonth() + 1);
        //        }

        //        $scope.loanAdvanceNew.InstallmentMonth = PaindingInsMonth;
        //    }
        //    else if ($scope.loanAdvanceNew.IsEqualMonthAmount === true) {
        //        PaindingInsMonth = $scope.loanAdvanceNew.InstallmentMonth;

        //        EntInstallAmt = BalanceAdvAmt / PaindingInsMonth;

        //        //if (!baseService.isUndefinedOrNull($scope.loanAdvanceNew.DefinitionCurrencyID) === !baseService.isUndefinedOrNull($scope.loanAdvanceNew.AmtDefinitionCurrencyID) && !baseService.isUndefinedOrNull($scope.loanAdvanceNew.AmtDisbusmentCurrency) === !baseService.isUndefinedOrNull($scope.loanAdvanceNew.LocalCurrency)) {
        //        //    InstallAmt = EntInstallAmt * $scope.loanAdvanceNew.AmtDefinitionRate;
        //        //}
        //        //else if (!baseService.isUndefinedOrNull($scope.loanAdvanceNew.DisbustCurrencyID) === !baseService.isUndefinedOrNull($scope.loanAdvanceNew.AmtDefinitionCurrencyID) && !baseService.isUndefinedOrNull($scope.loanAdvanceNew.DefinitionCurrencyID) === !baseService.isUndefinedOrNull($scope.loanAdvanceNew.LocalCurrency)) {
        //        //    InstallAmt = EntInstallAmt / $scope.loanAdvanceNew.AmtDefinitionRate;
        //        //}
        //        //else {
        //        InstallAmt = EntInstallAmt;
        //        // }

        //        $scope.loanAdvanceNew.InstallmentAmount = InstallAmt;

        //        for (var i = 0; i < PaindingInsMonth; i++) {
        //            SrNo = SrNo + 1;
        //            PaidAmt += parseInt(InstallAmt);
        //            if (BalanceAdvAmt > InstallAmt) {
        //                BalanceAdvAmt -= InstallAmt;
        //            }
        //            var mn = getDate.getMonth();
        //            var ChildData = {
        //                SystemID: null,
        //                MonthNo: mn + 1,
        //                MonthName: monthNames[getDate.getMonth()],
        //                YearNo: getDate.getFullYear(),
        //                //DefMonthlyAdjAmount: null,
        //                MonthlyAdjAmount: InstallAmt,
        //                PaidAmount: PaidAmt,
        //                BalanceAmount: BalanceAdvAmt,
        //                IsDisbusted: false,
        //                SequenceNo: SrNo
        //            };
        //            $scope.childDataList.push(ChildData);

        //            if (BalanceAdvAmt <= InstallAmt) {
        //                InstallAmt = BalanceAdvAmt;
        //                BalanceAdvAmt = 0;
        //            }
        //            getDate.setMonth(getDate.getMonth() + 1);
        //        }
        //    }
        //};
    }
}