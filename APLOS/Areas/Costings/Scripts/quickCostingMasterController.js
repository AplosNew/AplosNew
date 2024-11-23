'use strict';
quickCostingMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$controller', '$filter', 'cboService', '$window', 'fileReader'];
function quickCostingMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $controller, $filter, $cboService, $window, fileReader) {
    $rootScope.title = 'Costing Template';
    $scope.ModelList = [];
    $scope.path = 'Costings/quickCostingMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.Action = 'Save';
    $scope.searchBy = "UserName"; $scope.search = "";
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.partyType = 'Customer';
    $scope.piemarker = { dataLabel: { visible: true, shape: 'none', connectorLine: { type: 'bezier', color: 'black' }, font: { size: '14px' } } };

    $scope.CostingSummaryDataMain = { BuyerTotal: 0, QuickCostingValue: 0, PreCostingValue: 0, ProfitQuickCosting: 0, ProfitPreCosting: 0 };
    $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

    $scope.tranCurrencyList = [];
    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.TotalSegmentedCostValue = 0;
    $scope.CostingComponentId = null;
    $scope.PurchaseGroupList = [];
    $scope.BOQCriteriaList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data.DATA;
            $scope.PurchaseGroupList = response.data.PurchaseGroup;

        });

        $http({
            method: 'POST',
            url: "Costings/BOQCriteria/GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BOQCriteriaList = response.data;
        });

    }
    $scope.getData();


    $scope.getArticlePopUp = function () {
        $scope.getMaterialMasterWithArticle(null);
    }
    $scope.setInputeMaterialArticleData = function (ob) {
        try {
            $scope.ModelNew.ArticleId = ob.data.Id;
            $scope.ModelNew.Article = ob.data.StandardName;
            angular.element(document.querySelector('#materialarticleNewPopUp')).modal('hide');

        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.ProductUOM = [];
    //cboService.getUnitOfMeasurementCbo(function (response) {
    //    $scope.ProductUOM = response;

    //});

    $scope.GetUoMCboByProductMaster = function () {
        $http({
            method: 'GET',
            url: 'Costings/QuickCostingMaster/GetProductUOMCbo?ProductMasterId=' + $scope.ModelNew.ProductMasterId
        }).then(function successCallback(response) {
            $scope.ProductUOM = response.data;
        });
    };

    $scope.GetDirectProcessRateValue = function (args) {
        if (args == null)
            return;

        $http({
            method: 'POST',
            url: $scope.path + "GetDirectProcessRateValue",
            data: { CostingItemId: args.CostingItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            args.Value = response.data[0].ValueLossPercentage;
            if (args.ExecutionType == 'Inside')
                args.Rate = response.data[0].InternalRate;
            else
                args.Rate = response.data[0].ExternalRate;


            $scope.CalculateFinalCosting(args);
        });


    }

    $scope.UpChargeMatrix = [];
    $scope.calculateSPT = function () {
        if ($scope.ModelNew.TargetOrSPT == 'SPT') {
            $scope.ModelNew.MKTTargetPerHour = 0;
            if ($scope.ModelNew.SPT > 0) {
                $scope.ModelNew.MKTTargetPerHour = $scope.ModelNew.NoOfWorkstation * 60 / $scope.ModelNew.SPT * ($scope.ModelNew.EfficiencyPercentage / 100);
                $scope.ModelNew.MKTTargetPerHour = $scope.ModelNew.MKTTargetPerHour.toFixed(2);
            }
        }
        else {
            $scope.ModelNew.SPT = 0;
            if ($scope.ModelNew.MKTTargetPerHour > 0 && $scope.ModelNew.EfficiencyPercentage > 0) {
                $scope.ModelNew.SPT = ($scope.ModelNew.NoOfWorkstation * 60) / ($scope.ModelNew.MKTTargetPerHour / ($scope.ModelNew.EfficiencyPercentage / 100));
                $scope.ModelNew.SPT = $scope.ModelNew.SPT.toFixed(2);
            }

        }

        $scope.ModelNew.WorkCenterTargetPerDay = ($scope.ModelNew.MKTTargetPerHour * $scope.ModelNew.StandardWorkingHours).toFixed(2);

        var costingItem = ej.DataManager($scope.QuickCostingItemList).executeLocal(ej.Query().where("Code", "equal", 'CM'));
        var CMValue = 0;
        if (costingItem.length > 0) {
            costingItem[0].Value = 0;
            costingItem[0].TotalGrossAmount = 0;

            if ($scope.ModelNew.MKTTargetPerHour > 0) {
                var additionalCost = 0;

                if ($scope.ModelNew.StandardWorkingHours > $scope.ModelNew.StandardWorkingHoursForProduct) {
                    additionalCost = ($scope.ModelNew.StandardWorkingHours - $scope.ModelNew.StandardWorkingHoursForProduct) * $scope.ModelNew.AdditionalWorkingHourCostPerHour;
                }


                CMValue = ($scope.ModelNew.StandardWorkingHourCost + additionalCost) /
                    $scope.ModelNew.StandardWorkingHours /
                    $scope.ModelNew.MKTTargetPerHour;
            }
            costingItem[0].Value = CMValue;
            costingItem[0].TotalGrossAmount = CMValue;
        }
        var costingItem = ej.DataManager($scope.QuickCostingItemList).executeLocal(ej.Query().where("Code", "equal", 'UPC'));
        if (costingItem.length > 0) {
            costingItem[0].Value = 0;
            costingItem[0].TotalGrossAmount = 0;

            try {

                var _workdays = $scope.ModelNew.OrderSize /
                    $scope.ModelNew.StandardWorkingHours /
                    $scope.ModelNew.MKTTargetPerHour;

                var WorkDysRequired = Math.ceil(_workdays);

                var UpChargeMatrix = ej.DataManager($scope.UpChargeMatrix).executeLocal(ej.Query().where("WorkCenterDays", "equal", WorkDysRequired));
                if (UpChargeMatrix.length == 0)
                    UpChargeMatrix = ej.DataManager($scope.UpChargeMatrix).executeLocal(ej.Query().where("WorkCenterDays", "lessorequal", WorkDysRequired));

                if (UpChargeMatrix.length > 0) {
                    CMValue = CMValue * (UpChargeMatrix[0][$scope.ModelNew.CriticalLevel] / 100);
                }

                costingItem[0].Value = CMValue;
                costingItem[0].TotalGrossAmount = CMValue;

            } catch (e) {

            }
        }
        $scope.CalculateFinalCosting(null);

    }
    $scope.CostingItemList = [];
    $scope.AddNewCostingItem = function () {
        try {
            if (angular.isUndefinedOrNull($scope.CostingMasterTemplateId))
                throw 'Please save the costing master first';
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "GetCostingItemForSelection",
                    data: { CostingMasterTemplateId: $scope.CostingMasterTemplateId, costingComponentId: $scope.CostingComponentId, Segment: $scope.Segment },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.CostingItemList = response.data;
                });
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }


    $scope.SaveCostingItemsForCostingComponent = function () {
        try {

            //var Sequenc = $scope.DirectMaterialList.length + 1;

            if (angular.isUndefinedOrNull($scope.CostingMasterTemplateId))
                throw 'Please save the costing master first';

            $http({
                method: 'POST',
                url: $scope.path + "SaveCostingItemsForCostingComponent",
                data: { itemList: $scope.CostingItemList, CostingMasterTemplateId: $scope.CostingMasterTemplateId, costingComponentId: $scope.CostingComponentId, Segment: $scope.Segment },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.AfterAddRemoveCostingItem();

            });
        } catch (e) {
            ShowResult(e, "failure");
        }

    }


    $scope.CostingMasterTemplateId = null;
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
        CostingTypeName: null,
        TargetCM: 0,
        TargetProfit: 0,
        IsPercentage: 'true',

        UOM: null,
        TargetOrSPT: 'SPT',
        CriticalLevel: null,
        SPT: 0,
        NoOfWorkstation: 0,
        EfficiencyPercentage: 0,
        StandardWorkingHours: 0,
        StandardWorkingHoursForProduct: 0,
        WorkCenterTargetPerDay: 0,
        StandardWorkingHourCost: 0,
        AdditionalWorkingHourCostPerHour: 0


    };
    $scope.ModelNew = Object.assign({}, $scope.ModelMain);
    $scope.PreCostingDirectMaterial = {
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
        CostingMasterTemplateId: null,
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

        $http({
            method: 'POST',
            url: $scope.path + "GetListItem",
            data: { Id: args.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BackToQuickCostingComponent();

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

            $scope.CostingMasterTemplateId = $scope.ModelNew.Id;

            $scope.getBuyerData();
            $scope.getLatestVersion();
            $scope.SumCostingValue();
            $scope.CalculateProfit();
            $scope.GetUoMCboByProductMaster();
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
                $scope.BackToQuickCostingComponent();

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
                        $scope.CostingMasterTemplateId = response.data.data.Id;
                        // ClearFields(response.data.Sequence);
                        $scope.getData();
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
            $scope.BackToQuickCostingComponent();

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
        $scope.SummaryBySegmentList = [];
        $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);
        //$("#graphdivComparison").ejChart("redraw");
        //$("#graphdivBuyerTarget").ejChart("redraw");
        //$("#graphdivQuickCosting").ejChart("redraw");
        //$("#graphdivPreCosting").ejChart("redraw");
        $scope.tabQ = 1;

        var chartObj = $("#graphdivComparison").data("ejChart");
        chartObj.redraw();
        chartObj = $("#graphdivBuyerTarget").data("ejChart");
        chartObj.redraw();
        chartObj = $("#graphdivQuickCosting").data("ejChart");
        chartObj.redraw();
        chartObj = $("#graphdivPreCosting").data("ejChart");
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
    $scope.showPartyPopUp = function () {
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
            $scope.ModelNew.Customer = party.UserName;
            $scope.ModelNew.CustomerId = party.Id;
            angular.element(document.querySelector('#partyPopUp')).modal('hide');

        }

    };

    //#endregion End customer info

    $scope.getLatestVersion = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "GetVersion?CostingMasterTemplateId=" + $scope.ModelNew.Id,
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
        $scope.VersionModelNew.CostingMasterTemplateId = $scope.ModelNew.Id;
        $http({

            method: 'POST',
            url: 'Costings/quickCostingMaster/CreateCostingDetail',
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
                url: 'Costings/quickCostingMaster/DeleteCostingDetail?id=' + $scope.buyerMsterActivityId
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


            $scope.ModelNew.ProductSubCategory = response.data.Product[0].ProductSubCategory;
            $scope.ModelNew.CostingType = response.data.Product[0].CostingType;
            $scope.ModelNew.CostingTypeName = response.data.Product[0].CostingTypeName;
            $scope.ModelNew.ProductCategory = response.data.Product[0].ProductCategory;

            $scope.ModelNew.NoOfWorkstation = response.data.Product[0].NoOfWorkstation;
            $scope.ModelNew.EfficiencyPercentage = response.data.Product[0].EfficiencyPercentage;
            $scope.ModelNew.StandardWorkingHours = response.data.Product[0].StandardWorkingHours;
            $scope.ModelNew.StandardWorkingHoursForProduct = response.data.Product[0].StandardWorkingHours;

            $scope.ModelNew.SPT = response.data.Product[0].SPT;

            $scope.ModelNew.StandardWorkingHourCost = response.data.Product[0].StandardWorkingHourCost;
            $scope.ModelNew.AdditionalWorkingHourCostPerHour = response.data.Product[0].AdditionalWorkingHourCostPerHour;

            $scope.ModelNew.StandardWorkingHourCost = response.data.Product[0].StandardWorkingHourCost;
            $scope.ModelNew.AdditionalWorkingHourCostPerHour = response.data.Product[0].AdditionalWorkingHourCostPerHour;

            $scope.calculateSPT();

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
            $scope.UpChargeMatrix = response.data.UpChargeMatrix;
            $scope.MakeSummaryBySegment();


        });
    }
    $scope.AfterAddRemoveCostingItem = function () {


        $http({
            method: 'GET',
            url: $scope.path + "GetQuickCostingDetailByProductMaster?ProductMasterId=" + $scope.ModelNew.ProductMasterId + "&CostingVersionMasterTemplateId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.QuickCostingDetailList = response.data.ComponentList;
            $scope.QuickCostingItemList = response.data.ItemList;
            $scope.MakeSummaryBySegment();

            $scope.NavigateToPreCosting($scope.SelectedQuickCostingComponent);
        });

    }

    $scope.PiechartData = [];
    $scope.tabQ = 1;
    $scope.setTabQ = function (newTab) {
        $scope.tabQ = newTab;
        if (newTab == 3) {

            //for (var i = 0; i < $scope.QuickCostingDetailList.length; i++) {
            //    if ($scope.QuickCostingDetailList[i].BuyerTarget > 0 ? $scope.QuickCostingDetailList[i].BuyerTarge : $scope.QuickCostingDetailList[i].BuyerTarget = 0);
            //    if ($scope.QuickCostingDetailList[i].CostingValue > 0 ? $scope.QuickCostingDetailList[i].CostingValue : $scope.QuickCostingDetailList[i].CostingValue = 0);
            //    if ($scope.QuickCostingDetailList[i].TotalGrossAmount > 0 ? $scope.QuickCostingDetailList[i].TotalGrossAmount : $scope.QuickCostingDetailList[i].TotalGrossAmount = 0);
            //}


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
    $scope.PreCostingDetail = {
        Id: null,
        Sequence: 0,
        CostingItemId: null,
        PreCostingVersionMasterId: null,
        CostingValue: 0,
        BuyerTarget: 0
    };
    $scope.AddCostingItem = function () {

        $scope.CostingItemList.push($scope.CostingItem);
    };
    $scope.SaveCostingItemsIncludingComponent = function () {
        $http({
            method: 'POST',
            url: 'Costings/quickCostingMaster/SaveCostingItemsIncludingComponent',
            data: { costingItems: $scope.CostingItemList, preCostingDetail: $scope.PreCostingDetail },
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


    $scope.getCostingItemByComponentId = function () {
        $scope.data = [];
        $http({
            method: 'GET',
            url: 'Costings/quickCostingMaster/GetCostingItemByComponentId?costingComponentId=' + $scope.CostingComponentId,

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CostingItemList = response.data;


        });
    };

    $scope.showCostingItemListPopUp = function () {
        $scope.getCostingItemByComponentId();
        angular.element(document.querySelector("#costingItemListPopUp")).modal("show");
    };


    $scope.GetDirectCostingMaterialWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/quickCostingMaster/GetDirectCostingMaterialWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&costingMasterTemplateId=' + $scope.ModelNew.Id,
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
        $scope.GetDirectCostingMaterialWithItemByComponentId();
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
                        data.GrossConsumption = data.Consumption / ((100 - data.ValueLoss) / 100); //(data.Consumption * data.ValueLoss / 100) + data.Consumption;
                        data.GrossAmount = data.GrossConsumption * data.Rate;
                        $scope.QuickCostingItemList[i].TotalGrossAmount = data.GrossConsumption * data.Rate;


                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'Operation') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;

                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'DirectProcess') {
                        //first push the 
                        $scope.QuickCostingItemList[i].Rate = data.Rate;
                        $scope.QuickCostingItemList[i].Value = data.Value;


                        var totalPre = getFixedAmountDirectMaterial();

                        $scope.QuickCostingItemList[i].TotalGrossAmount = (totalPre / ((100 - data.Value) / 100)) - totalPre;// totalPre * (data.Value / 100)
                        $scope.QuickCostingItemList[i].TotalGrossAmount += data.Rate;

                        $scope.QuickCostingItemList[i].Rate = data.Rate;
                        $scope.QuickCostingItemList[i].Value = data.Value;

                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'SalesExpense') {


                        if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {
                            var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                            var totalCurr = getCurrentFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                            var totalPercent = getCurrentPercent($scope.QuickCostingItemList[i].ComponentSequence);

                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }

                            if ($scope.QuickCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);
                            else
                                $scope.QuickCostingItemList[i].TotalGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100);

                        }
                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'ValueLoss') {

                        if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {

                            var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                            var totalCurr = getCurrentFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                            var totalPercent = getCurrentPercent($scope.QuickCostingItemList[i].ComponentSequence);
                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }

                            if ($scope.QuickCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);
                            else
                                $scope.QuickCostingItemList[i].TotalGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100);


                            //var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);

                            //$scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'Profit') {

                        if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {

                            var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                            var totalCurr = getCurrentFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                            var totalPercent = getCurrentPercent($scope.QuickCostingItemList[i].ComponentSequence);
                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }

                            if ($scope.QuickCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);
                            else
                                $scope.QuickCostingItemList[i].TotalGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100);

                            //var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);

                            //$scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);

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

                    var totalPre = getFixedAmountDirectMaterial();

                    $scope.QuickCostingItemList[i].TotalGrossAmount = (totalPre / ((100 - $scope.QuickCostingItemList[i].Value) / 100)) - totalPre;//totalPre * ($scope.QuickCostingItemList[i].Value / 100);
                    $scope.QuickCostingItemList[i].TotalGrossAmount += $scope.QuickCostingItemList[i].Rate;

                }
                else if ($scope.QuickCostingItemList[i].CostingSegment == 'SalesExpense') {

                    if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = $scope.QuickCostingItemList[i].Value;
                    }
                    else {

                        var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                        var totalCurr = getCurrentFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                        var totalPercent = getCurrentPercent($scope.QuickCostingItemList[i].ComponentSequence);

                        if ($scope.QuickCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);
                        else
                            $scope.QuickCostingItemList[i].TotalGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.QuickCostingItemList[i].Value / 100);

                        //var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);

                        //$scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);

                    }
                }
                else if ($scope.QuickCostingItemList[i].CostingSegment == 'ValueLoss') {

                    if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = $scope.QuickCostingItemList[i].Value;
                    }
                    else {
                        var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                        var totalCurr = getCurrentFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                        var totalPercent = getCurrentPercent($scope.QuickCostingItemList[i].ComponentSequence);

                        if ($scope.QuickCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);
                        else
                            $scope.QuickCostingItemList[i].TotalGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.QuickCostingItemList[i].Value / 100);

                        //var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);

                        //$scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);

                    }
                }
                else if ($scope.QuickCostingItemList[i].CostingSegment == 'Profit') {


                    if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = $scope.QuickCostingItemList[i].Value;
                    }
                    else {
                        //var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);

                        //$scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);
                        var totalPre = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                        var totalCurr = getCurrentFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                        var totalPercent = getCurrentPercent($scope.QuickCostingItemList[i].ComponentSequence);

                        if ($scope.QuickCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);
                        else
                            $scope.QuickCostingItemList[i].TotalGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.QuickCostingItemList[i].Value / 100);

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

                //$scope.CostingSummaryDataMain = { BuyerTotal: 0, QuickCostingValue: 0, PreCostingValue, ProfitBuyerCosting: 0, ProfitQuickCosting: 0, ProfitPreCosting: 0 };

                //calculation
                if ($scope.QuickCostingDetailList[i].CostingSegment == 'Profit') {
                    $scope.CostingSummaryDataNew.ProfitBuyerCosting += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.ProfitQuickCosting += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.ProfitPreCosting += $scope.QuickCostingDetailList[i].TotalGrossAmount;
                }
                else {
                    $scope.CostingSummaryDataNew.BuyerTotal += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.QuickCostingValue += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.PreCostingValue += $scope.QuickCostingDetailList[i].TotalGrossAmount;

                }


            }

            liveUpdateCostingComponent();

        } catch (e) {

        }



    }

    $scope.CalculateFinalCosting_backup = function (data) {

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
                        $scope.QuickCostingItemList[i].TotalGrossAmount = (totalPre / ((100 - data.Value) / 100)) - totalPre;// totalPre * (data.Value / 100) + data.Rate;

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

            for (var i = 0; i < $scope.QuickCostingItemList[i].length; i++) {
                if ($scope.QuickCostingItemList[i].Rate > 0 || $scope.QuickCostingItemList[i].ValueType == "PERCENTAGE") {
                    var TotalFixedValue = getFixedAmount($scope.QuickCostingItemList[i].ComponentSequence);
                    var CurrentGrossValue = 0;
                    var Percentage = 0;
                    if ($scope.QuickCostingItemList[i].Rate > 0) {
                        //because we have both fixed value and a rate (e.g. DirectProcess) which will sum up to TotalGrossValue
                        CurrentGrossValue = $scope.QuickCostingItemList[i].Rate;
                    }
                    Percentage = $scope.QuickCostingItemList[i].Value;


                    //now add percentage portion with the CurrentGrossValue
                    //CurrentGrossValue += (TotalFixedValue / (100 - (Percentage / 100))) * (Percentage / 100);
                    CurrentGrossValue += TotalFixedValue * (Percentage / 100);
                    $scope.QuickCostingItemList[i].TotalGrossAmount = CurrentGrossValue;
                }
                else {
                    $scope.QuickCostingItemList[i].TotalGrossAmount = $scope.QuickCostingItemList[i].Value;

                }
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

                //$scope.CostingSummaryDataMain = { BuyerTotal: 0, QuickCostingValue: 0, PreCostingValue, ProfitBuyerCosting: 0, ProfitQuickCosting: 0, ProfitPreCosting: 0 };

                //calculation
                if ($scope.QuickCostingDetailList[i].CostingSegment == 'Profit') {
                    $scope.CostingSummaryDataNew.ProfitBuyerCosting += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.ProfitQuickCosting += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.ProfitPreCosting += $scope.QuickCostingDetailList[i].TotalGrossAmount;
                }
                else {
                    $scope.CostingSummaryDataNew.BuyerTotal += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.QuickCostingValue += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.PreCostingValue += $scope.QuickCostingDetailList[i].TotalGrossAmount;

                }


            }
        } catch (e) {

        }



    }

    function liveUpdateCostingComponent() {

        var currentData = [];
        if ($scope.Segment == 'DirectMaterial') {

            return;
        }
        else if ($scope.Segment == 'Operation') {
            //currentData = $scope.DirectMaterialList;
            return;
        }
        else if ($scope.Segment == 'DirectProcess') {
            currentData = $scope.DirectProcessList;

        }
        else if ($scope.Segment == 'SalesExpense') {
            currentData = $scope.SalesExpenseList;

        }
        else if ($scope.Segment == 'ValueLoss') {
            currentData = $scope.ValueLossList;

        }
        else if ($scope.Segment == 'Profit') {
            currentData = $scope.ProfitList;

        }

        var data = ej.DataManager($scope.QuickCostingItemList).executeLocal(ej.Query().where("CostingComponentId", "equal", parseInt($scope.CostingComponentId), true));
        for (var i = 0; i < data.length; i++) {
            var single = ej.DataManager(currentData).executeLocal(ej.Query().where("CostingItemId", "equal", data[i].Id, true));
            if (single != null) {
                if (single.length > 0) {
                    try {
                        single[0].Amount = data[i].TotalGrossAmount;
                    } catch (e) {

                    }
                }

            }
        }
    }

    function getFixedAmount(sequence) {

        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {
            if ($scope.QuickCostingItemList[i].ComponentSequence < sequence) {

                TotalPreviousAmount += $scope.QuickCostingItemList[i].TotalGrossAmount;
            }
        }

        return TotalPreviousAmount;
    }
    $scope.TotalDirectMaterialCost = 0;
    function getFixedAmountDirectMaterial() {
        $scope.TotalDirectMaterialCost = 0;
        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {
            if ($scope.QuickCostingItemList[i].CostingSegment.toUpperCase() == 'DIRECTMATERIAL') {

                TotalPreviousAmount += $scope.QuickCostingItemList[i].TotalGrossAmount;
            }
        }
        $scope.TotalDirectMaterialCost = TotalPreviousAmount;
        return TotalPreviousAmount;
    }
    function getCurrentFixedAmount(sequence) {

        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {

            if ($scope.QuickCostingItemList[i].ComponentSequence == sequence
                && ($scope.QuickCostingItemList[i].ValueType.toUpperCase() != 'PERCENTAGE')) {

                TotalPreviousAmount += $scope.QuickCostingItemList[i].TotalGrossAmount;
            }
            if ($scope.QuickCostingItemList[i].ComponentSequence == sequence
                && ($scope.QuickCostingItemList[i].Rate > 0 || $scope.QuickCostingItemList[i].ValueType.toUpperCase() == 'PERCENTAGE')) {

                TotalPreviousAmount += $scope.QuickCostingItemList[i].Rate;
            }
        }

        return TotalPreviousAmount;
    }
    function getCurrentPercent(sequence) {

        var TotalPreviousAmount = 0;
        for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {

            if ($scope.QuickCostingItemList[i].ComponentSequence == sequence && $scope.QuickCostingItemList[i].ValueType.toUpperCase() == 'PERCENTAGE') {

                TotalPreviousAmount += $scope.QuickCostingItemList[i].Value;
            }
        }

        return TotalPreviousAmount;
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




    $scope.SavePreCostingDirectMaterial = function () {
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
            url: 'Costings/quickCostingMaster/SavePreCostingDirectMaterial',
            data: { 'data': $scope.DirectMaterialList, costingMasterTemplateId: $scope.ModelNew.Id },
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
            url: 'Costings/quickCostingMaster/GetOperationWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&costingMasterTemplateId=' + $scope.ModelNew.Id,
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
                    url: 'Costings/quickCostingMaster/SaveOperation',
                    data: { 'data': $scope.OperationList, costingMasterTemplateId: $scope.ModelNew.Id },
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
        try {
            if (angular.isUndefinedOrNull($scope.CostingMasterTemplateId))
                throw 'Please save the costing master first';

            angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("show");
            $scope.AddNewCostingItem();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.hideCostingItemListWithOperationPopUp = function () {
        angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("hide");
    }

    $scope.CostingItemWithOperationList = [];
    $scope.GetCostingItemWithOperationByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/quickCostingMaster/GetCostingItemWithOperationByComponentId?costingComponentId=' + $scope.CostingComponentId,

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
            url: 'Costings/quickCostingMaster/GetDirectProcessWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&costingMasterTemplateId=' + $scope.ModelNew.Id,
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
                    url: 'Costings/quickCostingMaster/SaveDirectProcess',
                    data: { 'data': $scope.DirectProcessList, costingMasterTemplateId: $scope.ModelNew.Id },
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
            url: 'Costings/quickCostingMaster/GetCostingItemWithDirectProcessByComponentId?costingComponentId=' + $scope.CostingComponentId,

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
            url: 'Costings/QuickCostingMaster/DeleteDirectProcess',
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

                url: $scope.path + 'GetCostingItemsWithoutFilterForDirectProcess?costingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.data = response.data;
                $scope.UpdateGridItems();
            });
        } catch (e) {

        }
    }

    // #endregion PreCosting Direct Process

    // #region Precosting SalesExpense

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
            url: 'Costings/quickCostingMaster/GetSalesExpenseWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&costingMasterTemplateId=' + $scope.ModelNew.Id,
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
        var flag = false;
        if ($scope.SalesExpenseList.length > 0) {

            try {

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
                        url: 'Costings/quickCostingMaster/SaveSalesExpense',
                        data: { 'data': $scope.SalesExpenseList, costingMasterTemplateId: $scope.ModelNew.Id },
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
            } catch (e) {
                ShowResult(e, 'failure');
            }


        }
        else {
            $scope.hideSalesExpensePopUp();
        }
    };

    $scope.CostingItemWithSalesExpenseList = [];
    $scope.GetCostingItemWithSalesExpenseByComponentId = function () {

        $http({
            method: 'GET',
            url: 'Costings/quickCostingMaster/GetCostingItemWithSalesExpenseByComponentId?costingComponentId=' + $scope.CostingComponentId,

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
            url: 'Costings/QuickCostingMaster/DeleteSalesExpense',
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

                url: $scope.path + 'GetCostingItemsWithoutFilterForSalesExpense?costingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.data = response.data;
                $scope.UpdateGridItems();
            });
        } catch (e) {

        }
    }
    // #endregion Precosting SalesExpense

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
            url: 'Costings/quickCostingMaster/GetValueLossWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&costingMasterTemplateId=' + $scope.ModelNew.Id,
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
            url: 'Costings/quickCostingMaster/GetProfitWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&costingMasterTemplateId=' + $scope.ModelNew.Id,
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
                        url: 'Costings/quickCostingMaster/SaveValueLoss',
                        data: { 'data': $scope.ValueLossList, costingMasterTemplateId: $scope.ModelNew.Id },
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
                        url: 'Costings/quickCostingMaster/SaveProfit',
                        data: { 'data': $scope.ProfitList, costingMasterTemplateId: $scope.ModelNew.Id },
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
            url: 'Costings/quickCostingMaster/GetCostingItemWithValueLossByComponentId?costingComponentId=' + $scope.CostingComponentId,

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
            url: 'Costings/QuickCostingMaster/DeleteValueLoss',
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

                url: $scope.path + 'GetCostingItemsWithoutFilterForValueLoss?costingMasterTemplateId=' + $scope.ModelNew.Id + '&costingComponentId=' + $scope.CostingComponentId,
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
        CostingMasterTemplateId: null,
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
            url: 'Costings/quickCostingMaster/GetBuyerDataByCostingMasterId?costingMasterId=' + $scope.ModelNew.Id,
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
            url: 'Costings/quickCostingMaster/DeleteCostingBuyer?id=' + $scope.costingBuyerNew.Id
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
        $scope.CostingBuyer.CostingMasterTemplateId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.BuyerForm.$valid) {
                if ($scope.BuyerAction === 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Costings/quickCostingMaster/CreateCostingBuyer',
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
                        url: 'Costings/quickCostingMaster/CreateCostingBuyer',
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
    $scope.DeletePreCostingDirectMaterialPopUp = function (x) {
        $scope.DirectMaterialId = x.Id;
        $scope.message_confirmation = "Are you sure to Delete permanently ?";
        angular.element(document.querySelector("#DeletePreCostingDirectMaterialPopUp")).modal("show");
    };

    $scope.DeleteDirectMaterial = function () {
        $http({
            method: 'POST',
            url: 'Costings/QuickCostingMaster/DeleteDirectMaterial',
            data: { 'DirectMaterialId': $scope.DirectMaterialId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector("#DeletePreCostingDirectMaterialPopUp")).modal("hide");
                //$scope.DirectMaterialList = [];
                $scope.GetDirectCostingMaterialWithItemByComponentId();
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
            url: 'Costings/QuickCostingMaster/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.setEmpData = function (obj) {


        var data = obj.data;
        if ($scope.Segment == 'DirectMaterial') {
            $scope.DirectMaterialList[$scope.index].ResponsiblePersonId = data.SystemID;
            $scope.DirectMaterialList[$scope.index].ResponsiblePerson = data.EmployeeName;
        }
        else if ($scope.Segment == 'Operation') {

            $scope.OperationList[$scope.index].ResponsiblePersonId = data.SystemID;
            $scope.OperationList[$scope.index].ResponsiblePerson = data.EmployeeName;
        }
        else if ($scope.Segment == 'DirectProcess') {

            $scope.DirectProcessList[$scope.index].ResponsiblePersonId = data.SystemID;
            $scope.DirectProcessList[$scope.index].ResponsiblePerson = data.EmployeeName;
        }
        else if ($scope.Segment == 'SalesExpense') {

            $scope.SalesExpenseList[$scope.index].ResponsiblePersonId = data.SystemID;
            $scope.SalesExpenseList[$scope.index].ResponsiblePerson = data.EmployeeName;
        }
        else if ($scope.Segment == 'ValueLoss') {

            $scope.ValueLossList[$scope.index].ResponsiblePersonId = data.SystemID;
            $scope.ValueLossList[$scope.index].ResponsiblePerson = data.EmployeeName;
        }
        else if ($scope.Segment == 'Profit') {

            $scope.ProfitList[$scope.index].ResponsiblePersonId = data.SystemID;
            $scope.ProfitList[$scope.index].ResponsiblePerson = data.EmployeeName;
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
        $scope.totalCost = 0;
        $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

        if ($scope.QuickCostingDetailList.length > 0) {
            for (let i = 0; i < $scope.QuickCostingDetailList.length; i++) {


                if (!isNaN($scope.QuickCostingDetailList[i].CostingValue)) {
                    $scope.totalCost += $scope.QuickCostingDetailList[i].CostingValue;
                }

                //calculation
                if ($scope.QuickCostingDetailList[i].CostingSegment == 'Profit') {
                    $scope.CostingSummaryDataNew.ProfitBuyerCosting += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.ProfitQuickCosting += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.ProfitPreCosting += $scope.QuickCostingDetailList[i].TotalGrossAmount;
                }
                else {
                    $scope.CostingSummaryDataNew.BuyerTotal += $scope.QuickCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.QuickCostingValue += $scope.QuickCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.PreCostingValue += $scope.QuickCostingDetailList[i].TotalGrossAmount;

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
        $scope.ProfitList = [];
        $scope.Segment = '';



        var elmnt = document.getElementById("costingMain");
        elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
    }
    $scope.NavigateToPreCosting = function (args) {
        if (angular.isUndefinedOrNull($scope.ModelNew.Id)) {
            return ShowResult('Please save the template first', 'failure');
        }


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

            $scope.GetDirectCostingMaterialWithItemByComponentId();
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

    //#region -- Updating by Saad
    $scope.SelectedDirectMaterial = {};
    $scope.NavigateToUpdatePreCosting = function (args) {
        $scope.PreCostingDirectMaterialId = args.Id;
        $scope.CostingItemId = args.CostingItemId;
        $scope.CostingMasterTemplateId = args.CostingMasterTemplateId;
        $scope.SelectedDirectMaterial = args;
        $scope.AddLineIdem();
    }
    $scope.ItemConsumption = null;
    $scope.ItemList = [];
    $scope.GetDataFormItem = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDataFromItemCon",
            data: { ProductId: $scope.ModelNew.ProductMasterId, MaterialId: $scope.CostingItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ItemList = response.data;
        });
    }

    $scope.Height = [];
    $scope.Width = [];
    $scope.Height.push(Object.assign({}, $scope.HeightDetail));
    $scope.Width.push(Object.assign({}, $scope.WidthDetail));
    $scope.HeightDetail = {
        Id: null,
        AreaType: '',
        ParameterName: 'Height',
        Parameter: null,
        Actual: 0,
        Allowance: 0,
        WithAllowance: 0,
        NoOfParameter: 0,
        tempId: null,
        Total: 0
    }
    $scope.WidthDetail = {
        Id: null,
        AreaType: '',
        ParameterName: 'Height',
        Parameter: null,
        Actual: 0,
        Allowance: 0,
        WithAllowance: 0,
        NoOfParameter: 0,
        tempId: null,
        Total: 0
    }

    $scope.Calculation = function ($data) {
        $data.WithAllowance = $data.Actual + $data.Allowance;
        $data.Total = $data.WithAllowance * $data.NoOfParameter;
    }


    $scope.GetPreCostingDetail = function (PreCostingDirectMaterialId) {

        $http({
            method: 'POST',
            url: $scope.path + "GetHeightData",
            data: { PreCostMaterialId: PreCostingDirectMaterialId, ParameterName: 'Height' },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.Height = response.data;
        });

        $http({
            method: 'POST',
            url: $scope.path + "GetHeightData",
            data: { PreCostMaterialId: PreCostingDirectMaterialId, ParameterName: 'Width' },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.Width = response.data;
        });
    }

    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {

            $scope.ShowDiv = true;
            var eDialog = $("#UpdatePOPup").data("ejDialog");
            eDialog.open();
            $scope.GetPreCostingDetail($scope.PreCostingDirectMaterialId);
            $scope.GetDataFormItem();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.SaveUpdate = function () {
        try {
            $scope.SaveChildDataList = [];
            for (var i = 0; i < $scope.Height.length; i++) {
                if ($scope.Height[i].Parameter == null && $scope.Height[i].Actual == null) {
                    $scope.Height.splice(i, 1);
                }

            }
            if ($scope.Height.length == 0) {
                $scope.Height.push(Object.assign({}, $scope.HeightDetail));
                throw "Parameter 1 is required..";
            }
            else {
                for (var i = 0; i < $scope.Height.length; i++) {
                    $scope.SaveChildDataList.push($scope.Height[i]);
                }
            }

            for (var i = 0; i < $scope.Width.length; i++) {
                if ($scope.Width[i].Parameter == null && $scope.Width[i].Actual == null) {
                    $scope.Width.splice(i, 1);
                }
            }
            for (var i = 0; i < $scope.Height.length; i++) {
                if ($scope.Height[i].AreaType != 'Circle') {
                    if ($scope.Width.length == 0) {
                        $scope.Width.push(Object.assign({}, $scope.WidthDetail));
                        throw "Parameter 2 is required..";
                    }
                    else {
                        for (var i = 0; i < $scope.Width.length; i++) {
                            $scope.SaveChildDataList.push($scope.Width[i]);
                        }
                    }
                }
            }

            if ($scope.SaveChildDataList.length == 0) {
                throw "Insert Parameter Value";
            }
            $http({
                method: 'POST',
                url: $scope.path + "SaveUpdate",
                data: { 'PreCostingDirectMaterialId': $scope.PreCostingDirectMaterialId, 'ChildData': $scope.SaveChildDataList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SelectedDirectMaterial.Consumption = response.data.Consumption;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.confirmdelete = false;
    $scope.Confirm = function () {
        try {
            if ($scope.ItemConsumption == null || $scope.ItemConsumption == "") {
                throw "Select Item Consumption first..";
            }
            var eDialog = $("#dialogAPI").data("ejDialog");
            eDialog.open();
            $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.ConfirmClose = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.close();
    };

    $scope.ApplyItemConsumption = function () {
        try {
            if ($scope.ItemConsumption == null) {
                throw "Select Item Consumption first..";
            }
            $http({
                method: 'POST',
                url: $scope.path + "SaveNewItemConsumptionData",
                data: { PreCostingDirectMaterialId: $scope.PreCostingDirectMaterialId, ItemConsumtionId: $scope.ItemConsumption, CostingMasterTemplateId: $scope.CostingMasterTemplateId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SelectedDirectMaterial.Consumption = response.data.Consumption;
                    $scope.GetPreCostingDetail($scope.PreCostingDirectMaterialId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }

    }

    //#endregion
    $scope.SaveCostingComponentItems = function () {

        if ($scope.Segment == 'DirectMaterial') {

            $scope.SavePreCostingDirectMaterial();
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

    $scope.PreCostingTemplete = function (data) {
        try {

            var file_src = $scope.path + 'GetPreCostingReport?CostingTempleteId=' + data.Id;
            $rootScope.report(file_src);

        } catch (e) {
        }
    }

    $scope.POCriteriaList = [];
    cboService.getEnumCbo("enum/GetPOInvoiceCriteriaEnumCbo", function (result) {
        $scope.POCriteriaList = result;
    });

    //#region POPUP For Material By SAAD

    $scope.SubMaterialList = [];
    $scope.SelectedLine = {};
    $scope.GetSubMaterial = function (item) {
        //try {
        $scope.SelectedLine = {};
        $scope.SelectedLine = item;
        
        $scope.getSubMaterialData($scope.SelectedLine.Id);
        angular.element(document.querySelector("#SubMaterialPOPup")).modal("show");

    }
    $scope.showCostingItemListWithOperationPopUpInSubMaterial = function () {
        try {
            if (angular.isUndefinedOrNull($scope.CostingMasterTemplateId))
                throw 'Please save the costing master first';

            var eDialog = $("#General").data("ejDialog");
            eDialog.open();

            $scope.AddNewCostingItemPopUp();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.NewCostingItemList = [];
    $scope.AddNewCostingItemPopUp = function () {
        try {
            if (angular.isUndefinedOrNull($scope.CostingMasterTemplateId))
                throw 'Please save the costing master first';
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "GetSubMaterialSelection",
                data: { CostingMasterTemplateId: $scope.CostingMasterTemplateId, costingComponentId: $scope.CostingComponentId, Segment: $scope.Segment },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.NewCostingItemList = response.data;
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.CloseSubMaterial = function () {
        angular.element(document.querySelector("#SubMaterialPOPup")).modal("hide");
    }
    $scope.hideSubMaterialPart = function () {
        var eDialog = $("#General").data("ejDialog");
        eDialog.close();
    }
    $scope.SaveSubMaterialPart = function () {
        try {
            //$scope.SelectedLine;
            var selectedList = [];
            for (var i = 0; i < $scope.NewCostingItemList.length; i++) {
                if ($scope.NewCostingItemList[i].Selected) {
                    selectedList.push($scope.NewCostingItemList[i]);
                }
            }
            if (angular.isUndefinedOrNull($scope.CostingMasterTemplateId))
                throw 'Please save the costing master first';
            $http({
                method: 'POST',
                url: $scope.path + "SaveSubMaterial",
                data: { itemList: selectedList, 'PreCDMaterial': $scope.SelectedLine },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSubMaterialData($scope.SelectedLine.Id);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.getSubMaterialData = function (PreCostingDirectMaterialId) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetSubMaterialData",
                data: { MasterId: PreCostingDirectMaterialId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.SubMaterialList = response.data.data;
            });
        } catch (e) {
            ShowResult(e,'info');
        }
    }
    $scope.SaveSubMaterial = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "UpdatePreCostingChild",
                data: { subMaterilaList: $scope.SubMaterialList, MasterId: $scope.SelectedLine.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSubMaterialData($scope.SelectedLine.Id);
                }
            });
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.DeleteSubMaterialId = null;
    $scope.DeleteSubMaterial = function (obj) {
        $scope.DeleteSubMaterialId = obj;        
        angular.element(document.querySelector('#confirmDelete')).modal('show');
    }

    $scope.DeleteSubMaterials = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "DeleteSubMaterial",
                data: { SubMaterialId: $scope.DeleteSubMaterialId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSubMaterialData($scope.SelectedLine.Id);                   
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CalculationSubMaterial = function (data, index) {
        data.GrossConsumption = parseFloat(data.Consumption) / (((parseFloat(100) - (parseFloat(data.ValueLoss)))/ 100));
        data.GrossAmount = parseFloat(data.Rate) * parseFloat(data.GrossConsumption);
        $scope.SubMaterialList[index].GrossConsumption = parseFloat(data.GrossConsumption.toFixed(4));
        $scope.SubMaterialList[index].GrossAmount = parseFloat(data.GrossAmount.toFixed(4));
    };

    //#endregion

}