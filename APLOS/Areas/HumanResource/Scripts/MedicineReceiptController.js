'use strict';
MedicineReceiptController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', '$window' ,'baseService', '$routeParams', '$location', '$http', '$controller' ,'$filter'];
function MedicineReceiptController(cboService, commonMessage, $scope, $rootScope, $window, baseService, $routeParams, $location, $http, $controller, $filter) {
    $rootScope.title = 'Medicine Receipt';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/MedicineReceipt/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveUrlP = $scope.path + 'SavePurpose';
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.partyType = 'Vendor';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    

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

    var curDate = new Date()
    $scope.ModelTemp = {
        Id: null,
        InvoiceDate: curDate,
        PartyName: null,
        PartyCode:null,
        InvoiceNo: null,
        PartyId: null,
       
    };
    $scope.ModalNew = Object.assign({}, $scope.ModelTemp);

    $scope.MedicineList = [];

    $scope.getMedicineData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getMedicineData',
            dataType:'JSON'
        }).then(function successCallback(response) {
            $scope.MedicineList = response.data;
        });
    }
    $scope.getMedicineData();

    $scope.getPartyList = function () {
        $http({
            url: 'Parties/party/GetCompanyPartyDataList' + $scope.partyType
        }).then(function successCallback(response) {
            $scope.partyList = response.Rows;
        });      
    }

   
    $scope.productNew = Object.assign({}, $scope.product);
    $scope.partyList = [];
    $scope.showPartyPopUp = function () {
        //baseService.setCurrentPage('partyList');
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
       
        $scope.ModalNew.PartyCode = party.Code;
        $scope.ModalNew.PartyName = party.UserName;
        $scope.ModalNew.PartyId = party.Id;
        
        $scope.hidePartyPopUp();
    };

    $scope.displaychild = function () {
        // check input field Validations
       /* if (baseService.isUndefinedOrNull($scope.ModalNew.InvoiceNo)) {
            ShowResult('Invoice Number is Required.', 'failure');
            throw 'Invalid Request';
        }
        if (baseService.isUndefinedOrNull($scope.ModalNew.PartyName)) {
            ShowResult('Vendor is Required.', 'failure');
            throw 'Invalid Request';
        } if (baseService.isUndefinedOrNull($scope.ModalNew.InvoiceDate)) {
            ShowResult('InvoiceDate is Required.', 'failure');
            throw 'Invalid Request';
        }*/
        document.querySelector("#medicinereceiptchildId").style.display = "block";
        
    }

    $scope.calcAmount = function (e) {
        $scope.Quantity = 123;
        $scope.Amount = $scop.Quantity * $scope.Rate;
    }
    
}
