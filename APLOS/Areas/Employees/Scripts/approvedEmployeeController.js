'use strict';
approvedEmployeeController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function approvedEmployeeController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Recruitment Approval';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'employees/approvedemployee/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.message = null;
    $scope.employeeInformation = {
        SystemId: null,
        EmployeeId: null,
        PreRecruitmentEmployeeId: null,
        EmployeeCode: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        UnitId: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        SubdivisionID: null,
        LineId: null,
        DesignationGroupId: null,
        DesignationSystemID: null,
        BudgetCode: null,
        PositionID: null,
        IsDirect: null,
        SalaryPercentage: null,
        CardNumber: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        EmployeeName: null,
        NickName: null,
        LocalEmployeeName: null,
        EmpPicPath: null,
        EmpType: null,
        EmploymentType: null,
        EmployeeGroupSystemID: null,
        JobLocationID: null,
        DOB: null,
        DOJ: null,
        DOCIsDay: null,
        DOCDay: null,
        DOCIsMonth: null,
        DOCMonth: null,
        DOC: null,
        DOS: null,
        IsConfirmed: null,
        ReActiveDate: null,
        EmployeeStatus: null,
        NationalID: null,
        TIN: null,
        CitizenID: null,
        FatherName: null,
        MotherName: null,
        ReligionID: null,
        CivilStatusID: null,
        BloodGroupID: null,
        GenderID: null,
        SpouseName: null,
        SpouseNationalID: null,
        SpouseOccupation: null,
        NoOfChildren: null,
        PresentAddress1: null,
        PresentAddress2: null,
        ParmanentAddress1: null,
        ParmanentAddress2: null,
        PresThanaID: null,
        ParmThanaID: null,
        PresPostOfficeID: null,
        ParmPostOfficeID: null,
        PresZipCode: null,
        ParmZipCode: null,
        PresDistrictID: null,
        ParmDistrictID: null,
        PresCountryID: null,
        ParmCountryID: null,
        PresCityID: null,
        ParmCityID: null,
        PresAreaID: null,
        ParmAreaID: null,
        TelePhnNo: null,
        CellPhnNo: null,
        EmailId: null,
        BudgetCategoryID: null,
        EmployeeCategorySystemID: null,
        LVPolicyMasterSystemID: null,
        SalaryRuleMasterSystemID: null,
        BankSystemID: null,
        BankName: null,
        BankAccNo: null,
        BankAddedBy: null,
        BankDateAdded: null,
        BankUpdatedBy: null,
        BankDateUpdated: null,
        RegisterFP: null,
        RegisterProximate: null,
        SuperViser: null,
        IsSlvDevReg: null,
        IsAttdnProcBaseOnDeviceData: null,
        SubSecStrucSystemID: null,
        AddedBy: null,
        DateAdded: null,
        UpdatedBy: null,
        DateUpdated: null,
        EmrCntPer1Name: null,
        EmrCntPer1CellNo: null,
        EmrCntPer2Name: null,
        EmrCntPer2CellNo: null,
        GivenDesignationId: null,
        LegalDesignationId: null,
        AgreedDOJ: null,
        TotalSalary: null,
        SpecialReviewDuration: null,
        SpecialReviewAmount: null,
        EmrCntPer1CellNo2: null,
        EmrCntPer1CellNo3: null,
        EmrCntPer2CellNo2: null,
        EmrCntPer2CellNo3: null,
        IsApproved: null,
        ApprovedBy: null,
        ApprovalDateTime: null,
        ApprovedFromIP: null
    };

    $scope.SelectionParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'EmployeeName',
        searchBy: 'EmployeeName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GLUrl3 = 'employees/approvedemployee/getlist/',
        $scope.LoadDataList = function (pageno) {
            baseService.paginationBase($scope.GLUrl3, pageno, $scope.SelectionParameters)
                .then(function (data) {
                    if (data.Error) return $scope.message = data.Message;
                    else {
                        $scope.preRecruitmentEmployees = data.Rows;
                        $scope.SelectionParameters.total_count = data.Total;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
    $scope.LoadDataList();

    $scope.showEntityPopUp = function () {
        $http.get('employees/approvedemployee/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    }

    $scope.showDocumentPopUp = function (obj, index) {
        $scope.index = index;
        $http.get('employees/approvedemployee/getemployeedata?eId=' + obj.SystemId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.employee = response.data[0];
                    $scope.imageSrc = virtualPath.EmployeePic + '/' + $scope.employee.EmpPicPath;
                }
            });

        $http.get('employees/approvedemployee/getqualificationdata?eId=' + obj.SystemId)
            .then(function (response) {
                $scope.empQualifications = response.data;
            });
        $http.get('employees/approvedemployee/getexperiencedata?eId=' + obj.SystemId)
            .then(function (response) {
                $scope.empExperiences = response.data;
            });
        $http.get('employees/approvedemployee/gettrainingdata?eId=' + obj.SystemId)
            .then(function (response) {
                $scope.empTrainings = response.data;
            });

        $http.get('employees/approvedemployee/getemployeedocumentdata?eId=' + obj.SystemId)
            .then(function (response) {
                $scope.documentdataList = response.data;
            });
        angular.element(document.querySelector('#DocumentPopUp')).modal('show');
    }

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
        $scope.dwonloadUrl = virtualPath.EmployeeQualificationDocument + '/' + data.FileId + extention;
    };

    $scope.ExperienceFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeExperienceDocument + '/' + data.FileId + extention;
    };

    $scope.TrainingFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeTrainingDocument + '/' + data.FileId + extention;
    };

    $scope.DocFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeDocument + '/' + data.FileId + extention;
    };
    $scope.savedisable = false;

    //$scope.confirmSubmit = function () {
    //    $scope.confirm = $scope.user;
    //    $scope.message_confirmation = 'Are you sure you want to submit? Once you approve it will go for Recruitment Approval.';
    //    angular.element(document.querySelector('#confirmSubmit')).modal('show');
    //};

    $scope.Action = 'Approved';
    $scope.Approved = function () {
        try {
            if ($scope.employee.IsImage === false) {
                throw 'Approve image is required.';
            }
            for (var i = 0; i < $scope.empQualifications.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.empQualifications[i].FileName)
                    && $scope.empQualifications[i].IsQualificationApproved === false) {
                    throw 'Qualification Approved is required';
                }
            }
            for (var i = 0; i < $scope.empExperiences.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.empExperiences[i].FileName)
                    && $scope.empExperiences[i].IsExperienceApproved === false) {
                    throw 'Experience Approved is required';
                }
            }
            for (var i = 0; i < $scope.empTrainings.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.empTrainings[i].FileName)
                    && $scope.empTrainings[i].IsTrainingApproved === false) {
                    throw 'Training Approved is required';
                }
            }
            for (var i = 0; i < $scope.documentdataList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.documentdataList[i].FileName)
                    && $scope.documentdataList[i].IsDocumentApproved === false) {
                    throw 'Document Approved is required';
                }
            }
            $scope.savedisable = true;
            if ($scope.Action === 'Approved') {
                $http({
                    method: 'POST',
                    url: 'employees/approvedemployee/create',
                    data: {
                        'employeeInformation': $scope.employee
                        , 'empAcademicQualificationInformations': $scope.empQualifications
                        , 'empExperienceInformations': $scope.empExperiences
                        , 'empTrainingInformations': $scope.empTrainings
                        , 'employeeDocuments': $scope.documentdataList
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

                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'DocumentPopUp');
            $scope.savedisable = false;
        }
    }
}