UserAccessController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService'];
function UserAccessController($scope, $http, $location, $rootScope, $window, $compile, baseService) {
    $scope.title = 'User Access';
    $scope.employees = [];
    $scope.companyGroups = [];
    $http({
        method: 'GET',
        url: 'aplosempfieldtag/getcompanygroupcbo'
    }).then(function successCallback(response) {
        $scope.companyGroups = response.data;
    });

    $scope.userAccess = {
        CompanyGroupId: null,
        Id: null,
        Name: null,
        InitialPIN: null,
        Col1: null,
        Col2: null,
        Col3: null,
        Col4: null,
        Col5: null,
        Col6: null,
        Col7: null,
        Col8: null,
        Col9: null,
        Col10: null,
        Col11: null,
        Col12: null,
        Col13: null,
        Col14: null,
        Col15: null,
        Col16: null,
        Col17: null,
        Col18: null,
        Col19: null,
        Col20: null,
        AccessUser: false
    };
    $scope.userAccessNew = Object.assign({}, $scope.userAccess);

    // #region Employee

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
            if (baseService.isUndefinedOrNull($scope.userAccessNew.CompanyGroupId)) {
                throw "First Select Company Group";
            }
            baseService.setCurrentPage('employeeData');
            $scope.loadEmployeeData = function (pageno) {
                baseService.paginationBase('employee/getemployeebycompanygroup?companyGroupId=' + $scope.userAccessNew.CompanyGroupId, pageno, $scope.employeeParameters)
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
        $scope.userAccessNew.Id = ob.Id;
        $scope.userAccessNew.Name = ob.Name;
        $scope.userAccessNew.InitialPIN = ob.InitialPIN;
        $scope.userAccessNew.Col1 = ob.Col1;
        $scope.userAccessNew.Col2 = ob.Col2;
        $scope.userAccessNew.Col3 = ob.Col3;
        $scope.userAccessNew.Col4 = ob.Col4;
        $scope.userAccessNew.Col5 = ob.Col5;
        $scope.userAccessNew.Col6 = ob.Col6;
        $scope.userAccessNew.Col7 = ob.Col7;
        $scope.userAccessNew.Col8 = ob.Col8;
        $scope.userAccessNew.Col9 = ob.Col9;
        $scope.userAccessNew.Col10 = ob.Col10;
        $scope.userAccessNew.Col11 = ob.Col11;
        $scope.userAccessNew.Col12 = ob.Col12;
        $scope.userAccessNew.Col13 = ob.Col13;
        $scope.userAccessNew.Col14 = ob.Col14;
        $scope.userAccessNew.Col15 = ob.Col15;
        $scope.userAccessNew.Col16 = ob.Col16;
        $scope.userAccessNew.Col17 = ob.Col17;
        $scope.userAccessNew.Col18 = ob.Col18;
        $scope.userAccessNew.Col19 = ob.Col19;
        $scope.userAccessNew.Col20 = ob.Col20;
        $scope.userAccessNew.AccessUser = ob.AccessUser;
        angular.element(document.querySelector('#employeemodal')).modal('hide');
        $scope.LoadData();
    };

    $scope.LoadData = function () {
        $http.get('employee/getdynamicdata?employeeId=' + $scope.userAccessNew.Id)
            .then(function (response) {
                $scope.employees = response.data;
                var obj = $scope.employees;
                $scope.LoadDynamicData(obj);
            })
    };

    $scope.LoadDynamicData = function (obj) {
        $scope.left = '';
        $scope.right = '';
        if (obj !== null) {
            angular.forEach(obj, function (obj, i) {
                var colData = null;
                var dynamicHtml = '';
                dynamicHtml = '<input type="text" ng-model="userAccessNew.' + obj.ColumnName + '"  class="form-control" disabled>';
                if (i % 2 == 0) {
                    $scope.left += '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">' + obj.AplosColumnName + '</label>' +
                        '<div class="col-sm-8">' + dynamicHtml + '</div></div>';
                }
                else {
                    $scope.right += '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">' + obj.AplosColumnName + '</label>' +
                        '<div class="col-sm-8">' + dynamicHtml + '</div></div>';
                }
            });
        }
    };

    $scope.Save = function () {
        try {
            $http({
                method: "post",
                url: 'employee/updateuseraccess',
                data: $scope.userAccessNew,
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.clearuserAccess();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.clearuserAccess = function () {
        $scope.userAccessNew.Id = null;
        $scope.userAccessNew.Name = null;
        $scope.userAccessNew.InitialPIN = null;
        $scope.userAccessNew.Col1 = null;
        $scope.userAccessNew.Col2 = null;
        $scope.userAccessNew.Col3 = null;
        $scope.userAccessNew.Col4 = null;
        $scope.userAccessNew.Col5 = null;
        $scope.userAccessNew.Col6 = null;
        $scope.userAccessNew.Col7 = null;
        $scope.userAccessNew.Col8 = null;
        $scope.userAccessNew.Col9 = null;
        $scope.userAccessNew.Col10 = null;
        $scope.userAccessNew.Col11 = null;
        $scope.userAccessNew.Col12 = null;
        $scope.userAccessNew.Col13 = null;
        $scope.userAccessNew.Col14 = null;
        $scope.userAccessNew.Col15 = null;
        $scope.userAccessNew.Col16 = null;
        $scope.userAccessNew.Col17 = null;
        $scope.userAccessNew.Col18 = null;
        $scope.userAccessNew.Col19 = null;
        $scope.userAccessNew.Col20 = null;
        $scope.userAccessNew.AccessUser = null;
    };
    // #endregion

    $scope.LogOff = function () {
        location.href = 'CPanel';
    }
};