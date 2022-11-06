'use strict';
UtilityMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function UtilityMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'UtilityMaster';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/UtilityMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChildUrl = $scope.path + 'CreateChild';

    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        UtilityGroup: null,
        UtilitySubGroup: null,
        UtilityCategory: null,
        UtilityGroupId: null,
        Item: null,
        UoMId: null,
        UoM: null,
        IsPartyApplicable: false,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        IsReadingApplicable: false,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        AdminId: null,
        Admin: null,
        Description: null,
        EntryLegDays: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.UtilityGroupList = [];
    cboService.getUtilityGroupCbo(function (response) {
        $scope.UtilityGroupList = response;
    });

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.searchByParty = "UserName"; $scope.searchParty = "";

    $scope.partyList = [];
    $scope.partyType = "Vendor";
    $scope.ShowCustomerPopUpNew = function (partyType) {
        $scope.partyType = partyType;
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
        if ($scope.partyType == "Vendor" || $scope.partyType == "Customer") {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + '' + '&PlantId=' + '';
        } else {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + null + '&CompanyId=' + '' + '&PlantId=' + '';
        }

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('show');
    };

    $scope.SetCustomerData = function (obj) {
        var party = obj.data;
        $scope.ModelNew.PartyCode = party.Code;
        $scope.ModelNew.PartyName = party.UserName;
        $scope.ModelNew.PartyId = party.Id;

        $scope.hidePartyPopUp();
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.searchParty = '';
    }

    $scope.closeCustomerPopUpNew = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.hidePartyPopUp();
        $scope.partyType = "Customer";
        $scope.searchParty = '';
    }

    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.ChangeCustomer = function () {
        if ($scope.ModelNew.IfPartyApplicable) {
            $scope.ModelNew.PartyName = null;
            $scope.ModelNew.PartyId = null;
        }
        else {
            $scope.ModelNew.PartyName = party.UserName;
            $scope.ModelNew.PartyId = party.Id;
        }

    };


    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.employeeUrl = 'OrderManagements/masterorder/GetEmployeeListResponsible';

    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;
            //$scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        //$scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];

            if ($scope.Name == 'mo') {
                $scope.ModelNew.ResponsiblePersonId = employee.SystemId;
                $scope.ModelNew.ResponsiblePersonName = employee.EmployeeName;
            }
            else if ($scope.Name == 'ad') {
                $scope.ModelNew.AdminId = employee.SystemId;
                $scope.ModelNew.Admin = employee.EmployeeName;
            }

        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.getUtilityGridData(args.data.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.utilityDetails = [];
    $scope.getUtilityGridData = function (id) {
        $http({
            method: 'POST',
            url: $scope.path + "GetUtilityData",
            data: { 'UtilityMasterId': id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.utilityDetails = response.data;
            //$scope.ModelChildNew = Object.assign({}, response.data);
        });
    }

    $scope.UtilityDetaildoubleclick = function (args) {
        $scope.ModelChildNew = Object.assign({}, args);
    };

    $scope.removeUtilityDetailsRowModal = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmUtilityDetailsRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeUtilityDetailsRow = function () {
        $http({
            method: 'POST',
            url: 'Materials/UtilityMaster/utilityDetailsDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getUtilityGridData($scope.ModelNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Save = function () {
        if ($scope.ModelNew.ResponsiblePersonName === null || $scope.ModelNew.ResponsiblePersonName === "") {
            ShowResult('Select Responsible Person Name', 'failure');
            return false;
        }
        else if ($scope.ModelNew.Admin === null || $scope.ModelNew.Admin === "") {
            ShowResult('Select Admin', 'failure');
            return false;
        }

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
                    $scope.ModelNew.Id = response.data.Id;
                    //ClearFields(response.data.Sequence);
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
    $scope.ModelChild = {
        Id: null,
        EffectiveDate: null,
        Rate: 0,
        Remark: null
    };
    $scope.ModelChildNew = Object.assign({}, $scope.ModelChild);

    $scope.SaveChild = function () {
        $http({
            method: 'POST',
            url: $scope.saveChildUrl,
            data: { 'data': $scope.ModelChildNew, 'UtilityMasterId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
                $scope.getUtilityGridData($scope.ModelNew.Id);
                $scope.ClearUtilityDetail();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
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
        $scope.utilityDetails = [];
    }

    $scope.ClearUtilityDetail = function () {
        $scope.ModelChildNew = Object.assign({}, $scope.ModelChild);
    }

    $scope.GetAssetpDataList = function () {

    }

    // #region Asset

    $scope.assetpDataList = [];
    $scope.GetAssetpDataList = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetAssetData'
        }).then(function successCallback(response) {
            $scope.assetpDataList = response.data;
        });
        angular.element(document.querySelector('#AssetPopUp')).modal('hide');

    };

    $scope.addSFGInventory = function () {
        if (baseService.arrayLength($scope.SFGInventoryDataList) > 0) {
            angular.forEach($scope.SFGInventoryDataList, function (a) {
                // if (!baseService.valueCheckInList($scope.userSFGInventoryList, 'SFGInventoryId', a.Id)) {
                if (checkSFGInventoryExist($scope.userSFGInventoryList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.userSFGInventoryList.push({
                            Id: null
                            , SFGInventoryId: a.Id
                            , UserId: $scope.userNew.Id
                            , Code: a.Code
                            , Sequence: a.Sequence
                            , ShortName: a.ShortName
                            , UserName: a.UserName
                            , StandardName: a.StandardName
                        });
                    }
                }
            });
        }
        else
            $scope.userSFGInventoryList = [];
        angular.forEach($scope.userSFGInventoryList, function (a) {
            if (!baseService.valueCheckInList($scope.SFGInventoryDataList, 'Id', a.SFGInventoryId))
                $scope.userSFGInventoryList.splice(a, 1);
        });
        $scope.closeSFGInventoryPopUp();
    };

    $scope.closeSFGInventoryPopUp = function () {
        $scope.SFGInventoryUpUrl = null;
        $scope.SFGInventoryDataList = [];
        $scope.SFGInventorySearchList = [];
        angular.element(document.querySelector('#SFGInventoryPopUp')).modal('hide');
    };

    function getUserSFGInventoryList() {
        $http({
            method: 'GET',
            url: 'Products/SFGMovement/GetUserSFGMovementList?userid=' + $scope.userNew.Id
        }).then(function successCallback(response) {
            $scope.userSFGInventoryList = response.data;
        });
    }

    function checkSFGInventoryExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SFGInventoryId === Id) {
                return true;
            }
        }
        return false;
    }

    // #endregion Asset
}