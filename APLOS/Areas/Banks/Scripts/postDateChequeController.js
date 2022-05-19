"use strict";
postDateChequeController.$inject = ["commonMessage","cboService", "$scope", "$rootScope", "baseService", "$http", "$filter","$controller"];
function postDateChequeController(commonMessage, cboService, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "PDC";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.path = "Banks/CheckManagement/";
    $scope.saveUrl = $scope.path + 'CreatePdc';
    //$scope.getLotNumberUrl = $scope.path + "getlotnumber/";
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = "Vendor";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.employeeUrl = 'IE/MachineMasterTransaction/GetEmployeeListByWhom';
    //$scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom',
    $scope.deleteUrl = $scope.path + 'delete/';


    $scope.pdc = {
        Id: null,
        BankMasterId: null,
        BankName: null,
        PartyId: null,
        PartyName: null,
        DocRefNo: null,
        DocDate: null,
        BaseDate: null,
        CurrencyId: null,
        Days: null,
        PostingDate: null,
        PaymentDate: null,
        //VoucherId: false,
        //IsClose: false,
        //PaymentTermId: null,
        Amount: 0,
        ResponsiblePersonId: null,
        ResponsiblePersonCode: null,
        ResponsiblePerson: null,
        POId: null,
        RemainderDays: null,
        Days:7,
        Remarks: null

    };
    $scope.pdcNew = Object.assign({}, $scope.pdc);

    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
        $scope.selectedBank = id;
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            selectBankRow();
        }
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
        $scope.bankIndex = -1;
    };

    function selectBankRow() {
        var bank = $scope.bankList[$scope.bankIndex];
        if (bank.GLGeneralInfoId === null) {
            ShowResult("Bank GL not found!", "failure");
        }
        else if (bank.CurrencyId === null) {
            ShowResult("Bank Transaction Currency not found!", "failure");
        }
        else {
            $scope.pdcNew.BankMasterId = bank.BankMasterId;
            $scope.pdcNew.BankName = bank.BankName;
           
           //$scope.checkLotNew.BankCurrencyId = bank.CurrencyId;
            //$scope.checkLotNew.BankCurrency = bank.CurrencyCode;
        }
    }

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
       
            //$scope.removeDrRow();
            $scope.pdcNew.PartyId = party.Id;
            $scope.pdcNew.PartyCode = party.Code;
            $scope.pdcNew.PartyName = party.UserName;
            //$scope.pdcNew.CurrencyId = party.CurrencyId;
          
        
        $scope.hidePartyPopUp();
    };

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Save = function () {
        try {
            $scope.$broadcast("show-errors-check-validity");
            
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: { 'Pdc': $scope.pdcNew },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.
                            data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.Action = 'Update';
                        $scope.getData();
                        $scope.Clear();
                    }
                });
                    return true;
            }
      catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.pdcNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.pdcNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
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

    $scope.Name = null;
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;

        $scope.pdcNew.ResponsiblePersonId = data.SystemId;
        $scope.pdcNew.ResponsiblePerson = data.EmployeeName;
        $scope.pdcNew.ResponsiblePersonCode = data.EmployeeCode;

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };

    $scope.ModelList = [];
    $scope.getData = function () {
        $scope.ModelList = [];
        $http.get('Banks/CheckManagement/getlist')
            .then(function (response) {
                $scope.ModelList = response.data;

            });
    };
    $scope.getData();

    $scope.recorddoubleclick = function (args) {
        $scope.pdcNew = Object.assign({}, args.data);
        try {
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        catch (e) {

        }
    };


    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.pdcNew = Object.assign({}, $scope.pdc);
        return true;
    };

    $scope.getBank = function () {
        try {
            $scope.getBankData = function (pageno) {
                baseService.paginationBase("banks/bankmaster/GetHouseBankBankMasterList", pageno, $scope.bankParameters)
                    .then(function (result) {
                        $scope.bankList = result.Rows;
                        $scope.bankParameters.ToNotal_count = result.ToNotal;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            $scope.getBankData();
            angular.element(document.querySelector("#bankPopUp")).modal("show");
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.getchequeLotDetailList = function (chequeId) {
        $scope.TempchequeLot = null;
        try {
        $http({
            method: "get",
            url: "banks/CheckManagement/GetChequeLotDetailList?chequeId=" + chequeId
        }).then(function successCallback(response) {
            $scope.chequeLotDetailList = response.data;
            $scope.TempchequeLot = chequeId;
            });
            angular.element(document.querySelector("#chequeLotPopUp")).modal("show");
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector("#chequeLotPopUp")).modal("hide");

    }

    $scope.Griddata = [];
    $scope.getalldata = function () {
        //debugger;
        var PoType = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Banks/CheckManagement/GetListOfPO?PoType=' + PoType + '&Status=' + $scope.status,
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
        });
    };

    $scope.POPopUp = function () {
        //$scope.getalldata();
        //debugger
        $scope.status = 'PO';
        if ($scope.status === 'PO') {
            $scope.status = 'PO';
            //alert('1');
            $scope.productNew.PO = 'PO';
            $scope.getalldata();
        }
        else if ($scope.status === 'Acceptance') {
            $scope.status = 'Acceptance';
            $scope.productNew.PO = 'Acceptance';
            $scope.getalldata();
        }
        angular.element(document.querySelector('#POPopUp')).modal('show');

    };

    $scope.POPopUpClose = function () {
        //debugger;
        angular.element(document.querySelector('#POPopUp')).modal('hide');

    };

//    $scope.Get = function (data) {
//        $scope.checkLotNew.Id = data.Id;
//        $scope.checkLotNew.FromNo = data.FromNo;
//        $scope.checkLotNew.ToNo = data.ToNo;
//        $scope.checkLotNew.LotNumber = data.LotNumber;
//        $scope.Action = "Update";
//    };
}