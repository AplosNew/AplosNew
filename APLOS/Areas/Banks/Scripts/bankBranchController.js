"use strict";
bankBranchController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter"];
function bankBranchController(addressService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Bank Branch";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.bankbranches = [];
    $scope.path = "banks/bankbranch/";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.getListUrl = $scope.path + "getlist";
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        $rootScope.parameters.BankId = $scope.bankbranch.BankId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.bankbranches = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $scope.bankbranch = {
        Id: null,
        AddressMasterId: null,
        ContactMasterId: null,
        BankId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        RoutingNo: null,
        SWIFTCode: null,
        IFSCCode: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), "yyyy-MM-dd"),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    $scope.contactMaster = {
        Id: null,
        ContactPerson: null,
        ContactPersonDesignation: null,
        Phone1: null,
        Phone2: null,
        Phone3: null,
        Fax: null,
        Email1: null,
        Email2: null,
        Email3: null,
        Website: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), "yyyy-MM-dd"),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    $scope.addressMaster = {
        Id: null,
        ContinentId: null,
        CountryId: null,
        StateId: null,
        CityId: null,
        AreaId: null,
        Thana: null,
        Circle: null,
        Ward: null,
        Village: null,
        Address1: null,
        Address2: null,
        Address3: null,
        Postcode: null,
        Phone: null,
        Email: null,
        Website: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), "yyyy-MM-dd"),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    $scope.ContinentList = [];
    $scope.CountryList = [];
    $scope.StateList = [];
    $scope.AreaList = [];
    $scope.CityList = [];
    $scope.bankList = [];
    $http({
        method: "GET",
        url: "banks/bank/getbanklistcbo"
    }).then(function successCallback(response) {
        $scope.bankList = response.data;
    });

    addressService.getContinentCbo(function (result) {
        $scope.ContinentList = result;
    });

    $scope.onContinentChange = function (continentId) {
        addressService.getCountryByContinentCbo(continentId, function (result) {
            $scope.CountryList = result;
        });
    };

    $scope.onCountryChange = function (countryId) {
        addressService.getCboStateByCountry(countryId, function (result) {
            $scope.StateList = result;
        });
    };

    $scope.onStateChange = function (countryId) {
        addressService.getCboCityByCountry(countryId, function (result) {
            $scope.CityList = result;
        });
    };

    $scope.onCityChange = function (cityId) {
        addressService.getCboAreaByCity(cityId, function (result) {
            $scope.AreaList = result;
        });
    };

    $scope.GetSequence = function () {
        $http.get("banks/bankbranch/getautosequence")
            .then(function (response) {
                $scope.bankbranch.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.bankbranch = $scope.bankbranches[$scope.index];
        $scope.bankbranch.AddedDate = $filter("dateFilter")($scope.bankbranch.AddedDate);
        $scope.bankbranch.UpdatedDate = $filter("dateFilter")($scope.bankbranch.UpdatedDate);
        $scope.GetAddressMaster($scope.bankbranch.AddressMasterId);
        $scope.GetContactMaster($scope.bankbranch.ContactMasterId);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetAddressMaster = function (id) {
        $http.get("addresses/addressmaster/get/" + id)
            .then(function (response) {
                $scope.addressMaster = response.data;
                $scope.addressMaster.AddedDate = $filter("dateFilter")($scope.addressMaster.AddedDate);
                $scope.addressMaster.UpdatedDate = $filter("dateFilter")($scope.addressMaster.UpdatedDate);
                $scope.onContinentChange($scope.addressMaster.ContinentId);
                $scope.onCountryChange($scope.addressMaster.CountryId);
                $scope.onCityChange($scope.addressMaster.CityId);
            });
    };

    $scope.GetContactMaster = function (id) {
        $http.get("addresses/contactmaster/get/" + id)
            .then(function (response) {
                $scope.contactMaster = response.data;
                $scope.contactMaster.AddedDate = $filter("dateFilter")($scope.contactMaster.AddedDate);
                $scope.contactMaster.UpdatedDate = $filter("dateFilter")($scope.contactMaster.UpdatedDate);
            });
    };

    function reDirectToRequiredTab() {
        if ($scope.bankBrchForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.bankBrchForm2.$invalid) {
            $scope.setTab(2);
        }
    }

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        reDirectToRequiredTab();
        if ($scope.bankBrchForm.$valid && $scope.bankBrchForm1.$valid && $scope.bankBrchForm2.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: { "bankBranch": $scope.bankbranch, "addressMaster": $scope.addressMaster, "contactMaster": $scope.contactMaster },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.bankbranches.push(response.data.BankBranch);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, "failure");
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: { "bankBranch": $scope.bankbranch, "addressMaster": $scope.addressMaster, "contactMaster": $scope.contactMaster },
                    dataType: "JSON"
                }).then(function successCallBack(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.bankbranches[$scope.index] = $scope.bankbranch;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, "failure");
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.bankbranch.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.bankbranch.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.bankbranches.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                } function errorCallback(response) {
                    ShowResult(response.data.Message, "failure");
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.BankId = $scope.bankbranch.BankId;
        $scope.bankbranch = {};
        $scope.bankbranch.BankId = $scope.BankId;
        $scope.addressMaster = {};
        $scope.contactMaster = {};
        $scope.contactMasters = [];
        $scope.contactMaster.Email1 = null;
        $scope.contactMaster.Email2 = null;
        $scope.contactMaster.Email3 = null;
        $scope.bankbranch.Sequence = seq;
        $scope.bankbranch.Active = true;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}