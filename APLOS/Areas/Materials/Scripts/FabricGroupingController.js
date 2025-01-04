'use strict';
FabricGroupingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FabricGroupingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Fabric Grouping';
    $scope.Action = 'Save';
    $scope.fabricPendingDetailList = [];
    $scope.path = 'Materials/FabricRoll/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.getPendingData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricRollMaster",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.fabricPendingDetailList = response.data;
        });
    }
    $scope.getPendingData();

    $scope.fabricPendingChildListList = [];
    $scope.GetFabricRollChildList = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricRollChildList?FabricRollManagementMasterId=" + $scope.masterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.fabricPendingChildListList = response.data;
        });
    }

    $scope.Get = function (args) {
        $scope.fabricRollMaster = Object.assign({}, args.data);
        $scope.masterId = args.data.Id;

        $scope.GetFabricRollChildList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ShowModal = function () {
        try {
            var getRow = $filter("filter")($scope.fabricPendingChildListList, { "Flag": true });
            if (getRow.length === 0)
                throw "Select data.";

            angular.element(document.querySelector('#entryPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SetDataEntry = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.Model.CutableWidth)) {
                throw "CutableWidth is required.";
            }
            if (baseService.isUndefinedOrNull($scope.Model.Shade)) {
                throw "Shade is required.";
            }
            if (baseService.isUndefinedOrNull($scope.Model.ShrinkageLengthWise)) {
                throw "ShrinkageLengthWise is required.";
            }
            if (baseService.isUndefinedOrNull($scope.Model.ShrinkageWidthWise)) {
                throw "ShrinkageWidthWise is required.";
            }
            for (var i = 0; i < $scope.fabricPendingChildListList.length; i++) {
                if ($scope.fabricPendingChildListList[i].Flag) {
                    $scope.fabricPendingChildListList[i].Color = $scope.Model.Color;
                    $scope.fabricPendingChildListList[i].LotNo = $scope.Model.LotNo;
                    $scope.fabricPendingChildListList[i].FabricType = $scope.Model.FabricType;
                    $scope.fabricPendingChildListList[i].FabricQuality = $scope.Model.FabricQuality;
                    $scope.fabricPendingChildListList[i].SupplierRollNo = $scope.Model.SupplierRollNo;
                    $scope.fabricPendingChildListList[i].OwnRollNo = $scope.Model.OwnRollNo;
                    $scope.fabricPendingChildListList[i].SupplierQty = $scope.Model.SupplierQty;
                    $scope.fabricPendingChildListList[i].ActualQty = $scope.Model.ActualQty;
                    $scope.fabricPendingChildListList[i].CutableWidth = $scope.Model.CutableWidth;
                    $scope.fabricPendingChildListList[i].OwnGSM = $scope.Model.OwnGSM;
                    $scope.fabricPendingChildListList[i].StdGSM = $scope.Model.StdGSM;
                    $scope.fabricPendingChildListList[i].GSMVariation = $scope.Model.GSMVariation;
                    $scope.fabricPendingChildListList[i].GSMVariationPer = $scope.Model.GSMVariationPer;
                    $scope.fabricPendingChildListList[i].Shade = $scope.Model.Shade;
                    $scope.fabricPendingChildListList[i].ShrinkageLengthWise = $scope.Model.ShrinkageLengthWise;
                    $scope.fabricPendingChildListList[i].ShrinkageWidthWise = $scope.Model.ShrinkageWidthWise;
                    $scope.fabricPendingChildListList[i].Dia = $scope.Model.Dia;
                    $scope.fabricPendingChildListList[i].SupplierQualityGrade = $scope.Model.SupplierQualityGrade;
                    $scope.fabricPendingChildListList[i].QualityStatus = $scope.Model.QualityStatus;
                    $scope.fabricPendingChildListList[i].FTPReportNo = $scope.Model.FTPReportNo;
                    $scope.fabricPendingChildListList[i].FTPReceiveDate = $scope.Model.FTPReceiveDate;
                    $scope.fabricPendingChildListList[i].FTPStatus = $scope.Model.FTPStatus
                }
            }
            $scope.CloseModal();
            var gridObj = $("#GridPF").data("ejGrid");
            gridObj.refreshContent();
            gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.CloseModal = function () {
        angular.element(document.querySelector('#entryPopUp')).modal('hide');
    }


    $scope.SaveRollData = function () {
        try {

            

            $http({
                method: "POST",
                url: 'Materials/FabricRoll/CreateFabricRollManage',
                data: {
                    "data": $scope.fabricRollMaster
                    , "grnDetailList": $scope.fabricPendingChildListList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getPendingData();
                    $scope.GetFabricRollChildList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

}