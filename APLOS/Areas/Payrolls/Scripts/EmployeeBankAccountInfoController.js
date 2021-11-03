'use strict';
EmployeeBankAccountInfoController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeBankAccountInfoController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Bank Account Information';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/EmployeeBankAccountInfo/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);

    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'employees/leaveApplication/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.leaveApplication = {
        EmployeeCode: null,
        EmpSystemID: null,
        EmployeeName: null,
        LegalDesignation: null,
        DOJ: null,
        DOC: null,
    };
    $scope.leaveApplicationNew = Object.assign({}, $scope.leaveApplication);


    $scope.setEmpData = function (obj) {
        $scope.Clear();
        var data = obj.data;
        $scope.leaveApplicationNew.EmployeeCode = data.EmployeeCode;
        $scope.leaveApplicationNew.EmpSystemID = data.SystemID;
        $scope.leaveApplicationNew.EmployeeName = data.EmployeeName;
        $scope.leaveApplicationNew.LegalDesignation = data.LegalDesignation;
        $scope.leaveApplicationNew.DOJ = data.DOJ;
        $scope.leaveApplicationNew.DOC = data.DOC;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        $scope.getMaster(data.SystemID);
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.leaveApplication = { SectionId: $scope.leaveApplication.SectionId };
        $scope.leaveApplicationNew = { SectionId: $scope.leaveApplicationNew.SectionId };
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
        $scope.LeaveBalanceList = [];
        $scope.LeaveTransactionList = [];
        $scope.imageSrc = virtualPath.EmployeePic + '';
    }

    $scope.BankList = [];
    $scope.getBankPopUpData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetBank",
            data: {},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BankList = response.data;
        });
        angular.element(document.querySelector('#BankNewPopUp')).modal('show');
    }
    $scope.closeBankPopUp = function () {
        angular.element(document.querySelector('#BankPopUp')).modal('hide');
        angular.element(document.querySelector('#BankNewPopUp')).modal('hide');
    };
    $scope.Bank = {
        BankId: null,
        BranchId: null,
        BankName: null,
        BranchName: null,
    };

    $scope.BankInfo = Object.assign({}, $scope.Bank);

    $scope.setBankdata = function (obj) {
        $scope.ClearBank();
        var data = obj.data;
        $scope.BankInfo.BankId = data.BankId;
        $scope.BankInfo.BranchId = data.BranchId;
        $scope.BankInfo.BankName = data.BankName;
        $scope.BankInfo.BranchName = data.BranchName;
        angular.element(document.querySelector('#BankNewPopUp')).modal('hide');
    };
    $scope.ClearBank = function () {
        ClearB();
        return true;
    };
    function ClearB() {
        $scope.Action = 'Save';
        $scope.BankInfo.BankId = null;
        $scope.BankInfo.BranchId = null;
        $scope.BankInfo.BankName = null;
        $scope.BankInfo.BranchName = null;
        $scope.BankList = [];
    }

    $scope.EmployeeBankInfo = {
        RowID: null,
        EmpSystemID: null,
        BankSystemID: null,
        BankBranchId: null,
        BankAccNo: null,
        SalaryPercentage: null,
        IFSCCode: null,
        MICRCode: null,
        PaymentMode: null,
    };

    //#region Get Master
    $scope.MasterList = [];
    $scope.getMaster = function (employeeID) {
        $http({
            method: 'GET',
            url: $scope.path + "GetMaster?EmpID=" + employeeID,
        }).then(function successCallback(response) {
            $scope.EmployeeBankInfo = response.data[0];
            $scope.BankInfo.BankId = $scope.EmployeeBankInfo.BankSystemID;
            $scope.BankInfo.BankName = $scope.EmployeeBankInfo.Bank;
            $scope.BankInfo.BranchId = $scope.EmployeeBankInfo.BankBranchId;
            $scope.BankInfo.BranchName = $scope.EmployeeBankInfo.BankBranch;
        });
    }
    //$scope.getMaster();
    $scope.getDetail = function (obj) {
        $scope.EmployeeBankInfo = obj.data;
        $scope.leaveApplicationNew.EmpSystemID = $scope.EmployeeBankInfo.EmpSystemID;
        $scope.imageSrc = $scope.EmployeeBankInfo.EmpPicPath;
        $scope.leaveApplicationNew.EmployeeName = $scope.EmployeeBankInfo.EmployeeName;
        $scope.leaveApplicationNew.LegalDesignation = $scope.EmployeeBankInfo.LegalDesignation;
        $scope.leaveApplicationNew.DOJ = $scope.EmployeeBankInfo.DOJ;
        $scope.BankInfo.BankId = $scope.EmployeeBankInfo.BankSystemID;
        $scope.BankInfo.BankName = $scope.EmployeeBankInfo.Bank;
        $scope.BankInfo.BranchId = $scope.EmployeeBankInfo.BankBranchId;
        $scope.BankInfo.BranchName = $scope.EmployeeBankInfo.BankBranch;
    }

    //#endregion

    //#region save 
    $scope.Save = function () {
        try {
            if ($scope.EmployeeBankInfo.SalaryPercentage > 100) {
                throw "Salary Percentage can not exceed 100...";
            }
            $scope.EmployeeBankInfo.EmpSystemID = $scope.leaveApplicationNew.EmpSystemID;
            $scope.EmployeeBankInfo.BankSystemID = $scope.BankInfo.BankId;
            $scope.EmployeeBankInfo.BankBranchId = $scope.BankInfo.BranchId;
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'master': $scope.EmployeeBankInfo },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getMaster($scope.EmployeeBankInfo.EmpSystemID);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //#endregion

    //#region Clear Master
    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.EmployeeBankInfo = {
            RowID: null,
            EmpSystemID: null,
            BankSystemID: null,
            BankBranchId: null,
            BankAccNo: null,
            SalaryPercentage: null,
            IFSCCode: null,
            MICRCode: null,
            PaymentMode: null,
        };
        $scope.MasterList = [];
        $scope.leaveApplicationNew.EmpSystemID = null;
        $scope.leaveApplicationNew.EmployeeCode = null;
        $scope.leaveApplicationNew.EmployeeName = null;
        $scope.leaveApplicationNew.LegalDesignation = null;
        $scope.leaveApplicationNew.DOJ = null;
        $scope.BankInfo.BankId = null;
        $scope.BankInfo.BankName = null;
        $scope.BankInfo.BranchId = null;
        $scope.BankInfo.BranchName = null;
        $scope.imageSrc = null;
    };
    //#endregion

}
