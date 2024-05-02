'use strict';
GLManagementController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'accountService', '$window'];
function GLManagementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, accountService, $window) {
    $rootScope.title = 'GL Management';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Accounts/GLManagement/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'CreateGlManagementHeader';
    $scope.saveEmpCatUrl = $scope.path + 'CreateGlManagementEmployeeCategory';
    $scope.saveDesignationUrl = $scope.path + 'CreateGlManagementDesignation';
    $scope.saveDepartmentUrl = $scope.path + 'CreateGlManagementDepartment';
    $scope.savePositionCodeUrl = $scope.path + 'CreateGlManagementPositionCode';
    $scope.saveBudgetCodeUrl = $scope.path + 'CreateGlManagementBudgetCode';
    $scope.saveProcessUrl = $scope.path + 'CreateGlManagementProcess';
    $scope.saveEmpUrl = $scope.path + 'CreateGlManagementEmployee';
    $scope.saveDrCrUrl = $scope.path + 'CreateGlManagementControlDrCr';
    $scope.saveABUrl = $scope.path + 'CreateGlManagementActionBy';
    $scope.saveAPBUrl = $scope.path + 'CreateGlManagementApproveBy';
    $scope.saveRPUrl = $scope.path + 'CreateGlManagementResponsiblePersosn';
    $scope.saveAccessControlUrl = $scope.path + 'CreateGlManagementAccessControl';
    $scope.deleteUrl = $scope.path + 'DeleteGlControl/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }];
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.Type = [];
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 10;
    $scope.setTab2 = function (newTab2) {
        $scope.tab2 = newTab2;
    };

    $scope.isSet2 = function (tabNum2) {
        return $scope.tab2 === tabNum2;
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetGlManagementList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.employeeTypeList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeTypeList = result;
    });

    $scope.ModelTempMat = {
        Id: null,
        UserName: null,
        StorageLocationId: null,
        StorageSubLocation: null,
        MaterialTypeId: null,
        MaterialGroupMasterId: null,
        MaterialMasterId: null,
        MaterialMasterArticleId: null,
        AccessType: null,
        NoOfBin: null,
        Remarks: null,
        StorageLevel: null,
    };
    $scope.ModelNewMat = Object.assign({}, $scope.ModelTempMat);


    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetEmployeeCategory(args.data.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm2.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    $scope.GlManagementId = response.data.Data.Id;
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.GlManagementId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.GlManagementId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                    $scope.selectExpenseGL();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.EmployeeCategoryList = [];
        $scope.employee = [];
        $scope.DesignationList = [];
        $scope.PositionCodeListData = [];
        $scope.BudgetCodeList = [];
        $scope.EmployeeList = [];
        $scope.ControlDrListData = [];
        $scope.ControlCrListData = [];
        $scope.ActionByListData = [];
        $scope.ApproveByListData = [];
        $scope.ResponsiblePersonListData = [];
    }

    $scope.EmployeeCategory = {
        Id: null,
        EmployeeCategoryId: null,
        EmployeeCategory: null
    };
    $scope.EmpCatNew = Object.assign({}, $scope.EmployeeCategory);

    $scope.SaveEmployeeCategory = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.EmployeeCategoryList.length; i++) {
            if ($scope.EmployeeCategoryList[i].EmployeeCategoryId == $scope.EmpCatNew.EmployeeCategoryId) {
                return ShowResult('Same Employee Category already exists!!!', 'failure');
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveEmpCatUrl,
            data: { 'data': $scope.EmpCatNew, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetEmployeeCategory($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    $scope.EmployeeCategoryList = [];
    $scope.GetEmployeeCategory = function (id) {
        $http({
            method: 'POST',
            url: $scope.path + "GetMaterialData",
            data: { 'glManagementId': id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
            $scope.EmpCatNew.EmployeeCategoryId = $scope.EmployeeCategoryList[0].EmployeeCategoryId;
        })
        $scope.GlManagementId = id;
    }

    $scope.RemoveEmployeeCategory = function (data) {
        $scope.EmployeeCategoryId = data.EmployeeCategoryId;
        if (baseService.isUndefinedOrNull(data.EmployeeCategoryId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete.....';
        angular.element(document.querySelector('#EmpCategoryPopUp')).modal('show');
    };

    $scope.DeleteRowEmpCategory = function () {
        if (baseService.isUndefinedOrNull($scope.EmployeeCategoryList[0].EmployeeCategoryId)) {
            if ($scope.EmployeeCategoryList[0].EmployeeCategoryId === $scope.EmployeeCategoryId) {
                $scope.EmployeeCategoryList.splice(0, 1);
            }
        }
        else {
            $scope.DeleteEmployeeCategory()
        }
    };
    $scope.DeleteEmployeeCategory = function () {
        $http.get('Accounts/GLManagement/DeleteEmployeeCategory?Id=' + $scope.EmployeeCategoryId)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetEmployeeCategory($scope.GlManagementId);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    //#region LegalDesignation

    $scope.DesignationDataList = [];
    $scope.GetDesignationInformation = function () {
        try {
            $http({
                method: 'GET',
                url: 'Accounts/GLManagement/GetDesignationInformation?GlManagementId=' + $scope.GlManagementId,
            }).then(function successCallback(response) {
                $scope.DesignationDataList = response.data;
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateDesignation = function (args) {
        $("#Designationheadchk").ejCheckBox({ "change": CheckBoxSelectAllDesignation });
    };

    function CheckBoxSelectAllDesignation(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridDes").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DesignationDataList.length; i++) {
                $scope.DesignationDataList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDes").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.DesignationList = [];
    $scope.OKDesignationInformation = function () {
        try {
            $scope.DesignationList = [];
            for (var i = 0; i < $scope.DesignationDataList.length; i++) {
                if ($scope.DesignationDataList[i].CheckBoxSelect == true) {
                    $scope.DesignationList.push($scope.DesignationDataList[i]);
                }
                if ($scope.DesignationDataList[i].CheckBoxSelect == false && $scope.DesignationDataList[i].Id != null) {
                    $scope.DesignationList.push($scope.DesignationDataList[i]);
                }
            }
            $scope.SaveDesignation();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.SaveDesignation = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.DesignationList.length; i++) {
            for (var j = 0; j < $scope.DesignationDataList.length; j++) {
                if ($scope.DesignationDataList[j].Id == $scope.DesignationList[i].Id) {
                    if ($scope.DesignationDataList[j].CheckBoxSelect == false) {
                        $scope.DesignationList[i].CheckBoxSelect = false;
                    }
                }
            }
        }

        $http({
            method: 'POST',
            url: $scope.saveDesignationUrl,
            data: { 'data': $scope.DesignationList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetDesignationInformation();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.GetDesignationData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDesignationData",
            data: { 'glManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DesignationList = response.data;
        })
    }

    //#endregion LegalDesignation

    //#region Department
    $scope.DepartmentDataList = [];
    $scope.GetDepartmentInformation = function () {
        try {
            $http({
                method: 'GET',
                url: 'Accounts/GLManagement/GetDepartmentInformation?GlManagementId=' + $scope.GlManagementId,
            }).then(function successCallback(response) {
                $scope.DepartmentDataList = response.data;
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateDepartment = function (args) {
        $("#Depheadchk").ejCheckBox({ "change": CheckBoxSelectAllDepartment });
    };

    function CheckBoxSelectAllDepartment(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#depInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DepartmentDataList.length; i++) {
                $scope.DepartmentDataList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#depInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.DepartmentList = [];
    $scope.OKDepartmentInformation = function () {
        try {
            $scope.DepartmentList = [];
            for (var i = 0; i < $scope.DepartmentDataList.length; i++) {
                if ($scope.DepartmentDataList[i].CheckBoxSelect == true) {
                    $scope.DepartmentList.push($scope.DepartmentDataList[i]);
                }
                if ($scope.DepartmentDataList[i].CheckBoxSelect == false && $scope.DepartmentDataList[i].Id != null) {
                    $scope.DepartmentList.push($scope.DepartmentDataList[i]);
                }
            }
            $scope.SaveDepartment();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.SaveDepartment = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.DepartmentList.length; i++) {
            for (var j = 0; j < $scope.DepartmentDataList.length; j++) {
                if ($scope.DepartmentDataList[j].Id == $scope.DepartmentList[i].Id) {
                    if ($scope.DepartmentDataList[j].CheckBoxSelect == false) {
                        $scope.DepartmentList[i].CheckBoxSelect = false;
                    }
                }
            }
        }

        $http({
            method: 'POST',
            url: $scope.saveDepartmentUrl,
            data: { 'data': $scope.DepartmentList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetDepartmentInformation();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    //#endregion Department

    //#region position 
    $scope.PositionCodeListData = [];
    $scope.getPositionCode = function () {
        $http({
            method: 'Get',
            url: 'Accounts/GLManagement/GetPositionCode?GlManagementId=' + $scope.GlManagementId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PositionCodeListData = resp.data;
        });
    }

    $scope.refreshTemplatePC = function (args) {
        $("#PCheadchk").ejCheckBox({ "change": CheckBoxSelectAllPC });
    };

    function CheckBoxSelectAllPC(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PositionCodeListData.length; i++) {
                $scope.PositionCodeListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPC").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.PositionCodeData = [];
    $scope.OKPositionCode = function () {
        try {
            $scope.PositionCodeData = [];
            for (var i = 0; i < $scope.PositionCodeListData.length; i++) {
                if ($scope.PositionCodeListData[i].CheckBoxSelect == true) {
                    $scope.PositionCodeData.push($scope.PositionCodeListData[i]);
                }
                if ($scope.PositionCodeListData[i].CheckBoxSelect == false && $scope.PositionCodeListData[i].Id != null) {
                    $scope.PositionCodeData.push($scope.PositionCodeListData[i]);
                }
            }
            $scope.SavePositionCode();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.SavePositionCode = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.PositionCodeData.length; i++) {
            for (var j = 0; j < $scope.PositionCodeListData.length; j++) {
                if ($scope.PositionCodeListData[j].Id == $scope.PositionCodeData[i].Id) {
                    if ($scope.PositionCodeListData[j].CheckBoxSelect == false) {
                        $scope.PositionCodeData[i].CheckBoxSelect = false;
                    }
                }
            }
        }

        $http({
            method: 'POST',
            url: $scope.savePositionCodeUrl,
            data: { 'data': $scope.PositionCodeData, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getPositionCode();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };



    //#endregion position

    //#region BudgetCode

    $scope.BudgetCodepopUpDataList = [];
    $scope.GetBudgetCodeInformation = function () {
        try {
            $http({
                method: 'GET',
                url: 'Accounts/GLManagement/getbudgetcodelist?GlManagementId=' + $scope.GlManagementId
            }).then(function successCallback(response) {
                $scope.BudgetCodepopUpDataList = response.data;
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.BudgetCodeList = [];
    $scope.OKBudgetCode = function () {
        $scope.BudgetCodeList = [];
        try {
            for (var i = 0; i < $scope.BudgetCodepopUpDataList.length; i++) {
                if ($scope.BudgetCodepopUpDataList[i].CheckBoxSelect == true) {
                    $scope.BudgetCodeList.push($scope.BudgetCodepopUpDataList[i]);
                }
                if ($scope.BudgetCodepopUpDataList[i].CheckBoxSelect == false && $scope.BudgetCodepopUpDataList[i].Id != null) {
                    $scope.BudgetCodeList.push($scope.BudgetCodepopUpDataList[i]);
                }
            }
            $scope.SaveBudgetCode();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateBC = function (args) {
        $("#BCheadchk").ejCheckBox({ "change": CheckBoxSelectAllBC });
    };
    function CheckBoxSelectAllBC(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridBudgetCode").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.BudgetCodepopUpDataList.length; i++) {
                $scope.BudgetCodepopUpDataList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridBudgetCode").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveBudgetCode = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.BudgetCodeList.length; i++) {
            for (var j = 0; j < $scope.BudgetCodepopUpDataList.length; j++) {
                if ($scope.BudgetCodepopUpDataList[j].BudgetCodeId == $scope.BudgetCodeList[i].BudgetCodeId) {
                    if ($scope.BudgetCodepopUpDataList[j].CheckBoxSelect == false) {
                        $scope.BudgetCodeList[i].CheckBoxSelect = false;
                    }
                }
            }
        }

        $http({
            method: 'POST',
            url: $scope.saveBudgetCodeUrl,
            data: { 'data': $scope.BudgetCodeList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetBudgetCodeInformation();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion BudgetCode

    //#region Employee 
    $scope.employee = [];
    $scope.getEmployeePopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/GLManagement/getemployeelist?GlManagementId=' + $scope.GlManagementId
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
    }

    $scope.EmployeeList = [];
    $scope.OKEmployee = function () {
        $scope.EmployeeList = [];
        try {
            for (var i = 0; i < $scope.employee.length; i++) {
                if ($scope.employee[i].CheckBoxSelect == true) {
                    $scope.EmployeeList.push($scope.employee[i]);
                }
                if ($scope.employee[i].CheckBoxSelect == false && $scope.employee[i].Id != null) {
                    $scope.EmployeeList.push($scope.employee[i]);
                }
            }
            $scope.SaveEmployeeData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateEmployee = function (args) {
        $("#Empheadchk").ejCheckBox({ "change": CheckBoxSelectAllEmp });
    };

    function CheckBoxSelectAllEmp(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridEmp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.employee.length; i++) {
                $scope.employee[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmp").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveEmployeeData = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.EmployeeList.length; i++) {
            for (var j = 0; j < $scope.employee.length; j++) {
                if ($scope.employee[j].SystemID == $scope.EmployeeList[i].EmpSystemId) {
                    if ($scope.employee[j].CheckBoxSelect == false) {
                        $scope.EmployeeList[i].CheckBoxSelect = false;
                    }
                }
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveEmpUrl,
            data: { 'data': $scope.EmployeeList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getEmployeePopUpData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion Employee

    //#region Control Dr 
    $scope.ControlDr = false;
    $scope.ControlCr = false;
    $scope.Remarks = null;

    $scope.ControlDrListData = [];
    $scope.GetControlDrData = function (tab) {
        $scope.TabName = tab;
        $http({
            method: 'GET',
            url: 'Accounts/GLManagement/getControlDrlist?GlManagementId=' + $scope.GlManagementId + '&tabName=' + $scope.TabName,
        }).then(function successCallback(response) {
            $scope.ControlDrListData = response.data;
        });
    }

    $scope.ControlDrList = [];
    $scope.OKControlDr = function () {
        $scope.ControlDrList = [];
        try {
            for (var i = 0; i < $scope.ControlDrListData.length; i++) {
                if ($scope.ControlDrListData[i].CheckBoxSelect == true && $scope.ControlDrListData[i].ControlDr == true && $scope.ControlDrListData[i].ControlCr == true) {
                    $scope.ControlDrList.push($scope.ControlDrListData[i]);
                }
                if ($scope.ControlDrListData[i].CheckBoxSelect == true && $scope.ControlDrListData[i].ControlDr == true && $scope.ControlDrListData[i].ControlCr == false) {
                    $scope.ControlDrList.push($scope.ControlDrListData[i]);
                }
                if ($scope.ControlDrListData[i].CheckBoxSelect == true && $scope.ControlDrListData[i].ControlDr == false && $scope.ControlDrListData[i].ControlCr == true) {
                    $scope.ControlDrList.push($scope.ControlDrListData[i]);
                }
                if ($scope.ControlDrListData[i].CheckBoxSelect == false && $scope.ControlDrListData[i].Id != null) {
                    $scope.ControlDrList.push($scope.ControlDrListData[i]);
                }
            }
            $scope.SaveControlDrData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateControlDr = function (args) {
        $("#CDrheadchk").ejCheckBox({ "change": CheckBoxSelectAllCDr });
    };

    function CheckBoxSelectAllCDr(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridCDr").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ControlDrListData.length; i++) {
                $scope.ControlDrListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridCDr").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveControlDrData = function () {
        $scope.tabN = 'ControlDr';
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
       
        $http({
            method: 'POST',
            url: $scope.saveDrCrUrl,
            data: { 'data': $scope.ControlDrList, 'GlManagementId': $scope.GlManagementId, 'TabName': $scope.TabName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetControlDrData($scope.tabN);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion Control Dr 

    //#region Control Cr
    $scope.ControlCrListData = [];
    $scope.GetControlCrData = function (tab) {
        $scope.TabName = tab;
        $http({
            method: 'GET',
            url: 'Accounts/GLManagement/getControlDrlist?GlManagementId=' + $scope.GlManagementId + '&tabName=' + $scope.TabName,
        }).then(function successCallback(response) {
            $scope.ControlCrListData = response.data;
        });
    }

    $scope.ControlCrList = [];
    $scope.OKControlCr = function (obj) {
        $scope.TabName = obj;
        $scope.ControlCrList = [];
        try {
            for (var i = 0; i < $scope.ControlCrListData.length; i++) {
                if ($scope.ControlCrListData[i].CheckBoxSelect == true) {
                    $scope.ControlCrList.push($scope.ControlCrListData[i]);
                }
                if ($scope.ControlCrListData[i].CheckBoxSelect == false && $scope.ControlCrListData[i].Id != null) {
                    $scope.ControlCrList.push($scope.ControlCrListData[i]);
                }
            }
            $scope.SaveControlCrData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateControlCr = function (args) {
        $("#CCrheadchk").ejCheckBox({ "change": CheckBoxSelectAllCCr });
    };

    function CheckBoxSelectAllCCr(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridCCr").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ControlCrListData.length; i++) {
                $scope.ControlCrListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridCCr").data("ejGrid");
        gridObj.refreshContent();
    };
    $scope.SaveControlCrData = function () {
        $scope.tabN = 'ControlCr';
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.ControlCrList.length; i++) {
            for (var j = 0; j < $scope.ControlCrListData.length; j++) {
                if ($scope.ControlCrListData[j].BudgetMasterActivityId == $scope.ControlCrList[i].BudgetMasterActivityIdCr) {
                    if ($scope.ControlCrListData[j].CheckBoxSelect == false) {
                        $scope.ControlCrList[i].CheckBoxSelect = false;
                    }
                }
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveDrCrUrl,
            data: { 'data': $scope.ControlCrList, 'GlManagementId': $scope.GlManagementId, 'TabName': $scope.TabName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetControlCrData($scope.tabN);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion Control Cr


    //#region Action By
    $scope.ActionByListData = [];
    $scope.getActionByPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/GLManagement/getActionBylist?GlManagementId=' + $scope.GlManagementId
        }).then(function successCallback(response) {
            $scope.ActionByListData = response.data;
        });
    }

    $scope.ActionByList = [];
    $scope.OKActionBy = function () {
        $scope.ActionByList = [];
        try {
            for (var i = 0; i < $scope.ActionByListData.length; i++) {
                if ($scope.ActionByListData[i].CheckBoxSelect == true) {
                    $scope.ActionByList.push($scope.ActionByListData[i]);
                }
                if ($scope.ActionByListData[i].CheckBoxSelect == false && $scope.ActionByListData[i].Id != null) {
                    $scope.ActionByList.push($scope.ActionByListData[i]);
                }
            }

            $scope.SaveABData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.refreshTemplateAB = function (args) {
        $("#ABheadchk").ejCheckBox({ "change": CheckBoxSelectAllAB });
    };

    function CheckBoxSelectAllAB(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridAB").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ActionByListData.length; i++) {
                $scope.ActionByListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridAB").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveABData = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.ActionByList.length; i++) {
            for (var j = 0; j < $scope.ActionByListData.length; j++) {
                if ($scope.ActionByListData[j].SystemID == $scope.ActionByList[i].ActionById) {
                    if ($scope.ActionByListData[j].CheckBoxSelect == false) {
                        $scope.ActionByList[i].CheckBoxSelect = false;
                    }
                }
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveABUrl,
            data: { 'data': $scope.ActionByList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getActionByPopUpData($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion Action By

    //#region Approve By
    $scope.ApproveByListData = [];
    $scope.getApproveByPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/GLManagement/getApproveBylist?GlManagementId=' + $scope.GlManagementId
        }).then(function successCallback(response) {
            $scope.ApproveByListData = response.data;
        });
    }

    $scope.ApproveByList = [];
    $scope.OKApproveBy = function () {
        $scope.ApproveByList = [];
        try {
            for (var i = 0; i < $scope.ApproveByListData.length; i++) {
                if ($scope.ApproveByListData[i].CheckBoxSelect == true) {
                    $scope.ApproveByList.push($scope.ApproveByListData[i]);
                }
                if ($scope.ApproveByListData[i].CheckBoxSelect == false && $scope.ApproveByListData[i].Id != null) {
                    $scope.ApproveByList.push($scope.ApproveByListData[i]);
                }
            }
            $scope.SaveAPBData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateAPB = function (args) {
        $("#APBheadchk").ejCheckBox({ "change": CheckBoxSelectAllAPB });
    };

    function CheckBoxSelectAllAPB(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridAPB").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ApproveByListData.length; i++) {
                $scope.ApproveByListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridAPB").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveAPBData = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.ApproveByList.length; i++) {
            for (var j = 0; j < $scope.ApproveByListData.length; j++) {
                if ($scope.ApproveByListData[j].SystemID == $scope.ApproveByList[i].ApproveById) {
                    if ($scope.ApproveByListData[j].CheckBoxSelect == false) {
                        $scope.ApproveByList[i].CheckBoxSelect = false;
                    }
                }
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveAPBUrl,
            data: { 'data': $scope.ApproveByList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getApproveByPopUpData($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion Approve By

    //#region ControlDrCr
    $rootScope.titleTab1 = 'Control Dr';
    $rootScope.titleTab = 'Control Cr';
    //#endregion ControlDrCr

    //#region Responsible Person
    $scope.ResponsiblePersonListData = [];
    $scope.GetResponsiblePersonData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/GLManagement/getResponsiblePersonlist?GlManagementId=' + $scope.GlManagementId
        }).then(function successCallback(response) {
            $scope.ResponsiblePersonListData = response.data;
        });
    }

    $scope.ResponsiblePersonList = [];
    $scope.OKResponsiblePerson = function () {
        $scope.ResponsiblePersonList = [];
        try {
            for (var i = 0; i < $scope.ResponsiblePersonListData.length; i++) {
                if ($scope.ResponsiblePersonListData[i].CheckBoxSelect == true) {
                    $scope.ResponsiblePersonList.push($scope.ResponsiblePersonListData[i]);
                }
                if ($scope.ResponsiblePersonListData[i].CheckBoxSelect == false && $scope.ResponsiblePersonListData[i].Id != null) {
                    $scope.ResponsiblePersonList.push($scope.ResponsiblePersonListData[i]);
                }
            }
            $scope.SaveRPData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.refreshTemplateRP = function (args) {
        $("#RPheadchk").ejCheckBox({ "change": CheckBoxSelectAllRP });
    };

    function CheckBoxSelectAllRP(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridRP").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ResponsiblePersonListData.length; i++) {
                $scope.ResponsiblePersonListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridRP").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveRPData = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.ResponsiblePersonList.length; i++) {
            for (var j = 0; j < $scope.ResponsiblePersonListData.length; j++) {
                if ($scope.ResponsiblePersonListData[j].SystemID == $scope.ResponsiblePersonList[i].ResponsiblePersonId) {
                    if ($scope.ResponsiblePersonListData[j].CheckBoxSelect == false) {
                        $scope.ResponsiblePersonList[i].CheckBoxSelect = false;
                    }
                }
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveRPUrl,
            data: { 'data': $scope.ResponsiblePersonList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetResponsiblePersonData($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion Responsible Person

    //#region Process

    $scope.ProcessListData = [];
    $scope.GetProcessData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/GLManagement/GetGLManagementProcessData?GlManagementId=' + $scope.GlManagementId
        }).then(function successCallback(response) {
            $scope.ProcessListData = response.data;
        });
    }


    $scope.ProcessList = [];
    $scope.OKProcess = function () {
        $scope.ProcessList = [];
        try {
            for (var i = 0; i < $scope.ProcessListData.length; i++) {
                if ($scope.ProcessListData[i].CheckBoxSelect == true) {
                    $scope.ProcessList.push($scope.ProcessListData[i]);
                }
                if ($scope.ProcessListData[i].CheckBoxSelect == false && $scope.ProcessListData[i].Id != null) {
                    $scope.ProcessList.push($scope.ProcessListData[i]);
                }
            }
            $scope.SaveProcessData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.refreshTemplateProcess = function (args) {
        $("#Processheadchk").ejCheckBox({ "change": CheckBoxSelectAllProcess });
    };

    function CheckBoxSelectAllProcess(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridProcess").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProcessListData.length; i++) {
                $scope.ProcessListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridProcess").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveProcessData = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.ProcessList.length; i++) {
            for (var j = 0; j < $scope.ProcessListData.length; j++) {
                if ($scope.ProcessListData[j].Id == $scope.ProcessList[i].Id) {
                    if ($scope.ProcessListData[j].CheckBoxSelect == false) {
                        $scope.ProcessList[i].CheckBoxSelect = false;
                    }
                }
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveProcessUrl,
            data: { 'data': $scope.ProcessList, 'GlManagementId': $scope.GlManagementId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetProcessData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion Process

    //region View

    $scope.AllCombineData = [];
    $scope.GetData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/GLManagement/GetGLManagementViewData?GlManagementId=' + $scope.GlManagementId
        }).then(function successCallback(response) {
            $scope.AllCombineData = response.data;
        });
    }


    $scope.GlManagementAccessControlData = [];
    $scope.GetGlManagementAccessControl = function () {
        $http({
            method: 'GET',
            url: 'Accounts/GLManagement/GlManagementAccessControl?GlManagementId=' + $scope.GlManagementId
        }).then(function successCallback(response) {
            $scope.GlManagementAccessControlData = response.data;
        });
    }


    $scope.CombineList = [];
    $scope.OKGlManagementAccessControl = function () {
        $scope.AccessControlList = [];
        try {
            for (var i = 0; i < $scope.GlManagementAccessControlData.length; i++) {
                if ($scope.GlManagementAccessControlData[i].CheckBoxSelect == true) {
                    $scope.AccessControlList.push($scope.CombineListData[i]);
                }
                if ($scope.GlManagementAccessControlData[i].CheckBoxSelect == false && $scope.GlManagementAccessControlData[i].Id != null) {
                    $scope.AccessControlList.push($scope.CombineListData[i]);
                }
            }
            $scope.SaveAccessControl();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.refreshTemplateProcess = function (args) {
        $("#Processheadchk").ejCheckBox({ "change": CheckBoxSelectAllProcess });
    };

    function CheckBoxSelectAllCombine(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridProcess").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProcessListData.length; i++) {
                $scope.ProcessListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridProcess").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveAccessControl = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        
        $http({
            method: 'POST',
            url: $scope.saveAccessControlUrl,
            data: { 'data': $scope.AccessControlList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetProcessData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#End Region
    $scope.report = {
        GLName: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null
    };


    $scope.glList = [];
    $scope.getCompanyGLCboList = function () {
        accountService.getCompanyGLCboList(function (result) {
            $scope.glList = result;
        });
    };
    $scope.getCompanyGLCboList();

    $scope.budgetList = [];
    $scope.getBudgetMasterCboList = function (glId) {
        accountService.getBudgetMasterCboList(glId, function (result) {
            $scope.budgetList = result;
        });
    };

    $scope.activityList = [];
    $scope.getBudgetMasterActivityCbo = function (budgetMasterId) {
        accountService.getBudgetMasterActivityCbo(budgetMasterId, function (result) {
            $scope.activityList = result;
        });
    };

    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "RefNo",
            "value": "RefNo"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.tabType = "";
    $scope.GetCOAICodeList = function (data) {
        $scope.tabType = data;
        $scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityList";
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    function checkConsumableExist(list, data) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].GLGeneralInfoId === data.GLGeneralInfoId && list[i].BudgetMasterId === data.BudgetMasterId && list[i].ActivityId === data.ActivityId) {
                return true;
            }
        }
        return false;
    }


    $scope.ExpenseGLList = [];
    $scope.selectGLBudget = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetExpenseGLData",
            data: {
                'glId': data.GLGeneralInfoId,
                'budgetId': data.BudgetMasterId,
                'activityId': data.ActivityId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ExpenseGLList = response.data;
        })
    }


    $scope.GLControlReport = function (data, index) {
        $scope.fileName = "GLControlReport.xlsx";

        $http({
            method: 'POST',
            url: $scope.path + "GetGLControlReport",
            data: { 'glControlId': data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

}