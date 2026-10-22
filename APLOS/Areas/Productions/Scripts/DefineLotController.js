'use strict';
DefineLotController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function DefineLotController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Define Lot';
    $scope.path = 'Productions/Packing/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.Action = "Save";
    $scope.skuList = [];

    $scope.modal = {
        Id: null,
        Sequence: null,
        LotNumber: null,
        FromDate: null,
        ToDate: null,
        ProcessId: null,
        LotQty: null,
        LotWeight: null,
        NoOfUnit: null,
        PerUnitWeight: null,
        LotStatus: "Active",
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.modalNew = Object.assign({}, $scope.modal);


    $scope.LotStatusList = [{ Value: 'Active', Text: "Active" }, { Value: 'Running', Text: "Running" }, { Value: 'Pending', Text: "Pending" }, { Value: 'Closed', Text: "Closed" }]

    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.ModelList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetDefineLot",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();


    $scope.GetLotNumber = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetLotNumber",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.modalNew.LotNumber = response.data;
        });
    };
    $scope.GetLotNumber();

    $scope.GetScanFromToDate = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetScanFromToDate",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.modalNew.FromDate = response.data[0].FromDate;
            $scope.modalNew.ToDate = response.data[0].ToDate;
        });
    };
    $scope.GetScanFromToDate();

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.modal.Sequence = data;
            $scope.modalNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.SubProductionOrderList = [];
    $scope.skuList = [];
    $scope.purposeList = [];
    $http({
        method: 'GET',
        url: "Productions/MovementMaterialMaster/getPurposeCbo",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.purposeList = response.data;
    });

    $scope.getLocation = function () {
        if (!baseService.isUndefinedOrNull($scope.modalNew.PurposeId)) {
            var paymentTerm = $.grep($scope.purposeList, function (item) { return item.Value === $scope.modalNew.PurposeId; })[0];
            $scope.modalNew.Location = paymentTerm.FromToLocation;
            $scope.modalNew.MaterialMovementMasterId = paymentTerm.MaterialMovementMasterId;
            $scope.modalNew.ProcessId = paymentTerm.ProcessId;

        }
    };

    $scope.CalNoOfUnit = function () {
        if (!baseService.isUndefinedOrNull($scope.modalNew.LotWeight) && !baseService.isUndefinedOrNull($scope.modalNew.PerUnitWeight)) {
            $scope.modalNew.NoOfUnit = Math.ceil(
                ($scope.modalNew.LotWeight * 1000) / $scope.modalNew.PerUnitWeight
            );
        }
    };


    $scope.CustomersList = [];
    $http({
        method: 'GET',
        url: $scope.path + "getCustomers",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.CustomersList = response.data;
    });

    $scope.selectCustomer = function () {
        angular.element(document.querySelector('#customersModal')).modal('show');
    }

    $scope.doubleCustomer = function (e) {
        $scope.modalNew.Customer = e.data.username;
        $scope.modalNew.CustomerId = e.data.id;
        angular.element(document.querySelector('#customersModal')).modal('hide');
    }

    $scope.ProcessList = [];
    $scope.GetProcessByCompany = function () {
        cboService.getCompanyProductionProcessCbo($window.companyId, function (response) {
            $scope.ProcessList = response;
        });
    }
    $scope.GetProcessByCompany();


    $scope.Get = function (args) {
        $scope.modalNew = Object.assign({}, args.data);
        $scope.Getskudata();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.Getskudata = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'GET',
            url: "Productions/Packing/GetSUBPOData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.skuList = response.data;
        });
    }

    $scope.selectSinglePackingCategory = function (selectedItem) {
        try {
            if (selectedItem.Flag) {
                var alreadySelected = false;
                angular.forEach($scope.skuList, function (item) {
                    if (item !== selectedItem && item.Flag) {
                        alreadySelected = true;
                    }
                });

                if (alreadySelected) {
                    selectedItem.Flag = false; // revert the new click
                    throw "Please only one row.";
                }
            }
        } catch (e) {
            ShowResult(e, 'failure')
        }
    };

    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function () {

        $scope.ProductOrderList = [];
        $http.get('Productions/ProductionSummary/GetProductionOrderData?entityid=' + null + '&workCenterMasterId=' + null + '&productionLevel=' + null + '&processId=' + $scope.modalNew.ProcessId + '&status=' + null)
            .then(
                function successCallback(response) {
                    $scope.ProductionOrderList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');

    };

    $scope.SetPrOData = function ($event) {
        $scope.modalNew.ProductionOrderId = $event.data.POId;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');

    }

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

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }

    $scope.SubProductionOrderList = [];
    $scope.GetSPOata = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.modalNew.ProductionOrderId)) {
                throw "Select Production Order.";
            }
            $http({
                method: 'POST',
                data: { 'poId': $scope.modalNew.ProductionOrderId },
                url: 'OrderManagements/ProductionOrder/GetSavedSKUData'
            }).then(function successCallback(response) {
                $scope.SubProductionOrderList = response.data;
                angular.element(document.querySelector('#SPOItemPopup')).modal('show');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.selectSPOItem = function (e) {
        $scope.modalNew.SubProductionOrderId = e.data.ID;
        $scope.modalNew.SalesOrder = e.data.SONo;
        angular.element(document.querySelector('#SPOItemPopup')).modal('hide');
    }

    $scope.skuList = [];
    $scope.masterGo = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.GeneralForm.$valid) {
            $http({
                method: 'GET',
                url: "Productions/Packing/GetSUBPOData?poId=" + $scope.modalNew.ProductionOrderId + "&spoId=" + $scope.modalNew.SubProductionOrderId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.skuList = response.data;
            });
        }
    }


    $scope.cartonDetail = [];
    $scope.inactiveCartons = [];
    $scope.GetScanData = function (data) {
        try {
            if (data.data.Flag) {
                $http({
                    method: 'GET',
                    url: "Productions/Packing/GetScanData?MaterialMovementMasterId=" + $scope.modalNew.MaterialMovementMasterId + "&SONo=" + data.data.SONo + "&SKU1Id=" + data.data.SKU1Id + "&SKU2Id=" + data.data.SKU2Id + "&fromDate=" + $scope.modalNew.FromDate + "&toDate=" + $scope.modalNew.ToDate,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.cartonDetail = response.data;
                    angular.element(document.querySelector('#cartonDetailModal')).modal('show');
                });
            }
            else {
                throw "Select a row.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    // #region checkbox all

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#CartonDetails").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.cartonDetail.length; i++) {
                $scope.cartonDetail[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#CartonDetails").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    //Closing of The Carton Details Modal

    $scope.cartonList = [];
    function MakeData() {

        for (var i = 0; i < $scope.cartonDetail.length; i++) {
            if ($scope.cartonDetail[i].Flag == true) {
                if (checkExists($scope.cartonList, $scope.cartonDetail[i].Id) === false) {
                    var ob = {};
                    ob.Id = $scope.cartonDetail[i].Id;
                    ob.NoOfPcs = $scope.cartonDetail[i].NoOfPcs;
                    ob.RefNo = $scope.cartonDetail[i].RefNo;
                    ob.LotNo = $scope.modalNew.LotNumber;

                    $scope.cartonList.push(ob);
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseCarton = function () {
        try {
            MakeData();
            $scope.Save();
            angular.element(document.querySelector('#cartonDetailModal')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }



    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.GeneralForm.$valid) {

                var fd = $filter('dateFiltering')($scope.modalNew.FromDate, 'dd-MM-yyyy');
                var td = $filter('dateFiltering')($scope.modalNew.ToDate, 'dd-MM-yyyy');

                if (new Date(fd) > new Date(td)) {
                    throw 'From Date cann\'t be greater than To Date.';
                }

                if (baseService.arrayLength($scope.cartonList) > 0) {
                    $scope.modalNew.LotQty = $scope.cartonList.reduce((sum, item) => sum + item.NoOfPcs, 0);
                }
                else {
                    throw "Select Carton."
                }


                $http({
                    method: 'POST',
                    url: "Productions/Packing/CreateLot",
                    data: { 'data': $scope.modalNew, 'list': $scope.cartonList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.cartonList = [];
                        $scope.skuList = [];
                        $scope.cartonDetail = [];
                        ClearFields(response.data.Sequence);
                        $scope.getData();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modalNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modalNew.Id,
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
        $scope.GetLotNumber();
        return true;
    };

    function ClearFields(seq) {
        $scope.GetLotNumber();
        $scope.Action = 'Save';
        $scope.modalNew = Object.assign({}, $scope.modal);
        $scope.modalNew.Sequence = seq;
    }

    $scope.QRCodeGenerateModel = {};
    $scope.pdfdownloadgriddataUrl = 'GridReports/DownloadPdf';
    $scope.generateQRCode = function (obj) {
        try {
            $scope.QRCodeGenerateModel.LotNumber = obj.data.LotNumber;
            $scope.QRCodeGenerateModel.LotId = obj.data.Id;
            $scope.fileName = "QRCode_" + $scope.QRCodeGenerateModel.LotId + ".pdf"; // was "QRCode.pptx" — fix extension too

            $http({
                method: 'POST',
                url: "OrderManagements/ProductionOrder/GenerateLotQRCode",
                data: {
                    'data': $scope.QRCodeGenerateModel
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.pdfdownloadgriddataUrl + "?FileName=" + $scope.fileName);
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }














}