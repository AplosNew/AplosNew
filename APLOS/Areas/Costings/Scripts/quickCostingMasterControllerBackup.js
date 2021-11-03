'use strict';
OrderCostingController_backup.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$controller', '$filter', 'cboService', '$window', 'fileReader'];
function OrderCostingController_backup(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $controller, $filter, $cboService, $window, fileReader) {
    $rootScope.title = 'Order Costing';
    $scope.ModelList = [];
    $scope.path = 'Costings/OrderCosting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.Action = 'Save';
    $scope.searchBy = "UserName"; $scope.searchBySO = "MasterOrderId"; $scope.searchSO = ''; $scope.search = "";
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.partyType = 'Customer';
    $scope.piemarker = { dataLabel: { visible: true, shape: 'none', connectorLine: { type: 'bezier', color: 'black' }, font: { size: '14px' } } };

    $scope.CostingSummaryDataMain = { BuyerTotal: 0, QuickCostingValue: 0, OrderCostingValue: 0, ProfitQuickCosting: 0, ProfitOrderCosting: 0 };
    $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.TotalSegmentedCostValue = 0;
    $scope.CostingComponentId = null;
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

    $scope.UnitOfMeasurementList = [];
    cboService.getUnitOfMeasurementCbo(function (response) {
        $scope.UnitOfMeasurementList = response;

    });
    $scope.tranCurrencyList = [];
    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    $scope.IsShowEntryForm = false;
    $scope.CreateNewPopUp = function () {
        $scope.message_confirmation = "Do you want to create from template ?";
        angular.element(document.querySelector("#CreateNewPopUp")).modal("show");
    }
    $scope.ModelList1 = [];
    $scope.getCostingPopUp = function () {
        $http({
            method: 'POST',
            url: 'Costings/QuickCostingMaster/GetList',
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList1 = response.data;
        });
        angular.element(document.querySelector('#CostingTemplatePopUp')).modal('show');
    }
    $scope.tempModel = {};
    $scope.toCopyPopup = function (args) {
        $scope.tempModel = args.data;
        $scope.tempModel.CostingMasterTemplateId = $scope.tempModel.Id;
        $scope.tempModel.Id = null;
        $scope.message_confirmation = "Are you sure ?";

        //
        angular.element(document.querySelector("#costingTemplateCopy")).modal("show");
    };
    $scope.showOrdercostingUniqueFieldFormPopUp = function () {

        angular.element(document.querySelector('#CostingTemplatePopUp')).modal('hide');
        angular.element(document.querySelector("#orderCostingUniqueFieldpopUp")).modal("show");
    }
    $scope.closeCostingPopUp = function () {
        //$scope.tempModel = Object.assign({}, $scope.tempModelNew);

        angular.element(document.querySelector('#CostingTemplatePopUp')).modal('hide');
        angular.element(document.querySelector('#orderCostingUniqueFieldpopUp')).modal('hide');
        //$scope.showOrderCostingForm();
        //$scope.GetOrderModelList();
    }
    $scope.CopyCosting = function () {

        $http({
            method: 'POST',
            url: $scope.path + 'CopyCostingTemplate',
            dataType: 'JSON',
            data: { CopyData: $scope.tempModel }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Get(response);
                $scope.tempModel = {};
                angular.element(document.querySelector('#orderCostingUniqueFieldpopUp')).modal('hide');
                $scope.getData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });

    }


    $scope.SOListForSearch = [];
    $scope.getSalesOrderData = function () {
        $scope.SOListForSearch = [];
        $http({
            method: 'POST',
            url: $scope.path + "GetSOList",
            data: { column: $scope.searchBySO, value: $scope.searchSO, TemplateId: $scope.OrderCostingMasterTemplateId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                try {
                    response.data.DeliveryDate = new Date(response.data.DeliveryDate);
                } catch (e) {

                }
            }
            $scope.SOListForSearch = response.data;
        });
    }
    $scope.showSalesOrderTagPopUp = function () {

        angular.element(document.querySelector("#SalesOrderTagPopUp")).modal("show");
        $scope.getSalesOrderData();
    }

    $scope.SOTemplateList = [];
    $scope.GetSOListForTemplate = function () {
        $scope.SOTemplateList = [];

        $http({
            method: 'POST',
            url: $scope.path + "GetSOListForTemplate",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                try {
                    response.data.DeliveryDate = new Date(response.data.DeliveryDate);
                } catch (e) {

                }
            }
            $scope.SOTemplateList = response.data;
        });
    }
    $scope.SaveSalesOrder = function () {

        var saveList = ej.DataManager($scope.SOListForSearch).executeLocal(ej.Query().where("isChecked", "equal", true));

        $http({
            method: 'POST',
            url: $scope.path + "UpdateSOData",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId, SOList: saveList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GetSOListForTemplate();
            angular.element(document.querySelector("#SalesOrderTagPopUp")).modal("hide");

        });


    }

    $scope.DeleteSalesOrderData = {};
    $scope.DeleteSalesOrderConfirm = function (args) {
        $scope.DeleteSalesOrderData = args;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#confirmSODelete")).modal("show");

    }
    $scope.DeleteSalesOrder = function () {

        $http({
            method: 'POST',
            url: $scope.path + "DeleteSOData",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId, SOId: $scope.DeleteSalesOrderData.data.SalesOrder },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.GetSOListForTemplate();
        });
    }
    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }




    $scope.OrderCostingMasterTemplateId = null;
    $scope.CostingVersionMasterTemplateId = null;
    $scope.ModelMain = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        CustomerId: null,
        Customer: null,
        ProductMasterId: null,
        SpecifyTo: null,
        OrderSize: null,
        ProductionAvailableDays: null,
        MKTTargetPerHour: null,
        TargetSellingPrice: 0,
        PaymentDays: null,
        PackingType: null,
        EstNoOfPackingList: null,
        ExcessShipmentPer: null,
        FileName: null,
        ProductCategory: null,
        ProductSubCategory: null,
        CostingType: null,
        TargetCM: 0,
        TargetProfit: 0,
        IsPercentage: 'true',

        UOM: null,
        TargetOrSPT: 0,
        CriticalLevel: null,
        SPT: 0,
        NoOfWorkstation: 0,
        EfficiencyPercentage: 0,
        StandardWorkingHours: 0,
        WorkCenterTargetPerDay: 0

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelMain);
    $scope.OrderCostingDirectMaterial = {
        Id: null,
        CostingItemId: null,
        Consumption: 0,
        UOM: 0,
        Rate: 0,
        ValueLoss: 0,
        GrossConsumption: 0,
        GrossAmount: 0

    };
    $scope.VersionModelNew = {
        Id: null,
        OrderCostingMasterTemplateId: null,
        Version: 1,
        Description: null
    };
    $scope.VersionDetailModelNew = {
        Id: null,
        CostingComponentId: null,
        CostingVersionMasterTemplateId: null,
        Sequence: null,
        CostingValue: null,
        //Code: null,
        CostingComponent: null,
        BuyerTarget: 0.00,


    };

    $scope.SpecifyToList = [];
    $scope.PackingTypeList = [];

    cboService.getEnumCbo("enum/getSpecifyToEnumCbo", function (result) {
        $scope.SpecifyToList = result;
    });
    cboService.getEnumCbo("enum/getPackingTypeEnumCbo", function (result) {
        $scope.PackingTypeList = result;
    });
    $scope.ProductMasterList = [];
    cboService.getProductMasterCbo(function (result) {
        $scope.ProductMasterList = result.Rows;
    });

    $scope.IsCustomer = false;
    $scope.CheckeSpecifyTo = function (SpecifyTo) {
        $scope.IsCustomer = false;
        if ($scope.ModelNew.SpecifyTo == "Customer")
            $scope.IsCustomer = true;
    }



    $scope.Get = function (args) {

        $scope.IsShowEntryForm = true;
        $http({
            method: 'POST',
            url: $scope.path + "GetListItem",
            data: { Id: args.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelNew = response.data[0];


            if ($scope.ModelNew.SpecifyTo == 'Customer')
                $scope.IsCustomer = true;
            if ($scope.ModelNew.IsPercentage == true) {
                $scope.ModelNew.IsPercentage = 'true';
            }
            else {
                $scope.ModelNew.IsPercentage = 'false';
            }


            $scope.CheckeSpecifyTo();

            var str = $scope.ModelNew.FileName;
            if (!baseService.isUndefinedOrNull(str)) {
                var extention = str.substr(str.indexOf('.'));
                $scope.imageSrc = virtualPath.QuickCostingImagePath + '/' + $scope.ModelNew.Id + extention;

                $scope.filedata = $scope.ModelNew.FileName;
            }

            $scope.OrderCostingMasterTemplateId = $scope.ModelNew.Id;
            $scope.GetSOListForTemplate();

            $scope.getBuyerData();
            $scope.getLatestVersion();
            $scope.SumCostingValue();
            $scope.CalculateProfit();

            //$scope.AssignSegmentByeDirectMaterial();
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.picdata = null;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.ProductMasterId)) {
                throw "Plase select a product";
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.CostingType)) {
                throw "Costing type is missing for the product";
            }

            if ($scope.ModelNew.SpecifyTo == "Customer") {
                if (baseService.isUndefinedOrNull($scope.ModelNew.CustomerId)) {
                    throw "Customer is required";
                }
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.CurrencyId)) {
                throw "Currency is required";
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        picData.append("QuickCostingData", angular.toJson(data.QuickCostingData));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata, 'QuickCostingData': $scope.QuickCostingDetailList }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.Action = 'Update';
                        ShowResult(response.data.Message, 'success');
                        $scope.ModelNew.Id = response.data.data.Id;
                        $scope.OrderCostingMasterTemplateId = response.data.data.Id;
                        // ClearFields(response.data.Sequence);
                        //$scope.getData();
                        //$scope.getVersion();
                        $scope.getLatestVersion();
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
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
        $scope.IsShowEntryForm = false;

        ClearFields();

        $scope.buyerList = [];
        return true;
    };

    function ClearFields() {

        $scope.CostingVersionMasterTemplateId = null;
        $scope.Status = 0;
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelMain);
        $scope.ModelNew.Id = null;
        $scope.VersionModelNew = {};
        $scope.ModelNew.Active = true;
        $scope.VersionDetailModelNew = {};
        $scope.CostingDetailList = [];
        $scope.SelectedQuickCostingComponent = '';
        $scope.SOTemplateList = [];
        $scope.SummaryBySegmentList = [];
        $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

        $scope.tabQ = 1;

        var chartObj = $("#graphdivComparison").data("ejChart");
        chartObj.redraw();
        chartObj = $("#graphdivBuyerTarget").data("ejChart");
        chartObj.redraw();
        chartObj = $("#graphdivQuickCosting").data("ejChart");
        chartObj.redraw();
        chartObj = $("#graphdivOrderCosting").data("ejChart");
        chartObj.redraw();

        $scope.Segment = '';
        $scope.CostingComponentId = '';
        $scope.DirectMaterialList = [];
        $scope.OperationList = [];
        $scope.DirectProcessList = [];
        $scope.SalesExpenseList = [];
        $scope.ValueLossList = [];

        $scope.QuickCostingDetailList = [];
        $scope.VersionList = [];
        $scope.VersionModelNew.Version = 1;
        $scope.ModelNew.IsPercentage = 'true';
        $scope.picData = null;
        $scope.IsCustomer = false;
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);

        $scope.SumCostingValue();
        $scope.CalculateProfit();
    }

    //#region Customer info
    $scope.partySearchByList = [
        {
            'name': $scope.partyType + ' Code',
            'value': 'Code'
        },
        {
            'name': $scope.partyType + ' Name',
            'value': 'PartyName'
        },
        {
            'name': 'Account Group',
            'value': 'PartyAccountGroupName'
        },
        {
            'name': 'Country',
            'value': 'CountryName'
        },
        {
            'name': 'State',
            'value': 'StateName'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        }
    ];
    $scope.searchBy
    $scope.SOSearchByList = [
        {
            'name': 'Master Order',
            'value': 'MasterOrderId'
        },
        {
            'name': 'Prod. Name',
            'value': 'Product'
        },
        {
            'name': 'Sales Order',
            'value': 'SalesOrderId'
        },
        {
            'name': 'Customer',
            'value': 'PartyId'
        }
    ];
    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'PartyName, PartyAccountGroupName'
        , searchBy: 'PartyName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.partyList = [];
    $scope.SelectedMaterialRow = {};
    $scope.MaterialVendorPopUp = function (index) {
        $scope.SelectedMaterialRow = $scope.DirectMaterialList[index];
        $scope.showPartyPopUp('Vendor');
    }
    $scope.showPartyPopUp = function (ptype) {
        $scope.partyType = ptype;
        $scope.partyList = [];
        $scope.getPartyList = function (pageno) {

            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList/' + 'GetCompanyPartyDataList?companyId=' + $window.companyId + '&PlantId=' + $window.plantId + '&partyType=' + $scope.partyType;
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };

    $scope.selectPartyPopUpRow = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedCustomer = id;
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            if ($scope.partyType == 'Customer') {
                $scope.ModelNew.Customer = party.UserName;
                $scope.ModelNew.CustomerId = party.Id;
            }
            else {

                $scope.SelectedMaterialRow.Vendor = party.UserName;
                $scope.SelectedMaterialRow.VendorId = party.Id;

            }

            angular.element(document.querySelector('#partyPopUp')).modal('hide');

        }

    };

    //#endregion End customer info

    $scope.getLatestVersion = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "GetVersion?OrderCostingMasterTemplateId=" + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.onChengeProductMaster();

            });
        } catch (e) {
            ShowResult(e.Message, 'failure');
        }
    }








    $scope.CostingSubCategoryList = [];
    $scope.getCostingSubCategory = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "getCostingSubCategory",
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.CostingSubCategoryList = response.data;
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {

        }
    }




    $scope.SaveQuickCostingVersion = function () {
        //angular.element(document.querySelector("#confirmPostPopUp")).modal("hide");
        //if (baseService.isUndefinedOrNull($scope.VersionModelNew.Description)) {
        //    ShowResult('Description cannot empty', 'failure');
        //    return;
        //}
        $scope.VersionModelNew.OrderCostingMasterTemplateId = $scope.ModelNew.Id;
        $http({

            method: 'POST',
            url: 'Costings/OrderCosting/CreateCostingDetail',
            data: { "VersionModelNew": $scope.VersionModelNew, "data": $scope.QuickCostingDetailList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.getVersion();
                $scope.getData();
                $scope.getLatestVersion();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };



    $scope.QuickCostingDetailId = null;

    $scope.ShowCostingSubCategoryPopUp = function () {
        $scope.getCostingSubCategory();
        $("#CostingSubCategoryPoUp").ejDialog();
        var eDialog = $("#CostingSubCategoryPoUp").data("ejDialog");
        eDialog.open();

        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering

    };
    $scope.CloseCostingSubCategoryPopUp = function (args) {
        $scope.VersionDetailModelNew.CostingSubCategory = args.data.UserName;
        $scope.VersionDetailModelNew.Code = args.data.Code;
        $scope.VersionDetailModelNew.CostingSubCategoryId = args.data.Id;
        $("#CostingSubCategoryPoUp").ejDialog();
        var eDialog = $("#CostingSubCategoryPoUp").data("ejDialog");
        eDialog.close();
    };

    //endregion Version

    //delete costingDetail
    $scope.valuePassInActivityFormDelModal = function (index, data) {
        $scope.buyerMsterActivityId = data.Id;
        $scope.bActivityIndex = index;
        if (baseService.isUndefinedOrNull($scope.buyerMsterActivityId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.BuyerActivityName + ' ]';
        angular.element(document.querySelector('#confirmActivityPopUp')).modal('show');
    };

    $scope.DeleteActivitySavedItem = function () {
        if (baseService.isUndefinedOrNull($scope.buyerMsterActivityId)) {
            $scope.SelectedActivityList.splice($scope.bActivityIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Costings/OrderCosting/DeleteCostingDetail?id=' + $scope.buyerMsterActivityId
            }).then(function successCallback(response) {

            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure', 'buyerActivityPopUp');
            }).finally(function () {
            });
        }
    };


    $scope.confirmDeleteCostingDetail = function (index, obj) {
        $scope.costingDetailId = obj.Id;
        $scope.index = index;
        $scope.message_confirmation = "Do your want delete permanently?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");

    };




    $scope.confirmDetachPopUp = function (index, obj) {

        $scope.message_confirmation = "Do your want detach this file?";
        angular.element(document.querySelector("#confirfileDetachPopUp")).modal("show");

    };
    $scope.DetachFile = function () {

        $scope.ClearDocument();
        $scope.Save();

    }

    $scope.RemoveCostingDetail = function () {

        angular.element(document.querySelector("#confirmDeletePopUp")).modal("hide");

        if ($scope.QuickCostingDetailList.length > 0) {
            $scope.QuickCostingDetailList.splice($scope.index, 1);

        }
    }
    $scope.RemoveCostingDetailConfirmPopUp = function (index) {
        $scope.indexOfCostingDetail = index;

        $scope.message_confirmation = "Do your want delete?";
        angular.element(document.querySelector("#removeCostingDetailPopUp")).modal("show");

    };
    $scope.RemoveCostingDetailPermanently = function () {
        angular.element(document.querySelector("#removeCostingDetailPopUp")).modal("hide");
        if ($scope.QuickCostingDetailList.length > 0) {
            $scope.QuickCostingDetailList.splice($scope.indexOfCostingDetail, 1);

        }

    };
    $scope.ProductMasterDetail = function (ProductMasterId) {
        $http({
            method: 'GET',
            url: $scope.path + "ProductMasterDetail?ProductMasterId=" + ProductMasterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {


            $scope.ModelNew.ProductSubCategory = response.data[0].ProductSubCategory;
            $scope.ModelNew.CostingType = response.data[0].CostingType;
            $scope.ModelNew.ProductCategory = response.data[0].ProductCategory;

        });
    }
    $scope.GetCostingComponentByProductMasterId = function (ProductMasterId) {
        $http({
            method: 'GET',
            url: $scope.path + "GetCostingComponentByProductMasterId?ProductMasterId=" + ProductMasterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.QuickCostingDetailList = response.data;


        });
    }

    $scope.backColor = 'white';
    $scope.isRemoveFromCostingTypeComponent = false;

    $scope.QuickCostingDetailList = [];
    $scope.QuickCostingItemList = [];
    $scope.Status = 0;
    $scope.onChengeProductMaster = function () {

        $http({
            method: 'GET',
            url: $scope.path + "GetQuickCostingDetailByProductMaster?ProductMasterId=" + $scope.ModelNew.ProductMasterId + "&CostingVersionMasterTemplateId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.QuickCostingDetailList = response.data.ComponentList;
            $scope.QuickCostingItemList = response.data.ItemList;

            $scope.MakeSummaryBySegment();
        });
    }

    $scope.tabQ = 1;
    $scope.setTabQ = function (newTab) {
        $scope.tabQ = newTab;
        if (newTab == 3) {
            for (var i = 0; i < $scope.QuickCostingDetailList.length; i++) {
                if ($scope.QuickCostingDetailList[i].BuyerTarget > 0 ? $scope.QuickCostingDetailList[i].BuyerTarge : $scope.QuickCostingDetailList[i].BuyerTarget = 0);
                if ($scope.QuickCostingDetailList[i].CostingValue > 0 ? $scope.QuickCostingDetailList[i].CostingValue : $scope.QuickCostingDetailList[i].CostingValue = 0);
                if ($scope.QuickCostingDetailList[i].TotalGrossAmount > 0 ? $scope.QuickCostingDetailList[i].TotalGrossAmount : $scope.QuickCostingDetailList[i].TotalGrossAmount = 0);
            }


            var chartObj = $("#graphdivComparison").data("ejChart");
            chartObj.redraw();
            chartObj = $("#graphdivBuyerTarget").data("ejChart");
            chartObj.redraw();
            chartObj = $("#graphdivQuickCosting").data("ejChart");
            chartObj.redraw();
            chartObj = $("#graphdivPreCosting").data("ejChart");
            chartObj.redraw();
        }
    };
    $scope.isSetQ = function (tabNum) {
        return $scope.tabQ === tabNum;

    };

    $scope.tab1 = 1;
    $scope.setTab1 = function (newTab) {
        $scope.tab1 = newTab;

    };
    $scope.isSet1 = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    }

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };



    $scope.CostingItem = {
        Sequence: 0,
        Code: null,
        ShortName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        POIssueDeadLine: null,
        Active: true,
        CostingCategoryId: null,
        CostingComponentId: null,
        UnitOfMeasurementId: null,
        MinimumOfQuantity: 0,
        Wastage: null,
        BudgetMasterId: null,
        ProcessId: null,
        ActivityId: null,
        PurchaseGroupId: null,
        CostingGroupId: null,
        // CostingItemType: null,
    };
    $scope.OrderCostingDetail = {
        Id: null,
        Sequence: 0,
        CostingItemId: null,
        OrderCostingVersionMasterId: null,
        CostingValue: 0,
        BuyerTarget: 0
    };
    $scope.AddCostingItem = function () {

        $scope.CostingItemList.push($scope.CostingItem);
    };
    $scope.SaveCostingItemsIncludingComponent = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/SaveCostingItemsIncludingComponent',
            data: { costingItems: $scope.CostingItemList, OrderCostingDetail: $scope.OrderCostingDetail },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };



    // #region Costing Items PopUp
    $scope.costingItemParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.IssueGroups = [];
    $scope.GetIssueGroup = function (x) {
        $scope.issueGroupNew = x;
        $scope.issueTransactionNew.IssueGroupName = $scope.issueGroupNew.Name;
        $scope.issueTransactionNew.IssueGroupId = $scope.issueGroupNew.Id;
        $scope.hideCostingItemListPopUp();
    }


    // #endregion end Costing Items PopUp



    $scope.detailTemp = "#tabGridContents";


    $scope.CostingItemList = [];

    $scope.getCostingItemByComponentId = function () {
        $scope.data = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemList = response.data;


        });
    };

    $scope.showCostingItemListPopUp = function () {
        $scope.getCostingItemByComponentId();
        angular.element(document.querySelector("#costingItemListPopUp")).modal("show");
    };


    $scope.GetDirectCostingMeterialWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetDirectCostingMeterialWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DirectMaterialList = response.data;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.showPopUp = function (args) {
        $scope.CostingComponentId = args.data.CostingComponentId;
        $scope.TotalSegmentedCostValue = args.data.CostingValue;
        if ($scope.Segment == 'DirectMaterial') {

            $scope.showDirectMaterialWithItemPopUp();
        }
        else if ($scope.Segment == 'Operation') {


            $scope.showOperationPopUp();
        }
        else if ($scope.Segment == 'DirectProcess') {

            $scope.showDirectProcessPopUp();
        }
        else if ($scope.Segment == 'SalesExpense') {

            $scope.showSalesExpensePopUp();
        }
        else if ($scope.Segment == 'ValueLoss') {

            $scope.showValueLossPopUp();
        }
    };
    $scope.showDirectMaterialWithItemPopUp = function () {
        $scope.GetDirectCostingMeterialWithItemByComponentId();
        angular.element(document.querySelector("#DirectMaterialWithItemPopUp")).modal("show");
    }
    $scope.hideDirectMaterialWithItemPopUp = function () {
        angular.element(document.querySelector("#DirectMaterialWithItemPopUp")).modal("hide");
    };

    $scope.totalItemGrossAmount = 0;
    $scope.toatalItemGrossConsumption = 0;

    $scope.CalculateItemValueByPerComponent = function () {


        $scope.totalItemGrossAmount = 0;
        $scope.toatalItemGrossConsumption = 0;
        //if ($scope.DirectMaterialList.length > 0) {
        for (var i = 0; i < $scope.DirectMaterialList.length; i++) {
            $scope.totalItemGrossAmount += $scope.DirectMaterialList[i].GrossAmount;
            $scope.toatalItemGrossConsumption += $scope.DirectMaterialList[i].GrossConsumption;

        }
        //}

        for (var i = 0; i < $scope.QuickCostingDetailList.length; i++) {
            if ($scope.QuickCostingDetailList[i].CostingComponentId == $scope.CostingComponentId) {
                $scope.QuickCostingDetailList[i].TotalGrossAmount = $scope.totalItemGrossAmount;

            }
        }
    };
    $scope.SummaryBySegmentList = [];
    $scope.MakeSummaryBySegment = function () {
        $scope.SummaryBySegmentList = [];
        var DistinctSegments = ej.DataManager($scope.QuickCostingDetailList).executeLocal(ej.Query().group("CostingSegment"));
        for (var s = 0; s < DistinctSegments.length; s++) {
            var ItemsBySegments = DistinctSegments[s].items; //ej.DataManager($scope.QuickCostingDetailList).executeLocal(ej.Query().where("CostingSegment", "equal", DistinctSegments[0].items[s]));
            var BuyerTarget = 0, CostingValue = 0, TotalGrossAmount = 0;
            for (var i = 0; i < ItemsBySegments.length; i++) {
                BuyerTarget += ItemsBySegments[i].BuyerTarget;
                CostingValue += ItemsBySegments[i].CostingValue;
                TotalGrossAmount += ItemsBySegments[i].TotalGrossAmount;
            }

            var tempData = { Segment: DistinctSegments[s].key, BuyerTarget: BuyerTarget, CostingValue: CostingValue, TotalGrossAmount: TotalGrossAmount };
            $scope.SummaryBySegmentList.push(tempData);
        }

        $scope.SumCostingValue();
    }
    $scope.displayTextRendering = function (args) {
        var Total = 0;
        for (var i = 0; i < $scope.QuickCostingDetailList.length; i++) {
            Total += $scope.QuickCostingDetailList[i][args.data.series.yName];

        }


        args.data.text = (parseFloat(args.data.text) / Total * 100).toFixed(0) + '%';


    }
    $scope.CalculateFinalCosting = function (data) {

        //first try to push the data into main list
        try {
            for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {
                if ($scope.QuickCostingItemList[i].Id == data.CostingItemId) {
                    if ($scope.Segment == "SalesExpense" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.QuickCostingItemList[i].ValueType = data.Type;
                        $scope.QuickCostingItemList[i].Value = data.Value;
                    }
                    if ($scope.Segment == "ValueLoss" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.QuickCostingItemList[i].ValueType = data.Type;
                        $scope.QuickCostingItemList[i].Value = data.Value;
                    }
                    if ($scope.Segment == "Profit" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.QuickCostingItemList[i].ValueType = data.Type;
                        $scope.QuickCostingItemList[i].Value = data.Value;
                    }

                    if ($scope.QuickCostingItemList[i].CostingSegment == 'DirectMaterial') {
                        data.GrossConsumption = (data.Consumption * data.ValueLoss / 100) + data.Consumption;
                        data.GrossAmount = data.GrossConsumption * data.Rate;
                        $scope.QuickCostingItemList[i].TotalGrossAmount = data.GrossConsumption * data.Rate;


                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'Operation') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;

                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'DirectProcess') {
                        var totalPre = calValue("DirectMaterial");
                        totalPre += calValue("Operation");
                        $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100) + data.Rate;

                        $scope.QuickCostingItemList[i].Rate = data.Rate;
                        $scope.QuickCostingItemList[i].Value = data.Value;

                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'SalesExpense') {

                        if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {
                            var totalPre = calValue("DirectMaterial");
                            totalPre += calValue("Operation");
                            totalPre += calValue("DirectProcess");

                            $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'ValueLoss') {

                        if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {
                            var totalPre = calValue("DirectMaterial");
                            totalPre += calValue("Operation");
                            totalPre += calValue("DirectProcess");

                            $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'Profit') {

                        if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {
                            var totalPre = calValue("DirectMaterial");
                            totalPre += calValue("Operation");
                            totalPre += calValue("DirectProcess");
                            totalPre += calValue("SalesExpense");
                            totalPre += calValue("ValueLoss");

                            $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }

                }
            }
        } catch (e) {

        }


        try {
            for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {

                if ($scope.QuickCostingItemList[i].CostingSegment == 'DirectProcess') {
                    var totalPre = calValue("DirectMaterial");
                    totalPre += calValue("Operation");
                    $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100) + $scope.QuickCostingItemList[i].Rate;

                    // data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                }
                else if ($scope.QuickCostingItemList[i].CostingSegment == 'SalesExpense') {


                    if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = $scope.QuickCostingItemList[i].Value;
                    }
                    else {
                        var totalPre = calValue("DirectMaterial");
                        totalPre += calValue("Operation");
                        totalPre += calValue("DirectProcess");
                        $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);

                    }
                }
                else if ($scope.QuickCostingItemList[i].CostingSegment == 'ValueLoss') {

                    if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = $scope.QuickCostingItemList[i].Value;
                    }
                    else {
                        var totalPre = calValue("DirectMaterial");
                        totalPre += calValue("Operation");
                        totalPre += calValue("DirectProcess");

                        $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);

                    }
                }
                else if ($scope.QuickCostingItemList[i].CostingSegment == 'Profit') {

                    if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = $scope.QuickCostingItemList[i].Value;
                    }
                    else {
                        var totalPre = calValue("DirectMaterial");
                        totalPre += calValue("Operation");
                        totalPre += calValue("DirectProcess");
                        totalPre += calValue("SalesExpense");
                        totalPre += calValue("ValueLoss");

                        $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);

                    }
                }

                //}
            }
        } catch (e) {

        }



        try {
            $scope.totalItemGrossAmount = 0;
            $scope.toatalOperationValue = 0;
            $scope.totalDirectProcessAmount = 0;
            $scope.totalSalesExpenseAmount = 0;
            $scope.totalValueLossAmount = 0;
            $scope.TotalSegmentedValueByComponent = 0;

            $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

            for (var i = 0; i < $scope.QuickCostingDetailList.length; i++) {


                var TotalValue = 0;
                for (var k = 0; k < $scope.QuickCostingItemList.length; k++) {
                    if ($scope.QuickCostingDetailList[i].CostingComponentId == $scope.QuickCostingItemList[k].CostingComponentId) {
                        TotalValue += $scope.QuickCostingItemList[k].TotalGrossAmount;
                    }
                }
                $scope.QuickCostingDetailList[i].TotalGrossAmount = TotalValue;

                if ($scope.QuickCostingDetailList[i].CostingComponentId == $scope.CostingComponentId) {
                    $scope.TotalSegmentedValueByComponent = TotalValue;
                }


                //calculation
                if ($scope.QuickCostingDetailList[i].CostingSegment == 'Profit') {
                    $scope.CostingSummaryDataNew.ProfitBuyerCosting += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.ProfitQuickCosting += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.ProfitOrderCosting += $scope.QuickCostingDetailList[i].TotalGrossAmount;
                }
                else {
                    $scope.CostingSummaryDataNew.BuyerTotal += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.QuickCostingValue += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.OrderCostingValue += $scope.QuickCostingDetailList[i].TotalGrossAmount;

                }
            }
        } catch (e) {

        }



    }
    $scope.TotalSegmentedValueByComponent = 0;


    function calValue(segmentName) {
        var sum = 0;
        for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {
            if ($scope.QuickCostingItemList[i].CostingSegment == segmentName)
                sum += $scope.QuickCostingItemList[i].TotalGrossAmount;
        }
        return sum;
    }




    $scope.SaveOrderCostingDirectMaterial = function () {
        $scope.hideDirectMaterialWithItemPopUp();

        if ($scope.DirectMaterialList.length > 0) {
            for (var i = 0; i < $scope.DirectMaterialList.length; i++) {
                if ($scope.DirectMaterialList[i].IsGeneric == false) {
                    $scope.DirectMaterialList[i].MaterialMasterId = null;
                    $scope.DirectMaterialList[i].ArticleId = null;
                }
            }
        }

        //$scope.CalculateItemValueByPerComponent();

        //if ($scope.TotalSegmentedCostValue < $scope.totalItemGrossAmount) {
        //    ShowResult('Total Gross Amount cannot be greate than CostingValue', 'failure');
        //    //$scope.UpdateGridItems();
        //    return;
        //}



        $http({

            method: 'POST',
            url: 'Costings/OrderCosting/SaveOrderCostingDirectMaterial',
            data: { 'data': $scope.DirectMaterialList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.onChengeProductMaster();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    $scope.DirectMaterialList = [];
    var isExist = false;
    $scope.hideCostingItemListPopUp = function (x) {

        angular.element(document.querySelector("#costingItemListPopUp")).modal("hide");


        if ($scope.DirectMaterialList.length == 0 && x != undefined) {
            $scope.DirectMaterialList.push(x);
            return;
        }

        if ($scope.DirectMaterialList.length > 0) {
            for (var i = 0; i < $scope.DirectMaterialList.length; i++) {
                if (x.CostingItemId == $scope.DirectMaterialList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.DirectMaterialList.push(x);
            }
        }
    };




    // #region Pre Costing Operation
    $scope.OperationList = [];
    $scope.showOperationPopUp = function () {

        //$scope.CostingComponentId = args.data.CostingComponentId;
        $scope.GetOperationWithItemByComponentId();
        angular.element(document.querySelector("#OperationPopUp")).modal("show");

    };
    $scope.GetOperationWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetOperationWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OperationList = response.data;
            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }
    $scope.hideOperationPopUp = function () {
        angular.element(document.querySelector("#OperationPopUp")).modal("hide");
    };
    $scope.SaveOperation = function () {
        var flag = false;
        if ($scope.OperationList.length > 0) {
            //for (var i = 0; i < $scope.OperationList.length; i++) {
            //    if ($scope.OperationList[i].Value == 0 || $scope.OperationList[i].Value == null) {
            //        var flag = true;
            //        ShowResult('Value cannot be 0 or empty', 'failure');
            //        break;
            //    }
            //}

            //if ($scope.TotalSegmentedCostValue < $scope.toatalOperationValue) {
            //    ShowResult('Total value cannot be greate than CostingValue', 'failure');
            //    return;
            //}

            if (flag == false) {
                $scope.hideOperationPopUp();
                $http({
                    method: 'POST',
                    url: 'Costings/OrderCosting/SaveOperation',
                    data: { 'data': $scope.OperationList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.onChengeProductMaster();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        else {
            $scope.hideOperationPopUp();
        }
    };

    $scope.showCostingItemListWithOperationPopUp = function () {
        $scope.GetCostingItemWithOperationByComponentId();

        angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("show");
    };
    $scope.hideCostingItemListWithOperationPopUp = function () {
        angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("hide");
    }

    $scope.CostingItemWithOperationList = [];
    $scope.GetCostingItemWithOperationByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemWithOperationByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemWithOperationList = response.data;

        });
    };

    $scope.OperationList = [];
    var isExist = false;
    $scope.hideCostingItemListWithOerationPopUp = function (x) {

        angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("hide");


        if ($scope.OperationList.length == 0 && x != undefined) {
            $scope.OperationList.push(x);
            return;
        }

        if ($scope.OperationList.length > 0) {
            for (var i = 0; i < $scope.OperationList.length; i++) {
                if (x.CostingItemId == $scope.OperationList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.OperationList.push(x);
            }
        }
    };
    $scope.DeleteOperationPopUp = function (x) {
        $scope.operationId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteOperationPopUp")).modal("show");
    };



    $scope.directProcessExecutionTypeList = ['Inside', 'Outside'];
    $scope.DirectProcessList = [];
    $scope.showDirectProcessPopUp = function () {
        $scope.GetDirectProcessWithItemByComponentId();
        angular.element(document.querySelector("#DirectProcessPopUp")).modal("show");
    };
    $scope.GetDirectProcessWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetDirectProcessWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DirectProcessList = response.data;
            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }
    $scope.hideDirectProcessPopUp = function () {
        angular.element(document.querySelector("#DirectProcessPopUp")).modal("hide");
    };
    $scope.SaveDirectProcess = function () {
        var flag = false;
        if ($scope.DirectProcessList.length > 0) {

            if (flag == false) {
                $scope.hideDirectProcessPopUp();
                $http({
                    method: 'POST',
                    url: 'Costings/OrderCosting/SaveDirectProcess',
                    data: { 'data': $scope.DirectProcessList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.onChengeProductMaster();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        else {
            $scope.hideDirectProcessPopUp();
        }
    };

    $scope.showCostingItemListWithDirectProcessPopUp = function () {



        angular.element(document.querySelector("#costingItemListWithDirectProcessPopUp")).modal("show");
    };
    $scope.hideCostingItemListWithDirectProcessPopUp = function () {
        angular.element(document.querySelector("#costingItemListWithDirectProcessPopUp")).modal("hide");
    }

    $scope.CostingItemWithDirectProcessList = [];
    $scope.GetCostingItemWithDirectProcessByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemWithDirectProcessByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemWithDirectProcessList = response.data;

        });
    };

    var isExist = false;
    $scope.hideCostingItemListWithDirectProcessPopUp = function (x) {

        angular.element(document.querySelector("#costingItemListWithDirectProcessPopUp")).modal("hide");


        if ($scope.DirectProcessList.length == 0 && x != undefined) {
            $scope.DirectProcessList.push(x);
            return;
        }

        if ($scope.DirectProcessList.length > 0) {
            for (var i = 0; i < $scope.DirectProcessList.length; i++) {
                if (x.CostingItemId == $scope.DirectProcessList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.DirectProcessList.push(x);
            }
        }
    };
    $scope.DeleteDirectProcessPopUp = function (x) {
        $scope.directProcessId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteDirectProcessPopUp")).modal("show");
    };

    $scope.DeleteDirectProcess = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/DeleteDirectProcess',
            data: { 'directProcessId': $scope.directProcessId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');



                angular.element(document.querySelector("#DeleteDirectProcessPopUp")).modal("hide");

                $scope.GetDirectProcessWithItemByComponentId();
                $scope.GetCostingItemsWithoutFilterForDirectProcess();

                $scope.UpdateGridItems();
                $scope.RefreshComponenctGridItems();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    //$scope.data = [];
    $scope.GetCostingItemsWithoutFilterForDirectProcess = function () {

        $scope.data = [];
        try {
            $http({
                method: 'GET',

                url: $scope.path + 'GetCostingItemsWithoutFilterForDirectProcess?OrderCostingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.data = response.data;
                $scope.UpdateGridItems();
            });
        } catch (e) {

        }
    }

    // #endregion OrderCosting Direct Process

    // #region OrderCosting SalesExpense

    $scope.SalesExpenseList = [];
    $scope.showSalesExpensePopUp = function () {
        $scope.GetSalesExpenseWithItemByComponentId();
        angular.element(document.querySelector("#SalesExpensePopUp")).modal("show");
    };

    $scope.showCostingItemListWithSalesExpensePopUp = function () {
        $scope.GetCostingItemWithSalesExpenseByComponentId();

        angular.element(document.querySelector("#costingItemListWithSalesExpensePopUp")).modal("show");
    };
    $scope.hideCostingItemListWithSalesExpensePopUp = function () {
        angular.element(document.querySelector("#costingItemListWithSalesExpensePopUp")).modal("hide");
    }

    $scope.GetSalesExpenseWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetSalesExpenseWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SalesExpenseList = response.data;

            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }
    $scope.hideSalesExpensePopUp = function () {
        angular.element(document.querySelector("#SalesExpensePopUp")).modal("hide");
    };
    $scope.SaveSalesExpense = function () {
        try {
            var flag = false;
            if ($scope.SalesExpenseList.length > 0) {

                for (var i = 0; i < $scope.SalesExpenseList.length; i++) {
                    if ($scope.SalesExpenseList[i].Value > 0) {
                        if (angular.isUndefinedOrNull($scope.SalesExpenseList[i].Type))
                            throw 'Type is missing';
                    }
                }


                if (flag == false) {
                    $scope.hideSalesExpensePopUp();
                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/SaveSalesExpense',
                        data: { 'data': $scope.SalesExpenseList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.onChengeProductMaster();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
            else {
                $scope.hideSalesExpensePopUp();
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CostingItemWithSalesExpenseList = [];
    $scope.GetCostingItemWithSalesExpenseByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemWithSalesExpenseByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemWithSalesExpenseList = response.data;

        });
    };

    var isExist = false;
    $scope.hideCostingItemListWithSalesExpensePopUp = function (x) {

        angular.element(document.querySelector("#costingItemListWithSalesExpensePopUp")).modal("hide");


        if ($scope.SalesExpenseList.length == 0 && x != undefined) {
            $scope.SalesExpenseList.push(x);
            return;
        }

        if ($scope.SalesExpenseList.length > 0) {
            for (var i = 0; i < $scope.SalesExpenseList.length; i++) {
                if (x.CostingItemId == $scope.SalesExpenseList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.SalesExpenseList.push(x);
            }
        }
    };
    $scope.DeleteSalesExpensePopUp = function (x) {
        $scope.salesExpenseId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteSalesExpensePopUp")).modal("show");
    };

    $scope.DeleteSalesExpense = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/DeleteSalesExpense',
            data: { 'salesExpenseId': $scope.salesExpenseId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector("#DeleteSalesExpensePopUp")).modal("hide");

                $scope.GetSalesExpenseWithItemByComponentId();
                $scope.GetCostingItemsWithoutFilterForSalesExpense();

                $scope.UpdateGridItems();
                $scope.RefreshComponenctGridItems();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    $scope.GetCostingItemsWithoutFilterForSalesExpense = function () {

        $scope.data = [];
        try {
            $http({
                method: 'GET',

                url: $scope.path + 'GetCostingItemsWithoutFilterForSalesExpense?OrderCostingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.data = response.data;
                $scope.UpdateGridItems();
            });
        } catch (e) {

        }
    }
    // #endregion OrderCosting SalesExpense

    // #region ValueLoss
    $scope.ValueLossList = [];
    $scope.showValueLossPopUp = function () {
        $scope.GetValueLossWithItemByComponentId();
        angular.element(document.querySelector("#ValueLossPopUp")).modal("show");
    };

    $scope.showCostingItemListWithValueLossPopUp = function () {
        $scope.GetCostingItemWithValueLossByComponentId();

        angular.element(document.querySelector("#costingItemListWithValueLossPopUp")).modal("show");
    };
    $scope.hideCostingItemListWithValueLossPopUp = function () {
        angular.element(document.querySelector("#costingItemListWithValueLossPopUp")).modal("hide");
    }

    $scope.GetValueLossWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetValueLossWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ValueLossList = response.data;
            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.ProfitList = [];
    $scope.GetProfitWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetProfitWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProfitList = response.data;
            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.hideValueLossPopUp = function () {
        angular.element(document.querySelector("#ValueLossPopUp")).modal("hide");
    };
    $scope.SaveValueLoss = function () {
        try {
            var flag = false;
            if ($scope.ValueLossList.length > 0) {
                for (var i = 0; i < $scope.ValueLossList.length; i++) {
                    if ($scope.ValueLossList[i].Value > 0) {
                        if (angular.isUndefinedOrNull($scope.ValueLossList[i].Type))
                            throw 'Type is missing';
                    }
                }
                if (flag == false) {

                    $scope.hideValueLossPopUp();

                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/SaveValueLoss',
                        data: { 'data': $scope.ValueLossList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.onChengeProductMaster();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
            else {
                $scope.hideValueLossPopUp();
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.SaveProfit = function () {
        try {
            var flag = false;
            if ($scope.ProfitList.length > 0) {
                for (var i = 0; i < $scope.ProfitList.length; i++) {
                    if ($scope.ProfitList[i].Value > 0) {
                        if (angular.isUndefinedOrNull($scope.ProfitList[i].Type))
                            throw 'Type is missing';
                    }
                }
                if (flag == false) {


                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/SaveProfit',
                        data: { 'data': $scope.ProfitList, OrderCostingMasterTemplateId: $scope.ModelNew.Id },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.onChengeProductMaster();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.CostingItemWithValueLossList = [];
    $scope.GetCostingItemWithValueLossByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetCostingItemWithValueLossByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemWithValueLossList = response.data;

        });
    };

    var isExist = false;
    $scope.hideCostingItemListWithValueLossPopUp = function (x) {

        angular.element(document.querySelector("#costingItemListWithValueLossPopUp")).modal("hide");


        if ($scope.ValueLossList.length == 0 && x != undefined) {
            $scope.ValueLossList.push(x);
            return;
        }

        if ($scope.ValueLossList.length > 0) {
            for (var i = 0; i < $scope.ValueLossList.length; i++) {
                if (x.CostingItemId == $scope.ValueLossList[i].CostingItemId) {
                    isExist = true;
                    break;
                }
                else {
                    isExist = false;
                }
            }

            if (isExist == true) {
                ShowResult(x.UserName + ' already has been taken ', 'failure')
            }
            else {
                $scope.ValueLossList.push(x);
            }
        }
    };
    $scope.DeleteValueLossPopUp = function (x) {
        $scope.ValueLossId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteValueLossPopUp")).modal("show");
    };

    $scope.DeleteValueLoss = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/DeleteValueLoss',
            data: { 'ValueLossId': $scope.ValueLossId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector("#DeleteValueLossPopUp")).modal("hide");

                $scope.GetValueLossWithItemByComponentId();
                $scope.GetCostingItemsWithoutFilterForValueLoss();

                $scope.UpdateGridItems();
                $scope.RefreshComponenctGridItems();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    $scope.GetCostingItemsWithoutFilterForValueLoss = function () {

        $scope.data = [];
        try {
            $http({
                method: 'GET',

                url: $scope.path + 'GetCostingItemsWithoutFilterForValueLoss?OrderCostingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.data = response.data;
                $scope.UpdateGridItems();
            });
        } catch (e) {

        }
    }
    // #endregion 




    $scope.Segmentgrid = function detailGridData(arg) {
        try {
            $scope.tempComponenListBySegment = [];
            $scope.Segment = arg;
            if ($scope.QuickCostingDetailList.length > 0) {
                for (var i = 0; i < $scope.QuickCostingDetailList.length; i++) {
                    if ($scope.QuickCostingDetailList[i].CostingSegment == arg) {
                        $scope.tempComponenListBySegment.push($scope.QuickCostingDetailList[i]);
                    }
                }
            }

        } catch (e) {
            var k = e;
        }

        //$scope.CalculateTotalComponetValueForPerSegment();
        //$scope.loadDataForEdit();

    }
    //$scope.GetCostingItemsWithoutFilter();


    //$scope.Segment == 'DirectMaterial';

    $scope.RefreshComponenctGridItems = function () {
        var _totalGrossAmount = 0;
        if ($scope.Segment == 'DirectMaterial') {

            _totalGrossAmount = $scope.totalItemGrossAmount;
        }
        else if ($scope.Segment == 'Operation') {

            _totalGrossAmount = $scope.toatalOperationValue;
        }
        else if ($scope.Segment == 'DirectProcess') {

            _totalGrossAmount = $scope.totalDirectProcessAmount;

        }
        else if ($scope.Segment == 'SalesExpense') {

            _totalGrossAmount = $scope.totalSalesExpenseAmount;

        }
        else if ($scope.Segment == 'ValueLoss') {

            _totalGrossAmount = $scope.totalValueLossAmount;

        }


        for (var i = 0; i < $scope.tempComponenListBySegment.length; i++) {
            if ($scope.tempComponenListBySegment[i].CostingComponentId == $scope.CostingComponentId)
                $scope.tempComponenListBySegment[i].TotalGrossAmount = _totalGrossAmount;
        }
        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();


    };
    $scope.GridOperationSummaryRows = [{
        title: "Total Amount", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CostingValue", dataMember: "CostingValue", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BuyerTarget", dataMember: "BuyerTarget", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalGrossAmount", dataMember: "TotalGrossAmount", format: "{0:N2}" }],
        showCaptionSummary: true

    }];
    $scope.GridSummaryBySegmentSummaryRows = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "BuyerTarget", dataMember: "BuyerTarget", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "CostingValue", dataMember: "CostingValue", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "TotalGrossAmount", dataMember: "TotalGrossAmount", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

    $scope.selectedComponentRowDetail = null;
    $scope.detailgrid = function detailGridData(e) {
        $scope.selectedComponentRowDetail = e;
        $scope.CostingComponentId = e.data["CostingComponentId"];

        if ($scope.Segment == 'DirectMaterial') {
            $scope.GetCostingItemsWithoutFilter();
        }
        else if ($scope.Segment == 'Operation') {
            $scope.GetCostingItemsWithoutFilterForOperation();
        }
        else if ($scope.Segment == 'DirectProcess') {
            $scope.GetCostingItemsWithoutFilterForDirectProcess();
        }
        else if ($scope.Segment == 'SalesExpense') {
            $scope.GetCostingItemsWithoutFilterForSalesExpense();
        }
        else if ($scope.Segment == 'ValueLoss') {
            $scope.GetCostingItemsWithoutFilterForValueLoss();
        }


        // $scope.getCostingItemByComponentId();
        //$scope.data = ej.DataManager($scope.CostingItemListWithoutFilter).executeLocal(ej.Query().where("CostingComponentId", "equal", parseInt($scope.CostingComponentId), true).take(100));


        //e.detailsElement.find(".tabcontrol").ejTab();
    }

    $scope.expanded = [];
    $scope.flag = true;
    $scope.expand = function (args) {
        if ($scope.flag) {
            $scope.expanded.push(args.masterRow[0].rowIndex);
            $scope.flag = true;
        }
    }


    $scope.tempComponenListBySegment = [];
    $scope.AssignSegmentByeDirectMaterial = function () {
        $scope.tempComponenListBySegment = [];

        var kk = 0;
        try {
            for (var i = 0; baseService.arrayLength($scope.QuickCostingDetailList); i++) {
                kk = i;
                if ($scope.QuickCostingDetailList[i].CostingSegment == 'DirectMaterial') {
                    $scope.tempComponenListBySegment.push($scope.QuickCostingDetailList[i]);
                }
            }

        } catch (e) {

        }

        //var obj = $("#GridOperation").ejGrid("instance");
        ////obj.model.childGrid.dataSource = data1;  //updating the dataSource of childgrid 
        //$("#GridOperation").ejGrid("dataSource", $scope.tempComponenListBySegment); //updating the dataSource of parent grid 
        //// $scope.loadDataForEdit();
        //var gridObj = $("#GridOperation").data("ejGrid");
        //gridObj.refreshContent(true);
        //gridObj.refreshTemplate();

    }




    //#endregion

    //#region Buyer Information
    $scope.CostingBuyer = {
        Id: null,
        OrderCostingMasterTemplateId: null,
        BuyerId: null,
        BuyerStyleRefNo: null,
        OwnStyleRefNo: null
    }
    $scope.costingBuyerNew = Object.assign({}, $scope.CostingBuyer);

    $scope.AddNewBuyer = function () {
        $scope.CostingBuyer = {};
        $scope.costingBuyerNew = {};
        angular.element(document.querySelector('#BuyerPoUp')).modal('show');
    }
    $scope.buyerList = [];
    $scope.getBuyerData = function () {
        $scope.buyerList = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetBuyerDataByCostingMasterId?costingMasterId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.buyerList = response.data;
        });
    }

    $scope.EditBuyer = function (obj) {
        var gridObj = $("#GridBuyer").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.costingBuyerNew = data;
        $scope.BuyerAction = 'Update';
        angular.element(document.querySelector('#BuyerPoUp')).modal('show');
    }
    $scope.removeBuyer = function (obj) {
        var gridObj = $("#GridBuyer").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.costingBuyerNew = data;
        if (!baseService.isUndefinedOrNull($scope.costingBuyerNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [' + $scope.costingBuyerNew.Buyer + ' ]';
        angular.element(document.querySelector('#confirmBuyerPopUp')).modal('show');
    }
    $scope.DeleteBuyer = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/DeleteCostingBuyer?id=' + $scope.costingBuyerNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.buyerList = [];
                $scope.getBuyerData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };
    $scope.buyerCboList = [];
    cboService.getCboBuyer(function (response) {
        $scope.buyerCboList = response;
    });

    $scope.bulletinTemplate = {
        Id: null,
        CompanyGroupId: null,
        BulletinName: null,
        AlternativeName: null,
        ByWhom: null,
        ProductMasterId: null,
        SizeGroupId: null
    };
    $scope.bulletinTemplateNew = Object.assign({}, $scope.bulletinTemplate);
    $scope.BuyerAction = 'Save'
    $scope.SaveBuyer = function () {
        angular.copy($scope.costingBuyerNew, $scope.CostingBuyer);
        $scope.CostingBuyer.OrderCostingMasterTemplateId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.BuyerForm.$valid) {
                if ($scope.BuyerAction === 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/CreateCostingBuyer',
                        data: { data: $scope.CostingBuyer },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'BuyerPoUp');
                            $scope.getBuyerData();
                            angular.element(document.querySelector('#BuyerPoUp')).modal('hide');
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                    };
                }
                else if ($scope.BuyerAction === 'Update') {
                    $http({
                        method: 'POST',
                        url: 'Costings/OrderCosting/CreateCostingBuyer',
                        data: $scope.CostingBuyer,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'BuyerPoUp');
                            $scope.getBuyerData();
                            angular.element(document.querySelector('#BuyerPoUp')).modal('hide');
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'BuyerPoUp');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'BuyerPoUp');
        }
    };
    $scope.CloseBuyer = function () {
        angular.element(document.querySelector('#BuyerPoUp')).modal('hide');
    }

    //#endregion 

    //#region Profit Calculation
    $scope.GrossProfit = 0;
    $scope.MKTGainOrLoss = 0;
    $scope.NetProfit = 0;
    ``
    $scope.CalculateProfit = function () {

        if ($scope.ModelNew.IsPercentage != null && $scope.ModelNew.IsPercentage == 'true') {
            //Percentage
            if ($scope.ModelNew.TargetProfit != NaN)
                $scope.GrossProfit = $scope.ModelNew.TargetSellingPrice - ($scope.ModelNew.TargetSellingPrice * (100 / (100 + $scope.ModelNew.TargetProfit)));
        }
        else {

            //Fixed
            if ($scope.ModelNew.TargetSellingPrice != NaN && $scope.ModelNew.TargetProfit != NaN)
                $scope.GrossProfit = $scope.ModelNew.TargetProfit;
        }
        $scope.SumNetProfitOfGross();
    }
    //#endregion end Profit Calculation

    $scope.CalculateGrossConsumption_Amount = function (x) {
        var GrossConsumption = (x.GrossConsumption * x.Consumption / 100) + x.GrossConsumption;
        x.GrossConsumption = GrossConsumption;

        x.GrossAmount = x.GrossConsumption * x.Rate;

    }

    //delete DirectMaterial
    $scope.DeleteOrderCostingDirectMaterialPopUp = function (x) {
        $scope.DirectMaterialId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeleteOrderCostingDirectMaterialPopUp")).modal("show");
    };

    $scope.DeleteDirectMaterial = function () {
        $http({
            method: 'POST',
            url: 'Costings/OrderCosting/DeleteDirectMaterial',
            data: { 'DirectMaterialId': $scope.DirectMaterialId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector("#DeleteOrderCostingDirectMaterialPopUp")).modal("hide");
                //$scope.DirectMaterialList = [];
                $scope.GetDirectCostingMeterialWithItemByComponentId();
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    // #region Costing formula 
    $scope.TotalDirectMaterial = 0;
    $scope.TotalOperation = 0;
    $scope.TotalProcess = 0;



    // #endregion end Costing formula 
    // #region Responsible Person
    $scope.employee = [];
    $scope.getPopUpData = function (index) {
        $scope.index = index;
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.setEmpData = function (obj) {


        var data = obj.data;
        if ($scope.Segment == 'DirectMaterial') {
            $scope.DirectMaterialList[$scope.index].ResponsiblePersoinId = data.SystemID;
            $scope.DirectMaterialList[$scope.index].ResponsiblePersoin = data.EmployeeName;
        }
        else if ($scope.Segment == 'Operation') {

            $scope.OperationList[$scope.index].ResponsiblePersoinId = data.SystemID;
            $scope.OperationList[$scope.index].ResponsiblePersoin = data.EmployeeName;
        }
        else if ($scope.Segment == 'DirectProcess') {

            $scope.DirectProcessList[$scope.index].ResponsiblePersoinId = data.SystemID;
            $scope.DirectProcessList[$scope.index].ResponsiblePersoin = data.EmployeeName;
        }
        else if ($scope.Segment == 'SalesExpense') {

            $scope.SalesExpenseList[$scope.index].ResponsiblePersoinId = data.SystemID;
            $scope.SalesExpenseList[$scope.index].ResponsiblePersoin = data.EmployeeName;
        }
        else if ($scope.Segment == 'ValueLoss') {

            $scope.ValueLossList[$scope.index].ResponsiblePersoinId = data.SystemID;
            $scope.ValueLossList[$scope.index].ResponsiblePersoin = data.EmployeeName;
        }
        else if ($scope.Segment == 'Profit') {

            $scope.ProfitList[$scope.index].ResponsiblePersoinId = data.SystemID;
            $scope.ProfitList[$scope.index].ResponsiblePersoin = data.EmployeeName;
        }

        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');

    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };
    // #region Master Order 
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['BOM'];
    $scope.getMaterial = function (index) {
        $scope.itemIndex = index;
        $scope.getMaterialMasterbyTypePopUp();
    };
    $scope.selectMaterialByType = function (ob) {
        $scope.DirectMaterialList[$scope.itemIndex].MaterialMasterId = ob.Id;
        $scope.DirectMaterialList[$scope.itemIndex].MaterialMasterName = ob.UserName;
        $scope.DirectMaterialList[$scope.itemIndex].ArticleId = null;
        $scope.DirectMaterialList[$scope.itemIndex].ArticleName = null;
        $scope.DirectMaterialList[$scope.itemIndex].InquiryItemId = null;
        $scope.DirectMaterialList[$scope.itemIndex].SampleItemId = null;
        $scope.DirectMaterialList[$scope.itemIndex].HasAttribute = ob.HasAttribute;
        $scope.mmChangeFlag = true;
        if ($scope.DirectMaterialList[$scope.itemIndex].HasAttribute) {
            $scope.getArticleSearchList(ob.Id);
        } else {
            $scope.closeMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }
        // getTaxCategoryList(ob.HSNCodeId);
        $scope.HSNCodeId = ob.HSNCodeId;
        $scope.closeMaterialMasterbyTypePopUp();
    };



    $scope.selectarticle = function (ob) {
        try {
            $scope.DirectMaterialList[$scope.itemIndex].MaterialMasterId = ob.MaterialMasterId;
            $scope.DirectMaterialList[$scope.itemIndex].MaterialMasterName = ob.MaterialMasterName;
            $scope.DirectMaterialList[$scope.itemIndex].ArticleId = ob.Id;
            $scope.DirectMaterialList[$scope.itemIndex].ArticleName = ob.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
            $scope.itemIndex = -1;
            $scope.mmChangeFlag = true;
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.getArticle = function (index) {
        $scope.itemIndex = index;
        if (!baseService.isUndefinedOrNull($scope.DirectMaterialList[$scope.itemIndex].MaterialMasterId) && !$scope.DirectMaterialList[$scope.itemIndex].HasAttribute)
            return ShowResult('This material has no attribute', 'failure');
        $scope.getArticleSearchList($scope.DirectMaterialList[$scope.itemIndex].MaterialMasterId);
    };

    $scope.totalCost = 0;
    $scope.SumCostingValue = function () {
        $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);
        $scope.totalCost = 0;
        if ($scope.QuickCostingDetailList.length > 0) {
            for (let i = 0; i < $scope.QuickCostingDetailList.length; i++) {

                if (!isNaN($scope.QuickCostingDetailList[i].CostingValue)) {
                    $scope.totalCost += $scope.QuickCostingDetailList[i].CostingValue;
                }


                //calculation
                if ($scope.QuickCostingDetailList[i].CostingSegment == 'Profit') {
                    $scope.CostingSummaryDataNew.ProfitBuyerCosting += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.ProfitQuickCosting += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.ProfitOrderCosting += $scope.QuickCostingDetailList[i].TotalGrossAmount;
                }
                else {
                    $scope.CostingSummaryDataNew.BuyerTotal += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.QuickCostingValue += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.OrderCostingValue += $scope.QuickCostingDetailList[i].TotalGrossAmount;

                }
            }


        }
        $scope.SumNetProfitOfSelling();
    }

    $scope.netProfit = 0;
    $scope.gainOrLos = 0;
    $scope.SumNetProfitOfSelling = function () {
        $scope.netProfit = 0;
        $scope.gainOrLos = 0;
        $scope.netProfit = $scope.ModelNew.TargetSellingPrice - $scope.totalCost;
        $scope.gainOrLos = $scope.netProfit;
        $scope.SumNetProfitOfGross();
    }
    $scope.NetProfitofGross = 0;
    $scope.SumNetProfitOfGross = function () {
        $scope.NetProfitofGross = $scope.GrossProfit - $scope.gainOrLos;
    }

    $scope.totalItemVale = 0;
    $scope.SumTotalItemsByComponetForPerSegment = function () {
        if ($scope.data.length > 0) {
            for (var i = 0; i < $scope.data.length; i++) {

            }
        }
    }

    $scope.totalComponentOfDirectMaterial = 0;
    $scope.totalComponentOfOperation = 0;
    $scope.totalComponentOfDirectProcess = 0;
    $scope.totalComponentOfSalesExpense = 0;
    $scope.totalComponentOfValueLoss = 0;


    $scope.backColor = 'red';
    $scope.fontColor = 'white';

    $scope.row = null;
    $scope.taskcolorchange = function (args) {
        //$scope.row = null;
        //$scope.row = args;
        //if (args.data.CostingValue < args.data.TotalGrossAmount) {
        //    $scope.row.cell.bgColor = "green";
        //}
        //else {
        //    $scope.row.cell.bgColor = "red";
        //}

    }
    // #endregion Costing Reports
    $scope.SelectedQuickCostingComponent = {};
    $scope.BackToQuickCostingComponent = function () {
        $scope.DirectMaterialList = [];
        $scope.OperationList = [];
        $scope.DirectProcessList = [];
        $scope.SalesExpenseList = [];
        $scope.ValueLossList = [];
        $scope.Segment = '';



        var elmnt = document.getElementById("costingMain");
        elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
    }
    $scope.NavigateToOrderCosting = function (args) {
        $scope.DirectMaterialList = [];
        $scope.toatalItemGrossConsumption = 0;
        $scope.totalItemGrossAmount = 0;

        $scope.OperationList = [];
        $scope.toatalOperationValue = 0;

        $scope.DirectProcessList = [];
        $scope.totalDirectProcessAmount = 0;

        $scope.SalesExpenseList = [];
        $scope.totalSalesExpenseAmount = 0;

        $scope.ValueLossList = [];
        $scope.totalValueLossAmount = 0;

        $scope.ProfitList = [];

        $scope.SelectedQuickCostingComponent = args;
        $scope.CostingComponentId = args.CostingComponentId;
        $scope.Segment = args.CostingSegment;
        if ($scope.Segment == 'DirectMaterial') {

            $scope.GetDirectCostingMeterialWithItemByComponentId();
        }
        else if ($scope.Segment == 'Operation') {


            $scope.GetOperationWithItemByComponentId();
        }
        else if ($scope.Segment == 'DirectProcess') {

            $scope.GetDirectProcessWithItemByComponentId();
        }
        else if ($scope.Segment == 'SalesExpense') {

            $scope.GetSalesExpenseWithItemByComponentId();
        }
        else if ($scope.Segment == 'ValueLoss') {

            $scope.GetValueLossWithItemByComponentId();
        }
        else if ($scope.Segment == 'Profit') {

            $scope.GetProfitWithItemByComponentId();
        }
        $scope.CalculateFinalCosting(null);






    }
    $scope.SaveCostingComponentItems = function () {

        if ($scope.Segment == 'DirectMaterial') {

            $scope.SaveOrderCostingDirectMaterial();
        }
        else if ($scope.Segment == 'Operation') {


            $scope.SaveOperation();
        }
        else if ($scope.Segment == 'DirectProcess') {

            $scope.SaveDirectProcess();
        }
        else if ($scope.Segment == 'SalesExpense') {

            $scope.SaveSalesExpense();
        }
        else if ($scope.Segment == 'ValueLoss') {

            $scope.SaveValueLoss();
        }
        else if ($scope.Segment == 'Profit') {

            $scope.SaveProfit();
        }
    }
}