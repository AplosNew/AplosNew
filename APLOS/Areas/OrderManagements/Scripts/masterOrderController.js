'use strict';
masterOrderController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function masterOrderController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
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

    $scope.path = 'OrderManagements/masterorder/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListResponsible';
    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $controller("MasterOrderTaskTemplateController", { cboService: cboService, $scope: $scope, $http: $http });
    $controller("TaskScheduleController", { cboService: cboService, $scope: $scope, $http: $http });


    // $scope.ExchangeRateTableName = 'MasterOrderExchangeRates';//very important to provide the table where the exchange rates will be saved
    $controller("CurrencyExchangeController", { cboService: cboService, $scope: $scope, $http: $http, TableName: 'MasterOrderExchangeRates' });

    //$controller('employeeBaseController', { $scope: $scope, $http: $http });

    $scope.getData = function () {
        baseService.setCurrentPage('files');
        $rootScope.parameters.companyId = $scope.fileNew.CompanyId;
        // baseService.init($scope.getListUrl, null, null, null, 'CONVERT(int,MasterOrderNo)', 'MasterOrderNo');
        baseService.init($scope.getListUrl, null, null, "DESC", 'AddedDate', 'MasterOrderNo');
        $scope.loadMasterData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.files = result.Rows;
                    if (baseService.arrayLength($scope.searchMasterFilterList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchMasterFilterList);

                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMasterData();
    };

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
    };
    $scope.fileNew = Object.assign({}, $scope.file);
    $scope.isBuyerApplicable = false;

    // #region Ddl
    $scope.typeList = [
        { Value: "Manufacture", Text: "Manufacture" },
        { Value: "Trading", Text: "Trading" },
        { Value: "JobWork", Text: "Job Work" },
        { Value: "OutSource", Text: "Out Source" }
    ];

    $scope.ProductLibraryList = [];
    $scope.GetProductLibraryCbo = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetProductLibrary/'
        }).then(function successCallback(response) {
            $scope.ProductLibraryList = response.data;
        });
    };
    $scope.GetProductLibraryCbo();

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
        //cboService.getCboProductionEntityByPlant(null, null, $scope.fileNew.PlantId, function (result) {
        //    $scope.entityList = result;
        //    $scope.GetResponsiblePersonList();
        //});
        cboService.getCboProductionEntitiesByPlant($scope.fileNew.PlantId, function (result) {
            $scope.entityList = result;
        });
    };

    //$scope.getAllEntities = function () {
    //    $http({
    //        method: 'POST',
    //        url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
    //    }).then(function successCallback(response) {
    //        $scope.entityList = response.data;
    //        //$scope.GetResponsiblePersonList();
    //    });
    //}
    //$scope.getAllEntities();


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
        if ($scope.fileNewForm.$valid) {

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


    $scope.currency = null;
    $scope.Get = function (index) {
        $scope.getPlantConfigByPlant();
        $scope.index = index;
        angular.copy($scope.files[$scope.index], $scope.file);
        $scope.file.IsExtraOrderPercentage = $scope.file.ExtraOrderPercentage > 0;
        angular.copy($scope.file, $scope.fileNew);
        $scope.fileNew.OrderYear = parseInt($scope.fileNew.OrderYear);
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
        $scope.GetContractByMasterOrder();
    };


    $scope.GetPaymentTermChangeable = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/masterOrder/GetPaymentTermChangeable?CompanyId=' + $scope.fileNew.CompanyId + '&PartyId=' + $scope.fileNew.PartyId
        }).then(function successCallback(response) {
            $scope.fileNew.IsPaymentTermChangeable = response.data[0].IsPaymentTermChangeable;
        });
    }



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
        if ($scope.fileNewForm.$valid) {

            if ($scope.ExchangeSaveExchangeRates($scope.fileNew.CurrencyId) == false) {
                return;
            }

            if ($scope.Action === "Save") {

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
                        'entity': $scope.file, 'taskList': $scope.taskList, 'CurrencyData': $scope.ExchangeDisplayCurrency
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
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
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                for (var i = 0; i < baseService.arrayLength($scope.itemList); i++) {
                    if (baseService.isUndefinedOrNull($scope.itemList[i].MaterialMasterId))
                        return ShowResult('Material master need in row number ' + (i + 1), 'failure');
                    if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
                        $scope.itemList[i].ContractId = $scope.modelNew.Id;
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
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');


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
            , PlantId: null
            , OrderType: 'ExternalOrder'
            , PartyId: $scope.fileNew.PartyId
            , CompanyId: $scope.fileNew.CompanyId
        };
        $scope.getPlantConfigByPlant();
        $scope.SpecialTax = false;
        $scope.mmChangeFlag = false;
        $scope.customerName = null;
        $scope.ExchangeReset();
        $scope.enableJobOrOutSource = true;
        $scope.modelNew = Object.assign({}, $scope.model);
    }

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

        $scope.fileNew.PaymentTermId = party.PaymentTermId;
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



    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.fileNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.fileNew.PaymentTermId; })[0];
            $scope.fileNew.PaymentTermDays = paymentTerm.NoOfDay;
        }
    };

    $scope.paymentTermList = [];
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getcustomercbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });


    //#region ResponsiblePerson
    //$scope.GetResponsiblePersonList = function () {
    //    $scope.personList = [];
    //    $http.get($scope.path + "GetResponsiblePersonList?masterId=" + $scope.fileNew.Id)
    //        .then(function (response) {
    //            $scope.personList = response.data;
    //            if ($scope.fileNew.PlantId !== null && ($scope.personList === null || $scope.personList.length <= 0)) {
    //                $scope.popUpUrl = $scope.path + "GetDepartmentPersonList?plantId=" + $scope.fileNew.PlantId + '&partyAccountGroupId=' + $scope.fileNew.PartyAccountGroupId + '&partyId=' + $scope.fileNew.PartyId + '&flag=' + false;
    //                $scope.getPopUpData = function (pageno) {
    //                    baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
    //                        .then(function (result) {
    //                            if (baseService.arrayLength(result) !== 0) {
    //                                for (var i = 0; i < result.length; i++) {
    //                                    var obj = result[i];
    //                                    $scope.personList.push({
    //                                        Id: obj.Id
    //                                        , MasterOrderId: $scope.fileNew
    //                                        , CustomerDivisionId: obj.CustomerDivisionId
    //                                        , OrderResponsibleDepartmentId: obj.OrderResponsibleDepartmentId
    //                                        , Department: obj.Department
    //                                        , OurRespnsiblePersonId: obj.OurRespnsiblePersonId
    //                                        , EmployeeCode: obj.EmployeeCode
    //                                        , EmployeeName: obj.EmployeeName
    //                                        , PartyRespnsiblePersonId: obj.PartyRespnsiblePersonId
    //                                        , PartyRespnsiblePerson: obj.PartyRespnsiblePerson
    //                                    });
    //                                }
    //                                GetDepartmentPersonCbo();
    //                            }
    //                        }, function () {
    //                            ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
    //                        }).finally(function () {
    //                        });
    //                };
    //                $scope.getPopUpData();
    //            }
    //        });
    //};
    //#endregion

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
        $scope.getArticleSearchList($scope.itemList[$scope.itemIndex].MaterialMasterId);
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.itemList[$scope.itemIndex].MaterialMasterId = ob.MaterialMasterId;
            $scope.itemList[$scope.itemIndex].MaterialMasterName = ob.MaterialMasterName;
            $scope.itemList[$scope.itemIndex].ArticleId = ob.Id;
            $scope.itemList[$scope.itemIndex].ArticleName = ob.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
            $scope.itemIndex = -1;
            $scope.mmChangeFlag = true;
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

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
            else if ($scope.Name === 'boq') {
                $scope.qboqModel.ResponsiblePersonId = employee.SystemId;
                $scope.qboqModel.ResponsiblePersonName = employee.EmployeeName;
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
    };

    $scope.enableJobOrOutSource = true;
    $scope.ChangeJobType = function (Type) {
        if (Type == "JobWork" || Type == "OutSource") {
            $scope.enableJobOrOutSource = false;
        } else {

            $scope.enableJobOrOutSource = true;
        }
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
            list[index].EntityOrVendorName = data.Name;
        }
        else if (list[index].JobWorkType === 'EntityWithinGroup') {
            list[index].EntityIdWithinGroup = data.Id;
            list[index].EntityOrVendorName = data.Name;
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
                if (baseService.arrayLength($scope.itemList) > 0) {
                    for (var i = 0; i < $scope.itemList.length; i++) {
                        if ($scope.itemList[i].Type == 'JobWork' || $scope.itemList[i].Type == 'OutSource') {
                            $scope.enableJobOrOutSource = false;
                        } else {
                            $scope.enableJobOrOutSource = true;
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

            $scope.openPopup('CostingPopUp');
        } catch (e) {
        }
    }

    $scope.OrderPreCosting = function () {
        try {
            $scope.PreCosting = 1;
            var file_src = $scope.CostingPath + 'GetOrderCostingReport?OrderCostingId=' + $scope.OrderCostingId + '&preCosting=' + $scope.PreCosting;
            $rootScope.report(file_src);

        } catch (e) {
        }
    }
    $scope.OrderProcurementCosting = function () {
        try {
            $scope.ProcurementCosting = 1;
            var file_src = $scope.CostingPath + 'GetOrderCostingReport?OrderCostingId=' + $scope.OrderCostingId + '&procurementCosting=' + $scope.ProcurementCosting;
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

    $scope.getSalesOrder = function (id, materialMasterId, mName, aName, hsnCodeId, BuyerReferenceNo) {
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
            , HSNCodeId: hsnCodeId
            , TotalTaxAmount: 0
            , MainRawMaterialInhouseDate: null
            , OtherRawMaterialInhouseDate: null
            , SalesOrderYear: null
            , WeekNo: null
            , PlanExFactoryDate: null
            , QtyChangedBy: null
            , QtyChangedDate: null
            , QtyChangedFromIP: null
            , DestinationDescription: null
            , SalesExpense: null
            , NetSalesRealization: null
        };
        getSalesOrderList();
        $scope.getDestination();
        $scope.GetContractPercentage(id);
        angular.element(document.querySelector('#soPoUp')).modal('show');
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

    $scope.ProdBookedQty = 0;
    $scope.TotalProducedQty = 0;
    $scope.GetSOBookedQtyAndLevel = function (salesOrderId) {
        //$scope.TotalProducedQty = 0;
        //$scope.ProdBookedQty = 0;
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

    $scope.saveSalesOrder = function () {

        //if ($scope.soModel.PONumber === null || $scope.soModel.OrderStatusId === null || $scope.soModel.OrderCategoryId === null || $scope.soModel.DestinationId === null || $scope.soModel.ShipmentModeId === null || $scope.soModel.Qty === null) {
        //    ShowResult("Please enter mandatory fields", 'failure', 'soPoUp');
        //    return false;
        //}
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

        $scope.$broadcast('show-errors-check-validity');

        if ($scope.soForm.$valid) {
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
                        clearSO();
                        $scope.getMasterItemList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'soPoUp');
                };
            } else {
                getSalesOrderTaxCategoryUpdateList($scope.soModel.Id);
            }
        }
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
                clearSO();
                $scope.getMasterItemList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'soPoUp');
        };
    }

    $scope.delivaryDate = null;
    $scope.soEdit = function (data) {
        $scope.TotalProducedQty = 0;
        angular.copy(data, $scope.soModel);
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
        angular.element(document.querySelector('#soPoUp')).modal('hide');
    };

    function clearSO() {
        $scope.soModel = {
            Id: null
            , MasterOrderItemId: $scope.masterItemId
            , DeliveryDate: null
            , CommitmentDate: null
            , DestinationId: null
            , ShipmentModeId: null
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
            , MainRawMaterialInhouseDate: null
            , OtherRawMaterialInhouseDate: null
            , CM: 0
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
    };

    $scope.SplitSO = function (data) {
        $scope.soSplitModel.Id = null
        $scope.soSplitModel.MasterOrderItemId = $scope.masterItemId
        $scope.soSplitModel.DeliveryDate = data.DeliveryDate;
        $scope.soSplitModel.DestinationId = data.DestinationId;
        $scope.soSplitModel.CommitmentDate = data.CommitmentDate;
        $scope.soSplitModel.ShipmentModeId = data.ShipmentModeId;
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

    // #endregion Split Sales Order

    // #endregion Sales Order

    // #region Sales Order Tax

    $scope.TaxAction = 'Save';

    accountService.getTaxCategoryCbo(" ", function (result) {
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
        $scope.soModel.CustomerPOId = id;
        $scope.soModel.PONumber = poNumber;
        angular.element(document.querySelector('#poSearchPopup')).modal('hide');
    };

    $scope.poFgEntryPopup = function () {
        $scope.poModel = {
            Id: null
            , PONumber: null
            , CustomerId: $scope.fileNew.PartyId
            , CompanyGroupId: $window.companyGroupId
            , CompanyId: $window.companyId
            , MasterOrderId: $scope.fileNew.Id
            , PODate: null
            , Active: null
        };
        angular.element(document.querySelector('#poEntryPopup')).modal('show');
    };

    $scope.SavePO = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.poModel.PONumber)) throw "[PO No] can not be blank...";
            if (baseService.isUndefinedOrNull($scope.poModel.PODate)) throw "[PO Date] can not be blank...";
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

    $scope.getSku = function (salesOrderId, hasFirst, soItemQty) {
        $scope.salesOrderId = salesOrderId;
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
                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('show');
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 2) {
                        getSkuMatrix(firstData, secondtData);
                        $scope.rowName = $scope.characteristicsList[0].Text;
                        $scope.columnName = $scope.characteristicsList[1].Text;
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
        $http.get($scope.path + 'GetChValueCbo?materialId=' + $scope.materialMasterId)
            .then(function (response) {
                $scope.charValueList = [];
                $scope.charValueList = response.data;
            });
    };

    function generateCharPopUp() {
        angular.element(document.querySelector('#generatePopup')).modal('show');
    }

    $scope.generate = function () {
        var firstCharId = '';
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

        angular.element(document.querySelector('#generatePopup')).modal('hide');
        if (baseService.arrayLength($scope.characteristicsList) === 3)
            angular.element(document.querySelector('#thirdPopup')).modal('show');
        else
            angular.element(document.querySelector('#secondPopup')).modal('show');
    };

    function getSkuMatrix(rowDataList, columnDataList) {
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
                    $scope.getSalesOrder($scope.masterItemId, $scope.materialMasterId, $scope.mName, $scope.BuyerReferenceNo);
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

    $scope.chvChange = function (characteristicsValueId, index) {
        for (var i = 1; i < baseService.arrayLength($scope.skuList); i++) {
            $scope.skuList[i].childList[index].CharacteristicsValueId = characteristicsValueId;
        }
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
                    $scope.getMasterItemList();
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
        $scope.qboqModel.GrossConsumption = parseFloat($scope.qboqModel.NetConsumptionPerUnit / (1 - ($scope.qboqModel.ValueLossPercentage / 100))).toFixed(2);
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
            //if ($scope.qboqModel.IsOutSource) {

            //}
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

    //#region Quick BOQ Job Work Type
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


    //#endregion QBOQ

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

    $scope.showPartyPopUpNew = function () {
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
            url: 'Commercial/LCFundUtilization/GetFundUtilizationList',
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

    $scope.buyerDeductionList = [];
    cboService.getEnumCbo("enum/GetBuyerDeductionEnumCbo", function (result) {
        $scope.buyerDeductionList = result;

        $scope.getBuyerDeductionData();
    });

    $scope.dataList = [];
    $scope.getBuyerDeductionData = function () {
        $http({
            method: 'POST',
            url: 'Commercial/LCFundUtilization/GetBuyerDeductionList',
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.dataList = response.data;
                for (var i = 0; i < $scope.dataList.length; i++) {
                    for (var j = 0; j < $scope.buyerDeductionList.length; j++) {
                        if ($scope.dataList[i].FundUtilization == $scope.buyerDeductionList[j].Text) {

                            $scope.buyerDeductionList[j].FundUtilization = $scope.dataList[i].FundUtilization;
                            $scope.buyerDeductionList[j].FundUtilizationText = $scope.dataList[i].FundUtilizationText;
                            $scope.buyerDeductionList[j].Percentage = $scope.dataList[i].Percentage;
                            $scope.buyerDeductionList[j].OldPercentage = $scope.dataList[i].Percentage;
                            $scope.buyerDeductionList[j].CurrencyId = $scope.dataList[i].CurrencyId;
                        }
                    }
                }
            }
        });
    };

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

    $scope.SaveContract = function () {
        try {


            if (baseService.isUndefinedOrNull($scope.modelNew.MasterOrderId)) {
                $scope.modelNew.MasterOrderId = $scope.fileNew.Id;
            }



            $scope.modelNew.Amount = $scope.modelNew.Amount.toFixed(2);
            $scope.modelNew.Amount = parseFloat($scope.modelNew.Amount);
            $scope.saveFunds = [];

            for (var i = 0; i < $scope.buyerDeductionList.length; i++) {
                $scope.saveFunds.push($scope.buyerDeductionList[i]);
            }

            for (var i = 0; i < $scope.fundUtilizationList.length; i++) {
                $scope.saveFunds.push($scope.fundUtilizationList[i]);
            }

            for (var i = 0; i < $scope.saveFunds.length; i++) {
                if (baseService.isUndefinedOrNull($scope.saveFunds[i].Id)) {
                    $scope.saveFunds[i].Id = null;
                }
                if ($scope.saveFunds[i].Percentage !== $scope.saveFunds[i].OldPercentage) {
                    if (baseService.isUndefinedOrNull($scope.saveFunds[i].Reason)) {
                        throw "Reason is required for " + $scope.saveFunds[i].FundUtilizationText + "";
                    }
                }
            }


            if (baseService.isUndefinedOrNull($scope.modelNew.ContractNo)) {
                throw "ContractNo is required.";
            }
            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/MasterOrder/CreateContract',
                    data: {
                        'model': $scope.modelNew
                        , 'funds': $scope.saveFunds
                        , 'masterOrderItem': $scope.itemList
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.modelNew.Id = response.data.Id;

                        //$scope.GetContractByMasterOrder();
                        $scope.GetContractFundData($scope.modelNew.Id);
                        $scope.getMasterItemList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    //#endregion Contract
}


