'use strict';
incedentUpdateController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function incedentUpdateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "IncedentUpdate";
    $scope.Action = 'Save';
    $scope.path = 'Machines/IncedentUpdate/';
    $scope.saveUrl = $scope.path + 'create';
    var CurrentTime = new Date();
    $scope.IncedentTypeList = [];
    $scope.CriticalityLevelList = [];
    $scope.StoryPointsList = [];
    $scope.FinalStatusList = [];

    $scope.IncedentCategoryList = [];
    $scope.GetIncedentCategoryList = function () {
        $http({
            method: 'GET',
            url: 'Machines/IncedentUpdate/GetIncedentCategoryList'
        }).then(function successCallback(response) {
            $scope.IncedentCategoryList = response.data;
        });
    }
    $scope.GetIncedentCategoryList();

    $scope.IncedentTypeList = [
        {
            'Value': 'Positive',
            'Text': 'Positive'
        },
        {
            'Value': 'Improvement',
            'Text': 'Improvement'
        }
    ];

    $scope.CriticalityLevelList = [
        {
            'Value': 'Normal',
            'Text': 'Normal'
        },
        {
            'Value': 'Important',
            'Text': 'Important'
        },
        {
            'Value': 'Semi Critical',
            'Text': 'Semi Critical'
        },
        {
            'Value': 'Critical',
            'Text': 'Critical'
        },
        {
            'Value': 'Higly Critical',
            'Text': 'Higly Critical'
        }

    ];

    $scope.StoryPointsList = [
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
        }

    ];

    $scope.FinalStatusList = [
        {
            'Value': 'Inprogress',
            'Text': 'Inprogress'
        },
        {
            'Value': 'Close',
            'Text': 'Close'
        }
    ];

    $scope.incedent = {
        Id: null
        , Date: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
        , Time: CurrentTime
        , EmployeeName: null
        , EmployeeId: null
        , BudgetCode: null
        , ROName: null
        , RONameId: null
        , IncedentCategoryId:null
        , IncedentItemTitle: null
        , IncedentDetail: null
        , IncedentType: null
        , CriticalityLevel: null
        , ActionTaken: null
        , StoryPoints: null
        , FollowUpApplicable: true
        , FollowUpDays: null
        , FollowUpById: null
        , FollowUpBy: null
        , IssueInchargeId: null
        , IssueIncharge: null
        , FinalStatus: null
        , Remarks:null

    };
    $scope.incedentupdate = Object.assign({}, $scope.incedent);

    $scope.incedents = {
        Id: null
        , IncedentId: null
        , Date: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
        , Update: null
        , ActionToBeTaken: null
        , ResponsiblePersonId: null
        , ResponsiblePerson: null
        , Remarks: null
    };
    $scope.incedentupdateNew = Object.assign({}, $scope.incedents);

    $scope.IncedentCategoryUpdateList = [];
    $scope.GetIncedentCategoryUpdate = function () {
        $http({
            method: 'Get',
            url: 'Machines/IncedentUpdate/LoadIncedentCategoryUpdate'
        }).then(function successCallback(response) {
            $scope.IncedentCategoryUpdateList = response.data;
            var gridObj = $("#GridIncedentCategoryUpdate").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.GetIncedentCategoryUpdate();

    $scope.selectEmployee = function () {
        $scope.getEmployee();
        angular.element(document.querySelector('#EmployeePopup')).modal('show');
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
        $scope.incedentupdate.EmployeeId = e.data.SystemId;
        $scope.incedentupdate.EmployeeName = e.data.EmployeeName;
        $scope.incedentupdate.BudgetCode = e.data.BudgetCode;
        angular.element(document.querySelector('#EmployeePopup')).modal('hide');
    }

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePopup')).modal('hide');
    }

    $scope.selectROEmployee = function () {
        $scope.getROEmployee();
        angular.element(document.querySelector('#ROEmployeePopup')).modal('show');
    }

    $scope.ROEmployeeList = [];
    $scope.getROEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetROEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ROEmployeeList = resp.data;
        });
    }

    $scope.doubleROEmployee = function (e) {
        $scope.incedentupdate.RONameId = e.data.RONameId;
        $scope.incedentupdate.ROName = e.data.ROName;
        $scope.incedentupdate.FollowUpById = e.data.RONameId;
        $scope.incedentupdate.FollowUpBy = e.data.ROName;
        angular.element(document.querySelector('#ROEmployeePopup')).modal('hide');
    }

    $scope.closeROEmployeePopUp = function () {
        angular.element(document.querySelector('#ROEmployeePopup')).modal('hide');
    }

    $scope.selectFollowUpBy = function () {
        $scope.getFollowUpBy();
        angular.element(document.querySelector('#FollowUpByPopup')).modal('show');
    }

    $scope.FollowUpByList = [];
    $scope.getFollowUpBy = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetFollowUpBy',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.FollowUpByList = resp.data;
        });
    }

    $scope.doubleFollowUpBy = function (e) {
        $scope.incedentupdate.FollowUpById = e.data.FollowUpById;
        $scope.incedentupdate.FollowUpBy = e.data.FollowUpBy;
        angular.element(document.querySelector('#FollowUpByPopup')).modal('hide');
    }

    $scope.closeFollowUpByPopUp = function () {
        angular.element(document.querySelector('#FollowUpByPopup')).modal('hide');
    }

    $scope.selectResponsiblePerson = function () {
        $scope.getResponsiblePerson();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.ResponsiblePersonList = [];
    $scope.getResponsiblePerson = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetResponsiblePerson',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsiblePersonList = resp.data;
        });
    }

    $scope.doubleResponsiblePerson = function (e) {
        $scope.incedentupdateNew.ResponsiblePersonId = e.data.SystemId;
        $scope.incedentupdateNew.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.GetIssueInchargeById = function (incedentid) {
        $http({
            method: 'GET',
            url: 'Machines/IncedentUpdate/GetIssueInchargeById?id=' + incedentid
        }).then(function successCallback(response) {
            $scope.incedentupdate.IssueIncharge = response.data[0].EmployeeName;
            $scope.incedentupdate.IssueInchargeId = response.data[0].EmpId;

        });
    }

    $scope.IncedentUpdateList = [];
    $scope.GetIncedentUpdate = function (Id) {
        $http({
            method: 'Get',
            url: 'Machines/IncedentUpdate/LoadIncedentUpdate?IncedentId=' + Id
        }).then(function successCallback(response) {
            $scope.IncedentUpdateList = response.data;
            var gridObj = $("#GridIncedentUpdate").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    /* $scope.GetIncedentUpdate();*/

    $scope.LoadIncedentUpdateGrids = function () {
        $http({
            method: 'Get',
            url: 'Machines/IncedentUpdate/LoadIncedentUpdateGrid'
        }).then(function successCallback(response) {
            $scope.IncedentUpdateList = response.data;
            var gridObj = $("#GridIncedentUpdate").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.IncedentUpdateForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'IncedentUpdateData': $scope.incedentupdateNew, 'PId': $scope.incedentupdate.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.LoadIncedentUpdateGrids();
                        IncedentUpdateClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.GetICUDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/IncedentUpdate/LoadICUEditData?ICUId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.incedentupdate = response.data.incedentupdate[0];
            $scope.GetIncedentUpdate(args.data.Id);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetIUDetails = function (args1) {
        $http({
            method: 'Get',
            url: 'Machines/IncedentUpdate/LoadIUEditData?IUId=' + args1.data.Id
        }).then(function successCallback(response) {
            $scope.incedentupdateNew = response.data.incedentupdates[0];
        }
        )
    }
   
    $scope.Clear = function () {
        IncedentUpdateClearFields();
    };

    function IncedentUpdateClearFields() {
        $scope.Action = "Save";
        $scope.incedentupdateNew = Object.assign({}, $scope.incedents);
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Machines/IncedentUpdate/IncedentUpdateDelete?id=' + $scope.incedentupdateNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadIncedentUpdateGrids();
                IncedentUpdateClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
 
    //#region ICU File 
    $scope.ItemId = null;
    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull(args.model.Data))
                throw 'Please select/save the order first'
            $scope.ItemId = args.model.Data;
            args.data = args.model.Data;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "Machines/IncedentUpdate/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ItemId))
            ShowResult('Please select/save the order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ICUPath + '/' + data.Id + extention;
    };

    //#endregion
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.IncedentReport = function () {
        $scope.fileName = "IncedentReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetIncedentReport",
            data: { 'reportFileName': $scope.fileName},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}