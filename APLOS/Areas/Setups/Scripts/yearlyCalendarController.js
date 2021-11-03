'use strict';
YearlyCalendarController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function YearlyCalendarController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Yearly Calendar";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.yearlyCalendars = [];
    $scope.path = 'Setups/yearlycalendar/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.yearlyCalendar = {
        Id: null,
        CompanyGroupId: null,
        PlantId: null,
        YearNo: null,
        FromDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),
        ToDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: new Date()
    };
    $('.datepicker').datepicker({
        forceParse: false,
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });
    $scope.plantList = [];

    $http({
        method: 'GET',
        url: 'Organizations/Plant/GetCbo'
    }).then(function successCallback(response) {
        $scope.plantList = response.data;
    });

    $rootScope.searchByList = [
        {
            'name': 'YearNo',
            'value': 'YearNo'
        }
    ];
    $scope.yearlyCalenderParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'YearNo',
        searchBy: "YearNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getYearData = function () {
        $scope.getData = function (pageno) {
            $scope.yearlyCalenderParameters.plantId = $scope.yearlyCalendar.PlantId;
            baseService.paginationBase($scope.getListUrl, pageno, $scope.yearlyCalenderParameters)
                .then(function (result) {
                    $scope.yearlyCalendars = result.Rows;
                    $scope.yearlyCalenderParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        //baseService.init($scope.getListUrl, null, null, null, 'Id', 'YearNo');
        //$scope.getData = function (pageno) {
        //    $rootScope.parameters.plantId = $scope.yearlyCalendar.PlantId;
        //    baseService.pagination(pageno)
        //        .then(function (result) {
        //            $scope.yearlyCalendars = result.Rows;
        //            console.log($scope.yearlyCalendars);
        //        }, function () {
        //            ShowResult(commonMessage.NetworkError, 'failure');
        //        }).finally(function () {
        //        });
        //};
    }
    $scope.getYearData();
    $scope.checkYear = function () {
        try {
            if ($scope.yearlyCalendarForm.YearNo.$error.number) {
                throw "Pleas input a valid year !!!";
            }
        } catch (e) {
            throw e;
        }
    };

    $scope.ValidateDate = function () {
        try {
            var months;
            var d1 = new Date($scope.yearlyCalendar.ToDate);
            var d2 = new Date($scope.yearlyCalendar.FromDate);
            var year = $scope.yearlyCalendar.YearNo;
            if (new Date(d2) > new Date(d1)) {
                throw "From date must be smaller then to date !!!";
            }
            //if ((d1.getFullYear() > year || d1.getFullYear() < year) || (d2.getFullYear() > year || d2.getFullYear() < year)) {
            //    throw "From date must be between Year !!!";
            //}
        } catch (e) {
            throw e;
        }
    };
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.copyYearlyCalendar = angular.copy($scope.yearlyCalendars[$scope.index]);
        $scope.yearlyCalendar = $scope.copyYearlyCalendar;
        $scope.yearlyCalendar.FromDate = $filter('dateFiltering')($scope.yearlyCalendar.FromDate);
        $scope.yearlyCalendar.ToDate = $filter('dateFiltering')($scope.yearlyCalendar.ToDate);
        $scope.yearlyCalendar.AddedDate = $filter('dateFilter')($scope.yearlyCalendar.AddedDate);
        $scope.yearlyCalendar.UpdatedDate = $filter('dateFilter')($scope.yearlyCalendar.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            $scope.checkYear();
            $scope.ValidateDate();
            if ($scope.yearlyCalendarForm.$valid) {
                if ($scope.Action == 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.yearlyCalendar,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            baseService.paginationAdd();
                            ClearFields();
                        }
                    }), function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action == 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.yearlyCalendar,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.yearlyCalendars[$scope.index] = $scope.yearlyCalendar;
                            }
                            ClearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }//Update
            }//$valid
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.yearlyCalendar.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.yearlyCalendar.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.yearlyCalendars.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields($scope.yearlyCalendar.PlantId);
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        $('.datepicker').datepicker({
            forceParse: false,
            format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
        });

        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.yearlyCalendar = { PlantId: $scope.yearlyCalendar.PlantId, FromDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'), ToDate: $filter("date")(Date.now(), 'dd-MMM-yyyy') };
        $('.datepicker').datepicker({
            forceParse: false,
            format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
        });
    }
}