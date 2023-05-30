'use strict';
manpowerBudgetDashboardController.$inject = ['cboService', '$scope', '$rootScope', '$routeParams', 'baseService', '$http', '$filter'];
function manpowerBudgetDashboardController(cboService, $scope, $rootScope, $routeParams, baseService, $http, $filter) {
    $scope.chartList = [];
    $scope.list = [];
    $scope.index = -1;
    $scope.chartLabel = [];
    $scope.ColList = [];
    $scope.ModalColList = [];
    $scope.DDList = [];
    $scope.stIndex = -2;
    var ManPowerbarChart;
    var salarybarChart;
    $scope.Date = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy');
    $scope.hrStatus = {
        pstatus: ''
    };
    $scope.mpDrpDownModel = {
        EmplyeeTypeOrCategoryId: null
    };

    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.docEmployeeCategoryList = result;
    });

    $scope.ManPowerBudget = function () {
        $scope.chartList = [];
        $scope.ManPowerList = [];
        $http({
            method: 'POST',
            url: 'ManpowerBudgetDashboard/GetGroupWiseCompanyList/',
            data: {
                'date': $scope.Date,
                'status': $scope.hrStatus.pstatus,
                'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if ($scope.hrStatus.pstatus !== null) {
                setList(response.data);
                $scope.DDList = response.data;
                createChart();
                createColList();
            } else {
                setList(response.data);
                createChart();
            }
        });
    };
    $scope.ManPowerBudget();

    $scope.GetDetailDrillDownTableJS = function (data) {
        //$scope.clickCount++;
        //if ($scope.clickCount == 1) {
        //    createCompanyColList(data.CompanyId)
        //}

        $scope.DDList = [];
        if ($scope.index + 3 < $scope.ColList.length) {
            $http({
                method: 'POST',
                url: 'ManpowerBudgetDashboard/GetDetailDrillDownTable/',
                data: {
                    'ChartColumnList': $scope.ColList,
                    'seq': $scope.index,
                    'date': $scope.Date,
                    'status': $scope.hrStatus.pstatus,
                    'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.DDList = response.data;
                setList(response.data);
                createChart();

                $scope.index += 1;
                $scope.stIndex = $scope.index - 1;
            });
        }
    }

    $scope.isDirectChange = function () {
        //$scope.dynamicAttendanseList = $scope.dynamicAttendanseList2;

        setList($scope.DDList);
        createChart();

    };

    function setList(list) {
        $scope.date = new Date();
        $scope.chartLabel = [];
        $scope.chartLabelSal = [];
        var CurrentTotalEmp = 0;
        var proposedTotalEmp = 0;
        var Short = 0;
        var excess = 0;
        var unallocated = 0;

        var OnRoleSal = 0;
        var BudgetedSal = 0;


        for (var i = 0; i < list.length; i++) {

            if (baseService.isUndefinedOrNull($scope.hrStatus.pstatus) || $scope.hrStatus.pstatus == "") {

                $scope.list = list.filter(function (ls) {
                    return ls.IsDirect == "General";
                });
            }
            else if ($scope.hrStatus.pstatus === "Indirect") {
                $scope.list = list.filter(function (ls) {
                    return ls.IsDirect == "Indirect";
                });

            }
            else if ($scope.hrStatus.pstatus === "Direct") {
                $scope.list = list.filter(function (ls) {
                    return ls.IsDirect == "Direct";
                });

            }
        }



        //$scope.list = list;
        angular.forEach($scope.list, function (item, i) {
            CurrentTotalEmp += item.TotalManpower;
            proposedTotalEmp += item.ProposedManpowerBudget;
            Short += item.Short;
            excess += item.Excess;
            unallocated += item.Unallocated;
            OnRoleSal += item.OnRoleSalaryC;
            BudgetedSal += item.ProposedSalaryC;
        });
        $scope.currentTotalEmp = CurrentTotalEmp;
        $scope.proposedTotalEmp = proposedTotalEmp;
        $scope.Short = Short;
        $scope.excess = excess;
        $scope.unallocated = unallocated;

        $scope.chartList = [];
        $scope.chartList.push(proposedTotalEmp);
        $scope.chartList.push(CurrentTotalEmp);

        $scope.chartDataSalary = [];
        $scope.chartDataSalary.push(BudgetedSal);
        $scope.chartDataSalary.push(OnRoleSal);

        $scope.chartLabel = ['Budgeted', 'On Role'];
        $scope.chartLabelSal = ['Budgeted Salary', 'On Role Salary'];
    }
    function createChart() {
        Chart.defaults.global.legend.display = false;
        var MPctx = document.getElementById("ManPowerbarChart").getContext('2d');
        if (ManPowerbarChart !== undefined && typeof ManPowerbarChart === 'object' && typeof ManPowerbarChart.destroy === 'function') ManPowerbarChart.destroy();
        ManPowerbarChart = new Chart(MPctx, {
            type: 'bar',
            data: {
                labels: $scope.chartLabel,
                datasets: [{
                    data: $scope.chartList,
                    backgroundColor: ['rgba(46, 204, 113,.6)', 'rgba(240, 52, 52, .6)'],
                    borderColor: ['rgba(46, 204, 113,.8)', 'rgba(240, 52, 52, .8)'],
                    fill: true,
                    borderWidth: 2
                }]
            },
            options: {
                legend: {
                    onClick: (e) => e.stopPropagation()
                },
                title: {
                    display: true,
                    text: 'Manpower Forecast'
                },
                label: false,
                hover: { mode: null },
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var value = data.datasets[0].data[tooltipItem.index];
                            value = value.toString();
                            value = value.split(/(?=(?:...)*$)/);
                            value = value.join(',');
                            return value;
                        }
                    }
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                            userCallback: function (value, index, values) {
                                value = value.toString();
                                value = value.split(/(?=(?:...)*$)/);
                                value = value.join(',');
                                return value;
                            }
                        }
                    }],
                    xAxes: [{
                        ticks: {
                        },
                        barThickness: 60
                    }]
                }
            }
        });

        Chart.defaults.global.legend.display = false;
        var salctx = document.getElementById("salBarChart").getContext('2d');
        if (salarybarChart !== undefined && typeof salarybarChart === 'object' && typeof salarybarChart.destroy === 'function') salarybarChart.destroy();
        salarybarChart = new Chart(salctx, {
            type: 'bar',
            data: {
                labels: $scope.chartLabelSal,
                datasets: [{
                    data: $scope.chartDataSalary,
                    backgroundColor: ['rgba(46, 204, 113,.6)', 'rgba(240, 52, 52, .6)'],
                    borderColor: ['rgba(46, 204, 113,.8)', 'rgba(240, 52, 52, .8)'],
                    fill: true,
                    borderWidth: 2
                }]
            },
            options: {
                legend: {
                    onClick: (e) => e.stopPropagation()
                },
                label: false,
                hover: { mode: null },
                title: {
                    display: true,
                    text: 'Salary Forecast'
                },
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var value = data.datasets[0].data[tooltipItem.index];
                            value = value.toString();
                            value = value.split(/(?=(?:...)*$)/);
                            value = value.join(',');
                            return value;
                        }
                    } // end callbacks:
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true
                            //userCallback: function (value, index, values) {
                            //    // Convert the number to a string and splite the string every 3 charaters from the end

                            //    if (value > 10)
                            //    {
                            //        value = value.toString();
                            //        value = value.split(/(?=(?:...)*$)/);
                            //        value = value.join(',');
                            //        return value;
                            //    }
                            //    else {
                            //        return '' + value;

                            //    }
                            //}
                        }
                    }],
                    xAxes: [{
                        ticks: {
                        },
                        barThickness: 60
                    }]
                }
            }
        });
    }

    function getDrillDownList() {
        $http({
            method: 'POST',
            url: 'ManpowerBudgetDashboard/GetDrillDownListJSON/',
            data: {},
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
                    $scope.ModalColList.push(row);
                }
            }
        });
    }

    function GetCompanyDrillDownList(companyId) {
        $http({
            method: 'GET',
            url: 'ManpowerBudgetDashboard/GetCompanyDrillDownListJSON/',
            data: {},
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
                    $scope.ModalColList.push(row);
                }
            }
        });
    }

    function createCompanyColList(companyId) {
        if (baseService.arrayLength($scope.list) >= 0) {
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
            row.Id = $scope.list[0].CompanyGroupId;
            row.StandardName = "Group";
            row.ColumnName = "Group";
            row.Text = $scope.list[0].GroupName;
            row.Name = $scope.list[0].GroupName;
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
            rowc.Id = $scope.list[0].CompanyId;
            rowc.StandardName = "Company";
            rowc.ColumnName = "Company";
            rowc.Text = $scope.list[0].UId;
            rowc.Name = $scope.list[0].UserName;
            rowc.date = $scope.date;
            $scope.ColList.push(rowc);
            GetCompanyDrillDownList(companyId);
        }
    }

    function createColList() {
        if (baseService.arrayLength($scope.list) >= 0) {
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
            row.Id = $scope.list[0].CompanyGroupId;
            row.StandardName = "Group";
            row.ColumnName = "Group";
            row.Text = $scope.list[0].GroupName;
            row.Name = $scope.list[0].GroupName;
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
            rowc.Id = $scope.list[0].CompanyId;
            rowc.StandardName = "Company";
            rowc.ColumnName = "Company";
            rowc.Text = $scope.list[0].UId;
            rowc.Name = $scope.list[0].UserName;
            rowc.date = $scope.date;
            $scope.ColList.push(rowc);
            getDrillDownList();
        }
    }
    $scope.setIndex = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UId;
                $scope.ColList[i].Name = x.UserName;
            }
        }
    };
    function getCol(seq) {
        for (var i = 0; i < baseService.arrayLength($scope.ColList); i++) {
            if ($scope.ColList[i].Sequence === seq) {
                return $scope.ColList[i].ColumnName;
            }
        }
    }
    $scope.setIndexHead = function (x) {
        $scope.index = x.Sequence;
    };

    $scope.EmpCategory = function (EmplyeeTypeOrCategoryId) {
        $http({
            method: 'POST',
            url: 'ManpowerBudgetDashboard/GetGroupWiseCompanyList/',
            data: { 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setList(response.data);
            createChart();
            $scope.index = -1;
            $scope.stIndex = $scope.index - 1;
        });
    };

    $scope.dFunction = function () {
        $scope.clickCount = 0;
        $http({
            method: 'POST',
            url: 'ManpowerBudgetDashboard/GetGroupWiseCompanyList/',
            data: { 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setList(response.data);
            createChart();
            $scope.index = -1;
            $scope.stIndex = $scope.index - 1;
        });
    };

    $scope.directIndirect = function () {
        $scope.clickCount = 0;
        $http({
            method: 'POST',
            url: 'ManpowerBudgetDashboard/GetGroupWiseCompanyList/',
            data: { 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setList(response.data);
            createChart();
            $scope.index = -1;
            $scope.stIndex = $scope.index - 1;
        });
    };

    $scope.headerNav = function (x) {
        $scope.clickCount = 0;
        if (x.Sequence !== -2) {
            $scope.setIndexHead(x);
            $scope.GetDetailDrillDownTableJS(x.Id);
        }
        else {
            $scope.setIndexHead(x);
            $http({
                method: 'POST',
                url: 'ManpowerBudgetDashboard/GetGroupWiseCompanyList/',
                data: {
                    'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId
                },

                dataType: 'JSON'
            }).then(function successCallback(response) {
                setList(response.data);
                createChart();
                $scope.index = -1;
                $scope.stIndex = $scope.index - 1;
            });
        }
    };
    $scope.EmployeeSummaryParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'BudgetCode',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true
    };
    $scope.getModalEmployeeSummaryJSPagn = function (pageno) {
        $scope.getModalEmployeeSummaryJS(pageno);
    };
    $scope.getModalEmployeeSummaryJS = function (pageno) {
        //$scope.DEmpList = [];

        //if (baseService.isUndefinedOrNull($scope.EmployeeSummaryParameters.searchBy) === false &&
        //    baseService.isUndefinedOrNull($scope.EmployeeSummaryParameters.search) === false &&
        //    undefined === pageno) {
        //    $scope.EmployeeSummaryParameters.offset = 0;
        //} else if (pageno === 0) {
        //    $scope.EmployeeSummaryParameters.offset = 0;
        //    baseService.setCurrentPage('DEmpList');
        //}
        //else if (!baseService.isUndefinedOrNull(pageno) && pageno > 0) {
        //    $scope.EmployeeSummaryParameters.offset = $scope.EmployeeSummaryParameters.limit * (pageno - 1);
        //}
        //var tormData = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.EmployeeSummaryParameters };
        //baseService.paginationPost('ManpowerBudgetDashboard/ModalEmployeeSummary/', tormData)
        //    .then(function (result) {
        //        $scope.DEmpList = result.Rows;
        //        $scope.EmployeeSummaryParameters.total_count = result.Total;

        //        $scope.propertyName = '';
        //        $scope.reverse = true;
        //        $scope.sortBy = function (propertyName) {
        //            $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
        //            $scope.propertyName = propertyName;
        //        };
        //        angular.element(document.querySelector('#EmployeeModalSummary')).modal('show');
        //    }, function () {
        //        ShowResult(commonMessage.NetworkError, 'failure');
        //    }).finally(function () {
        //    });

        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.EmployeeSummaryParameters };

        $scope.searchbyonRoleEmpList = [];
        //$scope.setModal(data);
        //var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of OnRole Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'ManpowerBudgetDashboard/ModalEmployeeSummary/',
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
                    //scrollSettings: { width: 1200, height: 400 }
                    minWidth: 400,
                    isResponsive: true
                });
                $scope.dataGrid = "#onRoleEmpGrid";
              //  angular.element(document.querySelector('#EmployeeModalSummary')).modal('show');
  var eDialog = $("#EmployeeModalSummary").data("ejDialog");
              eDialog.open();
            }
        });
    };

    $scope.EmployeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'BudgetCode',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
        tempData: {}
    };

    $scope.GetModalEmployeeDetailJSPagination = function (pageno) {
        $scope.GetModalEmployeeDetailJS(pageno, $scope.EmployeeParameters.tempData);
    };

    $scope.GetModalEmployeeDetailJS = function (pageno, data) {
        

        $scope.searchbyOnRoleDetailEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'companyId': data.CompanyId, 'seq': $scope.index, 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.EmployeeParameters };
        $scope.status = "List of OnRole Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'ManpowerBudgetDashboard/ModalEmployeeDetail/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyOnRoleDetailEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyOnRoleDetailEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyOnRoleDetailEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyOnRoleDetailEmpList[i].Value, visible: true, width: "180px" });
                }
                var scrollerwidth = $("#onRoleEmpGridDetail").width();//Obtain the width of the container
                $('#onRoleEmpGridDetail').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    isResponsive: true
                });
                $scope.dataGrid = "#onRoleEmpGridDetail";
                //var dataGridRole = "#onRoleEmpGridDetail";

                //dataGridRole.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                //angular.element(document.querySelector('#EModalDetail')).modal('show');
                
                var eDialog = $("#EModalDetail").data("ejDialog");
              eDialog.open();
            }

        });
    };

    $scope.BudgetSummaryParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'Excess',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true
    };
    $scope.GetModalBudgetSummaryJSPagn = function (pageno) {
        $scope.GetModalBudgetSummaryJS(pageno);
    };
    $scope.GetModalBudgetSummaryJS = function (pageno) {

        $scope.searchbyOnRoleDetailEmpList = [];
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.BudgetSummaryParameters };

        $scope.status = "Manpower Budget Details";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'ManpowerBudgetDashboard/ModalBudgetSummary/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyOnRoleDetailEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyOnRoleDetailEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyOnRoleDetailEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyOnRoleDetailEmpList[i].Value, visible: true, width: "180px" });
                }
                var scrollerwidth = $("#ManpowerBudgetInfo").width();//Obtain the width of the container
                $('#ManpowerBudgetInfo').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    isResponsive: true
                });
                $scope.dataGrid = "#ManpowerBudgetInfo";
               // angular.element(document.querySelector('#BudgetSummaryModal')).modal('show');
  var eDialog = $("#BudgetSummaryModal").data("ejDialog");
              eDialog.open();
            }

        });
    };

    $scope.BudgetDetailParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'Excess',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
        tempData: {}
    };

    $scope.GetModalBudgetDetailJSPagination = function (pageno) {
        $scope.GetModalBudgetDetailJS(pageno, $scope.BudgetDetailParameters.tempData);
    };

    $scope.GetModalBudgetDetailJS = function (pageno, data) {
        
        $scope.searchbyOnRoleDetailEmpList = [];
        //$scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'companyId': data.CompanyId, 'seq': $scope.index, 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.EmployeeParameters };
        $scope.status = "Manpower Budget Details";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'ManpowerBudgetDashboard/ModalBudgetDetail/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyOnRoleDetailEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyOnRoleDetailEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyOnRoleDetailEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyOnRoleDetailEmpList[i].Value, visible: true, width: "180px" });
                }
                var scrollerwidth = $("#ManpowerBudgetDetail").width();//Obtain the width of the container
                $('#ManpowerBudgetDetail').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    isResponsive: true,
                    
                    title: "Total", summaryColumns: [
                        {
                            summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Proposed", dataMember: "Proposed", format: "{0:N2}"
                        }],
                    showCaptionSummary: true
                });
                $scope.dataGrid = "#ManpowerBudgetDetail";

                //dataGridRole.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
               // angular.element(document.querySelector('#BudgetDetailModal')).modal('show');
                var eDialog = $("#BudgetDetailModal").data("ejDialog");
              eDialog.open();
            }

        });
    };

    $scope.ExcessSummaryParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'Excess',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
    };
    $scope.GetModalExcessSummaryJSPagn = function (pageno) {
        $scope.ExcessSummary(pageno);
    };
    $scope.ExcessSummary = function (pageno) {
        
        $scope.searchbyOnRoleDetailEmpList = [];
        //$scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'parameters': $scope.ExcessSummaryParameters };

        //var parameters = { 'ChartColumnList': $scope.ColList, 'companyId': data.CompanyId, 'seq': $scope.index, 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.EmployeeParameters };
        $scope.status = "Manpower Budget Details";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'ManpowerBudgetDashboard/ModalExcessSummary/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyOnRoleDetailEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyOnRoleDetailEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyOnRoleDetailEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyOnRoleDetailEmpList[i].Value, visible: true, width: "180px" });
                }
                var scrollerwidth = $("#ManpowerBudgeExcesstDetail").width();//Obtain the width of the container
                $('#ManpowerBudgeExcesstDetail').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    isResponsive: true
                });
                $scope.dataGrid = "#ManpowerBudgeExcesstDetail";

                //dataGridRole.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
               // angular.element(document.querySelector('#ExcessModalSummary')).modal('show');
    var eDialog = $("#ExcessModalSummary").data("ejDialog");
              eDialog.open();
            }

        });
    };

    $scope.ExcessDetailParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'Excess',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
        tempData: {}
    };
    $scope.GetModalExcessDetailJSPagination = function (pageno) {
        $scope.GetExcessDetail(pageno, $scope.ExcessDetailParameters.tempData);
    };
    $scope.GetExcessDetail = function (pageno, data) {

        $scope.searchbyOnRoleDetailEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'companyId': data.CompanyId, 'seq': $scope.index, 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'parameters': $scope.ExcessDetailParameters };
        $scope.status = "Manpower Budget Details";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'ManpowerBudgetDashboard/ModalExcessDetail/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyOnRoleDetailEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyOnRoleDetailEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyOnRoleDetailEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyOnRoleDetailEmpList[i].Value, visible: true, width: "180px" });
                }
                var scrollerwidth = $("#ManpowerBudgeExcesstDetailList").width();//Obtain the width of the container
                $('#ManpowerBudgeExcesstDetailList').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    isResponsive: true
                });
                $scope.dataGrid = "#ManpowerBudgeExcesstDetailList";

                //angular.element(document.querySelector('#ExcessDetailList')).modal('show');
       var eDialog = $("#ExcessDetailList").data("ejDialog");
              eDialog.open();
            }

        });
    };

    $scope.ShortSummaryParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'short',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true
    };
    $scope.GetModalShortSummaryJSPagn = function (pageno) {
        $scope.GetShortSummary(pageno);
    };
    $scope.GetShortSummary = function (pageno) {
        $scope.searchbyOnRoleDetailEmpList = [];
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'parameters': $scope.ExcessSummaryParameters };

        $scope.status = "Budget Wise Short List";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'ManpowerBudgetDashboard/ModalShortSummary/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyOnRoleDetailEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyOnRoleDetailEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyOnRoleDetailEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyOnRoleDetailEmpList[i].Value, visible: true, width: "180px" });
                }
                var scrollerwidth = $("#ShortSummaryModalList").width();//Obtain the width of the container
                $('#ShortSummaryModalList').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    isResponsive: true
                });
                $scope.dataGrid = "#ShortSummaryModalList";

               // angular.element(document.querySelector('#ShortSummaryModal')).modal('show');

 var eDialog = $("#ShortSummaryModal").data("ejDialog");
              eDialog.open();
            }

        });
    };

    $scope.ShortDetailParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'short',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
        tempData: {}
    };

    $scope.GetModalShortDetailJSPagination = function (pageno) {
        $scope.GetShortDetail(pageno, $scope.ShortDetailParameters.tempData);
    };

    $scope.GetShortDetail = function (pageno, data) {

        $scope.searchbyOnRoleDetailEmpList = [];
        var parameters = { 'ChartColumnList': $scope.ColList, 'companyId': data.CompanyId, 'seq': $scope.index, 'date': $scope.Date, 'status': $scope.hrStatus.pstatus, 'parameters': $scope.ShortDetailParameters };

        $scope.status = "Budget Wise Short List";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'ManpowerBudgetDashboard/ModalShortDetail/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyOnRoleDetailEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyOnRoleDetailEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyOnRoleDetailEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyOnRoleDetailEmpList[i].Value, visible: true, width: "180px" });
                }
                var scrollerwidth = $("#ShortDetailModalList").width();//Obtain the width of the container
                $('#ShortDetailModalList').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    isResponsive: true
                });
                $scope.dataGrid = "#ShortDetailModalList";

                //angular.element(document.querySelector('#ShortDetailModal')).modal('show');
   var eDialog = $("#ShortDetailModal").data("ejDialog");
              eDialog.open();
            }

        });

    };

    $scope.BudgetCodeWiseEmpParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'EmployeeName',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
        tempData: {}
    };

    $scope.GetModalBudgetCodeWiseEmpJSPagination = function (pageno) {
        $scope.getBudgetCodeWiseEmpList(pageno, $scope.BudgetCodeWiseEmpParameters.tempData);
    };

    $scope.getBudgetCodeWiseEmpList = function (pageno, data) {

        //if (baseService.isUndefinedOrNull($scope.BudgetCodeWiseEmpParameters.searchBy) === false &&
        //    baseService.isUndefinedOrNull($scope.BudgetCodeWiseEmpParameters.search) === false &&
        //    undefined === pageno) {
        //    $scope.BudgetCodeWiseEmpParameters.offset = 0;
        //} else if (pageno === 0) {
        //    $scope.BudgetCodeWiseEmpParameters.offset = 0;
        //    baseService.setCurrentPage('BudgetCWiseEmp');
        //}
        //else if (!baseService.isUndefinedOrNull(pageno) && pageno > 0) {
        //    $scope.BudgetCodeWiseEmpParameters.offset = $scope.BudgetCodeWiseEmpParameters.limit * (pageno - 1);
        //}
        //var formData = { 'ChartColumnList': $scope.ColList, 'budgetCode': data.MbId, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.BudgetCodeWiseEmpParameters };
        //baseService.paginationPost('ManpowerBudgetDashboard/BudgetCodeWiseEmpList/', formData)
        //    .then(function (result) {
        //        $scope.BudgetCWiseEmp = result.Rows;
        //        $scope.BudgetCodeWiseEmpParameters.total_count = result.Total;

        //        $scope.propertyName = '';
        //        $scope.reverse = true;
        //        $scope.sortBy = function (propertyName) {
        //            $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
        //            $scope.propertyName = propertyName;
        //        };
        //        angular.element(document.querySelector('#BudgetWiseEmpList')).modal('show');
        //    }, function () {
        //        ShowResult(commonMessage.NetworkError, 'failure');
        //    }).finally(function () {
        //    });

        //$scope.BudgetCWiseEmp = [];


        $scope.BudgetCodeWiseEmpParameters.tempData = data;
        $scope.BEmpBudgetCode = data.BudgetCode;
        $scope.BudgetCWiseEmp = [];
        $scope.BudgetedTotal;
        $scope.setModal(data);
        $scope.searchbyOnRoleDetailEmpList = [];
        var parameters = { 'ChartColumnList': $scope.ColList, 'budgetCode': data.MbId, 'EmplyeeTypeOrCategoryId': $scope.mpDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.BudgetCodeWiseEmpParameters };

        $scope.status = "Budget Code Wise Employee List";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'ManpowerBudgetDashboard/BudgetCodeWiseEmpList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyOnRoleDetailEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyOnRoleDetailEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyOnRoleDetailEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyOnRoleDetailEmpList[i].Value, visible: true, width: "180px" });
                }
                var scrollerwidth = $("#ShortDetailModalList").width();//Obtain the width of the container
                $('#ShortDetailModalList').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    isResponsive: true
                });
                $scope.dataGrid = "#ShortDetailModalList";

                angular.element(document.querySelector('#BudgetWiseEmpList')).modal('show');
            }

        });
    };

    $scope.setModal = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UId;
            }
        }
    };

    $scope.OnRoleEmpReportXLx = function () {
        var dataList = [];
        var g = $("#onRoleEmpGridDetail").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.onRoleEmpList;
        }

        $scope.fileName = 'On Role Employee Report.xlsx';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

        $http({
            method: 'POST',
            url: "HumanResource/ManpowerBudgetDashboard/OnRoleEmployeeReport",
            data: { 'data': dataList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.printReportBudget = function () {
        var dataList = [];
        var g = $("#ManpowerBudgetDetail").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.onRoleEmpList;
        }

        $scope.fileName = 'Man Power Budget Budgeted Report.xlsx';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

        $http({
            method: 'POST',
            url: "HumanResource/ManpowerBudgetDashboard/BudgetEmployeeReport",
            data: { 'data': dataList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.printReportShort = function () {
        var dataList = [];
        var g = $("#ShortDetailModalList").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.onRoleEmpList;
        }

        $scope.fileName = 'Man Power Budget Budgeted Report.xlsx';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

        $http({
            method: 'POST',
            url: "HumanResource/ManpowerBudgetDashboard/ShortEmployeeReport",
            data: { 'data': dataList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.printReportExcess = function () {
        var dataList = [];
        var g = $("#ManpowerBudgeExcesstDetailList").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.onRoleEmpList;
        }

        $scope.fileName = 'Man Power Budget Budgeted Report.xlsx';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

        $http({
            method: 'POST',
            url: "HumanResource/ManpowerBudgetDashboard/ExcessEmployeeReport",
            data: { 'data': dataList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

}