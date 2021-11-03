'use strict';
candidateAdministrationController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$cookies'];
function candidateAdministrationController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $cookies) {
    $rootScope.title = 'Candidate Administration';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'employees/candidateadministration/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.savedisable = false;

    $scope.preRecruitmentEmployee = {
        Id: null,
        Image: null,
        InterviewRankingId: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        UnitId: null,
        PositionID: null,
        BudgetId: null,
        IsDirect: true,
        FullName: null,
        Gender: null,
        NationalID: null,
        DOBE: null,
        Phone: null,
        Email: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        NickName: null,
        EmployeeName: null,
        EmpType: null,
        Status: null,
        TIN: null,
        FatherName: null,
        MotherName: null,
        CitizenID: null,
        ReligionID: null,
        CivilStatusID: null,
        BloodGroupID: null,
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
        PresCityID: null,
        ParmCityID: null,
        PresCountryID: null,
        EmrCntPer1Name: null,
        EmrCntPer2Name: null,
        EmrCntPer1CellNo: null,
        EmrCntPer2CellNo: null,
        SubmitDateTime: null,
        SelectionDateTime: null,
        SelectedBy: null,
        Submitted: false,
        ReadyForCandidateAccess: false,
        AppAddedDateTime: null,
        AppAddedBy: null,
        SelectionStatus: null,
        MarriagedayCelebrationDate: null,
        BirthdayCelebrationDate: null,
        SpecialReviewAmount: null,
        SpecialReviewDuration: null,
        TotalSalary: null,
        AgreedDOJ: null,
        PresentArea: null,
        ParmanentArea: null,
        PresPostOfficeName: null,
        PresCountryName: null,
        PresDistrictName: null,
        PresThanaName: null,
        IsKnownPerson: false,
        NumberOfKnownPerson: 1,
        EmrCntPer1CellNo2: null,
        EmrCntPer1CellNo3: null,
        EmrCntPer2CellNo2: null,
        EmrCntPer2CellNo3: null,
        HasPartner: false,
        SalaryRangeForTax: 0,
        ApplyingAsFresher: false
    };

    cboService.getCboPlantByCompany(null, function (result) {
        $scope.plantList = result;
    });

    $scope.searchbyEmployeelist = [
        {
            'name': 'Candidate Id',
            'value': 'Id'
        },
        {
            'name': 'Candidate Name',
            'value': 'FullName'
        },
        {
            'name': 'Email',
            'value': 'Email'
        },
        {
            'name': 'Phone',
            'value': 'Phone'
        }
    ];

    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'FullName',
        searchBy: 'FullName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getEmployeeData = function () {
        $scope.preRecruitmentEmployee.Id = null;
        $scope.preRecruitmentEmployee.FullName = null;
        $scope.preRecruitmentEmployee.NewPIN = null;
        $scope.preRecruitmentEmployee.ExpiredDays = null;
        $scope.preRecruitmentEmployee.Submitted = null;
        try {
            if (baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.PlantId)) {
                throw 'First select plant.';
            }
            baseService.setCurrentPage('employeeData');
            $scope.loadEmployeeData = function (pageno) {
                baseService.paginationBase('employees/candidateadministration/getlist?plantId=' + $scope.preRecruitmentEmployee.PlantId, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeData = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeemodal')).modal('show');
            $scope.loadEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.getuserAccess = function (ob) {
        $scope.preRecruitmentEmployee.Id = ob.Id;
        $scope.preRecruitmentEmployee.FullName = ob.FullName;
        $scope.preRecruitmentEmployee.NewPIN = ob.NewPIN;
        $scope.preRecruitmentEmployee.ExpiredDays = ob.ExpiredDays;
        $scope.preRecruitmentEmployee.Submitted = ob.Submitted;
        angular.element(document.querySelector('#employeemodal')).modal('hide');
    };

    $scope.clearuserAccess = function () {
        $scope.preRecruitmentEmployee.Id = null;
        $scope.preRecruitmentEmployee.FullName = null;
        $scope.preRecruitmentEmployee.NewPIN = null;
        $scope.preRecruitmentEmployee.ExpiredDays = null;
        $scope.preRecruitmentEmployee.Submitted = null;
    };

    $scope.Save = function () {
        try {
            $scope.savedisable = true;
            if ($scope.preRecruitmentEmployeeForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl = $scope.path + 'update',
                        data: $scope.preRecruitmentEmployee,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.savedisable = false;
                            ShowResult(response.data.Message, 'success');
                            $scope.Clear();
                        }
                    }, function errorCallback(response) {
                        $scope.savedisable = false;
                        //ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
            }
        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.preRecruitmentEmployee = {};
    }

}