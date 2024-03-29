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

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };

    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };
    // #endregion TAB CHANGE
    $scope.ModelList = [];
    $scope.path = 'accounts/BudgetMaster/';
    $scope.saveUrl = $scope.path + 'CreateBudgetControl';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';

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
        BudgetedDays: 0,
        BudgetType: null,
        BudgetCategory: null,
        Remarks: null,
        ApproveBy: null,
        ApproveById: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'RefNo', name: "RefNo" }, { value: 'Remarks', name: "Remarks" }];

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
        $scope.ModelNew.FromDate = $scope._firstDay;
        $scope.ModelNew.ToDate = $scope._lastDay;
        $('.datepicker').datepicker({
            startDate: $scope._firstDay,
            endDate: $scope._lastDay,
            datesDisabled: $scope.DisabledDates,
            format: 'dd-M-yyyy',
            todayHighlight: true,
            autoclose: true,
            inline: true,
            changeMonth: false
        });
        $scope.countDate();
    };

    $scope.countDate = function () {

        var st = new Date($scope.ModelNew.FromDate);
        var ed = new Date($scope.ModelNew.ToDate);

        let Difference_In_Time = ed.getTime() - st.getTime();

        let Difference_In_Days = Math.round(Difference_In_Time / (1000 * 3600 * 24)) + 1;

        $scope.ModelNew.WorkingDays = Difference_In_Days;


    };
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
            url: $scope.path + "GetBudgetControlList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields();
        });
    }
    $scope.getData();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.FromDate = $filter('dateFiltering')(new Date($scope.ModelNew.FromDate), 'dd-MM-yyyy');
        $scope.ModelNew.ToDate = $filter('dateFiltering')(new Date($scope.ModelNew.ToDate), 'dd-MM-yyyy');
        $scope.countDate();
        $scope.GetBudgetControlChildList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            $scope._FromDate = (new Date($scope.ModelNew.FromDate).getMonth() + 1).toString();
            $scope._ToDate = (new Date($scope.ModelNew.ToDate).getMonth() + 1).toString();
            if ($scope.ModelNew.MonthNo != $scope._FromDate) {
                throw "Select From Date by selected month.";
            }
            if ($scope.ModelNew.MonthNo != $scope._ToDate) {
                throw "Select From Date by selected month.";
            }
            if ($scope.ModelNew.BudgetedDays > $scope.ModelNew.WorkingDays) {
                throw "Budgeted Days cann't greater than Working Days";
            }

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
                        $scope.getData();
                        $scope.BudgetControlChildList = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        } catch (e) {
            ShowResult(e, 'failure');
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
        $scope.BudgetControlChildList = [];
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.BudgetControlChildList = [];
    }

    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href =  'accounts/BudgetMaster/GetSampleFile?reportFormat=' + ReportFormat;
    };

    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.ShowSaveBtn = false;
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };
    $scope.BudgetControlChildList = [];
    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: 'accounts/BudgetMaster/ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'file': $scope.picdata

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.BudgetControlChildList = [];
                        $scope.BudgetControlChildList = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.SaveUploadedData = function () {
        try {
            $http({
                method: "POST",
                url: 'accounts/BudgetMaster/CreateBudgetControlChild',
                data: {
                    'data': $scope.BudgetControlChildList, 'headerId': $scope.ModelNew.Id
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.BudgetControlChildList = [];
                    $("#uploadImage").val(null);
                    $scope.ShowSaveBtn = false;
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.BudgetControlChildList = [];
    $scope.GetBudgetControlChildList = function () {
       
        $http({
            method: 'GET',
            url: 'accounts/BudgetMaster/GetBudgetControlChildList?headerId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.BudgetControlChildList = response.data;
        });
    };

}