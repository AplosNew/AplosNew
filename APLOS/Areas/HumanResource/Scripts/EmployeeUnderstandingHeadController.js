'use strict';
EmployeeUnderstandingHeadController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', '$window', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeUnderstandingHeadController(cboService, commonMessage, $scope, $rootScope, $window, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'EmployeeUnderstandingHead';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/EmployeeUnderstandingHead/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveActivityUrl = $scope.path + 'SaveActivity';
    $scope.saveDocumentUrl = $scope.path + 'SaveDocument';
    $scope.saveKPIUrl = $scope.path + 'SaveKPI';
    $scope.saveChildUrl = $scope.path + 'CreateChild';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.deleteAttachmentUrl = $scope.path + 'DeleteQualification/';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';

    $scope.ModelTemp = {
        Id: null,
        Date: null,
        PositionCode: null,
        BudgetCode: null,
        PCode: null,
        MBCode: null,
        EmployeeCode: null,
        EmployeeName: null,
        EmployeeId: null,
        Remarks: null,
        Status: 'InProgress'
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
        KPI: false,
        ApplicableDocument: false,
        ApplicableKPI: false
    }
    $scope.activityNew = Object.assign({}, $scope.activity);
    $scope.documentActivity = {
        Id: null, EmpUnderstandingActivityId: null, DocumentCategoryId: null, EmployeeId: null, DocumentPreprationFrequency: null, DocumentType: null, DocumentFormat: null, DocumentClass: null, DocumentCode: null, DocumentName: null, Remarks: null, Attachment: null, FileName: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, DocumentGeneration: null, PreparedBy: null
    }
    $scope.documentActivityNew = Object.assign({}, $scope.documentActivity);

    $scope.kpi = {
        Id: null,
        EmpUnderstandingActivityId: null,
        KPIName: null,
        Remarks: null,
        KPIDetail: null,
        KPIReviewPeriod: null
    }
    $scope.kpiNew = Object.assign({}, $scope.kpi);

    $scope.getMasterInfoEI = function () {
        $http({
            method: 'GET',
            url: 'HumanResource/EmployeeUnderstandingHead/GetMasterDataFromEI?EmployeeId=' + $scope.ModelNew.EmployeeId
        }).then(function (response) {
            $scope.ModelNew = Object.assign({}, response.data[0]);
            $scope.ModelNew.Status = 'InProgress';
        });
    };

    $scope.getData = function () {
        $http({
            method: 'GET',
            url: 'HumanResource/EmployeeUnderstandingHead/GetList?employeeId=' + $scope.ModelNew.EmployeeId
        }).then(function (response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.ModelNew = Object.assign({}, response.data[0]);
                $scope.ModelNew.Date = $filter('dateFiltering')(new Date($scope.ModelNew.Date), 'dd-MM-yyyy');
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
                $scope.getMasterInfoEI();
            } else {
                $scope.getActivityGridData();
            }
        });
    }
    $scope.getData();

    $scope.ActivityList = [];
    $scope.getActivityGridData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetActivityList",
            data: { 'EmpUnderstandingHeadId': $scope.ModelNew.Id },
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
            data: { 'EmpUnderstandingActivityId': $scope.ActivityId },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.DocumentList = response.data;
        });
    }
    $scope.KPIList = [];
    $scope.getKPIGridData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetKPIList",
            data: { 'EmpUnderstandingActivityId': $scope.ActivityId },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.KPIList = response.data;
        });
    }

    $scope.DocumentCategoryList = [];
    $scope.getDocumentCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDocumentCategoryList",
            //data: { 'DocumentCategoryId': $scope.documentActivityNew.DocumentCategoryId},
            //dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocumentCategoryList = response.data;
        });
    };

    $scope.dwonloadUrl = null;
    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;

        //var extention = str.substr(str.indexOf('.'));
        const last2 = str.slice(-5);

        var extentions = last2.substr(last2.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ActivityDocuments + '/' + data.Id + extentions;
        $window.open($scope.dwonloadUrl, '_blank');
    };

    $scope.StatusList = [{ value: 'InProgress', name: 'In-Progress' },
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
    { value: 'Other', name: 'Other (please Specify)' }]

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

    $scope.KPIReviewPeriodList = [{ value: 'Daily', name: 'Daily' },
    { value: 'Weekly', name: 'Weekly' },
    { value: 'Fortnight', name: 'Fortnight' },
    { value: 'Monthly', name: 'Monthly' },
    { value: 'Quarterly', name: 'Quarterly' },
    { value: 'HalfYearly', name: 'Half Yearly' },
    { value: 'Annually', name: 'Annually' }]

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.searchByParty = "UserName"; $scope.searchParty = "";

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

    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;
            if ($scope.Name == 'mo') {
                $scope.employeeUrl = 'OrderManagements/masterorder/GetEmployeeListResponsible';
            } else {
                $scope.employeeUrl = 'OrderManagements/masterorder/GetPreparedEmployeeList?employeeId=' + $window.employeeId
            }

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

            if ($scope.Name == 'mo') {
                $scope.ModelNew.EmployeeCode = employee.EmployeeCode;
                $scope.ModelNew.EmployeeId = employee.SystemId;
                $scope.ModelNew.EmployeeName = employee.EmployeeName;
                $scope.ModelNew.PositionCode = employee.PositionCode;
                $scope.ModelNew.BudgetCode = employee.BudgetCode;
                $scope.ModelNew.MBCode = employee.MBCode;
                $scope.ModelNew.PCode = employee.PCode;
                $scope.getData();
            } else {
                $scope.documentActivityNew.PreparedByInCaseOfOtherName = employee.EmployeeName;
                $scope.documentActivityNew.EmployeeId = employee.SystemId;
            }
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
        $scope.filedata = {};
        $scope.documentActivityNew = Object.assign({}, args.data);
        $scope.filedata.name = $scope.documentActivityNew.FileName;
        var filename = document.getElementById("uploadFile").value = $scope.documentActivityNew.FileName;

    };

    $scope.GetKPI = function (args) {
        $scope.kpiNew = Object.assign({}, args.data);
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue == null || fieldValue == '') {
                throw ('[' + fieldName + '] is required...')
            }
        } catch (e) {
            throw e;
        }
    };

    function ValidationEmployee() {
        try {
            CheckField($scope.ModelNew.EmployeeId, "Employee");
            CheckField($scope.ModelNew.Status, "Status.");
            CheckField($scope.ModelNew.Date, "Date");
        } catch (e) {
            throw e;
        }
    };

    function ValidationActivity() {
        try {
            CheckField($scope.activityNew.ActivityName, "Activity");
            CheckField($scope.activityNew.ActivityDetail, "Activity Detail");
            CheckField($scope.activityNew.ActivityCategory, "Activity Category");
            CheckField($scope.activityNew.OtherActivityCategory, "Other Activity Category");
            CheckField($scope.activityNew.ActivityImportance, "Activity Importance");
            CheckField($scope.activityNew.ActivityClass, "Activity Class");
            CheckField($scope.activityNew.ActivityType, "Activity Type");
            CheckField($scope.activityNew.Priority, "Priority");
            CheckField($scope.activityNew.Frequency, "Frequency");
            CheckField($scope.activityNew.Period, "Period");
            CheckField($scope.activityNew.FinancialImpact, "Financial Impact");
            CheckField($scope.activityNew.AverageTime, "Average Time");
            CheckField($scope.activityNew.PurposeOfTheActivity, "Purpose of the activity");
        } catch (e) {
            throw e;
        }
    };

    function ValidationDocument() {
        try {
            CheckField($scope.documentActivityNew.DocumentName, "Document Name");
            CheckField($scope.documentActivityNew.DocumentFormat, "Document Format");
            CheckField($scope.documentActivityNew.DocumentType, "Document Type");
            CheckField($scope.documentActivityNew.DocumentCode, "Document Code");
            CheckField($scope.documentActivityNew.DocumentGeneration, "Document Generation");
            CheckField($scope.documentActivityNew.DocumentPreprationFrequency, "Document Prepration Frequency");
            CheckField($scope.documentActivityNew.DocumentCategoryId, "Document Category");
            CheckField($scope.documentActivityNew.DocumentClass, "Document Class");
            CheckField($scope.documentActivityNew.PreparedBy, "Prepared by");
        } catch (e) {
            throw e;
        }
    };


    function ValidationKPI() {
        try {
            CheckField($scope.kpiNew.KPIName, "KPI Name");
            CheckField($scope.kpiNew.KPIDetail, "KPI Detail");
            CheckField($scope.kpiNew.KPIReviewPeriod, "KPI Review Period");
        } catch (e) {
            throw e;
        }
    };


    $scope.Save = function () {
        try {
            ValidationEmployee();
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
                    $scope.ModelNew.Id = response.data.Id

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.savebtndisable = false;
    $scope.SaveActivity = function () {
        try {
            ValidationActivity();
            $scope.savebtndisable = true;
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
                    $scope.getActivityGridData($scope.ModelNew.Id);
                    $scope.clearactivity();
                    $scope.savebtndisable = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
                $scope.savebtndisable = false;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.clearactivity = function () {
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
            KPI: false,
            ApplicableDocument: false,
            ApplicableKPI: false
        }
        $scope.activityNew = Object.assign({}, $scope.activity);
        $scope.savebtndisable = false;
    };

    $scope.filedata = null;
    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };

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
    };

    $scope.SaveDocument = function () {
        try {
            ValidationDocument();
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = '';
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.documentActivityNew.FileName = fileName;
            //$scope.documentActivityNew.Attachment = $scope.fileId();
            $scope.documentActivityNew.EmpUnderstandingActivityId = $scope.ActivityId;
            var formData = new FormData();


            $http({
                method: 'POST',
                url: $scope.saveDocumentUrl,
                //data: { 'data': $scope.documentActivityNew, 'EmpUnderstandingActivityId': $scope.ActivityId },
                //dataType: 'JSON'

                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("documentActivityNew", angular.toJson(data.documentActivityNew));
                    if (baseService.isUndefinedOrNull($scope.filedata) == false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'documentActivityNew': $scope.documentActivityNew, 'file': $scope.filedata }

            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDocumentGridData();
                    $scope.ClearDocument();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure', 'documentPopUp');
        }
    };

    $scope.SaveKPI = function () {
        try {
            ValidationKPI();
            $http({
                method: 'POST',
                url: $scope.saveKPIUrl,
                data: { 'data': $scope.kpiNew, 'EmpUnderstandingActivityId': $scope.ActivityId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    /* ClearFields(response.data.Sequence);*/
                    $scope.getData();
                    $scope.getKPIGridData();
                    $scope.ClearKPI();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure', 'kpiPopUp');
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

    $scope.DeleteAttachment = function () {
        $http({
            method: 'POST',
            url: $scope.deleteAttachmentUrl + $scope.documentActivityNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });

    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.ClearDocument = function () {
        $scope.documentActivity = {
            Id: null, EmpUnderstandingActivityId: null, DocumentCategoryId: null, EmployeeId: null, DocumentPreprationFrequency: null, DocumentType: null, DocumentFormat: null, DocumentClass: null, DocumentCode: null, DocumentName: null, Remarks: null, Attachment: null, FileName: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
        }
        $scope.documentActivityNew = Object.assign({}, $scope.documentActivity);
        $scope.ClearDoc();
    }
    $scope.ClearKPI = function () {
        $scope.kpiNew = Object.assign({}, $scope.kpi);
    }


    $scope.GetApplicableDocument = function (args) {
        try {
            if (args.data.IsApplicableDocument =="Yes") {
                $scope.ActivityId = args.data.Id;
                $scope.getDocumentGridData();
                $scope.getDocumentCategory();
                angular.element(document.querySelector('#documentPopUp')).modal('show');
            } else {
                throw "Document is not Applicable";
            }
        } catch (e) {
            ShowResult(e, 'info');
        }

    };
    $scope.GetApplicableKPI = function (args) {
        try {
            if (args.data.IsApplicableKPI =="Yes") {
                $scope.ActivityId = args.data.Id;
                $scope.getKPIGridData();
                angular.element(document.querySelector('#kpiPopUp')).modal('show');
            } else {
                throw "KPI is not Applicable";
            }
        } catch (e) {
            ShowResult(e, 'info');
        }

    };

    $scope.CloseDocumentPopUp = function () {
        $scope.ClearDocument();
        angular.element(document.querySelector('#documentPopUp')).modal('hide');
    };

    $scope.CloseKPIPopUp = function () {
        $scope.ClearKPI();
        angular.element(document.querySelector('#kpiPopUp')).modal('hide');
    };
    $scope.confirmClosedocDelete = function () {
        angular.element(document.querySelector('#confirmdocDelete')).modal('hide');
    };

    $scope.documentRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmdocDelete')).modal('show');
    };

    $scope.getPreparedByCode = function (ob) {
        $scope.documentActivityNew.PreparedByInCaseOfOther = ob.Id;
        $scope.documentActivityNew.PreparedByInCaseOfOtherName = ob.Name;
        angular.element(document.querySelector('#Prepared')).modal('hide');
    };

    $scope.clearPreparedByCode = function () {
        $scope.documentActivityNew.PreparedByInCaseOfOther = null;
        $scope.documentActivityNew.PreparedByInCaseOfOtherName = null;
    };

}