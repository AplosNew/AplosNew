'use strict';
EmployeeSalaryRuleSetupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeSalaryRuleSetupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Payroll Rule Setup';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/EmployeeSalaryRuleSetup/';
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
        $scope.GetItemAutoSequence();
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
        $scope.EmployeeCategoryList = [];
        $scope.ProcessParameterList = [];
        $scope.SelectedDesignationList = [];
    }

    $scope.ModelETTemp = {
        Id: null,
        EmployeeSalaryRuleSetupId: null,
        EmployeeTypeId: null
    };
    $scope.EmpCatNew = Object.assign({}, $scope.ModelETTemp);

    $scope.employeeTypeList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeTypeList = result;
    });

    $scope.SaveEmployeeType = function () {
        $scope.EmpCatNew.EmployeeSalaryRuleSetupId = $scope.ModelNew.Id;
        $http({
            method: 'POST',
            url: 'Payrolls/EmployeeSalaryRuleSetup/CreateSalaryRuleEmployeeType',
            data: { 'data': $scope.EmpCatNew, 'masterId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetEmployeeCategory();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    $scope.EmployeeCategoryList = [];
    $scope.GetEmployeeCategory = function () {
        $http({
            method: 'Get',
            url: "Payrolls/EmployeeSalaryRuleSetup/GetSalaryRuleEmployeeTypeData?masterId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
            $scope.GetSavedDesignation();
        })
    }


    $scope.message_detailconfirmation = null;
    $scope.removeEmployeeCategory = function (obj) {
        $scope.EmployeeCat = obj;
        if (!baseService.isUndefinedOrNull($scope.EmployeeCat.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.EmployeeCat.EmployeeCategory + ' ]';
        angular.element(document.querySelector('#confirmEmployeeCategoryPopUp')).modal('show');
    }

    $scope.DeleteEmployeeCategory = function () {
        $http({
            method: 'POST',
            url: 'Payrolls/EmployeeSalaryRuleSetup/DeleteEmployeeCategory?id=' + $scope.EmployeeCat.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetEmployeeCategory();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.message_dgconfirmation = null;
    $scope.RemoveDG = function (obj) {
        $scope.DG = obj.data;
        if (!baseService.isUndefinedOrNull($scope.DG.Id))
            $scope.message_dgconfirmation = 'Are you sure want to delete permanently [ ' + $scope.DG.UserName + ' ]';
        angular.element(document.querySelector('#confirmDGPopUp')).modal('show');
    }

    $scope.DeleteDG = function () {
        $http({
            method: 'POST',
            url: 'Payrolls/EmployeeSalaryRuleSetup/DeleteDesignation?id=' + $scope.DG.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedDesignation();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }
    $scope.sqlInStatement = "";
    $scope.idList = [];
    $scope.DesignationList = [];
    $scope.AddDesignation = function () {
        try {
            if (baseService.arrayLength($scope.EmployeeCategoryList) == 0) {
                throw "Select Employee Category first";
            }
            for (var di = 0; di < $scope.EmployeeCategoryList.length; di++) {
                $scope.idList.push($scope.EmployeeCategoryList[di]);
            }

            if ($scope.idList.length > 0) {
                var uniqueecId = removeDuplicates($scope.idList, 'EmployeeTypeId');
                var wcECId = "";
                if (uniqueecId.length > 0) {
                    wcECId = "IN(";
                    wcECId += Array.prototype.map.call(uniqueecId, function (item) { return "'" + item.EmployeeTypeId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcECId;
            }

            $http({
                method: 'Get',
                url: "Payrolls/EmployeeSalaryRuleSetup/GetDesignationData?ecId=" + $scope.sqlInStatement,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.DesignationList = response.data;
                for (var i = 0; i < $scope.SelectedDesignationList.length; i++) {
                    for (var j = 0; j < $scope.DesignationList.length; j++) {
                        if ($scope.DesignationList[j].Id == $scope.SelectedDesignationList[i].DesignationId) {
                            $scope.DesignationList.splice(j, 1);
                        }
                    }
                }
                $scope.ShowResultCustom();
            })
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ShowResultCustom = function (message, type) {
        $("#DesignationPoUp").ejDialog("setTitle", "Designation");
        var eDialog = $("#DesignationPoUp").data("ejDialog");
        eDialog.open();
        var gridObj = $("#GridDesignation").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering
    };

    // #region checkbox all

    $scope.refreshTemplateOperation = function (args) {
        $("#headchk").ejCheckBox({ "change": headCheckChangeOperation });
    };

    function headCheckChangeOperation(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridDesignation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DesignationList.length; i++) {
                $scope.DesignationList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDesignation").data("ejGrid");
        gridObj.refreshContent();
    };


    // #endregion


    $scope.SelectedDesignationList = [];
    $scope.CloseDesignation = function () {
        try {
            for (var i = 0; i < $scope.DesignationList.length; i++) {
                if ($scope.DesignationList[i].Flag == true) {
                    if (checkExists($scope.SelectedDesignationList, $scope.DesignationList[i].Id) === false) {
                        var ob = {};
                        ob.Id = null;
                        ob.EmployeeSalaryRuleSetupId = $scope.ModelNew.Id;
                        ob.DesignationId = $scope.DesignationList[i].Id;
                        ob.Sequence = $scope.DesignationList[i].Sequence;
                        ob.Code = $scope.DesignationList[i].Code;
                        ob.ShortName = $scope.DesignationList[i].ShortName;
                        ob.StandardName = $scope.DesignationList[i].StandardName;
                        ob.UserName = $scope.DesignationList[i].UserName;


                        $scope.SelectedDesignationList.push(ob);
                        ob = {};
                    }
                }
            }
            $scope.SaveDesignation();
            var eDialog = $("#DesignationPoUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DesignationId === id) {
                return true;
            }
        }
        return false;
    }


    $scope.GetSavedDesignation = function () {
        $http({
            method: 'Get',
            url: "Payrolls/EmployeeSalaryRuleSetup/GetSalaryRuleDesignationData?masterId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedDesignationList = response.data;
            $scope.GetProcessParameterData();
        })
    }
    $scope.SaveDesignation = function () {
        try {
            if (baseService.arrayLength($scope.SelectedDesignationList) < 0) {
                throw "Select Designation.";
            }

            $http({
                method: 'POST',
                url: 'Payrolls/EmployeeSalaryRuleSetup/CreateDesignation',
                data: { 'data': $scope.SelectedDesignationList, 'masterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedDesignation();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ModelProcessPara = { Id: null, EmployeeSalaryRuleSetupId: null, DrBudgetMasterActivityId: null, CrBudgetMasterActivityId: null, Sequence: 0, UserName: null, StandardName: null, Active: true, IsDefault: false, IsReportItem: false, ViewItem: null, DefaultValue: null, EntryState: 'Auto', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
    $scope.ModelProcessParaNew = Object.assign({}, $scope.ModelProcessPara);

    $scope.ModelProcessPara.FormulaDes = null;
    $scope.ModelProcessPara.FormulaDesID = null;
    $scope.ModelProcessPara.SalaryHeadFormula = null;
    $scope.ModelProcessPara.FormulaDescription = null;
    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];

    $scope.checkFormula = function (List, lastvalue) {
        var available = false;
        for (var i = 0; i < List.length; i++) {
            if (List[i].Text === lastvalue) {
                available = true;
                break;
            }
        }
        return available;
    }

    $scope.FormulaDetails = [];
    $scope.SetFormula = function (formula) {
        try {
            var formulaObj = {};

            if (formula === 'SHead') {

                formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                formulaObj.EmployeeSalaryRuleItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                formulaObj.EmployeeSalaryRuleItemHeadId = $scope.ModelProcessPara.HeadIdFormula;
                formulaObj.SalaryHead = $("#HeadFormula option:selected").text();
                formulaObj.Component = null;
                $scope.FormulaDetails.push(formulaObj);

                $scope.ModelProcessPara.FormulaDes = '';
                $scope.ModelProcessPara.FormulaDesID = '';

                $scope.ModelProcessPara.FormulaDescription = '';
                $scope.ModelProcessPara.FormulaIDDescription = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.FormulaDes)) {
                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId);
                    } else {
                        $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID = $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId;
                    }
                }

                $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.EmployeeSalaryRuleItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                        formulaObj.EmployeeSalaryRuleItemHeadId = null;
                        formulaObj.Component = $scope.ModelProcessPara.Operator;
                        formulaObj.SalaryHead = $scope.ModelProcessPara.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.ModelProcessPara.FormulaDes = '';
                        $scope.ModelProcessPara.FormulaDesID = '';

                        $scope.ModelProcessPara.FormulaDescription = '';
                        $scope.ModelProcessPara.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId);

                        }

                        $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                        $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                    }
                }
                else {
                    throw "First select Head or input value.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.EmployeeSalaryRuleItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                    formulaObj.EmployeeSalaryRuleItemHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelProcessPara.Precedence;
                    formulaObj.Component = $scope.ModelProcessPara.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.ModelProcessPara.FormulaDes = '';
                    $scope.ModelProcessPara.FormulaDesID = '';

                    $scope.ModelProcessPara.FormulaDescription = '';
                    $scope.ModelProcessPara.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId);

                    }

                    $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                    $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.EmployeeSalaryRuleItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                    formulaObj.EmployeeSalaryRuleItemHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelProcessPara.Value;
                    formulaObj.Component = $scope.ModelProcessPara.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.ModelProcessPara.FormulaDes = '';
                    $scope.ModelProcessPara.FormulaDesID = '';

                    $scope.ModelProcessPara.FormulaDescription = '';
                    $scope.ModelProcessPara.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId);

                    }

                    $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                    $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.RemoveFormula = function () {

        var maxseq = Math.max.apply(Math, $scope.FormulaDetails.map(function (o) { return o.Sequence; }))

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (maxseq === $scope.FormulaDetails[i].Sequence) {
                $scope.FormulaDetails.splice(i, 1);
                break;
            }
        }

        $scope.ModelProcessPara.FormulaDes = '';
        $scope.ModelProcessPara.FormulaDesID = '';

        $scope.ModelProcessPara.FormulaDescription = '';
        $scope.ModelProcessPara.FormulaIDDescription = '';

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.FormulaDes)) {
                $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId);
            } else {
                $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelProcessPara.FormulaDesID = ($scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId);
            }
        }

        $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
        $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

    }

    function CheckField(fieldValue, fieldName) {
        try {
            if (baseService.isUndefinedOrNull(fieldValue) || fieldValue === '') {
                throw ('[' + fieldName + '] is required...');
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.AddEditRow = function () {
        try {
            $scope.ModelProcessPara.FormulaDes = $scope.ModelProcessPara.FormulaDescription;
            $scope.ModelProcessPara.FormulaDesID = $scope.ModelProcessPara.FormulaIDDescription;

            $scope.ModelProcessPara.Formula = $scope.ModelProcessPara.FormulaDescription;
            $scope.ModelProcessPara.FormulaId = $scope.ModelProcessPara.FormulaIDDescription;

            $scope.ModelProcessPara.SalaryHead = $("#SH option:selected").text();


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ProcessParameterList = [];
    $scope.GetProcessParameterData = function () {
        $scope.ProcessParameterList = [];
        $http.get("Payrolls/EmployeeSalaryRuleSetup/GetProcessParameterList?masterId=" + $scope.masterId)
            .then(
                function successCallback(response) {
                    $scope.ProcessParameterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.setCheckedEntry = function (name) {
        if (name === 'Auto') {
            $scope.ModelProcessPara.EntryState = 'Auto';
            $scope.ModelProcessPara.Formula = null;
            $scope.ModelProcessPara.FormulaId = null;
            $scope.ModelProcessPara.FormulaDes = null;
            $scope.ModelProcessPara.FormulaDesID = null;
            $scope.ModelProcessPara.SalaryHeadFormula = null;
            $scope.ModelProcessPara.FormulaDescription = null;
            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        } else {
            $scope.ModelProcessPara.EntryState = 'Entry';
            $scope.ModelProcessPara.Formula = null;
            $scope.ModelProcessPara.FormulaId = null;
            $scope.ModelProcessPara.FormulaDes = null;
            $scope.ModelProcessPara.FormulaDesID = null;
            $scope.ModelProcessPara.SalaryHeadFormula = null;
            $scope.ModelProcessPara.FormulaDescription = null;
            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        }
    }


    $scope.ProductionAction = 'Save';
    $scope.ProcessParameterNewList = [];
    $scope.SaveProcessParameter = function () {
        try {
            $scope.ModelProcessPara.EmployeeSalaryRuleSetupId = $scope.masterId;
            CheckField($scope.ModelProcessPara.EmployeeSalaryRuleSetupId, "Master");
            CheckField($scope.ModelProcessPara.SalaryHeadID, "Salary Head");
            CheckField($scope.ModelProcessPara.UserName, "User Name");
            CheckField($scope.ModelProcessPara.StandardName, "Standard Name");
            $scope.AddEditRow();
            if (baseService.arrayLength($scope.ProcessParameterList) > 0) {

                $http({
                    method: 'POST',
                    url: 'Payrolls/EmployeeSalaryRuleSetup/CreateSalaryRuleItem',
                    data: { 'data': $scope.ModelProcessPara, 'details': $scope.FormulaDetails },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetItemAutoSequence();
                        $scope.GetProcessParameterData();
                        $scope.GetOrderLineCostingItemCbo();
                        $scope.ClearItem();
                        $scope.FormulaDetails = [];
                        $scope.Row = 'Add Row';
                        $scope.ModelProcessPara.FormulaDescription = null;
                        $scope.ModelProcessPara.FormulaIDDescription = null;

                        $scope.ModelProcessPara.HeadIdFormula = null;
                        $scope.ModelProcessPara.Operator = null;
                        $scope.ModelProcessPara.Precedence = null;
                        $scope.ModelProcessPara.Value = null;

                        $scope.FormulaArray = [];
                        $scope.FormulaIdArray = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else {
                var newobj = { Id: null, EmployeeSalaryRuleSetupId: null, DrBudgetMasterActivityId: null, CrBudgetMasterActivityId: null, Sequence: 0, UserName: null, StandardName: null, Active: 1, IsReportItem: 0, ViewItem: null, EntryState: null, FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, IsDefault: true }
                $scope.ProcessParameterNewList = [];
                for (var i = 1; i < 16; i++) {
                    var obj = angular.copy(newobj);
                    obj.Sequence = i;
                    //if (i == 1) {
                    //    obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                    //    obj.UserName = 'JoiningMonthEndDate';
                    //    obj.StandardName = 'Joining Month End Date';
                    //    obj.EntryState = 'Auto';
                    //    obj.IsDefault = true;
                    //}
                    //if (i == 2) {
                    //    obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                    //    obj.UserName = 'JoiningMonthDays';
                    //    obj.StandardName = 'Joining Month Days';
                    //    obj.EntryState = 'Auto';
                    //    obj.IsDefault = true;
                    //}
                    //if (i == 3) {
                    //    obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                    //    obj.UserName = 'JoiningMonthNoOfWeekOff';
                    //    obj.StandardName = 'Joining MonthNo Of WeekOff';
                    //    obj.EntryState = 'Auto';
                    //    obj.IsDefault = true;
                    //}
                    //if (i == 4) {
                    //    obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                    //    obj.UserName = 'DayStatusCount';
                    //    obj.StandardName = 'Day Status Count';
                    //    obj.EntryState = 'Auto';
                    //    obj.IsDefault = true;
                    //}

                    if (i == 1) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'MonthStartDate';
                        obj.StandardName = 'Month Start Date';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 2) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'MonthEndDate';
                        obj.StandardName = 'Month End Date';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 3) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'SalaryProcessingStartDate';
                        obj.StandardName = 'Salary Processing StartDate';
                        obj.EntryState = 'Calculate';
                        obj.IsDefault = true;
                    }
                    if (i == 4) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'SalaryProcessingEndDate';
                        obj.StandardName = 'Salary Processing EndDate';
                        obj.EntryState = 'Calculate';
                        obj.IsDefault = true;
                    }
                    if (i == 5) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'TotalWorkingDays';
                        obj.StandardName = 'Total Working Days';
                        obj.EntryState = 'Calculate';
                        obj.IsDefault = true;
                    }
                    if (i == 6) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'WeekOff';
                        obj.StandardName = 'Week Off';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 7) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'Leave';
                        obj.StandardName = 'Leave';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 8) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'HoliDay';
                        obj.StandardName = 'Holi Day';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 9) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'PayDay';
                        obj.StandardName = 'Pay Day';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 10) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'PresentDay';
                        obj.StandardName = 'Present Day';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 11) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'NetDay';
                        obj.StandardName = 'Net Day';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 12) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'NightShiftDays';
                        obj.StandardName = 'Night Shift Days';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 13) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'ShortDuration';
                        obj.StandardName = 'Short Duration';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 14) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'LateIN';
                        obj.StandardName = 'LateIN';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 15) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'EarlyOut';
                        obj.StandardName = 'Early Out';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    if (i == 16) {
                        obj.EmployeeSalaryRuleSetupId = $scope.masterId;
                        obj.UserName = 'HalfDuration';
                        obj.StandardName = 'Half Duration';
                        obj.EntryState = 'Auto';
                        obj.IsDefault = true;
                    }
                    $scope.ProcessParameterNewList.push(obj);
                }

                $http({
                    method: 'POST',
                    url: 'Payrolls/EmployeeSalaryRuleSetup/CreateSalaryRuleItemWithDefault',
                    data: { 'data': $scope.ModelProcessPara, 'details': $scope.FormulaDetails, 'Itemdetails': $scope.ProcessParameterNewList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetItemAutoSequence();
                        $scope.GetProcessParameterData();
                        $scope.GetOrderLineCostingItemCbo();
                        $scope.ClearItem();
                        $scope.FormulaDetails = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.ClearItem = function () {
        $scope.ModelProcessPara = { Id: null, EmployeeSalaryRuleSetupId: null, DrBudgetMasterActivityId: null, CrBudgetMasterActivityId: null, Sequence: 0, UserName: null, StandardName: null, Active: true, IsDefault: false, IsReportItem: false, ViewItem: null, DefaultValue: null, EntryState: 'Auto', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
        $scope.ModelProcessParaNew = Object.assign({}, $scope.ModelProcessPara);
        $scope.GetItemAutoSequence();
        $scope.ProductionAction = 'Save';
    }

    $scope.GetItemAutoSequence = function () {
        if (baseService.isUndefinedOrNull($scope.masterId)) {
            $scope.masterId = null;
        }
        $http.get("Payrolls/EmployeeSalaryRuleSetup/GetSalaryRuleItemAutoSequence?masterId=" + $scope.masterId)
            .then(
                function successCallback(response) {
                    $scope.ModelProcessPara.Sequence = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetItemAutoSequence();
    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }, { Text: "<=", Value: "<=" }, , { Text: ">=", Value: ">=" }];
    $scope.ItemList = [];
    $scope.GetOrderLineCostingItemCbo = function () {
        try {
            $http({
                method: 'GET',
                url: 'Payrolls/EmployeeSalaryRuleSetup/GetHeaderItemCbo?Id=' + $scope.ModelProcessPara.Id + '&masterId=' + $scope.masterId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.ItemList = response.data;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.message_PrductionParaconfirmation = "Are you sure want to delete permanently";
    $scope.removePrductionPara = function (obj) {

        $scope.PrductionPara = obj.data;
        if (!baseService.isUndefinedOrNull($scope.PrductionPara.Id))
            $scope.message_PrductionParaconfirmation = 'Are you sure want to delete permanently [ ' + $scope.PrductionPara.UserName + ' ]';
        angular.element(document.querySelector('#confirmDeleteProductionBookingParameterPopUp')).modal('show');
    }

    $scope.DeleteEmployeeSeperationItem = function () {
        $http({
            method: 'POST',
            url: 'Payrolls/EmployeeSalaryRuleSetup/DeleteEmployeeSalaryRuleItem?id=' + $scope.PrductionPara.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetProcessParameterData();
                $scope.GetOrderLineCostingItemCbo();
                $scope.GetItemAutoSequence();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.GetProcessPara = function (obj) {
        $scope.ModelProcessPara = { Id: null, EmployeeSalaryRuleSetupId: null, DrBudgetMasterActivityId: null, CrBudgetMasterActivityId: null, Sequence: 0, UserName: null, StandardName: null, Active: true, IsDefault: false, IsReportItem: false, ViewItem: null, DefaultValue: null, EntryState: 'Auto', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
        if (obj.data.IsDefault == false) {
            $scope.ProductionAction = 'Update';

            $scope.FormulaDetails = [];
            $scope.ModelProcessPara.HeadIdFormula = null;
            $scope.ModelProcessPara.Operator = null;
            $scope.ModelProcessPara.Precedence = null;
            $scope.ModelProcessPara.Value = null;

            $scope.objectData = obj.data;
            $scope.ModelProcessPara = Object.assign({}, $scope.objectData);
            if ($scope.ModelProcessPara.EntryState == "Calculate") {

                $http({
                    method: 'GET',
                    url: "Payrolls/EmployeeSalaryRuleSetup/GetDetailList?ItemId=" + $scope.ModelProcessPara.Id
                }).then(function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.FormulaDetails = response.data;

                        $scope.ModelProcessPara.FormulaDes = '';
                        $scope.ModelProcessPara.FormulaDesID = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.FormulaDes)) {
                                $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;

                                $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId);
                            } else {
                                $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                                $scope.ModelProcessPara.FormulaDesID = $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSalaryRuleItemHeadId;
                            }
                        }

                        $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                        $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                        $scope.ModelProcessPara.Formula = $scope.ModelProcessPara.FormulaDescription;
                        $scope.ModelProcessPara.FormulaId = $scope.ModelProcessPara.FormulaIDDescription;

                    }
                });
            }


            var value = null;

            $scope.GetOrderLineCostingItemCbo();
        }

    };

    $scope.SalaryHeadList =
        $scope.GetSalaryHeadCbo = function () {
            $http({
                method: 'Get',
                url: "Payrolls/EmployeeSalaryRuleSetup/GetCbo",
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.SalaryHeadList = response.data;
            })
        }
    $scope.GetSalaryHeadCbo();

    $scope.ControlCrListData = [];
    $scope.ControlDrListData = [];
    $scope.GetControlDrCrData = function (tab) {
        $scope.TabName = tab;
        $http({
            method: 'GET',
            url: 'Payrolls/EmployeeSalaryRuleSetup/getControlDrlist?tabName=' + $scope.TabName,
        }).then(function successCallback(response) {
            if ($scope.TabName == "ControlCr") {
                $scope.ControlCrListData = response.data;
                $("#CrGLPoUp").ejDialog("setTitle", "Cr GL");
                var eDialog = $("#CrGLPoUp").data("ejDialog");
                eDialog.open();
                var gridObj = $("#CrGLPoUp").data("ejGrid");
                gridObj.clearFiltering();  // clears all the filtering
            } else {
                $scope.ControlDrListData = response.data;
                $("#DrGLPoUp").ejDialog("setTitle", "Dr GL");
                var eDialog = $("#DrGLPoUp").data("ejDialog");
                eDialog.open();
                var gridObj = $("#DrGLPoUp").data("ejGrid");
                gridObj.clearFiltering();  // clears all the filtering
            }

        });
    }

    $scope.SelectDr = function (args) {
        $scope.ModelProcessPara.DrAccountGroupName = args.data.AccountGroupName;
        $scope.ModelProcessPara.DrActivityName = args.data.ActivityName;
        $scope.ModelProcessPara.DrBudgetMasterActivityId = args.data.BudgetMasterActivityId;
        var eDialog = $("#DrGLPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.SelectCr = function (args) {
        $scope.ModelProcessPara.CrActivityName = args.data.ActivityName;
        $scope.ModelProcessPara.CrAccountGroupName = args.data.AccountGroupName;
        $scope.ModelProcessPara.CrBudgetMasterActivityId = args.data.BudgetMasterActivityId;
        var eDialog = $("#CrGLPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.CloseDr = function () {
        var eDialog = $("#DrGLPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.CloseCr = function () {
        var eDialog = $("#CrGLPoUp").data("ejDialog");
        eDialog.close();
    }
}