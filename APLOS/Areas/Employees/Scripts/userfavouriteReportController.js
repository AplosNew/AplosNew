'use strict';
userfavouriteReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function userfavouriteReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.Action = 'Save';

    $scope.ModelTemp = {
        Id: null,
        FavouriteMasterId: null,
        EmployeeId: null,
        UserId: null,
        UserName: null,
        StandardName: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

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

    //#region  ***********************************User ********************************************************//
    $rootScope.searchByUserList = [
        {
            'name': 'Username',
            'value': 'UserId'
        },
        {
            'name': 'User Type',
            'value': 'UserType'
        },
        {
            'name': 'Employee Id',
            'value': 'EmployeeId'
        },
        {
            'name': 'Full Name',
            'value': 'FullName'
        },
        {
            'name': 'AuthToken',
            'value': 'AuthToken'
        }
    ];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserId',
        searchBy: "UserId",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpDataList = [];
        $scope.popUpUrl = 'Securities/user/getlist';
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.selectDoubleClick = function (data) {
        if (data.SysAdmin)
            return ShowResult("User [" + data.UserId + "] is [" + data.UserType + "], so role is not required.", 'failure', 'popUpId')
        $scope.ModelNew.UserId = data.Id;
        $scope.ModelNew.User = data.UserId;
        $scope.ModelNew.FullName = data.FullName;
        $scope.ModelNew.EmployeeId = data.EmployeeId;
        $scope.closePopUp();
    };
    $scope.selectSingleClick = function (data) {
        $scope.rowSelected = data.UserId;
        $scope.valueData = data;
    };
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData)
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    //#endregion***********************************User ********************************************************//




    $scope.HrefList = [];
    $scope.UINameList = [];

    $scope.getMenuMasterCbo = function () {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInFoReport/getMenuMasterCbo'
        }).then(function successCallback(response) {
            $scope.HrefList = response.data;
            $scope.UINameList = response.data;
        });
    }
    $scope.getMenuMasterCbo();

    $scope.GetHref = function () {
        for (var i = 0; i < $scope.UINameList.length; i++) {
            if ($scope.ModelNew.UIName == $scope.UINameList[i].Description) {
                $scope.ModelNew.Href = $scope.UINameList[i].Href;
                break;
            }
        }
    }

    function containsSpecialChars(str) {
        const specialChars = /[`!@#$%^&*()_+\=\[\]{};':"\\|,.<>\/?~ ]/;
        return specialChars.test(str);
    }

    $scope.CheckSpecialCharecter = function () {
        try {

            if (containsSpecialChars($scope.ModelNew.UserName)) {

                $scope.ModelNew.UserName =
                    $scope.ModelNew.UserName.replace(/\s/g, '')
                        .replace(/[`!@#$%^&*()_+\=\[\]{};':"\\|,.<>\/?~]/g, '');

                throw "No spaces or special characters allowed for User Name.";
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveFavouriteFilter = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.filterNewForm.$valid) {

                $http({
                    method: 'POST',
                    url: "Employees/EmployeeInFoReport/SaveFavouriteFilter",
                    data: { 'data': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        $scope.ModelTemp = {
            Id: null,
            ShiftId: null,
            PositionCategory: null,
            EntityId: null,
            SectionId: null,
            UserId: null,
            EmployeeId: null,
            UIName: null,
            VisibleToAll: false,
            VisibleAtBudgetCode: false,
            VisibleToAllPositionCode: false,
            IsGlobalEmpApplicable: false,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null

        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.SaveFavourite = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.filterNewForm.$valid) {

                $http({
                    method: 'POST',
                    url: "Employees/EmployeeInFoReport/SaveFavouriteFilter",
                    data: { 'data': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.modelList = [];
    $scope.GetData = function () {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInFoReport/GetFavouriteMaster'
        }).then(function successCallback(response) {
            $scope.modelList = response.data;
        });
    }
    $scope.GetData();

    $scope.modelChildList = [];

    $scope.GetChildData = function () {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInFoReport/GetFavouriteMasterChild?masterId=' + $scope.ModelNew.FavouriteMasterId
        }).then(function successCallback(response) {
            $scope.modelChildList = response.data;
        });
    }

    $scope.Get = function (index) {
        $scope.index = index.data;
        angular.copy(index.data, $scope.ModelNew);
        $scope.GetChildData();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.ModelCTemp = {
        Id: -1,
        ColumnName: null,
        FilterApply: null,
        MandatoryDisplay: null

    };
    $scope.ModelCNew = Object.assign({}, $scope.ModelCTemp);

    // #region checkbox all

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.columnList.length; i++) {
                $scope.columnList[i].Flag = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridC").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.SaveChild = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelChildForm.$valid) {

                $http({
                    method: 'POST',
                    url: "Employees/EmployeeInFoReport/SaveFavouriteChild",
                    data: { 'data': $scope.ModelCNew, 'masterId': $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetChildData();
                        $scope.ClearChild();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ClearChild = function () {
        $scope.ModelCTemp = {
            Id: -1,
            ColumnName: null,
            FilterApply: null,
            MandatoryDisplay: null

        };
        $scope.ModelCNew = Object.assign({}, $scope.ModelCTemp);
    }




}