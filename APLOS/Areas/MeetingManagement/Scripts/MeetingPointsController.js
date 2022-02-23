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
    $scope.ModelExpectedPerson = [];
    $scope.path = 'MeetingManagement/MeetingPoints/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.ActionTalkingPoint = 'Save';
    $scope.ActionSuggestionsRecommendation = 'Save';
    $scope.ActionActionablePoints = 'Save';
    $scope.ActionMeetingDecision = 'Save';
    $scope.ActionExpectedPerson = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $scope.employeeUrl2 = $scope.path + 'GetExpectedPersonList';
    $scope.talkingPointUrl = $scope.path + 'GettalkingPoint';
    //$scope.ExpectedPersonUrl = $scope.path + 'GetExpectedPerson';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.EmployeeListNew = [];
    $scope.ModelTemp = {
        Id: null,
        Department: null,
        MeetingType: null,
        ItemDetail: null,
        BackgroundIssueDetail: null,
        ActionApplicable: false,
        DecisionApplicable: false,
        IssueStatus: 'Active',
        Attachment: null,
        ByWhomId: null,
        ByWhomCode: null,
        ItemTitle: null,
        IssueMeetingItemTitle: null,
        IssueCritically: null,
        CostEstimation: null,
        MeetingLegDays: 7,
        CostApplicable: false,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelTalkingTemp = {
        Id: null,
        MeetingItemHeaderId: null,
        TalkingPoint: null,
        DiscussionDetail: null,
        ByWhomId: null,
        ByWhomCode: null,
        ByWhomName: null,
    };
    $scope.ModelTalkingPoint = Object.assign({}, $scope.ModelTalkingTemp);

    $scope.ModelMeetSug = {
        Id: null,
        MeetingItemHeaderId: null,
        Suggestion: null,
        DiscussionDetail: null,
        ByWhomId: null,
        ByWhomCode: null,
        ByWhomName: null,
    };
    $scope.ModelMeetingSuggestion = Object.assign({}, $scope.ModelMeetSug);

    $scope.ModelActionable = {
        Id: null,
        MeetingItemHeaderId: null,
        ActionToBeTaken: null,
        ActionablePointsId: null,
        ByWhomId: null,
        ByWhomCode: null,
        ByWhomName: null,
        TargetDate: null,
        Remarks: null,
        FinalStatus: null,
        CurrentStatus: null
    };
    $scope.ModelActionablePoints = Object.assign({}, $scope.ModelActionable);

    $scope.ModelMeetingDec = {
        Id: null,
        MeetingItemHeaderId: null,
        Decision: null,
        ByWhomId: null,
        ByWhomCode: null,
        ByWhomName: null,
        Remarks: null
    };
    $scope.ModelMeetingDecision = Object.assign({}, $scope.ModelMeetingDec);

    $scope.ModelExpected = {
        Id: null,
        MeetingItemHeaderId: null,
        ExpectedPersonId: null,
        ExpectedPersonCode: null,
        ExpectedPersonName: null
    };
    $scope.ModelExpectedPerson = Object.assign({}, $scope.ModelExpected);


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

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

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
        $scope.ActionTalkingPoint = 'Update';
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
        $scope.ActionSuggestionsRecommendation = 'Update';
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
        $scope.ActionActionablePoints = 'Update';
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
        $scope.ActionMeetingDecision = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.getExpectedPersonData = function () {
             $http({
                method: 'POST',
                url: $scope.path + "GetExpectedPersonList",
                data: { column: $scope.searchBy, value: $scope.search, meetingItemHeaderId: $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                //  $scope.ModelExpectedPersonList = response.data;
                $scope.EmployeeListNew = response.data;

            });
        }
   
    $scope.Clear = function () {
        $scope.ModelNew = {
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
            CostApplicable: false,
            Remarks: null
        };
        $scope.Action = 'Save';
    };
   
    $scope.Save = function () {

        try {
            angular.copy($scope.ModelNew, $scope.ModelTemp);
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.ModelNewForm.$valid) {
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
                            $scope.Clear();
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
                    scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    


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
            $scope.ModelTalkingPoint.ByWhomName = data.EmployeeName;
            $scope.ModelTalkingPoint.ByWhomCode = data.EmployeeCode;
        }
        else if ($scope.Name == 'Suggestions') {
            $scope.ModelMeetingSuggestion.ByWhomId = data.SystemId;
            $scope.ModelMeetingSuggestion.ByWhomName = data.EmployeeName;
            $scope.ModelMeetingSuggestion.ByWhomCode = data.EmployeeCode;
        }
        else if ($scope.Name == 'Action') {
            $scope.ModelActionablePoints.ByWhomId = data.SystemId;
            $scope.ModelActionablePoints.ByWhomName = data.EmployeeName;
            $scope.ModelActionablePoints.ByWhomCode = data.EmployeeCode;
        }
        else {
            $scope.ModelMeetingDecision.ByWhomId = data.SystemId;
            $scope.ModelMeetingDecision.ByWhomName = data.EmployeeName;
            $scope.ModelMeetingDecision.ByWhomCode = data.EmployeeCode;
        }
        //else {
        //    $scope.ModelExpectedPerson.ExpectedPersonId = data.SystemId;
        //    $scope.ModelExpectedPerson.ExpectedPersonName = data.EmployeeName;
        //    $scope.ModelExpectedPerson.ExpectedPersonCode = data.EmployeeCode;
        //}

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.selectTalkingPointPopUp = function (index, data) {
        $scope.TalkingPointIndex = index;

        $scope.TalkingPointId = data.Id;
        angular.element(document.querySelector('#talkingPointPopUp')).modal('hide');
    };

    $scope.selectsuggestionsRecommendationPopUp = function (index, data) {
        $scope.SuggestionsRecommendationIndex = index;

        $scope.MeetingSuggestionId = data.Id;
        angular.element(document.querySelector('#suggestionsRecommendationPopUp')).modal('hide');

    };

    $scope.selectActionablePointsPopUp = function (index, data) {
        $scope.ActionablePointsIndex = index;

        $scope.MeetingActionId = data.Id;
        angular.element(document.querySelector('#actionablePointsPopUp')).modal('hide');

    };

    $scope.selectMeetingDecisionPopUp = function (index, data) {
        $scope.MeetingDecisionIndex = index;

        $scope.MeetingDecisionId = data.Id;
        angular.element(document.querySelector('#meetingDecisionPopUp')).modal('hide');

    };

    $scope.selectExpectedPersonPopUp = function (index, data) {
        $scope.ExpectedPersonIndex = index;

        $scope.ExpectedPersonId = data.Id;
        angular.element(document.querySelector('#expectedPersonPopUp')).modal('hide');

    };


    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };


    //#region Meeting Points Picture upload

    $scope.FilesList = [];
    $scope.TempFileDesc = '';
    $scope.onBeginPBUpload = function (args) {
        try {
            if ($scope.ModelNew.Id == null || $scope.ModelNew.Id == '')
                throw 'Please select/save the To Do first'

            var _data = { Id: $scope.ModelNew.Id };

            args.data = JSON.stringify(_data);


        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadPBUrl = "MeetingManagement/MeetingPoints/SaveMeetingPointsDefault";
   

    $scope.errorPBPicUpload = function (e) {

        if ($scope.ModelNew.Id == null || $scope.ModelNew.Id == '')
            throw 'Please select/save the To Do first'

        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    $scope.getFileList = function () {
        $scope.TempFileDesc = '';
        $scope.FilesList = [];
        $http({
            method: 'POST', url: $scope.path + 'GetFileInfo', dataType: 'JSON',
            data: { Id: $scope.ModelNew.Id }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.FilesList = response.data;
                $scope.ModelNew.Attachment = response.data[0].Attachment;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
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
                        $scope.TalkingPointClear();
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

    $scope.ConfirmTalkingPoint = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelTalkingPoint.Id))
            $scope.message_confirmation = 'Are you sure want to delete ?';
        angular.element(document.querySelector('#confirmTalkingPoint')).modal('show');
    }

    $scope.DeleteTalkingPoint = function () {

        $http({
            method: 'POST',
            url: 'MeetingManagement/MeetingPoints/DeleteTalkingPointData',
            data: { id: $scope.ModelTalkingPoint.Id },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getTalkingPointData();
                $scope.TalkingPointClear();

            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.TalkingPointClear = function () {
        $scope.ModelTalkingPoint = {
            Id: null,
            MeetingItemHeaderId: null,
            TalkingPoint: null,
            DiscussionDetail: null,
            ByWhomId: null,
            ByWhomCode: null,
            ByWhomName: null,
        };
        $scope.ActionTalkingPoint = 'Save';
    };


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
                        $scope.SuggestionsRecommendationClear();
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

    $scope.ConfirmSuggestionsRecommendation = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelMeetingSuggestion.Id))
            $scope.message_confirmation = 'Are you sure want to delete ?';
        angular.element(document.querySelector('#confirmSuggestionsRecommendation')).modal('show');
    }

    $scope.DeleteSuggestionsRecommendation = function () {

        $http({
            method: 'POST',
            url: 'MeetingManagement/MeetingPoints/DeleteSuggestionsRecommendationData',
            data: { id: $scope.ModelMeetingSuggestion.Id },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getSuggestionsRecommendationData();
                $scope.SuggestionsRecommendationClear();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };


    $scope.SuggestionsRecommendationClear = function () {
        $scope.ModelMeetingSuggestion = {
            Id: null,
            MeetingItemHeaderId: null,
            Suggestion: null,
            DiscussionDetail: null,
            ByWhomId: null,
            ByWhomCode: null,
            ByWhomName: null,
        };
        $scope.ActionSuggestionsRecommendation = 'Save';
    };


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
                        $scope.ActionablePointsClear();
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

    $scope.ConfirmActionablePoints = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelActionablePoints.Id))
            $scope.message_confirmation = 'Are you sure want to delete ?';
        angular.element(document.querySelector('#confirmActionablePoints')).modal('show');
    }

    $scope.DeleteActionablePoints = function () {
        $http({
            method: 'POST',
            url: 'MeetingManagement/MeetingPoints/DeleteActionablePointData',
            data: { id: $scope.ModelActionablePoints.Id },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getActionablePointsData();
                $scope.ActionablePointsClear();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.ActionablePointsClear = function () {
        $scope.ModelActionablePoints = {
            Id: null,
            MeetingItemHeaderId: null,
            ActionToBeTaken: null,
            ActionablePointsId: null,
            ByWhomId: null,
            ByWhomCode: null,
            ByWhomName: null,
            TargetDate: null,
            Remarks: null,
            FinalStatus: null,
            CurrentStatus: null
        };
        $scope.ActionActionActionablePoints = 'Save';
    };



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
                        $scope.MeetingDecisionClear();
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

    $scope.ConfirmMeetingDecision = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelMeetingDecision.Id))
            $scope.message_confirmation = 'Are you sure want to delete ?';
        angular.element(document.querySelector('#confirmMeetingDecision')).modal('show');
    }

    $scope.DeleteMeetingDecision = function () {

        $http({
            method: 'POST',
            url: 'MeetingManagement/MeetingPoints/DeleteMeetingDecisionData',
            data: { id: $scope.ModelMeetingDecision.Id },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMeetingDecisionData();
                $scope.MeetingDecisionClear();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.MeetingDecisionClear = function () {
        $scope.ModelMeetingDecision = {
            Id: null,
            MeetingItemHeaderId: null,
            Decision: null,
            ByWhomId: null,
            ByWhomCode: null,
            ByWhomName: null,
            Remarks: null
        };
        $scope.ActionMeetingDecision = 'Save';
    };

    $scope.showExpectedPersonPopUp = function () {
        try {
            angular.element(document.querySelector('#expectedPersonPopUp')).modal('show');
            $scope.getExpectedPersonData();
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.closeExpectedPersonPopUp = function () {

        angular.element(document.querySelector('#expectedPersonPopUp')).modal('hide');
    };

    $scope.SaveExpectedPerson = function () {
        try {
            $scope.SaveEmployeeListNew = [];

            for (var i = 0; i < $scope.EmployeeListNew.length; i++) {
                var obj = {};
                obj.MeetingItemHeaderId = $scope.ModelNew.Id;
                obj.Id = $scope.EmployeeListNew[i].Id;
                obj.ExpectedPersonId = $scope.EmployeeListNew[i].SystemId;
                $scope.SaveEmployeeListNew.push(obj);
                obj = {};
            }

            $http({
                method: 'POST',
                url: 'MeetingManagement/MeetingPoints/CreateExpectedPerson',
                data: { 'data': $scope.SaveEmployeeListNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getExpectedPersonData();
                    $scope.ExpectedPersonClear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }

        catch (ex) {
            ShowResult(ex, 'failure');
        }

        //angular.element(document.querySelector('#meetingDecisionPopUp')).modal('hide');
    };

    //$scope.ConfirmExpectedPerson = function () {
    //    if (!baseService.isUndefinedOrNull($scope.ModelExpectedPerson.Id))
    //        $scope.message_confirmation = 'Are you sure want to delete ?';
    //    angular.element(document.querySelector('#confirmExpectedPerson')).modal('show');
    //}

    //$scope.DeleteExpectedPerson = function () {

    //    $http({
    //        method: 'POST',
    //        url: 'MeetingManagement/MeetingPoints/DeleteExpectedPersonData',
    //        data: { id: $scope.ModelExpectedPerson.Id },
    //        dataType: 'JSON'
    //    }).then(function (response) {
    //        if (response.data.Error === true)
    //            ShowResult(response.data.Message, 'failure');
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            $scope.getExpectedPersonData();
    //            $scope.ExpectedPersonClear();
    //        }
    //        function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    });
    //};

    $scope.message_confirmation = null;
    $scope.removeExpectedPerson = function (obj) {

        $scope.ModelExpectedPerson = obj.data;
        if (!baseService.isUndefinedOrNull($scope.ModelExpectedPerson.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmExpectedPerson')).modal('show');
    }

    $scope.DeleteExpectedPerson = function () {
        $http({
            method: 'POST',
            url: 'MeetingManagement/MeetingPoints/DeleteExpectedPersonData',
            data: { id: $scope.ModelExpectedPerson.Id },
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getExpectedPersonData();
                $scope.ExpectedPersonClear();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };


    $scope.ExpectedPersonClear = function () {
        $scope.ModelExpectedPerson = {
            Id: null,
            MeetingItemHeaderId: null,
            ExpectedPersonId: null,
            ExpectedPersonCode: null,
            ExpectedPersonName: null
        };
        $scope.ActionMeetingDecision = 'Save';
    };

    $scope.master = {
        SystemID: null,
        IsFix: 'true',
        CheckBox: true
    };



    $scope.OK = function () {
        
            try {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    if ($scope.EmployeeList[i].CheckBoxSelect == true) {
                        if (checkDoubleEmployee($scope.EmployeeListNew, $scope.EmployeeList[i].SystemId) === false) {
                            $scope.EmployeeListNew.push($scope.EmployeeList[i]);
                        }
                    }
                }
                var eDialog = $("#dialogShiftInfo").data("ejDialog");
                eDialog.close();

                //if ($rootScope.isCollapsed) {
                //    $rootScope.toggle();
                //}

            } catch (e) {
                ShowResult(e, "failure");
            }
  
    };

    function checkDoubleEmployee(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemId === Id) {
                return true;
            }
        }
        return false;
    }


    $scope.EmployeeList = [];
    $scope.GetEmployeeInformation = function () {

        try {

            var eDialog = $("#dialogShiftInfo").data("ejDialog");
            eDialog.open();

            $scope.EmployeeList = [];
            $http({
                method: 'GET',
                url: 'MeetingManagement/MeetingPoints/GetEmployeeInformation'
            }).then(function successCallback(response) {
                $scope.EmployeeList = response.data;
            });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SearchedEmployeesList = [];
    $scope.LoadEmployees = function () {
        try {
            var eDialog = $("#dialogShiftInfo").data("ejDialog");
            eDialog.open();

            $http.get($scope.searchShiftUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.SearchedEmployeesList = response.data.LeaveInfo;
                        //eDialog.close();
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#empInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                $scope.EmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#empInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {

                    $scope.EmployeeList[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].EmployeeId == filtered[j].EmployeeId)
                            // $scope.EmployeeList[i].isSelect = true;
                            $scope.EmployeeList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    $scope.EmployeeList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].Id == filtered[j].Id)
                            $scope.EmployeeList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChangeemployee });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    };

    $scope.remove = function (obj) {
        var gridObj = $("#GridNew").data("ejGrid");

        for (var i = 0; i < $scope.EmployeeListNew.length; i++) {
            if ($scope.EmployeeListNew[i].SystemId === obj.data.SystemId) {
                $scope.EmployeeListNew.splice(i, 1);
                break;
            }
        }
        //gridObj.refreshContent();
    };

}