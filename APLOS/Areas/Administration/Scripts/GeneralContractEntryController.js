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

    //#region List object
    var CurrentDate = new Date();
    $scope.ModelTemp = {
        Id: null,
        Date: CurrentDate,
        GeneralContractId: null,
        EntityId: null,
        CheckBySystemId:null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //#endregion List object

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
                $scope.GetChildList();
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

    // #region Double Tap open grid
    $scope.Get = function (args) {
        $scope.GetAllContractItem();
        $scope.GetAllCheckById();
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };
    // #endregion Double Tap open grid

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
                ClearFields(response.data.Sequence);
                $scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //  #endregion Save

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
                ClearFields(response.data.Sequence);
                $scope.getData();

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
            Date: CurrentDate,
            GeneralContractId: null,
            EntityId: null,
            CheckBySystemId: null
        };
        $scope.ContractItemList = [];
        $scope.CheckedByIdList = [];
        $scope.ContractList = [];
        $scope.EntityList = [];
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
    //  #endregion Clear
}