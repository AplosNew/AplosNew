'use strict';
entityFixedAssetsRegisterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$controller', '$filter', '$window'];
function entityFixedAssetsRegisterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $controller, $filter, $window) {
    $rootScope.title = 'Entity Fixed Assets Register';
    $scope.Action = 'Save';
    $scope.path = 'FixedAssets/EntityFixedAssetsRegister/';

    //$scope.path = 'FixedAssets/FixedAssetRegister/';
    $scope.partyType = 'Vendor';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialMasterList1 = [];


    $scope.saveUrl = $scope.path + 'create';
    var dt = new Date();

    //$scope.reportParameters = {
    //    FromDate: $filter("dateFiltering")(new Date(dt.setDate(dt.getDate() - 10))), //$filter("dateFiltering")(Date.now()) - 10,
    //    ToDate: $filter("dateFiltering")(Date.now()),
    //    TransactionType: 'LoanTaken',
    //    ReportFormat: 'Excel'
    //    VoucherId: null
    //    IsOrderSpecific: true,
    //   FromDate: $filter('dateFiltering')(Date.now()),
    //};

    $scope.report = {

        //FromDate: $filter("dateFiltering")(Date.now()),
        //ToDate: $filter("dateFiltering")(Date.now()),

        PartyType: 'All', 
        PartyId: null,
        FixedAssetMasterId: null,
        MaterialMasterId: null,
        CapitalizationDate: null,

        partyType: 'All',
        PartyName: null,
        MaterialMasterName: null,
        FixedAssetMasterName: null,
        EntityId: null,

        EntityId: null,
        DepartmentId: null
    };


    $scope.EntityFixedAssetRegisterList = [];
    $scope.GetEntityFixedAssetRegisterData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetEntityFixedAssetRegisterDataList",
                //data: { FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {

                //if (response.data.Error == false) {
                //    for (var i = 0; i < response.data.DATA.length; i++) {
                //      response.data.DATA[i].MasterLCDate = new Date(response.data.DATA[i].MasterLCDate);
                //    }
                //    $scope.EntityFixedAssetRegister = response.data.DATA;
                //}
                //else {
                //    ShowResult(response.data.Message, 'failure');
                //}
                $scope.EntityFixedAssetRegisterList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        catch (e) {

        }
    }
    $scope.GetEntityFixedAssetRegisterData();

    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });


    $scope.refreshTemplateEntityandDepartment = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEntityAndDepartment });
    };

    function CheckBoxSelectAllEntityAndDepartment(e) {

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridEntityFixedAssetRegister").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EntityFixedAssetRegisterList.length; i++) {
                $scope.EntityFixedAssetRegisterList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridEntityFixedAssetRegister").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.NewEntityFixedAssetRegisterList = [];
    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.report.EntityId)) {
            ShowResult('Please select Entity', 'failure');
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.report.DepartmentId)) {
            ShowResult('Please select Department', 'failure');
            return true;
        }
        $scope.NewEntityFixedAssetRegisterList = [];
        for (var i = 0; i < $scope.EntityFixedAssetRegisterList.length; i++) {
            if ($scope.EntityFixedAssetRegisterList[i].isSelected == true) {
                $scope.NewEntityFixedAssetRegisterList.push($scope.EntityFixedAssetRegisterList[i]);
            }
        }

        if ($scope.NewEntityFixedAssetRegisterList.length == 0) {
            //(angular.isUndefinedOrNull(NewMasterLCList)) 
            ShowResult('Please select at least one Fixed Assets', 'failure');
            return true;
        }

        else {
            return false;

        }
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if (!$scope.validation()) {

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'entityId': $scope.report.EntityId, 'departmentId': $scope.report.DepartmentId, 'entityFixedAssetList': $scope.NewEntityFixedAssetRegisterList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.GetEntityFixedAssetRegisterData();
                    //$scope.Clear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
       // $scope.GetEntityFixedAssetRegisterData();
    }

    $scope.partyTypeChange = function (partyType) {
        //Database table  
        $scope.report.PartyType = partyType;
        $scope.report.PartyId = '';
        $scope.report.MaterialMasterId = '';
        $scope.report.FixedAssetMasterId = ''; 
        //input type ng-model
        $scope.report.partyType = partyType;
        $scope.report.PartyName = '';
        $scope.report.MaterialMasterName = '';
        $scope.report.FixedAssetMasterName = '';
    }

    $scope.clearVendor = function () {
        $scope.report.FixedAssetMasterName = null;
        $scope.report.MaterialMasterName = null;
        $scope.report.PartyName = null;
    }

    $scope.searchByMaterialMasterModalList = [
        {
            "name": "Asset Category",
            "value": "FixedAssetCategory"
        }
        ,
        {
            "name": "Asset Sub Category",
            "value": "FixedAssetSubCategory"
        },
        {
            "name": "Asset Master",
            "value": "FixedAssetMasterName"
        }
    ];

    $scope.searchMaterialMasterParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "FixedAssetMasterName",
        searchBy: "FixedAssetMasterName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getFixedAssetData = function () {
        var url1 = "FixedAssets/FixedAssetMaster/GetFixedAssetMasterData";
        baseService.setCurrentPage("materialMasterList1");
        //for search loard
        $scope.loadMaterialMasterModalList = function (pageno) {
            baseService.paginationBase(url1, pageno, $scope.searchMaterialMasterParameters)
                .then(function (result) {
                    $scope.materialMasterList1 = result.Rows;
                    $scope.searchMaterialMasterParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
            angular.element(document.querySelector("#assetmastermodal")).modal("show");
        };
        $scope.loadMaterialMasterModalList();
    };

    // for close (crose*)
    $scope.closeFixedAssetPopUp = function () {
        angular.element(document.querySelector("#assetmastermodal")).modal("hide");
    }
    // select data double click
    $scope.selectFixedAssetMaster = function (data) {
        $scope.report.FixedAssetMasterName = data.FixedAssetMasterName;
        $scope.report.FixedAssetMasterId = data.FixedAssetMasterId;
        angular.element(document.querySelector("#assetmastermodal")).modal("hide");

    };

    $scope.setMaterialMasterData = function (ob) {
        $scope.report.MaterialMasterId = ob.Id;
        $scope.report.MaterialMasterName = ob.UserName;
        angular.element(document.querySelector('#materialmastersearchpopup')).modal('hide');
    };

};






