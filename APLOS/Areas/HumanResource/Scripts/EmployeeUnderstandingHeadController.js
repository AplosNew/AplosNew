'use strict';
EmployeeUnderstandingHeadController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeUnderstandingHeadController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'EmployeeUnderstandingHead';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/EmployeeUnderstandingHead/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveActivityUrl = $scope.path + 'SaveActivity';
    $scope.saveDocumentUrl = $scope.path + 'SaveDocument';
    $scope.saveChildUrl = $scope.path + 'CreateChild';
    
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


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
    $scope.ActivityList = [];
    $scope.getActivityGridData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetActivityList",
            data: { 'EmpUnderstandingHeadId': $scope.ModelNew.Id  },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ActivityList = response.data;
        });
    }
    $scope.DocumentList = [];
    $scope.getDocumentGridData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDocumentList",
            data: { 'EmpUbderstandingActivityId': $scope.activityNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.DocumentList = response.data;
        });
    }

    $scope.StatusList = [{ value: 'DefaltInProgress', name: 'Defalt In-Progress' },
        { value: 'Confirm', name: 'Confirm' },
        { value: 'Approved', name: 'Approved' }]

    $scope.ActivityClassList = [{ value: 'Prime', name: 'Prime' },
        { value: 'Secondery', name: 'Secondery' },
        { value: 'Other', name: 'Other' }]

    $scope.ActivityCategoryList = [{ value: 'Planning', name: 'Planning' },
        { value: 'FollowUp', name: 'Follow-Up' },
        { value: 'Decision', name: 'Decision' },
        { value: 'Execution', name: 'Execution' },
        { value: 'Review', name: 'Review' },
        { value: 'Other', name: 'Other (please Specify)' }     ]

    $scope.PriorityList = [{ value: 'Top5', name: 'Top 5' },
        { value: 'Top10', name: 'Top 10' },
        { value: 'Other', name: 'Other' }]

    $scope.ActivityTypeList = [{ value: 'ValueAdded', name: 'Value Added' },
        { value: 'NonValueAddedNecessary', name: 'Non-Value Added (Necessary)' },
        { value: 'NonValueAddedUnnecessary', name: 'Non-Value Added (Unnecessary)' }]

    $scope.ActivityImportanceList = [{ value: 'Normal', name: 'Normal' },
        { value: 'High', name: 'High' },
        { value: 'Medium', name: 'Medium' },
        { value: 'Critical', name: 'Critical' }]

    $scope.PeriodList = [{ value: 'Daily', name: 'Daily' },
        { value: 'Weekly', name: 'Weekly' },
        { value: 'Fortnight', name: 'Fortnight' },
        { value: 'Monthly', name: 'Monthly' },
        { value: 'Quarterly', name: 'Quarterly' },
        { value: 'HalfYearly', name: 'Half Yearly' },
        { value: 'Annually', name: 'Annually' }]

    $scope.FinancialImpactList = [{ value: 'Yes', name: 'Yes' },
        { value: 'No', name: 'No' }]

    $scope.PreparedByList = [{ value: 'Self', name: 'Self' },
        { value: 'OtherEmployee', name: 'Other Employee' },
        { value: 'Custmor', name: 'Custmor' },
        { value: 'Vendor', name: 'Vendor' },
        { value: 'Other', name: 'Other' },
        { value: 'Government', name: 'Government' }]


    $scope.DocumentTypeList = [{ value: 'WithinDepartment', name: 'Within Department' },
        { value: 'WithinEntity', name: 'Within Entity' },
        { value: 'WithinCompany', name: 'Within Company' },
        { value: 'WithinGroup', name: 'Within Group' },
        { value: 'Customer', name: 'Customer' },
        { value: 'Vendor', name: 'Vendor' },
        { value: 'Other', name: 'Other' }]

    $scope.DocumentGenerationList = [{ value: 'System', name: 'System' },
        { value: 'Manual', name: 'Manual' },
        { value: 'Other', name: 'Other' }]

    $scope.DocumentPreprationFrequencyList = [{ value: 'OnLine', name: 'On Line' },
        { value: 'Daily', name: 'Daily' },
        { value: 'Weekly', name: 'Weekly' },
        { value: 'Fortnight', name: 'Fortnight' },
        { value: 'Monthly', name: 'Monthly' },
        { value: 'Quarterly', name: 'Quarterly' },
        { value: 'HalfYearly', name: 'Half Yearly' },
        { value: 'Annually', name: 'Annually' },
        { value: 'AsAndWhenRequired', name: 'As and When Required' }]

    $scope.DocumentClassList = [{ value: 'Register', name: 'Register' },
        { value: 'Document', name: 'Document' },
        { value: 'Form', name: 'Form' },
        { value: 'Report', name: 'Report' },
        { value: 'Email', name: 'Email' }]

    $scope.DocumentFormatList = [{ value: 'PDF', name: 'PDF' },
        { value: 'JPEG', name: 'JPEG' },
        { value: 'Excel', name: 'Excel' },
        { value: 'Word', name: 'Word' },
        { value: 'Register', name: 'Register' },
        { value: 'Form', name: 'Form' },
        { value: 'Email', name: 'Email' },
        { value: 'PPT', name: 'PPT' },
        { value: 'CrystalReport', name: 'Crystal Report' },
        { value: 'Txt', name: 'Txt' },
        { value: 'CSV', name: 'CSV' }]
    
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.ModelTemp = {
        Id: null,
        Date: null,
        PositionCode: null,
        BudgetCode: null,
        EmployeeCode: null,
        EmployeeName: null,
        EmployeeId: null,
        Status: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.activity = {
        Id: null,
        EmpUnderstandingHeadId: null,
        EmployeeId: null,
        Code: null,
        ActivityName: null,
        ActivityDetail: null,
        PurposeOfTheActivity: null,
        ActivityCategory: null,
        OtherActivityCategory: null,
        ActivityClass: null,
        Priority: null,
        ActivityType: null,
        Period: null,
        Frequency: 1,
        AverageTime: null,
        ActivityImportance: null,
        ValueInActivity: null,
        FinancialImpact: null,
        Documents: false,
        Remarks: null,
        KPI: false
    }
    $scope.activityNew = Object.assign({}, $scope.activity);
    $scope.documentActivity = {
        Id: null,
        EmpUbderstandingActivityId: null,
        Name: null,
        FileName: null,
        DocumentType: null,
        DataSourceCategoryId: null,
        DocumentFormateId: null,
        ApplicationName: null,
        PreparedBy: null,
        Remarks: null,
        PreparedByInCaseOfOther: null,
        PreparedByInCaseOfOtherName: null
    }
    $scope.documentActivityNew = Object.assign({}, $scope.documentActivity);

    $scope.kpi = {
        Id: null,
        EmpUbderstandingActivityId: null,
        Name: null,
        Remarks: null,
        KPIDetail: null
    }
    $scope.kpiNew = Object.assign({}, $scope.kpi);
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.searchByParty = "UserName"; $scope.searchParty = "";

    //$scope.getActivityList = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'HumanResource/EmployeeUnderstandingHead/getactivitycbolist?employeeId=' + $scope.employee.Id
    //    }).then(function (response) {
    //        $scope.activitydocumentList = response.data;
    //        $scope.documentActivityNew.ActivityId = $scope.activityId;
    //    });
    //};




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

    $scope.employeeUrl = 'OrderManagements/masterorder/GetEmployeeListResponsible';

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
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];

            $scope.ModelNew.EmployeeCode = employee.EmployeeCode;
            $scope.ModelNew.EmployeeId = employee.SystemId;
            $scope.ModelNew.EmployeeName = employee.EmployeeName;
            $scope.ModelNew.PositionCode = employee.PositionCode;
            $scope.ModelNew.BudgetCode = employee.BudgetCode;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
       /* $scope.GetActivity(args.data.Id);*/
        $scope.getActivityGridData($scope.ModelNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.GetActivity = function (args) {
        $scope.activityNew = Object.assign({}, args.data);
    };
    $scope.GetDocument = function (args) {
        $scope.documentActivityNew = Object.assign({}, args.data);
    };

    $scope.Save = function () {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   /* ClearFields(response.data.Sequence);*/
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    };
    $scope.SaveActivity = function () {
        $http({
            method: 'POST',
            url: $scope.saveActivityUrl,
            data: { 'data': $scope.activityNew, 'EmpUnderstandingHeadId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
               /* ClearFields(response.data.Sequence);*/
                $scope.getData();
                $scope.getActivityGridData($scope.ModelNew.Id);

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    $scope.SaveDocument = function () {
        $http({
            method: 'POST',
            url: $scope.saveDocumentUrl,
            data: { 'data': $scope.documentActivityNew, 'EmpUbderstandingActivityId': $scope.activityNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                /* ClearFields(response.data.Sequence);*/
                $scope.getData();
                $scope.getDocumentGridData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
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
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }

    $scope.ClearActivity = function () {
        $scope.activityNew = Object.assign({}, $scope.activity);
    };

    $scope.ClearDocument = function() {
        $scope.documentActivityNew = Object.assign({}, $scope.documentActivity);
    }

    $scope.filedata = null;
    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });

    $("#uploadBtn2").change(function () {
        $scope.filedata = this.files[0];
    });
    //document.getElementById("uploadBtn").onchange = function () {
    //    var filename = document.getElementById("uploadFile").value = this.value;
    //    var res = filename.replace(/C:\\fakepath\\/i, '');
    //    document.getElementById("uploadFile").value = res;
    //};
    //document.getElementById("uploadBtn2").onchange = function () {
    //    var filename = document.getElementById("uploadFile2").value = this.value;
    //    var res = filename.replace(/C:\\fakepath\\/i, '');
    //    document.getElementById("uploadFile2").value = res;
    //};

    $scope.documentRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmdocDelete')).modal('show');
    };
    $scope.removeDocument = function () {
        angular.element(document.querySelector('#confirmdocDelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.documentActivityNew.FileName)) {
            document.getElementById('uploadBtn').value = '';
            document.getElementById('uploadFile').value = "";
            $scope.filedata = null;
        }
        else {
            $scope.ClearDoc();
        }
    };
    $scope.confirmClosedocDelete = function () {
        angular.element(document.querySelector('#confirmdocDelete')).modal('hide');
    };

    $scope.ClearDoc = function () {
        document.getElementById('uploadBtn').value = '';
        $scope.filedata = '';
        $scope.documentActivityNew.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
        $scope.UpdateDoc();
    };
    function confirmPopUp(d, msg) {
        var message = '';
        if (d !== null || d !== undefined) {
            if (d === 'd') {
                if (!baseService.isUndefinedOrNull($scope.documentId))
                    message = 'Document Created : [' + $scope.documentId + ']<br />';
                $scope.message_confirmation = message + 'Does this activity have <b>' + msg + '</b> Document?';
            }
            else
                $scope.message_confirmation = 'Does this activity have <b>' + msg + '</b> KPI?';
            angular.element(document.querySelector('#documentPopUp')).modal('hide');
            angular.element(document.querySelector('#document')).modal('show');
        }
    };
}