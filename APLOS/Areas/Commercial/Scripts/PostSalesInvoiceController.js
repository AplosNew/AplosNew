'use strict';
PostSalesInvoiceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'bankService'];
function PostSalesInvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, bankService) {
    $rootScope.title = 'Post Sales Invoice';

    $scope.ModelList = [];
    $scope.path = 'Commercial/PostSalesInvoice/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    $scope.partyType = "Vendor";
    //$controller("partyBaseController", { $scope: $scope, $http: $http });
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http.get("Commercial/PostSalesInvoice/getlist")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ModelList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    $scope.getData();

    $scope.portList = [];
    cboService.getPortByPlantCbo(function (result) {
        $scope.portList = result;
    });

    $scope.deliveryPortList = [];
    cboService.getPortCbo(function (result) {
        $scope.deliveryPortList = result;
    });

    $scope.bankMasterList = [];
    bankService.GetNegotiatingBankMasterCboListByPlant(function (result) {
        $scope.bankMasterList = result;

    });

    $scope.shipmentModeList = [];
    $scope.getShipmode = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/shipmode/GetCbo/'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.shipmentModeList = response.data;
            }
        });
    };
    $scope.getShipmode();


    $scope.destinationList = [];
    $scope.getDestination = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/destination/GetCbo'
        }).then(function successCallback(response) {
            $scope.destinationList = response.data;
        });
    };
    $scope.getDestination();

    $scope.dischargePortList = [];
    $scope.GetPortOfDischargeByDstination = function () {
        $http({
            method: 'GET',
            url: 'Commercial/PostSalesInvoice/GetPortByDestinationCbo?destinationId=' + $scope.ModelNew.FinalDestinationId
        }).then(function successCallback(response) {
            $scope.dischargePortList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.dischargePortList = response.data;
            }
        });
    };


    $scope.ModelTemp = {
        Id: null,
        SalesId: null,
        InvoiceDate: null,
        BankMasterId: null,
        ShipmentModeId: null,
        PortOfLoadingId: null,
        ExpFormNo: null,
        ExpDate: null,
        CargoNetWt: 0,
        CargoGrossWt: 0,
        Dimension: null,
        ExFactoryDocRef: null,
        ExFactoryDate: null,
        TransportAgentId: null,
        TransportDocRefNo: null,
        TransportDocDate: null,
        TransportVehicleNo: null,
        TransportDriverName: null,
        TransportDriverNo: null,
        PreCarriageBy: null,
        PlaceOfReceiptByPreCarriage: null,
        PreCarriageDocRef: null,
        PreCarriageDocDate: null,
        CNFAgentId: null,
        CNFContainerNo: null,
        CNFVesselTrackingNo: null,
        CNFVesselName: null,
        CNFVesselSalesDetails: null,
        CNFBLAWB: null,
        CNFBLAWBDate: null,
        ETA: null,
        FinalDestinationId: null,
        PortOfDischargeId: null,
        PortOfDelivaryId: null,
        BankDocRef: null,
        BankDocDate: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null,
        FileName:null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.invoiceList = [];
    $scope.GetSalesList = function () {
        $http({
            method: 'GET',
            url: "Commercial/PostSalesInvoice/GetSalesList"
        }).then(function successCallback(response) {
            $scope.invoiceList = response.data;
            angular.element(document.querySelector('#SalesPopUp')).modal('show');
        });

    }
    $scope.CloseSalesPopUp = function () {
        angular.element(document.querySelector('#SalesPopUp')).modal('hide');
    };

    $scope.SetInvoiceNo = function ($event) {
        var x = $event;
        $scope.ModelNew.SalesId = x.data.Id;
        $scope.ModelNew.InvoiceNo = x.data.InvoiceNo;
        $scope.ModelNew.PartyName = x.data.PartyName;
        $scope.ModelNew.InvoiceDate = x.data.InvoiceDate;
        $scope.ModelNew.Amount = x.data.Amount;
        $scope.ModelNew.Currency = x.data.CurrencyCode;
        $scope.ModelNew.ContractNo = x.data.ContractNo;
        $scope.ModelNew.MLCRef = x.data.LCRef;
        $scope.ModelNew.BenificiaryBankId = x.data.BenificiaryBankId;
        angular.element(document.querySelector('#SalesPopUp')).modal('hide');
    };

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "" + fieldname + " is required.";
            }
        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            CheckField("Invoice No", $scope.ModelNew.InvoiceNo);
            CheckField("Customer", $scope.ModelNew.PartyName);
            CheckField("Bank", $scope.ModelNew.BankMasterId);
            CheckField("ExFactory Date", $scope.ModelNew.ExFactoryDate);
            CheckField("Shipment Mode", $scope.ModelNew.ShipmentModeId);
            CheckField("Port of Loading", $scope.ModelNew.PortOfLoadingId);
            CheckField("Final Destination", $scope.ModelNew.FinalDestinationId);
            CheckField("Port Of Discharge", $scope.ModelNew.PortOfDischargeId);
            CheckField("Port Of Delivery", $scope.ModelNew.PortOfDelivaryId);
            CheckField("Transport Agent", $scope.ModelNew.TransportAgentId);
            CheckField("Transport Doc Ref No.", $scope.ModelNew.TransportDocRefNo);
            CheckField("Pre-CarriageBy", $scope.ModelNew.PreCarriageBy);
            CheckField("Place Of Receipt", $scope.ModelNew.PlaceOfReceiptByPreCarriage);
            CheckField("Pre-Carriage Doc Ref No.", $scope.ModelNew.PreCarriageDocRef);
            CheckField("Pre-Carriage DocDate", $scope.ModelNew.PreCarriageDocDate);
            CheckField("CNF Agent", $scope.ModelNew.CNFAgentId);
            CheckField("Container No", $scope.ModelNew.CNFContainerNo);
            CheckField("Vessel Tracking No", $scope.ModelNew.CNFVesselTrackingNo);



        } catch (ex) {
            throw ex;
        }
    }

    $scope.Save = function () {
        try {
            //ValidationMaster();

            if (baseService.isUndefinedOrNull($scope.ModelNew.ExpDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.ExpDate)) {
                    throw "Expected Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.ExpDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.ExFactoryDate)) {
                    throw "ExFactory Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.CNFBLAWBDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.CNFBLAWBDate)) {
                    throw "BL Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.BankDocDate)) {
                if (new Date($scope.ModelNew.CNFBLAWBDate) < new Date($scope.ModelNew.BankDocDate)) {
                    throw "Bank Doc Date should greater than BL Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.ETA)) {
                if (new Date($scope.ModelNew.CNFBLAWBDate) < new Date($scope.ModelNew.ETA)) {
                    throw "ETA Date should greater than BL Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.TransportDocDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.TransportDocDate)) {
                    throw "Transport Doc Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.PreCarriageDocDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.PreCarriageDocDate)) {
                    throw "Pre-Carriage Doc Date should greater than Invoice Date";
                }
            }
            //$scope.$broadcast('show-errors-check-validity');
            //if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'entity': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Id;
                    $scope.getData();
                    $scope.Action = 'Save';
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

            //}
        } catch (e) {
            ShowResult(e, 'failure');
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
                    $scope.ModelList = [];
                    ClearFields();
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
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    }

    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'UserName, PartyAccountGroupName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.flag = null;
    $scope.showPartyPopUp = function (flg) {
        $scope.flag = flg;
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

    $scope.selectPartyPopUpRow = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedCustomer = id;
    };

    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.closePartyPopUp = function () {
        if ($scope.flag === 'CNF') {
            if ($scope.partyIndex !== -1) {
                var party = $scope.partyList[$scope.partyIndex];
                $scope.ModelNew.CNFAgentId = party.Id;
                $scope.ModelNew.CNFAgentCode = party.Code;
                $scope.ModelNew.CNFAgentName = party.UserName;
            }
        } else {
            if ($scope.partyIndex !== -1) {
                var party = $scope.partyList[$scope.partyIndex];
                $scope.ModelNew.TransportAgentId = party.Id;
                $scope.ModelNew.TransportAgentCode = party.Code;
                $scope.ModelNew.TransportAgentName = party.UserName;
            }
        }
        $scope.hidePartyPopUp();
    };

    //#region Production Bulletin Picture upload

    $scope.onBeginPBUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.PostSalesInvoice.Id))
                throw 'Please select/save Post Sales Invoice first'

            args.data = $scope.bulletinTemplateNew.Id;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadPBUrl = "Commercial/PostSalesInvoice/SavePostSaleFile";
    $scope.fileselect = function (e) {

    }
    $scope.errorPBPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
            ShowResult('Please select/save the production order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }
    $scope.getFileList = function () {
        $http({
            method: 'POST', url: $scope.path + 'GetFileInfo', dataType: 'JSON',
            data: { Id: $scope.bulletinTemplateNew.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                var str = response.data[0].PicFileName;
                var extention = str.substr(str.indexOf('.'));
                $scope.PicFileName = virtualPath.ProductionBulletinImage + '/' + $scope.bulletinTemplateNew.Id + extention;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    //#endregion Production Bulletin Picture upload

}