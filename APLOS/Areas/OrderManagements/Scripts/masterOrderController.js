'use strict';
masterOrderController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', 'toaster'];
function masterOrderController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, toaster) {
    $rootScope.title = "Master Order";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.files = [];
    $scope.orderCategoryList = [];
    $scope.orderStatusList = [];
    $scope.searchMasterFilterList = [];
    $scope.itemList = [];
    $scope.personCboList = [];
    $scope.attributeList = [];
    $scope.personList = [];

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.path = 'OrderManagements/masterorder/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListResponsible';
    $scope.ItemListUrl = $scope.path + 'GetMasterItemList';
    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $controller("MasterOrderTaskTemplateController", { cboService: cboService, $scope: $scope, $http: $http });
    $controller("TaskScheduleController", { cboService: cboService, $scope: $scope, $http: $http });
    $controller("CurrencyExchangeController", { cboService: cboService, $scope: $scope, $http: $http, TableName: 'MasterOrderExchangeRates' });

    $scope.SearchColumn = 'MasterOrderNo';
    $scope.SearchValue = null;

    $scope.modelFilterByList = [
        { 'name': 'Creation Date', 'value': 'AddedDate' },
        { 'name': 'Created By', 'value': 'AddedBy' },
        { 'name': 'Order Type', 'value': 'OrderType' },
        { 'name': 'Plant', 'value': 'UserName' },
        { 'name': 'Entity', 'value': 'Entity' },
        { 'name': 'Customer Name', 'value': 'CustomerName' },
        { 'name': 'Buyer', 'value': 'Buyer' },
        { 'name': 'Master Order No', 'value': 'MasterOrderNo' },
        { 'name': 'Order Category', 'value': 'OrderCategory' },
        { 'name': 'Order Year', 'value': 'OrderYear' },
        { 'name': 'Total Qty', 'value': 'TotalQty' },
        { 'name': 'Line Item No', 'value': 'NoOfLineItem' },
        { 'name': 'Responsible Person', 'value': 'ResponsiblePersonName' },
        { 'name': 'Bill To', 'value': 'InvoicingPartyPlant' },
        { 'name': 'Ship To', 'value': 'DeliveryPartyPlant' },
        { 'name': 'Buyer Ref. No-Item', 'value': 'BuyerReferenceNoItem' },
        { 'name': 'Own Item', 'value': 'OwnItem' },
        { 'name': 'Buyer Orde/Ref No', 'value': 'BuyerReferenceNo' },
        { 'name': 'Own Order/Ref No', 'value': 'OwnReferenceNo' }
    ];
    $scope.files = [];
    $scope.getData = function () {
        $scope.files = [];
        if (!baseService.isUndefinedOrNull($scope.fileNew.CompanyId)) {
            $http({
                method: 'POST',
                data: {
                    'companyId': $scope.fileNew.CompanyId, 'column': $scope.SearchColumn, 'value': $scope.SearchValue
                },
                url: $scope.getListUrl
            }).then(function successCallback(response) {
                $scope.files = response.data;
            });
        }
    };

    $scope.IsBillDiscountingDays = false;

    $scope.file = {
        Id: null
        , CompanyId: null
        , PlantId: null
        , EntityId: null
        , CommitmentId: null
        , InquiryId: null
        , PartyId: null
        , BuyerId: null
        , BuyerBrandId: null
        , BuyerDivisionId: null
        , BuyerDepartmentId: null
        , TestingStandardId: null
        , MasterOrderNo: null
        , OrderStatusId: null
        , OrderCategoryId: null
        , SeasonId: null
        , OrderYear: null
        , CurrencyId: null
        , OrderType: 'ExternalOrder'
        , TotalQty: null
        , NoOfLineItem: null
        , ResponsiblePersonId: null
        , ResponsiblePersonName: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , OrderWastagePercentage: null
        , ExtraOrderPercentage: null
        , IsExtraOrderPercentage: false
        , TotalQtyUOMId: null
        , IsReplacement: false
        , Type: null
        , SpecialTaxId: null
        , BuyerReferenceNo: null
        , OwnReferenceNo: null
        , PaymentTermId: null
        , OwnReferenceNo: null
        , IsPaymentTermChangeable: null
        , ExceptionalProcessId: null
        , ExceptionalSubProcessId: null
        , DefaultPaymentTermId: null
        , IsPaymentTermChangeable: false
        , AddedDate: $filter("dateFiltering")(Date.now())
    };
    $scope.fileNew = Object.assign({}, $scope.file);
    $scope.isBuyerApplicable = false;

    // #region Ddl
    $scope.typeList = [
        { Value: "Manufacture", Text: "Manufacture" },
        { Value: "Trading", Text: "Trading" },
        { Value: "JobWork", Text: "Job Work" },
        { Value: "OutSource", Text: "Out Source" },
        { Value: "Other", Text: "Other" }
    ];

    $scope.searchRCBy = "Process"; $scope.searchRC = "";
    $scope.searchByRCList = [{ value: 'Id', name: "Id" }, { value: 'Process', name: "Process" }];


    $scope.RemarksControlmodel = { Id: null, MasterOrderId: null, RemarkControlId: null, RemarksControl: null, UserRemarks: null };
    $scope.RemarksControlList = [];
    $scope.GetRemarksControlList = function () {
        $http({
            method: 'POST',
            url: "Setups/RemarksControl/GetList",
            data: { column: $scope.searchRCBy, value: $scope.searchRC },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RemarksControlList = response.data;
            angular.element(document.querySelector('#RemarksControlPopUp')).modal('show');
        });
    }
    $scope.SelectRemarksControl = function (data) {
        $scope.RemarksControlmodel.MasterOrderId = $scope.fileNew.Id;
        $scope.RemarksControlmodel.RemarkControlId = data.data.Id;
        $scope.RemarksControlmodel.RemarksControl = data.data.Process;
        $scope.RemarksControlmodel.UserRemarks = data.data.UserRemarks;
        angular.element(document.querySelector('#RemarksControlPopUp')).modal('hide');
    }


    $scope.ProductLibraryList = [];
    $scope.GetProductLibraryList = function (index) {
        $scope.itemIndex = index;
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetProductLibrary?ArticleId=' + $scope.itemList[$scope.itemIndex].ArticleId
        }).then(function successCallback(response) {
            $scope.ProductLibraryList = response.data;
            angular.element(document.querySelector('#ProductLibraryPopUp')).modal('show');
        });
    };

    $scope.SetProductLibrary = function (obj) {
        $scope.itemList[$scope.itemIndex].ProductLibraryId = obj.data.Id;
        $scope.itemList[$scope.itemIndex].ProductLibrary = obj.data.UserName;
        angular.element(document.querySelector('#ProductLibraryPopUp')).modal('hide');
    }

    $scope.clearProductLibrary = function (index) {
        $scope.itemIndex = index;
        $scope.itemList[$scope.itemIndex].ProductLibraryId = null;
        $scope.itemList[$scope.itemIndex].ProductLibrary = null;
    }


    $scope.closeProductLibraryPopUp = function () {
        angular.element(document.querySelector('#ProductLibraryPopUp')).modal('hide');
    }

    $scope.ProductMasterUoMList = [];
    $scope.GetUoMCboByProductMaster = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetUoMCboByProductMaster/'
        }).then(function successCallback(response) {
            $scope.ProductMasterUoMList = response.data;
        });
    };
    $scope.GetUoMCboByProductMaster();

    $scope.yearList = [];
    $scope.getYearOfHaving = function () {
        $scope.yearList = [];
        var endYear = new Date();
        var ey = parseInt(endYear.getFullYear());
        for (var i = ey; i <= 2099; i++) {
            var ob = {
                Value: i,
                Text: i
            };
            $scope.yearList.push(ob);
        }

        var d = new Date();
        var n = d.getFullYear();
        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === n) {
                $scope.fileNew.OrderYear = $scope.yearList[i].Text;
                break;
            }
        }

    };
    $scope.getYearOfHaving();

    $scope.weekNoList = [];
    $scope.WeekNo = function () {
        $scope.weekNoList = [];
        for (var i = 1; i <= 54; i++) {
            var ob = {
                Value: i,
                Text: i
            };
            $scope.weekNoList.push(ob);
        }
    };
    $scope.WeekNo();

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (response) {
        $scope.companyList = response;
    });

    $scope.plantList = [];
    $scope.getPlantCbo = function () {
        cboService.getCboPlantByCompany($scope.fileNew.CompanyId, function (response) {
            $scope.plantList = response;
        });
    };

    $scope.specialTaxList = [];
    $scope.getSpecialTaxByPlantCbo = function () {
        cboService.getCboSpecialTaxByPlant($scope.fileNew.PlantId, function (response) {
            $scope.specialTaxList = response;
        });
    };

    $scope.buyerList = [];
    cboService.getCboBuyer(function (data) {
        $scope.buyerList = data;
    });
    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.ProcessList = [];
    $scope.SubProcessList = [];
    $scope.GetProcessByCompany = function () {
        cboService.getCompanyProductionProcessCbo($scope.fileNew.CompanyId, function (response) {
            $scope.ProcessList = response;
        });
    }

    $scope.GetSubProcessByProcess = function () {
        cboService.loadSubprocessCbo($scope.fileNew.ExceptionalProcessId, function (response) {
            $scope.SubProcessList = response;
        });
    }

    $scope.departmentList = [];
    $scope.buyerChange = function () {
        $http.get("Parties/BuyerBrand/GetCbo?buyerId=" + $scope.fileNew.BuyerId)
            .then(function (response) {
                $scope.brandList = response.data;
            });
        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
            $scope.divisionList = result;
            if ($scope.divisionList.length == 1) {
                $scope.fileNew.BuyerDivisionId = $scope.divisionList[0].Value;
            }

        });
        cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
            $scope.departmentList = result;
            if ($scope.departmentList.length == 1) {
                $scope.fileNew.BuyerDepartmentId = $scope.departmentList[0].Value;
            }

        });
    };

    cboService.getCboWithBuyer(null, function (result) {
        $scope.testingStandardList = result;
    });

    $scope.entityList = [];
    $scope.getEntityCbo = function () {
        cboService.getCboProductionEntitiesByPlant($scope.fileNew.PlantId, function (result) {
            $scope.entityList = result;
        });
    };



    $scope.getPlantConfigByPlant = function () {
        $scope.isBuyerApplicable = false;
        $scope.fileNew.BuyerId = null;
        $scope.fileNew.BuyerDivisionId = null;
        $scope.fileNew.BuyerBrandId = null;
        $scope.fileNew.TestingStandardId = null;
        $http({
            method: 'GET',
            url: 'Setups/plantconfig/GetPlantConfigDataByPlantId?plantid=' + $window.plantId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0)
                $scope.isBuyerApplicable = response.data[0].BuyerApplicable;
        });
    };
    $scope.getPlantConfigByPlant();

    $scope.irregularList = [];
    $http.get("OrderManagements/MasterOrder/GetSpecialTaxList?plantId=" + $window.plantId)
        .then(function (response) {
            $scope.irregularList = response.data;
        });

    $scope.taskList = [];
    $scope.GEEMasterOrderId = '';
    $scope.GVMasterOrderId = '';
    $scope.tabTNA = 1;
    $scope.setTabTNA = function (newTab) {
        $scope.tabTNA = newTab;
    };
    $scope.isSetTNA = function (tabNum) {
        return $scope.tabTNA === tabNum;
    };
    $scope.onactivetab = function (args) {
        if (args.activeIndex == 0)
            $scope.GEEGetSelectedTasks($scope.fileNew.Id);
        else
            $scope.GVGetSelectedTasks2($scope.fileNew.Id);
    }
    $scope.PrintLinear = function () {

        //var MasterOrderId = "1935";
        try {
            var file_src = $scope.path + "MasterOrderReport?MasterOrderId=" + $scope.fileNew.Id + "&isMatrix=false";
            $rootScope.report(file_src);


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.PrintTaskList = function () {

        //var MasterOrderId = "1935";
        try {
            var file_src = "OrderManagements/ProductionOrderReports/TNAAuditReport?MasterOrderId=" + $scope.fileNew.Id; //?
            $rootScope.report(file_src);


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.PrintMatrix = function () {

        //var MasterOrderId = "1935";
        try {
            var file_src = $scope.path + "MasterOrderReport?MasterOrderId=" + $scope.fileNew.Id + "&isMatrix=true"; //?
            $rootScope.report(file_src);


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.getTaskList = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.tab1.$valid) {

            if ($scope.fileNew.Id != null) {
                $("#dialogViewTNADetail").data("ejDialog").open();
                $scope.GEEMasterOrderId = $scope.fileNew.Id;
                $scope.GVMasterOrderId = $scope.fileNew.Id;

                $scope.GEEGetSelectedTasks($scope.fileNew.Id);
                $scope.GVGetSelectedTasks2($scope.fileNew.Id);


            }
        }
    }

    $http.get("OrderManagements/ordercategory/getcbo/")
        .then(function (response) {
            $scope.orderCategoryList = response.data;
        });

    $http.get("OrderManagements/orderstatus/getcbo/")
        .then(function (response) {
            $scope.orderStatusList = response.data;
        });

    //cboService.getEnumCbo("enum/GetOrderStatusEnumCbo", function (result) {
    //    $scope.orderStatusList = result;
    //});

    $scope.OrderTypeList = [];
    cboService.getEnumCbo("enum/GetOrderTypeEnumCbo", function (result) {
        $scope.OrderTypeList = result;
    });


    cboService.getCboSeasons(function (result) {
        $scope.seasonList = result;
    });

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.fileNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });

    // #endregion Ddl

    $scope.index = -1;
    $scope.SetProductionRef = function (ind) {
        $scope.index = ind;
        $http.get("OrderManagements/MasterOrder/GetProductionRef?pg=" + $scope.itemList[$scope.index].OwnReferenceNo)
            .then(function (response) {
                $scope.itemList[$scope.index].ProductionGrouping = response.data[0].ProductionGrouping;
            });
    }


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.tab3 = 1;
    $scope.setTab3 = function (newTab) {
        $scope.tab3 = newTab;
    };
    $scope.isSet3 = function (tabNum) {
        return $scope.tab3 === tabNum;
    };


    $scope.currency = null;
    $scope.Get = function (index) {
        $scope.getPlantConfigByPlant();
        $scope.index = index.data;
        angular.copy(index.data, $scope.file);
        $scope.file.IsExtraOrderPercentage = $scope.file.ExtraOrderPercentage > 0;
        angular.copy($scope.file, $scope.fileNew);
        $scope.IsBillDiscountingDays = $scope.fileNew.IsBillDiscountingDays;
        $scope.fileNew.OrderYear = parseInt($scope.fileNew.OrderYear);
        $scope.RemarksControlmodel.Id = $scope.fileNew.UserRemarksControlId;
        $scope.RemarksControlmodel.MasterOrderId = $scope.fileNew.Id;
        $scope.RemarksControlmodel.RemarkControlId = $scope.fileNew.RemarkControlId;
        $scope.RemarksControlmodel.RemarksControl = $scope.fileNew.RemarksControl;
        $scope.Action = 'Update';
        getPartyPlantList();
        //$scope.GetResponsiblePersonList();
        //GetDepartmentPersonCbo();
        $scope.getMasterItemList();
        //$scope.getAllEntities();
        $scope.buyerChange();

        cboService.getCboProductionEntitiesByPlant($scope.fileNew.PlantId, function (result) {
            $scope.entityList = result;
        });
        $http.get("Parties/BuyerBrand/GetCbo?buyerId=" + $scope.fileNew.BuyerId)
            .then(function (response) {
                $scope.brandList = response.data;
            });
        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
            $scope.divisionList = result;

            cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
                $scope.departmentList = result;
            });
        });
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        $scope.currency = $scope.fileNew.Currency;
        $scope.currency = $("#Currency option:selected").text();

        cboService.getCboSpecialTaxByPlant($scope.fileNew.PlantId, function (response) {
            $scope.specialTaxList = response;
        });

        if ($scope.fileNew.IsExtraOrderPercentage === false) {
            $scope.fileNew.ExtraOrderPercentage = 0;
        }

        if (!baseService.isUndefinedOrNull($scope.fileNew.SpecialTaxId)) {
            $scope.fileNew.SpecialTaxId = $scope.fileNew.SpecialTaxId;
            $scope.SpecialTax = true;
        } else {
            $scope.SpecialTax = false;
        }
        $scope.mmChangeFlag = false;

        $scope.ResponsiblePersonName = $scope.fileNew.ResponsiblePersonName;
        $scope.ResponsiblePersonId = $scope.fileNew.ResponsiblePersonId;

        $scope.ExchangeDisplayExchangeRates($scope.fileNew.Id, $scope.fileNew.CurrencyId);//reloading currency exchange rates
        $scope.GetPaymentTermChangeable();
        $scope.GetPackingDetail();
        //$scope.GetContractByMasterOrder();
    };

    $scope.GetPaymentTermChangeable = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/masterOrder/GetPaymentTermChangeable?CompanyId=' + $scope.fileNew.CompanyId + '&PartyId=' + $scope.fileNew.PartyId
        }).then(function successCallback(response) {
            $scope.fileNew.IsPaymentTermChangeable = response.data[0].IsPaymentTermChangeable;
        });
    }

    $scope.btndisable = false;
    $scope.Save = function () {

        if (baseService.isUndefinedOrNull($scope.fileNew.ResponsiblePersonId)) {
            return ShowResult('Responsible Person is required.', 'failure');
        }
        $scope.customerName = $scope.fileNew.CustomerName;
        $scope.modelNew.CustomerId = $scope.fileNew.PartyId;
        $scope.modelNew.CustomerName = $scope.fileNew.CustomerName;
        $scope.ResponsiblePersonName = $scope.fileNew.ResponsiblePersonName;
        $scope.ResponsiblePersonId = $scope.fileNew.ResponsiblePersonId;
        if ($scope.isBuyerApplicable) {
            if (baseService.isUndefinedOrNull($scope.fileNew.BuyerId)) {
                return ShowResult('Buyer is required.', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.fileNew.BuyerDivisionId)) {
                return ShowResult('Division is required.', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.fileNew.BuyerDepartmentId)) {
                return ShowResult('Department is required.', 'failure');
            }
        }

        if (!baseService.isUndefinedOrNull($scope.fileNew.PaymentTermId) && !baseService.isUndefinedOrNull($scope.fileNew.DefaultPaymentTermId)) {
            if (($scope.fileNew.PaymentTermId !== $scope.fileNew.DefaultPaymentTermId) && baseService.isUndefinedOrNull($scope.RemarksControlmodel.RemarkControlId)) {
                return ShowResult('Payment Term Remarks is required.', 'failure');
            }
        }

        if (parseFloat(baseService.isUndefinedOrNull($scope.fileNew.TotalQty) ? 0 : $scope.fileNew.TotalQty) === 0) return ShowResult('Please insert total qty.', 'failure');

        if (baseService.isUndefinedOrNull($scope.fileNew.TotalQtyUOMId)) {
            return ShowResult('Total Quantity UoM is required.', 'failure');
        }

        if ($scope.fileNew.IsExtraOrderPercentage && $scope.fileNew.ExtraOrderPercentage === 0)
            return ShowResult('Please insert Extra Order Percentage.', 'failure');
        if (!baseService.isUndefinedOrNull($scope.fileNew.OrderWastagePercentage)) {
            if ($scope.fileNew.OrderWastagePercentage > 99) {
                return ShowResult('Order Wastage Percentage should less than 99 Percent.', 'failure');
            }
        }
        angular.copy($scope.fileNew, $scope.file);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.tab1.$valid) {
            $scope.btndisable = true;
            if ($scope.ExchangeSaveExchangeRates($scope.fileNew.CurrencyId) == false) {
                return;
            }

            if ($scope.Action === "Save") {
                $scope.btndisable = true;
                //if (baseService.arrayLength($scope.taskList) === 0) {
                //    return ShowResult('Select Task.', 'failure');
                //}

                for (var i = 0; i < $scope.taskList.length; i++) {

                    if ($scope.taskList[i].Active) {
                        $scope.taskList[i].IsRequired = $scope.taskList[i].Active;
                    }
                }

                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: {
                        'entity': $scope.file, 'taskList': $scope.taskList, 'CurrencyData': $scope.ExchangeDisplayCurrency, 'userRemarksControl': $scope.RemarksControlmodel
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                        $scope.btndisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.btndisable = false;
                        $scope.fileNew = response.data.MasterOrder;
                        $scope.ExchangeDisplayExchangeRates(response.data.MasterOrder.Id, response.data.MasterOrder.CurrencyId);//reloading currency exchange rates
                        $scope.getData();
                        $scope.setTab(2);
                        $scope.getMasterItemList();
                        //$scope.getAllEntities();
                        $scope.Action = 'Update';
                        $scope.fileNew.CustomerName = $scope.customerName;
                        $scope.fileNew.ResponsiblePersonName = $scope.ResponsiblePersonName;
                        $scope.fileNew.ResponsiblePersonId = $scope.ResponsiblePersonId;
                        //ClearFields();
                        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.divisionList = result;
                        });
                        cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.departmentList = result;
                        });
                        angular.element(document.querySelector('#TaskListPopUp')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    $scope.btndisable = false;
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {

                $scope.btndisable = true;
                for (var i = 0; i < baseService.arrayLength($scope.itemList); i++) {
                    if (baseService.isUndefinedOrNull($scope.itemList[i].MaterialMasterId)) {
                        $scope.btndisable = false;
                        return ShowResult('Material master need in row number ' + (i + 1), 'failure');
                    }
                    if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
                        $scope.itemList[i].ContractId = $scope.modelNew.Id;
                    }
                    if (baseService.isUndefinedOrNull($scope.itemList[i].UOMId)) {
                        $scope.btndisable = false;
                        return ShowResult('Item UoM is required.', 'failure');
                    }
                    if (baseService.isUndefinedOrNull($scope.itemList[i].ProductionGrouping)) {
                        $scope.btndisable = false;
                        return ShowResult('Production Group is required.', 'failure');
                    }
                }
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: {
                        'entity': $scope.file
                        , 'masterId': $scope.fileNew.Id
                        , 'personList': $scope.personList
                        , 'itemList': $scope.itemList
                        , 'CurrencyData': $scope.ExchangeDisplayCurrency
                        , 'userRemarksControl': $scope.RemarksControlmodel
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btndisable = false;
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.btndisable = false;
                        //$scope.GetResponsiblePersonList();
                        $scope.getMasterItemList();
                        $scope.getData();
                        //GetDepartmentPersonCbo();
                        //$scope.getAllEntities();
                        $scope.mmChangeFlag = false;
                        $scope.fileNew.ResponsiblePersonName = $scope.ResponsiblePersonName;
                        $scope.fileNew.ResponsiblePersonId = $scope.ResponsiblePersonId;
                        if (!baseService.isUndefinedOrNull($scope.fileNew.SpecialTaxId)) {
                            $scope.SpecialTax = true;
                        }
                        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.divisionList = result;
                        });
                        cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.departmentList = result;
                        });

                        $scope.ExchangeDisplayExchangeRates($scope.fileNew.Id, $scope.fileNew.CurrencyId);//reloading currency exchange rates

                    }
                }, function errorCallBack(response) {
                    $scope.btndisable = false;
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.removeMaster = function () {
        try {
            $scope.message_confirmation = "Are you sure want to permanent delete";
            angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.fileNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.fileNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.files.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.deleteItem = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST'
                , url: $scope.path + 'deleteItem?id=' + $scope.id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getMasterItemList();
                    $scope.id = null;
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        $scope.personList = [];
        $scope.itemList = [];
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.isBuyerApplicable = false;
        $scope.file = {};
        $scope.fileNew = {
            Id: null
            , EntityId: $scope.fileNew.EntityId
            , PlantId: $scope.fileNew.PlantId
            , OrderType: 'ExternalOrder'
            , PartyId: null
            , CompanyId: $scope.fileNew.CompanyId
        };
        $scope.getPlantConfigByPlant();
        $scope.SpecialTax = false;
        $scope.mmChangeFlag = false;
        $scope.customerName = null;
        $scope.ExchangeReset();
        $scope.enableJobOrOutSource = true;
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.btndisable = false;
        $scope.RemarksControlmodel = { Id: null, MasterOrderId: null, RemarkControlId: null, RemarksControl: null, UserRemarks: null };
    }

    // #region
    //$scope.partySearchByList = [
    //    {
    //        'name': $scope.partyType + ' Code',
    //        'value': 'Code'
    //    },
    //    {
    //        'name': $scope.partyType + ' Name',
    //        'value': 'PartyName'
    //    },
    //    {
    //        'name': 'Account Group',
    //        'value': 'PartyAccountGroupName'
    //    },
    //    {
    //        'name': 'Country',
    //        'value': 'CountryName'
    //    },
    //    {
    //        'name': 'State',
    //        'value': 'StateName'
    //    },
    //    {
    //        'name': 'Currency',
    //        'value': 'CurrencyCode'
    //    }
    //];
    //$scope.partyParameters = {
    //    limit: 10
    //    , offset: 0
    //    , order: 'ASC'
    //    , sort: 'PartyName, PartyAccountGroupName'
    //    , searchBy: 'PartyName'
    //    , pageSize: 10
    //    , total_count: 0
    //    , search: null
    //    , serverPagination: true
    //};
    //$scope.showPartyPopUp = function () {
    //    if (baseService.isUndefinedOrNull($scope.fileNew.CompanyId)) {
    //        ShowResult('Select Company', 'failure');
    //        return false;
    //    }
    //    if (baseService.isUndefinedOrNull($scope.fileNew.PlantId)) {
    //        ShowResult('Select Plant', 'failure');
    //        return false;
    //    }
    //    baseService.setCurrentPage('partyList');
    //    $scope.getPartyList = function (pageno) {
    //        $scope.partyUrl = $scope.path + 'GetCompanyPartyDataList?companyId=' + $scope.fileNew.CompanyId + '&plantId=' + $scope.fileNew.PlantId + '&partyType=' + $scope.partyType;
    //        baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
    //            .then(function (result) {
    //                $scope.partyList = result.Rows;
    //                $scope.partyParameters.total_count = result.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#partyPopUp')).modal('show');
    //    $scope.getPartyList();
    //};

    //$scope.selectPartyPopUpRow = function (index, id) {
    //    $scope.partyIndex = index;
    //    $scope.selectedParty = id;
    //};

    //$scope.selectCustomerPopUp = function (index, id) {
    //    $scope.partyIndex = index;
    //    $scope.selectedCustomer = id;
    //};

    //$scope.closePartyPopUp = function (x) {
    //    var party = x.data;
    //    $scope.fileNew.PartyCode = party.Code;
    //    $scope.fileNew.CustomerName = party.UserName;
    //    $scope.fileNew.PartyId = party.Id;
    //    $scope.fileNew.CurrencyId = party.CurrencyId;
    //    $scope.fileNew.PartyAccountGroupId = party.PartyAccountGroupId;

    //    $scope.fileNew.IsPaymentTermChangeable = '';
    //    $scope.fileNew.PaymentTermId = '';

    //    $scope.fileNew.PaymentTermId = party.PaymentTermId;
    //    $scope.fileNew.IsPaymentTermChangeable = party.IsPaymentTermChangeable;

    //    $scope.changePaymentTerm($scope.fileNew.PaymentTermId);
    //    $scope.personList = [];
    //    getPartyPlantList();
    //    //GetDepartmentPersonCbo();
    //    $scope.hidePartyPopUp();
    //};

    // #endregion

    $scope.searchByParty = "UserName"; $scope.searchParty = "";

    $scope.ShowCustomerPopUpNew = function () {
        $scope.partyType = "Customer";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
        if (baseService.isUndefinedOrNull($scope.fileNew.CompanyId)) {
            ShowResult('Select Company', 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.fileNew.PlantId)) {
            ShowResult('Select Plant', 'failure');
            return false;
        }


        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + $scope.fileNew.CompanyId + '&PlantId=' + $scope.fileNew.PlantId;

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
        $scope.fileNew.PartyCode = party.Code;
        $scope.fileNew.CustomerName = party.UserName;
        $scope.fileNew.PartyId = party.Id;
        $scope.fileNew.CurrencyId = party.CurrencyId;
        $scope.fileNew.PartyAccountGroupId = party.PartyAccountGroupId;

        $scope.fileNew.IsPaymentTermChangeable = '';
        $scope.fileNew.PaymentTermId = '';
        $scope.fileNew.PaymentTermDays = 0;

        $scope.fileNew.PaymentTermId = party.PaymentTermId;
        $scope.fileNew.DefaultPaymentTermId = party.PaymentTermId;
        $scope.fileNew.IsPaymentTermChangeable = party.IsPaymentTermChangeable;

        $scope.changePaymentTerm($scope.fileNew.PaymentTermId);
        $scope.personList = [];
        getPartyPlantList();
        //GetDepartmentPersonCbo();
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

    //$scope.changePaymentTerm = function () {
    //    if (!baseService.isUndefinedOrNull($scope.fileNew.PaymentTermId)) {
    //        var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.fileNew.PaymentTermId; })[0];
    //        $scope.fileNew.PaymentTermDays = paymentTerm.NoOfDay;

    //    }
    //};

    $scope.paymentTermList = [];
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getcustomercbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

    $scope.packingTypeList = [];
    $http({
        method: 'GET',
        url: 'OrderManagements/PackingType/GetCbo'
    }).then(function successCallback(response) {
        $scope.packingTypeList = response.data;
    });

    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.fileNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.fileNew.PaymentTermId; })[0];
            $scope.fileNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.fileNew.PaymentTermDays = paymentTerm.NoOfDay;
            $scope.BaseLineDate = paymentTerm.BaseLineDate;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.fileNew.BaseOnDueDate = $filter('dateFiltering')($scope.fileNew.AddedDate);
                    $scope.IsBaseOnDueDateEnable = false;
                }
                else if (paymentTerm.BaseLineDate === 'postingdate') {
                    $scope.fileNew.BaseOnDueDate = $filter('dateFiltering')($scope.fileNew.AddedDate);
                    $scope.fileNew.BaseOnDueDate = null;
                    $scope.fileNew.PaymentTermDays = null;
                    $scope.fileNew.MatureDate = null;
                    $scope.IsBaseOnDueDateEnable = false;
                }

                else {
                    $scope.fileNew.BaseOnDueDate = null;
                    $scope.IsBaseOnDueDateEnable = true;
                }

            $scope.getMatureDate($scope.fileNew.BaseOnDueDate, $scope.fileNew.PaymentTermDays);
        }
    };
    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.fileNew.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.fileNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };

    $scope.commitmentList = [];

    $scope.showCommitmentPopUp = function () {
        $http.get('OrderManagements/Commitment/GetCommitmentData')
            .then(function (response) {
                $scope.commitmentList = response.data;
            });
        angular.element(document.querySelector('#commitmentPop')).modal('show');
    }

    $scope.SetCommitment = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.fileNew.CommitmentId = $scope.data.Id;
        angular.element(document.querySelector('#commitmentPop')).modal('hide');
    }

    $scope.CloseCommitment = function () {
        angular.element(document.querySelector('#commitmentPop')).modal('hide');
    }

    function GetDepartmentPersonCbo() {
        $scope.personCboList = [];
        $http.get($scope.path + "GetDepartmentPersonCbo?plantId=" + $scope.fileNew.PlantId + '&partyAccountGroupId=' + $scope.fileNew.PartyAccountGroupId + '&partyId=' + $scope.fileNew.PartyId)
            .then(function (response) {
                $scope.personCboList = response.data;
            });
    }

    $scope.itemIndex = -1;

    $scope.mmChangeFlag = false;

    //$scope.materialType = ['FinishedGoods'];
    $scope.materialType = 'ProductDefinition';

    $scope.getMaterial = function (index) {
        $scope.itemIndex = index;
        $scope.getMaterialMasterbyTypePopUp();
    };

    $scope.selectMaterialByType = function (ob) {
        $scope.itemList[$scope.itemIndex].MaterialMasterId = ob.Id;
        $scope.itemList[$scope.itemIndex].MaterialMasterName = ob.UserName;
        $scope.itemList[$scope.itemIndex].ArticleId = null;
        $scope.itemList[$scope.itemIndex].ArticleName = null;
        $scope.itemList[$scope.itemIndex].InquiryItemId = null;
        $scope.itemList[$scope.itemIndex].SampleItemId = null;
        $scope.itemList[$scope.itemIndex].HasAttribute = ob.HasAttribute;
        $scope.mmChangeFlag = true;
        if ($scope.itemList[$scope.itemIndex].HasAttribute) {
            $scope.getArticleSearchList(ob.Id);
        } else {
            $scope.closeMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }
        // getTaxCategoryList(ob.HSNCodeId);
        $scope.HSNCodeId = ob.HSNCodeId;
        $scope.closeMaterialMasterbyTypePopUp();
    };

    $scope.getArticle = function (index) {
        $scope.itemIndex = index;
        if (!baseService.isUndefinedOrNull($scope.itemList[$scope.itemIndex].MaterialMasterId) && !$scope.itemList[$scope.itemIndex].HasAttribute)
            return ShowResult('This material has no attribute', 'failure');
        // $scope.getArticleSearchList($scope.itemList[$scope.itemIndex].MaterialMasterId);
        $scope.getMaterialMasterWithArticle(null);
    };

    $scope.ArticleId = null;
    // selectarticle setInputeMaterialArticleData
    $scope.setInputeMaterialArticleData = function (ob) {
        try {
            $scope.itemList[$scope.itemIndex].MaterialMasterId = ob.data.MaterialMasterId;
            $scope.itemList[$scope.itemIndex].MaterialMasterName = ob.data.MaterialMasterName;
            $scope.itemList[$scope.itemIndex].ArticleId = ob.data.Id;
            $scope.itemList[$scope.itemIndex].IsDefault = ob.data.IsDefault;
            $scope.itemList[$scope.itemIndex].ProductionGrouping = ob.data.ProductionGrouping;
            $scope.ArticleId = ob.data.Id;
            $scope.itemList[$scope.itemIndex].ArticleName = ob.data.StandardName;
            angular.element(document.querySelector('#materialarticleNewPopUp')).modal('hide');

            $scope.mmChangeFlag = true;
            GetArticleAlias();
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    function GetArticleAlias() {
        $http.get("Materials/materialmasterarticle/getArticleAliaslist?articleId=" + $scope.ArticleId)
            .then(function (response) {
                $scope.itemList[$scope.itemIndex].CustomerArticle = response.data[0].ArticlePartyName;
            });
    }


    $scope.clearArticle = function (index) {
        $scope.itemList[index].ArticleId = null;
        $scope.itemList[index].ArticleName = null;
    };

    $scope.getArticleValue = function (articleId, mName, aName, BuyerReferenceNo) {
        $scope.articleValueList = [];
        $scope.mName = mName;
        $scope.aName = aName;
        $scope.BuyerReferenceNo = BuyerReferenceNo;
        $http({
            method: 'GET',
            url: 'Materials/MaterialMasterArticle/GetMaterialArticleValue?articleId=' + articleId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) === 0)
                return ShowResult('This material has no article value', 'failure');
            $scope.articleValueList = response.data;
            angular.element(document.querySelector('#articleValuePoUp')).modal('show');
        });
    };

    $scope.closeArticleValuePopUp = function () {
        angular.element(document.querySelector('#articleValuePoUp')).modal('hide');
    };

    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Department',
        searchBy: "Department",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUpList = [];
    $scope.popUp = function (flag) {
        if (baseService.isUndefinedOrNull($scope.fileNew.PlantId)) return ShowResult('Select plant', 'failure');
        $scope.popUpDataList = [];
        $scope.popUpUrl = $scope.path + "GetDepartmentPersonList?plantId=" + $scope.fileNew.PlantId + '&partyAccountGroupId=' + $scope.fileNew.PartyAccountGroupId + '&partyId=' + $scope.fileNew.PartyId + '&flag=' + flag;
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    if (baseService.arrayLength(result) > 0) {
                        for (var i = 0; i < result.length; i++) {
                            if (!baseService.valueCheckInList($scope.personList, 'OrderResponsibleDepartmentId', result[i].OrderResponsibleDepartmentId)) {
                                $scope.popUpDataList.push(result[i]);
                            }
                        }
                    }
                    //$scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0)
                        baseService.getDDLSearchColumn(result, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    //$scope.selectDoubleClick = function (obj) {
    //    if (baseService.valueCheckInList($scope.personList, 'OrderResponsibleDepartmentId', obj.OrderResponsibleDepartmentId))
    //        return ShowResult(obj.Department + ' already taken.', '', 'popUpId');
    //    $scope.personList.push({
    //        Id: obj.Id
    //        , MasterOrderId: $scope.fileNew
    //        , CustomerDivisionId: obj.CustomerDivisionId
    //        , OrderResponsibleDepartmentId: obj.OrderResponsibleDepartmentId
    //        , Department: obj.Department
    //        , OurRespnsiblePersonId: obj.OurRespnsiblePersonId
    //        , EmployeeCode: obj.EmployeeCode
    //        , EmployeeName: obj.EmployeeName
    //        , PartyRespnsiblePersonId: obj.PartyRespnsiblePersonId
    //        , PartyRespnsiblePerson: obj.PartyRespnsiblePerson
    //    });
    //    //GetDepartmentPersonCbo();
    //    angular.element(document.querySelector('#popUpId')).modal('hide');
    //};

    $scope.removeRowModal = function (ob, index) {
        try {
            $scope.message_confirmation = "Are you sure want to permanent delete [" + ob.Submaterial + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
            $scope.popUpIndex = index;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        $scope.personList.splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
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

    $scope.showEmployeeListPopUps = function (name) {
        try {
            $scope.Name = name;
            //$scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                //$scope.employeeParameters.plantId = $scope.fileNew.PlantId;
                //$scope.employeeParameters.partyAccountGroupId = $scope.fileNew.PartyAccountGroupId;
                //$scope.employeeParameters.partyId = $scope.fileNew.PartyId;
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

    $scope.showEmployeeListPopUp = function (name) {
        try {
            if (baseService.isUndefinedOrNull($scope.fileNew.CompanyId)) {
                throw 'Select Company';
            }
            if (baseService.isUndefinedOrNull($scope.fileNew.PlantId)) {
                throw 'Select Plant';
            }

            $scope.Name = name;
            //$scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                $scope.employeeParameters.plantId = $scope.fileNew.PlantId;
                $scope.employeeParameters.partyAccountGroupId = $scope.fileNew.PartyAccountGroupId;
                $scope.employeeParameters.partyId = $scope.fileNew.PartyId;
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
            if ($scope.Name === 'mo') {
                $scope.fileNew.ResponsiblePersonId = employee.SystemId;
                $scope.fileNew.ResponsiblePersonName = employee.EmployeeName;
            } else if ($scope.Name === 'so') {
                $scope.soModel.ResponsiblePersonId = employee.SystemId;
                $scope.soModel.ResponsiblePersonName = employee.EmployeeName;
            }
            else if ($scope.Name === 'Stock') {
                $scope.soModel.StockResponsiblePersonId = employee.SystemId;
                $scope.soModel.StockResponsiblePerson = employee.EmployeeName;
            }
            else if ($scope.Name === 'boq') {
                $scope.qboqModel.ResponsiblePersonId = employee.SystemId;
                $scope.qboqModel.ResponsiblePersonName = employee.EmployeeName;
            }
            else if ($scope.Name === 'pd') {
                $scope.modelNewPD.ResponsiblePersonId = employee.SystemId;
                $scope.modelNewPD.ResponsiblePerson = employee.EmployeeName;
            }
            else {
                $scope.soSplitModel.ResponsiblePersonId = employee.SystemId;
                $scope.soSplitModel.ResponsiblePersonName = employee.EmployeeName;

            }
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.getLineItemType = function () {
        if (baseService.isUndefinedOrNull($scope.fileNew.Type)) {
            $scope.linetypeList = [
                { Value: "Manufacture", Text: "Manufacture" },
                { Value: "Trading", Text: "Trading" },
                { Value: "JobWork", Text: "Job Work" },
                { Value: "OutSource", Text: "Out Source" }
            ];
        }
        else if ($scope.fileNew.Type === "JobWork") {
            $scope.linetypeList = [
                { Value: "JobWork", Text: "Job Work" }
            ];
        }
        else if ($scope.fileNew.Type === "OutSource") {
            $scope.linetypeList = [
                { Value: "OutSource", Text: "Out Source" }
            ];
        } else {
            $scope.linetypeList = [
                { Value: "Manufacture", Text: "Manufacture" },
                { Value: "Trading", Text: "Trading" }
            ];
        }

        for (var i = 0; i < $scope.itemList.length; i++) {
            $scope.itemList[i].JobWorkType = null;
            $scope.itemList[i].EntityOrVendorName = null;
            $scope.itemList[i].EntityIdWithinCompany = null;
            $scope.itemList[i].EntityIdWithinGroup = null;
            $scope.itemList[i].PartyId = null;
        }
    };


    $scope.ChangeJobType = function (index) {
        $scope.itemList[index].JobWorkType = null;
        $scope.itemList[index].EntityOrVendorName = null;
        $scope.itemList[index].EntityIdWithinCompany = null;
        $scope.itemList[index].EntityIdWithinGroup = null;
        $scope.itemList[index].PartyId = null;
    }

    //#region Job Work PopUp

    $scope.jobWorkTypeList = [];
    cboService.getEnumCbo("enum/GetJobWorkTypeListCbo", function (result) {
        $scope.jobWorkTypeList = result;
    });

    $scope.boqjobWorkTypeList = [];
    cboService.getEnumCbo("enum/GetEnumJobWorkTypeListCbo", function (result) {
        $scope.boqjobWorkTypeList = result;
    });
    //#endregion

    //#region Job Work Type

    $scope.popUpTitle = '';
    $scope.valueData = '';
    $scope.popUp = function (index) {
        $scope.popUpList = [];
        $scope.popUpDataList = [];
        $scope.popUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Name'
            , searchBy: "Name"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        if (isJobWorkApplicable($scope.itemList, index))
            return ShowResult('Please select at first job work type!', 'failure');
        $scope.popUpUrl = typeCheckAndCreateUrl($scope.itemList, index);
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.index = index;
        $scope.getPopUpData();
    };

    $scope.selectDoubleClick = function (data) {
        valueSetInGrid($scope.itemList, data, $scope.index);
        $scope.index = -1;
        $scope.closePopUp();
    };

    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };

    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    function typeCheckAndCreateUrl(list, index) {
        if (list[index].JobWorkType === 'EntityWithinCompany') {
            $scope.popUpTitle = 'Entity within company';
            return 'Organizations/entity/withincompany?companyId=' + $scope.fileNew.companyId + '&entityId=' + $scope.fileNew.EntityId;
        }
        else if (list[index].JobWorkType === 'EntityWithinGroup') {
            $scope.popUpTitle = 'Entity in group';
            return 'Organizations/entity/withingroup?companyGroupId=' + $window.companyGroupId + '&companyId=' + $scope.fileNew.companyId + '&entityId=' + $scope.fileNew.EntityId;
        }
        else {
            $scope.popUpParameters.sort = 'PartyName';
            $scope.popUpParameters.searchBy = 'PartyName';
            if (list[index].Type === 'OutSource') {
                $scope.popUpTitle = 'Vendor';
                return 'Parties/party/GetCompanyPartyDataListByPlantId?CompanyId=' + $scope.fileNew.CompanyId + '&PlantId=' + $scope.fileNew.PlantId + '&partyType=' + 'vendor';
            } else {
                $scope.popUpTitle = 'Customer';
                return 'Parties/party/GetCompanyPartyDataListByPlantId?CompanyId=' + $scope.fileNew.CompanyId + '&PlantId=' + $scope.fileNew.PlantId + '&partyType=' + 'customer';
            }
        }
    }

    function isJobWorkApplicable(list, index) {
        if (baseService.isUndefinedOrNull(list[index].JobWorkType))
            return true;
        else
            return false;
    }
    function valueSetInGrid(list, data, index) {
        $scope.clearEntityOrVendor(list, index);
        if (list[index].JobWorkType === 'EntityWithinCompany') {
            list[index].EntityIdWithinCompany = data.Id;
            list[index].EntityOrVendorName = data.Company + ' - ' + data.Name;
        }
        else if (list[index].JobWorkType === 'EntityWithinGroup') {
            list[index].EntityIdWithinGroup = data.Id;
            list[index].EntityOrVendorName = data.Company + ' - ' + data.Name;
        }
        else {
            list[index].PartyId = data.Id;
            list[index].EntityOrVendorName = data.PartyName;
        }
    }


    $scope.clearEntityOrVendor = function (list, index) {
        list[index].EntityIdWithinCompany = null;
        list[index].EntityIdWithinGroup = null;
        list[index].PartyId = null;
        list[index].EntityOrVendorName = null;
    };

    $scope.clearJobType = function (list, index) {
        list[index].JobWorkType = null;
    };
    //#endregion Job Work Type

    $scope.mitemList = [];
    $scope.getMasterItemList = function () {
        $scope.itemList = [];
        $scope.itemTestingStandardList = [];
        if (baseService.isUndefinedOrNull($scope.fileNew.BuyerId))
            $scope.itemTestingStandardList = $filter('unique')($scope.testingStandardList, 'Text');
        else
            $scope.itemTestingStandardList = $filter('filter')($scope.testingStandardList, { BuyerId: $scope.fileNew.BuyerId }, true);
        $http.get($scope.path + "GetMasterItemList?masterOrderId=" + $scope.fileNew.Id)
            .then(function (response) {
                $scope.itemList = response.data;
                $scope.mitemList = response.data;
                var obj = { MasterOrderItemId: null, MaterialMasterId: null };
                if (baseService.arrayLength($scope.itemList) > 0) {
                    for (var i = 0; i < $scope.itemList.length; i++) {
                        $scope.itemList[i].TempList = [];

                        if ($scope.itemList[i].Type == 'JobWork' || $scope.itemList[i].Type == 'OutSource') {
                            $scope.enableJobOrOutSource = false;
                        } else {
                            $scope.enableJobOrOutSource = true;
                        }

                        for (var j = 0; j < $scope.mitemList.length; j++) {
                            if ($scope.mitemList[j].Id != $scope.itemList[i].Id) {
                                obj.MasterOrderItemId = $scope.mitemList[j].Id;
                                obj.MaterialMasterId = $scope.mitemList[j].MaterialMasterId;
                                obj.TotalQty = $scope.mitemList[j].TotalQty;
                                $scope.itemList[i].TempList.push(obj);
                                obj = {};
                            }
                        }
                    }
                }

                if (baseService.arrayLength($scope.itemList) === 0) {
                    for (var i = 0; i < parseInt($scope.fileNew.NoOfLineItem); i++) {
                        $scope.itemList.push({
                            Id: null,
                            MasterOrderId: $scope.fileNew.Id == null ? null : $scope.fileNew.Id,
                            InquiryItemId: null,
                            SampleItemId: null,
                            MaterialMasterId: null,
                            MaterialMasterName: null,
                            ArticleId: null,
                            ArticleName: null,
                            BuyerReferenceNo: null,
                            OwnReferenceNo: null,
                            TotalQty: null,
                            AddedBy: null,
                            AddedDate: null,
                            AddedFromIP: null,
                            UpdatedBy: null,
                            UpdatedDate: null,
                            UpdatedFromIP: null,
                            OrderWastagePercentage: $scope.fileNew.OrderWastagePercent,
                            ExtraOrderPercentage: $scope.fileNew.ExtraOrderPercentage,
                            ProductionGrouping: null,
                            TestingStandardId: $scope.fileNew.TestingStandardId,
                            IsRepeat: false,
                            Consignment: false,
                            Type: $scope.fileNew.Type,
                            ContractId: $scope.modelNew.Id == null ? null : $scope.modelNew.Id,
                            BuyerItemDescription: null,
                            MainRawMaterialDescription: null,
                            JobWorkType: null,
                            EntityIdWithinCompany: null,
                            EntityIdWithinGroup: null,
                            PartyId: null,
                            ProductLibraryId: null,
                            FileName: null,
                            Remark: null,
                            OrderStatusId: null,
                            UOMId: $scope.fileNew.TotalQtyUOMId
                        });
                    }
                }
            });
        $scope.getLineItemType();
    }


    $scope.CostingPath = 'Costings/OrderCosting/';

    $scope.OrderCostingId = null;
    $scope.ReportPopUp = function (x) {
        try {
            $scope.OrderCostingId = x.OrderCostingMasterTemplateId;
            $scope.MOIId = x.Id;

            //$scope.openPopup('CostingPopUp');
            angular.element(document.querySelector('#CostingPopUp')).modal('show');
        } catch (e) {
        }
    }

    $scope.SORatePopUp = function (x) {
        try {
            $scope.OrderCostingId = x.OrderCostingMasterTemplateId;
            $scope.MOIId = x.Id;

            //$scope.openPopup('CostingPopUp');
            angular.element(document.querySelector('#CostingPopUp')).modal('show');
        } catch (e) {
        }
    }

    $scope.OrderPreCosting = function () {
        try {
            $scope.PreCosting = 1;
            var file_src = $scope.CostingPath + 'GetOrderCostingReport?OrderCostingId=' + $scope.OrderCostingId + '&preCosting=' + $scope.PreCosting + '&MOIId=' + $scope.MOIId;
            $rootScope.report(file_src);

        } catch (e) {
        }
    }
    $scope.OrderProcurementCosting = function () {
        try {
            $scope.ProcurementCosting = 1;
            var file_src = $scope.CostingPath + 'GetOrderCostingReport?OrderCostingId=' + $scope.OrderCostingId + '&procurementCosting=' + $scope.ProcurementCosting + '&MOIId=' + $scope.MOIId;
            $rootScope.report(file_src);

        } catch (e) {
        }
    }

    $scope.addNewItem = function () {
        $scope.getLineItemType();
        $scope.itemList.push({
            Id: null,
            MasterOrderId: $scope.fileNew.Id == null ? null : $scope.fileNew.Id,
            InquiryItemId: null,
            SampleItemId: null,
            MaterialMasterId: null,
            MaterialMasterName: null,
            ArticleId: null,
            ArticleName: null,
            BuyerReferenceNo: null,
            OwnReferenceNo: null,
            TotalQty: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null,
            OrderWastagePercentage: $scope.fileNew.OrderWastagePercent,
            ExtraOrderPercentage: $scope.fileNew.ExtraOrderPercentage,
            ProductionGrouping: null,
            TestingStandardId: $scope.fileNew.TestingStandardId,
            IsRepeat: false,
            Consignment: false,
            Type: $scope.fileNew.Type,
            ContractId: $scope.modelNew.Id == null ? null : $scope.modelNew.Id,
            BuyerItemDescription: null,
            MainRawMaterialDescription: null,
            JobWorkType: null,
            EntityIdWithinCompany: null,
            EntityIdWithinGroup: null,
            PartyId: null,
            ProductLibraryId: null,
            FileName: null,
            Remark: null,
            OrderStatusId: null,
            UOMId: $scope.fileNew.TotalQtyUOMId
        });
    };

    $scope.removeLineItem = function (index) {
        $scope.itemList.splice(index, 1);
    };

    function containsSpecialChars(str) {
        const specialChars = /[`!@#$%^&*()_+\=\[\]{};':"\\|,.<>\/?~]/;
        return specialChars.test(str);
    }

    $scope.CheckSpecialCharecter = function (index) {
        try {
            if (containsSpecialChars($scope.itemList[index].ProductionGrouping)) {
                $scope.itemList[index].ProductionGrouping = $scope.itemList[index].ProductionGrouping.substring(0, $scope.itemList[index].ProductionGrouping.length - 1);
                throw "No special characters allowed for Production Group.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //#region Party plant 

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };
    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.fileNew.InvoicingState = state;
                $scope.fileNew.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.fileNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.fileNew.DeliveryState = state;
                $scope.fileNew.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.fileNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.fileNew.InvoicingState = null;
                $scope.fileNew.InvoicingGSTIN = null;
                return $scope.fileNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.fileNew.DeliveryState = null;
                $scope.fileNew.DeliveryGSTIN = null;
                return $scope.fileNew.DeliveryByAddress = null;
            }
        }
    };

    function getPartyPlantList() {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.fileNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.IsDefault) {
                    $scope.fileNew.InvoicingPartyPlantId = item.Value;
                    $scope.fileNew.DeliveryPartyPlantId = item.Value;
                    $scope.fileNew.InvoicingByAddress = item.Address1;
                    $scope.fileNew.DeliveryByAddress = item.Address1;
                    $scope.fileNew.InvoicingState = item.StateName;
                    $scope.fileNew.InvoicingGSTIN = item.GSTIN;
                    $scope.fileNew.DeliveryState = item.StateName;
                    $scope.fileNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }

    //#endregion Party plant 

    // #region Attribute

    $scope.searchFreeField = false;

    $scope.getAttribute = function (id, materialMasterId, mName) {
        if ($scope.mmChangeFlag) return ShowResult('Please update changes data', 'failure');
        $scope.mName = mName;
        $scope.masterItemId = id;
        var url = '';
        if (baseService.isUndefinedOrNull($scope.masterItemId))
            url = 'OrderManagements/MasterOrder/GetAttributeListByMaterialMasterId?materialMasterId=' + materialMasterId;
        else
            url = 'OrderManagements/MasterOrder/GetOrderAttributeListByMasterId?masterItemId=' + $scope.masterItemId + '&materialMasterId=' + materialMasterId;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.attributeList = response.data;
            if (baseService.arrayLength(response.data) === 0)
                return ShowResult('This material has no attribute', 'failure');
            for (var i = 0; i < $scope.attributeList.length; i++) {
                $scope.searchFreeField = $scope.attributeList[i].ValueFreeText !== null ? true : false;
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
            }
            angular.element(document.querySelector('#attributePoUp')).modal('show');
        });
    };

    $scope.idNullByValueFreeText = function (id, index) {
        if ($scope.attributeList[index].AttributeId === id) {
            $scope.attributeList[index].MaterialAttributeValueId = null;
            $scope.attributeList[index].MaterialMasterAttributeValueId = null;
        }
    };
    $scope.IsFreeFieldOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.searchFreeField)
                return true;//disabled true
            else
                return false;//disabled false
        }
        else
            return true;//disabled true
    };
    $scope.IsMandatoryButNull = function (isMandatory, ValueFreeText) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(ValueFreeText)) return true;
            else return false;
        }
        else return false;
    };

    $scope.saveAttribute = function () {
        $http({
            method: 'POST'
            , url: $scope.path + 'CreateAttributeValue'
            , data: {
                'masterItemId': $scope.masterItemId
                , 'attributeValueList': $scope.attributeList
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.attributeList = [];
                $scope.masterItemId = null;
                angular.element(document.querySelector('#attributePoUp')).modal('hide');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.closeAttributePopUp = function () {
        angular.element(document.querySelector('#attributePoUp')).modal('hide');
    };

    // #endregion Attribute

    // #region value

    $scope.valueindex = -1;
    $scope.searchvalueList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'StanderName',
            'value': 'StanderName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }
    ];
    $scope.valueParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'Code'
        , searchBy: "UserName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.valuePoUp = function (data, index) {
        $scope.materialAttributeValueUrl = 'Materials/MaterialMasterArticle/GetAttributeValueList';
        baseService.setCurrentPage('valueList');
        $scope.getValueData = function (pageno) {
            $scope.valueParameters.assignment = data.ValueAssignmentLevel;
            $scope.valueParameters.materialMasterId = data.MaterialMasterId;
            $scope.valueParameters.attributeId = data.AttributeId;
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.valueParameters)
                .then(function (result) {
                    $scope.valueList = result.Rows;
                    $scope.valueParameters.total_count = result.Total;
                    $scope.valueindex = index;
                    $scope.searchFreeField = true;
                    angular.element(document.querySelector('#attributeValuePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getValueData();
    };
    $scope.getAttrValue = function (data) {
        $scope.attributeList[$scope.valueindex].AttributeValueId = data.MaterialAttributeValueId;
        $scope.attributeList[$scope.valueindex].ValueFreeText = data.UserName;
        $scope.attributeList[$scope.valueindex].FlagDisable = $scope.searchFreeField;
        $scope.valueindex = -1;
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
    };
    $scope.materialAttributeValueClear = function (index) {
        $scope.attributeList[index].MaterialAttributeValueId = null;
        $scope.attributeList[index].MaterialMasterAttributeValueId = null;
        $scope.attributeList[index].ValueFreeText = null;
        $scope.searchFreeField = false;
        var isFree = $scope.attributeList[index].IsFreeField;
        $scope.attributeList[index].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    };
    $scope.closeValuePopUp = function () {
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
        CloseModalShowResult('attributeValuePopUp');
    };

    // #endregion value

    // #region Sales Order
    $scope.TotalMOIQty = 0;
    $scope.JobWorkType = '';

    $scope.ProductionTypeList = [
        {
            'Value': 'Order',
            'Text': 'Order'
        },
        {
            'Value': 'Stock',
            'Text': 'Stock'
        }
    ];

    $scope.getSalesOrder = function (x, id, materialMasterId, mName, aName, hsnCodeId, BuyerReferenceNo) {
        try {
            $http({
                method: 'GET',
                url: 'OrderManagements/MasterOrder/GetCostingSOFormulaData?masterOrderItemId=' + x.Id
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0) {
                    for (var i = 0; i < response.data.length; i++) {
                        if (baseService.isUndefinedOrNull(response.data[i].Id)) {
                            return ShowResult("First Save Item Costing data.", 'failure');
                        }
                        else {
                            angular.element(document.querySelector('#soPoUp')).modal('show');
                        }
                    }
                }
            });


            $scope.TotalMOIQty = x.TotalQty;
            $scope.JobWorkType = baseService.isUndefinedOrNull(x.JobWorkType) ? x.JobWorkType : x.JobWorkType + '>> ' + baseService.isUndefinedOrNull(x.EntityOrVendorName) ? x.EntityOrVendorName : x.EntityOrVendorName;

            $scope.TotalProducedQty = 0;
            $scope.ProdBookedQty = 0;
            if ($scope.mmChangeFlag) return ShowResult('Please update changes data', 'failure');
            $scope.mName = baseService.isUndefinedOrNull(aName) ? mName : mName + '   >>>   ' + aName + ' > ' + BuyerReferenceNo;
            $scope.masterItemId = id;
            $scope.materialMasterId = materialMasterId;
            $scope.currency = $("#Currency option:selected").text();
            $scope.soModel = {
                Id: null
                , MasterOrderItemId: $scope.masterItemId
                , DeliveryDate: null
                , CommitmentDate: null
                , DestinationId: null
                , ShipmentModeId: null
                , ReviseDate: null
                , BillDiscountingDays: 0
                , CustomerPOId: null
                , PONumber: null
                , OrderStatusId: null
                , OrderCategoryId: $scope.fileNew.OrderCategoryId
                , SOType: null
                , ResponsiblePersonId: $scope.ResponsiblePersonId
                , ResponsiblePersonName: $scope.ResponsiblePersonName
                , Qty: 0
                , Rate: 0
                , HSNCodeId: hsnCodeId
                , TotalTaxAmount: 0
                , LSD: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
                , MainRawMaterialInhouseDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
                , OtherRawMaterialInhouseDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
                , SalesOrderYear: null
                , WeekNo: null
                , PlanExFactoryDate: null
                , QtyChangedBy: null
                , QtyChangedDate: null
                , QtyChangedFromIP: null
                , DestinationDescription: null
                , SalesExpense: null
                , NetSalesRealization: null
                , DirectCost: 0
                , ValueLoss: 0
                , Other: 0
                , UpCharge: 0
                , Discount: 0
                , CM: 0
                , ProductionType: 'Order'
                , ShipmentFromStock: null
                , StockResponsiblePersonId: null
                , StockResponsiblePerson: null
                , PackingTypeId: null
                , ContractId: null
                , ContractNo: null
                , CheckByDate: null, CheckByStatus: 'To Be Check', ApproveBy: null, ApproveByDate: null, ApprovedStatus: null, DeliveryGroup: null
            };
            getSalesOrderList();
            $scope.getDestination();

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    function getSalesOrderList() {
        $scope.salesOrderList = [];
        $http.get('OrderManagements/MasterOrder/GetSOandItemList?masterItemId=' + $scope.masterItemId)
            .then(function (response) {
                $scope.salesOrderList = response.data;
            });
    }

    $scope.getDestination = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/destination/GetCbo/'
        }).then(function successCallback(response) {
            $scope.destinationList = response.data;
        });
        $http({
            method: 'GET',
            url: 'OrderManagements/shipmode/GetCbo/'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.shipmentModeList = response.data;
            }
        });
    };

    $scope.packingTypeDataList = [];
    $scope.getPackingTypeData = function () {
        $scope.packingTypeDataList = [];
        $http.get('OrderManagements/MasterOrder/GetpackingTypeDataList?SOId=' + $scope.sqlInStatement + '&PackingType=' + $scope.PT)
            .then(function (response) {
                $scope.packingTypeDataList = response.data;
            });
    }
    $scope.PT = null;

    $scope.showSku1 = false;
    $scope.showSku2 = false;
    $scope.showQty = false;
    $scope.showToPlanQty = false;
    $scope.showPlan = false;

    $scope.GetPackingTypeChidChangeData = function () {
        for (var i = 0; i < $scope.packingTypeList.length; i++) {
            if ($scope.packingTypeList[i].Value == $scope.ModelPTNew.PackingTypeId) {
                if ($scope.packingTypeList[i].PackingType == 'AssortedAssorted') {
                    $scope.showSku1 = true;
                    $scope.showSku2 = true;
                    $scope.showQty = true;
                    $scope.showToPlanQty = false;
                    $scope.showPlan = false;
                    $scope.PT = $scope.packingTypeList[i].PackingType;
                }
                if ($scope.packingTypeList[i].PackingType == 'AssortedSolid') {
                    $scope.showSku1 = true;
                    $scope.showSku2 = false;
                    $scope.showQty = true;
                    $scope.showToPlanQty = true;
                    $scope.showPlan = true;
                    $scope.PT = $scope.packingTypeList[i].PackingType;
                }
                if ($scope.packingTypeList[i].PackingType == 'SolidSolid') {
                    $scope.showSku1 = false;
                    $scope.showSku2 = false;
                    $scope.showQty = true;
                    $scope.showToPlanQty = true;
                    $scope.showPlan = true;
                    $scope.PT = $scope.packingTypeList[i].PackingType;
                }
                if ($scope.packingTypeList[i].PackingType == 'SolidAssorted') {
                    $scope.showSku1 = false;
                    $scope.showSku2 = true;
                    $scope.showQty = true;
                    $scope.showToPlanQty = true;
                    $scope.showPlan = true;
                    $scope.PT = $scope.packingTypeList[i].PackingType;
                }

            }
        }
        if (!baseService.isUndefinedOrNull($scope.ModelPTNew.Id)) {
            $scope.GetPackingTypeChildData();

        } else {

            $scope.getPackingTypeData();
        }
    }

    $scope.ProdBookedQty = 0;
    $scope.TotalProducedQty = 0;
    $scope.GetSOBookedQtyAndLevel = function (salesOrderId) {

        if (!baseService.isUndefinedOrNull($scope.soModel.Id) && $scope.soModel.OrderStatusId !== 'Active') {
            $http({
                method: 'GET',
                url: 'OrderManagements/MasterOrder/GetSOBookedQtyAndLevel?salesOrderId=' + salesOrderId
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0) {
                    if ($scope.soModel.ProductionBookedQty === 0) {
                        $scope.soModel.ProductionBookedQty = response.data[0].Quantity;
                        $scope.soModel.ProductionBookingLevel = response.data[0].BookingLevel;
                        $scope.ProdBookedQty = response.data[0].Quantity;
                        $scope.TotalProducedQty = $scope.ProdBookedQty;
                    }

                }
                if ($scope.soModel.ProductionBookedQty == 0.00) {
                    $http({
                        method: 'GET',
                        url: 'OrderManagements/MasterOrder/GetPOBookedQtyAndLevel?salesOrderId=' + salesOrderId
                    }).then(function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            if ($scope.soModel.ProductionBookedQty === 0) {
                                $scope.soModel.ProductionBookedQty = 0;
                                $scope.soModel.ProductionBookingLevel = response.data[0].BookingLevel;
                                $scope.ProdBookedQty = response.data[0].Quantity;
                                $scope.TotalProducedQty = $scope.ProdBookedQty;
                            }

                        }
                    });
                }
            });
        } else {
            $scope.soModel.ProductionBookedQty = 0;
        }
    };

    $scope.SetTotalProdQty = function () {
        $scope.TotalProducedQty = $scope.soModel.ProductionBookedQty + $scope.ProdBookedQty;
    }

    $scope.ShowCostingSORatePopup = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetCostingSORateData?SalesOrderId=' + $scope.soModel.Id + '&lineId=' + $scope.masterItemId
        }).then(function successCallback(response) {
            $scope.costingSOConfirmList = [];
            $scope.costingSOConfirmList = response.data;
            angular.element(document.querySelector('#SOCostingRatePopup')).modal('show');
        });
    };

    $scope.saveSalesOrder = function () {

        //if ($scope.soModel.PONumber === null || $scope.soModel.OrderStatusId === null || $scope.soModel.OrderCategoryId === null || $scope.soModel.DestinationId === null || $scope.soModel.ShipmentModeId === null || $scope.soModel.Qty === null) {
        //    ShowResult("Please enter mandatory fields", 'failure', 'soPoUp');
        //    return false;
        //}


        $scope.$broadcast('show-errors-check-validity');

        if ($scope.soForm.$valid) {
            if ($scope.soModel.Qty <= 0) {
                ShowResult("Sales order quantity can't be zero", 'failure', 'soPoUp');
                return false;
            }
            if ($scope.soModel.Rate < $scope.soModel.Discount) {
                ShowResult("Sales order discount can't greater than Rate", 'failure', 'soPoUp');
                return false;
            }

            if (!baseService.isUndefinedOrNull($scope.soModel.Id)) {
                if ($scope.delivaryDate !== $scope.soModel.DeliveryDate) {
                    if (baseService.isUndefinedOrNull($scope.soModel.Reason)) {
                        ShowResult("Reason is required on Delivery Date change.", 'failure', 'soPoUp');
                        return false;
                    }
                }
            }

            if ($scope.soModel.OrderStatusId !== 'Active') {
                if ($scope.soModel.ProductionBookedQty < 0) {
                    ShowResult("Production Booked Qty can't less than 0.", 'failure', 'soPoUp');
                    return false;
                }

                if ($scope.soModel.ProductionBookingLevel == 'ProductionOrder') {
                    if ($scope.ProdBookedQty < $scope.soModel.ProductionBookedQty) {
                        ShowResult("Production Booked Qty '[" + $scope.soModel.ProductionBookedQty + "]' can't greater than Produced Qty '[" + $scope.ProdBookedQty + "]'.", 'failure', 'soPoUp');
                        return false;
                    }
                }
            }

            if (baseService.isUndefinedOrNull($scope.soModel.ProductionType)) {
                ShowResult("Select Production Type", 'failure', 'soPoUp');
                return false;
            }
            if ($scope.soModel.ProductionType === 'Stock') {
                if (baseService.isUndefinedOrNull($scope.soModel.StockResponsiblePersonId)) {
                    ShowResult("Select Stock Responsible Person", 'failure', 'soPoUp');
                    return false;
                }
            }
            if (baseService.isUndefinedOrNull($scope.soModel.ShipmentFromStock)) {
                ShowResult("Select Shipment From Stock", 'failure', 'soPoUp');
                return false;
            }


            if (baseService.isUndefinedOrNull($scope.soModel.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'CreateSalesOrder'
                    , data: {
                        'masterItemId': $scope.masterItemId
                        , 'salesOrderMaster': $scope.soModel
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'soPoUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'soPoUp');
                        getSalesOrderList();
                        $scope.soModel.Id = response.data.Data.Id;

                        $scope.ShowCostingSORatePopup();
                        $scope.getMasterItemList();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'soPoUp');
                };
            } else {
                getSalesOrderTaxCategoryUpdateList($scope.soModel.Id);
            }
        }
    };

    $scope.GetSOCostingConfirmData = function (soId, lieneId) {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetCostingSORateData?SalesOrderId=' + soId + '&lineId=' + lieneId
        }).then(function successCallback(response) {
            $scope.costingSOConfirmList = [];
            $scope.costingSOConfirmList = response.data;
            if (baseService.isUndefinedOrNull($scope.costingSOConfirmList[0].Id)) {
                ShowResult("Please do SO Costing Confirmation.", 'failure', 'soPoUp');
            }
        });
    };

    function getSalesOrderTaxCategoryUpdateList(salesOrderId) {
        $scope.SoTotalAmount = ((parseFloat($scope.soModel.Qty) * parseFloat($scope.soModel.Rate)) - parseFloat($scope.soModel.Discount)).toFixed(2);
        $http({
            method: 'GET'
            , url: $scope.path + 'getSalesOrderTaxCategoryList?salesOrderId=' + salesOrderId
        }).then(function (response) {
            $scope.taxList = response.data;
            for (var i = 0; i < baseService.arrayLength($scope.taxList); i++) {
                $scope.taxList[i].TaxAmount = $scope.SoTotalAmount * (parseFloat($scope.taxList[i].Percentage) / 100);
            }
            UpdateSOWithTax();
        });

    }

    function UpdateSOWithTax() {
        $http({
            method: 'POST'
            , url: $scope.path + 'UpdateSalesOrder'
            , data: {
                'masterItemId': $scope.masterItemId
                , 'salesOrderMaster': $scope.soModel
                , 'taxCategoryList': $scope.taxList
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'soPoUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'soPoUp');
                getSalesOrderTaxCategoryList(response.data.Id);
                getSalesOrderList();
                $scope.ShowCostingSORatePopup();
                $scope.getMasterItemList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'soPoUp');
        };
    }

    $scope.delivaryDate = null;

    $scope.ShipmentFromStocklist = [
        { Text: "Yes", Value: 1 },
        { Text: "No", Value: 0 }
    ];
    $scope.soEdit = function (data) {
        $scope.TotalProducedQty = 0;
        angular.copy(data, $scope.soModel);
        if ($scope.soModel.ShipmentFromStock) {
            $scope.soModel.ShipmentFromStock = 1;
        } else {
            $scope.soModel.ShipmentFromStock = 0;
        }
        $scope.delivaryDate = null;
        $scope.delivaryDate = $scope.soModel.DeliveryDate;
        $scope.soModel.SalesOrderYear = parseInt($scope.soModel.SalesOrderYear);

        if (!baseService.isUndefinedOrNull($scope.soModel.Id) && $scope.soModel.OrderStatusId !== 'Active') {
            if ($scope.soModel.ProductionBookingLevel === 'SalesOrder') {
                $http({
                    method: 'GET',
                    url: 'OrderManagements/MasterOrder/GetSOBookedQtyAndLevel?salesOrderId=' + $scope.soModel.Id
                }).then(function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {

                        $scope.ProdBookedQty = response.data[0].Quantity;
                        $scope.TotalProducedQty = $scope.soModel.ProductionBookedQty + $scope.ProdBookedQty;
                    }

                });
            }
            else if ($scope.soModel.ProductionBookingLevel === 'ProductionOrder') {

                $http({
                    method: 'GET',
                    url: 'OrderManagements/MasterOrder/GetPOBookedQtyAndLevel?salesOrderId=' + $scope.soModel.Id
                }).then(function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ProdBookedQty = response.data[0].Quantity;
                        $scope.TotalProducedQty = $scope.soModel.ProductionBookedQty + $scope.ProdBookedQty;
                    }
                });

            }
        }

        $scope.SetNetSalesRealization();

    };

    $scope.removeSOItemList = function (index, data) {
        $scope.tempEmpOb = data;
        $scope.empIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.message = 'Are you sure want to parmenently delete?';
        else
            $scope.message = 'Are you sure want to parmenently delete?';
        angular.element(document.querySelector('#removePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.tempEmpOb.Id)) {

            $scope.soDelete($scope.empIndex, $scope.tempEmpOb);
        }
        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
    };

    $scope.soDelete = function (index, soModel) {
        //if (confirm("Are you sure to delete")) {
        $http({
            method: 'POST'
            , url: $scope.path + 'DeleteSalesOrder'
            , data: {
                'masterItemId': $scope.masterItemId
                , 'salesOrderMaster': soModel
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'soPoUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'soPoUp');
                getSalesOrderList();
                clearSO();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'soPoUp');
        };
        $scope.salesOrderList.splice(index, 1);
        //}

    };

    $scope.closeSOPopUp = function () {
        try {
            //if (!baseService.isUndefinedOrNull($scope.soModel.Id)) {
            //    $http({
            //        method: 'GET',
            //        url: 'OrderManagements/MasterOrder/GetCostingSORateData?SalesOrderId=' + $scope.soModel.Id + '&lineId=' + $scope.masterItemId
            //    }).then(function successCallback(response) {
            //        $scope.costingSOConfirmList = [];
            //        $scope.costingSOConfirmList = response.data;
            //        if (baseService.isUndefinedOrNull($scope.costingSOConfirmList[0].Id)) {
            //            ShowResult("Please do SO Costing Confirmation.", 'failure', 'soPoUp');
            //        }
            //        else {
            //            angular.element(document.querySelector('#soPoUp')).modal('hide');
            //        }
            //    });
            //} else {
            //    angular.element(document.querySelector('#soPoUp')).modal('hide');
            //}
            angular.element(document.querySelector('#soPoUp')).modal('hide');

        } catch (e) {
            ShowResult(e, 'failure', 'soPoUp');
        }
    };

    function clearSO() {
        $scope.soModel = {
            Id: null
            , MasterOrderItemId: $scope.masterItemId
            , DeliveryDate: null
            , CommitmentDate: null
            , DestinationId: null
            , ShipmentModeId: null
            , ReviseDate: null
            , BillDiscountingDays: 0
            , CustomerPOId: null
            , PONumber: null
            , OrderStatusId: $scope.fileNew.OrderStatusId
            , OrderCategoryId: $scope.fileNew.OrderCategoryId
            , SOType: null
            , ResponsiblePersonId: $scope.ResponsiblePersonId
            , ResponsiblePersonName: $scope.ResponsiblePersonName
            , Qty: 0
            , Rate: 0
            , HSNCodeId: $scope.HSNCodeId
            , TotalTaxAmount: 0
            , LSD: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
            , MainRawMaterialInhouseDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
            , OtherRawMaterialInhouseDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
            , SalesOrderYear: null
            , WeekNo: null
            , PlanExFactoryDate: null
            , ProductionBookedQty: null
            , ProductionBookingLevel: null
            , QtyChangedBy: null
            , QtyChangedDate: null
            , QtyChangedFromIP: null
            , DestinationDescription: null
            , SalesExpense: null
            , NetSalesRealization: null
            , DirectCost: 0
            , ValueLoss: 0
            , Other: 0
            , UpCharge: 0
            , Discount: 0
            , CM: 0
            , ProductionType: 'Order'
            , ShipmentFromStock: null
            , StockResponsiblePersonId: null
            , StockResponsiblePerson: null
            , PackingTypeId: null
            , ContractId: null
            , ContractNo: null
            , CheckByDate: null, CheckByStatus: 'To Be Check', ApproveBy: null, ApproveByDate: null, ApprovedStatus: null, DeliveryGroup: null
        };
    }

    //$scope.Percentage = 0;
    //$scope.GetContractPercentage = function (id) {
    //    $http({
    //        method: 'GET',
    //        url: 'OrderManagements/masterOrder/GetContractPercentage?masterOrderItemId=' + id
    //    }).then(function successCallback(response) {
    //        if (baseService.arrayLength(response.data) > 0) {
    //            $scope.Percentage = response.data[0].Percentage;
    //        }
    //    });
    //}

    $scope.SetNetSalesRealization = function () {
        //$scope.soModel.NetSalesRealization = $scope.soModel.Rate - $scope.soModel.Discount - $scope.soModel.SalesExpense - $scope.Percentage / 100;
        $scope.soModel.NetSalesRealization = $scope.soModel.SalesExpense - $scope.soModel.Discount;
    }

    // #region Split Sales Order

    $scope.soSplitModel = {
        Id: null
        , MasterOrderItemId: $scope.masterItemId
        , DeliveryDate: null
        , CommitmentDate: null
        , DestinationId: null
        , ShipmentModeId: null
        , ReviseDate: null
        , CustomerPOId: null
        , PONumber: null
        , UpCharge: null
        , OrderStatusId: $scope.fileNew.OrderStatusId
        , OrderCategoryId: $scope.fileNew.OrderCategoryId
        , SOType: null
        , ResponsiblePersonId: $scope.ResponsiblePersonId
        , ResponsiblePersonName: $scope.ResponsiblePersonName
        , Qty: null
        , Rate: null
        , HSNCodeId: $scope.HSNCodeId
        , TotalTaxAmount: 0
        , ParentId: null
        , Discount: null
        , LSD: null
        , MainRawMaterialInhouseDate: null
        , OtherRawMaterialInhouseDate: null
        , CM: 0
        , SalesOrderYear: null
        , WeekNo: null
        , ProductionBookedQty: null
        , ProductionBookingLevel: null
        , ProductionBookedQty: null
        , OrderStatusChangedBy: null
        , OrderStatusChangedDate: null
        , OrderStatusChangedFromIP: null
        , DestinationDescription: null
        , ProductionType: null
        , ShipmentFromStock: null
        , StockResponsiblePersonId: null
        , StockResponsiblePerson: null
        , PackingTypeId: null
        , ContractId: null
        , ContractNo: null
        , CheckByDate: null, CheckByStatus: 'To Be Check', ApproveBy: null, ApproveByDate: null, ApprovedStatus: null, DeliveryGroup: null, BillDiscountingDays: 0
    };

    $scope.SplitSO = function (data) {
        $scope.soSplitModel.Id = null
        $scope.soSplitModel.MasterOrderItemId = $scope.masterItemId
        $scope.soSplitModel.DeliveryDate = data.DeliveryDate;
        $scope.soSplitModel.DestinationId = data.DestinationId;
        $scope.soSplitModel.CommitmentDate = data.CommitmentDate;
        $scope.soSplitModel.ShipmentModeId = data.ShipmentModeId;
        $scope.soSplitModel.ReviseDate = data.ReviseDate;
        $scope.soSplitModel.CustomerPOId = data.CustomerPOId;
        $scope.soSplitModel.UpCharge = data.UpCharge;
        $scope.soSplitModel.OrderStatusId = data.OrderStatusId
        $scope.soSplitModel.OrderCategoryId = data.OrderCategoryId
        $scope.soSplitModel.SOType = data.SOType;
        $scope.soSplitModel.ResponsiblePersonId = data.ResponsiblePersonId;
        $scope.soSplitModel.ResponsiblePersonName = data.ResponsiblePersonName;
        $scope.soSplitModel.Qty = 0;
        $scope.soSplitModel.Rate = data.Rate;
        $scope.soSplitModel.IsFirstEntry = data.IsFirstEntry;
        $scope.soSplitModel.PONumber = data.PONumber;
        $scope.soSplitModel.HSNCodeId = $scope.HSNCodeId
        $scope.soSplitModel.ConfirmDate = data.ConfirmDate;
        $scope.soSplitModel.IsConfirm = data.IsConfirm;
        $scope.soSplitModel.ConfirmationEntryDate = data.ConfirmationEntryDate;
        $scope.soSplitModel.ConfirmationEntryBy = data.ConfirmationEntryBy;
        $scope.soSplitModel.TotalTaxAmount = data.TotalTaxAmount;
        $scope.soSplitModel.ParentId = data.Id;
        $scope.soSplitModel.ParentQty = data.Qty;
        $scope.soSplitModel.Discount = data.Discount;
        $scope.soSplitModel.LSD = data.LSD;
        $scope.soSplitModel.MainRawMaterialInhouseDate = data.MainRawMaterialInhouseDate;
        $scope.soSplitModel.OtherRawMaterialInhouseDate = data.OtherRawMaterialInhouseDate;
        $scope.soSplitModel.Reason = data.Reason;
        $scope.soSplitModel.CM = data.CM;
        $scope.soSplitModel.OrderCostingMasterTemplateId = data.OrderCostingMasterTemplateId;
        $scope.soSplitModel.SalesOrderYear = data.SalesOrderYear;
        $scope.soSplitModel.WeekNo = data.WeekNo;
        $scope.soSplitModel.Description = data.Description;
        $scope.soSplitModel.PlanExFactoryDate = data.PlanExFactoryDate;
        $scope.soSplitModel.OrderStatus = data.OrderStatus;
        $scope.soSplitModel.ProductionBookingLevel = data.ProductionBookingLevel;
        $scope.soSplitModel.ProductionBookedQty = data.ProductionBookedQty;
        $scope.soSplitModel.OrderStatusChangedBy = data.OrderStatusChangedBy;
        $scope.soSplitModel.OrderStatusChangedDate = data.OrderStatusChangedDate;
        $scope.soSplitModel.OrderStatusChangedFromIP = data.OrderStatusChangedFromIP;
        $scope.soSplitModel.ContractId = data.ContractId;
        $scope.soSplitModel.ContractNo = data.ContractNo;
        $scope.soSplitModel.CheckByDate = data.CheckByDate;
        $scope.soSplitModel.CheckByStatus = data.CheckByStatus;
        $scope.soSplitModel.ApproveBy = data.ApproveBy;
        $scope.soSplitModel.ApproveByDate = data.ApproveByDate;
        $scope.soSplitModel.ApprovedStatus = data.ApprovedStatus;
        $scope.soSplitModel.DeliveryGroup = data.DeliveryGroup;
        $scope.soSplitModel.BillDiscountingDays = data.BillDiscountingDays;
        angular.element(document.querySelector('#soSplitPoUp')).modal('show');
    }

    $scope.closeSplitSOPopUp = function () {
        angular.element(document.querySelector('#soSplitPoUp')).modal('hide');
    };

    $scope.saveSplitSalesOrder = function () {

        if ($scope.soSplitModel.Qty <= 0) {
            ShowResult("Sales order split quantity can't be zero", 'failure', 'soSplitPoUp');
            return false;
        }

        if ($scope.soSplitModel.ParentQty <= $scope.soSplitModel.Qty) {
            ShowResult("Sales order split quantity '" + $scope.soSplitModel.Qty + "' can't greater than or equal Parent quantity '" + $scope.soSplitModel.ParentQty + "'", 'failure', 'soSplitPoUp');
            return false;
        }

        if ($scope.soSplitModel.Rate < $scope.soSplitModel.Discount) {
            ShowResult("Sales order split discount can't greater than Rate", 'failure', 'soSplitPoUp');
            return false;
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.soSplitForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.soSplitModel.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'CreateSplitSalesOrder'
                    , data: {
                        'masterItemId': $scope.masterItemId
                        , 'salesOrderMaster': $scope.soSplitModel
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'soSplitPoUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'soSplitPoUp');
                        getSalesOrderList();
                        $scope.getMasterItemList();
                        angular.element(document.querySelector('#soSplitPoUp')).modal('hide');
                        $scope.popCode('success', response.data.Message);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'soSplitPoUp');
                };
            }
            //else {
            //    getSalesOrderTaxCategoryUpdateList($scope.soModel.Id);
            //}
        }
    };

    // #region Toaster

    $scope.popCode = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 3000
        });
    };
    // #endregion

    // #endregion Split Sales Order

    // #endregion Sales Order

    // #region Sales Order Tax

    $scope.TaxAction = 'Save';

    accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
        $scope.taxCategoryList = result;
    });

    $scope.addTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null,
            SpecialTaxId: null
        };
        $scope.taxList.push(data);

    };

    $scope.getTaxCategoryList = function (data, index) {
        $scope.total = 0;
        $scope.SoTotalAmount = 0;
        if (baseService.isUndefinedOrNull($scope.HSNCodeId)) {
            $scope.HSNCodeId = $scope.soModel.HSNCodeId;
        }
        $scope.salesOrderId = data.Id;
        $scope.taxList = [];
        $scope.soIndex = index;
        $scope.STA = (parseFloat(data.Qty) * parseFloat(data.Rate)) - parseFloat(data.Discount);
        $scope.SoTotalAmount = ($scope.STA).toFixed(2);
        if (data.isTax === 0) {
            $http({
                method: 'GET'
                , url: $scope.path + 'GetTaxCategoryList?masterOrderId=' + $scope.fileNew.Id + '&plantId=' + $scope.fileNew.PlantId + '&hsnCodeId=' + $scope.HSNCodeId + '&specialTaxId=' + $scope.fileNew.SpecialTaxId
            }).then(function (response) {
                $scope.taxList = response.data;
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.HSNCode = response.data[0]['HSNCode'];
                }
                for (var i = 0; i < baseService.arrayLength($scope.taxList); i++) {
                    $scope.taxList[i].TaxAmount = $scope.SoTotalAmount * parseFloat($scope.taxList[i].Percentage) / 100;
                }
                $scope.TaxAction = 'Save';
            });
        }
        else {
            getSalesOrderTaxCategoryList($scope.salesOrderId);
            $scope.TaxAction = 'Update';
        }
        angular.element(document.querySelector('#taxPopup')).modal('show');
    };

    $scope.onchangeFunction = function (id) {
        $scope.TaxCategoryId = id;
        var getRow = $filter("filter")($scope.taxList, { "TaxCategoryId": id });
        if (getRow.length == 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'taxPopup');
        }
    };


    $scope.taxSave = function () {
        if (!baseService.isUndefinedOrNull($scope.TaxCategoryId)) {
            var getRow = $filter("filter")($scope.taxList, { "TaxCategoryId": $scope.TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'taxPopup');
                return false;
            }

        }
        for (var i = 0; i < $scope.taxList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.taxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'taxPopup');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.taxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'taxPopup');
                return false;
            }
            if ($scope.taxList[i].Percentage === 0) {
                ShowResult("Percentage must be greater than 0.", 'failure', 'taxPopup');
                return false;
            }
        }
        $http({
            method: 'POST'
            , url: $scope.path + 'CreateSalesOrderTax'
            , data: {
                'salesOrderId': $scope.salesOrderId
                , 'taxCategoryList': $scope.taxList
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'taxPopup');
            }
            else {
                $scope.salesOrderList[$scope.soIndex].isTax = 1;
                $scope.closeTaxPopUp();
                ShowResult(response.data.Message, 'success', 'soPoUp');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'taxPopup');
        };
    };

    $scope.closeTaxPopUp = function () {
        $scope.soIndex = -1;
        $scope.SoTotalAmount = 0;
        angular.element(document.querySelector('#taxPopup')).modal('hide');
    };

    function getSalesOrderTaxCategoryList(salesOrderId) {
        $http({
            method: 'GET'
            , url: $scope.path + 'getSalesOrderTaxCategoryList?salesOrderId=' + salesOrderId
        }).then(function (response) {
            $scope.taxList = response.data;
            if (baseService.arrayLength(response.data) > 0) {
                $scope.HSNCode = response.data[0]['HSNCode'];
            }
            //for (var i = 0; i < baseService.arrayLength($scope.taxList); i++) {
            //    $scope.taxList[i].TaxAmount = $scope.SoTotalAmount * (parseFloat($scope.taxList[i].Percentage) / 100);
            //}
            $scope.total = 0;
            $scope.totals = 0;
            for (var j = 0; j < $scope.taxList.length; j++) {
                $scope.totals = $scope.totals + $scope.taxList[j].TaxAmount;
            }
            $scope.total = $scope.totals.toFixed(2);

        });
    }

    $scope.calculateTaxAmount = function (data) {
        //data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
        data.TaxAmount = Math.round($scope.SoTotalAmount * data.Percentage) / 100;
    };

    $scope.dindex = -1;
    $scope.removeTax = function (id, index) {
        $scope.tempId = id;
        $scope.delindex = index;
        if (baseService.isUndefinedOrNull($scope.tempId))
            $scope.message = 'Are you sure want to delete?';
        else
            $scope.message = 'Are you sure want to delete?';
        angular.element(document.querySelector('#removPopUp')).modal('show');
    };

    $scope.removeTaxRow = function () {
        $scope.Del($scope.tempId, $scope.delindex);
        angular.element(document.querySelector('#removPopUp')).modal('hide');
    };

    $scope.Del = function (id, delindex) {
        $scope.dindex = delindex;
        for (var i = 0; i < $scope.taxList.length; i++) {
            if ($scope.taxList[i].Id === id) {
                $scope.taxList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };

    //#endregion Sales Order Tax

    // #region PO Number

    $scope.getPOSearchData = function () {
        $http({
            method: 'GET'
            , url: $scope.path + 'GetListByMasterOrder/'
            , params: {
                companyId: $window.companyId
                , masterOrderId: $scope.fileNew.Id
            }
        }).then(function successCallback(response) {
            $scope.customerPOlsit = response.data;
            angular.element(document.querySelector('#poSearchPopup')).modal('show');
        });
    };

    $scope.getPOData = function (id, poNumber) {
        $scope.poModel.Id = id;
        $scope.soModel.CustomerPOId = id;
        $scope.soModel.PONumber = poNumber;
        angular.element(document.querySelector('#poSearchPopup')).modal('hide');
    };

    $scope.poModel = {
        Id: null
        , PONumber: null
        , CustomerId: $scope.fileNew.PartyId
        , CompanyGroupId: $window.companyGroupId
        , CompanyId: $window.companyId
        , MasterOrderId: $scope.fileNew.Id
        , PODate: null
        , Active: true
    };
    $scope.poFgEntryPopup = function () {
        if (!baseService.isUndefinedOrNull($scope.soModel.CustomerPOId)) {
            $scope.poModel.Id = $scope.soModel.CustomerPOId;
            $scope.poModel.PONumber = $scope.soModel.PONumber;
            $scope.poModel.PODate = $scope.soModel.PODate;
        } else {
            $scope.poModel = {
                Id: null
                , PONumber: null
                , CustomerId: $scope.fileNew.PartyId
                , CompanyGroupId: $window.companyGroupId
                , CompanyId: $window.companyId
                , MasterOrderId: $scope.fileNew.Id
                , PODate: null
                , Active: true
            };
        }
        angular.element(document.querySelector('#poEntryPopup')).modal('show');
    };

    $scope.SavePO = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.poModel.PONumber)) throw "[PO No] can not be blank...";
            if (baseService.isUndefinedOrNull($scope.poModel.PODate)) throw "[PO Date] can not be blank...";
            if (baseService.isUndefinedOrNull($scope.poModel.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'CreatePO'
                    , data: $scope.poModel
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'poEntryPopup');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'poEntryPopup');

                        $scope.soModel.CustomerPOId = response.data.tuple.Item1;
                        $scope.soModel.PONumber = response.data.tuple.Item2;

                        angular.element(document.querySelector('#poEntryPopup')).modal('hide'); //Hide Detail Add/Edit Modal
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure', 'poEntryPopup');
                });
                return true;
            } else {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'UpdatePO'
                    //, data: $scope.poModel
                    , data: { 'data': $scope.poModel }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'poEntryPopup');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'poEntryPopup');

                        $scope.soModel.CustomerPOId = $scope.poModel.Id;
                        $scope.soModel.PONumber = $scope.poModel.PONumber;

                        angular.element(document.querySelector('#poEntryPopup')).modal('hide'); //Hide Detail Add/Edit Modal
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure', 'poEntryPopup');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure', "poEntryPopup");
        }
    };

    // #endregion PO Number

    //#region Characteristics 

    $scope.clearCharNames = function () {
        $scope.char1 = {};
        $scope.char2 = {};
        $scope.char3 = {};
    };

    $scope.hasFirst = null;
    $scope.getSku = function (salesOrderId, hasFirst, soItemQty) {
        $scope.salesOrderId = salesOrderId;
        $scope.hasFirst = hasFirst;
        $scope.rowName = null;
        $scope.columnName = null;
        $scope.rowNo = null;
        $scope.columnNo = null;
        $scope.clearCharNames();
        $scope.skuList = [];
        $scope.firstSKUList = [];
        $scope.soItemCurentSkuQty = soItemQty;
        if (hasFirst === 0) {
            $http.get($scope.path + 'getcharacteristicsbymaterialmasterid?materialMasterId=' + $scope.materialMasterId)
                .then(function (response) {
                    $scope.characteristicsList = [];
                    $scope.characteristicsList = response.data;
                    if (baseService.arrayLength($scope.characteristicsList) === 1) {
                        $scope.firstSKUList = [];
                        $scope.char1Id = $scope.characteristicsList[0].Value;
                        $scope.char1ValueAssignmentLevel = $scope.characteristicsList[0].ValueAssignmentLevel;

                        $scope.addFirstSkuList();

                        angular.element(document.querySelector('#firstPopup')).modal('show');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 2) {
                        $scope.rowName = $scope.characteristicsList[0].Text;
                        $scope.colorCharacteristicsId = $scope.characteristicsList[0].Value;
                        $scope.columnName = $scope.characteristicsList[1].Text;
                        $scope.sizeCharacteristicsId = $scope.characteristicsList[1].Value;

                        $scope.char1Id = $scope.characteristicsList[0].Value;
                        $scope.char2Id = $scope.characteristicsList[1].Value;
                        $scope.char1ValueAssignmentLevel = $scope.characteristicsList[0].ValueAssignmentLevel;
                        $scope.char2ValueAssignmentLevel = $scope.characteristicsList[1].ValueAssignmentLevel;

                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');

                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');
                        angular.element(document.querySelector('#fourthPopup')).modal('show');

                        $scope.rowNo = 1;
                        $scope.columnNo = 1;
                        $scope.generate();
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 3) {
                        $scope.rowName = $scope.characteristicsList[1].Text;
                        $scope.columnName = $scope.characteristicsList[2].Text;
                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        generateCharPopUp();
                    }
                    if (baseService.arrayLength($scope.characteristicsList) !== 0) {
                        $scope.char1 = {
                            Id: $scope.characteristicsList[0].Value
                            , Name: $scope.characteristicsList[0].Text
                            , CharacteristicsValueId: $scope.characteristicsList[0].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[0].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                            , FirstCharacteristicsId: null
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 1) {
                        $scope.char2 = {
                            Id: $scope.characteristicsList[1].Value
                            , Name: $scope.characteristicsList[1].Text
                            , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[1].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 2) {
                        $scope.char3 = {
                            Id: $scope.characteristicsList[2].Value
                            , Name: $scope.characteristicsList[2].Text
                            , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[2].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };
                    }

                });
        }
        else {
            $http.get($scope.path + 'getAllSkuSalesOrderId?salesOrderId=' + salesOrderId)
                .then(function (response) {
                    var firstData = response.data.firstData;
                    var secondtData = response.data.secondtData;
                    var thirdData = response.data.thirdData;
                    $scope.characteristicsList = [];

                    if (baseService.arrayLength(firstData) > 0) {
                        $scope.characteristicsList.push({
                            Value: firstData[0].CharacteristicsId
                            , Text: firstData[0].CharacteristicsName
                            , CharacteristicsValueId: null //firstData[0].CharacteristicsValueId
                            , ValueFreeText: null //firstData[0].ValueFreeText
                            , ValueAssignmentLevel: firstData[0].ValueAssignmentLevel
                            , MaterialMasterId: firstData[0].MaterialMasterId
                            , Qty: null //firstData[0].Qty
                            , FirstCharacteristicsId: null //firstData[0].Id
                        });
                    }
                    if (baseService.arrayLength(secondtData) > 0) {
                        $scope.characteristicsList.push({
                            Value: secondtData[0].CharacteristicsId
                            , Text: secondtData[0].CharacteristicsName
                            , CharacteristicsValueId: secondtData[0].CharacteristicsValueId
                            , ValueFreeText: secondtData[0].ValueFreeText
                            , ValueAssignmentLevel: secondtData[0].ValueAssignmentLevel
                            , MaterialMasterId: secondtData[0].MaterialMasterId
                            , Qty: secondtData[0].Qty
                        });
                    }
                    if (baseService.arrayLength(thirdData) > 0) {
                        $scope.characteristicsList.push({
                            Value: thirdData[0].CharacteristicsId
                            , Text: thirdData[0].CharacteristicsName
                            , CharacteristicsValueId: thirdData[0].CharacteristicsValueId
                            , ValueFreeText: thirdData[0].ValueFreeText
                            , ValueAssignmentLevel: thirdData[0].ValueAssignmentLevel
                            , MaterialMasterId: thirdData[0].MaterialMasterId
                            , Qty: thirdData[0].Qty
                        });
                    }

                    if (baseService.arrayLength($scope.characteristicsList) !== 0) {
                        $scope.char1 = {
                            Id: $scope.characteristicsList[0].Value
                            , Name: $scope.characteristicsList[0].Text
                            , CharacteristicsValueId: null //$scope.characteristicsList[0].CharacteristicsValueId
                            , ValueFreeText: null //$scope.characteristicsList[0].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null //$scope.characteristicsList[0].Qty
                            , FirstCharacteristicsId: null //$scope.characteristicsList[0].FirstCharacteristicsId
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 1) {
                        $scope.char2 = {
                            Id: $scope.characteristicsList[1].Value
                            , Name: $scope.characteristicsList[1].Text
                            , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[1].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 2) {
                        $scope.char3 = {
                            Id: $scope.characteristicsList[2].Value
                            , Name: $scope.characteristicsList[2].Text
                            , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[2].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };

                    }

                    if (baseService.arrayLength($scope.characteristicsList) === 3) {
                        $scope.firstSkuEdit(firstData[0]);
                        getSkuMatrix(secondtData, thirdData);
                        $scope.rowName = $scope.characteristicsList[1].Text;
                        $scope.columnName = $scope.characteristicsList[2].Text;

                        $scope.char1Id = $scope.characteristicsList[1].Value;
                        $scope.char2Id = $scope.characteristicsList[2].Value;

                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('show');
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 2) {
                        getSkuMatrix(firstData, secondtData);
                        $scope.rowName = $scope.characteristicsList[0].Text;
                        $scope.columnName = $scope.characteristicsList[1].Text;

                        $scope.char1Id = $scope.characteristicsList[0].Value;
                        $scope.char2Id = $scope.characteristicsList[1].Value;

                        $scope.char1ValueAssignmentLevel = $scope.characteristicsList[0].ValueAssignmentLevel;
                        $scope.char2ValueAssignmentLevel = $scope.characteristicsList[1].ValueAssignmentLevel;

                        //angular.element(document.querySelector('#firstPopup')).modal('hide');
                        //angular.element(document.querySelector('#secondPopup')).modal('show');
                        //angular.element(document.querySelector('#thirdPopup')).modal('hide');

                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');
                        angular.element(document.querySelector('#fourthPopup')).modal('show');
                        $scope.sumTwoMatQuantity();
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 1) {
                        $scope.firstSKUList = firstData;
                        angular.element(document.querySelector('#firstPopup')).modal('show');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');
                    }




                });
        }

    };

    function generateCharPopUp() {
        angular.element(document.querySelector('#generatePopup')).modal('show');
    }

    $scope.generate = function () {
        var firstCharId = '';


        $http.get($scope.path + 'GetChValueCbo?materialId=' + $scope.materialMasterId)
            .then(function (response) {

                $scope.charValueList = [];
                $scope.char1ValueList = [];
                $scope.char2ValueList = [];
                $scope.charValueList = response.data;

                $scope.char1ValueList = $filter("filter")($scope.charValueList, { "CharacteristicsId": $scope.char1Id });
                $scope.char2ValueList = $filter("filter")($scope.charValueList, { "CharacteristicsId": $scope.char2Id });

                for (var i = 0; i < $scope.rowNo; i++) {
                    firstCharId = '-' + (i + 1);
                    $scope.skuList.push(
                        {
                            Id: firstCharId
                            , SalesOrderId: $scope.salesOrderId
                            , FirstCharacteristicsId: null
                            , SecondCharacteristicsId: null
                            , CharacteristicsId: $scope.colorCharacteristicsId
                            , CharacteristicsValueId: null
                            , ValueFreeText: null
                            , Sequence: i + 1
                            , Qty: null
                            , childList: []
                            , Flag: null
                        }
                    );
                    for (var t = 0; t < $scope.columnNo; t++) {
                        $scope.skuList[i].childList.push(
                            {
                                Id: null
                                , SalesOrderId: $scope.salesOrderId
                                , FirstCharacteristicsId: baseService.arrayLength($scope.characteristicsList) === 2 ? firstCharId : null
                                , SecondCharacteristicsId: baseService.arrayLength($scope.characteristicsList) === 3 ? firstCharId : null
                                , CharacteristicsId: $scope.sizeCharacteristicsId
                                , CharacteristicsValueId: null
                                , ValueFreeText: null
                                , Sequence: t + 1
                                , Qty: null
                            }
                        );
                    }
                }


            });


        angular.element(document.querySelector('#generatePopup')).modal('hide');
        if (baseService.arrayLength($scope.characteristicsList) === 3)
            angular.element(document.querySelector('#thirdPopup')).modal('show');
        else
            angular.element(document.querySelector('#secondPopup')).modal('show');
    };

    function getSkuMatrix(rowDataList, columnDataList) {
        $http.get($scope.path + 'GetChValueCbo?materialId=' + $scope.materialMasterId)
            .then(function (response) {

                $scope.charValueList = [];
                $scope.char1ValueList = [];
                $scope.char2ValueList = [];
                $scope.charValueList = response.data;

                $scope.char1ValueList = $filter("filter")($scope.charValueList, { "CharacteristicsId": $scope.char1Id });
                $scope.char2ValueList = $filter("filter")($scope.charValueList, { "CharacteristicsId": $scope.char2Id });

                for (var i = 0; i < baseService.arrayLength(rowDataList); i++) {
                    $scope.skuList.push({
                        Id: rowDataList[i].Id
                        , SalesOrderId: rowDataList[i].SalesOrderId
                        , FirstCharacteristicsId: rowDataList[i].FirstCharacteristicsId
                        , SecondCharacteristicsId: rowDataList[i].SecondCharacteristicsId
                        , CharacteristicsId: rowDataList[i].CharacteristicsId
                        , CharacteristicsValueId: rowDataList[i].CharacteristicsValueId
                        , ValueFreeText: rowDataList[i].ValueFreeText
                        , Sequence: rowDataList[i].Sequence
                        , Qty: rowDataList[i].Qty
                        , childList: []
                        , Flag: null
                    });
                    for (var t = 0; t < baseService.arrayLength(columnDataList); t++) {
                        if (columnDataList[t].FirstCharacteristicsId === rowDataList[i].Id || columnDataList[t].SecondCharacteristicsId === rowDataList[i].Id) {
                            $scope.skuList[i].childList.push({
                                Id: columnDataList[t].Id
                                , SalesOrderId: columnDataList[t].SalesOrderId
                                , FirstCharacteristicsId: columnDataList[t].FirstCharacteristicsId
                                , SecondCharacteristicsId: columnDataList[t].SecondCharacteristicsId
                                , CharacteristicsId: columnDataList[t].CharacteristicsId
                                , CharacteristicsValueId: columnDataList[t].CharacteristicsValueId
                                , ValueFreeText: columnDataList[t].ValueFreeText
                                , Sequence: columnDataList[t].Sequence
                                , Qty: columnDataList[t].Qty
                            });
                        }
                    }
                }

                $scope.sumTwoMatQuantity();
            });

    }

    $scope.addSkuMatrixColumn = function () {
        var t = 0;

        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
            $scope.skuList[i].childList.push({
                Id: null
                , SalesOrderId: $scope.salesOrderId
                , FirstCharacteristicsId: null
                , SecondCharacteristicsId: null
                , CharacteristicsId: $scope.char2.Id
                , CharacteristicsValueId: null
                , ValueFreeText: null
                , Sequence: i + 1
                , Qty: null
            });
        }
    }

    $scope.removeSkuMatrixColumn = function (index) {
        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
            $scope.skuList[i].childList.splice(index, 1);
        }
        $scope.verifySkuMatrix();
    }

    $scope.addSkuMatrixRow = function () {
        var skuChildList = [];
        for (var i = 0; i < baseService.arrayLength($scope.skuList[0].childList); i++) {
            skuChildList.push({
                Id: null
                , SalesOrderId: $scope.salesOrderId
                , FirstCharacteristicsId: null
                , SecondCharacteristicsId: null
                , CharacteristicsId: $scope.char2.Id
                , CharacteristicsValueId: $scope.skuList[0].childList[i].CharacteristicsValueId
                , ValueFreeText: null
                , Sequence: $scope.skuList[0].childList[i].Sequence
                , Qty: null
            });
        }

        $scope.skuList.push({
            Id: '-' + (baseService.arrayLength($scope.skuList) + 1)
            , SalesOrderId: $scope.salesOrderId
            , FirstCharacteristicsId: null
            , SecondCharacteristicsId: null
            , CharacteristicsId: $scope.char1.Id
            , CharacteristicsValueId: $scope.char1.CharacteristicsValueId
            , Sequence: (baseService.arrayLength($scope.skuList) + 1)
            , ValueFreeText: $scope.char1.ValueFreeText
            , Qty: null
            , Flag: '1st'
            , childList: skuChildList
        });
    }

    $scope.removeSkuMatrixRow = function (index) {
        $scope.skuList.splice(index, 1);
        $scope.verifySkuMatrix();
    }

    $scope.verifySkuMatrix = function () {

        $scope.IsSkuColumnIsValid = true;
        if ($scope.skuList[0].childList.length > 1) {
            for (var i = 0; i < $scope.skuList[0].childList.length; i++) {
                var count = 0;
                var iSKU = $scope.skuList[0].childList[i];
                for (var j = 0; j < $scope.skuList[0].childList.length; j++) {
                    var skuChild = $scope.skuList[0].childList[j];
                    //if (skuChild.ValueFreeText != null && skuChild.CharacteristicsValueId != null && iSKU.ValueFreeText != null && iSKU.CharacteristicsValueId != null)
                    //    if (iSKU.ValueFreeText.toLowerCase() == skuChild.ValueFreeText.toLowerCase() && iSKU.CharacteristicsValueId == skuChild.CharacteristicsValueId) {
                    //        count++;
                    //    }

                    if (skuChild.ValueFreeText != null || skuChild.CharacteristicsValueId != null || iSKU.ValueFreeText != null || iSKU.CharacteristicsValueId != null)
                        if (!baseService.isUndefinedOrNull(iSKU.ValueFreeText) || !baseService.isUndefinedOrNull(skuChild.ValueFreeText)) {
                            if (iSKU.ValueFreeText.toLowerCase() == skuChild.ValueFreeText.toLowerCase() || iSKU.CharacteristicsValueId == skuChild.CharacteristicsValueId) {
                                count++;
                            }
                        }
                        else {
                            if (iSKU.CharacteristicsValueId == skuChild.CharacteristicsValueId) {
                                count++;
                            }
                        }
                }

                if (count > 1) {
                    $scope.skuList[0].childList[i].isDuplicate = true;
                }
                else {
                    $scope.skuList[0].childList[i].isDuplicate = false;
                }
            }
            if (findWithAttr($scope.skuList[0].childList, 'isDuplicate', true) >= 0) {
                $scope.IsSkuColumnIsValid = false;
            }
            else {
                $scope.IsSkuColumnIsValid = true;
            }
        }

        $scope.IsSkuRowIsValid = true;
        if ($scope.skuList.length > 1) {
            for (var i = 0; i < $scope.skuList.length; i++) {
                var count = 0;
                var iSKU = $scope.skuList[i];
                for (var j = 0; j < $scope.skuList.length; j++) {
                    var skuChild = $scope.skuList[j];
                    if (skuChild.ValueFreeText != null && skuChild.CharacteristicsValueId != null && iSKU.ValueFreeText != null && iSKU.CharacteristicsValueId != null)
                        if (iSKU.ValueFreeText.toLowerCase() == skuChild.ValueFreeText.toLowerCase() && iSKU.CharacteristicsValueId == skuChild.CharacteristicsValueId) {
                            count++;
                        }
                }

                if (count > 1) {
                    $scope.skuList[i].isDuplicate = true;
                }
                else {
                    $scope.skuList[i].isDuplicate = false;
                }

            }
            if (findWithAttr($scope.skuList, 'isDuplicate', true) >= 0) {
                $scope.IsSkuRowIsValid = false;
            }
            else {
                $scope.IsSkuRowIsValid = true;
            }
        }
    }

    function findWithAttr(array, attr, value) {
        for (var i = 0; i < array.length; i += 1) {
            if (array[i][attr] === value) {
                return i;
            }
        }
        return -1;
    }

    $scope.charSave = function (charLength) {
        //if (baseService.arrayLength($scope.characteristicsList) > 2 || baseService.arrayLength($scope.characteristicsList) === 1) {
        //    var data = $filter('filter')($scope.skuList, { Flag: '1st' }, true);
        //    var qty = 0;
        //    if (baseService.arrayLength($scope.characteristicsList) > 2) qty = parseFloat($filter('sumByKey')($scope.skuList, 'Qty', true));
        //    if (baseService.arrayLength($scope.characteristicsList) === 1) qty = $scope.char1.Qty;
        //    if (baseService.arrayLength(data) === 0) {
        //        $scope.skuList.unshift({
        //            Id: $scope.char1.FirstCharacteristicsId
        //            , SalesOrderId: $scope.salesOrderId
        //            , FirstCharacteristicsId: null
        //            , SecondCharacteristicsId: null
        //            , CharacteristicsId: $scope.char1.Id
        //            , CharacteristicsValueId: $scope.char1.CharacteristicsValueId
        //            , Sequence: 1
        //            , ValueFreeText: $scope.char1.ValueFreeText
        //            , Qty: qty
        //            , Flag: '1st'
        //            , childList: []
        //        });
        //    }
        //    else if (baseService.arrayLength(data) === 1) {
        //        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
        //            if ($scope.skuList[i].CharacteristicsId === $scope.char1.Id) {
        //                $scope.skuList[i].CharacteristicsValueId = $scope.char1.CharacteristicsValueId;
        //                $scope.skuList[i].ValueFreeText = $scope.char1.ValueFreeText;
        //                $scope.skuList[i].Qty = qty;
        //            }
        //        }
        //    }
        //}

        if (charLength == 1) {
            var totlQty = 0;
            for (var i = 0; i < baseService.arrayLength($scope.firstSKUList); i++) {
                if (($scope.firstSKUList[i].ValueFreeText == null || $scope.firstSKUList[i].ValueFreeText == "") && $scope.firstSKUList[i].CharacteristicsValueId == null) {
                    ShowResult("SKU item can't be blank", 'failure', 'firstPopup');
                    return false;
                }
                $scope.firstSKUList[i].IsQtyZero = $scope.firstSKUList[i].Qty <= 0;
                if ($scope.firstSKUList[i].IsQtyZero) {
                    ShowResult("SKU quantity can't be zero", 'failure', 'firstPopup');
                    return false;
                }
                totlQty = totlQty + $scope.firstSKUList[i].Qty;
            }
            if (totlQty > $scope.soItemCurentSkuQty) {
                ShowResult("Sum of SKU quantity can't be greater than " + $scope.soItemCurentSkuQty, 'failure', 'firstPopup');
                return false;
            }

            if (!$scope.IsSkuFormIsValid) {
                ShowResult("Duplicate data", 'failure', 'firstPopup');
                return false;
            }

            $scope.skuList = $scope.firstSKUList;

        }
        else {
            if ($scope.skuList.length == 1) {
                for (var i = 0; i < $scope.skuList[0].childList.length; i++) {
                    if ($scope.skuList[0].childList[i].Qty <= 0) {
                        ShowResult("SKU quantity can't be zero", 'failure', 'fourthPopup');
                        return false;
                    }
                }
            }

            if ($scope.skuList.length >= 1) {

                for (var j = 0; j < $scope.skuList.length; j++) {

                    var skuPar = $scope.skuList[j];
                    if ((skuPar.ValueFreeText == null || skuPar.ValueFreeText == "") && skuPar.CharacteristicsValueId == null) {
                        ShowResult("SKU item can't be blank", 'failure', 'fourthPopup');
                        return false;
                    }
                    if ($scope.skuList[j].childList.length > 1 && j == 0) {
                        for (var i = 0; i < $scope.skuList[j].childList.length; i++) {
                            var skuChild = $scope.skuList[j].childList[i];
                            if ((skuChild.ValueFreeText == null || skuChild.ValueFreeText == "") && skuChild.CharacteristicsValueId == null) {
                                ShowResult("SKU item can't be blank", 'failure', 'fourthPopup');
                                return false;
                            }

                        }
                    }
                }
            }



            if (baseService.arrayLength($scope.skuList) > 0) {
                $scope.verifySkuMatrix();
                if (!$scope.IsSkuColumnIsValid || !$scope.IsSkuRowIsValid) {
                    ShowResult("Duplicate data", 'failure', 'fourthPopup');
                    return false;
                }
            }

        }

        for (var j = 0; j < $scope.skuList.length; j++) {
            if (baseService.arrayLength($scope.skuList[j].childList) > 0) {
                $scope.skuList[j].Qty = 0;
                for (var i = 0; i < $scope.skuList[j].childList.length; i++) {
                    $scope.skuList[j].Qty += $scope.skuList[j].childList[i].Qty;
                }
            }
        }


        $http({
            method: 'POST'
            , url: $scope.path + 'CreateCharacteristics'
            , data: {
                'entities': $scope.skuList
                , 'listLength': charLength
                , 'soId': $scope.salesOrderId
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                if (baseService.arrayLength($scope.characteristicsList) == 1) {
                    ShowResult(response.data.Message, 'failure', 'firstPopup');
                }
                else {
                    $scope.closeCharPopUp();
                    showCharMessage(response.data.Message, 'failure');
                }
            }
            else {
                if (baseService.arrayLength($scope.characteristicsList) === 1) {
                    getFirstSkuList($scope.salesOrderId);
                    $scope.char1.FirstCharacteristicsId = null;
                    $scope.char1.CharacteristicsValueId = null;
                    $scope.char1.ValueFreeText = null;
                    $scope.char1.CharacteristicsValueName = null;
                    $scope.char1.Qty = null;
                    if (baseService.arrayLength($scope.firstSKUList) === 1) {
                        for (var i = 0; i < baseService.arrayLength($scope.salesOrderList); i++) {
                            if ($scope.salesOrderId === $scope.salesOrderList[i].Id) {
                                $scope.salesOrderList[i].hasFirst = 1;
                                break;
                            }
                        }
                    }
                    showCharMessage(response.data.Message, 'success');
                    $scope.getSalesOrder($scope.masterItemId, $scope.materialMasterId, $scope.mName, $scope.BuyerReferenceNo);
                }
                else {
                    angular.element(document.querySelector('#firstPopup')).modal('hide');
                    angular.element(document.querySelector('#secondPopup')).modal('hide');
                    angular.element(document.querySelector('#thirdPopup')).modal('hide');
                    for (var t = 0; t < baseService.arrayLength($scope.salesOrderList); t++) {
                        if ($scope.salesOrderId === $scope.salesOrderList[t].Id) {
                            $scope.salesOrderList[t].hasFirst = 1;
                            break;
                        }
                    }
                    $scope.salesOrderId = null;
                    $scope.skuList = [];
                    $scope.closeCharPopUp();
                    showCharMessage(response.data.Message, 'success');
                    //$scope.getSalesOrder($scope.masterItemId, $scope.materialMasterId, $scope.mName, $scope.BuyerReferenceNo);
                }

            }
        }), function errorCallBack(response) {
            showCharMessage(response.data.Message, 'failure');
        };
    };

    function showCharMessage(message, state) {
        if (baseService.arrayLength($scope.characteristicsList) === 3) ShowResult(message, state, 'thirdPopup');
        if (baseService.arrayLength($scope.characteristicsList) === 2) ShowResult(message, state, 'soPoUp');
        if (baseService.arrayLength($scope.characteristicsList) === 1) ShowResult(message, state, 'firstPopup');
    }

    $scope.sumQty = function (parentData, parentIndex) {
        var tqty = parseFloat($filter('sumByKey')(parentData.childList, 'Qty', true));
        parentData.Qty = isNaN(tqty) ? 0 : parseFloat(tqty);
        $scope.sumTwoMatQuantity();
    };

    $scope.skuTwoMatQuantity = 0;
    $scope.sumTwoMatQuantity = function () {
        var tqty = parseFloat(0);
        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
            for (var j = 0; j < baseService.arrayLength($scope.skuList[i].childList); j++) {
                tqty = tqty + $scope.skuList[i].childList[j].Qty;
            }
        }
        $scope.skuTwoMatQuantity = isNaN(tqty) ? 0 : parseFloat(tqty);
        if ($scope.skuTwoMatQuantity > $scope.soItemCurentSkuQty) {
            ShowResult("Sum of SKU quantity can't be greater than " + $scope.soItemCurentSkuQty, 'failure', 'fourthPopup');
        }

    };

    //$scope.chvChange = function (characteristicsValueId, index) {
    //    for (var i = 1; i < baseService.arrayLength($scope.skuList); i++) {
    //        $scope.skuList[i].childList[index].CharacteristicsValueId = characteristicsValueId;
    //    }
    //};

    $scope.chvChange = function (args) {
        if (!args.isInteraction)
            return;
        for (var i = 1; i < baseService.arrayLength($scope.skuList); i++) {
            //$scope.skuList[i].childList[index].CharacteristicsValueId = args.selectedValue;
            $scope.skuList[i].childList[args.model.name].CharacteristicsValueId = args.selectedValue;
        }
        $scope.verifySkuMatrix();
    };


    $scope.chvKeyChange = function (value, index) {
        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
            $scope.skuList[i].childList[index].ValueFreeText = value;
        }
    };

    $scope.closeCharPopUp = function () {
        $scope.firstSKUList = [];
        $scope.skuList = [];
        angular.element(document.querySelector('#firstPopup')).modal('hide');
        angular.element(document.querySelector('#secondPopup')).modal('hide');
        angular.element(document.querySelector('#thirdPopup')).modal('hide');
        angular.element(document.querySelector('#fourthPopup')).modal('hide');
    };

    $scope.firstSkuEdit = function (data) {
        $scope.char1.FirstCharacteristicsId = data.Id;
        $scope.char1.CharacteristicsValueId = data.CharacteristicsValueId;
        $scope.char1.ValueFreeText = data.ValueFreeText;
        $scope.char1.CharacteristicsValueName = data.CharacteristicsValueName;
        $scope.char1.Qty = data.Qty;
    };

    function getFirstSkuList() {
        $http.get($scope.path + 'GetFirstSkuList?salesOrderId=' + $scope.salesOrderId)
            .then(function (response) {
                $scope.firstSKUList = [];
                $scope.firstSKUList = response.data;
            });
    }

    $scope.firstSKUList = [];
    $scope.addFirstSkuList = function () {
        $http.get($scope.path + 'GetChValueCbo?materialId=' + $scope.materialMasterId)
            .then(function (response) {

                $scope.charValueList = [];
                $scope.char1ValueList = [];
                $scope.charValueList = response.data;

                $scope.char1ValueList = $filter("filter")($scope.charValueList, { "CharacteristicsId": $scope.char1Id });

                $scope.firstSKUList.push({
                    Id: null
                    , SalesOrderId: $scope.salesOrderId
                    , FirstCharacteristicsId: null
                    , SecondCharacteristicsId: null
                    , CharacteristicsId: 1
                    , CharacteristicsValueId: null
                    , Sequence: (baseService.arrayLength($scope.firstSKUList) + 1)
                    , ValueFreeText: null
                    , Qty: null
                    , Flag: null
                });

            });

    }

    $scope.removeFirstSkuList = function (id, index) {
        $scope.firstSKUList.splice(index, 1);
        $scope.verifyFirstSkuList();
    }

    $scope.IsSkuFormIsValid = true;
    $scope.verifyFirstSkuList = function () {
        for (var i = 0; i < $scope.firstSKUList.length; i++) {
            var iSKU = $scope.firstSKUList[i];
            var count = 0;
            for (var j = 0; j < $scope.firstSKUList.length; j++) {
                var skuChild = $scope.firstSKUList[j];
                if (iSKU.ValueFreeText == null) {
                    iSKU.ValueFreeText = "";
                }
                if (skuChild.ValueFreeText == null) {
                    skuChild.ValueFreeText = "";
                }

                if (iSKU.ValueFreeText.toLowerCase() == skuChild.ValueFreeText.toLowerCase() && iSKU.CharacteristicsValueId == skuChild.CharacteristicsValueId) {
                    count++;
                }

            }
            if (count > 1) {
                $scope.firstSKUList[i].isDuplicate = true;
                $scope.IsSkuFormIsValid = false;
            }
            else {
                $scope.firstSKUList[i].isDuplicate = false;
                $scope.IsSkuFormIsValid = true;
            }

        }
    }

    $scope.firstSkuClosePopUp = function () {
        $scope.skuList = [];
        $scope.firstSKUList = [];
        $scope.salesOrderId = null;
        angular.element(document.querySelector('#firstPopup')).modal('hide');
        angular.element(document.querySelector('#secondPopup')).modal('hide');
        angular.element(document.querySelector('#thirdPopup')).modal('hide');
    };

    //#endregion Characteristics 

    //#region Generic

    $scope.genericDelete = function (id, flag) {
        $scope.id = id;
        $scope.message_confirmation = "Are you sure want to permanently delete ";
        angular.element(document.querySelector('#genericConfirm')).modal('show');
        $scope.flag = flag;
    };

    $scope.genericRemove = function () {
        if ($scope.flag === 'item')
            $scope.deleteItem();
        else if ($scope.flag === 'so')
            $scope.deleteSO();
        else if ($scope.flag === 'first')
            $scope.firstSkuDelete();
    };

    //#endregion Generic

    $scope.FinishingExFactory = 0;
    $scope.FinishingExFactoryProduction = 0;

    $scope.SetOtherDates = function () {
        $http.get("OrderManagements/MasterOrder/GetOrderDateSetting?shipmentModeId=" + $scope.soModel.ShipmentModeId + '&buyerId=' + $scope.fileNew.BuyerId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        var cddate = new Date($scope.soModel.DeliveryDate);
                        var lddate = new Date($scope.soModel.DeliveryDate);
                        var eddate = new Date($scope.soModel.DeliveryDate);

                        $scope.FinishingExFactory = response.data[0].FinishingLeadTime + response.data[0].ExFactoryLeadTime;
                        $scope.FinishingExFactoryProduction = response.data[0].FinishingLeadTime + response.data[0].ExFactoryLeadTime + response.data[0].ProductionLeadTime;

                        $scope.CommitmentDate = cddate.setDate(cddate.getDate() - $scope.FinishingExFactory);
                        $scope.soModel.CommitmentDate = $filter('dateFiltering')(new Date($scope.CommitmentDate), 'dd-MM-yyyy');

                        $scope.LSD = lddate.setDate(lddate.getDate() - $scope.FinishingExFactoryProduction);
                        $scope.soModel.LSD = $filter('dateFiltering')(new Date($scope.LSD), 'dd-MM-yyyy');

                        $scope.ExFactory = eddate.setDate(eddate.getDate() - response.data[0].ExFactoryLeadTime);
                        $scope.soModel.PlanExFactoryDate = $filter('dateFiltering')(new Date($scope.ExFactory), 'dd-MM-yyyy');

                        var mddate = new Date($scope.soModel.LSD);
                        var oddate = new Date($scope.soModel.LSD);

                        $scope.Main = mddate.setDate(mddate.getDate() - response.data[0].MainRawMaterialInhouseLeadTime);
                        $scope.soModel.MainRawMaterialInhouseDate = $filter('dateFiltering')(new Date($scope.Main), 'dd-MM-yyyy');

                        $scope.Other = oddate.setDate(oddate.getDate() - response.data[0].OtherRawMaterialInhouseLeadTime);
                        $scope.soModel.OtherRawMaterialInhouseDate = $filter('dateFiltering')(new Date($scope.Other), 'dd-MM-yyyy');
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.GetDelivaryDate = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.soModel.SalesOrderYear) && !baseService.isUndefinedOrNull($scope.soModel.WeekNo)) {
                $http.get("OrderManagements/MasterOrder/GetDelivaryDate?year=" + $scope.soModel.SalesOrderYear + '&weekNo=' + $scope.soModel.WeekNo + '&buyerId=' + $scope.fileNew.BuyerId)
                    .then(
                        function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                            }
                            else {
                                var DeliveryDate = new Date(response.data);
                                $scope.soModel.DeliveryDate = $filter('dateFiltering')(DeliveryDate, 'dd-M-yyyy');
                            }
                        },
                        function errorCallback(response) {
                            ShowResult(response.data.Message, 'failure');
                        });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.AddDescription = function (data) {
        $scope.masterOrderItem = data;
        angular.element(document.querySelector('#DescriptionPopup')).modal('show');
    }

    $scope.costingSOFormulaList = [];
    $scope.CloseMOSORatePopup = function () {
        angular.element(document.querySelector('#MOSORatePopup')).modal('hide');
    }
    $scope.ProductLibraryList = [];
    $scope.masterItemId = null;
    $scope.GetMOSORatePopup = function (index, data) {
        $scope.itemIndex = index;
        $scope.masterItemId = data.Id;

        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetCostingSOFormulaData?masterOrderItemId=' + $scope.masterItemId
        }).then(function successCallback(response) {
            $scope.costingSOFormulaList = response.data;
            angular.element(document.querySelector('#MOSORatePopup')).modal('show');
        });
    };

    $scope.MOAdditionalInfoList = [];
    $scope.GetMOAdditionalInfoPopup = function (index, data) {
        $scope.itemIndex = index;
        $scope.masterItemId = data.Id;

        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetLineItemAdditionalInfoData?lineItemId=' + $scope.masterItemId
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i].LineItemId = $scope.masterItemId;

                if (response.data[i].CharecterType == "Text" || response.data[i].CharecterType == "DateTime") {
                    response.data[i].CharType = "text";
                }
                else {
                    response.data[i].CharType = "number";
                }
                if (response.data[i].CharecterType == "DateTime") {
                    response.data[i].datepic = 'datepicker';
                }
            }
            $scope.MOAdditionalInfoList = response.data;
            angular.element(document.querySelector('#MOAddInfoPopup')).modal('show');
        });
    };

    function GetLineItemAdditionalInfo() {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetLineItemAdditionalInfoData?lineItemId=' + $scope.masterItemId
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i].LineItemId = $scope.masterItemId;

                if (response.data[i].CharecterType == "Text" || response.data[i].CharecterType == "DateTime") {
                    response.data[i].CharType = "text";
                }
                else {
                    response.data[i].CharType = "number";
                }
                if (response.data[i].CharecterType == "DateTime") {
                    response.data[i].datepic = 'datepicker';
                }
            }
            $scope.MOAdditionalInfoList = response.data;
        });
    }

    $scope.monthList = [
        { 'Value': "1", 'Text': "Jan", 'Days': 31 },
        { 'Value': "2", 'Text': "Feb", 'Days': 28 },
        { 'Value': "3", 'Text': "Mar", 'Days': 31 },
        { 'Value': "4", 'Text': "Apr", 'Days': 30 },
        { 'Value': "5", 'Text': "May", 'Days': 31 },
        { 'Value': "6", 'Text': "Jun", 'Days': 30 },
        { 'Value': "7", 'Text': "Jul", 'Days': 31 },
        { 'Value': "8", 'Text': "Aug", 'Days': 31 },
        { 'Value': "9", 'Text': "Sep", 'Days': 30 },
        { 'Value': "10", 'Text': "Oct", 'Days': 31 },
        { 'Value': "11", 'Text': "Nov", 'Days': 30 },
        { 'Value': "12", 'Text': "Dec", 'Days': 31 }
    ];

    function validatedate(dateText) {

        if (dateText) {
            try {
                var errorMessage = "";
                var monthNO = 0;
                var daysPerMonth = 0;
                var splitComponents = dateText.split('-');
                if (splitComponents.length > 0) {
                    var day = parseInt(splitComponents[0]);
                    var month = splitComponents[1];
                    var year = parseInt(splitComponents[2]);

                    if (isNaN(day) || isNaN(year)) {
                        errorMessage = "Please enter the date in dd-MMM-yyyy format.";
                        throw errorMessage;
                        return false;
                    }

                    var monthName = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                    if (monthName.includes(month)) {
                        for (var i = 0; i < $scope.monthList.length; i++) {
                            if ($scope.monthList[i].Text == month) {
                                monthNO = $scope.monthList[i].Value;
                                daysPerMonth = $scope.monthList[i].Days;
                                break;
                            }
                        }
                    }
                    else {
                        throw "Invalid Month Name.";
                    }

                    if (day <= 0 || year <= 0) {
                        throw "The day and year need to be positive values greater than 0";
                    }

                    if (errorMessage == "") {
                        // assuming no leap year by default
                        //var daysPerMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
                        if (year % 4 == 0) {
                            // current year is a leap year
                            daysPerMonth = 29;
                        }

                        if (day > daysPerMonth) {
                            errorMessage = "Number of days are more than those allowed for the month";
                        }
                    }
                } else {
                    throw errorMessage = "Please enter the date in dd-MMM-yyyy format.";
                }

                if (errorMessage) {
                    throw errorMessage;
                    return false;
                }
            } catch (e) {
                throw e;
                return false;
            }
        }

        return true;
    }


    $scope.SaveAddInfo = function () {
        try {
            for (var i = 0; i < $scope.MOAdditionalInfoList.length; i++) {
                if ($scope.MOAdditionalInfoList[i].Mandatory) {
                    if (baseService.isUndefinedOrNull($scope.MOAdditionalInfoList[i].Value)) {
                        throw "Value is required for " + $scope.MOAdditionalInfoList[i].UserName + ".";
                    }
                }

                if ($scope.MOAdditionalInfoList[i].CharecterType == "DateTime") {
                    validatedate($scope.MOAdditionalInfoList[i].Value);
                }


                if ($scope.MOAdditionalInfoList[i].CharecterType == "Decimal") {
                    if (isNaN($scope.MOAdditionalInfoList[i].Value)) {
                        throw "Number is required for " + $scope.MOAdditionalInfoList[i].UserName + ".";
                    }
                }
            }


            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CreateMOAdditionalInfo',
                data: { 'data': $scope.MOAdditionalInfoList, 'lineId': $scope.masterItemId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetLineItemAdditionalInfo();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SOAdditionalInfoList = [];
    $scope.GetSOAdditionalInfoPopup = function (data) {
        $scope.SOId = data.Id;

        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetSOAdditionalInfoData?SalesOrderId=' + $scope.SOId
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i].SalesOrderId = $scope.SOId;

                if (response.data[i].CharecterType == "Text" || response.data[i].CharecterType == "DateTime") {
                    response.data[i].CharType = "text";
                }
                else {
                    response.data[i].CharType = "number";
                }
                if (response.data[i].CharecterType == "DateTime") {
                    response.data[i].datepic = 'datepicker';
                }
            }
            $scope.SOAdditionalInfoList = response.data;
            angular.element(document.querySelector('#SOAddInfoPopup')).modal('show');
        });
    };

    function GetSOAdditionalInfo() {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetSOAdditionalInfoData?SalesOrderId=' + $scope.SOId
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i].SalesOrderId = $scope.SOId;

                if (response.data[i].CharecterType == "Text" || response.data[i].CharecterType == "DateTime") {
                    response.data[i].CharType = "text";
                }
                else {
                    response.data[i].CharType = "number";
                }
                if (response.data[i].CharecterType == "DateTime") {
                    response.data[i].datepic = 'datepicker';
                }
            }
            $scope.SOAdditionalInfoList = response.data;
        });
    }

    $scope.SaveSOAddInfo = function () {
        try {
            for (var i = 0; i < $scope.SOAdditionalInfoList.length; i++) {
                if ($scope.SOAdditionalInfoList[i].Mandatory) {
                    if (baseService.isUndefinedOrNull($scope.SOAdditionalInfoList[i].Value)) {
                        throw "Value is required for " + $scope.SOAdditionalInfoList[i].UserName + ".";
                    }
                }

                if ($scope.SOAdditionalInfoList[i].CharecterType == "DateTime") {
                    validatedate($scope.SOAdditionalInfoList[i].Value);
                }


                if ($scope.SOAdditionalInfoList[i].CharecterType == "Decimal") {
                    if (isNaN($scope.SOAdditionalInfoList[i].Value)) {
                        throw "Number is required for " + $scope.SOAdditionalInfoList[i].UserName + ".";
                    }
                }
            }


            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CreateSOAdditionalInfo',
                data: { 'data': $scope.SOAdditionalInfoList, 'SOId': $scope.SOId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetSOAdditionalInfo();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.CalculateRate = function () {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CalculateRate',
                data: { 'OpenHeadNew': $scope.costingSOFormulaList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.costingSOFormulaList = [];
                $scope.costingSOFormulaList = response.data.NewData;
                for (var i = 0; i < $scope.costingSOFormulaList.length; i++) {
                    $scope.costingSOFormulaList[i].Value = parseFloat($scope.costingSOFormulaList[i].Value).toFixed(4);
                }
            }, function errorCallback(response) {
                $scope.ShowResultCustom(response.status.Message, "failure");
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveItemCostingRate = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/MasterOrder/CreateMasterOrderItemCostingRate',
            data: { 'data': $scope.costingSOFormulaList, 'lineId': $scope.masterItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                // $scope.getMasterItemList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.costingSOConfirmList = [];
    $scope.GetCostingSORatePopup = function (index, data) {
        $scope.itemIndex = index;
        $scope.masterItemId = data.MasterOrderItemId;
        $scope.soId = data.Id;
        $scope.soModel = data;
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetCostingSORateData?SalesOrderId=' + data.Id + '&lineId=' + $scope.masterItemId
        }).then(function successCallback(response) {
            $scope.costingSOConfirmList = response.data;
            angular.element(document.querySelector('#SOCostingRatePopup')).modal('show');
        });
    };
    $scope.CloseSOCostPopup = function () {
        angular.element(document.querySelector('#SOCostingRatePopup')).modal('hide');
        $scope.soId = null;
    }

    $scope.CalculateSOCost = function () {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CalculateSOCost',
                data: { 'OpenHeadNew': $scope.costingSOConfirmList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.costingSOConfirmList = response.data.NewData;
                for (var i = 0; i < $scope.costingSOConfirmList.length; i++) {
                    $scope.costingSOConfirmList[i].ItemValue = parseFloat($scope.costingSOConfirmList[i].ItemValue).toFixed(6);
                    $scope.costingSOConfirmList[i].SOValue = parseFloat($scope.costingSOConfirmList[i].SOValue).toFixed(6);
                }

                for (var j = 0; j < $scope.costingSOConfirmList.length; j++) {
                    $scope.costingSOConfirmList[j].ValueDiff = parseFloat($scope.costingSOConfirmList[j].ItemValue).toFixed(6) - parseFloat($scope.costingSOConfirmList[j].SOValue).toFixed(6);
                    $scope.costingSOConfirmList[j].ValueDiff = parseFloat($scope.costingSOConfirmList[j].ValueDiff).toFixed(6);
                }

            }, function errorCallback(response) {
                $scope.ShowResultCustom(response.status.Message, "failure");
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.calculateDiffValue = function (data) {
        data.ValueDiff = 0;
        data.ValueDiff = data.ItemValue - data.SOValue;
        data.ValueDiff = parseFloat(data.ValueDiff).toFixed(6);
    }

    $scope.TempList = [];
    $scope.SaveSOCost = function () {
        try {
            //if (baseService.isUndefinedOrNull($scope.soId)) {
            //    $scope.soId = $scope.soModel.Id;
            //}
            $scope.soId = $scope.soModel.Id;

            for (var i = 0; i < $scope.costingSOConfirmList.length; i++) {
                if ($scope.costingSOConfirmList[i].ItemValue !== $scope.costingSOConfirmList[i].SOValue) {
                    if (baseService.isUndefinedOrNull($scope.costingSOConfirmList[i].Remark)) {
                        throw "Remarks is required for '" + $scope.costingSOConfirmList[i].UserName + "'";
                    }
                }
            }

            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CreateSOCostingConfirm',
                data: { 'data': $scope.costingSOConfirmList, 'lineId': $scope.soId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedCostingSORateData($scope.soId, $scope.masterItemId);
                    getSalesOrderList();
                    clearSO();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetSavedCostingSORateData = function (soId, lieneId) {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetCostingSORateData?SalesOrderId=' + soId + '&lineId=' + lieneId
        }).then(function successCallback(response) {
            $scope.costingSOConfirmList = response.data;
        });
    };

    $scope.SaveItemDescription = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.Descriptionform.$valid) {
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CreateItemDescription',
                data: { 'data': $scope.masterOrderItem },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    // $scope.getMasterItemList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.UpdateLoggedTnA = function () {

        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/UpdateLoggedTnA',
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
    }

    //#region MOI File 
    $scope.ItemId = null;
    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull(args.model.Data))
                throw 'Please select/save the order first'
            $scope.ItemId = args.model.Data;
            args.data = args.model.Data;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "OrderManagements/MasterOrder/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ItemId))
            ShowResult('Please select/save the order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.MOIPath + '/' + data.Id + extention;
    };

    //#endregion

    //#region QBOQ
    $scope.processList = [];
    cboService.getCompanyProductionProcessCbo($scope.fileNew.CompanyId, function (result) {
        $scope.processList = result;
        //if (baseService.arrayLength(result) === 1) {
        //    $scope.productionSummaryNew.ProcessId = $scope.processList[0].Value;
        //}
    });

    $scope.CostingItemList = [];
    $scope.GetCostingItemCbo = function () {
        try {
            $http.get("OrderManagements/MasterOrder/GetCostingItemCbo")
                .then(
                    function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.CostingItemList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, 'failure');
        }

    };
    $scope.GetCostingItemCbo();

    $scope.getAutoSequence = function (itemId) {
        $http.get("OrderManagements/MasterOrder/GetAutoSequence?itemId=" + itemId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.qboqModel.Sequence = response.data[0].Sequence;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.AddQBOQ = function (obj) {
        $scope.MasterOrderItemId = obj.Id;
        $scope.qboq =
            { Id: null, Sequence: 0, MasterOrderItemId: $scope.MasterOrderItemId, MaterialMasterId: null, ArticleId: null, CostingItemId: null, UoMId: null, Description: null, Remarks: null, NetConsumptionPerUnit: 0, ValueLossPercentage: 0, GrossConsumption: 0, MateriaCostPerUnit: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, ProcessId: null, ResponsiblePersonId: null, IsOutSource: false, JobWorkType: null, EntityIdWithinCompany: null, EntityIdWithinGroup: null, VendorId: null, ProcessGroup: null }
        $scope.qboqModel = Object.assign({}, $scope.qboq);
        $scope.getAutoSequence($scope.MasterOrderItemId);
        $scope.GetQBOQByMasterOrderItem($scope.MasterOrderItemId);
        angular.element(document.querySelector('#QBOQPoUp')).modal('show');
    }

    $scope.GetQBOQ = function (obj) {
        $scope.QBOQAction = 'Update';

        $scope.qboq =
            { Id: null, Sequence: 0, MasterOrderItemId: $scope.MasterOrderItemId, MaterialMasterId: null, ArticleId: null, CostingItemId: null, UoMId: null, Description: null, Remarks: null, NetConsumptionPerUnit: 0, ValueLossPercentage: 0, GrossConsumption: 0, MateriaCostPerUnit: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, ProcessId: null, ResponsiblePersonId: null, IsOutSource: false, JobWorkType: null, EntityIdWithinCompany: null, EntityIdWithinGroup: null, VendorId: null, ProcessGroup: null }
        $scope.qboq = Object.assign({}, obj);
        $scope.qboqModel = Object.assign({}, $scope.qboq);
    };

    $scope.qboq =
        { Id: null, Sequence: 0, MasterOrderItemId: null, MaterialMasterId: null, ArticleId: null, CostingItemId: null, UoMId: null, Description: null, Remarks: null, NetConsumptionPerUnit: 0, ValueLossPercentage: 0, GrossConsumption: 0, MateriaCostPerUnit: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, ProcessId: null, ResponsiblePersonId: null, IsOutSource: false, JobWorkType: null, EntityIdWithinCompany: null, EntityIdWithinGroup: null, VendorId: null }
    $scope.qboqModel = Object.assign({}, $scope.qboq);

    $scope.CalculateNetConsumption = function () {
        $scope.qboqModel.GrossConsumption = parseFloat($scope.qboqModel.NetConsumptionPerUnit / (1 - ($scope.qboqModel.ValueLossPercentage / 100))).toFixed(4);
        //$scope.qboqModel.GrossConsumption = parseFloat(($scope.qboqModel.NetConsumptionPerUnit / 100) - $scope.qboqModel.ValueLossPercentage).toFixed(2);
        //$scope.qboqModel.NetConsumptionPerUnit = (($scope.qboqModel.GrossConsumption / 100) - $scope.qboqModel.ValueLossPercentage);
    }

    $scope.businessProcesses = "BOM";
    $scope.searchList = [];
    $scope.dataPlate = [];
    $scope.searchbyMaterialMasterDatalist = [
        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Material',
            'value': 'UserName'
        },
        {
            'name': 'Product',
            'value': 'ProductMasterName'
        },
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'Text': 'Base UoM',
            'Value': 'BaseUoM'
        }
    ];

    $scope.getRMaterialMasterSearchData = function () {
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        //$scope.popUpUrl = 'Materials/MaterialMaster/GetNonAssetMaterialList';
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#rmaterialmastersearchpopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'rmaterialmastersearchpopup');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.setRMaterialMasterData = function (ob) {
        $scope.qboqModel.MaterialMasterId = ob.Id;
        $scope.qboqModel.MaterialMaster = ob.UserName;
        $scope.qboqModel.ArticleId = null;
        $scope.qboqModel.Article = null;
        $scope.qboqModel.HasAttribute = ob.HasAttribute;
        if ($scope.qboqModel.HasAttribute) {
            $scope.materialType = null;
            $scope.getRMArticleSearchList(ob.Id);
        } else {
            $scope.closeMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }
        $scope.closeRMMaterialMasterbyTypePopUp();
        UomCboByMaterialMaster($scope.qboqModel.MaterialMasterId);
    }

    $scope.uOMList = [];
    function UomCboByMaterialMaster(materilaMasterId) {
        var mmId = []; mmId.push(materilaMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (response) {
            $scope.uOMList = response;
            if (baseService.arrayLength($scope.uOMList) == 1) {
                $scope.qboqModel.UoMId = $scope.uOMList[0].Value;
            }
        });
    }

    $scope.closeRMMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('rmaterialmastersearchpopup');
        angular.element(document.querySelector('#rmaterialmastersearchpopup')).modal('hide');
    };

    $scope.getRMArticle = function (index) {
        $scope.getRMArticleSearchList($scope.qboqModel.MaterialMasterId);
    };
    $scope.getRMArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            //$scope.popUpUrl = 'Materials/MaterialMasterArticle/GetMaterialArticle';
            $scope.materialType = null;
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {

                            angular.element(document.querySelector('#rarticleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };
    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('rarticleSearchPop');
        angular.element(document.querySelector('#rarticleSearchPop')).modal('hide');
    };
    $scope.selectRMarticle = function (ob) {
        try {
            $scope.qboqModel.MaterialMasterId = ob.MaterialMasterId;
            $scope.qboqModel.MaterialMaster = ob.MaterialMasterName;
            $scope.qboqModel.ArticleId = ob.Id;
            $scope.qboqModel.Article = ob.StandardName;
            angular.element(document.querySelector('#rarticleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'rarticleSearchPop');
        }
    };

    $scope.QBOQAction = 'Save';
    $scope.SaveQBOQ = function () {
        var sbt = 0;
        try {
            if (baseService.isUndefinedOrNull($scope.qboqModel.MaterialMasterId)) {
                throw 'Material is required.';
            }
            if (baseService.isUndefinedOrNull($scope.qboqModel.ArticleId)) {
                throw 'Article is required.';
            }
            //if (baseService.isUndefinedOrNull($scope.qboqModel.NetConsumptionPerUnit) || $scope.qboqModel.NetConsumptionPerUnit < 0 || isNaN($scope.qboqModel.NetConsumptionPerUnit)) {
            //    throw "Net Consumption Per Unit should greater than 0.";
            //}
            if (baseService.isUndefinedOrNull($scope.qboqModel.ValueLossPercentage) || $scope.qboqModel.ValueLossPercentage < 0 || isNaN($scope.qboqModel.ValueLossPercentage)) {
                throw "Value Loss Percentage should greater than 0.";
            }
            if (baseService.isUndefinedOrNull($scope.qboqModel.GrossConsumption) || $scope.qboqModel.GrossConsumption < 0 || isNaN($scope.qboqModel.GrossConsumption)) {
                throw "Gross Consumption should greater than 0.";
            }
            if (baseService.arrayLength($scope.qboqList) > 0) {
                for (var i = 0; i < $scope.qboqList.length; i++) {
                    if ($scope.qboqList[i].Id != $scope.qboqModel.Id) {
                        sbt = sbt + $scope.qboqList[i].NetConsumptionPerUnit;
                    }
                }
                sbt = Math.round((sbt) * 10000 + Number.EPSILON) / 10000;
                if (sbt + $scope.qboqModel.NetConsumptionPerUnit > 1) {
                    throw "Total of Net Consumption Per Unit can not exceed 1.";
                }
            }
            $scope.$broadcast('show-errors-check-validity');
            angular.copy($scope.qboqModel, $scope.qboq);
            if ($scope.QBOQForm.$valid) {
                if ($scope.QBOQAction == 'Save') {
                    $http({
                        method: 'POST',
                        url: 'OrderManagements/MasterOrder/CreateQBOQ',
                        data: { 'data': $scope.qboq },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'QBOQPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'QBOQPoUp');
                            $scope.getAutoSequence($scope.MasterOrderItemId);
                            $scope.GetQBOQByMasterOrderItem($scope.MasterOrderItemId);

                            $scope.QBOQAction = 'Save';
                            $scope.ClearQBOQByMasterOrderItem();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'QBOQPoUp');
                    }
                }
                else {
                    $http({
                        method: 'POST',
                        url: 'OrderManagements/MasterOrder/EditQBOQ',
                        data: { 'data': $scope.qboq },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'QBOQPoUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'QBOQPoUp');
                            $scope.getAutoSequence($scope.MasterOrderItemId);
                            $scope.GetQBOQByMasterOrderItem($scope.MasterOrderItemId);

                            $scope.QBOQAction = 'Save';
                            $scope.ClearQBOQByMasterOrderItem();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'QBOQPoUp');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'QBOQPoUp');
        }
    };

    $scope.messageBOQconfirmation = null;
    $scope.removeLineItemBOQ = function (data) {
        $scope.qboq = data;
        if (!baseService.isUndefinedOrNull($scope.qboq.Id))
            $scope.messageBOQconfirmation = 'Are you sure want to delete permanently';
        //angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');

        var eDialog = $("#confirmBOQPopUp").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    }

    $scope.ConfirmClose = function () {
        var eDialog = $("#confirmBOQPopUp").data("ejDialog");
        eDialog.close();
    };

    $scope.DeleteBOQ = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/MasterOrder/DeleteQuickBOQ?id=' + $scope.qboq.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getAutoSequence($scope.MasterOrderItemId);
                $scope.GetQBOQByMasterOrderItem($scope.MasterOrderItemId);
                $scope.ClearQBOQByMasterOrderItem();
                $scope.ConfirmClose();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.ClearQBOQByMasterOrderItem = function () {
        $scope.qboq =
            { Id: null, Sequence: 0, MasterOrderItemId: $scope.MasterOrderItemId, MaterialMasterId: null, ArticleId: null, CostingItemId: null, UoMId: null, Description: null, Remarks: null, NetConsumptionPerUnit: 0, ValueLossPercentage: 0, GrossConsumption: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, ProcessId: null, ResponsiblePersonId: null, IsOutSource: false, JobWorkType: null, EntityIdWithinCompany: null, EntityIdWithinGroup: null, VendorId: null, ProcessGroup: null }
        $scope.qboqModel = Object.assign({}, $scope.qboq);

    }

    $scope.GetQBOQByMasterOrderItem = function (itemId) {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetQBOQByMasterOrderItem?itemId=' + itemId
        }).then(function successCallback(response) {
            $scope.qboqList = response.data;
        })
    };

    $scope.closeQBOQPopUp = function () {
        $scope.ClearQBOQByMasterOrderItem();
        angular.element(document.querySelector('#QBOQPoUp')).modal('hide');
    }


    $scope.popUpDataList = [];
    $scope.popUpTitle = '';
    $scope.valueData = '';
    $scope.BOQJobPopUp = function () {
        $scope.popUpList = [];
        $scope.popUpDataList = [];
        $scope.popUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Name'
            , searchBy: "Name"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        //if (isJobWorkApplicable($scope.itemList, index))
        //    return ShowResult('Please select at first job work type!', 'failure');
        $scope.popUpUrl = typeCheckAndCreateBOQUrl();
        $scope.getPopUpDataBOQ = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'BOQJobPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#BOQJobPopUp')).modal('show');
        $scope.getPopUpDataBOQ();
    };

    $scope.selectBOQDoubleClick = function (data) {
        valueSetInCtrl(data);
        $scope.closeBOQPopUp();
    };

    $scope.selectBOQSingleClick = function (data) {
        $scope.valueData = data;
    };

    $scope.selectByButtonBOQ = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'BOQJobPopUp');
        }
        $scope.selectBOQDoubleClick($scope.valueData);
        $scope.closeBOQPopUp();
    };

    $scope.closeBOQPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#BOQJobPopUp')).modal('hide');
    };

    function typeCheckAndCreateBOQUrl() {
        if ($scope.qboqModel.JobWorkType === 'EntityWithinCompany') {
            $scope.popUpTitle = 'Entity within company';
            return 'Organizations/entity/withincompany?companyId=' + $scope.fileNew.CompanyId + '&entityId=' + $scope.fileNew.EntityId;
        }
        else if ($scope.qboqModel.JobWorkType === 'EntityWithinGroup') {
            $scope.popUpTitle = 'Entity in group';
            return 'Organizations/entity/withingroup?companyGroupId=' + $window.companyGroupId + '&companyId=' + $scope.fileNew.CompanyId + '&entityId=' + $scope.fileNew.EntityId;
        }
        else {
            $scope.popUpParameters.sort = 'PartyName';
            $scope.popUpParameters.searchBy = 'PartyName';
            $scope.popUpTitle = 'Vendor';
            return 'Parties/party/GetCompanyPartyDataListByPlantId?CompanyId=' + $scope.fileNew.CompanyId + '&PlantId=' + $scope.fileNew.PlantId + '&partyType=' + 'vendor';
        }
    }

    function isJobWorkApplicable(list, index) {
        if (baseService.isUndefinedOrNull(list[index].JobWorkType))
            return true;
        else
            return false;
    }
    function valueSetInCtrl(data) {
        $scope.clearBoQEntityOrVendor();
        if ($scope.qboqModel.JobWorkType === 'EntityWithinCompany') {
            $scope.qboqModel.EntityIdWithinCompany = data.Id;
            $scope.qboqModel.EntityOrVendorName = data.Name;
        }
        else if ($scope.qboqModel.JobWorkType === 'EntityWithinGroup') {
            $scope.qboqModel.EntityIdWithinGroup = data.Id;
            $scope.qboqModel.EntityOrVendorName = data.Name;
        }
        else {
            $scope.qboqModel.VendorId = data.Id;
            $scope.qboqModel.EntityOrVendorName = data.PartyName;
        }
    }


    $scope.clearBoQEntityOrVendor = function () {
        $scope.qboqModel.EntityIdWithinCompany = null;
        $scope.qboqModel.EntityIdWithinGroup = null;
        $scope.qboqModel.VendorId = null;
        $scope.qboqModel.EntityOrVendorName = null;
    };

    $scope.clearBoQJobType = function () {
        $scope.qboqModel.JobWorkType = null;
    };
    //#endregion Job Work Type

    $scope.modelPD = {
        Id: null,
        UserName: null,
        LineItem: null,
        PackingLevel: null,
        OwnRefNo: null,
        CustomerRefNo: null,
        MasterOrderItemId: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        Remarks: null,
    };
    $scope.modelNewPD = Object.assign({}, $scope.modelPD);

    $scope.ClearPackingDetail = function () {
        $scope.modelNewPD = Object.assign({}, $scope.modelPD);
    };

    //#region Contract

    $scope.model = {
        Id: null,
        CompanyId: null,
        MasterOrderId: $scope.fileNew.Id == null ? null : $scope.fileNew.Id,
        ContractNo: null,
        CustomerId: null,
        Descriotion: null,
        IsLC: false,
        CustomerName: null,
        Currency: null,
        TotalQty: 0,
        SOQty: 0,
        Amount: 0,
        UDNo: null,
        IsPrint: false,
        IsMarketingCommisssionApplicable: false,
        MarketingCommisssionId: null,
        IsBusinessDevelopmentChargesApplicable: false,
        BusinessDevelopmentCharge: 'Percentage',
        MarketingCommisssionCharge: 'Percentage',
        BusinessDevelopmentChargeValue: null,
        MarketingCommisssionChargeValue: null,
        InvoicingPartyPlantId: null,
        DeliveryPartyPlantId: null,
        InvoicingByAddress: null,
        DeliveryByAddress: null,

    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.XshowPartyPopUpNew = function () {
        $scope.partyType = 'Vendor';
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
        if (baseService.isUndefinedOrNull($scope.fileNew.CompanyId)) {
            ShowResult('Select Company', 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.fileNew.PlantId)) {
            ShowResult('Select Plant', 'failure');
            return false;
        }


        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + $scope.fileNew.CompanyId + '&PlantId=' + $scope.fileNew.PlantId;

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#partyPopUpNew')).modal('show');
    };

    $scope.SetVendorData = function (obj) {
        $scope.modelNew.MarketingCommisssionId = obj.data.Id;
        $scope.modelNew.MarketingCommisssion = obj.data.UserName;
        angular.element(document.querySelector('#partyPopUpNew')).modal('hide');
        $scope.searchParty = '';
    }

    $scope.closePartyPopUpNew = function () {
        angular.element(document.querySelector('#partyPopUpNew')).modal('hide');
        $scope.hidePartyPopUp();
        $scope.partyType = "Vendor";
        $scope.searchParty = '';
    }

    $scope.SetMarComValues = function () {
        if ($scope.modelNew.IsMarketingCommisssionApplicable === false) {
            $scope.modelNew.MarketingCommisssion = null;
            $scope.modelNew.MarketingCommisssionId = null;
            $scope.modelNew.MarketingCommisssionCharge = 'Percentage';
            $scope.modelNew.MarketingCommisssionValue = null;
        }
    }

    $scope.SetBusinessDevelopmentValues = function () {
        if ($scope.modelNew.IsBusinessDevelopmentChargesApplicable === false) {
            $scope.modelNew.BusinessDevelopmentCharge = 'Percentage';
            $scope.modelNew.BusinessDevelopmentChargeValue = null;
        }
    }

    $scope.GetContractByMasterOrder = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/masterOrder/GetContractByMasterOrder?masterId=' + $scope.fileNew.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) != 0) {
                //$scope.modelNew = response.data[0];
                $scope.modelNew = Object.assign({}, response.data[0]);
            } else {
                $scope.modelNew = Object.assign({}, $scope.model);
            }

            $scope.modelNew.CustomerId = $scope.fileNew.PartyId;
            $scope.modelNew.CustomerName = $scope.fileNew.CustomerName;
            $scope.GetContractFundData($scope.modelNew.Id);
            if (!baseService.isUndefinedOrNull($scope.modelNew.InvoicingPartyPlantId)) {
                getPartyPlantEditList($scope.modelNew.InvoicingPartyPlantId, $scope.modelNew.InvoicingByAddress, $scope.modelNew.DeliveryPartyPlantId, $scope.modelNew.DeliveryByAddress, $scope.modelNew.DeliveryState, $scope.modelNew.DeliveryGSTIN);
            }
            else {
                $scope.getPartyPlant();
            }

            $scope.GetContractTermsAndConditionsList();
        });
    }

    $scope.getPartyPlant = function () {
        $scope.getCboPartyPlantList($scope.modelNew.CustomerId, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.modelNew.InvoicingPartyPlantId = item.Value;
                    $scope.modelNew.DeliveryPartyPlantId = item.Value;
                    $scope.modelNew.InvoicingByAddress = item.Address1;
                    $scope.modelNew.DeliveryByAddress = item.Address1;
                    $scope.modelNew.InvoicingState = item.StateName;
                    $scope.modelNew.InvoicingGSTIN = item.GSTIN;
                    $scope.modelNew.DeliveryState = item.StateName;
                    $scope.modelNew.DeliveryGSTIN = item.GSTIN;
                    $scope.modelNew.InvoicingStateId = item.StateId;
                }
            });
        });
    }

    function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.modelNew.CustomerId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.Value == invoicingPartyPlantId) {
                    $scope.partyPlantId = item.Value;
                    $scope.modelNew.InvoicingPartyPlantId = item.Value;
                    $scope.modelNew.DeliveryPartyPlantId = deliveryplant;
                    $scope.modelNew.InvoicingByAddress = invoAddress;
                    $scope.modelNew.DeliveryByAddress = deliAddress;
                    $scope.modelNew.InvoicingState = item.StateName;
                    $scope.modelNew.InvoicingGSTIN = item.GSTIN;
                    $scope.modelNew.DeliveryState = deliState;
                    $scope.modelNew.DeliveryGSTIN = deliGSTIN;
                    $scope.modelNew.InvoicingStateId = item.StateId;
                }
            });

        });
    }

    $scope.contractFundList = [];
    $scope.GetContractFundData = function (contractId) {
        $scope.contractFundList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetContractFundData?contractId=" + contractId
        }).then(function (response) {
            $scope.contractFundList = response.data;

            if (baseService.arrayLength($scope.contractFundList) > 0) {
                for (var i = 0; i < $scope.contractFundList.length; i++) {
                    if ($scope.contractFundList[i].UtilizationSourceType === 'BuyerDeduction') {
                        for (var j = 0; j < $scope.buyerDeductionList.length; j++) {
                            if ($scope.contractFundList[i].FundUtilization === $scope.buyerDeductionList[j].FundUtilization) {
                                $scope.buyerDeductionList[j].Percentage = $scope.contractFundList[i].Percentage;
                                $scope.buyerDeductionList[j].OldPercentage = $scope.contractFundList[i].OldPercentage;
                                $scope.buyerDeductionList[j].Commission = $scope.contractFundList[i].Commission;
                            }
                        }
                    }
                }
            }

            if (baseService.arrayLength($scope.contractFundList) > 0) {
                for (var l = 0; l < $scope.contractFundList.length; l++) {
                    if ($scope.contractFundList[l].UtilizationSourceType === 'FundUtilization') {
                        for (var k = 0; k < $scope.fundUtilizationList.length; k++) {
                            if ($scope.contractFundList[l].FundUtilization === $scope.fundUtilizationList[k].FundUtilization) {

                                $scope.fundUtilizationList[k].Percentage = $scope.contractFundList[l].Percentage;
                                $scope.fundUtilizationList[k].OldPercentage = $scope.contractFundList[l].OldPercentage;
                                $scope.fundUtilizationList[k].PurchaseMargin = $scope.contractFundList[l].PurchaseMargin;
                            }
                        }
                    }
                }
            }


        });
    };

    $scope.getPercentageValue = function () {
        var negotiable = 0;
        for (var j = 0; j < $scope.fundUtilizationList.length; j++) {
            if (!baseService.isUndefinedOrNull($scope.fundUtilizationList[j].Percentage) && $scope.fundUtilizationList[j].Text !== 'Negotiable') {
                negotiable += $scope.fundUtilizationList[j].Percentage;
            }
        }
        for (var j = 0; j < $scope.fundUtilizationList.length; j++) {
            if ($scope.fundUtilizationList[j].Text === 'Negotiable') {
                $scope.fundUtilizationList[j].Percentage = 100 - negotiable;
            }
        }
    }

    $scope.fundUtilizationList = [];
    cboService.getEnumCbo("enum/GetFundUtilizationEnumCbo", function (result) {
        $scope.fundUtilizationList = result;

        $scope.getFundUtilizationData();
    });

    $scope.ModelList = [];
    $scope.getFundUtilizationData = function () {
        $http({
            method: 'POST',
            url: 'Commercial/ContractFundUtilization/GetFundUtilizationList',
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.ModelList = response.data;
                for (var i = 0; i < $scope.ModelList.length; i++) {
                    for (var j = 0; j < $scope.fundUtilizationList.length; j++) {
                        if ($scope.ModelList[i].FundUtilization == $scope.fundUtilizationList[j].Text) {
                            $scope.fundUtilizationList[j].FundUtilization = $scope.ModelList[i].FundUtilization;
                            $scope.fundUtilizationList[j].FundUtilizationText = $scope.ModelList[i].FundUtilizationText;
                            $scope.fundUtilizationList[j].Percentage = $scope.ModelList[i].Percentage;
                            $scope.fundUtilizationList[j].OldPercentage = $scope.ModelList[i].Percentage;
                            $scope.fundUtilizationList[j].CurrencyId = $scope.ModelList[i].CurrencyId;
                        }
                    }
                }
            }
        });
    };

    //#region

    //$scope.buyerDeductionList = [];
    //cboService.getEnumCbo("enum/GetBuyerDeductionEnumCbo", function (result) {
    //    $scope.buyerDeductionList = result;

    //    $scope.getBuyerDeductionData();
    //});

    //$scope.dataList = [];
    //$scope.getBuyerDeductionData = function () {
    //    $http({
    //        method: 'POST',
    //        url: 'Commercial/LCFundUtilization/GetBuyerDeductionList',
    //        data: { column: $scope.searchBy, value: $scope.search },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (baseService.arrayLength(response.data) > 0) {
    //            $scope.dataList = response.data;
    //            for (var i = 0; i < $scope.dataList.length; i++) {
    //                for (var j = 0; j < $scope.buyerDeductionList.length; j++) {
    //                    if ($scope.dataList[i].FundUtilization == $scope.buyerDeductionList[j].Text) {

    //                        $scope.buyerDeductionList[j].FundUtilization = $scope.dataList[i].FundUtilization;
    //                        $scope.buyerDeductionList[j].FundUtilizationText = $scope.dataList[i].FundUtilizationText;
    //                        $scope.buyerDeductionList[j].Percentage = $scope.dataList[i].Percentage;
    //                        $scope.buyerDeductionList[j].OldPercentage = $scope.dataList[i].Percentage;
    //                        $scope.buyerDeductionList[j].CurrencyId = $scope.dataList[i].CurrencyId;
    //                    }
    //                }
    //            }
    //        }
    //    });
    //};

    //#endregion

    // #region checkbox all for TermsAndConditions

    $scope.TermsAndConditionsList = [];

    $scope.GetContractTermsAndConditionsList = function () {

        $http({
            method: 'GET',
            url: 'Commercial/Contract/GetContractTermsAndConditionsList?ContractId=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            $scope.TermsAndConditionsList = response.data;
        });
    }

    $scope.searchdata = [];
    $scope.GetTermsAndConditionsList = function () {
        $scope.searchdata = [];
        $http({
            method: 'GET',
            url: 'Commercial/Contract/GetTermsAndConditionsList'
        }).then(function successCallback(response) {
            $scope.searchdata = response.data;
        });
    }

    $scope.AddTermsAndConditions = function () {

        $scope.GetTermsAndConditionsList();
        $scope.ShowResultCustom();
    }

    $scope.ShowResultCustom = function (message, type) {
        $("#TermsAndConditionsPoUp").ejDialog("setTitle", "Terms And Conditions");
        var eDialog = $("#TermsAndConditionsPoUp").data("ejDialog");
        eDialog.open();

        var gridObj = $("#GridTermsAndConditions").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering

    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridTermsAndConditions").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.searchdata.length; i++) {
                $scope.searchdata[i].Flag = ChkOrUnchk;
            }

        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridTermsAndConditions").data("ejGrid");
        gridObj.refreshContent();

    };

    $scope.TermsAndConditionsList = [];

    function MakeData() {

        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].Flag == true) {
                if (checkExists($scope.TermsAndConditionsList, $scope.searchdata[i].Id) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.TermsAndConditionsId = $scope.searchdata[i].Id;;
                    ob.ContractId = $scope.modelNew.Id;
                    ob.Sequence = $scope.searchdata[i].Sequence;;
                    ob.Code = $scope.searchdata[i].Code;;
                    ob.ShortName = $scope.searchdata[i].ShortName;;
                    ob.StandardName = $scope.searchdata[i].StandardName;;
                    ob.UserName = $scope.searchdata[i].UserName;
                    ob.Description = $scope.searchdata[i].Description;

                    $scope.TermsAndConditionsList.push(ob);
                }
                else {
                    throw "This Terms & Conditions " + $scope.searchdata[i].UserName + " is already taken.";
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].TermsAndConditionsId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseTermsAndConditions = function () {
        try {
            MakeData();
            $scope.SaveTNC();
            var eDialog = $("#TermsAndConditionsPoUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveTNC = function () {
        try {
            $http({
                method: 'POST',
                url: 'Commercial/Contract/CreateTNC',
                data: {
                    'data': $scope.TermsAndConditionsList
                    , 'contractId': $scope.modelNew.Id
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetContractTermsAndConditionsList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.message_detailconfirmation = null;
    $scope.removeBoMDetail = function (obj) {
        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    }

    $scope.DeleteTNC = function () {
        $http({
            method: 'POST',
            url: 'Commercial/Contract/DeleteContractTermsAndConditions?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetContractTermsAndConditionsList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // #endregion checkbox all

    $scope.GetMasterOrderAmountAndQty = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetMasterOrderAmountAndQty?masterId=' + $scope.fileNew.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.modelNew.Amount = response.data[0].Amount;
                $scope.modelNew.TotalQty = response.data[0].TotalQty
                $scope.modelNew.SOQty = response.data[0].Qty
            }
        });
    }

    $scope.MasterItemList = [];
    $scope.GetMasterItemDataList = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/?masterOrderId=' + $scope.fileNew.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.MasterItemList = response.data;
            }
        });
    }
    $scope.GetMasterItemDataList();

    $scope.itemListData = [];
    $scope.MasterItemListPopUp = function () {

        $http.get('OrderManagements/MasterOrder/GetItemsData?masterOrderId=' + $scope.fileNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.itemListData = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#masterItemListPopUpId')).modal('show');
    };

    $scope.closeItemListPopUp = function (data) {
        $scope.modelNewPD.MasterOrderItemId = data.MasterOrderItemId;
        $scope.hideItemListPopUp();
    };

    $scope.hideItemListPopUp = function () {
        angular.element(document.querySelector('#masterItemListPopUpId')).modal('hide');
    };

    $scope.SavePackingDetail = function () {
        $scope.MasterOrderId = $scope.fileNew.Id;
        try {
            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/MasterOrder/CreatePackingDetail',
                    data: { 'data': $scope.modelNewPD, 'MasterOrderId': $scope.MasterOrderId },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPackingDetail();
                        //$scope.modelNewPD.Id = response.data.Id;
                        $scope.ClearPackingDetail();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.PackingDetailDataList = [];
    $scope.GetPackingDetail = function () {
        $scope.PackingDetailDataList = [];
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetPackingDetail?masterOderId=' + $scope.fileNew.MasterOrderNo
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.PackingDetailDataList = response.data;
            }
        });
    }
    $scope.GetPackingDetail();

    $scope.recorddoubleclicks = function (args) {
        try {
            $scope.Action = 'Update';
            $scope.modelNewPD = Object.assign({}, args.data);

            //$scope.getCityList();
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.message_PackingDetailconfirmation = null;
    $scope.RemovePackingDetail = function (data) {
        $scope.modelNewPD = data.data;
        if (!baseService.isUndefinedOrNull($scope.modelNewPD.Id))
            $scope.message_PackingDetailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmPackingDetailPopUp')).modal('show');
    }

    $scope.DeletePackingDetail = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNewPD.Id)) {
            $http.get('OrderManagements/MasterOrder/DeletePackingDetail?PackingDetailId=' + $scope.modelNewPD.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.modelNewPD = Object.assign({}, $scope.modelPD);
                        $scope.GetPackingDetail();
                        //$scope.ClearPackingDetail();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.PackingDetailList = [];
    $scope.GetPackingDetailData = function () {
        try {
            $http.get("OrderManagements/MasterOrder/GetPackingDetailData")
                .then(
                    function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.PackingDetailList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }

    };
    $scope.GetCostingItemCbo();

    $scope.GetPopUpTab = function (obj) {
        $scope.tab2 = 1;
        $scope.ClearPT();
        $scope.modelNewPD = obj.data;
        $scope.ModelSO.PackingDetailId = obj.data.Id;
        //$scope.ModelSO.SOId = obj.data;
        $scope.ModelPTNew.PackingDetailId = obj.data.Id;

        $scope.GetSavedPackingType($scope.ModelPTNew.PackingDetailId);


        angular.element(document.querySelector('#SOPopUpData')).modal('show');
    }

    //$scope.GetDetailChild = function (obj) {
    //    $scope.modelNewPD = obj.data;
    //    $scope.ModelSO.LineItemId = obj.data.MasterOrderItemId;
    //    $scope.GetSavedSOData($scope.modelNewPD.Id);
    //    angular.element(document.querySelector('#DetailChildPopUp')).modal('show');
    //}

    $scope.SODataList = [];
    $scope.GetSavedSOData = function (packingDetailId) {
        $scope.SODataList = [];
        $http.get('OrderManagements/MasterOrder/GetSavedSOData?PackingDetailId=' + packingDetailId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SODataList = response.data;
                    if ($scope.SODataList.length > 0) {
                        var uniqueSalesOrderId = removeDuplicates($scope.SODataList, 'SOId');
                        var wcEmpCode = "";
                        if (uniqueSalesOrderId.length > 0) {
                            wcEmpCode = "IN(";
                            wcEmpCode += Array.prototype.map.call(uniqueSalesOrderId, function (item) { return "'" + item.SOId + "'"; }).join(",") + ")";
                        }
                        $scope.sqlInStatement = wcEmpCode;
                    }
                }
            });
    }

    $scope.SOItemList = [];
    $scope.GetSOPopUp = function () {
        $scope.SOItemList = [];
        $http.get('OrderManagements/MasterOrder/GetSOData?lineItem=' + $scope.modelNewPD.MasterOrderItemId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#SOItemPopup')).modal('show');
    };

    $scope.GetSODataDbl = function (args) {
        try {
            $scope.Action = 'Update';
            $scope.ModelSO = Object.assign({}, args.data);

            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.selectSOItem = function ($event) {
        try {
            var selectedSO = false;
            for (var i = 0; i < $scope.SODataList.length; i++) {
                if ($scope.SODataList[i].SOId == $event.data.SOId) {
                    selectedSO = true;
                    break;

                }
            }

            if (selectedSO == true) {
                ShowResult("SO Id already exists!", 'failure');
            }
            else {
                var soitem = $event.data;
                $scope.ModelSO.SOId = soitem.SOId;
                angular.element(document.querySelector('#SOItemPopup')).modal('hide');

            }
            //var soitem = $event.data;
            //$scope.ModelSO.SOId = soitem.SOId;
            //angular.element(document.querySelector('#SOItemPopup')).modal('hide');

        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.SaveSOData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelSO.SOId)) {
                throw "Select SO No.";
            }

            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CreateSOData',
                data: { 'data': $scope.ModelSO },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearSO();
                    $scope.GetSavedSOData($scope.modelNewPD.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ModelSO = {
        Id: null,
        PackingDetailId: $scope.modelNewPD.Id,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    $scope.ClearSO = function () {
        $scope.ModelSO = {
            Id: null,
            PackingDetailId: $scope.modelNewPD.Id,
            Remarks: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null
        };
    }

    $scope.removeChildSO = function (obj) {
        $scope.SODetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.SODetailNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmChildSOPopUp')).modal('show');
    }

    $scope.DeleteChildSO = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/MasterOrder/DeleteChildSO?id=' + $scope.SODetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedSOData($scope.modelNewPD.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.ModelPT = {
        Id: null,
        PackingDetailId: $scope.modelNewPD.Id,
        PackingCode: null,
        PackingTypeId: null,
        PackingType: null,
        CustomerRefCode: null,
        Remarks: null,

        PackingTypeId: $scope.PackingTypeId,
        FGFirstCharacteristicsValueId: null,
        FirstCharacteristics: null,
        FGSecondCharacteristicsValueId: null,
        SecondCharacteristics: null,
        Quantity: null,
        Plan: null,
        ToPlanQuantity: null
    };
    $scope.ModelPTNew = Object.assign({}, $scope.ModelPT);

    $scope.ClearPT = function () {
        $scope.ModelPTNew = {
            Id: null,
            PackingDetailId: $scope.modelNewPD.Id,
            PackingCode: null,
            PackingTypeId: null,
            PackingType: null,
            CustomerRefCode: null,
            Remarks: null,
        };
        $scope.packingTypeDataList = [];
    };

    $scope.SavePackingType = function () {
        try {
            //if (baseService.isUndefinedOrNull($scope.ModelSO.SOId)) {
            //    throw "Select SO No.";
            //}

            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CreatePackingType',
                data: { 'data': $scope.ModelPTNew, 'SKUList': $scope.packingTypeDataList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearPT();
                    $scope.GetSavedPackingType($scope.modelNewPD.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.PackingTypeListData = [];
    $scope.GetSavedPackingType = function (packingDetailId) {
        $scope.PackingTypeListData = [];
        $http.get('OrderManagements/MasterOrder/GetSavedPackingType?PackingDetailId=' + packingDetailId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.PackingTypeListData = response.data;
                }
                $scope.GetSavedSOData($scope.ModelSO.PackingDetailId);
            });
    }

    $scope.GetPT = function (args) {
        try {
            $scope.Action = 'Update';
            $scope.ModelPTNew = Object.assign({}, args.data);
            $scope.GetPackingTypeChidChangeData();
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetPackingTypeChildData = function () {
        $scope.packingTypeDataList = [];
        $http.get('OrderManagements/MasterOrder/GetSavedPackingTypeChild?PTId=' + $scope.ModelPTNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.packingTypeDataList = response.data;
                }
            });
    }

    $scope.removeChild2 = function (obj) {
        $scope.packingTypeNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.packingTypeNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmChildPackingTypePopUp')).modal('show');
    }

    $scope.DeletePackingType = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/MasterOrder/DeletePackingType?id=' + $scope.packingTypeNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedPackingType($scope.modelNewPD.Id);
                $scope.ClearPT();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.GetPopUpTab2 = function (obj) {
        $scope.tab2 = 2;
        $scope.PackingTypeId = obj.data.Id;
        $scope.ModelSKUDNew = Object.assign({}, $scope.ModelSku);
        $scope.ModelSKUDNew.PackingTypeId = $scope.PackingTypeId;
        //$scope.GetSavedPackingType$scope.ModelPTNew.PackingTypeId
        if ($scope.SODataList.length > 0) {
            var uniqueSalesOrderId = removeDuplicates($scope.SODataList, 'SOId');
            var wcEmpCode = "";
            if (uniqueSalesOrderId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueSalesOrderId, function (item) { return "'" + item.SOId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }

        angular.element(document.querySelector('#SKUPopUp')).modal('show');
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    //$scope.ModelSku = {
    //    Id: null,
    //    PackingTypeId: $scope.PackingTypeId,
    //    FGFirstCharacteristicsId: null,
    //    FirstCharacteristics: null,
    //    FGSecondCharacteristicsId: null,
    //    SecondCharacteristics: null,
    //    Quantity: null,
    //    Plan: null
    //};
    //$scope.ModelSKUDNew = Object.assign({}, $scope.ModelSku);

    //$scope.ClearSKUD = function () {
    //    $scope.ModelSKUDNew = Object.assign({}, $scope.ModelSku);
    //    $scope.ModelSKUDNew.PackingTypeId = $scope.PackingTypeId;
    //}

    //$scope.sku1List = [];
    //$scope.sku1 = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'OrderManagements/MasterOrder/GetSKU1List?SOId=' + $scope.sqlInStatement
    //    }).then(function successCallback(response) {
    //        $scope.sku1List = response.data;
    //    })
    //};

    //$scope.sku2List = [];
    //$scope.sku2 = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'OrderManagements/MasterOrder/GetSKU2List?SOId=' + $scope.sqlInStatement
    //    }).then(function successCallback(response) {
    //        $scope.sku2List = response.data;
    //    })
    //};
    $scope.PackingTypeId = null;
    $scope.SaveSKUDetail = function () {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CreateSKUDetail',
                data: { 'data': $scope.ModelSKUDNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearSKUD();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetSKUDetailDblClick = function (args) {
        try {
            $scope.Action = 'Update';
            $scope.ModelSKUDNew = Object.assign({}, args.data);

            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.removeSKUDetail = function (obj) {
        $scope.SKUDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.SKUDetailNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmSKUDetailPopUp')).modal('show');
    }

    //#region   SO Copy    
    $scope.CopySO = function (data) {
        $http({
            method: 'POST',
            url: $scope.path + 'CopySalesOrder?MasterId=' + data.Id + '&masterItemId=' + data.MasterOrderItemId + '&TotalMOIQty=' + $scope.TotalMOIQty,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                getSalesOrderList();
                $scope.getMasterItemList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.ModelSKU = {
        FromSKU1Id: null,
        ToSKU1Id: null,
        FromSKU2Id: null,
        ToSKU2Id: null
    }

    $scope.ShowSKUMapPopUp = function (data, MasterOrderItemId) {
        try {
            if (!baseService.isUndefinedOrNull(MasterOrderItemId)) {

                for (var i = 0; i < $scope.itemList.length; i++) {
                    for (var j = 0; j < $scope.itemList[i].TempList.length; j++) {
                        if ($scope.itemList[i].TempList[j].MasterOrderItemId == MasterOrderItemId) {
                            if (data.TotalQty > $scope.itemList[i].TempList[j].TotalQty) {
                                throw "Destination Total Qty can't greater than Source Total Qty.";
                            }
                        }
                    }
                }


                $scope.setTab3(1);
                $scope.ToMasterOrderItemId = data.Id;
                $scope.FromMasterOrderItemId = MasterOrderItemId;
                $scope.ToMaterialMasterId = data.MaterialMasterId;

                $scope.GetFromItemMaterialSKU1Data($scope.FromMasterOrderItemId);
                $scope.GetFromItemMaterialSKU2Data($scope.FromMasterOrderItemId);

                $scope.GetToItemMaterialSKU1($scope.ToMaterialMasterId);
                $scope.GetToItemMaterialSKU2($scope.ToMaterialMasterId);
                angular.element(document.querySelector('#SKUsPopUp')).modal('show');
            }
            else {
                throw "Select Master Order line item.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.FromSKU1List = [];
    $scope.GetFromItemMaterialSKU1Data = function (ItemId) {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetFromItemMaterialSKU1Data?ItemId=' + ItemId
        }).then(function successCallback(response) {
            $scope.FromSKU1List = response.data;
        });
    }

    $scope.FromSKU2List = [];
    $scope.GetFromItemMaterialSKU2Data = function (ItemId) {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetFromItemMaterialSKU2Data?ItemId=' + ItemId
        }).then(function successCallback(response) {
            $scope.FromSKU2List = response.data;
        });
    }
    $scope.char1Id = null;
    $scope.ToSKU1List = [];
    $scope.GetToItemMaterialSKU1 = function (materialId) {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetItemMaterialSKUData?materialMasterId=' + materialId + '&sequence=' + '1'
        }).then(function successCallback(response) {
            $scope.ToSKU1List = response.data;
            $scope.char1Id = response.data[0].CharacteristicsId;
        });
    }
    $scope.char2Id = null;
    $scope.ToSKU2List = [];
    $scope.GetToItemMaterialSKU2 = function (materialId) {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetItemMaterialSKUData?materialMasterId=' + materialId + '&sequence=' + '2'
        }).then(function successCallback(response) {
            $scope.ToSKU2List = response.data;
            $scope.char2Id = response.data[0].CharacteristicsId;
        });
    }

    $scope.CopySObyMOI = function () {
        try {
            if (baseService.arrayLength($scope.FromSKU1List) > 0) {
                for (var i = 0; i < $scope.FromSKU1List.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.FromSKU1List[i].ToSKU1Id)) {
                        throw "Select SKU1";
                    }
                }
            }
            if (baseService.arrayLength($scope.FromSKU2List) > 0) {
                for (var i = 0; i < $scope.FromSKU2List.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.FromSKU2List[i].ToSKU2Id)) {
                        throw "Select SKU2";
                    }
                }
            }

            $http({
                method: 'POST',
                url: $scope.path + 'CopySOByMOI',
                data: { 'MasterId': $scope.ToMasterOrderItemId, 'masterItemId': $scope.FromMasterOrderItemId, 'SKU1List': $scope.FromSKU1List, 'SKU2List': $scope.FromSKU2List },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector('#SKUsPopUp')).modal('hide');
                    getSalesOrderList();
                    $scope.getMasterItemList();

                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //#endregion

    //#region Create New SKU

    $scope.characteristicsValueList = [];
    $scope.SKU = null;
    $scope.SKULevel = null;
    $scope.name = null;
    $scope.state = null;

    $scope.AddSKU = function (state, name) {
        $scope.SKU = null;
        $scope.SKULevel = null;

        $scope.name = name;
        $scope.state = state;

        if ($scope.state == '1st') {
            $scope.charId = $scope.char1Id;
            //$scope.SKU = $scope.rmchar1.Name;
            $scope.SKULevel = 'Specific';
        }
        if ($scope.state == '2nd') {
            $scope.charId = $scope.char2Id;
            //  $scope.SKU = $scope.rmchar2.Name;
            $scope.SKULevel = 'Specific';
        }

        $scope.characteristicsValue = {
            Id: baseService.pk()
            , MaterialMasterId: $scope.ToMaterialMasterId
            , CharacteristicsId: $scope.charId
            , Sequence: null
            , Code: null
            , ShortName: null
            , StandardName: null
            , UserName: null
            , SourceType: $scope.SKULevel
            , Description: null
            , Remarks: null
            , IsDefault: false
            , Active: true
        };
        $scope.characteristicsvalueNew = angular.copy($scope.characteristicsValue);
        $scope.GetMaterialMasterCharacteristicsValueSequence();
        angular.element(document.querySelector('#SKUpopup')).modal('show');
    }

    $scope.GetMaterialMasterCharacteristicsValueSequence = function () {
        $http.get('Materials/characteristicsvalue/getautosequence?characteristicsId=' + $scope.charId + '&materialId=' + $scope.ToMaterialMasterId)
            .then(function (response) {
                $scope.characteristicsvalueNew.Sequence = response.data;
            });
    };


    $scope.SaveBOMSKUState = function () {
        if ($scope.state == '1st') {
            $scope.characteristicsvalueNew.SourceType = 'Specific';
        }
        if ($scope.state == '2nd') {
            $scope.characteristicsvalueNew.SourceType = 'Specific';
        }

        if (baseService.isUndefinedOrNull($scope.name)) {
            $scope.SaveBOMSKU();
        }

    }

    $scope.SaveBOMSKU = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.skuForm.$valid) {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/MasterOrder/CreateCharacteristicsValue',
                    data: { 'entity': $scope.characteristicsvalueNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'SKUpopup');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'SKUpopup');

                        if ($scope.state == '1st') {
                            $scope.GetToItemMaterialSKU1($scope.ToMaterialMasterId);
                        }
                        if ($scope.state == '2nd') {
                            $scope.GetToItemMaterialSKU2($scope.ToMaterialMasterId);
                        }

                        // $scope.clearMasterCharacteristicsValue();
                        angular.element(document.querySelector('#SKUpopup')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'SKUpopup');
                }

            }
        } catch (e) {
            ShowResult(e, 'failure', 'SKUpopup');
        }
    };

    $scope.CloseCharacteristicsValuePopUp = function () {
        angular.element(document.querySelector('#SKUpopup')).modal('hide');
    }
    $scope.CloseCharacteristicsValuePopUp1 = function () {
        angular.element(document.querySelector('#SOSKUpopup')).modal('hide');
    }
    $scope.clearMasterCharacteristicsValue = function () {
        $scope.characteristicsValue = {};
        $scope.characteristicsvalueNew = {
            Id: baseService.pk()
            , MaterialMasterId: $scope.ToMaterialMasterId
            , CharacteristicsId: $scope.charId
            , Sequence: 0, Active: true, IsDefault: false
        };
        $scope.GetMaterialMasterCharacteristicsValueSequence();
    }

    $scope.ValueAssign = null;
    $scope.AddSOSKU = function (sku) {
        $scope.skubtn = false;
        if (sku == 1) {
            $scope.charId = $scope.char1Id;
            $scope.charName = $scope.rowName;
            $scope.ValueAssign = $scope.char1ValueAssignmentLevel;
        }
        else {
            $scope.charId = $scope.char2Id;
            $scope.charName = $scope.columnName;
            $scope.ValueAssign = $scope.char2ValueAssignmentLevel;
        }

        $scope.characteristicsValue = {
            Id: baseService.pk()
            , MaterialMasterId: $scope.materialMasterId
            , CharacteristicsId: $scope.charId
            , Sequence: null
            , Code: null
            , ShortName: null
            , StandardName: null
            , UserName: null
            , SourceType: $scope.ValueAssign
            , Description: null
            , Remarks: null
            , IsDefault: false
            , Active: true
        };
        if ($scope.ValueAssign == 'General') {
            $scope.characteristicsValue.MaterialMasterId = null;
        }
        $scope.characteristicsvalueNew = angular.copy($scope.characteristicsValue);
        $scope.GetSOMaterialMasterCharacteristicsValueSequence();
        angular.element(document.querySelector('#SOSKUpopup')).modal('show');
    }

    $scope.GetSOMaterialMasterCharacteristicsValueSequence = function () {
        $http.get('Materials/characteristicsvalue/getautosequence?characteristicsId=' + $scope.charId + '&materialId=' + $scope.characteristicsValue.MaterialMasterId)
            .then(function (response) {
                $scope.characteristicsvalueNew.Sequence = response.data;
            });
    };

    $scope.char1ValueList = [];
    $scope.char2ValueList = [];

    function GetChValueCbo() {
        $http.get($scope.path + 'GetChValueCbo?materialId=' + $scope.materialMasterId)
            .then(function (response) {
                $scope.charValueList = [];
                $scope.char1ValueList = [];
                $scope.char2ValueList = [];
                $scope.charValueList = response.data;

                $scope.char1ValueList = $filter("filter")($scope.charValueList, { "CharacteristicsId": $scope.char1.Id });
                $scope.char2ValueList = $filter("filter")($scope.charValueList, { "CharacteristicsId": $scope.char2.Id });

                $scope.getSkus();
            });
    }

    $scope.getSkus = function () {

        $scope.skuList = [];
        $scope.firstSKUList = [];
        if ($scope.hasFirst === 0) {
            $http.get($scope.path + 'getcharacteristicsbymaterialmasterid?materialMasterId=' + $scope.materialMasterId)
                .then(function (response) {
                    $scope.characteristicsList = [];
                    $scope.characteristicsList = response.data;
                    if (baseService.arrayLength($scope.characteristicsList) === 1) {
                        $scope.firstSKUList = [];
                        $scope.char1Id = $scope.characteristicsList[0].Value;
                        $scope.char1ValueAssignmentLevel = $scope.characteristicsList[0].ValueAssignmentLevel;

                        $scope.addFirstSkuList();
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 2) {
                        $scope.rowName = $scope.characteristicsList[0].Text;
                        $scope.colorCharacteristicsId = $scope.characteristicsList[0].Value;
                        $scope.columnName = $scope.characteristicsList[1].Text;
                        $scope.sizeCharacteristicsId = $scope.characteristicsList[1].Value;

                        $scope.char1Id = $scope.characteristicsList[0].Value;
                        $scope.char2Id = $scope.characteristicsList[1].Value;
                        $scope.char1ValueAssignmentLevel = $scope.characteristicsList[0].ValueAssignmentLevel;
                        $scope.char2ValueAssignmentLevel = $scope.characteristicsList[1].ValueAssignmentLevel;

                        $scope.rowNo = 1;
                        $scope.columnNo = 1;
                        $scope.generate();
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 3) {
                        $scope.rowName = $scope.characteristicsList[1].Text;
                        $scope.columnName = $scope.characteristicsList[2].Text;
                        generateCharPopUp();
                    }
                    if (baseService.arrayLength($scope.characteristicsList) !== 0) {
                        $scope.char1 = {
                            Id: $scope.characteristicsList[0].Value
                            , Name: $scope.characteristicsList[0].Text
                            , CharacteristicsValueId: $scope.characteristicsList[0].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[0].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                            , FirstCharacteristicsId: null
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 1) {
                        $scope.char2 = {
                            Id: $scope.characteristicsList[1].Value
                            , Name: $scope.characteristicsList[1].Text
                            , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[1].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 2) {
                        $scope.char3 = {
                            Id: $scope.characteristicsList[2].Value
                            , Name: $scope.characteristicsList[2].Text
                            , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[2].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };
                    }

                });
        }
        else {
            $http.get($scope.path + 'getAllSkuSalesOrderId?salesOrderId=' + $scope.salesOrderId)
                .then(function (response) {
                    var firstData = response.data.firstData;
                    var secondtData = response.data.secondtData;
                    var thirdData = response.data.thirdData;
                    $scope.characteristicsList = [];

                    if (baseService.arrayLength(firstData) > 0) {
                        $scope.characteristicsList.push({
                            Value: firstData[0].CharacteristicsId
                            , Text: firstData[0].CharacteristicsName
                            , CharacteristicsValueId: null //firstData[0].CharacteristicsValueId
                            , ValueFreeText: null //firstData[0].ValueFreeText
                            , ValueAssignmentLevel: firstData[0].ValueAssignmentLevel
                            , MaterialMasterId: firstData[0].MaterialMasterId
                            , Qty: null //firstData[0].Qty
                            , FirstCharacteristicsId: null //firstData[0].Id
                        });
                    }
                    if (baseService.arrayLength(secondtData) > 0) {
                        $scope.characteristicsList.push({
                            Value: secondtData[0].CharacteristicsId
                            , Text: secondtData[0].CharacteristicsName
                            , CharacteristicsValueId: secondtData[0].CharacteristicsValueId
                            , ValueFreeText: secondtData[0].ValueFreeText
                            , ValueAssignmentLevel: secondtData[0].ValueAssignmentLevel
                            , MaterialMasterId: secondtData[0].MaterialMasterId
                            , Qty: secondtData[0].Qty
                        });
                    }
                    if (baseService.arrayLength(thirdData) > 0) {
                        $scope.characteristicsList.push({
                            Value: thirdData[0].CharacteristicsId
                            , Text: thirdData[0].CharacteristicsName
                            , CharacteristicsValueId: thirdData[0].CharacteristicsValueId
                            , ValueFreeText: thirdData[0].ValueFreeText
                            , ValueAssignmentLevel: thirdData[0].ValueAssignmentLevel
                            , MaterialMasterId: thirdData[0].MaterialMasterId
                            , Qty: thirdData[0].Qty
                        });
                    }

                    if (baseService.arrayLength($scope.characteristicsList) !== 0) {
                        $scope.char1 = {
                            Id: $scope.characteristicsList[0].Value
                            , Name: $scope.characteristicsList[0].Text
                            , CharacteristicsValueId: null //$scope.characteristicsList[0].CharacteristicsValueId
                            , ValueFreeText: null //$scope.characteristicsList[0].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null //$scope.characteristicsList[0].Qty
                            , FirstCharacteristicsId: null //$scope.characteristicsList[0].FirstCharacteristicsId
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 1) {
                        $scope.char2 = {
                            Id: $scope.characteristicsList[1].Value
                            , Name: $scope.characteristicsList[1].Text
                            , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[1].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 2) {
                        $scope.char3 = {
                            Id: $scope.characteristicsList[2].Value
                            , Name: $scope.characteristicsList[2].Text
                            , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[2].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };

                    }

                    if (baseService.arrayLength($scope.characteristicsList) === 3) {
                        $scope.firstSkuEdit(firstData[0]);
                        getSkuMatrix(secondtData, thirdData);
                        $scope.rowName = $scope.characteristicsList[1].Text;
                        $scope.columnName = $scope.characteristicsList[2].Text;

                        $scope.char1Id = $scope.characteristicsList[1].Value;
                        $scope.char2Id = $scope.characteristicsList[2].Value;


                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 2) {
                        getSkuMatrix(firstData, secondtData);
                        $scope.rowName = $scope.characteristicsList[0].Text;
                        $scope.columnName = $scope.characteristicsList[1].Text;

                        $scope.char1Id = $scope.characteristicsList[0].Value;
                        $scope.char2Id = $scope.characteristicsList[1].Value;

                        $scope.char1ValueAssignmentLevel = $scope.characteristicsList[0].ValueAssignmentLevel;
                        $scope.char2ValueAssignmentLevel = $scope.characteristicsList[1].ValueAssignmentLevel;


                        $scope.sumTwoMatQuantity();
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 1) {
                        $scope.firstSKUList = firstData;
                    }
                });
        }

    };


    $scope.skubtn = false;
    $scope.SaveSOSKU = function () {
        try {
            $scope.skubtn = true;
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.skuForm.$valid) {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/BOMMaster/CreateCharacteristicsValue',
                    data: { 'entity': $scope.characteristicsvalueNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'SOSKUpopup');
                        $scope.skubtn = false;
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'SOSKUpopup');
                        GetChValueCbo();
                        angular.element(document.querySelector('#SOSKUpopup')).modal('hide');
                        $scope.skubtn = false;
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'SOSKUpopup');
                }

            }
        } catch (e) {
            ShowResult(e, 'failure', 'SOSKUpopup');
        }
    };

    //#endregion 

    $scope.orderCostingMasterTemplateList = [];
    $scope.ShowOrderCostingMasterTemplatePopUp = function (index, articleId) {
        $scope.orderCostingMasterTemplateList = [];
        $scope.itemIndex = index;
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetOrderCostingMasterTemplateDataByArticle?articleId=' + articleId
        }).then(function successCallback(response) {
            $scope.orderCostingMasterTemplateList = response.data;
            angular.element(document.querySelector('#OrderCostingMasterTemplatePopup')).modal('show');
        });
    };

    $scope.CloseOrderCostingMasterTemplatePopup = function () {
        angular.element(document.querySelector('#OrderCostingMasterTemplatePopup')).modal('hide');
    }

    $scope.SetOrderCosting = function (obj) {
        $scope.itemList[$scope.itemIndex].OrderCostingMasterTemplateId = obj.data.Id;
        $scope.itemList[$scope.itemIndex].OrderCostingMasterTemplate = obj.data.UserName;
        angular.element(document.querySelector('#OrderCostingMasterTemplatePopup')).modal('hide');
        $scope.itemIndex = -1;
    }

    $scope.clearOrderCosting = function (index) {
        $scope.itemIndex = index;
        $scope.itemList[$scope.itemIndex].OrderCostingMasterTemplateId = null;
        $scope.itemList[$scope.itemIndex].OrderCostingMasterTemplate = null;
    }

    $scope.SODataReport = function () {
        try {
            $scope.reportFormat = "Excel";
            $scope.fileName = 'SO Data Report.xls';

            //var gridObj = $("#GridReceiptPaymentStatus").data("ejGrid");
            //var data = gridObj.model.dataSource();

            //var NewReceiptPaymentStatusList = [];
            //for (var i = 0; i < $scope.ReceiptPaymentStatusList.length; i++) {
            //    if ($scope.ReceiptPaymentStatusList[i].isSelected == true) {
            //        if (NewReceiptPaymentStatusList, $scope.ReceiptPaymentStatusList[i].CustomerCode) {
            //            NewReceiptPaymentStatusList.push($scope.ReceiptPaymentStatusList[i].CustomerCode);
            //        }
            //    }
            //}

            $http({
                method: 'POST',
                url: $scope.path + 'SODataReport',
                data: { 'masterOrderId': $scope.fileNew.Id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SODataDetailReport = function () {
        try {
            $scope.reportFormat = "Excel";
            $scope.fileName = 'SO Data Detail Report.xls';

            $http({
                method: 'POST',
                url: $scope.path + 'SODataDetailReport',
                data: { 'masterOrderId': $scope.fileNew.Id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //#region ArticleAlias

    $scope.partyType = 'Party';

    $scope.articleId = null;
    $scope.ind = -1;
    $scope.GetArticleAliasData = function (index) {
        try {
            $scope.ind = index;
            $scope.articleId = $scope.itemList[$scope.ind].ArticleId;
            $scope.ArticleName = $scope.itemList[$scope.ind].ArticleName;
            $scope.MasterOrderItemId = $scope.itemList[$scope.ind].Id;
            if (baseService.isUndefinedOrNull($scope.articleId)) {
                throw "Select Article first.";
            }

            $scope.articleAliasModel = {
                Id: null
                , ArticleId: $scope.articleId
                , MasterOrderItemId: $scope.MasterOrderItemId
                , Code: $scope.fileNew.CustomerCode
                , PartyId: $scope.fileNew.PartyId
                , PartyName: $scope.fileNew.CustomerName
                , ArticlePartyName: $scope.ArticleName
                , UserGroup: null
                , Remark: null
            };
            $scope.articleAlias = Object.assign({}, $scope.articleAliasModel);

            $scope.GetArticleAliasDatas();
            angular.element(document.querySelector('#ArticleAliasPoUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.articleAlias.PartyName = party.UserName;
        $scope.articleAlias.PartyId = party.Id;
        $scope.articleAlias.Code = party.Code;

        $scope.hidePartyPopUp();
    };


    $scope.aliasList = [];
    $scope.GetArticleAliasDatas = function () {
        $http({
            method: 'GET',
            url: 'Materials/materialmasterarticle/getArticleAliaslist?articleId=' + $scope.articleId + '&masterOrderItemId=' + $scope.MasterOrderItemId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.articleAlias = Object.assign({}, response.data[0]);
                $scope.itemList[$scope.ind].CustomerArticle = response.data[0].ArticlePartyName;
            }
        });
    }
    $scope.articleAliasModel = {
        Id: null
        , ArticleId: $scope.articleId
        , MasterOrderItemId: null
        , Code: $scope.fileNew.CustomerCode
        , PartyId: $scope.fileNew.PartyId
        , PartyName: $scope.fileNew.CustomerName
        , ArticlePartyName: null
        , UserGroup: null
        , Remark: null
    };
    $scope.articleAlias = Object.assign({}, $scope.articleAliasModel);


    $scope.closePartyPopUp = function (x) {
        var party = x.data;

        $scope.articleAlias.PartyName = party.UserName;
        $scope.articleAlias.PartyId = party.Id;
        $scope.articleAlias.Code = party.Code;

        $scope.hidePartyPopUp();
    };

    $scope.GetArticleAlias = function (args) {

        $scope.articleAlias = Object.assign({}, args);
        $scope.GetArticleAliasDatas(args.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.SaveArticleAlias = function () {
        try {
            $scope.articleAlias.Id = baseService.isUndefinedOrNull($scope.articleAlias.Id) == null ? null : $scope.articleAlias.Id;
            $scope.articleAlias.ArticleId = $scope.articleId;
            $scope.articleAlias.MasterOrderItemId = $scope.MasterOrderItemId;
            if (baseService.isUndefinedOrNull($scope.articleAlias.ArticleId)) {
                throw "Article is required.";
            }
            if (baseService.isUndefinedOrNull($scope.articleAlias.PartyId)) {
                throw "Party is required.";
            }
            if (baseService.isUndefinedOrNull($scope.articleAlias.ArticlePartyName)) {
                throw "Article Party Name is required.";
            }
            if (baseService.isUndefinedOrNull($scope.articleAlias.UserGroup)) {
                throw "User Group is required.";
            }

            $http({
                method: 'POST',
                url: 'Materials/materialmasterarticle/CreateArticleAlias',
                data: { 'data': $scope.articleAlias },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetArticleAliasDatas();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure', 'ArticleAliasPoUp');
        }
    };


    //#endregion

    $scope.GetAddinfoPopUp = function (x) {
        try {
            $scope.MOIId = x.Id;
            angular.element(document.querySelector('#addInfoPopUp')).modal('show');
        } catch (e) {
        }
    }
}


