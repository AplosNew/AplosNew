'use strict';
PrePurchaseInvoiceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function PrePurchaseInvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'PrePurchase Invoice';
    
    $scope.ModelList = [];
    $scope.path = 'Commercial/PrePurchaseInvoice/';
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
        $http.get("Commercial/PrePurchaseInvoice/getlist")
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

    $scope.ModelTemp = {
        Id: null,
        InvoiceNo: null,
        InvoiceDate: null,
        InvoiceAttachment: null,
        PurchaseLCId: null,
        PurchaseLCNo: null,
        BLAWBNo: null,
        BLAWBDate: null,
        BLAWBAttachment: null,
        ShipmentModeId: null,
        PackingDescription: null,
        MaterialDescription: null,
        VesselDetail: null,
        VesselSalesDetail: null,
        VesselAttachment: null,
        VesselTrackingNo: null,
        ETA: null,
        PackingListAttachment: null,
        NegotiableDocDispatchNo: null,
        NegotiableDocDispatchDate: null,
        CNFAgentDocument: null,
        CNFAgent: null,
        TransportAgent: null,
        TransportDcument: null,
        TransportDocumentAttachment: null,
        PortOfArrival: null,
        VechileNo: null,
        CustomsEntryNo: null,
        PassBookNo: null,

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.LcList = [];
    $scope.GetLC = function () {
        var PoType = 'PO';
        $http({
            method: 'GET',
            //url: "Commercial/PrePurchaseInvoice/GetLCList"
            url: 'Products/PurchaseDocumentsAcceptance/GetPOWithLCList?PoType=' + PoType,
        }).then(function successCallback(response) {
            $scope.LcList = response.data;
            angular.element(document.querySelector('#ContractPopUp')).modal('show');
        });

    }
    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    };

    $scope.recorddoubleclickLC = function ($event) {
        var x = $event;
        $scope.ModelNew.PurchaseLCId = x.data.PurchaseLCNO;
        $scope.ModelNew.PurchaseLCNo = x.data.LCRef;
        $scope.ModelNew.Amount = x.data.Amount;
        $scope.ModelNew.Vendor = x.data.PartyName;
        $scope.ModelNew.LCDate = x.data.LCOpeningDate;
        $scope.ModelNew.Currency = x.data.CurrencyName;
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    };

    $scope.Save = function () {

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
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
                $scope.ModelNew.CNFAgent = party.Id;
                $scope.ModelNew.CNFAgentCode = party.Code;
                $scope.ModelNew.CNFAgentName = party.UserName;
            }
        } else {
            if ($scope.partyIndex !== -1) {
                var party = $scope.partyList[$scope.partyIndex];
                $scope.ModelNew.TransportAgent = party.Id;
                $scope.ModelNew.TransportAgentCode = party.Code;
                $scope.ModelNew.TransportAgentName = party.UserName;
            }
        }
        $scope.hidePartyPopUp();
    };

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };

    $("#uploadBtn1").change(function () {
        $scope.filedata1 = this.files[0];
    });

    document.getElementById("uploadBtn1").onchange = function () {
        var filename = document.getElementById("uploadFile1").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile1").value = res;
    };

    $("#uploadBtn2").change(function () {
        $scope.filedata2 = this.files[0];
    });

    document.getElementById("uploadBtn2").onchange = function () {
        var filename = document.getElementById("uploadFile2").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile2").value = res;
    };

    $("#uploadBtn3").change(function () {
        $scope.filedata3 = this.files[0];
    });

    document.getElementById("uploadBtn3").onchange = function () {
        var filename = document.getElementById("uploadFile3").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile3").value = res;

    };

    $("#uploadBtn4").change(function () {
        $scope.filedata4 = this.files[0];
    });

    document.getElementById("uploadBtn4").onchange = function () {
        var filename = document.getElementById("uploadFile4").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile4").value = res;
    };

    $("#uploadBtn5").change(function () {
        $scope.filedata5 = this.files[0];
    });

    document.getElementById("uploadBtn5").onchange = function () {
        var filename = document.getElementById("uploadFile5").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile5").value = res;
    };

    $scope.SaveInvoiceFile = function (flg) {
        $scope.ModelNew.Flag = flg;
        if ($scope.ModelNew.Flag === 'Invoice') {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.ModelNew.InvoiceAttachment = fileName;
            if (!baseService.isUndefinedOrNull($scope.ModelNew.InvoiceAttachment)) {
                if ($scope.ModelNew.InvoiceAttachment.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
        }
        else if ($scope.ModelNew.Flag === 'BLAWB') {
            if (!baseService.isUndefinedOrNull($scope.filedata3) && $scope.filedata3.size > 2000000)
                throw $scope.filedata3.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata3))
                fileName = $scope.filedata3.name;
            $scope.ModelNew.BLAWBAttachment = fileName;
            if (!baseService.isUndefinedOrNull($scope.ModelNew.BLAWBAttachment)) {
                if ($scope.ModelNew.BLAWBAttachment.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            $scope.filedata = $scope.filedata3;
        }
        else if ($scope.ModelNew.Flag === 'Vessel') {
            if (!baseService.isUndefinedOrNull($scope.filedata1) && $scope.filedata1.size > 2000000)
                throw $scope.filedata1.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata1))
                fileName = $scope.filedata1.name;
            $scope.ModelNew.VesselAttachment = fileName;
            if (!baseService.isUndefinedOrNull($scope.ModelNew.VesselAttachment)) {
                if ($scope.ModelNew.VesselAttachment.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            $scope.filedata = $scope.filedata1;
        }
        else if ($scope.ModelNew.Flag === 'Packing') {
            if (!baseService.isUndefinedOrNull($scope.filedata4) && $scope.filedata4.size > 2000000)
                throw $scope.filedata4.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata4))
                fileName = $scope.filedata4.name;
            $scope.ModelNew.PackingListAttachment = fileName;
            if (!baseService.isUndefinedOrNull($scope.ModelNew.PackingListAttachment)) {
                if ($scope.ModelNew.PackingListAttachment.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            $scope.filedata = $scope.filedata4;
        }
        
        else if ($scope.ModelNew.Flag === 'CNF') {
            if (!baseService.isUndefinedOrNull($scope.filedata2) && $scope.filedata2.size > 2000000)
                throw $scope.filedata2.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata2))
                fileName = $scope.filedata2.name;
            $scope.ModelNew.CNFAgentDocument = fileName;
            if (!baseService.isUndefinedOrNull($scope.ModelNew.CNFAgentDocument)) {
                if ($scope.ModelNew.CNFAgentDocument.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            $scope.filedata = $scope.filedata2;
        }

        else if ($scope.ModelNew.Flag === 'Transport') {
            if (!baseService.isUndefinedOrNull($scope.filedata5) && $scope.filedata5.size > 2000000)
                throw $scope.filedata5.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata5))
                fileName = $scope.filedata5.name;
            $scope.ModelNew.TransportDocumentAttachment = fileName;
            if (!baseService.isUndefinedOrNull($scope.ModelNew.TransportDocumentAttachment)) {
                if ($scope.ModelNew.TransportDocumentAttachment.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            $scope.filedata = $scope.filedata5;
        }
        
        var formData = new FormData();

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'Commercial/PrePurchaseInvoice/CreateAttachment',

                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("data", angular.toJson(data.data));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }

                    return formData;
                },
                data: { 'data': $scope.ModelNew, 'file': $scope.filedata },


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

        }
    };

}