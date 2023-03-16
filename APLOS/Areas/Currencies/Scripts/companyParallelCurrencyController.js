'use strict';
CompanyParallelCurrencyController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function CompanyParallelCurrencyController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $scope.Action = 'Save';
    $scope.companyList = [];
    $scope.companyCurrencyList = [];
    $scope.companyGroupCurrencyList = [];
    $scope.hardCurrencyList = [];
    $scope.curcomParalist = [];
    $scope.curcomPara = {
        CompanyGroupId: null,
        comparaId: null,
        groupparaId: null,
        hardparaId: null,
        CompanyId: null,
        comcurType: null,
        comcurId: null,
        GroCurType: null,
        groupcurId: null,
        hardcurType: null,
        hardcurId: null,
        GroupParrallelCurrencyDel: false,
        HardParrallelCurrencyDel: false,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null
    };
    $scope.companyGroupList = [];
    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });


    $scope.changeCurrencyGroup = function (comGroupId) {
        cboService.getCompanyGroupCompanyCbo(comGroupId, function (result) {
        $scope.companyList = result;
    });
    }
    

    $scope.onChangecompanyGetlist = function (item) {
        $http({
            method: 'get',
            url: 'currencies/CompanyParallelCurrency/getcompanyparallelcurrency?companyId=' + item
        }).then(function successCallback(response) {
            ClearFields();
            $scope.curcomParalist = response.data;
            getparallelcurrency($scope.curcomParalist);
        });
    }

    $scope.companyCurrencyChange = function (item) {
        $http({
            method: 'GET',
            url: 'Organizations/Company/GetCompanyCurrency?param1=' + item
        }).then(function successCallback(response) {
            $scope.companyCurrencyList = response.data;
        });
    };
    function getparallelcurrency(list) {
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].ParallelCurrencyType === 'CompanyCurrency') {
                    $scope.curcomPara.comparaId = list[i].Id;
                    $scope.curcomPara.comcurId = list[i].CurrencyId;
                    $scope.curcomPara.comcurType = list[i].ParallelCurrencyType;
                }
                else if (list[i].ParallelCurrencyType === 'CompanyGroupCurrency') {
                    $scope.curcomPara.groupparaId = list[i].Id;
                    $scope.curcomPara.groupcurId = list[i].CurrencyId;
                    $scope.curcomPara.GroCurType = list[i].ParallelCurrencyType;
                }
                else if (list[i].ParallelCurrencyType === 'HardCurrency') {
                    $scope.curcomPara.hardparaId = list[i].Id;
                    $scope.curcomPara.hardcurId = list[i].CurrencyId;
                    $scope.curcomPara.hardcurType = list[i].ParallelCurrencyType;
                    $scope.hardCurrencyChange($scope.curcomPara.comcurId, $scope.curcomPara.groupcurId);
                }
            }
        }
        else {
            $scope.curcomPara.comparaId = '';
            $scope.curcomPara.comcurType = 'CompanyCurrency';
        }
       
    }

    $http({
        method: 'GET',
        url: 'Organizations/CompanyGroup/GetCompanyGroupCurrency'
    }).then(function successCallback(response) {
        $scope.companyGroupCurrencyList = response.data;
    });

    $scope.hardCurrencyChange = function (item, data) {
        $http({
            method: 'GET',
            url: 'currencies/Currency/GetHardCurrencyList?param1=' + item + '&param2=' + data
        }).then(function successCallback(response) {
            $scope.hardCurrencyList = response.data;
        });
    }

    $scope.checkBaseCurrencyMsg = '';
    $scope.checkBaseCurrency = function () {
        if ($scope.curcomPara.CompanyId === "") {
            $scope.checkBaseCurrencyMsg = '';
            $scope.pop('error', 'Please select Company  !');
            return false;
        }
        //if (baseService.isUndefinedOrNull($scope.curcomPara.comparaId)) {
        //    $scope.curcomPara.comparaId = $scope.curcomPara.CompanyId;
        //}
        if ($scope.curcomPara.comcurType === "") {
            $scope.checkBaseCurrencyMsg = '';
            $scope.pop('error', 'Please select Company Currency Type !');
            return false;
        }
        if ($scope.curcomPara.comcurId === "") {
            $scope.checkBaseCurrencyMsg = '';
            $scope.pop('error', 'Please select Company Currency !');
            return false;
        }

        if ($scope.curcomPara.GroCurType !== "" && $scope.curcomPara.groupcurId === "") {
            $scope.checkBaseCurrencyMsg = '';
            $scope.pop('error', 'Please select Company Group Currency  !');
            return false;
        }
        if ($scope.curcomPara.GroCurType === "" && $scope.curcomPara.groupcurId !== "") {
            $scope.checkBaseCurrencyMsg = '';
            $scope.pop('error', 'Please select Company Group Currency  !');
            return false;
        }
        if ($scope.curcomPara.hardcurType !== "" && $scope.curcomPara.hardcurId === "") {
            $scope.checkBaseCurrencyMsg = '';
            $scope.pop('error', 'Please select Company Group Currency  !');
            return false;
        }
        else {
            return true
        }
    };
    $scope.pop = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 3000
        });
    };

    $scope.Save = function () {
        $scope.curcomPara.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
        $scope.curcomPara.UpdatedDate = null;
        if ($scope.checkBaseCurrency()) {//$scope.comParallelCurrencyForm.$valid &&
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "currencies/CompanyParallelCurrency/Save?comcurType=" + $scope.curcomPara.comcurType + "&&comcurId=" + $scope.curcomPara.comcurId + "&&comparaId=" + $scope.curcomPara.comparaId
                    + "&&GroCurType" + $scope.curcomPara.GroCurType + "&&groupcurId" + $scope.curcomPara.groupcurId + "&&groupparaId=" + $scope.curcomPara.groupparaId
                    + "&&hardcurType" + $scope.curcomPara.hardcurType + "&&hardcurId" + $scope.curcomPara.hardcurId + "&&hardparaId=" + $scope.curcomPara.hardparaId
                    + "&&CompanyId" + $scope.curcomPara.CompanyId + "&&GroupParrallelCurrencyDel" + $scope.curcomPara.GroupParrallelCurrencyDel
                    + "&&HardParrallelCurrencyDel" + $scope.curcomPara.HardParrallelCurrencyDel,
                    data: $scope.curcomPara,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        ClearFields();
                        $scope.curcomPara.CompanyId = "";
                    }
                });
                return true;
            }
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        // $scope.curcomPara.CompanyId = "",
        $scope.curcomPara.comcurType = "";
        $scope.curcomPara.comcurId = "";
        $scope.curcomPara.comparaId = null;
        $scope.curcomPara.GroCurType = "";
        $scope.curcomPara.groupcurId = "";
        $scope.curcomPara.groupparaId = null;
        $scope.curcomPara.hardcurType = "";
        $scope.curcomPara.hardcurId = "";
        $scope.curcomPara.hardparaId = null;
        $scope.curcomPara.Active = true;
        $scope.curcomPara.GroupParrallelCurrencyDel = false;
        $scope.curcomPara.HardParrallelCurrencyDel = false;
    }
}