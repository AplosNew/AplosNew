'use strict';
dashBoardController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$cookies'];
function dashBoardController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $cookies) {

    $scope.ColList = [];
    var OAChart;
    var LChart;
    var NLChart;
    var docbarChart;

    $scope.OverAllStatusJSON = function () {
        $scope.overAllTotalSql = [];
        $scope.PreDocSubmitted = [];
        $scope.LoggedInStat = [];
        $scope.mendatorySOVD = 0;
        $scope.optionalSOVD = 0;
        $scope.mendatoryNC = 0;
        $scope.optionalNC = 0;
        $scope.mendatoryS = 0;
        $scope.optionalS = 0;
        $scope.mendatoryLoggedInDocOVD = 0;
        $scope.optionalLoggedInDocOVD = 0;
        $scope.mendatoryNLoggedInDocOVD = 0;
        $scope.optionalNLoggedInDocOVD = 0;
        $scope.mendatoryLoggedInDoc = 0;
        $scope.optionalLoggedInDoc = 0;
        $scope.mendatoryNLoggedInDoc = 0;
        $scope.optionalNLoggedInDoc = 0;


        $http({
            method: 'POST',
            url: 'Recruitments/predashboard/OverAllStatus/',

            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.overAllTotalSql = response.data.fg;
            $scope.PreDocSubmitted = response.data.PreDocSubmitted

            setList(response.data.fg);
            createChart();

            setDocList(response.data.PreDocSubmitted);

            setNSDocList(response.data.PreDocNotSubmitted);
            createDocChart();


            $scope.selDoc = response.data.selDoc;

            angular.forEach(response.data.selDoc, function (item, i) {

                if (item.OptionalOrMandatory == "Mandatory") {
                    $scope.mendatoryS = item.totalDoc;
                }
                else if (item.OptionalOrMandatory == "Optional") {
                    $scope.optionalS = item.totalDoc;
                }
            });
            angular.forEach(response.data.selDocOVD, function (item, i) {


                if (item.OptionalOrMandatory == "Mandatory") {
                    $scope.mendatorySOVD = item.totalDoc;

                }
                else if (item.OptionalOrMandatory == "Optional") {
                    $scope.optionalSOVD = item.totalDoc === 'undefined' ? 0 : item.totalDoc;
                }
            });

            angular.forEach(response.data.LoggedInDoc, function (item, i) {
                if (item.OptionalOrMandatory == "Mandatory") {
                    $scope.mendatoryLoggedInDoc = item.totalDoc;
                }
                else if (item.OptionalOrMandatory == "Optional") {
                    $scope.optionalLoggedInDoc = item.totalDoc;
                }

            });
            angular.forEach(response.data.LoggedInDocOVD, function (item, i) {
                if (item.OptionalOrMandatory == "Mandatory") {
                    $scope.mendatoryLoggedInDocOVD = item.totalDoc;
                }
                else if (item.OptionalOrMandatory == "Optional") {
                    $scope.optionalLoggedInDocOVD = item.totalDoc;
                }

            });
            angular.forEach(response.data.NotLoggedInDoc, function (item, i) {
                if (item.OptionalOrMandatory == "Mandatory") {
                    $scope.mendatoryNLoggedInDoc = item.totalDoc;
                }
                else if (item.OptionalOrMandatory == "Optional") {
                    $scope.optionalNLoggedInDoc = item.totalDoc;
                }

            });
            angular.forEach(response.data.NotLoggedInDocOVD, function (item, i) {
                if (item.OptionalOrMandatory == "Mandatory") {
                    $scope.mendatoryNLoggedInDocOVD = item.totalDoc;
                }
                else if (item.OptionalOrMandatory == "Optional") {
                    $scope.optionalNLoggedInDocOVD = item.totalDoc;
                }

            });
        });
    }
    $scope.OverAllStatusJSON();

    $scope.mendatoryNCDoc = 0;
    $scope.optionalNCDoc = 0
    $scope.NotConfirmeddoc = function () {
        $http({
            method: 'POST',
            url: 'Recruitments/predashboard/NotConfirmeddoc/',
            //data: { 'OrgStructureList': $scope.ColList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            angular.forEach(response.data, function (item, i) {

                if (item.OptionalOrMandatory == "Mandatory") {
                    $scope.mendatoryNCDoc = item.totalDoc;

                }
                else if (item.OptionalOrMandatory == "Optional") {
                    $scope.optionalNCDoc = item.totalDoc;
                }
            });

        });
    }
    $scope.NotConfirmeddoc();


    //---------------------Chart---------------------------//
    function setList(list) {

        $scope.chartLabel = [];

        var NotSel = 0;
        var Selected = 0;
        var ToverDue = 0;
        var NotConfirmed = 0;

        var LoggedIn = 0;
        var LIoverDue = 0;

        var NotLoggedIn = 0;
        var NLIoverDue = 0;

        $scope.list = list;
        angular.forEach(list, function (item, i) {

            NotSel = item.notSelected;
            Selected = item.Selected;
            ToverDue = item.TOverDue;
            NotConfirmed = item.NotConfirmed;

            LoggedIn = item.LoggedIn;
            LIoverDue = item.LOverDue;

            NotLoggedIn = item.NotLoggedIn;
            NLIoverDue = item.NLOverDue;
        });

        $scope.chartList1 = [];
        $scope.chartList2 = [];
        $scope.chartList3 = [];

        $scope.chartList1.push(NotSel);
        $scope.chartList1.push(Selected);
        $scope.chartList1.push(ToverDue);
        $scope.chartList1.push(NotConfirmed);

        $scope.chartList2.push(LoggedIn);
        $scope.chartList2.push(LIoverDue);

        $scope.chartList3.push(NotLoggedIn);
        $scope.chartList3.push(NLIoverDue);

        $scope.chartLabel1 = ['Not Selected', 'Selected', 'Over due', 'Not confirmed'];
        $scope.chartLabel2 = ['Logged In', 'Over due'];
        $scope.chartLabel3 = ['Not Logged In', 'Over due'];
    }


    function createChart() {

        var OActx = document.getElementById("OAChart").getContext('2d');
        if (OAChart != undefined && typeof OAChart == 'object' && typeof OAChart.destroy == 'function') OAChart.destroy();
        OAChart = new Chart(OActx, {
            type: 'doughnut',
            data: {
                labels: $scope.chartLabel1,
                datasets: [{
                    data: $scope.chartList1,
                    backgroundColor: ['rgba(11, 15, 63,.6)', 'rgba(17, 224, 93,.9)', 'rgba(240, 52, 52,1)', 'rgba(216, 207, 190, 1)'],
                    fill: true,
                    borderWidth: 2
                }]
            },
            options: {
                tooltips: { enabled: true },
                circumference: Math.PI,
                rotation: 1 * Math.PI,
                legend: {
                    onClick: (e) => e.stopPropagation(),
                    display: false,
                    position: 'bottom'
                },
                cutoutPercentage: 60,
                label: true,
                hover: { mode: null }
            }
        });

        //-----------------------2-------------------------------//
        var lctx = document.getElementById("LoggedInchart").getContext('2d');
        if (LChart != undefined && typeof LChart == 'object' && typeof LChart.destroy == 'function') LChart.destroy();
        LChart = new Chart(lctx, {
            type: 'doughnut',
            data: {
                labels: $scope.chartLabel2,
                datasets: [{
                    data: $scope.chartList2,
                    backgroundColor: ['rgba(46, 204, 113,.6)', 'rgba(240, 52, 52,1)'],               
                    fill: true,
                    borderWidth: 2
                }]
            },
            options: {
                tooltips: { enabled: true },
                circumference: Math.PI,
                rotation: 1 * Math.PI,
                legend: {
                    onClick: (e) => e.stopPropagation(),
                    display: false
                },
                cutoutPercentage: 60,
                label: true,
                hover: { mode: null }
            }
        });
        //------------------------------------------------------//
        //-------------------------3---------------------------//

        var NLctx = document.getElementById("NotLoggedInchart").getContext('2d');
        if (NLChart != undefined && typeof NLChart == 'object' && typeof NLChart.destroy == 'function') NLChart.destroy();
        NLChart = new Chart(NLctx, {
            type: 'doughnut',
            data: {
                labels: $scope.chartLabel3,
                datasets: [{
                    data: $scope.chartList3,
                    backgroundColor: ['rgba(241, 196, 15, .8)', 'rgba(231, 76, 60, 1)'],
                    //borderColor: ['rgba(46, 204, 113,.8)', 'rgba(241, 196, 15,.8)'],
                    fill: true,
                    borderWidth: 2
                }]
            },
            options: {
                tooltips: { enabled: true },
                circumference: Math.PI,
                rotation: 1 * Math.PI,
                legend: {
                    onClick: (e) => e.stopPropagation(),
                    display: false
                },
                cutoutPercentage: 60,
                label: true,
                hover: { mode: null }
            }
        });
    }
    //----------------------------------------------------//

    function setDocList(list) {

        $scope.docChartLabel = [];
        $scope.docChartList = [];
        $scope.ttDocName = [];
        $scope.docType = [];

        var totalDoc = 0;
        var label = '';

        $scope.list = list;
        angular.forEach(list, function (item, i) {

            totalDoc = item.totDoc;
            label = item.docName;

            $scope.docChartLabel.push(item.docName);
            $scope.docChartList.push(item.totDoc);
            $scope.ttDocName.push(item.fullDocName);
            $scope.docType.push(item.OptionalOrMandatory);
        });
    }

    function setNSDocList(list) {
        $scope.docChartLabelNS = [];
        $scope.docChartListNS = []

        var totalDocNS = 0;
        var labelNS = '';

        $scope.list = list;
        angular.forEach(list, function (item, i) {

            totalDocNS = item.totDoc;
            labelNS = item.docName;

            $scope.docChartLabelNS.push(item.docName);
            $scope.docChartListNS.push(item.totDoc);
        });
    }
    window.chartColors = {
        red: 'rgba(240, 52, 52, .6)',
        orange: 'rgb(255, 159, 64)',
        yellow: 'rgb(255, 205, 86)',
        green: 'rgba(46, 204, 113,.6)',
        blue: 'rgb(54, 162, 235)',
        purple: 'rgb(153, 102, 255)',
        grey: 'rgb(201, 203, 207)'
    };

    function createDocChart() {

        Chart.defaults.global.legend.display = false;
        var DocCtx = document.getElementById("docbarChart").getContext('2d');
        if (docbarChart != undefined && typeof docbarChart == 'object' && typeof docbarChart.destroy == 'function') docbarChart.destroy();
        docbarChart = new Chart(DocCtx, {
            type: 'bar',
            data: {
                labels: $scope.docChartLabel,
                datasets: [{
                    label: 'Not Submitted',
                    backgroundColor: window.chartColors.red,
                    borderColor: window.chartColors.red,

                    data: $scope.docChartList,
                },
                {
                    label: 'Submitted',

                    backgroundColor: window.chartColors.green,
                    borderColor: window.chartColors.green,

                    data: $scope.docChartListNS,
                }
                ]
            },
            options: {
                responsive: true,
                title: {
                    display: true,
                    text: 'Douments submission status',
                    position:'bottom'
                },
                tooltips: {
                    mode: 'index',
                    intersect: true,
                  
                    callbacks: {

                        title: function (tooltipItem, data) {
                            return $scope.ttDocName[tooltipItem[0].index];                                                 
                        },
                        afterTitle: function (tooltipItem, data) {
                            return $scope.docType[tooltipItem[0].index];
                        }
                    }
                },
                legend: {
                    display: true,
                    labels: {
                        border:0
                    }
                },
                hover: {
                    mode: 'nearest',
                    intersect: true
                },
                scales: {
                    xAxes: [{
                        display: true,
                        scaleLabel: {
                        },
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 90,
                            minRotation: 90
                        },
                    }],
                    yAxes: [{
                        display: true,
                        scaleLabel: {
                        }
                    }]
                }
            }
        });
    }

    //----------------------------------------------//
    $scope.GetOrgStrunctureListJS = function () {
        $http({
            method: 'POST',
            url: 'predashboard/GetOrgStrunctureList/',
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (baseService.arrayLength(response.data) > 0) {

                for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                    var row = {
                        Id: null,
                        ColumnName: null,
                        RType: null,
                        Text: null,
                        Name: null,
                        date: ''
                    }
                    row.Sequence = i;

                    row.ColumnName = response.data[i].ColumnName;
                    row.RType = response.data[i].Rtype;
                    row.Text = response.data[i].UId;
                    row.date = $scope.date;
                    $scope.ColList.push(row);
                }
            }
        });
    }
    $scope.GetOrgStrunctureListJS();

    //-----------------EmployeeWiseDocuments-------------------//

    $scope.modalIntervieweeWiseDoc = function (list) {
        //$scope.EmpInfo = [];
        var empId = list.Id;
        var empName = list.FullName;
        var GivenDesignation = list.GivenDesignation;

        $scope.empId = empId;
        $scope.empName = empName;
        $scope.GivenDesignation = GivenDesignation;

        $scope.IntervieweeWiseDocList = [];
        $http({
            method: 'POST',
            url: 'Recruitments/predashboard/IntervieweeDocuments/',
            data: { 'EmpId': list.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.IntervieweeWiseDocList = response.data;

            angular.element(document.querySelector('#ModalSelfDocument')).modal('show');
        });
    }

    $scope.IntervieweeWiseDocDept = function (x) {
        var empId = x.Id;
        var empName = x.FullName;
        var GivenDesignation = x.GivenDesignation;

        $scope.empId = empId;
        $scope.empName = empName;
        $scope.GivenDesignation = GivenDesignation;
        $scope.IntervieweeWiseDocDeptList = [];

        $http({
            method: 'POST',
            url: 'Recruitments/predashboard/IntervieweeDocumentsDept/',
            data: { 'EmpId': x.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.IntervieweeWiseDocDeptList = response.data;
            angular.element(document.querySelector("#ModalDeptDocument")).modal('show');
        });
    }
    //////////////////////////////////////////////////
    $scope.IntervieweeWiseDocSelfNU = function (x) {
        var empId = x.Id;
        var empName = x.FullName;
        var GivenDesignation = x.GivenDesignation;

        $scope.empId = empId;
        $scope.empName = empName;
        $scope.GivenDesignation = GivenDesignation;
        $scope.IntervieweeWiseDocList = [];

        $http({
            method: 'POST',
            url: 'Recruitments/predashboard/GetIntervieweeDocumentsSelfNU/',
            data: { 'EmpId': x.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.IntervieweeWiseDocList = response.data;
            angular.element(document.querySelector("#ModalSelfDocument")).modal('show');
        });
    }

    $scope.IntervieweeWiseDocDeptNU = function (x) {
        var empId = x.Id;
        var empName = x.FullName;
        var GivenDesignation = x.GivenDesignation;

        $scope.empId = empId;
        $scope.empName = empName;
        $scope.GivenDesignation = GivenDesignation;
        $scope.IntervieweeWiseDocDeptList = [];

        $http({
            method: 'POST',
            url: 'Recruitments/predashboard/GetIntervieweeDocumentsDeptNU/',
            data: { 'EmpId': x.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.IntervieweeWiseDocDeptList = response.data;
            angular.element(document.querySelector("#ModalDeptDocument")).modal('show');
        });
    }
    //---------------------------------------------------------//

    //-----------------Modals------------------//
    $scope.setClickedRowSTI = function (index) {
        $scope.selectedRow = index;
    }
    $scope.projectPlanningListParametersSTI = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'RowNum',
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ModalSelectedTotalInterviewee = function () {
        baseService.setCurrentPage('SelITList');
        $scope.SelITList = [];
        $scope.GetProjectPlanningListDataSTI = function (pageno) {
            baseService.paginationBase('Recruitments/predashboard/GetListSelTotalInterviewee', pageno, $scope.projectPlanningListParametersSTI)
                .then(function (data) {
                    $scope.SelITList = data.Rows;
                    $scope.projectPlanningListParametersSTI.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetProjectPlanningListDataSTI();
        angular.element(document.querySelector('#ModalSelTotalInterviewee')).modal('show');
        $scope.TblDocumentUploadingStatus("Selected");
    }
    $scope.setClickedRowMNS = function (index) {
        $scope.selectedRow = index;
    }
    $scope.projectPlanningListParametersMNS = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'RowNum',
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ModalNotSelected = function () {
        baseService.setCurrentPage('NSelITList');
        $scope.NSelITList = [];
        $scope.GetProjectPlanningListDataMNS = function (pageno) {
            baseService.paginationBase('Recruitments/predashboard/GetListNotSelectedEmp', pageno, $scope.projectPlanningListParametersMNS)
                .then(function (data) {
                    $scope.NSelITList = data.Rows;
                    $scope.projectPlanningListParametersMNS.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetProjectPlanningListDataMNS();
        angular.element(document.querySelector('#ModalNotSelected')).modal('show');
    };
    $scope.setClickedRowSBNC = function (index) {
        $scope.selectedRow = index;
    }
    $scope.projectPlanningListParametersSBNC = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'RowNum',
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    
    $scope.ModalSubmittedButNotConfirmed = function () {
        baseService.setCurrentPage('NotConList');
        $scope.NotConList = [];
        $scope.GetProjectPlanningListDataSBNC = function (pageno) {
            baseService.paginationBase('Recruitments/predashboard/SubmittedButNotConfirmed', pageno, $scope.projectPlanningListParametersSBNC)
                .then(function (data) {
                    $scope.NotConList = data.Rows;
                    $scope.projectPlanningListParametersSBNC.total_count = data.Total;
                    }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetProjectPlanningListDataSBNC();
        angular.element(document.querySelector('#ModalSubmittedButNotConfirmed')).modal('show');
        $scope.TblDocumentUploadingStatus("NotConfirmed");
    }
    $scope.setClickedRowODT = function (index) {
        $scope.selectedRow = index;
    }
    $scope.projectPlanningListParametersODT = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'RowNum',
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ModalOverDueTotalInterviewee = function () {
        baseService.setCurrentPage('ODList');
        $scope.ODList = [];
        $scope.GetProjectPlanningListDataODT = function (pageno) {
            baseService.paginationBase('Recruitments/predashboard/GetListOverDueTotalInterviewee', pageno, $scope.projectPlanningListParametersODT)
                .then(function (data) {
                    $scope.ODList = data.Rows;
                    $scope.projectPlanningListParametersODT.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetProjectPlanningListDataODT();
        angular.element(document.querySelector('#ModalOverDueTotalInterviewee')).modal('show');    
        $scope.TblDocumentUploadingStatus("TotalOverDue");
    }
    $scope.setClickedRowLI = function (index) {
        $scope.selectedRow = index;
    }
    $scope.projectPlanningListParametersLI = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'RowNum',
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ModalLoggedInInterviewee = function () {
        baseService.setCurrentPage('LIList');
        $scope.LIList = [];    
        $scope.GetProjectPlanningListDataLI = function (pageno) {
            baseService.paginationBase('Recruitments/predashboard/GetListLoggedInInterviewee', pageno, $scope.projectPlanningListParametersLI)
                .then(function (data) {
                    $scope.LIList = data.Rows;
                    $scope.projectPlanningListParametersLI.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetProjectPlanningListDataLI();
        angular.element(document.querySelector('#ModalLoggedInInterviewee')).modal('show');
        $scope.TblDocumentUploadingStatus("LoggedIn");
    }
    $scope.setClickedRowODLI = function (index) {
        $scope.selectedRow = index;
    }
    $scope.projectPlanningListParametersODLI = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'RowNum',
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ModalODLoggedInInterviewee = function () {
        baseService.setCurrentPage('ODLIList');
        $scope.ODLIList = [];
       
        $scope.GetProjectPlanningListDataODLI = function (pageno) {
            baseService.paginationBase('Recruitments/predashboard/GetListODLoggedInInterviewee', pageno, $scope.projectPlanningListParametersODLI)
                .then(function (data) {
                    $scope.ODLIList = data.Rows;
                    $scope.projectPlanningListParametersODLI.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetProjectPlanningListDataODLI();
        angular.element(document.querySelector('#ModalODLoggedInInterviewee')).modal('show');     
        $scope.TblDocumentUploadingStatus("LoggedInOverDue");
    }
    $scope.setClickedRowNLI = function (index) {
        $scope.selectedRow = index;
    }
    $scope.projectPlanningListParametersNLI = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'RowNum',
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ModalNotLoggedInInterviewee = function () {
        baseService.setCurrentPage('NLIList');
        $scope.NLIList = [];

        $scope.GetProjectPlanningListDataNLI = function (pageno) {
            baseService.paginationBase('Recruitments/predashboard/GetListNotLoggedInInterviewee', pageno, $scope.projectPlanningListParametersNLI)
                .then(function (data) {
                    $scope.NLIList = data.Rows;
                    $scope.projectPlanningListParametersNLI.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetProjectPlanningListDataNLI();
        angular.element(document.querySelector('#ModalNotLoggedInInterviewee')).modal('show');   
        $scope.TblDocumentUploadingStatus("NotLoggedIn");
    }
    $scope.setClickedRowODNLI = function (index) {
        $scope.selectedRow = index;
    }
    $scope.projectPlanningListParametersODNLI = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'RowNum',
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ModalODNotLoggedInInterviewee = function () {
        baseService.setCurrentPage('ODNLIList');
        $scope.ODNLIList = [];

        $scope.GetProjectPlanningListDataODNLI = function (pageno) {
            baseService.paginationBase('Recruitments/predashboard/GetListODNotLoggedInInterviewee', pageno, $scope.projectPlanningListParametersODNLI)
                .then(function (data) {
                    $scope.ODNLIList = data.Rows;
                    $scope.projectPlanningListParametersODNLI.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetProjectPlanningListDataODNLI();
       angular.element(document.querySelector('#ModalODNotLoggedInInterviewee')).modal('show');
       $scope.TblDocumentUploadingStatus("NotLoggedInOverDue");
    }

    $scope.TblDocumentUploadingStatus = function (status) {
        $scope.docStatList = [];

        $http({
            method: 'POST',
            url: 'Recruitments/predashboard/GetDocumentUploadingStatus/',
            data: { 'status': status },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.docStatList = response.data;

        });
    }
};
