'use strict';
profileViewController.$inject = ['fileReader', 'cboService', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function profileViewController(fileReader, cboService, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = 'Profile';
    $scope.index = -1;
    $scope.employees = [];
    $scope.path = 'employees/employeeinformation/';
    $scope.getListUrl = $scope.path + 'employeebyemployeeid';

    $scope.GetEmployeeById = function () {
        $http.get('employees/employeeinformation/GetEmpProfileData')
            .then(function (response) {
                $scope.employeeInformation = response.data;
                $scope.imageSrc = virtualPath.EmployeePic + $scope.employeeInformation.EmpPicPath;
                $scope.Loaddocumentdatalist();
            });
    };
    $scope.GetEmployeeById();

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
        Image: null
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.clearImage = function () {
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    };

    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeDocument + '/' + data.FileId + extention;
    };

    // #region Document

    $scope.Loaddocumentdatalist = function () {
        $http.get('employees/employeeinformation/getempdocumentdatalist?companyGroupId=' + $scope.employeeInformation.CompanyGroupID + '&pId=' + $scope.employeeInformation.SystemId + '&plantId=' + $scope.employeeInformation.PlantId)
            .then(function (response) {
                $scope.documentdataList = response.data;
            });
    };

    $scope.getInd = function (idx, dt) {
        $scope.indext = idx;
        $scope.documentData = dt;
    };

    $scope.fg = false;
    $scope.DocShow = function (data) {
        $scope.documentdata = data;
        $scope.filedata = {};
        if (!baseService.isUndefinedOrNull(data.FileName))
            $scope.filedata.name = data.FileName;
        else
            $scope.filedata = null;
        $scope.documentdata.FileName = data.FileName;
        var filename = document.getElementById("uploadFile").value = data.FileName;

        if ($scope.documentdata.ProfileType === 'NID') {
            if (!baseService.isUndefinedOrNull($scope.NationalID)) {
                $scope.documentdata.DocNumber = $scope.NationalID;
            }
            else {
                $scope.documentdata.DocNumber = $scope.employeeInformation.NationalID;
            }
        }

        if ($scope.documentdata.ProfileType === 'NID') {
            if (!baseService.isUndefinedOrNull($scope.documentdata.DocNumber)) {
                $scope.fg = true;
            }
            else if (baseService.isUndefinedOrNull($scope.documentdata.DocNumber)) {
                $scope.fg = false;
            }
        }
        angular.element(document.querySelector('#DocPopUp')).modal('show');
    };

    $scope.getColor = function (item) {
        var remark = item.FileName;
        if (remark === null || remark === '') {
            return 'empty';
        } else {
            return 'filled';
        }
    };
    $scope.filedata = null;
    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
        $scope.documentdata.FileName = $scope.filedata.name;
    });

    document.getElementById("uploadFile").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };

    $scope.SaveDocument = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.documentdata.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.documentdata.FileName)) {
                if ($scope.documentdata.FileName.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }

            if ($scope.documentdata.DocNumberRequired === true) {
                if (baseService.isUndefinedOrNull($scope.documentdata.DocNumber)) {
                    throw "Document Number is required.";
                }
            }
            if ($scope.documentdata.DocDateRequired === true) {
                if (baseService.isUndefinedOrNull($scope.documentdata.DocDate)) {
                    throw "Document Date is required.";
                }
            }

            if ($scope.documentdata.OptionalOrMandatory === 'Mandatory' && baseService.isUndefinedOrNull($scope.documentdata.FileName)) {
                throw 'File attachment is Mandatory';
            }

            $scope.savedisable = true;
            //$scope.documentdata.PreRecruitmentEmployeeId = $scope.user;
            $scope.btnDisable = true;
            var formData = new FormData();

            //if ($scope.Action === "Save") {
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createdocument',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("employeeDocument", angular.toJson(data.employeeDocument));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'employeeDocument': $scope.documentdata, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    ShowResult(response.data.Message, "failure", "DocPopUp");
                    $scope.savedisable = false;
                }
                else {
                    ShowResult(response.data.Message, "success", "DocPopUp");
                    $scope.btnDisable = false;
                    $scope.Loaddocumentdatalist();
                    $scope.filedata = {};
                    $scope.savedisable = false;
                    angular.element(document.querySelector('#DocPopUp')).modal('hide');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", "DocPopUp");
                $scope.savedisable = false;
            });
            return true;
            //}
        } catch (e) {
            $scope.btnDisable = false;
            $scope.savedisable = false;
            ShowResult(e, "failure", "DocPopUp");
        }
    };

    $scope.DocumentRemove = function (id) {
        $scope.idd = id;
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmDocDelete')).modal('show');
        $scope.docList = [];
        $scope.preRecruitmentDocumentList = [];
        $scope.filedata = {};
    };

    $scope.removeDoc = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
        $http({
            method: 'POST',
            url: 'employees/employeeinformation/deletedocument?Id=' + $scope.idd,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', "DocPopUp");
            }
            else {
                ShowResult(response.data.Message, 'success', "DocPopUp");
                $scope.Loaddocumentdatalist();
                $scope.documentdata.FileName = "";
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure', "DocPopUp");
        });
        return true;
    };

    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.employee = {};
        return true;
    };
}