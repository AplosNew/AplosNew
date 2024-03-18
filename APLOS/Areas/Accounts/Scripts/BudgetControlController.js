'use strict';
BudgetControlController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function BudgetControlController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE
    $scope.ModelList = [];
    $scope.path = 'accounts/BudgetMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.ModelTemp = {
        Id: null,
        Code: null,
        RefNo: null,
        StandardName: null,
        UserName: null,
        MonthNo: null,
        FromDate: null,
        ToDate: null,
        WorkingDays: 0,
        BudgetDays: 0,
        BudgetType: null,
        BudgetCategory: null,
        Remaks: null,
        ApproveBy: null,
        ApproveById: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

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
    $scope.year = new Date().getFullYear().toString();
    $scope.ModelNew.MonthNo = (new Date().getMonth() + 1).toString();
   

    $scope.budgetTypeList = [
        {
            Value: "Regular",
            Text: 'Regular'
        },
        {
            Value: "Additional",
            Text: 'Additional'
        }
    ]
    $scope.budgetCategoryList = [
        {
            Value: "Monthly",
            Text: 'Monthly'
        },
        {
            Value: "Quartly",
            Text: 'Quartly'
        },
        {
            Value: "SixMonthly",
            Text: 'Six Monthly'
        },
        {
            Value: "Annually",
            Text: 'Annually'
        }
    ]

    $scope.CalenderFunc = function () {

        $scope._firstDay = $filter('dateFiltering')(new Date($scope.year, $scope.ModelNew.MonthNo - 1, 1), 'dd-MM-yyyy');
        $scope._lastDay = $filter('dateFiltering')(new Date($scope.year, $scope.ModelNew.MonthNo, 0), 'dd-MM-yyyy');

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
    $scope.popUpDataList = [];
    $scope.showApproveByPopUp = function () {
        try {
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

        $scope.ModelNew.ApproveById = data.SystemID;
        $scope.ModelNew.ApproveBy = data.EmployeeName;

        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields();
        });
    }
   // $scope.getData();

  

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                  //  $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
}