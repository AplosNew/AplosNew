'use strict';
GeneralContractEntryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function GeneralContractEntryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Contract Entry';
    $scope.ModelList = [];
    $scope.path = 'Administration/GeneralContractEntry/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Update';
    $scope.deleteUrl = 'Administration/GeneralContractItemMaster/Delete'

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    //#region List object

    $scope.ModelTemp = {
        Id: null,
        Date: null,
        GeneralContractId: null,
        EntityId: null,
        CheckBySystemId: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //#endregion List object

    $scope.ContractList = [];
    $scope.GetContractName = function () {
        $http.get('Administration/GeneralContractEntry/GetContract')
            .then(function successCallback(response) {
                $scope.ContractList = response.data;
            })
    }
    $scope.GetContractName();

    $scope.EntityList = [];
    $scope.GetEntity = function () {
        $http.get('Administration/GeneralContractEntry/GetEntity')
            .then(function successCallback(response) {
                $scope.EntityList = response.data;
            })
    }
    $scope.GetEntity();

    

    $scope.ContractItemList = [];
    $scope.GetAllContractItem = function () {
        $http.get('Administration/GeneralContractEntry/GetAllContractItem?headerId=' + $scope.ModelNew.GeneralContractId)
            .then(function successCallback(response) {
                $scope.ContractItemList = response.data;
            })
    }

    $scope.CheckedByIdList = [];
    $scope.GetAllCheckById = function () {
        $http.get('Administration/GeneralContractEntry/GetAllCheckById?headerId=' + $scope.ModelNew.GeneralContractId)
            .then(function successCallback(response) {
                $scope.CheckedByIdList = response.data;
            })
    }

    $scope.GetList = function () {
        $http.get('Administration/GeneralContractEntry/GetList')
            .then(function successCallback(response) {
                $scope.ModelList = response.data;
              
            })
    }
    $scope.GetList();

    $scope.GetChildList = function () {
        $http.get('Administration/GeneralContractEntry/GetChildList?headerId=' + $scope.ModelNew.Id)
            .then(function successCallback(response) {
                $scope.ContractItemList = response.data;
            })
    }

    // FETCH VALUE FROM Transaction QANTITY, RATE AND CALCULATE
    $scope.ob = {};
    $scope.calcAmount = function (data1, index) {

        if (data1.TransactionQuantity == null || data1.TransactionQuantity == '') {
            $scope.ContractItemList[index].Amount = data1.TransactionQuantity * 1
        }
        else if (data1.Amount == null || data1.Amount == '') {
            $scope.ContractItemList[index].Amount = 1 * data1.Rate
        }
        else {
            $scope.ContractItemList[index].Amount = data1.TransactionQuantity * data1.Rate;

        }

    }

    //  #region Save
   
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModelNew,
                'contractItemDetail': $scope.ContractItemList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //ClearFields(response.data.Sequence);
                $scope.GetList();
                $scope.Clear();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //  #endregion Save
    // #region Double Tap open grid
    $scope.Get = function (args) {
       
        $scope.ModelNew = Object.assign({}, args.data);
        document.getElementById("updatebtn").style.display = "block"
        document.getElementById("savebtn").style.display = "none"
        $scope.GetAllCheckById();
        $scope.GetChildList();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };
    // #endregion Double Tap open grid
    // #region Update
    $scope.Update = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.updateUrl,
            data: {
                'data': $scope.ModelNew,
                'contractItemDetail': $scope.ContractItemList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');               
                $scope.GetList();
                $scope.Clear();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    // #endregion Update

    //  #region Clear
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelTemp = {
            Id: null,
            Date: null,
            GeneralContractId: null,
            EntityId: null,
            CheckBySystemId: null
        };
        $scope.ContractItemList = [];
        //$scope.CheckedByIdList = [];
        //$scope.ContractList = [];
        //$scope.EntityList = [];
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
    //  #endregion Clear

   

    $scope.invalidDate = false;
    $scope.DateValidation = function () {
        var msg = "";
        if (new Date($scope.ModelNew.Date) > new Date()) {
         
            throw "Date must be equal to current Date!";
        }
        else if (new Date($scope.ModelNew.Date) < new Date()) {
            throw "Doc date must be equal to current Date!";
            $scope.invalidDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.ModelNew.Date)) {
            msg = "Date is required.";
            $scope.invalidDate = true;
        }
        else $scope.invalidDocDate = false;
       // return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    }

    $scope.PrintData = function (data) {
        try {
            $scope.fileName = "General Contract Approved Report.xlsx";
            //$scope.ReportFormat = 'Excel';
            $scope.ReportFormat = 'Pdf';
            var url = 'Administration/GeneralContractChecked/GetGeneralContractReport?reportFormat=' + $scope.ReportFormat + '&ContractId=' + data.data.Id;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

}