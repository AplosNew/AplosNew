'use strict';
issueTransactionController.$inject = ['cboService', 'commonMessage', '$window', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'fileReader'];
function issueTransactionController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, fileReader) {


    $rootScope.title = 'Issue Transaction';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerIndex = -1;
    $scope.issueSubTaskResponsiblePersonIndex = -1;
    $scope.colorOfReleseButton = 'red';


    $scope.selectedBuyer = null;
    $scope.issueTransactions = [];
    $scope.path = 'issueTracker/IssueTransaction/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'IssueTransactionCreate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.IssueTransactionId = null;
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    var frequencyError = false;
    var issueId = null;
    $scope.updateFlog = false;
    $scope.followUpFlog = false;
    $scope.internalFlog = false;
    $scope.externalFlog = false;
    $scope.isPermitToRelease = false;

    $scope.isSubTaskDisabled = true;
    $scope.disable = false;

    $scope.searchByList = [
        {
            "name": "Issue",
            "value": "Issue"
        },
        {
            "name": "Issue Detail",
            "value": "IssueDetail"
        },
        {
            "name": "Issue Category",
            "value": "IssueCategory"
        }
        ,
        {
            "name": "IssueS SubCategory",
            "value": "IssueSubCategory"
        },
        {
            "name": "Issue Importance",
            "value": "IssueImportance"
        },
        {
            "name": "Buyer Name",
            "value": "BuyerName"
        }
    ];

    baseService.init("issueTracker/issueTransaction/getlist", null, null, null, "Issue", "Issue");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.issueTransactions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.issueTransactionNew = {
        Id: null,
        Issue: null,
        IssueDate: $filter("dateFiltering")(Date.now()),
        IssueType: null,
        IssueDetail: null,
        IssueCurrentStatus: null,
        ObservedBy: null,
        Mentor: null,
        MentorId: null,
        Remarks: null,
        RequiredDate: null,
        TargetDate: null,
        RevisedTargetDate: null,
        OverdueDays: null,
        FollowupAuditBy: null,
        CloseBy: null,
        CloseDate: null,
        ArchiveBy: null,
        Archive: null,
        BuyerName: null,
        BuyerId: null,
        IssueStandardId: null,
        IssueCategoryId: null,
        IssueSubCategoryId: null,
        FinalStatus: 'ToStart',
        IssueImportanceId: null,
        AssignToId: null,
        AssignToName: null,
        //AuthorisedPersonId: null,
        AssignById: null,
        //AuthorisedPersonName: null,
        AssignBy: null,
        CostCenterId: null,
        Active: null,
        StoryPoint: 0,
        Customer: null,
        CustomerId: null,
        Code: null,
        SortName: null,
        UserName: null,
        StandardName: null,

        //update
        IsUpdateApplicable: false,
        IsUpdateRecurring: false,
        UpdateAuditTaskSchedulerMasterId: null,
        UpdateAuditScheduleMessage: '',
        UpdateResponsiblePersonId: null,
        UpdateOneTimeDateTime: null,

        //FollowUp Audit 
        IsFollowUpApplicable: false,
        IsFollowUpRecurring: false,
        FollowUpAuditTaskSchedulerMasterId: null,
        FollowUpAuditScheduleMessage: '',
        FollowUpResponsiblePersonId: null,
        FollowUpOneTimeDateTime: null,


        //Internal Audit By
        IsInternalApplicable: false,
        IsInternalRecurring: false,
        InternalAuditTaskSchedulerMasterId: null,
        InternalAuditScheduleMessage: '',
        InternalResponsiblePersonId: null,
        InternalOneTimeDateTime: null,

        IsExternalApplicable: false,
        IsExternalRecurring: false,
        ExternalAuditTaskSchedulerMasterId: null,
        ExternalAuditScheduleMessage: '',
        ExternalResponsiblePersonId: null,
        ExternalOneTimeDateTime: null,

        ExternalResponsiblePerson: null,
        ExternalRespPersonEmail: null,
        ExternalRespPersonDesignation: null,
        IsReleased: false,
        IssueGroupName: null,
        IssueGroupId: null,
        TaskCategoryId: null,
        TaskSubCategoryId: null,
        CostIfAny: 0,
        ExpiryDate: null,
        IsExpiry: false,
        CurrencyId: null
    };


    $scope.issueTransactionNew.AssignBy = null;
    $scope.getUser = function () {
        $scope.issueTransactionNew.AssignBy = $window.employeeName;
        $scope.issueTransactionNew.AssignById = $window.employeeId;
    }
    $scope.getUser();

    $scope.issueRef = {

        //Id: null,
        IssueTransactionId: null,
        IssueRefTime: null,
        IsUpdateApplicable: false,
        IsUpdateRecurring: false,
        UpdateResponsiblePersonId: null,
        UpdateOneTimeDateTime: null,
        DueDate: null
    };
    $scope.issueAudit = {
        //Id: null,
        IssueTransactionId: null,
        IssueRefTime: null,


        IsInternalApplicable: false,
        IsInternalRecurring: false,

        InternalResponsiblePersonId: null,
        InternalOneTimeDateTime: null,
        DueDate: null
    };

    $scope.issueSubTaskNew = {
        Id: null,
        IssueTransactionId: null,
        RequiredDate: new Date(),
        TaskDetail: null,
        IsDone: false,
        ResponsiblePersonId: null,
        Remarks: null,
        ResponsiblePerson: null
    };
    $scope.issueFollowUpResponsible = {
        //Id: null,
        IssueTransactionId: null,
        IssueRefTime: null,

        IsFollowUpApplicable: false,
        IsFollowUpRecurring: false,

        FollowUpResponsiblePersonId: null,
        FollowUpOneTimeDateTime: null,
        DueDate: null
    };

    $scope.issueExternalAudit = {
        //Id: null,
        IssueTransactionId: null,
        IssueRefTime: null,

        IsExternalApplicable: false,
        IsExternalRecurring: false,

        ExternalResponsiblePersonId: null,
        ExternalOneTimeDateTime: null,
        DueDate: null
    };


    $scope.costCenterList = [];
    $scope.CostCenterLoad = function () {
        cboService.getCostCenterCbo(function (result) {
            $scope.costCenterList = result;
        });
    }
    $scope.CostCenterLoad();


    $scope.taskCategoryList = [];
    $scope.getTaskCategoryCbo = function () {
        $http({
            method: 'GET',
            url: 'issueTracker/IssueTransaction/GetTaskCategory',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //sccuess 
                $scope.taskCategoryList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    $scope.getTaskCategoryCbo();

    $scope.taskSubCategoryList = [];
    $scope.getTaskSubCategoryCbo = function () {
        $http({
            method: 'GET',
            url: 'issueTracker/IssueTransaction/GetTaskSubCategory',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.taskSubCategoryList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    $scope.getTaskSubCategoryCbo();


    $scope.issueStandardList = [];
    cboService.getIssueStandardCbo(function (result) {
        $scope.issueStandardList = result;
    });

    $scope.issueImportanceList = [];
    cboService.getIssueImportanceCbo(function (result) {
        $scope.issueImportanceList = result;
    });

    $scope.taskManagerSubTasks = [];
    $scope.getTaskManagerSubTasksByIssueTransactionId = function () {
        $http({
            method: 'GET',
            url: 'issueTracker/IssueTransaction/GetTaskManagerSubTasksByIssueTransactionId?issueTransactionId=' + $scope.issueTransactionNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                $scope.taskManagerSubTasks = response.data;

            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.auditIds = [];
    $scope.Get = function (id, index) {
        $scope.taskManagerSubTasks = [];
        $scope.ListOfAuditOfReleasedIssueTransaction = [];
        $scope.isSubTaskDisabled = false;
        $scope.index = index;
        $scope.issueTransactionNew.AssignBy = $window.employeeName;
        $scope.issueTransactionNew.AssignById = $window.employeeId;

        $scope.issueTransactionNew = $scope.issueTransactions[$scope.index];

        $scope.IsReleased = $scope.issueTransactionNew.IsReleased;
        if ($scope.issueTransactionNew.IsReleased == true) {
            $scope.colorOfReleseButton = 'green';
        }
        else {
            $scope.colorOfReleseButton = 'red';
        }

        // $scope.issueTransactionNew.TargetDate = $filter("dateFiltering")($scope.issueTransactionNew.TargetDate);
        $scope.issueTransactionNew.IssueDate = $filter("dateFiltering")($scope.issueTransactionNew.IssueDate);
        $scope.issueTransactionNew.RequiredDate = $filter("dateFiltering")($scope.issueTransactionNew.RequiredDate);
        $scope.issueTransactionNew.CloseDate = $filter("dateFiltering")($scope.issueTransactionNew.CloseDate);
        //$scope.issueTransactionNew.RevisedTargetDate = $filter("dateFiltering")($scope.issueTransactionNew.RevisedTargetDate);
        $scope.issueTransactionNew.UpdateEndDateTime = $filter("dateFiltering")($scope.issueTransactionNew.UpdateEndDateTime);
        $scope.issueTransactionNew.UpdateOneTimeDateTime = $filter("dateFiltering")($scope.issueTransactionNew.UpdateOneTimeDateTime);
        $scope.issueTransactionNew.FollowUpEndDateTime = $filter("dateFiltering")($scope.issueTransactionNew.FollowUpEndDateTime);
        $scope.issueTransactionNew.FollowUpOneTimeDateTime = $filter("dateFiltering")($scope.issueTransactionNew.FollowUpOneTimeDateTime);
        $scope.issueTransactionNew.InternalEndDateTime = $filter("dateFiltering")($scope.issueTransactionNew.InternalEndDateTime);
        $scope.issueTransactionNew.InternalOneTimeDateTime = $filter("dateFiltering")($scope.issueTransactionNew.InternalOneTimeDateTime);
        $scope.issueTransactionNew.ExternalEndDateTime = $filter("dateFiltering")($scope.issueTransactionNew.ExternalEndDateTime);
        $scope.issueTransactionNew.ExternalOneTimeDateTime = $filter("dateFiltering")($scope.issueTransactionNew.ExternalOneTimeDateTime);
        $scope.issueTransactionNew.ExpiryDate = $filter("dateFiltering")($scope.issueTransactionNew.ExpiryDate);

        if ($scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId != null) {
            $scope.auditIds.push($scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId);
            $scope.GetRecurringData($scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId);

        }
        if ($scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId != null) {
            $scope.auditIds.push($scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId);
            $scope.taskSchedule = $scope.GetRecurringData($scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId);

        }
        if ($scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId != null) {
            $scope.auditIds.push($scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId);
            $scope.taskSchedule = $scope.GetRecurringData($scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId);

        }
        if ($scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId != null) {
            $scope.auditIds.push($scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId);
            $scope.taskSchedule = $scope.GetRecurringData($scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId);

        }
        $scope.LoadIssueDocumentsData($scope.issueTransactionNew.Id);
        $scope.setEnableDate();
        $scope.GetCboParallelCurrency();
        //just for test affter test it getRecurringDataForEveryAuditTaskSchedulerMasterId will be removed
        //$scope.getRecurringDataForEveryAuditTaskSchedulerMasterId($scope.auditIds);

        //var arr = document.getElementsByClassName("help-block");
        //if (arr.length > 0)
        //{
        //    alert(arr.length);


        //    for (var i = 0; i < arr.length; i++)
        //    {
        //        arr[i].remove()//.style.display = "none";
        //    }
        //}

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.ChangeIssueStandard = function (id) {
        $http({
            method: "get",
            url: "IssueTracker/IssueStandard/GetById?issueStandardId=" + id
        }).then(function successCallback(response) {
            $scope.issueStandards = response.data;
            $scope.issueTransactionNew.IssueCategoryId = $scope.issueStandards[0].IssueCategoryId;
            $scope.issueTransactionNew.IssueSubCategoryId = $scope.issueStandards[0].IssueSubCategoryId;
            $scope.issueTransactionNew.IssueImportanceId = $scope.issueStandards[0].IssueImportanceId;

            $scope.issueTransactionNew.Issue = $scope.issueStandards[0].Issue;
            //$scope.issueTransactionNew.BuyerName = $scope.issueStandards[0].BuyerName;
            $scope.issueTransactionNew.StatusUpdateInterval = $scope.issueStandards[0].StatusUpdateInterval;
            $scope.issueTransactionNew.OverdueDays = $scope.issueStandards[0].OverdueDays;
            //$scope.issueTransactionNew.InternalAuditLagDay = $scope.issueStandards[0].InternalAuditLagDay;
            $scope.issueTransactionNew.Remarks = $scope.issueStandards[0].Remarks;
            $scope.issueTransactionNew.IssueDetail = $scope.issueStandards[0].IssueDetail;
            //$scope.issueTransactionNew.BuyerId = $scope.issueStandards[0].BuyerId;
            $scope.issueTransactionNew.UserName = $scope.issueStandards[0].UserName;
            $scope.issueTransactionNew.Code = $scope.issueStandards[0].Code;
            $scope.issueTransactionNew.SortName = $scope.issueStandards[0].SortName;
            $scope.issueTransactionNew.StandardName = $scope.issueStandards[0].StandardName;
            $scope.issueTransactionNew.TaskCategoryId = $scope.issueStandards[0].TaskCategoryId;
            $scope.issueTransactionNew.TaskSubCategoryId = $scope.issueStandards[0].TaskSubCategoryId;
        });
    }

    $scope.ChangeIssueStatus = function (IssueType) {
        $scope.standardStatus = IssueType;

        if (IssueType == 'New') {
            $scope.colorSet = '';
            $scope.issueTransactionNew.IssueStandardId = null;
            $scope.issueTransactionNew.TaskCategoryId = null;
            $scope.issueTransactionNew.TaskSubCategoryId = null;
            $scope.issueTransactionNew.IssueImportanceId = null;

            $scope.issueTransactionNew.Issue = null;
            $scope.issueTransactionNew.BuyerName = null;
            $scope.issueTransactionNew.StatusUpdateInterval = null;
            $scope.issueTransactionNew.OverdueDays = null;
            //$scope.InternalAuditLagDay.InternalAuditLagDay = null;
            $scope.issueTransactionNew.Remarks = null;
            $scope.issueTransactionNew.Remarks = null;
            $scope.issueTransactionNew.IssueDetail = null;
        }
        else {
            $scope.colorSet = '#f2f6f6';
        }
    }
    //BuyerPopup
    $scope.buyerList = [];

    $scope.searchByBuyerList = [
        {
            'name': 'Buyer',
            'value': 'UserName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        }
    ];
    $scope.buyerParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showBuyerListPopUp = function () {
        // baseService.setCurrentPage('buyerList');
        $scope.getBuyerData = function (pageno) {
            var url = null;
            url = 'IssueTracker/IssueTransaction/BuyerList';
            baseService.paginationBase(url, pageno, $scope.buyerParameters)
                .then(function (result) {
                    $scope.buyerList = result.Rows;
                    $scope.buyerParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#buyerPopUp')).modal('show');
        $scope.getBuyerData();
    };

    $scope.buyerCloseListPopUp = function (data) {
        $scope.issueTransactionNew.BuyerId = data.BuyerId;

        $scope.issueTransactionNew.BuyerName = data.BuyerName;
        angular.element(document.querySelector('#buyerSearchModal')).modal('hide');
    };

    $scope.SaveIssueSubTask = function () {
        if (baseService.isUndefinedOrNull($scope.issueSubTaskNew.Id)) {
            $scope.issueSubTaskNew.ResponsiblePersonId = $scope.issueTransactionNew.AssignToId;
            $scope.issueSubTaskNew.IssueTransactionId = $scope.issueTransactionNew.Id;
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.issueTransactionSubTaskForm.$valid == false)
                return;
            $http({
                method: 'POST',
                url: 'issueTracker/IssueSubTask/Create',
                data: { model: $scope.issueSubTaskNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    //angular.element(document.querySelector('#issueSubTaskPopUp')).modal('hide');
                    $scope.getTaskManagerSubTasksByIssueTransactionId();
                    $scope.clearIssueSubTask();
                    //$scope.AddSubTaskconfirmRelease();
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } else {
            $scope.UpdateIssueSubTask();
        }

    }

    $scope.clearIssueSubTask = function () {
        $scope.issueSubTaskNew.IssueTransactionId = null;
        $scope.issueSubTaskNew.Id = null;
        $scope.issueSubTaskNew.IssueSubTaskResponsiblePerson = null;
        $scope.issueSubTaskNew.TaskDetail = null;
        $scope.issueSubTaskNew.Remarks = null;
        $scope.issueSubTaskNew.RequiredDate = new Date();
        $scope.issueSubTaskNew.ResponsiblePersonId = null;
        $scope.issueSubTaskNew.IsDone = null;
    }
    $scope.saveFlag = false;

    $scope.ExpiryDateFlag = true;
    $scope.setEnableDate = function () {
        if ($scope.issueTransactionNew.IsExpiry === true) {
            $scope.ExpiryDateFlag = false;
        } else {
            $scope.ExpiryDateFlag = true;
            $scope.issueTransactionNew.ExpiryDate = null;
        }
    };

    $scope.currency = null;
    $scope.GetCboParallelCurrency = function () {
        cboService.getCboParallelCurrency(function (result) {
            $scope.tranCurrencyList = result;
            if ($scope.tranCurrencyList.length == 1) {
                $scope.issueTransactionNew.CurrencyId = $scope.tranCurrencyList[0].CurrencyId;
                $scope.currency = $scope.tranCurrencyList[0].Text;
            }
        });
    }
    $scope.GetCboParallelCurrency();

    $scope.ValidateDate = function () {
        var fd = $filter('dateFiltering')(new Date(), 'dd-MM-yyyy');
        var td = $filter('dateFiltering')($scope.issueTransactionNew.ExpiryDate, 'dd-MM-yyyy');

        if (new Date(fd) > new Date(td)) {
            throw 'Back date is not allowed for Expiry Date.';
        }
    };

    //new to singletransaction
    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.issueTransactionNewForm.$valid && !$scope.checkEmailValidityWhenSave() && !$scope.checkAuditApplicableNullValidity() && !$scope.checkIssueCloseDate() && !$scope.checkIssueRequiredDate()) {
                $scope.savebtndisable = true;
                if ($scope.issueTransactionNew.IsExpiry === true) {
                    if (baseService.isUndefinedOrNull($scope.issueTransactionNew.ExpiryDate)) {
                        throw "Expiry Date is required.";
                    }
                    //$scope.ValidateDate();
                }
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { issueTransactionNew: $scope.issueTransactionNew, buyers: $scope.buyers },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.savebtndisable = false;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.isPermitToRelease = true;
                            $scope.issueTransactionNew.Id = response.data.IssueTransaction.Id;
                            $scope.issueTransactionNew.IsReleased = response.data.IssueTransaction.IsReleased;
                            $scope.Action = 'Update';
                            $scope.AddSubTaskconfirmRelease();
                            //$scope.disable = false;
                            $scope.isSubTaskDisabled = false;
                            $scope.getData();
                            $scope.savebtndisable = false;
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.saveBuyers = function (IssueTransactionId) {

        if ($scope.buyers.length > 0) {
            $scope.buyers[i].IssueTransactionId = IssueTransactionId;
        }

        $http({
            method: 'POST',
            url: 'issueTracker/IssueTransaction/CreateBuyers',
            data: { buyers: $scope.buyers },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
    }

    $scope.Update = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.issueTransactionNewForm.$valid && !$scope.checkEmailValidityWhenSave() && !$scope.checkAuditApplicableNullValidity()) {
                $scope.savebtndisable = true;
                if ($scope.issueTransactionNew.IsExpiry === true) {
                    if (baseService.isUndefinedOrNull($scope.issueTransactionNew.ExpiryDate)) {
                        throw "Expiry Date is required.";
                    }
                    $scope.ValidateDate();
                }
                if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: 'IssueTracker/IssueTransaction/edit',
                        data: $scope.issueTransactionNew,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.savebtndisable = false;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.disable = false;
                            //$scope.confirmRelease();
                            $scope.isSubTaskDisabled = false;
                            $scope.IssueTransactionId = response.data.IssueTransaction.Id;
                            $scope.issueTransactionNew.IsReleased = response.data.IssueTransaction.IsReleased;
                            $scope.savebtndisable = false;
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    var assignIssueRef = function () {
        if ($scope.issueTransactionNew.IsUpdateApplicable == true) {

            //IssueRefTime = null;
            if ($scope.issueTransactionNew.IsUpdateRecurring == true) {
                $scope.issueRef.IsUpdateApplicable = $scope.issueTransactionNew.IsUpdateApplicable;
                $scope.issueRef.IsUpdateRecurring = $scope.issueTransactionNew.IsUpdateRecurring;

                $scope.issueRef.UpdateFrequencyType = $scope.issueTransactionNew.UpdateFrequencyType;
                $scope.issueRef.UpdateFrequencyDays = $scope.issueTransactionNew.UpdateFrequencyDays;

                $scope.issueRef.UpdateEndDateTime = $scope.issueTransactionNew.UpdateEndDateTime;
                $scope.issueRef.UpdateResponsiblePersonId = $scope.issueTransactionNew.UpdateResponsiblePersonId;

                //UpdateOneTimeDateTime = $scope.issueTransactionNew.UpdateOneTimeDateTime;
                $scope.issueRef.DueDate = $filter("dateFiltering")(Date.now()); //$scope.issueTransactionNew.DueDate;
            }
            else {
                $scope.issueRef.UpdateResponsiblePersonId = $scope.issueTransactionNew.UpdateResponsiblePersonId;
                $scope.issueRef.UpdateOneTimeDateTime = $scope.issueTransactionNew.UpdateOneTimeDateTime;
                $scope.issueRef.DueDate = $filter("dateFiltering")(Date.now()); //$scope.issueTransactionNew.DueDate;
            }

        }
        return;
    }


    $scope.AddSubTaskconfirmRelease = function () {
        $scope.message_confirmation = "Are you sure to Add Sub task?";
        angular.element(document.querySelector("#AddSubtaskconfirmPostPopUp")).modal("show");

    };

    $scope.AddSubTaskWhenUpdateconfirmRelease = function () {
        $scope.message_confirmation = "Are you sure to Add Sub task?";
        angular.element(document.querySelector("#AddSubtaskWhenUpdateconfirmPostPopUp")).modal("show");
    };

    $scope.AddSubTask = function () {
        $scope.issueSubTaskNew = { RequiredDate:new Date() };
        angular.element(document.querySelector('#issueSubTaskPopUp')).modal('show');
    }


    $scope.confirmRelease = function () {
        $scope.message_confirmation = "Are you sure to Release?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.savebtndisable = false;

    //Check Audits Applicable or Not
    $scope.updateAuditRelesed = false;
    $scope.checkIsUpdateAuditRelesed = function () {
        $http({
            method: 'GET',
            url: 'issueTracker/IssueUpdateAudit/GetIssueTransactionId?issueTransactionId=' + $scope.issueTransactionNew.Id,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //ShowResult('Data is released', 'failure');

                if (response.data != null) {
                    ShowResult('Data already has been released', 'failure');
                    $scope.updateAuditRelesed = true;
                    return true;
                }
                else {
                    return false;
                }
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.checkForRelease = function () {
        if ($scope.issueTransactionNew.IsUpdateApplicable) {
            if ($scope.checkIsUpdateAuditRelesed() == true) {
                return true;
            }
            else {
                return false;
            }
        }
        if ($scope.issueTransactionNew.IsInternalApplicable) {

        }
        if ($scope.issueTransactionNew.IsExternalApplicable) {

        }
        if ($scope.issueTransactionNew.IsFollowUpApplicable) {

        }
    }
    $scope.Release = function () {
        $scope.savebtndisable = true;
        if ($scope.Action !== 'Save') {
            if (!$scope.checkEmailValidityWhenSave() && !$scope.checkAuditApplicableNullValidity()) {
                try {

                    $http({
                        method: 'POST',
                        url: 'issueTracker/IssueTransaction/IssueRelease',
                        data: $scope.issueTransactionNew,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.colorOfReleseButton = 'green';
                            $scope.issueTransactionNew.IsReleased = response.data.IssueTransaction.IsReleased;
                            if (response.data.IsSuccess >= 1) {
                                ShowResult(response.data.Message, 'success');
                                //add subtask to TaskMasterSubTask
                            }
                            else if (response.data.Message != "") {
                                ShowResult(response.data.Message, 'failure');
                            }
                            //$scope.disable = true;
                            $scope.savebtndisable = false;
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                } catch (e) {
                    ShowResult(e, 'failure');
                }
            }
        }
        else {
            ShowResult('Save first before release.', 'failure');
        }
    }

    $scope.Delete = function () {

        if (!baseService.isUndefinedOrNull($scope.issueTransactionNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.issueTransactionNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.issueTransactions.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        var AssignBy = $scope.issueTransactionNew.AssignBy;
        var AssignById = $scope.issueTransactionNew.Id;
        var IssueCreationDate = $scope.issueTransactionNew.IssueDate;
        var FinalStatus = $scope.issueTransactionNew.FinalStatus;
        $scope.Action = 'Save';
        $scope.issueTransaction = {};
        $scope.issueTransactionNew = {};

        $scope.issueTransactionNew.AssignBy = AssignBy;

        $scope.issueTransactionNew.IssueDate = IssueCreationDate;
        $scope.issueTransactionNew.FinalStatus = FinalStatus;
        $scope.issueTransactionNew.Id = null;
        $scope.IssueTransactionId = null;
        //$scope.taskTypeNew.Active = true;
        //scope.issueTransactionNew.Sequence = seq;
        $scope.issueTransactionDocumentList = [];
        $scope.GetCboParallelCurrency();
        $scope.setEnableDate();
        $scope.ClearImage();
    }

    $scope.selectIssueSubTaskResponsiblePersonPopUp = function (index, id) {

        $scope.issueSubTaskResponsiblePersonIndex = index;
        $scope.issueSubTaskResponsiblePersonId = id;
    };
    $scope.closeIssueSubTasResponsiblePersonPopUp = function () {

        if ($scope.issueSubTaskResponsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.issueSubTaskResponsiblePersonIndex];
            $scope.issueSubTaskNew.IssueSubTaskResponsiblePerson = employee.EmployeeName;
            $scope.issueSubTaskNew.ResponsiblePersonId = employee.SystemId;
        }
        $scope.hideIssueTaskResponsiblePersonPopUp();
    };
    $scope.hideIssueTaskResponsiblePersonPopUp = function () {

        angular.element(document.querySelector('#issueSubTaskResponsiblePersonPopUp')).modal('hide');
        $scope.issueSubTaskResponsiblePersonIndex = -1;
        $scope.issueSubTaskResponsiblePersonId = null;
    };

    $scope.hideIssueTaskPopUp = function () {
        angular.element(document.querySelector('#issueSubTaskPopUp')).modal('hide');
        $scope.confirmRelease();
    };

    //UpdateResponsiblePopUp
    $scope.closeUpdateResponsiblePersionPopUp = function () {
        if ($scope.updateResponsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.updateResponsiblePersonIndex];
            $scope.issueTransactionNew.UpdateResponsiblePerson = employee.EmployeeName;
            $scope.issueTransactionNew.UpdateResponsiblePersonId = employee.SystemId;
        }
        $scope.hideEUpdateResponsiblePersionPopUp();
    };
    $scope.hideEUpdateResponsiblePersionPopUp = function () {
        angular.element(document.querySelector("#updateResponsiblePersonPopUp")).modal("hide");
    };
    //FollowUp Responsible PopUp
    $scope.closeFollowUpResponsiblePersionPopUp = function () {
        if ($scope.followUpResponsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.followUpResponsiblePersonIndex];
            $scope.issueTransactionNew.FollowUpResponsiblePerson = employee.EmployeeName;
            $scope.issueTransactionNew.FollowUpResponsiblePersonId = employee.SystemId;
        }
        $scope.hideFollowUpResponsiblePersionPopUp();
    };
    $scope.hideFollowUpResponsiblePersionPopUp = function () {
        angular.element(document.querySelector("#followupResponsiblePersonPopUp")).modal("hide");
    };

    //internal Responsible Popup
    $scope.closeInternalResponsiblePersionPopUp = function () {
        if ($scope.internalResponsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.internalResponsiblePersonIndex];
            $scope.issueTransactionNew.InternalResponsiblePerson = employee.EmployeeName;
            $scope.issueTransactionNew.InternalResponsiblePersonId = employee.SystemId;
        }
        $scope.hideInternalResponsiblePersionPopUp();
    };
    $scope.hideInternalResponsiblePersionPopUp = function () {
        angular.element(document.querySelector("#internalResponsiblePersonPopUp")).modal("hide");
    };

    //External Responsible Popup
    $scope.closeExternalResponsiblePersionPopUp = function () {
        if ($scope.externalResponsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.externalResponsiblePersonIndex];
            $scope.issueTransactionNew.ExternalResponsiblePersonName = employee.EmployeeName;
            $scope.issueTransactionNew.ExternalResponsiblePersonId = employee.SystemId;
        }
        $scope.hideExternalResponsiblePersionPopUp();
    };
    $scope.hideExternalResponsiblePersionPopUp = function () {
        angular.element(document.querySelector("#externalResponsiblePersonPopUp")).modal("hide");
    };

    $scope.selectBuyerPopUp = function (index, id) {
        $scope.buyerIndex = index;
        $scope.selectedBuyer = id;
    };
    $scope.buyers = [];
    $scope.buyerObj = { Id: null, BuyerCode: null, Name: null, BuyerId: null, IssueTransactionId: null };
    $scope.closeBuyerPopUp = function () {
        if ($scope.buyerIndex !== -1) {
            var buyer = $scope.buyerList[$scope.buyerIndex];
            //$scope.issueTransactionNew.BuyerName = buyer.UserName;
            $scope.issueTransactionNew.BuyerId = buyer.Id;
            $scope.buyerObj.Name = buyer.UserName;
            $scope.buyerObj.BuyerId = buyer.Id;
            $scope.buyerObj.BuyerCode = buyer.Code;
            var isRepeted = false;
            if ($scope.buyers.length == 0) {
                $scope.buyers.push($scope.buyerObj);
                $scope.buyerObj = {};

            }
            else {
                for (var i = 0; i < $scope.buyers.length; i++) {
                    if ($scope.buyers[i].BuyerId == $scope.buyerObj.BuyerId) {
                        isRepeted = true;
                        break;
                    }
                }
                if (isRepeted == false) {
                    $scope.buyers.push($scope.buyerObj);
                    $scope.buyerObj = {};
                }

            }

        }
        $scope.hideBuyerPopUp();
    };
    $scope.showSelectedBuyerListPopUp = function () {
        angular.element(document.querySelector('#issueSelectedBuyerListPopUp')).modal('show');
    }
    $scope.issueTransactionNew.BuyerName = "";
    $scope.hideSelectedBuyerListPopUp = function () {
        angular.element(document.querySelector("#issueSelectedBuyerListPopUp")).modal("hide");
        if ($scope.buyers.length > 0) {
            $scope.issueTransactionNew.BuyerName = "Data is Selected";
        }

    };
    $scope.removeBuyer = function (buyerId) {
        for (var i = 0; i < $scope.buyers.length; i++) {
            if ($scope.buyers[i].BuyerId === buyerId) {
                $scope.buyers.splice(i, 1);
            }
        }
    }

    $scope.hideBuyerPopUp = function () {
        angular.element(document.querySelector("#buyerPopUp")).modal("hide");
    };
    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };


    $scope.closeAuthorisedPopUp = function () {
        if ($scope.authorisepersonindex !== -1) {
            var employee = $scope.employeeList[$scope.authorisepersonindex];
            $scope.issueTransactionNew.AssignBy = employee.EmployeeName;
            $scope.issueTransactionNew.AssignById = employee.SystemId;
        }
        $scope.hideAuthorisedPopUp();
    };
    //assign to popup
    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeindex !== -1) {
            var employee = $scope.employeeList[$scope.employeeindex];
            $scope.issueTransactionNew.AssignToName = employee.EmployeeName;
            $scope.issueTransactionNew.AssignToId = employee.SystemId;
            $scope.issueTransactionNew.UpdateResponsiblePerson = employee.EmployeeName;
            $scope.issueTransactionNew.UpdateResponsiblePersonId = employee.SystemId;
        }
        $scope.hideEmployeePopUp();
    };
    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };
    $scope.closeMentorPopUp = function () {
        if ($scope.mentorPersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.mentorPersonIndex];
            $scope.issueTransactionNew.Mentor = employee.EmployeeName;
            $scope.issueTransactionNew.MentorId = employee.SystemId;
        }
        $scope.hideMentorPopUp();
    };

    $scope.hideAuthorisedPopUp = function () {
        angular.element(document.querySelector("#authorisedPersonPopUp")).modal("hide");
    };
    $scope.hideMentorPopUp = function () {
        angular.element(document.querySelector("#mentorPersonPopUp")).modal("hide");
    };

    $scope.clearEmployee = function () {
        $scope.issueTransactionNew.AssignToName = null;
        $scope.issueTransactionNew.AssignToId = null;
    };

    $scope.hideResponsiblePopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };

    $scope.clearAuthorisedEmployee = function () {
        $scope.issueTransactionNew.AssignBy = null;
        $scope.issueTransactionNew.AssignById = null;
    };
    $scope.clearMentorEmployee = function () {
        $scope.issueTransactionNew.Mentor = null;
        $scope.issueTransactionNew.MendorId = null;
    };

    $scope.invalidIssueDate = false;
    var today = new Date();


    //Date validation
    $scope.checkIssueDate = function () {
        var msg = "";
        if ($filter("dateFiltering")($scope.issueTransactionNew.IssueDate) < $filter("dateFiltering")(Date.now())) {
            msg += "Date cannot be past.";
            $scope.invalidIssueDate = true;

        }
        else if ($filter("dateFiltering")($scope.issueTransactionNew.IssueDate) >= $filter("dateFiltering")(Date.now())) {

            msg = "";
            $scope.invalidIssueDate = false;

        }
        return manualValidation("div_IssueDate", $scope.invalidIssueDate, msg);
    };

    $scope.invalidIssueRequiredDate = false;
    $scope.checkIssueRequiredDate = function () {
        var msg = "";
        //if ($filter("dateFiltering")($scope.issueTransactionNew.RequiredDate) < $filter("dateFiltering")(Date.now())) {

        //    msg += "Date cannot be past.";
        //    $scope.invalidIssueRequiredDate = true;

        //}
        //if (new Date($scope.issueTransactionNew.IssueDate) > new Date($scope.issueTransactionNew.RequiredDate))
        if (new Date($scope.issueTransactionNew.IssueDate) > new Date($scope.issueTransactionNew.RequiredDate)) {
            msg += "Require Date must be greater or equel to Issue Date.";
            $scope.invalidIssueRequiredDate = true;

        }
        else if (new Date($scope.issueTransactionNew.RequiredDate) >= new Date(Date.now())) {
            msg = "";
            $scope.invalidIssueRequiredDate = false;

        }
        else {
            msg = "";
            $scope.invalidIssueRequiredDate = false;

        }
        return manualValidation("div_IssueRequireDate", $scope.invalidIssueRequiredDate, msg);
    };

    $scope.invalidIssueCloseDate = false;
    $scope.checkIssueCloseDate = function () {
        var msgCloseDate = "";
        var x = $scope.issueTransactionNew.CloseDate;
        if (new Date($scope.issueTransactionNew.RequiredDate) > new Date(x)) {
            msgCloseDate += "Closing Date should be greater or equel to RequiredDate.";
            $scope.invalidIssueCloseDate = true;
        }
        else {
            msgCloseDate = "";
            $scope.invalidIssueCloseDate = false;
        }
        return manualValidation("div_IssueCloseDate", $scope.invalidIssueCloseDate, msgCloseDate);
    }

    $scope.chkIssueCloseDate = function () {
      
        var x = $scope.issueTransactionNew.CloseDate;
        if (new Date($scope.issueTransactionNew.RequiredDate) > new Date(x)) {
            ShowResult("Closing Date should be greater or equel to RequiredDate.",'failure');
           
        }
        
    }

    $scope.invalidIssueTargetDate = false;
    $scope.invalidIssueRevisedTargetDate = false;

    $scope.isEmailValidate = false;
    $scope.checkEmailValidity = function () {
        if (!baseService.isUndefinedOrNull($scope.issueTransactionNew.ExternalRespPersonEmail)) {
            if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.issueTransactionNew.ExternalRespPersonEmail)) {
                $scope.isEmailValidate = false;

            } else {
                $scope.isEmailValidate = true;
                //ShowResult("You have entered an invalid email address.", 'failure');
            }
        }
    }

    $scope.checkEmailValidityWhenSave = function () {
        $scope.isEmailValidate = false;
        if (!baseService.isUndefinedOrNull($scope.issueTransactionNew.ExternalRespPersonEmail)) {
            if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.issueTransactionNew.ExternalRespPersonEmail)) {
                $scope.isEmailValidate = false;

            } else {
                $scope.isEmailValidate = true;
                ShowResult("You have entered an invalid email address.", 'failure');
            }
        }
        return $scope.isEmailValidate;
    }

    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }
    $scope.checkAuditApplicableNullValidity = function () {
        $scope.isAuditApplicableNull = false;
        $scope.auditApplicableErrorMessage = "";

        if ($scope.issueTransactionNew.IsUpdateApplicable == true) {
            if (nullrecorder($scope.issueTransactionNew.UpdateResponsiblePerson) == "") {
                $scope.isAuditApplicableNull = true;
                $scope.auditApplicableErrorMessage += "ResponsiblePerson Of UpdateAudit cannot be empty ";
            }

            if ($scope.issueTransactionNew.IsUpdateRecurring == true) {

            }
            else {
                if (nullrecorder($scope.issueTransactionNew.UpdateOneTimeDateTime) == "") {
                    $scope.isAuditApplicableNull = true;
                    $scope.auditApplicableErrorMessage += "OneTime Of UpdateAudit cannot be empty. ";
                }
            }
        }

        if ($scope.issueTransactionNew.IsFollowUpApplicable == true) {
            if (nullrecorder($scope.issueTransactionNew.FollowUpResponsiblePerson) == "") {
                $scope.isAuditApplicableNull = true;
                $scope.auditApplicableErrorMessage += "ResponsiblePerson Of FollowUpAudit cannot be empty ";
            }

            if ($scope.issueTransactionNew.IsFollowUpRecurring == true) {

            }
            else {
                if (nullrecorder($scope.issueTransactionNew.FollowUpOneTimeDateTime) == "") {
                    $scope.isAuditApplicableNull = true;
                    $scope.auditApplicableErrorMessage += "OneTime Of FollowUpAudit cannot be empty. ";
                }
            }
        }

        if ($scope.issueTransactionNew.IsInternalApplicable == true) {
            if (nullrecorder($scope.issueTransactionNew.InternalResponsiblePerson) == "") {
                $scope.isAuditApplicableNull = true;
                $scope.auditApplicableErrorMessage += "ResponsiblePerson Of InternalAudit cannot be empty ";
            }

            if ($scope.issueTransactionNew.IsInternalRecurring == true) {

            }
            else {
                if (nullrecorder($scope.issueTransactionNew.InternalOneTimeDateTime) == "") {
                    $scope.isAuditApplicableNull = true;
                    $scope.auditApplicableErrorMessage += "OneTime Of InternalAudit cannot be empty. ";
                }
            }
        }

        if ($scope.issueTransactionNew.IsExternalApplicable == true) {
            if (nullrecorder($scope.issueTransactionNew.ExternalResponsiblePerson) == "") {
                $scope.isAuditApplicableNull = true;
                $scope.auditApplicableErrorMessage += "ResponsiblePerson Of ExternalAudit cannot be empty ";
            }
            if ($scope.issueTransactionNew.IsExternalRecurring == true) {


            }
            else {
                if (nullrecorder($scope.issueTransactionNew.ExternalOneTimeDateTime) == "") {
                    $scope.isAuditApplicableNull = true;
                    $scope.auditApplicableErrorMessage += "OneTime Of ExternalAudit cannot be empty. ";
                }
            }
        }
        //check error and return 
        if ($scope.isAuditApplicableNull == true) {
            //error occured
            ShowResult($scope.auditApplicableErrorMessage, 'failure');
        }
        return $scope.isAuditApplicableNull;
    }



    //Task Scheduler
    $scope.taskSchedule = {
        Id: null,
        RepeatType: "Daily",
        StartDate: new Date(),
        EndDate: new Date(),
        AfterNoOfAccurence: 1,
        EveryInterval: 1,
        RepeatByDayNumber: 1,
        RepeatbyNthWeek: 'First',
        RepeatByMonth: 'January',
        RepeatbyOfEarly: 'January',
        RepeatByWeek: 'Sunday',

        IsAfter: false,
        IsOn: false,
        IsNever: true,
        WeeklyRepeatationBycommaSepDayName: "",
        Details: null,
        isWeekly: false,
        isYearly: false,
        isDaily: true,
        EveryWeekDay: false,

        isRepeatByDay: true,
        isRepeatByTheNthWeekForMonthly: false,

        isRepeatByTheMonth: true,
        isRepeatByTheNthWeekForYearly: false,
        OnPreviousAccomplishment: true
    };
    $scope.AuditSchedulerStatement = {
        RepeatType: '',
        Details: ''
    }
    $scope.dayList = [
        { day: 'Sun', isChecked: false },
        { day: 'Mon', isChecked: false },
        { day: 'Tue', isChecked: false },
        { day: 'Wed', isChecked: false },
        { day: 'Thu', isChecked: false },
        { day: 'Fri', isChecked: false },
        { day: 'Sat', isChecked: false }
    ];

    $scope.EveryRepeatedFlag = $scope.taskSchedule.RepeatType;

    $scope.flagCarringRecurring = '';
    $scope.checkUpdateFrequencyTypeAndDay = function () {

        if ($scope.issueTransactionNew.IsUpdateRecurring) {

            if ($scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId != null) {
                $scope.GetRecurringData($scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId);

            }
            else {
                //$scope.updateTaskScheduleDetails = "";
                $scope.ClearReccuringData();
            }

            $scope.flagCarringRecurring = 'UpdateAudit';
            $scope.showTaskSchedulerPopUp();
        }
        else {
            $scope.flagCarringRecurring = '';
            $scope.updateFlog = false;
        }

    }
    $scope.checkFollowUpFrequencyTypeAndDay = function () {

        if ($scope.issueTransactionNew.IsFollowUpRecurring) {

            if ($scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId != null) {
                $scope.GetRecurringData($scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId);
            }
            else {
                $scope.ClearReccuringData();
            }
            $scope.flagCarringRecurring = 'FollowUpAudit';
            $scope.showTaskSchedulerPopUp();
        }
        else {
            $scope.flagCarringRecurring = '';
            $scope.followUpFlog = false;
        }

    }
    $scope.checkInternalFrequencyTypeAndDay = function () {
        if ($scope.issueTransactionNew.IsInternalRecurring) {

            if ($scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId != null) {
                $scope.GetRecurringData($scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId);
            }
            else {
                $scope.ClearReccuringData();
            }
            $scope.flagCarringRecurring = 'InternalAudit';
            $scope.showTaskSchedulerPopUp();
        }
        else {
            $scope.flagCarringRecurring = '';
            $scope.internalFlog = false;
        }

    }
    $scope.checkExternalFrequencyTypeAndDay = function () {
        if ($scope.issueTransactionNew.IsExternalRecurring) {

            if ($scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId != null) {
                $scope.GetRecurringData($scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId);
            }
            else {
                $scope.ClearReccuringData();
            }
            $scope.flagCarringRecurring = 'ExternalAudit';
            $scope.showTaskSchedulerPopUp();
        }
        else {
            $scope.flagCarringRecurring = '';
            $scope.externalFlog = false;
        }

    }
    $scope.checkSelectedWeeks = function () {
        var x = '';
        if ($scope.dayList.length > 0) {
            for (var i = 0; i < $scope.dayList.length; i++) {
                if ($scope.dayList[i].isChecked == true) {
                    x += $scope.dayList[i].day + ',';
                }

            }
        }
        $scope.taskSchedule.WeeklyRepeatationBycommaSepDayName = x.slice(0, -1);
    }
    $scope.checkRepeatedStatus = function () {

        if ($scope.taskSchedule.RepeatType === 'Daily') {
            //data for dinamic control
            $scope.EveryRepeatedFlag = 'Day';
            $scope.taskSchedule.isWeekly = false;
            $scope.taskSchedule.isYearly = false;
            $scope.taskSchedule.isDaily = true;
            $scope.taskSchedule.EveryWeekDay = false;

            //$scope.taskSchedule.RepeatByDayNumber = null;
            //$scope.taskSchedule.RepeatbyNthWeek = null;
            //$scope.taskSchedule.RepeatByWeek = null;

            //$scope.taskSchedule.RepeatbyOfEarly = null;
            //$scope.taskSchedule.RepeatByMonth = null;

        }
        else if ($scope.taskSchedule.RepeatType === 'Weekly') {
            $scope.EveryRepeatedFlag = 'Week';
            $scope.taskSchedule.isWeekly = true;
            $scope.taskSchedule.isYearly = false;
            $scope.taskSchedule.isDaily = false;
            $scope.taskSchedule.EveryWeekDay = false;

            //$scope.taskSchedule.RepeatByDayNumber = null;
            //$scope.taskSchedule.RepeatbyNthWeek = null;
            //$scope.taskSchedule.RepeatByWeek = null;

            //$scope.taskSchedule.RepeatbyOfEarly = null;
            //$scope.taskSchedule.RepeatByMonth = null;

        }
        else if ($scope.taskSchedule.RepeatType === 'Monthly') {
            $scope.EveryRepeatedFlag = 'Month';
            $scope.taskSchedule.isYearly = false;
            $scope.taskSchedule.isWeekly = false;
            $scope.taskSchedule.isDaily = false;
            $scope.taskSchedule.EveryWeekDay = false;
            //$scope.taskSchedule.RepeatbyOfEarly = null;
            //$scope.taskSchedule.RepeatByMonth = null;

        }
        else if ($scope.taskSchedule.RepeatType === 'Yearly') {
            $scope.EveryRepeatedFlag = 'Year';
            $scope.taskSchedule.isWeekly = false;
            $scope.taskSchedule.isYearly = true;
            $scope.taskSchedule.isDaily = false;
            $scope.taskSchedule.EveryWeekDay = false;

        }
        else if ($scope.taskSchedule.RepeatType === 'Every') {
            $scope.taskSchedule.isWeekly = false;
            $scope.taskSchedule.isYearly = false;
            $scope.taskSchedule.isDaily = true;
            $scope.taskSchedule.EveryWeekDay = true;

            //$scope.taskSchedule.EveryInterval = null;
            //$scope.taskSchedule.RepeatByDayNumber = null;
            //$scope.taskSchedule.RepeatbyNthWeek = null;
            //$scope.taskSchedule.RepeatByWeek = null;

            //$scope.taskSchedule.RepeatbyOfEarly = null;
            //$scope.taskSchedule.RepeatByMonth = null;

        }
    }
    $scope.assignPrimeryKeyOfAllAudit = function (taskSchedule) {
        $scope.taskSchedule = taskSchedule;
        if ($scope.flagCarringRecurring === 'UpdateAudit') {
            $scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId = taskSchedule.Id;
        }
        else if ($scope.flagCarringRecurring === 'FollowUpAudit') {
            $scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId = taskSchedule.Id;
        }
        else if ($scope.flagCarringRecurring === 'InternalAudit') {
            $scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId = taskSchedule.Id;
        }
        else if ($scope.flagCarringRecurring === 'ExternalAudit') {
            $scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId = taskSchedule.Id;
        }
        $scope.flagCarringRecurring = '';
    }

    var x = '';
    $scope.SaveReccuringData = function () {
        if ($scope.dayList.length > 0) {
            for (var i = 0; i < $scope.dayList.length; i++) {
                if ($scope.dayList[i].isChecked == true) {
                    x += $scope.dayList[i].day + ',';
                }

            }
        }
        $scope.taskSchedule.WeeklyRepeatationBycommaSepDayName = x.slice(0, -1);

        if ($scope.flagCarringRecurring === 'UpdateAudit') {
            if ($scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId == null) {
                $scope.SaveRecurring($scope.flagCarringRecurring);
                //save
            }
            else {
                $scope.UpdateRecurring($scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId, $scope.flagCarringRecurring);
                //update
            }
        }
        else if ($scope.flagCarringRecurring === 'FollowUpAudit') {
            if ($scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId == null) {
                $scope.SaveRecurring($scope.flagCarringRecurring);
            }
            else {
                $scope.UpdateRecurring($scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId, $scope.flagCarringRecurring);
                //update
            }
        }
        else if ($scope.flagCarringRecurring === 'InternalAudit') {
            if ($scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId == null) {
                //save
                $scope.SaveRecurring($scope.flagCarringRecurring);
            }
            else {
                $scope.UpdateRecurring($scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId, $scope.flagCarringRecurring);
                //update
            }
        }
        else if ($scope.flagCarringRecurring === 'ExternalAudit') {
            if ($scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId == null) {
                //save
                $scope.SaveRecurring($scope.flagCarringRecurring);
            }
            else {
                $scope.UpdateRecurring($scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId, $scope.flagCarringRecurring);
                //update
            }
        }



    }
    $scope.AssignTaskScheduleFromResponseData = function (taskScheduleFromResponse) {

        $scope.taskSchedule = taskScheduleFromResponse;
        //$scope.taskSchedule.StartDate = new Date(taskScheduleFromResponse.StartDate);
        //$scope.taskSchedule.EndDate = new Date(taskScheduleFromResponse.EndDate);

        var arr = taskScheduleFromResponse.WeeklyRepeatationBycommaSepDayName.split(",");
        if (arr.length > 0) {
            for (var i = 0; i < arr.length; i++) {
                for (var j = 0; j < $scope.dayList.length; j++) {
                    if ($scope.dayList[j].day === arr[i]) {
                        $scope.dayList[j].isChecked = true;
                        break;
                    }
                }
            }
        }
    }
    //$scope.getRecurringDataForEveryAuditTaskSchedulerMasterId = function (auditIds) {
    //    if ($scope.auditIds.length > 0) {
    //        $http({
    //            method: 'GET',
    //            url: 'issueTracker/IssueTransaction/GetRecurringDataForEveryAuditTaskSchedulerMasterId?auditIds=' + auditIds,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {

    //                $scope.returnedTaskSchedule = response.data;
    //                //success
    //                //if ($scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId == response.data.Id) { $scope.updateTaskScheduleDetails = response.data.Details; }
    //                //else if ($scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId == response.data.Id) { $scope.followUpTaskScheduleDetails = response.data.Details; }
    //                //else if ($scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId == response.data.Id) { $scope.internalTaskScheduleDetails = response.data.Details; }
    //                //else if ($scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId == response.data.Id) { $scope.externalTaskScheduleDetails = response.data.Details; }

    //                //$scope.AssignTaskScheduleFromResponseData(response.data);
    //                //$scope.checkRepeatedStatus();
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult('Failed', 'failure');
    //        });
    //    }
    //}
    $scope.GetRecurringData = function (AuditTaskSchedulerMasterId) {
        $http({
            method: 'GET',
            url: 'issueTracker/IssueTransaction/GetTaskScheduleByAuditTaskSchedulerMasterId?auditTaskSchedulerMasterId=' + AuditTaskSchedulerMasterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                $scope.returnedTaskSchedule = response.data;
                //success
                if ($scope.issueTransactionNew.UpdateAuditTaskSchedulerMasterId == response.data.Id) { $scope.updateTaskScheduleDetails = response.data.Details; }
                else if ($scope.issueTransactionNew.FollowUpAuditTaskSchedulerMasterId == response.data.Id) { $scope.followUpTaskScheduleDetails = response.data.Details; }
                else if ($scope.issueTransactionNew.InternalAuditTaskSchedulerMasterId == response.data.Id) { $scope.internalTaskScheduleDetails = response.data.Details; }
                else if ($scope.issueTransactionNew.ExternalAuditTaskSchedulerMasterId == response.data.Id) { $scope.externalTaskScheduleDetails = response.data.Details; }

                $scope.AssignTaskScheduleFromResponseData(response.data);
                $scope.checkRepeatedStatus();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }
    $scope.CreateTaskScheduleMessage = function (Schedule, flagCarringRecurring) {
        if (Schedule.RepeatType === 'Daily') {

            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeate ' + Schedule.RepeatType;
            $scope.taskSchedule.Details += ' Every ' + Schedule.EveryInterval + ' Day(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);
            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + Schedule.EndDate;
            }
        }
        else if (Schedule.RepeatType === 'Weekly') {


            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeate ' + Schedule.RepeatType;
            $scope.taskSchedule.Details += ' Every ' + Schedule.EveryInterval + ' Week(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);
            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + Schedule.EndDate;
            }
        }
        else if (Schedule.RepeatType === 'Monthly') {

            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeate ' + Schedule.RepeatType;
            $scope.taskSchedule.Details += ' Every ' + Schedule.EveryInterval + ' Month(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);

            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + 'occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + $filter("dateFiltering")(Schedule.EndDate);
            }

            if ($scope.taskSchedule.isRepeatByDay == true) {
                $scope.taskSchedule.Details += 'Repeat On ' + Schedule.RepeatByDayNumber + ' day(s) of the month';
            }
            else if ($scope.taskSchedule.isRepeatByTheNthWeekForMonthly == true) {
                $scope.taskSchedule.Details += 'Repeat On ' + Schedule.RepeatbyNthWeek + ' ' + Schedule.RepeatByWeek + ' of the month';
            }
        }
        else if (Schedule.RepeatType === 'Yearly') {

            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeate ' + Schedule.RepeatType;
            $scope.taskSchedule.Details += ' Every ' + Schedule.EveryInterval + ' Year(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);

            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + $filter("dateFiltering")(Schedule.EndDate);
            }

            if ($scope.taskSchedule.isRepeatByTheMonth == true) {
                $scope.taskSchedule.Details += ' Repeat On ' + Schedule.RepeatByDayNumber + ' Day(s) of ' + Schedule.RepeatByMonth;
            }
            else if ($scope.taskSchedule.isRepeatByTheNthWeekForYearly == true) {
                $scope.taskSchedule.Details += ' Repeat On ' + Schedule.RepeatbyNthWeek + ' ' + Schedule.RepeatByWeek + ' of ' + Schedule.RepeatbyOfEarly;
            }

        }
        else if (Schedule.RepeatType === 'Every') {

            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeate ' + Schedule.RepeatType + ' Week Day';
            $scope.taskSchedule.Details += 'Week Days starting from ' + $filter("dateFiltering")(Schedule.StartDate);
            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + $filter("dateFiltering")(Schedule.EndDate);
            }
        }

        if (flagCarringRecurring === 'UpdateAudit') { $scope.updateTaskScheduleDetails = $scope.taskSchedule.Details; }
        else if (flagCarringRecurring === 'FollowUpAudit') { $scope.followUpTaskScheduleDetails = $scope.taskSchedule.Details; }
        else if (flagCarringRecurring === 'InternalAudit') { $scope.internalTaskScheduleDetails = $scope.taskSchedule.Details; }
        else if (flagCarringRecurring === 'ExternalAudit') { $scope.externalTaskScheduleDetails = $scope.taskSchedule.Details; }
    }
    $scope.SaveRecurring = function (flagCarringRecurring) {
        $scope.CreateTaskScheduleMessage($scope.taskSchedule, flagCarringRecurring);
        try {
            $http({
                method: 'POST',
                url: 'issueTracker/IssueTransaction/CreateTaskSchedule',
                data: { taskSchedule: $scope.taskSchedule },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.assignPrimeryKeyOfAllAudit(response.data.TaskSchedule);
                    //$scope.CreateTaskScheduleMessage(response.data.TaskSchedule);
                    $scope.ClearReccuringData();
                    $scope.hideSchedulerPopUp();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.UpdateRecurring = function (AuditTaskSchedulerMasterId, flagCarringRecurring) {
        $scope.CreateTaskScheduleMessage($scope.taskSchedule, flagCarringRecurring);
        $http({
            method: 'POST',
            url: 'issueTracker/IssueTransaction/EditTaskSchedule',
            data: { auditTaskSchedulerMasterId: AuditTaskSchedulerMasterId, taskSchedule: $scope.taskSchedule },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.CreateTaskScheduleMessage(response.data.TaskSchedule, flagCarringRecurring);
                $scope.hideSchedulerPopUp();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.showTaskSchedulerPopUp = function (auditType) {
        angular.element(document.querySelector('#taskScheduler')).modal('show');
    }

    $scope.hideSchedulerPopUp = function () {
        angular.element(document.querySelector('#taskScheduler')).modal('hide');
    }

    $scope.ClearReccuringData = function () {
        $scope.dayList = [
            { day: 'Sun', isChecked: false },
            { day: 'Mon', isChecked: false },
            { day: 'Tue', isChecked: false },
            { day: 'Wed', isChecked: false },
            { day: 'Thu', isChecked: false },
            { day: 'Fri', isChecked: false },
            { day: 'Sat', isChecked: false }
        ];
        $scope.taskSchedule.RepeatType = "Daily";
        $scope.taskSchedule.isDaily = true;
        $scope.taskSchedule.isWeekly = false;
        $scope.taskSchedule.isYearly = false;
        $scope.taskSchedule.EveryWeekDay = false;

        $scope.taskSchedule.IsNever = true;
        $scope.taskSchedule.IsAfter = false;
        $scope.taskSchedule.IsOn = false;

        $scope.taskSchedule.StartDate = new Date();
        $scope.taskSchedule.EndDate = new Date();
        $scope.taskSchedule.AfterNoOfAccurence = 1;
        $scope.taskSchedule.EveryInterval = 1;
        $scope.taskSchedule.RepeatByDayNumber = 1;
        $scope.taskSchedule.RepeatByMonth = 'January';
        $scope.taskSchedule.RepeatbyOfEarly = 'January';
        $scope.taskSchedule.RepeatbyNthWeek = 'First';
        $scope.taskSchedule.RepeatByWeek = 'Sunday';


        $scope.taskSchedule.isRepeatByDay = true;
        $scope.taskSchedule.isRepeatByTheNthWeekForMonthly = false;

        $scope.taskSchedule.isRepeatByTheNthWeekForYearly = false;
        $scope.taskSchedule.isRepeatByTheMonth = true;
        $scope.taskSchedule.OnPreviousAccomplishment = true;

    }

    $scope.ChangeToNever = function () {

        $scope.taskSchedule.IsNever = true;
        $scope.taskSchedule.IsAfter = false;
        $scope.taskSchedule.IsOn = false;

    };
    $scope.ChangeToAfter = function () {

        $scope.taskSchedule.IsAfter = true;
        $scope.taskSchedule.IsNever = false;
        $scope.taskSchedule.IsOn = false;

    };
    $scope.ChangeToOn = function () {

        $scope.taskSchedule.IsOn = true;
        $scope.taskSchedule.IsNever = false;
        $scope.taskSchedule.IsAfter = false;

    };

    //repeatType for monthly
    $scope.ChangeToRepeatByDay = function () {

        $scope.taskSchedule.isRepeatByDay = true;
        $scope.taskSchedule.isRepeatByTheNthWeekForMonthly = false;

    };
    $scope.ChangeToRepeatByTheNthWeekForMonthly = function () {

        $scope.taskSchedule.isRepeatByTheNthWeekForMonthly = true;
        $scope.taskSchedule.isRepeatByDay = false;

    };

    //repeatType for yearly 
    $scope.ChangeToRepeatByTheNthWeekForYearly = function () {

        //$scope.taskSchedule.isRepeatByTheNthWeek = true;
        $scope.taskSchedule.isRepeatByTheNthWeekForYearly = true;
        $scope.taskSchedule.isRepeatByTheMonth = false;

    };
    $scope.ChangeToRepeatByTheMonth = function () {

        $scope.taskSchedule.isRepeatByTheMonth = true;
        $scope.taskSchedule.isRepeatByTheNthWeekForYearly = false;

    };

    //Tab For Released IssueTransaction 
    $scope.tab = 1;
    var auditPerson = null;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
        auditPerson = newTab;

        if (newTab <= 5) {
            $scope.getAuditOfReleasedIssue();
        }
        else {
            $scope.getTaskManagerSubTasksByIssueTransactionId();
        }
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.ListOfAuditOfReleasedIssueTransaction = [];
    $scope.getAuditOfReleasedIssue = function () {
        $http({
            method: 'GET',
            url: 'issueTracker/IssueTransaction/GetAuditOfReleasedIssue?issueTransactionId=' + $scope.issueTransactionNew.Id + '&audit=' + auditPerson,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //success scope
                $scope.ListOfAuditOfReleasedIssueTransaction = response.data;

            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }


    //IssueGroup PopUp
    $scope.issueGroupNew = {
        Id: null,
        Name: null,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        CreateDate: $filter("dateFiltering")(Date.now())

    };
    $scope.addIssueGroup = function () {
        if ($scope.issueGroupNew.ResponsiblePersonName == null) {
            $scope.ResMessage = "Responsible person is required.";
        }
        if ($scope.issueGroupNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'issueTracker/IssueTransaction/CreateIssueGroup',
                data: { issueGroup: $scope.issueGroupNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.issueTransactionNew.IssueGroupName = response.data.IssueGroup.Name
                    $scope.issueTransactionNew.IssueGroupId = response.data.IssueGroup.Id;

                    $scope.hideIssueGroupPopUp();
                    $scope.cleareIssueGroupNew();

                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    }
    $scope.checkIssueGroupFormValidation = function () {
        var responsiblePerson = document.forms["issueGroupNewForm"]["ResponsiblePersonName"];

        if (responsiblePerson.value == "") {
            $scope.messageForIssueGroupFormVal = "Responsible Person is Required";
            name.focus();
            return false;
        }
        return true;
    }
    $scope.cleareIssueGroupNew = function () {
        $scope.issueGroupNew = {
            Id: null,
            Name: null,
            ResponsiblePersonId: null,
            ResponsiblePersonName: null,
            CreateDate: $filter("dateFiltering")(Date.now())
        };
    }
    $scope.IssueGroups = [];
    $scope.GetIssueGroup = function (x) {
        $scope.issueGroupNew = x;
        $scope.issueTransactionNew.IssueGroupName = $scope.issueGroupNew.Name;
        $scope.issueTransactionNew.IssueGroupId = $scope.issueGroupNew.Id;
        $scope.hideIssueGroupListPopUp();
    }
    $scope.getIssueGroups = function () {
        $http({
            method: 'GET',
            url: 'issueTracker/IssueTransaction/GetIssueGroups',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                $scope.IssueGroups = response.data;
                $scope.showIssueGroupListPopUp();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }

    //IssueGroup ResponsiblePersonPopUp
    $scope.showIssueGroupResponsiblePersonListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {

            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompany';
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

        angular.element(document.querySelector('#issueGroupResponsiblePersonPopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.selectIssueGroupResponsiblePersonListPopUp = function (index, id) {
        $scope.issueGroupIndex = index;
        $scope.selectedIssueGroupId = id;

    };

    $scope.closeIssueGroupResponsiblePersonListPopUp = function () {
        if ($scope.issueGroupIndex !== -1) {
            var employee = $scope.employeeList[$scope.issueGroupIndex];
            $scope.issueGroupNew.ResponsiblePersonName = employee.EmployeeName;
            $scope.issueGroupNew.ResponsiblePersonId = employee.SystemId;
            $scope.issueGroupNew.ResponsiblePersonId = employee.SystemId;
        }
        $scope.hideIssueGroupResponsiblePersonListPopUp();
    };
    $scope.hideIssueGroupResponsiblePersonListPopUp = function () {
        angular.element(document.querySelector('#issueGroupResponsiblePersonPopUp')).modal('hide');
        $scope.issueGroupIndex = -1;
        $scope.selectedIssueGroupId = null;
    };
    //End IssueGroup ResponsiblePersonPopUp

    $scope.showIssueGroupPopUp = function () {

        angular.element(document.querySelector('#issueGroupAddPopUp')).modal('show');
    };
    $scope.hideIssueGroupPopUp = function () {
        $scope.cleareIssueGroupNew()
        $scope.ResMessage = "";
        angular.element(document.querySelector("#issueGroupAddPopUp")).modal("hide");
    };

    $scope.showIssueGroupListPopUp = function () {

        angular.element(document.querySelector('#issueGroupListPopUp')).modal('show');
    };
    $scope.hideIssueGroupListPopUp = function () {
        angular.element(document.querySelector("#issueGroupListPopUp")).modal("hide");
    };

    $scope.isChecked = true;
    $scope.isBuyerApplicableChecked = function () {
        if ($scope.issueTransactionNew.IsBuyerApplicable) {
            $scope.isChecked = true;
            $scope.buyers = [];
            $scope.issueTransactionNew.BuyerName = "";
        }
        else {
            $scope.isChecked = false;
        }
    }



    //PopUp for issueTransaction
    $scope.showEmployeeListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompany';
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
    $scope.showAuthorisePersonListPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompany';
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
        angular.element(document.querySelector('#authorisedPersonPopUp')).modal('show');
        $scope.getEmployeeData();
    };
    $scope.showIssueSubTaskResponsiblePersonPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompany';
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
        angular.element(document.querySelector('#issueSubTaskResponsiblePersonPopUp')).modal('show');
        $scope.getEmployeeData();
    };
    $scope.showUpdateResponsiblePersonListPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompany';
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
        angular.element(document.querySelector('#updateResponsiblePersonPopUp')).modal('show');
        $scope.getEmployeeData();
    };
    $scope.showInternalResponsiblePersonListPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompany';
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
        angular.element(document.querySelector('#internalResponsiblePersonPopUp')).modal('show');
        $scope.getEmployeeData();
    };
    $scope.showExternalResponsiblePersonListPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompany';
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
        angular.element(document.querySelector('#externalResponsiblePersonPopUp')).modal('show');
        $scope.getEmployeeData();
    };
    $scope.showFollowUpResponsiblePersonListPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompany';
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
        angular.element(document.querySelector('#followupResponsiblePersonPopUp')).modal('show');
        $scope.getEmployeeData();
    };
    $scope.showMentorPersonListPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompany';
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
        angular.element(document.querySelector('#mentorPersonPopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeindex = index;
        $scope.selectedEmployee = id;
        $scope.updateResponsiblePersonIndex = index;
        $scope.selectedupdateResponsiblePersonId = id;
    };
    $scope.selectUpdateResponsiblePersionPopUp = function (index, id) {
        $scope.updateResponsiblePersonIndex = index;
        $scope.selectedupdateResponsiblePersonId = id;
    };
    $scope.selectFollowUpResponsiblePersonPopUp = function (index, id) {
        $scope.followUpResponsiblePersonIndex = index;
        $scope.selectedFollowUpResponsiblePersonId = id;
    };
    $scope.selectInternalResponsiblePersonPopUp = function (index, id) {
        $scope.internalResponsiblePersonIndex = index;
        $scope.selectedInternalResponsiblePersonId = id;
    };
    $scope.selectExtarnalResponsiblePersonPopUp = function (index, id) {
        $scope.externalResponsiblePersonIndex = index;
        $scope.selectedExternalResponsiblePersonId = id;
    };
    $scope.selectResponsiblePersonPopUp = function (index, id) {
        $scope.responsiblePersonIndex = index;
        $scope.selectedResponsiblePerson = id;
    };
    $scope.selectAuthorisePopUp = function (index, id) {
        $scope.authorisepersonindex = index;
        $scope.selectedauthorisepersonId = id;
    };
    $scope.selectMentorPopUp = function (index, id) {
        $scope.mentorPersonIndex = index;
        $scope.selectedMentorPerson = id;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#responsiblePersonPopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    }
    $scope.hideAuthorisePersonPopUp = function () {
        angular.element(document.querySelector('#authorisedPersonPopUp')).modal('hide');
        $scope.authorisePersonIndex = -1;
        $scope.selectedAuthorisePerson = null;
    };
    $scope.hideMentorPersonPopUp = function () {
        angular.element(document.querySelector('#mentorPersonPopUp')).modal('hide');
        $scope.mentorPersonIndex = -1;
        $scope.selectedMentorPerson = null;
    };

    //#region start IssueSubTask 

    $scope.IssueSubTaskPopUpForUpdate = function (data) {
        $scope.issueSubTaskNew.Id = data.Id;
        $scope.issueSubTaskNew.Remarks = data.Remarks;
        $scope.issueSubTaskNew.IsDone = data.IsDone;
        $scope.issueSubTaskNew.RequiredDate = data.RequiredDate;
        $scope.issueSubTaskNew.issueSubTaskNewResponsiblePerson = data.ResponsiblePerson;
        $scope.issueSubTaskNew.ResponsiblePersonId = data.ResponsiblePersonId;
        $scope.issueSubTaskNew.TaskDetail = data.TaskDetail;
        $scope.issueSubTaskNew.IssueTransactionId = data.IssueTransactionId;

        angular.element(document.querySelector("#issueSubTaskPopUp")).modal("show");
        //angular.element(document.querySelector("#issueSubTaskPopUpForUpdate")).modal("show");
    }

    $scope.UpdateIssueSubTask = function () {

        try {
            $http({
                method: 'POST',
                url: 'IssueTracker/IssueSubTask/edit',
                //data: $scope.issueSubTaskNew,
                data: { model: $scope.issueSubTaskNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getTaskManagerSubTasksByIssueTransactionId();
                    $scope.hideIssueTaskPopUpForUpdate();
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.DeleteIssueSubTaskConfirmation = function (x) {
        $scope.issueSubTaskNew.Id = x.Id;
        $scope.message_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteIssueSubTaskPopUp")).modal("show");
    }

    $scope.DeleteIssueSubTask = function () {
        $http({
            method: 'POST',
            url: 'IssueTracker/IssueSubTask/delete/' + $scope.issueSubTaskNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getTaskManagerSubTasksByIssueTransactionId();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

    $scope.hideIssueTaskPopUpForUpdate = function () {
        angular.element(document.querySelector('#issueSubTaskPopUpForUpdate')).modal('hide');

    };

    //#endRegion end IssueSubTask

    $scope.RemoveIssueGroupPopUp = function (x) {
        $scope.issueGroupId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteGroupPopUp")).modal("show");
    };

    $scope.DeleteGroup = function () {
        $http({
            method: 'POST',
            url: 'issueTracker/IssueTransaction/DeleteGroup',
            data: { 'issueGroupId': $scope.issueGroupId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector("#DeleteGroupPopUp")).modal("hide");
                $scope.getIssueGroups();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };



    // #region Customer popup
    $scope.partyType = 'Customer';
    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'PartyName, PartyAccountGroupName'
        , searchBy: 'PartyName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.partyList = [];
    $scope.showPartyPopUp = function () {
        $scope.partyList = [];
        $scope.getPartyList = function (pageno) {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList/' + 'GetCompanyPartyDataList?companyId=' + $window.companyId + '&PlantId=' + $window.plantId + '&partyType=' + $scope.partyType;
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };
    $scope.selectCustomerPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedCustomer = id;
    };
    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.issueTransactionNew.Customer = party.UserName;
            $scope.issueTransactionNew.CustomerId = party.Id;
            angular.element(document.querySelector('#partyPopUp')).modal('hide');

        }
    };
    $scope.selectPartyPopUpRow = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
    };
    //#endregion 

    //$scope.filedata = [];
    //$("#uploadBtn").change(function () {
    //    $scope.filedata = this.files[0];
    //});

    //document.getElementById("uploadBtn").onchange = function () {
    //    var filename = document.getElementById("uploadFile").value = this.value;
    //    var res = filename.replace(/C:\\fakepath\\/i, '');
    //    document.getElementById("uploadFile").value = res;
    //};


    $scope.issueTransactionDocuments = [];
    $scope.addAttachment = function () {
        angular.element(document.querySelector('#DocumentPopUp')).modal('show');
    };

    $scope.Close = function () {
        angular.element(document.querySelector('#DocumentPopUp')).modal('hide');
    };

    function checkFileExist(list, name) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].name === name) {
                return true;
            }
        }
        return false;
    }
    $scope.ClearImage = function () {
        document.getElementById('uploadBtn').value = '';
        document.getElementById("uploadFile").value = '';
        $scope.issueTransactionDocuments = {};
        $scope.filedata = null;
    };

    $scope.ClearDoc = function () {
        $scope.issueTransactionDocuments = {
            Id: null,
            IssueTransactionId: null,
            FileName: null,
            Description: null
        }
    };

    $scope.issueTransactionDocuments = {
        Id: null,
        IssueTransactionId: null,
        FileName: null,
        Description: null
    }

    $scope.SaveDocuments = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb.';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.issueTransactionDocuments.FileName = fileName;
            //if (!baseService.isUndefinedOrNull($scope.issueTransactionDocuments.FileName)) {
            //    if ($scope.issueTransactionDocuments.FileName.length > 50) {
            //        throw "File Name must be less than 50 character.";
            //    }
            //}
            $scope.issueTransactionDocuments.IssueTransactionId = $scope.issueTransactionNew.Id;
            var formData = new FormData();

            $http({
                method: 'POST',
                url: 'IssueTracker/IssueTransaction/CreateDocuments',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("issueTransactionDocuments", angular.toJson(data.issueTransactionDocuments));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'issueTransactionDocuments': $scope.issueTransactionDocuments, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {

                    ShowResult(response.data.Message, "failure", 'DocumentPopUp');
                }
                else {
                    ShowResult(response.data.Message, "success", 'DocumentPopUp');
                   
                    $scope.Close();
                    $scope.LoadIssueDocumentsData($scope.issueTransactionNew.Id);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", 'DocumentPopUp');
            });
            return true;
            //}
        } catch (e) {
            ShowResult(e, "failure", 'DocumentPopUp');
        }
    };


    $scope.onBeginUpload = function (args) {
        try {
          
            if (baseService.isUndefinedOrNull($scope.issueTransactionNew.Id))
                throw 'Please select/save Issue Transaction first.'
          
            var _data = [{ Id:null,IssueTransactionId: $scope.issueTransactionNew.Id, Description: $scope.issueTransactionDocuments.Description }];

            args.data = JSON.stringify(_data);
         
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = 'IssueTracker/IssueTransaction/SaveDefault';
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (baseService.isUndefinedOrNull($scope.issueTransactionNew.Id))
            ShowResult('Please select/save Issue Transaction first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than 10 MB", 'failure');
    }


    $scope.issueTransactionDocumentList = [];
    $scope.LoadIssueDocumentsData = function () {
        $scope.ClearDoc();
        $http.get('IssueTracker/IssueTransaction/GetIssueDocumentsData?issueTransactionId=' + $scope.issueTransactionNew.Id)
            .then(function (response) {
                $scope.issueTransactionDocumentList = response.data;
            });
    };

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.IssueTransactionDocument + '/' + data.Id + extention;
    };

    $scope.indexQua = -1;
    $scope.GetDocumentData = function (data, index) {
        $scope.filedata = {};
        $scope.issueTransactionDocuments = Object.assign({}, data);
        $scope.filedata.name = data.FileName;
        $scope.issueTransactionDocuments.FileName = data.FileName;
       // var filename = document.getElementById("uploadFile").value = data.FileName;

        $scope.indexQua = index;
        angular.element(document.querySelector('#DocumentPopUp')).modal('show');
    };

    $scope.confirmQualificationDelete = function (data) {
        $scope.deleteQualificationId = data.Id;
        $scope.message_confirmation = "Are you sure to delete [" + data.FileName + "]? ";
    };

    $scope.DeleteDocument = function () {
        $http({
            method: 'POST',
            url: 'IssueTracker/IssueTransaction/DeleteDocument',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteQualificationId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadIssueDocumentsData($scope.issueTransactionNew.Id);
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };
}