'use strict';
AssignControlSetupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function AssignControlSetupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
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

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
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
        $scope.GetAssignControlBudgetCodeSetupData();
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
                    //  ClearFields(response.data.Sequence);
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
        $scope.BudgetCodeList = [];
        $scope.assignFor = null;
        $scope.CreationBudgetedEmployeeList = [];
        $scope.CheckingBudgetedEmployeeList = [];
        $scope.ApprovingBudgetedEmployeeList = [];
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

            $scope.popUpUrl = 'employees/recruitment/GetBudgetCodeList';

            $scope.popUpEmpDataList = [];
            $http({
                method: 'GET',
                url: $scope.popUpUrl

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data.Rows;
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
        if (e.model.checkassignFor === "check") {
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
                    if (checkDoubleGWS($scope.BudgetCodeList, $scope.popUpDataList[i].Id) === false) {
                        ob.Id = null;
                        ob.Activity = $scope.popUpDataList[i].Activity;
                        ob.BudgetId = $scope.popUpDataList[i].Id;
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

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    };

    $scope.callbackbuttoncancel = function () {
        $scope.closePopUp();
    };

    $scope.BCSave = function () {


        $http({
            method: 'POST',
            url: $scope.path + "CreateBudgetCode",
            data: {
                'data': $scope.BudgetCodeList
                , 'assignControlSetupId': $scope.ModelNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetAssignControlBudgetCodeSetupData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.GetAssignControlBudgetCodeSetupData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetAssignControlBudgetCodeSetupData?assignControlSetupId=" + $scope.ModelNew.Id
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
                $scope.GetAssignControlBudgetCodeSetupData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    //#endregion BudgetCode

    //#region
    // #region  Dynamic PopUp
    $scope.assignFor = null;
    $scope.BudgetedEmployeeList = [];
    $scope.popUpEmployee = function (assignFor) {
        try {
            $scope.assignFor = assignFor;
            $scope.BudgetedEmployeeList = [];
            $http({
                method: 'GET',
                url: 'Administration/AssignControlSetup/GetBudgetedEmployeeData?masterId=' + $scope.ModelNew.Id

            }).then(function successCallback(response) {
                $scope.BudgetedEmployeeList = response.data;
            });
            angular.element(document.querySelector('#popUpEmp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.refreshTemplateBE = function (args) {
        $("#headchkBE").ejCheckBox({ "change": CheckBoxSelectBE });
    };
    function CheckBoxSelectBE(e) {
        var ChkOrUnchk = false;
        if (e.model.checkassignFor === "check") {
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

    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#popUpEmp')).modal('hide');
    };

    // #endregion

    $scope.refreshTemplateRemove = function (args) {
        $("#headchkRemove").ejCheckBox({ "change": CheckBoxSelectRemove });
    };
    function CheckBoxSelectRemove(e) {
        var ChkOrUnchk = false;
        if (e.model.checkassignFor === "check") {
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

    //#endregion

    $scope.CreationBudgetedEmployeeList = [];
    $scope.CheckingBudgetedEmployeeList = [];
    $scope.ApprovingBudgetedEmployeeList = [];
    $scope.GetBudgetedEmployee = function (assignFor) {
        try {
            $scope.assignFor = assignFor
            $scope.BudgetedEmployeeList = [];
            $http({
                method: 'GET',
                url: 'Administration/AssignControlSetup/GetSavedBudgetedEmployeeData?AssignControlSetupId=' + $scope.ModelNew.Id + '&assignFor=' + $scope.assignFor

            }).then(function successCallback(response) {
                if ($scope.assignFor == 'Creation') {
                    $scope.CreationBudgetedEmployeeList = response.data;
                } else if ($scope.assignFor == 'Checking') {
                    $scope.CheckingBudgetedEmployeeList = response.data;
                } else {
                    $scope.ApprovingBudgetedEmployeeList = response.data;
                }
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
        if (e.model.checkassignFor === "check") {
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

    function CheckExistsEmployee(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId == Id) {
                return true;
            }
        }
        return false;
    }

    $scope.BESave = function () {
        try {
            var savebelist = [];
            for (var i = 0; i < $scope.BudgetedEmployeeList.length; i++) {
                if ($scope.BudgetedEmployeeList[i].BEFlag == true) {
                    if ($scope.assignFor == 'Creation') {
                        if ($scope.CreationBudgetedEmployeeList.length > 0) {
                            if (CheckExistsEmployee($scope.CreationBudgetedEmployeeList, $scope.BudgetedEmployeeList[i].EmployeeId) == false) {
                                $scope.BudgetedEmployeeList[i].AssignFor = $scope.assignFor;
                                savebelist.push($scope.BudgetedEmployeeList[i]);
                            }
                        } else {
                            $scope.BudgetedEmployeeList[i].AssignFor = $scope.assignFor;
                            savebelist.push($scope.BudgetedEmployeeList[i]);
                        }
                    } else if ($scope.assignFor == 'Checking') {
                        if ($scope.CheckingBudgetedEmployeeList.length > 0) {
                            if (CheckExistsEmployee($scope.CheckingBudgetedEmployeeList, $scope.BudgetedEmployeeList[i].EmployeeId) == false) {
                                $scope.BudgetedEmployeeList[i].AssignFor = $scope.assignFor;
                                savebelist.push($scope.BudgetedEmployeeList[i]);
                            }
                        }
                        else {
                            $scope.BudgetedEmployeeList[i].AssignFor = $scope.assignFor;
                            savebelist.push($scope.BudgetedEmployeeList[i]);
                        }
                    } else {
                        if ($scope.ApprovingBudgetedEmployeeList.length > 0) {
                            if (CheckExistsEmployee($scope.ApprovingBudgetedEmployeeList, $scope.BudgetedEmployeeList[i].EmployeeId) == false) {
                                $scope.BudgetedEmployeeList[i].AssignFor = $scope.assignFor;
                                savebelist.push($scope.BudgetedEmployeeList[i]);
                            }
                        }
                        else {
                            $scope.BudgetedEmployeeList[i].AssignFor = $scope.assignFor;
                            savebelist.push($scope.BudgetedEmployeeList[i]);
                        }
                    }
                }
            }
            if (savebelist.length > 0) {
                $http({
                    method: 'POST',
                    url: "Administration/AssignControlSetup/CreateBudgetedEmployee",
                    data: {
                        'data': savebelist
                        , 'assignControlSetupId': $scope.ModelNew.Id
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetBudgetedEmployee($scope.assignFor);
                        $scope.closeEmpPopUp();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.removeEmployee = function (data) {
        try {
            $scope.tempId = data.data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmAuthorityRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.DeleteEmployee = function () {
        $http({
            method: 'POST',
            url: 'Administration/AssignControlSetup/DeleteEmployee',
            data: { 'Id': $scope.tempId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetBudgetedEmployee($scope.assignFor);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

}