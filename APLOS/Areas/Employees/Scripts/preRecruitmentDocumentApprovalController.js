'use strict';
preRecruitmentDocumentApprovalController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function preRecruitmentDocumentApprovalController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Document Approve';
    $scope.Action = 'Approved';
    $scope.index = -1;
    $scope.message = null;

    $scope.preRecruitmentEmployee = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsImage: false,
        IsApproved: false
    };
    $scope.preRecruitmentEmpTraining = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsTrainingApproved: false
    };
    $scope.preRecruitmentEmpQualification = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsQualificationApproved: false
    };
    $scope.preRecruitmentEmpExperience = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsExperienceApproved: false
    };
    $scope.preRecruitmentDocument = {
        Id: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        IsDocumentApproved: false
    };
    $rootScope.searchDataByList = [
        {
            'name': 'Candidate Id',
            'value': 'Id'
        },
        {
            'name': 'Full Name',
            'value': 'FullName'
        },
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Budget Id',
            'value': 'BudgetId'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Position',
            'value': 'PositionName'
        }
    ];
    $scope.SelectionParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'AppAddedDateTime',
        searchBy: 'FullName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.message = '';
    $scope.getListUrl = 'employees/prerecruitmentdocumentapproval/getlist',
        baseService.init($scope.getListUrl, null, null, null, 'FullName', 'FullName');
    $scope.LoadDataList = function (pageno) {
        baseService.pagination(pageno)
            .then(function (data) {
                if (data.Error) {
                    return $scope.message = data.Message;
                } else {
                    $scope.preRecruitmentEmployees = data.Data.Rows;
                    $rootScope.total_count = data.Data.Total;
                    $scope.message = data.Message;
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.LoadDataList();

    $scope.showEntityPopUp = function () {
        $http.get('employees/prerecruitmentdocumentapproval/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    };

    $scope.showDocumentPopUp = function (obj, index) {
        $scope.index = index;
        $http.get('employees/prerecruitmentdocumentapproval/getemployeedata?eId=' + obj.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.employee = response.data[0];
                    $scope.imageSrc = virtualPath.EmpPic + '/' + $scope.employee.Image;
                }
            });
        $http.get('employees/prerecruitmentdocumentapproval/GetEmployeeDocumentData?eId=' + obj.Id)
            .then(function (response) {
                $scope.documentdataList = response.data;
            });

        $http.get('Recruitments/home/getqualificationdata?id=' + obj.Id)
            .then(function (response) {
                $scope.preRecruitmentEmpQualifications = response.data;
            });
        $http.get('Recruitments/home/getexperiencedata?id=' + obj.Id)
            .then(function (response) {
                $scope.preRecruitmentEmpExperiences = response.data;
            });
        $http.get('Recruitments/home/gettrainingdata?id=' + obj.Id)
            .then(function (response) {
                $scope.preRecruitmentEmpTrainings = response.data;
            });
        angular.element(document.querySelector('#DocumentPopUp')).modal('show');
    };

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
        //$scope.dwonloadUrl = $rootScope.QualificationDocument + '/' + data.FileId + extention;
        $scope.dwonloadUrl = virtualPath.QualificationDocument + '/' + data.FileId + extention;
    };

    $scope.ExperienceFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExperienceDocument + '/' + data.FileId + extention;
    };

    $scope.TrainingFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.TrainingDocument + '/' + data.FileId + extention;
    };

    $scope.DocFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var extention = baseService.getFileExtension(data.FileName);
        $scope.dwonloadUrl = virtualPath.PreRecruitmentDocument + '/' + data.FileId + extention;
    };

    $scope.savedisable = false;

    $scope.confirmSubmit = function () {
        try {
            if ($scope.employee.Submitted === false) {
                throw 'Candidate has not submitted the form yet.';
            }

            $scope.confirm = $scope.user;
            $scope.message_confirmation = 'Are you sure you want to submit? Once you approve it will go for Recruitment Approval.';
            angular.element(document.querySelector('#confirmSubmit')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure', 'DocumentPopUp');
        }
    };

    $scope.Approved = function () {
        try {
            if ($scope.employee.IsImage === false) {
                throw 'Approve image is required.';
            }
            for (var i = 0; i < $scope.preRecruitmentEmpQualifications.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpQualifications[i].FileName)
                    && $scope.preRecruitmentEmpQualifications[i].IsQualificationApproved === false) {
                    throw 'Qualification Approved is required';
                }
            }
            for (var i = 0; i < $scope.preRecruitmentEmpExperiences.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpExperiences[i].FileName)
                    && $scope.preRecruitmentEmpExperiences[i].IsExperienceApproved === false) {
                    throw 'Experience Approved is required';
                }
            }
            for (var i = 0; i < $scope.preRecruitmentEmpTrainings.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpTrainings[i].FileName)
                    && $scope.preRecruitmentEmpTrainings[i].IsTrainingApproved === false) {
                    throw 'Training Approved is required';
                }
            }
            for (var i = 0; i < $scope.documentdataList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.documentdataList[i].FileName)
                    && $scope.documentdataList[i].IsDocumentApproved === false) {
                    throw 'Document Approved is required';
                }

                if ($scope.documentdataList[i].OptionalOrMandatory === 'Mandatory' && baseService.isUndefinedOrNull($scope.documentdataList[i].FileName)) {
                    throw 'Document is mandatory for ' + $scope.documentdataList[i].ComplianceDocument+'.';
                }
            }
            $scope.savedisable = true;
            if ($scope.Action === 'Approved') {
                $http({
                    method: 'POST',
                    url: 'employees/prerecruitmentdocumentapproval/create',
                    data: {
                        'preRecruitmentEmployees': $scope.employee
                        , 'preRecruitmentEmpQualification': $scope.preRecruitmentEmpQualifications
                        , 'preRecruitmentEmpExperience': $scope.preRecruitmentEmpExperiences
                        , 'preRecruitmentEmpTraining': $scope.preRecruitmentEmpTrainings
                        , 'preRecruitmentDocument': $scope.documentdataList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'DocumentPopUp');
                        $scope.savedisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'DocumentPopUp');
                        $scope.savedisable = false;
                        $scope.preRecruitmentEmployees.splice($scope.index, 1);
                        angular.element(document.querySelector('#DocumentPopUp')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'DocumentPopUp');
                    $scope.savedisable = false;

                };
            }
        } catch (e) {
            ShowResult(e, 'failure', 'DocumentPopUp');
            $scope.savedisable = false;
        }
    };

}