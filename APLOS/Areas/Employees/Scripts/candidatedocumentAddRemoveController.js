'use strict';
candidatedocumentAddRemoveController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function candidatedocumentAddRemoveController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Candidate Document Add Remove';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.candidates = [];
    $scope.path = 'employees/candidateadministration/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, 10, null, 'FullName', 'FullName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.candidates = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.candidateInformation = {
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

    $rootScope.searchByList = [
        {
            'name': 'Full Name',
            'value': 'FullName'
        },
        {
            'name': 'Phone',
            'value': 'Phone'
        }
        ,
        {
            'name': 'Email',
            'value': 'EmailId'
        },
        {
            'name': 'Department',
            'value': 'Department'
        }
        ,
        {
            'name': 'Designation',
            'value': 'Designation'
        }
        ,
        {
            'name': 'Given Designation',
            'value': 'GivenDesignation'
        }
    ];

    function setUserImage(data) {
        if (!baseService.isUndefinedOrNull(data.SystemId)) {
            $scope.imageSrc = $rootScope.HRMSImage + data.EmpPicPath;
            $scope.imageBtnDisable = true;
            $scope.employee.EmpPicPath = data.EmpPicPath;
        }
        else {
            $scope.imageBtnDisable = false;
            $scope.employee.EmpPicPath = null;
        }
    }
    $scope.filedata = null;
    $scope.picData = null;
    $("#uploadImage").change(function () {
        $scope.picData = this.files[0];
    });
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.preRecruitmentEmployeeDocument = {
        Id: null,
        EmpSystemID: null,
        FileId: null,
        FileName: null,
        ComplianceDocumentId: null,
        ComplianceDocumentSetId: null
    };

    cboService.getCivilStatus(function (result) {
        $scope.civilStatusList = result;
    });

    $scope.VisibleDiv = function () {
        if ($scope.showdiv === true) {
            return true;
        }
        else {
            return false;
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.candidateInformation = $scope.candidates[$scope.index];
        $scope.user = $scope.candidateInformation.Id;
        $scope.PlantId = $scope.candidateInformation.PlantId;
        $scope.CompanyGroupID = $scope.candidateInformation.GroupID;
        $scope.EmpType = $scope.candidateInformation.EmpType;
        $scope.BudgetId = $scope.candidateInformation.BudgetId;
        $scope.GivenDesignationId = $scope.candidateInformation.GivenDesignationId;

        $scope.candidateInformation.DOB = $filter('dateFiltering')($scope.candidateInformation.DOB, 'dd-M-yyyy');
        $scope.candidateInformation.AgreedDOJ = $filter('dateFiltering')($scope.candidateInformation.AgreedDOJ, 'dd-M-yyyy');

        $scope.Tin = $scope.candidateInformation.TINCaption;
        if (baseService.isUndefinedOrNull($scope.Tin)) {
            $scope.Tin = "TIN";
        }
        $scope.Nid = $scope.candidateInformation.NIDCaption;
        if (baseService.isUndefinedOrNull($scope.Nid)) {
            $scope.Nid = "National ID";
        }
        $scope.NationalID = $scope.candidateInformation.NationalID;
        $scope.TIN = $scope.candidateInformation.TIN;

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Loaddocumentdatalist($scope.user);
    };

    $scope.picdata = null;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.fileId = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };

    $scope.getNum = function () {
        if ($scope.candidateInformation.IsKnownPerson)
            $scope.candidateInformation.NumberOfKnownPerson = 0;
        else
            $scope.candidateInformation.NumberOfKnownPerson = 1;
    };

    $scope.setHeight = function (id) {
        var element = angular.element(document.getElementById(id));
        $scope.height = element[0].scrollHeight;
    };
    $scope.Clear = function () {
        ClearFields();
        ClearFile();
        return true;
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
        $http.get('employees/candidateadministration/getempdocumentdatalist?companyGroupId=' + $scope.CompanyGroupID + '&pId=' + $scope.user + '&plantId=' + $scope.PlantId)
            .then(function (response) {
                $scope.documentdataList = response.data;
                for (var i = 0; i < $scope.documentdataList.length; i++) {
                    $scope.getColor($scope.documentdataList[i].FileName);
                }
            });
    };

    $scope.AddDocument = function () {
        $http.get('employees/candidateadministration/getdocumentlist?plantId=' + $scope.PlantId + '&empType=' + $scope.EmpType + '&budgetCode=' + $scope.BudgetId + '&givenDesignationId=' + $scope.GivenDesignationId)
            .then(function (response) {
                $scope.documentList = response.data;
                angular.element(document.querySelector('#DocumentPopUp')).modal('show');
            });
    };

    $scope.getInd = function (idx, dt) {
        $scope.indext = idx;
        $scope.documentData = dt;
    };

    $scope.docList = [];
    $scope.preRecruitmentDocumentList = [];
    $scope.fileNameChanged = function (d) {
        $scope.filedata = [];
        try {
            var tempInd = $scope.indext;
            var filename = d.value;
            var res = filename.replace(/C:\\fakepath\\/i, '');
            document.getElementById("" + tempInd + "").value = res;
            $scope.filedata = d.files[0];

            var fName = res;
            if (checkFileExist($scope.preRecruitmentDocumentList, fName)) {
                document.getElementById("" + tempInd + "").value = "";
                throw fName + ' This file already added, Please choose another one.';
            }

            if (checkSameFileExist($scope.documentdataList, fName)) {
                document.getElementById("" + tempInd + "").value = "";
                throw fName + ' This file already added, Please choose another one.';
            }

            if ($scope.filedata.size > 2000000) {
                document.getElementById("" + tempInd + "").value = "";
                throw fName + ' File size must be below 2 mb';
            }
            $scope.preRecruitmentDocumentList.push($scope.filedata);

            var nn = $scope.documentData;
            nn.FileName = fName;
            if (nn.FileName.length > 50) {
                throw "File Name must be less than 50 character.";
            }
            nn.PreRecruitmentEmployeeId = $scope.user;
            $scope.docList.push(nn);
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    function checkFileExist(list, name) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].name === name) {
                return true;
            }
        }
        return false;
    }
    function checkSameFileExist(list, name) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FileName === name) {
                return true;
            }
        }
        return false;
    }

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

    $scope.tempList = [];
    $scope.selectValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.DocumentName) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].DocumentName === data.DocumentName) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    function checkExistTempList(list, DocumentName) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DocumentName === DocumentName) {
                return true;
            }
        }
        return false;
    }

    function getActive(list, DocumentName) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DocumentName === DocumentName) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveNewDocument = function () {
        try {
            if ($scope.tempList.length === 0) {
                throw "Select Document Name.";
            }
            else {
                $http({
                    method: 'POST',
                    url: 'employees/candidateadministration/createnewdocument',
                    data: { 'candidateDocument': $scope.tempList, 'empId': $scope.user }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        ShowResult(response.data.Message, "failure", "DocumentPopUp");
                        $scope.savedisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success", "DocumentPopUp");
                        $scope.btnDisable = false;
                        $scope.Loaddocumentdatalist();
                        $scope.savedisable = false;
                        angular.element(document.querySelector('#DocumentPopUp')).modal('hide');
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "DocumentPopUp");
                    $scope.savedisable = false;
                });
                return true;
            }
        } catch (e) {
            $scope.btnDisable = false;
            $scope.savedisable = false;
            ShowResult(e, "failure", "DocumentPopUp");
        }
    };

    // #endregion

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    $scope.Delete = function (x, index) {
        $scope.delIndex = index;
        $scope.dId = x.Id;
        $scope.message = 'Are you sure to delete permanently?';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.removeDoc = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
        $http({
            method: 'POST',
            url: 'employees/candidateadministration/deletesingledocument?Id=' + $scope.dId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Loaddocumentdatalist();
                $scope.documentdata.FileName = "";
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };
}