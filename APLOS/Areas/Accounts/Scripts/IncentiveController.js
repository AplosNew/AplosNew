'use strict';
incentiveController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function incentiveController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Incentive Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Accounts/Incentive/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.partyType = "Customer";
    $scope.partyGLType = "Reconciliation";
    $controller("partyBaseController", { $scope: $scope, $http: $http });

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Days: null,
        Percentage: null,
        COAId: null,
        PartyId: null,
        PartyPlantId: null,
        DrGLGeneralInfoId: null,
        DrBudgetMasterId: null,
        DrActivityId: null,
        CrGLGeneralInfoId: null,
        CrBudgetMasterId: null,
        CrActivityId: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        $scope.ModelNew.COAId = $scope.companyConfig.COAId;
    });

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.COAId = $scope.companyConfig.COAId;
        $scope.AssetGLInof = $scope.ModelNew.AssetGLInof;
        $scope.RevenueGLInof = $scope.ModelNew.RevenueGLInof;
        getRevenueBudget();
        getAssetBudget();
        $scope.getAssetActivity();
        $scope.getRevenueActivity();
        $scope.getPartyPlantList($scope.ModelNew.PartyId);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
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
                    ClearFields(response.data.Sequence);
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

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.AssetGLInof = "";
        $scope.RevenueGLInof = "";
    }
    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });
    $scope.searchAssetTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.assetTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getAssetTypeList = function () {
        if ($scope.ModelNew.COAId === null || $scope.ModelNew.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWiseIncentive?coaId=' + $scope.ModelNew.COAId;
        $scope.getAssetTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.assetTypeListParameters)
                .then(function (data) {
                    $scope.assetTypeGLList = data.Rows;
                    $scope.assetTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#assetTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getAssetTypeListData();
    };

    $scope.closeAssetTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#assetTypeListPopUp')).modal('hide');
        }
    };

    $scope.setAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.AssetGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.ModelNew.DrGLGeneralInfoId = x.GLGeneralInfoId;
        getAssetBudget();
    };

    $scope.refreshAssetGL = function () {
        $scope.AssetGLInof = null;
        $scope.ModelNew.DrGLGeneralInfoId = null;
        $scope.assetBudgetList = [];
        $scope.assetActivityList = [];
        $scope.ModelNew.DrBudgetMasterId = null;
        $scope.ModelNew.DrActivityId = null;
    };

    $scope.assetBudgetList = [];
    function getAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.ModelNew.COAId, $scope.ModelNew.DrGLGeneralInfoId, function (result) {
            $scope.assetBudgetList = result;
        });
    }

    $scope.assetActivityList = [];
    $scope.getAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.ModelNew.DrBudgetMasterId, function (result) {
            $scope.assetActivityList = result;
        });
    };

    $scope.searchRevenueTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.revenueTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getRevenueTypeList = function () {
        if ($scope.ModelNew.COAId === null || $scope.ModelNew.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }

        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.ModelNew.COAId;
        $scope.getRevenueTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.revenueTypeListParameters)
                .then(function (data) {
                    $scope.revenueTypeGLList = data.Rows;
                    $scope.revenueTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#revenueTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getRevenueTypeListData();
    };

    $scope.closeRevenueTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#revenueTypeListPopUp')).modal('hide');
        }
    };

    $scope.setRevenueGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.RevenueGLSelectedData = x;
        $scope.RevenueGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.ModelNew.CrGLGeneralInfoId = x.GLGeneralInfoId;
        getRevenueBudget();
    };

    $scope.refreshRevenueGL = function () {
        $scope.RevenueGLInof = null;
        $scope.ModelNew.CrGLGeneralInfoId = null;
        $scope.revenueBudgetList = [];
        $scope.revenueActivityList = [];
        $scope.ModelNew.CrBudgetMasterId = null;
        $scope.ModelNew.CrActivityId = null;
    };

    $scope.revenueBudgetList = [];
    function getRevenueBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.ModelNew.COAId, $scope.ModelNew.CrGLGeneralInfoId, function (result) {
            $scope.revenueBudgetList = result;
        });
    }

    $scope.revenueActivityList = [];
    $scope.getRevenueActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.ModelNew.CrBudgetMasterId, function (result) {
            $scope.revenueActivityList = result;
        });
    };
    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
            ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
            return;
        }
        else if ($scope.companyConfig.IsModelNewFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
            ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
            return;
        }
        else {
         
            $scope.ModelNew.PartyId = party.Id;
            $scope.ModelNew.PartyCode = party.Code;
            $scope.ModelNew.PartyName = party.UserName;
            $scope.ModelNew.PartyType = $scope.partyType;
            $scope.getPartyPlantList($scope.ModelNew.PartyId);
        }
        $scope.hidePartyPopUp();
    };
}