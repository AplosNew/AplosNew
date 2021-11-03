'use strict';
mixingController.$inject = ["addressService", 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function mixingController(addressService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Mixing";
    $scope.Action = 'Save';
    $scope.path = 'OrderManagements/Mixing/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';


    $scope.ModelTemp = {
        Id: null,
        Code: null,
        ShortName: null,
        Description: null,
        StandardName: null,
        UserName: null,
        ContractId: null,
        CustomerName: null,
        ContractNo: null,
        LCRef: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    $scope.ModelList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetList",            
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    };
    $scope.getData();
    //$scope.Get = function (args) {

    //    $scope.ModelNew = Object.assign({}, args.data);
    //    //UomCboByFGMaterialMaster($scope.ModelNew.MaterialMasterId);
    //    $scope.Action = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    //------Contract Part
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

    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    };

    $scope.SelectedContract = function (obj) {
        //debugger;
        //var data = obj.data.ContractId;
        $scope.ModelNew.ContractId = obj.data.ContractId;
        $scope.ModelNew.CustomerName = obj.data.CustomerName;
        $scope.ModelNew.ContractNo = obj.data.ContractNo;
        $scope.ModelNew.LCRef = obj.data.LCRef;

        $scope.GetMasterOrderByContractList();
        $scope.GetBOQMixingMasterOrderItem();
        //console.log($scope.productNew);
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    };
    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    };
    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        $scope.masterOrderCustomerList = [];
        $http({
            method: 'GET',
            url: "OrderManagements/Mixing/GetMasterOrderListbyContract?contractId=" + $scope.ModelNew.ContractId
        }).then(function (response) {
            $scope.masterOrderCustomerList = response.data;


        });
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
    };


    $scope.GetMasterOrderBySavedContrList = function () {
        $scope.masterOrderCustomerList = [];
        $http({
            method: 'GET',
            url: "OrderManagements/Mixing/GetMasterOrderListbySavedContract?contractId=" + $scope.ModelNew.ContractId
        }).then(function (response) {
            $scope.masterOrderCustomerList = response.data;


        });
        /*angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');*/
    };
    $scope.GetMasterOrderBySavedContractList = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        //UomCboByFGMaterialMaster($scope.ModelNew.MaterialMasterId);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        $scope.GetMasterOrderBySavedContrList();
        $scope.GetBOQMixingMasterOrderItem();
    }



    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    };
    $scope.qboqMixingList2 = [];
    $scope.qboqMixingListFiltered = [];
    var qboqMixingList = [];
    $scope.MasterOrderSelect = function (args, data) {
        var selectedList = $filter('filter')($scope.masterOrderCustomerList, { 'isToBeSelect': true });
        qboqMixingList = [];// $filter('filter')($scope.masterOrderCustomerList, { 'isToBeSelect': true });

        for (var i = 0; i < selectedList.length; i++) {
            $scope.qboqMixingList2 = $scope.qboqMixingList.filter(d => d.Id == selectedList[i]["MasterOrderItemId"]);
            for (var j = 0; j < $scope.qboqMixingList2.length; j++) {
                qboqMixingList.push($scope.qboqMixingList2[j]);
            }

        }
        $scope.qboqMixingListFiltered = qboqMixingList;




        // var selectedId =  ;
    }
    //----Contract-----


    //------BOQ Item------
    $scope.qboqList = [];
    $scope.GetQBOQByMasterOrderItem = function (data) {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetQBOQByMasterOrderItem?itemId=' + data.data.MasterOrderItemId
        }).then(function successCallback(response) {
            $scope.qboqList = response.data;

            if ($scope.qboqList.length > 0) {
                angular.element(document.querySelector('#QBOQPoUp')).modal('show');
            }
            else {
                throw "No Item Found";
            }
        })
    };


    $scope.qboqMixingList = [];
    $scope.GetBOQMixingMasterOrderItem = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/Mixing/GetBOQMixingMasterOrderItem?cotractId=' + $scope.ModelNew.ContractId
        }).then(function successCallback(response) {
            $scope.qboqMixingList = response.data;

            //if ($scope.qboqList.length > 0) {
            //    angular.element(document.querySelector('#QBOQPoUp')).modal('show');
            //}
        })
    };

    //------BOQ Item------


    $scope.Save = function () {
        //debugger;
        try {

            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'data': $scope.ModelNew
                        , 'MixingChildList': $scope.qboqMixingListFiltered
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
                      //  $scope.getalldata();
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
                        'data': $scope.ModelNew

                        , 'MixingChildList': $scope.qboqMixingListFiltered
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                       // $scope.getalldata();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
        catch (e) {
            throw e;
        }
    };
}