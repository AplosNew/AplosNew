'use strict';
MaternityLeaveTransactionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function MaternityLeaveTransactionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Maternity Leave Transaction';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'humanresource/MaternityLeaveTransaction/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.maternityLeaveTransaction = {
        Id: null,
        EmpSystemID: null,
        MaternityLeavePolicyId: null,
        FromDate: null,
        ToDate: null,
        ExpectedDelivaryDate: null,
        IsApproved: false,
        EffectiveDate: null,
        MaternityLeaveEndDay: null,
        MaternityLeaveStartDay: null,
        MaternityStartDay: null,
        MaternityEndDay: null,
        LeaveDays: 0,
        CanAvailAfterDOJ: 0
    };
  
    $scope.maternityLeaveTransactionNew = Object.assign({}, $scope.maternityLeaveTransaction);

    $scope.dataList = [];
    $scope.GetFemaleEmployee = function () {
        $scope.employeeInfo = {};
        $scope.maternityLeaveTransaction = {};
        $scope.maternityLeaveTransactionNew = {};
        $scope.maternityLeaveTransactionNew.ToDate = null;
        $scope.getDuration = null;
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: 'humanresource/maternityleaveTransaction/getfemaleemployee'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }


    $scope.employeeInfo = {};
    $scope.SetData = function (obj) {
        var emp = obj.data;
        $scope.employeeInfo.EmpSystemID = emp.SystemID;
        if (baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.EmpSystemID)) {
            $scope.maternityLeaveTransactionNew.EmpSystemID = $scope.employeeInfo.EmpSystemID;
        }
        $scope.employeeInfo.EmpPic = virtualPath.EmployeePic + emp.EmpPicPath;
        $scope.employeeInfo.EmployeeCode = emp.EmployeeCode;
        $scope.employeeInfo.EmployeeName = emp.EmployeeName;
        $scope.employeeInfo.DOJ = emp.DOJ;
        $scope.employeeInfo.EmailId = emp.EmailId;
        $scope.employeeInfo.Code = emp.Code;
        $scope.employeeInfo.Department = emp.Department;
        $scope.employeeInfo.LegalDesignation = emp.LegalDesignation;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.GetPreData = function (empId) {
        $http.get('humanresource/maternityleavetransaction/getleavebyempid?empId=' + empId)
            .then(function (response) {
                $scope.maternityLeaveTransactions = response.data;
            });
    };

    $scope.recorddoubleclick = function () {

        var gridObj = $("#Grid").data("ejGrid");
        var v = gridObj.getSelectedRecords()[0];
        $scope.maternityLeaveTransaction = v;
        $scope.maternityLeaveTransaction.MaternityStartDay = $filter('dateFiltering')($scope.maternityLeaveTransaction.MaternityStartDay, 'dd-MM-yyyy');
        $scope.maternityLeaveTransaction.MaternityEndDay = $filter('dateFiltering')($scope.maternityLeaveTransaction.MaternityEndDay, 'dd-MM-yyyy');
        $scope.maternityLeaveTransaction.ExpectedDelivaryDate = $filter('dateFiltering')($scope.maternityLeaveTransaction.ExpectedDelivaryDate, 'dd-MM-yyyy');
        $scope.maternityLeaveTransaction.EffectiveDate = $filter('dateFiltering')($scope.maternityLeaveTransaction.EffectiveDate, 'dd-MM-yyyy');
        $scope.maternityLeaveTransaction.MaternityLeaveStartDay = $filter('dateFiltering')($scope.maternityLeaveTransaction.FromDate, 'dd-MM-yyyy');
        $scope.maternityLeaveTransaction.MaternityLeaveEndDay = $filter('dateFiltering')($scope.maternityLeaveTransaction.ToDate, 'dd-MM-yyyy');
        $scope.maternityLeaveTransactionNew = Object.assign({}, $scope.maternityLeaveTransaction);
        $scope.Action = 'Update';
        $scope.GetEffectiveDateBabyPolicy();
        $scope.maternityLeaveTransactionNew.MaternityLeavePolicyId = $scope.maternityLeaveTransaction.MaternityLeavePolicyId;

    };

    $scope.cboBabyList = [];

    $scope.GetEffectiveDateBabyPolicy = function () {
        if (!baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.ExpectedDelivaryDate)) {
            $http({
                method: 'GET',
                url: 'humanresource/maternityleavetransaction/GetPolicyData?EffectiveDate=' + $filter('dateFiltering')($scope.maternityLeaveTransactionNew.ExpectedDelivaryDate, 'dd-MM-yyyy')
            }).then(function successCallback(response) {

                if (baseService.arrayLength(response.data) > 0) {

                    $scope.cboBabyList = [];
                    for (var i = 0; i < response.data.length; i++) {
                        var ob = {
                            Value: response.data[i].Id,
                            Text: response.data[i].ChildNo
                        };
                        $scope.cboBabyList.push(ob);
                    }

                }

            });
        }
        $scope.GetTakeBabyPolicy();
    }

    $scope.GetEffectiveDatePolicy = function () {
        if (!baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.ExpectedDelivaryDate)) {
            $http({
                method: 'GET',
                url: 'humanresource/maternityleavetransaction/GetPolicyData?EffectiveDate=' + $filter('dateFiltering')($scope.maternityLeaveTransactionNew.ExpectedDelivaryDate, 'dd-MM-yyyy')
            }).then(function successCallback(response) {

                if (baseService.arrayLength(response.data) > 0) {
                   
                    $scope.maternityLeaveTransactionNew.EffectiveDate = $filter('dateFiltering')(response.data[0].EffectiveDate, 'dd-MM-yyyy');
                    $scope.cboBabyList = [];
                    for (var i = 0; i < response.data.length; i++) {
                        var ob = {
                            Value: response.data[i].Id,
                            Text: response.data[i].ChildNo
                        };
                        $scope.cboBabyList.push(ob);
                    }

                }

            });
        }
        $scope.TakeBabyPolicy();
    }

    $scope.GetTakeBabyPolicy = function () {

        try {
            if (!baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.MaternityLeavePolicyId)) {
                $http.get('humanresource/maternityleavetransaction/getChildNo?Id=' + $scope.maternityLeaveTransactionNew.MaternityLeavePolicyId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.MaternityStartDay = response.data[0].MaternityStartDay;
                            $scope.MaternityEndDay = response.data[0].MaternityEndDay;
                            $scope.CanAvailAfterDOJ = response.data[0].CanAvailAfterDOJ;

                        }
                        $scope.TakeEndDate();
                        //$scope.GetEffectiveDatePolicy();
                    });
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.TakeBabyPolicy = function () {

        try {
            if (!baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.MaternityLeavePolicyId)) {
                $http.get('humanresource/maternityleavetransaction/getChildNo?Id=' + $scope.maternityLeaveTransactionNew.MaternityLeavePolicyId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.MaternityStartDay = response.data[0].MaternityStartDay;
                            $scope.MaternityEndDay = response.data[0].MaternityEndDay;
                            $scope.CanAvailAfterDOJ = response.data[0].CanAvailAfterDOJ;
                            
                            var mlsddate = new Date($scope.maternityLeaveTransactionNew.ExpectedDelivaryDate);
                            var mleddate = new Date($scope.maternityLeaveTransactionNew.ExpectedDelivaryDate);

                            var mlsd = mlsddate.setDate(mlsddate.getDate() - response.data[0].MaternityLeaveStartDay);
                            $scope.maternityLeaveTransactionNew.MaternityLeaveStartDay = $filter('dateFiltering')(new Date(mlsd), 'dd-MM-yyyy');

                            var mled = mleddate.setDate(mleddate.getDate() + response.data[0].MaternityLeaveEndDay) - 1;
                            $scope.maternityLeaveTransactionNew.MaternityLeaveEndDay = $filter('dateFiltering')(new Date(mled), 'dd-MM-yyyy');

                            var date1 = new Date($scope.maternityLeaveTransactionNew.MaternityLeaveStartDay);
                            var date2 = new Date($scope.maternityLeaveTransactionNew.MaternityLeaveEndDay);

                            $scope.maternityLeaveTransactionNew.LeaveDays = (parseInt((date2 - date1) / (1000 * 60 * 60 * 24), 10) + 1);
                        }
                        $scope.TakeEndDate();
                    });
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.TakeEndDate = function () {

        if (!baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.MaternityLeaveStartDay)) {

            var startdate = new Date($scope.maternityLeaveTransactionNew.MaternityLeaveStartDay);
            var numberOfDaysToAdd = $scope.maternityLeaveTransactionNew.LeaveDays - 1;
            var ver = startdate.setDate(startdate.getDate($scope.maternityLeaveTransactionNew.MaternityLeaveStartDay) + numberOfDaysToAdd);
            $scope.maternityLeaveTransactionNew.MaternityLeaveEndDay = $filter('dateFiltering')(new Date(ver), 'dd-MM-yyyy');

            var msddate = new Date($scope.maternityLeaveTransactionNew.MaternityLeaveStartDay);
            var msd = msddate.setDate(msddate.getDate() - $scope.MaternityStartDay);
            $scope.maternityLeaveTransactionNew.MaternityStartDay = $filter('dateFiltering')(new Date(msd), 'dd-MM-yyyy');

            var meddate = new Date($scope.maternityLeaveTransactionNew.MaternityLeaveEndDay);
            var med = meddate.setDate(meddate.getDate() + $scope.MaternityEndDay);
            $scope.maternityLeaveTransactionNew.MaternityEndDay = $filter('dateFiltering')(new Date(med), 'dd-MM-yyyy');
        }
    }

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.EmpSystemID)) {
                $scope.maternityLeaveTransactionNew.EmpSystemID = $scope.employeeInfo.EmpSystemID;
            }

            if (baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.MaternityLeavePolicyId)) {
                throw 'Please Select Baby No';
            }
            var fosd = $filter('dateFiltering')($scope.maternityLeaveTransactionNew.MaternityStartDay, 'dd-MM-yyyy'); // FollowUp StartDate
            var foed = $filter('dateFiltering')($scope.maternityLeaveTransactionNew.MaternityEndDay, 'dd-MM-yyyy');  // FolloUp EndDate
            var expd = $filter('dateFiltering')($scope.maternityLeaveTransactionNew.ExpectedDelivaryDate, 'dd-MM-yyyy');  //ExpectedDelivaryDate
            var mlsd = $filter('dateFiltering')($scope.maternityLeaveTransactionNew.MaternityLeaveStartDay, 'dd-MM-yyyy'); //  Leave Start Date
            var mled = $filter('dateFiltering')($scope.maternityLeaveTransactionNew.MaternityLeaveEndDay, 'dd-MM-yyyy');  //Leave End Date

            //if (new Date(mlsd) <= new Date(fosd)) {
            //    throw 'Leave Start Date cann\'t be Smaller than FollowUp Start Date.';
            //}
            if (new Date(mlsd) >= new Date(expd)) {
                throw 'Leave Start Date cann\'t be Greater than Expected Delevary Date.';
            }

            $scope.maternityLeaveTransactionNew.FromDate = $scope.maternityLeaveTransactionNew.MaternityLeaveStartDay;

            $scope.maternityLeaveTransactionNew.ToDate = $scope.maternityLeaveTransactionNew.MaternityLeaveEndDay;

            var EmpDOJ = new Date($scope.employeeInfo.DOJ);

            var med = EmpDOJ.setDate(EmpDOJ.getDate() + $scope.CanAvailAfterDOJ);
            $scope.maternityLeaveTransactionNew.CheckEmpDoJ = $filter('dateFiltering')(new Date(med), 'dd-MM-yyyy');
            if (new Date($scope.maternityLeaveTransactionNew.ExpectedDelivaryDate) < new Date($scope.maternityLeaveTransactionNew.CheckEmpDoJ)) {
                throw 'Can apply after "' + $scope.CanAvailAfterDOJ + '" days (' + $scope.maternityLeaveTransactionNew.CheckEmpDoJ + ') from DOJ: (' + $scope.employeeInfo.DOJ + ')';
            }

            angular.copy($scope.maternityLeaveTransactionNew, $scope.maternityLeaveTransaction);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.maternityLeaveTransactionNewForm.$valid) {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.maternityLeaveTransaction,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.GetPreData(response.data.MaternityLeaveTransaction.EmpSystemID);
                            //ClearFields();
                            ClearExpectedFields();
                            $scope.maternityLeaveTransactionNew.LeaveDays = 0;
                            
                        }

                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }

            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.maternityLeaveTransactionNew.SystemID)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.maternityLeaveTransactionNew.SystemID,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.maternityLeaveTransactions.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearExpectedFields();
                    //$scope.DurationType = null;
                    $scope.DurationValue = null;
                    
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        ClearExpectedFields();
        return true;

    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.maternityLeaveTransaction = {};
        $scope.maternityLeaveTransactionNew = {};
        $scope.employeeInfo = [];
        $scope.maternityLeaveTransactions = [];
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    }
    function ClearExpectedFields() {
        $scope.Action = 'Save';
        $scope.maternityLeaveTransaction = {};
        $scope.maternityLeaveTransactionNew = {};
        //$scope.employeeInfo = [];
        $scope.maternityLeaveTransactions = [];
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    }

}