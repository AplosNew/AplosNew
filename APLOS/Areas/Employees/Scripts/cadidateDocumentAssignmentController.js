'use strict';
candidateDocumentAssignmentController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function candidateDocumentAssignmentController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Candidate Document Assignment';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.recruitmentSelections = [];
    $scope.path = 'employees/candidateadministration/';
    $scope.saveUrl = $scope.path + 'createcandidatedocument';

    $scope.candidateInfo = {
        Id: null,
        GroupID: null,
        CompanyId: null,
        Image: null,
        InterviewRankingId: null,
        PlantId: null,
        PositionID: null,
        BudgetId: null,
        FullName: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        NickName: null,
        EmployeeName: null,
        FatherName: null,
        MotherName: null,
        Gender: null,
        NationalID: null,
        DOJ: null,
        DOB: null,
        Phone: null,
        Email: null,
        EmpType: null,
        TIN: null,
        CitizenID: null,
        ReligionID: null,
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
        PresAreaID: null,
        ParmAreaID: null,
        PresZipCode: null,
        ParmZipCode: null,
        PresCityID: null,
        ParmCityID: null,
        PresDistrictID: null,
        ParmDistrictID: null,
        PresCountryID: null,
        ParmCountryID: null,
        EmrCntPer1Name: null,
        EmrCntPer2Name: null,
        EmrCntPer1CellNo: null,
        EmrCntPer2CellNo: null,
        CivilStatusID: null,
        GivenDesignationId: null,
        LegalDesignationId: null,
        BirthdayCelebrationDate: null,
        MarriagedayCelebrationDate: null,
        AgreedDOJ: null,
        TotalSalary: null,
        SpecialReviewDuration: null,
        SpecialReviewAmount: null,
        IsFirstlogin: null,
        InitialPIN: null,
        NewPIN: null,
        LastLoginTime: null,
        Status: null,
        AppAddedDateTime: null,
        AppAddedBy: null,
        AppUpdatedBy: null,
        AppUpdatedDateTime: null,
        Submitted: null,
        SubmitDateTime: null,
        SelectionDateTime: null,
        SelectedBy: null,
        SelectionStatus: null,
        ReadyForCandidateAccess: null,
        ExpiredDays: null,
        ConfirmationStatus: null,
        ConfirmationDate: null,
        ConfirmationBy: null,
        Completed: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null,
        ParmanentArea: null,
        PresentArea: null,
        IsKnownPerson: null,
        NumberOfKnownPerson: null,
        IsExceptionalDesigApplicable: null,
        PresStateId: null,
        ParmStateId: null,
        IsImage: null,
        IsApproved: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        EmrCntPer1CellNo2: null,
        EmrCntPer1CellNo3: null,
        EmrCntPer2CellNo2: null,
        EmrCntPer2CellNo3: null,
        ConfirmAfterDays: null,
        IsDepartmentSubmit: null,
        DeptDocumentBy: null,
        DeptDocumentDateTime: null,
        ApplyingAsFresher: null,
        EmployeeCode: null,
        IsSalaryFixationAccepted: null,
        NonAssign: null,
        Assign: 'Assign'
    };

    cboService.getCboPlantByCompany(null, function (result) {
        $scope.plantList = result;
    });

    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.Id) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].Id === data.Id) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SelectionParameters = {
        limit: 500,
        offset: 0,
        order: 'DESC',
        sort: 'Id',
        searchBy: "FullName",
        pageSize: 500,
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
            'name': 'Candidate Name',
            'value': 'FullName'
        },
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'EmailId',
            'value': 'EmailId'
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

    $scope.LoadData = function () {
        try {
            $scope.GLUrl3 = 'employees/candidateadministration/getcandidatedatawithassignnonassigndoc?assign=' + $scope.candidateInfo.Assign + '&plantId=' + $scope.candidateInfo.PlantId,
                $scope.LoadDataList = function (pageno) {
                    baseService.paginationBase($scope.GLUrl3, pageno, $scope.SelectionParameters)
                        .then(function (data) {
                            $scope.recruitmentSelections = data.Rows;
                            $scope.SelectionParameters.total_count = data.Total;
                            $scope.SelectionParameters.search = null;
                            for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
                                $scope.recruitmentSelections[i].Active = getActive($scope.tempList, $scope.recruitmentSelections[i].Id);
                            }
                        }, function () {
                            ShowResult(commonMessage.NetworkError, 'failure');
                        }).finally(function () {
                        });
                };
            $scope.LoadDataList();
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.RefreshBody = function () {
        $scope.LoadData();
    };

    $scope.pushValue = function (data) {
        angular.forEach($scope.tempList, function (item, i) {
            if (item.Id === data.Id) {
                $scope.tempList[i].SelectionStatus = data.SelectionStatus;
            }
        });
    };
    $scope.tempInputValue = [];
    $scope.getInputValue = function () {
        for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
            $scope.tempInputValue.push($scope.recruitmentSelections[i]);
        }
    };

    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
            $scope.recruitmentSelections[i].Active = _isselected;
        }

        for (var i = 0; i < baseService.arrayLength($scope.recruitmentSelections); i++) {
            if (_isselected)
                $scope.tempList.push($scope.recruitmentSelections[i]);
            else
                for (var j = 0; j < $scope.tempList.length; j++) {
                    if ($scope.tempList[j].Id === $scope.recruitmentSelections[i].Id) {
                        $scope.tempList.splice(j, 1);
                        break;
                    }
                }
        }
    };

    $scope.showDocumentPopUp = function (obj, index) {
        $scope.index = index;
        $http.get('employees/candidateadministration/getdocumentdatalist?empId=' + obj.Id)
            .then(function (response) {
                $scope.documentdataList = response.data;
            });
        angular.element(document.querySelector('#DocumentPopUp')).modal('show');
    };
    $scope.closepopUp = function () {
        angular.element(document.querySelector('#DocumentPopUp')).modal('hide');
    };

    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.PreRecruitmentDocument + '/' + data.FileId + extention;
    };

    // #region Save
    $scope.Save = function () {
        try {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'candidateInfo': JSON.stringify($scope.tempList)
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.LoadData($scope.candidateInfo.PlantId);
                        $scope.tempList = [];
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    // #endregion
}