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
    $scope.savePositionCodeUrl = $scope.path + 'CreateGlManagementPositionCode';
    $scope.saveBudgetCodeUrl = $scope.path + 'CreateGlManagementBudgetCode';
    $scope.saveEmpUrl = $scope.path + 'CreateGlManagementEmployee';
    $scope.saveRPUrl = $scope.path + 'CreateGlManagementResponsiblePersosn';
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
            for (var i = 0; i < $scope.ModelList.length; i++) {
                $scope.GlManagementId = $scope.ModelList[i].Id;
            }
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
        $scope.GetDesignationData(args.data.Id);
        $scope.GetPositionCodeData(args.data.Id);
        $scope.GetBudgetCodeData(args.data.Id);
        $scope.GetEmployeeData(args.data.Id);
        //$scope.GetCapitalGL(args.data.Id);
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
                    $scope.getData();
                    //ClearFields(response.data.Sequence);
                    //$scope.selectIDs();
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
        $scope.MaterialDataList = [];
        $scope.ExpenseGLList = [];
        $scope.InventoryGLList = [];
        $scope.InventoryCapitalGLList = [];
        $scope.CapitalGLList = [];
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
            data: { 'data': $scope.EmpCatNew, 'GlManagementId': $scope.GlManagementId},
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
    $scope.GetEmployeeCategory = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetMaterialData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
            $scope.EmpCatNew.EmployeeCategoryId = $scope.EmployeeCategoryList[0].EmployeeCategoryId;
        })
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

    $scope.popUpDataList = [];
    $scope.GetDesignationInformation = function () {
        try {
            $http({
                method: 'GET',
                url: 'Accounts/GLManagement/GetDesignationInformation'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#LDPopUp')).modal('show');
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

        var filtered = $("#desInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#desInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.OK = function () {
        try {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                if ($scope.popUpDataList[i].CheckBoxSelect == true) {
                    if (checkDoubleDesignationInformation($scope.DesignationList, $scope.popUpDataList[i].DesignationId) === false) {
                        $scope.DesignationList.push($scope.popUpDataList[i]);
                    }
                }
            }
            angular.element(document.querySelector('#LDPopUp')).modal('hide');
            $scope.SaveDesignation();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoubleDesignationInformation(list, DesignationId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DesignationId === DesignationId) {
                return true;
            }
        }
        return false;
    }

    $scope.designation = {
        Id: null,
        DesignationId: null,
        Designation: null
    };
    $scope.designationNew = angular.copy($scope.designation);


    $scope.SaveDesignation = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        $http({
            method: 'POST',
            url: $scope.saveDesignationUrl,
            data: { 'data': $scope.DesignationList, 'GlManagementId': $scope.GlManagementId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetDesignationData($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.DesignationList = [];
    $scope.GetDesignationData = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetDesignationData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DesignationList = response.data;
        })
    }
    $scope.closeDesignationCodePopUp = function () {
        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    }

    $scope.message_detailconfirmation = null;
    $scope.removeDesignation = function (obj) {
        $scope.designationNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.designationNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.designationNew.Designation + ' ]';
        angular.element(document.querySelector('#confirmDesignationDeletePopUp')).modal('show');
    }

    $scope.DeleteDesignation = function () {
        $http.get('Accounts/GLManagement/DeleteDesignationData?id=' + $scope.designationNew.DesignationId)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetDesignationData($scope.GlManagementId);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    //#endregion LegalDesignation

    //#region position

    $scope.selectPositionCode = function () {
        $scope.getPositionCode();
        angular.element(document.querySelector('#PositionCodePopUp')).modal('show');
    }

    $scope.PositionCodeList = [];
    $scope.getPositionCode = function () {
        $http({
            method: 'Get',
            url: $scope.path + 'GetPositionCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PositionCodeList = resp.data;
        });
    }

    $scope.closePositionCodePopUp = function () {
        angular.element(document.querySelector('#PositionCodePopUp')).modal('hide');
    }

    $scope.refreshTemplatePC = function (args) {
        $("#PCheadchk").ejCheckBox({ "change": CheckBoxSelectAllPC });
    };

    function CheckBoxSelectAllPC(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPositionCode").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PositionCodeList.length; i++) {
                $scope.PositionCodeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPositionCode").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.OKPositionCode = function () {
        try {
            for (var i = 0; i < $scope.PositionCodeList.length; i++) {
                if ($scope.PositionCodeList[i].CheckBoxSelect == true) {
                    if (checkDoublePositionCodeInformation($scope.PositionCodeListData, $scope.PositionCodeList[i].PositionCodeId) === false) {
                        $scope.PositionCodeListData.push($scope.PositionCodeList[i]);
                    }
                }
            }
            angular.element(document.querySelector('#PositionCodePopUp')).modal('hide');
            $scope.SavePositionCode();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoublePositionCodeInformation(list, PositionCodeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PositionCodeId === PositionCodeId) {
                return true;
            }
        }
        return false;
    }


    $scope.SavePositionCode = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        $http({
            method: 'POST',
            url: $scope.savePositionCodeUrl,
            data: { 'data': $scope.PositionCodeListData, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetPositionCodeData($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.PositionCodeListData = [];
    $scope.GetPositionCodeData = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetPositionCodeData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PositionCodeListData = response.data;
        })
    }
    $scope.message_PCconfirmation = null;
    $scope.removePositionCode = function (obj) {
        $scope.PCNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.PCNew.Code))
            $scope.message_PCconfirmation = 'Are you sure want to delete permanently [ ' + $scope.PCNew.Code + ' ]';
        angular.element(document.querySelector('#confirmPCDeletePopUp')).modal('show');
    }

    $scope.DeletePositionCode = function () {
        $http.get('Accounts/GLManagement/DeletePositionCodeData?id=' + $scope.PCNew.PositionCodeId)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetPositionCodeData($scope.GlManagementId);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    //#endregion position

    //#region BudgetCode
      
    $scope.BudgetCodepopUpDataList = [];
    $scope.GetBudgetCodeInformation = function () {
        try {
            $http({
                method: 'GET',
                url: 'employees/recruitment/getbudgetcodelist'
            }).then(function successCallback(response) {
                $scope.BudgetCodepopUpDataList = response.data.Rows;
            });
            angular.element(document.querySelector('#BudgetCodepopUpId')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.closeBudgetCodePopUp = function () {
        angular.element(document.querySelector('#BudgetCodepopUpId')).modal('hide');
    }

    $scope.OKBudgetCode = function () {
        try {
            var ob = {};
            for (var i = 0; i < $scope.BudgetCodepopUpDataList.length; i++) {
                if ($scope.BudgetCodepopUpDataList[i].CheckBoxSelect == true) {
                    if (checkDoubleBudgetCodeInformation($scope.BudgetCodeList, $scope.BudgetCodepopUpDataList[i].Id) === false) {
                        //$scope.BudgetCodeList.push($scope.BudgetCodepopUpDataList[i]);
                        ob.Id = null;
                        ob.BudgetCodeId = $scope.BudgetCodepopUpDataList[i].Id;
                        ob.Code = $scope.BudgetCodepopUpDataList[i].Code;
                        ob.Position = $scope.BudgetCodepopUpDataList[i].PositionName;
                        $scope.BudgetCodeList.push(ob);
                    }
                }
            }
            angular.element(document.querySelector('#BudgetCodepopUpId')).modal('hide');
            $scope.SaveBudgetCode();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoubleBudgetCodeInformation(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].BudgetCodeId === Id) {
                return true;
            }
        }
        return false;
    }

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
                $scope.GetBudgetCodeData($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.BudgetCodeList = [];
    $scope.GetBudgetCodeData = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetBudgetCodeData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetCodeList = response.data;
        })
    }

    $scope.message_BCconfirmation = null;
    $scope.removeBudgetCode = function (obj) {
        $scope.BCNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.BCNew.Id))
            $scope.message_BCconfirmation = 'Are you sure want to delete permanently [ ' + $scope.BCNew.Code + ' ]';
        angular.element(document.querySelector('#confirmBCDeletePopUp')).modal('show');
    }

    $scope.DeleteBudgetCode = function () {
        $http.get('Accounts/GLManagement/DeleteBudgetCodeData?id=' + $scope.BCNew.BudgetCodeId)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetBudgetCodeData($scope.GlManagementId);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
    };

    //#endregion BudgetCode

    //#region Employee 
    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'HumanResource/leaveApplicationNew/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.OKEmployee = function () {
        try {
            var ob = {};
            for (var i = 0; i < $scope.employee.length; i++) {
                if ($scope.employee[i].CheckBoxSelect == true) {
                    if (checkDoubleEmployeeInformation($scope.EmployeeList, $scope.employee[i].SystemID) === false) {
                        //$scope.EmployeeList.push($scope.employee[i]);
                        ob.Id = null;
                        ob.EmpSystemId = $scope.employee[i].SystemID;
                        ob.EmployeeCode = $scope.employee[i].Code;
                        ob.EmployeeName = $scope.employee[i].EmployeeName;
                        $scope.EmployeeList.push(ob);
                    }
                }
            }
            $scope.SaveEmployeeData();
            angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoubleEmployeeInformation(list, SystemID) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === SystemID) {
                return true;
            }
        }
        return false;
    }

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
                $scope.GetEmployeeData($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.EmployeeList = [];
    $scope.GetEmployeeData = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetEmployeeData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
        })
    }
    $scope.message_Empconfirmation = null;
    $scope.removeEmployee = function (obj) {
        $scope.EmpNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.EmpNew.EmpSystemId))
            $scope.message_Empconfirmation = 'Are you sure want to delete permanently [ ' + $scope.EmpNew.EmployeeName + ' ]';
        angular.element(document.querySelector('#confirmEmpDeletePopUp')).modal('show');
    }
    $scope.DeleteEmployee = function () {
        if (baseService.isUndefinedOrNull($scope.EmployeeList[0].EmpSystemId)) {
            if ($scope.EmployeeList[0].EmpSystemId === $scope.EmpSystemId) {
                $scope.EmployeeList.splice(0, 1);
            }
        }
        else {
            $http.get('Accounts/GLManagement/DeleteEmployeeData?id=' + $scope.EmpNew.EmpSystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetEmployeeData($scope.GlManagementId);
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    //#endregion Employee

    //#region ControlDrCr
    $rootScope.titleTab = 'Control Dr';
    $rootScope.titleTab = 'Control Cr';
    //#endregion ControlDrCr

    //#region Responsible Person

    $scope.ResPersonDataList = [];
    $scope.GetResponsiblePersonInformation = function () {
        try {
            $http({
                method: 'GET',
                url: 'employees/EmployeeInformation/GetEmployeeListByPlant'
            }).then(function successCallback(response) {
                $scope.ResPersonDataList = response.data;
            });
            angular.element(document.querySelector('#employeePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.closeRPPopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    }

    $scope.refreshTemplateRP = function (args) {
        $("#RPheadchk").ejCheckBox({ "change": CheckBoxSelectAllRP });
    };

    function CheckBoxSelectAllRP(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridResPer").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ResPersonDataList.length; i++) {
                $scope.ResPersonDataList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridResPer").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.OKResponsiblePerson = function () {
        try {
            for (var i = 0; i < $scope.ResPersonDataList.length; i++) {
                if ($scope.ResPersonDataList[i].CheckBoxSelect == true) {
                    if (checkDoubleResPerInformation($scope.ResPerList, $scope.ResPersonDataList[i].SystemID) === false) {
                        $scope.ResPerList.push($scope.ResPersonDataList[i]);
                    }
                }
            }
            angular.element(document.querySelector('#employeePopUp')).modal('hide');
            $scope.SaveResponsiblePersosnData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoubleResPerInformation(list, SystemID) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === SystemID) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveResponsiblePersosnData = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        $http({
            method: 'POST',
            url: $scope.saveRPUrl,
            data: { 'data': $scope.ResPerList, 'GlManagementId': $scope.GlManagementId },
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

    $scope.ResPerList = [];
    $scope.GetResponsiblePersonData = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + "GetResPersonData",
            data: { 'glManagementId': data },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ResPerList = response.data;
        })
    }
    //#endregion Responsible Person

    $scope.DeleteMaterial = function () {

        $http({
            method: 'POST',
            url: 'Accounts/GeneralAccountDeterminate/UpdateMaterial',
            data: { 'materialId': $scope.materialId, 'materialList': $scope.materialMasterDataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.selectIDs();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    }

    // #endregion ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//


    // #region ---------------------------------      Expense     -----------------------------------//

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

    $scope.setSelected = function (data) {

        if ($scope.tabType == 'consumableTab') {
            $scope.Type = "Consumable";

            if (checkConsumableExist($scope.ExpenseGLList, data) === false) {
                $scope.ExpenseGLList.push({
                    Id: null,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetName: data.BudgetName,
                    ActivityName: data.ActivityName,
                    ActivityId: data.ActivityId,
                    BudgetMasterActivityId: data.BudgetMasterActivityId,
                    Type: $scope.Type
                });
            }
            else {
                ShowResult(data.GLGeneralInfoName + " is already  Exist", "failure");
            }
        }

        else if ($scope.tabType == 'inventoryTab') {
            $scope.Type = "Inventory";
            if (checkConsumableExist($scope.InventoryGLList, data) === false) {
                $scope.InventoryGLList.push({
                    Id: null,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetName: data.BudgetName,
                    ActivityName: data.ActivityName,
                    ActivityId: data.ActivityId,
                    BudgetMasterActivityId: data.BudgetMasterActivityId,
                    Type: $scope.Type
                });
            }
            else {
                ShowResult(data.GLGeneralInfoName + " is already  Exist", "failure");
            }
        }
        else if ($scope.tabType == 'inventoryCapitalTab') {
            $scope.Type = "InventoryCapital";
            if (checkConsumableExist($scope.InventoryCapitalGLList, data) === false) {
                $scope.InventoryCapitalGLList.push({
                    Id: null,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetName: data.BudgetName,
                    ActivityName: data.ActivityName,
                    ActivityId: data.ActivityId,
                    BudgetMasterActivityId: data.BudgetMasterActivityId,
                    Type: $scope.Type
                });
            }
            else {
                ShowResult(data.GLGeneralInfoName + " is already  Exist", "failure");
            }
        }
        else {
            $scope.Type = "Capital";
            if (checkConsumableExist($scope.CapitalGLList, data) === false) {
                $scope.CapitalGLList.push({
                    Id: null,
                    GLGeneralInfoId: data.GLGeneralInfoId,
                    GLGeneralInfoName: data.GLGeneralInfoName,
                    BudgetMasterId: data.BudgetMasterId,
                    BudgetName: data.BudgetName,
                    ActivityName: data.ActivityName,
                    ActivityId: data.ActivityId,
                    BudgetMasterActivityId: data.BudgetMasterActivityId,
                    Type: $scope.Type
                });
            }
            else {
                ShowResult(data.GLGeneralInfoName + " is already  Exist", "failure");
            }
        }

        $scope.closeCOAICodeListPopUp();
    };

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

    $scope.tempIndex = [];
    $scope.RemoveIndex = [];
    $scope.RemoveExpense = function (data, index, removeRow) {
        $scope.tempIndex = index;
        $scope.RemoveIndex = removeRow;
        if (data.Id != null) {
            $scope.consumableId = data.Id;
        }
        else {
            $scope.consumableId = "";
        }
        if (baseService.isUndefinedOrNull(data.UserName))
            $scope.message_confirmation = 'Are you sure want to remove this data....';
        else
            $scope.message_confirmation = 'Are you sure want to remove ?';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.RemoveRow = function () {
        if ($scope.RemoveIndex == 'inventoryTabDel') {
            if (baseService.isUndefinedOrNull($scope.consumableId)) {
                $scope.InventoryGLList.splice($scope.tempIndex, 1);
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/GeneralAccountDeterminate/DeleteConsumerable',
                    data: { 'Id': $scope.consumableId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.InventoryGLList.splice($scope.tempIndex, 1);
            }

        }

        else if ($scope.RemoveIndex == 'consumableTabDel') {
            if (baseService.isUndefinedOrNull($scope.consumableId)) {
                $scope.ExpenseGLList.splice($scope.tempIndex, 1);
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/GeneralAccountDeterminate/DeleteConsumerable',
                    data: { 'Id': $scope.consumableId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.ExpenseGLList.splice($scope.tempIndex, 1);
            }

        }

        else if ($scope.RemoveIndex == 'inventoryCapitalTabDel') {
            if (baseService.isUndefinedOrNull($scope.consumableId)) {
                $scope.InventoryGLList.splice($scope.tempIndex, 1);
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/GeneralAccountDeterminate/DeleteConsumerable',
                    data: { 'Id': $scope.consumableId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.InventoryCapitalGLList.splice($scope.tempIndex, 1);
            }

        }

        else {
            if (baseService.isUndefinedOrNull($scope.consumableId)) {
                $scope.CapitalGLList.splice($scope.tempIndex, 1);
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Accounts/GeneralAccountDeterminate/DeleteConsumerable',
                    data: { 'Id': $scope.consumableId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.CapitalGLList.splice($scope.tempIndex, 1);
            }

        }

    }

    $scope.selectExpenseGL = function (data) {
        $scope.TabType = "Consumable";
        $http({
            method: 'POST',
            url: $scope.path + "GetConsumableData",
            data: { 'glControlDetailId': data, 'type': $scope.TabType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ExpenseGLList = response.data;
        })
    }

    $scope.InventoryGLList = [];
    $scope.selectInventoryGLBudget = function (data) {
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
            $scope.InventoryGLList = response.data;
        })
    }


    $scope.GetInventoryGL = function (data) {
        $scope.TabType = "Inventory";
        $http({
            method: 'POST',
            url: $scope.path + "GetConsumableData",
            data: { 'glControlDetailId': data, 'type': $scope.TabType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InventoryGLList = response.data;
        })
    }

    $scope.InventoryCapitalGLList = [];
    $scope.GetInventoryCapitalGL = function (data) {
        $scope.TabType = "InventoryCapital";
        $http({
            method: 'POST',
            url: $scope.path + "GetConsumableData",
            data: { 'glControlDetailId': data, 'type': $scope.TabType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InventoryCapitalGLList = response.data;
        })
    }

    $scope.CapitalGLList = [];
    $scope.GetCapitalGL = function (data) {
        $scope.TabType = "Capital";
        $http({
            method: 'POST',
            url: $scope.path + "GetConsumableData",
            data: { 'glControlDetailId': data, 'type': $scope.TabType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CapitalGLList = response.data;
        })
    }

    // #endregion --------------------------------- Inventory  -----------------------------------//

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