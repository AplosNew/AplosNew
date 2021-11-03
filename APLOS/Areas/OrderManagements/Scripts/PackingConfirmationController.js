'use strict';
PackingConfirmationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function PackingConfirmationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Packing Confirmation";
    $scope.PackingConfirmationList = [];

    $scope.GetPackingConfirmationList = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/PackingConfirmation/GetDataList'
        }).then(function successCallback(response) {
            $scope.PackingConfirmationList = response.data;

        });
    };
    $scope.GetPackingConfirmationList();

    $scope.model = {
        Id: null, ProductionOrderId: null, Qty: 0, UoMId: null, NetWeight: 0, GrossWeight: 0, WeightUoMId: null, LotNo: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, IsPackingSKURequired: false, PackingForm: null, Entity: null
        , EntityId: null, ProcessId: null, ProductionDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'), ProductionShiftId: null, MaterialMasterId: null, ArticleId: null, Quantity: 0
    }
    $scope.packingContenNew = Object.assign({}, $scope.model);
    $scope.productionSummaryNew = Object.assign({}, $scope.model);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.packingContenNew.EntityId = $scope.entityList[0].Value;
                //default
                $scope.GetPackingProcessCbo($scope.packingContenNew.EntityId);
            }
        });
    };
    $scope.getAllEntities();

    $scope.processList = [];
    $scope.GetPackingProcessCbo = function (entityid) {
        $http({
            method: 'GET',
            url: "OrderManagements/PackingConfirmation/GetPackingProcessCbo?entity=" + entityid
        }).then(function successCallback(response) {
            $scope.processList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.packingContenNew.ProcessId = $scope.processList[0].Id;
            }
            $scope.GetSFGMovementToCbo();
        });
    };

    $scope.listToProcessOrSFGInventory = [];
    $scope.GetSFGMovementToCbo = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/PackingConfirmation/GetToProcessCbo?FromId=' + $scope.packingContenNew.ProcessId + "&EntityId=" + $scope.packingContenNew.EntityId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.listToProcessOrSFGInventory = response.data;
                if (baseService.arrayLength(response.data) === 1) {
                    $scope.packingContenNew.ToProcessId = $scope.listToProcessOrSFGInventory[0].ToId;
                }
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.shiftList = [];
    cboService.GetProductionShiftCbo(function (result) {
        $scope.shiftList = result;
        if (baseService.arrayLength(result) === 1) {
            $scope.packingContenNew.ProductionShiftId = $scope.shiftList[0].Value;
        }
    });

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required...";
            }

        } catch (ex) {
            throw ex;
        }
    }

    function ValidationPreMaster() {
        try {
            CheckField("Entity", $scope.packingContenNew.EntityId);
            CheckField("Process", $scope.packingContenNew.ProcessId);
            CheckField("Production Date", $scope.packingContenNew.ProductionDate);
            CheckField("Shift", $scope.packingContenNew.ProductionShiftId);
        } catch (ex) {
            throw ex;
        }
    }

    $scope.IsGo = false;
    $scope.masterGo = function (isdisabled) {
        try {
            ValidationPreMaster();
            $scope.SetGo(isdisabled);
            //$scope.getLineGrid();

        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SetGo = function (isdisabled) {
        $scope.IsGo = isdisabled;
    };

    $scope.SetBack = function (isdisabled) {
        $scope.IsGo = isdisabled;
        $scope.ClearMasterPart();

    };

    $scope.ClearMasterPart = function () {
        $scope.ProductionOrderId = $scope.packingContenNew.ProductionOrderId;
        var entityid = $scope.packingContenNew.EntityId;
        var processid = $scope.packingContenNew.ProcessId;
        var workdate = $scope.packingContenNew.ProductionDate;
        var shiftid = $scope.packingContenNew.ProductionShiftId;

    };

    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        $scope.ProductionOrderList = [];
        $http.get("OrderManagements/PackingConfirmation/GetProductionOrderDataList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ProductionOrderList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };
    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.SalesOrderListForProductionOrderId = [];
    $scope.getSalesOrderOfProdOrderList = function (prodOrdId) {
        $scope.openPopup('dialogSOItemsForProductionOrder');
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }


    $scope.PackingContentDataListByPR = [];
    $scope.GetPackingContentDataByPRId = function (obj) {
        $scope.packingContenNew.ProductionOrderId = obj.data.POId;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
        $http({
            method: 'GET',
            url: 'OrderManagements/PackingContent/GetPackingContentDataByPRIdWithTran?PRId=' + $scope.packingContenNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.PackingContentDataListByPR = response.data;
            $scope.packingContenNew.PackingForm = "No of " + response.data[0].PackingForm;
            $scope.packingContenNew.QtyPackingForm = "Qty/" + response.data[0].PackingForm;
            $scope.packingContenNew.ConPackingForm = "Confirmed " + response.data[0].PackingForm;
            $scope.packingContenNew.BalancePackingForm = "Balance " + response.data[0].PackingForm;
            $scope.packingContenNew.ColumnName =  response.data[0].PackingForm;
        });

    };


    $scope.PackingContentDataList = [];
    $scope.getDetailData = function (obj) {
        $scope.PackingContentDataList = [];
        $http.get("OrderManagements/PackingContent/GetPackingContentDetailDataList?MasterId=" + obj.data.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.PackingContentDataList = response.data;
                        angular.element(document.querySelector('#PackingContentPopUp')).modal('show');
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };


    $scope.ClosePackingContentPopUp = function () {
        angular.element(document.querySelector('#PackingContentPopUp')).modal('hide');
    }

    $scope.lineItemNo = [];
    $scope.getPackingChildData = function (obj) {
        $scope.lineItemNo = [];
        $http.get("OrderManagements/PackingContent/GetPackingChildDataList?MasterId=" + obj.data.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.lineItemNo = response.data;


                        $scope.PackingContentDataList = [];
                        $http.get("OrderManagements/PackingContent/GetPackingContentDetailDataList?MasterId=" + obj.data.Id)
                            .then(
                                function successCallback(response) {
                                    if (baseService.arrayLength(response.data) > 0) {
                                        $scope.PackingContentDataList = response.data;
                                    }
                                },
                                function errorCallback(response) {
                                    ShowResult(response, 'failure');
                                });


                        angular.element(document.querySelector('#IPPopup')).modal('show');
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.ConfirmedPack = 0;
    $scope.PackMasterId = null;
    $scope.PackingContentDataMultiByPackList = [];
    $scope.PackingContentSaveList = [];
    $scope.lineItemNoNew = [];

    $scope.CloseIPPopup = function () {

        $scope.ConfirmedPack = 0;
        $scope.PackMasterId = null;

        for (var i = 0; i < $scope.lineItemNo.length; i++) {
            if ($scope.lineItemNo[i].IsConfirmed == true && $scope.lineItemNo[i].State == false) {
                $scope.ConfirmedPack++;
                $scope.PackMasterId = $scope.lineItemNo[i].PackingContentMasterId;

                $scope.lineItemNoNew.push($scope.lineItemNo[i]);
            }
        }

        for (var i = 0; i < $scope.PackingContentDataListByPR.length; i++) {
            if ($scope.PackMasterId == $scope.PackingContentDataListByPR[i].Id) {
                $scope.PackingContentDataListByPR[i].RecvQty = $scope.ConfirmedPack;
            }
        }

        angular.forEach($scope.PackingContentDataList, function (a) {
            $scope.PackingContentSaveList.push({
                Id: null
                , Characteristics1ValueId: a.FirstCharacteristicsValueId
                , Characteristics2ValueId: a.SecondCharacteristicsValueId
                , Characteristics3ValueId: a.ThirdCharacteristicsValueId
                , Qty: a.Qty * $scope.ConfirmedPack
            });

            $scope.packingContenNew.MaterialMasterId = a.MaterialMasterId;
            $scope.packingContenNew.ArticleId = a.ArticleId;

        });

        angular.element(document.querySelector('#IPPopup')).modal('hide');
    }

    $scope.MakeData = function (obj) {
        try {
            if (obj.data.Balance != 0 && obj.data.Balance > 0) {
                if (obj.data.Balance < parseFloat(obj.data.RecvQty)) {
                    ShowResult("Courrent packet cann't greater than No of Packet.", 'failure');
                }
                else {
                    $http.get("OrderManagements/PackingContent/GetPackingChildDataList?MasterId=" + obj.data.Id)
                        .then(
                            function successCallback(response) {
                                if (baseService.arrayLength(response.data) > 0) {
                                    $scope.lineItemNo = response.data;

                                    $scope.PackingContentDataList = [];
                                    $http.get("OrderManagements/PackingContent/GetPackingContentDetailDataList?MasterId=" + obj.data.Id)
                                        .then(
                                            function successCallback(response) {
                                                if (baseService.arrayLength(response.data) > 0) {
                                                    $scope.PackingContentDataList = response.data;

                                                    for (var i = 0; i < $scope.lineItemNo.length; i++) {
                                                        if ($scope.lineItemNo[i].IsConfirmed == false && $scope.lineItemNo[i].State == false) {
                                                            $scope.lineItemNo[i].IsConfirmed = true;
                                                            $scope.lineItemNoNew.push($scope.lineItemNo[i]);
                                                        }
                                                    }

                                                    //for (var i = 0; i < $scope.PackingContentDataList.length; i++) {
                                                    //    $scope.PackingContentDataList[i].NewQty = $scope.PackingContentDataList[i].Qty * obj.data.RecvQty;

                                                    //    $scope.packingContenNew.MaterialMasterId = $scope.PackingContentDataList[i].MaterialMasterId;
                                                    //    $scope.packingContenNew.ArticleId = $scope.PackingContentDataList[i].ArticleId;
                                                    //}

                                                    angular.forEach($scope.PackingContentDataList, function (a) {
                                                        $scope.PackingContentSaveList.push({
                                                            Id: null
                                                            , Characteristics1ValueId: a.FirstCharacteristicsValueId
                                                            , Characteristics2ValueId: a.SecondCharacteristicsValueId
                                                            , Characteristics3ValueId: a.ThirdCharacteristicsValueId
                                                            , Qty: a.Qty * obj.data.RecvQty
                                                        });

                                                        $scope.packingContenNew.MaterialMasterId = a.MaterialMasterId;
                                                        $scope.packingContenNew.ArticleId = a.ArticleId;

                                                    });

                                                }
                                            },
                                            function errorCallback(response) {
                                                ShowResult(response, 'failure');
                                            });
                                }
                            },
                            function errorCallback(response) {
                                ShowResult(response, 'failure');
                            });
                }
            } else {
                ShowResult("There in no more Packet.", 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Save = function () {
        try {
            angular.copy($scope.packingContenNew, $scope.productionSummaryNew);

            $scope.ProdQty = 0;
            if (baseService.arrayLength($scope.PackingContentSaveList) > 0) {
                for (var i = 0; i < $scope.PackingContentSaveList.length; i++) {

                    if (!baseService.isUndefinedOrNull($scope.PackingContentSaveList[i].Qty)) {
                        $scope.ProdQty = $scope.ProdQty + $scope.PackingContentSaveList[i].Qty;
                    }
                }
                $scope.productionSummaryNew.Quantity = $scope.ProdQty;
            }

            $http({
                method: 'POST',
                url: 'OrderManagements/PackingConfirmation/Create',
                data: {
                    "ps": $scope.productionSummaryNew,
                    "pc": $scope.productionSummaryNew,
                    "psd": $scope.PackingContentSaveList,
                    "packingChild": $scope.lineItemNoNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.ClearMasterPart();
                    $scope.GetPackingConfirmationList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Clear = function () {
        $scope.SetBack();
        $scope.packingContenNew = Object.assign({}, $scope.model);
        $scope.PackingContentDataList = [];
    }




}

