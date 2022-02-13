'use strict';
MeetingPointsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MeetingPointsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Meeting Points';
    $scope.Action = 'Save'; 
    $scope.ModelList = [];
    $scope.ModelTalkingPointList = [];
    $scope.ModelSuggestionsRecommendationList = [];
    $scope.ModelActionablePointsList = [];
    $scope.ModelMeetingDecisionList = [];
    $scope.path = 'MeetingManagement/MeetingPoints/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/'; 
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $scope.talkingPointUrl = $scope.path + 'GettalkingPoint';


    $scope.ModelTemp = {
        Id: null,
        Department: null,
        MeetingType: null,
        ItemDetail: null,
        BackgroundIssueDetail: null,
        ActionApplicable: true,
        DecisionApplicable: true,
        IssueStatus: 'Active',
        Attachment: null,
        ByWhomId: null,
        ByWhomCode: null,
        ItemTitle: null,
        IssueMeetingItemTitle: null,
        IssueCritically: null,
        CostEstimation: null,
        MeetingLegDays: 7,
        CostApplicable: null,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelTalking = {
        Id: null,
        MeetingItemHeaderId: null,
        TalkingPoint: null,
        DiscussionDetail: null,
        TalkingPointByWhomName: null,
        TalkingPointByWhomCode: null
    };
    $scope.ModelTalkingPoint = Object.assign({}, $scope.ModelTalking);

    $scope.ModelMeetSug = {
        Id: null,
        MeetingItemHeaderId: null,
        Suggestion: null,
        DiscussionDetail: null,
        SuggestionsByWhomCode: null,
        SuggestionsByWhomName: null
    };
    $scope.ModelMeetingSuggestion = Object.assign({}, $scope.ModelMeetSug);

    $scope.ModelActionable = {
        Id: null,
        MeetingItemHeaderId: null,
        ActionToBeTaken: null,
        ActionByWhomCode: null,
        ActionByWhomName: null,
        Status: null
    };
    $scope.ModelActionablePoints = Object.assign({}, $scope.ModelActionable);

    $scope.ModelMeetingDec = {
        Id: null,
        MeetingItemHeaderId: null,
        Decision: null,
        DecisionByWhomName: null,
        DecisionByWhomCode: null,
        Remarks: null
    };
    $scope.ModelMeetingDecision = Object.assign({}, $scope.ModelMeetingDec);


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

    $scope.getTalkingPointData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetTalkingPointList",
            data: { column: $scope.searchBy, value: $scope.search, meetingItemHeaderId: $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
           
            $scope.ModelTalkingPointList = response.data;
        });
    }
   
    $scope.GetTalkingPoint = function (args) {

        $scope.ModelTalkingPoint = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getSuggestionsRecommendationData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetSuggestionsRecommendationList",
            data: { column: $scope.searchBy, value: $scope.search, meetingItemHeaderId: $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelSuggestionsRecommendationList = response.data;
        });
    }
    

    $scope.GetSuggestionsRecommendation = function (args) {

        $scope.ModelMeetingSuggestion = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getActionablePointsData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetActionablePointsList",
            data: { column: $scope.searchBy, value: $scope.search, meetingItemHeaderId: $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelActionablePointsList = response.data;
        });
    }
    

    $scope.GetActionablePoints = function (args) {

        $scope.ModelActionablePoints = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getMeetingDecisionData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetMeetingDecisionList",
            data: { column: $scope.searchBy, value: $scope.search, meetingItemHeaderId: $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelMeetingDecisionList = response.data;
        });
    }

    $scope.GetMeetingDecision = function (args) {

        $scope.ModelMeetingDecision = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        
        try {
            angular.copy($scope.ModelNew, $scope.ModelTemp);
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.ModelNewForm.$valid) {
                if ($scope.Action === 'Save') {
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
                            $scope.ModelNew.Id = response.data.Id;
                            $scope.getData();

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
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
                    $scope.getData();
                    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
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
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }


    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });


    $scope.meetingTypeList = [];
    cboService.getCbomeetingType(function (result) {
        $scope.meetingTypeList = result;
    });

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

    
    $scope.Name = null;
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
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;
        
        if ($scope.Name == 'Main') {
            $scope.ModelNew.ByWhomId = data.SystemId;
            $scope.ModelNew.ByWhomName = data.EmployeeName;
            $scope.ModelNew.ByWhomCode = data.EmployeeCode;
        }
        else if ($scope.Name == 'Talking') {
            $scope.ModelTalkingPoint.ByWhomId = data.SystemId;
            $scope.ModelTalkingPoint.TalkingPointByWhomName = data.EmployeeName;
            $scope.ModelTalkingPoint.TalkingPointByWhomCode = data.EmployeeCode;
        }
        else if ($scope.Name == 'Suggestions') {
            $scope.ModelMeetingSuggestion.ByWhomId = data.SystemId;
            $scope.ModelMeetingSuggestion.SuggestionsByWhomName = data.EmployeeName;
            $scope.ModelMeetingSuggestion.SuggestionsByWhomCode = data.EmployeeCode;
        }
        else if ($scope.Name == 'Action') {
            $scope.ModelActionablePoints.ByWhomId = data.SystemId;
            $scope.ModelActionablePoints.ActionByWhomName = data.EmployeeName;
            $scope.ModelActionablePoints.ActionByWhomCode = data.EmployeeCode;
        }
        else {
            $scope.ModelMeetingDecision.ByWhomId = data.SystemId;
            $scope.ModelMeetingDecision.DecisionByWhomName = data.EmployeeName;
            $scope.ModelMeetingDecision.DecisionByWhomCode = data.EmployeeCode;
        }

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.selectTalkingPointPopUp = function (index, data) {
        $scope.TalkingPointIndex = index;

        $scope.ModelTalkingPoint.Id = data.Id;
        angular.element(document.querySelector('#talkingPointPopUp')).modal('hide');
    };

    $scope.selectsuggestionsRecommendationPopUp = function (index, data) {
        $scope.SuggestionsRecommendationIndex = index;

        $scope.ByWhomId = data.Id;
        angular.element(document.querySelector('#suggestionsRecommendationPopUp')).modal('hide');

    };

    $scope.selectActionablePointsPopUp = function (index, data) {
        $scope.ActionablePointsIndex = index;

        $scope.ByWhomId = data.Id;
        angular.element(document.querySelector('#actionablePointsPopUp')).modal('hide');

    };

    $scope.selectMeetingDecisionPopUp = function (index, data) {
        $scope.MeetingDecisionIndex = index;

        $scope.ByWhomId = data.Id;
        angular.element(document.querySelector('#meetingDecisionPopUp')).modal('hide');

    };


    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };

    //#region Meeting Points Picture upload

    $scope.onBeginPBUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.ModelNew.Id))
                throw 'Please select/save the Meeting Points first'

            args.data = $scope.ModelNew.Id;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadPBUrl = "MeetingManagement/MeetingPoints/SaveMeetingPointsDefault";

    $scope.getFileList = function () {
        $http({
            method: 'POST', url: $scope.path + 'GetFileInfo', dataType: 'JSON',
            data: { Id: $scope.ModelNew.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                var str = response.data[0].Attachment;
                var extention = str.substr(str.indexOf('.'));
                $scope.Attachment = virtualPath.MeetingPointsTemplateImage + '/' + $scope.ModelNew.Id + extention;
                $scope.ModelNew.Attachment = response.data[0].Attachment;
                $scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }


    $scope.fileselect = function (e) {

    }
    $scope.errorPBPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ModelNew.Id))
            ShowResult('Please select/save first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    //#endregion Meeting Points Picture upload

    $scope.showTalkingPointListPopUp = function () {
        try {
            angular.element(document.querySelector('#talkingPointPopUp')).modal('show');
            $scope.getTalkingPointData();
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.closeTalkingPointPopUp = function () {

        angular.element(document.querySelector('#talkingPointPopUp')).modal('hide');
    };

    $scope.SaveTalkingPoint = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $scope.ModelTalkingPoint.MeetingItemHeaderId = $scope.ModelNew.Id;
            angular.copy($scope.ModelTalkingPoint, $scope.ModelTalking);

            if ($scope.talkingPointForm.$valid) {
                    $http({
                        method: 'POST',
                        url: 'MeetingManagement/MeetingPoints/CreateTalkingPoint',
                        data: { 'data': $scope.ModelTalkingPoint },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getTalkingPointData();
                            $scope.ModelTalkingPoint = Object.assign({}, $scope.ModelActionable);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
      
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.DeleteTalkingPoint = function () {
        
                $http({
                    method: 'POST',
                    url: 'MeetingManagement/MeetingPoints/deleteTalkingPoint/' + $scope.ModelTalkingPoint.Id,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getTalkingPointData();
                        $scope.ModelTalkingPoint = {};

                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
    };

    $scope.TalkingPointClear = function () {
        TalkingPointClearFields();
        return true;
    };

    function TalkingPointClearFields() {
        $scope.Action = 'Save';
        $scope.ModelTalkingPoint = Object.assign({}, $scope.ModelTalking);
    }


    $scope.showSuggestionsRecommendationListPopUp = function () {
        try {
            angular.element(document.querySelector('#suggestionsRecommendationPopUp')).modal('show');
            $scope.getSuggestionsRecommendationData();
            }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.closeSuggestionsRecommendationPopUp = function () {

        angular.element(document.querySelector('#suggestionsRecommendationPopUp')).modal('hide');
    };

    $scope.SaveSuggestionsRecommendation = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $scope.ModelMeetingSuggestion.MeetingItemHeaderId = $scope.ModelNew.Id;
            angular.copy($scope.ModelMeetingSuggestion, $scope.ModelMeetSug);

            if ($scope.suggestionsRecommendationForm.$valid) {
                
                    $http({
                        method: 'POST',
                        url: 'MeetingManagement/MeetingPoints/CreateSuggestionsRecommendation',
                        data: { 'data': $scope.ModelMeetingSuggestion },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getSuggestionsRecommendationData();
                            $scope.ModelMeetingSuggestion = {};
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
        
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
       
    };

    $scope.DeleteSuggestionsRecommendation = function () {

        $http({
            method: 'POST',
            url: 'MeetingManagement/MeetingPoints/deleteSuggestionsRecommendation/' + $scope.ModelMeetingSuggestion.Id,
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getSuggestionsRecommendationData();
                $scope.ModelMeetingSuggestion = Object.assign({}, $scope.ModelMeetSug);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.SuggestionsRecommendationClear = function () {
        SuggestionsRecommendationClearFields();
        return true;
    };

    function SuggestionsRecommendationClearFields() {
        $scope.Action = 'Save';
        $scope.ModelMeetingSuggestion = Object.assign({}, $scope.ModelMeetSug);
    }


    $scope.showActionablePointsPopUp = function () {
        try {
            angular.element(document.querySelector('#actionablePointsPopUp')).modal('show');
            $scope.getActionablePointsData();
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.closeActionablePointsPopUp = function () {

        angular.element(document.querySelector('#actionablePointsPopUp')).modal('hide');
    };

    $scope.SaveActionablePoints = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $scope.ModelActionablePoints.MeetingItemHeaderId = $scope.ModelNew.Id;
            angular.copy($scope.ModelActionablePoints, $scope.ModelActionable);

            if ($scope.actionablePointsForm.$valid) {
               
                    $http({
                        method: 'POST',
                        url: 'MeetingManagement/MeetingPoints/CreateActionablePoints',
                        data: { 'data': $scope.ModelActionablePoints },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getActionablePointsData();
                            $scope.ModelActionablePoints = {};
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }

                }
            }
      
        catch (ex) {
                    ShowResult(ex, 'failure');
                }
    };



    $scope.DeleteActionablePoints = function () {
        $http({
            method: 'POST',
            url: 'MeetingManagement/MeetingPoints/deleteActionablePoint/' + $scope.ModelActionablePoints.Id,
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getActionablePointsData();
                $scope.ModelActionablePoints = Object.assign({}, $scope.ModelActionable);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.ActionablePointsClear = function () {
        ActionablePointsClearFields();
        return true;
    };

    function ActionablePointsClearFields() {
        $scope.Action = 'Save';
        $scope.ModelActionablePoints = Object.assign({}, $scope.ModelActionable);
    }

    $scope.showMeetingDecisionPopUp = function () {
        try {
            angular.element(document.querySelector('#meetingDecisionPopUp')).modal('show');
            $scope.getMeetingDecisionData();
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.closeMeetingDecisionPopUp = function () {

        angular.element(document.querySelector('#meetingDecisionPopUp')).modal('hide');
    };

    $scope.SaveMeetingDecision = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $scope.ModelMeetingDecision.MeetingItemHeaderId = $scope.ModelNew.Id;
            angular.copy($scope.ModelMeetingDecision, $scope.ModelMeetingDec);

            if ($scope.meetingDecisionForm.$valid) {
                
                    $http({
                        method: 'POST',
                        url: 'MeetingManagement/MeetingPoints/CreateMeetingDecision',
                        data: { 'data': $scope.ModelMeetingDecision },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getMeetingDecisionData();
                            $scope.ModelMeetingDecision ={};
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
     
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        
        //angular.element(document.querySelector('#meetingDecisionPopUp')).modal('hide');
    };

    $scope.DeleteMeetingDecision = function () {

        $http({
            method: 'POST',
            url: 'MeetingManagement/MeetingPoints/deleteMeetingDecision/' + $scope.ModelMeetingDecision.Id,
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMeetingDecisionData();
                $scope.ModelMeetingDecision = Object.assign({}, $scope.ModelMeetingDec);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.MeetingDecisionClear = function () {
        MeetingDecisionClearFields();
        return true;
    };

    function MeetingDecisionClearFields() {
        $scope.Action = 'Save';
        $scope.ModelMeetingDecision = Object.assign({}, $scope.ModelMeetingDec);
        }
}