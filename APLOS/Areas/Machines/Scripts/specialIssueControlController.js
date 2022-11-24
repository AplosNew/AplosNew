'use strict';
specialIssueControlController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function specialIssueControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "SpecialIssueControl";
    $scope.MonitoringPeriodList = [];
    $scope.IssueStatusList = [];
    $scope.Action = 'Save';
    $scope.path = 'Machines/SpecialIssueControl/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlItem = $scope.path + 'createItem';
    $scope.saveUrlPeriod = $scope.path + 'createPeriod';
  

    $scope.CategoryList = [];
    $scope.GetCategoryList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SpecialIssueControl/GetCategoryList'
        }).then(function successCallback(response) {
            $scope.CategoryList = response.data;
        });
    }
    $scope.GetCategoryList();

    $scope.SubCategoryList = [];
    $scope.GetSubCategoryList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SpecialIssueControl/GetSubCategoryList'
        }).then(function successCallback(response) {
            $scope.SubCategoryList = response.data;
        });
    }
    $scope.GetSubCategoryList();

    $scope.MonitoringPeriodList = [
        {
            'Value': '1',
            'Text': '1'
        },
        {
            'Value': '2',
            'Text': '2'
        },
        {
            'Value': '3',
            'Text': '3'
        },
        {
            'Value': '4',
            'Text': '4'
        },
        {
            'Value': '5',
            'Text': '5'
        },
        {
            'Value': '6',
            'Text': '6'
        },
        {
            'Value': '7',
            'Text': '7'
        },
        {
            'Value': '8',
            'Text': '8'
        },
        {
            'Value': '9',
            'Text': '9'
        },
        {
            'Value': '10',
            'Text': '10'
        }
    ];
    $scope.IssueStatusList = [
        {
            'Value': 'Inprogress',
            'Text': 'Inprogress'
        },
        {
            'Value': 'Close',
            'Text': 'Close'
        }
    ];
    $scope.issue = {
        Id: null
        , Category: null
        , SubCategory: null
        , SpecialIssueName: null
        , SpecialIssueDetails: null
        , TargetDate:null
        , Remarks: null
        , ResponsiblePersonId: null
        , ResponsiblePerson: null
        , MonitoringPeriod: null
        , IssueStatus: null
    };
    $scope.issueNew = Object.assign({}, $scope.issue);

    $scope.Item = {
        Id: null
        , SpecialIssueItem: null
        , Actiontaken: null
        , ActiontakenById: null
        , ActiontakenBy: null
        , SampleSize: null
        , Remarks: null
        , SpecialIssueControlId:null
    };
    $scope.ItemNew = Object.assign({}, $scope.Item);

    $scope.Period = {
        Id: null
        , Sequence: null
        , PeriodName: null
        , Shift: null
        , Time: null
    };
    $scope.PeriodNew = Object.assign({}, $scope.Period);

    $scope.ShiftList = [];
    $scope.GetShiftList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SpecialIssueControlUpdate/GetShiftList'
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.GetShiftList();

    $scope.IssueControlMasterList = [];
    $scope.LoadSpecialIssueMasterList = function () {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControl/LoadSpecialIssueMasterList'
        }).then(function successCallback(response) {
            $scope.IssueControlMasterList = response.data;
            var gridObj = $("#GridSpecialIssueControlMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }
    $scope.LoadSpecialIssueMasterList();

    $scope.IssueItemList = [];
    $scope.LoadItemDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/SpecialIssueControl/LoadItemDetails?IssueId='+$scope.issueNew.Id
        }).then(function successCallback(response) {
            $scope.IssueItemList = response.data;
        }
        )
    }

    $scope.PeriodList = [];
    $scope.LoadPeriodDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/SpecialIssueControl/LoadPeriodDetails'
        }).then(function successCallback(response) {
            $scope.PeriodList = response.data;
        }
        )
    }
    $scope.LoadPeriodDetails();

    $scope.selectEmployee = function () {
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.issueNew.ResponsiblePersonId = e.data.SystemId;
        $scope.issueNew.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePopup')).modal('hide');
    }

    $scope.closeResponsiblePopUp = function () {
        angular.element(document.querySelector('#ResponsiblePopup')).modal('hide');
    }

    $scope.selectActionTakenBy = function () {
        $scope.getActionTakenBy();
        angular.element(document.querySelector('#ActionTakenByPopup')).modal('show');
    }

    $scope.ActionTakenByList = [];
    $scope.getActionTakenBy = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetActionTakenBy',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ActionTakenByList = resp.data;
        });
    }

    $scope.doubleActionTakenBy = function (e) {
        $scope.ItemNew.ActiontakenById = e.data.SystemId;
        $scope.ItemNew.ActiontakenBy = e.data.EmployeeName;
        angular.element(document.querySelector('#ActionTakenByPopup')).modal('hide');
    }

    $scope.closeActionTakenByPopUp = function () {
        angular.element(document.querySelector('#ActionTakenByPopup')).modal('hide');
    }



    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SpecialIssueControlForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'IssueData': $scope.issueNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadSpecialIssueMasterList();
                    SpecialIssueClearFields();
                 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    $scope.ItemSave = function () {
        angular.copy($scope.ItemNew, $scope.Item);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SpecialIssueItemForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlItem,
                data: {
                    'ItemData': $scope.ItemNew,
                    'Pid': $scope.issueNew.Id,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadItemDetails($scope.issueNew.Id);
                    ItemClearFields();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.PeriodSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SpecialDefinePeirodForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlPeriod,
                data: {
                    'PeriodData': $scope.PeriodNew,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadPeriodDetails($scope.PeriodNew.Id);
                    PeriodClearFields();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.GetDetails = function (args) {
        $scope.IssueMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControl/LoadIssueEditData?IssueID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.issueNew = response.data.issue[0];
            $scope.LoadItemDetails($scope.IssueMasterId);
           // ItemClearFields();
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetItemDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControl/LoadItemEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ItemNew = response.data.item[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetPeriodDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControl/LoadPeriodEditData?PeriodId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.PeriodNew = response.data.Period[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }


    $scope.Clear = function () {
        SpecialIssueClearFields();
    };
    $scope.ItemClear = function () {
        ItemClearFields();
    };
    $scope.PeriodClear = function () {
        PeriodClearFields();
    };
    function SpecialIssueClearFields() {
        $scope.Action = "Save";
        $scope.issueNew = Object.assign({}, $scope.issue);
    }
    function PeriodClearFields() {
        $scope.Action = "Save";
        $scope.PeriodNew = Object.assign({}, $scope.Period);
    }
    function ItemClearFields() {
        $scope.Action = "Save";
        $scope.Item = {
            Id: null
            , SpecialIssueItem: null
            , Actiontaken: null
            , ActiontakenById: null
            , ActiontakenBy: null
            , SampleSize: null
            , Remarks: null
            , SpecialIssueControlId: null
        };
        $scope.ItemNew = Object.assign({}, $scope.Item);
    }

    $scope.removeRowModal = function (index,data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveItem')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
   
    $scope.removeItemRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/SpecialIssueControl/ItemDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadItemDetails($scope.issueNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    
    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Machines/SpecialIssueControl/IssueDelete?id=' + $scope.issueNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadSpecialIssueMasterList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
}