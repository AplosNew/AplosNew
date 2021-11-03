'use strict';
EntityCalendarController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function EntityCalendarController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Weekend";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.EntityCalendarLists = [];
    $scope.EntityCalendarMainLists = [];
    $scope.path = 'Setups/entityCalendar/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.updateChildUrl = $scope.path + 'editchildtable';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteOffdayUrl = $scope.path + 'entityCalendardetaildelete/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.EntityCalendarMainListParameters = {
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
        console.log($scope.entityCalendar.EntityId, $scope.entityCalendar.PlantId);
        $scope.url = $scope.path + 'getentityCalendarlistformaster?plantId=' + $scope.entityCalendar.PlantId + '&entityId=' + $scope.entityCalendar.EntityId;
        baseService.paginationBase($scope.url, pageno, $scope.EntityCalendarMainListParameters)
            .then(function (result) {
                $scope.EntityCalendarMainLists = result.Rows;
                $scope.EntityCalendarMainListParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        $scope.dayTblShow = false;
        $scope.weekTblShow = false;
        $scope.Action = 'Save';
    };

    function validate(value, name) {
        if (value === null || value === "") {
            throw "Please select " + name + "";
        }
    }

    $scope.getOffdayDataListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'OffDayDate',
        searchBy: "DayName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.dayTblShow = false;
    $scope.getOffdayData = function (pageno) {
        try {
            $scope.getValidateDate();
            validate($scope.entityCalendar.EntityId, "Entity");
            validate($scope.entityCalendar.YearlyCalendarId, "Year");
            validate($scope.entityCalendar.FromDate, "FromDate");
            validate($scope.entityCalendar.ToDate, "ToDate");
            $scope.url = $scope.path + 'getentityCalendartlist?plantId=' + $scope.entityCalendar.PlantId + '&fromDate=' + $scope.entityCalendar.FromDate + '&toDate=' + $scope.entityCalendar.ToDate + '&entityId=' + $scope.entityCalendar.EntityId;
            baseService.paginationBase($scope.url, pageno, $scope.getOffdayDataListParameters)
                .then(function (result) {
                    $scope.EntityCalendarLists = result.Rows;
                    $scope.getOffdayDataListParameters.total_count = result.Total;
                    $scope.dayTblShow = true;
                    console.log('list', $scope.EntityCalendarLists);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
        $scope.weekTblShow = false;
    };

    $scope.searchByEntityCalendarList = [
        {
            name: "Date",
            value: "OffDayDate"
        },
        {
            name: "Day Name",
            value: "DayName"
        }
    ];

    $scope.searchByList = [
        {
            name: "FromDate",
            value: "FromDate"
        },
        {
            name: "ToDate",
            value: "ToDate"
        }
    ];
    $scope.entityCalendar = {
        Id: null,
        OffDayId: null,
        PlantId: null,
        EntityId: null,
        YearlyCalendarId: null,
        CldDescription: null,
        FromDate: null,
        ToDate: null,
        TotalDay: null,
        Remarks: null
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
    };

    $scope.msg = false;
    $scope.getAset = function () {
        var FromDate = $filter('dateFiltering')($scope.getYearFTdateList.FromDate, 'dd-MM-yyyy');
        var ToDate = $filter('dateFiltering')($scope.getYearFTdateList.ToDate, 'dd-MM-yyyy');
        $scope.msg = true;

        ClearFields();
        var yearr = angular.element("#year :selected").text();
        $scope.setMassge = FromDate + " To " + ToDate;
        if (yearr === "") {
            $scope.msg = false;
            $scope.entityCalendar.FromDate = null;
            $scope.entityCalendar.ToDate = null;
        } else {
            $scope.entityCalendar.FromDate = FromDate;
            $scope.entityCalendar.ToDate = ToDate;
            $("#ToDate").datepicker("setDate", new Date($scope.entityCalendar.ToDate));
        }
    };

    $scope.setTodateHilight = function (id, date) {
        $(id).datepicker("setDate", new Date(date));
    };

    $scope.getValidateDate = function () {
        try {
            var FromDate = $filter('dateFiltering')($scope.getYearFTdateList.FromDate, 'dd-MM-yyyy');
            var ToDate = $filter('dateFiltering')($scope.getYearFTdateList.ToDate, 'dd-MM-yyyy');
            if (new Date($scope.entityCalendar.FromDate) < new Date(FromDate)) {
                throw "You can select Date with in date range!";
            }
            if (new Date($scope.entityCalendar.ToDate) > new Date(ToDate)) {
                throw "You can select Date with in date range!";
            }
        } catch (e) {
            throw e;
        }
    };
    $scope.setDate = function () {
        if ($scope.entityCalendar.FromDate != null)
            $("#FromDate").datepicker("setDate", new Date($scope.entityCalendar.FromDate));
    };

    // #region EntityCalendarDetailLists
    $scope.getEntityCalendarDetailListsData = function () {
        $scope.EntityCalendarDetailLists = [];
        var weekDays = ['Saturday', 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'];
        for (var i = 0; i < weekDays.length; i++) {
            $scope.EntityCalendarDetailLists.push({
                DayName: weekDays[i],
                WorkingTime: null,
                InNoShift: 1,
                StandardOT: 0,
                ExtraOT: 0,
                Selected: false
            });
        }
    };

    $scope.getEntityCalendarDetailListsData();
    // #endregion
    $scope.weekTblShow = false;
    $scope.addNew = function () {
        try {
            $scope.getValidateDate();
            validate($scope.entityCalendar.PlantId, "Plant");
            validate($scope.entityCalendar.YearlyCalendarId, "Year");
            validate($scope.entityCalendar.FromDate, "FromDate");
            validate($scope.entityCalendar.ToDate, "ToDate");
            $scope.weekTblShow = true;
            $scope.dayTblShow = false;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.vewEdit = function () {
    };

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.entityList = [];
    $scope.getEntity = function (companyId) {
        $http({
            method: 'GET',
            url: 'Organizations/entity/getcbolist?companyId=' + companyId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }

    $scope.uomList = [];
    $http({
        method: 'GET',
        url: 'Setups/unitofmeasurement/getcbo'
    }).then(function successCallback(response) {
        $scope.uomList = response.data;
    });

    // #region entity
    $scope.entities = [];
    $scope.getEntityMapData = function (id) {
        $scope.entities = [];
        $http({
            method: 'GET',
            url: 'Organizations/entity/get?id=' + id
        }).then(function successCallback(response) {
            if (baseService.arrayLength($scope.entities) == 0) {
                var localValue = [];
                localValue.push(response.data);
                baseService.getDDLSearchColumn(localValue, $scope.entities);
                $scope.entityValue = localValue;
                $scope.entityCalendar.PlantId = $scope.entityValue[0].PlantId;
                $scope.getYearList($scope.entityCalendar.PlantId);
                $scope.getData();
            }
        });
    };

    // #endregion
    $scope.plantList = [];
    $http({
        method: 'GET',
        url: 'Organizations/Plant/GetCbo'
    }).then(function successCallback(response) {
        $scope.plantList = response.data;
    });
    $scope.yearList = [];
    $scope.getYearList = function (plantId) {
        $http({
            method: 'GET',
            url: 'Setups/yearlycalendar/getcbo?plantId=' + plantId
        }).then(function successCallback(response) {
            $scope.yearList = response.data;
        });

        $scope.msg = false;
        $scope.entityCalendar.FromDate = null;
        $scope.entityCalendar.ToDate = null;
    }

    $scope.searchByOffDayMasterList = [
        {
            name: "Day Name",
            value: "DayName"
        },
        {
            name: "Standerd Working Time",
            value: "WorkingTime"
        }
    ];

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
    $scope.EntityCalendarDetailSelectedLists = [];
    $scope.getOffDaySelectedList = function (event, list, index) {
        var setIndex = index;
        try {
            if (list.Selected) {
                if (list.WorkingTime == null) {
                    event.target.checked = false;
                    throw "Please give Working Time";
                }
                //if (list.UomId == null) {
                //    event.target.checked = false;
                //    throw "Please select Uom";
                //}
                $scope.EntityCalendarDetailSelectedLists.push(list);
            }
            else if ($scope.EntityCalendarDetailSelectedLists.length > 0) {
                for (var i = 0; i < $scope.EntityCalendarDetailSelectedLists.length; i++) {
                    if ($scope.EntityCalendarDetailSelectedLists[i].DayName === list.DayName) {
                        $scope.EntityCalendarDetailSelectedLists.splice(i, 1);
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.EntityCalendarDetailEditSelectedLists = [];
    $scope.getEntityCalendarEditSelectedList = function (list) {
        try {
            if (list.Selected) {
                if (list.WorkingTime === null || list.WorkingTime == '') {
                    throw "Please give Working Time";
                }
                //if (list.UomId == null || list.WorkingTime == '') {
                //    throw "Please select Uom";
                //}
                $scope.EntityCalendarDetailEditSelectedLists.push(list);
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ValidateDate = function () {
        try {
            var months;
            var d1 = new Date($scope.entityCalendar.ToDate);
            var d2 = new Date($scope.entityCalendar.FromDate);
            var year = angular.element("#year :selected").text();
            if ($scope.entityCalendar.ToDate === null || $scope.entityCalendar.FromDate == null) {
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
    };

    $scope.Get = function (id, index) {
        $scope.EntityCalendarDetailSelectedLists = [];
        $scope.index = index;
        $scope.entityCalendar = $scope.EntityCalendarLists[$scope.index];
        $scope.entityCalendar.FromDate = $filter('dateFiltering')($scope.entityCalendar.FromDate);
        $scope.entityCalendar.ToDate = $filter('dateFiltering')($scope.entityCalendar.ToDate);
        $scope.entityCalendar.AddedDate = $filter('dateFilter')($scope.entityCalendar.AddedDate);
        $scope.entityCalendar.UpdatedDate = $filter('dateFilter')($scope.entityCalendar.UpdatedDate);
        $scope.getEntityCalendarDetailListsData();

        $scope.Action = 'Update';
    };
    $scope.getMaster = function (index, data) {
        //$scope.getAset();
        $scope.EntityCalendarDetailSelectedLists = [];
        $scope.index = index;
        $scope.EntityCalendarDetailEditLists = [];
        $scope.EntityCalendarDetailEditLists.push({
            Id: data.CalendarDetailId,
            CompanyGroupId: data.CompanyGroupId,
            DayName: data.DayName,
            EntityId: data.EntityId,
            ExtraOT: data.ExtraOT,
            EntityCalendarId: data.Id,
            InNoShift: data.InNoShift,
            PlantId: data.PlantId,
            OffDayDate: $filter('dateFilter')(data.OffDayDate),
            AddedDate: $filter('dateFilter')(data.AddedDate),
            UpdatedDate: $filter('dateFilter')(data.UpdatedDate),
            StandardOT: data.StandardOT,
            WorkingTime: data.WorkingTime,
            Selected: true
        });
        $scope.ChildAction = 'Update';
        angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
    };
    //Deleting Rows from CompanyDepartmentList
    $scope.valuePassInDelModal = function (index, Id) {
        $scope.index = index;
        $scope.OffDayId = Id;
        if (baseService.isUndefinedOrNull($scope.OffDayId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.OffDayId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.entityCalendarForm.$valid) {
                $scope.ValidateDate();
                $scope.getValidateDate();
                if ($scope.EntityCalendarDetailSelectedLists.length < 1) {
                    throw "Select at least one day by clicking on Add new button!";
                }
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'entityCalendar': $scope.entityCalendar, 'details': $scope.EntityCalendarDetailSelectedLists },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.EntityCalendarDetailSelectedLists = [];
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            $scope.getOffdayData();
                            ClearFields();
                            $scope.dayTblShow = true;
                            baseService.paginationAdd();
                        }
                    }), function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: { 'entityCalendar': $scope.entityCalendar, 'details': $scope.EntityCalendarDetailSelectedLists },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.EntityCalendarDetailSelectedLists = [];
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.EntityCalendarMainLists[$scope.index] = $scope.entityCalendar;
                                $scope.EntityCalendarDetailSelectedLists = [];
                            }
                            ClearFields();
                            $scope.getOffdayData();
                            $scope.dayTblShow = true;
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                        $scope.EntityCalendarDetailSelectedLists = [];
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.EditChildTable = function () {
        try {
            $scope.getEntityCalendarEditSelectedList($scope.EntityCalendarDetailEditLists[0]);
            if ($scope.EntityCalendarDetailEditSelectedLists.length < 1) {
                throw "Select at least one day by clicking on Add new button!";
            }

            if ($scope.ChildAction === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateChildUrl,
                    data: { 'details': $scope.EntityCalendarDetailEditSelectedLists[0] },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                        $scope.EntityCalendarDetailEditSelectedLists = [];
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.EntityCalendarDetailEditSelectedLists[$scope.index] = $scope.entityCalendar;
                            $scope.EntityCalendarDetailEditSelectedLists = [];
                            angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                        }
                        ClearFields();
                        $scope.getOffdayData();
                        $scope.dayTblShow = true;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.EntityCalendarDetailEditSelectedLists = [];
                });
            }
        } catch (e) {
            ShowResult(e, 'Error', 'masteraddeditpopup');
        }
    }
    $scope.DeleteItem = function () {
        if (!baseService.isUndefinedOrNull($scope.OffDayId)) {
            $http({
                method: 'POST',
                url: $scope.deleteOffdayUrl + $scope.OffDayId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.EntityCalendarLists.splice($scope.index, 1);
                    //$scope.getOffdayData();
                    baseService.paginationRemove();
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.entityCalendar.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.entityCalendar.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.EntityCalendarMainLists.splice($scope.index, 1);
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
        //$scope.entities = [];
        //$scope.entityValue = [];
        $scope.entityCalendar = { CompanyId: $scope.entityCalendar.CompanyId, EntityId: $scope.entityCalendar.EntityId, PlantId: $scope.entityCalendar.PlantId, YearlyCalendarId: $scope.entityCalendar.YearlyCalendarId, FromDate: $scope.entityCalendar.FromDate, ToDate: $scope.entityCalendar.ToDate };
        $scope.EntityCalendarDetailSelectedLists = [];
        //==============week day empty============
        $scope.EntityCalendarDetailLists = [];
        $scope.getEntityCalendarDetailListsData();
    }
}