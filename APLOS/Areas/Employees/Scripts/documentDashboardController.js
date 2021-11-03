'use strict';
documentDashboardController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$cookies'];
function documentDashboardController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $cookies) {
    var docSChart;
    var docbarChart;
    $scope.list = [];
    $scope.overdue = 0;
    $scope.pendinglist = [];
    $scope.dueStatusList = [];
    $scope.pieChartData = [];
    var overDueChart;
    $scope.pieOverDue = 0;
    $scope.pieDue = 0;
    $scope.pieCompleted = 0;
    $scope.dataGrid = "";
    $scope.segmentEmp = "";
    var x = document.getElementById("myDIV");

    var y = document.getElementById("MainDiv");
    var z = document.getElementById("inDiv");
    x.style.display = "none";
    y.style.display = "block";
    z.style.display = "none";
    $scope.docEmployeeCategoryList = [];

    $scope.clickdde = function () {
        if (x.style.display === "none"|| z.style.display==="none") {
            y.style.display = "none";
            x.style.display = "block";
            z.style.display = "none";
            $scope.documentDataList = [];
            if ($rootScope.isCollapsed == true) {
                $rootScope.toggle();
            }
        }
    };
    $scope.clickdde2 = function () {
        if (y.style.display === "none" || z.style.display == "none") {
          
            y.style.display = "block";
            x.style.display = "none";
            z.style.display = "none";
            
        }
    };
    $scope.clickdde3 = function () {
        if (x.style.display === "none" || y.style.display == "none") {
            $scope.documentDataList = [];
            y.style.display = "none";
            x.style.display = "none";
            z.style.display = "block";
            if ($rootScope.isCollapsed == true) {
                $rootScope.toggle();
            }
        }
    };
    
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.docByModel = {
        DocumentCategoryId: null,
        DocumentSubCategoryId: null,
        ComplianceDocumentId: null,
        EmplyeeTypeOrCategoryId: null,
        DocumentationBy: null,
        ResponsiblePersonId: null,
        Importance: null,
        OptionalOrMandatory: null,
        DocumentType: null
    };

    var submittedChart;

    cboService.getResponsiblePersonCbo(null, function (result) {
        $scope.ResopnsiblePersontList = result;
    });
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.docEmployeeCategoryList = result;
    });
    cboService.getEnumCbo('enum/GetDocumentationByCbo', function (result) {
        $scope.documentationByList = result;
    });
    cboService.getEnumCbo('Enum/GetEnumForDocumentType/', function (result) {
        $scope.documentTypeList = result;
    });
    cboService.getEnumCbo('Enum/GetEnumForImportance/', function (result) {
        $scope.importanceList = result;
    });
    cboService.getEnumCbo('Enum/GetEnumEmploymentStage/', function (result) {
        $scope.employmentStageList = result;
    });
    cboService.getEnumCbo('Enum/GetEnumPostRecruitment/', function (result) {
        $scope.postRecruitmentList = result;
    });
    cboService.getEnumCbo('Enum/GetEnumDependateDate/', function (result) {
        $scope.dependateDateList = result;
    });

    cboService.getCboDocumnetCategoryList(function (result) {

        $scope.complianceDocumentCategoryList = result;


    });
    $scope.docByModelClear = function () {
        $scope.docByModel.Importance = null;
        $scope.docByModel.OptionalOrMandatory = null;
        $scope.docByModel.DocumentationBy = null;
        $scope.docByModel.DocumentType = null;
        $scope.docByModel.DocumentSubCategoryId = null;
    };
    $scope.getDocumentSubCategory = function () {
        cboService.getCboCascadingComplianceDocumentSubCategory($scope.docByModel.DocumentCategoryId, function (result) {
            $scope.complianceDocumentSubCategoryList = result;
        });
    };
    $scope.getDocumentSubCategory();

    $scope.getComplianceDocument = function () {
        cboService.getCboComplianceDocumnetList($scope.docByModel.DocumentCategoryId, $scope.docByModel.DocumentSubCategoryId, function (result) {
            $scope.ComplianceDocumentList = result;
        });
    };
    $scope.getComplianceDocument();

    $scope.GetComplianceDocumentDetail = function (complianceDocumentId) {
        $scope.docByModel.Importance = null;
        $scope.docByModel.OptionalOrMandatory = null;
        $scope.docByModel.DocumentationBy = null;
        $scope.docByModel.DocumentType = null;
        $scope.docByModel.DocumentCategoryId = null;
        $scope.docByModel.DocumentSubCategoryId = null;

        $http({
            method: 'GET',
            url: 'DocumentDashboard/GetComplianceDocumentDetail',
            params: {
                'complianceDocumentId': complianceDocumentId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.docList = response.data;

            $scope.docByModel.Importance = $scope.docList[0].Importance;
            $scope.docByModel.OptionalOrMandatory = $scope.docList[0].OptionalOrMandatory;
            $scope.docByModel.DocumentationBy = $scope.docList[0].DocumentationBy;
            $scope.docByModel.DocumentType = $scope.docList[0].DocumentType;
            $scope.docByModel.DocumentCategoryId = $scope.docList[0].ComplianceDocCategory;
            $scope.docByModel.DocumentSubCategoryId = $scope.docList[0].ComplianceDocumentSubCategory;
        });
    };



    $scope.getProfileTypeList = [];
    cboService.getEnumCbo("Enum/GetProfileTypeEnumCbo", function (result) {
        $scope.getProfileTypeList = result;
    });
    cboService.getEnumCbo("Enum/getcompliancedocumentcategoryenumcbo", function (result) {
        $scope.typeList = result;
    });
    cboService.getEnumCbo("Enum/getdurationuomenumcbo", function (result) {
        $scope.durationUOMList = result;
    });

    $scope.qualificationLabelList = [];
    cboService.getCboQualificationLevel(function (result) {
        $scope.qualificationLabelList = result;
    });

    $scope.DailyOverDueStatus = function () {
        $scope.totalOverdueMandt = [];
        $scope.totalOverdueOpt = [];
        $scope.totalOverdueCompleted = [];
        $scope.dueDate = [];

        $http({
            method: 'GET',
            url: 'DocumentDashboard/DailyOverDueStatus',
            params: {
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId, 
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DailyOverDueStatusList = response.data;

            angular.forEach($scope.DailyOverDueStatusList, function (item, i) {
                $scope.totalOverdueMandt.push(item.TotalOverdueMandt);
                $scope.totalOverdueOpt.push(item.TotalOverdueOpt);
                $scope.totalOverdueCompleted.push(item.Completed);
                $scope.dueDate.push(item.SDueDate);
            });
            var overDuectx = document.getElementById("overDueChart").getContext('2d');
            if (overDueChart !== undefined && typeof overDueChart === 'object' && typeof overDueChart.destroy === 'function') overDueChart.destroy();
            overDueChart = new Chart(overDuectx, {
                type: 'line',
                data: {
                    labels: $scope.dueDate,
                    datasets: [{
                        label: 'Mandatory',
                        data: $scope.totalOverdueMandt,
                        backgroundColor: 'rgba(255, 99, 132, 0.7)',
                        borderColor: 'rgba(255,99,132,1)',
                        fill: false,
                        borderWidth: 2
                    },
                    {
                        label: 'Optional',
                        data: $scope.totalOverdueOpt,
                        backgroundColor: 'rgba(241, 196, 15, 0.7)',
                        borderColor: 'rgba(241, 196, 15, 1)',
                        fill: false,
                        borderWidth: 2
                    }
                    ]
                },
                options: {
                    legend: {
                        display: true,
                        position: 'top'
                    },
                    title: {
                        display: true,
                        text: 'Daily Documents Overdue Status',
                        position: 'bottom'
                    },
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: true,
                                onClick: (e) => e.stopPropagation()
                            }
                        }]
                    },
                    elements: {
                        line: {
                            tension: 0,
                            borderCapStyle: 'butt'
                        }
                    }
                }
            });
        });
    };
    $scope.DailyOverDueStatus();

    $scope.pieTotal = 0;
    $scope.PendingDocumentsStatusList = [];

    $scope.PendingDocumentStatus = function () {
        $http({
            method: 'GET',
            url: 'DocumentDashboard/PendingDocuments',
            params: {
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PendingDocumentsStatusList = response.data;
            //createDocChart($scope.PendingDocumentsStatusList);
        });
    };
    $scope.PendingDocumentStatus();

    $scope.totalOverdueMandt = [];
    $scope.totalOverdueOpt = [];
    $scope.totalOverdueCompleted = [];
    $scope.dueDate = [];
    function createOverdueChart(list) {
        $scope.list = list;
        angular.forEach(list, function (item, i) {
            $scope.totalOverdueMandt.push(item.TotalOverdueMandt);
            $scope.totalOverdueOpt.push(item.TotalOverdueOpt);
            $scope.totalOverdueCompleted.push(item.Completed);
            $scope.dueDate.push(item.DueDate);
        });
        var overDuectx = document.getElementById("overDueChart").getContext('2d');
        var overDueChart = new Chart(overDuectx, {
            type: 'line',
            data: {
                labels: $scope.dueDate,
                datasets: [{
                    label: 'Mandatory',
                    data: $scope.totalOverdueMandt,
                    backgroundColor: 'rgba(255, 99, 132, 0.7)',
                    borderColor: 'rgba(255,99,132,1)',
                    fill: false,
                    borderWidth: 2
                },
                {
                    label: 'Optional',
                    data: $scope.totalOverdueOpt,
                    backgroundColor: 'rgba(241, 196, 15, 0.7)',
                    borderColor: 'rgba(241, 196, 15, 1)',
                    fill: false,
                    borderWidth: 2
                }
                ]
            },
            options: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    text: 'Daily Documents Overdue Status',
                    position: 'bottom'
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                            onClick: (e) => e.stopPropagation()
                        }
                    }]
                },
                elements: {
                    line: {
                        tension: 0
                    }
                }
            }
        });
    }

    //Bar chart//

    function setSDocList(list) {
        $scope.docChartLabel = [];
        $scope.docChartList = [];
        $scope.ttDocName = [];

        var totalDoc = 0;
        var label = '';

        $scope.list = list;
        angular.forEach(list, function (item, i) {
            totalDoc = item.totDoc;
            label = item.docName;

            $scope.docChartLabel.push(item.docName);
            $scope.docChartList.push(item.totDoc);
            $scope.ttDocName.push(item.fullDocName);
        });
    }

    function setNSDocList(list) {
        $scope.docChartLabelNS = [];
        $scope.docChartListNS = [];

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

    function createDocBarChart(list) {
        $scope.due = [];
        $scope.overDue = [];
        $scope.docname = [];
        $scope.docShortName = [];
        $scope.list = [];
        $scope.list = list;
        angular.forEach(list, function (item, i) {
            $scope.due.push(item.TotalDueDocument);
            $scope.overDue.push(item.TotalOverDueDocument);
            $scope.docname.push(item.ComplianceDocumentUserName);
            $scope.docShortName.push(item.ComplianceDocumentShortName);
        });
        Chart.defaults.global.legend.display = false;
        var DocCtx = document.getElementById("docbarChart").getContext('2d');
        if (docbarChart !== undefined && typeof docbarChart === 'object' && typeof docbarChart.destroy === 'function') docbarChart.destroy();
        docbarChart = new Chart(DocCtx, {
            type: 'bar',
            data: {
                labels: $scope.docShortName,
                datasets: [{
                    label: 'Due',
                    backgroundColor: window.chartColors.yellow,
                    borderColor: window.chartColors.yellow,
                    data: $scope.due
                },
                {
                    label: 'OverDue',
                    backgroundColor: window.chartColors.red,
                    borderColor: window.chartColors.red,
                    data: $scope.overDue
                }
                ]
            },
            options: {
                responsive: true,

                maxBarThickness: 20,
                title: {
                    display: true,
                    text: 'Documents Due Status',
                    position: 'bottom'
                },
                tooltips: {
                    mode: 'index',
                    intersect: true,

                    callbacks: {
                        title: function (tooltipItem, data) {
                            return $scope.docname[tooltipItem[0].index];
                        }
                    }
                },
                legend: {
                    display: true,
                    labels: {
                        border: 0
                    }
                },
                hover: {
                    mode: 'nearest',
                    intersect: true
                },
                scales: {
                    yAxes: [{
                        stacked: true,
                        ticks: {
                            beginAtZero: true
                        }
                    }],
                    xAxes: [{
                        stacked: true,
                        maxBarThickness: 40,
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 90,
                            minRotation: 90
                        }
                    }]
                }
            }
        });
    }

    $scope.predoc1 = '';
    $scope.predoc2 = '';
    $scope.predoc3 = '';
    $scope.preemp1 = '';
    $scope.preemp2 = '';
    $scope.preemp3 = '';

    $scope.GetOverDue = function () {
        $scope.OverDuelist = [];
        $scope.overdue = 0;
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/OverDueStatus',
            params: {
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId ': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OverDuelist = response.data;
            angular.forEach(response.data, function (item, i) {
                $scope.overdue = item.OverAllemp;
            });
        });
    };
    $scope.GetOverDue();
    //---------------------------------------------Modal----------------------------------------//
    $scope.PreEmployeeParameters3 = {
        limit: 20,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode',
        searchBy: "EmployeeName",
        pageSize: 20,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.searchByList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Plant',
            'value': 'plant'
        }
    ];

    $scope.clearSearchList = function () {
        $scope.PreEmployeeParameters3.search = null;
    };

    $scope.GetModalPreEmployee = function (employmentstage) {
        baseService.setCurrentPage('EmployeeList');
        $scope.EmployeeList = [];
        $scope.employmentStage = employmentstage;
        $scope.GetPreEmployeeData3 = function (pageno) {
            $scope.PreEmployeeParameters3.employmentstage = employmentstage;
            $scope.PreEmployeeParameters3.DocumentCategoryId = $scope.docByModel.DocumentCategoryId;
            $scope.PreEmployeeParameters3.DocumentSubCategoryId = $scope.docByModel.DocumentSubCategoryId;
            $scope.PreEmployeeParameters3.ComplianceDocumentId = $scope.docByModel.ComplianceDocumentId;
            $scope.PreEmployeeParameters3.DocumentationBy = $scope.docByModel.DocumentationBy;
            $scope.PreEmployeeParameters3.ResponsiblePersonId = $scope.docByModel.ResponsiblePersonId;
            $scope.PreEmployeeParameters3.Importance = $scope.docByModel.Importance;
            $scope.PreEmployeeParameters3.OptionalOrMandatory = $scope.docByModel.OptionalOrMandatory;
            $scope.PreEmployeeParameters3.DocumentType = $scope.docByModel.DocumentType;
            $scope.PreEmployeeParameters3.EmplyeeTypeOrCategoryId = $scope.docByModel.EmplyeeTypeOrCategoryId;

            baseService.paginationBase('Employees/DocumentDashboard/PreEmp', pageno, $scope.PreEmployeeParameters3)
                .then(function (data) {
                    $scope.EmployeeList = data.Rows;
                    $scope.PreEmployeeParameters3.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetPreEmployeeData3();
        angular.element(document.querySelector('#EmpModal')).modal('show');
    };

    $scope.GetModalPreEmployee1 = function (employmentstage) {
        baseService.setCurrentPage('EmployeeList');
        $scope.EmployeeList = [];
        $scope.employmentStage = employmentstage;
        $scope.GetPreEmployeeData3 = function (pageno) {
            $scope.PreEmployeeParameters3.employmentstage = employmentstage;
            $scope.PreEmployeeParameters3.DocumentCategoryId = $scope.docByModel.DocumentCategoryId;
            $scope.PreEmployeeParameters3.DocumentSubCategoryId = $scope.docByModel.DocumentSubCategoryId;
            $scope.PreEmployeeParameters3.ComplianceDocumentId = $scope.docByModel.ComplianceDocumentId;
            $scope.PreEmployeeParameters3.DocumentationBy = $scope.docByModel.DocumentationBy;
            $scope.PreEmployeeParameters3.ResponsiblePersonId = $scope.docByModel.ResponsiblePersonId;
            $scope.PreEmployeeParameters3.Importance = $scope.docByModel.Importance;
            $scope.PreEmployeeParameters3.OptionalOrMandatory = $scope.docByModel.OptionalOrMandatory;
            $scope.PreEmployeeParameters3.DocumentType = $scope.docByModel.DocumentType;
            $scope.PreEmployeeParameters3.EmplyeeTypeOrCategoryId = $scope.docByModel.EmplyeeTypeOrCategoryId;
            baseService.paginationBase('Employees/DocumentDashboard/PreEmp1', pageno, $scope.PreEmployeeParameters3)
                .then(function (data) {
                    $scope.EmployeeList = data.Rows;
                    $scope.PreEmployeeParameters3.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetPreEmployeeData3();
        angular.element(document.querySelector('#EmpModal')).modal('show');
    }

    $scope.GetModalPreEmployee2 = function (employmentstage) {
        baseService.setCurrentPage('EmployeeList');
        $scope.EmployeeList = [];
        $scope.employmentStage = employmentstage;
        $scope.GetPreEmployeeData3 = function (pageno) {
            $scope.PreEmployeeParameters3.employmentstage = employmentstage;
            $scope.PreEmployeeParameters3.DocumentCategoryId = $scope.docByModel.DocumentCategoryId;
            $scope.PreEmployeeParameters3.DocumentSubCategoryId = $scope.docByModel.DocumentSubCategoryId;
            $scope.PreEmployeeParameters3.ComplianceDocumentId = $scope.docByModel.ComplianceDocumentId;
            $scope.PreEmployeeParameters3.DocumentationBy = $scope.docByModel.DocumentationBy;
            $scope.PreEmployeeParameters3.ResponsiblePersonId = $scope.docByModel.ResponsiblePersonId;
            $scope.PreEmployeeParameters3.Importance = $scope.docByModel.Importance;
            $scope.PreEmployeeParameters3.OptionalOrMandatory = $scope.docByModel.OptionalOrMandatory;
            $scope.PreEmployeeParameters3.DocumentType = $scope.docByModel.DocumentType;
            $scope.PreEmployeeParameters3.EmplyeeTypeOrCategoryId = $scope.docByModel.EmplyeeTypeOrCategoryId;
            baseService.paginationBase('Employees/DocumentDashboard/PreEmp2', pageno, $scope.PreEmployeeParameters3)
                .then(function (data) {
                    $scope.EmployeeList = data.Rows;
                    $scope.PreEmployeeParameters3.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetPreEmployeeData3();
        angular.element(document.querySelector('#EmpModal')).modal('show');
    }
    $scope.GetModalPreEmployee3 = function (employmentstage) {
        baseService.setCurrentPage('EmployeeList');
        $scope.EmployeeList = [];
        $scope.employmentStage = employmentstage;
        $scope.GetPreEmployeeData3 = function (pageno) {
            $scope.PreEmployeeParameters3.employmentstage = employmentstage;
            $scope.PreEmployeeParameters3.DocumentCategoryId = $scope.docByModel.DocumentCategoryId;
            $scope.PreEmployeeParameters3.DocumentSubCategoryId = $scope.docByModel.DocumentSubCategoryId;
            $scope.PreEmployeeParameters3.ComplianceDocumentId = $scope.docByModel.ComplianceDocumentId;
            $scope.PreEmployeeParameters3.DocumentationBy = $scope.docByModel.DocumentationBy;
            $scope.PreEmployeeParameters3.ResponsiblePersonId = $scope.docByModel.ResponsiblePersonId;
            $scope.PreEmployeeParameters3.Importance = $scope.docByModel.Importance;
            $scope.PreEmployeeParameters3.OptionalOrMandatory = $scope.docByModel.OptionalOrMandatory;
            $scope.PreEmployeeParameters3.DocumentType = $scope.docByModel.DocumentType;
            $scope.PreEmployeeParameters3.EmplyeeTypeOrCategoryId = $scope.docByModel.EmplyeeTypeOrCategoryId;
            baseService.paginationBase('Employees/DocumentDashboard/PreEmp3', pageno, $scope.PreEmployeeParameters3)
                .then(function (data) {
                    $scope.EmployeeList = data.Rows;
                    $scope.PreEmployeeParameters3.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetPreEmployeeData3();
        angular.element(document.querySelector('#EmpModal')).modal('show');
    };
    
    $scope.GetModalDoc = function (employmentstage) {
        $scope.DocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/Doc',
            params: {
                'employmentstage': employmentstage,
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocList = response.data;
            //createXLfilters($scope.DocList, ['ComplianceDocument', 'DocCatg', 'DocSubCatg', 'DocumentType', 'DocumentationBy', 'Importance','OptionalOrMandatory']);
            $scope.elasticFilter($scope.DocList);
            angular.element(document.querySelector('#DocModal')).modal('show');
        });
    }

    $scope.GetModalDoc1 = function (employmentstage) {
        $scope.DocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/Doc1',
            params: {
                'employmentstage': employmentstage,
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocList = response.data;
            // createXLfilters($scope.DocList, ['ComplianceDocument', 'DocCatg', 'DocSubCatg', 'DocumentType', 'DocumentationBy', 'Importance', 'OptionalOrMandatory']);
            $scope.elasticFilter($scope.DocList);
            angular.element(document.querySelector('#DocModal')).modal('show');
        });
    }

    $scope.GetModalDoc2 = function (employmentstage) {
        $scope.DocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/Doc2',
            params: {
                'employmentstage': employmentstage,
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocList = response.data;
            $scope.elasticFilter($scope.DocList);
            //createXLfilters($scope.DocList, ['ComplianceDocument', 'DocCatg', 'DocSubCatg', 'DocumentType', 'DocumentationBy', 'Importance', 'OptionalOrMandatory']);
            angular.element(document.querySelector('#DocModal')).modal('show');
        });
    }

    $scope.GetModalDoc3 = function (employmentstage) {
        $scope.DocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/Doc3',
            params: {
                'employmentstage': employmentstage,
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocList = response.data;
            $scope.elasticFilter($scope.DocList);
            // createXLfilters($scope.DocList, ['ComplianceDocument', 'DocCatg', 'DocSubCatg', 'DocumentType', 'DocumentationBy', 'Importance', 'OptionalOrMandatory']);
            angular.element(document.querySelector('#DocModal')).modal('show');
        });
    };

    $scope.GetModalOthersDoc = function () {
        $scope.DocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/PieOthersDoc',
            params: {
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocList = response.data;
            $scope.propertyName = '';
            $scope.reverse = true;
            $scope.sortBy = function (propertyName) {
                $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
                $scope.propertyName = propertyName;
            };
            angular.element(document.querySelector('#OthersDocModal')).modal('show');
        });
    };

    $scope.GetModalDueDoc = function () {
        $scope.DocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/PieDueDoc',
            params: {
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocList = response.data;
            $scope.propertyName = '';
            $scope.reverse = true;
            $scope.sortBy = function (propertyName) {
                $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
                $scope.propertyName = propertyName;
            };
            angular.element(document.querySelector('#DueDocModal')).modal('show');
        });
    };

    $scope.GetModalOverDueDoc = function () {
        $scope.DocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/PieOverDueDoc',
            params: {
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'EmplyeeTypeOrCategoryId': $scope.docByModel.EmplyeeTypeOrCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocList = response.data;
            $scope.propertyName = '';
            $scope.reverse = true;
            $scope.sortBy = function (propertyName) {
                $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
                $scope.propertyName = propertyName;
            };

            angular.element(document.querySelector('#OverDocModal')).modal('show');
        });
    };

    $scope.GetEmpWiseDocOpt = function (employmentstage, segment, preRecruitementEmpId, EmployeeId) {
        $scope.EmpWiseDocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/EmpWiseDocOpt',
            params: {
                'segment': segment,
                'employmentStage': employmentstage,
                'preRecEmployeeId': preRecruitementEmpId,
                'employeeId': EmployeeId,
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmpWiseDocList = response.data;
            $scope.propertyName = '';
            $scope.reverse = true;
            $scope.sortBy = function (propertyName) {
                $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
                $scope.propertyName = propertyName;
            };
            angular.element(document.querySelector('#EmpWiseDocModalOpt')).modal('show');
        });
    };

    $scope.GetEmpWiseDocMandt = function (employmentstage, segment, preRecruitementEmpId, EmployeeId) {
        $scope.EmpWiseDocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/EmpWiseDocMandt',
            params: {
                'segment': segment,
                'employmentStage': employmentstage,
                'preRecEmployeeId': preRecruitementEmpId,
                'employeeId': EmployeeId,
                'DocumentCategoryId': $scope.docByModel.DocumentCategoryId,
                'DocumentSubCategoryId': $scope.docByModel.DocumentSubCategoryId,
                'ComplianceDocumentId': $scope.docByModel.ComplianceDocumentId,
                'DocumentationBy': $scope.docByModel.DocumentationBy,
                'ResponsiblePersonId': $scope.docByModel.ResponsiblePersonId,
                'Importance': $scope.docByModel.Importance,
                'OptionalOrMandatory': $scope.docByModel.OptionalOrMandatory,
                'DocumentType': $scope.docByModel.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmpWiseDocList = response.data;
            $scope.propertyName = '';
            $scope.reverse = true;
            $scope.sortBy = function (propertyName) {
                $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
                $scope.propertyName = propertyName;
            };
            angular.element(document.querySelector('#EmpWiseDocModalMandt')).modal('show');
        });
    };

    $scope.DocWiseEmpParameters = {
        limit: 20,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeId',
        searchBy: "EmployeeName",
        pageSize: 20,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetDocWiseEmp = function (employmentstage, segment, complianceDocId) {
        $scope.DocWiseEmpList = [];
        baseService.setCurrentPage('DocWiseEmpList');

        $scope.GetDocWiseEmpData = function (pageno) {
            $scope.DocWiseEmpParameters.segment = segment;
            $scope.DocWiseEmpParameters.employmentstage = employmentstage;
            $scope.DocWiseEmpParameters.CompDocumentId = complianceDocId;
            $scope.DocWiseEmpParameters.DocumentCategoryId = $scope.docByModel.DocumentCategoryId;
            $scope.DocWiseEmpParameters.EmplyeeTypeOrCategoryId = $scope.docByModel.EmplyeeTypeOrCategoryId;
            $scope.DocWiseEmpParameters.DocumentSubCategoryId = $scope.docByModel.DocumentSubCategoryId;
            $scope.DocWiseEmpParameters.ComplianceDocumentId = $scope.docByModel.ComplianceDocumentId;
            $scope.DocWiseEmpParameters.DocumentationBy = $scope.docByModel.DocumentationBy;
            $scope.DocWiseEmpParameters.ResponsiblePersonId = $scope.docByModel.ResponsiblePersonId;
            $scope.DocWiseEmpParameters.Importance = $scope.docByModel.Importance;
            $scope.DocWiseEmpParameters.OptionalOrMandatory = $scope.docByModel.OptionalOrMandatory;
            $scope.DocWiseEmpParameters.DocumentType = $scope.docByModel.DocumentType;
            baseService.paginationBase('Employees/DocumentDashboard/DocWiseEmp', pageno, $scope.DocWiseEmpParameters)
                .then(function (data) {
                    $scope.DocWiseEmpList = data.Rows;
                    $scope.DocWiseEmpParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetDocWiseEmpData();
        angular.element(document.querySelector('#DocWiseEmpModal')).modal('show');
    }

    $scope.GetModalOthersDocWiseEmp = function (complianceDocumentId) {
        $scope.DocWiseEmpList = [];
        baseService.setCurrentPage('DocWiseEmpList');

        $scope.GetDocWiseEmpData = function (pageno) {
            $scope.DocWiseEmpParameters.CompDocumentId = complianceDocumentId;
            $scope.DocWiseEmpParameters.DocumentCategoryId = $scope.docByModel.DocumentCategoryId;
            $scope.DocWiseEmpParameters.DocumentSubCategoryId = $scope.docByModel.DocumentSubCategoryId;
            $scope.DocWiseEmpParameters.EmplyeeTypeOrCategoryId = $scope.docByModel.EmplyeeTypeOrCategoryId;
            $scope.DocWiseEmpParameters.ComplianceDocumentId = $scope.docByModel.ComplianceDocumentId;
            $scope.DocWiseEmpParameters.DocumentationBy = $scope.docByModel.DocumentationBy;
            $scope.DocWiseEmpParameters.ResponsiblePersonId = $scope.docByModel.ResponsiblePersonId;
            $scope.DocWiseEmpParameters.Importance = $scope.docByModel.Importance;
            $scope.DocWiseEmpParameters.OptionalOrMandatory = $scope.docByModel.OptionalOrMandatory;
            $scope.DocWiseEmpParameters.DocumentType = $scope.docByModel.DocumentType;
            baseService.paginationBase('Employees/DocumentDashboard/OthersDocWiseEmp', pageno, $scope.DocWiseEmpParameters)
                .then(function (data) {
                    $scope.DocWiseEmpList = data.Rows;
                    $scope.DocWiseEmpParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetDocWiseEmpData();
        angular.element(document.querySelector('#DocWiseEmpModal')).modal('show');
    }

    $scope.GetModalDueDocWiseEmp = function (complianceDocumentId) {
        $scope.DocWiseEmpList = [];
        baseService.setCurrentPage('DocWiseEmpList');

        $scope.GetDocWiseEmpData = function (pageno) {
            $scope.DocWiseEmpParameters.CompDocumentId = complianceDocumentId;
            $scope.DocWiseEmpParameters.DocumentCategoryId = $scope.docByModel.DocumentCategoryId;
            $scope.DocWiseEmpParameters.DocumentSubCategoryId = $scope.docByModel.DocumentSubCategoryId;
            $scope.DocWiseEmpParameters.ComplianceDocumentId = $scope.docByModel.ComplianceDocumentId;
            $scope.DocWiseEmpParameters.EmplyeeTypeOrCategoryId = $scope.docByModel.EmplyeeTypeOrCategoryId;
            $scope.DocWiseEmpParameters.DocumentationBy = $scope.docByModel.DocumentationBy;
            $scope.DocWiseEmpParameters.ResponsiblePersonId = $scope.docByModel.ResponsiblePersonId;
            $scope.DocWiseEmpParameters.Importance = $scope.docByModel.Importance;
            $scope.DocWiseEmpParameters.OptionalOrMandatory = $scope.docByModel.OptionalOrMandatory;
            $scope.DocWiseEmpParameters.DocumentType = $scope.docByModel.DocumentType;
            baseService.paginationBase('Employees/DocumentDashboard/DueWiseEmp', pageno, $scope.DocWiseEmpParameters)
                .then(function (data) {
                    $scope.DocWiseEmpList = data.Rows;
                    $scope.DocWiseEmpParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetDocWiseEmpData();
        angular.element(document.querySelector('#DocWiseEmpModal')).modal('show');
    };

    $scope.GetModalOverDueDocWiseEmp = function (complianceDocumentId) {
        $scope.DocWiseEmpList = [];
        baseService.setCurrentPage('DocWiseEmpList');

        $scope.GetDocWiseEmpData = function (pageno) {
            $scope.DocWiseEmpParameters.CompDocumentId = complianceDocumentId;
            $scope.DocWiseEmpParameters.DocumentCategoryId = $scope.docByModel.DocumentCategoryId;
            $scope.DocWiseEmpParameters.DocumentSubCategoryId = $scope.docByModel.DocumentSubCategoryId;
            $scope.DocWiseEmpParameters.ComplianceDocumentId = $scope.docByModel.ComplianceDocumentId;
            $scope.DocWiseEmpParameters.EmplyeeTypeOrCategoryId = $scope.docByModel.EmplyeeTypeOrCategoryId;
            $scope.DocWiseEmpParameters.DocumentationBy = $scope.docByModel.DocumentationBy;
            $scope.DocWiseEmpParameters.ResponsiblePersonId = $scope.docByModel.ResponsiblePersonId;
            $scope.DocWiseEmpParameters.Importance = $scope.docByModel.Importance;
            $scope.DocWiseEmpParameters.OptionalOrMandatory = $scope.docByModel.OptionalOrMandatory;
            $scope.DocWiseEmpParameters.DocumentType = $scope.docByModel.DocumentType;
            baseService.paginationBase('Employees/DocumentDashboard/OverDueWiseEmp', pageno, $scope.DocWiseEmpParameters)
                .then(function (data) {
                    $scope.DocWiseEmpList = data.Rows;
                    $scope.DocWiseEmpParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetDocWiseEmpData();
        angular.element(document.querySelector('#DocWiseEmpModal')).modal('show');
    };

    $scope.StatusName = ['Completed', 'Due', 'OverDue', 'No Due'];

    $scope.pieChartData = function (PieChartOverDue, PieChartDue, PieChartCompleted) {
        $scope.pieList = [];
        $scope.pieTotal = 0;
        
        $scope.pieTotal = PieChartOverDue + PieChartDue + PieChartCompleted;

        var submittedctx = document.getElementById("submittedChart").getContext('2d');

        if (submittedChart !== undefined && typeof submittedChart === 'object' && typeof submittedChart.destroy === 'function') submittedChart.destroy();
        submittedChart = new Chart(submittedctx, {
            type: 'doughnut',
            data: {
                labels: ['Completed', 'Due', 'OverDue'],
                datasets: [{
                    label: '',
                    data: [PieChartCompleted, PieChartDue, PieChartOverDue],
                    backgroundColor: [
                        'rgba(46, 204, 113,0.7)',
                        'rgba(241, 196, 15, 0.7)',
                        'rgba(231, 76, 60,0.7)',
                        'rgba(82, 179, 217, 0.7)'
                    ],
                    borderColor: [
                        'rgba(46, 204, 113,1.0)',
                        'rgba(241, 196, 15, 1.0)',
                        'rgba(231, 76, 60,1.0)',
                        'rgba(82, 179, 217, 1)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                legend: {
                    display: false,
                    position: 'bottom'
                    //onClick: (e) => e.stopPropagation()
                },
                title: {
                    display: true,
                    text: 'Overall Status',
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
                            var precentage = (((currentValue / total) * 100) + 0.0).toFixed(2);
                            return precentage + "%";
                        },
                        title: function (tooltipItem, data) {
                            return $scope.StatusName[tooltipItem[0].index];
                        }
                    }
                }
            }
        });
        //});
    };

    //$scope.pieChartData();

    $scope.getDocDashboardSync = function () {
        $scope.DocList = [];
        $http({
            method: 'GET',
            url: 'Employees/DocumentDashboard/DocDashboardSync',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.docByModel = {};
            $scope.GetOverDue();
            $scope.DailyOverDueStatus();
            $scope.OnloadDocumentListLoad();
            $scope.PendingDocumentStatus();            
            var gridObj = $("#docFilterGrid").data("ejGrid");
            gridObj.clearFiltering();

        });
    };
    //-----------------------------Excel Report-----------------------//
    $scope.employeeDocumentStatusReport = function () {
        location.href = 'Employees/DocumentDashboard/PreRecruitmentDocumentReport';
    };
    //-----------------------------Excel Report End-----------------------//

    //----------------------------Employee InforMation----------------------------------------//
    $rootScope.title = 'Employee Information';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.employees = [];
    $scope.path = 'employees/employeeinformation/';
    $scope.getListUrl = $scope.path + 'GetEmployeeListDocDashboard';
    baseService.init($scope.getListUrl, null, 10, null, 'EmployeeCode', 'EmployeeCode');

    $scope.getData = function (pageno) {
        $rootScope.parameters.plantId = $scope.employeeInformation.PlantId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.employees = [];
                $scope.employees = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    // $scope.getData();

    $scope.employeeInformation = {
        SystemId: null,
        EmployeeId: null,
        PreRecruitmentEmployeeId: null,
        EmployeeCode: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        UnitId: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        SubdivisionID: null,
        LineId: null,
        DesignationGroupId: null,
        DesignationSystemID: null,
        BudgetCode: null,
        PositionID: null,
        IsDirect: null,
        SalaryPercentage: null,
        CardNumber: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        EmployeeName: null,
        NickName: null,
        LocalEmployeeName: null,
        EmpPicPath: null,
        EmpType: null,
        EmploymentType: null,
        EmployeeGroupSystemID: null,
        JobLocationID: null,
        DOB: null,
        DOJ: null,
        DOCIsDay: null,
        DOCDay: null,
        DOCIsMonth: null,
        DOCMonth: null,
        DOC: null,
        DOS: null,
        IsConfirmed: null,
        ReActiveDate: null,
        EmployeeStatus: null,
        NationalID: null,
        TIN: null,
        CitizenID: null,
        FatherName: null,
        MotherName: null,
        ReligionID: null,
        CivilStatusID: null,
        employeeID: null,
        GenderID: null,
        SpouseName: null,
        SpouseNationalID: null,
        SpouseOccupation: null,
        NoOfChildren: null,
        PresentAddress1: null,
        PresentAddress2: null,
        ParmanentAddress1: null,
        ParmanentAddress2: null,
        PresThanaID: null,
        ParmThanaID: null,
        PresPostOfficeID: null,
        ParmPostOfficeID: null,
        PresZipCode: null,
        ParmZipCode: null,
        PresDistrictID: null,
        ParmDistrictID: null,
        PresCountryID: null,
        ParmCountryID: null,
        PresCityID: null,
        ParmCityID: null,
        PresAreaID: null,
        ParmAreaID: null,
        TelePhnNo: null,
        CellPhnNo: null,
        EmailId: null,
        BudgetCategoryID: null,
        EmployeeCategorySystemID: null,
        LVPolicyMasterSystemID: null,
        SalaryRuleMasterSystemID: null,
        BankSystemID: null,
        BankName: null,
        BankAccNo: null,
        BankAddedBy: null,
        BankDateAdded: null,
        BankUpdatedBy: null,
        BankDateUpdated: null,
        RegisterFP: null,
        RegisterProximate: null,
        SuperViser: null,
        IsSlvDevReg: null,
        IsAttdnProcBaseOnDeviceData: null,
        SubSecStrucSystemID: null,
        AddedBy: null,
        DateAdded: null,
        UpdatedBy: null,
        DateUpdated: null,
        EmrCntPer1Name: null,
        EmrCntPer1CellNo: null,
        EmrCntPer2Name: null,
        EmrCntPer2CellNo: null,
        GivenDesignationId: null,
        LegalDesignationId: null,
        AgreedDOJ: null,
        TotalSalary: null,
        SpecialReviewDuration: null,
        SpecialReviewAmount: null,
        Image: null
    };

    $rootScope.searchByList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Designation',
            'value': 'Designation'
        }
    ];

    //$scope.imageSrc = null;

    function setUserImage(data) {
        if (!baseService.isUndefinedOrNull(data.SystemId)) {
            $scope.imageSrc = $rootScope.HRMSImage + data.EmpPicPath;
            $scope.imageBtnDisable = true;
            $scope.employee.EmpPicPath = data.EmpPicPath;
        }
        else {
            $scope.imageBtnDisable = false;
            $scope.employee.EmpPicPath = null;
        }
    }
    $scope.filedata = null;
    $scope.picData = null;
    $("#uploadImage").change(function () {
        $scope.picData = this.files[0];
    });
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
    });


    $scope.empReferenceInformation = {
        SystemID: null,
        EmpSystemID: null,
        Ref1Name: null,
        Ref1EmployerName: null,
        Ref1EmployerAddress: null,
        Ref1Designation: null,
        Ref1CellPhnNo: null,
        Ref1TelePhnNo: null,
        Ref1Email: null,
        Ref1Address: null,
        Ref2Name: null,
        Ref2EmployerName: null,
        Ref2EmployerAddress: null,
        Ref2Designation: null,
        Ref2CellPhnNo: null,
        Ref2TelePhnNo: null,
        Ref2Email: null,
        Ref2Address: null
    };


    $scope.employeeDocument = {
        Id: null,
        EmpSystemID: null,
        FileId: null,
        FileName: null,
        ComplianceDocumentId: null,
        ComplianceDocumentSetId: null
    };


    $scope.VisibleDiv = function () {
        if ($scope.showdiv === true) {
            return true;
        }
        else {
            return false;
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.employeeInformation = $scope.employees[$scope.index];
        $scope.imageSrc = virtualPath.EmployeePic + $scope.employeeInformation.EmpPicPath;
        $rootScope.img = $scope.employeeInformation.EmpPicPath;
        $scope.user = $scope.employeeInformation.SystemId;
        $scope.CompanyGroupID = $scope.employeeInformation.GroupID;
        $scope.CompanyID = $scope.employeeInformation.CompanyId;
        $scope.CountryId = $scope.employeeInformation.CountryId;
        $scope.BudgetCode = $scope.employeeInformation.BudgetCode;
        $scope.PlantId = $scope.employeeInformation.PlantId;

        $scope.employeeInformation.DOB = $filter('dateFiltering')($scope.employeeInformation.DOB, 'dd-M-yyyy');
        $scope.employeeInformation.BirthdayCelebrationDate = $filter('dateFiltering')($scope.employeeInformation.BirthdayCelebrationDate, 'dd-M-yyyy');
        $scope.employeeInformation.DOJ = $filter('dateFiltering')($scope.employeeInformation.DOJ, 'dd-M-yyyy');
        $scope.employeeInformation.MarriagedayCelebrationDate = $filter('dateFiltering')($scope.employeeInformation.MarriagedayCelebrationDate, 'dd-M-yyyy');

        $scope.Tin = $scope.employeeInformation.TINCaption;
        if (baseService.isUndefinedOrNull($scope.Tin)) {
            $scope.Tin = "TIN";
        }
        $scope.Nid = $scope.employeeInformation.NIDCaption;
        if (baseService.isUndefinedOrNull($scope.Nid)) {
            $scope.Nid = "National ID";
        }
        $scope.NidLength = $scope.employeeInformation.NIDLength;
        $scope.TinLength = $scope.employeeInformation.TINLength;
        $scope.SalaryRangeForTax = $scope.employeeInformation.TINRequiredForSalaryAbove;
        $scope.SalaryRangeForTaxRequired = $scope.employeeInformation.IsTINRequiredForSalaryAbove;
        $scope.TotalSalary = $scope.employeeInformation.TotalSalary;
        $scope.NationalID = $scope.employeeInformation.NationalID;
        $scope.TIN = $scope.employeeInformation.TIN;
        $rootScope.PhoneLength = $scope.employeeInformation.PhoneLength;

        if (baseService.isUndefinedOrNull($scope.employeeInformation.Salutation)) {
            $scope.showdiv = false;
        }
        else {
            if ($scope.employeeInformation.Salutation.length > 0) {
                $scope.showdiv = true;
            }
            else {
                $scope.showdiv = false;
            }
        }
        // $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        //$scope.getSalutationList($scope.CompanyGroupID);
        $scope.Loaddocumentdatalist($scope.user);
        if (baseService.isUndefinedOrNull($scope.employeeInformation.EmpPicPath)) {
            $scope.imageSrc = null;
            if ($rootScope.GenderID === 'Male') {
                $scope.imageSrc = "empprofile/Images/male-alt.png";
            } else {
                $scope.imageSrc = "empprofile/Images/female-alt.png";
            }
        }
        //$scope.celebrationMarriage();
    };

    //function setUserImage(data) {
    //    if (!baseService.isUndefinedOrNull(data.SystemId)) {
    //        $scope.imageSrc = virtualPath.EmpPic + data.EmpPicPath;
    //        $scope.imageBtnDisable = true;
    //        $scope.employeeInformation.EmpPicPath = data.EmpPicPath;
    //    }
    //    else {
    //        $scope.imageBtnDisable = false;
    //        $scope.employeeInformation.EmpPicPath = null;
    //    }
    //};

    $scope.picdata = null;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    // #endregion

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeQualificationDocument + '/' + data.FileId + extention;
    };

    $scope.ExperienceFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeExperienceDocument + '/' + data.FileId + extention;
    };

    $scope.TrainingFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeTrainingDocument + '/' + data.FileId + extention;
    };

    $scope.fileId = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };

    $scope.getNum = function () {
        if ($scope.employeeInformation.IsKnownPerson)
            $scope.employeeInformation.NumberOfKnownPerson = 0;
        else
            $scope.employeeInformation.NumberOfKnownPerson = 1;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.clearImage = function () {
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    };

    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeDocument + '/' + data.FileId + extention;
    };

    // #region Document

    $scope.Loaddocumentdatalist = function () {
        $http.get('employees/employeeinformation/getempdocumentdatalist?companyGroupId=' + $scope.CompanyGroupID + '&pId=' + $scope.user + '&plantId=' + $scope.PlantId)
            .then(function (response) {
                $scope.documentdataList = response.data;
                //$scope.getColor($scope.documentdataList.FileName);
            });
    };

    $scope.getInd = function (idx, dt) {
        $scope.indext = idx;
        $scope.documentData = dt;
    };
    OverDocModal
    $scope.docList = [];
    $scope.preRecruitmentDocumentList = [];
    $scope.fileNameChanged = function (d) {
        $scope.filedata = [];
        try {
            var tempInd = $scope.indext;
            var filename = d.value;
            var res = filename.replace(/C:\\fakepath\\/i, '');
            document.getElementById("" + tempInd + "").value = res;
            $scope.filedata = d.files[0];

            var fName = res;
            if (checkFileExist($scope.preRecruitmentDocumentList, fName)) {
                document.getElementById("" + tempInd + "").value = "";
                throw fName + ' This file already added, Please choose another one.';
            }

            if (checkSameFileExist($scope.documentdataList, fName)) {
                document.getElementById("" + tempInd + "").value = "";
                throw fName + ' This file already added, Please choose another one.';
            }

            if ($scope.filedata.size > 2000000) {
                document.getElementById("" + tempInd + "").value = "";
                throw fName + ' File size must be below 2 mb';
            }
            $scope.preRecruitmentDocumentList.push($scope.filedata);

            var nn = $scope.documentData;
            nn.FileName = fName;
            if (nn.FileName.length > 50) {
                throw "File Name must be less than 50 character.";
            }
            nn.PreRecruitmentEmployeeId = $scope.user;
            $scope.docList.push(nn);
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    function checkFileExist(list, name) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].name === name) {
                return true;
            }
        }
        return false;
    }
    function checkSameFileExist(list, name) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FileName === name) {
                return true;
            }
        }
        return false;
    }

    $scope.fg = false;
    $scope.DocShow = function (data) {
        $scope.documentdata = data;
        $scope.filedata = {};
        if (!baseService.isUndefinedOrNull(data.FileName))
            $scope.filedata.name = data.FileName;
        else
            $scope.filedata = null;
        $scope.documentdata.FileName = data.FileName;
        //var filename = document.getElementById("uploadFile").value = data.FileName;

        if ($scope.documentdata.ProfileType === 'NID') {
            if (!baseService.isUndefinedOrNull($scope.NationalID)) {
                $scope.documentdata.DocNumber = $scope.NationalID;
            }
            else {
                $scope.documentdata.DocNumber = $scope.employeeInformation.NationalID;
            }
        }

        if ($scope.documentdata.ProfileType === 'NID') {
            if (!baseService.isUndefinedOrNull($scope.documentdata.DocNumber)) {
                $scope.fg = true;
            }
            else if (baseService.isUndefinedOrNull($scope.documentdata.DocNumber)) {
                $scope.fg = false;
            }
        }
        angular.element(document.querySelector('#DocPopUp')).modal('show');
    };

    $scope.getColor = function (item) {
        var remark = item.FileName;
        if (remark === null || remark === '') {
            return 'empty';
        } else {
            return 'filled';
        }
    };


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    var filtereddata = [];
    var sqlInStatement = "";
    $scope.overDueDataListNew = [];
    $scope.actionCompleteSelected = function (args) {
        if (args.requestType === "filtering") {
            var gridObj = $("#docFilterGrid").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            var uniqueDocCategory = removeDuplicates(filtereddata, 'DocumentCategoryId');
            var uniqueDocSubCategory = removeDuplicates(filtereddata, 'DocumentSubCategoryId');
            var uniqueDocId = removeDuplicates(filtereddata, 'DocumentID');
            var uniqueDocType = removeDuplicates(filtereddata, 'DocumentType');
            var uniqueDocBy = removeDuplicates(filtereddata, 'DocumentationBy');
            var uniqueDocImportance = removeDuplicates(filtereddata, 'Importance');
            var uniqueDocOptionalORMandatory = removeDuplicates(filtereddata, 'OptionalOrMandatory');
            var uniqueEmployeeCategory = removeDuplicates(filtereddata, 'EmpCatgId');



            var wcEmpCatg = "";
            if (uniqueEmployeeCategory.length > 0) {
                wcEmpCatg = "WHERE ISNULL(EmpCatgId,'null') IN(";
                wcEmpCatg += Array.prototype.map.call(uniqueEmployeeCategory, function (item) { return "'" + item.EmpCatgId + "'"; }).join(",") + ")";
            }
            var wcDocCatg = "";
            if (uniqueDocCategory.length > 0) {
                wcDocCatg = "AND ISNULL(DocumentCategoryId,'null') IN(";
                wcDocCatg += Array.prototype.map.call(uniqueDocCategory, function (item) { return "'" + item.DocumentCategoryId + "'"; }).join(",") + ")";
            }
            var wcDocSubCatg = "";
            if (uniqueDocSubCategory.length > 0) {
                wcDocSubCatg = " AND ISNULL (DocumentSubCategoryId,'null') IN(";
                wcDocSubCatg += Array.prototype.map.call(uniqueDocSubCategory, function (item) { return "'" + item.DocumentSubCategoryId + "'"; }).join(",") + ")";
            }
            var wcDocument = "";
            if (uniqueDocId.length > 0) {
                wcDocument = " AND ISNULL(DocumentID,'null') IN(";
                wcDocument += Array.prototype.map.call(uniqueDocId, function (item) { return "'" + item.DocumentID + "'"; }).join(",") + ")";
            }
            var wcDocType = "";
            if (uniqueDocType.length > 0) {
                wcDocType = " AND ISNULL(DocumentType,'null') IN(";
                wcDocType += Array.prototype.map.call(uniqueDocType, function (item) { return "'" + item.DocumentType + "'"; }).join(",") + ")";
            }
            var wcDocBy = "";
            if (uniqueDocBy.length > 0) {
                wcDocBy = " AND ISNULL(DocumentationBy,'null') IN(";
                wcDocBy += Array.prototype.map.call(uniqueDocBy, function (item) { return "'" + item.DocumentationBy + "'"; }).join(",") + ")";
            }
            var wcDocImportance = "";
            if (uniqueDocImportance.length > 0) {
                wcDocImportance = " AND ISNULL(Importance,'null') IN(";
                wcDocImportance += Array.prototype.map.call(uniqueDocImportance, function (item) { return "'" + item.Importance + "'"; }).join(",") + ")";
            }
            var wcDocOptionalOrMandatory = "";
            if (uniqueDocImportance.length > 0) {
                wcDocOptionalOrMandatory = " AND ISNULL(OptionalOrMandatory,'null') IN(";
                wcDocOptionalOrMandatory += Array.prototype.map.call(uniqueDocOptionalORMandatory, function (item) { return "'" + item.OptionalOrMandatory + "'"; }).join(",") + ")";

            }
            
            sqlInStatement = wcEmpCatg + wcDocCatg + wcDocSubCatg + wcDocument + wcDocType + wcDocBy + wcDocImportance + wcDocOptionalOrMandatory;


            $scope.pieOverDue = 0;
            $scope.pieDue = 0;
            $scope.pieCompleted = 0;
            $http({
                method: 'POST',
                url: 'employees/DocumentDashboard/GetMasterSegmentedData',
                params: {
                    'parameterString': sqlInStatement
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.overDueDataListNew = response.data;
                angular.forEach($scope.overDueDataListNew, function (item, i) {
                    $scope.pieOverDue += item.OverAllDueDoc;
                    $scope.pieDue += item.TotalDue;
                    $scope.pieCompleted += item.TotalCompleted;
                });

                $scope.pieChartData($scope.pieOverDue, $scope.pieDue, $scope.pieCompleted);
            });
            $http({
                method: 'POST',
                url: 'employees/DocumentDashboard/GetBarChartInfo',
                params: {
                    'parameterString': sqlInStatement
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.barChartDataList = response.data;
                createDocBarChart($scope.barChartDataList);
            });

            //$http({
            //    method: 'POST',
            //    url: 'employees/DocumentDashboard/GetOverAllOverDueEmployeeList',
            //    params: {
            //        'parameterString': sqlInStatement
            //    },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //    $scope.barDataList = response.data;
            //    //createDocBarChart($scope.barChartDataList);
            //});
        }
    };
    $scope.documentDataList = [];
    $scope.OnloadDocumentListLoad = function () {
        $scope.pieOverDue = 0;
        $scope.pieDue = 0;
        $scope.pieCompleted = 0;
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetMasterSegmentedData',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.overDueDataListNew = response.data;
            angular.forEach($scope.overDueDataListNew, function (item, i) {
                $scope.pieOverDue += item.OverAllDueDoc;
                $scope.pieDue += item.TotalDue;
                $scope.pieCompleted += item.TotalCompleted;
            });
            $scope.pieChartData($scope.pieOverDue, $scope.pieDue, $scope.pieCompleted);

        });
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetBarChartInfo',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.barChartDataList = response.data;
            createDocBarChart($scope.barChartDataList);
        });

    };
    $scope.OnloadDocumentListLoad();


    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }
    $scope.filterDataList = [];
    $scope.GetMasterFilterationData = function () {

        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetMasterFilterationData',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filterDataList = response.data;

            $("#docFilterGrid").children('.e-pager.e-js.e-pager').hide();
            $("#docFilterGrid").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#docFilterGrid").children('.e-gridcontent').hide();
            $("#docFilterGrid").children('.e-grid.e-headercell').css('background-color', 'red');
            //$("#docFilterGrid").children('.e-grid.e-headercell').rowFilter('background-color', 'red');

        });
    };

    $scope.DocOverDueDataList = [];
    $scope.GetMasterFilterationData();
    $scope.GetModalOverDueDoc = function (x,segment) {
        $scope.DocOverDueDataList = [];
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetPieChartDataModalInfo',
            params: {
                'parameterString': sqlInStatement
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.documentDataList = response.data;
            if (x === "") {
                $scope.DocOverDueDataList = $scope.documentDataList.filter(element => element.TotalOverDueDocument > 0);
            }
            else {
                $scope.DocOverDueDataList = $scope.documentDataList.filter(element => element.TotalOverDueDocument > 0 && element.ComplianceDocumentType === x.DocumentType);
            }
           
        });
        $scope.dataGrid = "#docOverDueDoct";
        var eDialog = $("#OverDocModal").data("ejDialog");
        eDialog.open();
        //angular.element(document.querySelector('#OverDocModal')).modal('show');
    };
    $scope.SegmentedDocOverDueDataList = [];
    $scope.GetSegmentedModalOverDueDoc = function (x, segment) {
        $scope.segmentEmp = segment;
        $scope.DocOverDueDataList = [];
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetSegmentedOverDueDocDataModalInfo',
            params: {
                'parameterString': sqlInStatement,
                'segment': segment,
                'documentType': x.DocumentType
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SegmentedDocOverDueDataList = response.data;         
            
        });
        $scope.dataGrid = "#SegmentedOverDueDoct";

        var eDialog = $("#SegmentedOverDueDocModal").data("ejDialog");
        eDialog.open();
        //angular.element(document.querySelector('#SegmentedOverDueDocModal')).modal('show');
    };

    $scope.DocDueDataList = [];

    $scope.GetModalDueDoc = function () {
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetPieChartDataModalInfo',
            params: {
                'parameterString': sqlInStatement
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.DocDueDataList = response.data.filter(element => element.TotalDueDocument > 0);
        });
        $scope.dataGrid = "#docDueDoct";
        var eDialog = $("#DueDocModal").data("ejDialog");
        eDialog.open();
        //angular.element(document.querySelector('#DueDocModal')).modal('show');
    };
    $scope.DocCompletedDataList = [];
    $scope.GetModalCompletedDoc = function () {
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetPieChartDataModalInfo',
            params: {
                'parameterString': sqlInStatement
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.DocCompletedDataList = response.data.filter(element => element.TotalCompletedDocument > 0);
        });
        $scope.dataGrid = "#docCompletedDoc";
        var eDialog = $("#CompletedDocModal").data("ejDialog");
        eDialog.open();

    //    angular.element(document.querySelector('#CompletedDocModal')).modal('show');
    };
    
    $scope.OverAllOverDueDocList = [];
    $scope.GetModalOverAllOverDueDocEmp = function (x, segment) {
        $scope.segmentEmp = segment;
        $scope.OverAllOverDueDocList = [];
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetOverAllOverDueEmployeeList',
            params: {
                'parameterString': sqlInStatement,
                'documentType': x.DocumentType,
                'segment': segment
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OverAllOverDueDocList = response.data;
            
        });
        $scope.dataGrid = "#OverAllOverDueDoc";

        var eDialog = $("#EmpModal").data("ejDialog");
        eDialog.open();
    };

    $scope.ExcelPrint = function (data) {

        //var gridObjExcel = $($scope.dataGrid).data("ejGrid"); docOverDueDoct
        //var gridObjExcel = $($scope.dataGrid).data("ejGrid");
        //var obj = $("#docOverDueDoct").ejGrid("instance");
        //var data = gridObjExcel.model.currentViewData;
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


    $scope.ExcelDownload = function () {
        var gridObj = $($scope.dataGrid).data("ejGrid");
        var data = gridObj.model.dataSource();//columns
        //var data = gridObj.model.columns;//columns

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
    $scope.EmployeeWiseDocList = [];

    $scope.GetEmployeeWiseOptionalOrMandatoryDocumentList = function (x, OptionalOrMandatory) {
        $scope.EmployeeWiseDocList = [];

        //string employeeId, string documentType, string OptionalOrMandatory, string segment
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetEmployeeWiseOptionalOrMandatoryDocumentList',
            params: {
                'parameterString': sqlInStatement,
                'employeeId': x.data.EmployeeId,
                'documentType': x.data.DocumentType,
                'OptionalOrMandatory': OptionalOrMandatory,
                'segment': $scope.segmentEmp
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeWiseDocList = response.data;
        });
        $scope.dataGrid = "#docCompletedDocEmp";

        var eDialog = $("#EmpDocModal").data("ejDialog");
        eDialog.open();
        //angular.element(document.querySelector('#EmpDocModal')).modal('show');
    };
    $scope.DocWiseEmployeeList = [];
    $scope.GetDocumentWiseEmployeeList = function (x) {
        $scope.DocWiseEmployeeList = [];
        //string employeeId, string documentType, string OptionalOrMandatory, string segment
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetDocumentWiseEmployeeList',
            params: {
                'parameterString': sqlInStatement,
                'documentId': x.data.DocumentID,
                'segment': $scope.segmentEmp,
                'documentType': x.data.DocumentType,
                'OptionalOrMandatory': x.data.OptionalOrMandatory
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocWiseEmployeeList = response.data;
        });
        $scope.dataGrid = "#DocWiseEmp";

        var eDialog = $("#DocWiseEmpModal").data("ejDialog");
        eDialog.open();
    };

    $scope.GetCompleteAndDueDocumentWiseEmployeeList = function (x,seg) {
        $scope.DocWiseEmployeeList = [];
        //string employeeId, string documentType, string OptionalOrMandatory, string segment
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetCompleteAndDueDocumentWiseEmployeeList',
            params: {
                'parameterString': sqlInStatement,
                'documentId': x.data.ComplianceDocumentId,
                'segment': seg,
                'documentType': x.data.ComplianceDocumentType,
                'OptionalOrMandatory': x.data.ComplianceDocumentOptionalOrMandatory
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocWiseEmployeeList = response.data;
        });
        $scope.dataGrid = "#DocWiseEmp";

        var eDialog = $("#DocWiseEmpModal").data("ejDialog");
        eDialog.open();
    };

    ///////////////////////////////////////////THE 3RD PAGE \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    // The Inactive EmployeeList

    $scope.employeeInformation1 = {
        SystemId: null,
        EmployeeId: null,
        PreRecruitmentEmployeeId: null,
        EmployeeCode: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        UnitId: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        SubdivisionID: null,
        LineId: null,
        DesignationGroupId: null,
        DesignationSystemID: null,
        BudgetCode: null,
        PositionID: null,
        IsDirect: null,
        SalaryPercentage: null,
        CardNumber: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        EmployeeName: null,
        NickName: null,
        LocalEmployeeName: null,
        EmpPicPath: null,
        EmpType: null,
        EmploymentType: null,
        EmployeeGroupSystemID: null,
        JobLocationID: null,
        DOB: null,
        DOJ: null,
        DOCIsDay: null,
        DOCDay: null,
        DOCIsMonth: null,
        DOCMonth: null,
        DOC: null,
        DOS: null,
        IsConfirmed: null,
        ReActiveDate: null,
        EmployeeStatus: null,
        NationalID: null,
        TIN: null,
        CitizenID: null,
        FatherName: null,
        MotherName: null,
        ReligionID: null,
        CivilStatusID: null,
        employeeID: null,
        GenderID: null,
        SpouseName: null,
        SpouseNationalID: null,
        SpouseOccupation: null,
        NoOfChildren: null,
        PresentAddress1: null,
        PresentAddress2: null,
        ParmanentAddress1: null,
        ParmanentAddress2: null,
        PresThanaID: null,
        ParmThanaID: null,
        PresPostOfficeID: null,
        ParmPostOfficeID: null,
        PresZipCode: null,
        ParmZipCode: null,
        PresDistrictID: null,
        ParmDistrictID: null,
        PresCountryID: null,
        ParmCountryID: null,
        PresCityID: null,
        ParmCityID: null,
        PresAreaID: null,
        ParmAreaID: null,
        TelePhnNo: null,
        CellPhnNo: null,
        EmailId: null,
        BudgetCategoryID: null,
        EmployeeCategorySystemID: null,
        LVPolicyMasterSystemID: null,
        SalaryRuleMasterSystemID: null,
        BankSystemID: null,
        BankName: null,
        BankAccNo: null,
        BankAddedBy: null,
        BankDateAdded: null,
        BankUpdatedBy: null,
        BankDateUpdated: null,
        RegisterFP: null,
        RegisterProximate: null,
        SuperViser: null,
        IsSlvDevReg: null,
        IsAttdnProcBaseOnDeviceData: null,
        SubSecStrucSystemID: null,
        AddedBy: null,
        DateAdded: null,
        UpdatedBy: null,
        DateUpdated: null,
        EmrCntPer1Name: null,
        EmrCntPer1CellNo: null,
        EmrCntPer2Name: null,
        EmrCntPer2CellNo: null,
        GivenDesignationId: null,
        LegalDesignationId: null,
        AgreedDOJ: null,
        TotalSalary: null,
        SpecialReviewDuration: null,
        SpecialReviewAmount: null,
        Image: null
    };



    $scope.GetInactive = function (e) {
       
        $scope.employeeInformation1 = e.data;
        $scope.imageSrc = virtualPath.EmployeePic + $scope.employeeInformation1.EmpPicPath;
        $rootScope.img = $scope.employeeInformation1.EmpPicPath;
        $scope.user = $scope.employeeInformation1.SystemId;
        $scope.CompanyGroupID = $scope.employeeInformation1.GroupID;
        $scope.CompanyID = $scope.employeeInformation1.CompanyId;
        $scope.CountryId = $scope.employeeInformation1.CountryId;
        $scope.BudgetCode = $scope.employeeInformation1.BudgetCode;
        $scope.PlantId = $scope.employeeInformation1.PlantId;

        $scope.employeeInformation1.DOB = $filter('dateFiltering')($scope.employeeInformation1.DOB, 'dd-M-yyyy');
        $scope.employeeInformation1.BirthdayCelebrationDate = $filter('dateFiltering')($scope.employeeInformation1.BirthdayCelebrationDate, 'dd-M-yyyy');
        $scope.employeeInformation1.DOJ = $filter('dateFiltering')($scope.employeeInformation1.DOJ, 'dd-M-yyyy');
        $scope.employeeInformation1.MarriagedayCelebrationDate = $filter('dateFiltering')($scope.employeeInformation1.MarriagedayCelebrationDate, 'dd-M-yyyy');
        $scope.employeeInformation1.DOS = $filter('dateFiltering')($scope.employeeInformation1.DOS, 'dd-M-yyyy');
        $scope.Tin = $scope.employeeInformation1.TINCaption;
        if (baseService.isUndefinedOrNull($scope.Tin)) {
            $scope.Tin = "TIN";
        }
        $scope.Nid = $scope.employeeInformation1.NIDCaption;
        if (baseService.isUndefinedOrNull($scope.Nid)) {
            $scope.Nid = "National ID";
        }
        $scope.NidLength = $scope.employeeInformation1.NIDLength;
        $scope.TinLength = $scope.employeeInformation1.TINLength;
        $scope.SalaryRangeForTax = $scope.employeeInformation1.TINRequiredForSalaryAbove;
        $scope.SalaryRangeForTaxRequired = $scope.employeeInformation1.IsTINRequiredForSalaryAbove;
        $scope.TotalSalary = $scope.employeeInformation1.TotalSalary;
        $scope.NationalID = $scope.employeeInformation1.NationalID;
        $scope.TIN = $scope.employeeInformation1.TIN;
        $rootScope.PhoneLength = $scope.employeeInformation1.PhoneLength;

        if (baseService.isUndefinedOrNull($scope.employeeInformation1.Salutation)) {
            $scope.showdiv = false;
        }
        else {
            if ($scope.employeeInformation1.Salutation.length > 0) {
                $scope.showdiv = true;
            }
            else {
                $scope.showdiv = false;
            }
        }
        // $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        //$scope.getSalutationList($scope.CompanyGroupID);
        $scope.Loaddocumentdatalist($scope.user);
        if (baseService.isUndefinedOrNull($scope.employeeInformation1.EmpPicPath)) {
            $scope.imageSrc = null;
            if ($rootScope.GenderID === 'Male') {
                $scope.imageSrc = "empprofile/Images/male-alt.png";
            } else {
                $scope.imageSrc = "empprofile/Images/female-alt.png";
            }
        }
        //$scope.celebrationMarriage();
    };



    $scope.searchParam = "";
    $scope.searchData = "";
    $scope.searchInactive = [
        {
            'name': 'Employee Adhar No',
            'value': 'NationalID'
        },
        {
            'name': 'Employee Cell No',
            'value': 'CellPhnNo'
        }
    ];
   
    $scope.getInactiveData = function (pageno) {
       
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/GetInactiveEmployeeList',
            params: {'col':$scope.searchParam , 'val':$scope.searchData},
            dataType: 'Json'
        }).then(function success(response) {
            var ColumnList = [
                { field: 'EmployeeCode', width: 80, headerText: "Employee Code", type: "string" },
                { field: 'EmployeeName', width: 80, headerText: "Employee Name", type: "string" },
                { field: 'EDOJ', width: 80, headerText: "DOJ", type: "string" },
                { field: 'EDOB', width: 80, headerText: "DOB", type: "string" },
                { field: 'EDOS', width: 80 , headerText: "DOS", type: "date"},
                { field: 'Department', width: 80, headerText: "Department", type: "string" },
                { field: 'Designation', width: 80, headerText: "Designmation", type: "string" }
               

            ];
            $("#inactiveGrid").ejGrid({
                dataSource: response.data,
                minWidth: 450, minHeight: 4000,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true,  allowTextWrap: true, allowScrolling: true, 
                filterSettings: { filterType: "excel" },
                recordDoubleClick: $scope.GetInactive,
                columns: ColumnList
            });

            var gridObj = $("#inactiveGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            //$scope.employees = [];
            //$scope.employees = response.data;
        });
    }


    
    //var button = new ej.buttons.Button({ $scope.ExcelPrint }, '#linkbtn');
    //-----------------------------Employee InforMation End-----------------------------------//
}