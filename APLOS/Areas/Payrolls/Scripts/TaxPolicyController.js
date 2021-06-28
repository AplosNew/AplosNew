'use strict';
TaxPolicyController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TaxPolicyController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Tax Policy';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/TaxPolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);
    $scope.saveBP = $scope.path + 'SaveTaxPolicyPlantWise';

    $scope.plantList = [];
    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.TaxPolicyMaster.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }
    //$scope.SalaryHeadList = [];
    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });

    //#region TAB
    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    //#endregion

    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion Tab

    //#region Model
    $scope.TaxPolicyMaster = {
        SystemID: null,
        TaxPolicyName: null,
        Description: null,
        TaxTypeId: null,
        TaxYearID: null,
        MinimumTaxableAmount: 0.00,
        //GenderID: null,
        CompanyId: null,
        PlantID: null,
        GroupID: null,
        CalculationBasis: null,
        TaxLimitInvestAll: 0,
        TaxFixedTaxInvestAll: 0.00,
        TaxPercentageInvestAll: 0,
        TaxFixedTaxRebate: 0.00,
        TaxPercentageRebate: 0,
        BaseOnIncomeTaxRebate: false,
        //IsGenderSpecific: false,
        IsFixedTaxInvestAll: true,
        IsPercentageTaxInvestAll: false,
        IsBaseOnActEntAmt: false,
        IsLimitInvestAll: false,
        IsFixedTaxRebate: true,
        IsPercentageTaxRebate: false,
        TaxAbleIncomeUpperForRebate: null,
        TaxFixedBonusDefine: 0,
        TaxFixedLvEncash: 0,
        IsFixedTaxBonusDefine: true,
        IsTaxAsPerActual: false,
        IsTaxAsPerProjection: false,
        IsFixedTaxLvEncash: true,
        IsTaxAsPerActualLvEncash: false,
        IsTaxAsPerProjectionLvEncash: false,
        IsCumulativeTaxSlabDefine: false,
        IsBrakeTaxSlabDefine: true,
        Male: true,
        Female: true,
        AgeFrom: null,
        AgeTo: null,
    }

    $scope.TaxPolicyGeneral = {
        SystemID: null,
        TaxPolicyMstID: null,
        SalaryHeadID: null,
        IsTaxable: false,
        IsFixedTaxGeneral: true,
        //$scope.radioAa = true,
        TaxFixedGeneral: 0,
        IsPercentageTaxGeneral: false,
        TaxPercentageGeneral: 0.00,
        IsExemption: false,
        IsExmWhichEverLess: false,
        IsMaxExmpAmt: false,
        TaxMaxExmpAmt: 0.00,
        IsExmBaseOnActual: false,
        IsExmBaseOnOtherSlrHd: false,
        ExmSalaryHeadID: null,
        PercentageExmAmtOtherSlrHd: 0.00,
        IsLessOrMore: 'Which Ever Is Less',
        Sequence: 0
    }

    $scope.TaxRebateSlabDefine = {
        SystemID: null,
        TaxPolicyMstID: null,
        TaxAbleIncomeLowerForRebate: null,
        TaxAbleIncomeUpperForRebate: null,
        //SlabDefine: null,
        InvesmentAmtForRebate: null,
        InvestAmtTaxPercentageRebate: null,
    }

    $scope.TaxSlabDefineProfessional = {
        Id: null,
        TaxPolicyMasterId: null,
        YearlyMinValue: null,
        YearlyMaxValue: null,
        MonthlyMinValue: null,
        MonthlyMaxValue: null,
        YearlyTaxAmount: null,
        MonthlyTaxAmount: null,
        SeqenceNo: null,
        AdjustingAmount: null,
        MonthOfAdjustment: null,
    }

    $scope.TaxSlabDefine = {
        SystemID: null,
        TaxPolicyMstID: $scope.TaxPolicyMaster.SystemID,
        //SlabType: 'Cumulative',
        Minimum: null,
        Maximum: null,
        TaxRate: null,
    }
    $scope.TaxSlabDefinee = {
        Cumulative: null,
        BrakeUp: null,
    }
    $scope.TaxPolicyGeneralFormula = {
        Id: null,
        TaxPolicyGeneralId: null,
        Formula: null,
        FormulaID: null,
        Description: null,
        IsOptionBased: false,
        OptionBasedValue: null,
        IsOptionBaseDefault: true,
    }

    $scope.TaxRebate = {
        Id: null,
        CumulativeOrBrakeUp:'Brake Up',
        FixedOrPercentage:'Fixed',
        TaxableIncomeOrTax:'Tax',
    }

    // #endregion Model

    //#region Get Master
    $scope.MasterList = [];
    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetMaster",
        }).then(function successCallback(response) {
            $scope.MasterList = response.data;
            $scope.getData();
            $scope.PlantCompanyList();
        });
    }
    $scope.getMaster();
    $scope.DetailList = [];
    $scope.getDetail = function (obj) {
        $scope.Action = 'Update';
        $scope.TaxPolicyMaster = Object.assign({}, obj.data);
        //$scope.TaxPolicyMaster = obj.data;
        if ($scope.TaxPolicyMaster.IsFixedTaxInvestAll) {
            $scope.radioFixedValue = true;
        }
        else {
            $scope.radioFixedValue = false;
        }
        if ($scope.TaxPolicyMaster.IsPercentageTaxInvestAll) {
            $scope.radioFormulaValue = true;
        }
        else {
            $scope.radioFormulaValue = false;
        }

        if ($scope.TaxPolicyMaster.BaseOnIncomeTaxRebate) {
            $scope.radioB3Value = true;
        }
        else {
            $scope.radioB3Value = false;
        }

        if ($scope.TaxPolicyMaster.IsFixedTaxRebate) {
            $scope.radioB1Value = true;
        }
        else {
            $scope.radioB2Value = false;
        }
        if ($scope.TaxPolicyMaster.IsFixedTaxBonusDefine) {
            $scope.radioB4Value = true;
        }
        if ($scope.TaxPolicyMaster.IsFixedTaxLvEncash) {
            $scope.radioB7Value = true;
        }
        if ($scope.TaxPolicyMaster.IsCumulativeTaxSlabDefine == true) {
            $scope.TaxSlabDefinee.Cumulative = true;
            $scope.radioCu = true;

            $scope.TaxSlabDefinee.BrakeUp = false;
            $scope.radioBr = false;
        }
        else {
            $scope.TaxSlabDefinee.BrakeUp = true;
            $scope.radioBr = true;

            $scope.TaxSlabDefinee.Cumulative = false;
            $scope.radioCu = false;
        }

        if ($scope.TaxPolicyMaster.IsCumulativeInvestmentCredit) {
            $scope.TaxSlabDefineeg.IsCumulativeInvestmentCredit = true;
            $scope.radioCug = true;

            $scope.TaxSlabDefineeg.IsBrakeInvestmentCredit = false;
            $scope.radioBrg = false;
        }
        else {
            $scope.TaxSlabDefineeg.IsBrakeInvestmentCredit = true;
            $scope.radioBrg = true;

            $scope.TaxSlabDefineeg.IsCumulativeInvestmentCredit = false;
            $scope.radioCug = false;
        }
        if ($scope.TaxPolicyMaster.IsTaxRebateCumulative) {
            $scope.TaxRebate.CumulativeOrBrakeUp = 'Cumulative';
        }
        else {
            $scope.TaxRebate.CumulativeOrBrakeUp = 'Brake Up';
        }

        if ($scope.TaxPolicyMaster.IsTaxRebateFixed) {
            $scope.TaxRebate.FixedOrPercentage = 'Fixed';
        }
        else {
            $scope.TaxRebate.FixedOrPercentage = 'Percentage';
        }

        if ($scope.TaxPolicyMaster.IsTaxRebateTaxableIncome) {
            $scope.TaxRebate.TaxableIncomeOrTax = 'Taxable Income';
        }
        else {
            $scope.TaxRebate.TaxableIncomeOrTax = 'Tax';
        }

        if (baseService.arrayLength($scope.TaxTypeList) > 0) {
            for (var i = 0; i < $scope.TaxTypeList.length; i++) {
                if ($scope.TaxTypeList[i].Id == $scope.TaxPolicyMaster.TaxTypeId) {
                    $scope.TaxPolicyMaster.TaxType = $scope.TaxTypeList[i].Category;
                    break;
                }
            }
        }
        $scope.getGeneralTax($scope.TaxPolicyMaster.SystemID);
        $scope.getIncome($scope.TaxPolicyMaster.SystemID);
        $scope.getProTax($scope.TaxPolicyMaster.SystemID);
        $scope.getTaxMonth($scope.TaxPolicyMaster.TaxYearID);
        $scope.getValidationForPlant($scope.TaxPolicyMaster.SystemID);
        $scope.GetSequence($scope.TaxPolicyMaster.SystemID);
        $scope.getTaxRebate($scope.TaxPolicyMaster.SystemID);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    $scope.GenderCheck = false;
    $scope.TaxPolicyMaster.GenderCheck = false;
    $scope.getValidationForPlant = function (Id) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetValidationForPlant?TPId=' + Id,
        }).then(function successCallback(response) {
            $scope.GenderCheck = response.data[0].GenderCheck;
            if ($scope.GenderCheck == 1) {
                $scope.GenderCheck = true;
            }
            else {
                $scope.GenderCheck = false;
            }
            //$scope.getDetail();
        });
    }
    //$scope.getData();

    //#endregion

    //#region Get others
    $scope.YearList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetTaxYear",
        }).then(function successCallback(response) {
            $scope.YearList = response.data;
        });
    }
    $scope.getData();

    $scope.TaxTypeList = [];
    $scope.getTaxGroup = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetTaxGroup",
        }).then(function successCallback(response) {
            $scope.TaxTypeList = response.data;

        });
    }
    $scope.getTaxGroup();

    $scope.setvalue = function () {
        // $scope.TaxPolicyMaster.TaxType = $("#TaxType option:selected").text();
        $scope.TaxPolicyMaster.TaxType = $.grep($scope.TaxTypeList, function (item) {
            return item.Id === $scope.TaxPolicyMaster.TaxTypeId;
        })[0].Category;
        $scope.tab = 1;
    }

    $scope.TaxMonthList = [];
    $scope.getTaxMonth = function (year) {
        $http({
            method: 'GET',
            url: $scope.path + "GetTaxMonth?Year=" + year,
        }).then(function successCallback(response) {
            $scope.TaxMonthList = response.data;

        });
    }

    //#endregion

    //#region Get General Tax
    $scope.GeneralTaxList = [];
    $scope.GeneralTaxFormulaList = [];
    $scope.getGeneralTax = function (Master) {
        $http({
            method: 'GET',
            url: $scope.path + "GetGeneral?Master=" + Master,
        }).then(function successCallback(response) {
            $scope.GeneralTaxList = response.data;
            //$scope.getData();
        });
    }
    $scope.getGeneralTaxFormula = function (TaxGeneralID) {
        $http({
            method: 'GET',
            url: $scope.path + "GetGeneralFormula?GeneralID=" + TaxGeneralID,
        }).then(function successCallback(response) {
            $scope.GeneralTaxFormulaList = response.data;
            //$scope.getData();
        });
    }
    $scope.getTax = function (obj) {
        $scope.TaxPolicyGeneral = Object.assign({}, obj.data);
        if ($scope.TaxPolicyGeneral.IsFixedTaxGeneral) {
            $scope.radioAa = true;
        }
        else {
            $scope.radioAa = false;
        }
        if ($scope.TaxPolicyGeneral.IsPercentageTaxGeneral) {
            $scope.radioBb = true;
        }
        else {
            $scope.radioBb = false;
        }
    }
    $scope.GeneralFormula = function (obj) {
        $scope.TaxPolicyGeneralFormula = Object.assign({}, obj.data);

        $http({
            method: 'GET',
            url: $scope.path + "GetFormulaList?GeneralFormulaId=" + $scope.TaxPolicyGeneralFormula.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.FormulaDetails = response.data;

                $scope.NoticePeriodNew.FormulaDes = '';
                $scope.NoticePeriodNew.FormulaDesID = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                    if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.FormulaDes)) {
                        $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;

                        $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                    } else {
                        $scope.NoticePeriodNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.NoticePeriodNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
                    }
                }

                $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;


            }
        });

    };
    //#endregion

    //#region Get Professional Tax
    $scope.ProTaxList = [];
    $scope.getProTax = function (Master) {
        $http({
            method: 'GET',
            url: $scope.path + "GetPro?Master=" + Master + '&YearId=' + $scope.TaxPolicyMaster.TaxYearID,
        }).then(function successCallback(response) {
            $scope.ProTaxList = response.data;
            //$scope.getData();
        });
    }


    $scope.getProTaxD = function (obj) {
        $scope.TaxSlabDefineProfessional = Object.assign({}, obj.data);
        $scope.TaxSlabDefineProfessional.MonthOfAdjustment = obj.data.MonthOfAdjustment;
    }
    //#endregion

    //#region Get Rebate
    $scope.RebateTaxList = [];
    $scope.getRebate = function (Master) {
        $http({
            method: 'GET',
            url: $scope.path + "GetRebate?Master=" + Master,
        }).then(function successCallback(response) {
            $scope.RebateTaxList = response.data;
            if ($scope.RebateTaxList.length == 0) {
                $scope.RebateTaxList.push(Object.assign({}, $scope.TaxRebateSlabDefinee));
            }
        });
    }
    $scope.getRebateD = function (obj) {
        $scope.TaxRebateSlabDefine = obj.data;
    }
    //#endregion

    //#region Get Income Tax
    $scope.DataList = [];
    $scope.getIncome = function (Master) {
        $http({
            method: 'GET',
            url: $scope.path + "GetIncome?Master=" + Master,
        }).then(function successCallback(response) {
            if (response.data.length == 0) {
                $scope.DataList = [];
                $scope.DataList.push(Object.assign({}, $scope.TaxSlabDefine));
            }
            else {
                $scope.DataList = response.data;
            }
        });
    }
    $scope.getIncomeD = function (obj) {
        $scope.TaxSlabDefine = obj.data;
        if ($scope.TaxSlabDefine.Cumulative) {
            $scope.radioCu = true;
        }
        else {
            $scope.radioBr = true;
        }
    }
    $scope.TaxSlabDefinee = Object.assign({}, $scope.TaxSlabDefine);
    $scope.DataList.push(Object.assign({}, $scope.TaxSlabDefinee));

    $scope.TaxRebateSlabDefinee = Object.assign({}, $scope.TaxRebateSlabDefine);
    $scope.RebateTaxList.push(Object.assign({}, $scope.TaxRebateSlabDefinee));

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
        if ($scope.DataList.length == 0) {
            $scope.DataList.push(Object.assign({}, $scope.TaxSlabDefine));
        }
    }
    $scope.RebateRemove = function (index) {
        var removed = $scope.RebateTaxList.splice(index, 1);
        $scope.Detail = removed;
    }
    $scope.SubmitH = function (data) {

        try {
            if (data.Minimum < 0)
                throw 'Minimum value cannot be negative';
            if (data.Minimum == null) {
                throw 'Enter Minimum Value';
            }

            if (data.Maximum < 0)
                throw 'Maximum value cannot be negative';


            if (data.Minimum >= data.Maximum)
                throw 'Maximum value should be greater than minimum value';



            var newObj = Object.assign({}, $scope.TaxSlabDefine);
            if (data != null) {
                newObj = {
                    SystemID: null,
                    TaxRate: null,
                    Minimum: data.Maximum,
                    Maximum: 0,
                    TaxPolicyMstID: $scope.TaxPolicyMaster.SystemID,
                }
            }

            $scope.DataList.push(newObj);
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.SubmitRebate = function (data) {

        try {
            if (data.TaxAbleIncomeLowerForRebate < 0)
                throw 'Minimum value cannot be negative';
            if (data.TaxAbleIncomeLowerForRebate == null) {
                throw 'Enter Minimum Value';
            }

            if (data.TaxAbleIncomeUpperForRebate < 0)
                throw 'Maximum value cannot be negative';
            if (data.InvestAmtTaxPercentageRebate < 0)
                throw 'Invest Amount Tax Percentage Rebate cannot be negative';


            if (data.TaxAbleIncomeLowerForRebate >= data.TaxAbleIncomeUpperForRebate)
                throw 'Maximum value should be greater than minimum value';
            //if (data.SlabDefine == null || data.SlabDefine == '')
            //    throw 'Select Slab Define';



            var newObjs = Object.assign({}, $scope.TaxRebateSlabDefine);
            if (data != null) {
                newObjs = {
                    SystemID: null,
                    //SlabDefine: null,
                    TaxAbleIncomeLowerForRebate: data.TaxAbleIncomeUpperForRebate,
                    TaxAbleIncomeUpperForRebate: null,
                    TaxPolicyMstID: $scope.TaxPolicyMaster.SystemID,
                    InvesmentAmtForRebate: null,
                    InvestAmtTaxPercentageRebate: null,
                }
            }

            $scope.RebateTaxList.push(newObjs);
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };
    //#endregion

    //#region plant List
    $scope.PlantList = [];
    $scope.PlantCompanyList = function () {
        $http.post('Payrolls/TaxPolicy/GetPlantWisePolicy')
            .then(function (response) {
                $scope.PlantList = response.data;

            });
    };
    //#endregion

    //#region tax policy plant

    $scope.TaxPolicyPlantWise = {
        Id: null,
        TaxPolicyId: null,
        PlantId: null
    }

    $scope.PlantWiseBPolicyList = [];
    $scope.getplantPolicy = function () {
        $http.get("Payrolls/TaxPolicy/GetPlantTaxPolicy?plantID=" + $scope.PlantId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.PlantWiseBPolicyList = response.data;
                        if (baseService.arrayLength($scope.PlantWiseBPolicyList) > 0) {
                            for (var i = 0; i < $scope.PlantWiseBPolicyList.length; i++) {
                                $scope.PlantWiseBPolicyList[i].PlantId = $scope.PlantId;
                            }
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.AddLineItemT = function (obj) {
        try {
            $scope.ShowDiv = true;
            $scope.PlantId = obj.data.PlantId;
            var eDialog = $("#policyID").data("ejDialog");
            eDialog.open();
            $scope.TaxPolicyPlantWise = {
                Id: null,
                TaxPolicyId: null,
                PlantId: $scope.PlantId
            }
            $scope.getplantPolicy();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.SaveTP = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.saveBP,
                data: { 'BP': $scope.PlantWiseBPolicyList, plantID: $scope.PlantId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getplantPolicy();
                    $scope.getMaster();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //#endregion

    //#region Modal
    $scope.ShowDiv = false;

    $scope.AddLineItemF = function () {
        try {
            $scope.ShowDiv = true;
            if ($scope.TaxPolicyMaster.IsCumulativeInvestmentCredit) {
                $scope.TaxSlabDefineeg.IsCumulativeInvestmentCredit = true;
                $scope.radioCug = true;

                $scope.TaxSlabDefineeg.IsBrakeInvestmentCredit = false;
                $scope.radioBrg = false;
            }
            else {
                $scope.TaxSlabDefineeg.IsBrakeInvestmentCredit = true;
                $scope.radioBrg = true;

                $scope.TaxSlabDefineeg.IsCumulativeInvestmentCredit = false;
                $scope.radioCug = false;
            }
            var eDialog = $("#Base").data("ejDialog");
            eDialog.open();
            $scope.getRebate($scope.TaxPolicyMaster.SystemID)
            $scope.TaxRebateSlabDefine = {
                SystemID: null,
                TaxPolicyMstID: $scope.TaxPolicyMaster.SystemID,
                TaxAbleIncomeLowerForRebate: null,
                TaxAbleIncomeUpperForRebate: null,
                //SlabDefine: null,
                InvesmentAmtForRebate: null,
                InvestAmtTaxPercentageRebate: null,
            }
            $scope.TaxRebateSlabDefine.TaxPolicyMstID = $scope.TaxPolicyMaster.SystemID;
        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.AddLineItemG = function () {
        try {
            $scope.ShowDiv = true;
            var gridObj = $("#BPolicyId").data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];
            $scope.TaxPolicyGeneralFormula.TaxPolicyGeneralId = data.SystemID;
            var eDialog = $("#General").data("ejDialog");
            if (data.IsExemption == true) {
                $scope.getGeneralTaxFormula($scope.TaxPolicyGeneralFormula.TaxPolicyGeneralId);
                $("#General").ejDialog("setTitle", data.SalaryHead + " Examption");
                eDialog.open();
            }
            else {
                throw "Exemption Applicable is not checked for this Taxable Income";
            }
        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    //#endregion

    //#region clear checkbox
    $scope.ClearCheck = function () {
        if ($scope.TaxPolicyGeneral.IsTaxable == false) {
            $scope.TaxPolicyGeneral.IsFixedTaxGeneral = true;
            $scope.TaxPolicyGeneral.IsPercentageTaxGeneral = false;
            $scope.TaxPolicyGeneral.IsExemption = false;
            $scope.TaxPolicyGeneral.TaxFixedGeneral = 0;
            $scope.TaxPolicyGeneral.TaxPercentageGeneral = 0;
            $scope.TaxPolicyGeneral.PercentageExmAmtOtherSlrHd = 0;
            $scope.TaxPolicyGeneral.TaxMaxExmpAmt = 0;
            $scope.TaxPolicyGeneral.IsExmBaseOnOtherSlrHd = false;
            $scope.TaxPolicyGeneral.IsExmWhichEverLess = false;
            $scope.TaxPolicyGeneral.ExmSalaryHeadID = '';
            $scope.TaxPolicyGeneral.IsMaxExmpAmt = false;
            $scope.TaxPolicyGeneral.IsExmBaseOnActual = false;
            $scope.radioAa = true;
            $scope.radioBb = false;

        }
    }
    $scope.Exemption = function () {
        if ($scope.TaxPolicyGeneral.IsExemption == false) {
            $scope.TaxPolicyGeneral.IsLessOrMore = 'Which Ever Is Less';
        }
    };
    //#endregion

    // #region Radio button Value for Investment Allow
    $scope.radiovalue = false;
    $scope.radioFixedValue = true;
    $scope.radioFormulaValue = false;
    $scope.setRadioFixedValue = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValue = true;
        $scope.radioFormulaValue = false;
        $scope.TaxPolicyMaster.IsFixedTaxInvestAll = true;
        $scope.TaxPolicyMaster.IsPercentageTaxInvestAll = false;
    }
    $scope.setRadioFormulaValue = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValue = false;
        $scope.radioFormulaValue = true;
        $scope.TaxPolicyMaster.IsPercentageTaxInvestAll = true;
        $scope.TaxPolicyMaster.IsFixedTaxInvestAll = false;
    }
    // #endregion Radio button Value for Investment Allow

    // #region Radio button Value for Investment Credit
    $scope.radiovalueC = false;
    $scope.radioB1Value = true;
    $scope.radioB2Value = false;
    $scope.radioB3Value = false;

    $scope.setRadioF = function () {
        $scope.radiovalueC = true;
        $scope.radioB1Value = true;
        $scope.radioB2Value = false;
        $scope.radioB3Value = false;
        $scope.TaxPolicyMaster.IsFixedTaxRebate = true;
        $scope.TaxPolicyMaster.IsPercentageTaxRebate = false;
        $scope.TaxPolicyMaster.BaseOnIncomeTaxRebate = false;
    }
    $scope.setRadioP = function () {
        $scope.radiovalueC = true;
        $scope.radioB1Value = false;
        $scope.radioB2Value = true;
        $scope.radioB3Value = false;
        $scope.TaxPolicyMaster.IsPercentageTaxRebate = true;
        $scope.TaxPolicyMaster.IsFixedTaxRebate = false;
        $scope.TaxPolicyMaster.BaseOnIncomeTaxRebate = false;
    }
    $scope.setRadioI = function () {
        $scope.radiovalueC = true;
        $scope.radioB1Value = false;
        $scope.radioB2Value = false;
        $scope.radioB3Value = true;
        $scope.TaxPolicyMaster.BaseOnIncomeTaxRebate = true;
        $scope.TaxPolicyMaster.IsFixedTaxRebate = false;
        $scope.TaxPolicyMaster.IsPercentageTaxRebate = false;
    }
    //#endregion Radio button Value for Investment Credit

    // #region Radio button Value for Bonus Definition
    $scope.radiovalueD = false;
    $scope.radioB4Value = true;
    $scope.radioB5Value = false;
    $scope.radioB6Value = false;

    $scope.setRadioFi = function () {
        $scope.radiovalueD = true;
        $scope.radioB4Value = true;
        $scope.radioB5Value = false;
        $scope.radioB6Value = false;
        $scope.TaxPolicyMaster.IsFixedTaxBonusDefine = true;
        $scope.TaxPolicyMaster.IsTaxAsPerActual = false;
        $scope.TaxPolicyMaster.IsTaxAsPerProjection = false;
    }
    $scope.setRadioPi = function () {
        $scope.radiovalueD = true;
        $scope.radioB4Value = false;
        $scope.radioB5Value = true;
        $scope.radioB6Value = false;
        $scope.TaxPolicyMaster.IsTaxAsPerActual = true;
        $scope.TaxPolicyMaster.IsFixedTaxBonusDefine = false;
        $scope.TaxPolicyMaster.IsTaxAsPerProjection = false;
    }
    $scope.setRadioIi = function () {
        $scope.radiovalueD = true;
        $scope.radioB4Value = false;
        $scope.radioB5Value = false;
        $scope.radioB6Value = true;
        $scope.TaxPolicyMaster.IsTaxAsPerProjection = true;
        $scope.TaxPolicyMaster.IsFixedTaxBonusDefine = false;
        $scope.TaxPolicyMaster.IsTaxAsPerActual = false;
    }
    //#endregion Radio button Value for Bonus Definition

    // #region Radio button Value for Bonus Definition
    $scope.radiovalueE = false;
    $scope.radioB7Value = true;
    $scope.radioB8Value = false;
    $scope.radioB9Value = false;

    $scope.setRadioFix = function () {
        $scope.radiovalueE = true;
        $scope.radioB7Value = true;
        $scope.radioB8Value = false;
        $scope.radioB9Value = false;
        $scope.TaxPolicyMaster.IsFixedTaxLvEncash = true;
        $scope.TaxPolicyMaster.IsTaxAsPerActualLvEncash = false;
        $scope.TaxPolicyMaster.IsTaxAsPerProjectionLvEncash = false;
    }
    $scope.setRadioPix = function () {
        $scope.radiovalueE = true;
        $scope.radioB7Value = false;
        $scope.radioB8Value = true;
        $scope.radioB9Value = false;
        $scope.TaxPolicyMaster.IsTaxAsPerActualLvEncash = true;
        $scope.TaxPolicyMaster.IsFixedTaxLvEncash = false;
        $scope.TaxPolicyMaster.IsTaxAsPerProjectionLvEncash = false;
    }
    $scope.setRadioIix = function () {
        $scope.radiovalueE = true;
        $scope.radioB7Value = false;
        $scope.radioB8Value = false;
        $scope.radioB9Value = true;
        $scope.TaxPolicyMaster.IsTaxAsPerProjectionLvEncash = true;
        $scope.TaxPolicyMaster.IsFixedTaxLvEncash = false;
        $scope.TaxPolicyMaster.IsTaxAsPerActualLvEncash = false;
    }
    //#endregion Radio button Value for Bonus Definition

    // #region Radio button Value for Slab Definition
    $scope.radiovalueG = false;
    $scope.radioCu = false;
    $scope.radioBr = false;
    $scope.setRadioCu = function () {
        $scope.radiovalueG = true;
        $scope.radioCu = true;
        $scope.radioBr = false;
        $scope.TaxSlabDefinee.Cumulative = true;
        $scope.TaxSlabDefinee.BrakeUp = false;
    }
    $scope.setRadioBr = function () {
        $scope.radiovalueG = true;
        $scope.radioCu = false;
        $scope.radioBr = true;
        $scope.TaxSlabDefinee.BrakeUp = true;
        $scope.TaxSlabDefinee.Cumulative = false;
    }

    $scope.TaxSlabDefineeg = {
        IsCumulativeInvestmentCredit: false,
        IsBrakeInvestmentCredit: false,
    }
    $scope.radiovalueGg = false;
    $scope.radioCug = false;
    $scope.radioBrg = false;
    $scope.setRadioCug = function () {
        $scope.radiovalueGg = true;
        $scope.radioCug = true;
        $scope.radioBrg = false;
        $scope.TaxSlabDefineeg.IsBrakeInvestmentCredit = false ;
        $scope.TaxSlabDefineeg.IsCumulativeInvestmentCredit =  true;
    }
    $scope.setRadioBrg = function () {
        $scope.radiovalueGg = true;
        $scope.radioCug = false;
        $scope.radioBr = true;
        $scope.TaxSlabDefineeg.IsCumulativeInvestmentCredit = false ;
        $scope.TaxSlabDefineeg.IsBrakeInvestmentCredit = true;
    }

    // #endregion Radio button Value for Slab Definition

    // #region Radio button Value for Tex General Policy
    $scope.radiovalue1 = false;
    $scope.radioAa = true;
    $scope.radioBb = false;
    $scope.set1 = function () {
        $scope.radiovalue1 = true;
        $scope.radioAa = true;
        $scope.radioBb = false;
        $scope.TaxPolicyGeneral.IsFixedTaxGeneral = true;
        $scope.TaxPolicyGeneral.IsPercentageTaxGeneral = false;
    }
    $scope.set2 = function () {
        $scope.radiovalue1 = true;
        $scope.radioAa = false;
        $scope.radioBb = true;
        $scope.TaxPolicyGeneral.IsPercentageTaxGeneral = true;
        $scope.TaxPolicyGeneral.IsFixedTaxGeneral = false;
    }
    // #endregion Radio button Value for Tex General Policy

    //#region I N C O M E -- S L A B

    $scope.DataList = [];

    //#endregion

    //#region F o r m u l a For Exemption
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

    $scope.SetFormula = function (formula) {
        try {
            var formulaObj = {};

            if (formula === 'SHead') {

                formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                formulaObj.TaxPolicyGeneralId = $scope.TaxPolicyGeneralFormula.Id == null ? null : $scope.TaxPolicyGeneralFormula.Id;
                formulaObj.SalaryHeadID = $scope.NoticePeriodNew.SalaryHeadIdFormula;
                formulaObj.SalaryHead = $("#SalaryHeadFormula option:selected").text();
                formulaObj.Component = null;
                $scope.FormulaDetails.push(formulaObj);

                $scope.NoticePeriodNew.FormulaDes = '';
                $scope.NoticePeriodNew.FormulaDesID = '';

                $scope.NoticePeriodNew.FormulaDescription = '';
                $scope.NoticePeriodNew.FormulaIDDescription = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.FormulaDes)) {
                        $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        //$scope.NoticePeriodNew.FormulaDesID += ' ' + $scope.FormulaDetails[i].SalaryHeadID;
                        $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                    } else {
                        $scope.NoticePeriodNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.NoticePeriodNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
                    }
                }

                $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.TaxPolicyGeneralId = $scope.TaxPolicyGeneralFormula.Id == null ? null : $scope.TaxPolicyGeneralFormula.Id;
                        formulaObj.SalaryHeadID = null;
                        formulaObj.Component = $scope.NoticePeriodNew.Operator;
                        formulaObj.SalaryHead = $scope.NoticePeriodNew.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.NoticePeriodNew.FormulaDes = '';
                        $scope.NoticePeriodNew.FormulaDesID = '';

                        $scope.NoticePeriodNew.FormulaDescription = '';
                        $scope.NoticePeriodNew.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                        }

                        $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                        $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;

                    }
                }
                else {
                    throw "First select Salary Head or input value.";
                }



            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.TaxPolicyGeneralId = $scope.TaxPolicyGeneralFormula.Id == null ? null : $scope.TaxPolicyGeneralFormula.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.NoticePeriodNew.Precedence;
                    formulaObj.Component = $scope.NoticePeriodNew.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.NoticePeriodNew.FormulaDes = '';
                    $scope.NoticePeriodNew.FormulaDesID = '';

                    $scope.NoticePeriodNew.FormulaDescription = '';
                    $scope.NoticePeriodNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                    $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.TaxPolicyGeneralId = $scope.TaxPolicyGeneralFormula.Id == null ? null : $scope.TaxPolicyGeneralFormula.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.NoticePeriodNew.Value;
                    formulaObj.Component = $scope.NoticePeriodNew.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.NoticePeriodNew.FormulaDes = '';
                    $scope.NoticePeriodNew.FormulaDesID = '';

                    $scope.NoticePeriodNew.FormulaDescription = '';
                    $scope.NoticePeriodNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                    $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;

                }
            }
            else if (formula === 'Other') {

                if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.Other)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.TaxPolicyGeneralId = $scope.TaxPolicyGeneralFormula.Id == null ? null : $scope.TaxPolicyGeneralFormula.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.NoticePeriodNew.Other;
                    formulaObj.Component = $scope.NoticePeriodNew.Other;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.NoticePeriodNew.FormulaDes = '';
                    $scope.NoticePeriodNew.FormulaDesID = '';

                    $scope.NoticePeriodNew.FormulaDescription = '';
                    $scope.NoticePeriodNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                    $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;

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

        $scope.NoticePeriodNew.FormulaDes = '';
        $scope.NoticePeriodNew.FormulaDesID = '';

        $scope.NoticePeriodNew.FormulaDescription = '';
        $scope.NoticePeriodNew.FormulaIDDescription = '';

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.FormulaDes)) {
                $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                $scope.NoticePeriodNew.FormulaDesID += ' ' + $scope.FormulaDetails[i].SalaryHeadID;
            } else {
                $scope.NoticePeriodNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.NoticePeriodNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
            }
        }

        $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
        $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;

    }

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

    $scope.SaveFormula = function () {
        try {
            $scope.TaxPolicyGeneralFormula.Formula = $scope.NoticePeriodNew.FormulaDes;
            $scope.TaxPolicyGeneralFormula.FormulaID = $scope.NoticePeriodNew.FormulaDesID;
            $http({
                method: 'POST',
                url: $scope.path + "SaveGeneralFormula",
                data: { 'GeneralFormula': $scope.TaxPolicyGeneralFormula, 'details': $scope.FormulaDetails },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getGeneralTaxFormula($scope.TaxPolicyGeneralFormula.TaxPolicyGeneralId);
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

    //#region Clear Master
    $scope.Clear = function () {
        $scope.tab = 1;
        $scope.Action = 'Save';
        $scope.radioAa = true;
        $scope.TaxPolicyMaster = {
            SystemID: null,
            TaxPolicyName: null,
            Description: null,
            TaxTypeId: null,
            TaxYearID: null,
            MinimumTaxableAmount: 0.00,
            //GenderID: null,
            CalculationBasis: null,
            CompanyId: null,
            PlantID: null,
            GroupID: null,
            TaxLimitInvestAll: 0,
            TaxFixedTaxInvestAll: 0.00,
            TaxPercentageInvestAll: 0,
            TaxFixedTaxRebate: 0.00,
            TaxPercentageRebate: 0,
            BaseOnIncomeTaxRebate: false,
            //IsGenderSpecific: false,
            IsFixedTaxInvestAll: true,
            IsPercentageTaxInvestAll: false,
            IsBaseOnActEntAmt: false,
            IsLimitInvestAll: false,
            IsFixedTaxRebate: true,
            IsPercentageTaxRebate: false,
            TaxAbleIncomeUpperForRebate: null,
            TaxFixedBonusDefine: 0,
            TaxFixedLvEncash: 0,
            IsFixedTaxBonusDefine: true,
            IsTaxAsPerActual: false,
            IsTaxAsPerProjection: false,
            IsFixedTaxLvEncash: true,
            IsTaxAsPerActualLvEncash: false,
            IsTaxAsPerProjectionLvEncash: false,
            IsCumulativeTaxSlabDefine: false,
            IsBrakeTaxSlabDefine: true,
            Male: true,
            Female: true,
            AgeFrom: null,
            AgeTo: null,
        }
        $scope.GenderCheck = false;
        $scope.radioBr = true;
        $scope.radioCu = false;
        $scope.TaxPolicyGeneral = {
            SystemID: null,
            TaxPolicyMstID: null,
            SalaryHeadID: null,
            IsTaxable: false,
            IsFixedTaxGeneral: true,
            TaxFixedGeneral: 0,
            IsPercentageTaxGeneral: false,
            TaxPercentageGeneral: 0.00,
            IsExemption: false,
            IsExmWhichEverLess: false,
            IsMaxExmpAmt: false,
            TaxMaxExmpAmt: 0.00,
            IsExmBaseOnActual: false,
            IsExmBaseOnOtherSlrHd: false,
            ExmSalaryHeadID: null,
            PercentageExmAmtOtherSlrHd: 0.00,
            IsLessOrMore: 'Which Ever Is Less',
        }

        $scope.TaxSlabDefine = {
            SystemID: null,
            TaxPolicyMstID: null,
            //SlabDefine: null,
            TaxAbleIncome: 0,
            TaxRate: 0,
            SequenceNo: 0,
        }
        //$scope.TaxSlabDefinee = {
        //    Cumulative: false,
        //    BrakeUp: true,
        //}
        $scope.TaxSlabDefineProfessional = {
            Id: null,
            TaxPolicyMasterId: null,
            YearlyMinValue: null,
            YearlyMaxValue: null,
            MonthlyMinValue: null,
            MonthlyMaxValue: null,
            YearlyTaxAmount: null,
            MonthlyTaxAmount: null,
            SeqenceNo: null,
            AdjustingAmount: null,
            MonthOfAdjustment: null,
            PlantID: $scope.TaxPolicyMaster.PlantID,
        }
        $scope.GetSequence($scope.TaxPolicyMaster.SystemID);
        $scope.ProTaxList = [];
        $scope.DataList = [];
        $scope.GeneralTaxList = [];

    };
    //#endregion

    //#region Clear Other
    $scope.ClearIncome = function () {
        $scope.Action = 'Save';
        $scope.TaxSlabDefine = {
            SystemID: null,
            TaxPolicyMstID: $scope.TaxPolicyMaster.SystemID,
            Cumulative: false,
            BrakeUp: true,
            //SlabDefine: null,
            TaxAbleIncome: null,
            TaxRate: null,
            SequenceNo: null,
        }
    };


    $scope.ClearPro = function () {
        $scope.Action = 'Save';
        $scope.TaxSlabDefineProfessional = {
            Id: null,
            TaxPolicyMasterId: $scope.TaxPolicyMaster.SystemID,
            YearlyMinValue: null,
            YearlyMaxValue: null,
            MonthlyMinValue: null,
            MonthlyMaxValue: null,
            YearlyTaxAmount: null,
            MonthlyTaxAmount: null,
            SeqenceNo: null,
            AdjustingAmount: null,
            MonthOfAdjustment: null,
            PlantID: $scope.TaxPolicyMaster.PlantID,
        }
    };

    $scope.ClearGeneral = function () {
        $scope.radioAa = true;
        $scope.Action = 'Save';
        $scope.TaxPolicyGeneral = {
            SystemID: null,
            TaxPolicyMstID: $scope.TaxPolicyMaster.SystemID,
            SalaryHeadID: null,
            IsTaxable: false,
            IsFixedTaxGeneral: true,
            //$scope.radioAa = true,
            TaxFixedGeneral: 0,
            IsPercentageTaxGeneral: false,
            TaxPercentageGeneral: 0.00,
            IsExemption: false,
            IsExmWhichEverLess: false,
            IsMaxExmpAmt: false,
            TaxMaxExmpAmt: 0.00,
            IsExmBaseOnActual: false,
            IsExmBaseOnOtherSlrHd: false,
            ExmSalaryHeadID: null,
            PercentageExmAmtOtherSlrHd: 0.00,
            IsLessOrMore: 'Which Ever Is Less',
        }
        $scope.GetSequence($scope.TaxPolicyMaster.SystemID);
    };

    $scope.ClearRebate = function () {
        $scope.Action = 'Save';
        $scope.TaxRebateSlabDefine = {
            SystemID: null,
            TaxPolicyMstID: $scope.TaxPolicyMaster.SystemID,
            TaxAbleIncomeLowerForRebate: null,
            TaxAbleIncomeUpperForRebate: null,
            //SlabDefine: null,
            InvesmentAmtForRebate: null,
            InvestAmtTaxPercentageRebate: null,
        }
    };
    $scope.ClearFormula = function () {
        $scope.TaxPolicyGeneralFormula = {
            Id: null,
            TaxPolicyGeneralId: $scope.TaxPolicyGeneralFormula.TaxPolicyGeneralId,
            Formula: null,
            FormulaID: null,
            Description: null,
            IsOptionBased: false,
            OptionBasedValue: null
        }
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

    //#region Save General Tax
    $scope.SaveTaxG = function () {
        try {
            $scope.TaxPolicyGeneral.TaxPolicyMstID = $scope.TaxPolicyMaster.SystemID;
            $http({
                method: 'POST',
                url: $scope.path + "SaveGeneral",
                data: { 'GeneralTax': $scope.TaxPolicyGeneral },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getGeneralTax($scope.TaxPolicyGeneral.TaxPolicyMstID);
                    $scope.ClearGeneral($scope.GetSequence($scope.TaxPolicyMaster.SystemID));
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    //#endregion

    //#region save Master
    $scope.Save = function () {
        try {
            ValidationMaster();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'master': $scope.TaxPolicyMaster },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.TaxPolicyMaster.SystemID = response.data.Data.SystemID;
                    $scope.TaxPolicyMaster.TaxYearID = response.data.Data.TaxYearID;
                    $scope.getTaxMonth($scope.TaxPolicyMaster.TaxYearID);
                    $scope.getMaster();
                    $scope.getIncome($scope.TaxPolicyMaster.SystemID);


                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function ValidationMaster() {
        try {
            if ($scope.TaxPolicyMaster.IsFixedTaxInvestAll == true) {
                $scope.TaxPolicyMaster.IsPercentageTaxInvestAll == false;
                $scope.TaxPolicyMaster.TaxPercentageInvestAll = 0;
            }
            else {
                $scope.TaxPolicyMaster.TaxFixedTaxInvestAll = 0;
            }

            if ($scope.TaxPolicyMaster.IsLimitInvestAll == false) {
                $scope.TaxPolicyMaster.TaxLimitInvestAll = 0;
            }

            if ($scope.TaxPolicyMaster.IsFixedTaxRebate == true) {
                $scope.TaxPolicyMaster.IsPercentageTaxRebate == false;
                $scope.TaxPolicyMaster.BaseOnIncomeTaxRebate == false;
                $scope.TaxPolicyMaster.TaxPercentageRebate = 0;
            }
            else {
                $scope.TaxPolicyMaster.TaxFixedTaxRebate = 0;
            }
            if ($scope.TaxPolicyMaster.IsPercentageTaxRebate == true) {
                $scope.TaxPolicyMaster.IsFixedTaxRebate == false;
                $scope.TaxPolicyMaster.BaseOnIncomeTaxRebate == false;
                $scope.TaxPolicyMaster.TaxFixedTaxRebate = 0;
            }
            else {
                $scope.TaxPolicyMaster.TaxPercentageRebate = 0;
            }
            if ($scope.TaxPolicyMaster.BaseOnIncomeTaxRebate == true) {
                $scope.TaxPolicyMaster.IsFixedTaxRebate == false;
                $scope.TaxPolicyMaster.IsPercentageTaxRebate == false;
                $scope.TaxPolicyMaster.TaxFixedTaxRebate = 0;
                $scope.TaxPolicyMaster.TaxPercentageRebate = 0;
            }

            if ($scope.TaxPolicyMaster.IsFixedTaxBonusDefine == false) {
                $scope.TaxPolicyMaster.TaxFixedBonusDefine = 0;
            }
            if ($scope.TaxPolicyMaster.IsFixedTaxLvEncash == false) {
                $scope.TaxPolicyMaster.TaxFixedLvEncash = 0;
            }

        } catch (ex) {
            throw ex;
        }
    };

    //#endregion

    //#region Save Professional Tax
    $scope.SavePro = function () {
        try {
            $scope.TaxSlabDefineProfessional.TaxPolicyMasterId = $scope.TaxPolicyMaster.SystemID;
            $http({
                method: 'POST',
                url: $scope.path + "SaveProfessionalTax",
                data: { 'ProTax': $scope.TaxSlabDefineProfessional },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getProTax($scope.TaxSlabDefineProfessional.TaxPolicyMasterId);
                    $scope.ClearPro();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    //#endregion

    //#region Save Rebate Tax
    $scope.SaveRebate = function () {
        try {
            $scope.TaxRebateSlabDefine.TaxPolicyMstID = $scope.TaxPolicyMaster.SystemID;
            $http({
                method: 'POST',
                url: $scope.path + "SaveRebate",
                data: { 'Rebate': $scope.RebateTaxList, 'MasterID': $scope.TaxPolicyMaster.SystemID, 'InvestmentCredit': $scope.TaxSlabDefineeg },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getRebate($scope.TaxRebateSlabDefine.TaxPolicyMstID);
                    $scope.getMaster();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    //#endregion

    //#region Save Income Slab
    $scope.SaveIncomeSlab = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "SaveIncomeSlab",
                data: { 'IncomeSlab': $scope.DataList, Master: $scope.TaxPolicyMaster.SystemID, Slab: $scope.TaxSlabDefinee },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getIncome($scope.TaxPolicyMaster.SystemID);
                    $scope.getMaster();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    //#endregion

    //#region Delete Master

    $scope.RemoveMaster = function (obj) {
        $scope.TaxPolicyMaster = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.TaxPolicyMaster.SystemID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
    }
    $scope.DeleteMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteMaster?ID=' + $scope.TaxPolicyMaster.SystemID,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Delete Other Tax Details first!");
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Clear();
                $scope.getMaster();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };
    //#endregion

    //#region Delete General

    $scope.Confirm = function (obj) {
        $scope.TaxPolicyGeneral = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.TaxPolicyGeneral.SystemID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    }


    $scope.DeleteGeneral = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "DeleteGeneral",
                data: { ID: $scope.TaxPolicyGeneral.SystemID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getGeneralTax($scope.TaxPolicyGeneral.TaxPolicyMstID);
                    $scope.ClearGeneral($scope.GetSequence($scope.TaxPolicyMaster.SystemID));
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.confirmdelete = false;
    $scope.ConfirmDeleteFormula = function (obj) {
        $scope.TaxPolicyGeneralFormula.Id = obj.data.Id;
        var eDialog = $("#GeneralFormulaDelete").data("ejDialog");
        eDialog.open();
        $("#GeneralFormulaDelete_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmDeleteFormulaClose = function () {
        var eDialog = $("#GeneralFormulaDelete").data("ejDialog");
        eDialog.close();
    };

    $scope.DeleteGeneralFormula = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "DeleteGeneralFormula",
                data: { ID: $scope.TaxPolicyGeneralFormula.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.getGeneralTaxFormula($scope.TaxPolicyGeneralFormula.TaxPolicyGeneralId);
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

    //#region Delete Professional Tax

    $scope.ConfirmProfessional = function (obj) {
        $scope.TaxSlabDefineProfessional = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.TaxSlabDefineProfessional.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmProcessPro')).modal('show');
    }




    $scope.DeleteProTax = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "DeleteProfessionalTax",
                data: { ID: $scope.TaxSlabDefineProfessional.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearPro();
                    $scope.getProTax($scope.TaxSlabDefineProfessional.TaxPolicyMasterId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //#endregion

    //#region Delete Rebate

    $scope.confirmdelete = false;
    $scope.ConfirmRebate = function () {
        var eDialog = $("#rebate").data("ejDialog");
        eDialog.open();
        $("#rebate_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmrebateClose = function () {
        var eDialog = $("#rebate").data("ejDialog");
        eDialog.close();
    };


    $scope.DeleteRebatex = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "DeleteRebate",
                data: { ID: $scope.TaxPolicyMaster.SystemID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearRebate();
                    $scope.getRebate($scope.TaxPolicyMaster.SystemID);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //#endregion

    //#region Delete Income

    $scope.ConfirmIncome = function (obj) {
        $scope.TaxSlabDefine = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.TaxSlabDefine.SystemID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmProcessIncome')).modal('show');
    }


    $scope.DeleteIncome = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "DeleteIncome",
                data: { ID: $scope.TaxSlabDefine.SystemID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearIncome();
                    $scope.getIncome($scope.TaxSlabDefine.TaxPolicyMstID);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteIncomeSlabe = function (obj) {
        $scope.DeleteIncomeSlab = $scope.TaxPolicyMaster.SystemID;
        if (!baseService.isUndefinedOrNull($scope.DeleteIncomeSlab))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmProcessIncomeSlab')).modal('show');
    }
    $scope.DeleteIncomeSlabs = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "DeleteIncomeSlab",
                data: { ID: $scope.DeleteIncomeSlab},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearIncome();
                    $scope.getIncome($scope.TaxPolicyMaster.SystemID);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //#endregion

    //#region s e q u e n c e

    $scope.GetSequence = function (MasterID) {
        $http({
            method: 'GET',
            url: 'Payrolls/TaxPolicy/getautosequence?MasterID=' + MasterID,
        }).then(function successCallback(response) {
            $scope.TaxPolicyGeneral.Sequence = response.data;

        });
    };

    //#endregion

    //#region Tax Rebate
    $scope.TaxRebateList = [];
    $scope.TaxRebateSlab = {
        SystemID: null,
        TaxPolicyMstID: $scope.TaxPolicyMaster.SystemID,
        Minimum: null,
        Maximum: null,
        TaxRate: null,
    }
    //$scope.TaxRebateSlabb = Object.assign({}, $scope.TaxRebateSlab);
    $scope.TaxRebateList.push(Object.assign({}, $scope.TaxRebateSlab));

    $scope.SubmitTaxRebate = function (data) {

        try {
            if (data.Minimum < 0)
                throw 'Minimum value cannot be negative';
            if (data.Minimum == null) {
                throw 'Enter Minimum Value';
            }

            if (data.Maximum < 0)
                throw 'Maximum value cannot be negative';


            if (data.Minimum >= data.Maximum)
                throw 'Maximum value should be greater than minimum value';



            var newObj = Object.assign({}, $scope.TaxRebateSlab);
            if (data != null) {
                newObj = {
                    SystemID: null,
                    TaxRate: null,
                    Minimum: data.Maximum,
                    Maximum: null,
                    TaxPolicyMstID: $scope.TaxPolicyMaster.SystemID,
                }
            }

            $scope.TaxRebateList.push(newObj);
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };
    $scope.RemoveTaxRebate = function () {
        var removed = $scope.TaxRebateList.splice(index, 1);
        $scope.Detail = removed;
        if ($scope.TaxRebateList.length == 0) {
            $scope.TaxRebateList.push(Object.assign({}, $scope.TaxRebateSlab));
        }
    };
    $scope.SaveTaxRebate = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "SaveTaxRebate",
                data: { 'TaxRebateList': $scope.TaxRebateList, 'Master': $scope.TaxPolicyMaster.SystemID, 'Slab': $scope.TaxRebate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getTaxRebate($scope.TaxPolicyMaster.SystemID);
                    $scope.getMaster();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.getTaxRebate = function (Master) {
        $http({
            method: 'GET',
            url: $scope.path + "GetTaxRebate?Master=" + Master,
        }).then(function successCallback(response) {
            if (response.data.length == 0) {
                $scope.TaxRebateList = [];
                $scope.TaxRebateList.push(Object.assign({}, $scope.TaxRebateSlab));
            }
            else {
                $scope.TaxRebateList = response.data;
            }
        });
    };

    //#endregion
}
