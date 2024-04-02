'use strict';
EmployeeSeperationSetupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeSeperationSetupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Seperation Setup';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/EmployeeSeperationSetup/';
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

    $scope.ModelETTemp = {
        Id: null,
        EmployeeSeperationSetupId: null,
        EmployeeTypeId: null
    };
    $scope.EmpCatNew = Object.assign({}, $scope.ModelETTemp);

    $scope.employeeTypeList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeTypeList = result;
    });

    $scope.SaveEmployeeType = function () {
        $scope.EmpCatNew.EmployeeSeperationSetupId = $scope.ModelNew.Id;
        $http({
            method: 'POST',
            url: 'Payrolls/EmployeeSeperationSetup/CreateEmpSeperationEmployeeType',
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
            url: "Payrolls/EmployeeSeperationSetup/GetEmpSeperationEmployeeTypeData?masterId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
            $scope.GetSavedDesignationGroup();
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
            url: 'Payrolls/EmployeeSeperationSetup/DeleteEmployeeCategory?id=' + $scope.EmployeeCat.Id
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

    $scope.DesignationGroupList = [];
    $scope.AddDesignationGroup = function () {
        $http({
            method: 'Get',
            url: "Payrolls/EmployeeSeperationSetup/GetDesignationGroupData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DesignationGroupList = response.data;
            $scope.ShowResultCustom();
        })
    }

    $scope.ShowResultCustom = function (message, type) {
        $("#DesignationGroupPoUp").ejDialog("setTitle", "DesignationGroup");
        var eDialog = $("#DesignationGroupPoUp").data("ejDialog");
        eDialog.open();
        var gridObj = $("#GridDesignationGroup").data("ejGrid");
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

        var filtered = $("#GridDesignationGroup").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DesignationGroupList.length; i++) {
                $scope.DesignationGroupList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDesignationGroup").data("ejGrid");
        gridObj.refreshContent();
    };


    // #endregion


    $scope.SelectedDesignationGroupList = [];
    $scope.CloseDesignationGroup = function () {
        try {
            for (var i = 0; i < $scope.DesignationGroupList.length; i++) {
                if ($scope.DesignationGroupList[i].Flag == true) {
                    if (checkExists($scope.SelectedDesignationGroupList, $scope.DesignationGroupList[i].Id) === false) {
                        var ob = {};
                        ob.Id = null;
                        ob.EmployeeSeperationSetupId = $scope.ModelNew.Id;
                        ob.DesignationGroupId = $scope.DesignationGroupList[i].Id;
                        ob.Sequence = $scope.DesignationGroupList[i].Sequence;
                        ob.Code = $scope.DesignationGroupList[i].Code;
                        ob.ShortName = $scope.DesignationGroupList[i].ShortName;
                        ob.StandardName = $scope.DesignationGroupList[i].StandardName;
                        ob.UserName = $scope.DesignationGroupList[i].UserName;


                        $scope.SelectedDesignationGroupList.push(ob);
                        ob = {};
                    }
                }
            }
            $scope.SaveDesignationGroup();
            var eDialog = $("#DesignationGroupPoUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DesignationGroupId === id) {
                return true;
            }
        }
        return false;
    }


    $scope.GetSavedDesignationGroup = function () {
        $http({
            method: 'Get',
            url: "Payrolls/EmployeeSeperationSetup/GetEmpSepDesignationGroupData?masterId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedDesignationGroupList = response.data;
            $scope.GetProcessParameterData();
        })
    }
    $scope.SaveDesignationGroup = function () {
        try {
            if (baseService.arrayLength($scope.SelectedDesignationGroupList) < 0) {
                throw "Select Designation Group.";
            }

            $http({
                method: 'POST',
                url: 'Payrolls/EmployeeSeperationSetup/CreateDesignationGroup',
                data: { 'data': $scope.SelectedDesignationGroupList, 'masterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedDesignationGroup();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ModelProcessPara = { Id: null, EmployeeSeperationSetupId: null, DrBudgetMasterActivityId: null, CrBudgetMasterActivityId: null, Sequence: 0, UserName: null, SandardName: null, Active: true, IsReportItem: false, ViewItem: null, DefaultValue: null, EntryState: 'Auto', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
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
                formulaObj.EmployeeSeperationItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                formulaObj.EmployeeSeperationItemHeadId = $scope.ModelProcessPara.HeadIdFormula;
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
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSeperationItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSeperationItemHeadId);
                    } else {
                        $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID = $scope.FormulaDetails[i].EmployeeSeperationItemHeadId;
                    }
                }

                $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.EmployeeSeperationItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                        formulaObj.EmployeeSeperationItemHeadId = null;
                        formulaObj.Component = $scope.ModelProcessPara.Operator;
                        formulaObj.SalaryHead = $scope.ModelProcessPara.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.ModelProcessPara.FormulaDes = '';
                        $scope.ModelProcessPara.FormulaDesID = '';

                        $scope.ModelProcessPara.FormulaDescription = '';
                        $scope.ModelProcessPara.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSeperationItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSeperationItemHeadId);

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
                    formulaObj.EmployeeSeperationItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                    formulaObj.EmployeeSeperationItemHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelProcessPara.Precedence;
                    formulaObj.Component = $scope.ModelProcessPara.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.ModelProcessPara.FormulaDes = '';
                    $scope.ModelProcessPara.FormulaDesID = '';

                    $scope.ModelProcessPara.FormulaDescription = '';
                    $scope.ModelProcessPara.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSeperationItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSeperationItemHeadId);

                    }

                    $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                    $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.EmployeeSeperationItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                    formulaObj.EmployeeSeperationItemHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelProcessPara.Value;
                    formulaObj.Component = $scope.ModelProcessPara.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.ModelProcessPara.FormulaDes = '';
                    $scope.ModelProcessPara.FormulaDesID = '';

                    $scope.ModelProcessPara.FormulaDescription = '';
                    $scope.ModelProcessPara.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSeperationItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSeperationItemHeadId);

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
                $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSeperationItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSeperationItemHeadId);
            } else {
                $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelProcessPara.FormulaDesID = ($scope.FormulaDetails[i].EmployeeSeperationItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSeperationItemHeadId);
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

            $scope.Row = 'Add Row';
            $scope.ModelProcessPara.FormulaDescription = null;
            $scope.ModelProcessPara.FormulaIDDescription = null;

            $scope.ModelProcessPara.HeadIdFormula = null;
            $scope.ModelProcessPara.Operator = null;
            $scope.ModelProcessPara.Precedence = null;
            $scope.ModelProcessPara.Value = null;

            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ProcessParameterList = [];
    $scope.GetProcessParameterData = function () {
        $scope.ProcessParameterList = [];
        $http.get("Payrolls/EmployeeSeperationSetup/GetProcessParameterList?masterId=" + $scope.masterId)
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
        }
    }


    $scope.ProductionAction = 'Save';
    $scope.SaveProcessParameter = function () {
        try {
            $scope.ModelProcessPara.EmployeeSeperationSetupId = $scope.masterId;
            CheckField($scope.ModelProcessPara.EmployeeSeperationSetupId, "Master");
            CheckField($scope.ModelProcessPara.UserName, "User Name");
            CheckField($scope.ModelProcessPara.SandardName, "Sandard Name");
            $scope.AddEditRow();

            $http({
                method: 'POST',
                url: 'Payrolls/EmployeeSeperationSetup/CreateSeperationItem',
                data: { 'data': $scope.ModelProcessPara, 'details': $scope.FormulaDetails },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSeperationItemAutoSequence();
                    $scope.GetProcessParameterData();
                    $scope.GetOrderLineCostingItemCbo();
                    $scope.ClearSeperationItem();
                    $scope.FormulaDetails = [];
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.ClearSeperationItem = function () {
        $scope.ModelProcessPara = { Id: null, EmployeeSeperationSetupId: null, DrBudgetMasterActivityId: null, CrBudgetMasterActivityId: null, Sequence: 0, UserName: null, SandardName: null, Active: true, IsReportItem: false, ViewItem: null, DefaultValue: null, EntryState: 'Auto', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
        $scope.ModelProcessParaNew = Object.assign({}, $scope.ModelProcessPara);
        $scope.GetSeperationItemAutoSequence();
    }

    $scope.GetSeperationItemAutoSequence = function () {
        if (baseService.isUndefinedOrNull($scope.masterId)) {
            $scope.masterId = null;
        }
        $http.get("Payrolls/EmployeeSeperationSetup/GetSeperationItemAutoSequence?masterId=" + $scope.masterId)
            .then(
                function successCallback(response) {
                    $scope.ModelProcessPara.Sequence = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetSeperationItemAutoSequence();
    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];
    $scope.ItemList = [];
    $scope.GetOrderLineCostingItemCbo = function () {
        try {
            $http({
                method: 'GET',
                url: 'Payrolls/EmployeeSeperationSetup/GetHeaderItemCbo?Id=' + $scope.ModelProcessPara.Id + '&masterId=' + $scope.masterId,
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
            url: 'Payrolls/EmployeeSeperationSetup/DeleteEmployeeSeperationItem?id=' + $scope.PrductionPara.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetProcessParameterData();
                $scope.GetOrderLineCostingItemCbo();
                $scope.GetSeperationItemAutoSequence();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.GetProcessPara = function (obj) {
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
                url: "Payrolls/EmployeeSeperationSetup/GetDetailList?ItemId=" + $scope.ModelProcessPara.Id
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.FormulaDetails = response.data;

                    $scope.ModelProcessPara.FormulaDes = '';
                    $scope.ModelProcessPara.FormulaDesID = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.FormulaDes)) {
                            $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;

                            $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].EmployeeSeperationItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSeperationItemHeadId);
                        } else {
                            $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                            $scope.ModelProcessPara.FormulaDesID = $scope.FormulaDetails[i].EmployeeSeperationItemHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].EmployeeSeperationItemHeadId;
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

    };

    $scope.ControlCrListData = [];
    $scope.ControlDrListData = [];
    $scope.GetControlDrCrData = function (tab) {
        $scope.TabName = tab;
        $http({
            method: 'GET',
            url: 'Payrolls/EmployeeSeperationSetup/getControlDrlist?tabName=' + $scope.TabName,
        }).then(function successCallback(response) {
            if ($scope.TabName == "ControlCr") {
                $scope.ControlCrListData = response.data;
                $("#CrGLPoUp").ejDialog("setTitle", "Cr GL");
                var eDialog = $("#CrGLPoUp").data("ejDialog");
                eDialog.open();
                var gridObj = $("#c").data("ejGrid");
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
        $scope.ModelProcessPara.DrBudgetMasterActivityId = args.data.BudgetMasterActivityId;
        var eDialog = $("#DrGLPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.SelectCr = function (args) {
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