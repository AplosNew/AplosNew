'use strict';
OffDayMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','cboService'];
function OffDayMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Weekend";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.OffDayMasterLists = [];
    $scope.OffDayMasterMainLists = [];
    $scope.path = 'Setups/offdaymaster/';
    $scope.saveUrl = $scope.path + 'weekendcreate';
    $scope.updateUrl = $scope.path + 'weekendedit';
    $scope.deleteUrl = $scope.path + 'weekenddelete/';
    $scope.deleteOffdayUrl = $scope.path + 'WeekendHolidayDelete/';
    $scope.getListUrl = $scope.path + 'getweekendlist';
    $scope.OffDayMasterMainListParameters = {
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
        $scope.url = $scope.path + 'getweekendlistformaster?plantId=' + $scope.offDayMaster.PlantId;
        baseService.paginationBase($scope.url, pageno, $scope.OffDayMasterMainListParameters)
            .then(function (result) {
                $scope.OffDayMasterMainLists = result.Rows;
                $scope.OffDayMasterMainListParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        $scope.dayTblShow = false;
        $scope.weekTblShow = false;
        $scope.Action = 'Save';
    };
    function validate(value, name) {
        if (value == null || value == "") {
            throw "Please select " + name + "";
        }
    }
    $scope.getOffdayDataListParameters = {
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
    $scope.getOffdayData = function (pageno) {
        try {
            $scope.getValidateDate();
            validate($scope.offDayMaster.PlantId, "Plant");
            validate($scope.offDayMaster.YearlyCalendarId, "Year");
            validate($scope.offDayMaster.FromDate, "FromDate");
            validate($scope.offDayMaster.ToDate, "ToDate");
            $scope.url = $scope.path + 'getweekendlist?plantId=' + $scope.offDayMaster.PlantId + '&fromDate=' + $scope.offDayMaster.FromDate + '&toDate=' + $scope.offDayMaster.ToDate;
            baseService.paginationBase($scope.url, pageno, $scope.getOffdayDataListParameters)
                .then(function (result) {
                    $scope.OffDayMasterLists = result.Rows;
                    $scope.getOffdayDataListParameters.total_count = result.Total;
                    $scope.dayTblShow = true;
                    console.log($scope.OffDayMasterLists);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
        $scope.weekTblShow = false;
    };
    $scope.searchByOffDayMasterList = [
        {
            name: "Date",
            value: "OffDayDate"
        },
        {
            name: "Day Name",
            value: "DayName"
        },
        {
            name: "OffDayType",
            value: "DayLengthType"
        }
    ]
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
    $scope.offDayMaster = {
        Id: null,
        OffDayId: null,
        PlantId: null,
        YearlyCalendarId: null,
        CldDescription: null,
        FromDate: null,
        ToDate: null,
        TotalDay: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: new Date()
    };
    $('.datepicker').datepicker({
        forceParse: false,
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });
    $scope.getYearFTdateList = [];
    $scope.getYearFTdate = function (yearId) {
        $http({
            method: 'GET',
            url: 'Setups/yearlycalendar/getFromTodate?yearId=' + yearId
        }).then(function successCallback(response) {
            $scope.getYearFTdateList = response.data.Rows[0];
            $scope.getAset();
        });
    }
    $scope.msg = false;
    $scope.getAset = function () {
        var FromDate = $filter('dateFiltering')($scope.getYearFTdateList.FromDate, 'dd-MM-yyyy');
        var ToDate = $filter('dateFiltering')($scope.getYearFTdateList.ToDate, 'dd-MM-yyyy');
        $scope.msg = true;

        ClearFields();
        var yearr = angular.element("#year :selected").text();
        $scope.setMassge = FromDate + " To " + ToDate;
        if (yearr == "") {
            $scope.msg = false;
            $scope.offDayMaster.FromDate = null;
            $scope.offDayMaster.ToDate = null;
        } else {
            $scope.offDayMaster.FromDate = FromDate;
            $scope.offDayMaster.ToDate = ToDate;
            $("#ToDate").datepicker("setDate", new Date($scope.offDayMaster.ToDate));
        }
    }

    $scope.setTodateHilight = function (id, date) {
        $(id).datepicker("setDate", new Date(date));
    }

    $scope.getValidateDate = function () {
        try {
            var FromDate = $filter('dateFiltering')($scope.getYearFTdateList.FromDate, 'dd-MM-yyyy');
            var ToDate = $filter('dateFiltering')($scope.getYearFTdateList.ToDate, 'dd-MM-yyyy');
            if (new Date($scope.offDayMaster.FromDate) < new Date(FromDate)) {
                throw "You can select Date with in date range!";
            }
            if (new Date($scope.offDayMaster.ToDate) > new Date(ToDate)) {
                throw "You can select Date with in date range!";
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.setDate = function () {
        if ($scope.offDayMaster.FromDate != null)
            $("#FromDate").datepicker("setDate", new Date($scope.offDayMaster.FromDate));
    }
    // #region OffDayMasterDetailLists
    $scope.getOffDayMasterDetailListsData = function () {
        $scope.OffDayMasterDetailLists = [
            {
                DayName: 'Saturday',
                DayLengthType: null,
                Selected: false
            },
            {
                DayName: 'Sunday',
                DayLengthType: null,
                Selected: false
            }
            ,
            {
                DayName: 'Monday',
                DayLengthType: null,
                Selected: false
            }
            ,
            {
                DayName: 'Tuesday',
                DayLengthType: null,
                Selected: false
            }
            ,
            {
                DayName: 'Wednesday',
                DayLengthType: null,
                Selected: false
            },
            {
                DayName: 'Thursday',
                DayLengthType: null,
                Selected: false
            },
            {
                DayName: 'Friday',
                DayLengthType: null,
                Selected: false
            }

        ];
    }
    $scope.getOffDayMasterDetailListsData();
    // #endregion
    $scope.weekTblShow = false;
    $scope.addNew = function () {
        try {
            $scope.getValidateDate();
            validate($scope.offDayMaster.PlantId, "Plant");
            validate($scope.offDayMaster.YearlyCalendarId, "Year");
            validate($scope.offDayMaster.FromDate, "FromDate");
            validate($scope.offDayMaster.ToDate, "ToDate");
            $scope.weekTblShow = true;
            $scope.dayTblShow = false;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.vewEdit = function () {
    }
    $scope.plantList = [];
    //$http({
    //    method: 'GET',
    //    url: 'Organizations/Plant/GetCbo'
    //}).then(function successCallback(response) {
    //    $scope.plantList = response.data;
    //});

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.offDayMaster.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };



    $scope.yearList = [];
    $scope.getPlantList = function (plantId) {
        $http({
            method: 'GET',
            url: 'Setups/yearlycalendar/getcbo?plantId=' + plantId
        }).then(function successCallback(response) {
            $scope.yearList = response.data;
        });
        $scope.msg = false;
        $scope.offDayMaster.FromDate = null;
        $scope.offDayMaster.ToDate = null;
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
    function isAvailable(dayName, List) {
        for (var i = 0; i < List.length; i++) {
            if (List[i].DayName == dayName) {
                return true;
            }
        }
        return false;
    }
    $scope.OffDayMasterDetailSelectedLists = [];
    $scope.getOffDaySelectedList = function (event, list, index) {
        var setIndex = index;
        try {
            if (list.Selected) {
                if (list.DayLengthType == null) {
                    throw "Select Day Length Type";
                }
                $scope.OffDayMasterDetailSelectedLists.push(list);
            }
            else if ($scope.OffDayMasterDetailSelectedLists.length > 0) {
                for (var i = 0; i < $scope.OffDayMasterDetailSelectedLists.length; i++) {
                    if ($scope.OffDayMasterDetailSelectedLists[i].DayName == list.DayName) {
                        $scope.OffDayMasterDetailSelectedLists.splice(i, 1);
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

        //if ($scope.OffDayMasterDetailSelectedLists.length > 2) {
        //          event.currentTarget.checked = false;
        //          throw "You can select only 2 days!";
        //}
        //var has = false;
        //for (var i = 0; i < $scope.OffDayMasterDetailLists.length; i++) {
        //    if ($scope.OffDayMasterDetailLists[i].Selected) {
        //        if ($scope.OffDayMasterDetailLists[i].DayLengthType == null) {
        //            event.currentTarget.checked = false;
        //            ShowResult("Select Day Length Type !!!", 'failure');
        //        }
        //        if ($scope.OffDayMasterDetailSelectedLists.length < 2) {
        //            if ($scope.OffDayMasterDetailLists[i].DayLengthType != null)
        //                if ($scope.OffDayMasterDetailLists[i].Selected) {
        //                    if (isAvailable($scope.OffDayMasterDetailLists[i].DayName, $scope.OffDayMasterDetailSelectedLists)==false)
        //                    $scope.OffDayMasterDetailSelectedLists.push($scope.OffDayMasterDetailLists[i]);
        //                }
        //        } else {
        //            event.currentTarget.checked = false;
        //            ShowResult("You can select only 2 days!", 'failure');

        //        }

        //    }
        //}
    }
    $scope.ValidateDate = function () {
        try {
            var months;
            var d1 = new Date($scope.offDayMaster.ToDate);
            var d2 = new Date($scope.offDayMaster.FromDate);
            var year = angular.element("#year :selected").text();
            if ($scope.offDayMaster.ToDate == null || $scope.offDayMaster.FromDate == null) {
                throw "Input From day and To day !!!";
            }
            if (new Date(d2) > new Date(d1)) {
                throw "From <b>(" + d2 + ")</b> date must be smaller than to <b>(" + d1 + ")</b> date !!!";
            }
            if (d1.getFullYear() > year || d1.getFullYear() < year || (d2.getFullYear() > year || d2.getFullYear() < year)) {
                throw "From <b>(" + d2 + ")</b> date year must be between <b>(" + year + ")</b> !!!";
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.Get = function (id, index) {
        $scope.OffDayMasterDetailSelectedLists = [];
        $scope.index = index;
        $scope.offDayMaster = $scope.OffDayMasterLists[$scope.index];
        $scope.offDayMaster.FromDate = $filter('dateFiltering')($scope.offDayMaster.FromDate);
        $scope.offDayMaster.ToDate = $filter('dateFiltering')($scope.offDayMaster.ToDate);
        $scope.offDayMaster.AddedDate = $filter('dateFilter')($scope.offDayMaster.AddedDate);
        $scope.offDayMaster.UpdatedDate = $filter('dateFilter')($scope.offDayMaster.UpdatedDate);
        $scope.getOffDayMasterDetailListsData();

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.GetMaster = function (id, index) {
        $scope.getAset();
        $scope.OffDayMasterDetailSelectedLists = [];
        $scope.index = index;
        $scope.tempOffDayList = angular.copy($scope.OffDayMasterMainLists[$scope.index]);
        $scope.offDayMaster = $scope.tempOffDayList;
        $scope.offDayMaster.FromDate = $filter('dateFiltering')($scope.offDayMaster.FromDate);
        $scope.offDayMaster.ToDate = $filter('dateFiltering')($scope.offDayMaster.ToDate);
        $scope.offDayMaster.AddedDate = $filter('dateFilter')($scope.offDayMaster.AddedDate);
        $scope.offDayMaster.UpdatedDate = $filter('dateFilter')($scope.offDayMaster.UpdatedDate);
        $scope.getOffDayMasterDetailListsData();
        $scope.dayTblShow = false;
        $scope.weekTblShow = false;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
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
    function getOffDayMasterDetailSelectedLists() {
        angular.forEach($scope.OffDayMasterDetailLists, function (item) {
            if (item.Selected && item.DayLengthType != null) {
                $scope.OffDayMasterDetailSelectedLists.push(item);
            }
            if (item.Selected && item.DayLengthType == null) {
                throw "Select Day Length Type!";
            }
        })
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.weekendForm.$valid) {
                getOffDayMasterDetailSelectedLists();
                $scope.ValidateDate();
                $scope.getValidateDate();
                if ($scope.OffDayMasterDetailSelectedLists.length < 1) {
                    throw "Select at least one day by clicking on Add new button!";
                }
                if ($scope.OffDayMasterDetailSelectedLists.length > 2) {
                    throw "You can select only two days";
                }
                if ($scope.Action == 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'offDayMaster': $scope.offDayMaster, 'details': $scope.OffDayMasterDetailSelectedLists },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.OffDayMasterDetailSelectedLists = [];
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            $scope.getOffdayData()
                            ClearFields();
                            $scope.dayTblShow = true;
                            baseService.paginationAdd();
                        }
                    }), function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action == 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: { 'offDayMaster': $scope.offDayMaster, 'details': $scope.OffDayMasterDetailSelectedLists },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.OffDayMasterDetailSelectedLists = [];
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.OffDayMasterMainLists[$scope.index] = $scope.offDayMaster;
                                $scope.OffDayMasterDetailSelectedLists = [];
                            }
                            ClearFields();
                            $scope.getOffdayData()
                            $scope.dayTblShow = true;
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                        $scope.OffDayMasterDetailSelectedLists = [];
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.DeleteItem = function () {
        if (!baseService.isUndefinedOrNull($scope.OffDayId)) {
            $http({
                method: 'POST',
                url: $scope.deleteOffdayUrl + $scope.OffDayId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.OffDayMasterLists.splice($scope.index, 1);
                    //$scope.getOffdayData();
                    baseService.paginationRemove();
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.offDayMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.offDayMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.OffDayMasterMainLists.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
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
        $scope.Action = 'Save';
        $scope.dayTblShow = false;
        $scope.weekTblShow = false;
        $scope.offDayMaster = { CompanyId:$scope.offDayMaster.CompanyId,PlantId: $scope.offDayMaster.PlantId, YearlyCalendarId: $scope.offDayMaster.YearlyCalendarId, FromDate: $scope.offDayMaster.FromDate, ToDate: $scope.offDayMaster.ToDate };
        $scope.OffDayMasterDetailSelectedLists = [];
        //==============week day empty============
        $scope.OffDayMasterDetailLists = [];
        $scope.getOffDayMasterDetailListsData();
    }
}