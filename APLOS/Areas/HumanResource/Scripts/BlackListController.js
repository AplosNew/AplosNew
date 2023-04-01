'use strict';
BlackListController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function BlackListController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Black List';
    $scope.Action = 'Save';
    $scope.BlkList = [];
    $scope.path = 'HumanResource/BlackList/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "AadharNumber"; $scope.search = "";
    $scope.searchByList = [{ value: 'AadharNumber', name: "National Id" }, { value: 'bl.Date', name: "Date" }, { value: 'EMP.EmployeeCode', name: "Employee Code" }, { value: 'EMP.EmployeeName', name: "Employee Name" }, { value: 'empl.EmployeeName', name: "By Whom" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BlkList = response.data;
            ClearFields();
        });
    }
    $scope.getData();

    $scope.BlackListModelTemp = {
        Id: null,
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        AadharNumber: null,
        CompanyEmployeeOutsider: 'CompanyEmp',
        EmpSystemId: null,
        OutsiderName: null,
        OutsiderFatherName: null,
        OutsiderMotherName: null,
        Reason: null,
        ByWhomId: $window.employeeId,
        BlacklistingDate: null,
        Remarks: null,
        DOB: null,
        ResponsiblePerson: null,
        EmployeeCode: null,
        EmployeeStatus: null,
        FatherName: null,
        MotherName: null,
        Plant: null,
        EmpStatus: null,
        ByWhom: null,
        EmpCode: null,
    };
    $scope.BlackList = Object.assign({}, $scope.BlackListModelTemp);

    $scope.Get = function (args) {

        $scope.BlackList = Object.assign({}, args.data);
        $scope.BlackList.Date = $scope.BlackList.BLDate;
        $scope.BlackList.BlacklistingDate = $scope.BlackList.BLBlacklistingDate;
        $scope.BlackList.DOB = $scope.BlackList.EMPDOB;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.BlackList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.BlackList.Id = response.data.Data.Id;
                    //ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.BlackList.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.BlackList.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };


    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.BlackListDocument + '/' + data.Id + extention;
    };


    function ClearFields() {
        $scope.Action = 'Save';
        $scope.BlackList = Object.assign({}, $scope.BlackListModelTemp);
    }

    $scope.OutSiderEmpclear = function () {
        $scope.BlackList.OutsiderName = null;
        $scope.BlackList.OutsiderFatherName = null;
        $scope.BlackList.OutsiderMotherName = null;
        $scope.BlackList.AadharNumber = null;
    }

    $scope.CompanyEmpclear = function () {
        $scope.ResponsiblePersonClear();
    }

    // #region field

    $scope.EmpResPersonList = [];
    $scope.ResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpResPersonList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.BlackList.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.BlackList.EmpSystemId = null;
        $scope.BlackList.ResponsiblePerson = null;
        $scope.BlackList.EmployeeCode = null;
        $scope.BlackList.EmployeeStatus = null;
        $scope.BlackList.FatherName = null;
        $scope.BlackList.MotherName = null;
        $scope.BlackList.DOB = null;
        $scope.BlackList.Plant = null;
        $scope.BlackList.AadharNumber = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.BlackList.EmployeeCode = data.Code;
        $scope.BlackList.EmpSystemId = data.Id;
        $scope.BlackList.ResponsiblePerson = data.EmployeeName;

        $scope.BlackList.FatherName = data.FatherName;
        $scope.BlackList.MotherName = data.MotherName;
        $scope.BlackList.DOB = data.EMPDOB;
        $scope.BlackList.Plant = data.Plant;
        $scope.BlackList.AadharNumber = data.NationalID;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region

    // #region field

    $scope.ByWhomList = [];
    $scope.ByWhomPopUp = function () {
        angular.element(document.querySelector("#ByWhomPopUp")).modal("show");
        $scope.getByWhomData();

    }
    $scope.getByWhomData = function () {
        $scope.ByWhomList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.BlackList.Id },
            url: $scope.path + 'LoadAllByWhomDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.ByWhomList = response.data;
        });
    }

    $scope.ByWhomClear = function () {
        $scope.BlackList.ByWhomId = null;
        $scope.BlackList.ByWhom = null;
        $scope.BlackList.EmpCode = null;
        $scope.BlackList.EmpStatus = null;
    };
    $scope.closeByWhomPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setByWhomData = function (obj) {

        var data = obj.data;
        $scope.BlackList.EmpCode = data.Code;
        $scope.BlackList.ByWhomId = data.Id;
        $scope.BlackList.ByWhom = data.EmployeeName;
        angular.element(document.querySelector('#ByWhomPopUp')).modal('hide');
    };

    $scope.getByWhomDatabyUserEmp = function () {
        $http({
            method: 'Get',
            url: $scope.path + 'getByWhomDatabyUserEmp?empId=' + $window.employeeId
        }).then(function successCallback(response) {
            $scope.BlackList.EmpCode = response.data[0].EmployeeCode;
            $scope.BlackList.ByWhomId = response.data[0].SystemId;
            $scope.BlackList.ByWhom = response.data[0].EmployeeName;
        });
    }
    $scope.getByWhomDatabyUserEmp();

    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.BlackList.Id))
                throw 'Please select/save the data first'

            args.data = $scope.BlackList.Id;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "HumanResource/BlackList/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.BlackList.Id))
            ShowResult('Please select/save the data first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.BlackList.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    // # end region
}