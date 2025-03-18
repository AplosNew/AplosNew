'use strict';
MarkerController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function MarkerController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Marker';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/Marker/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);


    $scope.WidthUomList = [];
    $scope.GetWidthUnit = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetWidthUnit",
        }).then(function successCallback(response) {
            $scope.WidthUomList = response.data;
            for (var i = 0; i < $scope.WidthUomList.length; i++) {
                if ($scope.WidthUomList[i].UserName == "Inch") {
                    $scope.ModelNew.WidthUomId = $scope.WidthUomList[i].Id;
                    break;
                }
            }
        });
    }
    $scope.GetWidthUnit();

    $scope.lengthUomList = [];
    $scope.GetLengthUnit = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetLengthUnit",
        }).then(function successCallback(response) {
            $scope.lengthUomList = response.data;
            for (var i = 0; i < $scope.lengthUomList.length; i++) {
                if ($scope.lengthUomList[i].UserName == "Yard") {
                    $scope.ModelNew.LengthUomId = $scope.lengthUomList[i].Id;
                    break;
                }
            }
        });
    }
    $scope.GetLengthUnit();

    $scope.CheckByList = [];
    $scope.GetCheckByCboList = function () {
        $http({
            method: 'GET',
            url: 'Productions/Marker/GetCheckByCbo'
        }).then(function successCallback(response) {
            $scope.CheckByList = response.data;
            if (baseService.arrayLength($scope.CheckByList) == 1) {
                $scope.ModelNew.CheckById = $scope.CheckByList[0].Value;
            }
        });
    }
    $scope.GetCheckByCboList();

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

    $scope.MasterPlanList = [];
    $scope.GetMasterPlanPopUp = function () {
        $http({
            method: 'Get',
            url: 'Productions/MasterPlan/GetMasterPlanList'
        }).then(function successCallback(response) {
            $scope.MasterPlanList = response.data;
            var gridObj = $("#GridMasterPlan").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MasterPlanPoPUp')).modal('show');
        }
        )
    }

    $scope.SOList = [];
    $scope.GetSOPopUp = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.MasterPlanId)) {
                throw "Select Master Plan";
            }
            $http({
                method: 'Get',
                url: 'Productions/Marker/GetSOList?masterPlanId=' + $scope.ModelNew.MasterPlanId
            }).then(function successCallback(response) {
                $scope.SOList = response.data;
                var gridObj = $("#GridSO").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
                angular.element(document.querySelector('#SOPoPUp')).modal('show');
            }
            )
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.CloseSOPopUp = function () {
        angular.element(document.querySelector('#SOPoPUp')).modal('hide');
    }

    $scope.sqlInStatement = "";
    $scope.selectedSOList = [];
    $scope.ApplySOSelection = function () {
        for (var i = 0; i < $scope.SOList.length; i++) {
            if ($scope.SOList[i].Flag == true) {
                if (checkExists($scope.selectedSOList, $scope.SOList[i].SONo) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.SONo = $scope.SOList[i].SONo;
                    ob.SalesOrderId = $scope.SOList[i].SONo;
                    ob.DeliveryDate = $scope.SOList[i].DeliveryDate;
                    ob.OwnReferenceNo = $scope.SOList[i].OwnReferenceNo;
                    ob.BuyerReferenceNo = $scope.SOList[i].BuyerReferenceNo;
                    ob.Qty = $scope.SOList[i].Qty;
                    ob.SOPlanQty = $scope.SOList[i].SOPlanQty;
                    ob.Remarks = $scope.SOList[i].Remarks;

                    $scope.selectedSOList.push(ob);
                }
            }
        }
        $scope.CloseSOPopUp();
    }

    $scope.FabricGRNRowList = [];
    $scope.GetFabricGRNRowList = function () {
        $scope.FabricGRNRowList = [];
        if ($scope.selectedSOList.length > 0) {
            var uniquePackingId = removeDuplicates($scope.selectedSOList, 'SalesOrderId');
            var wcSOId = "";
            if (uniquePackingId.length > 0) {
                wcSOId = "IN(";
                wcSOId += Array.prototype.map.call(uniquePackingId, function (item) { return "'" + item.SalesOrderId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcSOId;
        }
        $http({
            method: 'GET',
            url: "Productions/Marker/GetFabricGRNRowList?soId=" + $scope.sqlInStatement
        }).then(function (response) {
            $scope.FabricGRNRowList = response.data;
        });
        angular.element(document.querySelector('#FabricGRNRowPoPUp')).modal('show');
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.CloseFabricGRNRowPopUp = function () {
        angular.element(document.querySelector('#FabricGRNRowPoPUp')).modal('hide');

    }

    $scope.SelectedFabricGRNRowList = [];
    $scope.SelectFabricGRNRow = function () {
        for (var i = 0; i < $scope.FabricGRNRowList.length; i++) {
            if ($scope.FabricGRNRowList[i].FlagG == true) {
                if (checkExistsGRN($scope.SelectedFabricGRNRowList, $scope.FabricGRNRowList[i].InventoryReceiveDetailId, $scope.FabricGRNRowList[i].FirstCharacteristicsValueId) === false) {
                    var ob = {};
                    ob.Id = $scope.FabricGRNRowList[i].Id;
                    ob.InventoryReceiveDetailId = $scope.FabricGRNRowList[i].InventoryReceiveDetailId;
                    ob.GRNNo = $scope.FabricGRNRowList[i].GRNNo;
                    ob.GRNDate = $scope.FabricGRNRowList[i].GRNDate;
                    ob.MaterialMasterName = $scope.FabricGRNRowList[i].MaterialMasterName;
                    ob.ArticleName = $scope.FabricGRNRowList[i].ArticleName;
                    ob.SKUValue = $scope.FabricGRNRowList[i].SKUValue;
                    ob.UOM = $scope.FabricGRNRowList[i].UOM;
                    ob.FirstCharacteristicsValueId = $scope.FabricGRNRowList[i].FirstCharacteristicsValueId;
                    ob.TransactionQty = $scope.FabricGRNRowList[i].TransactionQty;

                    $scope.SelectedFabricGRNRowList.push(ob);
                }
            }
        }
        $scope.CloseFabricGRNRowPopUp();
    }


    function checkExistsGRN(list, id, Fid) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InventoryReceiveDetailId === id && list[i].FirstCharacteristicsValueId === Fid) {
                return true;
            }
        }
        return false;
    }
    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalesOrderId === id) {
                return true;
            }
        }
        return false;
    }

    // #region checkbox all

    $scope.refreshTemplateSO = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllSO });
    };

    function CheckBoxSelectAllSO(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSO").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SOList.length; i++) {
                $scope.SOList[i].Flag = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSO").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.refreshTemplateGRN = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllGRN });
    };

    function CheckBoxSelectAllGRN(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridGRR").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.FabricGRNRowList.length; i++) {
                $scope.FabricGRNRowList[i].FlagG = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridGRR").data("ejGrid");
        gridObj.refreshContent();
    };


    // #endregion checkbox all
    $scope.MasterPlanId = null;
    $scope.SetMasterPlan = function (args) {

        $scope.ModelNew.MasterPlanId = args.data.Id;
        if ($scope.MasterPlanId != $scope.ModelNew.MasterPlanId) {
            $scope.selectedSOList = [];
        }
        $scope.MasterPlanId = $scope.ModelNew.MasterPlanId;
        $scope.ModelNew.MasterPlan = args.data.PlanName;
        $scope.GetCutPlanCbo();
        $scope.CloseMasterPlanPopUp();
    }

    $scope.CloseMasterPlanPopUp = function () {
        angular.element(document.querySelector('#MasterPlanPoPUp')).modal('hide');
    }

    $scope.CutPlantList = [];
    $scope.GetCutPlanCbo = function () {
        try {
            $http.get('Productions/Productionsummary/GetCutPlanCbo?masterPlanId=' + $scope.ModelNew.MasterPlanId)
                .then(function (response) {
                    $scope.CutPlantList = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

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
                if (!baseService.isUndefinedOrNull($scope.ModelNew.GSM)) {
                    $scope.ModelNew.NetConsumptionperPcs = $scope.ModelNew.NetWeight / $scope.ModelNew.NoOfPcs;
                    $scope.ModelNew.GrossConsumptionperPcs = $scope.ModelNew.GrossWeight / $scope.ModelNew.NoOfPcs;
                }
                break;
            }
        }
    }

    $scope.GetGrossWeight = function () {
        $scope.wuom = $("#WidthUomId option:selected").text();
        $scope.luom = $("#LengthUomId option:selected").text();
       
        if ($scope.wuom == "Yard" && $scope.luom == "Yard") {
            $scope.ModelNew.GSM = ($scope.ModelNew.Width * 0.914) * ($scope.ModelNew.GrossLength * 0.914);
            $scope.ModelNew.GrossWeight = parseFloat((($scope.ModelNew.Width * 0.914) * ($scope.ModelNew.GrossLength * 0.914) * $scope.ModelNew.GSM) / 1000).toFixed(2);

        } else if ($scope.wuom == "Inch" && $scope.luom == "Yard") {
            $scope.ModelNew.GSM = ($scope.ModelNew.Width * 0.0254) * ($scope.ModelNew.GrossLength * 0.914);
            $scope.ModelNew.GrossWeight = parseFloat((($scope.ModelNew.Width * 0.0254) * ($scope.ModelNew.GrossLength * 0.914) * $scope.ModelNew.GSM) / 1000).toFixed(2);

        } else {
            
            if ($scope.luom == "Yard") {
                $scope.ModelNew.GSM = ($scope.ModelNew.Width / 100) * ($scope.ModelNew.GrossLength * 0.914);
                $scope.ModelNew.GrossWeight = parseFloat((($scope.ModelNew.Width / 100) * ($scope.ModelNew.GrossLength * 0.914) * $scope.ModelNew.GSM) / 1000).toFixed(2);

            }
            else {
                $scope.ModelNew.GSM = ($scope.ModelNew.Width / 100) * $scope.ModelNew.GrossLength;
                $scope.ModelNew.GrossWeight = parseFloat((($scope.ModelNew.Width / 100) * $scope.ModelNew.GrossLength * $scope.ModelNew.GSM) / 1000).toFixed(2);
            }
        }
        if (!baseService.isUndefinedOrNull($scope.ModelNew.NoOfPcs)) {
            $scope.ModelNew.GrossConsumptionperPcs = $scope.ModelNew.GrossWeight / $scope.ModelNew.NoOfPcs;
        }
    }

    $scope.GetNetWeight = function () {
        $scope.wuom = $("#WidthUomId option:selected").text();
        $scope.luom = $("#LengthUomId option:selected").text();

        if ($scope.wuom == "Yard" && $scope.luom == "Yard") {
            $scope.ModelNew.GSM = ($scope.ModelNew.CutableWidth * 0.914) * ($scope.ModelNew.NetLength * 0.914);
            $scope.ModelNew.NetWeight = parseFloat((($scope.ModelNew.CutableWidth * 0.914) * ($scope.ModelNew.NetLength * 0.914) * $scope.ModelNew.GSM) / 1000).toFixed(2);

        } else if ($scope.wuom == "Inch" && $scope.luom == "Yard") {
            $scope.ModelNew.GSM = ($scope.ModelNew.CutableWidth * 0.0254) * ($scope.ModelNew.NetLength * 0.914);
            $scope.ModelNew.NetWeight = parseFloat((($scope.ModelNew.CutableWidth * 0.0254) * ($scope.ModelNew.NetLength * 0.914) * $scope.ModelNew.GSM) / 1000).toFixed(2);

        } else {

            if ($scope.luom == "Yard") {
                $scope.ModelNew.GSM = ($scope.ModelNew.CutableWidth / 100) * ($scope.ModelNew.NetLength * 0.914);
                $scope.ModelNew.NetWeight = parseFloat((($scope.ModelNew.CutableWidth / 100) * ($scope.ModelNew.NetLength * 0.914) * $scope.ModelNew.GSM) / 1000).toFixed(2);

            }
            else {
                $scope.ModelNew.GSM = ($scope.ModelNew.CutableWidth / 100) * $scope.ModelNew.NetLength;
                $scope.ModelNew.NetWeight = parseFloat((($scope.ModelNew.CutableWidth / 100) * $scope.ModelNew.NetLength * $scope.ModelNew.GSM) / 1000).toFixed(2);
            }
        }
        if (!baseService.isUndefinedOrNull($scope.ModelNew.NoOfPcs)) {
            $scope.ModelNew.NetConsumptionperPcs = $scope.ModelNew.NetWeight / $scope.ModelNew.NoOfPcs;
        }
    }

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





    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: {},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
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

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();
    $scope.CustomeFileName = null;
    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.getFGCharacteristicsListNew($scope.ModelNew.FGMaterialMasterId, $scope.ModelNew.Id);
        $scope.HeaderName = $scope.ModelNew.HeaderName;
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Attachment)) {
            var str = $scope.ModelNew.Attachment;
            var extention = str.substr(str.indexOf('.'));
            $scope.CustomeFileName = $scope.ModelNew.Id + extention;
        }
        //$scope.filedata.name = $scope.ModelNew.Attachment;

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
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
        $scope.ModelNew.Sequence = seq;
        $scope.FGCharacteristicsValueList = [];
        $scope.characteristicsList = [];
        $scope.SelectFGCharacteristicsValueList = [];
        $scope.SKUDisable = false;
    }

    $scope.FabricWidthList = [];
    $scope.getFabricWidth = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricWidth",
        }).then(function successCallback(response) {
            $scope.FabricWidthList = response.data;
        });
    }
    $scope.getFabricWidth();




}