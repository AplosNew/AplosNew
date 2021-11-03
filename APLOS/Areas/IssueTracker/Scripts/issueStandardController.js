'use strict';
issueStandardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function issueStandardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'issueStandard';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.issueStandards = [];
    $scope.path = 'issueTracker/issueStandard/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    $scope.searchByList = [
        {
            "name": "Code",
            "value": "Code"
        },
        {
            "name": "Sort Name",
            "value": "SortName"
        },
        {
            "name": "User Defined Name",
            "value": "UserName"
        },
        {
            "name": "Standard Name",
            "value": "StandardName"
        },
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
            "name": "IssueS ubCategory",
            "value": "IssueSubCategory"
        },
        {
            "name": "Issue Status",
            "value": "IssueStatus"
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
    
    baseService.init("issueTracker/issueStandard/getlist", null, null, null, "UserName", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.issueStandards = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.issueStandard = {
        Id: null,
        Code: null,
        SortName: null,
        UserName: null,
        StandardName: null,
        Issue: null,
        IssueDetail: null,
        Remarks: null,
        StatusUpdateInterval: null,
        OverdueDays: null,
        InternalAuditLagDay: null,
        AddedBy: null,
        AddedDate: null,
        IssueCategoryId: null,
        IssueSubCategoryId: null,
        IssueImportanceId: null,
        BuyerMasterId: null,
        TaskCategoryId: null,
        TaskSubCategoryId: null

    };

    $scope.issueStandardNew = Object.assign({}, $scope.issueStandard);

    //$scope.issueCategoryList = [];
    //cboService.getIssueCategoryCbo(function (result) {
    //    $scope.issueCategoryList = result;

    //});
    //$scope.issueSubCategoryList = [];
    //cboService.getIssueSubCategoryCbo(function (result) {
    //    $scope.issueSubCategoryList = result;

    //});

    $scope.taskCategoryList = [];
    $scope.getTaskCategoryCbo = function () {
        $http({
            method: 'GET',
            url: 'issueTracker/IssueTransaction/GetTaskCategory',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                //sccuess 
                $scope.taskCategoryList = response.data;
                
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
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
                ShowResult('error', 'failure');
            }
            else {
                //success
                $scope.taskSubCategoryList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.getTaskSubCategoryCbo();

    

    $scope.issueImportanceList = [];
    cboService.getIssueImportanceCbo(function (result) {
        $scope.issueImportanceList = result;

    });
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.issueStandard = $scope.issueStandards[$scope.index];
        $scope.issueStandardNew = Object.assign({}, $scope.issueStandard);
        $scope.Action = 'Update';

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.buyerList = [];
    $scope.buyerPOPUP = function () {
       
        $scope.searchByBuyerList = [
            {
                'name': 'Buyer',
                'value': 'BuyerName'
            },
            {
                'name': 'Department',
                'value': 'DepartmentName'
            },
            {
                'name': 'Division',
                'value': 'DivisionName'
            }
        ];

        $scope.parameters.searchBy = 'BuyerName';
        baseService.init('Parties/BuyerMaster/GetABuyerMasterList' , null, null, null, 'BuyerName', 'BuyerName');
        $scope.getBuyerData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getBuyerData();
        angular.element(document.querySelector('#buyerSearchModal')).modal('show');
    };
    //Passing Data For IntermediateItemEntity List
    $scope.buyerCloseListPopUp = function (data) {
        $scope.issueStandardNew.BuyerMasterId = data.Id;
       
        $scope.issueStandardNew.BuyerName = data.BuyerName;
        angular.element(document.querySelector('#buyerSearchModal')).modal('hide');
    };
    

    $scope.Save = function () {
        angular.copy($scope.issueStandardNew, $scope.issueStandard);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.issueStandardNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.issueStandard,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        //$scope.issueStandards.push(response.data.IssueStandard);
                        //baseService.paginationAdd();
                        $scope.Clear();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.issueStandard,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.issueStandards[$scope.index] = $scope.issueStandard;
                            
                        }
                        $scope.Clear();
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.issueStandardNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.issueStandardNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.issueStandards.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                    $scope.getData();
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

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.issueStandard = {};
        $scope.issueStandardNew = {};
        //$scope.taskTypeNew.Active = true;
        $scope.issueStandardNew.Sequence = seq;
    }
}