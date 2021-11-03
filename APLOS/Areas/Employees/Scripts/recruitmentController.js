'use strict';
recruitmentController.$inject = ['$window','fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function recruitmentController($window,fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Recruitment';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.recruitments = [];
    $scope.path = 'employees/recruitment/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
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
        Code: null,
        Status: null,
        OperationMasterID:null
    };
    $scope.preRecruitmentEmployeeNew = Object.assign({}, $scope.preRecruitmentEmployee);

    //cboService.getCboPlantByCompany(null, function (result) {
    //    $scope.plantList = result;
    //});
    $scope.operationMasterList = [];
    cboService.getCboOperationMasterByCompanyGroup($window.companyGroupId, function (result) {
        $scope.operationMasterList = result;
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
            $scope.GLUrl = 'employees/recruitment/getcandidatedata';
            $scope.LoadDataList = function (pageno) {
                baseService.paginationBase($scope.GLUrl, pageno, $scope.SelectionParameters)
                    .then(function (data) {
                        $scope.recruitments = data.Rows;
                        $scope.SelectionParameters.total_count = data.Total;

                        if ($scope.recruitments.length === 0) {
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
    $scope.LoadData();

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
        $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
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

        cboService.getCboLowerGivenDesignation($scope.preRecruitmentEmployeeNew.DesignationId, function (result) {
            $scope.givenDesignationList = result;
            $scope.preRecruitmentEmployeeNew.GivenDesignationId = $scope.preRecruitmentEmployeeNew.DesignationId;
            //preRecruitmentEmployeeNew.GivenDesignationId
        });

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
        $scope.preRecruitmentEmployee = $scope.recruitments[$scope.index];
        if (data.IsExceptionalDesigApplicable === 1)
            $scope.preRecruitmentEmployee.IsExceptionalDesigApplicable = true;
        else if (data.IsExceptionalDesigApplicable === 0)
            $scope.preRecruitmentEmployee.IsExceptionalDesigApplicable = false;

        if ($scope.preRecruitmentEmployee.IsExceptionalDesigApplicable)
            $scope.uppderGivenDesignationCbo(data.DesignationId, data.GivenDesignationId);
        else
            $scope.lowerGivenDesignationCbo(data.DesignationId, data.GivenDesignationId);
        angular.copy($scope.preRecruitmentEmployee, $scope.preRecruitmentEmployeeNew);
        $scope.preRecruitmentEmployeeNew.BudgetId = $scope.preRecruitmentEmployee.BudgetId;
        $scope.preRecruitmentEmployeeNew.Code = $scope.preRecruitmentEmployee.Code;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    function checkValidation() {
        CheckField($scope.preRecruitmentEmployeeNew.BudgetId, "Budget Code");
        CheckField($scope.preRecruitmentEmployeeNew.GivenDesignationId, "Given Designation");
        //CheckField($scope.preRecruitmentEmployeeNew.OperationMasterID, "Operation Master");
        CheckField($scope.preRecruitmentEmployeeNew.FullName, "Full Name");
        CheckField($scope.preRecruitmentEmployeeNew.Gender, "Gender");
        CheckField($scope.preRecruitmentEmployeeNew.Phone, "Phone");
        CheckField($scope.preRecruitmentEmployeeNew.Email, "Email");
        CheckField($scope.preRecruitmentEmployeeNew.AgreedDOJ, "Agreed DOJ");
        CheckField($scope.preRecruitmentEmployeeNew.InterviewRankingId, "Rank");
        CheckField($scope.preRecruitmentEmployeeNew.Status, "Status");
        CheckField($scope.preRecruitmentEmployeeNew.EmpType, "Emp Type");

        if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.Phone)) {
            if (isNaN($scope.preRecruitmentEmployeeNew.Phone)) {
                throw "Enter valid phone number";
            }
        }
        //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.NationalID)) {
        //    if (isNaN($scope.preRecruitmentEmployeeNew.NationalID)) {
        //        throw "Enter valid national id";
        //    }
        //}
        if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.TotalSalary)) {
            if (isNaN($scope.preRecruitmentEmployeeNew.TotalSalary)) {
                throw "Enter valid number";
            }
        }
        if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.SpecialReviewDuration)) {
            if (isNaN($scope.preRecruitmentEmployeeNew.SpecialReviewDuration)) {
                throw "Enter valid number";
            }
        }
        if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.SpecialReviewAmount)) {
            if (isNaN($scope.preRecruitmentEmployeeNew.SpecialReviewAmount)) {
                throw "Enter valid number";
            }
        }
        if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.TotalSalary)) {
            if ($scope.preRecruitmentEmployeeNew.TotalSalary < 1) {
                throw "Total salary can not less than 1.";
            }
        }
        if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.SpecialReviewAmount)) {
            if ($scope.preRecruitmentEmployeeNew.SpecialReviewAmount < 0) {
                throw "Special review amount can not less than 0.";
            }
        }
        if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.SpecialReviewDuration)) {
            if ($scope.preRecruitmentEmployeeNew.SpecialReviewDuration < 0) {
                throw "Special review duration can not less than 0.";
            }
        }

        if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.AgreedDOJ) && !baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.DOB)) {
            var _ad = new Date($scope.preRecruitmentEmployeeNew.AgreedDOJ);
            var _db = new Date($scope.preRecruitmentEmployeeNew.DOB);

            var ad = $filter('dateFiltering')(_ad, 'dd-MMM-yyyy');
            var db = $filter('dateFiltering')(_db, 'dd-MMM-yyyy');

            if (_ad < _db) {
                throw "Date of birth [" + db + "] can not be greater than Agreed Date of join [" + ad + "]";
            }
        }

        //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployeeNew.DOB)) {
        //    var _ad = new Date();
        //    var _db = new Date($scope.preRecruitmentEmployeeNew.DOB);

        //    var ad = $filter('dateFiltering')(_ad, 'dd-MMM-yyyy');
        //    var db = $filter('dateFiltering')(_db, 'dd-MMM-yyyy');

        //    if (_ad > _db) {
        //        throw "Date of birth [" + db + "] can not be smaller than [" + ad + "]";
        //    }
        //}

    }

    $scope.Save = function () {
        try {
            checkValidation();
            if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.preRecruitmentEmployeeNew.Email)) {
                //allow
            } else {
                throw "Invalid email address.";
            }
            if ($scope.preRecruitmentEmployeeNew.IsExceptionalDesigApplicable === true)
                $scope.preRecruitmentEmployeeNew.IsExceptionalDesigApplicable = 1;

            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.preRecruitmentEmployeeNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.recruitments.push(response.data.PreRecruitmentEmployee);
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.LoadData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.preRecruitmentEmployeeNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.recruitments[$scope.index] = $scope.PreRecruitmentEmployee;
                        }
                        ClearFields();
                        $scope.LoadData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

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
        $scope.Action = 'Save';
        $scope.preRecruitmentEmployee = {};
        $scope.preRecruitmentEmployeeNew = {};
    }

    // #endregion

    // #region Delete

    $scope.Delete = function () {
        try {
            if ($scope.preRecruitmentEmployeeNew.Id === null || $scope.preRecruitmentEmployeeNew.Id === '') {
                $scope.recruitments.splice($scope._Index, 1);
                $scope._Index = -1;
            }
            else {
                $http({
                    method: 'POST',
                    url: 'employees/recruitment/delete?id=' + $scope.preRecruitmentEmployeeNew.Id,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector('#confirmDeletePopUp')).modal('hide');
                        deleteDeleted($scope.preRecruitmentEmployeeNew.Id, $scope.recruitments);
                        ClearFields();
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
            deleteDeleted($scope.preRecruitmentEmployeeNew.Id, $scope.recruitments);
        }
        else {
            $scope.Delete();
        }
    };

    $scope.confirmCloseDelete = function () {
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('hide');
    };
    // #endregion

    $scope.OperationList = [];
    $scope.Operation = null;
    $scope.showOperationPopUp = function (name) {
        if (name === 'OM') {
            $scope.Operation = "Operation Master";
            $http.get('employees/EmployeeInformation/GetOperationMaster')
                .then(function (response) {
                    $scope.OperationList = [];
                    $scope.OperationList = response.data;
                });
        }
        if (name === 'OV') {
            $scope.Operation = "Operation Variation";
            $http.get('employees/EmployeeInformation/GetOperationVariation')
                .then(function (response) {
                    $scope.OperationList = [];
                    $scope.OperationList = response.data;
                });
        }
        angular.element(document.querySelector('#OperationPopUp')).modal('show');
    };

    $scope.SetOperation = function (args) {
        if ($scope.Operation === "Operation Master") {
            var gridObj = $("#Grid").data("ejGrid");
            $scope.data = gridObj.getSelectedRecords()[0];
            $scope.preRecruitmentEmployeeNew.OperationMasterID = $scope.data.Id;
            $scope.preRecruitmentEmployeeNew.OperationMasterCode = $scope.data.Code;
            angular.element(document.querySelector('#OperationPopUp')).modal('hide');
            $scope.Operation = null;
        }
        if ($scope.Operation === "Operation Variation") {
            var gridObj = $("#Grid").data("ejGrid");
            $scope.data = gridObj.getSelectedRecords()[0];
            $scope.preRecruitmentEmployeeNew.OperationVariationId = $scope.data.Id;
            $scope.preRecruitmentEmployeeNew.OperationVariationCode = $scope.data.Code;
            angular.element(document.querySelector('#OperationPopUp')).modal('hide');
            $scope.Operation = null;
        }
    }
}