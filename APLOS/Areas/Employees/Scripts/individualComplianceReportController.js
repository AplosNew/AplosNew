'use strict';
individualComplianceReportController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function individualComplianceReportController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Individual Compliance';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.employees = [];
    $scope.path = 'employees/employeeinformation/';
    $scope.getListUrl = $scope.path + 'GetActiveAndInActiveEmployeeList';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, 10, null, 'EmployeeCode', 'EmployeeCode');

    $scope.EmployeeStatus = 'Active';


    $scope.getData = function (pageno) {
        $rootScope.parameters.EmployeeStatus = $scope.EmployeeStatus;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.employees = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });

    };
    $scope.getData();



    $scope.paraModel = {
        empId: null,
        reportType: null,
        tempId: null,
        FromDate: null,
        ToDate: null,
        empType: null
    };
    $scope.FromDate = null;
    $scope.ToDate = null;
    $scope.paraModel.empId = null;
    $scope.paraModel.reportType = null;
    $scope.paraModel.tempId = null;

    $scope.templateList = [];

    $scope.loadTemplateCbo = function (type, index) {
        $scope.Id = null;
        cboService.getTemplateCbo(type, function (result) {
            $scope.templateList = result;
        });
    };
    $scope.ReportTypeList = [];
    cboService.getEnumCbo('enum/GetLetterTypeCbo', function (result) {
        $scope.ReportTypeList = result;
    });

    $scope.workTypeList = [];
    cboService.getEmployeeWorkTypeCbo(function (result) {
        $scope.workTypeList = result;
    });

    $scope.PrintLocal = function (empId, id, selected) {
        try {
            if (baseService.isUndefinedOrNull(selected)) {
                ShowResult("Select Report Type.", 'failure');
                return false;
            }
            if (baseService.isUndefinedOrNull(id)) {
                ShowResult("Select Language.", 'failure');
                return false;
            }
            if (selected === 'AppointmentLetter') {

                try {
                    location.href = 'employees/EmployeeInformation/EmployeeAppointmentLetterLocal?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
                } catch (e) {
                    ShowResult(e, "failure");
                }
            }
            if (selected === 'Fixation') {

                try {
                    location.href = 'employees/EmployeeInformation/EmployeeFixationForm?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
                } catch (e) {
                    ShowResult(e, "failure");
                }
            }
            if (selected === 'ServiceBook') {
                location.href = 'employees/EmployeeInformation/EmployeeServiceBookInWord?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
            }
            if (selected === 'NomineeInfo') {
                location.href = 'employees/EmployeeInformation/EmployeeNomineeInMSWord?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
            }
            if (selected === 'JoiningLetter') {
                location.href = 'employees/EmployeeInformation/EmployeeJoiningLetterInMSWord?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
            }
            if (selected === 'Acknowledgement') {

                location.href = 'employees/EmployeeInformation/EmployeeAcknowledgementInMSWord?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
            }

            if (selected === 'IncrementHistory') {

                location.href = 'employees/EmployeeInformation/IncrementHistory?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
            }
            if (selected === 'ExitInterview') {

                location.href = 'employees/EmployeeInformation/ExitInterview?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
            }

            if (selected === 'ConfirmationLetter') {

                var href = 'employees/EmployeeInformation/ConfirmationletterInMSWord?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
                $rootScope.report(href);
            }
            if (selected === 'EmployeePersonalFile') {

                var href = 'employees/EmployeeInformation/GetEmployeePersonalFileInMSWord?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
                $rootScope.report(href);
            }

            if (selected === 'LeaveRegister') {
                $scope.paraModel.empId = empId;
                $scope.paraModel.reportType = selected;
                $scope.paraModel.tempId = id;
                $scope.parametersPopUp();

            }

            if (selected === 'IdCard') {
                $scope.model = {};
                $scope.obj = $filter("filter")($scope.employees, { SystemId: empId });
                $scope.paraModel.reportType = selected;
                $scope.paraModel.tempId = id;
                $scope.paraModel.empType = $scope.obj[0].EmploymentType;
                $scope.GetIssueIdCardByEmployee(empId);
                $scope.GetSequence(empId);
                //$scope.model.IssueDate = $filter('dateFiltering')($scope.obj[0].DOJ, 'dd-M-yyyy');
                $scope.model.IssueDate = $filter('date')(new Date(), 'dd-MM-yyyy');;
                $scope.model.DOJ = $filter('dateFiltering')($scope.obj[0].DOJ, 'dd-M-yyyy');
                $scope.model.EmpSystemId = $scope.obj[0].SystemId;

                angular.element(document.querySelector('#IdCardPopUpModel')).modal('show');
            }

            if (selected === 'WarningLetter') {
                $scope.obj = $filter("filter")($scope.employees, { SystemId: empId });
                $scope.paraModel.reportType = selected;
                $scope.paraModel.tempId = id;
                $scope.paraModel.empType = $scope.obj[0].EmploymentType;
                $scope.GetSequence(empId);
                $scope.WarningLettermodel.IssueDate = $filter('dateFiltering')($scope.obj[0].DOJ, 'dd-M-yyyy');
                $scope.WarningLettermodel.DOJ = $filter('dateFiltering')($scope.obj[0].DOJ, 'dd-M-yyyy');
                $scope.WarningLettermodel.EmpSystemId = $scope.obj[0].SystemId;
                var eDialog = $("#WarningLetterPopUpModel").data("ejDialog");
                eDialog.open();
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.EmployeeServiceBookInWord = function (empId, id) {
        if (!baseService.isUndefinedOrNull(id)) {
            location.href = 'employees/EmployeeInformation/EmployeeServiceBookInWord?empId=' + empId + '&reportType=' + selected + '&tempId=' + id;
        }
    };

    $scope.parametersPopUp = function () {
        angular.element(document.querySelector('#parametersPopUpModel')).modal('show');
    };

    $scope.idIndex = -1;
    $scope.IdCardPrint = function (index, data) {
        $scope.idIndex = index;
        location.href = 'Employees/EmployeeIdCard/PrintEmployeeIDCard?empId=' + $scope.model.EmpSystemId + '&tempId=' + $scope.paraModel.tempId + '&empType=' + $scope.paraModel.empType + '&reportType=' + $scope.paraModel.reportType + '&issuDate=' + data.IssueDate + '&workTypeId=' + data.EmployeeWorkTypeId;
        $scope.idIndex = -1;
    };

    $scope.model = {
        Id: null,
        Sequence: null,
        EmpSystemId: null,
        IssueDate: null,
        ExpiryDate: null,
        EmployeeWorkTypeId: null
    }
    $scope.WarningLettermodel = {
        Id: null,
        EmpSystemId: null,
        WarningColumn: 'firstwarning'
    }
    $scope.issueIdCardList = [];
    $scope.GetIssueIdCardByEmployee = function (employeeId) {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Employees/EmployeeInformation/GetIssueIdCardByEmployee?employeeId=' + employeeId
        }).then(function successCallback(response) {
            $scope.issueIdCardList = response.data;

        });
    }

    $scope.issueIdCardList = [];
    $scope.GetWarningLetterByEmployee = function (employeeId) {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Employees/EmployeeInformation/GetWarningLetterByEmployee?employeeId=' + employeeId
        }).then(function successCallback(response) {
            $scope.issueIdCardList = response.data;

        });
    }

    $scope.GetSequence = function (empSystemId) {
        $http.get("Employees/EmployeeInformation/getautosequence?empSystemId=" + empSystemId)
            .then(function (response) {
                $scope.model.Sequence = response.data;
            });
    };

    $scope.SaveIdIssue = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.model.IssueDate)) {
                throw "Issue Date is required.";
            }

            //if (baseService.isUndefinedOrNull($scope.model.EmployeeWorkTypeId)) {
            //    throw "Work Type is required.";
            //}

            if (new Date($scope.model.IssueDate) < new Date($scope.model.DOJ)) {
                throw "IssueDate " + $scope.model.IssueDate + " can not less than DOJ " + $scope.model.DOJ + "";
            }

            if (baseService.arrayLength($scope.issueIdCardList) === 0) {
                if ((new Date($scope.model.IssueDate) < new Date($scope.model.DOJ)) || (new Date($scope.model.IssueDate) > new Date($scope.model.DOJ))) {
                    throw "IssueDate " + $scope.model.IssueDate + " must be DOJ " + $scope.model.DOJ + "";
                }
            }

            $http({
                method: "POST",
                url: 'employees/employeeinformation/createemployeeidcardissue',
                data: { "employeeIdCardIssue": $scope.model },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure", 'IdCardPopUpModel');
                }
                else {
                    ShowResult(response.data.Message, "success", 'IdCardPopUpModel');
                    $scope.GetIssueIdCardByEmployee($scope.model.EmpSystemId);
                    $scope.GetSequence($scope.model.EmpSystemId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, "failure", 'IdCardPopUpModel');
            };
        } catch (e) {
            ShowResult(e, "failure", 'IdCardPopUpModel');
        }
    };

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
        employeeID: null,
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
        Image: null,
        PaymentMode: null,
        PaymentModeEffectiveDate: null,
        SelectedItem: null
    };

    $rootScope.searchByList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Section',
            'value': 'Section'
        },
        {
            'name': 'Designation',
            'value': 'Designation'
        }
    ];

    $scope.selectIdArray = new Array($scope.templateList.length);


    $scope.ClanderYearModel = {
        Id: null,
        YearNo: null,
        FromDate: null,
        ToDate: null
    }
    $scope.Id = null;
    $scope.ClanderYear = [];
    $scope.GetClanderYear = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Employees/EmployeeInformation/GetClanderYear'

        }).then(function successCallback(response) {
            $scope.ClanderYear = response.data.data;

        });
    }
    $scope.GetClanderYear();

    $scope.getFromAndToDate = function () {
        $scope.OBJ = $filter("filter")($scope.ClanderYear, { Id: $scope.Id });
        $scope.ClanderYearModel = $scope.OBJ[0];
        $scope.FromDate = $scope.ClanderYearModel.FromDate;
        $scope.ToDate = $scope.ClanderYearModel.ToDate;
    };

    $scope.LeaveRegisterReportPrint = function () {
        if (baseService.isUndefinedOrNull($scope.Id)) {
            ShowResult("Select Year.", 'failure');
            return false;
        }
        location.href = "Employees/EmployeeInformation/LeaveRegister?empId=" + $scope.paraModel.empId + "&CalanderYearId=" + $scope.Id + "&reportType=" + $scope.paraModel.reportType + "&ToDate=" + $scope.ToDate + "&FromDate=" + $scope.FromDate + "&tempId=" + $scope.paraModel.tempId;
    };


    $scope.MediasoftFairShopDataExport = function () {
        location.href = "Employees/EmployeeInformation/MediasoftFairShopEmpDataExport";
    };
}
