'use strict';
AssignControlSetupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AssignControlSetupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Assign Control Setup';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Administration/AssignControlSetup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.AssignTypeList = [
        { Value: "Advance", Text: "Advance" },
        { Value: "Leave", Text: "Leave" }
    ];


    $scope.AssignForList = [
        { Value: "Creation", Text: "Creation" },
        { Value: "Checking", Text: "Checking" },
        { Value: "Approving", Text: "Approving" }
    ];

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
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

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.masterId = $scope.ModelNew.Id;
        $scope.GetEmployeeCategory();
        $scope.GetSeperationItemAutoSequence();
        $scope.GetOrderLineCostingItemCbo();
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
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
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

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }
    $scope.popUpDataList = [];
    $scope.popUpBudgetCode = function () {
        try {
            var entityCode = "";
            if ($scope.selectedEntityList.length > 0) {
                var uniqueEntityId = removeDuplicates($scope.selectedEntityList, 'EntityId');
                var entityCode = "";
                if (uniqueEntityId.length > 0) {
                    entityCode = "IN(";
                    entityCode += Array.prototype.map.call(uniqueEntityId, function (item) { return "'" + item.EntityId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = entityCode;
            }
            $scope.popUpUrl = 'employees/recruitment/GetManpowerBudgetListByEntitySql?entityids=' + $scope.sqlInStatement;

            $scope.popUpEmpDataList = [];
            $http({
                method: 'GET',
                url: $scope.popUpUrl

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
                for (var j = 0; j < $scope.BudgetCodeList.length; j++) {
                    for (var i = 0; i < $scope.popUpDataList.length; i++) {
                        if ($scope.BudgetCodeList[j].BudgetId == $scope.popUpDataList[i].Id) {
                            $scope.popUpDataList.splice(i, 1);
                        }
                    }
                }
            });
            angular.element(document.querySelector('#popUpId')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.BudgetCodeList = [];
    $scope.popUpDataList = [];

    $scope.refreshTemplate = function (args) {
        $("#headchkGWS").ejCheckBox({ "change": CheckBoxSelectGWS });
    };
    function CheckBoxSelectGWS(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridpopUpId").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridpopUpId").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.selectDoubleClick = function () {
        try {
            var ob = {};
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                if ($scope.popUpDataList[i].isSelected == true) {
                    if (checkDoubleGWS($scope.BudgetCodeList, $scope.popUpDataList[i].BudgetId) === false) {
                        ob.Id = null;
                        ob.Activity = $scope.popUpDataList[i].Activity;
                        ob.BudgetId = $scope.popUpDataList[i].BudgetId;
                        ob.Code = $scope.popUpDataList[i].Code;
                        ob.Department = $scope.popUpDataList[i].Department;
                        ob.DepartmentId = $scope.popUpDataList[i].DepartmentId;
                        ob.Deployment = $scope.popUpDataList[i].Deployment;
                        ob.Designation = $scope.popUpDataList[i].Designation;
                        ob.DesignationId = $scope.popUpDataList[i].DesignationId;
                        ob.Division = $scope.popUpDataList[i].Division;
                        ob.DivisionId = $scope.popUpDataList[i].DivisionId;
                        ob.EmployeeType = $scope.popUpDataList[i].EmployeeType;
                        ob.EntityId = $scope.popUpDataList[i].EntityId;
                        ob.EntityCode = $scope.popUpDataList[i].EntityCode;
                        ob.EntityName = $scope.popUpDataList[i].EntityName;
                        ob.Flag = $scope.popUpDataList[i].Flag;
                        ob.IsDirect = $scope.popUpDataList[i].IsDirect;
                        ob.IsOTEntitled = $scope.popUpDataList[i].IsOTEntitled;
                        ob.Line = $scope.popUpDataList[i].Line;
                        ob.LineId = $scope.popUpDataList[i].LineId;
                        ob.PayrollGroupId = $scope.popUpDataList[i].PayrollGroupId;
                        ob.Plant = $scope.popUpDataList[i].Plant;
                        ob.PlantId = $scope.popUpDataList[i].PlantId;
                        ob.PositionCode = $scope.popUpDataList[i].PositionCode;
                        ob.PositionId = $scope.popUpDataList[i].PositionId;
                        ob.PositionName = $scope.popUpDataList[i].PositionName;
                        ob.Section = $scope.popUpDataList[i].Section;
                        ob.SectionId = $scope.popUpDataList[i].SectionId;
                        ob.ShiftDefination = $scope.popUpDataList[i].ShiftDefination;
                        ob.ShiftDefinationId = $scope.popUpDataList[i].ShiftDefinationId;
                        ob.SubDivision = $scope.popUpDataList[i].SubDivision;
                        ob.SubDivisionId = $scope.popUpDataList[i].SubDivisionId;
                        ob.SubSection = $scope.popUpDataList[i].SubSection;
                        ob.SubSectionId = $scope.popUpDataList[i].SubSectionId;
                        ob.Unit = $scope.popUpDataList[i].Unit;
                        ob.UnitId = $scope.popUpDataList[i].UnitId;
                        ob.UserGroup = $scope.popUpDataList[i].UserGroup;
                        ob.WorkGroupId = $scope.popUpDataList[i].WorkGroupId;
                        ob.DeployedManpower = $scope.popUpDataList[i].DeployedManpower;
                        ob.BudgetedManpower = $scope.popUpDataList[i].BudgetedManpower;
                        ob.IsGoodWorkApplicable = false;
                        ob.IsCompensatoryApplicable = false;
                        ob.IsEmployeeApplicable = false;
                        ob.GoodWorkCategory = null;
                        $scope.BudgetCodeList.push(ob);
                        ob = {};
                    }
                }
            }
            angular.element(document.querySelector('#popUpId')).modal('hide');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoubleGWS(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].BudgetId === Id) {
                return true;
            }
        }
        return false;
    }


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
                    if (response.data[0].TotalNumber < response.data[0].OnRollManPwr || response.data[0].TotalNumber == response.data[0].OnRollManPwr) {
                        ShowResult("On Roll Manpower is exceeding Budgeted Manpower.", 'failure', 'popUpId');;
                    }
                    else {
                        angular.element(document.querySelector('#popUpId')).modal('hide');
                    }
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    };

    $scope.callbackbuttoncancel = function () {
        $scope.closePopUp();
    };

    $scope.BCSave = function () {

        for (var i = 0; i < $scope.BudgetCodeList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.BudgetCodeList[i].GoodWorkCategory) || $scope.BudgetCodeList[i].GoodWorkCategory === 0) {
                ShowResult('Good Work Category can not be blank...');
                return false;
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + "CreateBudgetCode",
            data: {
                'data': $scope.BudgetCodeList
                , 'goodWorkSetupId': $scope.ModelNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetGoodWorkBudgetCodeData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.GetGoodWorkBudgetCodeData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetGoodWorkBudgetCodeSetupData?goodWorkSetupId=" + $scope.ModelNew.Id
        }).then(function (response) {
            $scope.BudgetCodeList = response.data;
            $scope.GetBudgetedEmployee();
        });
    }

    $scope.removeBudgetCode = function (tempId) {
        try {
            $scope.tempId = tempId.data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmBudgetCodeRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeBudgetCodeRow = function () {
        $http({
            method: 'POST',
            url: 'Attendances/GoodWorkSetup/BudgetCodeDelete',
            data: { 'Id': $scope.tempId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetGoodWorkBudgetCodeData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    //#endregion BudgetCode

    //#region
    // #region  Dynamic PopUp

    $scope.popUpList = [];
    $scope.employeeInformation = {
        PlantId: $window.plantId
        , EmployeeCode: null
        , EmployeeName: null
        , SystemId: null

    };
    $scope.popUpEmpDataList = [];
    $scope.popUpEmployee = function (obj) {
        try {
            $scope.tabName = obj;
            $scope.popUpEmpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpEmpDataList = response.data;
            });
            angular.element(document.querySelector('#popUpEmp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CheckByList = [];
    $scope.authorizationList = [];
    $scope.selectdblClick = function (data) {
        try {
            var data = data.data;
            var ob = {};

            if ($scope.tabName == 'Authority') {
                if (checkDoubleAuthorityCode($scope.CheckByList, data.EmployeeCode) === false) {
                    if (checkDoubleAuthorityCode($scope.authorizationList, data.EmployeeCode) === false) {
                        ob.AuthorityId = data.SystemId;
                        ob.EmployeeName = data.EmployeeName;
                        ob.EmployeeCode = data.EmployeeCode;
                        ob.CompanyId = data.CompanyId;
                        ob.Company = data.Company;
                        ob.Plant = data.Plant;
                        ob.Designation = data.LegalDesignation;
                        ob.Department = data.Department;
                        ob.Section = data.Section;
                        ob.SubSection = data.SubSection;
                        ob.Line = data.Line;

                        $scope.authorizationList.push(ob);
                        $scope.AuthoritySave();
                        ob = {};
                    }
                    angular.element(document.querySelector('#popUpEmp')).modal('hide');
                }
                else {
                    throw "This Employee already added in Checked By List"
                }
            }
            else {
                if (checkDoubleAuthorityCode($scope.authorizationList, data.EmployeeCode) === false) {
                    if (checkDoubleAuthorityCode($scope.CheckByList, data.EmployeeCode) === false) {
                        ob.CheckById = data.SystemId;
                        ob.EmployeeName = data.EmployeeName;
                        ob.EmployeeCode = data.EmployeeCode;
                        ob.CompanyId = data.CompanyId;
                        ob.Company = data.Company;
                        ob.Plant = data.Plant;
                        ob.Designation = data.LegalDesignation;
                        ob.Department = data.Department;
                        ob.Section = data.Section;
                        ob.SubSection = data.SubSection;
                        ob.Line = data.Line;

                        $scope.CheckByList.push(ob);
                        $scope.CheckBySave();
                        ob = {};
                    }
                    angular.element(document.querySelector('#popUpEmp')).modal('hide');
                }
                else {
                    throw "This Employee already added in Approved By List"
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    function checkDoubleAuthorityCode(list, EmployeeCode) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === EmployeeCode) {
                return true;
            }
        }
        return false;
    }

    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#popUpEmp')).modal('hide');
    };
    // #endregion

    $scope.AuthoritySave = function () {
        $http({
            method: 'POST',
            url: $scope.path + "CreateAuthority",
            data: {
                'data': $scope.authorizationList
                , 'goodWorkSetupId': $scope.ModelNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.GetGoodWorkAuthorityData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.GetGoodWorkAuthorityData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetGoodWorkAuthorityData?goodWorkSetupId=" + $scope.ModelNew.Id
        }).then(function (response) {
            $scope.authorizationList = response.data;
        });
    }
    $scope.removeAuthority = function (tempId) {
        try {
            $scope.tempId = tempId.data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmAuthorityRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeAuthorityRow = function () {
        $http({
            method: 'POST',
            url: 'Attendances/GoodWorkSetup/AuthorityDelete',
            data: { 'Id': $scope.tempId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetGoodWorkAuthorityData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.CheckBySave = function () {
        $http({
            method: 'POST',
            url: $scope.path + "CreateCheckBy",
            data: { 'data': $scope.CheckByList, 'goodWorkSetupId': $scope.ModelNew.Id },
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
    };

    $scope.GetCheckByData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetCheckByData?goodWorkSetupId=" + $scope.ModelNew.Id
        }).then(function (response) {
            $scope.CheckByList = response.data;
        });
    }
    $scope.removeCheckBy = function (tempId) {
        try {
            $scope.tempId = tempId.data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmCheckByRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeCheckByRow = function () {
        $http({
            method: 'POST',
            url: 'Attendances/GoodWorkSetup/CheckByDelete',
            data: { 'Id': $scope.tempId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetCheckByData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.refreshTemplateRemove = function (args) {
        $("#headchkRemove").ejCheckBox({ "change": CheckBoxSelectRemove });
    };
    function CheckBoxSelectRemove(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridBC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.BudgetCodeList.length; i++) {
                $scope.BudgetCodeList[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridBC").data("ejGrid");
        gridObj.refreshContent();
    };


    var getString = function (data) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i]) == false) {
                string += ",'" + data[i] + "'";
                collection.push(data[i]);
            }
        }
        return string;
    }

    $scope.dataList = [];
    $scope.removeBudgetCode = function () {
        try {
            $scope.NewBudgetCodeIds = [];
            //var dataList = [];
            var g = $("#GridBC").data("ejGrid");
            $scope.dataList = g.getFilteredRecords();
            if ($scope.dataList.length == 0) {
                $scope.dataList = $scope.BudgetCodeList;
            }
            for (var i = 0; i < $scope.dataList.length; i++) {
                if ($scope.dataList[i].isSelected == true) {
                    $scope.NewBudgetCodeIds.push($scope.dataList[i].BudgetId);
                }
            }
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmBudgetCodeRemovePopUps')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeBudgetCodeRows = function () {
        var deletedIds = getString($scope.NewBudgetCodeIds);
        $http({
            method: 'POST',
            url: 'Attendances/GoodWorkSetup/BudgetCodeDelete',
            data: { 'Id': deletedIds },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                for (var i = 0; i < $scope.BudgetCodeList.length; i++) {
                    for (var j = 0; j < $scope.NewBudgetCodeIds.length; j++) {
                        if ($scope.BudgetCodeList[i].BudgetId == $scope.NewBudgetCodeIds[j]) {
                            $scope.BudgetCodeList.splice(i, 1);
                        }
                    }

                }
                var gridObj = $("#GridBC").data("ejGrid");
                gridObj.refreshContent();
                gridObj.refreshTemplate();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.GWCategory = null;
    $scope.SetGoodWorkCategory = function () {
        for (var i = 0; i < $scope.BudgetCodeList.length; i++) {
            $scope.BudgetCodeList[i].GoodWorkCategory = $scope.GWCategory;
        }
    }

    //#endregion

    $scope.BudgetedEmployeeList = [];
    $scope.GetBudgetedEmployee = function () {
        try {
            $scope.BudgetedEmployeeList = [];
            $http({
                method: 'GET',
                url: 'Attendances/GoodWorkSetup/GetBudgetedEmployeeData?gwsId=' + $scope.ModelNew.Id

            }).then(function successCallback(response) {
                $scope.BudgetedEmployeeList = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.refreshTemplateBE = function (args) {
        $("#headchkBE").ejCheckBox({ "change": CheckBoxSelectBE });
    };
    function CheckBoxSelectBE(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridBE").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.BudgetedEmployeeList.length; i++) {
                $scope.BudgetedEmployeeList[i].BEFlag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].BEFlag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridBE").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.BESave = function () {
        var savebelist = [];
        for (var i = 0; i < $scope.BudgetedEmployeeList.length; i++) {
            if ($scope.BudgetedEmployeeList[i].BEFlag == false || !baseService.isUndefinedOrNull($scope.BudgetedEmployeeList[i].Id)) {
                savebelist.push($scope.BudgetedEmployeeList[i]);
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + "CreateExceptionBudgetedEmployee",
            data: {
                'data': savebelist
                , 'goodWorkSetupId': $scope.ModelNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetBudgetedEmployee();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
}