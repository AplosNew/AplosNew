'use strict';
MeetingPointsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MeetingPointsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Meeting Points';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'MeetingManagement/MeetingPoints/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.path = 'MeetingManagement/MeetingPoints/';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $scope.talkingPointUrl = $scope.path + 'GettalkingPoint';

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            //for (var i = 0; i < response.data.length; i++) {
            //    response.data[i].AddedDate = new Date(response.data[i].AddedDate);
            //}
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    
    $scope.ModelTemp = {
        Id: null,
        Department: null,
        MeetingType: null,
        BackgroundIssueDetail: null,
        ActionApplicable: null, 
        ItemTitle: null,
        ItemDetail: null,
        CostEstimation: null,
        IssueStatus: 'Active',
        ByWhomId: null,
        ByWhomName: null,
        IssueMeetingItemTitle:null,
        IssueCritically: null,
        DecisionApplicable: true,
        CostApplicable: null,
        MeetingLegDays: 7,
        Remarks: null, 
        Attachment: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelMeet = {
        Id: null,
        Suggestion: null,
        DiscussionDetail: null,
        ByWhomId: null
        };
    $scope.ModelMeetingPoint = Object.assign({}, $scope.ModelMeet);

    $scope.ModelMeetSug = {
        Id: null,
        TalkingPointId: null,
        DiscussionDetail: null,
        ByWhomId: null
    };
    $scope.ModelMeetingSuggestion = Object.assign({}, $scope.ModelMeetSug);

    $scope.ModelActionable = {
        Id: null,
        ActionToBeTaken: null,
        ByWhomId: null,
        Status: null
    };
    $scope.ModelActionablePoints = Object.assign({}, $scope.ModelActionable);

    $scope.ModelMeetingDec = {
        Id: null,
        Decision: null,
        ByWhomId: null,
        Remarks: null
    };
    $scope.ModelMeetingDecision = Object.assign({}, $scope.ModelMeetingDec);


    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
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
                 /*   ClearFields(response.data.Sequence);*/
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

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
                    /*ClearFields(response.data.Sequence);*/
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

    //$scope.talkingPointLParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: 'asc',
    //    sort: 'Id, TalkingPointId, DiscussionDetail, ByWhomId',
    //    searchBy: 'Id',
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};


    $scope.showEmployeeListPopUp = function () {
        try {
            
          
            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                //$scope.employeeParameters.plantId = $scope.fileNew.PlantId;
                //$scope.employeeParameters.partyAccountGroupId = $scope.fileNew.PartyAccountGroupId;
                //$scope.employeeParameters.partyId = $scope.fileNew.PartyId;
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
        
        $scope.ModelNew.ByWhomId = data.SystemId;
        $scope.ModelNew.ByWhomName = data.EmployeeName;
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };

    $scope.selectTalkingPointPopUp = function (index, data) {
        $scope.TalkingPointIndex = index;

        $scope.ModelMeetingPoint.Id = data.Id;
        //$scope.ModelNew.ByWhomName = data.EmployeeName;
        angular.element(document.querySelector('#talkingPointPopUp')).modal('hide');
    };

    $scope.selectsuggestionsRecommendationPopUp = function (index, data) {
        $scope.SuggestionsRecommendationIndex = index;

        $scope.ByWhomId = data.Id;
        //$scope.ModelNew.ByWhomName = data.EmployeeName;
        angular.element(document.querySelector('#suggestionsRecommendationPopUp')).modal('hide');

    };
    
    $scope.selectActionablePointsPopUp = function (index, data) {
        $scope.ActionablePointsIndex = index;

        $scope.ByWhomId = data.Id;
        //$scope.ModelNew.ByWhomName = data.EmployeeName;
        angular.element(document.querySelector('#actionablePointsPopUp')).modal('hide');

    };

    $scope.selectMeetingDecisionPopUp = function (index, data) {
        $scope.MeetingDecisionIndex = index;

        $scope.ByWhomId = data.Id;
        //$scope.ModelNew.ByWhomName = data.EmployeeName;
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
                var str = response.data[0].PicFileName;
                var extention = str.substr(str.indexOf('.'));
                $scope.PicFileName = virtualPath.MeetingPointsTemplateImage + '/' + $scope.ModelNew.Id + extention;
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
            ShowResult('Please select/save the Meeting Points first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    //#endregion Meeting Points Picture upload

    $scope.showTalkingPointListPopUp = function () {
        try {

            angular.element(document.querySelector('#talkingPointPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.closeTalkingPointPopUp = function () {

        angular.element(document.querySelector('#talkingPointPopUp')).modal('hide');
    };

    $scope.SaveMeetingPoint = function () {
        $scope.$broadcast('show-errors-check-validity');
       
            $http({
                method: 'POST',
                url: 'MeetingManagement/MeetingPoints/CreateMeetingPoint',
                data: { 'data': $scope.ModelMeetingPoint },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    /*   ClearFields(response.data.Sequence);*/
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

 
        angular.element(document.querySelector('#talkingPointPopUp')).modal('hide');
    };


    $scope.showSuggestionsRecommendationListPopUp = function () {
        try {
            angular.element(document.querySelector('#suggestionsRecommendationPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.closeSuggestionsRecommendationPopUp = function () {

        angular.element(document.querySelector('#suggestionsRecommendationPopUp')).modal('hide');
    };

    $scope.SaveSuggestionsRecommendation = function () {
        $scope.$broadcast('show-errors-check-validity');

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
                /*   ClearFields(response.data.Sequence);*/
                $scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }


        angular.element(document.querySelector('#suggestionsRecommendationPopUp')).modal('hide');
    };

    $scope.showActionablePointsPopUp = function () {
        try {
            angular.element(document.querySelector('#actionablePointsPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.closeActionablePointsPopUp = function () {

        angular.element(document.querySelector('#actionablePointsPopUp')).modal('hide');
    };

    $scope.SaveActionablePoints = function () {
        $scope.$broadcast('show-errors-check-validity');

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
                /*   ClearFields(response.data.Sequence);*/
                $scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }


        angular.element(document.querySelector('#actionablePointsPopUp')).modal('hide');
    };

    $scope.showMeetingDecisionPopUp = function () {
        try {
            angular.element(document.querySelector('#meetingDecisionPopUp')).modal('show');
        }
        catch (e)
        {
            ShowResult(e, 'failure');
        }
    };

    $scope.closeMeetingDecisionPopUp = function () {

        angular.element(document.querySelector('#meetingDecisionPopUp')).modal('hide');
    };

    $scope.SaveMeetingDecision = function () {
        $scope.$broadcast('show-errors-check-validity');

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
                /*   ClearFields(response.data.Sequence);*/
                $scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
        angular.element(document.querySelector('#meetingDecisionPopUp')).modal('hide');
    };
}