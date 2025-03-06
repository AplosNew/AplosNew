'use strict';
HolidayCalendarController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function HolidayCalendarController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Holiday Calendar";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.holidayCalendarLists = [];
    $scope.path = 'Setups/offdaymaster/';
    $scope.saveUrl = $scope.path + 'holidaycreate';
    $scope.updateUrl = $scope.path + 'holidayedit';
    $scope.deleteUrl = $scope.path + 'holidaydelete/';
    $scope.deleteOffdayUrl = $scope.path + 'WeekendHolidayDelete/';
    $scope.getListUrl = $scope.path + 'getholidaylist';
    $scope.holidayMainListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'FromDate',
        searchBy: "FromDate",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getData = function (pageno) {
        $scope.url = $scope.path + 'getholidaylist?plantId=' + $scope.holidayCalendar.PlantId + '&yearlyCalendarId=' + $scope.holidayCalendar.YearlyCalendarId;
        baseService.paginationBase($scope.url, pageno, $scope.holidayMainListParameters)
            .then(function (result) {
                $scope.holidayCalendarLists = result.Rows;
                $scope.holidayMainListParameters.total_count = result.Total;
                getTotalDays($scope.holidayCalendarLists);
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    function getDays(first, second) {
        var from = new Date(first);
        var toDate = new Date(second);
        var d = Math.floor((Date.parse(toDate) - Date.parse(from)) / 86400000);
        var days = parseInt(d);
        return days + 1;
    }
    $scope.TotalHolidayDay = 0;
    function getTotalDays(list) {
        $scope.TotalHolidayDay = 0;
        angular.forEach(list, function (item) {
            $scope.TotalHolidayDay += item.TotalDay;
        });
    }
    $scope.holidayCategoryLists = [];
    cboService.getHolidayCategoryCbo(null, function (result) {
        $scope.holidayCategoryLists = result;
    });
    //************OffDay Detail*************//
    function validate(value, name) {
        if (value === null || value === "") {
            throw "Please select " + name + "";
        }
    }
    $scope.getHolidayOffdayDataListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'OffDayDate',
        searchBy: "OffDayDate",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.dayTblShow = false;
    $scope.getHolidayOffdayData = function (pageno) {
        try {
            $scope.getValidateDate();
            validate($scope.holidayCalendar.FromDate, "FromDate");
            validate($scope.holidayCalendar.ToDate, "ToDate");
            $scope.url = $scope.path + 'getholidaylistfordetail?plantId=' + $scope.holidayCalendar.PlantId + '&fromDate=' + $scope.holidayCalendar.FromDate + '&toDate=' + $scope.holidayCalendar.ToDate;
            baseService.paginationBase($scope.url, pageno, $scope.getHolidayOffdayDataListParameters)
                .then(function (result) {
                    $scope.OffDayMasterLists = result.Rows;
                    $scope.getHolidayOffdayDataListParameters.total_count = result.Total;
                    $scope.dayTblShow = true;
                    console.log($scope.holidayCalendar);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
        $scope.weekTblShow = false;
    };
    $scope.searchHolidayByOffDayMasterList = [
        {
            name: "Date",
            value: "OffDayDate"
        },
        {
            name: "Day Name",
            value: "DayName"
        }
    ]
    $('.datepicker').datepicker({
        forceParse: false,
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });
    //**********************
    $scope.getYearFTdateList = [];
    $scope.getYearFTdate = function (yearId) {
        $http({
            method: 'GET',
            url: 'Setups/yearlycalendar/getFromTodate?yearId=' + yearId
        }).then(function successCallback(response) {
            $scope.getYearFTdateList = response.data.Rows[0];
            //$scope.getAset();
        });
    };
    $scope.setMsDate = function (yearId) {
        $scope.msg = true;
        $http({
            method: 'GET',
            url: 'Setups/yearlycalendar/getFromTodate?yearId=' + yearId
        }).then(function successCallback(response) {
            $scope.getYearFTdateList = response.data.Rows[0];
            //var FromDate = $filter('dateFiltering')($scope.getYearFTdateList.FromDate, 'dd-MM-yyyy');
            //var ToDate = $filter('dateFiltering')($scope.getYearFTdateList.ToDate, 'dd-MM-yyyy');
            //$scope.setMassge = FromDate + " To " + ToDate;
        });
    };
    $scope.getAset = function () {
        var FromDate = $filter('dateFiltering')($scope.getYearFTdateList.FromDate, 'dd-MM-yyyy');
        var ToDate = $filter('dateFiltering')($scope.getYearFTdateList.ToDate, 'dd-MM-yyyy');
        $scope.msg = true;
        ClearFieldsForAction();
        var yearr = angular.element("#year :selected").text();
        $scope.setMassge = FromDate + " To " + ToDate;
        if (yearr === "") {
            $scope.msg = false;
            $scope.holidayCalendar.FromDate = null;
            $scope.holidayCalendar.ToDate = null;
        } else {
            $scope.holidayCalendar.FromDate = FromDate;
            $scope.holidayCalendar.ToDate = ToDate;
            $scope.setTodateHilight("#ToDate", $scope.holidayCalendar.ToDate);
        }
    }
    $scope.holidayCalendar = {
        Id: null,
        PlantId: null,
        CompanyId:null,
        YearlyCalendarId: null,
        HolidayCategoryId: null,
        HolidayName: null,
        CldDescription: null,
        FromDate: null,
        ToDate: null,
        TotalDay: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: new Date(),
        IsMandatory: false


    };

    $scope.searchByList = [
        {
            name: "FromDate",
            value: "FromDate"
        },
        {
            name: "ToDate",
            value: "ToDate"
        }
    ]


    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.plantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.holidayCalendar.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };


    //$scope.plantList = [];
    //$http({
    //    method: 'GET',
    //    url: 'Organizations/Plant/GetCbo'
    //}).then(function successCallback(response) {
    //    $scope.plantList = response.data;
    //});
    $scope.yearList = [];
    $scope.getPlantList = function (plantId) {
        $http({
            method: 'GET',
            url: 'Setups/yearlycalendar/getcbo?plantId=' + plantId
        }).then(function successCallback(response) {
            $scope.yearList = response.data;
        });
    }
    $rootScope.searchByList = [
        {
            'name': 'TotalDay',
            'value': 'TotalDay'
        },
        {
            'name': 'OffDayType',
            'value': 'OffDayType'
        }
    ];
    $scope.Get = function (id, index) {
        $scope.CompanyId= $scope.holidayCalendar.CompanyId;
        $scope.index = index;
        $scope.holidayCalendar = $scope.OffDayMasterLists[$scope.index];
        $scope.holidayCalendar.FromDate = $filter('dateFiltering')($scope.holidayCalendar.FromDate);
        $scope.holidayCalendar.ToDate = $filter('dateFiltering')($scope.holidayCalendar.ToDate);
        $scope.holidayCalendar.AddedDate = $filter('dateFilter')($scope.holidayCalendar.AddedDate);
        $scope.holidayCalendar.UpdatedDate = $filter('dateFilter')($scope.holidayCalendar.UpdatedDate);
        $scope.holidayCalendar.CompanyId = $scope.CompanyId;
        $scope.getHolidayOffdayData();

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.GetMaster = function (id, index) {
        $scope.CompanyId = $scope.holidayCalendar.CompanyId;
        $scope.index = index;
        $scope.holidayTempCalendar = angular.copy($scope.holidayCalendarLists[$scope.index]);
        $scope.holidayCalendar = $scope.holidayTempCalendar;
        $scope.setMsDate($scope.holidayCalendar.YearlyCalendarId);
        
        $scope.holidayCalendar.FromDate = $filter('dateFiltering')($scope.holidayCalendar.FromDate);
        $scope.holidayCalendar.ToDate = $filter('dateFiltering')($scope.holidayCalendar.ToDate);
        $scope.holidayCalendar.AddedDate = $filter('dateFilter')($scope.holidayCalendar.AddedDate);
        $scope.holidayCalendar.UpdatedDate = $filter('dateFilter')($scope.holidayCalendar.UpdatedDate);
        $scope.holidayCalendar.CompanyId = $scope.CompanyId;
        $scope.dayTblShow = false;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.getValidateDate = function () {
        try {
            var FromDate = $filter('dateFiltering')($scope.getYearFTdateList.FromDate, 'dd-MM-yyyy');
            var ToDate = $filter('dateFiltering')($scope.getYearFTdateList.ToDate, 'dd-MM-yyyy');
            if (new Date($scope.holidayCalendar.FromDate) < new Date(FromDate)) {
                throw "You can select Date with in date range!";
            }
            if (new Date($scope.holidayCalendar.ToDate) > new Date(ToDate)) {
                throw "You can select Date with in date range!";
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.setTodateHilight = function (id, date) {
        $(id).datepicker("setDate", new Date(date));
    };
    //Deleting Rows from CompanyDepartmentList
    $scope.valuePassInDelModal = function (index, OffDayId) {
        $scope.index = index;
        $scope.OffDayId = OffDayId;
        if (baseService.isUndefinedOrNull($scope.OffDayId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + OffDayId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.ValidateDate = function () {
        try {
            var months;
            var d1 = new Date($scope.holidayCalendar.ToDate);
            var d2 = new Date($scope.holidayCalendar.FromDate);
            var year = angular.element("#year :selected").text();
            if (new Date(d2) > new Date(d1)) {
                throw "From date must be smaller then to date !!!";
            }
            if (d1.getFullYear() > year || d1.getFullYear() < year || (d2.getFullYear() > year || d2.getFullYear() < year)) {
                throw "From date must be between Year !!!";
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            $scope.ValidateDate();
            $scope.getValidateDate();
            if ($scope.holidayForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'holidayCaleder': $scope.holidayCalendar },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true)
                            ShowResult(response.data.Message, 'failure');
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            baseService.paginationAdd();
                            ClearFieldsForAction();
                            $scope.getHolidayOffdayData();
                            $scope.dayTblShow = true;
                        }
                    }), function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: { 'holidayCaleder': $scope.holidayCalendar },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            //if ($scope.index > -1) {
                            //    $scope.holidayCalendarLists[$scope.index] = response.data.OffDayMaster;
                            //}
                            ClearFieldsForAction();
                            $scope.getHolidayOffdayData();
                            $scope.dayTblShow = true;
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.holidayCalendar.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.holidayCalendar.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.holidayCalendarLists.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.DeleteItem = function () {
        if (!baseService.isUndefinedOrNull($scope.holidayCalendar.OffDayId)) {
            $http({
                method: 'POST',
                url: $scope.deleteOffdayUrl + $scope.holidayCalendar.OffDayId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.OffDayMasterLists.splice($scope.index, 1);
                    baseService.paginationRemove();
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.dayTblShow = false;
        $scope.Action = 'Save';
        $scope.holidayCalendar = { CompanyId:$scope.holidayCalendar.CompanyId, PlantId: $scope.holidayCalendar.PlantId, YearlyCalendarId: $scope.holidayCalendar.YearlyCalendarId };
    }

    function ClearFieldsForAction() {
        $scope.dayTblShow = false;
        $scope.OffDayMasterLists = [];
        $scope.Action = 'Save';
        $scope.holidayCalendar = { CompanyId: $scope.holidayCalendar.CompanyId,PlantId: $scope.holidayCalendar.PlantId, YearlyCalendarId: $scope.holidayCalendar.YearlyCalendarId, FromDate: $scope.holidayCalendar.FromDate, ToDate: $scope.holidayCalendar.ToDate };
    }
}