'use strict';
salaryHeadWisePaymentModePolicyController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function salaryHeadWisePaymentModePolicyController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Salary Head Wise Payment Mode Policy';
    $scope.Action = 'Save';
    $scope.path = 'Payrolls/SalaryHeadWisePaymentModePolicy/';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    $scope.SalaryHeadWisePaymentModePolicyModel = {
        Id: null,
        SalaryHeadId: null,
        PaymentMode: null,
        Amount: null,
        PlantId: null,
    };

    $scope.plantList = [];

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.SalaryHeadWisePaymentModePolicyModel.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.getSalaryHeatList = [];
    $scope.GetSalaryHeadPaymentPolicy = function () {
        $scope.getSalaryHeatList = [];
        $http({
            method: 'POST',
            url: 'Payrolls/SalaryHeadWisePaymentModePolicy/GetsalaryheadInformation',
            data: { PlantId: $scope.SalaryHeadWisePaymentModePolicyModel.PlantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.getSalaryHeatList = response.data;
        });
    }

    $scope.Save = function () {
        try {
            
            ValidationMaster();

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'salaryheadpayment': $scope.SalaryHeadWisePaymentModePolicyModel},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSalaryHeadPaymentPolicy();
                    $scope.Clear();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    
    $scope.Clear = function (obj) {
        ClearFields();
       
    };
    function ClearFields(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
        $scope.Action = 'Save';
        $scope.SalaryHeadWisePaymentModePolicyModel = {
            Id: null,
            SalaryHeadId: null,
            PaymentMode: null,
            Amount: null,
            PlantId: $scope.SalaryHeadWisePaymentModePolicyModel.PlantId,
            CompanyId: $scope.SalaryHeadWisePaymentModePolicyModel.CompanyId,
        };
    }


    $scope.SalaryHeadlist = [];
    $scope.GetCbo = function () {
        $http.get('Payrolls/SalaryHeadWisePaymentModePolicy/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SalaryHeadlist = [];
                        $scope.SalaryHeadlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();

    $scope.PaymentModelist = [];
    $scope.PaymentModeGetCbo = function () {
        $http.get('Payrolls/SalaryHeadWisePaymentModePolicy/PaymentModeCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.PaymentModelist = [];
                        $scope.PaymentModelist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.PaymentModeGetCbo();

    function CheckField(fieldname, field) {
        try {
            if (field == null || field == '' || field == 'undefined') {
                throw "" + fieldname + " cannot be blank";
            }
            //if (baseService.isUndefinedOrNull(field)) {
            //    throw "[" + fieldname + "] can not be blank...";
            //}

        } catch (ex) {
            throw ex;
        }
    }
    
    function ValidationMaster() {
        try {
            CheckField("Plant", $scope.SalaryHeadWisePaymentModePolicyModel.PlantId);
            CheckField("Salary Head", $scope.SalaryHeadWisePaymentModePolicyModel.SalaryHeadId);
            CheckField("Payment Mode", $scope.SalaryHeadWisePaymentModePolicyModel.PaymentMode);
            CheckField("Amount", $scope.SalaryHeadWisePaymentModePolicyModel.Amount);
            
        } catch (ex) {
            throw ex;
        }
    }

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridDesignation").data("ejGrid");
        $scope.SalaryHeadWisePaymentModePolicyModel = gridObj.getSelectedRecords()[0];
        try {
            $scope.Action = 'Update';           
        } catch (e) {

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.SalaryHeadWisePaymentModePolicyModel.Id)) {
            $http.get('Payrolls/SalaryHeadWisePaymentModePolicy/Delete?Id=' + $scope.SalaryHeadWisePaymentModePolicyModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');                      

                        $scope.GetSalaryHeadPaymentPolicy();                        
                        ClearFields();                       
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };
    
};