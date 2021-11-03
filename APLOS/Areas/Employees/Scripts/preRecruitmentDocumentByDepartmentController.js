'use strict';
preRecruitmentDocumentByDepartmentController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function preRecruitmentDocumentByDepartmentController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Document by Department';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'employees/prerecruitmentdocumentbydepartment/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.message = null;
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
        NumberOfKnownPerson: 1
    };
    $scope.preRecruitmentDocument = {
        Id: null,
        PreRecruitmentEmployeeId: null,
        FileId: null,
        FileName: null,
        ComplianceDocumentId: null
    };
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
            'name': 'Email',
            'value': 'Email'
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
    $scope.message = '';
    $scope.getListUrl = 'employees/prerecruitmentdocumentbydepartment/getlist',
        baseService.init($scope.getListUrl, null, null, null, 'FullName', 'FullName');
    $scope.LoadData = function (pageno) {
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
    $scope.LoadData();

    $scope.showDocumentPopUp = function (obj, index) {
        $scope.index = index;
        $http.get('employees/prerecruitmentdocumentbydepartment/getdocumentdatalist?companyGroupId=' + obj.GroupID + '&budgetId=' + obj.BudgetId + '&pId=' + obj.Id + '&plantId=' + obj.PlantId)
            .then(function (response) {
                $scope.documentdataList = response.data;
            });
        angular.element(document.querySelector('#DocumentPopUp')).modal('show');
    };
    $scope.confirmSubmit = function () {
        $scope.confirm = $scope.user;
        $scope.message_confirmation = 'Are you sure you want to submit? You won’t be able to modify your data after this.';
        angular.element(document.querySelector('#confirmSubmit')).modal('show');
    };

    $scope.showEntityPopUp = function () {
        $http.get('employees/prerecruitmentdocumentbydepartment/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    };

    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.PreRecruitmentDocument + '/' + data.FileId + extention;
    };

    // #region Document

    $scope.getInd = function (idx, dt) {
        $scope.indext = idx;
        $scope.documentData = dt;
    };

    //$scope.docList = [];
    //$scope.preRecruitmentDocumentList = [];
    //$scope.fileNameChanged = function (d) {
    //    $scope.filedata = [];
    //    try {
    //        var tempInd = $scope.indext;
    //        var filename = d.value;
    //        var res = filename.replace(/C:\\fakepath\\/i, '');
    //        document.getElementById('' + tempInd + '').value = res;
    //        $scope.filedata = d.files[0];

    //        var fName = res;
    //        if (checkFileExist($scope.preRecruitmentDocumentList, fName)) {
    //            document.getElementById('' + tempInd + '').value = '';
    //            throw fName + ' This file already added, Please choose another one.';
    //            $scope.filedata = [];
    //        }

    //        if (checkSameFileExist($scope.documentdataList, fName)) {
    //            document.getElementById('' + tempInd + '').value = '';
    //            $scope.filedata = [];
    //            throw fName + ' This file already added, Please choose another one.';
    //        }

    //        if ($scope.filedata.size > 2000000) {
    //            document.getElementById('' + tempInd + '').value = '';
    //            throw fName + ' File size must be below 2 mb';
    //            $scope.filedata = [];
    //        }
    //        $scope.preRecruitmentDocumentList.push($scope.filedata);

    //        var nn = $scope.documentData;
    //        nn.FileName = fName;
    //        //nn.PreRecruitmentEmployeeId = $scope.user;
    //        $scope.docList.push(nn);
    //    } catch (e) {
    //        ShowResult(e, 'failure', 'DocumentPopUp')
    //    }
    //}
    //function checkFileExist(list, name) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].name === name) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //function checkSameFileExist(list, name) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].FileName === name) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    $scope.fg = false;
    $scope.DocShow = function (data) {
        $scope.documentdata = data;
        $scope.filedata = {};
        if (!baseService.isUndefinedOrNull(data.FileName))
            $scope.filedata.name = data.FileName;
        else
            $scope.filedata = null;
        $scope.documentdata.FileName = data.FileName;
        //if (!baseService.isUndefinedOrNull(data.FileName)) {
        //    var filename = document.getElementById("uploadFile").value = data.FileName;
        //}

        if ($scope.documentdata.ProfileType === 'NID') {
            $scope.documentdata.Number = $scope.NationalID;
        }

        if ($scope.documentdata.ProfileType === 'TIN') {
            $scope.documentdata.Number = $scope.TIN;
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

    $("#uploadBtn4").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById("uploadBtn4").onchange = function () {
        var filename = document.getElementById("uploadFile4").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile4").value = res;
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

            if ($scope.documentdata.OptionalOrMandatory === 'Mandatory' && baseService.isUndefinedOrNull($scope.documentdata.FileName)) {
                throw 'File attachment is Mandatory';
            }

            $scope.savedisable = true;
            var formData = new FormData();

            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'employees/prerecruitmentdocumentbydepartment/createdepartmentdocument',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("preRecruitmentDocument", angular.toJson(data.preRecruitmentDocument));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'preRecruitmentDocument': $scope.documentdata, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        ShowResult(response.data.Message, "failure", "DocPopUp");
                        $scope.savedisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success", "DocPopUp");
                        //$scope.showDocumentPopUp();
                        $scope.filedata = {};
                        $scope.savedisable = false;
                        angular.element(document.querySelector('#DocPopUp')).modal('hide');
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "DocPopUp");
                    $scope.savedisable = false;
                });
                return true;
            }
        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure", "DocPopUp");
        }
    };


    //$scope.SaveDocument = function () {
    //    try {
    //        var formData = new FormData();
    //        for (var i = 0; i < $scope.documentdataList.length; i++) {
    //            if ($scope.documentdataList[i].OptionalOrMandatory === 'Mandatory'
    //                && baseService.isUndefinedOrNull($scope.documentdataList[i].FileName))
    //                throw 'File is 	Mandatory for ' + $scope.documentdataList[i].DocumentName + '.';
    //        }

    //        $http({
    //            method: 'POST',
    //            url: 'employees/prerecruitmentdocumentbydepartment/createdeptdocument',
    //            headers: { 'Content-Type': undefined },
    //            transformRequest: function (data) {
    //                formData.append('preRecruitmentDocument', angular.toJson(data.preRecruitmentDocument));
    //                formData.append('doc', angular.toJson(data.doc));
    //                formData.append('empId', $scope.documentdataList[0].PreRecruitmentEmployeeId);

    //                if (baseService.isUndefinedOrNull($scope.filedata) == false) {
    //                    for (var i = 0; i < data.file.length; i++) {
    //                        formData.append('file[' + i + ']', data.file[i]);
    //                    }
    //                }
    //                return formData;
    //            },
    //            data: { 'preRecruitmentDocument': $scope.docList, 'file': $scope.preRecruitmentDocumentList, 'empId': $scope.documentdataList[0].PreRecruitmentEmployeeId }
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == true) {
    //                ShowResult(response.data.Message, 'failure', 'DocumentPopUp');
    //                $scope.preRecruitmentDocumentList = [];
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success', 'DocumentPopUp');
    //                $scope.docList = [];
    //                $scope.preRecruitmentDocumentList = [];
    //                $scope.preRecruitmentEmployees.splice($scope.index, 1);
    //                $scope.index = -1;
    //                angular.element(document.querySelector('#DocumentPopUp')).modal('hide');
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.status.Message, 'failure', 'DocumentPopUp');
    //        });
    //    } catch (e) {
    //        ShowResult(e, '', 'DocumentPopUp');
    //    }
    //}

    $scope.closepopUp = function () {
        $scope.preRecruitmentDocumentList = [];
        angular.element(document.querySelector('#DocumentPopUp')).modal('hide');
    };
    // #endregion

    $scope.DocumentRemove = function (id) {
        $scope.idd = id;
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmDocDelete')).modal('show');
        $scope.filedata = {};
    };
    $scope.removeDoc = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
        $http({
            method: 'POST',
            url: 'employees/prerecruitmentdocumentbydepartment/deletedocument?Id=' + $scope.idd,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.documentdata.FileName = "";
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.confirmCloseDocDelete = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
    };

    function fileValidation() {
        for (var i = 0; i < $scope.documentdataList.length; i++) {
            if ($scope.documentdataList[i].OptionalOrMandatory === 'Mandatory'
                && baseService.isUndefinedOrNull($scope.documentdataList[i].FileName))
                throw 'File is 	Mandatory for ' + $scope.documentdataList[i].DocumentName + '.';
        }
    }
}