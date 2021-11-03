'use strict';
employeeLeaveCarryForwardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function employeeLeaveCarryForwardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Leave Balance';
    $scope.popUpList = [];
    $scope.LeaveTypeList = [];
    $scope.LeaveTypeCumulativeList = [];
    $scope.YearNo = [];
    $scope.valueData = '';
    $scope.filedata = '';
    $scope.message = null;
    $scope.imageSrc = null;
    $scope.Action = 'Save';
    $scope.maxDate = new Date().toDateString();

    $scope.EmployeeLeaveBalance = {
        Id: null,
        EmployeeId: null,
        CalanderYearId: null,
        LeaveTypeId: null,
        PreviousYearCarryForwardId:null,
        CarryForward: null,
        PreviousYearCarryForward:null,
        PreviousYearAllocation: null,
        CurrentYearAllocation: null,
        DaysCanBeSanctioned: null,
        AvailedOpeningBalance: null,
        AppliedDays: null,
        AvailedDays: null,

        PlantId: null,
        CompanyGroupId:null,
 
    };

    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });

    $scope.showEntity = function () {
        $http.get('employees/employeeLeaveBalance/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    }


    $scope.roleWiseMessage = function () {
        $http.get('employees/employeeLeaveBalance/getEntity')
            .then(function successCallback(response) {
                if (!baseService.isUndefinedOrNull(response.data.Message)) {
                    $scope.message = response.data.Message;
                }
                else {
                    $scope.message = response.data;
                }
            }
            ), function errorCallBack(response) {
                showResult(response.data.Message, 'failure');
            }
    }
    $scope.roleWiseMessage();


    cboService.getLeaveTypeCbo(null, function (result) {
        $scope.LeaveTypeList = result;
    });

    cboService.getLeaveTypeCumulativeCbo(null, function (result) {
        $scope.LeaveTypeCumulativeList = result;
    });

    cboService.getYearCbo(null, function (result) {
        $scope.YearNo = result;
    });

    cboService.getYearCbo

    $scope.Update = function () {
        try {
            console.log($scope.EmployeeLeaveBalance);
            //Validate();
            $scope.savedisable = true;
            $http({
                method: 'POST',
                url: 'employees/employeeLeaveCarryForward/update',
                data: { 'employeeLeaveBalance': $scope.EmployeeLeaveBalance, 'plantId':  $scope.EmployeeLeaveBalance.PlantId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.savedisable = false;
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
                ShowResult(response.status.Message, 'failure');
            });
            $scope.savedisable = false;
            return true;


        } catch (e) {
            $scope.savedisable = false;

            ShowResult(e, 'failure');
        }
    };

    $scope.loadNewEmployee = function () {
        //$scope.excluedEmpColumn = ['Email', 'Reason', 'position', 'ResignationDate', 'AttachLetter', 'ApprovalStatus', 'EffectiveDate', 'Picture', 'IsPastResignationAllowed', 'PastResignationDaysAllowed', 'EmployeeCategory'];
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeName',
            searchBy: 'EmployeeName',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUpUrl = 'employees/EmployeeLeaveCarryForward/ActiveEmployeeList?plantId=' + $scope.EmployeeLeaveBalance.PlantId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    $
                    if (baseService.arrayLength($scope.popUpList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId1');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId1')).modal('show');
        $scope.getPopUpData();

    };

    $scope.loadPendingEmployee = function () {
        $scope.excluedEmpColumn = ['Email', 'Reason', 'position', 'ResignationDate', 'AttachLetter', 'ApprovalStatus', 'EffectiveDate', 'Picture', 'IsPastResignationAllowed', 'PastResignationDaysAllowed', 'EmployeeCategory'];
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeName',
            searchBy: 'EmployeeName',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUpUrl = 'employees/employeeLeaveBalance/pendingList?plantId=' + $scope.EmployeeLeaveBalance.PlantId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.popUpList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId2');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId2')).modal('show');
        $scope.getPopUpData();

    }
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId1')).modal('hide');
    }
    $scope.closePopUp2 = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId2')).modal('hide');
    }

    function selectNewEmployee(data) {
        //$scope.Clear();
        $scope.EmployeeLeaveBalance.Id = data.LeaveSummaryId;
        $scope.EmployeeLeaveBalance.EmployeeId = data.EmployeeId;
        $scope.EmployeeLeaveBalance.EmployeeName = data.EmployeeName;
        $scope.EmployeeLeaveBalance.EmployeeCode = data.EmployeeCode;
        $scope.EmployeeLeaveBalance.GivenDesignation = data.GivenDesignation;
        $scope.EmployeeLeaveBalance.Designation = data.Designation;
        $scope.EmployeeLeaveBalance.DOJ = data.DOJ;
        $scope.EmployeeLeaveBalance.DOC = data.DOC;
        $scope.EmployeeLeaveBalance.EmployeeCategory = data.EmployeeCategory;
        $scope.EmployeeLeaveBalance.PlantId = data.PlantId;
        $scope.EmployeeLeaveBalance.CompanyGroupId = data.GroupId;
        $scope.EmployeeLeaveBalance.EmployeeCategory = data.EmployeeCategory;
        $scope.EmployeeLeaveBalance.CurrentYearAllocation = data.CurrentYearAllocation;
        $scope.EmployeeLeaveBalance.DaysCanBeSanctioned = data.DaysCanBeSanctioned;
        $scope.EmployeeLeaveBalance.Entity = data.Entity;
        $scope.closePopUp();
        $scope.Action = 'Save';
    };

    
    $scope.loadResignationHistory = function (Id) {
        $http.get('employees/employeeLeaveBalance/getResignationHistoryById?EmployeeId=' + $scope.EmployeeLeaveBalance.EmployeeId)
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#ResignationHistoryPopUp')).modal('show');
    }

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }
    function Validate() {
        try {
            CheckField($scope.EmployeeLeaveBalance.PlantId, 'Plant');
            CheckField($scope.EmployeeLeaveBalance.EmployeeName, 'Employee Name');
            CheckField($scope.EmployeeLeaveBalance.ResignationDate, 'Resignation Submission Date');
            CheckField($scope.EmployeeLeaveBalance.EffectiveDate, 'Applied Effective Date');
            CheckField($scope.EmployeeLeaveBalance.Reason, 'Reason');
            var regDate = new Date($scope.EmployeeLeaveBalance.ResignationDate);
            var effDate = new Date($scope.EmployeeLeaveBalance.EffectiveDate);
            var dojDate = new Date($scope.EmployeeLeaveBalance.DOJ);

            if (dojDate > regDate) {
                throw 'Resignation date must be greater than Date of Join'
            }
            if (regDate > effDate) {
                throw 'Applied Effective date cannot be less than Resignation date'
            }

            var d = new Date();
            var d1 = $filter('date')(d, 'dd-MMM-yy');
            var d3 = $filter('date')(regDate, 'dd-MMM-yy');
            var resignationDate = new Date(d3);
            var today = new Date(d1);
            if (resignationDate > today) {
                throw 'Future Resignation date is not allowed';
            }

            var effDate2 = $filter('date')(effDate, 'dd-MMM-yy')
            var effectiveDate = new Date(effDate2);

            d.setDate(d.getDate() + 90);
            var d1 = $filter('date')(d, 'dd-MMM-yy');
            var d2 = new Date(d1);
            if (effDate > d2) {
                throw 'Applied Effective Date Cannot be Greater then [' + d1 + ']'
            }

            var allowDays = new Date();
            allowDays.setDate(d.getDate() - $scope.Resignation.PastResignationDaysAllowed);
            var d7 = $filter('date')(allowDays, 'dd-MMM-yy');
            var d8 = new Date(d7);
            if ($scope.Resignation.IsPastResignationAllowed === true) {
                if (d8 > regDate) {
                    throw 'Past Resignation date before [' + d7 + '] Days is not allowed';
                }
            }

        } catch (e) {
            throw e;
        }
    };

    $scope.showSearch = function (flag) {
        try {
            $scope.search_flag = flag;
            switch (flag) {
                case 'PendingEMP':
                    CheckField($scope.EmployeeLeaveBalance.PlantId, 'Plant');
                    $scope.loadPendingEmployee();
                    break;
                case 'NewEMP':
                    CheckField($scope.EmployeeLeaveBalance.PlantId, 'Plant');
                    $scope.loadNewEmployee();
                    break;
                default:
                    return ShowResult('Search Flag is not defined!!!', 'failure');
            }
            //angular.element(document.querySelector('#popUpId')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.getSearchObject = function (ob) {
        try {
            switch ($scope.search_flag) {
                case 'PendingEMP':
                    selectPendingEmployee(ob);
                    break;
                case 'NewEMP':
                    selectNewEmployee(ob);
                    break;
                default:
            }
            $scope.search_flag = '';
            //angular.element(document.querySelector('#search_popup')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        ClearOb($scope.EmployeeLeaveBalance);
        $scope.Action = 'Save';
        $scope.filedata = null;
        document.getElementById('abc').value = null;

    };
    function ClearOb(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }
    
}