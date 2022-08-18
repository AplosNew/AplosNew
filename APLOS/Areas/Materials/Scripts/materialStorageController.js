'use strict';
materialStorageController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function materialStorageController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Material Storage";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerStyles = [];
    $scope.showTbl = false;
    $scope.path = 'Materials/MaterialStorage/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.buyerStyle = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.buyerStyleNew = Object.assign({}, $scope.buyerStyle);


    $scope.model = {
        SystemId: null,
        VendorId: null,
        PartyName: null,
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
        IsDirect: false,
        SalaryPercentage: 0,
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
        EmployeeCodeTypeId: null,
        EmployeeGroupSystemID: null,
        JobLocationID: null,
        DOB: null,
        DOJ: null,
        DOCIsDay: true,
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
        PayrollGroupId: null,
        AttendanceGroupId: null,
        AccountsGroupId: null,
        OperationMasterID: null,
        OperationVariationId: null,
        Unit: null,
        Division: null,
        Department: null,
        Section: null,
        Line: null,
        BudgetCategoryName: null,
        BudgetedDesignation: null,
        EmployeeGroup: null,
        EmpCategoryName: null,
        FixSystemID: null,
        IsEntryComplete: false,
        FirstTimeLock: false,
        Ref1CellPhnNo: null,
        Ref1Name: null,
        ApprovalAuthorityId: null,
        TransportGroupId: null,
        ResidenceGroupId: null,
        ExcludeOT: false,
        IsOutSider: false,
        EmpCodeType: null
    };
    $scope.employeeNew = Object.assign({}, $scope.model);
    $scope.employeeInformation = Object.assign({}, $scope.model);

    $scope.getDataList = function (buyerId) {
        baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.buyerStyleNew.CompanyId;
            $rootScope.parameters.plantId = $scope.buyerStyleNew.PlantId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerStyles = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        $scope.GetSequence();
    }
    $rootScope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.buyerStyleNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl = $scope.path + 'getautosequence?companyId=' + $scope.buyerStyleNew.CompanyId + '&plantId=' + $scope.buyerStyleNew.PlantId)
            .then(function (response) {
                $scope.buyerStyleNew.Sequence = response.data;
            });
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerStyle = $scope.buyerStyles[$scope.index];
        $scope.buyerStyleNew = $scope.buyerStyle;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    //#region Responsible ManPower

    $scope.name = null;
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.budgetpopUpParameters = {
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

        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.popUpParameters.sort = 'Code';
        $scope.popUpParameters.searchBy = 'Code';
        $scope.popUpUrl = 'Materials/MaterialStorage/getbudgetcodelist';
        baseService.setCurrentPage('dataList');
        $rootScope.parameters.plantId = $scope.buyerStyleNew.PlantId;

        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.budgetpopUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.budgetpopUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                    //$scope.popUpParameters.sort = 'Code';
                    //$scope.popUpParameters.searchBy = 'Code';
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
    };

    $scope.selectDoubleClick = function (data) {
        $scope.employeeNew.BudgetCode = data.Id;
        $scope.employeeNew.Code = data.Code;

        $scope.employeeNew.DesignationSystemID = data.DesignationId;
        $scope.employeeNew.Designation = data.Designation;
        $scope.employeeNew.PositionName = data.PositionName;
        $scope.employeeNew.DesignationId = data.DesignationId;
        $scope.employeeNew.UnitId = data.UnitId;
        $scope.employeeNew.DivisionId = data.DivisionId;
        $scope.employeeNew.DepartmentId = data.DepartmentId;
        $scope.employeeNew.SectionId = data.SectionId;
        $scope.employeeNew.SubSectionId = data.SubSectionId;
        $scope.employeeNew.SubdivisionID = data.SubdivisionID;
        $scope.employeeNew.LineId = data.LineId;
        $scope.employeeNew.EmploymentType = data.EmploymentType;
        $scope.employeeNew.PositionID = data.PositionId;
        $scope.employeeNew.IsDirect = data.IsDirect;

        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.clearCode = function () {
        $scope.employeeNew.BudgetCode = null;
        $scope.employeeNew.Code = null;
        $scope.employeeNew.EntityName = null;
        $scope.employeeNew.Designation = null;
        $scope.employeeNew.PositionName = null;

        $scope.employeeNew.DesignationId = null;
        $scope.employeeNew.UnitId = null;
        $scope.employeeNew.DivisionId = null;
        $scope.employeeNew.DepartmentId = null;
        $scope.employeeNew.SectionId = null;
        $scope.employeeNew.SubSectionId = null;
        $scope.employeeNew.SubdivisionID = null;
        $scope.employeeNew.LineId = null;
        $scope.employeeNew.EmployeeCodeTypeId = null;
        $scope.employeeNew.EmploymentType = null;
        $scope.employeeNew.PositionID = null;
        $scope.employeeNew.IsDirect = false;
    };

    //#endregion Responsible ManPower

    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.flg = null;
    $scope.popUpLD = function (flg) {
        $scope.flg = flg;
        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.popUpParameters.sort = 'Sequence';
        $scope.popUpParameters.searchBy = 'UserName';
        $scope.popUpUrl = 'employees/RecruitmentApproval/GetLegalDesignationCbo?companyGroupId=' + $window.companyGroupId + '&BudgetCode=' + $scope.employeeNew.BudgetCode;
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
                    $scope.popUpParameters.sort = 'Sequence';
                    $scope.popUpParameters.searchBy = 'UserName';
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#LDPopUp')).modal('show');
        $scope.getPopUpData();

    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.buyerStyleForm.$valid) {
            $scope.buyerStyle = Object.assign({}, $scope.buyerStyleNew);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.buyerStyle,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.buyerStyles.push($scope.buyerStyle);
                        $scope.buyerStyles = $filter('orderBy')($scope.buyerStyles, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.buyerStyle,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.buyerStyles[$scope.index] = $scope.buyerStyle;
                            $scope.buyerStyles = $filter('orderBy')($scope.buyerStyles, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.buyerStyleNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.buyerStyleNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.buyerStyles.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        $scope.buyerStyleNew = {};
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.buyerStyle = {};
        $scope.buyerStyleNew = {
            CompanyId: $scope.buyerStyleNew.CompanyId
            , PlantId: $scope.buyerStyleNew.PlantId
            , Sequence: seq
            , Active: true
        };
    }
};