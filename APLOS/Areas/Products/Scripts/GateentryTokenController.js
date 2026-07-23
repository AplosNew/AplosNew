'use strict';

GateentryTokenController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function GateentryTokenController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

    $rootScope.title = "Gate Entry";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/GateentryToken/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';

    $scope.deleteUrl = $scope.path + 'DeleteGateEntry/';
    $scope.deleteUrl1 = $scope.path + 'CancelGateEntry/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete/';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.PartyId = null;
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.inventoryMaterialList = [];
    $scope.chargesList = [];
    $scope.ChargeTaxList = [];
    $scope.StateData = [];

    $scope.report = {
        IsUpToLevel: null,
        IsBudgetLevel: false,
        IsActivityLevel: false,
        IsDetailLevel: false,

        //FromDate: $filter('dateFiltering')(Date.now()),
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now())
    };
    //#region Start Here

    $scope.product = {
        Id: null
        , CompanyGroupId: null
        , EntryDate: $filter("dateFiltering")(Date.now())
        , PartyCode: null
        , Description: null
        , PackageQty: null
        , ModeofTransport: null
        , Bill: null
        , PersonName: null
        , MobileNo: null
        , Remarks: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , CompanyId: null
        , PlantId: null
        , GateEntryTime: new Date().toLocaleTimeString({}, { hour12: true, hour: 'numeric', minute: 'numeric' })
        , EmployeeId: null
        , EmployeeName: null

        , GateEntryType: 'Vendor'
        , EmployeeIdForGateEntry: null
        , ResponsiblePersonName: null
        , PlantWiseGateId: null
        , LocalImported: null
        , ImportedNo: null
        , ImportedDate: null
        , ImportedDocNo: null

    };
    $scope.productNew = Object.assign({}, $scope.product);
    $scope.GateList = [];
    $scope.GetGateData = function () {
        $http({
            method: 'GET',
            url: 'Products/GateentryToken/GetAllReqdata'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i]["EntryDate"] = new Date(response.data[i]["EntryDate"]);
            }
            $scope.GateList = response.data;
        });
    }
    $scope.GetGateData();
    $scope.AllTabPrint = function (z) {

        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/GateentryToken/GateEntryReport?GateEntryId=" + data.Id;


    };
    $scope.PlantWiseGateList = [];
    $scope.GetPlantWiseGateList = function () {
        $http({
            method: 'GET',
            url: 'Products/GateentryToken/PlantWiseGateCbo'
        }).then(function successCallback(response) {
            $scope.PlantWiseGateList = response.data;
            if ($scope.PlantWiseGateList.length === 1) {
                $scope.productNew.PlantWiseGateId = $scope.PlantWiseGateList[0].Value;
            }
        });
    }
    $scope.GetPlantWiseGateList();
    $scope.changeSourceFrom = function (from) {
        if (from === 'Vendor') {
            $scope.productNew.ResponsiblePersonName = '';
            $scope.partyType = 'Vendor';
        }
        if (from === 'Employee') {
            $scope.productNew.PartyName = '';
            $scope.productNew.PartyCode = '';

        }
        if (from === 'Director') {
            $scope.productNew.PartyName = '';
            $scope.productNew.PartyCode = '';
            $scope.partyType = 'Director';
        }
    };
    $scope.partyList = [];
    $scope.showPartyPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {

            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
            }
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
    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.productNew.PartyCode = party.Code;
        $scope.productNew.PartyName = party.UserName;
        $scope.productNew.PartyId = party.Id;
        getPartyPlantList();
        $scope.hidePartyPopUp();
    };

    //#region  Employee CAll
    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.productNew.EmployeeName = employee.EmployeeName;
            $scope.productNew.EmployeeId = employee.SystemId;
        }
        $scope.hideEmployeePopUp();
    };
    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };
    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.responsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.responsiblePersonIndex];
            $scope.productNew.ResponsiblePersonName = employee.EmployeeName;
            $scope.productNew.EmployeeIdForGateEntry = employee.SystemId;
        }
        $scope.hideResponsiblePersonPopUp();
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };
    //#endregion

    //#region Save-Delete-Calcel
    $scope.disabledbtn = false;

    $scope.Save = function () {
        
        var dt = $filter("dateFiltering")(Date.now());
        if ($scope.productNew.EntryDate < dt) {
            if ($scope.productNew.LocalImported === 'Imported' && ($scope.productNew.Description === null || $scope.productNew.Description === "" || $scope.productNew.Description === undefined)) {

                ShowResult('Please enter Imported information in the description field', 'failure');
                return false;

            }
            if ($scope.Action === "Save") {
                $scope.ConModal();
            }
        }

        else {
            if ($scope.productNew.LocalImported === 'Imported' && ($scope.productNew.Description === null || $scope.productNew.Description === "" || $scope.productNew.Description === undefined)) {

                ShowResult('Please enter Imported information in the description field', 'failure');
                return false;

            }
            try {
                $scope.$broadcast('show-errors-check-validity');
                if ($scope.productNewForm.$valid) {
                    $scope.disabledbtn = true;
                    // $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                    // $scope.productNew.PlantWiseGateId = $scope.productNew.PlantWiseGateId;

                    $scope.product = Object.assign({}, $scope.productNew);
                    if ($scope.Action === "Save") {
                        $http({
                            method: 'POST',
                            url: $scope.saveUrl,
                            data: {
                                'entity': $scope.product,
                                'PlantWiseGateId': $scope.productNew.PlantWiseGateId,
                            },
                            dataType: 'JSON'
                        }).then(function (response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                                $scope.disabledbtn = false;
                            }
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.productNew.Id = response.data.entity.Id;
                                //$scope.productNew.PartyName = $scope.product.PartyName;

                                $scope.Action = "Update";
                                //$scope.getDataList();
                                $scope.GetGateData();
                                $scope.disabledbtn = false;
                            }
                        }), function (response) {
                            ShowResult(response.data.Message, 'failure');
                        };
                    }
                    else if ($scope.Action === "Update") {
                        ShowResult('You Do not have permission to update', 'failure');
                        $scope.disabledbtn = false;
                    }
                }
            } catch (e) {
                throw e;
            }
        }


    };
    $scope.Delete = function () {
        //if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
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
                    $scope.GetGateData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        //}
        //else
        //    ShowResult('First delete all line item.', 'failure');
    };
    $scope.Cancel = function () {
        //if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
        if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl1 + $scope.productNew.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetGateData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        //}
        //else
        //    ShowResult('First delete all line item.', 'failure');
    };
    //#endregion
    $scope.Clear = function () {
        ClearFields();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        return true;
    };
    function ClearFields() {
        //$scope.GateEntryType = 'Vendor';
        $scope.Action = "Save";
        $scope.product = {};
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew = {
            Id: null
            , CompanyGroupId: null
            , EntryDate: $filter("dateFiltering")(Date.now())
            , PartyCode: null
            , Description: null
            , PackageQty: null
            , ModeofTransport: null
            , Bill: null
            , PersonName: null
            , MobileNo: null
            , Remarks: null
            , InvoicingPartyPlantId: null
            , InvoicingByAddress: null
            , DeliveryPartyPlantId: null
            , DeliveryByAddress: null
            , CompanyId: null
            , PlantId: null
            , GateEntryTime: new Date()
            , GateEntryType: 'Vendor'
            , PlantWiseGateId: null
        };

        $scope.inventoryMaterialList = [];
        $scope.chargesList = [];
        $scope.grossTotal = 0;
        baseService.removeErrorClasses();
        //$scope.getToCurrencyRate();
    }


    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        var Id = x.data.Id;

        $scope.productNew = x.data;
        $scope.Id = $scope.productNew.Id;
        $scope.productNew.EntryDate = x.data.EntryDate1;
        //$scope.productNew.EmployeeName = x.data.em
        if (!baseService.isUndefinedOrNull($scope.productNew.PartyId)) {
            $scope.productNew.GateEntryType = 'Vendor';
            getPartyPlantList();
        }
        else {
            $scope.productNew.GateEntryType = 'Employee';
            $scope.productNew.ResponsiblePersonName = x.data.EmployeeName1;
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.plantList = [];
    function getPartyPlantList() {
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address1;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }
    //function getPartyPlantList() {
    //    $scope.plantList = [];
    //    $http.get('Products/Requisition/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
    //        angular.forEach(response.data, function (item) {
    //            $scope.plantList.push(item);
    //            if (item.IsDefault) {
    //                $scope.productNew.InvoicingPartyPlantId = item.Value;
    //                $scope.productNew.DeliveryPartyPlantId = item.Value;
    //                $scope.productNew.InvoicingByAddress = item.Address1;
    //                $scope.productNew.DeliveryByAddress = item.Address2;
    //                $scope.productNew.InvoicingState = item.StateName;
    //                $scope.productNew.InvoicingGSTIN = item.GSTIN;
    //                $scope.productNew.DeliveryState = item.StateName;
    //                $scope.productNew.DeliveryGSTIN = item.GSTIN;
    //            }
    //        });
    //    });

    //}

    //#endregion 
    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };
    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = state;
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

    $scope.ConModal = function (id) {
        $scope.id = id;
        $scope.message = 'Are you sure want to Save back date data?';
        angular.element(document.querySelector('#ConPopUp')).modal('show');
    };
    $scope.ConfirmSave = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {

                // $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.product,
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            //$scope.productNew.PartyName = $scope.product.PartyName;

                            $scope.Action = "Update";
                            //$scope.getDataList();
                            $scope.GetReq();
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    ShowResult('You Do not have permission to update', 'failure');
                    //$http({
                    //    method: 'POST',
                    //    url: $scope.updateUrl,
                    //    data: $scope.product,
                    //    dataType: 'JSON'
                    //}).then(function successCallback(response) {
                    //    if (response.data.Error === true) {
                    //        ShowResult(response.data.Message, 'failure');
                    //    }
                    //    else {
                    //        ShowResult(response.data.Message, 'success');

                    //        $scope.GetReq();

                    //    }
                    //}, function errorCallBack(response) {
                    //    ShowResult(response.data.Message, 'failure');
                    //});
                }
            }
        } catch (e) {
            throw e;
        }



        //   // GateenToken get data GateenRegisterList




    }
    //#region GateEntry Reginter by nurul
    //Date: 15-7-2020



    $scope.GateenRegisterList = [];
    $scope.GetGateentryRegister = function () {

        $http({
            method: 'GET',
            url: 'Products/GateentryToken/GateEntryLoadOnData?fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate

        }).then(function successCallback(response) {
            $scope.GateenRegisterList = response.data;

        });
        //$scope.report.FromDate = $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1));
        //$scope.report.ToDate = $filter('dateFiltering')(Date.now());
    }
    $scope.GetGateentryRegister();

    $scope.GateEntryReportExcel = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation('div_WDFromDate', true, "From Date is required.");
        }
        if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation('div_WDToDate', true, "To Date is required.");
        }
        else {

            try {
                var file_src = $scope.path + 'GateEntryReportExcel?fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate;

                $rootScope.report(file_src);

            } catch (e) {
                ShowResult(e, 'failure');
            }
        }
    }
    //GatenntryRegisterListPdf
    $scope.GatenntryRegisterListPdf = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }
        if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation('div_WDToDate', true, "To Date is required.");
        }
        else {
            try {
                var file_src = $scope.path + 'GatenntryRegisterListPdf?fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate;
                $rootScope.report(file_src);

            } catch (e) {
                ShowResult(e, 'failure');
            }
        }
    }


    //#endregion


    $scope.dateRange = "false";
    //$scope.fromDateTitle = "As On Date";
    //$scope.legendTitle = "As On Date";
    $scope.toDateShow = false;
    $scope.viewChange = function () {
        if ($scope.dateRange === "true") {
            $scope.fromDateTitle = "fromDate";
            $scope.legendTitle = "Within Date Range";
            $scope.toDateShow = true;
            $scope.report = {
                IsUpToLevel: null,
                IsBudgetLevel: false,
                IsActivityLevel: false,
                IsDetailLevel: false,
                ReportFormat: 'Pdf',
                //FromDate: $filter('dateFiltering')(Date.now()),
                FromDate: $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1)),
                ToDate: $filter('dateFiltering')(Date.now())
            };
        }
        else {
            $scope.fromDateTitle = "As On Date";
            $scope.legendTitle = "As On Date";
            $scope.toDateShow = false;
            $scope.report = {
                IsUpToLevel: null,
                IsBudgetLevel: false,
                IsActivityLevel: false,
                IsDetailLevel: false,
                ReportFormat: 'Pdf',
                //FromDate: $filter('dateFiltering')(Date.now()),
                FromDate: $filter('dateFiltering')(Date.now()),
                ToDate: $filter('dateFiltering')(Date.now())
            };
        }
    };






}