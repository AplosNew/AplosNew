'use strict';
onDutyTransactionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function onDutyTransactionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'ON Duty Transaction';
    $scope.Action = 'Save';
    $scope.path = 'Leave/OnDutyTransaction/';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.dataList = [];
    $scope.employeeInfo = {};
    $scope.GetEmployeeDeleteInfo = function () {
        $http({
            method: 'GET',
            url: 'employees/EmployeeDelete/getemployeeDelete'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.OnDutyModelOriginal = {
        Id: null,
        EmpSystemId: null,
        GroupId: null,
        PlantId: null,
        FromDate: null,
        ToDate: null,
        IsApproved: null,
        Reason: null,      
    }
    $scope.OnDutyModel = Object.assign({}, $scope.OnDutyModelOriginal);
    
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
        $scope.OnDutyModel = Object.assign({}, $scope.OnDutyModelOriginal);
        $scope.GetPreData($scope.employeeInfo.EmpSystemID);        
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.GetOffDutyList = [];
    $scope.GetPreData = function (empId) {
        $http.get('Leave/OnDutyTransaction/GetOffDuty?empId=' + empId)
            .then(function (response) {
                $scope.GetOffDutyList = response.data;
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
            CheckField("From Date", $scope.OnDutyModel.FromDate);
            CheckField("To Date", $scope.OnDutyModel.ToDate);
            CheckField("Reason", $scope.OnDutyModel.Reason);

        } catch (ex) {
            throw ex;
        }
    }

    $scope.recorddoubleclick = function (args) {
        $scope.OnDutyModel = Object.assign({}, args.data); 
        $scope.Action = 'Update';
        $scope.GetPreData($scope.employeeInfo.EmpSystemID); 
    };

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.OnDutyModel.FromDate)) {
                throw ("FromDate Date is required.");
            }
            if (baseService.isUndefinedOrNull($scope.OnDutyModel.ToDate)) {
                throw ("ToDate Date is required.");
            }
             else if (new Date($scope.OnDutyModel.FromDate) > new Date($scope.OnDutyModel.ToDate)) {
                throw ("To Date must be above or equal to From Date.");
            }
            $scope.OnDutyModel.EmpSystemId = $scope.employeeInfo.EmpSystemID
            ValidationMaster();
            if ($scope.OnDutyForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.OnDutyModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            $scope.OnDutyModel = {};
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }

                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.OnDutyModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            $scope.OnDutyModel = {};
                  
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
        //$scope.employeeInfo = {};
        $scope.OnDutyModel = {};
        //$scope.GetOffDutyList = [];
        $scope.Action = 'Save';
    }

    $scope.Delete = function () {
        $scope.OnDutyModel.EmpSystemId = $scope.employeeInfo.EmpSystemID
        if (!baseService.isUndefinedOrNull($scope.OnDutyModel.Id)) {
            $http.get('Leave/OnDutyTransaction/Delete?Id=' + $scope.OnDutyModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                        $scope.OnDutyModel = {};
                        $scope.GetShiftList = {};
                        $scope.Action = 'Save';
                        $scope.Clear();

                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

}