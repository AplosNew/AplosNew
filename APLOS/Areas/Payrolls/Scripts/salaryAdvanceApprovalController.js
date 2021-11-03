'use strict';
salaryAdvanceApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function salaryAdvanceApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Salary Advance Approval';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.salaryAdvanceApprovals = [];
    $scope.path = 'payrolls/salaryadvanceapproval/';
    $scope.getListUrl = $scope.path + 'getloanadvanceinfoplantwise';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'updatesalaryapprovaldetails';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'SystemID', 'SystemID');
    $scope.salaryAdvanceParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'SystemID',
        searchBy: "SystemID",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.message = '';
    $scope.xLoadData = function () {
        try {
            $scope.GLUrl3 = 'payrolls/salaryadvanceapproval/GetList?plantId=' + $scope.salaryAdvanceApprovalNew.PlantId,
                $scope.LoadDataList = function (pageno) {
                    baseService.paginationBase($scope.GLUrl3, pageno, $scope.salaryAdvanceParameters)
                        .then(function (data) {
                            if (data.Error) {
                                return $scope.message = data.Message;
                            } else {
                                $scope.salaryAdvanceApprovals = data.Data.Rows;
                                $rootScope.total_count = data.Data.Total;
                                $scope.message = data.Message;
                            }
                            $scope.salaryAdvanceParameters.search = null;
                            for (var i = 0; i < $scope.salaryAdvanceApprovals.length; i++) {
                                $scope.salaryAdvanceApprovals[i].Active = getActive($scope.tempList, $scope.salaryAdvanceApprovals[i].SystemID);
                            }
                        }, function () {
                            ShowResult(commonMessage.NetworkError, 'failure');
                        }).finally(function () {
                        });
                };
            $scope.LoadDataList();
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.LoadData = function () {
        try {
            $scope.salaryAdvanceApprovals = [];
            if (baseService.isUndefinedOrNull($scope.salaryAdvanceApprovalNew.PlantId) === false) {
                $scope.GLUrl3 = 'payrolls/salaryadvanceapproval/GetList?plantId=' + $scope.salaryAdvanceApprovalNew.PlantId,
                    $scope.LoadDataList = function (pageno) {
                        baseService.paginationBase($scope.GLUrl3, pageno, $scope.salaryAdvanceParameters)
                            .then(function (data) {
                                if (data.Error) {
                                    return $scope.message = data.Message;
                                } else {
                                    $scope.salaryAdvanceApprovals = data.Data.Rows;
                                    $rootScope.total_count = data.Data.Total;
                                    $scope.message = data.Message;
                                }
                                $scope.salaryAdvanceParameters.search = null;
                                for (var i = 0; i < $scope.salaryAdvanceApprovals.length; i++) {
                                    $scope.salaryAdvanceApprovals[i].Active = getActive($scope.tempList, $scope.salaryAdvanceApprovals[i].SystemID);
                                }
                            }, function () {
                                ShowResult(commonMessage.NetworkError, 'failure');
                            }).finally(function () {
                            });
                    };
                $scope.LoadDataList();
            }//if plant is not blank
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.pushValue = function (data) {
        angular.forEach($scope.tempList, function (item, i) {
            if (item.SystemID === data.SystemID) {
                $scope.tempList[i].ApprovalStatus = data.ApprovalStatus;
            }
        });
    }
    $scope.tempInputValue = [];
    $scope.getInputValue = function () {
        for (var i = 0; i < $scope.salaryAdvanceApprovals.length; i++) {
            $scope.tempInputValue.push($scope.salaryAdvanceApprovals[i]);
        }
    }
    $scope.validation = function (list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Active) {
                if (baseService.isUndefinedOrNull(list[i].ApprovalStatus)) {
                    throw "Status can\'t be Blank for [ " + $scope.salaryAdvanceApprovals[i].EmployeeName + " ] ";
                }
            }
        }
    }
    $scope.validateMainList = function () {
        for (var i = 0; i < $scope.salaryAdvanceApprovals.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.salaryAdvanceApprovals[i].ApprovalStatus) && $scope.salaryAdvanceApprovals[i].Active === false) {
                throw "Please select Checkbox for [ " + $scope.salaryAdvanceApprovals[i].EmployeeName + " ] ";
            }
        }
    }
    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.SystemID) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].SystemID === data.SystemID) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistTempList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemID === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemID === id) {
                return true;
            }
        }
        return false;
    }
    $scope.ApprovalList = [];
    cboService.getEnumCbo("enum/GetSalaryApprovalStatusCbo", function (result) {
        $scope.ApprovalList = result;
    });
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });
    $scope.salaryAdvanceApproval = {
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
        PaidAmount: 0,
        AmtDefinitionCurrencyID: null,
        AmtDefinitionRate: 0,
        IsFixedAmount: false,
        IsEqualMonthAmount: false,
        IsInterestApplicable: false,
        InterestPercentageAmount: 0,
        InstallmentAmount: 0,
        InstallmentMonth: 0,
        IsDisbusted: false,
        ApprovalStatus: null,
        //CurrencyRuleSystemID:null
        PlantId: null
    };
    $scope.salaryAdvanceApprovalNew = Object.assign({}, $scope.salaryAdvanceApproval);
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
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.salaryAdvanceApproval = $scope.salaryAdvanceApprovals[$scope.index];
        $scope.salaryAdvanceApprovalNew = Object.assign({}, $scope.salaryAdvanceApproval);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.showEntityPopUp = function () {
        $http.get('employees/prerecruitmentdocumentbydepartment/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    };
    $scope.Save = function () {
        try {
            $scope.validation($scope.tempList);
            $scope.validateMainList();
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'models': $scope.tempList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.LoadData();
                        $scope.tempList = [];
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
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
}