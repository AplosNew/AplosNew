'use strict';
DeductionInvestmentMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DeductionInvestmentMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Investment/Deduction Master';
    $scope.Action = 'Save';
    $scope.Action1 = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/DeductionInvestmentMaster/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.taxItemList = [{ Name: "Deduction" }, { Name: "Investment" }];
    //The Model For the Master Table
    $scope.ModelTemp = {
        CompanyId: null,
        TaxTypeId: null,
        TaxYearId: null,
        SystemId: null,
        UserCode: null,
        TaxSavingGroupId: null
    };

    $scope.GetSequence = function (MasterID) {
        //cboService.getSequence($scope.getSeqUrl, function (data, MasterID) {
        //    $scope.ModelChild.Sequence = data;
        //});
        $http({
            method: 'GET',
            url: 'Payrolls/DeductionInvestmentMaster/getautosequence?MasterID=' + MasterID,
        }).then(function successCallback(response) {
            $scope.ModelChild.Sequence = response.data;

        });
    };
   /* $scope.GetSequence();*/

    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //The Model for the Child Table
    $scope.ModelChild = {
        Id: null,
        TaxSavingGroupId: null,
        Nature: null,
        isPercentage: "Yes",
        isFix: false,
        Value: null,
        SalaryHeadId: null,
        TaxSavingItemId: null,
        taxItemType: null,
        isTaxableIncome: false,
        isTax: false,
        Limit: null,
        Remarks: null,
        IsInvestment: false,
        IsDeduction: false,
        IsEarning: false,
        IncomeTaxItemMasterId: $scope.ModelNew.SystemId,
        Sequence: 0
    };

    //Filling the Max Limit Box
    $scope.maxLimit = 0;
    $scope.fillMaxLimit = function () {
        for (var i = 0; i < $scope.TaxSavingGroupList.length; i++) {
            if ($scope.ModelNew.TaxSavingGroupId === $scope.TaxSavingGroupList[i].Id) {
                $scope.maxLimit = $scope.TaxSavingGroupList[i].MaxLimit;
            }
        }
        document.getElementById("taxGroupLimit").style.display = 'block';
    }

    //Getting the Company List
    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    //Getting the Tax Type
    $scope.TaxTypeList = [];
    $scope.getTaxGroup = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/TaxPolicy/GetTaxGroup',
        }).then(function successCallback(response) {
            $scope.TaxTypeList = response.data;

        });
    }
    $scope.getTaxGroup();

    //Getting the Salary Head List
    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });

    //Getting the Tax Year
    $scope.YearList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: 'Payrolls/DeductionInvestmentMaster/GetTaxYear',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.YearList = response.data;
        });
    }
    $scope.getData();

    //Getting the Tax Saving Group List
    $scope.TaxSavingGroupList = [];
    $scope.taxSavingGroup = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/DeductionInvestmentMaster/getTaxSavingGroup',
            dataType: 'JSON'
        }).then(function success(response) {
            $scope.TaxSavingGroupList = response.data;
        });
    }
    $scope.taxSavingGroup();

    //Getting the Tax Saving Item List
    $scope.TaxSavingItemList = [];
    $scope.taxSavingItem = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/DeductionInvestmentMaster/getTaxSavingItem',
            dataType: 'JSON'
        }).then(function success(response) {
            $scope.TaxSavingItemList = response.data;
        });
    }
    $scope.taxSavingItem();

    //Getting the Master Grid
    $scope.getMasterData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetList",
            params: { Company: $scope.ModelNew.CompanyId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }

    //Getting the Child Table
    $scope.childData = [];
    $scope.getChildData = function () {
        $scope.childData = [];
        $http({
            method: 'GET',
            url: $scope.path + "getChildList",
            params: { Id: $scope.ModelNew.SystemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.childData = response.data;
        });
    }

    //The Save for the Master
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.path + "Create",
            data: { 'data': $scope.ModelNew },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMasterData();
                $scope.ModelNew.SystemId = response.data.Data.SystemId;

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };



    //The Function to Check for the Validations like Radio Buttons
    $scope.validations = function () {


        if ($scope.ModelChild.isFix == false && $scope.ModelChild.isPercentage == false) {

            alert("Please select Fix Or Percentage");
            throw "Please select Fix Or Percentage"
        }
    }



    //The Save for the Child
    $scope.SaveChild = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $scope.validations();
            if ($scope.ModelChild.isPercentage == "Yes") {
                $scope.ModelChild.isPercentage = true;
                $scope.ModelChild.isFix = false;
            }
            else {
                $scope.ModelChild.isFix = true;
                $scope.ModelChild.isPercentage = false;
            }
            $scope.ModelChild.IncomeTaxItemMasterId = $scope.ModelNew.SystemId;
            $http({
                method: 'POST',
                url: $scope.path + "CreateChild",
                data: { 'dataChild': $scope.ModelChild, 'maxLimit': $scope.maxLimit },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    if ($scope.ModelChild.isPercentage == true) {
                        $scope.ModelChild.isPercentage = "Yes";
                    }
                    if ($scope.ModelChild.isPercentage == false) {
                        $scope.ModelChild.isPercentage = "No";
                    }

                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getChildData();
                    $scope.ClearChildFields($scope.GetSequence($scope.ModelNew.SystemId));

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
                $scope.ModelChild.isPercentage = "Yes";
            }
        }
    };


    ////Setting the Tab inside the Collapse 
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //The Delete For the Master 
    $scope.Delete = function () {
        if ($scope.childData.length > 0) {
            ShowResult("Please First Delete the Childs.", 'failure');
            throw ("Please First Delete the Childs.");
        }

        if (!baseService.isUndefinedOrNull($scope.ModelNew.SystemId)) {
            $http({
                method: 'POST',
                url: 'Payrolls/DeductionInvestmentMaster/Delete',
                data: { 'id': $scope.ModelNew.SystemId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getMasterData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    //Deleting The Child
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
                url: 'Payrolls/DeductionInvestmentMaster/DeleteChild',
                data: { 'id': obj.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getChildData();
                    $scope.GetSequence($scope.ModelNew.SystemId);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    //The Clear Button in Master
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    //The Double Click in the Master Grid
    $scope.getDetail = function (obj) {
        $scope.Action = 'Update';
        $scope.ModelNew = obj.data;
        $scope.maxLimit = obj.data.MaxLimit;
        $scope.childData = [];
        $scope.getChildData();
        $scope.fillMaxLimit();
        //$scope.ClearChildFields($scope.GetSequence($scope.ModelNew.SystemId));
        $scope.GetSequence($scope.ModelNew.SystemId);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }



    $scope.a = false;
    $scope.b = false;
    $scope.c = false;
    $scope.d = false;

    //The Double Click in the Child
    $scope.getChildDetail = function (obj) {
        $scope.ModelChild = {
            Id: null,
            TaxSavingGroup: null,
            Nature: null,
            isPercentage: "Yes",
            isFix: false,
            SalaryHeadId: null,
            TaxSavingItem: null,
            taxItemType: null,
            isTaxableIncome: false,
            isTax: false,
            MaxLimit: null,
            Remarks: null,
            isApplicable: null,
            IncomeTaxItemMasterId: null,
        };
        $scope.Action1 = "Update";
        $scope.ModelChild = Object.assign({}, obj.data);
        $scope.fillMaxLimit();


        if ($scope.ModelChild.isPercentage == true) {
            $scope.ModelChild.isPercentage = "Yes"
        } else {

            $scope.ModelChild.isPercentage = "No";
        }

        //if ($scope.ModelChild.isTaxableIncome == true) {
        //    $scope.ModelChild.isTaxableIncome = "Yes";
        //}
        //else {
        //    $scope.ModelChild.isTaxableIncome = "No";
        //}
    }

    //The Function for the Clearing of the Fields in Master Button
    function ClearFields() {
        $scope.Action = 'Save';
        var cmp = $scope.ModelNew.CompanyId;
        $scope.ModelNew = {
            Id: null,
            CompanyId: cmp,
            TaxTypeId: null,
            TaxYearId: null,
            SystemId: null,
            UserCode: null,
            TaxSavingGroupId: null
        };
        $scope.ClearChildFields($scope.GetSequence($scope.ModelNew.SystemId));
        $scope.childData = [];
        $scope.maxLimit = 0;
    }

    $scope.ClearChildFields = function () {
        $scope.Action1 = 'Save';
        $scope.ModelChild = {
            Id: null,
            TaxSavingGroup: null,
            Nature: null,
            isPercentage: "Yes",
            isFix: false,
            Value: null,
            SalaryHeadId: null,
            TaxSavingItem: null,
            taxItemType: null,
            isTaxableIncome: false,
            isTax: false,
            MaxLimit: null,
            Remarks: null,
            isApplicable: null,

            IsInvestment: false,
            IsDeduction: false,
            IsEarning: false,
            IncomeTaxItemMasterId: $scope.ModelNew.SystemId,
        };
        $scope.GetSequence($scope.ModelNew.SystemId);
    }

    $scope.Deduction = function () {
        if ($scope.ModelChild.IsDeduction == true) {
            $scope.ModelChild.IsEarning = false;
        }
    };
    $scope.Earning = function () {
        if ($scope.ModelChild.IsEarning == true) {
            $scope.ModelChild.IsDeduction = false;
        }
    };
    //#endregion
}