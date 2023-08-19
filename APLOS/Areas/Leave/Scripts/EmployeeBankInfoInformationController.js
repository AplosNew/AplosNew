'use strict';
EmployeeBankInfoInformationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function EmployeeBankInfoInformationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Employee Bank Info Information';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'Leave/EmployeeBankInfoInformation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

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
   

    $scope.dataList = [];
    $scope.employeeInfo = {};
    $scope.GetEmployeeDeleteInfo = function () {
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: 'employees/EmployeeDelete/getemployeeDelete'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

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
        IFSCCode:null,
        MICRCode:null
    }

    $scope.employeeInfo = {};
    $scope.SetData = function (obj) {
        var emp = obj.data;
        $scope.employeeInfo.EmpSystemID = emp.SystemID;
        $scope.employeeInfo.EmpPic = virtualPath.EmployeePic + emp.EmpPicPath;
        $scope.employeeInfo.EmployeeCode = emp.EmployeeCode;
        $scope.employeeInfo.EmployeeName = emp.EmployeeName;
        $scope.employeeInfo.DOJ = emp.DOJ;
        $scope.employeeInfo.DOC = emp.DOC;
        $scope.employeeInfo.EmailId = emp.EmailId;
        $scope.employeeInfo.Code = emp.Code;
        $scope.employeeInfo.Section = emp.Section;
        $scope.employeeInfo.SubSection = emp.SubSection;
        $scope.employeeInfo.Department = emp.Department;
        $scope.employeeInfo.LegalDesignation = emp.LegalDesignation;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        $scope.EmpBankInfoModel = {};
        $scope.GetPreData($scope.employeeInfo.EmpSystemID);

    };

    $scope.BankInfo = {};
    $scope.SetBankData = function (obj) {
        var Bankinfo = obj.data;
        $scope.BankInfo.UserName = Bankinfo.UserName;
        $scope.BankInfo.BankBranch = Bankinfo.BankBranch;

        $scope.EmpBankInfoModel.UserName = Bankinfo.UserName;
        $scope.EmpBankInfoModel.BankBranch = Bankinfo.BankBranch;
        $scope.EmpBankInfoModel.BankSystemID = Bankinfo.BankSystemID;
        $scope.EmpBankInfoModel.BankBranchId = Bankinfo.BankBranchId;

        angular.element(document.querySelector('#BankInFoPopUp')).modal('hide');       
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.closeBankInfoPopUp = function () {
        angular.element(document.querySelector('#BankInFoPopUp')).modal('hide');
    }

    $scope.EmpList = [];
    $scope.GetPreData = function (empId) {
        $http.get('Leave/EmployeeBankInfoInformation/GetList?EmpSystemId=' + empId)
            .then(function (response) {
                $scope.EmpList = response.data;
            });
    };

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }

        } catch (ex) {
            throw ex;
        }
    }

    function ValidationMaster() {
        try {
            CheckField("From Date", $scope.OffDutyHoursModel.FromDate);
            CheckField("To Date", $scope.OffDutyHoursModel.ToDate);

        } catch (ex) {
            throw ex;
        }
    }

    $scope.recorddoubleclick = function (args) {
        $scope.EmpBankInfoModel = Object.assign({}, args.data); // gridObj.getSelectedRecords()[0];
        $scope.BankInfo.UserName = $scope.EmpBankInfoModel.UserName;
        $scope.BankInfo.BankBranch = $scope.EmpBankInfoModel.BankBranch;
        $scope.Action = 'Update';
    };

    $scope.Save = function () {
        try {
            $scope.EmpBankInfoModel.EmpSystemId = $scope.employeeInfo.EmpSystemID
            //ValidationMaster();
            if ($scope.EmpBankInfoModel.SalaryPercentage > 100) {
                throw "Salary percentage can't greater than 100";
            }
            if ($scope.OffDutyHoursForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.EmpBankInfoModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            $scope.EmpBankInfoModel = {};

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }

                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.EmpBankInfoModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            $scope.EmpBankInfoModel = {};

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.employeeInfo = {};
        $scope.EmpBankInfoModel = {};
        $scope.EmpList = [];
    }

    $scope.Delete = function () {
        $scope.EmpBankInfoModel.EmpSystemId = $scope.employeeInfo.EmpSystemID
        if (!baseService.isUndefinedOrNull($scope.EmpBankInfoModel.Id)) {
            $http.get('Leave/OffDutyHours/Delete?Id=' + $scope.EmpBankInfoModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                        $scope.EmpBankInfoModel = {};

                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

}