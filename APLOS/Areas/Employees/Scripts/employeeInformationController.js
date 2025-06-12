'use strict';
employeeInformationController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', '$controller'];
function employeeInformationController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, $controller) {
    $rootScope.title = 'Employee Information';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.employees = [];
    $scope.path = 'employees/employeeinformation/';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $scope.partyType = "Vendor";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.partyList = [];
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveNewUrl = $scope.path + 'CreateNew';

    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.searchBy = "EmployeeCode"; $scope.search = "";
    $scope.searchByList = [{ value: 'EmployeeCode', name: "Employee Code" }, { value: 'EmployeeName', name: "Employee Name" }, { value: 'Department', name: "Department" }, { value: 'LegalDesignation', name: "Designation" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEmployeeDataList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employees = response.data;
        });
    }
    $scope.getData();

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
        EmpCodeType: null,
        CasteId: null,
        IsGlobalEmployee:false

    };
    $scope.employeeNew = Object.assign({}, $scope.model);
    $scope.employeeInformation = Object.assign({}, $scope.model);

    $scope.ResidenceGroupList = [];
    $scope.ResidenceGroupCbo = function () {
        $http.get('employees/ResidenceGroup/GetCbo')
            .then(function (response) {
                $scope.ResidenceGroupList = response.data;
            });
    }
    $scope.ResidenceGroupCbo();

    $scope.TransportGroupList = [];
    $scope.TransportGroupCbo = function () {
        $http.get('employees/TransportGroup/GetCbo')
            .then(function (response) {
                $scope.TransportGroupList = response.data;
            });
    }
    $scope.TransportGroupCbo();

    $scope.EmployeeCodeTypeList = [];
    $scope.EmployeeCodeTypeCbo = function () {
        $http.get('employees/EmployeeCodeType/GetCbo')
            .then(function (response) {
                $scope.EmployeeCodeTypeList = response.data;
            });
    }
    $scope.EmployeeCodeTypeCbo();

    $scope.AddNewEmpPopUp = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.employeeNew.EmployeeCodeTypeId)) {
                $scope.EmployeeCodeTypeId = $scope.employeeNew.EmployeeCodeTypeId;
                $scope.Clean();
                $scope.employeeNew.EmployeeCodeTypeId = $scope.EmployeeCodeTypeId;


                //$http({
                //    method: 'GET',
                //    url: 'Employees/EmployeeInformation/GetEmpCodeGenSetting?employeeCodeTypeId=' + $scope.employeeNew.EmployeeCodeTypeId
                //}).then(function successCallback(response) {
                //    if (baseService.arrayLength(response.data) == 0) {
                //        ShowResult("Employee Code Generation Setting is not defined.", 'failure');
                //    } else {
                //        $scope.IsEmployeeCodeOpenField = response.data[0].IsEmployeeCodeOpenField;
                //        $scope.ShowVendorCtrl();
                //    }
                //})
                        angular.element(document.querySelector('#NewEmpEntryPopUp')).modal('show');
               
            }
            else {
                throw "Select Employee Code Type.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ShowVendor = false;
    $scope.ShowEVendor = false;
    $scope.ShowVendorCtrl = function () {
        angular.forEach($scope.EmployeeCodeTypeList, function (item) {

            if (item.Value == $scope.employeeNew.EmployeeCodeTypeId) {
                $scope.employeeNew.EmpCodeType = item.Text;
                if (item.IsOutSider == true) {
                    $scope.employeeNew.IsOutSider = true;
                    $scope.ShowVendor = true;
                }
                else {
                    $scope.ShowVendor = false;
                }
            }

        });
    }

    $scope.EmpAddInfoLabel = null;
    $scope.EmpAddInfoLabelId = null;
    function GetCasteCboList() {
        $http({
            method: 'GET',
            url: 'Employees/Caste/GetCbo'
        }).then(function (response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.EmpAddInfoLabel = response.data[0].Text;
                $scope.EmpAddInfoLabelId = response.data[0].Value;
                GetCasteDetailCboList();
            }
        });
    }
    GetCasteCboList();

    $scope.CasteDetailCboList = [];
    function GetCasteDetailCboList() {
        $http({
            method: 'GET',
            url: 'Employees/Caste/GetChildCbo?masterId=' + $scope.EmpAddInfoLabelId
        }).then(function (response) {
            $scope.CasteDetailCboList = response.data;
        });
    }


    function GetEmpCodeGenSetting() {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/GetEmpCodeGenSetting?employeeCodeTypeId=' + $scope.employeeNew.EmployeeCodeTypeId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) == 0) {
                ShowResult("Employee Code Generation Setting is not defined.", 'failure');
            } else {
                $scope.IsEmployeeCodeOpenField = response.data[0].IsEmployeeCodeOpenField;
                $scope.ShowVendorCtrl();
            }
        })
    }

    $scope.CloseNewEmpPopUp = function () {
        $scope.EmployeeCodeTypeId = $scope.employeeNew.EmployeeCodeTypeId;
        $scope.Clean();
        $scope.employeeNew.EmployeeCodeTypeId = $scope.EmployeeCodeTypeId;
        angular.element(document.querySelector('#NewEmpEntryPopUp')).modal('hide');
        //$scope.ShowVendor = false;
        //$scope.ShowEVendor = false;

        //var eDialog = $("#NewEmpEntryPopUp").data("ejDialog");
        //eDialog.close();
    }

    $scope.CheckDuplicateEmployeeCode = function () {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/CheckDuplicateEmployeeCode?systemId=' + $scope.employeeNew.SystemId + '&employeeCode=' + $scope.employeeNew.EmployeeCode + '&EmployeeCodeTypeId=' + $scope.employeeNew.EmployeeCodeTypeId
        }).then(function successCallback(response) {
            if (response.data == false) {
                ShowResult("This EmployeeCode already exist.........EmployeeCode must be unique.", 'failure', 'NewEmpEntryPopUp');
            }
        })
    };

    $scope.EmpBankInfoModel = {
        RowID: null,
        EmpSystemID: null,
        BankSystemID: null,
        BankBranchId: null,
        BankAccNo: null,
        SalaryPercentage: 0,
        IsApproved: false,
        ApprovedDateTime: 0,
        PaymentMode: null,
        IFSCCode: null,
        MICRCode: null
    }


    $scope.ShowBankPopUp = function () {
        if ($scope.employeeNew.PaymentMode == "Bank") {
            $scope.EmpBankInfoModel.PaymentMode = $scope.employeeNew.PaymentMode;
            angular.element(document.querySelector('#EmpBankPopUp')).modal('show');
        } else {
            angular.element(document.querySelector('#EmpBankPopUp')).modal('hide');
        }
    }

    $scope.CloseBankPopUp = function () {
        angular.element(document.querySelector('#EmpBankPopUp')).modal('hide');
    }

    $scope.BankInfolist = [];
    $scope.GetBankInfo = function () {
        $http.get('Leave/EmployeeBankInfoInformation/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.BankInfolist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#BankInFoPopUp')).modal('show');

    };
    $scope.closeBankInfoPopUp = function () {
        angular.element(document.querySelector('#BankInFoPopUp')).modal('hide');
    }

    $scope.SetBankData = function (obj) {
        var Bankinfo = obj.data;
        $scope.EmpBankInfoModel.UserName = Bankinfo.UserName;
        $scope.EmpBankInfoModel.BankBranch = Bankinfo.BankBranch;
        $scope.EmpBankInfoModel.BankSystemID = Bankinfo.BankSystemID;
        $scope.EmpBankInfoModel.BankBranchId = Bankinfo.BankBranchId;

        angular.element(document.querySelector('#BankInFoPopUp')).modal('hide');
    };

    $scope.nomineeInfo = {
        Id: null,
        EmpSystemId: null,
        Name: null,
        LocalName: null,
        Relation: null,
        CellNo: null,
        AddressLocal: null,
        Address: null,
        DOB: null,
        NationalID: null
    }


    $scope.DOJpastDays = 0;
    $scope.Operation = null;
    $scope.GetPlantWiseHRMSSetting = function () {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/GetPlantWiseHRMSSetting'
        }).then(function successCallback(response) {

            $scope.CountryId = response.data[0].CountryId;
            $scope.DOJpastDays = response.data[0].PastDOJDaysAllowed;

            var _date = new Date();
            var _fdate = $filter('dateFiltering')(new Date(_date.setDate(_date.getDate() - $scope.DOJpastDays)), 'dd-MM-yyyy');
            var _ldate = $filter('dateFiltering')(new Date(), 'dd-MM-yyyy');

            if ($scope.DOJpastDays != 0) {
                $('.datepic').datepicker({
                    startDate: _fdate,
                    endDate: _ldate,
                    datesDisabled: $scope.DisabledDates,
                    format: 'dd-M-yyyy',
                    todayHighlight: true,
                    autoclose: true,
                    inline: true,
                    changeMonth: true
                });
            }

            if (response.data[0].DOCBaseON === "Month") {
                $scope.showMonthInput = true;
                $scope.showDayInput = false;
                $scope.employeeNew.DOCIsMonth = true;
                $scope.employeeNew.DOCIsDay = false;
                $scope.employeeNew.DOCMonth = response.data[0].DOCCount;

                $scope.showEmpMonthInput = true;
                $scope.showEmpDayInput = false;
                $scope.employeeInformation.DOCIsMonth = true;
                $scope.employeeInformation.DOCIsDay = false;
                $scope.employeeInformation.DOCMonth = response.data[0].DOCCount;


                $scope.ShowMonthIsDayInPut();

            }
            else {
                $scope.showMonthInput = false;
                $scope.showDayInput = true;
                $scope.employeeNew.DOCIsDay = true;
                $scope.employeeNew.DOCIsMonth = false;
                $scope.employeeNew.DOCDay = response.data[0].DOCCount;
                $scope.ShowDOCIsDayInPut();

            }
            $scope.IsEmployeeCodeOpenField = response.data[0].IsEmployeeCodeOpenField;
            $scope.EmployeeCodeCheckLevel = response.data[0].EmployeeCodeCheckLevel;
            $scope.IsReferenceRequired = response.data[0].IsReferenceRequired;
            $scope.IsTransportGroupMandatory = response.data[0].IsTransportGroupMandatory;
            $scope.IsResidenceGroupMandatory = response.data[0].IsResidenceGroupMandatory;


            $scope.Tin = response.data[0].TINCaption;
            if (baseService.isUndefinedOrNull($scope.Tin)) {
                $scope.Tin = "TIN";
            }
            $scope.Nid = response.data[0].NIDCaption;
            if (baseService.isUndefinedOrNull($scope.Nid)) {
                $scope.Nid = "National ID";
            }
            $scope.NidLength = response.data[0].NIDLength;
            $scope.TinLength = response.data[0].TINLength;
            $scope.Operation = response.data[0].Operation;

            addressService.getCountryCbo(function (result) {
                $scope.citizenList = result;
                $scope.employeeNew.CitizenID = $scope.CountryId;
                $scope.employeeInformation.CitizenID = $scope.CountryId;
            });
        })
    };
    $scope.GetPlantWiseHRMSSetting();

    $scope.DisabledDates = [];

    $scope.ShowDOCIsDayInPut = function () {
        if ($scope.employeeNew.DOCIsDay === true) {
            $scope.employeeNew.DOCIsMonth = false;
            $scope.showMonthInput = false;
            $scope.showDayInput = true;
        }
    }
    $scope.ShowMonthIsDayInPut = function () {
        if ($scope.employeeNew.DOCIsMonth === true) {
            $scope.employeeNew.DOCIsDay = false;
            $scope.showMonthInput = true;
            $scope.showDayInput = false;
        }
    }
    $scope.SetDoc = function () {
        if ($scope.employeeNew.DOCIsMonth) {
            var dt = new Date($scope.employeeNew.DOJ);
            $scope.DOC = new Date(dt.setMonth(dt.getMonth() + $scope.employeeNew.DOCMonth));
            $scope.employeeNew.DOC = $filter('dateFiltering')(new Date($scope.DOC), 'dd-MM-yyyy');
        }
        if ($scope.employeeNew.DOCIsDay) {
            var dt = new Date($scope.employeeNew.DOJ);
            $scope.DOC = new Date(dt.setDate(dt.getDate() + $scope.employeeNew.DOCDay));
            $scope.employeeNew.DOC = $filter('dateFiltering')(new Date($scope.DOC), 'dd-MM-yyyy');
        }
    }

    $scope.ShowEmpDOCIsDayInPut = function () {
        if ($scope.employeeInformation.DOCIsDay === true) {
            $scope.employeeInformation.DOCIsMonth = false;
            $scope.showEmpMonthInput = false;
            $scope.showEmpDayInput = true;
        }
    }
    $scope.ShowEmpMonthIsDayInPut = function () {
        if ($scope.employeeInformation.DOCIsMonth === true) {
            $scope.employeeInformation.DOCIsDay = false;
            $scope.showEmpMonthInput = true;
            $scope.showEmpDayInput = false;
        }
    }

    $scope.SetEmpDoc = function () {
        if ($scope.employeeInformation.DOCIsMonth) {
            var dt = new Date($scope.employeeInformation.DOJ);
            $scope.DOC = new Date(dt.setMonth(dt.getMonth() + $scope.employeeInformation.DOCMonth));
            $scope.employeeInformation.DOC = $filter('dateFiltering')(new Date($scope.DOC), 'dd-MM-yyyy');
        }
        if ($scope.employeeInformation.DOCIsDay) {
            var dt = new Date($scope.employeeInformation.DOJ);
            $scope.DOC = new Date(dt.setDate(dt.getDate() + $scope.employeeInformation.DOCDay));
            $scope.employeeInformation.DOC = $filter('dateFiltering')(new Date($scope.DOC), 'dd-MM-yyyy');
        }
    }

    $scope.SetLeaveOnDOC = function () {
        $scope.employeeInformation.isLeaveOnDOC = true;
        $scope.employeeInformation.isLeaveOnDOJ = false;
    }

    $scope.SetLeaveOnDOJ = function () {
        $scope.employeeInformation.isLeaveOnDOC = false;
        $scope.employeeInformation.isLeaveOnDOJ = true;
    }

    //#region BudgetCode

    $scope.name = null;
    $scope.popUpTitle = "Manpower Budget Information";
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
        $scope.budgetpopUpParameters.sort = 'Code';
        $scope.budgetpopUpParameters.searchBy = 'Code';
        $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
        baseService.setCurrentPage('dataList');
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
        $scope.getPopUpData();
    };

    $scope.selectDoubleClick = function (data) {
        try {
            $scope.employeeNew.BudgetCode = data.Id;
            $scope.employeeNew.Code = data.Code;
            $scope.GetOnRollByBudget(data.Id);
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
            $scope.employeeNew.FixSystemID = data.ShiftDefinationId;


        } catch (e) {
            ShowResult(e, 'failure');
        }
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

    $scope.GetOnRollByBudget = function (budgetId) {
        try {
            $http.get('employees/EmployeeInformation/GetOnRollByBudget?budgetId=' + budgetId)
                .then(function (response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        if (response.data[0].TotalNumber < response.data[0].OnRollManPwr || response.data[0].TotalNumber == response.data[0].OnRollManPwr) {
                            ShowResult("On Roll Manpower is exceeding Budgeted Manpower.", 'failure', 'popUpId');
                        }
                        else {
                            angular.element(document.querySelector('#popUpId')).modal('hide');
                        }
                    } else {
                        ShowResult("On Roll Manpower is not defined in Budgeted Manpower.", 'failure', 'popUpId');
                    }
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //#endregion BudgetCode

    //#region LegalDesignation

    cboService.getCboLegalDesignation(null, function (result) {
        $scope.LegalDesignationList = result;
    });

    $scope.legalDesignationMessage = null;
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


    $scope.selectLegalDesignationDoubleClick = function (data) {
        if ($scope.flg === 'new') {
            $scope.employeeNew.LegalDesignationId = data.Id;
            $scope.employeeNew.LegalDesignation = data.UserName;

            $scope.GetGivenDesignationByLegalDesignaiton($scope.employeeNew.LegalDesignationId);
        }
        else {
            $scope.employeeInformation.LegalDesignationId = data.Id;
            $scope.employeeInformation.LegalDesignation = data.UserName;

            $scope.GetGivenDesignationByLegalDesignaiton($scope.employeeInformation.LegalDesignationId);
        }

        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    };

    $scope.clearLegalDesignaitonCode = function () {
        $scope.budgetCodeChangeNew.LegalDesignationId = null;
        $scope.budgetCodeChangeNew.LegalDesignation = null;
    };

    $scope.GetGivenDesignationByLegalDesignaiton = function (legalDesignationId) {
        $http({
            method: 'GET',
            url: 'Employees/BudgetCodeChange/GetGivenDesignationByLegalDesignationCbo?legalDesignationId=' + legalDesignationId + '&BudgetCode=' + $scope.employeeNew.BudgetCode
        }).then(function successCallback(response) {
            $scope.givenDesignationList = response.data;
            if ($scope.flg === 'new') {
                $scope.employeeNew.GivenDesignationId = response.data[0].Value;
                $scope.employeeNew.GivenDesignation = response.data[0].Text;

            } else {
                $scope.employeeInformation.GivenDesignationId = response.data[0].Value;
                $scope.employeeInformation.GivenDesignation = response.data[0].Text;
                $scope.GetIsOTEntitled();
            }

        })
        $scope.GetInActiveLegalDesignaion(legalDesignationId);
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


    //#endregion LegalDesignation

    $scope.JobLocationList = [];
    $scope.LoadAllJobLocation = function () {
        $scope.JobLocationList = [];
        $scope.Flag = "Load All";
        $scope.PlantId = null;
        $http.get('employees/EmployeeInformation/GetJobLocationCbo?flag=' + $scope.Flag)
            .then(function (response) {
                $scope.JobLocationList = response.data;
                $scope.employeeNew.JobLocationID = response.data[0].SystemID;
            });

        $scope.Flag = "Load Less";
    };

    $scope.Flag = "Load Less";
    $scope.LoadPlantJobLocation = function () {
        $scope.JobLocationList = [];
        $scope.PlantId = null;
        $scope.Flag = "Load Less";
        $http.get('employees/EmployeeInformation/GetJobLocationCbo?flag=' + $scope.Flag)
            .then(function (response) {
                $scope.JobLocationList = response.data;
                $scope.employeeNew.JobLocationID = response.data[0].SystemID;
                $scope.GetShiftCbo();
            });
        $scope.Flag = "Load All";
    };
    $scope.LoadPlantJobLocation();

    $scope.fixedShitList = [];
    $scope.PlantId = null;
    $scope.GetShiftCbo = function () {
        $scope.fixedShitList = [];
        $scope.PlantId = $.grep($scope.JobLocationList, function (item) {
            return item.SystemID === $scope.employeeNew.JobLocationID;
        })[0].PlantID;

        $http.get('employees/EmployeeInformation/GetCboShiftDefinationByPlant?plantId=' + $scope.PlantId)
            .then(function (response) {
                $scope.fixedShitList = response.data;
            });
    };

    $scope.GetLocalLabel = function () {
        $http.get('employees/employeeinformation/getlocallanguagelabel?plantId=' + $window.plantId)
            .then(function (response) {
                $scope.LocalLabel = response.data;

                if (!baseService.isUndefinedOrNull($scope.LocalLabel.NameLabel)) $scope.NameLabel = $scope.LocalLabel.NameLabel; else $scope.NameLabel = "Employee Name";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.FatherNameLabel)) $scope.FatherNameLabel = $scope.LocalLabel.FatherNameLabel; else $scope.FatherNameLabel = "Father's Name";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.IdentificationMarksLabel)) $scope.IdentificationMarksLabel = $scope.LocalLabel.IdentificationMarksLabel; else $scope.IdentificationMarksLabel = "Identification Marks";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.PAddressLabel)) $scope.PAddressLabel = $scope.LocalLabel.PAddressLabel; else $scope.PAddressLabel = "Present Address";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.SpouseNameLabel)) $scope.SpouseNameLabel = $scope.LocalLabel.SpouseNameLabel; else $scope.SpouseNameLabel = "Spouse Name";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.MotherNameLabel)) $scope.MotherNameLabel = $scope.LocalLabel.MotherNameLabel; else $scope.MotherNameLabel = "Mother's Name";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.PermanentLabel)) $scope.PermanentLabel = $scope.LocalLabel.PermanentLabel + " " + $scope.LocalLabel.AddressLabel; else $scope.PermanentLabel = "Permanent Address";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.AddressLabel)) $scope.AddressLabel = $scope.LocalLabel.AddressLabel; else $scope.AddressLabel = "Address";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.DependantLabel)) $scope.DependantLabel = $scope.LocalLabel.DependantLabel; else $scope.DependantLabel = "Dependant";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.LandLabel)) $scope.LandLabel = $scope.LocalLabel.LandLabel; else $scope.LandLabel = "Land";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.MobileNoLabel)) $scope.MobileNoLabel = $scope.LocalLabel.MobileNoLabel; else $scope.MobileNoLabel = "MobileNo";
                if (!baseService.isUndefinedOrNull($scope.LocalLabel.MobileNoLabel)) $scope.MobileNoLabel = $scope.LocalLabel.MobileNoLabel; else $scope.MobileNoLabel = "MobileNo";


                // $scope.PAddressLabel = $scope.LocalLabel.PAddressLabel;
                //$scope.OperationSetting = $scope.LocalLabel.OperationSetting;

            });
    };
    $scope.GetLocalLabel();

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
            'name': 'Designation',
            'value': 'Designation'
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

    $scope.ApprovalAuthorityList = [];
    $scope.GetApprovalAuthority = function () {
        $http.get('employees/employeeinformation/GetApprovalAuthority')
            .then(function (response) {
                $scope.ApprovalAuthorityList = response.data;
            });
    };
    $scope.GetApprovalAuthority();

    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
    });

    cboService.getCboSalutationByCompanyGroup(null, function (result) {
        $scope.salutaionList = result;
    });

    cboService.getCboDepartment(function (result) {
        $scope.departmentList = result;
    });

    $scope.designationList = [];
    cboService.getCboDesignation(function (result) {
        $scope.designationList = result;
    });

    $scope.relationList = [];
    cboService.getRelationCbo(function (result) {
        $scope.relationList = result;
    });

    $scope.ProfessionList = [];
    cboService.getProfessionCbo(function (result) {
        $scope.ProfessionList = result;
    });

    $scope.payrollGroupList = [];
    cboService.getCboPayRollGroupCbo(null, function (result) {
        $scope.payrollGroupList = result;
    });
    $scope.givenDesignationList = [];
    cboService.getCboGivenDesignation(function (result) {
        $scope.givenDesignationList = result;
    });
    $scope.attendanceGroupList = [];
    cboService.getAttendanceGroupCbo(function (result) {
        $scope.attendanceGroupList = result;
    });
    $scope.showEntity = function () {
        $http.get('employees/employeeprobationalperiod/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    };

    $scope.SaveEmployeeComplianceDocument = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.user)) {
                throw "Select or Save an Employee first.";
            }

            $http({
                method: 'POST',
                url: 'Employees/EmployeeInformation/SaveEmployeeComplianceDocument',
                data: {
                    'empId': $scope.user, 'plantId': $scope.PlantId, 'givenDesignationId': $scope.employeeInformation.GivenDesignationId
                    , 'budgetId': $scope.employeeInformation.BudgetCode, 'empType': $scope.employeeInformation.EmpType
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.Loaddocumentdatalist($scope.user);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.OperationList = [];
    $scope.showOperationPopUp = function (name) {
        if (name === 'OM' || name === 'SOM') {
            $scope.Operation = "Operation Master";
            //$http.get('employees/EmployeeInformation/GetOperationMaster?empSystemId=' + $scope.employeeInformation.SystemId)
            $http.get('employees/EmployeeInformation/GetOperationMaster')
                .then(function (response) {
                    $scope.OperationList = [];
                    $scope.OperationList = response.data;
                });
        }
        if (name === 'OV' || name === 'SOV') {
            $scope.Operation = "Operation Variation";
            //$http.get('employees/EmployeeInformation/GetOperationVariation?empSystemId=' + $scope.employeeInformation.SystemId)
            $http.get('employees/EmployeeInformation/GetOperationVariation')
                .then(function (response) {
                    $scope.OperationList = [];
                    $scope.OperationList = response.data;
                });
        }

        if (name === 'MOM') {
            $scope.Operation = "Operation Master";
            //$http.get('employees/EmployeeInformation/GetOperationMaster?empSystemId=' + $scope.employeeInformation.SystemId)
            $http.get('employees/EmployeeInformation/GetOperationMaster')
                .then(function (response) {
                    $scope.OperationList = [];
                    $scope.OperationList = response.data;
                });
        }
        if (name === 'MOV') {
            $scope.Operation = "Operation Variation";
            //$http.get('employees/EmployeeInformation/GetOperationVariation?empSystemId=' + $scope.employeeInformation.SystemId)
            $http.get('employees/EmployeeInformation/GetOperationVariation')
                .then(function (response) {
                    $scope.OperationList = [];
                    $scope.OperationList = response.data;
                });
        }

        if (name === 'OM' || name === 'OV') {
            angular.element(document.querySelector('#OperationPopUp')).modal('show');
        } else if (name === 'SOM' || name === 'SOV') {
            angular.element(document.querySelector('#SOperationPopUp')).modal('show');
        }
        else {
            angular.element(document.querySelector('#MultiOperationPopUp')).modal('show');
        }
    };

    $scope.SetNewEmpOperation = function (args) {
        if ($scope.Operation === "Operation Master") {
            var gridObj = $("#GridO").data("ejGrid");
            $scope.data = gridObj.getSelectedRecords()[0];
            $scope.employeeNew.OperationMasterID = $scope.data.Id;
            $scope.employeeNew.OperationMasterCode = $scope.data.Code;
            angular.element(document.querySelector('#SOperationPopUp')).modal('hide');

        }
        if ($scope.Operation === "Operation Variation") {
            var gridObj = $("#GridO").data("ejGrid");
            $scope.data = gridObj.getSelectedRecords()[0];
            $scope.employeeNew.OperationVariationId = $scope.data.Id;
            $scope.employeeNew.OperationVariationCode = $scope.data.Code;
            angular.element(document.querySelector('#SOperationPopUp')).modal('hide');

        }
    }

    $scope.SetOperation = function (args) {
        if ($scope.Operation === "Operation Master") {
            var gridObj = $("#Grid").data("ejGrid");
            $scope.data = gridObj.getSelectedRecords()[0];
            $scope.employeeInformation.OperationMasterID = $scope.data.Id;
            $scope.employeeInformation.OperationMasterCode = $scope.data.Code;
            angular.element(document.querySelector('#OperationPopUp')).modal('hide');

        }
        if ($scope.Operation === "Operation Variation") {
            var gridObj = $("#Grid").data("ejGrid");
            $scope.data = gridObj.getSelectedRecords()[0];
            $scope.employeeInformation.OperationVariationId = $scope.data.Id;
            $scope.employeeInformation.OperationVariationCode = $scope.data.Code;
            angular.element(document.querySelector('#OperationPopUp')).modal('hide');

        }
    }

    // #region checkbox all

    $scope.refreshTemplateOperation = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridMultioperation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.OperationList.length; i++) {
                $scope.OperationList[i].check = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridMultioperation").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.empReferenceInformation = {
        SystemID: null,
        EmpSystemID: null,
        Ref1Name: null,
        Ref1EmployerName: null,
        Ref1EmployerAddress: null,
        Ref1Designation: null,
        Ref1CellPhnNo: null,
        Ref1TelePhnNo: null,
        Ref1Email: null,
        Ref1Address: null,
        Ref2Name: null,
        Ref2EmployerName: null,
        Ref2EmployerAddress: null,
        Ref2Designation: null,
        Ref2CellPhnNo: null,
        Ref2TelePhnNo: null,
        Ref2Email: null,
        Ref2Address: null
    };

    $scope.empAcademicQualificationInformation = {
        SystemID: null,
        EmpSystemID: null,
        TypeIsAcademic: null,
        EductLevelSystemID: null,
        IsEnglishMedium: null,
        HasDistinction: null,
        ExamDegreeType: null,
        StreamId: null,
        InstituteName: null,
        CountryId: null,
        YearOfPass: null,
        Session: null,
        Achievement: null,
        FileId: null,
        FileName: null,
        IsQualificationApproved: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        ComplianceDocumentId: null
    };

    $scope.empTrainingInformation = {
        SystemID: null,
        EmpSystemID: null,
        TrainingTitle: null,
        TopicCovered: null,
        InstituteName: null,
        CountrySystemID: null,
        Location: null,
        TrainingYear: null,
        Duration: null,
        DurationUOM: null,
        FileId: null,
        FileName: null,
        IsTrainingApproved: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        ComplianceDocumentId: null
    };

    $scope.empExperienceInformation = {
        SystemID: null,
        EmpSystemID: null,
        Employer: null,
        Designation: null,
        StartDate: null,
        EndDate: null,
        JobDescription: null,
        Achievement: null,
        IsCurrentJob: null,
        IsPartTime: null,
        DurationYear: null,
        DurationMonth: null,
        FileId: null,
        FileName: null,
        IsExperienceApproved: null,
        ApprovedBy: null,
        ApprovedFromIP: null,
        ApprovedDateTime: null,
        ComplianceDocumentId: null,
        IssueDate: null
    };
    $scope.employeeDocument = {
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

    $scope.Get = function (obj) {
        $scope.approved = "";
        $scope.employeeInformation = obj.data;

        $scope.imageSrc = virtualPath.EmployeePic + $scope.employeeInformation.EmpPicPath;
        $scope.EmpSignature = virtualPath.CardHolderSignature + $scope.employeeInformation.EmpSignature;
        $rootScope.img = $scope.employeeInformation.EmpPicPath;
        $scope.user = $scope.employeeInformation.SystemId;
        $scope.CompanyGroupID = $scope.employeeInformation.GroupID;
        $scope.CompanyID = $scope.employeeInformation.CompanyId;
        $scope.CountryId = $scope.employeeInformation.CountryId;
        $scope.BudgetCode = $scope.employeeInformation.BudgetCode;
        $scope.PlantId = $scope.employeeInformation.PlantId;
        $scope.PaymentMode = $scope.employeeInformation.PaymentMode;
        $scope.ShiftDefination = $scope.employeeInformation.ShiftDefination;

        $scope.employeeInformation.DOB = $filter('dateFiltering')($scope.employeeInformation.DOB, 'dd-M-yyyy');
        $scope.employeeInformation.BirthdayCelebrationDate = $filter('dateFiltering')($scope.employeeInformation.BirthdayCelebrationDate, 'dd-M-yyyy');
        $scope.employeeInformation.DOJ = $filter('dateFiltering')($scope.employeeInformation.DOJ, 'dd-M-yyyy');
        $scope.employeeInformation.DOC = $filter('dateFiltering')($scope.employeeInformation.DOC, 'dd-M-yyyy');
        $scope.employeeInformation.IssueDate = $filter('dateFiltering')($scope.employeeInformation.IssueDate, 'dd-M-yyyy');
        $scope.employeeInformation.MarriagedayCelebrationDate = $filter('dateFiltering')($scope.employeeInformation.MarriagedayCelebrationDate, 'dd-M-yyyy');
        $scope.employeeInformation.PaymentModeEffectiveDate = $filter('dateFiltering')($scope.employeeInformation.PaymentModeEffectiveDate, 'dd-M-yyyy');
        $scope.employeeInformation.EmpCodeType = $scope.employeeInformation.EmployeeCodeType;
        if ($scope.employeeInformation.isLeaveOnDOC == false) {
            $scope.employeeInformation.isLeaveOnDOJ = true;
        } else {
            $scope.employeeInformation.isLeaveOnDOC = true;
        }
        $scope.approved = "";
        if ($scope.employeeInformation.IsApproved) {
            $scope.approved = "Employee Profile is Approved.";
            $scope.Color = 'green';
        }
        else {
            $scope.approved = "Employee Profile is not Approved.";
            $scope.Color = 'red';
        }

        if (baseService.isUndefinedOrNull($scope.employeeInformation.FirstName)) {
            $scope.showdiv = false;
        }
        else {
            if ($scope.employeeInformation.FirstName.length > 0) {
                $scope.showdiv = true;
            }
            else {
                $scope.showdiv = false;
            }
        }

        if ($scope.employeeInformation.IsOutSider) {
            $scope.ShowEVendor = true;
        } else {
            $scope.ShowEVendor = false;
        }

        if (baseService.arrayLength($scope.LegalDesignationList) > 0) {
            for (var i = 0; i < $scope.LegalDesignationList.length; i++) {
                if ($scope.LegalDesignationList[i].Id !== $scope.employeeInformation.LegalDesignationId) {
                    $scope.GetInActiveLegalDesignaion($scope.employeeInformation.LegalDesignationId);
                }
            }
        }

        $scope.LoadReferenceData($scope.user);
        $scope.LoadQualificationData($scope.user);
        $scope.LoadExperienceData($scope.user);
        $scope.LoadTrainingData($scope.user);
        $scope.getSalutationList($scope.CompanyGroupID);
        $scope.Loaddocumentdatalist($scope.user);
        if (baseService.isUndefinedOrNull($scope.employeeInformation.EmpPicPath)) {
            $scope.imageSrc = null;
            if ($rootScope.GenderID === 'Male') {
                $scope.imageSrc = "empprofile/Images/male-alt.png";
            } else {
                $scope.imageSrc = "empprofile/Images/female-alt.png";
            }
        }

        $scope.citizenList = [];
        addressService.getCountryCbo(function (result) {
            $scope.citizenList = result;
            $scope.employeeInformation.CitizenID = $scope.CountryId;
            $scope.employeeNew.CitizenID = $scope.CountryId;

            $scope.PresCountryList = result;
            $scope.employeeInformation.PresCountryID = $scope.CountryId;

            $scope.GetPresStateOnCountryChange($scope.employeeInformation.PresCountryID);
            $scope.onPreDistrictChange($scope.employeeInformation.PresDistrictID);
            $scope.getPresDisOnPreStateChange($scope.employeeInformation.PresStateId);
            $scope.GetPresPoliceStationCboByDistrictChange($scope.employeeInformation.PresDistrictID);
            $scope.GetPresPostOfficeCboByDistrictChange($scope.employeeInformation.PresDistrictID);

            $scope.ParmCountryList = result;
            $scope.employeeInformation.ParmCountryID = $scope.CountryId;
            $scope.GetParmStateOnCountryChange($scope.employeeInformation.ParmCountryID);
            $scope.onParmDistrictChange($scope.employeeInformation.ParmDistrictID);
            $scope.getParmDisOnParmStateChange($scope.employeeInformation.ParmStateId);
            $scope.GetParmPoliceStationCboByDistrictChange($scope.employeeInformation.ParmDistrictID);
            $scope.GetParmPostOfficeCboByDistrictChange($scope.employeeInformation.ParmDistrictID);
        });
        //$scope.GetAllJobLocation();
        $scope.celebrationMarriage();
        $scope.getEmployeeNomineeInfo();
        $scope.getEmployeeDependantInfo();
        $scope.getEmployeeLandLordInfo();
        $scope.getSavedOperationData($scope.employeeInformation.SystemId);
        $scope.GetIsOTEntitled();


        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.IsOTEntitled = false;
    $scope.OTEntitledmsg = null;
    $scope.GetIsOTEntitled = function () {
        $http.get('employees/EmployeeInformation/GetIsOTEntitled?PlantId=' + $scope.employeeInformation.PlantId + '&designationId=' + $scope.employeeInformation.GivenDesignationId)
            .then(function (response) {
                $scope.IsOTEntitled = response.data[0].IsOTEntitled;

                if ($scope.IsOTEntitled)
                    $scope.OTEntitledmsg = "(OT entitle as per designation)";
                else
                    $scope.OTEntitledmsg = null;
            });
    }

    $scope.employeeInformation.ApplyingAsFresher = false;

    $scope.celebrationMarriage = function () {
        if (!baseService.isUndefinedOrNull($scope.employeeInformation.CivilStatusID)) {
            $scope.celebrationType = $.grep($scope.civilStatusList, function (item) {
                return item.Value === $scope.employeeInformation.CivilStatusID;
            })[0].HasPartner;
            if ($scope.celebrationType) {
                //do nothing
            }
            else {
                $scope.employeeInformation.MarriagedayCelebrationDate = null;
                $scope.employeeInformation.SpouseNationalID = null;
                $scope.employeeInformation.SpouseName = null;
                $scope.employeeInformation.SpouseOccupation = null;
                $scope.employeeInformation.NoOfChildren = null;
            }
        }
    };

    $scope.LoadReferenceData = function (empid) {
        $http.get('employees/employeeinformation/getreferencedata?empid=' + $scope.user)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.empReferenceInformation = response.data[0];
                }
            });
    };

    $scope.LoadQualificationData = function (empid) {
        $http.get('employees/employeeinformation/getqualificationdata?empid=' + $scope.user)
            .then(function (response) {
                $scope.empAcademicQualificationInformations = response.data;
            });
    };

    $scope.LoadExperienceData = function (empid) {
        $http.get('employees/employeeinformation/getexperiencedata?empid=' + $scope.user)
            .then(function (response) {
                $scope.empExperienceInformations = response.data;
            });
    };

    $scope.LoadTrainingData = function (empid) {
        $http.get('employees/employeeinformation/gettrainingdata?empid=' + $scope.user)
            .then(function (response) {
                $scope.empTrainingInformations = response.data;
            });
    };

    $scope.disableBtn = function () {
        if ($scope.employeeInformation.Submitted === false) {
            return false;
        }
        else {
            $scope.savedisable = true;
            return true;
        }
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

    $scope.countDate = function () {
        if ($scope.empExperienceInformation.IsCurrentJob === false) {
            var st = new Date($scope.empExperienceInformation.StartDate);
            var ed = new Date($scope.empExperienceInformation.EndDate);

            var nowyear = ed.getFullYear();
            var nowmonth = ed.getMonth() + 1;
            var nowday = ed.getDate();

            var styear = st.getFullYear();
            var stmonth = st.getMonth() + 1;
            var stday = st.getDate();

            var age = nowyear - styear;
            var age_month = nowmonth - stmonth;
            var age_day = nowday - stday;

            if (age_month < 0 || age_month === 0 && age_day < 0) {
                age = parseInt(age) - 1;
                age_month += 12;
            }
            if (age_month === 12) {
                age_month = 0;
                age = age + 1;
            }

            $scope.empExperienceInformation.DurationYear = age;
            $scope.empExperienceInformation.DurationMonth = age_month;
        }
        else {
            var std = new Date($scope.empExperienceInformation.StartDate);
            var end = new Date($scope.empExperienceInformation.EndDate);
            var snowyear = end.getFullYear();
            var snowmonth = end.getMonth() + 1;
            var snowday = end.getDate();

            var sstyear = std.getFullYear();
            var sstmonth = std.getMonth() + 1;
            var sstday = std.getDate();

            var sage = snowyear - sstyear;
            var sage_month = snowmonth - sstmonth;
            var sage_day = snowday - sstday;

            if (sage_month < 0 || sage_month === 0 && sage_day < 0) {
                sage = parseInt(sage) - 1;
                sage_month += 12;
            }
            if (sage_month === 12) {
                sage_month = 0;
                sage = sage + 1;
            }

            $scope.empExperienceInformation.DurationYear = sage;
            $scope.empExperienceInformation.DurationMonth = sage_month;
        }
    };

    $scope.getSalutationList = function (companyGroupId) {
        $http({
            method: 'GET',
            url: 'employees/salutation/getcbo?companyGroupId=' + $scope.CompanyGroupID
        }).then(function (response) {
            $scope.salutaionList = response.data;
        });
    };

    cboService.getCboReligion(function (result) {
        $scope.religionList = result;
    });

    cboService.getCboBloodGroup(function (result) {
        $scope.bloodGroupList = result;
    });

    addressService.getCountryCbo(function (result) {
        $scope.PresCountryList = result;
        $scope.employeeInformation.PresCountryID = $scope.CompanyID;

        $scope.ParmCountryList = result;
        $scope.employeeInformation.ParmCountryID = $scope.CompanyID;
    });

    $scope.GetParmStateOnCountryChange = function (countryId) {
        addressService.getCboStateByCountry(countryId, function (result) {
            $scope.ParmStateList = result;
        });
    };

    $scope.GetPresStateOnCountryChange = function (countryId) {
        addressService.getCboStateByCountry(countryId, function (result) {
            $scope.PresStateList = result;
        });
    };

    $scope.onPreDistrictChange = function (districtId) {
        addressService.getCboCityByDistrict(districtId, function (result) {
            $scope.PresCityList = result;
        });

    };

    $scope.onParmDistrictChange = function (districtId) {
        addressService.getCboCityByDistrict(districtId, function (result) {
            $scope.ParmCityList = result;
        });
    };

    $scope.getPresDisOnPreStateChange = function (stateId) {
        addressService.getCboDistrictByState(stateId, function (result) {
            $scope.PresDistrictList = result;
        });
    };

    $scope.getParmDisOnParmStateChange = function (stateId) {
        addressService.getCboDistrictByState(stateId, function (result) {
            $scope.ParmDistrictList = result;
        });
    };

    $scope.GetParmPoliceStationCboByDistrictChange = function (districtId) {
        addressService.getPoliceStationCboByDistrictChange(districtId, function (result) {
            $scope.ParmPoliceStationList = result;
        });
    };

    $scope.GetPresPoliceStationCboByDistrictChange = function (districtId) {
        addressService.getPoliceStationCboByDistrictChange(districtId, function (result) {
            $scope.PresPoliceStationList = result;
        });
    };

    $scope.GetPresPostOfficeCboByDistrictChange = function (districtId) {
        addressService.getCboPostOfficeByDistrict(districtId, function (result) {
            $scope.PresPostOfficeList = result;
        });
    };

    $scope.GetParmPostOfficeCboByDistrictChange = function (districtId) {
        addressService.getCboPostOfficeByDistrict(districtId, function (result) {
            $scope.ParmPostOfficeList = result;
        });
    };

    addressService.getCboArea(function (result) {
        $scope.AreaList = result;
    });

    cboService.getCboQualificationLevel(function (result) {
        $scope.EductLevelSystemList = result;
    });

    cboService.getCboQualificationStream(function (result) {
        $scope.StreamList = result;
    });

    $scope.IsSameAddress = false;
    $scope.SetAddress = function () {
        if ($scope.IsSameAddress) {
            $scope.employeeInformation.ParmanentAddress1 = $scope.employeeInformation.PresentAddress1;
            $scope.employeeInformation.ParmanentAddress2 = $scope.employeeInformation.PresentAddress2;
            $scope.employeeInformation.ParmCountryID = $scope.employeeInformation.PresCountryID;
            $scope.GetParmStateOnCountryChange($scope.employeeInformation.ParmCountryID);
            $scope.employeeInformation.ParmStateId = $scope.employeeInformation.PresStateId;

            $scope.employeeInformation.ParmDistrictID = $scope.employeeInformation.PresDistrictID;
            $scope.getParmDisOnParmStateChange($scope.employeeInformation.ParmStateId);
            $scope.GetParmPostOfficeCboByDistrictChange($scope.employeeInformation.ParmDistrictID);
            $scope.employeeInformation.ParmCityID = $scope.employeeInformation.PresCityID;
            $scope.onParmDistrictChange($scope.employeeInformation.ParmDistrictID);
            $scope.GetParmPoliceStationCboByDistrictChange($scope.employeeInformation.ParmDistrictID);
            $scope.employeeInformation.ParmThanaID = $scope.employeeInformation.PresThanaID;
            $scope.employeeInformation.ParmPostOfficeID = $scope.employeeInformation.PresPostOfficeID;
            $scope.employeeInformation.ParmZipCode = $scope.employeeInformation.PresZipCode;
            $scope.employeeInformation.ParmanentArea = $scope.employeeInformation.PresentArea;
        }
        else {
            $scope.employeeInformation.ParmanentAddress1 = null;
            $scope.employeeInformation.ParmanentAddress2 = null;
            $scope.employeeInformation.ParmCountryID = null;
            $scope.employeeInformation.ParmStateId = null;
            $scope.employeeInformation.ParmDistrictID = null;
            $scope.employeeInformation.ParmCityID = null;
            $scope.employeeInformation.ParmThanaID = null;
            $scope.employeeInformation.ParmPostOfficeID = null;
            $scope.employeeInformation.ParmZipCode = null;
            $scope.employeeInformation.ParmanentArea = null;
        }
    };

    $scope.SameDOB = false;
    $scope.SetSameDOB = function () {
        if ($scope.SameDOB) {
            $scope.employeeInformation.BirthdayCelebrationDate = $scope.employeeInformation.DOB;
        } else {
            $scope.employeeInformation.BirthdayCelebrationDate = null;
        }
    };

    $scope.SetSameCDOB = function () {
        if ($scope.SameDOB) {
            $scope.employeeNew.BirthdayCelebrationDate = $scope.employeeNew.DOB;
        } else {
            $scope.employeeNew.BirthdayCelebrationDate = null;
        }
    };

    $scope.citizenList = [];
    addressService.getCountryCbo(function (result) {
        $scope.citizenList = result;
        $scope.employeeInformation.CitizenID = $rootScope.CountryId;

    });

    //#region Validation

    function CheckField(fieldValue, fieldName) {
        try {
            if (baseService.isUndefinedOrNull(fieldValue)) {
                throw '[' + fieldName + '] is required...';
            }
            //if (fieldValue === null || fieldValue === '' || fieldValue === 'undefined') {
            //    throw '[' + fieldName + '] is required...';
            //}
        } catch (e) {
            throw e;
        }
    }
    function Validation() {
        try {
            CheckField($scope.employeeInformation.Salutation, "Salutation");
            CheckField($scope.employeeInformation.FirstName, "First Name");
            // CheckField($scope.employeeInformation.CellPhnNo, "Phone");
            //CheckField($scope.employeeInformation.DOB, "Date of Birth");
            //CheckField($scope.employeeInformation.BirthdayCelebrationDate, "Birthday Celebration Date");
            //CheckField($scope.employeeInformation.NationalID, "" + $scope.Nid + "");
            //CheckField($scope.employeeInformation.IssueDate, "Issue Date");

            //if (!baseService.isUndefinedOrNull($scope.employeeInformation.CellPhnNo)) {
            //    if (isNaN($scope.employeeInformation.CellPhnNo)) {
            //        throw "Enter the valid Phone Number";
            //    }
            //}
            //if ($scope.employeeInformation.CellPhnNo.length != $rootScope.PhoneLength) {
            //    throw "Phone Number must be " + $rootScope.PhoneLength + " character.";
            //}
            if (!baseService.isUndefinedOrNull($scope.employeeInformation.NumberOfKnownPerson)) {
                if (isNaN($scope.employeeInformation.NumberOfKnownPerson)) {
                    throw "Enter the valid Number";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmailId)) {
            //    if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.employeeInformation.EmailId)) {
            //        ///
            //    } else {
            //        throw "You have entered an invalid email address.";
            //    }
            //}

            //if (new Date($scope.employeeInformation.IssueDate) < new Date($scope.employeeInformation.DOJ)) {
            //    throw "IssueDate " + $scope.employeeInformation.IssueDate + " can not greater than DOJ " + $scope.employeeInformation.DOJ + "";

            //}

            if ($scope.employeeInformation.IsKnownPerson === true && baseService.isUndefinedOrNull($scope.employeeInformation.NumberOfKnownPerson)) {
                throw "Number Of Known Person is required.";
            }
            if ($scope.employeeInformation.NumberOfKnownPerson < 0) {
                throw "Number of known person can not be less than zero";
            }
            //if ($scope.employeeInformation.NationalID.length < $scope.NidLength) {
            //    throw "" + $scope.Nid + " must be " + $scope.NidLength + " character.";
            //}


        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            CheckField($scope.employeeInformation.FatherName, "Father's Name");
            CheckField($scope.employeeInformation.CitizenID, "Citizen");
            CheckField($scope.employeeInformation.BloodGroupID, "Blood Group");
            CheckField($scope.employeeInformation.CivilStatusID, "Civil Status");
        } catch (e) {
            throw e;
        }
    }

    function ValidateQualification() {
        try {
            CheckField($scope.empAcademicQualificationInformation.EductLevelSystemID, "Level Of Education");
            CheckField($scope.empAcademicQualificationInformation.StreamId, "Stream");
            CheckField($scope.empAcademicQualificationInformation.ExamDegreeType, "Exam/Degree Title");
            CheckField($scope.empAcademicQualificationInformation.InstituteName, "Institute Name");
            CheckField($scope.empAcademicQualificationInformation.YearOfPass, "Year of Passing");
        } catch (e) {
            throw e;
        }
    }

    function ValidateTraining() {
        try {
            CheckField($scope.empTrainingInformation.TrainingTitle, "Training Title");
            CheckField($scope.empTrainingInformation.InstituteName, "Institute Name");
            CheckField($scope.empTrainingInformation.TrainingYear, "Training Year");
            CheckField($scope.empTrainingInformation.Duration, "Duration");
            CheckField($scope.empTrainingInformation.DurationUOM, "Duration UOM");
        } catch (e) {
            throw e;
        }
    }

    function ValidateExperience() {
        try {
            CheckField($scope.empExperienceInformation.Employer, "Employer");
            CheckField($scope.empExperienceInformation.Designation, "Designation");
            CheckField($scope.empExperienceInformation.StartDate, "Start Date");
            if ($scope.empExperienceInformation.IsCurrentJob === false) {
                CheckField($scope.empExperienceInformation.EndDate, "End Date");
            }
        } catch (e) {
            throw e;
        }
    }

    function validationForExperience() {
        try {
            var sDate = $filter('dateFiltering')($scope.empExperienceInformation.StartDate, 'dd-MM-yyyy');
            var eDate = $filter('dateFiltering')($scope.empExperienceInformation.EndDate, 'dd-MM-yyyy');
            if ($scope.empExperienceInformation.IsCurrentJob === false) {
                if (new Date(sDate) === new Date(eDate) || new Date(sDate) > new Date(eDate)) {
                    throw "End Date must be greater than Start Date !!!";
                }
                else {
                    //do nothing
                }
            }
        } catch (e) {
            throw e;
        }
    }

    // #endregion

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeQualificationDocument + '/' + data.FileId + extention;
    };

    $scope.ExperienceFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeExperienceDocument + '/' + data.FileId + extention;
    };

    $scope.TrainingFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeTrainingDocument + '/' + data.FileId + extention;
    };

    $scope.fileId = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });
    $("#uploadBtn2").change(function () {
        $scope.filedata = this.files[0];
    });
    $("#uploadBtn3").change(function () {
        $scope.filedata = this.files[0];
    });

    $("#uploadBtn4").change(function () {
        $scope.filedata = this.files[0];
    });
    $scope.getNum = function () {
        if ($scope.employeeInformation.IsKnownPerson)
            $scope.employeeInformation.NumberOfKnownPerson = 0;
        else
            $scope.employeeInformation.NumberOfKnownPerson = 1;
    };

    $scope.NewEmpAddValidate = function () {
        CheckField($scope.employeeNew.EmployeeCodeTypeId, "Employee Code Type");
        if ($scope.IsEmployeeCodeOpenField == true && baseService.isUndefinedOrNull($scope.employeeNew.EmployeeCode)) {
            throw "Employee Code is required.";
        }
        if ($scope.IsEmployeeCodeOpenField == true && !baseService.isUndefinedOrNull($scope.employeeNew.EmployeeCode)) {
            $scope.CheckDuplicateEmployeeCode();
        }

        CheckField($scope.employeeNew.BudgetCode, "Budget Code");
        CheckField($scope.employeeNew.Salutation, "Salutation");
        CheckField($scope.employeeNew.FirstName, "First Name");
        CheckField($scope.employeeNew.EmpType, "Emp Type");
        CheckField($scope.employeeNew.CitizenID, "Citizen");
        CheckField($scope.employeeNew.GenderID, "Gender");
        CheckField($scope.employeeNew.NationalID, $scope.Nid);
        CheckField($scope.employeeNew.PaymentMode, "Payment Mode");
        CheckField($scope.employeeNew.DOB, "Date Of Birth");
        CheckField($scope.employeeNew.BirthdayCelebrationDate, "Birthday Celebration Date");
        CheckField($scope.employeeNew.LegalDesignationId, "Legal Designation");
        CheckField($scope.employeeNew.GivenDesignation, "Given Designation");
        CheckField($scope.employeeNew.JobLocationID, "Job Location");
        CheckField($scope.employeeNew.FixSystemID, "Shift(Fix)");
        CheckField($scope.employeeNew.DOJ, "Date Of Join");
        CheckField($scope.employeeNew.DOC, "Date Of Confirmation");
        CheckField($scope.employeeNew.EmploymentType, "Employment Type");


        if (baseService.isUndefinedOrNull($scope.employeeNew.LegalDesignationId)) {
            throw "Legal Designation is required.";
        }
        if (baseService.isUndefinedOrNull($scope.employeeNew.GivenDesignationId)) {
            throw "Given Designation is required.";
        }

        if ($scope.employeeNew.IsOutSider === true && baseService.isUndefinedOrNull($scope.employeeNew.VendorId)) {
            throw "Vendor is required.";
        }

        if ($scope.IsReferenceRequired === true && baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref1Name)) {
            throw "Reference Employee is required.";
        }
        //if ($scope.IsReferenceRequired === true && baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref1CellPhnNo)) {
        //    throw "Reference Employee Cell Phone No is required.";
        //}

        if ($scope.IsResidenceGroupMandatory === true && baseService.isUndefinedOrNull($scope.employeeNew.ResidenceGroupId)) {
            throw "Residence Group is required.";
        }
        if ($scope.IsTransportGroupMandatory === true && baseService.isUndefinedOrNull($scope.employeeNew.TransportGroupId)) {
            throw "Transport Group is required.";
        }
        CheckField($scope.employeeNew.EntryLevel, "EntryLevel");
    }


    $scope.SaveNewEmployee = function () {
        try {

            $scope.NewEmpAddValidate();
            $scope.employeeNew.ExcludeOT = false;
            //$scope.$broadcast('show-errors-check-validity');
            //if ($scope.newEmpForm.$valid) { // 'WeekOff': $scope.WeekOffChild , 'OT' : $scope.NonEligibleOTChild
            $http({
                method: 'POST',
                url: $scope.saveNewUrl,
                data: { 'entity': $scope.employeeNew, 'EmployeeCodeCheckLevel': $scope.EmployeeCodeCheckLevel, 'empRef': $scope.empReferenceInformation, 'empBank': $scope.EmpBankInfoModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.GetPlantWiseHRMSSetting();
                    $scope.getData();
                    ClearEmpFields();
                  
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

            //}
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    function ClearEmpFields() {
        $scope.employeeInformation = {};
        $scope.EmployeeCodeTypeId = $scope.employeeNew.EmployeeCodeTypeId;
        $scope.employeeNew = {};
        $scope.Clean();
        $scope.employeeNew.EmployeeCodeTypeId = $scope.EmployeeCodeTypeId;
        //$scope.ShowEVendor = false;
        //$scope.ShowVendor = false;
        $scope.EmployeeCodeCheckLevel = null;
        $scope.empReferenceInformation = {
            SystemID: null,
            EmpSystemID: null,
            Ref1Name: null,
            Ref1EmployerName: null,
            Ref1EmployerAddress: null,
            Ref1Designation: null,
            Ref1CellPhnNo: null,
            Ref1TelePhnNo: null,
            Ref1Email: null,
            Ref1Address: null,
            Ref2Name: null,
            Ref2EmployerName: null,
            Ref2EmployerAddress: null,
            Ref2Designation: null,
            Ref2CellPhnNo: null,
            Ref2TelePhnNo: null,
            Ref2Email: null,
            Ref2Address: null
        };
        $scope.EmpBankInfoModel = {
            RowID: null,
            EmpSystemID: null,
            BankSystemID: null,
            BankBranchId: null,
            BankAccNo: null,
            SalaryPercentage: 0,
            IsApproved: false,
            ApprovedDateTime: 0,
            PaymentMode: null,
            IFSCCode: null,
            MICRCode: null
        }
        $scope.EmpAddInfoLabel = null;
        $scope.EmpAddInfoLabelId = null;
        GetCasteCboList();
    }

    $scope.Save = function () {
        try {
            //$scope.currentDate = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy');
            //var cd = new Date($scope.currentDate);
            //var bd = new Date($scope.employeeInformation.DOB);
            //if (bd >= cd) {
            //    throw "DOB can not equal or greater than current date.";
            //}
            Validation();


            $scope.savedisable = true;
            if ($scope.employeeInformationForm2.$valid) {
                var picData = new FormData();
                //if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'employees/employeeinformation/Edit',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("employeeInformation", angular.toJson(data.employeeInformation));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'employeeInformation': $scope.employeeInformation
                        , 'file': $scope.picdata
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.savedisable = false;
                        $scope.showdiv = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.savedisable = false;
                        $scope.showdiv = true;
                        $scope.employeeInformation.EmpPicPath = response.data.EmployeeInformation.EmpPicPath;
                        $scope.employeeInformation.IsKnownPerson = response.data.EmployeeInformation.IsKnownPerson;
                        $scope.employeeInformation.NumberOfKnownPerson = response.data.EmployeeInformation.NumberOfKnownPerson;
                        $scope.employeeInformation.NationalID = response.data.EmployeeInformation.NationalID;
                        $scope.Loaddocumentdatalist();
                    }
                }, function errorCallback(response) {
                    $scope.savedisable = false;
                    $scope.showdiv = false;
                });
                return true;
                //}
            }
        } catch (e) {
            $scope.savedisable = false;
            $scope.showdiv = false;
            ShowResult(e, "failure");
        }
    };

    $scope.SavePersonal = function () {
        try {
            ValidationMaster();
            if ($scope.employeeInformation.FatherName === $scope.employeeInformation.FullName) {
                throw "Your name and your father name can not be same.";
            }

            //if ($scope.SalaryRangeForTaxRequired) {
            //    if ($scope.TotalSalary > $scope.SalaryRangeForTax && baseService.isUndefinedOrNull($scope.employeeInformation.TIN)) {
            //        throw "" + $scope.Tin + " No is required as per company rule.";
            //    }
            //}

            //if (!baseService.isUndefinedOrNull($scope.employeeInformation.TIN)) {
            //    if ($scope.employeeInformation.TIN.length != $scope.TinLength) {
            //        throw "" + $scope.Tin + " must be " + $scope.TinLength + " character.";
            //    }
            //}

            $scope.celebrationType = $.grep($scope.civilStatusList, function (item) {
                return item.Value === $scope.employeeInformation.CivilStatusID;
            })[0].HasPartner;

            if ($scope.celebrationType) {
                //if (baseService.isUndefinedOrNull($scope.employeeInformation.MarriagedayCelebrationDate)) {
                //    throw "Marriage day Celebration Date is required."
                //}
                if (new Date() < new Date($scope.employeeInformation.MarriagedayCelebrationDate)) {
                    throw "Marriage day Celebration Date is can not be greater than todays date.";
                }
                //if (baseService.isUndefinedOrNull($scope.employeeInformation.SpouseName)) {
                //    throw "Spouse Name is required.";
                //}
            }

            //if (!baseService.isUndefinedOrNull($scope.employeeInformation.SpouseNationalID)) {
            //    if ($scope.employeeInformation.SpouseNationalID.length != $scope.NidLength) {
            //        throw "Spouse " + $scope.Nid + " must be " + $scope.NidLength + " character.";
            //    }
            //}

            $scope.savedisable = true;

            $http({
                method: 'POST',
                url: $scope.updateUrl = $scope.path + 'createpersonal',
                data: $scope.employeeInformation,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
                //ShowResult(response.status.Message, "failure");
            });
            return true;


        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.SaveAddress = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.employeeInformation.PresentAddress1)) {
                throw "Present Address 1 is required";
            }
            if (baseService.isUndefinedOrNull($scope.employeeInformation.ParmanentAddress1)) {
                throw "Permanent Address 1 is required";
            }
            if (baseService.isUndefinedOrNull($scope.employeeInformation.PresStateId)) {
                throw "Present Address State is required";
            }
            if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmrCntPer1CellNo)) {
                if (isNaN($scope.employeeInformation.EmrCntPer1CellNo)) {
                    throw "Enter the valid Cell Number in Cell No1";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmrCntPer1CellNo)) {
            //    if ($scope.employeeInformation.EmrCntPer1CellNo.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmrCntPer1CellNo2)) {
                if (isNaN($scope.employeeInformation.EmrCntPer1CellNo2)) {
                    throw "Enter the valid Cell Number in Cell No2";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmrCntPer1CellNo2)) {
            //    if ($scope.employeeInformation.EmrCntPer1CellNo2.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}

            if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmrCntPer2CellNo)) {
                if (isNaN($scope.employeeInformation.EmrCntPer2CellNo)) {
                    throw "Enter the valid Cell Number in Cell No1";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmrCntPer2CellNo)) {
            //    if ($scope.employeeInformation.EmrCntPer2CellNo.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmrCntPer2CellNo2)) {
                if (isNaN($scope.employeeInformation.EmrCntPer2CellNo2)) {
                    throw "Enter the valid Cell Number in Cell No2";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmrCntPer2CellNo2)) {
            //    if ($scope.employeeInformation.EmrCntPer2CellNo2.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}

            $scope.savedisable = true;
            //if ($scope.employeeInformationForm1.$valid) {
            //if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: $scope.saveUrl = $scope.path + 'createaddress',
                data: $scope.employeeInformation,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                    $scope.savedisable = false;
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.savedisable = false;
                }
            }, function errorCallback(response) {
                //ShowResult(response.status.Message, "failure");
            });
            return true;
            //}
            //}
        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.SaveEmployment = function () {
        try {
            if ($scope.employeeInformation.IsOutSider === true && baseService.isUndefinedOrNull($scope.employeeInformation.VendorId)) {
                throw "Vendor is required.";
            }
            $scope.savedisable = true;

            //if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: $scope.saveUrl = $scope.path + 'createemployment',
                data: $scope.employeeInformation,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                    $scope.savedisable = false;
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.savedisable = false;
                }
            }, function errorCallback(response) {
                //ShowResult(response.status.Message, "failure");
            });
            return true;
            //}

        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    // #region SaveReference

    $scope.SaveReference = function () {
        try {
            if ($scope.IsReferenceRequired === true && baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref1Name)) {
                throw "Reference Employee is required.";
            }
            if ($scope.IsReferenceRequired === true && baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref1CellPhnNo)) {
                throw "Reference Employee cell is required.";
            }
            if (!baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref1CellPhnNo)) {
                if (isNaN($scope.empReferenceInformation.Ref1CellPhnNo)) {
                    throw "Enter the valid Cell Number";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref1CellPhnNo)) {
            //    if ($scope.empReferenceInformation.Ref1CellPhnNo.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (!baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref2CellPhnNo)) {
                if (isNaN($scope.empReferenceInformation.Ref2CellPhnNo)) {
                    throw "Enter the valid Cell Number";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref2CellPhnNo)) {
            //    if ($scope.empReferenceInformation.Ref2CellPhnNo.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (!baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref1TelePhnNo)) {
                if (isNaN($scope.empReferenceInformation.Ref1TelePhnNo)) {
                    throw "Enter the valid Tele Phone Number";
                }
            }
            if (!baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref2TelePhnNo)) {
                if (isNaN($scope.empReferenceInformation.Ref2TelePhnNo)) {
                    throw "Enter the valid Tele Phone Number";
                }
            }
            if (!baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref1Email)) {
                if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.empReferenceInformation.Ref1Email)) {
                    // allow
                } else {
                    throw "You have entered an invalid email address.";
                }
            }
            if (!baseService.isUndefinedOrNull($scope.empReferenceInformation.Ref2Email)) {
                if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.empReferenceInformation.Ref2Email)) {
                    // allow
                } else {
                    throw "You have entered an invalid email address.";
                }
            }
            $scope.savedisable = true;
            $scope.empReferenceInformation.EmpSystemID = $scope.user;
            $scope.empReferenceInformation.AddedBy = $scope.employeeInformation.FirstName;
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createreference',
                data: $scope.empReferenceInformation,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                    $scope.savedisable = false;
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.savedisable = false;
                    $scope.LoadReferenceData($scope.user);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    // #endregion

    // #region SaveQualification

    $scope.SaveQualification = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb.';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.empAcademicQualificationInformation.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.empAcademicQualificationInformation.FileName)) {
                if ($scope.empAcademicQualificationInformation.FileName.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            $scope.empAcademicQualificationInformation.FileId = $scope.fileId();

            ValidateQualification();
            $scope.savedisable = true;
            $scope.empAcademicQualificationInformation.EmpSystemID = $scope.user;
            $scope.empAcademicQualificationInformation.AddedBy = $scope.employeeInformation.FirstName;
            $scope.btnDisable = true;

            var formData = new FormData();

            //if ($scope.empAcademicQualificationInformations.length <= 0 && baseService.isUndefinedOrNull($scope.empAcademicQualificationInformation.FileName)) {
            //    throw "Attachment is mandatory.";
            //}

            //if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createqualification',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("empAcademicQualificationInformation", angular.toJson(data.empAcademicQualificationInformation));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'empAcademicQualificationInformation': $scope.empAcademicQualificationInformation, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "failure", "QualificationPopUp");
                }
                else {
                    ShowResult(response.data.Message, "success", "QualificationPopUp");

                    $scope.btnDisable = false;
                    $scope.LoadQualificationData(response.data.EmpAcademicQualificationInformation.EmpSystemID);
                    $scope.Clear();
                    $scope.savedisable = false;
                    ClearFile();
                    $scope.empAcademicQualificationInformation.SystemID = null;

                    addressService.getCountryCbo(function (result) {
                        $scope.CountryList = result;
                        $scope.empAcademicQualificationInformation.CountryId = $scope.CountryId;
                    });

                    $scope.empAcademicQualificationInformation = {};
                    $scope.imageSrc = virtualPath.EmployeePic + $rootScope.img;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", "QualificationPopUp");
                $scope.savedisable = false;
            });
            return true;
            //}
        } catch (e) {
            $scope.savedisable = false;
            $scope.btnDisable = false;
            ShowResult(e, "failure", "QualificationPopUp");
        }
    };

    // #endregion

    // #region SaveTraining

    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };
    document.getElementById("uploadBtn2").onchange = function () {
        var filename = document.getElementById("uploadFile2").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile2").value = res;
    };
    document.getElementById("uploadBtn3").onchange = function () {
        var filename = document.getElementById("uploadFile3").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile3").value = res;
    };

    $scope.SaveTraining = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.empTrainingInformation.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.empTrainingInformation.FileName)) {
                if ($scope.empTrainingInformation.FileName.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            $scope.empTrainingInformation.FileId = $scope.fileId();

            ValidateTraining();
            $scope.savedisable = true;
            $scope.empTrainingInformation.EmpSystemID = $scope.user;
            $scope.empTrainingInformation.AddedBy = $scope.employeeInformation.FirstName;
            $scope.btnDisable = true;
            var formData = new FormData();
            //if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createtraining',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("empTrainingInformation", angular.toJson(data.empTrainingInformation));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'empTrainingInformation': $scope.empTrainingInformation, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "failure", "TrainingPopUp");
                }
                else {
                    ShowResult(response.data.Message, "success", "TrainingPopUp");
                    $scope.btnDisable = false;
                    $scope.LoadTrainingData();
                    $scope.Clear();
                    $scope.savedisable = false;
                    ClearFile();
                    $scope.empTrainingInformation.SystemID = null;

                    addressService.getCountryCbo(function (result) {
                        $scope.CountryList = result;
                        $scope.empTrainingInformation.CountryId = $scope.CountryId;
                    });

                    $scope.empTrainingInformation = {};
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", "TrainingPopUp");
                $scope.savedisable = false;
            });
            return true;
            //}
            // }
        } catch (e) {
            $scope.savedisable = false;
            $scope.btnDisable = false;
            ShowResult(e, "failure", "TrainingPopUp");
        }
    };

    // #endregion

    // #region SaveExperience

    $scope.SaveExperience = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.empExperienceInformation.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.empExperienceInformation.FileName)) {
                if ($scope.empExperienceInformation.FileName.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            $scope.empExperienceInformation.FileId = $scope.fileId();

            ValidateExperience();
            $scope.savedisable = true;
            $scope.empExperienceInformation.EmpSystemID = $scope.user;
            $scope.empExperienceInformation.AddedBy = $scope.employeeInformation.FirstName;
            validationForExperience();
            $scope.btnDisable = true;
            var formData = new FormData();

            //if ($scope.empExperienceInformations.length <= 0 && baseService.isUndefinedOrNull($scope.empExperienceInformation.FileName)) {
            //    throw "Attachment is mandatory.";
            //}

            //if ($scope.Action === "Save") {
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createexperience',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("empExperienceInformation", angular.toJson(data.empExperienceInformation));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'empExperienceInformation': $scope.empExperienceInformation, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    ShowResult(response.data.Message, "failure", "ExperiencePopUp");
                    $scope.savedisable = false;
                }
                else {
                    ShowResult(response.data.Message, "success", "ExperiencePopUp");
                    $scope.btnDisable = false;
                    $scope.LoadExperienceData();
                    $scope.Clear();
                    $scope.savedisable = false;
                    ClearFile();
                    $scope.empExperienceInformation.SystemID = null;
                    $scope.empExperienceInformation = {};
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", "ExperiencePopUp");
                $scope.savedisable = false;
            });
            //angular.element(document.querySelector('#ExperiencePopUp')).modal('hide');
            return true;
            //}
            // }
        } catch (e) {
            $scope.btnDisable = false;
            $scope.savedisable = false;
            ShowResult(e, "failure", "ExperiencePopUp");
        }
    };

    // #endregion

    $scope.SaveAdvanceInfo = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.employeeInformation.SystemId)) {
                throw "Please select  an employee.";
            }
            $scope.savedisable = true;
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createadvanceinfo',
                data: $scope.employeeInformation,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
            });
            return true;

        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.SaveLocalInfo = function () {
        try {
            $scope.savedisable = true;
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createlocalinfo',
                data: $scope.employeeInformation,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
            });
            return true;

        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.SaveRelativeInfo = function () {
        try {
            $scope.savedisable = true;
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createRelativeinfo',
                data: $scope.employeeInformation,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
            });
            return true;

        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.employeeNomineeList = [];
    $scope.getEmployeeNomineeInfo = function () {
        $scope.employeeNomineeList = [];
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/GetEmployeeNomineeInfo?empId=' + $scope.user
        }).then(function successCallback(response) {
            $scope.employeeNomineeList = response.data;
        });
    }

    $scope.dependantList = [];
    $scope.getEmployeeDependantInfo = function () {
        $scope.dependantList = [];
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/GetEmployeeDependantInfo?empId=' + $scope.user
        }).then(function successCallback(response) {
            $scope.dependantList = response.data;
        });
    }

    $scope.LandLordList = [];
    $scope.getEmployeeLandLordInfo = function () {
        $scope.LandLordList = [];
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/GetEmployeeLandLoardInfo?empId=' + $scope.user
        }).then(function successCallback(response) {
            $scope.LandLordList = response.data;
        });
    }

    $scope.indexNominee = -1;
    $scope.NomineeData = function (data, index) {
        $scope.indexNominee = index;
        $scope.nomineeInfo = data;
    };

    $scope.indexLandLoard = -1;
    $scope.DependantData = function (data, index) {
        $scope.indexDependant = index;
        $scope.dependantInFo = data;
    };

    $scope.indexLandLoard = -1;
    $scope.LandLordData = function (data, index) {
        $scope.indexLandLoard = index;
        $scope.LandLordInfo = data;
    };

    $scope.SaveNomineeInfo = function () {
        try {
            $scope.savedisable = true;
            $scope.nomineeInfo.EmpSystemId = $scope.user;
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createnomineeinfo',
                data: $scope.nomineeInfo,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "success");
                    $scope.getEmployeeNomineeInfo();
                    $scope.ClearNominee();
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
            });
            return true;

        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.dependantInFo = {
        Id: null,
        EmpSystemId: null,
        Name: null,
        LocalName: null,
        RelationId: null,
        DOB: null,
        ProfessionId: null,
        Remarks: null
    }

    $scope.LandLordInfo = {
        Id: null,
        EmpSystemId: null,
        Name: null,
        LocalName: null,
        CellNo: null,
        CellNoLocal: null,
        Address: null,
        AddressLocal: null,
    }

    $scope.SaveDependantInFo = function () {
        try {
            $scope.savedisable = true;
            $scope.dependantInFo.EmpSystemId = $scope.user;
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/CreateDependantInfo',
                data: $scope.dependantInFo,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "success");
                    $scope.getEmployeeDependantInfo();
                    $scope.ClearDependant();
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
            });
            return true;

        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.SaveLandLordInFo = function () {
        try {
            $scope.savedisable = true;
            $scope.LandLordInfo.EmpSystemId = $scope.user;
            if (baseService.isUndefinedOrNull($scope.LandLordInfo.Name)) {
                throw "Name is required.";
            }
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/CreateLandLordInfo',
                data: $scope.LandLordInfo,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "success");
                    $scope.getEmployeeLandLordInfo();
                    $scope.ClearLandLoard();
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
            });
            return true;

        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.ClearNominee = function () {
        $scope.nomineeInfo = {};
    }

    $scope.ClearDependant = function () {
        $scope.dependantInFo = {};
    }

    $scope.ClearLandLoard = function () {
        $scope.LandLordInfo = {};
    }

    $scope.confirmNomineeDelete = function (data) {
        $scope.deleteTrainingId = data.Id;
        $scope.Name = data.Name;
        $scope.message_confirmation = "Are you sure to delete parmanently [" + $scope.Name + "]? ";
    };

    $scope.confirmDependantDelete = function (data) {
        $scope.deleteTrainingId = data.Id;
        $scope.Name = data.Name;
        $scope.message_confirmation = "Are you sure to delete parmanently [" + $scope.Name + "]? ";
    };

    $scope.confirmLandLordDelete = function (data) {
        $scope.deleteTrainingId = data.Id;
        $scope.Name = data.Name;
        $scope.message_confirmation = "Are you sure to delete parmanently [" + $scope.Name + "]? ";
    };

    $scope.removeNominee = function () {
        $http({
            method: 'POST',
            url: 'employees/employeeinformation/deleteNominee',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteTrainingId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getEmployeeNomineeInfo();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.removeDependant = function () {
        $http({
            method: 'POST',
            url: 'employees/employeeinformation/DeleteDependant',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteTrainingId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getEmployeeDependantInfo();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.removeLandLord = function () {
        $http({
            method: 'POST',
            url: 'employees/employeeinformation/DeleteLandLoard',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteTrainingId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getEmployeeLandLordInfo();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.setHeight = function (id) {
        var element = angular.element(document.getElementById(id));
        $scope.height = element[0].scrollHeight;
    };

    $scope.yearList = [];
    $scope.SetYearOfPassing = function () {
        var endYear = new Date();
        var ey = parseInt(endYear.getFullYear());
        for (var i = ey; i > 1945 - 1; i--) {
            var ob = {
                Value: i,
                Text: i
            };
            $scope.yearList.push(ob);
        }
    };

    $scope.indexQua = -1;
    $scope.QualificationData = function (data, index) {
        $scope.filedata = {};
        $scope.empAcademicQualificationInformation = Object.assign({}, data);
        $scope.filedata.name = data.FileName;
        $scope.empAcademicQualificationInformation.FileName = data.FileName;
        var filename = document.getElementById("uploadFile").value = data.FileName;
        if (!baseService.isUndefinedOrNull(data.YearOfPass)) {
            $scope.empAcademicQualificationInformation.YearOfPass = data.YearOfPass.toString();
        }
        $scope.SetYearOfPassing();
        $scope.indexQua = index;
        angular.element(document.querySelector('#QualificationPopUp')).modal('show');
    };

    $scope.indexTrn = -1;
    $scope.TrainingData = function (data, index) {
        $scope.filedata = {};
        $scope.empTrainingInformation = Object.assign({}, data);
        $scope.filedata.name = data.FileName;
        $scope.empTrainingInformation.FileName = data.FileName;
        var filename = document.getElementById("uploadFile2").value = data.FileName;
        $scope.indexTrn = index;
        angular.element(document.querySelector('#TrainingPopUp')).modal('show');
    };

    $scope.indexExp = -1;
    $scope.ExperienceData = function (data, index) {
        $scope.filedata = {};
        $scope.empExperienceInformation = Object.assign({}, data);
        $scope.filedata.name = data.FileName;
        $scope.empExperienceInformation.FileName = data.FileName;
        var filename = document.getElementById("uploadFile3").value = data.FileName;
        $scope.indexExp = index;
        angular.element(document.querySelector('#ExperiencePopUp')).modal('show');
    };

    addressService.getCountryCbo(function (result) {
        $scope.CountryList = result;
    });

    $scope.qualificationShow = function () {
        $scope.empAcademicQualificationInformation = {};
        $scope.SetYearOfPassing();

        addressService.getCountryCbo(function (result) {
            $scope.CountryList = result;
            $scope.empAcademicQualificationInformation.CountryId = $scope.CountryId;
        });

        $scope.Clear();
        angular.element(document.querySelector('#QualificationPopUp')).modal('show');
    };

    $scope.TrainingShow = function () {
        $scope.empTrainingInformation = {};

        addressService.getCountryCbo(function (result) {
            $scope.CountryList = result;
            $scope.empTrainingInformation.CountrySystemID = $scope.CountryId;
        });

        $scope.Clear();
        angular.element(document.querySelector('#TrainingPopUp')).modal('show');
    };

    $scope.ExperienceShow = function () {
        $scope.Clear();
        $scope.empExperienceInformation = {};
        $scope.empExperienceInformation.EndDate = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy');
        $scope.countDate();
        angular.element(document.querySelector('#ExperiencePopUp')).modal('show');
    };

    $scope.confirmQualificationDelete = function (Id) {
        $scope.deleteQualificationId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + Id + "]? ";
    };

    $scope.DeleteQualification = function () {
        $http({
            method: 'POST',
            url: 'employees/employeeinformation/deletequalification',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteQualificationId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadQualificationData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.QualificationRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmQualiDelete')).modal('show');
    };

    $scope.removeQualification = function () {
        angular.element(document.querySelector('#confirmQualiDelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.empAcademicQualificationInformation.SystemID)) {
            document.getElementById('uploadBtn').value = '';
            $scope.filedata = '';
            $scope.empAcademicQualificationInformation.FileName = "";
            $scope.filedata = {};
            document.getElementById('uploadFile').value = "";
        }
        else {
            $scope.ClearQualification();
        }
    };

    $scope.confirmCloseQualificationDelete = function () {
        angular.element(document.querySelector('#confirmQualiDelete')).modal('hide');
    };

    $scope.SaveQualific = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.empAcademicQualificationInformation.FileName = fileName;
            $scope.empAcademicQualificationInformation.FileId = $scope.fileId();

            ValidateQualification();
            $scope.savedisable = true;
            $scope.empAcademicQualificationInformation.EmpSystemID = $scope.user;
            $scope.empAcademicQualificationInformation.AddedBy = $scope.employeeInformation.FirstName;
            $scope.btnDisable = true;

            var formData = new FormData();

            //if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createqualification',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("empAcademicQualificationInformation", angular.toJson(data.empAcademicQualificationInformation));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'empAcademicQualificationInformation': $scope.empAcademicQualificationInformation, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "failure", "QualificationPopUp");
                }
                else {
                    ShowResult(response.data.Message, "success", "QualificationPopUp");
                    $scope.btnDisable = false;
                    $scope.LoadQualificationData();
                    $scope.savedisable = false;
                    $scope.empAcademicQualificationInformation = {};
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", "QualificationPopUp");
            });
            return true;
            //}
            // }
        } catch (e) {
            $scope.savedisable = false;
            $scope.btnDisable = false;
            ShowResult(e, "failure", "QualificationPopUp");
        }
    };

    $scope.ClearQualification = function () {
        document.getElementById('uploadBtn').value = '';
        $scope.filedata = '';
        $scope.empAcademicQualificationInformation.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
        $scope.SaveQualific();
    };

    $scope.confirmTrainingDelete = function (Id) {
        $scope.deleteTrainingId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + Id + "]? ";
    };

    $scope.DeleteTraining = function () {
        $http({
            method: 'POST',
            url: 'employees/employeeinformation/deletetraining',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteTrainingId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadTrainingData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.TrainingRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmTrainDelete')).modal('show');
    };

    $scope.removeTraining = function () {
        angular.element(document.querySelector('#confirmTrainDelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.empTrainingInformation.SystemID)) {
            document.getElementById('uploadBtn2').value = '';
            $scope.filedata = '';
            $scope.empTrainingInformation.FileName = "";
            $scope.filedata = {};
            document.getElementById('uploadFile2').value = "";
        }
        else {
            $scope.ClearTraining();
        }
    };

    $scope.confirmCloseTrainingDelete = function () {
        angular.element(document.querySelector('#confirmTrainDelete')).modal('hide');
    };

    $scope.SaveTrain = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.empTrainingInformation.FileName = fileName;
            $scope.empTrainingInformation.FileId = $scope.fileId();

            ValidateTraining();
            $scope.savedisable = true;
            $scope.empTrainingInformation.EmpSystemID = $scope.user;
            $scope.empTrainingInformation.AddedBy = $scope.employeeInformation.FirstName;
            $scope.btnDisable = true;
            var formData = new FormData();
            //if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createtraining',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("empTrainingInformation", angular.toJson(data.empTrainingInformation));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'empTrainingInformation': $scope.empTrainingInformation, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "failure", "TrainingPopUp");
                }
                else {
                    ShowResult(response.data.Message, "success", "TrainingPopUp");
                    $scope.btnDisable = false;
                    $scope.LoadTrainingData();
                    $scope.savedisable = false;
                    $scope.empTrainingInformation = {};
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", "TrainingPopUp");
            });
            return true;
            //}
            // }
        } catch (e) {
            $scope.savedisable = false;
            $scope.btnDisable = false;
            ShowResult(e, "failure", "TrainingPopUp");
        }
    };

    $scope.ClearTraining = function () {
        document.getElementById('uploadBtn2').value = '';
        $scope.filedata = '';
        $scope.empTrainingInformation.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile2').value = "";
        $scope.SaveTrain();
    };

    $scope.confirmExperienceDelete = function (Id) {
        $scope.deleteExperienceId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + Id + "]? ";
    };

    $scope.DeleteExperience = function () {
        $http({
            method: 'POST',
            url: 'employees/employeeinformation/deleteexperience',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteExperienceId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadExperienceData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.ExperienceRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmExpDelete')).modal('show');
    };

    $scope.removeExperience = function () {
        angular.element(document.querySelector('#confirmExpDelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.empExperienceInformation.SystemID)) {
            document.getElementById('uploadBtn3').value = '';
            $scope.filedata = '';
            $scope.empExperienceInformation.FileName = "";
            $scope.filedata = {};
            document.getElementById('uploadFile3').value = "";
        }
        else {
            $scope.ClearExperience();
        }
    };

    $scope.confirmCloseExperienceDelete = function () {
        angular.element(document.querySelector('#confirmExpDelete')).modal('hide');
    };

    $scope.SaveExp = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.empExperienceInformation.FileName = fileName;
            $scope.empExperienceInformation.FileId = $scope.fileId();

            ValidateExperience();
            $scope.savedisable = true;
            $scope.empExperienceInformation.EmpSystemID = $scope.user;
            $scope.empExperienceInformation.AddedBy = $scope.employeeInformation.FirstName;
            validationForExperience();
            $scope.btnDisable = true;
            var formData = new FormData();
            //if ($scope.Action === "Save") {
            $http({
                method: 'POST',
                url: 'employees/employeeinformation/createexperience',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("empExperienceInformation", angular.toJson(data.empExperienceInformation));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'empExperienceInformation': $scope.empExperienceInformation, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    ShowResult(response.data.Message, "failure", "ExperiencePopUp");
                    $scope.savedisable = false;
                }
                else {
                    ShowResult(response.data.Message, "success", "ExperiencePopUp");
                    $scope.btnDisable = false;
                    $scope.LoadExperienceData();
                    $scope.savedisable = false;
                    $scope.empExperienceInformation = {};
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", "ExperiencePopUp");
            });
            //angular.element(document.querySelector('#ExperiencePopUp')).modal('hide');
            return true;
            //}
            // }
        } catch (e) {
            $scope.btnDisable = false;
            $scope.savedisable = false;
            ShowResult(e, "failure", "ExperiencePopUp");
        }
    };

    $scope.ClearExperience = function () {
        document.getElementById('uploadBtn3').value = '';
        $scope.filedata = '';
        $scope.empAcademicQualificationInformation.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile3').value = "";
        $scope.SaveExp();
    };

    $scope.Clear = function () {
        ClearFields();
        ClearFile();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.empAcademicQualificationInformation = {};
        $scope.empTrainingInformation = {};
        $scope.empExperienceInformation = {};
    }

    function ClearFile() {
        document.getElementById('uploadBtn').value = '';
        document.getElementById('uploadBtn2').value = '';
        document.getElementById('uploadBtn3').value = '';
        $scope.filedata = '';
        $scope.empAcademicQualificationInformation.FileName = "";
        $scope.empTrainingInformation.FileName = "";
        $scope.empExperienceInformation.FileName = "";
        document.getElementById('uploadFile').value = "";
        document.getElementById('uploadFile2').value = "";
        document.getElementById('uploadFile3').value = "";
        $scope.filedata = {};
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.clearImage = function () {
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    };
    $scope.FileNam = null;
    $scope.tempdata = {};
    $scope.DocDownload = function (data) {
        $scope.tempdata = data;
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        $scope.FileNam = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeDocument + '/' + data.FileId + extention;
        angular.element(document.querySelector('#DocShowPopUp')).modal('show');
    };

    $scope.DownloadImageFile = function () {
        var str = $scope.tempdata.FileName;
        $scope.FileNam = $scope.tempdata.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.EmployeeDocument + '/' + $scope.tempdata.FileId + extention;
    };

    // #region Document

    $scope.Loaddocumentdatalist = function () {
        $http.get('employees/employeeinformation/GetEmpAllDocumentDataList?companyGroupId=' + $scope.CompanyGroupID + '&pId=' + $scope.user + '&plantId=' + $scope.PlantId)
            .then(function (response) {
                $scope.documentdataList = response.data;
                //$scope.getColor($scope.documentdataList.FileName);
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

    // #endregion

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

    $scope.confirmCloseDocDelete = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
    };
    $scope.confirmSubmit = function () {
        $scope.confirm = $scope.user;
        //$scope.message_confirmation = "Are you sure to Submit? <br/> You won't be able to change any information after submission.";
        $scope.message_confirmation = "Are you sure you want to submit? You won’t be able to modify your data after this.";
        angular.element(document.querySelector('#confirmSubmit')).modal('show');
    };

    function fileValidation() {
        for (var i = 0; i < $scope.documentdataList.length; i++) {
            if ($scope.documentdataList[i].OptionalOrMandatory === 'Mandatory' && baseService.isUndefinedOrNull($scope.documentdataList[i].FileName)) {
                throw "File is 	Mandatory for " + $scope.documentdataList[i].DocumentName + ".";
            }
        }
    }

    function QualyfileValidation() {
        for (var i = 0; i < $scope.empAcademicQualificationInformations.length; i++) {
            if (i === 0) {
                if (baseService.isUndefinedOrNull($scope.empAcademicQualificationInformations[i].FileName)) {
                    throw "File is 	Mandatory qualification tab for Education Level " + $scope.empAcademicQualificationInformations[i].EducationLevel + ".";
                }
            } else {
                break;
            }
        }
    }

    function TrainfileValidation() {
        for (var i = 0; i < $scope.empTrainingInformations.length; i++) {
            if (baseService.isUndefinedOrNull($scope.empTrainingInformations[i].FileName)) {
                throw "File is 	Mandatory for Training Title " + $scope.empTrainingInformations[i].TrainingTitle + ".";
            }
        }
    }

    function ExpfileValidation() {
        for (var i = 0; i < $scope.empExperienceInformations.length; i++) {
            if (i === 0) {
                if (baseService.isUndefinedOrNull($scope.empExperienceInformations[i].FileName)) {
                    throw "File is 	Mandatory in experience tab for Employer " + $scope.empExperienceInformations[i].Employer + ".";
                }
            } else {
                break;
            }
        }
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.employee = {};
        return true;
    };

    $scope.AnyRelativeWorkedHere = false;

    $scope.ChkClick = function () {
        if ($scope.employeeInformation.AnyRelativeWorkedHere)
            $scope.AnyRelativeWorkedHere = true;

        else
            $scope.AnyRelativeWorkedHere = false;

    };

    //# region Multiple Operation   
    $scope.EmployeeOperationList = [];

    //$scope.refreshTemplate = function (args) {
    //    $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllOperationWise });
    //};

    //function CheckBoxSelectAllOperationWise(e) {
    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;
    //    }
    //    var filtered = $("#GridMultioperation").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.OperationList.length; i++) {
    //            $scope.OperationList[i].check = ChkOrUnchk;
    //        }
    //    }
    //    else {
    //        for (var j = 0; j < filtered.length; j++) {
    //            filtered[j].check = ChkOrUnchk;
    //        }
    //    }
    //    var gridObj = $("#GridMultioperation").data("ejGrid");
    //    gridObj.refreshContent();
    //};

    function MakeOperationData() {
        for (var i = 0; i < $scope.OperationList.length; i++) {
            if ($scope.OperationList[i].check == true) {
                if (checkExists($scope.EmployeeOperationList, $scope.OperationList[i].Id) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.EmpSystemId = $scope.employeeInformation.SystemId;
                    if ($scope.Operation == "Operation Master") {
                        ob.OperationMasterId = $scope.OperationList[i].Id;
                        ob.OperationVariationId = null;
                    }
                    else {
                        ob.OperationMasterId = null;
                        ob.OperationVariationId = $scope.OperationList[i].Id;
                    }

                    ob.Code = $scope.OperationList[i].Code;
                    ob.ShortName = $scope.OperationList[i].ShortName;
                    ob.StandardName = $scope.OperationList[i].StandardName;
                    ob.UserName = $scope.OperationList[i].UserName;
                    ob.MachineMaster = $scope.OperationList[i].MachineMaster;
                    ob.Skill = $scope.OperationList[i].Skill;
                    ob.CycleTime = $scope.OperationList[i].CycleTime;
                    $scope.EmployeeOperationList.push(ob);
                }
            }
        }
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if ($scope.Operation == "Operation Master") {
                if (list[i].OperationMasterId === id) {
                    return true;
                }
            }
            else {
                if (list[i].OperationVariationId === id) {
                    return true;
                }
            }
        }
        return false;
    }

    $scope.CloseOperation = function () {
        MakeOperationData();

        $scope.SaveOperation();
        angular.element(document.querySelector('#MultiOperationPopUp')).modal('hide');
    }

    function CheckSequence() {
        var arr = [];
        for (var i = 0; i < $scope.EmployeeOperationList.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.EmployeeOperationList[i].Sequence)) {
                if (checkExistsSS(arr, $scope.EmployeeOperationList[i].Sequence) === false) {
                    arr.push($scope.EmployeeOperationList[i].Sequence);
                }
                else {
                    throw "Sequence :" + $scope.EmployeeOperationList[i].Sequence + " is exists for Code :" + $scope.EmployeeOperationList[i].Code + ".";
                }
            }
        }
    }

    function checkExistsSS(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i] === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveOperation = function () {
        try {
            if (baseService.arrayLength($scope.EmployeeOperationList) < 0) {
                throw "Select Opearation.";
            }
            CheckSequence();
            $http({
                method: 'POST',
                url: 'Employees/EmployeeInformation/SaveOperation',
                data: { 'data': $scope.EmployeeOperationList, 'EmpSystemId': $scope.employeeInformation.SystemId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.getSavedOperationData($scope.employeeInformation.SystemId);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getSavedOperationData = function (empsystemId) {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/getsavedoperationdata?empsystemId=' + empsystemId
        }).then(function successCallback(response) {
            $scope.EmployeeOperationList = response.data;
        });
    }

    $scope.OperationModel = {};

    $scope.removeRowModal = function (args) {
        try {
            $scope.OperationModel = args;
            $scope.Id = $scope.OperationModel.Id;
            if (baseService.isUndefinedOrNull($scope.Id))
                $scope.message = 'Are you sure want to delete this data....';
            else
                $scope.message = 'Are you sure want to delete permanently [ ' + $scope.OperationModel.UserName + ' ]';
            angular.element(document.querySelector('#removerPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteOperation = function () {
        $http({
            method: 'POST',
            url: 'Employees/EmployeeInformation/DeleteOperation?id=' + $scope.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getSavedOperationData($scope.employeeInformation.SystemId);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };
    //# endregion

    $scope.Clean = function () {
        $scope.OTEntitledmsg = null;
        $scope.approved = "";
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
            RegisterFP: false,
            RegisterProximate: false,
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
            SpecialReviewAmount: 0,
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
            EmpCodeType: null,
            EntryLevel: null
        };
        $scope.employeeNew = Object.assign({}, $scope.model);
        $scope.employeeInformation = Object.assign({}, $scope.model);
        $scope.employeeNew.EmployeeCodeTypeId = $scope.EmployeeCodeTypeId;
        $scope.GetPlantWiseHRMSSetting();
        //$scope.ShowVendor = false;
        //$scope.ShowEVendor = false;
        $scope.EmployeeOperationList = [];
        $scope.empAcademicQualificationInformations = [];
        $scope.empExperienceInformations = [];
        $scope.empTrainingInformations = [];
        $scope.employeeNomineeList = [];
        $scope.dependantList = [];
        $scope.LandLordList = [];
        $scope.documentdataList = [];
        $scope.imageSrc = virtualPath.EmployeePic + '';
        $scope.EmpSignature = virtualPath.CardHolderSignature + '';
        $scope.empReferenceInformation = {
            SystemID: null,
            EmpSystemID: null,
            Ref1Name: null,
            Ref1EmployerName: null,
            Ref1EmployerAddress: null,
            Ref1Designation: null,
            Ref1CellPhnNo: null,
            Ref1TelePhnNo: null,
            Ref1Email: null,
            Ref1Address: null,
            Ref2Name: null,
            Ref2EmployerName: null,
            Ref2EmployerAddress: null,
            Ref2Designation: null,
            Ref2CellPhnNo: null,
            Ref2TelePhnNo: null,
            Ref2Email: null,
            Ref2Address: null
        };
        $scope.IsOTEntitled = false;
        $scope.employeeInformation.isLeaveOnDOC = false;
        $scope.employeeInformation.isLeaveOnDOJ = true;
        //GetEmpCodeGenSetting();
    };

    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.employeeInformation.SystemId))
                throw 'Please select/save employee first'

            args.data = $scope.employeeInformation.SystemId;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "Employees/EmployeeInformation/SaveSignature";


    $scope.getFileList = function () {
        $http({
            method: 'POST', url: $scope.path + 'GetFileInfo', dataType: 'JSON',
            data: { Id: $scope.employeeInformation.SystemId }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.EmpSignature = virtualPath.CardHolderSignature + response.data[0].EmpSignature;
                $scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }

    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.employeeInformation.SystemId))
            ShowResult('Please select/save employee first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    //#region Vendor



    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.employeeNew.VendorId = party.Id;
        $scope.employeeNew.PartyCode = party.Code;
        $scope.employeeNew.PartyName = party.UserName;
        $scope.employeeInformation.VendorId = party.Id;
        $scope.employeeInformation.PartyCode = party.Code;
        $scope.employeeInformation.PartyName = party.UserName;

        angular.element(document.querySelector('#VpartyPopUp')).modal('hide');
    };

    $scope.clearVendor = function () {
        $scope.employeeNew.VendorId = null;
        $scope.employeeNew.PartyCode = null;
        $scope.employeeNew.PartyName = null;
    }



    $scope.showVendorPartyPopUp = function () {
        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
        }
        else if ($scope.partyType === 'Party') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        else if ($scope.partyType === 'Director') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        else if ($scope.partyType === 'Other') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#VpartyPopUp')).modal('show');

    };

    $scope.hideVPartyPopUp = function () {
        angular.element(document.querySelector('#VpartyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.closeVendorPartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.employeeInformation.VendorId = party.Id;
            $scope.employeeInformation.PartyCode = party.Code;
            $scope.employeeInformation.PartyName = party.UserName;
        }
        $scope.hideVPartyPopUp();
    };

    $scope.clearVendorData = function () {
        $scope.employeeInformation.VendorId = null;
        $scope.employeeInformation.PartyCode = null;
        $scope.employeeInformation.PartyName = null;
    }

    //#endregion Vendor



    $scope.RefemployeeList = [];
    $scope.RefEmppopUp = function () {
        try {

            $scope.RefemployeeList = [];
            $http({
                method: 'GET',
                url: 'employees/leaveApplication/GetEmployeeList'
            }).then(function successCallback(response) {
                $scope.RefemployeeList = response.data;
            });
            angular.element(document.querySelector('#employeePopUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.setRefEmpData = function (obj) {
        var data = obj.data;
        $scope.empReferenceInformation.Ref1NameCode = data.EmployeeCode;
        $scope.empReferenceInformation.RefEmpSystemID = data.SystemID;
        $scope.empReferenceInformation.Ref1Name = data.EmployeeName;

        $scope.empReferenceInformation.Ref1EmployerName = data.Company;
        $scope.empReferenceInformation.Ref1EmployerAddress = data.Address1;
        $scope.empReferenceInformation.Ref1Designation = data.LegalDesignation;
        $scope.empReferenceInformation.Ref1CellPhnNo = data.CellPhnNo;
        $scope.empReferenceInformation.Ref1Address = data.PresentAddress1;

        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };

    $scope.clearEmp = function () {
        $scope.empReferenceInformation.Ref1NameCode = null;
        $scope.empReferenceInformation.RefEmpSystemID = null;
        $scope.empReferenceInformation.Ref1Name = null;

        $scope.empReferenceInformation.Ref1EmployerName = null;
        $scope.empReferenceInformation.Ref1EmployerAddress = null;
        $scope.empReferenceInformation.Ref1Designation = null;
        $scope.empReferenceInformation.Ref1CellPhnNo = null;
        $scope.empReferenceInformation.Ref1Address = null;
    }

    $scope.RelemployeeList = [];
    $scope.showEmployeeListPopUp = function () {
        try {

            $scope.RelemployeeList = [];
            $http({
                method: 'GET',
                url: 'employees/leaveApplication/GetEmployeeList'
            }).then(function successCallback(response) {
                $scope.RelemployeeList = response.data;
            });
            angular.element(document.querySelector('#RelemployeePopUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.setRelEmpData = function (obj) {
        var data = obj.data;
        $scope.employeeInformation.RelativeCode = data.EmployeeCode;
        $scope.employeeInformation.RelativeSystemId = data.SystemID;
        $scope.employeeInformation.RelativeName = data.EmployeeName;
        $scope.employeeInformation.RelativeDesignation = data.LegalDesignation;
        $scope.employeeInformation.RelativeCellNo = data.CellPhnNo;
        angular.element(document.querySelector('#RelemployeePopUp')).modal('hide');
    };

    $scope.clearRelEmp = function () {
        $scope.employeeInformation.RelativeCode = null;
        $scope.employeeInformation.RelativeSystemId = null;
        $scope.employeeInformation.RelativeName = null;
        $scope.employeeInformation.RelativeDesignation = null;
        $scope.employeeInformation.RelativeCellNo = null;
    }

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    }

    $scope.closeRelEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    }

    $scope.Print = function (data) {
        location.href = "Employees/EmployeeInformation/GetPrintData?empId=" + data.data.SystemId;
    };


    //#region TrainingType
    $scope.TrainingTypes = [];
    $scope.getTrainingTypeData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getTrainingTypeData",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.TrainingTypes = response.data;
        });
    }
    //#endregion TrainingType


}