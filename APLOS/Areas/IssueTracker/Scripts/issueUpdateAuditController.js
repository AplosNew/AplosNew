'use strict';
issueUpdateAuditController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function issueUpdateAuditController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'issueUpdateAudit';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.issueUpdateAudits = [];
    $scope.path = 'issueTracker/issueUpdateAudit/';
    $scope.getListUrl = $scope.path + 'GetList';

    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'InsertIssueRefDetail';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    baseService.init('issueTracker/IssueTransaction/GetListIssueTransaction', null, null, "DESC", "Id", "Id");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.issueUpdateAudits = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.issueUpdateAudit = {
        Id: null,
        IssueTransactionId: null,
        IssueUpdateAuditTime: null,
        Remarks: null,
        OnSchedul: null,
        Attachment: null,
        IssueUpdateAuditDetail: null,
    
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
        OnSchedulList: null,
        EmployeeId: null,

        IsUpdateApplicable: false,
        IsUpdateRecurring: false,
        UpdateFrequencyType: null,
        UpdateFrequencyDays: null,
        UpdateEndDateTime: null,
        UpdateResponsiblePersonId: null,
        UpdateOneTimeDateTime: null,
        DueDate:null
        
    };

    $scope.OnSchedulList = ['Yes', 'No' ];
    $scope.issueUpdateAuditNew = Object.assign({}, $scope.issueUpdateAudit);

    $scope.getIssueUpdateAuditDetail = function (id) {
        $http({
            method: "get",
            url: "IssueTracker/IssueUpdateAudit/GetById?issueUpdateAuditId=" + id
        }).then(function successCallback(response) {
            $scope.issueRefDetailList = response.data;
        });
    }
   // $scope.getIssueRefDetail();
    $scope.issueUpdateAuditList = [];
    $scope.getIssueUpdateAuditByIssueTransactionId = function (issueTransactionId) {
        $http({
            method: "get",
            url: "IssueTracker/IssueUpdateAudit/GetIssueUpdateAuditByIssueTransactionId?issueTransactionId=" + issueTransactionId
        }).then(function successCallback(response) {
            $scope.issueUpdateAuditList = response.data;
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
       

    ];
    $scope.Get = function (id, index) {
       // $scope.Clear();
        $scope.index = index;
        $scope.ChangeIssueStandard(id);
        $scope.getIssueUpdateAuditByIssueTransactionId(id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.issueUpdateAuditNew.IssueTransactionId = null;
    $scope.ChangeIssueStandard = function (id) {
        $http({
            method: "get",
            url: "IssueTracker/IssueTransaction/GetById?issueTransactionId=" + id
        }).then(function successCallback(response) {
            $scope.issueStandards = response.data;
          
            $scope.issueUpdateAuditNew.Issue = $scope.issueStandards[0].Issue;
            $scope.issueUpdateAuditNew.IssueCategory = $scope.issueStandards[0].IssueCategory;
            $scope.issueUpdateAuditNew.IssueSubCategory = $scope.issueStandards[0].IssueSubCategory;
            $scope.issueUpdateAuditNew.IssueImportance = $scope.issueStandards[0].IssueImportance;
            $scope.issueUpdateAuditNew.IssueStatus = $scope.issueStandards[0].IssueStatus;
            $scope.issueUpdateAuditNew.BuyerName = $scope.issueStandards[0].BuyerName;
            $scope.issueUpdateAuditNew.FinalStatus = $scope.issueStandards[0].FinalStatus;
            $scope.issueUpdateAuditNew.OverdueDays = $scope.issueStandards[0].OverdueDays;
            $scope.issueUpdateAuditNew.StatusUpdateInterval = $scope.issueStandards[0].StatusUpdateInterval;
            $scope.issueUpdateAuditNew.CostCenterId = $scope.issueStandards[0].CostCenterId;
            //$scope.issueUpdateAuditNew.CostCenter = $scope.issueStandards[0].CostCenter;
            $scope.issueUpdateAuditNew.IssueTransactionId = id;
            
            
        });
    }
    
    $scope.Save = function () {
             if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.issueUpdateAuditDetailList,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.issueUpdateAudits[$scope.index] = $scope.issueUpdateAudit;
                            $scope.issueUpdateAudits = $filter('orderBy')($scope.issueUpdateAudits, 'Sequence');
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
            angular.copy($scope.issueUpdateAuditNew, $scope.issueUpdateAudit);
           
            var formdata = new FormData();
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;

            $scope.issueUpdateAudit.Attachment = fileName;
            if (!baseService.isUndefinedOrNull($scope.issueUpdateAudit.Attachment)) {
                if ($scope.issueUpdateAudit.Attachment.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }

            //if ($scope.action == "save") {
            $http({
                method: 'post',
                url: 'issueTracker/issueUpdateAudit/CreateIssueUpdateAudit',
                headers: { 'content-type': undefined },
                transformRequest: function (data) {
                    formdata.append("issueUpdateAudit", angular.toJson($scope.issueUpdateAudit));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formdata.append('file', data.file);
                    }
                    return formdata;
                },
                
                data: { 'issueUpdateAudit': $scope.issueUpdateAudit, 'file': $scope.filedata }
            }).then(function successcallback(response) {
                if (response.data.error === true) {
                    $scope.btndisable = false;
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getIssueUpdateAuditByIssueTransactionId($scope.issueUpdateAudit.IssueTransactionId);
                    
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
    //    angular.copy($scope.issuerefnew, $scope.issueref);
    //    $http({
    //        method: 'post',
    //        url: 'issuetracker/issueref/Createissueref',
    //        data: $scope.issueref,
    //        datatype: 'json'
    //    }).then(function successcallback(response) {
    //        if (response.data.error === true) {
    //            showresult(response.data.Message, 'failure');
    //        }
    //        else {
    //            showresult(response.data.Message, 'success');
    //            if ($scope.index > -1) {
    //                $scope.issuerefs[$scope.index] = $scope.issueref;
    //                $scope.issuerefs = $filter('orderby')($scope.issuerefs, 'sequence');
    //            }
    //            //$scope.clear();
    //        }
    //    });

    //    $scope.issuerefdetaillist.push($scope.issuerefnew)
    //};

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.issueUpdateAuditNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.issueUpdateAuditNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.issueUpdateAudits.splice($scope.index, 1);
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
        $scope.refDetailindex = index;
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
    $scope.selectedEmployee = null;
    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };
    $scope.showIssueUpdateAuditPopUp = function (index, issueUpdateAuditId) {
        $scope.issueUpdateAuditId = issueUpdateAuditId;
        angular.element(document.querySelector('#issueUpdateAuditPopUp')).modal('show');
        $scope.hideEmployeePopUp();
    };
    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#issueUpdateAuditDetailPopUp")).modal("hide");
    };

    $scope.closeIssueRefDetailPopUp = function () {
       
        $scope.hideIssueRefDetailPopUp();
    };
    $scope.hideIssueRefDetailPopUp = function () {
        angular.element(document.querySelector("#issueRefDetailPopUp")).modal("hide");
    };


    $scope.issueRefDetailList = [];
    $scope.closeEmployeePopUp = function () {
        $scope.employee = {};

        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];

           var getRow = $filter("filter")($scope.issueRefDetailList, { "EmployeeId": employee.SystemId });
            if (getRow.length === 0) {
                employee.IssueUpdateAuditId = $scope.issueUpdateAuditId;
                employee.EmployeeId = employee.SystemId;
                $scope.issueRefDetailList.push(employee);
            }
            else {
                ShowResult("This Employee is already added!", "failure", "issueRefDetailPopUp");
            }
           
            //alert(issueRefDetailList.length);
        }
        $scope.hideEmployeePopUp();
    };

    //$scope.issueRefDetailList = [];
    
    //$scope.closeEmployeePopUp = function () {
    //        $scope.employee = {};
    //    if ($scope.employeeIndex !== -1)
    //    {
           
    //            var employee = $scope.employeeList[$scope.employeeIndex];
    //            employee.IssueRefId = $scope.issueRefId;
    //            employee.EmployeeId = employee.SystemId;

    //            $scope.issueRefDetailList.push(employee);
    //            alert('Data added to list');
           
                
    //    }
           
    //        $scope.hideEmployeePopUp();
        
       
    //};


    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.issueUpdateAudit = {};
        $scope.issueUpdateAuditNew = {};
        $scope.taskTypeNew.Active = true;
        $scope.issueUpdateAuditNew.Sequence = seq;
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
    
