'use strict';
employeeDocumentAssignmentController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function employeeDocumentAssignmentController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Document Assignment';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'employees/employeedocumentassignment/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

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
        ApprovedDateTime: null,
        ApprovedFromIP: null,
        ParmStateId: null,
        PresStateId: null,
        IsImage: null,
        IsFirstlogin: null,
        InitialPIN: null,
        NewPIN: null,
        LastLoginTime: null,
        ApplyingAsFresher: null,
        DOCBy: null,
        ProbationConfirmEntryDate: null,
        DOSBy: null,
        DOSDate: null,
        ProximityAddedBy: null,
        ProximityAddedDate: null,
        ProximityUpdatedBy: null,
        ProximityUpdatedDate: null,
        BirthdayCelebrationDate: null,
        MarriagedayCelebrationDate: null,
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
                if (checkExistTempList($scope.tempList, data.SystemId) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].SystemId === data.SystemId) {
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

    function checkExistTempList(list, SystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemId === SystemId) {
                return true;
            }
        }
        return false;
    }

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SelectionParameters = {
        limit: 200,
        offset: 0,
        order: 'DESC',
        sort: 'SystemId',
        searchBy: "EmployeeName",
        pageSize: 200,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $rootScope.searchDataByList = [
        {
            'name': 'Employee Id',
            'value': 'SystemId'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
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
            'name': 'Budget Code',
            'value': 'BudgetCode'
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
            $scope.GLUrl3 = 'employees/employeedocumentassignment/getlist?assign=' + $scope.employeeInformation.Assign + '&plantId=' + $scope.employeeInformation.PlantId,
                $scope.LoadDataList = function (pageno) {
                    baseService.paginationBase($scope.GLUrl3, pageno, $scope.SelectionParameters)
                        .then(function (data) {
                            $scope.recruitmentSelections = data.Rows;
                            $scope.SelectionParameters.total_count = data.Total;
                            $scope.SelectionParameters.search = null;
                            for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
                                $scope.recruitmentSelections[i].Active = getActive($scope.tempList, $scope.recruitmentSelections[i].SystemId);
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
            if (item.SystemId === data.SystemId) {
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
                    if ($scope.tempList[j].SystemId === $scope.recruitmentSelections[i].SystemId) {
                        $scope.tempList.splice(j, 1);
                        break;
                    }
                }
        }
    };

    $scope.showDocumentPopUp = function (obj, index) {
        $scope.index = index;
        $http.get('employees/employeedocumentassignment/getdocumentdatalist?empId=' + obj.SystemId)
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
        $scope.dwonloadUrl = virtualPath.EmployeeDocument + '/' + data.FileId + extention;
    };

    // #region Save
    $scope.Save = function () {
        try {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'employeeInformation': JSON.stringify($scope.tempList)
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.tempList = [];
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.LoadData($scope.employeeInformation.PlantId);
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