'use strict';
ProformaInvoiceController.$inject = ['commonMessage', '$controller', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function ProformaInvoiceController(commonMessage, $controller, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Proforma Invoice";
    $scope.Action = 'Save';
    $scope.fabricRollMasters = [];
    $scope.selectedGRNList = [];
    $scope.path = 'Commercial/ProformaInvoice/';
    $scope.saveGridUrl = $scope.path + 'SaveData';
    $scope.CostingPath = 'Costings/costingItem/';
    $scope.Deletepath = $scope.path + 'DeletePI';
    $scope.saveUrl = $scope.path + 'create';
    $scope.newVersionUrl = $scope.path + 'NewVersion';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $controller("MasterOrderTaskTemplateController", { cboService: cboService, $scope: $scope, $http: $http });
    $controller("TaskScheduleController", { cboService: cboService, $scope: $scope, $http: $http });

    $controller("CurrencyExchangeController", { cboService: cboService, $scope: $scope, $http: $http, TableName: 'MasterOrderExchangeRates' });
    $scope.PIVersionModel = {
        Id: null,
        PIMasterId: null,
        VersionNo: '',
        VersionRefNo: null,
        VersionDate: null
    };
    $scope.SelectedPIVersion = null;
    $scope.VersionList = [];
    $scope.VersionList.push(Object.assign({}, $scope.PIVersionModel));


    $scope.PIHeaderModel = {
        Id: null
        , PINo: null
        , PIDate: null
        , RefNo: null
        , RevisionNo: null
        , BuyerId: null
        , CustomerId: null
        , Customer: null
        , Currency: null
        , Description: null
        , Quantity: 0
        , UoM: null
        , DeliveryDate: null
        , Amount: 0
        , CurrencyId: null
        , InvoicingPartyPlantId: null
        , DeliveryPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , PartyCode: null
        , CustomerName: null
        , PartyId: null
        , PartyAccountGroupId: null
        , IsPaymentTermChangeable: null
        , PaymentTermId: null
        , TermsAndConditionsId: null
    };
    $scope.PImodelNew = Object.assign({}, $scope.PIHeaderModel);

    $scope.buyerList = [];
    cboService.getCboBuyer(function (data) {
        $scope.buyerList = data;
    });

    $scope.searchPIByList = [
        {
            name: 'Id',
            value: 'Id'
        },
        {
            name: 'PI No.',
            value: 'PINo'
        },
        {
            name: 'Ref No.',
            value: 'RefNo'
        },
        {
            name: 'PI Date',
            value: 'PIDate'
        },
        {
            name: 'Currency',
            value: 'CurrencyId'
        },
        {
            name: 'Buyer',
            value: 'BuyerId'
        },
        {
            name: 'Customer',
            value: 'CustomerId'
        },
        {
            name: 'Invoicing by Address',
            value: 'InvoicingByAddress'
        },
        {
            name: 'Delivery by Address',
            value: 'DeliveryByAddress'
        }
    ];
    $scope.PIGridModelBase = {
        Id: null
        , PIMasterId: null
        , PIVersionId: null
        , MaterialGroupMasterId: null
        , HSNCodeId: null
        , Description: null
        , Quantity: 0
        , Rate: 0
        , UoMId: null
        , UoM: null
        , DeliveryDate: null
        , CurrencyId: null
        , Amount: 0

    };
    $scope.PIGridModel = Object.assign({}, $scope.PIGridModelBase);

    $scope.DataList = [];
    $scope.DataList.push(Object.assign({}, $scope.PIGridModel));
    $scope.SumAmount = function (item)
    {

       // item.Amount = parseFloat(item.Quantity) * parseFloat(item.Rate);
        item.Amount = parseFloat(parseFloat(item.Quantity) * parseFloat(item.Rate)).toFixed(2);
        
    }

    $scope.SubmitH = function (data) {
        try {
            var newObj = Object.assign({}, $scope.PIGridModel);
            if (data != null) {
                newObj = {
                    Id: null
                    , PIMasterId: null
                    , PIVersionId: null
                    , MaterialGroupMasterId: null
                    , HSNCodeId: null

                    , Description: null
                    , Quantity: 0
                    , Rate: 0
                    , UoMId: null
                    , UoM: null
                    , DeliveryDate: null
                    , CurrencyId: null
                    , Amount: 0
                }
            }
            $scope.DataList.push(newObj);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
    }
    $scope.ClearGrid = function () {
        $scope.DataList = [];
        $scope.DataList.push(Object.assign({}, $scope.PIGridModelBase));
    }

    $scope.PISearchBy = "Id";
    $scope.PISearch = "";
    $scope.PIGridList = [];
    $scope.LoadPISearchList = function () {
        $scope.PIGridList = [];
        try {
            $http({
                method: 'POST',
                url: $scope.path + "PIList",
                data: { 'column': $scope.PISearchBy, 'value': $scope.PISearch },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.PIGridList = [];
                $scope.PIGridList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.LoadPISearchList();
    $scope.Get = function (args) {
        //$scope.getHeader(args.data.Id, args.data.PIVersionId);
        $scope.SelectedPIVersion = args.data.PIVersionId;
        $http({
            method: 'GET',
            url: $scope.path + "GetAllData?PIMasterId=" + args.data.Id + '&VersionId=' + args.data.PIVersionId,
        }).then(function successCallback(response) {

            $scope.PImodelNew = response.data.PIMaster[0];
            $scope.PIVersionModel = response.data.VarsionData;
            $scope.DataList = response.data.ItemData;
            if ($scope.DataList == null || $scope.DataList.length == 0)
                $scope.ClearGrid();
            $scope.VersionList = $scope.PIVersionModel;
            getPartyPlantList();
          //  $scope.PIVersionModel["Id"] = $scope.PIVersionModel[0]["Id"];

        });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.GetAllVersionData = function () {
        //$scope.getHeader(args.data.Id, args.data.PIVersionId);
        $http({
            method: 'GET',
            url: $scope.path + "GetAllVersionData?PIMasterId=" + $scope.PImodelNew.Id,
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data)) {
                $scope.SelectedPIVersion = null;
                $scope.DataList = [];
                $scope.DataList.push(Object.assign({}, $scope.PIGridModel));

                $scope.VersionList = $scope.PIVersionModel;
            }

        });

    };

    $scope.selectedDataIndex = -1;
    $scope.OnUOMChange = function (data) {
        $scope.selectedDataIndex = data.model.ModelFieldsId;
        $scope.getUoM();
        $scope.OnHSNChange();
    }
    $scope.getUoM = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetUoMList?MaterialGroupMasterId=" + $scope.DataList[$scope.selectedDataIndex].MaterialGroupMasterId
        }).then(function successCallback(response) {
            $scope.DataList[$scope.selectedDataIndex].MaterialGroupUOMList = response.data.UOMList;

        });
    }

    $scope.OnHSNChange = function (data) {
        $scope.selectedHSNDataIndex = data.model.ModelFieldsId;
        $scope.getHSN();
    }
    $scope.selectedHSNDataIndex = -1;

    $scope.getHSN = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetHSNList?MaterialGroupMasterId=" + $scope.DataList[$scope.selectedHSNDataIndex].MaterialGroupMasterId
        }).then(function successCallback(response) {
            $scope.DataList[$scope.selectedHSNDataIndex].MaterialGroupUOMList = response.data.HSNList;

        });
    }
    // $scope.OnUOMChange();



    $scope.Clear = function () {
        $scope.PImodelNew = Object.assign({}, $scope.PIHeaderModel);
        $scope.DataList = [];
        $scope.DataList.push(Object.assign({}, $scope.PIGridModel));
        $scope.VersionList = [];
        // $scope.VersionList.push(Object.assign({}, $scope.PIVersionModel));

    };
    $scope.Clear();
    $rootScope.title = 'Proforma Invoice';
    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.ShowCustomerPopUpNew = function () {
        $scope.partyType = "Customer";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + $window.companyId + '&PlantId=' + $window.plantId;

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
    $scope.closeCustomerPopUpNew = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.hidePartyPopUp();
        $scope.partyType = "Customer";
        $scope.searchParty = '';
    }
    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.PImodelNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.PImodelNew.PaymentTermId; })[0];
            $scope.PImodelNew.PaymentTermDays = paymentTerm.NoOfDay;
        }
    };

    $scope.paymentTermList = [];
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getcustomercbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

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
                $scope.PImodelNew.InvoicingState = state;
                $scope.PImodelNew.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.PImodelNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.PImodelNew.DeliveryState = state;
                $scope.PImodelNew.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.PImodelNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.PImodelNew.InvoicingState = null;
                $scope.PImodelNew.InvoicingGSTIN = null;
                return $scope.PImodelNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.PImodelNew.DeliveryState = null;
                $scope.PImodelNew.DeliveryGSTIN = null;
                return $scope.PImodelNew.DeliveryByAddress = null;
            }
        }
    };

    function getPartyPlantList() {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.PImodelNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.IsDefault) {
                    $scope.PImodelNew.InvoicingPartyPlantId = item.Value;
                    $scope.PImodelNew.DeliveryPartyPlantId = item.Value;
                    $scope.PImodelNew.InvoicingByAddress = item.Address1;
                    $scope.PImodelNew.DeliveryByAddress = item.Address1;
                    $scope.PImodelNew.InvoicingState = item.StateName;
                    $scope.PImodelNew.InvoicingGSTIN = item.GSTIN;
                    $scope.PImodelNew.DeliveryState = item.StateName;
                    $scope.PImodelNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }
    $scope.departmentList = [];
    $scope.buyerChange = function () {
        $http.get("Parties/BuyerBrand/GetCbo?buyerId=" + $scope.PImodelNew.BuyerId)
            .then(function (response) {
                $scope.brandList = response.data;
            });
    };

    cboService.getCboWithBuyer(null, function (result) {
        $scope.testingStandardList = result;
    });

    $scope.SetCustomerData = function (obj) {

        var party = obj.data;
        $scope.PImodelNew.PartyCode = party.Code;
        $scope.PImodelNew.CustomerName = party.UserName;
        $scope.PImodelNew.PartyId = party.Id;
        $scope.PImodelNew.CurrencyId = party.CurrencyId;
        $scope.PImodelNew.PartyAccountGroupId = party.PartyAccountGroupId;
        $scope.PImodelNew.IsPaymentTermChangeable = '';
        $scope.PImodelNew.PaymentTermId = '';
        $scope.PImodelNew.PaymentTermId = party.PaymentTermId;
        $scope.PImodelNew.IsPaymentTermChangeable = party.IsPaymentTermChangeable;

        $scope.PImodelNew.Customer = party.UserName;
        $scope.PImodelNew.CustomerId = party.Id;

        $scope.changePaymentTerm($scope.PImodelNew.PaymentTermId);
        $scope.personList = [];
        getPartyPlantList();
        // GetDepartmentPersonCbo();
        $scope.hidePartyPopUp();
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.searchParty = '';
    }


    $scope.MaterialGroupList = [];
    $scope.GetMaterialGroupList = function () {
        $http({
            method: "GET",
            url: $scope.path + "GetMaterialGroupList",
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {

            }
            else {
                $scope.MaterialGroupList = response.data;
            }
        }, function errorCallback(response) {

        });
    }
    $scope.GetMaterialGroupList();
    $scope.HSNCodeList = [];
    $scope.GetHSNCode = function () {
        $http({
            method: "GET",
            url: $scope.path + "GetHSNCode",
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {

            }
            else {
                $scope.HSNCodeList = response.data;
            }
        }, function errorCallback(response) {

        });
    }
    $scope.GetHSNCode();
    

    $scope.SelectVersion = function () {

        $http({
            method: 'GET',
            url: $scope.path + "GetAllData?PIMasterId=" + $scope.PImodelNew.Id + '&VersionId=' + $scope.SelectedPIVersion
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data)) {
                $scope.PImodelNew = response.data.PIMaster[0];
                $scope.PIVersionModel = response.data.VarsionData;
                $scope.DataList = response.data.ItemData;
                $scope.PIVersionModel.Id = $scope.PIVersionModel[0].Id;
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
                $scope.VersionList = $scope.PIVersionModel;
            }

        });

    }
/*    $scope.isBtnActive = false;*/
    $scope.Save = function () {
        try {
            $scope.isBtnActive = true;
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'HeaderData': $scope.PImodelNew, 'MaterialData': $scope.DataList, 'PIMasterId': $scope.PImodelNew.Id, 'PIVersionId': $scope.SelectedPIVersion },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.PImodelNew.Id = response.data.Data.Id;
                    $scope.LoadPISearchList();
                    $scope.LoadTermsAndConditionGrid($scope.PImodelNew.TermsAndConditionsId, $scope.PImodelNew.Id);
                    $scope.LoadTermsAndConditionDetailGrid();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure')
        }

    };
    $scope.TermsAndConditionDetailGridList = [];
    $scope.LoadTermsAndConditionDetailGrid = function () {
        $scope.TermsAndConditionDetailGridList = [];

        $scope.termandconditiondetailURL = $scope.path + "GetTermsAndConditionsPIDetailList";

        try {
            $http({
                method: 'POST',
                url: $scope.termandconditiondetailURL,
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.TermsAndConditionDetailGridList = [];
                $scope.TermsAndConditionDetailGridList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');


        }
    }
    $scope.LoadTermsAndConditionDetailGrid();

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.PImodelNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });
    //$scope.NewVersion = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "GetNewVersion?PIMasterId=" + $scope.PImodelNew + "&PIVersionId=" + $scope.PIVersionModel.Id + "&PIMaterialId=" + $scope.PIGridModel.Id,
    //    }).then(function successCallback(response) {

    //    });
    //}


    $scope.NewVersion = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.newVersionUrl,
                data: { 'PIMasterId': $scope.PImodelNew.Id, 'PIVersionId': $scope.SelectedPIVersion },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadPISearchList();
                    $scope.SelectedPIVersion = response.data.VersionId;
                    $http({
                        method: 'GET',
                        url: $scope.path + "GetAllData?PIMasterId=" + $scope.PImodelNew.Id + '&VersionId=' + $scope.SelectedPIVersion,
                    }).then(function successCallback(response) {
                        $scope.SelectVersion();
                        $scope.PImodelNew = response.data.PIMaster[0];
                        $scope.PIVersionModel = response.data.VarsionData;
                        $scope.DataList = response.data.ItemData;
                        $scope.VersionList = $scope.PIVersionModel;
                        

                    })
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure')
        }

    };
    $scope.message_detailconfirmation = null;
    $scope.DeletePI = function () {

        if (!baseService.isUndefinedOrNull($scope.PImodelNew.Id) && !baseService.isUndefinedOrNull($scope.SelectedPIVersion)) {
            $scope.message_detailconfirmation = 'Are you sure? You want to delete this material permanently';
            angular.element(document.querySelector('#confirmPIDeletePopUp')).modal('show');
        }
        else {
            ShowResult('Please select version.');
        }
    }
    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: $scope.Deletepath,
            data: { 'PIMasterId': $scope.PImodelNew.Id, 'PIVersionId': $scope.SelectedPIVersion },
            dataType: 'JSON'
            //url: 'Commercial/ProformaInvoice/DeletePI?id=' + $scope.TitleModel.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetAllVersionData()
                $scope.LoadPISearchList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };
    //$scope.DeleteMaterial = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.Deletepath,
    //        data: { 'PIMasterId': $scope.PImodelNew.Id },
    //        dataType: 'JSON'

    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            //  $scope.LoadPISearchList();
    //        }
    //    }, function () {
    //        ShowResult(commonMessage.NetworkError, 'failure');
    //    }).finally(function () {
    //    });

    //};

    $scope.message_Materialconfirmation = null;
    $scope.removePIMaterial = function (data) {
        $scope.PIGridModel = data;
        if (!baseService.isUndefinedOrNull($scope.PIGridModel.Id))
            $scope.message_Materialconfirmation = 'Are you sure want to delete this material permanently';
        angular.element(document.querySelector('#confirmMaterialPopUp')).modal('show');
    }
    $scope.DeletePIMaterial = function () {
        $http({
            method: 'POST',
            url: 'Commercial/ProformaInvoice/DeleteMaterial?id=' + $scope.PIGridModel.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                // $scope.GridTitle();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    //Terms&Condition
    $scope.TermsAndConditionsList = [];
    $scope.TermsAndConditions = function () {

        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Commercial/ProformaInvoice/TermsAndConditions'
        }).then(function successCallback(response) {
            $scope.TermsAndConditionsList = response.data;
            //$scope.TermsAndCondition.TermsAndConditions = response.data[0].TermsAndConditions;

        });
    }
    $scope.TermsAndConditions();
    $scope.TermsAndConditionGridList = [];
    $scope.LoadTermsAndConditionGrid = function (TermsAndConditionId, PIMasterId) {
        $scope.TermsAndConditionGridList = [];

        $scope.termandconditionURL = $scope.path + "GetTermsAndConditionsPIList";

        try {
            $http({
                method: 'POST',
                url: $scope.termandconditionURL,
                data: { 'TermsAndConditionMasterId': TermsAndConditionId, 'PIMasterId': PIMasterId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.TermsAndConditionGridList = [];
                $scope.TermsAndConditionGridList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');


        }
    }
    $scope.TermsAndConditions = {
        Id: null
        , Description: null
        , TermsAndConditions: null
    };
    $scope.TitleModel = {
        Id: null,
        TermsAndConditionsMasterId: $scope.PImodelNew.TermsAndConditionsId,
        Title: null,
        Sequence: 0
    }
    $scope.showTermsAndConditionDetailPopUp = function (args) {
        $scope.TitleId = args.TermsAndConditionPIChildId;
        $scope.POPupList = [];
        $scope.GetRemarksByMaster($scope.TitleId);
        angular.element(document.querySelector('#GridPopUp')).modal('show');
    }
    $scope.removeBoMDetail = function (obj) {
        $scope.TitleModel = obj.data;
        if (!baseService.isUndefinedOrNull($scope.TitleModel.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.TitleModel.Title + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }
    $scope.closeRemarksPopUp = function () {

        angular.element(document.querySelector('#GridPopUp')).modal('hide');
    }

    $scope.POPupList = [];

    $scope.GetRemarksByMaster = function (id) {
        $scope.POPupList = [];
        $http.get('Commercial/ProformaInvoice/GetPopUp?TermsAndConditionsPIDetailId=' + id)
            .then(function successCallback(response) {
                $scope.POPupList = response.data;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            })
    }
    $scope.SaveGrid = function (model) {
        //$scope.TitleModel.TermsAndConditionsMasterId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveGridUrl,
            data: { 'GridData': model.data, 'TitleId': $scope.TitleId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetRemarksByMaster($scope.TitleId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };
    $scope.DeletePODetailPOPUp = function (model) {
        try {

            $http({
                method: 'POST',
                url: 'Commercial/ProformaInvoice/DeletePIDetailPOPup',
                data: { id: model.data.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetRemarksByMaster($scope.TitleId);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

}
