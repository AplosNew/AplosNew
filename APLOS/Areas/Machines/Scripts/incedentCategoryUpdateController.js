'use strict';
incedentCategoryUpdateController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function incedentCategoryUpdateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "IncedentCategoryUpdate";
    $scope.Action = 'Save';
    $scope.path = 'Machines/IncedentCategoryUpdate/';
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
            url: 'Machines/IncedentCategoryUpdate/GetIncedentCategoryList'
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

   

    $scope.IncedentCategoryUpdateList = [];
    $scope.GetIncedentCategoryUpdate = function (Id) {
        $http({
            method: 'Get',
            url: 'Machines/IncedentCategoryUpdate/LoadIncedentCategoryUpdate?Id=' + Id
        }).then(function successCallback(response) {
            $scope.IncedentCategoryUpdateList = response.data;
            var gridObj = $("#GridIncedentCategoryUpdate").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
  /*  $scope.GetIncedentCategoryUpdate();*/

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

    $scope.GetIssueInchargeById = function (incedentid) {
        $http({
            method: 'GET',
            url: 'Machines/IncedentCategoryUpdate/GetIssueInchargeById?id=' + incedentid
        }).then(function successCallback(response) {
            $scope.incedentupdate.IssueIncharge = response.data[0].EmployeeName;
            $scope.incedentupdate.IssueInchargeId = response.data[0].EmpId;

        });
    }

    $scope.tempId = null;
    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.IncedentCategoryUpdateForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'IncedentUpdateData': $scope.incedentupdate },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.tempId == "") {
                            $scope.tempId = "'','" + response.data.Data.Id + "'";
                        }
                        else {
                            $scope.tempId += ",'" + response.data.Data.Id + "'";
                        }
                        $scope.GetIncedentCategoryUpdate($scope.tempId);
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
            url: 'Machines/IncedentCategoryUpdate/LoadICUEditData?ICUId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.incedentupdate = response.data.incedentupdate[0];
        }
        )
    }
   
    $scope.Clear = function () {
        IncedentUpdateClearFields();
    };

    function IncedentUpdateClearFields() {
        $scope.Action = "Save";
        $scope.incedentupdate = Object.assign({}, $scope.incedent);
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Machines/IncedentCategoryUpdate/IncedentUpdateDelete?id=' + $scope.incedentupdate.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetIncedentCategoryUpdate();
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
    $scope.uploadUrl = "Machines/IncedentCategoryUpdate/SaveDefault";
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
}