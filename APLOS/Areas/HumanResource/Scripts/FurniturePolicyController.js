'use strict';
FurniturePolicyController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FurniturePolicyController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Furniture Policy';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/FurniturePolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.redirectTab = function () {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.tabForm3.$invalid) {
            $scope.setTab(3);
        }
        else if ($scope.tabForm4.$invalid) {
            $scope.setTab(4);
        }
    };

    //All Lists Are Here
    $scope.FurnitureMasterList = [];
    $scope.FurnitureGridList = [];
    $scope.DesignationMasterList = [];
    $scope.DesignationGridList = [];
    $scope.SelectedList = [];
    
    $scope.ModelTemp = {
        Id: null,
        FurnitureMaster: null,
        DesignationMaster: null,
    };
    $scope.FurniturePolicyNew = Object.assign({}, $scope.ModelTemp);

    
    $scope.getFurnitureMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getFurnitureMaster",
            dataType: 'JSON',

        }).then(function successCallback(response) {
            $scope.FurnitureMasterList = response.data;
        })
    }
    $scope.getFurnitureMaster();

    $scope.getDesignationMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getDesignationMaster",
            dataType: 'JSON',

        }).then(function successCallback(response) {
            $scope.DesignationMasterList = response.data;
        })
    }
    $scope.getDesignationMaster();

    $scope.getFurnitureGridView = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getFurnitureGridView",
            data: {
                'username': $scope.FurniturePolicyNew.FurnitureMaster,
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.FurnitureGridList = response.data;
        })
    }

    $scope.getDesignationGridView = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getDesignationGridView",
            data: {
                'username': $scope.FurniturePolicyNew.DesignationMaster,
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.DesignationGridList = response.data;
        })
    }

    $scope.viewFurniturePolicyGrids = function () {
        $scope.getFurnitureGridView();
        $scope.getDesignationGridView();
    }

    //$scope.checkORuncheck = function (e) {
    //    $('.rowCheckbox').on('change', function () {
    //        if (this.checked) {
    //            $scope.SelectedList.push(this.value);
    //        }
    //        else {
    //            $scope.SelectedList = $scope.SelectedList.filter(item => item != this.value);
    //        }

    //       // $('#show').html(listvalues.sort());
    //    });
    //}

    $scope.ActiveEmpcbx = function (args) {
        $("#cbxhead").ejCheckBox({ "change": chkFilteredData });
    };

    function chkFilteredData(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridOTCompensation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ModelList.length; i++) {
                $scope.ModelList[i].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridOTCompensation").data("ejGrid");
        gridObj.refreshContent();
    };
    
}