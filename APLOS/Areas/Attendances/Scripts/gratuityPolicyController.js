'use strict';
gratuityPolicyController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function gratuityPolicyController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Gratuity Policy';
    $scope.Action = 'Save';
    $scope.path = 'Attendances/GratuityPolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveDetailsUrl = $scope.path + 'SaveDetails';
    $scope.deleteUrl = $scope.path + 'delete/';

    $window.onresize = function (event) {
        $scope.actionCompleteSelected();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GratuityId").ejGrid("instance");
                var scrollerwidth = $("#NewId").width();

                $("#GratuityId").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 150 } });
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    };

    $scope.GratuityPolicyModel = {
        Id: null,
        //IsFirstMaturityRoudingSixMonth: true,
        IsRoudingSixMonth: true,
        UserName: null,
        Active: true,
        CompanyId: null,
        plantId: null
    };

    $scope.GratuityPolicyDetailsModel = {
        Id: null,
        GratuityPolicyMasterId: null,
        MaturityFromYear: null,
        MaturityToYear: null,
        MaturityFormulaDesID: null,
        MaturityFormulaDescription: null,
        plantId: null,
        CompanyId: null
    };

    $scope.GratuityList = [];

    $scope.getListData = function () {
        $scope.GratuityList = [];
        $http({
            method: 'POST',
            url: 'Attendances/GratuityPolicy/getGratuitylist',
            data: { PlantId: $scope.GratuityPolicyModel.plantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GratuityList = response.data;
        });
    }


    $scope.GratuityDetailsList = [];
    $scope.getListDetailsData = function () {
        $http.get('Attendances/GratuityPolicy/getGratuityDetailslist?MasterID=' + $scope.GratuityPolicyModel.Id)
            .then(
                function successCallback(response) {
                    $scope.GratuityDetailsList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        if (!baseService.isUndefinedOrNull(response.data)) {
                            $scope.GratuityDetailsList = response.data;
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridMaster").data("ejGrid");
        $scope.GratuityPolicyModel = gridObj.getSelectedRecords()[0];
        $scope.GratuityPolicyDetailsModel.GratuityPolicyMasterId = $scope.GratuityPolicyModel.Id;
        try {
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
            $scope.Action = 'Update';           
        } catch (e) {
        }
        $scope.getListDetailsData();
    };

    $scope.recorddoubleclickDetails = function () {
        var gridObj = $("#GratuityId").data("ejGrid");
        $scope.GratuityPolicyDetailsModel = gridObj.getSelectedRecords()[0];
        $scope.salaryRuleGeneral.FormulaDescription = $scope.GratuityPolicyDetailsModel.MaturityFormulaDescription; 
        $scope.salaryRuleGeneral.FormulaIDDescription = $scope.GratuityPolicyDetailsModel.MaturityFormulaDesID;

        if ($scope.salaryRuleGeneral.FormulaDescription != null) {
            var str = $scope.salaryRuleGeneral.FormulaDescription;
            $scope.FormulaArray = str.split(" ");

            var strId = $scope.salaryRuleGeneral.FormulaIDDescription;
            $scope.FormulaIdArray = strId.split(" ");
        }
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#gratuityDialog").data("ejDialog");
            eDialog.open();
            $scope.Action = 'Update';
            $scope.getListDetailsData();
        } catch (e) {
        }
        
    };
    
    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#gratuityDialog").data("ejDialog");
            eDialog.open();

            $scope.GratuityPolicyDetailsModel = {
                Id: null,
                GratuityPolicyMasterId: null,
                MaturityFromYear: null,
                MaturityToYear: null,
                MaturityFormulaDesID: null,
                MaturityFormulaDescription: null,
            };
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.GratuityPolicyDetailsModel.GratuityPolicyMasterId = $scope.GratuityPolicyModel.Id;

        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.MasterId = null;
    $scope.Save = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'GratuityPolicyMaster': $scope.GratuityPolicyModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GratuityPolicyModel.Id = response.data.MasterId;
                    $scope.GratuityPolicyDetailsModel.GratuityPolicyMasterId = $scope.GratuityPolicyModel.Id;
                    $scope.getListData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.SaveD = function () {
        try {
            $scope.GratuityPolicyDetailsModel.plantId = $scope.GratuityPolicyModel.plantId;
            $scope.GratuityPolicyDetailsModel.MaturityFormulaDescription = $scope.salaryRuleGeneral.FormulaDescription;
            $scope.GratuityPolicyDetailsModel.MaturityFormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;
            if ($scope.GratuityPolicyDetailsModel.MaturityFromYear < 0) {
                throw "Invalid Year";
            }
            if ($scope.GratuityPolicyDetailsModel.MaturityToYear < 0) {
                throw "Invalid Year";
            }
            if (parseFloat($scope.GratuityPolicyDetailsModel.MaturityToYear) > parseFloat($scope.GratuityPolicyDetailsModel.MaturityFromYear)) {
                
            }
            else {
                throw "Maturity ToYear can't be smaller then Maturity FromYear";
            }

            ValidationMasterSec();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveDetailsUrl,
                data: { 'GratuityPolicyDetails': $scope.GratuityPolicyDetailsModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearD();
                    $scope.getListDetailsData();
                    $scope.GratuityPolicyDetailsModel.GratuityPolicyMasterId = $scope.GratuityPolicyModel.Id;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.GratuityPolicyModel.Id)) {
            $http.get('Attendances/GratuityPolicy/Delete?SystemID=' + $scope.GratuityPolicyModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');                        
                        ClearFields();
                        $scope.getListData();
                        $scope.Action = 'Save';
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.DeleteDetailsFunction = function () {
        if (!baseService.isUndefinedOrNull($scope.GratuityPolicyModel.Id)) {
            $http.get('Attendances/GratuityPolicy/DeleteDetails?Id=' + $scope.GratuityPolicyDetailsModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GratuityPolicyDetailsModel = {
                            Id: null,
                            GratuityPolicyMasterId: null,
                            MaturityFromYear: null,
                            MaturityToYear: null,
                            MaturityFormulaDesID: null,
                            MaturityFormulaDescription: null,
                        };
                       // ClearFields();
                        $scope.getListDetailsData();
                        $scope.Action = 'Save';
                        $scope.salaryRuleGeneral.FormulaDescription = null;
                        $scope.salaryRuleGeneral.FormulaIDDescription = null;
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.ClearD = function () {
        $scope.GratuityPolicyDetailsModel = {
            Id: null,            
            MaturityFromYear: null,
            MaturityToYear: null,
            MaturityFormulaDesID: null,
            MaturityFormulaDescription: null,
            plantId: $scope.GratuityPolicyModel.plantId,
            CompanyId: $scope.GratuityPolicyModel.CompanyId,
        };
        $scope.salaryRuleGeneral.FormulaDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;
        $scope.salaryRuleGeneral.SalaryHeadIdFormula = null;
        $scope.salaryRuleGeneral.Operator = null;
        $scope.salaryRuleGeneral.Precedence = null;
        $scope.salaryRuleGeneral.Value = null;
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.GratuityPolicyDetailsModel.GratuityPolicyMasterId = $scope.GratuityPolicyModel.Id;

    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.GratuityPolicyModel = {
            Id: null,
            //IsFirstMaturityRoudingSixMonth: true,
            IsRoudingSixMonth: true,
            UserName: null,
            Active: true,
            plantId: $scope.GratuityPolicyModel.plantId,
            CompanyId: $scope.GratuityPolicyModel.CompanyId,
        };
        $scope.GratuityDetailsList = [];
        $scope.Action = 'Save';
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };
    function ValidationMaster() {
        try {
            CheckField("plant", $scope.GratuityPolicyModel.plantId);
            CheckField("UserName", $scope.GratuityPolicyModel.UserName);
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationMasterSec() {
        try {
            CheckField("Maturity From Year", $scope.GratuityPolicyDetailsModel.MaturityFromYear);
            CheckField("Maturity To Year", $scope.GratuityPolicyDetailsModel.MaturityToYear);
            CheckField("Maturity Formula", $scope.GratuityPolicyDetailsModel.MaturityFormulaDesID);
        } catch (ex) {
            throw ex;
        }
    }
    
    $scope.salaryHeadList = [];
    $scope.getSalaryHeadListList = function () {
        $http.get('Attendances/PFPolicy/GetSalaryHeadListeList')
            .then(function (response) {
                $scope.salaryHeadList = response.data;
            });
    };
    $scope.getSalaryHeadListList();

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];
    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];
    $scope.salaryRuleGeneral = {
        FormulaDescription: null,
        FormulaIDDescription: null,
    };
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

    $scope.plantList = [];
    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.GratuityPolicyModel.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.confirmdelete = false;
    $scope.Confirm = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmClose = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.close();
    };

}