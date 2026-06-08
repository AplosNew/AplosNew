'use strict';
favouriteReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function favouriteReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.Action = 'Save';

    $scope.ModelTemp = {
        Id: null,
        Href: null,
        UIName: null,
        StandardName: null,
        Remarks: null,
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

            if (containsSpecialChars($scope.ModelNew.StandardName)) {

                $scope.ModelNew.StandardName =
                    $scope.ModelNew.StandardName.replace(/\s/g, '')
                        .replace(/[`!@#$%^&*()_+\=\[\]{};':"\\|,.<>\/?~]/g, '');

                throw "No spaces or special characters allowed for Standard Name.";
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

    $scope.columnList = [];
    $scope.getColumnFiltersData = function () {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInFoReport/getColumnFiltersData'
        }).then(function successCallback(response) {
            $scope.columnList = response.data;
        });
    }
    $scope.getColumnFiltersData();

    $scope.GetChildData = function () {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInFoReport/GetFavouriteMasterChild?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.columnList = response.data;
            for (var i = 0; i < $scope.columnList.length; i++) {
                for (var j = 0; j < response.data.length; j++) {
                    if (baseService.isUndefinedOrNull(response.data[j].Id)) {
                        $scope.columnList[i].Flag = true;
                    }
                }
            }
        });
    }

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
                $scope.newcolumnList = [];
                for (var i = 0; i < $scope.columnList.length; i++) {
                    if ($scope.columnList[i].Flag) {
                        $scope.newcolumnList.push($scope.columnList[i]);
                    }
                }

                $http({
                    method: 'POST',
                    url: "Employees/EmployeeInFoReport/SaveFavouriteChild",
                    data: { 'data': $scope.newcolumnList, 'masterId' : $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetChildData();
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