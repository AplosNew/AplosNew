'use strict';
MarkerApproveController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function MarkerApproveController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Marker Approve';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.SelectedFabricGRNRowList = [];
    $scope.selectedSOList = [];
    $scope.path = 'Productions/Marker/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.ApproveByStatusList = [
        { 'Value': "Approved", 'Text': "Approved" },
        { 'Value': "Pending", 'Text': "Pending" },
        { 'Value': "Reject", 'Text': "Reject" }
    ];

    $scope.MarkerGroupList = [];
    $scope.GetMarkerGroupCbo = function () {
        try {
            $http.get('Productions/Productionsummary/GetMarkerGroupCbo')
                .then(function (response) {
                    $scope.MarkerGroupList = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.GetMarkerGroupCbo();

    $scope.CutPlantList = [];
    $scope.GetCutPlanCbo = function () {
        try {
            $http.get('Productions/Productionsummary/GetCutPlanCbo')
                .then(function (response) {
                    $scope.CutPlantList = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.GetCutPlanCbo();

    $scope.CutPlantRatioList = [];
    $scope.GetCutPlanRatioCbo = function () {
        $scope.ModelNew.NoOfPcs = 0;
        try {
            $http.get('Productions/Productionsummary/GetCutPlanRatioCbo?masterId=' + $scope.ModelNew.CutPlanId)
                .then(function (response) {
                    $scope.CutPlantRatioList = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.GetNoOfPcs = function () {
        for (var i = 0; i < $scope.CutPlantRatioList.length; i++) {
            if ($scope.CutPlantRatioList[i].Id == $scope.ModelNew.CutPlanRatioId) {
                $scope.ModelNew.NoOfPcs = $scope.CutPlantRatioList[i].AllotedQty;
                break;
            }
        }
    }

    $scope.GetGrossWeight = function () {
        $scope.ModelNew.GrossWeight = ($scope.ModelNew.Width * $scope.ModelNew.GrossLength * $scope.ModelNew.GSM) / TBA;
    }

    $scope.GetNetWeight = function () {
        $scope.ModelNew.NetWeight = ($scope.ModelNew.CutableWidth * $scope.ModelNew.NetLength * $scope.ModelNew.GSM) / TBA;
    }

   
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetApproveByList",
            data: {},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
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
        Active: true,
        FGMaterialMasterId: null,
        FGMaterialMaster: null,
        FGArticleId: null,
        FGArticle: null,
        FabricWidthId: null,
        ShrinkageGroupId: null,
        CharacteristicsId: null,
        ShadeId: null,
        Length: null,
        Attachment: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetCutPlanCbo();
        $scope.GetCutPlanRatioCbo();
        $scope.GetMarkerSO();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetMarkerSO = function () {
        try {
            $http({
                method: 'Get',
                url: 'Productions/Marker/GetMarkerSOData?markerId=' + $scope.ModelNew.Id
            }).then(function successCallback(response) {
                $scope.selectedSOList = response.data;
                $scope.GetMarkerFabricGRNRowList();
            }
            )
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.GetMarkerFabricGRNRowList = function () {
        try {
            if ($scope.selectedSOList.length > 0) {
                var uniqueSOId = removeDuplicates($scope.selectedSOList, 'SalesOrderId');
                var wcSOId = "";
                if (uniqueSOId.length > 0) {
                    wcSOId = "IN(";
                    wcSOId += Array.prototype.map.call(uniqueSOId, function (item) { return "'" + item.SalesOrderId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcSOId;
            }
            $http({
                method: 'Get',
                url: 'Productions/Marker/GetMarkerFabricGRNRowList?soId=' + $scope.sqlInStatement + '&markerId=' + $scope.ModelNew.Id
            }).then(function successCallback(response) {
                $scope.SelectedFabricGRNRowList = response.data;
            }
            )
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = {
            Id: null,
            Sequence: 0,
            Code: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Description: null,
            Remarks: null,
            Active: true,
            FGMaterialMasterId: null,
            FGMaterialMaster: null,
            FGArticleId: null,
            FGArticle: null,
            FabricWidthId: null,
            ShrinkageGroupId: null,
            CharacteristicsId: null,
            ShadeId: null,
            Length: null,
        };
       
    }

}