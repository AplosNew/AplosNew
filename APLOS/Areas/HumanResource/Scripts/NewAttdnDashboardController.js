'use strict';
NewAttdnDashboardController.$inject = ['cboService', '$scope', '$rootScope', '$routeParams', 'baseService', '$http', '$filter', '$window'];
function NewAttdnDashboardController(cboService, $scope, $rootScope, $routeParams, baseService, $http, $filter, $window) {

    $scope.Title = "Daily In Status";
    $scope.chartList = [];
    $scope.list = [];
    $scope.index = -1;
    $scope.chartLabel = [];
    $scope.ColList = [];
    $scope.ModalColList = [];
    $scope.stIndex = -2;
    var ManPowerbarChart;
    var salarybarChart;
    $scope.Date = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy');


    $scope.Stat = "All";
    $scope.EmpCat = null;
    $scope.EmpShift = null;
    $scope.PhysicalVerification = false;

    $scope.docEmployeeCategoryList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.docEmployeeCategoryList = result;
    });

    // Get Shift  Aman

    $scope.ShiftList = [];
    $scope.getShift = function () {
        $http({
            method: 'GET',
            url: "NewAttdnDashboard/GetShift",
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.getShift();

    $scope.EmpStatsList = [{ 'Value': 'All', 'Text': 'Select All' }, { 'Value': 'Active', 'Text': 'Active' }, { 'Value': 'TBS', 'Text': 'To Be Separated' }, { 'Value': 'LA', 'Text': 'LONG ABSENTEEISM' }];
    $scope.EmpStat = 'All';
    $scope.ManPowerBudget = function () {
        $scope.chartList = [];
        var currentTotalEmp = 0;
        var proposedTotalEmp = 0;
        var Short = 0;
        var excess = 0;
        var unallocated = 0;
        $scope.ManPowerList = [];
        $http({
            method: 'POST',
            url: 'NewAttdnDashboard/GetGroupWiseCompanyList',
            data: {
                'date': $scope.Date,
                'stat': $scope.Stat,
                'EmpCat': $scope.EmpCat,
                'EmpStat': $scope.EmpStat,
                'EmpShift': $scope.EmpShift,
                'pv': $scope.PhysicalVerification
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            setList(response.data);

            createColList();

        });
    };
    $scope.ManPowerBudget();

    $scope.GetDetailDrillDownTableJS = function (data) {
        //$scope.clickCount++;
        //if ($scope.clickCount == 1) {
        //    createCompanyColList(data.CompanyId)
        //}
        var getRow = $filter("filter")($scope.ColList, { "ColumnName": "Company" });
        //createColListWithCompany(getRow[0].Id);

        $scope.DDList = [];
        if ($scope.index + 3 < $scope.ColList.length) {
            $http({
                method: 'POST',
                url: 'NewAttdnDashboard/GetDetailDrillDownTable/',
                data: {
                    'ChartColumnList': $scope.ColList,
                    'seq': $scope.index,
                    'date': $scope.Date,
                    'stat': $scope.Stat,
                    'EmpCat': $scope.EmpCat,
                    'EmpStat': $scope.EmpStat,
                    'EmpShift': $scope.EmpShift,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.DDList = response.data;
                setList(response.data);

                $scope.index += 1;
                $scope.stIndex = $scope.index - 1;
            });
        }
    };



    function getDrillDownList(companyId) {
        $http({
            method: 'POST',
            url: 'NewAttdnDashboard/GetDrillDownListJSON/?CompanyId=' + companyId,
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
    //$scope.GetDrillDownAttnStatus = function (data) {
    //    var getRow = $filter("filter")($scope.ColList, { "ColumnName": "Company" });
    //    createColListWithCompany(getRow[0].Id);
    //};
    function GetCompanyDrillDownList(companyId) {
        $http({
            method: 'GET',
            url: 'NewAttdnDashboard/GetCompanyDrillDownListJSON/?CompanyId=' + companyId,
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
            rowc.Text = $scope.list[0].UserName;
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
            rowc.Text = $scope.list[0].UserName;
            rowc.Name = $scope.list[0].UserName;
            rowc.date = $scope.date;
            $scope.ColList.push(rowc);
            getDrillDownList();
        }
    }


    $scope.setIndex = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.Id;
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




    $scope.dFunction = function () {
        $scope.clickCount = 0;
        $http({
            method: 'POST',
            url: 'NewAttdnDashboard/GetGroupWiseCompanyList/',
            data: {
                'date': $scope.Date, 'stat': $scope.Stat,
                'EmpCat': $scope.EmpCat, 'EmpStat': $scope.EmpStat, 'EmpShift': $scope.EmpShift, 'pv': $scope.PhysicalVerification
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setList(response.data);
            $scope.ColList = [];
            createColList();
            $scope.index = -1;
            $scope.stIndex = $scope.index - 1;
        });
    };



    $scope.headerNav = function (x) {
        $scope.clickCount = 0;
        if (x.Sequence !== -2) {
            $scope.setIndexHead(x);
            $scope.GetDetailDrillDownTableJS(x);
        }
        else {
            $scope.setIndexHead(x);
            $http({
                method: 'POST',
                url: 'NewAttdnDashboard/GetGroupWiseCompanyList/',
                data: {
                    'date': $scope.Date, 'stat': $scope.Stat,
                    'EmpCat': $scope.EmpCat, 'EmpStat': $scope.EmpStat, 'EmpShift': $scope.EmpShift, 'pv': $scope.PhysicalVerification
                },

                dataType: 'JSON'
            }).then(function successCallback(response) {
                setList(response.data);

                $scope.index = -1;
                $scope.stIndex = $scope.index - 1;
            });
        }
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

        $scope.list = list;
        angular.forEach(list, function (item, i) {
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


    }

    // On Click on the Table
    $scope.ClickDetail = [];
    $scope.RptData = [];
    $scope.RptColumn = "";

    $scope.TableClick = function (data, column) {
        $scope.RptData = data;
        $scope.RptColumn = column;
        $http({
            method: 'POST',
            url: 'NewAttdnDashboard/DetailTableClick/',
            data: {
                'ChartColumnList': $scope.ColList,
                'seq': $scope.index,
                'date': $scope.Date,
                'Column': column,
                'data': data,
                'stat': $scope.Stat,
                'EmpCat': $scope.EmpCat,
                'EmpStat': $scope.EmpStat,
                'EmpShift': $scope.EmpShift,
                'pv': $scope.PhysicalVerification
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ClickDetail = [];
            $scope.ClickDetail = response.data;
            if ($scope.RptColumn == "BB") {
                angular.element(document.querySelector('#TableDetailModalBB')).modal('show');

            }
            else if ($scope.RptColumn == "LateIn") {
                angular.element(document.querySelector('#TableDetailModalLateIn')).modal('show');

            }
            else if ($scope.RptColumn == "InMissing") {
                angular.element(document.querySelector('#TableDetailModalInMissing')).modal('show');

            }
            else {
                angular.element(document.querySelector('#TableDetailModal')).modal('show');
            }
            var gridObj = $("#getClickDetail").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            gridObj.clearFiltering();
        });
    }

    // On Downloading the Excel
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.printReport = function () {
        //var dataList = [];
        //var g = $("#getClickDetail").data("ejGrid");
        //dataList = g.getFilteredRecords();

        //if (dataList.length == 0) {
        //    dataList = $scope.ClickDetail;
        //}
        //if (dataList.length == 0) {
        //    throw "First click on View button.";
        //}

        //$scope.fileName = "Daily Production Report.xlsx";
        $http({
            method: 'POST',
            url: 'NewAttdnDashboard/GetPrintReport',
            data: {
                'ChartColumnList': $scope.ColList,
                'seq': $scope.index,
                'date': $scope.Date,
                'Column': $scope.RptColumn,
                'data': $scope.RptData,
                'stat': $scope.Stat,
                'EmpCat': $scope.EmpCat,
                'EmpStat': $scope.EmpStat,
                'EmpShift': $scope.EmpShift,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }


    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.OnRoleprintReport = function () {
        var dataList = [];
        var g = $("#getClickDetail").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.OnRoleClickDetail;
        }
        if (dataList.length == 0) {
            throw "First click on View button.";
        }

        $scope.fileName = "Daily Production Report.xlsx";
        $http({
            method: 'POST',
            url: 'NewAttdnDashboard/GetOnRolePrintReport',
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.exportgriddataUrlUpdate2 = 'GridReports/ExcelExportUpdate2';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.DownLoadEmpData = function () {
        var dataList = [];
        var g = $("#getClickDetails").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ClickDetail;
        }
        $scope.fileName = $filter("dateFiltering")(Date.now()) + "-Budget";
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };



}