'use strict';
JobWorkTransformationPOController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', '$window', 'accountService'];
function JobWorkTransformationPOController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $window, accountService) {
    $rootScope.title = 'Job Work Transformation PO';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.JWPOByProductList = [];
    
    $scope.path = 'JobWork/JobWorkTransformationPO/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.TaxSaveUrl = $scope.path + 'SaveTaxList';

    $scope.detailDeleteUrl = $scope.path + 'DeleteDetail?Id=';
    $scope.detailGridListUrl = $scope.path + 'GetJWPOChildList?jwpoId=';
    $scope.detailGridListAllUrl = $scope.path + 'GetJWPOChildListAll';

    $scope.sreviceSaveUrl = $scope.path + 'ServiceChargeCreate';
    $scope.partyType = 'Vendor';
    $scope.inventoryMaterialList = [];
    $scope.chargesList = [];
    $scope.ChargeTaxList = [];
    $scope.StateData = [];
    $scope.plantList = [];
    $scope.jobWorkActivityList = [];
    $scope.GetListForMasterOrder = [];
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.paymentTermList = [];
    $scope.taxAbleAmnt = 0.00;
    $scope.JWPurchaseOrderFileLocation = virtualPath.OSTransformationPO;

    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });
    $scope.serviceList = [];
    $http.get('Setups/CompanyServiceMaster/GetCboList')

        .then(function (response) {
            $scope.serviceList = response.data;
        });
    //#region notification setting

    // Shashank
    $scope.pathJWCBO = 'JobWork/JWValueAddedContract/';

    $scope.JobWorkItemMasterList = [];
    $scope.OutputMaterialUOMList = [];
    $scope.ArticleCodeList = [];
    $scope.RateApplyList = [];
    $scope.CurrencyList = [];
    $scope.SelectedMaterialPlanningTabList = [];
    $scope.JobActivityList = [];

    $http({
        method: 'GET',
        url: $scope.pathJWCBO + 'getjobworkactivitylist',
    }).then(function successCallback(response) {
        $scope.JobActivityList = response.data;
    });

    $http({
        method: 'GET',
        url: $scope.pathJWCBO + 'getjobworkitemlist',
    }).then(function successCallback(response) {
        $scope.JobWorkItemMasterList = response.data;
    });

    $http({
        method: 'GET',
        url: $scope.pathJWCBO + 'getoutputunit',
    }).then(function successCallback(response) {
        $scope.OutputMaterialUOMList = response.data;
    });


    $scope.SelectedMatPlanningTabList = [];
    $scope.JobWorkItemMstList = [];
    $scope.MaterialLocList = [];
    $scope.OMatUOMList = [];
    $scope.ArticleList = [];
    $scope.RateList = [];
    $scope.CurrencyyyList = [];
    $scope.SelectedMaterialPlanningTabList = [];
    $scope.JobWorkActivityList = [];

    $scope.GetJWActivityListByPOType = function () {
        $http({
            method: 'GET',
            url: $scope.pathJWCBO + 'getactivitylistTransformation?ContractType=' + $scope.productNew.POType,
        }).then(function successCallback(response) {
            $scope.JobWorkActivityList = response.data;
        });
    }
  

    $scope.GetJWItems = function () {
        $http({
            method: 'GET',
            url: $scope.pathJWCBO + 'getTransformationjobworkitemlist?ActivityId=' + $scope.detailModel.JobActivityId + '&ContractType=' + $scope.productNew.POType,
        }).then(function successCallback(response) {
            $scope.JobWorkItemMstList = response.data;
            $scope.detailModel.OutputMaterialUOMId = null;
            $scope.detailModel.ByProductApplicable = null;
            $scope.MaterialMstClear();
        });
    }

    $scope.GetJWItemsToEdit = function () {
        $http({
            method: 'GET',
            url: $scope.pathJWCBO + 'getTransformationjobworkitemlist?ActivityId=' + $scope.detailModel.JobActivityId + '&ContractType=' + $scope.productNew.POType,
        }).then(function successCallback(response) {
            $scope.JobWorkItemMstList = response.data;
            //$scope.detailModel.OutputMaterialUOMId = null;
            //$scope.detailModel.ByProductApplicable = null;
            //$scope.MaterialMstClear();
        });
    }

    $scope.GetTransmstList = [];
    $scope.GetJWitemDataFromTrans = function () {
        if ($scope.productNew.POType == "JWTransformationPO") {
            $http({
                method: 'GET',
                url: $scope.pathJWCBO + 'GetJWitemDataFromTrans?ActivityId=' + $scope.detailModel.JobActivityId + '&JWItemId=' + $scope.detailModel.JobWorkItemMasterId + '&ContractType=' + $scope.productNew.POType,
            }).then(function successCallback(response) {
                $scope.GetTransmstList = response.data;
                if ($scope.GetTransmstList.length > 0) {

                    $scope.detailModel.ByProductApplicable = $scope.GetTransmstList[0].ByProductApplicable;
                }
            });
        }   
    }

    $scope.GetJWLocation = function () {
        $http({
            method: 'GET',
            url: 'Outsourcing/JobWorkValueAddedContract/getmateriallocation?EntityId=' + $scope.productNew.EntityId + '&JWActivityId=' + $scope.detailModel.JobActivityId,
        }).then(function successCallback(response) {
            $scope.MaterialLocList = response.data;
        });
    }
 

    $scope.MStorageList = [];
    $scope.GetJWMaterialStorage = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetJWMaterialStorage?JWLocId=' + $scope.detailModel.MaterialLocationId,
        }).then(function successCallback(response) {
            $scope.MStorageList = response.data;
            if ($scope.MStorageList.length > 0) {
                if ($scope.MStorageList[0].StoreLocationId != null) {
                    $scope.detailModel.JWMaterialStorage = $scope.MStorageList[0].MaterialStorage;
                }
                else {
                    $scope.detailModel.JWMaterialStorage = null;
                }
            }
        });
    }

    $http({
        method: 'GET',
        url: $scope.pathJWCBO + 'getoutputunit',
    }).then(function successCallback(response) {
        $scope.OMatUOMList = response.data;
    });

    //$scope.GetArticle = function () {
    //    $scope.ArticleList = [];
    //    $http({
    //        method: 'GET',
    //        url: $scope.pathJWCBO + 'getarticlecode?JobWorkItemId=' + $scope.detailModel.JobWorkItemMasterId,
    //    }).then(function successCallback(response) {
    //        $scope.ArticleList = response.data;
    //        if ($scope.ArticleList.length > 0) {
    //            $scope.detailModel.ArticleCodeId = $scope.ArticleList[0].Value;

    //        }
    //    });
    //}
    $scope.GetRate = function () {
        $scope.RateList = [];
        $http({
            method: 'GET',
            url: $scope.pathJWCBO + 'gettransformationrateapplylist?JobWorkItemId=' + $scope.detailModel.JobWorkItemMasterId + '&ActivityId=' + $scope.detailModel.JobActivityId + '&ContractType=' + $scope.productNew.POType,
        }).then(function successCallback(response) {
            $scope.RateList = response.data;
            if ($scope.RateList.length > 0) {
                $scope.detailModel.RateApplyId = $scope.RateList[0].Value;
                $scope.detailModel.RatePerUnit = $scope.RateList[0].MinRate;
                $scope.detailModel.MaxRate = $scope.RateList[0].MaxRate;
                $scope.detailModel.ServiceId = $scope.RateList[0].ServiceId;
                $scope.changeService($scope.detailModel.ServiceId);
            }
        });
    }

    $scope.GetRateToEdit = function () {
        $scope.RateList = [];
        $http({
            method: 'GET',
            url: $scope.pathJWCBO + 'gettransformationrateapplylist?JobWorkItemId=' + $scope.detailModel.JobWorkItemMasterId + '&ActivityId=' + $scope.detailModel.JobActivityId + '&ContractType=' + $scope.productNew.POType,
        }).then(function successCallback(response) {
            $scope.RateList = response.data;
            if ($scope.RateList.length > 0) {
                $scope.detailModel.RateApplyId = $scope.RateList[0].Value;
                //$scope.detailModel.RatePerUnit = $scope.RateList[0].MinRate;
                //$scope.detailModel.MaxRate = $scope.RateList[0].MaxRate;
                //$scope.detailModel.ServiceId = $scope.RateList[0].ServiceId;
                //$scope.changeService($scope.detailModel.ServiceId);
            }
        });
    }

    $scope.ValidateRate = function () {
        try {
            var MinimumRate = parseFloat($scope.detailModel.RatePerUnit);
            var MaximumRate = parseFloat($scope.detailModel.MaxRate);
            if (MinimumRate > MaximumRate) {
           //     $scope.detailModel.RatePerUnit = null;
                throw 'Rate Per Unit cannot be greater than Maximum Rate ' + MaximumRate + ' ';
            }
        }
        catch (e) {

            ShowResult(e, "failure");
            throw e;
        }
    }
    $scope.GetCurrencyyy = function () {
        $scope.CurrencyyyList = [];
        $http({
            method: 'GET',
            url: $scope.pathJWCBO + 'gettransformationcurrency?JobWorkItemId=' + $scope.detailModel.JobWorkItemMasterId + '&ActivityId=' + $scope.detailModel.JobActivityId + '&ContractType=' + $scope.productNew.POType,
        }).then(function successCallback(response) {
            $scope.CurrencyyyList = response.data;
            if ($scope.CurrencyyyList.length > 0) {
                $scope.detailModel.CurrencyId = $scope.CurrencyyyList[0].Value;
                //if (!baseService.isUndefinedOrNull($scope.CurrencyyyList[0].StdRejection)) {
                //    $scope.detailModel.Rejection = $scope.CurrencyyyList[0].StdRejection;
                //    $scope.detailModel.ValueLoss = $scope.CurrencyyyList[0].StdValueLoss;
                //}
            }
        });
    }
    // Material and Article
    $scope.MaterialMstList = [];
    $scope.MaterialMstPopUp = function () {
        angular.element(document.querySelector("#MaterialPopUp")).modal("show");
        $scope.getMaterialMstDetailsData();

    }
    $scope.getMaterialMstDetailsData = function () {
        $scope.MaterialMstList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.detailModel.Id },
            url: $scope.pathJWCBO + 'LoadAllMaterialMstDetails'
        }).then(function successCallback(response) {
            $scope.MaterialMstList = response.data;
        });
    }

    $scope.MaterialMstClear = function () {
        $scope.detailModel.MaterialMasterId = null;
        $scope.detailModel.MaterialName = null;
        $scope.detailModel.MaterialCode = null;
        $scope.detailModel.OutputMaterialUOMId = null;

    };
    $scope.closeMaterialMstPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setMaterialMstData = function (obj) {
        var data = obj.data;
        $scope.detailModel.MaterialCode = data.Code;
        $scope.detailModel.MaterialMasterId = data.Id;
        $scope.detailModel.MaterialName = data.MaterialName;
        $scope.detailModel.MaterialMasterName = data.MaterialName;
        $scope.detailModel.OutputMaterialUOMId = data.BaseUOMId;
        angular.element(document.querySelector('#MaterialPopUp')).modal('hide');
        $scope.MaterialMstArticlePopUp();
    };
    // # end region

    // MATERIAL MASTER ARTICLE
    // #region field

    $scope.MaterialArticleMstList = [];
    $scope.MaterialMstArticlePopUp = function () {
        angular.element(document.querySelector("#MaterialArticlePopUp")).modal("show");
        $scope.getMaterialMstArticleData();

    }
    $scope.getMaterialMstArticleData = function () {
        $scope.MaterialArticleMstList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.detailModel.Id, MaterialMstId: $scope.detailModel.MaterialMasterId },
            url: $scope.pathJWCBO + 'LoadAllMaterialMstArticle'
        }).then(function successCallback(response) {
            $scope.MaterialArticleMstList = response.data;
        });
    }

    $scope.MaterialMstArticleClear = function () {
   //     $scope.detailModel.ArticleCodeId = null;
        $scope.detailModel.ArticleName = null;
        $scope.detailModel.ArticleCode = null;
        $scope.detailModel.ArticleId = null;

    };
    $scope.closeMaterialArticlePopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setMaterialArticleData = function (obj) {
        var data = obj.data;
        $scope.detailModel.ArticleCode = data.ArticleCode;
  //      $scope.detailModel.ArticleCodeId = data.ArticleId;
        $scope.detailModel.ArticleName = data.StandardName;
        $scope.detailModel.ArticleId = data.ArticleId;
        angular.element(document.querySelector('#MaterialArticlePopUp')).modal('hide');
    };
    // Material and Article


    // SKU1

    $scope.SKU1List = [];
    $scope.SKU1PopUp = function () {
        angular.element(document.querySelector("#SKU1PopUp")).modal("show");
        $scope.getsku1Data();

    }
    $scope.getsku1Data = function () {
        $scope.SKU1List = [];
        $http({
            method: 'POST',
            data: { MaterialMstId: $scope.detailModel.MaterialMasterId, assignment: $scope.rmchar1.ValueAssignmentLevel, charId: $scope.detailModel.FirstCharacteristicsId },
            url: $scope.path + 'LoadAllSKU'
        }).then(function successCallback(response) {
            $scope.SKU1List = response.data;
        });
    }

    $scope.SKU1Clear = function () {
        $scope.detailModel.UserName1 = null;
    //    $scope.detailModel.Code1 = null;
    //    $scope.detailModel.FirstCharacteristicsId = null;
        $scope.detailModel.FirstCharacteristicsValueId = null;

    };
    //$scope.closeMaterialArticlePopUp = function (popupName) {
    //    angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    //}
    $scope.setSKU1Data = function (obj) {
        var data = obj.data;
    //    $scope.detailModel.Code1 = data.Code;
        $scope.detailModel.UserName1 = data.UserName;
   //     $scope.detailModel.FirstCharacteristicsId = data.CharacteristicsId;
        $scope.detailModel.FirstCharacteristicsValueId = data.CharacteristicsValueId;
        $scope.rmchar1.CharacteristicsValueId = data.CharacteristicsValueId;

        angular.element(document.querySelector('#SKU1PopUp')).modal('hide');
    };

    // SKU 2

    $scope.SKU2List = [];
    $scope.SKU2PopUp = function () {
        angular.element(document.querySelector("#SKU2PopUp")).modal("show");
        $scope.getsku2Data();

    }
    $scope.getsku2Data = function () {
        $scope.SKU2List = [];
        $http({
            method: 'POST',
            data: { MaterialMstId: $scope.detailModel.MaterialMasterId, assignment: $scope.rmchar2.ValueAssignmentLevel, charId: $scope.detailModel.SecondCharacteristicsId },
            url: $scope.path + 'LoadAllSKU'
        }).then(function successCallback(response) {
            $scope.SKU2List = response.data;
        });
    }

    $scope.SKU2Clear = function () {
        $scope.detailModel.UserName2 = null;
        //$scope.detailModel.Code2 = null;
        //$scope.detailModel.SecondCharacteristicsId = null;
        $scope.detailModel.SecondCharacteristicsValueId = null;

    };
    //$scope.closeMaterialArticlePopUp = function (popupName) {
    //    angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    //}
    $scope.setSKU2Data = function (obj) {
        var data = obj.data;
    //    $scope.detailModel.Code2 = data.Code;
        $scope.detailModel.UserName2 = data.UserName;
  //      $scope.detailModel.SecondCharacteristicsId = data.CharacteristicsId;
        $scope.detailModel.SecondCharacteristicsValueId = data.CharacteristicsValueId;
        $scope.rmchar2.CharacteristicsValueId = data.CharacteristicsValueId;
        angular.element(document.querySelector('#SKU2PopUp')).modal('hide');
    };

    // SKU 3

    $scope.SKU3List = [];
    $scope.SKU3PopUp = function () {
        angular.element(document.querySelector("#SKU3PopUp")).modal("show");
        $scope.getsku3Data();

    }
    $scope.getsku3Data = function () {
        $scope.SKU3List = [];
        $http({
            method: 'POST',
            data: { MaterialMstId: $scope.detailModel.MaterialMasterId, assignment: $scope.rmchar3.ValueAssignmentLevel, charId: $scope.detailModel.ThirdCharacteristicsId },
            url: $scope.path + 'LoadAllSKU'
        }).then(function successCallback(response) {
            $scope.SKU3List = response.data;
        });
    }

    $scope.SKU3Clear = function () {
        $scope.detailModel.UserName3 = null;
        //$scope.detailModel.Code3 = null;
        //$scope.detailModel.ThirdCharacteristicsId = null;
        $scope.detailModel.ThirdCharacteristicsValueId = null;

    };
    //$scope.closeMaterialArticlePopUp = function (popupName) {
    //    angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    //}
    $scope.setSKU3Data = function (obj) {
        var data = obj.data;
    //    $scope.detailModel.Code3 = data.Code;
        $scope.detailModel.UserName3 = data.UserName;
   //     $scope.detailModel.ThirdCharacteristicsId = data.CharacteristicsId;
        $scope.detailModel.ThirdCharacteristicsValueId = data.CharacteristicsValueId;
        $scope.rmchar3.CharacteristicsValueId = data.CharacteristicsValueId;
        angular.element(document.querySelector('#SKU3PopUp')).modal('hide');
    };

    //---Shahshank

    $scope.NotificationSettingStatus = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkTransformationPO/NotificationSetting',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            if ($scope.NotificationSetting.length > 0) {
                $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
                $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
                $scope.GetCheckedByAndApprovedBy1();
                if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
                    $scope.productNew.labelCheckAndApproved = 'To be checked by';
                }
                else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                    $scope.productNew.labelCheckAndApproved = 'To be approved by';
                }
                else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                    $scope.productNew.labelCheckAndApproved = 'To be checked by';
                }
            }
           
        });
    };
    $scope.NotificationSettingStatus();
    $scope.checkedByList = [];
    $scope.GetCheckedByAndApprovedBy1 = function () {
        //debugger;

        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/PurchaseOrder/GetCheckedByAndApprovedBYForOurSource?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }

    }
    $scope.HSNCode = null;
    $scope.HSNCodeId = null;

    //#region all Tab Function of PO Index

    $scope.POTypeStatus = '';
    $scope.tab1 = 1;
    $scope.setTabIndex = function (newTab) {

        $scope.POTypeStatus = 'Pending';
        $scope.getalldata();
        $scope.tab1 = newTab;

    };
    $scope.isSetIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.isSetIndex2 = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTab2 = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.setTabCHRIndex = function (newTab) {
        //alert('tabCHR');

        $scope.POTypeStatus = 'CheckedHoldRej';
        $scope.getalldata();
        $scope.tab1 = newTab;

    };
    $scope.isSetCHRIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabCheckedIndex = function (newTab) {

        $scope.POTypeStatus = 'Checked';
        $scope.getalldata();
        $scope.tab1 = newTab;


    };
    $scope.isSetCheckedIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };



    $scope.setTabAHRIndex = function (newTab) {
        $scope.ApproveRejectHold = 'HoldReject';
        $scope.getalldataPoApp();
        $scope.tab1 = newTab;
    };
    $scope.isSetAHRIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };



    $scope.setTabIndex1 = function (newTab) {
        $scope.ApproveRejectHold = 'Approved';
        $scope.getalldataPoApp();
        $scope.tab1 = newTab;
    };
    $scope.isSetIndex1 = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    //#endregion


    //#region PO Index Grid Data Display Load Function

    $scope.Griddata = [];
    $scope.POTypeStatus = 'Pending';
    $scope.getalldata = function () {
        if ($scope.POTypeStatus === 'Pending') {
            $scope.POTypeStatus = 'Pending';
        }
        else {

            // $scope.POTypeStatus;
        }

        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'JobWork/JobWorkTransformationPO/GetPOTypeList?POTypeStatus=' + $scope.POTypeStatus
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
            for (var i = 0; i < $scope.Griddata.length; i++) {
                response.data[i].PODate = new Date($scope.Griddata[i].PODate);
            }
        });
    };
    $scope.getalldata();



    $scope.GriddataPoApp = [];
    $scope.getalldataPoApp = function () {

        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'JobWork/JobWorkTransformationPO/GetListForHoldRejectApproved?ApproveRejectHold=' + $scope.ApproveRejectHold,
        }).then(function successCallback(response) {
            $scope.GriddataPoApp = response.data;
            //entrydata = copy(searchdata);
        });
    };



    //#region  PO  Details
    $scope.lst = [];


    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["Id"];
        var data = ej.DataManager($scope.PoChildListAll).executeLocal(ej.Query().where("JWTransformationPOId", "equal", filteredData, true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["JobWorkActivity", "JWItemName", "MaterialSpecification", "MaterialReference", "MaterialStorage", "JWItemUOM", "MaterialMasterName", "ArticleName", "CURR", "RateApplyId", "Process", "TransactionQty", "TransactionRate", "TransactionAmount", "TaxAmount", "BaseAmount", "Rejection", "ValueLoss", "Tolerance","ResponsiblePersonName"]
            //columns: ["JWItemName", "JWItemUOM", "MaterialMasterName", "ArticleName", "FirstCharacteristicsValue", "SecondCharacteristicsValue", "ThirdCharacteristicsValue", "TransactionQty", "TransactionUoM", "TransactionRate", "TransactionAmount", "CurrencyName", "TotalAmount"]
            //columns: ["MaterialGroupName", "MaterialName", "Article", "Sku1", "Sku2", "Sku3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    };
    //#endregion

    //#region Model

    var d = new Date();
    var hh = d.getHours();
    var mm = d.getMinutes();
    mm = (mm < 10 ? '0' + mm : mm);
    var ss = d.getSeconds()

    //   var _Time = hh + ":" + mm + ":" + ss;
    var _Time = hh + ":" + mm;

    $scope.products = {
        Id: null
        , GRNDate: null
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: $window.plantId
        , PartyId: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , CutOffDate: null
        , MaterialStorageId: null
        , CurrencyId: null
        , BaseCurrencyId: $scope.baseCurrencyId
        , ToCurrencyRate: 0
        , PaymentTermId: null
        , BaseOnDueDate: null
        , BaseNoOfDays: null
        , MatureDate: null
        , DocRefNo: null
        , DocDate: null
        , GateEntryNo: null
        , EntryDate: null
        , FixedAssetOrInventory: 'Inventory'
        , PODepended: false
        , AlongwithInvoice: true
        , InvoiceNo: null
        , InvoiceDate: null
        , IsNonCreditable: false
        , TaxApplicable: null
        , IsTaxApplicable: false
        , IsTaxApplicableChangeable: false
        , PartyType: $scope.partyType
        , IsClosed: false
        , DeliveryInstruction: null
        , SpecialInstruction: null
        , CheckedBy: null
        , AuthorizedBy: null
        , CheckedByStatus: null
        , AuthorizedByStatus: null
        , ContractId: null
        , ContractNo: null
        , OrderSpecific: 'No'
        , PurchaseLCId: null
        , CustomerName: null
        , PaymentMode: null
        , LCRef: null
        , labelCheckAndApproved: null
        , CheckedByStatusForNoti: null
        , ApprovedByStatusForNoti: null
        , DiscountAmount: 0
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionMatJWTax:'Yes'
        , TaxOptionService: "Yes"
        , TaxOptionServiceTPO: "Yes"
        , TaxOptionServiceModify: 'Yes'
        , FileName: null
        , UserFilename: null
        , SystemFileName: null
        , Description: null
        , Remarks: null
        , POType: null

        ,Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        Time: _Time,
        EntityId: null,
        //ProcessStartDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        //ProcessEndDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        //ContractClosingDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ProcessStartDate: null,
        ProcessEndDate: null,
        ContractClosingDate: null,
 //     Remarks: null,
        ContractStatus: "Active",

    };
    $scope.productNew = Object.assign({}, $scope.products);
    $scope.product = Object.assign({}, $scope.products);

    $scope.productNew.POType = "JWTransformationPO";
    $scope.SetPOType = function () {
        if ($scope.productNew.POType == "JWTransformationPO") {
            $scope.productNew.POType = "JWTransformationPO";
         //   alert($scope.productNew.POType);
        }
        else {
            $scope.productNew.POType == "JWValueAddedPO";
     //       alert($scope.productNew.POType);
        }
    }


    $scope.OrderSpecific = $scope.productNew.OrderSpecific;


    // To get Entity plant wise

    $scope.EntityList = [];
    $scope.GetEntityPlantWise = function () {
        var PLT = $window.plantId
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllEntity?PlantId=' + PLT
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;

        });
    }
    $scope.GetEntityPlantWise();



    $scope.productDocMap = {
        Id: null
        , FileName: null
        , UserFilename: null
        , SystemFileName: null
        , Description: null
        , Remarks: null
    };
    $scope.serviceModelTemp = {
        Id: null
        , ServiceMasterId: null
        , JWTransformationPOId: $scope.productNew.Id
        , CurrencyName: angular.element("#currency :selected").text()
        , CurrencyId: $scope.productNew.CurrencyId
        , BaseCurrencyId: $scope.baseCurrencyId
        , DocDate: $scope.productNew.DocDate
        , TransactionAmount: null
        , BaseAmount: 0
        , TotalTaxAmount: 0
        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
        , IsNonCreditable: $scope.productNew.IsNonCreditable
    };
    $scope.serviceModel = Object.assign({}, $scope.serviceModelTemp);


    $scope.ClearList = function (data) {

        $scope.inventoryMaterialList = [];
        $scope.OrderSpecific = data;

    };
    //#endregion

    //#region All Dropdownlist Load Function
    //#region Purchaser LC Intregrated to PurchaseOrder

    $scope.contractList = [];
    $scope.GetPopUpContract = function () {
        $scope.contractList = [];
        $http.get("Products/PurchaseOrder/GetLCContractList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.contractList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#ContractPopUp')).modal('show');
    };
    $scope.OrderSpecific = $scope.productNew.OrderSpecific;
    $scope.SelectedContract = function (obj) {
        //debugger;
        //var data = obj.data.ContractId;
        $scope.productNew.ContractId = obj.data.ContractId;
        $scope.productNew.CustomerName = obj.data.CustomerName;
        $scope.productNew.ContractNo = obj.data.ContractNo;
        $scope.productNew.LCRef = obj.data.LCRef;
        //console.log($scope.productNew);
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }

    $scope.ClearFields = function () {
        //$scope.purchaseLC = {};
        $scope.productNew.ContractId = null;
        //var DropDownListObj = $("#ddlActivityList").data("ejDropDownList");
        //DropDownListObj.uncheckAll();
        // $scope.productNew = { OrderSpecific: 'Yes', Id: null, Tenure: 0 };
        //$scope.purchaseLCChargesList = [];
        //$scope.Action = 'Save';
    };
    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    };
    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        $scope.masterOrderCustomerList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.productNew.ContractId
        }).then(function (response) {
            $scope.masterOrderCustomerList = response.data;
        });
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
    };

    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    };

    $scope.GriddataPOWithLC = [];
    $scope.getalldataPOWithLC = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetalldataPOWithLCMap'
        }).then(function successCallback(response) {
            $scope.GriddataPOWithLC = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.getalldataPOWithLC();



    $scope.GriddataPOWithOutLC = [];
    $scope.getalldataPOWithOutLC = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/GetalldataPOWithoutLCMap'
        }).then(function successCallback(response) {
            $scope.GriddataPOWithOutLC = response.data;
        });
    };
    $scope.getalldataPOWithOutLC();


    $scope.POTypeStatus = '';
    $scope.tab1 = 1;
    $scope.setTabPOLCMapIndex = function (newTab) {

        //$scope.POTypeStatus = 'Pending';
        $scope.tab1 = newTab;
        $scope.getalldataPOWithLC();
    };
    $scope.isSetPOLCMapIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };
    $scope.setTabPOLCMap = function (newTab) {
        //alert('tabCHR');

        // $scope.POTypeStatus = 'CheckedHoldRej';
        $scope.tab1 = newTab;
        $scope.getalldataPOWithOutLC();
    };
    $scope.isSetPOLCMap = function (tabNum) {
        return $scope.tab1 === tabNum;
    };
    $scope.LcList = [];
    $scope.GetLCByContract = function () {

        $http({
            method: 'GET',//?id=' + id+' & name='+name
            url: "Products/PurchaseOrder/GetLCListByCotract?ContractId=" + $scope.data.ContractId + "&VendorId=" + $scope.data.PartyId
        }).then(function successCallback(response) {
            $scope.LcList = response.data;
            angular.element(document.querySelector('#ContractPopUp')).modal('show');
            e
        });

    };
    $scope.taxCategoryList = [];
    $scope.closeDetaiPopUpEdit = function () {
        $scope.detailModel = {};
        $scope.taxCategoryList = [];
        removeValidationMsg();
        angular.element(document.querySelector('#detailPopUpEdit')).modal('hide');
    };
    $scope.recorddoubleclick = function ($event) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        $scope.Currency = $("#currency option:selected").text();
        $scope.productNew = x.data;
        $scope.Id = $scope.productNew.Id;
        $scope.productNew.PODate = $filter('dateFiltering')(x.data.PODate, 'dd-M-yyyy');

        $scope.productNew.ProcessStartDate = x.data.TConProcessStartDate;
        $scope.productNew.ProcessEndDate = x.data.TConProcessEndDate;
        $scope.productNew.ContractClosingDate = x.data.TConContractClosingDate;
        $scope.productNew.Time = x.data.TConTime;
        $scope.productNew.POType = x.data.POType;
        //if ($scope.productNew.POType == "OSTransformationPO") {
           
        //}
        

        //$scope.getJwActivityId($scope.productNew.Id);
        $scope.getPoChilddata();
        $scope.detailModel.JWTransformationPOId = $scope.productNew.Id;

        getPartyPlantEditList($scope.productNew.InvoicingPartyPlantId, $scope.productNew.InvoicingByAddress, $scope.productNew.DeliveryPartyPlantId, $scope.productNew.DeliveryByAddress, $scope.productNew.DeliveryState, $scope.productNew.DeliveryGSTIN);
        getPartyPlantEditList();
        //$scope.GetJWPOActivityService(x.data);
        getServiceChargeList($scope.productNew.Id);

        $scope.productNew.OrderSpecific = x.data.OrderSpecific;
        $scope.ImagedataLoad();

        $scope.BOQItemDisabled = 'GridClick';
        if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = false;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.ApprovedById;
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = true;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.CheckedById;
        }
        //$scope.GetCheckedByAndApprovedBy1();
        if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.productNew.CheckedBy = x.data.ApprovedById;
            $scope.productNew.labelCheckAndApproved = 'To be approved by';
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.productNew.CheckedBy = x.data.CheckedById;
            $scope.productNew.labelCheckAndApproved = 'To be checked by';
        }


        $scope.Action = 'Update';

        if (!$rootScope.isCollapsed) $rootScope.toggle();


    };


    //$scope.getJwActivityId = function (OSTransformationPOId) {
    //    $http.get('JobWork/OSTransformationPO/GetJWTransformationPurchaseOrderId?OSTransformationPOId=' + OSTransformationPOId).then(function (response) {
    //        var DropDownListObj = $("#ddlActivityList").data("ejDropDownList");
    //        for (var j = 0; j < response.data.length; j++) {
    //            DropDownListObj.selectItemByValue(response.data[j].Id);
    //        }
    //    });


    //};
    // $scope.GetLCByContract();

    $scope.CurrencyId = null;
    $scope.a = function (args) {
        var gridObj = $("#Grid123").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.rowID = $scope.data.Id;
        $scope.CurrencyId = $scope.data.CurrencyId;
        $scope.GetLCByContract();
    };
    $scope.calculateTaxCategoryRate = function () {

        $scope.detailModel.TotalTaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
        var tAmount = baseService.isUndefinedOrNull($scope.detailModel.TransactionRate) ? 0 : parseFloat($scope.detailModel.TransactionRate);
        if (tQty > 0)
            //$scope.detailModel.TransactionRate = tAmount / tQty;
            $scope.detailModel.TransactionAmount = tAmount * tQty;
        else
            //$scope.detailModel.TransactionRate = 0;
            $scope.detailModel.TransactionAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.detailModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;
    };

      $scope.productNew.TaxOptionService = "Yes";
  //  $scope.productNew.TaxOptionService = 'Yes';

    $scope.changeService = function (JWServiceId) {
       
        //productNewwww.TaxOptionService

        if (baseService.isUndefinedOrNull(JWServiceId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === JWServiceId; })[0].HSNCodeId;
        var HSNCode = $.grep($scope.serviceList, function (item) { return item.Value === JWServiceId; })[0].HSNCode;
        getTaxCategoryList(hsnCodeId, HSNCode);
        $scope.productNew.TaxOptionServiceTPO = "Yes";
  //      $scope.productNew.TaxOptionService = "Yes";
        
    };
    function getTaxCategoryList(hsnCodeId, HSNCode) {
        $scope.taxCategoryList = [];
        $scope.TotalAmount = $scope.detailModel.TransactionQty * $scope.detailModel.RatePerUnit;
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.productNew.PODate
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].hsnCodeId)) {
                    $scope.taxCategoryList[i].HSNCode = HSNCode;
                    $scope.taxCategoryList[i].HSNCodeId = hsnCodeId;
                    $scope.taxCategoryList[i].JWTransformationPOId = $scope.productNew.Id;
                    $scope.taxCategoryList[i].JWTransformationPODetailId = $scope.detailModel.Id;
                    //$scope.taxCategoryList[i].OSTransformationPODetailId = $scope.detailModel.Id;
                    $scope.taxCategoryList[i].TaxAmount = ($scope.TotalAmount * $scope.taxCategoryList[i].Percentage) / 100;

                    //$scope.HSNCode = HSNCode;
                }
            }
        });
    }


    function getDetailTaxCategoryList(x) {
        $scope.taxCategoryList = [];
        //if ($scope.productNew.OrderSpecific == 'Yes') {
        $scope.detailModel = x;
        //}
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetPODetailTaxList?jwPOId=' + $scope.productNew.Id + '&jwPoDetailId=' + x.Id
        }).then(function (response) {
            $scope.taxCategoryList = response.data;

            if ($scope.taxCategoryList.length == 0) {

                getTaxCategoryList('', '');
            }
            else {
                $scope.HSNCode = $scope.taxCategoryList[0].HSNCode;
                $scope.HSNCodeId = $scope.taxCategoryList[0].HSNCodeId;
            }
        });
    }
    $scope.recorddoubleclickContract = function ($event) {

        var x = $event;
        var Id = x.data.Id;

        for (var i = 0; i < $scope.GriddataPOWithOutLC.length; i++) {
            if ($scope.GriddataPOWithOutLC[i].Id === $scope.rowID) {

                if ($scope.CurrencyId === x.data.CurrencyId) {
                    $scope.GriddataPOWithOutLC[i].PurchaseLCId = x.data.Value;
                    angular.element(document.querySelector('#ContractPopUp')).modal('hide');
                } else {
                    ShowResult("Purchase Order Currency and PurchaseLC Currency is not same!!!", 'failure', 'ContractPopUp');
                }
            }
        }

    };



    $scope.UpdatePOforLCdata = function () {

        if ($scope.data.PurchaseLCId === null || $scope.data.PurchaseLCId === '' || $scope.data.PurchaseLCId === undefined) {
            ShowResult('Please select Purchase LC');
            return false;
        }


        $http({
            method: 'POST',
            url: "Products/PurchaseOrder/UpdatePOforLC",
            data:
            {
                POId: $scope.rowID,
                PurchaseLCId: $scope.data.PurchaseLCId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.getDataList();
                $scope.getalldataPOWithOutLC();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });





    }
    //#endregion


    $scope.invoicingPartyPopUp = function () {
        // getPartyPlantEditList();
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {

        //$scope.dbval = $scope.StateData;
        //$scope.UIval = $scope.productNew.InvoicingState;      

        //if ($scope.inventoryMaterialList.length == 0) {
        //    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //}
        //else if ($scope.dbval.length == 0)
        //{
        //    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //}
        //else if ($scope.dbval == $scope.UIval ) {            
        //    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //}
        //else {
        //    ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');

        //}

        if ($scope.inventoryMaterialList.length || $scope.chargesList.length) {
            if (!baseService.isUndefinedOrNull($scope.productNew.ChangeInvoicingStateId)) {
                if ($scope.productNew.PlantStateId === $scope.productNew.InvoicingStateId == $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.InvoicingStateId === $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.PlantStateId !== $scope.productNew.InvoicingStateId && $scope.productNew.PlantStateId != $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else
                    ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
            }
            else
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        }
        else
            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');






    };
    //#endregion
    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.productNew.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.productNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };

    function getPartyPlantList() {


        //var aa = $scope.Id;
        $scope.plantList = [];
        $http.get('Products/PurchaseOrder/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address2;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });

    }

    function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.Value == invoicingPartyPlantId) {
                    //$scope.partyPlantId = item.Value;
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = deliveryplant;
                    $scope.productNew.InvoicingByAddress = invoAddress;
                    $scope.productNew.DeliveryByAddress = deliAddress;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = deliState;
                    $scope.productNew.DeliveryGSTIN = deliGSTIN;
                }
            });
        });
    }

    //#region Save Update Delete Function

    $scope.Save = function () {
      //  $scope.$broadcast('show-errors-check-validity');
        try {
            $scope.dbval = $scope.StateData;
            $scope.UIval = $scope.productNew.InvoicingState;

            //if ($scope.inventoryMaterialList.length === 0) {
            //    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            //}
            //else 
            //if ($scope.dbval.length === 0) {
            //    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            //}
            //else if ($scope.dbval === $scope.UIval) {
            //    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            //}
            if ($scope.productNew.OrderSpecific === 'Yes' && baseService.isUndefinedOrNull($scope.productNew.ContractNo)) {

                ShowResult('Please Select Contract');
                return false;
            }
           
            else if (baseService.isUndefinedOrNull($scope.productNew.PartyName)) {
                ShowResult('Please Select Party Name');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
                ShowResult('Please Select Payment Term');
                return false;
            }
            
            else if ($scope.checkedByList.length > 0 && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult('Please Select Checked By/Approved By');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.productNew.EntityId)) {
                ShowResult('Please Select Entity');
                return false;
            }
            else if (baseService.isUndefinedOrNull($scope.productNew.ContractStatus)) {
                ShowResult('Please Select Contract Status');
                return false;
            }
            
            else {
                ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');

            }
            var DropDownActivityListObj = $("#ddlActivityList").data("ejDropDownList");
            //var activityList = DropDownActivityListObj.getSelectedValue().split(',');


            if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
            //$scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
            $scope.modelValidation('div_PODate', 'productNew', 'PODate', 'PO Entry Date');

            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);

            //$scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {

                if (new Date($scope.productNew.PODate) < new Date($scope.productNew.DocDate))
                    return manualValidation('div_PODate', true, "PO date can't be less than Doc entry date");
                else
                    manualValidation('div_PODate', false);

                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'data': $scope.product
                            , 'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti
                            , 'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.Data.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;

                            $scope.Action = "Update";
                            $scope.getalldata();
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {

                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'data': $scope.product
                            , 'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti
                            , 'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                            //, 'ActivityList': activityList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getalldata();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            throw e;
        }
    };
    $scope.manualValidationAddRemove = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else if (isNaN($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };

    $scope.Delete = function () {
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
            if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl + $scope.productNew.Id,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        //$scope.getDataList();
                        ClearFields();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        }
        else
            ShowResult('First delete all line item.', 'failure');
    };



    $scope.DetailDelete = function () {

        $http({
            method: 'POST',
            url: $scope.detailDeleteUrl,
            data: { 'Id': $scope.detailModel.Id, 'OrderSpecific': $scope.productNew.OrderSpecific},
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldata();
                $scope.getPoChilddata();
                $scope.clearCharNames();
                $scope.uom();
                $scope.detailClear();
                $scope.closeDetaiPopUp();

            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });

    };

    $scope.Clear = function () {

        ClearFields();
        $scope.NotificationSettingStatus();
        $scope.PoChildList = [];
        $scope.productNew.POType = "JWTransformationPO";
        $scope.Imagedata = [];
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        return true;

    };
    //#endregion


    //#region Otheres Code 

    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, "DESC", 'Id', 'PartyName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.products = [];
                    $scope.products = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };


    //$scope.getDataList();
    $scope.uom = function () {
        cboService.getUoMCbo(function (response) {
            $scope.uoMList = response;
        });
    }
    $scope.uom();
    $scope.storageList = [];
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });
    $scope.currencyList = [];


    //$scope.productNew.OrderSpecific = 'No';
    //addressService.getCountryCbo(function (result) {
    //    $scope.countryList = result;
    //});






    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        $scope.productNew.BaseCurrencyId = response.data[0].Value;

    });





    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });

    $http.get('accounts/OpeningBalance/GetACCCutOffDate')
        .then(function (response) {
            if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                $scope.productNew.CutOffDate = response.data.CutOffDate;
                $('#cutOffDate').datepicker('setStartDate', new Date($scope.productNew.CutOffDate));
            }
            else
                ShowResult('Cut Off date not found!', 'failure');
        });




    $scope.searchByList = [
        {
            value: 'PartyCode'
            , name: 'Vendor Code'
        },
        {
            value: 'PartyName'
            , name: 'Vendor Name'
        },
        {
            value: 'PartyAccountGroupName'
            , name: 'Account Group'
        },
        {
            value: 'Id'
            , name: 'GRN No'
        },
        {
            value: 'GRNDate'
            , name: 'GRN Date'
        },
        {
            value: 'DocRefNo'
            , name: 'Vendor DocRefNo'
        },
        {
            value: 'InvoiceNo'
            , name: 'Invoice No'
        },
        {
            value: 'InvoiceDate'
            , name: 'Invoice Date'
        }
    ];

    $scope.partySearchByList = [
        {
            'name': $scope.partyType + ' Code',
            'value': 'Code'
        },
        {
            'name': $scope.partyType + ' Name',
            'value': 'UserName'
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

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
        getPartyPlantList();
        //getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        //$scope.getToCurrencyRate();
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }


        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };



    //$scope.getJwActivityId = function (OSTransformationPOId) {
    //    $http.get('JobWork/OSTransformationPO/GetJWTransformationPurchaseOrderId?OSTransformationPOId=' + OSTransformationPOId).then(function (response) {
    //        var DropDownListObj = $("#ddlActivityList").data("ejDropDownList");
    //        for (var j = 0; j < response.data.length; j++) {
    //            DropDownListObj.selectItemByValue(response.data[j].Id);
    //        }
    //    });


    //};

    function GetMasterData() {
        var aa = $("#masterId").text();
        $http.get('Products/PurchaseOrder/GetPOMasterById?id=' + aa).then(function (response) {
            $scope.productNew = response.data;
        });

        getPartyPlantList();
        // getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        //$scope.getToCurrencyRate();
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };


    // #region Extra Tax Add
    $scope.calculateTaxAmount = function (data) {
        //data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.receiveTaxList = [];
    $scope.taxcboCategoryList = [];
    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
            $scope.taxcboCategoryList = result;
        });
    };
    $scope.taxcboCategoryList = [];
    accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
        $scope.taxcboCategoryList = result;
    });
    $scope.addTax = function () {
        var data = {
            JWTransformationPODetailId: $scope.detailModel.Id,
            TaxAmount: 0,
            Id: "",
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null

        };
        $scope.taxCategoryList.push(data);

    };

    // #endregion 

    $scope.Clearcontract = function () {
        $scope.productNew.CustomerName = "";
        $scope.productNew.ContractNo = "";
        $scope.productNew.ContractId = "";

    };
    function ClearFields() {
        $scope.Action = "Save";
        //$scope.product = {};
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew = Object.assign({}, $scope.products);

        // $scope.productNew = Object.assign({}, $scope.product);

        $scope.inventoryMaterialList = [];
        $scope.chargesList = [];
        $scope.grossTotal = 0;
        baseService.removeErrorClasses();
        $scope.ClearFields();
        $scope.PoChildList = [];
    }


    $scope.changeAllInvoice = function () {
        $scope.productNew.InvoiceNo = null;
        $scope.productNew.InvoiceDate = null;
    };

    $scope.closePartyPopUp = function (x) {
        //debugger;
        //if ($scope.partyIndex !== -1) {
        var party = x.data;
        // var party = $scope.partyList[$scope.partyIndex];
        $scope.productNew.PartyCode = party.Code;
        $scope.productNew.PartyName = party.UserName;
        $scope.productNew.PartyId = party.Id;
        $scope.productNew.PaymentTermId = party.PaymentTermId;
        $scope.productNew.CurrencyId = party.CurrencyId;
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew.BaseOnDueDate = null;
        $scope.productNew.BaseNoOfDays = null;
        $scope.productNew.MatureDate = null;

        $scope.productNew.TaxApplicable = party.TaxApplicable;
        $scope.productNew.IsTaxApplicableChangeable = party.IsTaxApplicableChangeable;
        if (party.TaxApplicable === 'Mandatory')
            $scope.productNew.IsTaxApplicable = true;
        else
            $scope.productNew.IsTaxApplicable = false;

        if (!baseService.isUndefinedOrNull($scope.productNew.DocDate))
            $scope.changePaymentTerm();
        getPartyPlantList();
        $scope.hidePartyPopUp();
        $scope.PaymentModeByPaymentTerm();
        //}
    };
    $scope.GetCurrencyExchangeRateList = function () {

        //if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
        if (!baseService.isUndefinedOrNull(!baseService.isUndefinedOrNull($scope.productNew.CurrencyId))) {
            $http({
                method: "GET",
                //url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?currencyId=" + $scope.productNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.productNew.ToCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };

    // GetToCurrencyRate
    $scope.getToCurrencyRate = function () {
        $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
        if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
            $scope.productNew.ToCurrencyRate = 1;
            return;
        }
        $http.get('Products/PurchaseOrder/GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.DocDate))
            .then(function (response) {
                if (parseFloat(response.data) === 0)
                    $scope.productNew.ToCurrencyRate = 1;
                else
                    $scope.productNew.ToCurrencyRate = response.data;
            });
    };



    $scope.billShippAddress = function (id, flag) {

        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
            var stateId = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateId;// 30-5
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = state;
                $scope.productNew.ChangeInvoicingStateId = stateId;//30-5
                $scope.productNew.InvoicingGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = state;
                $scope.productNew.DeliveryGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = null;
                $scope.productNew.InvoicingGSTIN = null;
                return $scope.productNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = null;
                $scope.productNew.DeliveryGSTIN = null;
                return $scope.productNew.DeliveryByAddress = null;
            }
        }

    };
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #region DetailsisSetIndex2
    $scope.detailTempModel = {
        Id: null
        , JWTransformationPOId: null
        , JWItemId: null
        , JWItemName: null
        , JWServiceName: null
        , JWServiceId: null
        , JWItemUOMId: null
        , JWItemUOM: null
        , MaterialMasterId: null
        , MaterialMasterName: null
        , HasAttribute: null
        , WithSKU: null
        , ArticleId: null
        , ArticleName: null
        , FirstCharacteristicsId: null
        , FirstCharacteristicsValueId: null
        , SecondCharacteristicsId: null
        , SecondCharacteristicsValueId: null
        , ThirdCharacteristicsId: null
        , ThirdCharacteristicsValueId: null
        , CountryId: null
        , TransactionQty: null
        , TransactionUoMId: null
        , BaseQty: null
        , BaseUOMId: null
        , BaseUoMFactor: null
        , TransactionRate: null
        , TotalTransactionAmount: null
        , ChargesAmount: null
        , TotalTaxAmount: null
        , WithInvoiceRate: null
        , AfterInvoiceRate: null
        , GRNRcvQty: null
        , QtyStatus: null
        , Description: null
        , DeliveryDate: null
        , AddedBy: null
        , AddedDate: null
        , AddedFromIP: null
        , UpdatedBy: null
        , UpdatedDate: null
        , UpdatedFromIP: null
        , DocRefNo: null
        , MaterialSpecification: null
        , FinalOutputCategory: null
        , MaterialLocationId: null
        , MaterialReference: null
        , OutputMaterialUOMId: null
        , OutputMaterialUOM: null
        , OrderSpecific: null
        , RequiredCapacity: null
        , ByProductApplicable: null
        , RateApplyOn: null
        , CurrencyId: null
        , RatePerUnit: null
        , Rejection: null
        , ValueLoss: null
        , ResponsiblePersonId: null
        , Remarks: null
        , Tolerance: null
        , ServiceId: null
        //, JWService: null
        , EmployeeCode: null
        , ResponsiblePerson: null
        , MaterialCode: null
        , MaterialName: null
        , MaterialMasterId: null
    };
    $scope.detailModel = Object.assign({}, $scope.detailTempModel);


    $scope.detailPopUp = function () {
        $scope.productNew.TaxOptionMat = 'Yes';
        $scope.productNew.TaxOptionService = "Yes";
    //    $scope.productNew.TaxOptionService = 'Yes';

        $scope.receiveTaxList = [];
        $scope.detailModel = Object.assign({}, $scope.detailTempModel);
        //$scope.MatPlanning = Object.assign({}, $scope.MatPlanningModelTemp);
        angular.element(document.querySelector('#detailPopUp')).modal('show');
        $scope.GetJWActivityListByPOType();
        $scope.GetJWLocation();
    };

    $scope.GetMatMstJW = [];
    $scope.GetMaterialfromJW = function () {
        $scope.GetMatMstJW = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetMaterialfromJW?JobWorkItemId=' + $scope.detailModel.JobWorkItemMasterId,
        }).then(function successCallback(response) {
            $scope.GetMatMstJW = response.data;
            if ($scope.GetMatMstJW.length > 0) {
                if ($scope.GetMatMstJW[0].Id != null) {
                    $scope.detailModel.MaterialMasterId = $scope.GetMatMstJW[0].Id;
                    $scope.detailModel.MaterialName = $scope.GetMatMstJW[0].Material;
                    $scope.detailModel.MaterialCode = $scope.GetMatMstJW[0].Code;
                      $scope.OMatUOMList = [];
                        for (var i = 0; i < $scope.GetMatMstJW.length; i++) {
                            if (!baseService.isUndefinedOrNull($scope.GetMatMstJW[i].Value)) {
                                $scope.OMatUOMList[i] = $scope.GetMatMstJW[i];
                                $scope.detailModel.OutputMaterialUOMId = $scope.GetMatMstJW[0].Value;
                                
                            }
                    }
                    if ($scope.GetMatMstJW[0].WithSKU == '1') {
                        $scope.getRMCharacteristicsList();
                    }
                }
                else {
                    $scope.detailModel.MaterialMasterId = null;
                    $scope.detailModel.MaterialName = null;
                    $scope.detailModel.MaterialCode = null;
                    $scope.detailModel.AlternateUoM = null;
                    $scope.SKU1disable = true;
                    $scope.SKU2disable = true;
                    $scope.SKU3disable = true;
                    $scope.OMatUOMList = [];
                    $scope.OMatUOMList[0] = $scope.GetMatMstJW[0];
                    $scope.detailModel.OutputMaterialUOMId = $scope.GetMatMstJW[0].Value;
                 
                }
                
            }
        });
    }

    $scope.detailPopUpForEdit = function (args) {
        if ($scope.productNew.OrderSpecific == 'Yes') {
            $scope.taxCategoryList = [];
            $scope.getalldataListForBOQListUpdate(args);
            $scope.BOQServiceGet();
        }
        else {
            $scope.taxCategoryList = [];
            $scope.detailModel = Object.assign({}, args);
            $scope.detailModel.JWMaterialStorage = $scope.detailModel.MaterialStorage;
            $scope.GetJWActivityListByPOType();
            $scope.GetJWItemsToEdit();         
            $scope.GetJWitemDataFromTrans();
            //$scope.GetRate();
            $scope.GetRateToEdit();
            $scope.GetCurrencyyy();
            $scope.GetJWLocation();
            $scope.productNew.TaxOptionServiceTPO = "Yes";
       //   $scope.detailModel.ServiceId = $scope.detailModel.ServiceId;
            //$scope.detailModel.ValueLoss = $scope.detailModel.ValueLoss;

            $scope.rmchar1 = {};
            $scope.rmchar2 = {};
            $scope.rmchar3 = {};

            if (!baseService.isUndefinedOrNull($scope.detailModel.ArticleId)) {
                $scope.hasArticle = true;
            }
            else {
                $scope.hasArticle = false;
            }

            if (!baseService.isUndefinedOrNull($scope.detailModel.FirstCharacteristicsId)) {
                $scope.rmchar1.CharacteristicsId = $scope.detailModel.FirstCharacteristicsId;
                $scope.rmchar1.CharacteristicsValueId = $scope.detailModel.FirstCharacteristicsValueId;

                $scope.rmchar1.Name = $scope.detailModel.FirstCharacteristics;
                $scope.rmchar1.FreeText = $scope.detailModel.FirstCharacteristicsValue;

          //      $scope.detailModel.Code1 = $scope.detailModel.SKU1ValueCode;
                $scope.detailModel.UserName1 = $scope.detailModel.FirstCharacteristicsValue;
                $scope.rmchar1.ValueAssignmentLevel = $scope.detailModel.ValueAssignmentLevel;
                $scope.SKU1disable = false;

            }
            if (!baseService.isUndefinedOrNull($scope.detailModel.SecondCharacteristicsId)) {
                $scope.rmchar2.CharacteristicsId = $scope.detailModel.SecondCharacteristicsId;
                $scope.rmchar2.CharacteristicsValueId = $scope.detailModel.SecondCharacteristicsValueId;

                $scope.rmchar2.Name = $scope.detailModel.SecondCharacteristics;
                $scope.rmchar2.FreeText = $scope.detailModel.SecondCharacteristicsValue;

            //    $scope.detailModel.Code2 = $scope.detailModel.SKU2ValueCode;
                $scope.detailModel.UserName2 = $scope.detailModel.SecondCharacteristicsValue;
                $scope.rmchar2.ValueAssignmentLevel = $scope.detailModel.ValueAssignmentLevel;
                $scope.SKU2disable = false;
            }
            if (!baseService.isUndefinedOrNull($scope.detailModel.ThirdCharacteristicsId)) {
                $scope.rmchar3.CharacteristicsId = $scope.detailModel.ThirdCharacteristicsId;
                $scope.rmchar3.CharacteristicsValueId = $scope.detailModel.ThirdCharacteristicsValueId;

                $scope.rmchar3.Name = $scope.detailModel.ThirdCharacteristics;
                $scope.rmchar3.FreeText = $scope.detailModel.ThirdCharacteristicsValue;

          //      $scope.detailModel.Code3 = $scope.detailModel.SKU3ValueCode;
                $scope.detailModel.UserName3 = $scope.detailModel.ThirdCharacteristicsValue;
                $scope.rmchar3.ValueAssignmentLevel = $scope.detailModel.ValueAssignmentLevel;
                $scope.SKU3disable = false;
            }

            getDetailTaxCategoryList($scope.detailModel);

            angular.element(document.querySelector('#detailPopUp')).modal('show');
        }



    };


    $scope.detailClear = function () {
        $scope.detailModel = Object.assign({}, $scope.detailTempModel);
        $scope.rmchar3 = {};
        $scope.rmchar3 = {};
        $scope.rmchar3 = {};
        $scope.hasArticle = false;
        $scope.clearCharNames();
    };


    $scope.closeDetaiPopUp = function () {
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
        $scope.detailModel = Object.assign({}, $scope.detailTempModel);
    };
    $scope.rmchar1 = {
        CharacteristicsId: null,
        CharacteristicsValueId: null,
        Name: null,
        FreeText: null
    };
    $scope.rmchar2 = {
        CharacteristicsId: null,
        CharacteristicsValueId: null,
        Name: null,
        FreeText: null
    };
    $scope.rmchar3 = {
        CharacteristicsId: null,
        CharacteristicsValueId: null,
        Name: null,
        FreeText: null
    };

    $scope.CombinationList = [];
    $scope.detailSave = function (type, onlyTax) {
        activityListsel = null;
        $scope.detailModelList = [];
        try {

            if (type !== "BOQ" && type !=="PODETAILLIST") {
            if (baseService.isUndefinedOrNull($scope.detailModel.JobActivityId)) {
                ShowResult('Please select Job Work Acticity', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.JobWorkItemMasterId)) {
                ShowResult('Please select Job Work Out Put Item', 'failure', 'detailPopUp');
                return false;
            }
            //if (baseService.isUndefinedOrNull($scope.detailModel.MaterialName)) {
            //    ShowResult('Please select Material', 'failure', 'detailPopUp');
            //    return false;
            //}
            if (!baseService.isUndefinedOrNull($scope.detailModel.MaterialName)) {
                if (baseService.isUndefinedOrNull($scope.detailModel.ArticleName)) {
                    ShowResult('Please select Article', 'failure', 'detailPopUp');
                    return false;
                }
            }

            if (baseService.isUndefinedOrNull($scope.detailModel.MaterialLocationId)) {
                ShowResult('Please select Material Storage Location', 'failure', 'detailPopUp');
                return false;
            }
            //if (baseService.isUndefinedOrNull($scope.detailModel.MaterialType)) {
            //    ShowResult('Please select Input Material Category', 'failure', 'detailPopUp');
            //    return false;
            //}
            //if (baseService.isUndefinedOrNull($scope.detailModel.FinalOutputCategory)) {
            //    ShowResult('Please select OutPut Material Category', 'failure', 'detailPopUp');
            //    return false;
            //}
            if (baseService.isUndefinedOrNull($scope.detailModel.MaterialSpecification)) {
                ShowResult('Please select Material Specification', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.OutputMaterialUOMId)) {
                ShowResult('Please select UOM', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) || $scope.detailModel.TransactionQty === '0') {
                ShowResult('Please select Quantity', 'failure', 'detailPopUp');
                return false;
            }
            //if (baseService.isUndefinedOrNull($scope.detailModel.OrderSpecific)) {
            //    ShowResult('Please select Order Specific', 'failure', 'detailPopUp');
            //    return false;
            //}
            //if (baseService.isUndefinedOrNull($scope.detailModel.RequiredCapacity)) {
            //    ShowResult('Please select Required Capacity', 'failure', 'detailPopUp');
            //    return false;
            //}

                if ($scope.productNew.POType == "JWTransformationPO") {
                if (baseService.isUndefinedOrNull($scope.detailModel.ByProductApplicable)) {
                    ShowResult('Please select ByProduct Applicable', 'failure', 'detailPopUp');
                    return false;
                }
            }

            if (baseService.isUndefinedOrNull($scope.detailModel.RateApplyId)) {
                ShowResult('Please select Rate Apply', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.CurrencyId)) {
                ShowResult('Please select Currency', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.RatePerUnit) || $scope.detailModel.RatePerUnit === '0') {
                ShowResult('Please select Rate Per Unit', 'failure', 'detailPopUp');
                return false;
            }
            //if (baseService.isUndefinedOrNull($scope.detailModel.Rejection)) {
            //    ShowResult('Please select Rejection', 'failure', 'detailPopUp');
            //    return false;
            //}
            //if (baseService.isUndefinedOrNull($scope.detailModel.ValueLoss)) {
            //    ShowResult('Please select ValueLoss', 'failure', 'detailPopUp');
            //    return false;
            //}
            //if (baseService.isUndefinedOrNull($scope.detailModel.Tolerance)) {
            //    ShowResult('enter the Tolerance', 'failure', 'detailPopUp');
            //    return false;
            //}
            if (baseService.isUndefinedOrNull($scope.detailModel.ServiceId)) {
                ShowResult('Please select Service', 'failure', 'detailPopUp');
                return false;
            }
           
            if (baseService.isUndefinedOrNull($scope.detailModel.EmployeeCode)) {
                ShowResult('Please select Responsible Person', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.ResponsiblePersonId)) {
                ShowResult('Please select Responsible Person', 'failure', 'detailPopUp');
                return false;
            }
                $scope.ValidateRate();


            $scope.detailModel.JWTransformationPOId = $scope.productNew.Id;
            if (!baseService.isUndefinedOrNull($scope.rmchar1.CharacteristicsId)) {
                $scope.detailModel.FirstCharacteristicsId = $scope.rmchar1.CharacteristicsId;
                $scope.detailModel.FirstCharacteristicsValueId = $scope.rmchar1.CharacteristicsValueId;
            }
            if (!baseService.isUndefinedOrNull($scope.rmchar2.CharacteristicsId)) {
                $scope.detailModel.SecondCharacteristicsId = $scope.rmchar2.CharacteristicsId;
                $scope.detailModel.SecondCharacteristicsValueId = $scope.rmchar2.CharacteristicsValueId;
            }
            //if (!baseService.isUndefinedOrNull($scope.rmchar3)) {
            //    $scope.detailModel.ThirdCharacteristicsId = $scope.rmchar3.CharacteristicsId;
            //    $scope.detailModel.ThirdCharacteristicsValueId = $scope.rmchar3.CharacteristicsValueId;
            //    }

                if (!baseService.isUndefinedOrNull($scope.rmchar3.CharacteristicsId)) {
                    $scope.detailModel.ThirdCharacteristicsId = $scope.rmchar3.CharacteristicsId;
                    $scope.detailModel.ThirdCharacteristicsValueId = $scope.rmchar3.CharacteristicsValueId;
                }
        }
       
        //try {

            if (type === "BOQ") {
                var DropDownActivityListObj = $("#ddlActivityList").data("ejDropDownList");
                activityListsel = "";//"'" + DropDownActivityListObj.getSelectedValue().split(",").join("','") + "'";
                $scope.detailModelList = $filter('filter')($scope.GetListForMasterOrder, { 'CheckedStatus': true });
                for (var i = 0; i < $scope.detailModelList.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.detailModelList[i].TransactionQty)) {
                        ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial');
                        return false;
                    }
                    else if ($scope.detailModelList[i].TransactionQty === '0' || $scope.detailModelList[i].TransactionQty === '0.00' || $scope.detailModelList[i].TransactionQty === '0.0') {
                        ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial');
                        return false;
                    }
                    else if ($scope.detailModelList[i].IncompleteMaterial === 'Yes') {
                        ShowResult('This is incomplete material.So you can not take this material', 'failure', 'ListOfPOMaterial');
                        return false;
                    }

                    if (baseService.isUndefinedOrNull($scope.detailModelList[i].TransactionRate)) {
                        ShowResult('Please Enter Rate', 'failure', 'ListOfPOMaterial');
                        return false;
                    }
                    if (baseService.isUndefinedOrNull($scope.detailModelList[i].ServiceId)) {
                        ShowResult('Please Select Service', 'failure', 'ListOfPOMaterial');
                        return false;
                    }

                    if ($scope.Action === "Save") {
                        if (!baseService.isUndefinedOrNull($scope.detailModelList[i].TransactionQty)) {
                            if ($scope.detailModelList[i].TransactionQty > $scope.detailModelList[i].BalanceQuantity) {
                                ShowResult('Current Quantity cannot be greater than Balance Quantity for Material ' + $scope.detailModelList[i].UserName + ' and Article ' + $scope.detailModelList[i].StandardName + ' ', 'failure', 'ListOfPOMaterial');
                                return false;
                            }

                        }
                    }
      

                    var MaterialId = $scope.detailModelList[i].MaterialMasterId;
                    var ArticleId = $scope.detailModelList[i].ArticleId;
                    var SKU1 = $scope.detailModelList[i].FirstCharacteristicsValueId;
                    var SKU2 = $scope.detailModelList[i].SecondCharacteristicsValueId;
                    var SKU3 = $scope.detailModelList[i].ThirdCharacteristicsValueId;
                    $scope.CombinationList = $filter('filter')($scope.detailModelList, { 'MaterialMasterId': MaterialId, 'ArticleId': ArticleId, 'FirstCharacteristicsValueId': SKU1, 'SecondCharacteristicsValueId': SKU2, 'ThirdCharacteristicsValueId': SKU3 });
                    if ($scope.CombinationList.length > 0) {
                        var Rate = $scope.CombinationList[0].TransactionRate;
                        var Service = $scope.CombinationList[0].ServiceId;
                        var TransUoMId = $scope.CombinationList[0].TransactionUoMId;
                        for (var p = 0; p < $scope.detailModelList.length; p++) {
                            if ($scope.CombinationList[0].MaterialMasterId == $scope.detailModelList[p].MaterialMasterId && $scope.CombinationList[0].ArticleId == $scope.detailModelList[p].ArticleId && $scope.CombinationList[0].FirstCharacteristicsValueId == $scope.detailModelList[p].FirstCharacteristicsValueId && $scope.CombinationList[0].SecondCharacteristicsValueId == $scope.detailModelList[p].SecondCharacteristicsValueId && $scope.CombinationList[0].ThirdCharacteristicsValueId == $scope.detailModelList[p].ThirdCharacteristicsValueId) {

                                if ($scope.detailModelList[p].TransactionRate != Rate) {
                                    ShowResult('Rate should be same for Material ' + $scope.detailModelList[p].UserName + ' and Article ' + $scope.detailModelList[p].StandardName + ' ', 'failure', 'ListOfPOMaterial');
                                    return false;
                                }
                                if ($scope.detailModelList[p].ServiceId != Service) {
                                    ShowResult('Service should be same for Material ' + $scope.detailModelList[p].UserName + ' and Article ' + $scope.detailModelList[p].StandardName + ' ', 'failure', 'ListOfPOMaterial');
                                    return false;
                                }
                                if ($scope.detailModelList[p].TransactionUoMId != TransUoMId) {
                                    ShowResult('Transaction UoM should be same for Material ' + $scope.detailModelList[p].UserName + ' and Article ' + $scope.detailModelList[p].StandardName + ' ', 'failure', 'ListOfPOMaterial');
                                    return false;
                                }
                            }

                          
                        }
                    }
                    


                    $scope.taxCategoryList = null;
                }
                $scope.materialValidationForBOQItem($scope.detailModelList);

            }
            else if (type === "PODETAILLIST") {
                $scope.detailModelList = $scope.PoChildList;
            }
            else if (type === "JWITEM") {
                $scope.detailModelList.push($scope.detailModel);
            }
            if ($scope.detailModelList.length > 0) {
                $http({
                    method: 'POST',
                    url: $scope.detailSaveUrl,
                    data: {
                        data: $scope.detailModelList,
                        onlyTax: onlyTax,
                        JWPurchaseOrderId: $scope.productNew.Id,
                        JWActivityId: activityListsel,
                        OrderSpecific: $scope.productNew.OrderSpecific,
                        type: type,
                        taxCategoryList: $scope.taxCategoryList,
                        JWPOToCurrencyRate: $scope.productNew.ToCurrencyRate,
                        JWPOIsNonCreditable: $scope.productNew.IsNonCreditable,
                        JWPODate: $scope.productNew.PODate,
                        JWPOType: $scope.productNew.POType

                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        if (type === "PODETAILLIST") {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'failure', 'detailPopUp');
                        }
                    }
                    else {
                        if (type === "PODETAILLIST") {
                            ShowResult(response.data.Message, 'success');
                        }
                        else {
                        //    ShowResult(response.data.Message, 'success', 'detailPopUp');
                            ShowResult(response.data.Message, 'success');
                        }

                        $scope.taxCategoryList = [];

                        $scope.getalldata();
                        $scope.getPoChilddata();
                        $scope.clearCharNames();
                        $scope.uom();
                        $scope.detailClear();
                        $scope.RequisitionListHide();
                        $scope.getPoChildAlldata();
                    //    $scope.clearCharNames();
                    }
                }), function errorCallBack(response) {
                    if (type === "PODETAILLIST") {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'failure', 'detailPopUp');
                    }

                };
            }
            else {
                if (type === "BOQ") {
                    ShowResult("Please select Atleast one BOQ Material", 'failure', 'ListOfPOMaterial');
                }
                else if (type === "BOQ") {
                    ShowResult("Data not Found", 'failure', 'detailPopUp');
                }
                else {
                    ShowResult("Data not Found", 'failure');
                }
            }


        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };

    $scope.SaveItemWiseTax = function (x) {
        $scope.detailModelList = [];
        $scope.detailModelList.push(x);
        $http({
            method: 'POST',
            url: $scope.detailSaveUrl,
            data: {
                data: $scope.detailModelList,
                TaxList: $scope.taxCategoryList,
                onlyTax: onlyTax,
                JWPurchaseOrderId: $scope.productNew.Id,
                JWActivityId: activityListsel,
                OrderSpecific: $scope.productNew.OrderSpecific

            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure', 'detailPopUp');
            else {
                ShowResult(response.data.Message, 'success', 'detailPopUp');

                $scope.taxCategoryList = [];

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'detailPopUp');
        };
    }

    $scope.valuePassInDelModal = function (id) {
        $scope.detailModel.Id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };

    $scope.PODetailsUpdatePOPUp = function (x, x1) {

    };



    $scope.getReceiveTaxList = function (x) {

        getDetailTaxCategoryList(x);

        //$scope.detailModel.Id = x.Id;

        $scope.taxAbleAmnt = x.TransactionAmount;

        $scope.productNew.TaxOptionMatJWTax = "Yes";
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };



    function checkChangeJWActivity(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.jwItemList, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headcheckChangeJWActivity(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#GridJWActivity").data("ejGrid");
            var filtered = $("#GridJWActivity").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.jwItemList.length; i++) {

                    $scope.jwItemList[i].isSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.jwItemList[i].EmpSystemId == filtered[j].EmpSystemId)
                            // $scope.EmployeeList[i].isSelect = true;
                            $scope.jwItemList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#GridJWActivity .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeJWActivity });
            }
        }
        else {
            var filtered = $("#GridJWActivity").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.jwItemList.length; i++) {
                    $scope.jwItemList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.jwItemList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.jwItemList[i].Id == filtered[j].Id)
                            $scope.jwItemList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#GridJWActivity .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeJWActivity });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#GridJWActivity .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headcheckChangeJWActivity });

    };
    $scope.refreshTemplateJWActivity = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headcheckChangeJWActivity });
        }

        var valobj = $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.EmployeeList, { 'EmpSystemId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].isToBeSelect == true)
                $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeJWActivity });
    };
    $scope.jwItemListTemp = [];

    $scope.saveJWItemdata = function () {
        $scope.jwItemListTemp = [];
        var row = $filter('filter')($scope.jwItemList, { 'isToBeSelect': true });

        $scope.jwItemListTemp = row;


        $scope.closeDetailJWItemPOPUp();
    };
    $scope.jwItemList = [];
    $scope.detailItemPopUp = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetJWItemList',
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.jwItemList = response.data;

        });
        angular.element(document.querySelector('#listofJWItem')).modal('show');
    };

    $scope.closeDetailJWItemPOPUp = function () {
        angular.element(document.querySelector('#listofJWItem')).modal('hide');
    };
    var activityListsel = "";
    $scope.getJWItemPOPUp = function () {
        var DropDownActivityListObj = $("#ddlActivityList").data("ejDropDownList");
        activityListsel = "'" + DropDownActivityListObj.getSelectedValue().split(",").join("','") + "'";

        $scope.getItemData();
        angular.element(document.querySelector('#jwITemPopupNew')).modal('show');

    };
    $scope.closeJWItemPOPUp = function () {
        angular.element(document.querySelector('#jwITemPopupNew')).modal('hide');
    };
    $scope.jwItemList = [];
    $scope.getItemData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetJWItemMAList?ActivityId=' + activityListsel,
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.jwItemList = response.data;
        });
    };

    $scope.getMaterial = function (obj) {

        $scope.detailModel.JWItemId = obj.data.JWItemId;
        $scope.detailModel.JWItemName = obj.data.JWItemName;
        $scope.detailModel.JWServiceName = obj.data.ServiceName;
        $scope.detailModel.ServiceId = obj.data.ServiceId;
        //  $scope.detailModel.MaterialMasterName = obj.data.MaterialMaster;
        $scope.detailModel.JWTransformationMasterId = obj.data.JWTransformationMasterId;

        $scope.detailModel.JWItemUOMId = obj.data.UOMId;
        $scope.detailModel.JWItemUOM = obj.data.UOM;
        $scope.detailModel.MaterialMasterName = obj.data.MaterialMaster;
        $scope.detailModel.MaterialMasterId = obj.data.MaterialMasterId;

        if (!baseService.isUndefinedOrNull($scope.detailModel.MaterialMasterId)) {

            $scope.setRMaterialMasterData(obj.data);
            UomCboByMaterialMaster($scope.detailModel.MaterialMasterId);

        }
        else {
            UomCboByMaterialMaster($scope.detailModel.MaterialMasterId);
        }
        $scope.changeService(obj.data.ServiceId);
        angular.element(document.querySelector('#jwITemPopupNew')).modal('hide');
    };
    $scope.PoChildList = [];
    $scope.getPoChilddata = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.detailGridListUrl + $scope.productNew.Id
        }).then(function successCallback(response) {
            $scope.PoChildList = response.data;
            if ($scope.PoChildList.length > 0) {
                for (var i = 0; i < $scope.PoChildList.length; i++) {
                    $scope.PoChildList[i].JWDeliveryDate = $filter('dateFiltering')($scope.PoChildList[i].DeliveryDate, 'dd-M-yyyy');
                }
                
            }
            
        });
    };
    $scope.PoChildListAll = [];
    $scope.getPoChildAlldata = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.detailGridListAllUrl
        }).then(function successCallback(response) {
            $scope.PoChildListAll = response.data;
        });
    };
    $scope.getPoChildAlldata();
    $scope.setRMaterialMasterData = function (ob) {

        $scope.detailModel.ArticleId = null;
        $scope.detailModel.ArticleName = null;
        $scope.detailModel.HasAttribute = ob.HasAttribute;
        $scope.detailModel.WithSKU = ob.WithSKU;

        $scope.detailModel.FirstCharacteristicsValueId = null;
        $scope.detailModel.SecondCharacteristicsValueId = null;
        $scope.detailModel.ThirdCharacteristicsValueId = null;

        $scope.detailModel.FirstCharacteristicsId = null;
        $scope.detailModel.SecondCharacteristicsId = null;
        $scope.detailModel.ThirdCharacteristicsId = null;

        $scope.clearCharNames();

        if ($scope.detailModel.HasAttribute) {
            $scope.materialType = null;
            $scope.getRMArticleSearchList(ob.MaterialMasterId);
        } else {
            $scope.closeMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }
        if ($scope.detailModel.WithSKU) $scope.getRMCharacteristicsList(ob.MaterialMasterId);

        if ($scope.detailModel.WithSKU) {
            $scope.msg = "has";
        } else {
            $scope.msg = "has no";
        }

        $scope.HSNCodeId = ob.HSNCodeId;
        $scope.closeRMMaterialMasterbyTypePopUp();
        UomCboByMaterialMaster($scope.detailModel.MaterialMasterId);

        $scope.detailModel.Specific = true;
    };


    $scope.SKU1disable = true;
    $scope.SKU2disable = true;
    $scope.SKU3disable = true;
    $scope.rmcharacteristicsList = [];
    $scope.rm1characteristicsList = [];
    $scope.rm2characteristicsList = [];
    $scope.getRMCharacteristicsList = function () {
        $scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                //        materialMasterId: id
                materialMasterId: $scope.detailModel.MaterialMasterId
            }
        }).then(function (response) {
            $scope.rmcharacteristicsList = [];
            $scope.rm1characteristicsList = [];
            $scope.rm2characteristicsList = [];
            $scope.rmcharacteristicsList = response.data.charData;
            $scope.rm1characteristicsList = response.data.charData;
            $scope.rm2characteristicsList = response.data.charData;
            if (baseService.arrayLength($scope.rmcharacteristicsList) > 0) {
                $scope.isSearch = $scope.rmcharacteristicsList[0].FreeText !== null ? true : false;
                $scope.rmchar1 = {
                    //CharacteristicsId: $scope.rmcharacteristicsList[0].Value
                    //, CharacteristicsValueId: $scope.rmcharacteristicsList[0].CharacteristicsValueId
                    //, MaterialMasterId: $scope.rmcharacteristicsList[0].MaterialMasterId
                     Name: $scope.rmcharacteristicsList[0].Text
                    //, IsFreeField: $scope.rmcharacteristicsList[0].IsFreeField
                    //, IsPreDefinedField: $scope.rmcharacteristicsList[0].IsPreDefinedField
                    //, IsMandatory: $scope.rmcharacteristicsList[0].IsMandatory
                    , ValueAssignmentLevel: $scope.rmcharacteristicsList[0].ValueAssignmentLevel
                    //, Sequence: $scope.rmcharacteristicsList[0].Sequence
                    //, FlagDisable: $scope.IsFreeOrNot($scope.rmcharacteristicsList[0].IsFreeField)

                    //, FreeText: $scope.rmcharacteristicsList[0].FreeText
                    //, show: true
                };
                $scope.detailModel.FirstCharacteristicsId = $scope.rmcharacteristicsList[0].Value;
                $scope.SKU1disable = false;
               
            }
            if (baseService.arrayLength($scope.rmcharacteristicsList) > 1) {
                $scope.isSearch = $scope.rmcharacteristicsList[1].FreeText !== null ? true : false;
                $scope.rmchar2 = {
                    //CharacteristicsId: $scope.rmcharacteristicsList[1].Value
                    //, CharacteristicsValueId: $scope.rmcharacteristicsList[1].CharacteristicsValueId
                    //, MaterialMasterId: $scope.rmcharacteristicsList[1].MaterialMasterId
                     Name: $scope.rmcharacteristicsList[1].Text
                    //, IsFreeField: $scope.rmcharacteristicsList[1].IsFreeField
                    //, IsPreDefinedField: $scope.rmcharacteristicsList[1].IsPreDefinedField
                    //, IsMandatory: $scope.rmcharacteristicsList[1].IsMandatory
                    , ValueAssignmentLevel: $scope.rmcharacteristicsList[1].ValueAssignmentLevel
                    //, Sequence: $scope.rmcharacteristicsList[1].Sequence
                    //, FlagDisable: $scope.IsFreeOrNot($scope.rmcharacteristicsList[1].IsFreeField)
                    //, FreeText: $scope.rmcharacteristicsList[1].FreeText
                    //, show: true
                };
                $scope.detailModel.SecondCharacteristicsId = $scope.rmcharacteristicsList[1].Value;
                $scope.SKU2disable = false;
                
            }
            if (baseService.arrayLength($scope.rmcharacteristicsList) > 2) {
                $scope.isSearch = $scope.rmcharacteristicsList[2].FreeText !== null ? true : false;
                $scope.rmchar3 = {
                    //CharacteristicsId: $scope.rmcharacteristicsList[2].Value
                    //, CharacteristicsValueId: $scope.rmcharacteristicsList[2].CharacteristicsValueId
                    //, MaterialMasterId: $scope.rmcharacteristicsList[2].MaterialMasterId
                     Name: $scope.rmcharacteristicsList[2].Text
                    //, IsFreeField: $scope.rmcharacteristicsList[2].IsFreeField
                    //, IsPreDefinedField: $scope.rmcharacteristicsList[2].IsPreDefinedField
                    //, IsMandatory: $scope.rmcharacteristicsList[2].IsMandatory
                    , ValueAssignmentLevel: $scope.rmcharacteristicsList[2].ValueAssignmentLevel
                    //, Sequence: $scope.rmcharacteristicsList[2].Sequence
                    //, FlagDisable: $scope.IsFreeOrNot($scope.rmcharacteristicsList[2].IsFreeField)
                    //, FreeText: $scope.rmcharacteristicsList[2].FreeText
                    //, show: true
                };
                $scope.detailModel.ThirdCharacteristicsId = $scope.rmcharacteristicsList[2].Value;
                $scope.SKU3disable = false;
            }
        });
    };

    $scope.clearCharNames = function () {
        //$scope.char1 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        //$scope.char2 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        //$scope.char3 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };

        $scope.rmchar1 = { Name: null, ValueAssignmentLevel: null };
        $scope.rmchar2 = { Name: null, ValueAssignmentLevel: null};
        $scope.rmchar3 = { Name: null, ValueAssignmentLevel: null };
        $scope.SKU1disable = true;
        $scope.SKU2disable = true;
        $scope.SKU3disable = true;
    };


    $scope.setCharData = function (data) {
        $scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope[$scope.charValueSearchFor].FreeText = data.UserName;
        $scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
        angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
    };

    $scope.closeRMMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('rmaterialmastersearchpopup');
        angular.element(document.querySelector('#rmaterialmastersearchpopup')).modal('hide');
        $scope.ShowHide();
    };

    $scope.getRMArticle = function (index) {
        //$scope.itemIndex = index;
        //if (!baseService.isUndefinedOrNull($scope.bomDetailNew.RMMaterialMasterId) && !$scope.bomNew.HasAttribute)
        //    return ShowResult('This material has no attribute', 'failure');
        $scope.getRMArticleSearchList($scope.detailModel.MaterialMasterId);
    };
    $scope.hasArticle = false;

    $scope.selectRMarticle = function (ob) {
        try {
            $scope.hasArticle = true;
            $scope.detailModel.MaterialMasterId = ob.MaterialMasterId;
            $scope.detailModel.MaterialMasterName = ob.MaterialMasterName;
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            angular.element(document.querySelector('#rarticleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'rarticleSearchPop');
        }
    };
    $scope.getRMArticleSearchList = function (id) {
        try {
            $scope.productNew.TaxOptionMat = 'Yes';
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

    $scope.showradiodiv = false;
    $scope.showradiocommon = false;
    $scope.showradiomatrix = false;

    $scope.ShowHide = function () {
        //if ($scope.bomDetailNew.WithSKU === true && $scope.bomNew.WithSKU) {
        //    $scope.showradiodiv = true;
        //    $scope.showradiocommon = true;
        //    $scope.showradiomatrix = true;
        //    $scope.matrixrad = true;
        //}
        //else if ($scope.bomDetailNew.WithSKU === true && $scope.bomNew.WithSKU === false) {
        //    $scope.showradiodiv = false;
        //    $scope.showradiocommon = true;
        //    $scope.showradiomatrix = false;
        //    $scope.matrixrad = true;
        //}
        //else {
        //    $scope.showradiodiv = false;
        //    $scope.showradiomatrix = false;
        //    $scope.showradiocommon = false;
        //    $scope.matrixrad = true;
        //}
    };


    //$scope.uOMList = [];
    function UomCboByMaterialMaster(materilaMasterId) {
        var mmId = []; mmId.push(materilaMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (response) {
            $scope.uOMList = response;
            //if (baseService.isUndefinedOrNull($scope.detailModel.JWItemUOM)) {

            //}
            if (baseService.arrayLength($scope.uOMList) == 1) {
                $scope.detailModel.TransactionUoMId = $scope.uOMList[0].Value;
            }
            else if (!baseService.isUndefinedOrNull($scope.uOMList)) {
                $scope.detailModel.TransactionUoMId = $scope.detailModel.JWItemUOMId;
            }
        });
    }
    $scope.changePaymentTerm = function () {

        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            $scope.productNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.productNew.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.productNew.BaseOnDueDate = null;
                    $scope.IsBaseOnDueDateEnable = false;
                }
            $scope.getMatureDate($scope.productNew.BaseOnDueDate, $scope.productNew.BaseNoOfDays);
        }
    };

    function getPartyPlantList() {


        //var aa = $scope.Id;
        $scope.plantList = [];
        $http.get('Products/PurchaseOrder/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address2;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });

    }
    $scope.PaymentModeList = [];
    $scope.PaymentModeByPaymentTerm = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/PurchaseOrder/PaymentModeByPaymentTerm?Id=' + $scope.productNew.PaymentTermId
        }).then(function successCallback(response) {
            $scope.PaymentModeList = response.data;
            $scope.productNew.PaymentMode = response.data[0].PaymentMode;

        });
    };
    $scope.TaxOption = function (data) {
        $scope.productNew.TaxOption = data;
    };
    $scope.TaxOptionMat = function (data) {
        $scope.productNew.TaxOptionMat = data;

    };
    $scope.TaxOptionService = function (data) {
        $scope.productNew.TaxOptionService = data;

    };
    $scope.TaxOptionServiceModify = function (data) {
        $scope.productNew.TaxOptionServiceModify = data;

    };

    // #region Extra Tax Add
    $scope.calculateTaxAmount = function (data) {
        //data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.calculateTaxAmountForMat = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.detailModel.TransactionAmount * data.Percentage) / 100;
    };
    $scope.checkRowValidationMat = function (x) {
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) || $scope.detailModel.TransactionAmount === 0) {
                ShowResult("Taxable Amount can not null or zero", 'failure', 'detailPopUp');
            }
            if ($scope.taxCategoryList[i].Id === x.Id) {
                $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.detailModel.TransactionAmount).toFixed(4) * 100);

            }

        }
    }

    $scope.calculateTaxAmountForService = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.serviceModel.TransactionAmount * data.Percentage) / 100;
    };
    $scope.calculateTaxAmountForServiceOutPut = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
       
        $scope.TransactionAmount = $scope.detailModel.TransactionQty * $scope.detailModel.RatePerUnit;
      //    data.TaxAmount = (Math.round(($scope.TransactionAmount * data.Percentage) *  100 + Number.EPSILON) / 100)/100;

        //(Math.round(($scope.TransactionAmount * data.Percentage) *  100 + Number.EPSILON) / 100)/100;

        var TaxAmt = parseFloat($scope.TransactionAmount * data.Percentage) / 100;
        data.TaxAmount = TaxAmt.toFixed(2);
    };
    $scope.checkRowValidationServiceOutPut = function (x) {

        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            //if (baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) || $scope.detailModel.TransactionAmount === 0) {
            //	ShowResult("Taxable Amount can not null or zero", 'failure', 'detailPopUp');
            //}
            $scope.TransactionAmount = $scope.detailModel.TransactionQty * $scope.detailModel.RatePerUnit;
            if ($scope.taxCategoryList[i].Id === x.Id) {
            //    $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.TransactionAmount).toFixed(4) * 100);
                var Per = (parseFloat(x.TaxAmount / $scope.TransactionAmount) * 100);
                $scope.taxCategoryList[i].Percentage = Per.toFixed(4);

            }

        }
    }
    $scope.checkRowValidationService = function (x) {

        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            //if (baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) || $scope.detailModel.TransactionAmount === 0) {
            //	ShowResult("Taxable Amount can not null or zero", 'failure', 'detailPopUp');
            //}
            if ($scope.taxCategoryList[i].Id === x.Id) {
                $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.TransactionAmount).toFixed(4) * 100);
            }

        }
    }

    $scope.calculateTaxAmountForServiceModify = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.checkRowValidationServiceModify = function (x) {

        for (var i = 0; i < $scope.receiveTaxList.length; i++) {

            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100);
            }

        }
    };
    $scope.closeServiceChargeTaxPopUp = function () { //hossain
        ////debugger;
        $scope.detailModel = {};
        $scope.detailModel.InventoryReceiveDetailId = $scope.ServiceId;
        $scope.detailModel.InventoryReceiveDetailId = $scope.DetailId;
        $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
                ShowResult("Input Tax Amount.", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
        }

        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/InsertserviceTax',
            data: {
                entity: $scope.detailModel
                , taxCategoryList: $scope.receiveTaxList
                , ServiceId: $scope.ServiceId
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'ServiceChargeTaxPopUp');
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
        };
    };
    $scope.closeReceiveTaxPopUpwindow = function () {

        //getInventoryMaterialList($scope.productNew.Id);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    };

    $scope.updateTaxPopUp = function () {
        //$scope.detailSave(true);
        $scope.detailModelList = [];
        $scope.detailModelList.push($scope.detailModel);
        // $scope.detailModel.OSTransformationPOId = $scope.productNew.Id;
        if (!baseService.isUndefinedOrNull($scope.rmchar1.CharacteristicsId)) {
            $scope.detailModel.FirstCharacteristicsId = $scope.rmchar1.CharacteristicsId;
            $scope.detailModel.FirstCharacteristicsValueId = $scope.rmchar1.CharacteristicsValueId;
        }
        if (!baseService.isUndefinedOrNull($scope.rmchar2.CharacteristicsId)) {
            $scope.detailModel.SecondCharacteristicsId = $scope.rmchar2.CharacteristicsId;
            $scope.detailModel.SecondCharacteristicsValueId = $scope.rmchar2.CharacteristicsValueId;
        }
        if (!baseService.isUndefinedOrNull($scope.rmchar3)) {
            $scope.detailModel.ThirdCharacteristicsId = $scope.rmchar3.CharacteristicsId;
            $scope.detailModel.ThirdCharacteristicsValueId = $scope.rmchar3.CharacteristicsValueId;
        }
        try {

            //if ($scope.invalid && $scope.invalid1) {
            $http({
                method: 'POST',
                url: $scope.TaxSaveUrl,
                data: {
                    data: $scope.detailModelList,
                    TaxList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'receiveTaxPopUp');

                    $scope.taxCategoryList = [];
                    //getInventoryMaterialList($scope.productNew.Id);
                    //$scope.getDataList();
                    $scope.getalldata();
                    $scope.getPoChilddata();
                    $scope.clearCharNames();
                    $scope.uom();
                    $scope.detailClear();
                    angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
            };
            //}
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };
    $scope.closeServiceChargeTaxPopUpwindow = function () {
        getServiceChargeList($scope.productNew.Id);
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    };
    function getServiceChargeList(jwpoId) {
        $scope.chargesList = [];
        $http.get($scope.path + 'GetServiceChargeList?jwpoId=' + jwpoId)
            .then(function (response) {
                $scope.chargesList = response.data;
                //$scope.ServiceId = $scope.chargesList[0].Id;
                $scope.GetServiceTaxData();
            });
    }

    $scope.serviceChargePopUp = function () {
        $scope.productNew.TaxOptionService = "Yes";
    //    $scope.productNew.TaxOptionService = 'Yes';

        //if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
        //    return ShowResult('Without material charges not aplicable.');
        $scope.serviceModel = Object.assign({}, $scope.serviceModelTemp);
        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };
    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };
    $scope.GetServiceTaxData = function (masterId) {
        $scope.ChargeTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetServiceTaxList?serviceId=' + $scope.productNew.Id
        }).then(function (response) {
            $scope.ChargeTaxList = response.data;
            for (var i = 0; i < $scope.chargesList.length; i++) {
                var linepk1 = $scope.chargesList[i].Id;
                var list1 = gettaxlist1(linepk1);
                $scope.chargesList[i].ChargeTaxList = list1;
            }
        });
    };
    function gettaxlist1(linepk1) {
        var result1 = [];
        for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
            if ($scope.ChargeTaxList[i].InventoryServiceId === linepk1) {
                result1.push($scope.ChargeTaxList[i]);
            }
        }
        return result1;
    }

    $scope.Del = function (Id, index) {
        $scope.dindex = index;
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            if ($scope.taxCategoryList[i].Id === Id) {
                $scope.taxCategoryList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };

    $scope.serviceSave = function () {
        try {
            $scope.manualValidationAddRemove('div_svc', 'serviceModel', 'ServiceMasterId');
            $scope.manualValidationAddRemove('div_svcRate', 'serviceModel', 'TransactionAmount', 'Amount');
            $scope.serviceModel.JWTransformationPOId = $scope.productNew.Id;
            $http({
                method: 'POST',
                url: $scope.sreviceSaveUrl,
                data: {
                    data: $scope.serviceModel
                    , TaxList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'serviceChargePopUp');

                    $scope.serviceModel = Object.assign({}, $scope.serviceModelTemp);
                    $scope.taxCategoryList = [];
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                    $scope.getalldata();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
            };
        } catch (e) {
        }
    };
    $scope.getArticle = function () {

        $scope.getArticleSearchList($scope.detailModel.MaterialMasterId);
    };
    $scope.clearArticle = function () {
        $scope.detailModel.ArticleId = null;
        $scope.detailModel.ArticleName = null;
    };

    $scope.GetBOQItemList = function () {
        //debugger;
        $scope.GetListForMasterOrder = [];
        $scope.groupList = [];
        $scope.GetListForMasterOrdernew = [];
        $scope.taxCategoryList = [];
        $scope.groupList = [];
        $scope.Action1 = 'Save';
        //$scope.uom();

        $scope.getalldataListForBOQList();
        //$scope.processgroupList1();
        // $scope.GerRequisition();


    };
    $scope.GetListForMasterOrder = [];
    $scope.getalldataListForBOQList = function () {
        //debugger;
        var gridObj = $("#GridReq").data("ejGrid");
        var DropDownActivityListObj = $("#ddlActivityList").data("ejDropDownList");
        var activityList = null;//DropDownActivityListObj.getSelectedValue().split(',');

        var activityListStr = "";//"'" + activityList.join("','") + "'";
        $scope.GetListForMasterOrder = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.path + 'GetBOQItems?ContractId=' + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyCode + '&IsOwnVendor=' + $scope.IsOwnVendor + '&JWPOId=' + $scope.productNew.Id + '&JWPODId=' + $scope.detailModel.Id + '&jwActivityId=' + activityListStr + '&POType=' + $scope.productNew.POType
        }).then(function successCallback(response) {
            $scope.GetListForMasterOrder = [];
            $scope.GetListForMasterOrder = response.data;
            gridObj.clearFiltering();
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        });

        $scope.ActionPOBOQ = 'Save';
        $scope.Action1 = 'Save';
        $scope.processgroupList1();
    };

    $scope.RateDisable = false;
    $scope.getalldataListForBOQListUpdate = function (x) {
        //debugger;
        var gridObj = $("#GridReq").data("ejGrid");
        //var DropDownActivityListObj = $("#ddlActivityList").data("ejDropDownList");
        var activityList = null; //DropDownActivityListObj.getSelectedValue().split(',');

        var activityListStr = "";  //"'" + activityList.join("','") + "'";
        $scope.GetListForMasterOrder = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.path + 'GetBOQItemsForUpdate?ContractId=' + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyCode + '&IsOwnVendor=' + $scope.IsOwnVendor + '&JWPOId=' + $scope.productNew.Id + '&JWPODId=' + x.Id + '&jwActivityId=' + activityListStr + '&MaterialId=' + x.MaterialMasterId + '&ArticleId=' + x.ArticleId
        }).then(function successCallback(response) { //datagatefun
            $scope.GetListForMasterOrder = [];
            $scope.GetListForMasterOrder = response.data;
            $scope.ActionPOBOQ = 'Update';
            gridObj.clearFiltering();
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        });


        $scope.Action1 = 'Save';
        $scope.processgroupList1();
    };






    $scope.groupList = [];
    $scope.processgroupList1 = function () {
        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrder;
            $scope.GetListForMasterOrder = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": $scope.newlistitems[i].ThitrdCharacteristicsValueId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrder.push($scope.newlistitems[i]);
                }
            }
        }
        $scope.Action1 = 'Save';
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    };


    $scope.RequisitionListHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('hide');
    };


    $scope.IsOwnVendor = 'OwnVendor';
    $scope.tab1 = 1;
    $scope.setOwnVendorTabIndex = function (newTab) {

        $scope.IsOwnVendor = 'OwnVendor';
        $scope.GetBOQItemList();
        $scope.tab1 = newTab;

    };
    $scope.isSetOwnVendorIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabOtherVendorIndex = function (newTab) {
        //alert('tabCHR');

        $scope.IsOwnVendor = 'OtherVendor';
        $scope.GetBOQItemList();
        $scope.tab1 = newTab;

    };
    $scope.isSetOtherVendorIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    $scope.setTabParentIndex = function (newTab) {


        $scope.IsOwnVendor = 'Parent';
        $scope.GetBOQItemList();
        $scope.tab1 = newTab;

    };
    $scope.isSetParentIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.calculateAmount = function (data) {

        data.TransactionAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TransactionAmount === 'NaN')
            data.TransactionAmount = 0;
        data.TaxAmount = 0;
        if ($scope.productNew.IsNonCreditable == 1) {
            //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
            if (data.BaseTaxAmount === null) {
                data.BaseTaxAmount = '0.00';
            }
            data.BaseAmount = parseFloat(data.TrnAmount + data.BaseTaxAmount);
        }
        else {
            // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
            data.BaseAmount = data.TransactionAmount;
        }
    };
    $scope.calculateRate = function (data, event) {

        data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        if (data.TransactionRate === 'NaN')
            data.TransactionRate = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;

            data.BaseTaxAmount += item.TaxAmount;
        });
        // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        if ($scope.productNew.IsNonCreditable == 1) {
            //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
            data.BaseAmount = data.TrnAmount + data.BaseTaxAmount;
        }
        else {
            // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
            data.BaseAmount = data.TrnAmount;
        }

    };
    $scope.calculateAmountForServiceCharge = function (data) {
        data.TotalTaxAmount = 0;
        for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
            if ($scope.ChargeTaxList[i].InventoryServiceId === data.Id) {
                $scope.ChargeTaxList[i].TaxAmount = data.Amount * $scope.ChargeTaxList[i].Percentage / 100;
                data.TotalTaxAmount += $scope.ChargeTaxList[i].TaxAmount;
            }
        }
    };
    $scope.onchangeFunction = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');

        }

    };
    $scope.onchangeFunction1 = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.taxCategoryList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
        }
    };

    $scope.materialValidationForBOQItem = function (list) {
        $scope.list = list;
        for (var i = 0; i < $scope.list.length; i++) {
            var getRow3 = $filter("filter")($scope.PoChildList, { "InventoryMaterialId": $scope.list[i].MaterialMasterId, "ArticleId": $scope.list[i].ArticleId, "FirstCharacteristicsValueId": $scope.list[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.list[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.list[i].ThirdCharacteristicsValueId });

            if (getRow3 == 0) {
                $scope.invalid = true;
            }
            else {
                ShowResult('Material Combination Already Exist', 'failure', 'ListOfPOMaterial');
                $scope.invalid = false;
            }
        }
    };

    $scope.ConvertedDataRowList = [];
    $scope.GetListForMasterOrderTemp = [];
    $scope.ConvertedDataRow = function (data) {
        var gridObj = $("#GridReq").data("ejGrid");
        //var x = $event;
        //var res = x.data;
        //debugger;
        $http({
            method: 'POST',
            url: $scope.path + 'ConverttedBOQUOMData',
            data: {
                'data': data
            },
            dataType: 'JSON'
        }).then(function (response) {
            $scope.ConvertedDataRowList = response.data;
            for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                if ($scope.GetListForMasterOrder[i].BOQId === $scope.ConvertedDataRowList.data.BOQId) {
                    $scope.GetListForMasterOrder[i].RequiredQtyPO = $scope.ConvertedDataRowList.data.RequiredQtyPO;
                    $scope.GetListForMasterOrder[i].OtherPOQty = $scope.ConvertedDataRowList.data.OtherPOQty;
                }
            }
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();

        });

    };
    $scope.GetJWPODTListChildMaterials = [];

    $scope.showChild = function ($event) {

        //debugger;
        var x = $event;
        $scope.GetListForMasterOrder = [];
        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.path + 'GetJWPODTChildMaterials',
            data: { 'data': x.data }
        }).then(function successCallback(response) { //datagatefun           
            $scope.GetJWPODTListChildMaterials = response.data;
        });

        angular.element(document.querySelector('#BOQChildModal')).modal('show');

    };
    $scope.GetJWPODTListChildMaterialSummary = [];
    $scope.showChildSummary = function (data) {

        //debugger;

        $scope.GetJWPODTListChildMaterialSummary = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.path + 'GetJWPODTChildMaterialsSummary?JWPODId=' + data.Id
        }).then(function successCallback(response) { //datagatefun           
            $scope.GetJWPODTListChildMaterialSummary = response.data;
        });

        angular.element(document.querySelector('#BOQChildSummaryModal')).modal('show');

    };
    $scope.serviceCboList = [];
    $http.get('Setups/CompanyServiceMaster/GetCboList')
        .then(function (response) {
            $scope.serviceCboList = response.data;
        });

    $scope.BOQserviceCboList = [];
    $scope.BOQServiceGet = function () {
        $http.get('Setups/CompanyServiceMaster/GetCboList')
            .then(function (response) {
                $scope.BOQserviceCboList = response.data;
            });
    }
  //  $scope.BOQServiceGet();

    //$scope.uom = function () {
    //    cboService.getUoMCbo(function (response) {
    //        $scope.uoMList = response;
    //    });
    //}
    //$scope.uom();


    $scope.EmployeeResPersonList = [];
    $scope.ResPersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpData();

    }
    $scope.getEmpData = function () {
        $scope.EmployeeResPersonList = [];
        $http({
            method: 'POST',
            //data: { Id: $scope.ModelNew.Id },
            url: $scope.path + 'LoadAllEmpDetails'
        }).then(function successCallback(response) {
            $scope.EmployeeResPersonList = response.data;
        });
    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.detailModel.EmployeeCode = data.Code;
        $scope.detailModel.ResponsiblePersonId = data.Id;
        $scope.detailModel.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    $scope.ResPersonClear = function () {
        $scope.detailModel.ResponsiblePersonId = null;
        $scope.detailModel.ResponsiblePerson = null;
        $scope.detailModel.EmployeeCode = null;
        $scope.detailModel.EmployeeStatus = null;

    };
    //$scope.JWPOActivityServiveList = [];
    //$scope.GetJWPOActivityService = function (data) {
    //    $scope.JWPOActivityServiveList = [];
    //    $http({
    //        method: "GET",
    //        dataType: 'JSON',
    //        url: $scope.path + 'GetJWPOActivityService?JWPODId=' + data.Id
    //    }).then(function successCallback(response) { //datagatefun           
    //        $scope.JWPOActivityServiveList = response.data;
    //    }); 
    //};

    $scope.JWPOinputList = [];
    $scope.JWPOByProductList = [];

    //$scope.GetJWPOinputByProductList = function (data) {
    //    $scope.JWPOActivityServiveList = [];
    //    $http({
    //        method: "GET",
    //        dataType: 'JSON',
    //        url: $scope.path + 'GetJwPoDetailByProduct?jwpoDetailId=' + data.Id
    //    }).then(function successCallback(response) { //datagatefun           
    //        $scope.JWPOByProductList = response.data;
    //    });
    //    $http({
    //        method: "GET",
    //        dataType: 'JSON',
    //        url: $scope.path + 'GetJwTransPoDetailInputMaterial?jwpoDetailId=' + data.Id
    //    }).then(function successCallback(response) { //datagatefun     

    //        $scope.JWPOinputList = response.data;
    //    });

    //    angular.element(document.querySelector('#ByProductInputMaterialModal')).modal('show');


    //};

    $scope.MatPlanningModelTemp = {
        Id: null,
        JWTransformationPOId: null,
        JobWorkItemMasterId: null,
        MaterialSpecification: null,
        MaterialReference: null,
        OutputMaterialUOMId: null,
        Quantity: null,
        ArticleCodeId: null,
        OrderSpecific: null,
        RequiredCapacity: null,
        ByProductApplicable: null,
        RateApplyId: null,
        CurrencyId: null,
        RatePerUnit: null,
        Rejection: null,
        ValueLoss: null,
        ResponsiblePersonId: null,
        Remarks: null,
        FileName: null,
        MaterialLocationId: null,
        MaterialType: null,
        FinalOutputCategory: null,
        JobActivityId: null,
        MaterialCode: null,
        MaterialName: null,
        MaterialMasterId: null,
        ArticleCode: null,
        ArticleName: null,
        EmployeeCode: null,
        ResponsiblePerson: null,
        EmployeeStatus: null,
        Tolerance: null,

    };
    $scope.MatPlanning = Object.assign({}, $scope.MatPlanningModelTemp);
    //PO Details save
    $scope.SaveMatPlanning = function () {
        $scope.MatPlanning.JWTransformationPOId = $scope.Transformation.Id;
        //      $scope.$broadcast('show-errors-check-validity');
        //     if ($scope.FarmerMasterPlotForm.$valid) {
        if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
            throw $scope.filedata.name + ' File size must be below 2 mb';
        var fileName = null;
        if (!baseService.isUndefinedOrNull($scope.filedata))
            fileName = $scope.filedata.name;
        $scope.MatPlanning.FileName = fileName;
        if (!baseService.isUndefinedOrNull($scope.MatPlanning.FileName)) {
            if ($scope.MatPlanning.FileName.length > 50) {
                throw "File Name must be less than 50 character.";
            }
        }
        var formData = new FormData();
        $scope.path = "JobWork/JWValueAddedContract/";
        $http({
            method: 'POST',
            url: $scope.path + 'saveUrlMatPlanning',
            headers: { 'Content-Type': undefined },
            transformRequest: function (data) {
                formData.append("MatPlanning", angular.toJson(data.MatPlanning));
                if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                    formData.append('file', data.file);
                }
                return formData;
            },
            //data: { 'MatPlanning': $scope.MatPlanning, 'file': $scope.filedata }
            data: { 'MatPlanning': $scope.MatPlanning, 'file': $scope.filedata }
            

            
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.MatPlanning = response.data.Data;
                $scope.getMatPlanningData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

        //    }
    };

    $scope.BPMaterialMstList = [];
    $scope.BPMaterialArticleMstList = [];

    //  JOB WORK ORDER WISE CODE

    // Order Wise Requirement tab

    //  $scope.MonthsList = [];
    $scope.ConfirmOrderWisePopUp = function (data) {
        $scope.MatPlanningTabId = data.Id;
        $scope.UnitId = data.OutputMaterialUOMId;
        //$scope.TransformOrderWiseReq.Quantity = data.Quantity;
        //$scope.PQuantity = data.Quantity;
        /*$scope.TransformOrderWiseReq.PlanQuantity = $scope.PQuantity;*/
        //  $scope.TransformOrderWiseReq.ArtclCode = data.ArticleCode 
        //$scope.TransformOrderWiseReq.Material = data.MaterialName;
        //$scope.TransformOrderWiseReq.ArtclCode = data.ArticleName;
    //    $scope.GetTransformOrderWiseUOM();
        $scope.getTransformOrderWiseData();
        angular.element(document.querySelector("#OrderWisePopUp")).modal("show");

    }


    $scope.closeTransformOrderWiseReqTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    // Order Wise Requirement Sales Order POP UP

    $scope.SOItemList = [];
    $scope.SalesOrderPopUP = function () {
        angular.element(document.querySelector("#SOItemPopup")).modal("show");
        $scope.getSalesorderData();

    }
    $scope.getSalesorderData = function () {
        $scope.SOItemList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.TransformOrderWiseReq.Id },
            url: $scope.path + 'GetSalesOrderData'
        }).then(function successCallback(response) {
            $scope.SOItemList = response.data;
        });
    }
    $scope.selectSOItem = function (obj) {

        var data = obj.data;
        $scope.TransformOrderWiseReq.SalesOrderId = data.SalesOrderId;
        $scope.TransformOrderWiseReq.Customer = data.Customer;
        $scope.TransformOrderWiseReq.MasterOrderNo = data.MasterOrderNo;
        $scope.TransformOrderWiseReq.MasterOrderItemId = data.MasterOrderItemId;

        $scope.TransformOrderWiseReq.MasterOrderUoM = data.MasterOrderUoM;
        $scope.TransformOrderWiseReq.Material = data.MaterialMasterName;
        $scope.TransformOrderWiseReq.Article = data.ArticleName;

        $scope.TransformOrderWiseReq.Quantity = data.Qty;
        $scope.TransformOrderWiseReq.ContractNo = data.ContractNo;
        $scope.TransformOrderWiseReq.MasterLCNo = data.MasterLCNo;
        angular.element(document.querySelector('#SOItemPopup')).modal('hide');
    };

    //$scope.ResPersonClear = function () {
    //    $scope.detailModel.ResponsiblePersonId = null;
    //    $scope.detailModel.ResponsiblePerson = null;
    //    $scope.detailModel.EmployeeCode = null;
    //    $scope.detailModel.EmployeeStatus = null;

    //};

    $scope.TransformOrderWiseRequirementList = [];
    $scope.AllCustomerList = [];
    $scope.AllMasterOrderNoList = [];
    $scope.AllMasterOrderItemList = [];
    $scope.AllUOMList = [];

    $http({
        method: 'GET',
        url:'JobWork/JWValueAddedContract/getcustomerlist/',
    }).then(function successCallback(response) {
        $scope.AllCustomerList = response.data;
    });

    $scope.GetAllMasterOrderNo = function () {
        $http({
            method: 'GET',
            url:'JobWork/JWValueAddedContract/getmasterorderlist?CustomerId=' + $scope.TransformOrderWiseReq.CustomerId,
        }).then(function successCallback(response) {
            $scope.AllMasterOrderNoList = response.data;
        });
    }

    $scope.GetAllMasterOrderItem = function () {
        $http({
            method: 'GET',
            url:'JobWork/JWValueAddedContract/getmasterorderitemlist?MasterOrderNoId=' + $scope.TransformOrderWiseReq.MasterOrderNoId,
        }).then(function successCallback(response) {
            $scope.AllMasterOrderItemList = response.data;
        });
    }

    $scope.GetTransformOrderWiseUOM = function () {
        $http({
            method: 'GET',
            url: 'JobWork/JWValueAddedContract/getoutputunit/',
        }).then(function successCallback(response) {
            $scope.AllUOMList = response.data;
            if (baseService.arrayLength($scope.AllUOMList) > 0) {
                $scope.TransformOrderWiseReq.OutputMaterialUOMId = $scope.UnitId;
            }
        });
    }


    $scope.TransformOrderWiseReqModelTemp = {
        Id: null,
        OSTransformationPODetailId: null,
        OrderType: null,
        CustomerId: null,
        MasterOrderNoId: null,
        MasterOrderItemId: null,
        ParticularSpecification: null,
        Remarks: null,
        OutputMaterialUOMId: null,
        Quantity: null,
        PlanQuantity: null,
        ArtclCode: null,
        SalesOrderId: null,
        Customer: null,
        MasterOrderNo: null,
        MasterOrderUoM: null,
        Material: null,
        Article: null,
        ContractNo: null,
        MasterLCNo: null,

    };
    $scope.TransformOrderWiseReq = Object.assign({}, $scope.TransformOrderWiseReqModelTemp);

    $scope.SaveTransformOrderWiseReqTab = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.TransformOrderWiseReqForm.$valid) {
            $http({
                method: 'POST',
                url: 'JobWork/JWValueAddedContract/SaveTransformOrderWiseReqTab/',
                data: { 'data': $scope.TransformOrderWiseReq, 'ChildMasterId': $scope.MatPlanningTabId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.TransformOrderWiseReq = response.data.Data;
                    ClearFieldsTransformOrderWiseChildData();
                    $scope.getTransformOrderWiseData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DelTransformOrderWise = function () {
        $http({
            method: 'GET',
            url: 'JobWork/JWValueAddedContract/DelTransformOrderWise?Id=' + $scope.TransformOrderWiseChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getTransformOrderWiseData();
                ClearFieldsTransformOrderWiseChildData();
            }

        });
    }

    $scope.ConfirmDeleteTransformOrderWiseTab = function (Id) {
        $scope.TransformOrderWiseChildTabId = Id;
        angular.element(document.querySelector("#DelTransformOrderWiseChildTabPopUp")).modal("show");
    }

    $scope.ClearTransformOrderWiseReqTab = function () {
        ClearFieldsTransformOrderWiseChildData();
    }

    function ClearFieldsTransformOrderWiseChildData() {
        $scope.TransformOrderWiseReq = Object.assign({}, $scope.TransformOrderWiseReqModelTemp);
        //$scope.TransformOrderWiseReq.Id = null;
        //$scope.TransformOrderWiseReq.OSTransformationPODetailId = null;
        //$scope.TransformOrderWiseReq.OrderType = null;
        //$scope.TransformOrderWiseReq.CustomerId = null;
        //$scope.TransformOrderWiseReq.MasterOrderNoId = null;
        //$scope.TransformOrderWiseReq.MasterOrderItemId = null;
        //$scope.TransformOrderWiseReq.ParticularSpecification = null;
        //$scope.TransformOrderWiseReq.Remarks = null;
        //$scope.TransformOrderWiseReq.PlanQuantity = $scope.PQuantity;
        //$scope.GetTransformOrderWiseUOM();
    }

    $scope.getTransformOrderWiseData = function () {

        $http({
            method: 'GET',
            url:'JobWork/JWValueAddedContract/getTransformOrderWiseData?MaterialMasterId=' + $scope.MatPlanningTabId
        }).then(function successCallback(response) {
            $scope.TransformOrderWiseRequirementList = response.data;

        });
    }

    // MATERIAL INPUT DATA CODE

    // MATERIAL INPUT tab

    $scope.ConfirmMaterialInputPopUp = function (data) {
        $scope.MatPlanningTabId = data.Id;
        $scope.JWInputId = data.JobWorkItemMasterId;
        $scope.JWActivityId = data.JobActivityId;
        $scope.getMatInputListData();
        $scope.getMaterialInputData();

        angular.element(document.querySelector("#MaterialInputPopUp")).modal("show");
    }


    $scope.closeMaterialInputTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setInputeMaterialArticleData = function (obj) {
        var data = obj.data;
        $scope.InputMaterialArticlelistData.ArticleId = data.Id;
        $scope.InputMaterialArticlelistData.InputArticleCode = data.Code;
        $scope.InputMaterialArticlelistData.InputArticleName = data.StandardName;
        $scope.InputMaterialArticlelistData.InputMaterial = data.MaterialMasterName;
        $scope.InputMaterialArticlelistData.InputMaterialId = data.MaterialMasterId;
        var gridObj = $("#GridMatInput").data("ejGrid");
        gridObj.refreshTemplate(true);
        angular.element(document.querySelector('#materialarticleNewPopUp')).modal('hide');
    };

    $scope.MaterialInputList = [];
    //$scope.MaterialMasterList = [];
    //$scope.InputUOMList = [];

    $scope.MatInputList = [];
    $scope.getMatInputListData = function () {

        $http({
            method: 'GET',
            url: 'JobWork/JWValueAddedContract/getMatInputListData?JobWorkItemId=' + $scope.JWInputId + '&ActivityId=' + $scope.JWActivityId + '&Id=' + $scope.MatPlanningTabId
        }).then(function successCallback(response) {
            $scope.MatInputList = response.data;
        });
    }

    // Select All Check Box 

    $scope.refreshTemplateMatInput = function () {
        $("#MIheadchk").ejCheckBox({ "change": CheckBoxSelectMatInput });
    };

    function CheckBoxSelectMatInput(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridMatInput").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MatInputList.length; i++) {
                $scope.MatInputList[i].isToBeSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isToBeSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridMatInput").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.GetGrossConsumption = function (data) {

        for (var i = 0; i < $scope.MatInputList.length > 0; i++) {
            if ($scope.MatInputList[i].Id === data.Id) {

                if ($scope.MatInputList[i].NetConsumption !== null && $scope.MatInputList[i].ValueLoss !== null && $scope.MatInputList[i].Rejection !== null) {
                    var NConsumption = parseFloat($scope.MatInputList[i].NetConsumption);
                    var VLoss = parseFloat($scope.MatInputList[i].ValueLoss);
                    var Rejection = parseFloat($scope.MatInputList[i].Rejection);
                    //    var Res = Math.abs((NConsumption) / (100 - VLoss));
                    var Res = Math.abs(NConsumption * (parseFloat(1) + (VLoss / 100) + (Rejection / 100)));
                    //      var Result = Math.abs(Res * 100);
                    var RoundRes = Math.round(Res * 100) / 100;
                    $scope.MatInputList[i].GrossConsumption = RoundRes;
                }
            }
        }
    }

    // Get Input Article

    // #region field

    $scope.InpArticleMstList = [];
    $scope.InputArticlePopUp = function (RowData) {
        angular.element(document.querySelector("#InpArticlePopUp")).modal("show");
        $scope.getInpArticleData(RowData);

    }
    $scope.getInpArticleData = function (RowData) {
        $scope.InpArticleMstList = [];

        for (var i = 0; i < $scope.MatInputList.length > 0; i++) {
            if ($scope.MatInputList[i].Id === RowData.Id) {
                $scope.MatMstId = $scope.MatInputList[i].InputMaterialId;
                $scope.a = i;
            }
        }

        $http({
            method: 'POST',
            data: { MaterialMstId: $scope.MatMstId },
            url: $scope.path + 'LoadInputArticle'
        }).then(function successCallback(response) {
            $scope.InpArticleMstList = response.data;
        });
    }

    $scope.BPMaterialMstArticleClear = function (data) {
        for (var i = 0; i < $scope.MatInputList.length > 0; i++) {
            if ($scope.MatInputList[i].Id === data.Id) {

                $scope.MatInputList[i].ArticleId = null;
                $scope.MatInputList[i].InputArticleCode = null;
                $scope.MatInputList[i].InputArticleName = null;
            }
        }
    };

    $scope.closeInpArticlePopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setInpArticleData = function (obj) {
        var b = $scope.a;
        var data = obj.data;
        $scope.MatInputList[b].ArticleId = data.ArticleId;
        $scope.MatInputList[b].InputArticleCode = data.ArticleCode;
        $scope.MatInputList[b].InputArticleName = data.StandardName;
        //$scope.SelectedArticleId = data.ArticleId;
        //$scope.GetByDefaultRate($scope.a);
        //$scope.GetLotNumberList($scope.a);
        angular.element(document.querySelector('#InpArticlePopUp')).modal('hide');
    };


    $scope.MaterialInputModelTemp = {
        Id: null,
        OSTransformationPODetailId: null,
        MaterialMasterId: null,
        MaterialSpecification: null,
        InputMaterialUOMId: null,
        NetConsumptionOutputUnit: null,
        Rejection: null,
        ValueLoss: null,
        GrossConsumption: null,
        ResponsiblePersonId: null,
        Remarks: null,

    };
    $scope.MaterialInput = Object.assign({}, $scope.MaterialInputModelTemp);

    //Save Function 
    $scope.SaveMaterialInputTab = function () {
   //     $scope.$broadcast('show-errors-check-validity');
        var MatInputSelData = [];
        try {
        for (var i = 0; i < $scope.MatInputList.length; i++) {
            if ($scope.MatInputList[i].isToBeSelect == true)
                if ($scope.MatInputList[i].InputMaterialId !== null) {
                    if ($scope.MatInputList[i].ArticleId == null) {
                        throw 'Please Select Article';
                    }
                    else {
                        MatInputSelData.push($scope.MatInputList[i]);
                    }
                }
                else {
                    MatInputSelData.push($scope.MatInputList[i]);
                }     
        }
       
            if (MatInputSelData.length == 0) {
                throw 'Please Select at least one Material Input';
            }
            $http({
                method: 'POST',
                data: { SelectedMatInputData: MatInputSelData, ChildMasterId: $scope.MatPlanningTabId },
                url: 'JobWork/JWValueAddedContract/SaveMaterialInputTab/'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getMatInputListData();
                    $scope.getMaterialInputData();
                }
            });

        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.DelMaterialInput = function () {
        $http({
            method: 'GET',
            url: 'JobWork/JWValueAddedContract/DelMaterialInput?Id=' + $scope.MaterialInputChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getMaterialInputData();
                $scope.getMatInputListData();
                //ClearFieldsMaterialInputChildData();
            }

        });
    }

    $scope.ConfirmDeleteMaterialInputTab = function (Id) {
        $scope.MaterialInputChildTabId = Id;
        angular.element(document.querySelector("#DelMaterialInputChildTabPopUp")).modal("show");
    }

    $scope.getMaterialInputData = function () {

        $http({
            method: 'GET',
            url: 'JobWork/JWValueAddedContract/getMaterialInputData?MaterialMasterId=' + $scope.MatPlanningTabId
        }).then(function successCallback(response) {
            $scope.MaterialInputList = response.data;

        });
    }

    // BY PRODUCT TAB

    // Select All Check Box 

    $scope.refreshTemplateemployee = function () {
        $("#BPheadchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridByProductMaster").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ByProductMasterList.length; i++) {
                $scope.ByProductMasterList[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridByProductMaster").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.ConfirmByProductPopUp = function (data) {
        $scope.MatInputTabId = data.Id;
        $scope.getByProductMasterData();
        $scope.getByProductData();
        angular.element(document.querySelector("#ByProductPopUp")).modal("show");
    }

    $scope.closeByProductTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.ByProductList = [];
    $scope.ByProductMasterList = [];
    $scope.getByProductMasterData = function () {

        $http({
            method: 'GET',
            url: 'JobWork/JWValueAddedContract/getByProductMasterData?JobWorkItemId=' + $scope.JWInputId + '&ActivityId=' + $scope.JWActivityId + '&Id=' + $scope.MatInputTabId
        }).then(function successCallback(response) {
            $scope.ByProductMasterList = response.data;
        });
    }

    // #region field By product

    $scope.ByProductMaterialMstList = [];
    $scope.ByProductMaterialMstPopUp = function (data) {
        angular.element(document.querySelector("#ByProductMaterialPopUp")).modal("show");
        $scope.getMaterialDetailsData(data);
    }

    $scope.getMaterialDetailsData = function (data) {
        $scope.ByProductMaterialMstList = [];

        for (var i = 0; i < $scope.ByProductMasterList.length > 0; i++) {
            if ($scope.ByProductMasterList[i].Id === data.Id) {
                $scope.MatMstId = $scope.ByProductMasterList[i].BPMaterialId;
                $scope.a = i;
            }
        }

        $http({
            method: 'POST',
            url: 'JobWork/JWValueAddedContract/LoadMaterialMstDetails/'
        }).then(function successCallback(response) {
            $scope.ByProductMaterialMstList = response.data;
        });
    }

    $scope.BPMaterialMstClear = function (data) {
        for (var i = 0; i < $scope.ByProductMasterList.length > 0; i++) {
            if ($scope.ByProductMasterList[i].Id === data.Id) {
                $scope.ByProductMasterList[i].BPMaterialId = null;
                $scope.ByProductMasterList[i].BPMaterialCode = null;
                $scope.ByProductMasterList[i].ByProductMaterial = null;
            }
        }
    };

    $scope.closeByProductMaterialMstPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setByProductMaterialMstData = function (obj) {
        var b = $scope.a;
        var data = obj.data;
        $scope.ByProductMasterList[b].BPMaterialId = data.Id;
        $scope.ByProductMasterList[b].BPMaterialCode = data.Code;
        $scope.ByProductMasterList[b].ByProductMaterial = data.MaterialName;

        $scope.ByProductMasterList[b].BPArticleId = null;
        $scope.ByProductMasterList[b].BPArticleCode = null;
        $scope.ByProductMasterList[b].BPArticleName = null;

        angular.element(document.querySelector('#ByProductMaterialPopUp')).modal('hide');
    };
    // # end region


    // GET ARTICLE
    // MATERIAL MASTER ARTICLE
    // #region field

    $scope.ByProductMaterialArticleMstList = [];
    $scope.BPMaterialMstArticlePopUp = function (RowData) {
        angular.element(document.querySelector("#ByProductMaterialArticlePopUp")).modal("show");
        $scope.getArticleData(RowData);

    }
    $scope.getArticleData = function (RowData) {
        $scope.ByProductMaterialArticleMstList = [];

        for (var i = 0; i < $scope.ByProductMasterList.length > 0; i++) {
            if ($scope.ByProductMasterList[i].Id === RowData.Id) {
                $scope.MatMstId = $scope.ByProductMasterList[i].BPMaterialId;
                $scope.a = i;
            }
        }

        $http({
            method: 'POST',
            data: { MaterialMstId: $scope.MatMstId },
            url: 'JobWork/JWValueAddedContract/LoadMaterialMstArticle/'
        }).then(function successCallback(response) {
            $scope.ByProductMaterialArticleMstList = response.data;
        });
    }

    $scope.BPMaterialMstArticleClear = function (data) {
        for (var i = 0; i < $scope.ByProductMasterList.length > 0; i++) {
            if ($scope.ByProductMasterList[i].Id === data.Id) {

                $scope.ByProductMasterList[i].BPArticleId = null;
                $scope.ByProductMasterList[i].BPArticleCode = null;
                $scope.ByProductMasterList[i].BPArticleName = null;
            }
        }
    };

    $scope.closeByProductMaterialArticlePopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setByProductMaterialArticleData = function (obj) {
        var b = $scope.a;
        var data = obj.data;
        $scope.ByProductMasterList[b].BPArticleId = data.ArticleId;
        $scope.ByProductMasterList[b].BPArticleCode = data.ArticleCode;
        $scope.ByProductMasterList[b].BPArticleName = data.StandardName;
        //$scope.SelectedArticleId = data.ArticleId;
        //$scope.GetByDefaultRate($scope.a);
        //$scope.GetLotNumberList($scope.a);
        angular.element(document.querySelector('#ByProductMaterialArticlePopUp')).modal('hide');
    };

    $scope.ByProductModelTemp = {
        Id: null,
        JobWorkTransformationContractChild3MasterId: null,
        MaterialMasterId: null,
        MaterialSpecification: null,
        StandardQuantityInputUnit: null,
        CurrencyId: null,
        StandardRatePerUnit: null,
        ResponsiblePersonId: null,
        Remarks: null,
        Tolerance: null,

    };
    $scope.ByProduct = Object.assign({}, $scope.ByProductModelTemp);

    // Save Function for By Product(Transformation)

    //Save Function 
    $scope.SaveByProductTab = function () {
        $scope.$broadcast('show-errors-check-validity');
        var checkedData = [];
        try {
            for (var i = 0; i < $scope.ByProductMasterList.length; i++) {
                if ($scope.ByProductMasterList[i].isSelected == true) {
                    if ($scope.ByProductMasterList[i].StandardRate > 0) {

                        if ($scope.ByProductMasterList[i].BPMaterialId !== null) {
                            if (!baseService.isUndefinedOrNull($scope.ByProductMasterList[i].BPArticleId)) {
                                checkedData.push($scope.ByProductMasterList[i]);
                            }
                            else {
                                throw 'Please Select Article';
                            }
                        }
                        else {
                            checkedData.push($scope.ByProductMasterList[i]);
                        }

                   //     checkedData.push($scope.ByProductMasterList[i]);
                    }
                    else {
                        throw 'Standard Rate should be greater than zero';
                    }
                   
                }
            }

            if (checkedData.length == 0) {
                throw 'Please Select at least one By Product';
            }
            $http({
                method: 'POST',
                data: { ByProductMstData: checkedData, ChildMasterId: $scope.MatInputTabId },
                url: 'JobWork/JWValueAddedContract/SaveByProductTab/'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    //         $scope.IssueChild = response.data.Data;
                    $scope.getByProductMasterData();
                    $scope.getByProductData();
                }
            });

        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.DelByProduct = function () {
        $http({
            method: 'GET',
            url: 'JobWork/JWValueAddedContract/DelByProduct?Id=' + $scope.ByProductTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");

                $scope.getByProductData();
                $scope.getByProductMasterData();
                //    ClearFieldsByProductChildData();
            }

        });
    }

    $scope.ConfirmDeleteByProductTab = function (Id) {
        $scope.ByProductTabId = Id;
        angular.element(document.querySelector("#DelByProductTabPopUp")).modal("show");
    }

    $scope.getByProductData = function () {
        $http({
            method: 'GET',
            url: 'JobWork/JWValueAddedContract/getByProductData?MaterialInputId=' + $scope.MatInputTabId
        }).then(function successCallback(response) {
            $scope.ByProductList = response.data;

        });
    }

    //#region start Reports
    $scope.ConfirmPrintTab = function (z) {
        try {
            var x = "#" + z;
            var gridObj = $(x).data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];
    //        location.href = "Products/InventoryIssue/JobWorkIssueReport?grnId=" + data.Id;

            $scope.PrintTabId = data.Id;

            var reportFormat = "Excel";
            if (data.POType == "JWTransformationPO") {
                window.open('JobWork/JWValueAddedContract/GetTransformationContractReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId, '_blank');
            }

            if (data.POType == "JWValueAddedPO") {
                window.open('JobWork/JWValueAddedContract/GetValueAddedPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId, '_blank');
            }
           

    //        var TabType = data.TabType;
            //if (TabType == "Value Added") {
            //    //     var data = args.data;
            //    var reportFormat = "Excel";
            //    window.open('JobWork/JWValueAddedContract/GetValueAddedPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId, '_blank');
            //    $scope.getData();
            //}
            //if (TabType == "Transformation") {
            //    //     var data = args.data;
            //    var reportFormat = "Excel";
            //    window.open('JobWork/JWValueAddedContract/GetTransformationContractReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId, '_blank');
            //    $scope.getData();
            //}

        } catch (e) {

        }
    };

    //$scope.showTcontract = function () {
    //    if ($scope.productNew.Transformation == "Yes") {
    //        $scope.productNew.ValueAdded = null;
    //    }
    //    else {
    //        $scope.productNew.Transformation = null;
    //    }
    //}

    //$scope.showValcontract = function () {
    //    if ($scope.productNew.ValueAdded == "Yes") {
    //        $scope.productNew.Transformation = null;
    //    }
    //    else {
    //        $scope.productNew.ValueAdded = null;
    //    }
    //}


    //end

    $scope.calculateSvcTaxCategory = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    };

    $scope.ActionPOBOQ = 'Save';

    $scope.ValidatePODate = function () {
        try {

            if (new Date($scope.productNew.PODate) < new Date($scope.productNew.DocDate)) {
                //$scope.productNew.PODate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
                $scope.productNew.PODate = null;
                throw 'PO Date cannot be less than Doc Date.';
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.ValidateProcessStartDate = function () {
        try {

            if (new Date($scope.productNew.ProcessStartDate) < new Date($scope.productNew.PODate)) {
                //$scope.productNew.ProcessStartDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
                $scope.productNew.ProcessStartDate = null;
                throw 'Process Start Date cannot be less than PO Date.';
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.ValidateProcessEndDate = function () {
        try {

            if (new Date($scope.productNew.ProcessEndDate) < new Date($scope.productNew.ProcessStartDate)) {
                //$scope.productNew.ProcessEndDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
                $scope.productNew.ProcessEndDate = null;
                throw 'Process End Date cannot be less than Process Start Date.';
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.ValidateContractClosingDate = function () {
        try {

            if (new Date($scope.productNew.ContractClosingDate) < new Date($scope.productNew.ProcessEndDate)) {
                //$scope.productNew.ContractClosingDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
                $scope.productNew.ContractClosingDate = null;
                throw 'Contract Closing Date cannot be less than Process End Date.';
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.AllTabPrint = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "JobWork/JobWorkTransformationPO/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
        $scope.getalldata();
    };

    $scope.GetAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) && !baseService.isUndefinedOrNull($scope.detailModel.RatePerUnit)) {
            var Amt = parseFloat($scope.detailModel.TransactionQty) * parseFloat($scope.detailModel.RatePerUnit)
            var TAmt = Amt.toFixed(2);
            $scope.detailModel.TransactionAmount = TAmt;
            if ($scope.productNew.TaxOptionServiceTPO == "Yes") {
                if ($scope.taxCategoryList.length > 0) {
                    for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].Percentage)) {
                            $scope.taxCategoryList[i].Percentage = 0;           
                        }
                        $scope.TransactionAmount = $scope.detailModel.TransactionQty * $scope.detailModel.RatePerUnit;
                        var TaxAmt = parseFloat($scope.TransactionAmount * $scope.taxCategoryList[i].Percentage) / 100;
                        $scope.taxCategoryList[i].TaxAmount = TaxAmt.toFixed(2);
                    }
                }
            }
            if ($scope.productNew.TaxOptionServiceTPO == "No") {
                if ($scope.taxCategoryList.length > 0) {
                    for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                        //if (baseService.isUndefinedOrNull($scope.taxCategoryList[0].Percentage)) {
                        //    $scope.taxCategoryList[0].Percentage = 0;
                        //}
                        $scope.TransactionAmount = $scope.detailModel.TransactionQty * $scope.detailModel.RatePerUnit;
                        var Per = (parseFloat($scope.taxCategoryList[i].TaxAmount / $scope.TransactionAmount) * 100);
                        $scope.taxCategoryList[i].Percentage = Per.toFixed(4);
                    }
                }
            }

        }
    }

    // DOCUMENT ATTACH

    //#region Document Upload
    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.UserFilename;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExpensesDocument + '/' + data.Id + extention;
    };

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });
    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };

    $scope.DocumentSave = function () {
        debugger;
        //$scope.$broadcast("show-errors-check-validity");

        if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
            throw $scope.filedata.name + ' File size must be below 2 mb';
        var fileName = null;
        if (!baseService.isUndefinedOrNull($scope.filedata))
            fileName = $scope.filedata.name;
        $scope.productDocMap.UserFilename = fileName;
        $scope.productDocMap.POId = $scope.productNew.Id;
        if (baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
            ShowResult('Select Attachment file');
            return false;
        }
        if (!baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
            if ($scope.productDocMap.UserFilename.length > 50) {
                throw "File Name must be less than 50 character.";
            }
        }
        for (var i = 0; i < $scope.Imagedata.length; i++) {
            var getRow = $filter("filter")($scope.Imagedata, { "UserFilename": $scope.productDocMap.UserFilename });
            if (getRow.length === 1) {
                ShowResult('File Already added');
                return false;
            }
        }

        try {

            var formData = new FormData();

            $http({
                method: "POST",
                url: 'JobWork/JobWorkTransformationPO/PODocCreate',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("PODocumentMap", angular.toJson($scope.productDocMap));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: {
                    "PODocumentMap": $scope.productDocMap,
                    "file": $scope.filedata,
                    "POId": $scope.productNew.Id,
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.ImagedataLoad();
                    $scope.productDocMap.UserFilename = "";
                    $scope.productDocMap.Description = "";
                    $scope.productDocMap.Remarks = "";
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            throw ShowResult(e, "failure");
        }

        return true;
    };
    $scope.Imagedata = [];
    $scope.ImagedataLoad = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'JobWork/JobWorkTransformationPO/PODocumentMapData?POID=' + $scope.productNew.Id,
        }).then(function successCallback(response) { //datagatefun
            $scope.Imagedata = response.data;

        });
    };
    $scope.removePopUpForDoc = function (Id) {
        $scope.DocId = Id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUpForDoc')).modal('show');
    };
    $scope.DeletePOIgame = function (Id) {

        if (!baseService.isUndefinedOrNull($scope.DocId)) {
            $http({
                method: 'POST',
                url: 'JobWork/JobWorkTransformationPO/POImageDelete?Id=' + $scope.DocId,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ImagedataLoad();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }


    };

    $scope.PODocumentMapDataAll = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'JobWork/JobWorkTransformationPO/PODocumentMapDataAll'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.Img = response.data;

        });
    }
    $scope.PODocumentMapDataAll();

	//#endregion

    // BOQ Material Input data

    $scope.MaterialInputDisplayModelTemp = {
        Material: null,
        Article: null,
        UoM: null,
        Quantity: null,

    };
    $scope.MaterialInputDisplay = Object.assign({}, $scope.MaterialInputDisplayModelTemp);

    $scope.ConfirmMaterialInputPopUpBOQ = function (data) {
        $scope.OutputMatId = data.Id;
        $scope.MaterialInputDisplay.Material = data.MaterialMasterName;
        $scope.MaterialInputDisplay.Article = data.ArticleName;
        $scope.MaterialInputDisplay.UoM = data.TransactionUoM;
        $scope.MaterialInputDisplay.Quantity = data.Quantity;
        //$scope.JWInputId = data.JobWorkItemMasterId;
        //$scope.JWActivityId = data.JobActivityId;
        $scope.getMatInputListBOQData();
   //     $scope.getMaterialInputData();

        angular.element(document.querySelector("#BOQMaterialInputPopUp")).modal("show");
    }


    $scope.closeMaterialInputTabPopUpBOQ = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

 //   $scope.MaterialInputList = [];
    //$scope.MaterialMasterList = [];
    //$scope.InputUOMList = [];

    $scope.MaterialInputListBOQ = [];
    $scope.getMatInputListBOQData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getMatInputListBOQData?Id=' + $scope.OutputMatId
        }).then(function successCallback(response) {
            $scope.MaterialInputListBOQ = response.data;
        });
    }

    $scope.DelMaterialInputBOQ = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DelMaterialInputBOQ?Id=' + $scope.MatInputChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getMatInputListBOQData();
                //ClearFieldsMaterialInputChildData();
            }

        });
    }

    $scope.ConfirmDeleteMaterialInputTabBOQ = function (Id) {
        $scope.MatInputChildTabId = Id;
        angular.element(document.querySelector("#DelMaterialInputChildBOQ")).modal("show");
    }

   
}