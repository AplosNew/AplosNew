'use strict';
salaryRuleController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function salaryRuleController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Salary Rule";
    $scope.Action = 'Save';
    $scope.Row = 'Add Row';
    $scope.payrollGroupMasters = [];
    $scope.path = 'Payrolls/salaryRule/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.savesalaryrulegeneralUrl = $scope.path + 'createsalaryrulegeneral';
    $scope.saveSalaryHeadUrl = $scope.path + 'CreateSalaryHeadSetting';
    $scope.saveESICSalaryHeadUrl = $scope.path + 'CreateESICSalaryHead';
    $scope.saveRtnBonusSalaryHeadUrl = $scope.path + 'CreateRetentionBonusSalaryHead';
    $scope.saveAttdnBonusSalaryHeadUrl = $scope.path + 'CreateAttnBonusSalaryHead';
    $scope.saveAbsenteeismSalaryHeadUrl = $scope.path + 'CreateAbsenteeismSalaryHead';
    $scope.saveOTSalaryHeadUrl = $scope.path + 'CreateOTSalaryHead';
    $scope.savePFSalaryHeadUrl = $scope.path + 'CreatePFSalaryHead';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.salaryRule = {
        SystemID: null,
        SalaryRuleName: null,
        SalaryRuleDescription: null,
        CurrencyRuleSystemID: null,
        GroupID: $window.companyGroupId,
        PlantID: null,
        IsUsed: null,
        TotalSalaryId: null,
        TaxGroupID: null,
        IsValidGovGrd: null,
        IsActive: true,
        IncomeTaxGroup: null,
        AddedFromIP: null,
        UpdatedFromIP: null
    };
    $scope.salaryRuleNew = Object.assign({}, $scope.salaryRule);

    $scope.salaryRuleGeneral = {
        IsFormulaDefine: false,
        IsFixed: false,
        IsNA: true,
        FixedValue: 0,
        IsOpenValue: false,
        IsDaysInAMonth: false,
        IsWorkDaysInAMonth: false,
        IsWorkDaysInAMonthIncHold: false,
        IsFixedDisbusment: false,
        RefAbsentism: false,
        SalaryRuleGeneralSystemID: null,
        SalaryRuleMasterSystemID: null,
        SalaryHeadID: null,
        IsGNRNetPayEffect: false,
        IsGNRTagAndUnTag: false,
        IsOpen: false,
        IsFormula: false,
        FormulaDes: null,
        FormulaDesID: null,
        IsFixedMonthDay: false,
        FixedMonthDayValue: null,
        IsMonthDay: true,
        IsMonthWorkDay: false,
        IsFixedDisbus: false,
        SequenceNo: 0,
        IsDisbusted: false,
        IsBankPayment: false,
        IsCashPayment: false,
        BaseOnNetPay: false,
        IsCTCComponent: false,
        IsGrossComponent: false,
        AddedBy: null,
        DateAdded: null,
        UpdatedBy: null,
        DateUpdated: null,
        GNRBaseOthSlrHDFormula: null,
        GNRApplicableMonthNo: null,
        IsGNRBaseOthSlrHD: null,
        IsRetain: false,
        IsMinWages: false,
        IsGNRWhichEverLess: false,
        HasMaxLimit: false,
        FixedMaxLimit: false,
        MaxLimitValue: 0,
        PercentageMaxLimit: false,
        PercentageMaxLimitSalaryHeadId: null,
        HasMinLimit: false,
        FixedMinLimit: false,
        MinLimitValue: 0,
        PercentageMinLimit: false,
        PercentageMinLimitSalaryHeadId: null,
        IsSlabBased: false,
        IsPayOnWeekoffForFixedMonthDay: false,
        IsPayOnHolidayForFixedMonthDay: false,
        AddedFromIP: null,
        UpdatedFromIP: null
    }

    $scope.salaryRuleAbsent = {
        SalaryRuleAbsenteeismSystemID: null, SalaryRuleMasterSystemID: null, SalaryHeadID: null, IsAbsNetPayEffect: false, IsAbsTagAndUnTag: false, IsFixed: true, FixedValue: 0, IsFormula: false, FormulaDes: null, FormulaDesID: null, IsFixedMonthDay: true, FixedMonthDayValue: 0, IsMonthDay: false, IsMonthWorkDay: false, IsFixedDisbus: false, SequenceNo: 0, IsDisbusted: false, SalaryHeadFormula: null, SalaryHeadIdFormula: null, IsDeductionOnGross:false
    }
    $scope.ShowResultCustom = function (message, type) {
        $("#TaxGroupPoUp").ejDialog("setTitle", "Tax Group");
        var eDialog = $("#TaxGroupPoUp").data("ejDialog");
        eDialog.open();
    };

    $scope.CloseTaxGroup = function () {
        var eDialog = $("#TaxGroupPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.currencyRuleList = [];
    cboService.getCurrencyRuleCbo(function (data) {
        $scope.currencyRuleList = data;
    });

    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });

    $scope.TaxGroupList = [];
    $scope.getTaxGroup = function () {
        $http.get("payrolls/salaryRule/SearchTaxGrpInfo")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.TaxGroupList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        $scope.ShowResultCustom();
    };

    $scope.SetTaxData = function (obj) {
        var gridObj = $("#GridTaxGroup").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.salaryRuleNew.TaxGroupID = data.SystemID;
        $scope.salaryRuleNew.IncomeTaxGroup = data.TaxGroupName;

        $scope.CloseTaxGroup();
    }

    $scope.ClearTaxGroup = function () {
        $scope.salaryRuleNew.TaxGroupID = null;
        $scope.salaryRuleNew.IncomeTaxGroup = null;
    }

    $scope.salaryRuleList = [];
    $scope.getSavedData = function () {
        $scope.salaryRuleList = [];
        $http.get("payrolls/salaryRule/getsalaryRulelist")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.salaryRuleList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSavedData();

    $scope.salaryHeadCboList = [];
    $scope.getSH = function () {
        //cboService.getSHCbo($scope.salaryRuleNew.CurrencyRuleSystemID, function (result) {
        //    $scope.salaryHeadList = result;
        //   // console.log($scope.salaryHeadList);
        //});
        $scope.salaryHeadCboList = [];

        $http.get("payrolls/salaryRule/GetSalaryHeadCbo?currencyRuleSystemID=" + $scope.salaryRuleNew.CurrencyRuleSystemID)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.salaryHeadCboList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    }

    $scope.Get = function (obj) {
        $scope.showFormula = false;
        $scope.salaryRuleNew = {};
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.salaryRuleNew = data;
        $scope.getsalaryRuleGeneral($scope.salaryRuleNew.SystemID);
        $scope.getSH();
        $scope.getAutoSequence($scope.salaryRuleNew.SystemID);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };

    $scope.salaryRuleGeneralList = [];
    $scope.getsalaryRuleGeneral = function (strSalaryRuleID) {
        $scope.salaryRuleGeneralList = [];
        $http.get("payrolls/salaryRule/LoadSalaryRuleGeneral?strSalaryRuleID=" + strSalaryRuleID)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.salaryRuleGeneralList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.getAutoSequence = function (SalaryRuleMasterSystemID) {
        $http.get("payrolls/SalaryRule/GetAutoSequence?SalaryRuleMasterSystemID=" + SalaryRuleMasterSystemID)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.salaryRuleGeneral.SequenceNo = response.data[0].SequenceNo;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    $scope.SalaryRuleESICSystemID = null;
    $scope.salaryRuleExtraList = [];
    $scope.getSalaryRuleESIC = function () {
        $scope.salaryRuleExtraList = [];
        $scope.SalaryRuleESIC = [];
        try {
            $http.get("payrolls/SalaryRule/GetSalaryRuleESIC?strSalaryRuleID=" + $scope.salaryRuleNew.SystemID + '&currencyRuleSystemID=' + $scope.salaryRuleNew.CurrencyRuleSystemID)
                .then(
                    function successCallback(response) {
                        try {
                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.SalaryRuleESICSystemID = response.data[0];
                                $scope.salaryRuleExtraList = response.data;
                                $scope.SalaryRuleESIC = response.data;
                                //if (baseService.isUndefinedOrNull($scope.SalaryRuleESICSystemID)) {
                                //    throw "Please save ESIC data.";
                                //}

                            }
                        } catch (e) {
                            throw e;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            throw e;
        }
    };

    $scope.MinimumWagesSalaryHeadModel = {
        CheckBoxSelect: false,
        SalaryHeadId: null,
        MinimumWagesSalaryHeadId: null,
        MinimumWagesSalaryHead: null
    };
    $scope.divBonusRetainIsMinimumWages = false;
    $scope.MinimumWagesSalaryHeadList = [];
    $scope.SalaryHeadId = null;

    $scope.BonusRetainIsMinimumWagesChange = function () {
        
        try {
           
            $scope.divBonusRetainIsMinimumWages = false;


            for (var i = 0; i < $scope.salaryRuleExtraList.length; i++) {
                if ($scope.salaryRuleExtraList[i].IsBonusRetainMinimumWages==true) {
                    $scope.divBonusRetainIsMinimumWages = true;
                }
            }


        } catch (e) {
            throw e;
        }
    };

    $scope.SalaryRuleRetentionPmtSystemID = null;

    $scope.SalaryRuleGovtGrd = [];
    $scope.getSalaryRuleRetentionBonus = function () {
        $scope.salaryRuleExtraList = [];
        $scope.SalaryRuleGovtGrd = [];
        $scope.SalaryHeadId = null;
        $scope.SalaryRuleRetentionBonus = [];
        try {
            $http.get("payrolls/SalaryRule/GetSalaryRuleRetentionBonus?strSalaryRuleID=" + $scope.salaryRuleNew.SystemID + '&currencyRuleSystemID=' + $scope.salaryRuleNew.CurrencyRuleSystemID)
                .then(
                    function successCallback(response) {
                        try {
                            
                            $scope.SalaryRuleGovtGrd = response.data.SalaryRuleGovtGrd;
                            if (baseService.arrayLength(response.data.SalaryRuleRetentionBonus) > 0) {
                                $scope.salaryRuleExtraList = response.data.SalaryRuleRetentionBonus;
                                $scope.SalaryRuleRetentionBonus = response.data.SalaryRuleRetentionBonus;
                                $scope.SalaryRuleRetentionPmtSystemID = response.data.SalaryRuleRetentionBonus;
                                //if (baseService.isUndefinedOrNull($scope.SalaryRuleRetentionPmtSystemID)) {
                                //    throw "Please save Bonus Retain data.";
                                //}
                            }
                            for (var i = 0; i < $scope.salaryRuleExtraList.length; i++) {                             

                                if (baseService.arrayLength($scope.SalaryRuleGovtGrd) > 0) {
                                    $scope.salaryRuleExtraList[i].IsBonusRetainMinimumWages = true;
                                    $scope.divBonusRetainIsMinimumWages = true;
                                } else {
                                    $scope.salaryRuleExtraList[i].IsBonusRetainMinimumWages = false;
                                    $scope.divBonusRetainIsMinimumWages = false;
                                } 
                            }




                            for (var i = 0; i < $scope.salaryHeadList.length; i++) {
                                $scope.MinimumWagesSalaryHeadModel = {
                                    CheckBoxSelect: false,
                                    SalaryHeadId: null,
                                    MinimumWagesSalaryHeadId: null,
                                    MinimumWagesSalaryHead: null
                                };

                                if (baseService.arrayLength($scope.SalaryRuleGovtGrd) > 0) {
                                    for (var j = 0; j < $scope.SalaryRuleGovtGrd.length; j++) {
                                        if ($scope.SalaryRuleGovtGrd[j].GovtSalaryHeadID == $scope.salaryHeadList[i].Value) {
                                            $scope.MinimumWagesSalaryHeadModel.CheckBoxSelect = true;
                                        }
                                        $scope.SalaryHeadId = $scope.SalaryRuleGovtGrd[j].SalaryHeadID;
                                    }
                                } else {
                                    $scope.MinimumWagesSalaryHeadModel.CheckBoxSelect = false;
                                }



                                $scope.MinimumWagesSalaryHeadModel.MinimumWagesSalaryHeadId = $scope.salaryHeadList[i].Value;
                                $scope.MinimumWagesSalaryHeadModel.MinimumWagesSalaryHead = $scope.salaryHeadList[i].Text;
                                $scope.MinimumWagesSalaryHeadList.push($scope.MinimumWagesSalaryHeadModel);

                            }

                            var gridObj = $("#GridMinimumWagesSalaryHeadList").data("ejGrid");
                            gridObj.refreshContent();



                        } catch (e) {
                            throw e;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                });



           

        } catch (e) {
            throw e;
        }
    };

    $scope.salaryAbsentCboList = [];
    $scope.getSalaryRuleAbsenteeism = function () {
        $http.get("payrolls/SalaryRule/GetSalaryRuleAbsenteeism?strSalaryRuleID=" + $scope.salaryRuleNew.SystemID + '&currencyRuleSystemID=' + $scope.salaryRuleNew.CurrencyRuleSystemID)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.salaryAbsentCboList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.GetSavedSalaryRuleAbsenteeism = function () {
        $scope.salaryRuleExtraList = [];
        $scope.SalaryRuleAbsenteeism = [];
        try {
            $http.get("payrolls/SalaryRule/GetSavedSalaryRuleAbsenteeism?strSalaryRuleID=" + $scope.salaryRuleNew.SystemID)
                .then(
                    function successCallback(response) {
                        try {
                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.salaryRuleExtraList = response.data;
                                $scope.SalaryRuleAbsenteeism = response.data;
                                $scope.salaryRuleAbsent.SalaryRuleAbsenteeismSystemID = response.data[0].SalaryRuleAbsenteeismSystemID;
                                //if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.SalaryRuleAbsenteeismSystemID)) {
                                //    throw "Please save Absenteeism data.";
                                //}
                            }
                        } catch (e) {
                            throw e;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            throw e;
        }
    };

    $scope.SetAbsentSalaryData = function (data) {
        $scope.getSalaryRuleAbsenteeism();
        $scope.salaryRuleAbsent = data;
        $scope.salaryRuleAbsent.FormulaDescription = $scope.salaryRuleAbsent.FormulaDes;
        $scope.salaryRuleAbsent.FormulaIDDescription = $scope.salaryRuleAbsent.FormulaDesID;
        if ($scope.salaryRuleAbsent.IsFormula) {
            $scope.showFormula = true;
            $scope.fixdisabled = false;
        }
    }

    $scope.RemoveAbsentData = function (data) {
        $scope.salaryRuleAbsent = data;
        if (!baseService.isUndefinedOrNull($scope.salaryRuleAbsent.SalaryRuleAbsenteeismSystemID))
            $scope.message_confirmation = 'Are you sure want to delete permanently [' + $scope.salaryRuleAbsent.SalaryHead + ' ]';
        angular.element(document.querySelector('#confirmRuleAbsentPopUp')).modal('show');
    };

    $scope.DeleteSalaryAbsent = function () {
        $http({
            method: 'POST',
            url: 'Payrolls/SalaryRule/DeleteSalaryRuleAbsent?id=' + $scope.salaryRuleAbsent.SalaryRuleAbsenteeismSystemID
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedSalaryRuleAbsenteeism();
                $scope.salaryRuleAbsent = {};
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    $scope.SalaryRuleAttdnBonusId = null;
    $scope.getSalaryRuleAttdnBonus = function () {
        $scope.salaryRuleExtraList = [];
        $scope.SalaryRuleAttdnBonus = [];
        try {
            $http.get("payrolls/SalaryRule/GetSalaryRuleAttdnBonus?strSalaryRuleID=" + $scope.salaryRuleNew.SystemID + '&currencyRuleSystemID=' + $scope.salaryRuleNew.CurrencyRuleSystemID)
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.salaryRuleExtraList = response.data;
                            $scope.SalaryRuleAttdnBonusId = response.data[0];
                            $scope.SalaryRuleAttdnBonus = response.data;
                            //if (baseService.isUndefinedOrNull($scope.SalaryRuleAttdnBonusId)) {
                            //    throw "Please save Attendance Bonus data.";
                            //}

                        }
                        throw "Please save Attendance Bonus data.";
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            throw e;
        }
    };

    $scope.getSalaryRuleOT = function () {
        $scope.salaryRuleExtraList = [];
        $scope.SalaryRuleOT = [];
        try {
            $http.get("payrolls/SalaryRule/GetSalaryRuleOT?strSalaryRuleID=" + $scope.salaryRuleNew.SystemID + '&currencyRuleSystemID=' + $scope.salaryRuleNew.CurrencyRuleSystemID)
                .then(
                    function successCallback(response) {
                        try {
                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.salaryRuleExtraList = response.data;
                                $scope.SalaryRuleOT = response.data;
                            }

                            //if (baseService.arrayLength($scope.OTSalaryHeadSaveList) === 0) {
                            //    throw "Please save OT data.";
                            //}
                        } catch (e) {
                            throw e;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            throw e;
        }
    };

    $scope.SalaryRulePFSystemID = null;
    $scope.getSalaryRulePF = function () {
        $scope.salaryRuleExtraList = [];
        $scope.SalaryRulePF = [];
        try {
            $http.get("payrolls/SalaryRule/GetSalaryRulePF?strSalaryRuleID=" + $scope.salaryRuleNew.SystemID + '&currencyRuleSystemID=' + $scope.salaryRuleNew.CurrencyRuleSystemID)
                .then(
                    function successCallback(response) {
                        try {
                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.salaryRuleExtraList = response.data;
                                $scope.SalaryRulePF = response.data;
                                $scope.SalaryRulePFSystemID = response.data[0];
                            }
                            //if (baseService.isUndefinedOrNull($scope.SalaryRulePFSystemID)) {
                            //    throw "Please save PF data.";
                            //}
                        } catch (e) {
                            throw e;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            throw e;
        }
    };

    $scope.head = null;
    $scope.ExtraSalaryHeadPop = function (head) {
        $scope.head = head;
        if (head === 'ESIC') {
            $scope.getSalaryRuleESIC();
            angular.element(document.querySelector('#AdditionalSalaryPopUp')).modal('show');
        }
        if (head === 'Absenteeism') {
            $scope.getSalaryRuleAbsenteeism();
            $scope.GetSavedSalaryRuleAbsenteeism();
            angular.element(document.querySelector('#AbsenteeismPopUp')).modal('show');
        }
        if (head === 'AttendanceBonus') {
            $scope.getSalaryRuleAttdnBonus();
            angular.element(document.querySelector('#AdditionalSalaryPopUp')).modal('show');
        }
        if (head === 'BonusRetain') {
            $scope.getSalaryRuleRetentionBonus();
            angular.element(document.querySelector('#BonusRetainPopUp')).modal('show');
        }
        if (head === 'OT') {
            $scope.getSalaryRuleOT();
            angular.element(document.querySelector('#AdditionalSalaryPopUp')).modal('show');
        }
        if (head === 'PF') {
            $scope.getSalaryRulePF();
            angular.element(document.querySelector('#AdditionalSalaryPopUp')).modal('show');
        }

    }

    $scope.SetAbsentFormula = function (formula) {

        if (formula === 'SHead') {
            $scope.salaryRuleAbsent.FormulaDescription = null;
            $scope.salaryRuleAbsent.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleAbsent.SalaryHeadIdFormula)) {
                $scope.salaryRuleAbsent.SalaryHeadFormula = $("#AbsSalaryHeadFormula option:selected").text();

                $scope.salaryRuleAbsent.FormulaDes = $scope.salaryRuleAbsent.SalaryHeadFormula;
                $scope.salaryRuleAbsent.FormulaDesID = $scope.salaryRuleAbsent.SalaryHeadIdFormula;
            }

            $scope.FormulaArray.push($scope.salaryRuleAbsent.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleAbsent.FormulaDesID);

            $scope.salaryRuleAbsent.FormulaDescription = null;
            $scope.salaryRuleAbsent.FormulaIDDescription = null;

            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaDescription)) {
                    $scope.salaryRuleAbsent.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleAbsent.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaIDDescription)) {
                    $scope.salaryRuleAbsent.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleAbsent.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Operator') {
            $scope.salaryRuleAbsent.FormulaIDDescription = null;
            $scope.salaryRuleAbsent.FormulaDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleAbsent.Operator)) {
                $scope.salaryRuleAbsent.FormulaDes = $scope.salaryRuleAbsent.Operator;
                $scope.salaryRuleAbsent.FormulaDesID = $scope.salaryRuleAbsent.Operator;
            }
            $scope.FormulaArray.push($scope.salaryRuleAbsent.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleAbsent.FormulaDesID);

            $scope.salaryRuleAbsent.FormulaIDDescription = null;
            $scope.salaryRuleAbsent.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaDescription)) {
                    $scope.salaryRuleAbsent.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleAbsent.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaIDDescription)) {
                    $scope.salaryRuleAbsent.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleAbsent.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Precedence') {
            $scope.salaryRuleAbsent.FormulaDescription = null;
            $scope.salaryRuleAbsent.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleAbsent.Precedence)) {
                $scope.salaryRuleAbsent.FormulaDes = $scope.salaryRuleAbsent.Precedence;
                $scope.salaryRuleAbsent.FormulaDesID = $scope.salaryRuleAbsent.Precedence;
            }
            $scope.FormulaArray.push($scope.salaryRuleAbsent.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleAbsent.FormulaDesID);

            $scope.salaryRuleAbsent.FormulaIDDescription = null;
            $scope.salaryRuleAbsent.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaDescription)) {
                    $scope.salaryRuleAbsent.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleAbsent.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaIDDescription)) {
                    $scope.salaryRuleAbsent.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleAbsent.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Value') {
            $scope.salaryRuleAbsent.FormulaDescription = null;
            $scope.salaryRuleAbsent.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleAbsent.Value)) {
                $scope.salaryRuleAbsent.FormulaDes = $scope.salaryRuleAbsent.Value;
                $scope.salaryRuleAbsent.FormulaDesID = $scope.salaryRuleAbsent.Value;
            }
            $scope.FormulaArray.push($scope.salaryRuleAbsent.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleAbsent.FormulaDesID);

            $scope.salaryRuleAbsent.FormulaIDDescription = null;
            $scope.salaryRuleAbsent.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaDescription)) {
                    $scope.salaryRuleAbsent.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleAbsent.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaIDDescription)) {
                    $scope.salaryRuleAbsent.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleAbsent.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
    }

    $scope.RemoveAbsentFormula = function () {
        $scope.salaryRuleAbsent.FormulaDesID = null;

        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);

        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);

        $scope.salaryRuleAbsent.FormulaDescription = null;
        $scope.salaryRuleAbsent.FormulaIDDescription = null;
        $scope.salaryRuleAbsent.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaDescription)) {
                $scope.salaryRuleAbsent.FormulaDes = $scope.FormulaArray[i];
                $scope.salaryRuleAbsent.FormulaDescription = $scope.FormulaArray[i];


            } else {
                $scope.salaryRuleAbsent.FormulaDes += $scope.FormulaArray[i];
                $scope.salaryRuleAbsent.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }

        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.FormulaIDDescription)) {
                $scope.salaryRuleAbsent.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.salaryRuleAbsent.FormulaIDDescription = $scope.FormulaIdArray[i];


            } else {
                $scope.salaryRuleAbsent.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.salaryRuleAbsent.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    }

    $scope.SaveExtraSalaryHead = function () {
        if ($scope.head === 'ESIC') {
            $scope.SaveESICSalaryHead();
        }
        if ($scope.head === 'Absenteeism') {
            $scope.SaveAbsenteeismSalaryHead();
        }
        if ($scope.head === 'AttendanceBonus') {
            $scope.SaveAttendanceBonusSalaryHead();
        }
        if ($scope.head === 'BonusRetain') {
            $scope.SaveRtnBonusSalaryHead();
        }
        if ($scope.head === 'OT') {
            $scope.SaveOTSalaryHead();
        }
        if ($scope.head === 'PF') {
            $scope.SavePFSalaryHead();
        }
    }

    $scope.ESICSalaryHeadSaveList = [];
    $scope.SaveESICSalaryHead = function () {
        $scope.ESICSalaryHeadSaveList = [];
        try {
            for (var i = 0; i < $scope.salaryRuleExtraList.length; i++) {
                if ($scope.salaryRuleExtraList[i].Active === true) {
                    $scope.ESICSalaryHeadSaveList.push($scope.salaryRuleExtraList[i]);
                }
            }

            for (var i = 0; i < $scope.ESICSalaryHeadSaveList.length; i++) {
                $scope.ESICSalaryHeadSaveList[i].SalaryRuleMasterSystemID = $scope.salaryRuleNew.SystemID;
            }

            $http({
                method: 'POST',
                url: $scope.saveESICSalaryHeadUrl,
                data: { 'entities': $scope.ESICSalaryHeadSaveList, 'SalaryRuleMasterSystemID': $scope.salaryRuleNew.SystemID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.CloseESIC();
                    $scope.getSalaryRuleESIC();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.RtnBonusSalaryHeadSaveList = [];
    $scope.SaveRtnBonusSalaryHead = function () {
        $scope.RtnBonusSalaryHeadSaveList = [];
        try {
            for (var i = 0; i < $scope.salaryRuleExtraList.length; i++) {
                if ($scope.salaryRuleExtraList[i].Active === true) {
                    $scope.RtnBonusSalaryHeadSaveList.push($scope.salaryRuleExtraList[i]);
                }
            }

            for (var i = 0; i < $scope.RtnBonusSalaryHeadSaveList.length; i++) {
                $scope.RtnBonusSalaryHeadSaveList[i].SalaryRuleMasterSystemID = $scope.salaryRuleNew.SystemID;
            }

            //$scope.MinimumWagesSalaryHeadList = [];
            //$scope.SalaryHeadId = null;

            var MinimumWagesSalaryHeadLists = [];

            for (var i = 0; i < $scope.MinimumWagesSalaryHeadList.length; i++) {
                if ($scope.MinimumWagesSalaryHeadList[i].CheckBoxSelect === true) {
                    MinimumWagesSalaryHeadLists.push($scope.MinimumWagesSalaryHeadList[i]);
                }
            }


            for (var i = 0; i < $scope.RtnBonusSalaryHeadSaveList.length; i++) {
                if ($scope.RtnBonusSalaryHeadSaveList[i].IsBonusRetainMinimumWages === true) {
                    if (baseService.isUndefinedOrNull($scope.SalaryHeadId)) {
                        throw "Please add Minimum Wages Salary Head.";
                    }
                    if (baseService.arrayLength(MinimumWagesSalaryHeadLists) === 0) {
                        throw "Please add Minimum Wages Salary Head.";
                    }
                }
            }


            $http({
                method: 'POST',
                url: $scope.saveRtnBonusSalaryHeadUrl,
                data: { 'entities': $scope.RtnBonusSalaryHeadSaveList, 'MinimumWagesSalaryHeadLists': MinimumWagesSalaryHeadLists, 'SalaryHeadId': $scope.SalaryHeadId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector('#BonusRetainPopUp')).modal('hide');
                    $scope.getSalaryRuleRetentionBonus();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.AttdnBonusSalaryHeadSaveList = [];
    $scope.SaveAttendanceBonusSalaryHead = function () {
        $scope.AttdnBonusSalaryHeadSaveList = [];
        try {
            for (var i = 0; i < $scope.salaryRuleExtraList.length; i++) {
                if ($scope.salaryRuleExtraList[i].Active === true) {
                    $scope.AttdnBonusSalaryHeadSaveList.push($scope.salaryRuleExtraList[i]);
                }
            }

            for (var i = 0; i < $scope.AttdnBonusSalaryHeadSaveList.length; i++) {
                $scope.AttdnBonusSalaryHeadSaveList[i].SalaryRuleMasterSystemID = $scope.salaryRuleNew.SystemID;
            }

            $http({
                method: 'POST',
                url: $scope.saveAttdnBonusSalaryHeadUrl,
                data: { 'entities': $scope.AttdnBonusSalaryHeadSaveList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.CloseESIC();
                    $scope.getSalaryRuleAttdnBonus();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveAbsenteeismSalaryHead = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.salaryRuleAbsent.SalaryHeadID)) {
                throw "Select Salary Head.";
            }
            $scope.salaryRuleAbsent.SalaryRuleMasterSystemID = $scope.salaryRuleNew.SystemID;
            //$scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleAbsent.FormulaDescription;
            //$scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleAbsent.FormulaIDDescription;
            $scope.salaryRuleAbsent.FormulaDes = $scope.salaryRuleAbsent.FormulaDescription;
            $scope.salaryRuleAbsent.FormulaDesID = $scope.salaryRuleAbsent.FormulaIDDescription;

            $http({
                method: 'POST',
                url: $scope.saveAbsenteeismSalaryHeadUrl,
                data: { 'entity': $scope.salaryRuleAbsent, 'SalaryRuleMasterSystemID': $scope.salaryRuleNew.SystemID},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'AbsenteeismPopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'AbsenteeismPopUp');
                    $scope.CloseESIC();
                    $scope.GetSavedSalaryRuleAbsenteeism();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'AbsenteeismPopUp');
            };

        } catch (e) {
            ShowResult(e, "failure", 'AbsenteeismPopUp');
        }
    };

    $scope.OTSalaryHeadSaveList = [];
    $scope.SaveOTSalaryHead = function () {
        $scope.OTSalaryHeadSaveList = [];
        try {
            for (var i = 0; i < $scope.salaryRuleExtraList.length; i++) {
                if ($scope.salaryRuleExtraList[i].Active === true) {
                    $scope.OTSalaryHeadSaveList.push($scope.salaryRuleExtraList[i]);
                }
            }

            for (var i = 0; i < $scope.OTSalaryHeadSaveList.length; i++) {
                $scope.OTSalaryHeadSaveList[i].SalaryRuleMasterSystemID = $scope.salaryRuleNew.SystemID;
            }

            $http({
                method: 'POST',
                url: $scope.saveOTSalaryHeadUrl,
                data: { 'entities': $scope.OTSalaryHeadSaveList, 'SalaryRuleMasterSystemID': $scope.salaryRuleNew.SystemID},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.CloseESIC();
                    $scope.getSalaryRuleOT();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.PFSalaryHeadSaveList = [];
    $scope.SavePFSalaryHead = function () {
        $scope.PFSalaryHeadSaveList = [];
        try {
            for (var i = 0; i < $scope.salaryRuleExtraList.length; i++) {
                if ($scope.salaryRuleExtraList[i].Active === true) {
                    $scope.PFSalaryHeadSaveList.push($scope.salaryRuleExtraList[i]);
                }
            }

            for (var i = 0; i < $scope.PFSalaryHeadSaveList.length; i++) {
                $scope.PFSalaryHeadSaveList[i].SalaryRuleMasterSystemID = $scope.salaryRuleNew.SystemID;
            }

            $http({
                method: 'POST',
                url: $scope.savePFSalaryHeadUrl,
                data: { 'entities': $scope.PFSalaryHeadSaveList, 'SalaryRuleMasterSystemID': $scope.salaryRuleNew.SystemID},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.CloseESIC();
                    $scope.getSalaryRulePF();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CloseESIC = function () {
        angular.element(document.querySelector('#AdditionalSalaryPopUp')).modal('hide');
        angular.element(document.querySelector('#AbsenteeismPopUp')).modal('hide');
    }

    $scope.SalaryHeadPop = function () {
        $scope.SalaryHeadSetting();
        angular.element(document.querySelector('#SalaryHeadSettingPopUp')).modal('show');
    }

    $scope.SalaryRuleESIC = [];
    $scope.SalaryRuleRetentionBonus = [];
    $scope.SalaryRuleAttdnBonus = [];
    $scope.SalaryRuleOT = [];
    $scope.SalaryRulePF = [];
    $scope.SalaryRuleAbsenteeism = [];

    $scope.salaryHeadSettingList = [];

    $scope.SalaryHeadSetting = function () {

        $scope.SalaryRuleESIC = [];
        $scope.SalaryRuleRetentionBonus = [];
        $scope.SalaryRuleAttdnBonus = [];
        $scope.SalaryRuleOT = [];
        $scope.SalaryRulePF = [];
        $scope.SalaryRuleAbsenteeism = [];

        cboService.getEnumCbo("enum/GetSalaryHeadEnum", function (result) {
            //$scope.salaryHeadSettingList = result;
            //for (var i = 0; i < $scope.salaryHeadSettingList.length; i++) {
            //    $scope.salaryHeadSettingList[i].Flag = false;
            //}
            $http({
                method: 'GET',
                url: 'Payrolls/SalaryRule/LoadSalaryHeadSetting',
                params: { 'strSalaryRuleID': $scope.salaryRuleNew.SystemID, 'currencyRuleSystemID': $scope.salaryRuleNew.CurrencyRuleSystemID }
            }).then(function successCallback(response) {


                $scope.salaryHeadSettingList = result;
                for (var i = 0; i < $scope.salaryHeadSettingList.length; i++) {
                    $scope.salaryHeadSettingList[i].Flag = false;
                }

                if (baseService.arrayLength(response.data.data) > 0) {
                    $scope.List = [];
                    $scope.List = response.data.data;
                    for (var t = 0; t < baseService.arrayLength($scope.salaryHeadSettingList); t++) {
                        for (var i = 0; i < baseService.arrayLength($scope.List); i++) {
                            if (!baseService.isUndefinedOrNull($scope.List[i].Id) && $scope.List[i].SalaryHeadEnum === $scope.salaryHeadSettingList[t].Value) {
                                $scope.salaryHeadSettingList[t].Id = $scope.List[i].Id;
                                $scope.salaryHeadSettingList[t].Flag = $scope.List[i].IsEditable;
                            }
                        }
                    }



                }



                $scope.SalaryRuleESIC = response.data.SalaryRuleESIC;
                $scope.SalaryRuleRetentionBonus = response.data.SalaryRuleRetentionBonus;
                $scope.SalaryRuleAttdnBonus = response.data.SalaryRuleAttdnBonus;
                $scope.SalaryRuleOT = response.data.SalaryRuleOT;
                $scope.SalaryRulePF = response.data.SalaryRulePF;
                $scope.SalaryRuleAbsenteeism = response.data.SalaryRuleAbsenteeism;


            });





        });

       
    }

    $scope.List = [];
    function getHSSettingList() {
        $http({
            method: 'GET',
            url: 'Payrolls/SalaryRule/LoadSalaryHeadSetting',
            params: { 'strSalaryRuleID': $scope.salaryRuleNew.SystemID }
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.List = [];
                $scope.List = response.data;
                for (var t = 0; t < baseService.arrayLength($scope.salaryHeadSettingList); t++) {
                    for (var i = 0; i < baseService.arrayLength($scope.List); i++) {
                        if (!baseService.isUndefinedOrNull($scope.List[i].Id) && $scope.List[i].SalaryHeadEnum === $scope.salaryHeadSettingList[t].Value) {
                            $scope.salaryHeadSettingList[t].Id = $scope.List[i].Id;
                            $scope.salaryHeadSettingList[t].Flag = $scope.List[i].IsEditable;
                        }
                    }
                }
            }
        });
    }

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }, { Text: "<=", Value: "<=" }, { Text: ">=", Value: ">=" }, { Text: "<", Value: "<" }, { Text: ">", Value: ">" }];

    $scope.salaryRuleGeneral.FormulaDes = null;
    $scope.salaryRuleGeneral.FormulaDesID = null;
    $scope.salaryRuleGeneral.SalaryHeadFormula = null;
    $scope.salaryRuleGeneral.FormulaDescription = null;
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

    $scope.SetFormula = function (formula) {
        try {

            if (formula === 'SHead') {

                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.SalaryHeadIdFormula)) {

                    $scope.salaryRuleGeneral.FormulaDescription = null;
                    $scope.salaryRuleGeneral.FormulaIDDescription = null;

                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];

                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormula($scope.OperatorList, lastvalue)) {
                            $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                            var str = $scope.salaryRuleGeneral.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.salaryRuleGeneral.FormulaDes = $scope.Formula;
                            $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                        }
                        else {
                            $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                            var str = $scope.salaryRuleGeneral.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.salaryRuleGeneral.FormulaDes = $scope.Formula;
                            $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                        }
                    }
                    else {
                        $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                        var str = $scope.salaryRuleGeneral.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');

                        $scope.salaryRuleGeneral.FormulaDes = $scope.Formula;
                        $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
                        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                    }
                }

                $scope.salaryRuleGeneral.FormulaDescription = null;
                $scope.salaryRuleGeneral.FormulaIDDescription = null;

                for (var i = 0; i < $scope.FormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                        $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                    }
                    else {
                        $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                    }
                }

                for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                        $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                    }
                    else {
                        $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                    }
                }

            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Operator)) {

                    $scope.salaryRuleGeneral.FormulaDescription = null;
                    $scope.salaryRuleGeneral.FormulaIDDescription = null;

                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];

                    if ($scope.checkFormula($scope.OperatorList, lastvalue) === false) {
                        $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Operator;
                        $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Operator;
                        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                    }

                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                            $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                            $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }


                } else {
                    throw "First select Salary Head.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Precedence)) {

                    $scope.salaryRuleGeneral.FormulaDescription = null;
                    $scope.salaryRuleGeneral.FormulaIDDescription = null;

                    $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Precedence;
                    $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Precedence;


                    if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDes)) {
                        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

                        for (var i = 0; i < $scope.FormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                                $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                            }
                            else {
                                $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                            }
                        }

                        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                            }
                            else {
                                $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                            }
                        }

                    }
                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Value)) {

                    $scope.salaryRuleGeneral.FormulaDescription = null;
                    $scope.salaryRuleGeneral.FormulaIDDescription = null;

                    $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Value;
                    $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Value;


                    if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDes)) {
                        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                    }


                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                            $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                            $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }

                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.RemoveFormula = function () {
        $scope.salaryRuleGeneral.FormulaDesID = null;

        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);

        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);

        $scope.salaryRuleGeneral.FormulaDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;
        $scope.salaryRuleGeneral.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.FormulaArray[i];
                $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];


            } else {
                $scope.salaryRuleGeneral.FormulaDes += $scope.FormulaArray[i];
                $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }

        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                $scope.salaryRuleGeneral.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];


            } else {
                $scope.salaryRuleGeneral.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    }

    $scope.GetGeneralSalaryData = function (obj) {
        $scope.Row = 'Update Row';
        $scope.showFormula = false;
        $scope.salaryRuleGeneral.SalaryHeadIdFormula = null;
        $scope.salaryRuleGeneral.Operator = null;
        $scope.salaryRuleGeneral.Precedence = null;
        $scope.salaryRuleGeneral.Value = null;

        //var gridObj = $("#RuleGeneralGrid").data("ejGrid");
        //var data = gridObj.getSelectedRecords()[0];

        $scope.salaryRuleGeneral = obj.data;
       
        $scope.salaryRuleGeneralId = obj.data.SalaryRuleGeneralSystemID;

        var value = null;

        if (obj.data.IsNA === true) {
            $scope.salaryRuleGeneral.IsNA = true;
            value = 'IsNA';
        } else {
            $scope.salaryRuleGeneral.IsNA = false;
        }

        if (obj.data.IsFormula === true) {
            $scope.salaryRuleGeneral.IsFormula = true;
            value = 'IsFormulaDefine';

            $scope.salaryRuleGeneral.FormulaDescription = obj.data.FormulaDes;
            $scope.salaryRuleGeneral.FormulaIDDescription = obj.data.FormulaDesID;
            var str = $scope.salaryRuleGeneral.FormulaDescription;
            $scope.FormulaArray = str.split(" ");

            var strId = $scope.salaryRuleGeneral.FormulaIDDescription;
            $scope.FormulaIdArray = strId.split(" ");

        } else {
            $scope.salaryRuleGeneral.IsFormula = false;
        }

        if (obj.data.IsFixed === true) {
            $scope.salaryRuleGeneral.IsFixed = true;
            value = 'IsFixed';
        } else {
            $scope.salaryRuleGeneral.IsFixed = false;
        }

        if (obj.data.IsPolicyDerived === true) {
            $scope.salaryRuleGeneral.IsPolicyDerived = true;
            value = 'IsPolicyDerived';
        } else {
            $scope.salaryRuleGeneral.IsPolicyDerived = false;
        }

        if (obj.data.IsMonthDay === true) {
            $scope.salaryRuleGeneral.IsMonthDay = true;
        } else {
            $scope.salaryRuleGeneral.IsMonthDay = false;
        }

        if (obj.data.IsMonthWorkDay === true) {
            $scope.salaryRuleGeneral.IsMonthWorkDay = true;
        } else {
            $scope.salaryRuleGeneral.IsMonthWorkDay = false;
        }

        if (obj.data.IsWorkDaysInAMonthIncHold === true) {
            $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = true;
        } else {
            $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = false;
        }

        if (obj.data.IsFixedDisbus === true) {
            $scope.salaryRuleGeneral.IsFixedDisbus = true;
        } else {
            $scope.salaryRuleGeneral.IsFixedDisbus = false;
        }
        $scope.minvalueName = null;
        $scope.maxvalueName = null;
        if ($scope.salaryRuleGeneral.FixedMinLimit === true) {
            $scope.minvalueName = 'FixedMinLimit';
        }
        if ($scope.salaryRuleGeneral.PercentageMinLimit === true) {
            $scope.minvalueName = 'PercentageMinLimit';
        }

        if ($scope.salaryRuleGeneral.FixedMaxLimit === true) {
            $scope.maxvalueName = 'FixedMaxLimit';
        }
        if ($scope.salaryRuleGeneral.PercentageMaxLimit === true) {
            $scope.maxvalueName = 'PercentageMaxLimit';
        }

        $scope.showFormulaDiv(value);

        $scope.checkMax();
        $scope.checkMaxLimit($scope.maxvalueName);
        $scope.checkMin();
        $scope.checkMinLimit($scope.minvalueName);

        
    }

    $scope.FixedMonthDaydisabled = true;

    $scope.setCheckedValue = function (name) {
        if (name === 'IsMonthDay') {
            $scope.salaryRuleGeneral.IsMonthDay = true;
            $scope.salaryRuleGeneral.IsMonthWorkDay = false;
            $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = false;
            $scope.salaryRuleGeneral.IsFixedDisbus = false;
            $scope.FixedMonthDaydisabled = true;
            $scope.salaryRuleGeneral.IsFixedMonthDay = false;
            $scope.salaryRuleGeneral.FixedMonthDayValue = null;
            $scope.salaryRuleGeneral.IsPayOnWeekoffForFixedMonthDay = false;
            $scope.salaryRuleGeneral.IsPayOnHolidayForFixedMonthDay = false;
        }

        if (name === 'IsMonthWorkDay') {
            $scope.salaryRuleGeneral.IsMonthWorkDay = true;
            $scope.salaryRuleGeneral.IsMonthDay = false;
            $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = false;
            $scope.salaryRuleGeneral.IsFixedDisbus = false;
            $scope.FixedMonthDaydisabled = true;
            $scope.salaryRuleGeneral.IsFixedMonthDay = false;
            $scope.salaryRuleGeneral.FixedMonthDayValue = null;
            $scope.salaryRuleGeneral.IsPayOnWeekoffForFixedMonthDay = false;
            $scope.salaryRuleGeneral.IsPayOnHolidayForFixedMonthDay = false;
        }

        if (name === 'IsWorkDaysInAMonthIncHold') {
            $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = true;
            $scope.salaryRuleGeneral.IsMonthWorkDay = false;
            $scope.salaryRuleGeneral.IsMonthDay = false;
            $scope.salaryRuleGeneral.IsFixedDisbus = false;
            $scope.FixedMonthDaydisabled = true;
            $scope.salaryRuleGeneral.IsFixedMonthDay = false;
            $scope.salaryRuleGeneral.FixedMonthDayValue = null;
            $scope.salaryRuleGeneral.IsPayOnWeekoffForFixedMonthDay = false;
            $scope.salaryRuleGeneral.IsPayOnHolidayForFixedMonthDay = false;
        }

        if (name === 'IsFixedDisbus') {
            $scope.salaryRuleGeneral.IsFixedDisbus = true;
            $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = false;
            $scope.salaryRuleGeneral.IsMonthWorkDay = false;
            $scope.salaryRuleGeneral.IsMonthDay = false;
            $scope.salaryRuleGeneral.IsFixedMonthDay = false;
            $scope.FixedMonthDaydisabled = true;
            $scope.salaryRuleGeneral.FixedMonthDayValue = null;
            $scope.salaryRuleGeneral.IsPayOnWeekoffForFixedMonthDay = false;
            $scope.salaryRuleGeneral.IsPayOnHolidayForFixedMonthDay = false;
        }
        if (name === 'IsFixedMonthDay') {
            
            $scope.salaryRuleGeneral.IsFixedMonthDay = true;
            $scope.salaryRuleGeneral.IsMonthDay = false;
            $scope.salaryRuleGeneral.IsMonthWorkDay = false;
            $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = false;
            $scope.salaryRuleGeneral.IsFixedDisbus = false;
            $scope.FixedMonthDaydisabled = false;
        }
    }

    $scope.showFormula = false;
    $scope.disabled = true;
    $scope.fixdisabled = false;
    $scope.abdisabled = false;
    $scope.BaseOnNetPayDisabled = true;
    $scope.RefAbsentismDisabled = false;

    $scope.showFormulaDiv = function (value) {
        if (value === 'IsFormulaDefine') {
            $scope.salaryRuleGeneral.IsFormula = true;
            $scope.salaryRuleGeneral.IsFixed = false;
            $scope.salaryRuleGeneral.IsNA = false;
            $scope.salaryRuleGeneral.FixedValue = 0;
            $scope.showFormula = true;
            $scope.salaryRuleGeneral.IsSlabBased = false;
            $scope.BaseOnNetPayDisabled = false;
            $scope.RefAbsentismDisabled = true;
            $scope.salaryRuleGeneral.RefAbsentism = false;
        } else {
            $scope.salaryRuleGeneral.IsFormula = false;
            $scope.showFormula = false;
            $scope.BaseOnNetPayDisabled = true;
            $scope.salaryRuleGeneral.BaseOnNetPay = false;
            $scope.RefAbsentismDisabled = false;
        }

        if (value === 'IsNA') {
            $scope.salaryRuleGeneral.IsNA = true;
            $scope.salaryRuleGeneral.IsFormula = false;
            $scope.salaryRuleGeneral.IsFixed = false;
            $scope.salaryRuleGeneral.FixedValue = 0;
            $scope.salaryRuleGeneral.FormulaDes = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaDesID = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.IsSlabBased = false;
            $scope.showFormula = false;
            $scope.BaseOnNetPayDisabled = true;
        } else {
            $scope.salaryRuleGeneral.IsNA = false;
        }

        if (value === 'IsFixed') {
            $scope.salaryRuleGeneral.IsFixed = true;
            $scope.salaryRuleGeneral.IsFormula = false;
            $scope.salaryRuleGeneral.IsNA = false;
            $scope.salaryRuleGeneral.FormulaDes = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaDesID = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.IsSlabBased = false;
            $scope.disabled = false;
            $scope.showFormula = false;
            $scope.BaseOnNetPayDisabled = true;
        } else {
            $scope.disabled = true;
            $scope.salaryRuleGeneral.IsFixed = false;
        }

        if (value === 'IsPolicyDerived') {
            $scope.salaryRuleGeneral.IsPolicyDerived = true;
            $scope.salaryRuleGeneral.IsNA = false;
            $scope.salaryRuleGeneral.IsFormula = false;
            $scope.salaryRuleGeneral.IsFixed = false;
            $scope.salaryRuleGeneral.FixedValue = 0;
            $scope.salaryRuleGeneral.FormulaDes = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaDesID = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.IsSlabBased = false;
            $scope.showFormula = false;
            $scope.BaseOnNetPayDisabled = true;
        } else {
            $scope.salaryRuleGeneral.IsPolicyDerived = false;
        }

        if (value === 'IsSlabBased') {
            $scope.salaryRuleGeneral.IsSlabBased = true;
            $scope.salaryRuleGeneral.IsPolicyDerived = false;
            $scope.salaryRuleGeneral.IsNA = false;
            $scope.salaryRuleGeneral.IsFormula = false;
            $scope.salaryRuleGeneral.IsFixed = false;
            $scope.salaryRuleGeneral.FixedValue = 0;
            $scope.salaryRuleGeneral.FormulaDes = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaDesID = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.showFormula = false;
            $scope.BaseOnNetPayDisabled = true;
        } else {
            $scope.salaryRuleGeneral.IsSlabBased = false;
        }
    }

    $scope.setAbsentCheckedValue = function (name) {
        if (name === 'IsFixedMonthDay') {
            $scope.salaryRuleAbsent.IsFixedMonthDay = true;
            $scope.salaryRuleAbsent.IsMonthDay = false;
            $scope.salaryRuleAbsent.IsMonthWorkDay = false;
            $scope.salaryRuleAbsent.IsFixedDisbus = false;
            $scope.abdisabled = false;
        }

        if (name === 'IsMonthDay') {
            $scope.salaryRuleAbsent.IsMonthDay = true;
            $scope.salaryRuleAbsent.IsFixedMonthDay = false;
            $scope.salaryRuleAbsent.IsMonthWorkDay = false;
            $scope.salaryRuleAbsent.IsFixedDisbus = false;
            $scope.salaryRuleAbsent.FixedMonthDayValue = 0
            $scope.abdisabled = true;
        }

        if (name === 'IsMonthWorkDay') {
            $scope.salaryRuleAbsent.IsMonthWorkDay = true;
            $scope.salaryRuleAbsent.IsMonthDay = false;
            $scope.salaryRuleAbsent.IsFixedMonthDay = false;
            $scope.salaryRuleAbsent.IsFixedDisbus = false;
            $scope.salaryRuleAbsent.FixedMonthDayValue = 0
            $scope.abdisabled = true;
        }

        if (name === 'IsFixedDisbus') {
            $scope.salaryRuleAbsent.IsFixedDisbus = true;
            $scope.salaryRuleAbsent.IsMonthDay = false;
            $scope.salaryRuleAbsent.IsMonthWorkDay = false;
            $scope.salaryRuleAbsent.IsFixedMonthDay = false;
            $scope.salaryRuleAbsent.FixedMonthDayValue = 0
            $scope.abdisabled = true;
        }
    }

    $scope.showAbsentFormulaDiv = function (value) {
        if (value === 'IsFormulaDefine') {
            $scope.salaryRuleAbsent.IsFormula = true;
            $scope.salaryRuleAbsent.IsFixed = false;
            $scope.showFormula = true;
            $scope.fixdisabled = true;
            $scope.salaryRuleAbsent.FixedValue = 0;
        }

        if (value === 'IsFixed') {
            $scope.salaryRuleAbsent.IsFixed = true;
            $scope.salaryRuleAbsent.IsFormula = false;
            $scope.fixdisabled = false;
            $scope.showFormula = false;
        }
    }

    function CheckDuplicate(ob) {
        try {
            for (var i = 0; i < $scope.salaryRuleGeneralList.length; i++) {
                if (ob.SalaryRuleGeneralSystemID !== $scope.salaryRuleGeneralList[i].SalaryRuleGeneralSystemID && ob.SalaryHeadID === $scope.salaryRuleGeneralList[i].SalaryHeadID) {
                    throw "Salary Head has already been taken...";
                }
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.AddEditSalaryRow = function () {
        try {
            $scope.salaryRuleGeneral.SalaryHead = $("#SH option:selected").text();
            var hasGL = $.grep($scope.salaryHeadCboList, function (item) { return item.Value === $scope.salaryRuleGeneral.SalaryHeadID; })[0];
            if (hasGL.HasGL == false) {
                throw "GL is not defined with this SalaryHead: " + $scope.salaryRuleGeneral.SalaryHead + "";
            }
            ValidationRuleGeneral();

            CheckDuplicate($scope.salaryRuleGeneral);

            $scope.salaryRuleGeneral.SalaryRuleMasterSystemID = $scope.salaryRuleNew.SystemID;

            $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.FormulaDescription;
            $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;

            $scope.salaryRuleGeneral.HasMaxLimit = $scope.salaryRuleGeneral.HasMaxLimit;
            $scope.salaryRuleGeneral.FixedMaxLimit = $scope.salaryRuleGeneral.FixedMaxLimit;
            $scope.salaryRuleGeneral.MaxLimitValue = $scope.salaryRuleGeneral.MaxLimitValue;
            $scope.salaryRuleGeneral.PercentageMaxLimit = $scope.salaryRuleGeneral.PercentageMaxLimit;
            $scope.salaryRuleGeneral.PercentageMaxLimitSalaryHeadId = $scope.salaryRuleGeneral.PercentageMaxLimitSalaryHeadId;
            $scope.salaryRuleGeneral.HasMinLimit = $scope.salaryRuleGeneral.HasMinLimit;
            $scope.salaryRuleGeneral.FixedMinLimit = $scope.salaryRuleGeneral.FixedMinLimit;
            $scope.salaryRuleGeneral.MinLimitValue = $scope.salaryRuleGeneral.MinLimitValue;
            $scope.salaryRuleGeneral.PercentageMinLimit = $scope.salaryRuleGeneral.PercentageMinLimit;
            $scope.salaryRuleGeneral.PercentageMinLimitSalaryHeadId = $scope.salaryRuleGeneral.PercentageMinLimitSalaryHeadId;

            if ($scope.salaryRuleGeneral.IsNA === true) {
                $scope.salaryRuleGeneral.IsNA = true;
                $scope.salaryRuleGeneral.IsFixed = false;
                $scope.salaryRuleGeneral.IsFormula = false;
            } else {
                $scope.salaryRuleGeneral.IsNA = false;
            }

            if ($scope.salaryRuleGeneral.IsFixed === true) {
                $scope.salaryRuleGeneral.IsFixed = true;
                $scope.salaryRuleGeneral.IsNA = false;
                $scope.salaryRuleGeneral.IsFormula = false;
            } else {
                $scope.salaryRuleGeneral.IsFixed = false;
            }

            if ($scope.salaryRuleGeneral.IsFormula === true) {
                $scope.salaryRuleGeneral.IsFormula = true;
                $scope.salaryRuleGeneral.IsFixed = false;
                $scope.salaryRuleGeneral.IsNA = false;
            } else {
                $scope.salaryRuleGeneral.IsFormula = false;
            }

            if ($scope.salaryRuleGeneral.IsPolicyDerived === true) {
                $scope.salaryRuleGeneral.IsPolicyDerived = true;
                $scope.salaryRuleGeneral.IsFixed = false;
                $scope.salaryRuleGeneral.IsNA = false;
                $scope.salaryRuleGeneral.IsFormula = false;
            } else {
                $scope.salaryRuleGeneral.IsPolicyDerived = false;
            }

            $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.FormulaDescription;

            if ($scope.salaryRuleGeneral.IsMonthDay === true) {
                $scope.salaryRuleGeneral.IsMonthDay = true;
                $scope.salaryRuleGeneral.IsMonthWorkDay = false;
                $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = false;
                $scope.salaryRuleGeneral.IsFixedDisbus = false;
            } else {
                $scope.salaryRuleGeneral.IsMonthDay = false;
            }

            if ($scope.salaryRuleGeneral.IsMonthWorkDay === true) {
                $scope.salaryRuleGeneral.IsMonthWorkDay = true;

                $scope.salaryRuleGeneral.IsMonthDay = false;
                $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = false;
                $scope.salaryRuleGeneral.IsFixedDisbus = false;

            } else {
                $scope.salaryRuleGeneral.IsMonthWorkDay = false;
            }

            if ($scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold === true) {
                $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = true;

                $scope.salaryRuleGeneral.IsMonthDay = false;
                $scope.salaryRuleGeneral.IsMonthWorkDay = false;
                $scope.salaryRuleGeneral.IsFixedDisbus = false;

            } else {
                $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = false;
            }

            if ($scope.salaryRuleGeneral.IsFixedDisbus === true) {
                $scope.salaryRuleGeneral.IsFixedDisbus = true;

                $scope.salaryRuleGeneral.IsMonthDay = false;
                $scope.salaryRuleGeneral.IsMonthWorkDay = false;
                $scope.salaryRuleGeneral.IsWorkDaysInAMonthIncHold = false;

            } else {
                $scope.salaryRuleGeneral.IsFixedDisbus = false;
            }

            if ($scope.salaryRuleGeneral.BaseOnNetPay === true && $scope.salaryRuleGeneral.IsFormula === false ) {
                throw "'Based On Netpay' is applicable only for Formula.";
            }

            $scope.SaveSalaryRuleGeneral();
            $scope.Row = 'Add Row';
            $scope.salaryRuleGeneral.SalaryHeadFormula = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;

            $scope.salaryRuleGeneral.SalaryHeadIdFormula = null;
            $scope.salaryRuleGeneral.Operator = null;
            $scope.salaryRuleGeneral.Precedence = null;
            $scope.salaryRuleGeneral.Value = null;

            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.CleanGeneral = function () {
        $scope.salaryRuleGeneral = { IsMonthDay: true, IsNA: true };
        $scope.Row = 'Add Row';
        $scope.getAutoSequence($scope.salaryRuleNew.SystemID);
        $scope.salaryRuleGeneral.FormulaDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.FixedMonthDaydisabled = true;
    }

    $scope.salaryHeadSettingSaveList = [];
    $scope.SaveSalaryHeadSetting = function () {
        try {
            $scope.salaryHeadSettingSaveList = [];
            for (var i = 0; i < $scope.salaryHeadSettingList.length; i++) {
                $scope.salaryHeadSettingSaveList.push($scope.salaryHeadSettingList[i]);
            }

            for (var i = 0; i < $scope.salaryHeadSettingSaveList.length; i++) {
                $scope.salaryHeadSettingSaveList[i].SalaryRuleId = $scope.salaryRuleNew.SystemID;
                $scope.salaryHeadSettingSaveList[i].SalaryHeadEnum = $scope.salaryHeadSettingSaveList[i].Value;
                $scope.salaryHeadSettingSaveList[i].IsEditable = $scope.salaryHeadSettingSaveList[i].Flag;
            }
            if (baseService.arrayLength($scope.salaryHeadSettingSaveList) === 0) {
                throw 'Select Salary Head.';
            }



            //$scope.SalaryRuleESIC = [];
            //$scope.SalaryRuleRetentionBonus = [];
            //$scope.SalaryRuleAttdnBonus = [];
            //$scope.SalaryRuleOT = [];
            //$scope.SalaryRulePF = [];



            for (var i = 0; i < $scope.salaryHeadSettingSaveList.length; i++) {

                if ($scope.salaryHeadSettingSaveList[i].SalaryHeadEnum === 'AttendanceBonus' && $scope.salaryHeadSettingSaveList[i].IsEditable === true) {
                    //$scope.getSalaryRuleAttdnBonus();
                    //if (baseService.isUndefinedOrNull($scope.SalaryRuleAttdnBonus)) {
                    //    throw "Please save Attendance Bonus data.";
                    //}
                    if (baseService.arrayLength($scope.SalaryRuleAttdnBonus) === 0) {
                        throw "Please save Attendance Bonus data.";
                    }
                }

                if ($scope.salaryHeadSettingSaveList[i].SalaryHeadEnum === 'Absenteeism' && $scope.salaryHeadSettingSaveList[i].IsEditable === true) {
                    //$scope.GetSavedSalaryRuleAbsenteeism();
                  
                    if (baseService.arrayLength($scope.SalaryRuleAbsenteeism) === 0) {
                        throw "Please save Absenteeism data.";
                    }
                }

                if ($scope.salaryHeadSettingSaveList[i].SalaryHeadEnum === 'BonusRetain' && $scope.salaryHeadSettingSaveList[i].IsEditable === true) {
                    //$scope.getSalaryRuleRetentionBonus();
                   

                    if (baseService.arrayLength($scope.SalaryRuleRetentionBonus) === 0) {
                        throw "Please save Bonus Retain data.";
                    }
                }

                if ($scope.salaryHeadSettingSaveList[i].SalaryHeadEnum === 'ESIC' && $scope.salaryHeadSettingSaveList[i].IsEditable === true) {
                    //$scope.getSalaryRuleESIC();
                   
                    if (baseService.arrayLength($scope.SalaryRuleESIC) === 0) {
                        throw "Please save ESIC data.";
                    }

                }

                if ($scope.salaryHeadSettingSaveList[i].SalaryHeadEnum === 'OT' && $scope.salaryHeadSettingSaveList[i].IsEditable === true) {
                    //$scope.getSalaryRuleOT();
                    if (baseService.arrayLength($scope.SalaryRuleOT) === 0) {
                        throw "Please save OT data.";
                    }
                }

                if ($scope.salaryHeadSettingSaveList[i].SalaryHeadEnum === 'PF' && $scope.salaryHeadSettingSaveList[i].IsEditable === true) {
                    //$scope.getSalaryRulePF();
                  
                    if (baseService.arrayLength($scope.SalaryRulePF) === 0) {
                        throw "Please save PF data.";
                    }
                }

            }

            $http({
                method: 'POST',
                url: $scope.saveSalaryHeadUrl,
                async: false,
                data: { 'entities': $scope.salaryHeadSettingSaveList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'SalaryHeadSettingPopUp');
                    $scope.salaryHeadSettingSaveList = [];
                }
                else {
                    ShowResult(response.data.Message, 'success', 'SalaryHeadSettingPopUp');

                    $scope.SalaryHeadSetting();
                    //getHSSettingList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'SalaryHeadSettingPopUp');
            };

        } catch (e) {
            ShowResult(e, "failure", 'SalaryHeadSettingPopUp');
        }
    };

    $scope.visiblecheckMax = false;
    $scope.checkMax = function () {
        if ($scope.salaryRuleGeneral.HasMaxLimit === true) {
            $scope.salaryRuleGeneral.FixedMaxLimit = true
            $scope.visiblecheckMax = true;
            $scope.disblecheckFixMax = false;
        } else {
            $scope.visiblecheckMax = false;
        }
    }

    $scope.disblecheckFixMax = true;
    $scope.disblecheckParMax = true;
    $scope.checkMaxLimit = function (name) {

        if (name === 'FixedMaxLimit') {
            $scope.disblecheckParMax = true;
            $scope.disblecheckFixMax = false;
            $scope.salaryRuleGeneral.FixedMaxLimit = true;
            $scope.salaryRuleGeneral.PercentageMaxLimit = false;
        }
        if (name === 'PercentageMaxLimit') {
            $scope.disblecheckFixMax = true;
            $scope.disblecheckParMax = false;
            $scope.salaryRuleGeneral.PercentageMaxLimit = true;
            $scope.salaryRuleGeneral.FixedMaxLimit = false;
        }
    }

    $scope.visiblecheckMin = false;
    $scope.checkMin = function () {
        if ($scope.salaryRuleGeneral.HasMinLimit === true) {
            $scope.salaryRuleGeneral.FixedMinLimit = true
            $scope.visiblecheckMin = true;
            $scope.disblecheckFixMin = false;
        } else {
            $scope.visiblecheckMin = false;
        }
    }

    $scope.disblecheckFixMin = true;
    $scope.disblecheckParMin = true;
    $scope.checkMinLimit = function (name) {

        if (name === 'FixedMinLimit') {
            $scope.disblecheckParMin = true;
            $scope.disblecheckFixMin = false;
            $scope.salaryRuleGeneral.FixedMinLimit = true;
            $scope.salaryRuleGeneral.PercentageMinLimit = false;
        }
        if (name === 'PercentageMinLimit') {
            $scope.disblecheckFixMin = true;
            $scope.disblecheckParMin = false;
            $scope.salaryRuleGeneral.PercentageMinLimit = true;
            $scope.salaryRuleGeneral.FixedMinLimit = false;
        }
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

    function ValidationMaster() {
        try {
            CheckField($scope.salaryRuleNew.SalaryRuleName, 'Salary Rule Name');
            //CheckField($scope.salaryRuleNew.IncomeTaxGroup, 'Income Tax Group');
            CheckField($scope.salaryRuleNew.CurrencyRuleSystemID, 'Currency Rule');
        } catch (e) {
            throw e;
        }
    }

    function ValidationRuleGeneral() {
        try {
            CheckField($scope.salaryRuleGeneral.SalaryHeadID, 'Salary Head');
        } catch (e) {
            throw e;
        }
    }

    $scope.Save = function () {
        try {
            ValidationMaster();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'entity': $scope.salaryRuleNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedData();
                    $scope.salaryRuleNew.SystemID = response.data.SystemId;
                    $scope.getAutoSequence($scope.salaryRuleNew.SystemID);
                    $scope.getsalaryRuleGeneral($scope.salaryRuleNew.SystemID);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveSalaryRuleGeneral = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.savesalaryrulegeneralUrl,
                data: { 'salaryRuleGenerals': $scope.salaryRuleGeneral },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getsalaryRuleGeneral($scope.salaryRuleNew.SystemID);
                    $scope.ClearGeneral();
                    $scope.getAutoSequence($scope.salaryRuleNew.SystemID);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ClearGeneral = function () {
        $scope.salaryRuleGeneral = { IsMonthDay: true, IsNA: true };
        //$scope.FixedMonthDaydisabled = true;
    };

    $scope.Delete = function () {
        try {
            $http({
                method: 'POST',
                url: 'payrolls/salaryRule/Delete?id=' + $scope.salaryRuleNew.SystemID
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.salaryRuleList = [];
                    $scope.getSavedData();
                    $scope.Clear();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields() {
        $scope.salaryRule = {};
        $scope.salaryRuleNew = { IsActive: true };
        //$scope.salaryRuleGeneral = { IsMonthDay: true, IsNA: true };
        $scope.salaryRuleGeneral = {
            IsFormulaDefine: false,
            IsFixed: false,
            IsNA: true,
            FixedValue: 0,
            IsOpenValue: false,
            IsDaysInAMonth: false,
            IsWorkDaysInAMonth: false,
            IsWorkDaysInAMonthIncHold: false,
            IsFixedDisbusment: false,
            RefAbsentism: false,
            SalaryRuleGeneralSystemID: null,
            SalaryRuleMasterSystemID: null,
            SalaryHeadID: null,
            IsGNRNetPayEffect: false,
            IsGNRTagAndUnTag: false,
            IsOpen: false,
            IsFormula: false,
            FormulaDes: null,
            FormulaDesID: null,
            IsFixedMonthDay: false,
            FixedMonthDayValue: null,
            IsMonthDay: true,
            IsMonthWorkDay: false,
            IsFixedDisbus: false,
            SequenceNo: 0,
            IsDisbusted: false,
            IsBankPayment: false,
            IsCashPayment: false,
            BaseOnNetPay: false,
            IsCTCComponent: false,
            IsGrossComponent: false,
            AddedBy: null,
            DateAdded: null,
            UpdatedBy: null,
            DateUpdated: null,
            GNRBaseOthSlrHDFormula: null,
            GNRApplicableMonthNo: null,
            IsGNRBaseOthSlrHD: false,
            IsRetain: false,
            IsMinWages: false,
            IsGNRWhichEverLess: false,
            HasMaxLimit: false,
            FixedMaxLimit: false,
            MaxLimitValue: 0,
            PercentageMaxLimit: false,
            PercentageMaxLimitSalaryHeadId: null,
            HasMinLimit: false,
            FixedMinLimit: false,
            MinLimitValue: 0,
            PercentageMinLimit: false,
            PercentageMinLimitSalaryHeadId: null,
            IsSlabBased: false,
            IsPayOnWeekoffForFixedMonthDay: false,
            IsPayOnHolidayForFixedMonthDay: false
        }
        $scope.salaryRuleGeneral.SalaryHeadIdFormula = null;
        $scope.salaryRuleGeneral.Operator = null;
        $scope.salaryRuleGeneral.Precedence = null;
        $scope.salaryRuleGeneral.Value = null;
        $scope.salaryHeadSettingSaveList = [];
        $scope.salaryRuleGeneralList = [];
        $scope.Action = 'Save';
        $scope.salaryRuleNew.SystemID = null;
    }

    $scope.RemoveData = function (arg) {
        var gridObj = $("#RuleGeneralGrid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.RuleGeneral = data;
        if (!baseService.isUndefinedOrNull($scope.RuleGeneral.SalaryRuleGeneralSystemID))
            $scope.message_confirmation = 'Are you sure want to delete permanently [' + $scope.RuleGeneral.SalaryHead + ' ]';
        angular.element(document.querySelector('#confirmRuleGeneralPopUp')).modal('show');
    };

    $scope.DeleteSalaryRuleGeneralData = function () {
        $http({
            method: 'POST',
            url: 'Payrolls/SalaryRule/DeleteSalaryRuleGeneral?id=' + $scope.RuleGeneral.SalaryRuleGeneralSystemID
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.salaryRuleGeneralList = [];
                $scope.getsalaryRuleGeneral($scope.salaryRuleNew.SystemID);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };


    //$scope.OpenPayDayCatagory = function () {

    //    var eDialog = $("#PayDayCatagory").data("ejDialog");
    //    eDialog.open();
    //};


}
