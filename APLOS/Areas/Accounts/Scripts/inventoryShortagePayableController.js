'use strict';
inventoryShortagePayableController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'factoryService', '$window'];
function inventoryShortagePayableController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, factoryService, $window) {
    $rootScope.title = "Inventory Payable";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Accounts/InventoryPayable/';
    $scope.getListUrl = 'Accounts/InventoryPayable/GetInventoryShortage/';
    $scope.saveUrl = 'Accounts/InvoicePost/CreateShortagePayable';


    $scope.ShortageList = [];
    $scope.GetShortageList = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'Accounts/InventoryPayable/GetInventoryShortage'
        }).then(function successCallback(response) {
            $scope.ShortageList = response.data;
        });
    }
    $scope.GetShortageList();


    //$scope.getDataList = function () {
    //    baseService.init($scope.getListUrl, null, null, null, 'PartyName, PartyAccountGroupName, Id, GRNDate', 'PartyName');
    //    $scope.getData = function (pageno) {
    //        baseService.pagination(pageno)
    //            .then(function (result) {
    //                $scope.products = [];
    //                $scope.products = result.Rows;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.getData();
    //};
    //$scope.getDataList();

    $scope.model = {
        AlongwithInvoice: null
        , BaseAmount: null
        , BaseCurrencyId: null
        , BaseNoOfDays: null
        , BaseOnDueDate: null
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: null
        , CurrencyCode: null
        , CurrencyId: null
        , DeliveryBy: null
        , DeliveryByAddress: null
        , DeliveryPartyPlantId: null
        , DeliveryState: null
        , DocDate: null
        , DocRefNo: null
        , EntryDate: null
        , FixedAssetOrInventory: null
        , GRNDate: null
        , GateEntryNo: null
        , Id: null
        , InvoiceDate: null
        , InvoiceNo: null
        , InvoicingBy: null
        , InvoicingByAddress: null
        , InvoicingPartyPlantId: null
        , InvoicingState: null
        , IsNonCreditable: null
        , MaterialStorageId: null
        , MatureDate: null
        , PODepended: null
        , PartyAccountGroupName: null
        , PartyCode: null
        , TransactionAmount: null
        , TransactionQty: null
        , TransactionUoM: null
        , TransactionUoMId: null
        , EmployeeTransactionTypeId: null
        , EmployeeId: null
        , EmployeeCode: null
        , EmployeeName: null

        , PartyId: null
        , PartyPlantId: null
        , PartyName: null
        , PaymentTermId: null
        , PaymentTermName: null
        , PostingDate: new Date()
        , VoucherTypeId: null
        , ToCurrencyRate:null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    // #region Tab

    //$scope.tab = 1;
    //$scope.setTab = function (newTab) {
    //    $scope.tab = newTab;
    //};

    //$scope.isSet = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};

    //$scope.redirectTab = function () {
    //    if ($scope.tabForm1.$invalid) {
    //        $scope.setTab(1);
    //    }
    //    else if ($scope.tabForm2.$invalid) {
    //        $scope.setTab(2);
    //    }
    //    else if ($scope.tabForm3.$invalid) {
    //        $scope.setTab(3);
    //    }
    //    else if ($scope.tabForm4.$invalid) {
    //        $scope.setTab(4);
    //    }
    //};

    // #endregion Tab

    cboService.getCboVoucherTypeAccountPayableList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });

    cboService.GetCboExpensesBookingTransactionType(function (result) {
        $scope.employeeTransactionTypeList = result;
    });

    //$scope.popUpList = [];
    //$scope.valueData = '';

    //$scope.searchByList = [
    //    {
    //        value: 'PartyCode'
    //        , name: 'Vendor Code'
    //    },
    //    {
    //        value: 'PartyName'
    //        , name: 'Vendor Name'
    //    },
    //    {
    //        value: 'PartyAccountGroupName'
    //        , name: 'Account Group'
    //    },
    //    {
    //        value: 'EmployeeName'
    //        , name: 'Employee Name'
    //    },
    //    {
    //        value: 'Id'
    //        , name: 'GRN No'
    //    },
    //    {
    //        value: 'GRNDate'
    //        , name: 'GRN Date'
    //    },
    //    {
    //        value: 'DocRefNo'
    //        , name: 'Vendor DocRefNo'
    //    },
    //    {
    //        value: 'InvoiceNo'
    //        , name: 'Invoice No'
    //    },
    //    {
    //        value: 'InvoiceDate'
    //        , name: 'Invoice Date'
    //    }
    //];
    $scope.columnExcluedList = ['BaseNoOfDays', 'BaseOnDueDate', 'MatureDate'];
    //$scope.popUp = function () {
    //    $scope.popUpParameters = {
    //        limit: 10,
    //        offset: 0,
    //        order: 'asc',
    //        sort: 'PartyName',
    //        searchBy: "PartyName",
    //        pageSize: 10,
    //        total_count: 0,
    //        search: null,
    //        serverPagination: true
    //    };
    //    $scope.popUpList = [];
    //    $scope.popUpDataList = [];
    //    $scope.popUpUrl = 'Products/InventoryReceive/GetListForInvShortagePayable';
    //    $scope.popUpTitle = 'Inventory Receive Data';
    //    baseService.setCurrentPage('popUpDataList');
    //    $scope.getPopUpData = function (pageno) {
    //        baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
    //            .then(function (result) {
    //                $scope.popUpDataList = result.Rows;
    //                $scope.popUpParameters.total_count = result.Total;
    //                if (baseService.arrayLength($scope.popUpList) === 0)
    //                    baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
    //                angular.element(document.querySelector('#shortagepopUp')).modal('show');
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure', 'shortagepopUp');
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.getPopUpData();
    //};

    $scope.shortageList = [];
    $scope.shortageData = function () {
        $http.get('Products/InventoryReceive/GetListForInvShortagePayable')
            .then(function (response) {
            $scope.shortageList = response.data;
            angular.element(document.querySelector('#shortagepopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    


    $scope.selectDoubleClick = function ($event) {
        var data = $event.data;
        var voucherTypeId = $scope.modelNew.VoucherTypeId;
        $scope.modelNew = data;
        $scope.modelNew.VoucherTypeId = voucherTypeId;
        $scope.modelNew.EmployeeTransactionTypeId = null;
        $scope.modelNew.IsWrittenOff = data.IsWrittenOff;
        $scope.modelNew.PostingDate = new Date();
        if (!baseService.isUndefinedOrNull(data.EmployeeId) && $scope.employeeTransactionTypeList.length === 1) {
            $scope.modelNew.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeId;
        }
        getRecievedList();
       // getServiceChargeList();
        getInventoryMaterialList(data.Id, data.EmployeeId, data.IsTaxApplicable);
        factoryService.getCurrencyPrecision(data.BaseCurrencyId);
        GetCurrencyExchangeRateList();
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#shortagepopUp')).modal('hide');
    };

    function getInventoryMaterialList(inveReveiveId, employeeId, isReversCharge) {
        $http.get('Products/InventoryReceive/GetInventoryMaterialShortagePayable?inveReveiveId=' + inveReveiveId + '&employeeId=' + employeeId + '&isReversCharge=' + isReversCharge)
            .then(function (response) {
                $scope.inventoryMaterialList = [];
                $scope.newList = [];
                $scope.inventoryMaterialList = response.data;
                console.log('inventoryMaterialList',$scope.inventoryMaterialList)
                if (!$scope.modelNew.IsNonCreditable)
                    reArrangeCreditableList($scope.inventoryMaterialList, $scope.newList);
                else if ($scope.modelNew.IsNonCreditable)
                    reArrangeNonCreditableList($scope.inventoryMaterialList, $scope.newList);
                if (!baseService.isUndefinedOrNull(employeeId))
                    $scope.glPushInList();
            });
    }

    function reArrangeCreditableList(list, newList) {
        var svcList = ($filter('filter')(list, { OtherName: 'Svc' }, true));
        for (var t = 0; t < baseService.arrayLength(svcList); t++) {
            var row = svcList[t];
            if (row.OtherName === 'Svc' && row.TrnType === 'Dr') {
                var taxList = ($filter('filter')(list, { OtherName: 'Tax', TrnType: 'Dr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId }, true));
                row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
                assignSvcInTax(row, list, 'Dr');
            }
            else if (row.OtherName === 'Svc' && row.TrnType === 'Cr') {
                var taxList = ($filter('filter')(list, { OtherName: 'Tax', TrnType: 'Cr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId }, true));
                row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
                assignSvcInTax(row, list, 'Cr');
            }
        }
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            var row = list[i];
            if (row.OtherName === 'Tax' && row.TrnType === 'Dr') {
                var flag = false;
                for (var t = 0; t < baseService.arrayLength(newList); t++) {
                    if (row.OtherName === newList[t].OtherName && row.TrnType === newList[t].TrnType && row.GLGeneralInfoId === newList[t].GLGeneralInfoId && row.BudgetMasterId === newList[t].BudgetMasterId && row.ActivityId === newList[t].ActivityId) {
                        newList[t].Dr += row.Dr;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    newList.push(list[i]);
            }
            else if (row.OtherName === 'Tax' && row.TrnType === 'Cr') {
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Dr += row.Dr;
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }
            else if (row.OtherName !== 'Svc') newList.push(list[i]);
            //else newList.push(list[i]);
        }
    }
    function distinct(taxList) {

        var lst = [];
        var newList = [];
        var newListRow = {};
        for (var i = 0; i < taxList.length; i++) {
            if (!lst.includes(taxList[i].TaxCategoryID)) {
                lst.push(taxList[i].TaxCategoryID);

                var svcList = ($filter('filter')(taxList, { TaxCategoryID: taxList[i].TaxCategoryID }, true));
               
                var sum = 0;
                for (var j = 0; j < svcList.length; j++) {
                    sum += svcList[j].Amount;
                }
                newListRow = taxList[i];
                newListRow.Amount = sum;
                newList.push(newListRow);
            }
        }

    }
    function assaignTax(taxList,newList) {

        var lst = [];//use only for check duplicate.
       // var newList = [];
        var newListRow = {};
        for (var i = 0; i < taxList.length; i++) {
           // var rowset = ($filter('filter')(taxList, { GLGeneralInfoId: taxList[i].GLGeneralInfoId, BudgetMasterId: taxList[i].BudgetMasterId, ActivityId: taxList[i].ActivityId }, true));
            if (!lst.includes(taxList[i].ActivityId)) {
                lst.push(taxList[i].ActivityId);
                var svcList = ($filter('filter')(taxList, { GLGeneralInfoId: taxList[i].GLGeneralInfoId, BudgetMasterId: taxList[i].BudgetMasterId, ActivityId: taxList[i].ActivityId }, true));

                var sum = 0;
                for (var j = 0; j < svcList.length; j++) {
                    sum += svcList[j].Amount;
                }
                newListRow = taxList[i];
                newListRow.Amount = sum;
                newListRow.Dr = sum;
                newList.push(newListRow);
            }
        }

    }


    function reArrangeNonCreditableList(list, newList) {
        var svcList = ($filter('filter')(list, { OtherName: 'Svc' }, true));
        var taxList0 = ($filter('filter')(list, { OtherName: 'Tax' }, true));
       var taxList = taxList0.concat(svcList);
        assaignTax(taxList, newList);
        //for (var t = 0; t < baseService.arrayLength(svcList); t++) {
        //    var row = svcList[t];
        //    if (row.OtherName === 'Svc' && row.TrnType === 'Dr') {
        //        var taxList = ($filter('filter')(list, { OtherName: 'Tax', TrnType: 'Dr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId, TaxCategoryId: row.TaxCategoryId }, true));
        //       // row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
        //        assignSvcInTax(row, taxList, 'Dr');
        //    }
        //    else if (row.OtherName === 'Svc' && row.TrnType === 'Cr') {
        //        var taxList = ($filter('filter')(list, { OtherName: 'Tax', TrnType: 'Cr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId, TaxCategoryId: row.TaxCategoryId }, true));
        //        row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
        //        assignSvcInTax(row, taxList, 'Cr');
        //    }
        //}

        for (var i = 0; i < baseService.arrayLength(list); i++) {
            var row = list[i];
            if (row.OtherName === 'Material' && row.TrnType === 'Cr') {
                var flag = false;
                for (var t = 0; t < baseService.arrayLength(newList); t++) {
                    if (newList[t].OtherName === 'Material' && newList[t].TrnType === 'Cr' && row.MaterialGroupMasterId === newList[t].MaterialGroupMasterId) {
                        //row.Dr = parseFloat(row.Dr) + (parseFloat(svcTotal) / baseService.arrayLength(materialList));
                        //row.Amount = row.Dr;
                        newList[t].Cr += row.Cr;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    newList.push(list[i]);
            }
            else if (row.OtherName == 'Charge' || row.OtherName == 'Vendor')
                newList.push(list[i]);
            //else if(row.OtherName !== 'Svc')
            //    if(row.OtherName !== 'Material')
            //    newList.push(list[i]);
            //else newList.push(list[i]);s
        }
    }

    function assignSvcInTax(row, taxList, trnType) {
        for (var i = 0; i < baseService.arrayLength(taxList); i++) {
            var row2 = taxList[i];
            if (row2.OtherName === 'Tax' && row2.TrnType === trnType && row2.GLGeneralInfoId === row.GLGeneralInfoId
                && row2.BudgetMasterId === row.BudgetMasterId && row2.ActivityId === row.ActivityId && row2.TaxCategoryId === row.TaxCategoryId) {
                row2[trnType] += row.Amount;
                row2.Amount += row.Amount;
            }
        }
    }

    $scope.glPushInList = function () {
        var data = $filter('filter')($scope.employeeTransactionTypeList, { EmployeeTransactionTypeId: $scope.modelNew.EmployeeTransactionTypeId }, true);
        for (var i = 0; i < baseService.arrayLength($scope.newList); i++) {
            if ($scope.newList[i].OtherName === 'Vendor') {
                if (baseService.arrayLength(data) > 0) {
                    $scope.newList[i].GLGeneralInfoId = data[0].PayableGLId;
                    $scope.newList[i].GLGeneralInfoCode = data[0].PayableGLCode;
                    $scope.newList[i].GLGeneralInfoName = data[0].PayableGLName;
                    $scope.newList[i].BudgetMasterId = data[0].PayableBudgetMasterId;
                    $scope.newList[i].BudgetCode = data[0].PayableBudgetCode;
                    $scope.newList[i].BudgetName = data[0].PayableBudgetName;
                    $scope.newList[i].ActivityId = data[0].PayableActivityId;
                    $scope.newList[i].ActivityCode = data[0].PayableActivityCode;
                    $scope.newList[i].ActivityName = data[0].PayableActivityName;
                }
                else {
                    $scope.newList[i].GLGeneralInfoId = null;
                    $scope.newList[i].GLGeneralInfoCode = null;
                    $scope.newList[i].GLGeneralInfoName = null;
                    $scope.newList[i].BudgetMasterId = null;
                    $scope.newList[i].BudgetCode = null;
                    $scope.newList[i].BudgetName = null;
                    $scope.newList[i].ActivityId = null;
                    $scope.newList[i].ActivityCode = null;
                    $scope.newList[i].ActivityName = null;
                }
                break;
            }
        }
    };

    $scope.parallelCurrencyTypeList = [];
    $scope.companyCurrencyId = null;
    $scope.companyGroupCurrencyId = null;
    $scope.hardCurrencyId = null;
    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
    }).then(function successCallback(response) {
        angular.forEach(response.data, function (item, i) {
            if (item.ParallelCurrencyType === 'CompanyCurrency') {
                $scope.companyCurrencyId = item.CurrencyId;
                $scope.companyCurrencyName = item.Code;
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyDr', CurrencyId: item.CurrencyId });
                $scope.parallelCurrencyTypeList.push({ ParallelCurrencyType: 'CompanyCurrency', CurrencyType: 'CompanyCurrencyCr', CurrencyId: item.CurrencyId });
            }
        });
    });
    function GetCurrencyExchangeRateList() {
        if ($scope.modelNew.CurrencyId !== null && undefined !== $scope.modelNew.CurrencyId) {
            $http({
                method: 'GET',
                url: 'currencies/ExchangeRate/ParallelExchangeRate?fromdate=' + $scope.modelNew.DocDate + '&currencyId=' + $scope.modelNew.CurrencyId
            }).then(function (response) {
                $scope.currencyExchangeRate = [];
                for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                    $scope.currencyExchangeRate.push({
                        CompanyCurrencyId: $scope.companyCurrencyId
                        , CompanyCurrencyName: $scope.companyCurrencyName
                        , CompanyFromCurrencyId: response.data[i].FromCurrencyId
                        , ToCurrencyId: response.data[i].ToCurrencyId
                        , CompanyCurrencyRate: response.data[i].ToCurrencyRate

                        , FromCurrencyUnit: response.data[i].FromCurrencyUnit
                        , FromCurrencyCode: response.data[i].FromCurrencyCode
                    });
                }
            });
        }
    }

    $scope.Post = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.EmployeeId)) {
            var data = $filter('filter')($scope.newList, { OtherName: 'Vendor' }, true);
            if (baseService.isUndefinedOrNull(data[0].GLGeneralInfoId)) return ShowResult('Employee GL not found', 'failure');
            if (baseService.isUndefinedOrNull(data[0].BudgetMasterId)) return ShowResult('Employee budget not found', 'failure');
            if (baseService.isUndefinedOrNull(data[0].ActivityId)) return ShowResult('Employee activity not found', 'failure');
            for (var i = 0; i < baseService.arrayLength($scope.inventoryMaterialList); i++) {
                if ($scope.inventoryMaterialList[i].OtherName === 'Vendor') {
                    $scope.inventoryMaterialList[i].GLGeneralInfoId = data[0].GLGeneralInfoId;
                    $scope.inventoryMaterialList[i].GLGeneralInfoCode = data[0].GLGeneralInfoCode;
                    $scope.inventoryMaterialList[i].GLGeneralInfoName = data[0].GLGeneralInfoName;
                    $scope.inventoryMaterialList[i].BudgetMasterId = data[0].BudgetMasterId;
                    $scope.inventoryMaterialList[i].BudgetCode = data[0].BudgetCode;
                    $scope.inventoryMaterialList[i].BudgetName = data[0].BudgetName;
                    $scope.inventoryMaterialList[i].ActivityId = data[0].ActivityId;
                    $scope.inventoryMaterialList[i].ActivityCode = data[0].ActivityCode;
                    $scope.inventoryMaterialList[i].ActivityName = data[0].ActivityName;
                }
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                receiveId: $scope.modelNew.Id
                , voucherVM: $scope.modelNew
                , voucherDetailVMList: $scope.newList/*$scope.inventoryMaterialList*/
                , voucherDetailCurrencyVMList: $scope.currencyExchangeRate
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDataList();
                $scope.Clear();
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.Clear = function () {
        $scope.model = {};
        $scope.modelNew = { PostingDate: new Date() };
        $scope.inventoryMaterialList = [];
        $scope.currencyExchangeRate = [];
        $scope.inventoryReceivedList = [];
        $scope.newList = [];
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    };

    function getRecievedList() {
        $http.get('Products/GoodsReceiveNote/GetInventoryShortageMaterialPayableList?inveReveiveId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.inventoryReceivedList = response.data.Rows;
                console.log($scope.inventoryReceivedList);
                checkSameValueInColumnList($scope.inventoryReceivedList, 'TransactionUoM');
            });
    }

    function getServiceChargeList() {
        $http.get('Products/GoodsReceiveNote/GetServiceChargeList?receiveId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
                console.log($scope.chargesList);
            });
    }

    $scope.sumORnot = false;
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }

    $scope.getPabyableJournal = function (data, reportFormat) {
        $window.open($scope.path + 'PabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&employeeId=' + data.EmployeeId + '&isReversCharge=' + data.IsTaxApplicable, '_blank');
        // $window.open('Products/InventoryReceive/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId, '_blank');
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });
    $scope.searchGLByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getGLPopUP = function (index) {
        $scope.index = index;
        $scope.getGLData = function (pageno) {
            baseService.paginationBase('accounts/glitem/GetAssetCOAWise?coaId=' + $scope.companyConfig.COAId, pageno, $scope.glListParameters)
                .then(function (data) {
                    $scope.glList = data.Rows;
                    $scope.glListParameters.total_count = data.Total;
                    angular.element(document.querySelector('#gltListPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getGLData();
    };

    $scope.setGL = function (data) {
        $scope.newList[$scope.index].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.newList[$scope.index].GLName = data.GLGeneralInfoCode + '-' + data.GLGeneralInfoName;
        getBudgetList($scope.index);
        $scope.closeGltListPopUp();
    };

    $scope.refreshGL = function (index) {
        $scope.newList[index].GLGeneralInfoId = null;
        $scope.newList[index].GLName = null;
        $scope.newList[index].BudgetMasterId = null;
        $scope.newList[index].ActivityId = null;
        $scope.newList[index].budgetList = null;
        $scope.newList[index].activityList = null;
    };

    $scope.downPaymentBudgetList = [];
    function getBudgetList(index) {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.companyConfig.COAId, $scope.newList[index].GLGeneralInfoId, function (result) {
            $scope.newList[index].BudgetMasterId = null;
            $scope.newList[index].budgetList = [];
            $scope.newList[index].budgetList = result;
        });
    }

    $scope.activityList = [];
    $scope.getActivity = function (index) {
        cboService.getBudgetMasterActivityCbo($scope.newList[index].BudgetMasterId, function (result) {
            $scope.newList[index].ActivityId = null;
            $scope.newList[index].activityList = [];
            $scope.newList[index].activityList = result;
        });
    };

    $scope.closeGltListPopUp = function () {
        $scope.index = -1;
        angular.element(document.querySelector('#gltListPopUp')).modal('hide');
    };
}