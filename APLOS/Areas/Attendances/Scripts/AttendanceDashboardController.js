'use strict';
AttendanceDashboardController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function AttendanceDashboardController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'Attendance Dashboard';
    $scope.path = 'Attendances/AttendanceDashboard/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.hrDate = $filter('dateFiltering')(Date.now(), 'dd-MMMM-yyyy');
    var noOfdrilDownClick = 0;
    //Chart Color
    window.chartColors = {
        red: 'rgba(240, 52, 52, .6)',
        orange: 'rgb(255, 159, 64)',
        yellow: 'rgb(255, 205, 86)',
        green: 'rgba(46, 204, 113,.6)',
        blue: 'rgb(54, 162, 235)',
        purple: 'rgb(153, 102, 255)',
        grey: 'rgb(201, 203, 207)'
    };
    $scope.chartAttdnLabel = ['Present', 'Late', 'Absent', 'Leave', 'Others'];//TooltipName
    $scope.date = new Date();
    $scope.ColList = [];
    $scope.dynamicAttendanseList = [];
    $scope.index = -1;
    var ATTNPieChart;
    var row = {
        ColumnName: null,
        OnRoleEmployee: null,
        totalPresentEmployee: null,
        totalLateEmployee: null,
        totalAbsentEmployee: null,
        totalLeaveEmployee: null,
        totalLongAbsentismEmployee: null,
        ShiftNotAssignedEmployee: null,
        totalAttdnNotProcessedToday: null,
        totalWeekoffEmployee: null
    };
    function setDynamicDashboardList(list) {//Dynamic Value set
        var row = {
            ColumnName: null,
            OnRoleEmployee: null,
            totalPresentEmployee: null,
            totalLateEmployee: null,
            totalAbsentEmployee: null,
            totalLeaveEmployee: null,
            totalLongAbsentismEmployee: null,
            ShiftNotAssignedEmployee: null,
            totalAttdnNotProcessedToday: null,
            totalWeekoffEmployee: null
        };        
        $scope.totalWeekoffEmployee = 0;
        $scope.totalOthersShiftNotAssignedEmployee = 0;
        $scope.ShiftNotAssignAsofToday = 0;
        $scope.totalAttdnNotProcessedToday = 0;
        $scope.totalShiftNotAssignAsofToday = 0;
        $scope.OnRoleEmployee = 0;
        $scope.present = 0;
        $scope.late = 0;
        $scope.absent = 0;
        $scope.leave = 0;
        $scope.others = 0;
        $scope.totalEarlyOutEmployee = 0;
        $scope.totalLounchOutEmployee = 0;
        $scope.totalLateInEmployee = 0;
        var columnName = "";
        for (var i = 0; i < list.length; i++) {
            if (baseService.isUndefinedOrNull($scope.hrDrpDownModel.PODirectIndirectStatus) || $scope.hrDrpDownModel.PODirectIndirectStatus == "") {
                $scope.dynamicAttendanseList = list.filter(function (ls) {
                    return ls.IsDirect == "General";
                });
            }
            else if ($scope.hrDrpDownModel.PODirectIndirectStatus === "Indirect") {
                $scope.dynamicAttendanseList = list.filter(function (ls) {
                    return ls.IsDirect == "Indirect";
                });
            }
            else if ($scope.hrDrpDownModel.PODirectIndirectStatus === "Direct") {
                $scope.dynamicAttendanseList = list.filter(function (ls) {
                    return ls.IsDirect == "Direct";
                });
            }
        }
        angular.forEach($scope.dynamicAttendanseList, function (item, i) {
            $scope.present += item.totalPresentEmployee;
            $scope.late += item.totalLateEmployee;
            $scope.absent += item.totalAbsentEmployee;
            $scope.leave += item.totalLeaveEmployee;
            $scope.totalOthersShiftNotAssignedEmployee += item.ShiftNotAssignedEmployee;
            $scope.totalShiftNotAssignAsofToday += item.totalShiftNotAssignAsofToday;
            $scope.totalWeekoffEmployee += item.totalWeekoffEmployee;
            $scope.totalAttdnNotProcessedToday += item.totalAttdnNotProcessedToday;
            $scope.totalEarlyOutEmployee += item.totalEarlyOutEmployee;
            $scope.totalLounchOutEmployee += item.totalLounchOutEmployee;
            $scope.totalLateInEmployee += item.totalLateInEmployee;
            $scope.OnRoleEmployee += item.OnRoleEmployee;
        });
        $scope.othersSummary = $scope.totalOthersShiftNotAssignedEmployee + $scope.totalAttdnNotProcessedToday + $scope.totalWeekoffEmployee;
        $scope.chartAttdnList = [$scope.present, $scope.late, $scope.absent, $scope.leave, $scope.othersSummary];
        createAttnPieChart();
    }
    $scope.hrDrpDownModel = {
        EmplyeeTypeOrCategoryId: null,
        PODirectIndirectStatus: null
    };
    $scope.dFunction = function () {
        $scope.groupWiseAttnList = [];
        $http({
            method: 'GET',
            url: 'Attendances/AttendanceDashboard/DefaultAttnStatus/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setDynamicDashboardList(response.data);
            $scope.dynamicAttendanseList2 = response.data;
            $scope.index = -1;
            $scope.stIndex = $scope.index - 1;
        });
    };
    $scope.GetDrillDownAttnStatus = function (data) {
        var getRow = $filter("filter")($scope.ColList, { "ColumnName": "Company" });
        createColListWithCompany(getRow[0].Id);
    };
    function getDrillDownList(companyId) {

        $http({
            method: 'GET',
            url: 'Attendances/AttendanceDashboard/OrgStructureListColList?CompanyId=' + companyId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                    var row = {
                        Sequence: -2,
                        Id: null,
                        StandardName: null,
                        ColumnName: null,
                        RType: null,
                        Text: null,
                        Name: null,
                        date: ''
                    };
                    row.Sequence = i;
                    row.StandardName = response.data[i].StandardName;
                    row.ColumnName = response.data[i].ColumnName;
                    row.RType = response.data[i].RType;
                    row.Text = response.data[i].UId;
                    row.date = $scope.date;
                    $scope.ColList.push(row);
                }
            }
        });
        $scope.strColList = $scope.ColList;
    }
    $scope.headerNav = function (x) {
        if (x.Sequence !== -2) {
            $scope.setIndexHead(x);
            $scope.GetDrillDownAttnStatus(x.Id);
        }
        else {
            $scope.setIndexHead(x);
            $http({
                method: 'GET',
                url: 'Attendances/AttendanceDashboard/DefaultAttnStatus',
                params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                setDynamicDashboardList(response.data);
                $scope.dynamicAttendanseList2 = response.data;
                $scope.index = -1;
                $scope.stIndex = $scope.index - 1;
            });
            $scope.overAllStatusList = [];
        }
    };
    $scope.GetModalOthersDetailJS = function () {
        var eDialog = $("#OthersSummaryStatus").data("ejDialog");
        eDialog.open();
    };
    $scope.GetModalHROnRoleStatusDetailList = function (pageno, data) {
        $scope.desigSummaryList = [];
        $scope.searchbyonRoleEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of OnRole Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Attendances/AttendanceDashboard/ModalOnRoleEmployeeList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyDetaillist) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyonRoleEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyonRoleEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyonRoleEmpList[i].Value, visible: true, width: "180px" });
                }
                $('#onRoleEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    width: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchonRole,
                    templateRefresh: $scope.actionCompleteSearchonRole,
                    isResponsive: true
                });
                $scope.dataGrid = "#onRoleEmpGrid";
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];
                $("#GridResultDesigGrpOnRole").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];
                $("#GridResultOpActOnRole").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
            }
            var eDialog = $("#empOnRole").data("ejDialog");
            eDialog.open();
        });
    };
   
    $scope.GetGruopWiseAttnStatus = function () {
        $scope.groupWiseAttnList = [];
        $http({
            method: 'GET',
            url: 'Attendances/AttendanceDashboard/DefaultAttnStatus/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setDynamicDashboardList(response.data);
            $scope.dynamicAttendanseList2 = response.data;
            createColList();
        });
    };
    $scope.GetGruopWiseAttnStatus();

    function createColList() {
        noOfdrilDownClick = 0;
        if (baseService.arrayLength($scope.ExpenseList) >= 0) {
            var row = {
                Sequence: null,
                Id: null,
                StandardName: null,
                ColumnName: null,
                RType: null,
                Text: null,
                Name: null,
                date: ''
            };
            row.Sequence = -2;
            row.Id = $scope.dynamicAttendanseList[0].CompanyGroupId;
            row.StandardName = "Group";
            row.ColumnName = "Group";
            row.Text = $scope.dynamicAttendanseList[0].GroupName;
            row.Name = $scope.dynamicAttendanseList[0].GroupName;
            row.date = $scope.date;

            $scope.ColList.push(row);
            var rowc = {
                Sequence: null,
                Id: null,
                StandardName: null,
                ColumnName: null,
                RType: null,
                Text: null,
                Name: null,
                date: ''
            };
            rowc.Sequence = -1;
            rowc.Id = $scope.dynamicAttendanseList[0].CompanyId;
            rowc.StandardName = "Company";
            rowc.ColumnName = "Company";
            rowc.Text = $scope.dynamicAttendanseList[0].UserName;
            rowc.Name = $scope.dynamicAttendanseList[0].UserName;
            rowc.date = $scope.date;
            $scope.ColList.push(rowc);
            getDrillDownList();
        }
    }
    function createColListWithCompany(companyId) {
        noOfdrilDownClick++;
        if (noOfdrilDownClick == 1) {
            $scope.ColList = [];
            if (baseService.arrayLength($scope.ExpenseList) >= 0) {
                var row = {
                    Sequence: null,
                    Id: null,
                    StandardName: null,
                    ColumnName: null,
                    RType: null,
                    Text: null,
                    Name: null,
                    date: ''
                };
                row.Sequence = -2;
                row.Id = $scope.dynamicAttendanseList[0].CompanyGroupId;
                row.StandardName = "Group";
                row.ColumnName = "Group";
                row.Text = $scope.dynamicAttendanseList[0].GroupName;
                row.Name = $scope.dynamicAttendanseList[0].GroupName;
                row.date = $scope.date;

                $scope.ColList.push(row);
                var rowc = {
                    Sequence: null,
                    Id: null,
                    StandardName: null,
                    ColumnName: null,
                    RType: null,
                    Text: null,
                    Name: null,
                    date: ''
                };
                rowc.Sequence = -1;
                rowc.Id = $scope.dynamicAttendanseList[0].CompanyId;
                rowc.StandardName = "Company";
                rowc.ColumnName = "Company";
                rowc.Text = $scope.dynamicAttendanseList[0].UserName;
                rowc.Name = $scope.dynamicAttendanseList[0].UserName;
                rowc.date = $scope.date;
                $scope.ColList.push(rowc);
            }

        }
        getDrillDownListWithCompany(companyId);

    }

    function getDrillDownListWithCompany(companyId) {
        $http({
            method: 'GET',
            url: 'Attendances/AttendanceDashboard/OrgStructureListColList?CompanyId=' + companyId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (noOfdrilDownClick == 1) {
                if (baseService.arrayLength(response.data) > 0) {
                    for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                        var row = {
                            Sequence: -2,
                            Id: null,
                            StandardName: null,
                            ColumnName: null,
                            RType: null,
                            Text: null,
                            Name: null,
                            date: ''
                        };
                        row.Sequence = i;
                        row.StandardName = response.data[i].StandardName;
                        row.ColumnName = response.data[i].ColumnName;
                        row.RType = response.data[i].RType;
                        row.Text = response.data[i].UId;
                        row.date = $scope.date;
                        $scope.ColList.push(row);
                    }
                }
            }
            if ($scope.index + 3 < $scope.ColList.length) {
                $http({
                    method: 'POST',
                    url: 'Attendances/AttendanceDashboard/DrillDownAttnStatus/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'hrDate': $scope.hrDate,
                        'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    setDynamicDashboardList(response.data);
                    $scope.dynamicAttendanseList2 = response.data;
                    $scope.index += 1;
                    $scope.stIndex = $scope.index - 1;
                });
            }

        });


        $scope.strColList = $scope.ColList;

    }
    $scope.setIndex = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UId;
                $scope.ColList[i].Name = x.ColumnName;
            }
        }
    };
    $scope.setIndexHead = function (x) {
        $scope.index = x.Sequence;
    };

    function createAttnPieChart() {
        Chart.defaults.global.legend.display = false;
        var ATTNctx = document.getElementById("attnPieChart").getContext('2d');

        if (ATTNPieChart !== undefined && typeof ATTNPieChart === 'object' && typeof ATTNPieChart.destroy === 'function') ATTNPieChart.destroy();
        ATTNPieChart = new Chart(ATTNctx, {
            type: 'doughnut',
            data: {
                labels: $scope.chartAttdnLabel,
                datasets: [{
                    label: '',
                    data: $scope.chartAttdnList,
                    backgroundColor: [
                        'rgba(46, 204, 113,0.7)',
                        'rgba(241, 196, 15, 0.7)',
                        'rgba(231, 76, 60,0.7)',
                        'rgba(82, 179, 217, 0.7)',
                        'rgba(253, 227, 167, 0.7)'
                    ],
                    borderColor: [
                        'rgba(46, 204, 113,1.0)',
                        'rgba(241, 196, 15, 1.0)',
                        'rgba(231, 76, 60,1.0)',
                        'rgba(82, 179, 217, 1.0)',
                        'rgba(253, 227, 167, 1.0)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                legend: {
                    display: false,
                    position: 'bottom'
                },
                title: {
                    display: true,
                    position: 'bottom'
                },
                hover: { mode: null },
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var dataset = data.datasets[tooltipItem.datasetIndex];
                            var total = dataset.data.reduce(function (previousValue, currentValue, currentIndex, array) {
                                return previousValue + currentValue;
                            });
                            var currentValue = dataset.data[tooltipItem.index];
                            var precentage = ((currentValue / total * 100) + 0.0).toFixed(2);
                            return precentage + "%";
                        },
                        title: function (tooltipItem, data) {
                            return $scope.chartAttdnLabel[tooltipItem[0].index];
                        }
                    }
                }
            }
        });
    }
    $scope.setModal = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UId;
            }
        }
    };
    function CreateSummaryList(list, idColumn, descColumn) {
        var SummaryList = [];

        var sortedData = sort_by_key(list, idColumn);

        var tempid = '';

        var count = 0;
        for (var id = 0; id < sortedData.length; id++) {
            if (sortedData[id][idColumn] != tempid) {
                if (SummaryList.length > 0)
                    SummaryList[SummaryList.length - 1].Total = count;

                SummaryList.push({ idColumn: sortedData[id][idColumn], descColumn: sortedData[id][descColumn], Total: 0 });

                count = 0;
            }
            count++;

            tempid = sortedData[id][idColumn];
        }
        if (SummaryList.length > 0)
            SummaryList[SummaryList.length - 1].Total = count;

        return SummaryList;
    }
    function sort_by_key(array, key) {
        return array.sort(function (a, b) {
            var x = a[key]; var y = b[key];
            return ((x < y) ? -1 : ((x > y) ? 1 : 0));
        });
    }
    function CreateSummaryList(list, idColumn, descColumn) {
        var SummaryList = [];

        var sortedData = sort_by_key(list, idColumn);

        var tempid = '';

        var count = 0;
        for (var id = 0; id < sortedData.length; id++) {
            if (sortedData[id][idColumn] != tempid) {
                if (SummaryList.length > 0)
                    SummaryList[SummaryList.length - 1].Total = count;

                SummaryList.push({ idColumn: sortedData[id][idColumn], descColumn: sortedData[id][descColumn], Total: 0 });

                count = 0;
            }
            count++;

            tempid = sortedData[id][idColumn];
        }
        if (SummaryList.length > 0)
            SummaryList[SummaryList.length - 1].Total = count;

        return SummaryList;
    }
    $scope.actionCompleteSearchonRole = function (args) {
        if (args.requestType == "refresh") {

            try {
                var gridObj = $("#onRoleEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };
    $scope.HROnRoleDetailParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeName',
        searchBy: 'EmployeeCode',
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
        tempData: {}
    };
    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion

    $scope.GetModalHRDailyPresentStatusDetailList = function (pageno, data) {
        $scope.searchbyPresentEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Present Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Attendances/AttendanceDashboard/ModalHRDailyPresentStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyPresentEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyPresentEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyPresentEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyPresentEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#presentEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    width: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchPresent,
                    templateRefresh: $scope.actionCompleteSearchPresent,
                    isResponsive: true
                });
                $scope.dataGrid = "#presentEmpGrid";
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];
                $("#GridResultDesigGrp").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActPresent").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

            }
            var eDialog = $("#empPrsntStatus").data("ejDialog");
            eDialog.open();
        });
    };
    $scope.GetModalHRDailyLateStatusList = function (pageno, data) {
        $scope.searchbyLateEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Late Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Attendances/AttendanceDashboard/ModalHRDailyLateStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyLateEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyLateEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyLateEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyLateEmpList[i].Value, visible: true, width: "180px" });
                }
                $('#lateEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchLate,
                    templateRefresh: $scope.actionCompleteSearchLate,
                    isResponsive: true
                });

                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpLate").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActLate").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                $scope.dataGrid = "#lateEmpGrid";
                var eDialog = $("#empLateStatus").data("ejDialog");
                eDialog.open();
            }
        });
    };
    $scope.AbsentList = [];
    $scope.GetModalHRDailyAbsentStatusDetailList = function (pageno, data) {
        $scope.searchbyAbsentEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Absent Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Attendances/AttendanceDashboard/ModalHRDailyAbsentStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            $scope.AbsentList = response.data;
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyAbsentEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyAbsentEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyAbsentEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyAbsentEmpList[i].Value, visible: true, width: "180px" });
                }
                $('#absentEmpGrid').ejGrid({
                    dataSource: $scope.AbsentList,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchAbsent,
                    templateRefresh: $scope.actionCompleteSearchAbsent,
                    isResponsive: true
                });
                $scope.dataGrid = "#absentEmpGrid";

                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpAbsent").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActAbsent").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationMasterId", "OperationMasterName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

                var eDialog = $("#empAbsentStatus").data("ejDialog");
                eDialog.open();
            }
        });
    };

    $scope.GetModalHRDailyLeaveStatusDetailList = function (pageno, data) {
        $scope.searchbyLeaveEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Leave Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Attendances/AttendanceDashboard/ModalHRDailyLeaveStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyLeaveEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyLeaveEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyLeaveEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyLeaveEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#leaveEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchLeave,
                    templateRefresh: $scope.actionCompleteSearchLeave,
                    isResponsive: true
                });
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpLeave").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActLeave").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

                var ColumnListLeaveCode = [{ field: 'descColumn', width: 150, headerText: "Leave Summary" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultLeaveCode").ejGrid({
                    dataSource: CreateSummaryList(response.data, "LeaveType", "LeaveType"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                $scope.dataGrid = "#leaveEmpGrid";
                var eDialog = $("#empLeaveStatus").data("ejDialog");
                eDialog.open();
            }
        });
    };
    $scope.longAbsentismList = [];

    $scope.GetModalHRLongAbsentismDetailList = function (pageno, data) {
        $scope.searchbyAbsentEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Absent Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Attendances/AttendanceDashboard/ModalLongAbsenteismStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            $scope.longAbsentismList = response.data;
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyAbsentEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyAbsentEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyAbsentEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyAbsentEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#longAbsentismEmpGrid').ejGrid({
                    dataSource: $scope.longAbsentismList,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchLongAbsentism,
                    templateRefresh: $scope.actionCompleteSearchLongAbsentism,
                    isResponsive: true
                });
                $scope.dataGrid = "#longAbsentismEmpGrid";
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];
                $("#GridResultDesigGrpLongAbsent").ejGrid({
                    dataSource: CreateSummaryList($scope.longAbsentismList, "DesignationId", "Designation"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var eDialog = $("#empLongAbsentism").data("ejDialog");
                eDialog.open();
            }
        });
    };
    $scope.GetOtherModalDetailJS = function (data) {
        $scope.setModal(data);
        $http({
            method: 'POST',
            url: 'Attendances/AttendanceDashboard/ModalOthersDetail/',
            data: { 'status': status, 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OthersList = response.data;
            var eDialog = $("#OthersDetailStatus").data("ejDialog");
            eDialog.open();
        });
    };
    $scope.PrintGRDes = function () {
        var gridObj = $($scope.dataGrid).data("ejGrid");
        var data = gridObj.model.dataSource;
        data = ej.DataManager(data).executeLocal(ej.Query().select(["EmployeeName", "EmployeeCode", "Shift", "Designation", "EmpCategory", "DOJ", "OperationActivityName", "OperationMasterName", "OperationCode", "CompanyName", "Plant", "Department", "Line", "CellPhnNo"]));
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };
    $scope.actionCompleteSearchPresent = function (args) {
        if (args.requestType == "refresh") {
            try {
                var gridObj = $("#presentEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };
}