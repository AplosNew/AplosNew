reportController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', '$cookies', '$filter'];
function reportController($scope, $http, $location, $rootScope, $window, $compile, baseService, $cookies, $filter) {
    $scope.title = 'Report';

    $scope.ReportParam = {
        CompanyGroupId: null,
        EmployeeName: null,
        withoutactivity: false,
        notloggedin: false,
        Submitted: false,
        NotSubmitted: false,
        Id: null
    };

    $scope.GetEmployeeInfo = function () {
        $scope.ReportParam.CompanyGroupId = $rootScope.CompanyGroupId;
        $scope.ReportParam.EmployeeName = $rootScope.EmployeeName;
        //console.log('99',$scope.ReportParam);
        location.href = "report/EmployeeInfo?cg=" + $scope.ReportParam.CompanyGroupId + "&un=" + $scope.ReportParam.EmployeeName + "&wa=" + $scope.ReportParam.withoutactivity + "&nl=" + $scope.ReportParam.notloggedin + "&s=" + $scope.ReportParam.Submitted + "&ns=" + $scope.ReportParam.NotSubmitted + "";
        //location.data:{'master': $scope.master};
        // data: { 'master': $scope.master }
    };
    LoadTimeCall();

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue == null || fieldValue == '') {
                throw ('[' + fieldName + '] is required...')
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.SetToDate = function () {
        try {
            if ($scope.FromDate == null || $scope.FromDate == '') {
            }
            else {
                var _fromdate = new Date($scope.FromDate);
                var todate = $filter('dateFiltering')(_fromdate, 'dd-MMM-yyyy');
                $scope.ToDate = todate;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.SetFromDate = function () {
        try {
            if ($scope.ToDate == null || $scope.ToDate == '') {
            }
            else {
                var _todate = new Date($scope.ToDate);
                var _fromdate = new Date();
                if ($scope.FromDate == null || $scope.FromDate == '') {
                }
                else {
                    _fromdate = new Date($scope.FromDate);
                }

                if (_fromdate > _todate) {
                    var todate = $filter('dateFiltering')(_todate, 'dd-MMM-yyyy');
                    $scope.FromDate = todate;
                }
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }

    function LoadTimeCall() {
        var _fromdate = new Date();
        var fromdate = $filter('dateFiltering')(_fromdate, 'dd-MMM-yyyy');
        $scope.FromDate = fromdate;
        $scope.ToDate = fromdate;
    }
    $scope.GetActivityInfo = function () {
        try {
            CheckField($scope.FromDate, "From Date");
            CheckField($scope.ToDate, "To Date");
            //var fromdate = $scope.FromDate;
            var _fromdate = new Date($scope.FromDate);
            var _todate = new Date($scope.ToDate);

            var fromdate = $filter('dateFiltering')(_fromdate, 'dd-MMM-yyyy');
            var todate = $filter('dateFiltering')(_todate, 'dd-MMM-yyyy');

            if (_fromdate > _todate) {
                throw "From Date [" + fromdate + "] can not be greater than To Date [" + todate + "]";
            }
            $scope.ReportParam.CompanyGroupId = $rootScope.CompanyGroupId;
            $scope.ReportParam.EmployeeName = $rootScope.EmployeeName;
            //console.log('99', $scope.ReportParam);
            location.href = "DateWiseActivity?cg=" + $scope.ReportParam.CompanyGroupId + "&un=" + $scope.ReportParam.EmployeeName + "&fd=" + fromdate + "&td=" + todate + "";
            //location.data:{'master': $scope.master};
            // data: { 'master': $scope.master }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.GetExceptionInfo = function () {
        try {
            $scope.ReportParam.CompanyGroupId = $rootScope.CompanyGroupId;
            $scope.ReportParam.EmployeeName = $rootScope.EmployeeName;
            location.href = "Exception?cg=" + $scope.ReportParam.CompanyGroupId + "&un=" + $scope.ReportParam.EmployeeName + "";
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.searchbyEmployeelist = [
        {
            'name': 'Employee Name',
            'value': 'Name'
        },
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Email',
            'value': 'Email'
        },
        {
            'name': 'Mobile',
            'value': 'Mobile'
        }
    ];
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Name',
        searchBy: "Name",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getEmployeeData = function () {
        try {
            baseService.setCurrentPage('employeeData');
            $scope.loadEmployeeData = function (pageno) {
                baseService.paginationBase('getemployeebycompanygroup?companyGroupId=' + $cookies.get('CompanyGroupId'), pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeData = result.Rows;
                        $scope.employeeParameters.total_count = result.total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeemodal')).modal('show');
            $scope.loadEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.getuserAccess = function (ob) {
        $scope.ReportParam.Id = ob.Id;
        $scope.ReportParam.Name = ob.Name;
        angular.element(document.querySelector('#employeemodal')).modal('hide');
    };


    $scope.GetIndividualInfo = function () {
        try {
            $scope.ReportParam.CompanyGroupId = $rootScope.CompanyGroupId;
            // $scope.ReportParam.EmployeeName = $rootScope.EmployeeName;
            location.href = "IndividualStatus?cg=" + $scope.ReportParam.CompanyGroupId + "&un=" + $scope.ReportParam.Name + "&uid=" + $scope.ReportParam.Id + "";
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    // #endregion
};