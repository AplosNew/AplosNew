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


    /// ******************************* Header Operations ******************************* \\\
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

    // #region TaxExemption Formula Functions

    $scope.TaxExemptionFormula = {
        Id: null,
        TaxEarningMasterChildId: null,
        Formula: null,
        FormulaID: null,
        Description: null       
    }

    $scope.NoticePeriodNew = {
        TaxPolicyGeneralId: null,
        FormulaDes: null,
        FormulaDesID: null,
    }
    $scope.NoticePeriodNew.FormulaDes = null;
    $scope.NoticePeriodNew.FormulaDesID = null;
    $scope.NoticePeriodNew.SalaryHeadFormula = null;
    $scope.NoticePeriodNew.FormulaDescription = null;

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

    $scope.FormulaDetails = [];

    $scope.getGeneralTaxFormula = function (TaxEarnChildId) {
        $http({
            method: 'GET',
            url: $scope.path + "GetGeneralFormula?TaxEarnChildId=" + TaxEarnChildId,
        }).then(function successCallback(response) {
            $scope.GeneralTaxFormulaList = response.data;
        });
    }

    $scope.AddLineItemG = function () {
        try {
            $scope.ShowDiv = true;
            var gridObj = $("#EarningMasterChildGrid").data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];
            $scope.TaxExemptionFormula.TaxEarningMasterChildId = data.Id;
            $scope.EarningChildId = data.Id;

            $scope.getGeneralTaxFormula($scope.TaxExemptionFormula.TaxEarningMasterChildId);

            var eDialog = $("#GeneralForm").data("ejDialog");
            if (data.ExemptionApplicable == true) {
                $("#GeneralForm").ejDialog("setTitle", data.SalaryHead + " Exemption");
                eDialog.open();
            }
            else {
                throw "Exemption Applicable is not checked for this Taxable Income";
            }
        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.ClearFormula = function () {


        $scope.TaxExemptionFormula = {
            Id: null,
            TaxEarningMasterChildId: $scope.EarningChildId,
            Formula: null,
            FormulaID: null,
            Description: null
        }

        // Rest Still Have to Check 

        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.NoticePeriodNew = {
            TaxPolicyGeneralId: null,
            FormulaDes: null,
            FormulaDesID: null,
        }
        $scope.NoticePeriodNew.FormulaDes = null;
        $scope.NoticePeriodNew.FormulaDesID = null;
        $scope.NoticePeriodNew.SalaryHeadFormula = null;
        $scope.NoticePeriodNew.FormulaDescription = null;
        $scope.FormulaDetails = [];
    };


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