var app = angular.module("recruitmentsApp", ['ngRoute', 'angularUtils.directives.dirPagination']);

app.controller("preRecruitmentDocumentApprovalController", function ($scope, $rootScope, $routeParams, $location, $http, $filter, baseService, fileReader) {
    $rootScope.title = 'Document Approve';
    $scope.Action = "Approved"
    $scope.id = null;
    if (window.location.href.indexOf("?") !== -1) {
        $scope.id = window.location.href.split('?')[1].split('=')[1];
    };

    $scope.preRecruitmentEmployee = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsImage: false,
        IsApproved: false
    }

    $scope.preRecruitmentEmpTraining = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsTrainingApproved: false
    }

    $scope.preRecruitmentEmpQualification = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsQualificationApproved: false
    }

    $scope.preRecruitmentEmpExperience = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsExperienceApproved: false
    }

    $scope.preRecruitmentDocument = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsDocumentApproved: false
    }

    $scope.employees = [];
    $scope.LoadEmployeeData = function () {
        $http.get('/prerecruitmentdocumentapproval/getemployeedata?eid=' + $scope.id)
            .then(function (response) {
                $scope.employees = response.data;
                $scope.imageSrc = "/EmpPic/" + $scope.employees[0].Image;
                $scope.ApprovedBy = $scope.employees[0].DocumentApprovedById;
            });
    };
    $scope.LoadEmployeeData();

    $scope.LoadQualificationData = function () {
        $http.get('Recruitments/home/getqualificationdata?id=' + $scope.id)
            .then(function (response) {
                $scope.preRecruitmentEmpQualifications = response.data;
               
            });
    };
    $scope.LoadQualificationData();

    $scope.LoadExperienceData = function () {
        $http.get('Recruitments/home/getexperiencedata?id=' + $scope.id)
            .then(function (response) {
                $scope.preRecruitmentEmpExperiences = response.data;
               
            });
    };
    $scope.LoadExperienceData();

    $scope.LoadTrainingData = function () {
        $http.get('Recruitments/home/gettrainingdata?id=' + $scope.id)
            .then(function (response) {
                $scope.preRecruitmentEmpTrainings = response.data;
                
            });
    };
    $scope.LoadTrainingData();

    $scope.LoadDocumentData = function () {
        $http.get('/prerecruitmentdocumentapproval/getemployeedocumentdata?eid=' + $scope.id)
            .then(function (response) {
                $scope.preRecruitmentDocuments = response.data;
               
            });
    };
    $scope.LoadDocumentData();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = 'F:\Aplos\APLOS.New\APLOS\EmployeeProfile\QualificationDoc' + '/' + data.FileId + extention;
    };

    $scope.ExperienceFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = 'F:\Aplos\APLOS.New\APLOS\EmployeeProfile\ExperienceDoc' + '/' + data.FileId + extention;
    };

    $scope.TrainingFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = 'F:\Aplos\APLOS.New\APLOS\EmployeeProfile\TrainingDoc' + '/' + data.FileId + extention;
    };

    $scope.DocFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = 'F:\Aplos\APLOS.New\APLOS\EmployeeProfile\Documents' + '/' + data.FileId + extention;
    };
    $scope.savedisable = false;

    $scope.Approved = function () {
        for (var i = 0; i < $scope.employees.length; i++) {
            $scope.employees[i].IsImage = $scope.preRecruitmentEmployee.IsImage;
            $scope.employees[i].ApprovedBy = $scope.ApprovedBy;
        }
        for (var i = 0; i < $scope.preRecruitmentEmpQualifications.length; i++) {
            $scope.preRecruitmentEmpQualifications[i].ApprovedBy = $scope.ApprovedBy;
        }
        for (var i = 0; i < $scope.preRecruitmentEmpExperiences.length; i++) {
            $scope.preRecruitmentEmpExperiences[i].ApprovedBy = $scope.ApprovedBy;
        }
        for (var i = 0; i < $scope.preRecruitmentEmpTrainings.length; i++) {
            $scope.preRecruitmentEmpTrainings[i].ApprovedBy = $scope.ApprovedBy;
        }
        for (var i = 0; i < $scope.preRecruitmentDocuments.length; i++) {
            $scope.preRecruitmentDocuments[i].ApprovedBy = $scope.ApprovedBy;
        }
        $scope.$broadcast('show-errors-check-validity');
        try {
            $scope.savedisable = true;
            if ($scope.Action === 'Approved') {
                    $http({
                        method: 'POST',
                        url: '/prerecruitmentdocumentapproval/create',
                        data: {
                            'preRecruitmentEmployees': $scope.employees[0]
                            , 'preRecruitmentEmpQualification': $scope.preRecruitmentEmpQualifications
                            , 'preRecruitmentEmpExperience': $scope.preRecruitmentEmpExperiences
                            , 'preRecruitmentEmpTraining': $scope.preRecruitmentEmpTrainings
                            , 'preRecruitmentDocument': $scope.preRecruitmentDocuments
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.savedisable = false;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.savedisable = false;
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                        $scope.savedisable = false;
                    }
                }
        } catch (e) {
            ShowResult(e, 'failure');
            $scope.savedisable = false;
        }
    }

});
app.factory('baseService', baseService)
app.filter('dateFilter', dateFilter)
app.filter('dateFiltering', dateFiltering)
app.factory('fileReader', fileReader)
