'use strict';
budgetCodeChangeController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function budgetCodeChangeController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'BudgetCode Update';
    $scope.index = -1;
    $scope.employees = [];
    $scope.path = 'employees/budgetcodechange/';
    $scope.getListUrl = $scope.path + 'getemployeelist';
    $scope.updateUrl = $scope.path + 'update';

    //The PAging System
    var x = document.getElementById("FDiv");
    var y = document.getElementById("SDiv");
    x.style.display = "block";
    y.style.display = "none";

    $scope.clickdde1 = function () {
        if (x.style.display === "none") {
            y.style.display = "none";
            x.style.display = "block";

        }
    };

    $scope.clickdde2 = function () {
        if (y.style.display === "none") {

            y.style.display = "block";
            x.style.display = "none";

        }
    };

    baseService.init($scope.getListUrl, null, 10, null, 'EmployeeCodeNumeric', 'EmployeeCode');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.employees = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.budgetCodeChange = {
        SystemId: null,
        EmployeeId: null,
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
        LegalDesignation: null,
        AgreedDOJ: null,
        TotalSalary: null,
        SpecialReviewDuration: null,
        SpecialReviewAmount: null,
        Image: null
    };
    $scope.budgetCodeChangeNew = Object.assign({}, $scope.budgetCodeChange);
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
            'name': 'Budget Code',
            'value': 'Code'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Designation',
            'value': 'Designation'
        },
        {
            'name': 'Phone Number',
            'value': 'CellPhnNo'
        }
    ];

    function checkValidation() {
        CheckField($scope.budgetCodeChangeNew.BudgetId, "Budget Code");
        CheckField($scope.budgetCodeChangeNew.GivenDesignationId, "Given Designation");
        CheckField($scope.budgetCodeChangeNew.Gender, "Gender");
        CheckField($scope.budgetCodeChangeNew.FullName, "Full Name");
        CheckField($scope.budgetCodeChangeNew.EmpType, "Emp Type");
        CheckField($scope.budgetCodeChangeNew.Email, "Email");
        CheckField($scope.budgetCodeChangeNew.Phone, "Phone");
        CheckField($scope.budgetCodeChangeNew.AgreedDOJ, "Agreed DOJ");
        CheckField($scope.budgetCodeChangeNew.InterviewRankingId, "Rank");
        CheckField($scope.budgetCodeChangeNew.Status, "Status");

        if (isNaN($scope.budgetCodeChangeNew.Phone)) {
            throw "Enter valid phone number";
        }
        if (isNaN($scope.budgetCodeChangeNew.NationalID)) {
            throw "Enter valid national id";
        }
        if (isNaN($scope.budgetCodeChangeNew.TotalSalary)) {
            throw "Enter valid number";
        }
        if (isNaN($scope.budgetCodeChangeNew.SpecialReviewDuration)) {
            throw "Enter valid number";
        }
        if (isNaN($scope.budgetCodeChangeNew.SpecialReviewAmount)) {
            throw "Enter valid number";
        }
        if ($scope.budgetCodeChangeNew.TotalSalary < 1) {
            throw "Total salary can not less than 1.";
        }
        if ($scope.budgetCodeChangeNew.SpecialReviewAmount < 0) {
            throw "Special review amount can not less than 0.";
        }
        if ($scope.budgetCodeChangeNew.SpecialReviewDuration < 0) {
            throw "Special review duration can not less than 0.";
        }

        var _ad = new Date($scope.budgetCodeChangeNew.AgreedDOJ);
        var _db = new Date($scope.budgetCodeChangeNew.DOB);

        var ad = $filter('dateFiltering')(_ad, 'dd-MMM-yyyy');
        var db = $filter('dateFiltering')(_db, 'dd-MMM-yyyy');

        if (_ad < _db) {
            throw "Date of birth [" + db + "] can not be greater than Agreed Date of join [" + ad + "]";
        }
    }

    $scope.NewbudgetCodeChange = {
        EntityName: null,
        Designation: null,
        PositionName: null,
        DesignationId: null
    };

    $scope.name = null;
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
    $scope.popUpTitle ="Budget Info"
    $scope.searchParameters = {
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
    $scope.popUp = function (name) {
        $scope.name = name;
        //if ($scope.name === 'Budget') {
        $scope.popUpDataList = [];
        $scope.popUpList = [];
        //$scope.popUpParameters.sort = 'Code';
        //$scope.popUpParameters.searchBy = 'Code';

        $scope.searchParameters.sort = 'Code';
        $scope.searchParameters.searchBy = 'Code';

        $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.searchParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                    $scope.searchParameters.sort = 'Code';
                    $scope.searchParameters.searchBy = 'Code';
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();


    };


    $scope.searchByUserList = [
        {
            'Text': 'Sequence',
            'Value': 'Sequence'
        },
        {
            'Text': 'Code',
            'Value': 'Code'
        },
        {
            'Text': 'Short Name',
            'Value': 'ShortName'
        },
        {
            'Text': 'Standard Name',
            'Value': 'StandardName'
        },
        {
            'Text': 'User Name',
            'Value': 'UserName'
        }
    ];
    $scope.popUpLD = function (name) {
        $scope.name = name;
        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.popUpParameters.sort = 'Sequence';
        $scope.popUpParameters.searchBy = 'UserName';
        $scope.popUpUrl = 'employees/RecruitmentApproval/GetLegalDesignationCbo?companyGroupId=' + $window.companyGroupId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        for (var i = 0; i < $scope.searchByUserList.length; i++) {
                            $scope.popUpList.push($scope.searchByUserList[i]);
                        }

                    }

                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#LDPopUp')).modal('show');
        $scope.getPopUpData();

    };


    $scope.selectDoubleClick = function (data) {
        if ($scope.name === 'Budget') {
            $scope.budgetCodeChangeNew.BudgetCode = data.Id;
            $scope.budgetCodeChangeNew.Code = data.Code;

            $scope.NewbudgetCodeChange.EntityName = data.EntityName;
            $scope.NewbudgetCodeChange.Designation = data.Designation;
            $scope.NewbudgetCodeChange.PositionName = data.PositionName;
            $scope.NewbudgetCodeChange.DesignationId = data.DesignationId;
            //$scope.name = null;
            angular.element(document.querySelector('#popUpId')).modal('hide');
            $scope.searchParameters.sort = 'Code';
            $scope.searchParameters.searchBy = 'Code';
        } else {
            $scope.budgetCodeChangeNew.LegalDesignationId = data.Id;
            $scope.budgetCodeChangeNew.LegalDesignation = data.UserName;
            $scope.GetGivenDesignationByLegalDesignaiton($scope.budgetCodeChangeNew.LegalDesignationId);
            //$scope.name = null;
            angular.element(document.querySelector('#LDPopUp')).modal('hide');

            $scope.popUpParameters.sort = 'Sequence';
            $scope.popUpParameters.searchBy = 'UserName';
        }
    };



    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.clearCode = function (name) {
        if (name === 'Budget') {
            $scope.budgetCodeChangeNew.BudgetId = null;
            $scope.budgetCodeChangeNew.EntityName = null;
            $scope.budgetCodeChangeNew.Designation = null;
            $scope.budgetCodeChangeNew.PositionName = null;
            $scope.budgetCodeChangeNew.GivenDesignationId = null;
            $scope.name = null;
        } else {
            $scope.budgetCodeChangeNew.LegalDesignationId = null;
            $scope.budgetCodeChangeNew.LegalDesignation = null;
            $scope.name = null;
        }
    };


    $scope.LegalDesignationList = [];
    //$scope.changeLegalDesignation = function () {
    //    cboService.getLegalDesignationCbobyGivenDesignation($scope.budgetCodeChangeNew.GivenDesignationId, function (result) {
    //        $scope.LegalDesignationList = result;
    //    });
    //}
    cboService.getCboLegalDesignation(null, function (result) {
        $scope.LegalDesignationList = result;
    });

    $scope.lowerGivenDesignationCbo = function (id, gid) {
        $scope.givenDesignationList = [];
        cboService.getCboLowerGivenDesignation(id, function (result) {
            $scope.givenDesignationList = result;
            $scope.budgetCodeChangeNew.GivenDesignationId = gid;
        });
    };

    $scope.uppderGivenDesignationCbo = function (id, gid) {
        $scope.givenDesignationList = [];
        cboService.getCboUpperGivenDesignation(id, function (result) {
            $scope.givenDesignationList = result;
            $scope.budgetCodeChangeNew.GivenDesignationId = gid;
        });
    };
    $scope.getDes = function () {
        if ($scope.budgetCodeChangeNew.IsExceptionalDesigApplicable === false) {
            $scope.lowerGivenDesignationCbo($scope.budgetCodeChangeNew.DesignationId);
        }
        else {
            $scope.uppderGivenDesignationCbo($scope.budgetCodeChangeNew.DesignationId);
        }
    };

    $scope.legalDesignationMessage = null;

    $scope.Get = function (data, index) {
        $scope.legalDesignationMessage = null;
        $scope.index = index;
        $scope.budgetCodeChange = $scope.employees[$scope.index];

        angular.copy($scope.budgetCodeChange, $scope.budgetCodeChangeNew);
        $scope.budgetCodeChangeNew.Code = $scope.budgetCodeChange.Code;
        $scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChange.GivenDesignationId;

        if (baseService.arrayLength($scope.LegalDesignationList) > 0) {
            for (var i = 0; i < $scope.LegalDesignationList.length; i++) {
                if ($scope.LegalDesignationList[i].Id !== $scope.budgetCodeChangeNew.LegalDesignationId) {
                    $scope.GetInActiveLegalDesignaion($scope.budgetCodeChangeNew.LegalDesignationId);
                }
            }
        }

        $scope.Action = 'Update';
        $scope.EntryShow();
    };

    $scope.EntryShow = function () {
        angular.element(document.querySelector('#EntryPopUp')).modal('show');
    };


    $scope.givenDesignationList = [];
    cboService.getCboGivenDesignation(function (result) {
        $scope.givenDesignationList = result;
    });

    $scope.GetGivenDesignationByLegalDesignaiton = function (legalDesignationId) {
        $http({
            method: 'GET',
            url: 'Employees/BudgetCodeChange/GetGivenDesignationByLegalDesignationCbo?legalDesignationId=' + legalDesignationId
        }).then(function successCallback(response) {
            $scope.givenDesignationList = response.data;
            $scope.budgetCodeChangeNew.GivenDesignationId = response.data[0].Value;
        })
    };


    $scope.GetInActiveLegalDesignaion = function (legalDesignationId) {
        $http({
            method: 'GET',
            url: 'Employees/BudgetCodeChange/GetInActiveLegalDesignaion?legalDesignationId=' + legalDesignationId
        }).then(function successCallback(response) {

            if (response.data[0].Active === false) {
                $scope.legalDesignationMessage = " designation is not Active.";
            } else {
                $scope.GetLegalSalaryGradeDesignation(legalDesignationId);
            }

        })
    };


    $scope.GetLegalSalaryGradeDesignation = function (legalDesignationId) {
        $http({
            method: 'GET',
            url: 'Employees/BudgetCodeChange/GetLegalSalaryGradeDesignation?legalDesignationId=' + legalDesignationId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) === 0) {
                $scope.legalDesignationMessage = " designation is not tagged with this plant.";
            }
        })
    };

    // #region Update
    $scope.Update = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.budgetCodeChangeNew.GivenDesignationId)) {
                throw "Given Designation is required.";
            }
            if (baseService.isUndefinedOrNull($scope.budgetCodeChangeNew.LegalDesignationId)) {
                throw "Legal Designation is required.";
            }
            if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'employeeInformation': $scope.budgetCodeChangeNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure", 'EntryPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, "success", 'EntryPopUp');
                        $scope.Clear();
                        angular.element(document.querySelector('#EntryPopUp')).modal('hide');
                        $scope.getData();
                        $scope.NewbudgetCodeChange = {};
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", 'EntryPopUp');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure", 'EntryPopUp');
        }
    };



    // #endregion

    $scope.Clear = function () {
        $scope.employee = {};
        return true;
        $scope.legalDesignationMessage = null;
    };



    // Workking on the 2nd Tab ************************************************************

    //Working of the Grids
    var ug = document.getElementById("UploadedList");
    var cg = document.getElementById("CurrentLists");

    ug.style.display = "none";
    cg.style.display = "none";



    //Variables
    $scope.ExcelUploadData = [];
    $scope.currentList = [];



    //Getting the Sample Data
    $scope.fileData = [];
    $scope.GetSample = function () {
        var reportFormat = "Excel";

        try {
            window.open('employees/budgetcodechange/GetSampleReport?reportFormat=' + reportFormat, '_blank');

        } catch (e) {

        }
    }

    //Importing the Data

    $scope.ModelNew = {
        FileName: null
    }

    $("#uploadFile").change(function () {
        $scope.fileData = this.files[0];
    });

    $scope.ImportData = function () {
        try {
            $scope.ExcelUploadData = [];
            $scope.msg = "";
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.fileData.length == 0) {

                throw ("Please Select A File!!");
            }


            var fileData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.fileData)) {
                $scope.ModelNew.FileName = $scope.fileData.name;
            }

            $http({
                method: 'POST',
                url: 'employees/budgetcodechange/' + 'ImportData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    fileData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.fileData) === false) {
                        fileData.append('file', data.file);
                    }
                    return fileData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.fileData }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");

                }

                else {
                    try {
                        $scope.ExcelUploadData = response.data;
                        ug.style.display = "block";
                        cg.style.display = "none";
                    }

                    catch (e) {
                        ShowResult(e, "failure");
                    }

                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    //Clearing the File List
    $scope.clearFileList = function () {
        $('#uploadFileTwo').val('');
        ug.style.display = "none";
        cg.style.display = "none";
    }

    //Saving the File List
    //Save the File Data
    $scope.saveFileList = function () {

        $http({
            method: 'POST',
            url: 'employees/budgetcodechange/' + 'SaveFileList',
            data: { 'data': $scope.ExcelUploadData }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                try {
                    if ($rootScope.isCollapsed == true) {
                        $rootScope.toggle();
                    }
                    $scope.getCurrentFileList();
                    ShowResult(response.data.Message, 'success')
                }
                catch (e) {

                    ShowResult(e, "failure");
                }
            }
        }, function errorCallback(response) {

        });
    }

    //Getting the Current List
    $scope.getCurrentFileList = function () {

        $http({
            method: 'GET',
            url: 'Employees/BudgetCodeChange/GetCurrentFileList'
        }).then(function successCallback(response) {
            $scope.currentList = response.data;
        })

        ug.style.display = "none";
        cg.style.display = "block";

    }
}