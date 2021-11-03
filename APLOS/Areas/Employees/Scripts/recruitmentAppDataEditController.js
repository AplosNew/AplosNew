'use strict';
recruitmentAppDataEditController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function recruitmentAppDataEditController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Pre-Recruitment Data';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'employees/recruitmentappdataedit/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.updateUrl = $scope.path + 'update';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.preRecruitmentEmployee = {
        Id: null,
        Image: null,
        InterviewRankingId: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        PositionID: null,
        BudgetId: null,
        IsDirect: true,
        FullName: null,
        Gender: null,
        NationalID: null,
        DOB: null,
        Phone: null,
        Email: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        NickName: null,
        EmployeeName: null,
        CivilStatus: null,
        EmpType: null,
        SelectionStatus: null,
        ConfirmationStatus: null,
        NatureOfEmployement: null,
        TIN: null,
        FatherName: null,
        MotherName: null,
        CitizenID: null,
        ReligionID: null,
        CivilStatusID: null,
        ResignationID: null,
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
        Completed: false,
        ReadyForCandidateAccess: false,
        BirthdayCelebrationDate: null,
        MarriagedayCelebrationDate: null,
        AgreedDOJ: null,
        TotalSalary: null,
        SpecialReviewDuration: null,
        SpecialReviewAmount: null,
        GivenDesignationId: null,
        Code: null
    };
    $scope.preRecruitmentEmployeeNew = Object.assign({}, $scope.preRecruitmentEmployee);

    cboService.getCboPlantByCompany(null, function (result) {
        $scope.plantList = result;
    });

    cboService.getCboRank(function (result) {
        $scope.rankList = result;
    });

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required.';
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.SetToDate = function () {
        try {
            if ($scope.FromDate === null || $scope.FromDate === '') {
                //
            }
            else {
                var _fromdate = new Date();
                var todate = $filter('dateFiltering')(_fromdate, 'dd-MMM-yyyy');
                $scope.ToDate = todate;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.SetFromDate = function () {
        try {
            if ($scope.ToDate === null || $scope.ToDate === '') {
                //
            }
            else {
                var _todate = new Date($scope.ToDate);
                var _fromdate = new Date();
                if ($scope.FromDate === null || $scope.FromDate === '') {
                    //
                }
                else {
                    _fromdate = new Date();
                }

                if (_fromdate > _todate) {
                    var todate = $filter('dateFiltering')(_todate, 'dd-MMM-yyyy');
                    $scope.FromDate = todate;
                }
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.SelectionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'Id',
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $rootScope.searchDataByList = [
        {
            'name': 'Employee Id',
            'value': 'Id'
        },
        {
            'name': 'Full Name',
            'value': 'FullName'
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
            //CheckField($scope.preRecruitmentEmployeeNew.PlantId, "Plant");
            CheckField($scope.FromDate, "From Date");
            CheckField($scope.ToDate, "To Date");

            var _fromdate = new Date($scope.FromDate);
            var _todate = new Date($scope.ToDate);

            var fromdate = $filter('dateFiltering')(_fromdate, 'dd-MMM-yyyy');
            var todate = $filter('dateFiltering')(_todate, 'dd-MMM-yyyy');

            if (_fromdate > _todate) {
                throw "From Date [" + fromdate + "] can not be greater than To Date [" + todate + "]";
            }

            $scope.GLUrl3 = 'employees/recruitmentappdataedit/getappdata?plantId=' + $scope.preRecruitmentEmployeeNew.PlantId + "&fd=" + fromdate + "&td=" + todate + "";
            $scope.LoadDataList = function (pageno) {
                baseService.paginationBase($scope.GLUrl3, pageno, $scope.SelectionParameters)
                    .then(function (data) {
                        $scope.recruitmentSelections = data.Rows;
                        $scope.SelectionParameters.total_count = data.Total;

                        if ($scope.recruitmentSelections.length === 0) {
                            ShowResult('No data found.', 'Error');
                            $scope.SelectionParameters.search = null;
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

    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpUrl = 'employees/recruitmentappdataedit/getbudgetcodelist?plantId=' + $scope.preRecruitmentEmployeeNew.PlantId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.selectDoubleClick = function (data) {
        $scope.preRecruitmentEmployeeNew.BudgetId = data.Id;
        $scope.preRecruitmentEmployeeNew.Code = data.Code;
        $scope.preRecruitmentEmployeeNew.EntityName = data.EntityName;
        $scope.preRecruitmentEmployeeNew.Designation = data.Designation;
        $scope.preRecruitmentEmployeeNew.PositionName = data.PositionName;
        $scope.preRecruitmentEmployeeNew.DesignationId = data.DesignationId;
        $scope.preRecruitmentEmployeeNew.GivenDesignationId = null;

        givenDesignationCbo($scope.preRecruitmentEmployeeNew.DesignationId);
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.clearCode = function () {
        $scope.preRecruitmentEmployeeNew.BudgetId = null;
        $scope.preRecruitmentEmployeeNew.EntityName = null;
        $scope.preRecruitmentEmployeeNew.Designation = null;
        $scope.preRecruitmentEmployeeNew.PositionName = null;
        $scope.preRecruitmentEmployeeNew.GivenDesignationId = null;
    };

    $scope.lowerGivenDesignationCbo = function (id, gid) {
        $scope.givenDesignationList = [];
        cboService.getCboLowerGivenDesignation(id, function (result) {
            $scope.givenDesignationList = result;
            $scope.preRecruitmentEmployeeNew.GivenDesignationId = gid;
        });
    };

    $scope.uppderGivenDesignationCbo = function (id, gid) {
        $scope.givenDesignationList = [];
        cboService.getCboUpperGivenDesignation(id, function (result) {
            $scope.givenDesignationList = result;
            $scope.preRecruitmentEmployeeNew.GivenDesignationId = gid;
        });
    };
    $scope.getDes = function () {
        if ($scope.preRecruitmentEmployeeNew.IsExceptionalDesigApplicable === false) {
            $scope.lowerGivenDesignationCbo($scope.preRecruitmentEmployeeNew.DesignationId);
        }
        else {
            $scope.uppderGivenDesignationCbo($scope.preRecruitmentEmployeeNew.DesignationId);
        }
    };

    $scope.Get = function (data, index) {
        $scope.index = index;
        $scope.preRecruitmentEmployee = $scope.recruitmentSelections[$scope.index];
        if (data.IsExceptionalDesigApplicable === 1)
            $scope.preRecruitmentEmployee.IsExceptionalDesigApplicable = true;
        else if (data.IsExceptionalDesigApplicable === 0)
            $scope.preRecruitmentEmployee.IsExceptionalDesigApplicable = false;

        if ($scope.preRecruitmentEmployee.IsExceptionalDesigApplicable)
            $scope.uppderGivenDesignationCbo(data.DesignationId, data.GivenDesignationId);
        else
            $scope.lowerGivenDesignationCbo(data.DesignationId, data.GivenDesignationId);
        angular.copy($scope.preRecruitmentEmployee, $scope.preRecruitmentEmployeeNew);
        $scope.PhoneLength = $scope.preRecruitmentEmployeeNew.PhoneLength;
        $scope.NIDLength = $scope.preRecruitmentEmployeeNew.NIDLength;
        $scope.NIDCaption = $scope.preRecruitmentEmployeeNew.NIDCaption;
        if (baseService.isUndefinedOrNull($scope.NIDCaption)) {
            $scope.NIDCaption = "National ID";
        }
        $scope.Action = 'Update';
        $scope.appAddedEntryShow();
    };

    $scope.appAddedEntryShow = function () {
        angular.element(document.querySelector('#appAddedEntryPopUp')).modal('show');
    };
    function checkValidation() {
        CheckField($scope.preRecruitmentEmployeeNew.BudgetId, "Budget Code");
        CheckField($scope.preRecruitmentEmployeeNew.GivenDesignationId, "Given Designation");
        CheckField($scope.preRecruitmentEmployeeNew.Gender, "Gender");
        CheckField($scope.preRecruitmentEmployeeNew.FullName, "Full Name");
        CheckField($scope.preRecruitmentEmployeeNew.EmpType, "Emp Type");
        CheckField($scope.preRecruitmentEmployeeNew.Email, "Email");
        CheckField($scope.preRecruitmentEmployeeNew.Phone, "Phone");
        CheckField($scope.preRecruitmentEmployeeNew.AgreedDOJ, "Agreed DOJ");
        CheckField($scope.preRecruitmentEmployeeNew.InterviewRankingId, "Rank");
        CheckField($scope.preRecruitmentEmployeeNew.Status, "Status");

        if (isNaN($scope.preRecruitmentEmployeeNew.Phone)) {
            throw "Enter valid phone number";
        }
        if (isNaN($scope.preRecruitmentEmployeeNew.NationalID)) {
            throw "Enter valid national id";
        }
        if (isNaN($scope.preRecruitmentEmployeeNew.TotalSalary)) {
            throw "Enter valid number";
        }
        if (isNaN($scope.preRecruitmentEmployeeNew.SpecialReviewDuration)) {
            throw "Enter valid number";
        }
        if (isNaN($scope.preRecruitmentEmployeeNew.SpecialReviewAmount)) {
            throw "Enter valid number";
        }
        if ($scope.preRecruitmentEmployeeNew.TotalSalary < 1) {
            throw "Total salary can not less than 1.";
        }
        if ($scope.preRecruitmentEmployeeNew.SpecialReviewAmount < 0) {
            throw "Special review amount can not less than 0.";
        }
        if ($scope.preRecruitmentEmployeeNew.SpecialReviewDuration < 0) {
            throw "Special review duration can not less than 0.";
        }

        var _ad = new Date($scope.preRecruitmentEmployeeNew.AgreedDOJ);
        var _db = new Date($scope.preRecruitmentEmployeeNew.DOB);

        var ad = $filter('dateFiltering')(_ad, 'dd-MMM-yyyy');
        var db = $filter('dateFiltering')(_db, 'dd-MMM-yyyy');

        if (_ad < _db) {
            throw "Date of birth [" + db + "] can not be greater than Agreed Date of join [" + ad + "]";
        }
    }

    // #region Save
    $scope.Update = function () {
        try {
            checkValidation();
            //if ($scope.preRecruitmentEmployeeNew.Phone.length !== $scope.PhoneLength) {
            //    throw "Phone Number must be " + $scope.PhoneLength + " character.";
            //}
            if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.preRecruitmentEmployeeNew.Email)) {
            } else {
                throw "You have entered an invalid email address.";
            }
            if ($scope.preRecruitmentEmployeeNew.IsExceptionalDesigApplicable === true)
                $scope.preRecruitmentEmployeeNew.IsExceptionalDesigApplicable = 1;
            if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'preRecruitmentEmployee': $scope.preRecruitmentEmployeeNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure", 'appAddedEntryPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, "success", 'appAddedEntryPopUp');
                        $scope.Clear();
                        angular.element(document.querySelector('#appAddedEntryPopUp')).modal('hide');
                        $scope.LoadData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", 'appAddedEntryPopUp');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure", 'appAddedEntryPopUp');
        }
    };
    // #endregion

    // #region Clear

    $scope.ClearBody = function () {
        $scope.preRecruitmentEmployeeNew.PlantId = null;
        $scope.FromDate = null;
        $scope.ToDate = null;
        $scope.recruitmentSelections = [];
        $scope.SetFromDate($scope.ToDate = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy'));
        $scope.SetToDate($scope.FromDate = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy'));
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.preRecruitmentEmployee = { PlantId: $scope.preRecruitmentEmployeeNew.PlantId };
        $scope.preRecruitmentEmployeeNew = { PlantId: $scope.preRecruitmentEmployeeNew.PlantId };
    }

    // #endregion

    // #region Delete

    $scope.Delete = function () {
        try {
            if ($scope.preRecruitmentEmployeeNew.Id === null || $scope.preRecruitmentEmployeeNew.Id === '') {
                $scope.recruitmentSelections.splice($scope._Index, 1);
                $scope._Index = -1;
            }
            else {
                $http({
                    method: 'POST',
                    url: 'employees/recruitmentappdataedit/delete?id=' + $scope.preRecruitmentEmployeeNew.Id,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector('#confirmDeletePopUp')).modal('hide');
                        deleteDeleted($scope.preRecruitmentEmployeeNew.Id, $scope.recruitmentSelections);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.confirmDelete = function (data, index) {
        $scope.preRecruitmentEmployeeNew.Id = data.Id;
        $scope.preRecruitmentEmployeeNew.FullName = data.FullName;
        $scope._Index = index;
        $scope.message_confirmation = 'Are you sure want to delete permanently ' + data.FullName + '?';
    };

    function deleteDeleted(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                list.splice(i, 1);
            }
        }
    }

    $scope.removeRow = function () {
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.Id)) {
            deleteDeleted($scope.preRecruitmentEmployeeNew.Id, $scope.recruitmentSelections);
        }
        else {
            $scope.Delete();
        }
    };

    $scope.confirmCloseDelete = function () {
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('hide');
    };
    // #endregion

    $scope.SetFromDate($scope.ToDate = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy'));
    $scope.SetToDate($scope.FromDate = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy'));
}