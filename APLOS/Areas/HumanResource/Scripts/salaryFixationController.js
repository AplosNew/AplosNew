'use strict';
salaryFixationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function salaryFixationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Salary Fixation";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SalaryFixation = [];
    $scope.salaryHeadListMonth = [];
    $scope.salaryHeadListLeave = [];
    $scope.salaryHeadListAc = [];
    $scope.salaryHeadListAnc = [];
    $scope.path = 'humanresource/salaryfixation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.mailUrl = $scope.path + 'sendmail';
    $scope.calculateUrl = $scope.path + 'calculate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.showEntityPopUp = function () {
        $http.get('employees/prerecruitmentdocumentbydepartment/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    };

    $scope.searchInEmpList = [
        {
            'name': 'Candidate Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Candidate Name',
            'value': 'FullName'
        },
        {
            'name': 'Email',
            'value': 'Email'
        },
        {
            'name': 'Budget Code',
            'value': 'Code'
        },
        {
            'name': 'Given Designation',
            'value': 'GivenDesignation'
        },
        {
            'name': 'Department',
            'value': 'Department'
        }];

    $scope.employeePopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode,FullName',
        searchBy: 'FullName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.message = "";
    $scope.getActiveApprovedEmployee = function (pageno) {
        baseService.paginationBase('humanresource/salaryfixation/getemployees', pageno, $scope.employeePopUpParameters)
            .then(function (data) {
                if (data.Error) {
                    return $scope.message = data.Message;
                } else {
                    $scope.dataListEmployee = data.Data.Rows;
                    $scope.employeePopUpParameters.total_count = data.Data.Total;
                    $scope.message = data.Message;
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        angular.element(document.querySelector('#PopUpEmployee')).modal('show');
    };

    $scope.getEmployeeDataOnLabels = function (emp) {
        $scope.SalaryFixation.PreRecruitmentEmployeeID = emp.PreRecruitmentEmployeeID;
        $scope.imageSrc = virtualPath.EmpPic + emp.Image;
        $scope.SalaryFixation.EmployeeCode = emp.EmployeeCode;
        $scope.SalaryFixation.FullName = emp.FullName;
        $scope.SalaryFixation.Email = emp.Email;
        $scope.SalaryFixation.BudgetId = emp.BudgetId;
        $scope.SalaryFixation.Code = emp.Code;
        $scope.SalaryFixation.Department = emp.Department;
        $scope.SalaryFixation.GivenDesignation = emp.GivenDesignation;
        $scope.SalaryFixation.GivenDesignationId = emp.GivenDesignationId;
        $scope.SalaryFixation.SalaryRuleName = emp.SalaryRuleName;
        $scope.SalaryFixation.SalaryRuleId = emp.SalaryRuleId;
        $scope.SalaryFixation.TotalSalary = emp.TotalSalary;
        $scope.SalaryFixation.PlantId = emp.PlantId;
        if (!baseService.isUndefinedOrNull(emp.SalaryRuleId)) {
            $scope.SalaryFixation.Formula = emp.Formula.split("#");// emp.Formula.replace("#", "\n");
            $scope.GetSalaryHeadsDataList();
        }
            $scope.GettermsAndConditions();
        angular.element(document.querySelector('#PopUpEmployee')).modal('hide');
    };

    $scope.GetSalaryHeadsDataList = function () {
        $http.get('humanresource/salaryfixation/getcalculationinfo?preRecruitmentEmployeeId=' + $scope.SalaryFixation.PreRecruitmentEmployeeID + '&givenDesignationId=' + $scope.SalaryFixation.GivenDesignationId + '&plantId=' + $scope.SalaryFixation.PlantId)
            .then(function (response) {
                $scope.salaryHeadList = response.data;
                DistributeList($scope.salaryHeadList);
            });
    };

    $scope.employeeWiseTermsAndConditions = {
        Id: null,
        PreRecruitmentEmployeeId: null,
        Description1: null,
        Description2: null
    };

    $scope.GettermsAndConditions = function () {
        $http.get('humanresource/salaryfixation/gettermsandconditionsbyemployee?preRecruitmentEmployeeid=' + $scope.SalaryFixation.PreRecruitmentEmployeeID)
            .then(function (response) {
                $scope.termsAndconditions = response.data[0];
                $scope.employeeWiseTermsAndConditions = $scope.termsAndconditions;

                if (baseService.isUndefinedOrNull($scope.employeeWiseTermsAndConditions)) {
                    $http.get('humanresource/salaryfixation/gettermsandconditionsbyplant?plantId=' + $scope.SalaryFixation.PlantId)
                        .then(function (response) {
                            $scope.termsAndconditions = response.data[0];
                            $scope.employeeWiseTermsAndConditions = $scope.termsAndconditions;
                        });
                }
            });
        $scope.employeeWiseTermsAndConditions.PreRecruitmentEmployeeId = $scope.SalaryFixation.PreRecruitmentEmployeeID;
    };
   
    function DistributeList(mainlist) {
        $scope.salaryHeadListMonth = [];
        $scope.salaryHeadListLeave = [];
        $scope.salaryHeadListAc = [];
        $scope.salaryHeadListAnc = [];
        try {
            for (var i = 0; i < baseService.arrayLength(mainlist); i++) {
                if (mainlist[i].IsMonthly) {
                    $scope.salaryHeadListMonth.push(mainlist[i]);
                }
                else if (mainlist[i].IsAnnualCash) {
                    $scope.salaryHeadListAc.push(mainlist[i]);
                }
                else if (mainlist[i].IsAnnualNonCash) {
                    $scope.salaryHeadListAnc.push(mainlist[i]);
                }
                else {
                    $scope.salaryHeadListLeave.push(mainlist[i]);
                }
            }
            $scope.salaryHeadListMonth = sortObj($scope.salaryHeadListMonth, 'SequenceNo');
            $scope.salaryHeadListLeave = sortObj($scope.salaryHeadListLeave, 'SequenceNo');
            $scope.salaryHeadListAc = sortObj($scope.salaryHeadListAc, 'SequenceNo');
            $scope.salaryHeadListAnc = sortObj($scope.salaryHeadListAnc, 'SequenceNo');
        } catch (e) {
            throw e;
        }
    }
    function CombineList(mainlist) {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.salaryHeadListMonth); i++) {
                mainlist.push($scope.salaryHeadListMonth[i]);
            }
            for (var i = 0; i < baseService.arrayLength($scope.salaryHeadListAc); i++) {
                mainlist.push($scope.salaryHeadListAc[i]);
            }
            for (var i = 0; i < baseService.arrayLength($scope.salaryHeadListAnc); i++) {
                mainlist.push($scope.salaryHeadListAnc[i]);
            }
            for (var i = 0; i < baseService.arrayLength($scope.salaryHeadListLeave); i++) {
                mainlist.push($scope.salaryHeadListLeave[i]);
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.SaveAsUpdate = function (ismail) {
        $scope.salaryHeadList = [];
        CombineList($scope.salaryHeadList);
        for (var i = 0; i < $scope.salaryHeadList.length; i++) {
            $scope.salaryHeadList[i].PreRecruitmentEmployeeID = $scope.SalaryFixation.PreRecruitmentEmployeeID;
            $scope.salaryHeadList[i].SalaryRuleId = $scope.SalaryFixation.SalaryRuleId;
        }
        if ($scope.Action === "Save") {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'salaryFixationList': $scope.salaryHeadList, 'plantid': $scope.SalaryFixation.PlantId,
                    'employeeWiseTermsAndConditions': $scope.employeeWiseTermsAndConditions, 'ismail': ismail
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SalaryFixation.push(response.data.SalaryFixationList);
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    };
    function sortObj(list, key) {
        function compare(a, b) {
            a = a[key];
            b = b[key];
            var type = (typeof (a) === 'string' ||
                typeof (b) === 'string') ? 'string' : 'number';
            var result;
            if (type === 'string') result = a.localeCompare(b);
            else result = a - b;
            return result;
        }
        return list.sort(compare);
    }

    $scope.SendMail = function () {
        $http({
            method: 'POST',
            url: $scope.mailUrl,
            data: { 'empid': $scope.SalaryFixation.PreRecruitmentEmployeeID, 'plantid': $scope.SalaryFixation.PlantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.CalculateFinal = function () {
        for (var i = 0; i < $scope.salaryHeadList.length; i++) {
            $scope.salaryHeadList[i].PreRecruitmentEmployeeID = $scope.SalaryFixation.PreRecruitmentEmployeeID;
        }
        $http({
            method: 'POST',
            url: $scope.calculateUrl,
            data: {
                'salaryFixationList': $scope.salaryHeadList, 'totalsalary': $scope.SalaryFixation.TotalSalary, 'empid': $scope.SalaryFixation.PreRecruitmentEmployeeID,
                'designationid': $scope.SalaryFixation.GivenDesignationId, 'plantId': $scope.SalaryFixation.PlantId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.salaryHeadList = response.data.SalaryFixation;
                DistributeList($scope.salaryHeadList);
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.SalaryFixation = {};
        $scope.SalaryFixation.salaryHeadList = {};
        $scope.SalaryFixation.dataListEmployee = {};
        $scope.SalaryFixation.SalaryFixationList = {};
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}