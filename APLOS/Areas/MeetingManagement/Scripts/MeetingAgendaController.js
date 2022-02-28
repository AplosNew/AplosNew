'use strict';
MeetingAgendaController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MeetingAgendaController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Meeting Agenda';
    $scope.Action = 'Save'; 
    $scope.ModelList = [];
    $scope.path = 'MeetingManagement/MeetingAgenda/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/'; 
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $scope.year = new Date().getFullYear().toString();

    $scope.ModelAgenda = {
        Id: null,
        MeetingTypeId: null,
        MeetingOrganizedById: null,
        MeetingOrganizedByCode: null,
        MeetingOrganizedBy: null,
        ChairedById: null,
        ChairedByCode: null,
        ChairedBy: null,
        Date: null,
        Location: null,
        MeetingName: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelAgenda);

    $scope.ModelMeetItem = {
        Id: null,
        MeetingAgendaId: null,
        MeetingItemHeaderId: null,
    };
    $scope.ModelMeetingItem = Object.assign({}, $scope.ModelMeetItem);
    

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
           
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

   
    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            var tempItem = [];

            for (var i = 0; i < $scope.MeetingList.length; i++) {
                if ($scope.MeetingList[i].Active) {
                    tempItem.push($scope.MeetingList[i]);
                }
            }
            angular.copy($scope.ModelNew, $scope.ModelAgenda);
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.ModelNewForm.$valid) {
                
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'data': $scope.ModelNew, 'MeetingData': tempItem },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.ModelNew.Id = response.data.Id;
                            $scope.getData();
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                   
                }
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
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
                    $scope.getData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.ModelNew = {
            Id: null,
            MeetingOrganizedById: null,
            MeetingOrganizedByCode: null,
            MeetingOrganizedBy: null,
            ChairedById: null,
            ChairedByCode: null,
            ChairedBy: null,
            Date: null,
            Location: null,
            MeetingName: null
        };
        $scope.Action = 'Save';
    };
    

    //$scope.meetingTypeList = [];
    //cboService.getCbomeetingType(function (result) {
    //    $scope.meetingTypeList = result;
    //});

    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    
    $scope.Name = null;
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;
        
        if ($scope.Name == 'Main') {
            $scope.ModelNew.MeetingOrganizedById = data.SystemId;
            $scope.ModelNew.MeetingOrganizedBy = data.EmployeeName;
            $scope.ModelNew.MeetingOrganizedByCode = data.EmployeeCode;
        }
        else {
            $scope.ModelNew.ChairedById = data.SystemId;
            $scope.ModelNew.ChairedBy = data.EmployeeName;
            $scope.ModelNew.ChairedByCode = data.EmployeeCode;
        }
        
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };


    //The Filters 
    $scope.filters = [];
    $scope.MeetingAgendaloadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'MeetingType', width: 20, headerText: "Meeting Type", type: "string" },
                { field: 'IssueStatus', width: 20, headerText: "Issue Status", type: "string" },
                { field: 'IssueCritically', width: 20, headerText: "Criticality", type: "string" },
                { field: 'Department', width: 20, headerText: "Department", type: "string" },
                { field: 'Attendee', width: 20, headerText: "Attendee", type: "string" },

            ];
            $("#filters").ejGrid({
                dataSource: $scope.filters,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: columnList
            });

            var gridObj = $("#filters").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#filters").children('.e-pager.e-js.e-pager').hide();
            $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#filters").children('.e-gridcontent').hide();
        });
    }
    $scope.MeetingAgendaloadfilters();

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "MeetingTypeId", "Value": getString(fl, "MeetingTypeId") });
        parameters.push({ "Key": "IssueStatus", "Value": getString(fl, "IssueStatus") });
        parameters.push({ "Key": "IssueCritically", "Value": getString(fl, "IssueCritically") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
        parameters.push({ "Key": "AttendeeId", "Value": getString(fl, "AttendeeId") });
      
        $scope.parameters = parameters;

    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.clearFilters = function () {

        var gridObj = $("#filters").data("ejGrid");
        gridObj.clearFiltering();
    }

    
    $scope.GetDateGenerate = function () {

        try {
            
            $http({
                method: 'GET',
                url: 'MeetingManagement/MeetingAgenda/GetDateInformation',
            }).then(function successCallback(response) {
                $scope.ToDate = response.data[0].ToDate;
                $scope.FromDate = response.data[0].FromDate;
            });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetDateGenerate();

    $scope.MeetingList = [];
    $scope.GetMeetingGenerate = function () {

        try {
            $scope.MeetingList = [];
            $scope.filterComplete();

            
            $http({
                method: 'POST',
                url: 'MeetingManagement/MeetingAgenda/GetMeetingInformation',
                data: { 'parameters': $scope.parameters, 'toDate': $scope.ToDate, 'fromDate': $scope.FromDate},
            }).then(function successCallback(response) {
                $scope.MeetingList = response.data;
            });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.MeetingListNew = [];

    //$scope.ok = function () {

    //    try {
    //        for (var i = 0; i < $scope.ModelList.length; i++) {
    //            if ($scope.ModelList[i].Active == true) {
    //                if (checkDoubleMeeting($scope.MeetingListNew, $scope.ModelList[i].SystemId) === false) {
    //                    $scope.MeetingListNew.push($scope.ModelList[i]);
    //                }
    //            }
    //        }
    //        var eDialog = $("#MeetingInfoGrid").data("ejDialog");
    //        eDialog.close();

    //        //if ($rootScope.isCollapsed) {
    //        //    $rootScope.toggle();
    //        //}

    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }

    //};

    function checkDoubleMeeting(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemId === Id) {
                return true;
            }
        }
        return false;
    }


    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllMeetingWise });
    };

    function CheckBoxSelectAllMeetingWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#meetingInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MeetingList.length; i++) {
                $scope.MeetingList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#meetingInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    function checkChangeMeeting(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.EmployeeBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }

    function headCheckChangeMeeting(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Gridmeeting").data("ejGrid");
            var filtered = $("#Gridmeeting").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.MeetingList.length; i++) {

                    $scope.MeetingList[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.MeetingList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.MeetingList[i].Id == filtered[j].Id)
                            // $scope.ModelList[i].isSelect = true;
                            $scope.MeetingList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#Gridmeeting .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "change": checkChangeMeeting });
            }
        }
        else {
            var filtered = $("#Gridmeeting").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.MeetingList.length; i++) {
                    $scope.MeetingList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.MeetingList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.MeetingList[i].Id == filtered[j].Id)
                            $scope.MeetingList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#Gridmeeting.rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "change": checkChangeMeeting });
            }
        }
        //header level check
    }

    $scope.dataBoundemployee = function (args) {
        $("#Gridmeeting .rowCheckbox").ejCheckBox({ "change": checkChangeMeeting });
        $("#headchk").ejCheckBox({ "change": headCheckChangeMeeting });

    };

    //$scope.MeetingItem = function () {
       
    //    var tempItem = [];
    //    for (var i = 1; i <= $scope.MeetingList.length; i++) {
    //        if ($scope.MeetingList[i].Active) {
    //            tempItem.push($scope.MeetingList[i]);
    //        }
    //    }
    //}

    $scope.ClearItem = function () {
        $scope.ModelNew = {
            Id: null,
            MeetingAgendaId: null,
            MeetingItemHeaderId: null,
        };
        $scope.Action = 'Save';
    };
}