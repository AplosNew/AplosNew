'use strict';
issueAuditController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function issueAuditController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'issueAudit';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.issueAudits = [];
    $scope.path = 'issueTracker/issueAudit/';
    $scope.getListUrl = $scope.path + 'GetList';

    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'InsertIssueAuditDetail';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    baseService.init('issueTracker/IssueTransaction/GetListIssueTransaction', null, null, "DESC", "Id", "Id");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.issueAudits = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.issueAudit = {
        Id: null,
        IssueTransactionId: null,
        IssueAuditTime: null,
        Remarks: null,
        Points: null,
        Attachment: null,
        IssueAuditDetail: null,
        IssueTransactionId: null,
        IssueStatus: null,
        IssueCategory: null,
        IssueSubCategory: null,
        IssueImportance: null,
        BuyerName: null,
        OverdueDays: null,
        StatusUpdateInterval: null,
        CostCenter: null,
        CostCenterId: null,
        Mentor: null,
        BuyerName: null,
        TargetDate: null,
        PointsList: null,
        EmployeeId: null,

        IsInternalApplicable: false,
        IsInternalRecurring: false,
        InternalFrequencyType: null,
        InternalFrequencyDays: null,
        InternalEndDateTime: null,
        InternalResponsiblePersonId: null,
        InternalOneTimeDateTime: null,
        DueDate:null
    };

    $scope.PointsList = ['0', '1','2'];
    $scope.issueAuditNew = Object.assign({}, $scope.issueAudit);

    $scope.getIssueAuditDetail = function (id) {
        $http({
            method: "get",
            url: "IssueTracker/IssueAudit/GetById?issueAuditId=" + id
        }).then(function successCallback(response) {
            $scope.issueAuditDetailList = response.data;
        });
    }
   // $scope.getIssueAuditDetail();
    $scope.issueAuditList = [];
    $scope.getIssueAuditByIssueTransactionId = function (issueTransactionId) {
        $http({
            method: "get",
            url: "IssueTracker/IssueAudit/GetIssueAuditByIssueTransactionId?issueTransactionId=" + issueTransactionId
        }).then(function successCallback(response) {
            $scope.issueAuditList = response.data;
        });
    }
    
    $scope.searchByList = [
        {
            "name": "Issue",
            "value": "Issue"
        },
        {
            "name": "Mentor",
            "value": "Mentor"
        },
        {
            "name": "Buyer Name",
            "value": "BuyerName"
        }
        ,
        {
            "name": "Target Date",
            "value": "TargetDate"
        }
       
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.ChangeIssueStandard(id);
        $scope.getIssueAuditByIssueTransactionId(id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.issueAuditNew.IssueTransactionId = null;
    $scope.ChangeIssueStandard = function (id) {
        $http({
            method: "get",
            url: "IssueTracker/IssueTransaction/GetById?issueTransactionId=" + id
        }).then(function successCallback(response) {
            $scope.issueStandards = response.data;
          
            $scope.issueAuditNew.Issue = $scope.issueStandards[0].Issue;
            $scope.issueAuditNew.IssueCategory = $scope.issueStandards[0].IssueCategory;
            $scope.issueAuditNew.IssueSubCategory = $scope.issueStandards[0].IssueSubCategory;
            $scope.issueAuditNew.IssueImportance = $scope.issueStandards[0].IssueImportance;
            $scope.issueAuditNew.IssueStatus = $scope.issueStandards[0].IssueStatus;
            $scope.issueAuditNew.BuyerName = $scope.issueStandards[0].BuyerName;
            $scope.issueAuditNew.OverdueDays = $scope.issueStandards[0].OverdueDays;
            $scope.issueAuditNew.StatusUpdateInterval = $scope.issueStandards[0].StatusUpdateInterval;
            $scope.issueAuditNew.CostCenterId = $scope.issueStandards[0].CostCenterId;
            $scope.issueAuditNew.CostCenter = $scope.issueStandards[0].CostCenter;
            $scope.issueAuditNew.IssueTransactionId = id;
            
            
        });
    }
    
    $scope.Save = function () {
             if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.issueAuditDetailList,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.issueAudits[$scope.index] = $scope.issueAudit;
                            $scope.issueAudits = $filter('orderBy')($scope.issueAudits, 'Sequence');
                        }
                       
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
    };


    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExpensesDocument + '/' + data.Id + extention;
    };

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };
    
    $scope.AddRow = function () {
        
        try {
            angular.copy($scope.issueAuditNew, $scope.issueAudit);
           
            var formdata = new FormData();
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;

            $scope.issueAudit.Attachment = fileName;
            if (!baseService.isUndefinedOrNull($scope.issueAudit.Attachment)) {
                if ($scope.issueAudit.Attachment.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }

            //if ($scope.action == "save") {
            $http({
                method: 'post',
                url: 'issueTracker/issueAudit/CreateIssueAudit',
                headers: { 'content-type': undefined },
                transformRequest: function (data) {
                    formdata.append("issueAudit", angular.toJson($scope.issueAudit));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formdata.append('file', data.file);
                    }
                    return formdata;
                },
                
                data: { 'issueAudit': $scope.issueAudit, 'file': $scope.filedata }
            }).then(function successcallback(response) {
                if (response.data.error === true) {
                    $scope.btndisable = false;
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getIssueAuditByIssueTransactionId($scope.issueAudit.IssueTransactionId);
                    
                }
            }, function errorcallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
           
        } catch (e) {
            showresult(e, "failure");
        }
    };

    //$scope.addrow = function () {
    //    angular.copy($scope.issueAuditnew, $scope.issueAudit);
    //    $http({
    //        method: 'post',
    //        url: 'issuetracker/issueAudit/CreateissueAudit',
    //        data: $scope.issueAudit,
    //        datatype: 'json'
    //    }).then(function successcallback(response) {
    //        if (response.data.error === true) {
    //            showresult(response.data.message, 'failure');
    //        }
    //        else {
    //            showresult(response.data.message, 'success');
    //            if ($scope.index > -1) {
    //                $scope.issueAudits[$scope.index] = $scope.issueAudit;
    //                $scope.issueAudits = $filter('orderby')($scope.issueAudits, 'sequence');
    //            }
    //            //$scope.clear();
    //        }
    //    });

    //    $scope.issueAuditdetaillist.push($scope.issueAuditnew)
    //};

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.issueAuditNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.issueAuditNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.issueAudits.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.searchEmployeeByList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'Middle Name',
            'value': 'MiddleName'
        },
        {
            'name': 'Last Name',
            'value': 'LastName'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Employment Type',
            'value': 'EmploymentType'
        },
        {
            'name': 'Status',
            'value': 'EmployeeStatus'
        }
    ];

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


    $scope.showEmployeeListPopUp = function (index) {
        baseService.setCurrentPage('employeeList');
        $scope.AuditDetailindex = index;
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };
    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };
    $scope.showIssueAuditDetailListPopUp = function (index, issueAuditId) {
        $scope.issueAuditId = issueAuditId;
        angular.element(document.querySelector('#issueAuditDetailPopUp')).modal('show');
        $scope.hideEmployeePopUp();
    };
    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#issueAuditDetailPopUp")).modal("hide");
    };

    $scope.closeIssueAuditDetailPopUp = function () {
       
        $scope.hideIssueAuditDetailPopUp();
    };
    $scope.hideIssueAuditDetailPopUp = function () {
        angular.element(document.querySelector("#issueAuditDetailPopUp")).modal("hide");
    };


    $scope.issueAuditDetailList = [];
    $scope.closeEmployeePopUp = function () {
        $scope.employeeinfo = {};
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            employee.IssueAuditId = $scope.issueAuditId;
            employee.EmployeeId = employee.SystemId;
            $scope.issueAuditDetailList.push(employee);
        }
        $scope.hideEmployeePopUp();
    };

    $scope.closeemployeepopup = function () {
        $scope.employeeinfo = {};
        if ($scope.employeeindex !== -1) {
            var employee = $scope.employeelist[$scope.employeeindex];
            employee.issueAuditid = $scope.issueAuditid;
            employee.employeeid = employee.systemid;
            $scope.issueAuditdetaillist.push(employee);
        }
        $scope.hideemployeepopup();
    };


    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.issueAudit = {};
        $scope.issueAuditNew = {};
        $scope.taskTypeNew.Active = true;
        $scope.issueAuditNew.Sequence = seq;
    }

    $scope.getIssueTransaction = function () {
        $http({
            method: "get",
            url: "IssueTracker/IssueTransaction/GetCbo"
        }).then(function successCallback(response) {
            $scope.issueTransactionlist = response.data;
            });
        
    }
    $scope.getIssueTransaction();
}
    
