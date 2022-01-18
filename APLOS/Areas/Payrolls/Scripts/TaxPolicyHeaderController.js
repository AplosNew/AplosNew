'use strict';
TaxPolicyHeaderController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function TaxPolicyHeaderController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Tax Policy Master';
    $scope.Action = 'Save';
    $scope.path = 'Payrolls/TaxPolicyHeader/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

  
    // The Tab Switching Code    

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
       
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };  
     
     
    $scope.masterList = [];
    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getMaster',
        }).then(function succ( resp ){
            $scope.masterList = [];
            $scope.masterList = resp.data;
        });
    }
    $scope.getMaster();

    $scope.getMasterDetails = function (e) {
        $scope.Master = e.data;
        $http({
            method: 'POST',
            url: $scope.path + 'getChildData',
            data: {'MasterId': $scope.Master.Id}
        }).then(function success(resp){

            $scope.Action = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        })
    }

    var j = document.getElementById("tab_show");
    j.style.display = "none";

    function showTabs() {
        if ($scope.Header.Id != null) {
            j.style.display = "block";
        }
        else {
            j.style.display = "none";
        }
    }   

    // Double Click the Main Header Grid
    $scope.getHeaderDetails = function (e) {
        $scope.Header = e.data;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }       
        $scope.ClearEarningMaster();
        $scope.EarningMasterModel.TaxPolicyHeaderId = e.data.Id;
        $scope.Child.HeaderId = e.data.Id;
        $scope.GetEarningMasterList();
        updateChild();
        showTabs();
        
    }


    // #region Header Operations

    $scope.Header = {
        Id: null,
        ShortName:null,
        StandardName:null,
        UserName: null,
        Sequence: 0,
        Remarks: null,
        Active:false,
    };

    $scope.HeaderList = [];

    // Operations to Get the Header
    $scope.getHeader = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getHeader',
        }).then(function succ(resp) {
            $scope.HeaderList = [];
            $scope.HeaderList = resp.data;
        });

    }
    $scope.getHeader();


    //Saving The Header
    $scope.saveHeader = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.HeaderForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveHeader',
                data: { 'Header': $scope.Header }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Header = response.data.Data;
                    $scope.EarningMasterModel.HeaderId = response.data.Data.Id;
                    $scope.Child.HeaderId = response.data.Data.Id;
                    $scope.getHeader();
                    showTabs();                   
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    //Getting the Header Sequence
    $scope.GetSequenceHeader = function () {
        cboService.getSequence($scope.path +'GetAutoSequenceHeader', function (data) {
            $scope.Header.Sequence = data;
        });
    };

    $scope.GetSequenceHeader();

    //Clearing the Whole Header
    $scope.clearHeader = function () {
        $scope.Header = {
            Id: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Sequence: 0,
            Remarks: null,
            Active: false,
        };
        $scope.GetSequenceHeader();
        showTabs();

    }

    // #endregion

    // #region TaxExemption Applicable Logic Functions

    // #region Modal Region

    $scope.TaxExemptionFormula = {
        Id: null,
        TaxEarningMasterChildId: null,
        Formula: null,
        FormulaID: null,
        Description: null       
    }

    $scope.FormulaChildModel = {
        TaxEarningMasterChildId: null,
        FormulaDes: null,
        FormulaDesID: null,
    }
    $scope.FormulaChildModel.SalaryHeadFormula = null;
    $scope.FormulaChildModel.FormulaDescription = null;

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

    $scope.FormulaDetails = [];

    // #endregion

    // #region Formula Configuration Functions

    $scope.SetFormula = function (formula) {
        try {
            var formulaObj = {};

            if (formula === 'SHead') {

                formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                formulaObj.TaxEarningMasterChildId = $scope.TaxExemptionFormula.Id == null ? null : $scope.TaxExemptionFormula.Id;
                formulaObj.SalaryHeadID = $scope.FormulaChildModel.SalaryHeadIdFormula;
                formulaObj.SalaryHead = $("#SalaryHeadFormula option:selected").text();
                formulaObj.Component = null;
                $scope.FormulaDetails.push(formulaObj);

                $scope.FormulaChildModel.FormulaDes = '';
                $scope.FormulaChildModel.FormulaDesID = '';

                $scope.FormulaChildModel.FormulaDescription = '';
                $scope.FormulaChildModel.FormulaIDDescription = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.FormulaChildModel.FormulaDes)) {
                        $scope.FormulaChildModel.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.FormulaChildModel.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                    } else {
                        $scope.FormulaChildModel.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.FormulaChildModel.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
                    }
                }

                $scope.FormulaChildModel.FormulaDescription = $scope.FormulaChildModel.FormulaDes;
                $scope.FormulaChildModel.FormulaIDDescription = $scope.FormulaChildModel.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.FormulaChildModel.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.TaxEarningMasterChildId = $scope.TaxExemptionFormula.Id == null ? null : $scope.TaxExemptionFormula.Id;
                        formulaObj.SalaryHeadID = null;
                        formulaObj.Component = $scope.FormulaChildModel.Operator;
                        formulaObj.SalaryHead = $scope.FormulaChildModel.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.FormulaChildModel.FormulaDes = '';
                        $scope.FormulaChildModel.FormulaDesID = '';

                        $scope.FormulaChildModel.FormulaDescription = '';
                        $scope.FormulaChildModel.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.FormulaChildModel.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.FormulaChildModel.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                        }

                        $scope.FormulaChildModel.FormulaDescription = $scope.FormulaChildModel.FormulaDes;
                        $scope.FormulaChildModel.FormulaIDDescription = $scope.FormulaChildModel.FormulaDesID;

                    }
                }
                else {
                    throw "First select Salary Head or input value.";
                }
            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.FormulaChildModel.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.TaxEarningMasterChildId = $scope.TaxExemptionFormula.Id == null ? null : $scope.TaxExemptionFormula.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.FormulaChildModel.Precedence;
                    formulaObj.Component = $scope.FormulaChildModel.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.FormulaChildModel.FormulaDes = '';
                    $scope.FormulaChildModel.FormulaDesID = '';

                    $scope.FormulaChildModel.FormulaDescription = '';
                    $scope.FormulaChildModel.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.FormulaChildModel.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.FormulaChildModel.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.FormulaChildModel.FormulaDescription = $scope.FormulaChildModel.FormulaDes;
                    $scope.FormulaChildModel.FormulaIDDescription = $scope.FormulaChildModel.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.FormulaChildModel.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.TaxEarningMasterChildId = $scope.TaxExemptionFormula.Id == null ? null : $scope.TaxExemptionFormula.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.FormulaChildModel.Value;
                    formulaObj.Component = $scope.FormulaChildModel.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.FormulaChildModel.FormulaDes = '';
                    $scope.FormulaChildModel.FormulaDesID = '';

                    $scope.FormulaChildModel.FormulaDescription = '';
                    $scope.FormulaChildModel.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.FormulaChildModel.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.FormulaChildModel.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.FormulaChildModel.FormulaDescription = $scope.FormulaChildModel.FormulaDes;
                    $scope.FormulaChildModel.FormulaIDDescription = $scope.FormulaChildModel.FormulaDesID;

                }
            }
            else if (formula === 'Other') {

                if (!baseService.isUndefinedOrNull($scope.FormulaChildModel.Other)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.TaxEarningMasterChildId = $scope.TaxExemptionFormula.Id == null ? null : $scope.TaxExemptionFormula.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.FormulaChildModel.Other;
                    formulaObj.Component = $scope.FormulaChildModel.Other;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.FormulaChildModel.FormulaDes = '';
                    $scope.FormulaChildModel.FormulaDesID = '';

                    $scope.FormulaChildModel.FormulaDescription = '';
                    $scope.FormulaChildModel.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.FormulaChildModel.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.FormulaChildModel.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.FormulaChildModel.FormulaDescription = $scope.FormulaChildModel.FormulaDes;
                    $scope.FormulaChildModel.FormulaIDDescription = $scope.FormulaChildModel.FormulaDesID;

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

        $scope.FormulaChildModel.FormulaDes = '';
        $scope.FormulaChildModel.FormulaDesID = '';

        $scope.FormulaChildModel.FormulaDescription = '';
        $scope.FormulaChildModel.FormulaIDDescription = '';

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.FormulaChildModel.FormulaDes)) {
                $scope.FormulaChildModel.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                $scope.FormulaChildModel.FormulaDesID += ' ' + $scope.FormulaDetails[i].SalaryHeadID;
            } else {
                $scope.FormulaChildModel.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.FormulaChildModel.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
            }
        }

        $scope.FormulaChildModel.FormulaDescription = $scope.FormulaChildModel.FormulaDes;
        $scope.FormulaChildModel.FormulaIDDescription = $scope.FormulaChildModel.FormulaDesID;

    }

    $scope.SaveFormula = function () {
        try {
            $scope.TaxExemptionFormula.Formula = $scope.FormulaChildModel.FormulaDes;
            $scope.TaxExemptionFormula.FormulaID = $scope.FormulaChildModel.FormulaDesID;
            $http({
                method: 'POST',
                url: $scope.path + "SaveGeneralFormula",
                data: { 'TaxExemptionFormula': $scope.TaxExemptionFormula, 'details': $scope.FormulaDetails },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getGeneralTaxFormula($scope.TaxExemptionFormula.TaxEarningMasterChildId);
                    $scope.ClearFormula();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    // #endregion

    // #region Grid Click Functionality

    $scope.GeneralTaxFormulaList = [];
    $scope.getGeneralTaxFormula = function (TaxEarnChildId) {
        $http({
            method: 'GET',
            url: $scope.path + "GetGeneralFormula?TaxEarnChildId=" + TaxEarnChildId,
        }).then(function successCallback(response) {
            $scope.GeneralTaxFormulaList = response.data;
        });
    }

    $scope.ModalShowName = null;

    $scope.AddEntry = function () {
        try {
            // EarningMasterChild Table Id fetching
            var gridObj = $("#EarningMasterChildGrid").data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];
            $scope.TaxExemptionFormula.TaxEarningMasterChildId = data.Id;
            $scope.EarningChildId = data.Id;
            $scope.ModalShowName = data.SalaryHead;

            $scope.getGeneralTaxFormula($scope.TaxExemptionFormula.TaxEarningMasterChildId);

            if (data.ExemptionApplicable == true)
            {
                angular.element(document.querySelector('#ExemptionPopup')).modal('show');
            }
            else
            {
                ShowResult("Exemption Applicable is not checked for this Taxable Income");
            }
        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    // #endregion

    // #region Clear Formula Function

    $scope.ClearFormula = function () {

        $scope.TaxExemptionFormula = {
            Id: null,
            TaxEarningMasterChildId: $scope.EarningChildId,
            Formula: null,
            FormulaID: null,
            Description: null
        }

        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.FormulaChildModel = {
            TaxEarningMasterChildId: null,
            FormulaDes: null,
            FormulaDesID: null,
        }
        $scope.FormulaChildModel.FormulaDes = null;
        $scope.FormulaChildModel.FormulaDesID = null;
        $scope.FormulaChildModel.SalaryHeadFormula = null;
        $scope.FormulaChildModel.FormulaDescription = null;
        $scope.FormulaDetails = [];
    };

    //#endregion

    // #region Delete Formula Functions

    $scope.ConfirmDeleteFormula = function (obj) {
        $scope.TaxExemptionFormula.Id = obj.data.Id;
        $scope.DeleteFormula();
    };

    $scope.DeleteFormula = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "DeleteFormula",
                data: { ID: $scope.TaxExemptionFormula.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.getGeneralTaxFormula($scope.TaxExemptionFormula.TaxEarningMasterChildId);
                    $scope.ClearFormula();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //#endregion

    // #region Double Click Formula Grid 

    $scope.GeneralFormula = function (obj) {
        $scope.TaxExemptionFormula = Object.assign({}, obj.data);

        $http({
            method: 'GET',
            url: $scope.path + "GetFormulaList?FormulaId=" + $scope.TaxExemptionFormula.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.FormulaDetails = response.data;

                $scope.FormulaChildModel.FormulaDes = '';
                $scope.FormulaChildModel.FormulaDesID = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                    if (!baseService.isUndefinedOrNull($scope.FormulaChildModel.FormulaDes)) {
                        $scope.FormulaChildModel.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;

                        $scope.FormulaChildModel.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                    } else {
                        $scope.FormulaChildModel.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.FormulaChildModel.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
                    }
                }

                $scope.FormulaChildModel.FormulaDescription = $scope.FormulaChildModel.FormulaDes;
                $scope.FormulaChildModel.FormulaIDDescription = $scope.FormulaChildModel.FormulaDesID;


            }
        });

    };

    //#endregion

    //#endregion

    // #region EarningMaster Functions
        
    $scope.EarningMasterModel = {
        Id: null,
        TaxPolicyHeaderId: null,
        SalaryHeadId: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Remarks: null,
        Active: false,
        TaxableAmountFix: 0,
        TaxableAmountFix: 0,
        ExemptionApplicable: false
    };


    $scope.SaveEarningMaster = function () {

        if (baseService.isUndefinedOrNull($scope.EarningMasterModel.SalaryHeadId)) {
            ShowResult("SalaryHead cann't be blank...");
        }
        else if (baseService.isUndefinedOrNull($scope.EarningMasterModel.StandardName)) {
            ShowResult("StandardName cann't be blank...");
        }
        else if (baseService.isUndefinedOrNull($scope.EarningMasterModel.UserName)) {
            ShowResult("UserName cann't be blank...");
        }
        else if (baseService.isUndefinedOrNull($scope.EarningMasterModel.ShortName)) {
            ShowResult("ShortName cann't be blank...");
        }
        else
        {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveEarningMaster',
                data: { 'EarningMasterData': $scope.EarningMasterModel }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetEarningMasterList();
                }
            })
            ,function errorCallBack(response)
             {
               ShowResult(response.data.Message, 'failure');
             }
        }
    }
      
    $scope.ClearEarningMaster = function () {
        $scope.EarningMasterModel = {
            Id: null,
            TaxPolicyHeaderId: null,
            SalaryHeadId: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Remarks: null,
            Active: false,
            TaxableAmountFix: 0,
            TaxableAmountPer: 0,
            ExemptionApplicable: false
        };
        $scope.EarningMasterModel.HeaderId = $scope.Header.Id;

    }

    $scope.EarningList = [];
    $scope.GetEarningMasterList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEarningMasterList",
            data: { 'Id': $scope.Header.Id},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EarningList = [];
            $scope.EarningList = response.data;
        });
    }
      
    $scope.getEarnMasterChildDetails = function (e) {
        $scope.EarningMasterModel = e.data; // Model which is used as ng-model will come here
    }

    $scope.SalaryHeadList = [];
    $scope.getSalaryHeadList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getSalaryHeadList'
        }).then(function success(response) {
            $scope.SalaryHeadList = response.data;
        })
    }

    $scope.getSalaryHeadList();

    //#endregion

    // #region Plant Tagging Tab Functions

    $scope.Child = {
        Id: null,
        HeaderId: null,
        PlantId: null
    };  

    $scope.PlantList = [];
    $scope.getPlants = function () {
        $http({
            method: 'GET',
            url: 'HumanResource/RosterPattern/getPlants',
            params: { 'cmp': $scope.Company }
        }).then(function success(response) {
            $scope.PlantList = response.data;
        })
    }

    $scope.Company = null;
    $scope.CompanyList = [];
    $scope.getCompany = function () {
        $http({
            method: 'GET',
            url: 'humanresource/RosterPattern/getCompany'
        }).then(function success(response) {
            $scope.CompanyList = response.data;
        })
    }

    $scope.getCompany();

    $scope.childDataList = [];
    function updateChild() {
        $http({
            method: 'POST',
            url: $scope.path + 'getChildData',
            data: { 'MasterId': $scope.Header.Id }
        }).then(function success(resp) {
            $scope.childDataList = [];
            $scope.childDataList = resp.data;
        });
    }

    $scope.DeleteChildData = [];
    $scope.confirmModal = function (data) {
        $scope.DeleteChildData = [];
        $scope.DeleteChildData = data;
        angular.element(document.querySelector('#confirmPOPUPD')).modal('show');
    }

    $scope.DeleteChild = function () {

        var obj = $scope.DeleteChildData;
        if (!baseService.isUndefinedOrNull(obj.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteChild',
                data: { 'id': obj.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    updateChild();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.saveChild = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ChildForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveChild',
                data: { 'Child': $scope.Child }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    console.log(response.data.Data);
                    updateChild();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    //#endregion

}