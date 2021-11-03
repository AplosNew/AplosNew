'use strict';
tBSController.$inject = ['$window','commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function tBSController($window,commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'TBS Transaction';
    $scope.Action = 'Save';
    $scope.path = 'Attendances/TBS/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.Actionlist = [];
    $scope.GetCbo = function () {
        $http.get('Attendances/TBS/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.Actionlist = [];
                        $scope.Actionlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();

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

    $scope.TBSModelList = [];
    $scope.employeeInfo = {};
    $scope.GetEmployeeInfo = function () {
        $scope.TBSModelList = [];
        $http({
            method: 'GET',
            url: 'Attendances/TBS/getTBSMaster'
        }).then(function successCallback(response) {
            $scope.TBSModelList = response.data;
        });
    }
    $scope.GetEmployeeInfo();


    $scope.TBSModelOriginal = {
        Id: null,
        EmpSystemId: null,
        DisciplinaryActionCategoryId: null,
        Description: null,
        EntryDate: $filter('dateFiltering')(Date.now()),
        ActionType: 'TBS'
    }
    $scope.TBSModel = Object.assign({}, $scope.TBSModelOriginal);

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
        $scope.TBSModel = Object.assign({}, $scope.TBSModelOriginal);
        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.GetTBSList = [];
    $scope.GetPreData = function (empId) {
        $scope.TBSModel = Object.assign({}, $scope.TBSModelOriginal);
        $scope.GetTBSList = [];
        $http.get('Attendances/TBS/GetTBS?empId=' + empId)
            .then(function (response) {
                $scope.GetTBSList = response.data;
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
            CheckField("Action Type", $scope.TBSModel.DisciplinaryActionCategoryId);
            CheckField("Description", $scope.TBSModel.Description);
            CheckField("Effective Date", $scope.TBSModel.EntryDate);
        } catch (ex) {
            throw ex;
        }
    }

    $scope.recorddoubleclick = function (args) {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();
            
        } catch (e) {

        }
        $scope.TBSModel = Object.assign({}, args.data);
        $scope.Action = 'Update';
    };

    $scope.employeeInfo = {};
    $scope.recorddoubleclickTBS = function (obj) {
        $scope.GetEmployeeInfo();
        var shi = obj.data;
        $scope.employeeInfo.EmpSystemID = shi.SystemID;
        $scope.employeeInfo.EmpPic = virtualPath.EmployeePic + shi.EmpPicPath;
        $scope.employeeInfo.EmployeeCode = shi.EmployeeCode;
        $scope.employeeInfo.EmployeeName = shi.EmployeeName;
        $scope.employeeInfo.DOJ = shi.DOJ;
        $scope.employeeInfo.DOC = shi.DOC;
        $scope.employeeInfo.EmailId = shi.EmailId;
        $scope.employeeInfo.Code = shi.Code;
        $scope.employeeInfo.Section = shi.Section;
        $scope.employeeInfo.SubSection = shi.SubSection;
        $scope.employeeInfo.Department = shi.Department;
        $scope.employeeInfo.LegalDesignation = shi.LegalDesignation;
        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent(true);
        
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
    };


    $window.onresize = function (event) {
        $scope.actionCompleteSelected();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid").ejGrid("instance");
                var scrollerwidth = $("#NewId").width();

                $("#Grid").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 150 } });
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    };

    $scope.AddLineIdem = function () {
        try {

            $scope.ShowDiv = true;

            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();

            $scope.TBSModelOriginal = {
                Id: null,
                EmpSystemId: null,
                DisciplinaryActionCategoryId: null,
                Description: null,
                EntryDate: $filter('dateFiltering')(Date.now()),
                ActionType: 'TBS'
            }
            $scope.TBSModel = Object.assign({}, $scope.TBSModelOriginal);
            $scope.Action = 'Save';
        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.Save = function () {
        try {
            $scope.TBSModel.EmpSystemId = $scope.employeeInfo.EmpSystemID
            if (baseService.isUndefinedOrNull($scope.TBSModel.EmpSystemId)) {
                throw ("Please Select Employee...");
            }
            ValidationMaster();
            if ($scope.TBSForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.TBSModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            //$scope.TBSModel = {};
                            $scope.TBSModel.EntryDate = $filter('dateFiltering')(Date.now());
                            $scope.TBSModel.ActionType = 'TBS';
                            $scope.GetEmployeeInfo();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.TBSModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            $scope.TBSModel = {};
                            $scope.TBSModel.EntryDate = $filter('dateFiltering')(Date.now());
                            $scope.TBSModel.ActionType = 'TBS';
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
        $scope.TBSModel = {};
        $scope.TBSModel.EntryDate = $filter('dateFiltering')(Date.now());
        $scope.TBSModel.ActionType = 'TBS';
        //$scope.GetTBSList = [];
        $scope.Action = 'Save';
    }

    $scope.ClearM = function () {
        $scope.employeeInfo = {};
        $scope.GetTBSList = [];
    };

    $scope.Delete = function () {
        $scope.TBSModel.EmpSystemId = $scope.employeeInfo.EmpSystemID
        if (!baseService.isUndefinedOrNull($scope.TBSModel.Id)) {
            $http.get('Attendances/TBS/Delete?Id=' + $scope.TBSModel.Id + '&EmpSystemId=' + $scope.TBSModel.EmpSystemId + '&Date=' + $scope.TBSModel.EntryDate)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                        $scope.Action = 'Save';
                        $scope.GetEmployeeInfo();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

}