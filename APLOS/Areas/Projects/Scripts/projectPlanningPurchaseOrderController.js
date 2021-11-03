'use strict';
function ProjectPlanningPurchaseOrderController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "ProjectPlanningPurchaseOrder ";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.projectplanningPurchaseOrders = [];
    $scope.budgetMasterSelectedList = [];
    $scope.projectplanningMasterList = [];
    //$scope.searchbyMachineTypelist = [];
    $scope.maxDate = new Date().toDateString();
    $scope.projectPlanningPOMaterialSavedList = [];
    $scope.path = 'Projects/projectplanningpurchaseorder/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    // #region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion
    function getUomList(materailMasterId) {
        $http({
            method: 'GET',
            url: 'Projects/projectplanningpurchaseorder/GetUomList?materailMasterId=' + $scope.materailMasterId,
        }).then(function successCallback(response) {
            $scope.alterNativeUomList = response.data;
        })
    };
    getUomList();
    $scope.searchByProjectPlanningPurchaseOrderList = [
        {
            'name': 'Vendor',
            'value': 'Vendor'
        },
        {
            'name': 'Project Planning',
            'value': 'Title'
        },
        {
            'name': 'Vendor ReferanceNo',
            'value': 'VendorReferanceNo'
        }
    ];
    $scope.projectplanningPurchaseOrderListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Vendor',
        searchBy: "Vendor",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    //*****************ProjectPlanningSearch********************/
    $scope.getProjectPlanningSearchPopup = function () {
        $scope.searchByProjectPlanningList = [
            {
                'name': 'Code',
                'value': 'Code'
            },
            {
                'name': 'Description',
                'value': 'Description'
            },
            {
                'name': 'Title',
                'value': 'Title'
            },
            {
                'name': 'Status',
                'value': 'Status'
            }
        ];
        $scope.projectPlanningListParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'Code',
            searchBy: "Code",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };

        $scope.GetProjectPlanningListData = function (pageno) {
            baseService.paginationBase('Projects/projectPlanning/getlist', pageno, $scope.projectPlanningListParameters)
                .then(function (data) {
                    $scope.projectPlannings = data.Rows;
                    for (var i = 0; i < $scope.projectPlannings.length; i++) {
                        if ($scope.projectPlannings[i].EmployeeId != null) {
                            $scope.projectPlannings[i].ResponsiblePersonName = $scope.projectPlannings[i].EmployeeName;
                        }
                        else if ($scope.projectPlannings[i].PositionId != null) {
                            $scope.projectPlannings[i].ResponsiblePersonName = $scope.projectPlannings[i].PositionName;
                        } else if ($scope.projectPlannings[i].ManpowerBudgetId != null) {
                            $scope.projectPlannings[i].ResponsiblePersonName = $scope.projectPlannings[i].ManpowerBudgetName;
                        }
                    }
                    $scope.projectPlanningListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#projectPlanningPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetProjectPlanningListData();
    }

    //*****************ProjectPlanningRequisitionSearch********************/
    $scope.projectplanningRequisitionNew = Object.assign({}, $scope.projectplanningRequisition);
    $scope.getProjectPlanningRequisitionSearchPopup = function () {
        if ($scope.projectplanningPurchaseOrderNew.ProjectPlanningId === null) {
            return ShowResult("Please select project planning", 'failure');
        }
        $scope.searchByProjectPlanningRequisitionList = [
            {
                'name': 'Project Planning Id',
                'value': 'ProjectPlanningId'
            },
            {
                'name': 'Requisition Date',
                'value': 'RequisitionDate'
            }
        ];
        $scope.projectPlanningRequisitionListParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'ProjectPlanningId',
            searchBy: "ProjectPlanningId",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };

        $scope.GetProjectPlanningRequisitionListData = function (pageno) {
            baseService.paginationBase('Projects/projectPlanningRequisition/GetListWithProjectPlanning?projectPlanningId=' + $scope.projectplanningPurchaseOrderNew.ProjectPlanningId, pageno, $scope.projectPlanningRequisitionListParameters)
                .then(function (data) {
                    $scope.projectPlanningRequisitions = data.Rows;

                    $scope.projectPlanningRequisitionListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#projectPlanningRequisitionPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetProjectPlanningRequisitionListData();
    }

    //********************* ProjectPlanningPruchaseOrder *******************************************//

    $scope.projectplanningPurchaseOrder = {
        Id: null,
        ProjectPlanningRequisitionId: null,
        ProjectPlanningId: null,
        PartyId: null,
        VendorReferanceNo: null,
        CurrencyId: null,
        ExchangeRate: 0,
        PoDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null,
    };

    $scope.projectplanningRequisition = {
        Id: null,
        ProjectPlanningId: null,
        PartyId: null,
        VendorReferanceNo: null,
        CurrencyId: null,
        ExchangeRate: 0,
        RequisitionDate: new Date(),
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.projectPlanningInfo = [];
    $scope.GetProjectPlanningInfo = function (data) {
        $scope.projectPlanningInfo = data;
        $scope.projectplanningPurchaseOrderNew.ProjectPlanningId = data.Id;
        angular.element(document.querySelector('#projectPlanningPopUp')).modal('hide');
    }

    $scope.getProjectPlanningPurchaseOrder = function () {
        $scope.GetProjectPlanningPurchaseOrderListData = function (pageno) {
            baseService.paginationBase($scope.getListUrl, pageno, $scope.projectplanningPurchaseOrderListParameters)
                .then(function (data) {
                    $scope.projectplanningPurchaseOrders = data.Rows;
                    $scope.projectplanningPurchaseOrderListParameters.total_count = data.Total;
                    //$scope.Action = "Update";
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#projectPlanningPurchaseOrderPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetProjectPlanningPurchaseOrderListData();
    };

    $scope.GetProjectPlanningPurchaseOrderInfo = function (data) {
        $scope.getProjectPlanningInfoOnChange(data.ProjectPlanningId);
        $scope.projectplanningPurchaseOrderNew = data;
        $scope.projectplanningPurchaseOrder1 = data;
        getPOMaterialMasterSavedList();
        $scope.projectplanningPurchaseOrderNew.PoDate = $filter('dateFiltering')(data.PoDate);
        angular.element(document.querySelector('#projectPlanningPurchaseOrderPopUp')).modal('hide');
    }

    $scope.GetProjectPlanningRequisitionInfo = function (data) {
        $scope.projectPlanningRequisitionInfo = data;
        $scope.projectplanningPurchaseOrderNew.ProjectPlanningRequisitionId = data.Id;
        $scope.projectplanningPurchaseOrderNew.RequisitionTitle = data.Description;
        //$scope.projectplanningRequisitionNew.Id = data.Id;
        //getPOMaterialMasterSavedList();
        angular.element(document.querySelector('#projectPlanningRequisitionPopUp')).modal('hide');
    }

    $scope.getProjectPlanningInfoOnChange = function (id) {
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanning/GetProjectPlanningById?id=' + id,
        }).then(function successCallback(response) {
            $scope.projectPlanningInfo = response.data.Rows[0];
            $scope.projectplanningPurchaseOrderNew.ProjectPlanningId = response.data.Rows[0].Id;
            getExchangeRate($scope.projectplanningPurchaseOrderNew.CurrencyId, $scope.projectPlanningInfo.CurrencyId, $scope.projectplanningPurchaseOrderNew.PoDate);
            $scope.getProjectPlanningPurchaseOrderDetail();
        })
        //$scope.ProjectPlanningPurchaseOrderDetailSelectedList = [];
        //getPOMaterialMasterSavedList(id);
    }

    function getPOMaterialMasterSavedList() {
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanningPurchaseOrder/GetProjectplanningPOMaterialMasterSavedList?ProjectPlanningPurchaseOrderId=' + $scope.projectplanningPurchaseOrderNew.Id + '&ProjectPlanningRequisitionId=' + $scope.projectplanningPurchaseOrderNew.ProjectPlanningRequisitionId + '&projectPlanningId=' + $scope.projectplanningPurchaseOrderNew.ProjectPlanningId,
        }).then(function successCallback(response) {
            $scope.projectPlanningPOMaterialSavedList = response.data;
        })
    };
    $scope.saveMaster = function () {
        angular.copy($scope.projectplanningPurchaseOrderNew, $scope.projectplanningPurchaseOrder);
        //$scope.$broadcast('show-errors-check-validity');
        if ($scope.projectPlanningPurchaseOrderForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'projectplanningPurchaseOrder': $scope.projectplanningPurchaseOrder },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getProjectPlanningPurchaseOrderById(response.data.ProjectPlanningPurchaseOrderId);

                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            //else if ($scope.Action == "Update") {
            //    $http({
            //        method: 'POST',
            //        url: $scope.saveUrl,
            //        data: $scope.projectplanningPurchaseOrder,
            //        dataType: 'JSON'
            //    }).then(function successCallback(response) {
            //        if (response.data.Error == true) {
            //            ShowResult(response.data.Message, 'failure');
            //        }
            //        else {
            //            ShowResult(response.data.Message, 'success');
            //            //angular.copy($scope.projectplanningPurchaseOrderNew, $scope.projectplanningPurchaseOrder);
            //            $scope.Clear();
            //            $scope.Action = "Save";
            //            if ($scope.index > -1) {
            //                $scope.projectplanningPurchaseOrders[$scope.index] = $scope.projectplanningPurchaseOrder;
            //                $scope.projectplanningPurchaseOrders = $filter('orderBy')($scope.projectplanningPurchaseOrders, 'Sequence');

            //            }

            //        }
            //    }, function errorCallBack(response) {
            //        ShowResult(response.data.Message, 'failure');
            //    });
            //}
        }
    }

    //********************* ProjectPlanningPOMasterDetail *******************************************//
    $scope.ShowProjectPlanningDetailForm = function () {
        $scope.ProjectPlanningPurchaseOrderDetailSelectedList = [];
        angular.element(document.querySelector('#projectPlanningPurchaseOrderDetailFormPopUp')).modal('show');
    }
    //Asset//
    $scope.searchByProjectPlanningDetailList = [

        {
            'name': 'Code',
            'value': 'Code'
        }, {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Base UOM',
            'value': 'BaseUOM'
        }
    ]
    $scope.projectPlanningDetailListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getProjectPlanningDetailListData = function () {
        $scope.projectPlanningDetailTempList = [];
        $scope.projectPlanningDetailList = [];
        $scope.materialMasterUrl = 'Projects/ProjectPlanningPurchaseOrder/GetProjectplanningRequisitionMaterialMasterSavedList?projectPlanningRequisitionId=' + $scope.projectplanningPurchaseOrderNew.ProjectPlanningRequisitionId + '&materialType=Asset&projectPlanningId=' + $scope.projectplanningPurchaseOrderNew.ProjectPlanningId;
        baseService.setCurrentPage('projectPlanningDetailList');
        $scope.loadProjectPlanningDetailListData = function (pageno) {
            baseService.paginationBase($scope.materialMasterUrl, pageno, $scope.projectPlanningDetailListParameters)
                .then(function (result) {
                    $scope.projectPlanningDetailList = result.Rows;
                    $scope.projectPlanningDetailListParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.projectPlanningDetailList.length; i++) {
                        $scope.projectPlanningDetailList[i].Flag = getActive($scope.projectPlanningDetailTempList, $scope.projectPlanningDetailList[i].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#projectPlanningListModal')).modal('show');
        $scope.loadProjectPlanningDetailListData();
    };
    $scope.projectPlanningModalCloseListPopUp = function () {
        setMaterialMasterSaveData();
        angular.element(document.querySelector('#projectPlanningListModal')).modal('hide');
    }
    //
    //NonAsset//
    $scope.searchByProjectPlanningDetailList = [

        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Base UOM',
            'value': 'BaseUOM'
        }
    ]
    $scope.projectPlanningDetailListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getProjectPlanningRMMNonAssetListData = function () {
        $scope.projectPlanningDetailTempList = [];
        $scope.projectPlanningDetailList = [];
        $scope.materialMasterUrl = 'Projects/ProjectPlanningPurchaseOrder/GetProjectplanningRequisitionMaterialMasterSavedList?projectPlanningRequisitionId=' + $scope.projectplanningPurchaseOrderNew.ProjectPlanningRequisitionId + '&materialType=AllMaterialMaster&projectPlanningId=' + $scope.projectplanningPurchaseOrderNew.ProjectPlanningId;
        baseService.setCurrentPage('projectPlanningDetailList');
        $scope.loadProjectPlanningDetailListData = function (pageno) {
            baseService.paginationBase($scope.materialMasterUrl, pageno, $scope.projectPlanningDetailListParameters)
                .then(function (result) {
                    $scope.projectPlanningDetailList = result.Rows;
                    $scope.projectPlanningDetailListParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.projectPlanningDetailList.length; i++) {
                        $scope.projectPlanningDetailList[i].Flag = getActive($scope.projectPlanningDetailTempList, $scope.projectPlanningDetailList[i].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#projectPlanningRMMNonAssetListModal')).modal('show');
        $scope.loadProjectPlanningDetailListData();
    };
    $scope.projectPlanningRMMNonAssetCloseListPopUp = function () {
        setMaterialMasterSaveData();
        angular.element(document.querySelector('#projectPlanningRMMNonAssetListModal')).modal('hide');
    }
    //
    function checkExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PPRequisitonMaterialMasterId === id) {
                return true;
            }
        }
        return false;
    }

    function setMaterialMasterSaveData() {
        angular.forEach($scope.projectPlanningDetailTempList, function (item) {
            if (item.Flag && checkExist($scope.ProjectPlanningPurchaseOrderDetailSelectedList, item.Id) === false) {
                $scope.ProjectPlanningPurchaseOrderDetailSelectedList.push(
                    {
                        Id: null,
                        PPRequisitonMaterialMasterId: item.Id,
                        ProjectPlanningId: $scope.projectplanningPurchaseOrderNew.ProjectPlanningId,
                        ProjectPlanningRequisitionId: $scope.projectplanningPurchaseOrderNew.ProjectPlanningRequisitionId,
                        ProjectPlanningMaterialMasterId: item.ProjectPlanningMaterialMasterId,
                        ProjectPlanningRequsitionMaterialMasterId: item.PPRequisitonMaterialMasterId,
                        ProjectPlanningPurchaseOrderId: $scope.projectplanningPurchaseOrderNew.Id,
                        MaterialMasterId: item.MaterialMasterId,
                        RaisedQuantity: item.RaisedQuantity,
                        Quantity: null,
                        alernativeUomLists: buildUomDropDown($scope.alterNativeUomList, item.MaterialMasterId),
                        AlternativeUomId: item.RequisitionUoMId,
                        BaseUOMId: item.BaseUOMId,
                        UserName: item.UserName,
                        FixedAssetName: item.FixedAssetName,
                        AssetType: item.AssetType,
                        BaseUom: item.BaseUom,
                        PlanningUOMId: item.PlanningUOMId,
                        RequisitionUoMId: item.RequisitionUoMId,
                        RequisitionUOM: item.RequisitionUOM,
                        RequisitionQuantity: item.RequisitionQuantity,
                        Rate: null,
                        Amount: null,
                    }
                );
            }
        });
    }
    $scope.ProjectPlanningPurchaseOrderDetailSelectedList = [];
    //function ProjectPlaningDetailListSave() {
    //    angular.forEach($scope.projectPlanningDetailList, function (item) {
    //        if (item.Flag) {
    //            if (checkProjectPlanningDetailExist($scope.ProjectPlanningPurchaseOrderDetailSelectedList, item.Id) === false) {
    //                $scope.ProjectPlanningPurchaseOrderDetailSelectedList.push(
    //                    {
    //                        Id: null,
    //                        ProjectPlanningId: $scope.projectplanningPurchaseOrderNew.ProjectPlanningId,
    //                        ProjectPlanningRequisitionId: $scope.projectplanningPurchaseOrderNew.ProjectPlanningRequisitionId,
    //                        ProjectPlanningMaterialMasterId: item.ProjectPlanningMaterialMasterId,
    //                        ProjectPlanningRequsitionMaterialMasterId: item.PPRequisitonMaterialMasterId,
    //                        ProjectPlanningPurchaseOrderId: $scope.projectplanningPurchaseOrderNew.Id,
    //                        MaterialMasterId: item.MaterialMasterId,
    //                        ReverseQuantity: item.ReverseTotalQuantity === null ? 0 : item.ReverseTotalQuantity,
    //                        BalanceQuantity: item.ReverseTotalQuantity2 === null ? 0 : item.ReverseTotalQuantity2,
    //                        Quantity: item.Quantity,
    //                        Rate: item.Rate,
    //                        alernativeUomLists: buildUomDropDown($scope.alterNativeUomList, item.MaterialMasterId),
    //                        AlternativeUomId: selectedDDL(buildUomDropDown($scope.alterNativeUomList, item.MaterialMasterId)),
    //                        BaseUOMId: item.BaseUOMId,

    //                        Code: item.Code,
    //                        UserName: item.UserName,
    //                        BaseUom: item.BaseUom,
    //                        PlanningUOMId: item.PlanningUOMId,
    //                        PlanningUOM: item.PlanningUOM,
    //                        PlanningQuantity: item.PlanningQuantity,
    //                        //ReverseQuantity: item.ReverseTotalQuantity + item.Quantity,
    //                        ReverseTotalQuantity: item.ReverseTotalQuantity === null ? 0 : item.ReverseTotalQuantity,
    //                        Quantity: item.Quantity,
    //                        Rate: null,
    //                        Amount: null,
    //                    }
    //                );
    //            }

    //        };
    //        console.log($scope.ProjectPlanningPurchaseOrderDetailSelectedLis);
    //    })
    //}

    $scope.projectPlaningPORequisitionMaterialListForSave = [];
    function ProjectPlaningPOMaterialListSave() {
        if (!$scope.ProjectPlanningPurchaseOrderDetailSelectedList.length > 0) {
            throw "No list found to save";
        }
        $scope.projectPlaningPORequisitionMaterialListForSave = [];
        angular.forEach($scope.ProjectPlanningPurchaseOrderDetailSelectedList, function (item) {
            //if (item.Flag) {
            if (checkMaterialMasterExist($scope.projectPlaningPORequisitionMaterialListForSave, item.Id) === false) {
                if (parseInt(item.Quantity) <= 0 || item.Quantity === null)
                    throw item.UserName + " Quantity must be greater than 0"
                if (parseInt(item.Rate) <= 0 || item.Rate === null)
                    throw item.UserName + " Rate must be greater than 0"

                $scope.projectPlaningPORequisitionMaterialListForSave.push(
                    {
                        Id: null,
                        ProjectPlanningId: item.ProjectPlanningId,
                        ProjectPlanningRequisitionId: item.ProjectPlanningRequisitionId,
                        ProjectPlanningMaterialMasterId: item.ProjectPlanningMaterialMasterId,
                        ProjectPlanningRequsitionMaterialMasterId: item.ProjectPlanningRequsitionMaterialMasterId,
                        MaterialMasterId: item.MaterialMasterId,
                        Quantity: item.Quantity,
                        Rate: item.Rate,
                        ProjectPlanningPurchaseOrderId: item.ProjectPlanningPurchaseOrderId,
                        AlternativeUomId: item.AlternativeUomId,
                        RequisitionUoMId: item.RequisitionUoMId,
                        BaseUOMId: item.BaseUOMId,
                        Amount: item.Amount
                    });
                console.log($scope.projectPlaningPORequisitionMaterialListForSave);
            }

            //}
        })
    }

    $scope.PPPurchaseOrderDetailSave = function () {
        try {
            ProjectPlaningPOMaterialListSave();
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'Projects/ProjectPlanningPurchaseOrder/PoMaterialCreate/',
                    //data: { 'projectplanningPurchaseOrder': $scope.projectplanningPurchaseOrder, 'projectPlanningPORequisitionMaterial': $scope.ProjectPlanningPurchaseOrderDetailSelectedList.length > 0 ? $scope.ProjectPlanningPurchaseOrderDetailSelectedList : $scope.projectPlaningPORequisitionMaterialListForSave },
                    data: { 'projectplanningPurchaseOrder': $scope.projectplanningPurchaseOrderNew, 'projectPlanningPORequisitionMaterial': $scope.projectPlaningPORequisitionMaterialListForSave },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure', 'projectPlanningPurchaseOrderDetailFormPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getProjectPlanningPurchaseOrderById(response.data.ProjectPlanningPurchaseOrderId);
                        $scope.ProjectPlanningPurchaseOrderDetailSelectedList = [];
                        $scope.projectPlaningPORequisitionMaterialListForSave = [];
                        getPOMaterialMasterSavedList();
                        $scope.projectPlanningDetailFormCloseListPopUp();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'projectPlanningPurchaseOrderDetailFormPopUp');
                }
            }
        } catch (e) {
            ShowResult(e, 'Error', 'projectPlanningPurchaseOrderDetailFormPopUp');
        }
    }

    $scope.projectPlanningDetailFormCloseListPopUp = function () {
        angular.element(document.querySelector('#projectPlanningPurchaseOrderDetailFormPopUp')).modal('hide');
    }

    //**************************** Deleting child with master ************************* //
    $scope.valuePassInProjectPlanningPurchaseOrderDelModal = function (index, Id) {
        $scope.ProjectPlanningSelectedId = Id;
        $scope.ProjectPlanningSelectedIdIndex = index;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.ProjectPlanningSelectedId + ' ]';
        angular.element(document.querySelector('#confirmgenericPurchaseOrderSelectedPopUp')).modal('show');
    };
    $scope.DeletePurchaseOrderSelectedItem = function () {
        $http({
            method: 'POST',
            url: 'Projects/ProjectPlanningPurchaseOrder/DeleteProjectPlanningPurchaseOrderPOMasterDetail?id=' + $scope.ProjectPlanningSelectedId,
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                ShowResult(response.data.Message, 'success');
                $scope.projectplanningPurchaseOrders.splice($scope.ProjectPlanningSelectedIdIndex, 1);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
        //}
        //}
        $scope.ProjectPlanningSelectedId = null;
        //$scope.ProjectPlanningSelectedIdIndex = null;
    };

    //***************************** delete only child **************************//
    $scope.valuePassInPORecMasterModal = function (index, Id) {
        $scope.selectedChildId = Id;
        $scope.bIndex = index;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + selectedChildId + ' ]';
        angular.element(document.querySelector('#confirmgenericPORecDetailSelectedItem')).modal('show');
    };

    $scope.DeletePORecDetailSelectedItem = function () {
        for (var i = 0; i < $scope.projectPlanningPOMaterialSavedList.length; i++) {
            if ($scope.projectPlanningPOMaterialSavedList[i].Id == $scope.selectedChildId) {
                $http({
                    method: 'POST',
                    url: 'projects/ProjectPlanningPurchaseOrder/DeleteProjectPlanningPORecMasterMaterial?id=' + $scope.selectedChildId,
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    } else {
                        ShowResult(response.data.Message, 'success');
                        $scope.projectPlanningPOMaterialSavedList.splice($scope.bIndex, 1);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            }
        }
        $scope.selectedChildId = null;
        //$scope.bIndex = null;
    };

    //***************************** delete selected temporary child **************************//

    $scope.valuePassInBudgetSelectedDelModal = function (index, Id) {
        $scope.ProjectPlanningPoDetailSelectedId = Id;
        $scope.ProjectPlanningPoDetailSelectedIdIndex = index;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.ProjectPlanningPoDetailSelectedId + ' ]';
        angular.element(document.querySelector('#confirmgenericPODetailSelectedPopUp')).modal('show');
    };

    $scope.DeletePODetailSelectedItem = function () {
        if (baseService.isUndefinedOrNull($scope.ProjectPlanningPoDetailSelectedId)) {
            $scope.ProjectPlanningPurchaseOrderDetailSelectedList.splice($scope.ProjectPlanningPoDetailSelectedIdIndex, 1);
        }
        $scope.ProjectPlanningPoDetailSelectedId = null;
        $scope.ProjectPlanningPoDetailSelectedIdIndex = null;
    };

    //function checkProjectPlanningDetailExist(list, id) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].ProjectPlanningDetailId === id) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    function checkMaterialMasterExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProjectPlanningMaterialMasterId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.Clear = function () {
        ClearFields();
        $scope.projectplanningPurchaseOrder = { PoDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy') };
        $scope.projectplanningPurchaseOrderNew = { PoDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy') };
        return true;
    }

    function ClearFields() {
        $scope.Action = "Save";
        $scope.projectplanningPurchaseOrder = {};
        $scope.projectplanningPurchaseOrderNew = { PoDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy') };
        $scope.projectplanningPurchaseOrderNew.Id = null;

        $scope.projectPlanningInfo = [];
        $scope.projectplanningPurchaseOrderNew = [];
        $scope.projectPlanningPOMaterialSavedList = [];
        $scope.projectplanningPurchaseOrders = [];
        $scope.projectplanningMasterList = [];
        $scope.materialMasterList = [];
        $scope.ProjectPlanningPurchaseOrderDetailSavedList = [];
        $scope.budgetMasterSelectedList = [];
        $scope.ProjectPlaningProjectPlanningListForSave = [];
        $scope.ProjectPlaningProjectPlanningListForSave = [];
        $scope.projectplanningPurchaseOrderNew.Active = true;
    }

    $scope.getCurrencyName = function (currencyId) {
        $scope.currencyName = angular.element("#CurrencyId :selected").text();
        getExchangeRate($scope.projectplanningPurchaseOrderNew.CurrencyId, $scope.projectPlanningInfo.CurrencyId, $scope.projectplanningPurchaseOrderNew.PoDate);
    }

    function getExchangeRate(fromCurrency, toCurrency, poDate) {
        if (fromCurrency === toCurrency) {
            $scope.projectplanningPurchaseOrderNew.ExchangeRate = 0;
        } else {
            $http({
                method: 'GET',
                url: 'Projects/ProjectPlanningPurchaseOrder/GetPOExchangeRate?poCurrencyId=' + fromCurrency + '&planningCurrencyId=' + toCurrency + '&poDate=' + poDate,
            }).then(function successCallback(response) {
                $scope.poExchangeRateValue = response.data;
                if (baseService.isUndefinedOrNull($scope.poExchangeRateValue.Rate) === false) {
                    $scope.projectplanningPurchaseOrderNew.ExchangeRate = $scope.poExchangeRateValue.Rate.toFixed(4);
                }
                else {
                    $scope.projectplanningPurchaseOrderNew.ExchangeRate = ' ';
                }
            });
        }
        //$scope.projectplanningPurchaseOrderNew.TotalAmount = $scope.projectplanningPurchaseOrderNew.TotalAmount * $scope.projectplanningPurchaseOrderNew.ExchangeRate;
    };

    //function checkExistPODetailSaved(list, id) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].ProjectPlanningDetailId === id) {
    //            return true
    //        }
    //    }
    //    return false;
    //}
    //function checkExistPODetail(selectedList, savedList) {
    //    if (selectedList != null) {
    //        for (var i = 0; i < selectedList.length; i++) {
    //            if (checkExistPODetailSaved(savedList, selectedList[i].ProjectPlanningDetailId)) {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }
    //}

    $scope.projectPlanningDetailTempList = [];
    $scope.selectPODetailChValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistPODetailList($scope.projectPlanningDetailTempList, data.Id) === false) {
                    $scope.projectPlanningDetailTempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.projectPlanningDetailTempList.length; i++) {
                    if ($scope.projectPlanningDetailTempList[i].Id === data.Id) {
                        $scope.projectPlanningDetailTempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistPODetailList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }
    $scope.editProjectPlanningDetail = function (index, data) {
        $scope.projectPlanningEditIndex = index;
        $scope.ProjectPlanningPOSavedEditTempList = Object.assign({}, data);
        $scope.ProjectPlanningPOSavedEditTempList.alernativeUomLists = buildUomDropDown($scope.alterNativeUomList, data.MaterialMasterId);
        angular.element(document.querySelector('#ProjectPlanningDetailEditPopUp')).modal('show');
    }

    $scope.projectPlanningDetailEditSave = function () {
        $scope.ProjectPlanningPOMaterialEditSaveList = [];
        $scope.ProjectPlanningPOMaterialEditSaveList.push($scope.ProjectPlanningPOSavedEditTempList);
        $scope.PPPurchaseOrderDetailEdit();
    }

    $scope.PPPurchaseOrderDetailEdit = function () {
        try {
            angular.forEach($scope.ProjectPlanningPOMaterialEditSaveList, function (item) {
                if (parseInt(item.Quantity) <= 0 || item.Quantity === null || item.Quantity === undefined)
                    throw item.UserName + " Quantity must be greater than 0"
                if (parseInt(item.Rate) <= 0 || item.Rate === null || item.Rate === undefined)
                    throw item.UserName + " Rate must be greater than 0"
            });
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'Projects/ProjectPlanningPurchaseOrder/PoMaterialCreate/',
                    //data: { 'projectplanningPurchaseOrder': $scope.projectplanningPurchaseOrder, 'projectPlanningPORequisitionMaterial': $scope.ProjectPlanningPurchaseOrderDetailSelectedList.length > 0 ? $scope.ProjectPlanningPurchaseOrderDetailSelectedList : $scope.projectPlaningPORequisitionMaterialListForSave },
                    data: { 'projectplanningPurchaseOrder': $scope.projectplanningPurchaseOrderNew, 'projectPlanningPORequisitionMaterial': $scope.ProjectPlanningPOMaterialEditSaveList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure', 'ProjectPlanningDetailEditPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getProjectPlanningPurchaseOrderById(response.data.ProjectPlanningPurchaseOrderId);
                        getPOMaterialMasterSavedList();
                        //$scope.ProjectPlanningPurchaseOrderDetailSelectedList = [];
                        //$scope.projectPlaningPORequisitionMaterialListForSave = [];
                        $scope.projectPlanningDetailFormCloseListPopUp();
                        angular.element(document.querySelector('#ProjectPlanningDetailEditPopUp')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ProjectPlanningDetailEditPopUp');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.projectplanningPurchaseOrder,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure', 'ProjectPlanningDetailEditPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.projectplanningPurchaseOrders[$scope.index] = $scope.projectplanningPurchaseOrder;
                            $scope.projectplanningPurchaseOrders = $filter('orderBy')($scope.projectplanningPurchaseOrders, 'Sequence');
                            getProjectPlanningPurchaseOrderById(response.data.ProjectPlanningPurchaseOrderId);
                            getPOMaterialMasterSavedList();
                            //$scope.ProjectPlanningPurchaseOrderDetailSelectedList = [];
                            //$scope.projectPlaningPORequisitionMaterialListForSave = [];
                            $scope.projectPlanningDetailFormCloseListPopUp();
                        }
                        angular.element(document.querySelector('#ProjectPlanningDetailEditPopUp')).modal('hide');
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ProjectPlanningDetailEditPopUp');
                });
            }
        } catch (e) {
            ShowResult(e, 'Error', 'ProjectPlanningDetailEditPopUp');
        }
    }

    $scope.showVendorModal = function () {
        $scope.getVendorData();
        angular.element(document.querySelector('#vendormodal')).modal('show');
    };
    $scope.getVendorCode = function (id, username) {
        $scope.projectplanningPurchaseOrderNew.PartyId = id;
        $scope.projectplanningPurchaseOrderNew.Vendor = username;
        angular.element(document.querySelector('#vendormodal')).modal('hide');
    };

    //function checkValidation() {
    //    try {
    //        var poMM = $scope.ProjectPlaningPOMaterialListForSave;
    //        for (var i = 0; i < poMM.length; i++) {
    //            if (poMM[i].Quantity < 1 || poMM[i].Quantity == null) {
    //                throw 'Quantity can not be less than 1!!'
    //            }
    //            if ((poMM[i].ReverseTotalQuantity - poMM[i].TempConvertQuantity + poMM[i].ReverseQuantity) > poMM[i].PlanningQuantity || poMM[i].Quantity == null) {
    //                throw 'Quantity can not be more than planning quantity!!'
    //            }
    //            //if (poMM[i].PlanningQuantity - (poMM[i].ReverseTotalQuantity - poMM[i].TempConvertQuantity + poMM[i].ReverseQuantity) > poMM[i].PlanningQuantity || poMM[i].Quantity == null) {
    //            //    throw 'Quantity can not be more than planning quantity!!'
    //            //}
    //            else if (poMM[i].Rate < 0 || poMM[i].Rate == null) {
    //                throw 'Rate can not be 0!!'
    //            }
    //            else if (poMM[i].AlternativeUomId === null) {
    //                throw 'UOM can not be empty!!'
    //            }
    //        }
    //    } catch (e) {
    //        throw e;
    //    }
    //}

    $scope.convertedUOMQuantity = 0
    $scope.convertMaterialUOMQuantity = function (fromUOMId, toUOMId, Quantity) {
        $http({
            method: 'GET',
            url: 'Setups/UOMConversion/GetUOMValueConvert?fromUOMId=' + fromUOMId + '&toUOMId=' + toUOMId + '&quantity=' + Quantity,
        }).then(function successCallback(response) {
            $scope.convertedUOMQuantity = response.data[0].ReverseQuantity
        });
    }

    //****************MaterialMasterArticle************/
    $scope.valuePassInPOArticleMasterModal = function (index, data) {
        $scope.articleIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete this data....';
        angular.element(document.querySelector('#confirmgenericPOArticleDetailSelectedItem')).modal('show');
    };
    $scope.confirmgenericPOArticleDetailSelectedItem = function () {
        $scope.PPRequisitionMaterialArticleListForSave.splice($scope.articleIndex, 1);
    }
    $scope.materialMasterArticleAddFormPopup = function (data) {
        $scope.articleMaterialMasterId = data.MaterialMasterId;
        $scope.ProjectPlanningPORequisitionMaterialMasterId = data.Id;
        $scope.ProjectPlanningRequisitionMaterialMasterId = data.ProjectPlanningRequsitionMaterialMasterId;
        $scope.PoRequisitionUoM = data.PoRequisitionUoM;
        $scope.setSingleRow = [];
        $scope.PPRequisitionMaterialArticleListForSave = [];
        getCheckAttribute();
    }
    function getCheckAttribute() {
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/getmaterialmasterattributelist?materialMasterId=' + $scope.articleMaterialMasterId,
        }).then(function successCallback(response) {
            if (response.data.length === 0) {
                return ShowResult("This material has no attribute", 'failure');
            } else {
                getMaterialMasterArticleSavedData();
                angular.element(document.querySelector('#materialMasterArticleFormModal')).modal('show');
            }
        })
    }
    $scope.materialMasterArticleAddFormPopupClose = function () {
        angular.element(document.querySelector('#materialMasterArticleFormModal')).modal('hide');
    }
    function getMaterialMasterArticleSavedData() {
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanningPurchaseOrder/GetProjectplanningPORequisitionMaterialMasterArticleSavedList?projectPlanningPORequisitionMaterialMasterId=' + $scope.ProjectPlanningPORequisitionMaterialMasterId + '&projectPlanningRequisitionId=' + $scope.projectplanningPurchaseOrderNew.ProjectPlanningRequisitionId,
        }).then(function successCallback(response) {
            $scope.PPRequisitionMaterialArticleListForSave = [];
            getArticleSaveValue(response.data);
        })
    }
    //---------
    //***************MaterialMaster Article Search Popup*********/
    $scope.materialMasterArticleSearchPopup = function () {
        //getMaterialAttributeValue();
        getArticle();
        angular.element(document.querySelector('#materialMasterArticleSearchPopup')).modal('show');
    }

    $scope.articleHead = [];
    $scope.articleList = [];
    $scope.materattributeValueDdlList = [];
    //function getMaterialAttributeValue() {
    //    $http({
    //        method: 'GET',
    //        url: 'Projects/ProjectPlanningRequisition/GetMaterialMasterAttributeValueList?materialMasterId=' + $scope.articleMaterialMasterId,
    //    }).then(function successCallback(response) {
    //        $scope.materattributeValueDdlList = response.data;
    //    })
    //}
    function makeArticleDropDownList(list, headingName, materialAttributeId, list2) {
        var valueListName = headingName.replace(/\s/g, '');
        var modelName = headingName.replace(/\s/g, '');
        $scope[modelName] = [];
        valueListName = valueListName + 'List'
        $scope[valueListName] = [];
        angular.forEach(list, function (item, i) {
            if (item.MaterialAttributeId === materialAttributeId)
                $scope[valueListName].push({
                    Value: baseService.pk(),
                    Text: item.Text
                });
        });
        list2.push({
            modelName: $scope[modelName],
            valueListName: $scope[valueListName],
            labelName: headingName
        });
        //createDDlModel();
    }

    function createdddlforattribute() {
        $scope.list = [];
        angular.forEach($scope.articleHead, function (item) {
            makeArticleDropDownList($scope.materattributeValueDdlList, item.MaterialAttributeName, item.MaterialAttributeId, $scope.list);
        });
    }
    //function createDDlModel() {
    //    angular.forEach($scope.list, function (item, i) {
    //        $scope[item.modelName] = [];
    //    });
    //}
    //function getAttribute() {
    //    $scope.attributeList = [];
    //    $http({
    //        method: 'GET',
    //        url: 'Materials/materialmaster/getmaterialmasterattributelist?materialMasterId=' + $scope.articleMaterialMasterId,
    //    }).then(function successCallback(response) {
    //        $scope.attributeList = response.data;
    //        if (baseService.arrayLength(response.data) == 0)
    //            return ShowResult('This material has no attribute', 'failure');
    //        for (var i = 0; i < $scope.attributeList.length; i++) {
    //            $scope.searchFreeField = $scope.attributeList[i].MaterialAttributeValueFreeText !== null ? true : false;
    //            var isFree = $scope.attributeList[i].IsFreeField;
    //            $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    //        }
    //    })
    //}
    function getArticleSaveValue(list) {
        $scope.articleHead = [];
        $scope.articleList = [];
        if (list.length > 0) {
            $http({
                method: 'GET',
                url: 'Materials/materialmasterarticle/GetArticleValueList?materialMasterId=' + $scope.articleMaterialMasterId,
                contentType: "application/json; charset=utf-8",
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data)) {
                    $scope.valueData = response.data
                    var valueData = response.data
                    $http({
                        method: 'GET',
                        url: 'Materials/materialmasterarticle/getarticlvaluehead?materialMasterId=' + $scope.articleMaterialMasterId,
                        contentType: "application/json; charset=utf-8",
                    }).then(function successCallback(response) {
                        $scope.articleHead = response.data;
                        if (baseService.arrayLength($scope.articleHead)) {
                            for (var i = 0; i < list.length; i++) {
                                list[i].MaterialMasterArticleValues = [];
                                for (var a = 0; a < $scope.articleHead.length; a++) {
                                    list[i].MaterialMasterArticleValues.push({
                                        Id: null
                                        , MaterialMasterId: null
                                        , MaterialMasterAttributeId: null
                                        , MaterialAttributeId: $scope.articleHead[a].MaterialAttributeId
                                        , MaterialAttributeName: $scope.articleHead[a].MaterialAttributeName
                                        , MaterialMasterArticleId: null
                                        , MaterialAttributeValueId: null
                                        , MaterialMasterAttributeValueId: null
                                        , MaterialAttributeValueFreeText: null
                                    });
                                }
                            }
                        }
                        for (var t = 0; t < baseService.arrayLength(list); t++) {
                            var articleRow = Object.assign({}, list[t]);
                            checkValueSubMaterialSavedId(valueData, articleRow);
                            $scope.articleList.push(articleRow);
                        }
                        getSavedSingleRow();
                    })
                }
            })
        }
    }
    function checkValueSubMaterialSavedId(valueData, articleRow) {
        for (var v = 0; v < baseService.arrayLength(articleRow.MaterialMasterArticleValues); v++) {
            var valueRow = articleRow.MaterialMasterArticleValues[v];
            for (var tt = 0; tt < baseService.arrayLength(valueData); tt++) {
                if (articleRow.PPReuisitionArticleId === valueData[tt].MaterialMasterArticleId
                    && valueRow.MaterialAttributeId === valueData[tt].MaterialAttributeId) {
                    var newValue = valueData[tt];
                    valueRow.Id = newValue.Id;
                    valueRow.MaterialMasterId = newValue.MaterialMasterId;
                    valueRow.MaterialMasterAttributeId = newValue.MaterialMasterAttributeId;
                    valueRow.MaterialAttributeId = newValue.MaterialAttributeId;
                    valueRow.MaterialAttributeName = newValue.MaterialAttributeName;
                    valueRow.MaterialMasterArticleId = newValue.MaterialMasterArticleId;
                    valueRow.MaterialAttributeValueId = newValue.MaterialAttributeValueId;
                    valueRow.MaterialMasterAttributeValueId = newValue.MaterialMasterAttributeValueId;
                    valueRow.MaterialAttributeValueFreeText = newValue.MaterialAttributeValueFreeText;
                    break;
                }
            }
        }
    }
    function getSavedSingleRow() {
        for (var t = 0; t < baseService.arrayLength($scope.articleList); t++) {
            var at = $scope.articleList[t];
            var ob = {};
            ob.MaterialMasterArticleId = at.MaterialMasterArticleId;
            ob.Id = at.Id;
            ob.ProjectPlanningRequisitionMaterialMasterId = at.ProjectPlanningRequisitionMaterialMasterId;
            ob.ProjectPlanningPORequisitionMaterialMasterId = at.ProjectPlanningPORequisitionMaterialMasterId;
            ob.PPlanningRequisitionMaterialMasterArticleId = at.PPlanningRequisitionMaterialMasterArticleId;
            ob.ProjectPlanningPurchaseOrderId = at.ProjectPlanningPurchaseOrderId;
            ob.Code = at.Code;
            ob.ShortName = at.ShortName;
            ob.StandardName = at.StandardName;
            ob.DbQuantity = at.Quantity;
            ob.Quantity = at.Quantity;
            ob.RequisitionQuantity = at.RequisitionQuantity;
            ob.RaisedQuantity = at.RaisedQuantity;
            ob.Rate = at.Rate;
            ob.PoRequisitionUoM = at.PoRequisitionUoM;
            angular.forEach($scope.articleHead, function (item) {
                ob[item.MaterialAttributeName] = getV(at.MaterialMasterArticleValues, at.PPReuisitionArticleId, item.MaterialAttributeId);
            });
            $scope.PPRequisitionMaterialArticleListForSave.push(ob);
        }
    }
    function getArticle() {
        $scope.articleHead = [];
        $scope.articleList = [];
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanningRequisition/ProjectPlanningRequisitionMaterialMasterArticleSavedListForPO?requisitionMaterialMasterId=' + $scope.ProjectPlanningRequisitionMaterialMasterId + '&ProjectPlanningRequisitionId=' + $scope.projectplanningPurchaseOrderNew.ProjectPlanningRequisitionId,
            contentType: "application/json; charset=utf-8",
        }).then(function successCallback(response) {
            $scope.articles = response.data;
            var articles = response.data;
            if (articles.length > 0) {
                $http({
                    method: 'GET',
                    url: 'Materials/materialmasterarticle/GetArticleValueList?materialMasterId=' + $scope.articleMaterialMasterId,
                    contentType: "application/json; charset=utf-8",
                }).then(function successCallback(response) {
                    if (baseService.arrayLength(response.data)) {
                        $scope.valueData = response.data
                        var valueData = response.data
                        $http({
                            method: 'GET',
                            url: 'Materials/materialmasterarticle/getarticlvaluehead?materialMasterId=' + $scope.articleMaterialMasterId,
                            contentType: "application/json; charset=utf-8",
                        }).then(function successCallback(response) {
                            $scope.articleHead = response.data;
                            if (baseService.arrayLength($scope.articleHead)) {
                                for (var i = 0; i < articles.length; i++) {
                                    articles[i].MaterialMasterArticleValues = [];
                                    for (var a = 0; a < $scope.articleHead.length; a++) {
                                        articles[i].MaterialMasterArticleValues.push({
                                            Id: null
                                            , MaterialMasterId: null
                                            , MaterialMasterAttributeId: null
                                            , MaterialAttributeId: $scope.articleHead[a].MaterialAttributeId
                                            , MaterialAttributeName: $scope.articleHead[a].MaterialAttributeName
                                            , MaterialMasterArticleId: null
                                            , MaterialAttributeValueId: null
                                            , MaterialMasterAttributeValueId: null
                                            , MaterialAttributeValueFreeText: null
                                        });
                                    }
                                }
                            }
                            for (var t = 0; t < baseService.arrayLength(articles); t++) {
                                var articleRow = Object.assign({}, articles[t]);
                                checkValueSubMaterialId(valueData, articleRow);
                                $scope.articleList.push(articleRow);
                            }
                            createdddlforattribute();
                            getSingleRow();
                        })
                    }
                })
            }
        });
    }
    function getSingleRow() {
        $scope.setSingleRow = [];
        for (var t = 0; t < baseService.arrayLength($scope.articleList); t++) {
            var at = $scope.articleList[t];
            var ob = {};
            ob.MaterialMasterArticleId = at.PPReuisitionArticleId;
            ob.Id = at.Id;
            ob.Code = at.Code;
            ob.ShortName = at.ShortName;
            ob.StandardName = at.StandardName;
            ob.RequisitionQuantity = at.Quantity;
            ob.RaisedQuantity = at.RaisedQuantity;
            ob.PoRequisitionUoM = at.PoRequisitionUoM;
            angular.forEach($scope.articleHead, function (item) {
                ob[item.MaterialAttributeName] = getV(at.MaterialMasterArticleValues, at.PPReuisitionArticleId, item.MaterialAttributeId);
            });
            $scope.setSingleRow.push(ob);
        }
    }
    function getV(list, articleId, MaterialAttributeId) {
        for (var i = 0; i < list.length; i++) {
            var item = list[i];
            if (item.MaterialMasterArticleId === articleId && item.MaterialAttributeId === MaterialAttributeId) {
                return item.MaterialAttributeValueFreeText;
                break;
            }
        }
        return null;
    }
    function checkValueSubMaterialId(valueData, articleRow) {
        for (var v = 0; v < baseService.arrayLength(articleRow.MaterialMasterArticleValues); v++) {
            var valueRow = articleRow.MaterialMasterArticleValues[v];
            for (var tt = 0; tt < baseService.arrayLength(valueData); tt++) {
                if (articleRow.PPReuisitionArticleId === valueData[tt].MaterialMasterArticleId
                    && valueRow.MaterialAttributeId === valueData[tt].MaterialAttributeId) {
                    var newValue = valueData[tt];
                    valueRow.Id = newValue.Id;
                    valueRow.MaterialMasterId = newValue.MaterialMasterId;
                    valueRow.MaterialMasterAttributeId = newValue.MaterialMasterAttributeId;
                    valueRow.MaterialAttributeId = newValue.MaterialAttributeId;
                    valueRow.MaterialAttributeName = newValue.MaterialAttributeName;
                    valueRow.MaterialMasterArticleId = newValue.MaterialMasterArticleId;
                    valueRow.MaterialAttributeValueId = newValue.MaterialAttributeValueId;
                    valueRow.MaterialMasterAttributeValueId = newValue.MaterialMasterAttributeValueId;
                    valueRow.MaterialAttributeValueFreeText = newValue.MaterialAttributeValueFreeText;
                    break;
                }
            }
        }
    }
    //function checkSeelectc(articleRow) {
    //    for (var x = 0; x < articleRow.MaterialMasterArticleValues.length; x++) {
    //        var valueRow = articleRow.MaterialMasterArticleValues[x];
    //        for (var i = 0; i < $scope.selectedAaa.length; i++) {
    //            if (valueRow.MaterialAttributeValueFreeText === $scope.selectedAaa[i]) {
    //                return true;
    //                break;
    //            }
    //        }
    //        return false;
    //    }
    //}
    $scope.materialMasterArticleSearchModalCloseListPopUp = function () {
        PPMaterialArticleListSave();
        angular.element(document.querySelector('#materialMasterArticleSearchPopup')).modal('hide');
    }
    $scope.PPRequisitionMaterialArticleListForSave = [];
    function PPMaterialArticleListSave() {
        angular.forEach($scope.setSingleRow, function (item) {
            if (item.Flag) {
                if (checkMaterialMasterArticleExist($scope.PPRequisitionMaterialArticleListForSave, item.Id) === false) {
                    item.PPlanningRequisitionMaterialMasterArticleId = item.Id;
                    item.Id = null;
                    item.ProjectPlanningRequisitionMaterialMasterId = $scope.ProjectPlanningRequisitionMaterialMasterId;
                    item.ProjectPlanningPurchaseOrderId = $scope.projectplanningPurchaseOrderNew.Id;
                    item.ProjectPlanningPORequisitionMaterialMasterId = $scope.ProjectPlanningPORequisitionMaterialMasterId;
                    item.Quantity = null;
                    item.Rate = null;
                    item.PoRequisitionUoM = $scope.PoRequisitionUoM;
                    $scope.PPRequisitionMaterialArticleListForSave.push(item);
                }
            }
        })
        console.log('$scope.PPRequisitionMaterialArticleListForSave', $scope.PPRequisitionMaterialArticleListForSave)
    }
    function checkMaterialMasterArticleExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProjectPlanningPORequisitionMaterialMasterId === id) {
                return true;
            }
        }
        return false;
    }
    function checkRMMAValidation() {
        angular.forEach($scope.PPRequisitionMaterialArticleListForSave, function (item) {
            if (parseInt(item.Quantity) <= 0 || item.Quantity === null) {
                throw item.StandardName + " quantity must be greater than 0";
            }
            if (parseInt(item.Rate) <= 0 || item.Rate === null) {
                throw item.StandardName + " rate must be greater than 0";
            }
        });
    }
    $scope.ProjectPlanningRequisitionMaterialMasterAticleSave = function () {
        angular.copy($scope.projectPlanningRequisitionNew, $scope.projectPlanningRequisition);
        try {
            checkRMMAValidation();
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'Projects/projectplanningpurchaseorder/PoArticleCreate?poMaterialMasterId=' + $scope.ProjectPlanningPORequisitionMaterialMasterId,
                    data: { 'requisitionArticleList': $scope.PPRequisitionMaterialArticleListForSave },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure', 'materialMasterArticleFormModal');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getPOMaterialMasterSavedList();
                        $scope.materialMasterArticleAddFormPopupClose();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'materialMasterArticleFormModal');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.projectplanningRequisition,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure', 'materialMasterArticleFormModal');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.projectplanningRequisitions[$scope.index] = $scope.projectplanningRequisition;
                            $scope.projectplanningRequisitions = $filter('orderBy')($scope.projectplanningRequisitions, 'Sequence');
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'materialMasterArticleFormModal');
                });
            }
        } catch (e) {
            ShowResult(e, 'failure', 'materialMasterArticleFormModal');
        }
    }
    //---------
    //***************************** end **************************//
    $scope.ProjectPlanningPurchaseOrderDetail = {
        Id: null,
        ProjectPlanningPurchaseOrderId: null,
        ProjectPlanningDetailId: null,
        BaseUOMId: null,
        Quantity: null,
        Rate: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    }

    $('.datepicker').datepicker({
        forceParse: false,
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });
    $scope.projectplanningPurchaseOrderNew = Object.assign({}, $scope.projectplanningPurchaseOrder);
    function getProjectPlanningPurchaseOrderById(Id) {
        $http({
            method: 'GET',
            url: 'Projects/projectplanningpurchaseorder/GetProjectPlanningPurchaseOrderById?id=' + Id,
        }).then(function successCallback(response) {
            if (response.data.Rows.length > 0) {
                $scope.getProjectPlanningInfoOnChange(response.data.Rows[0].ProjectPlanningId);
                $scope.projectplanningPurchaseOrderNew = response.data.Rows[0];
                $scope.projectplanningPurchaseOrderNew.PoDate = $filter('dateFiltering')(response.data.Rows[0].PoDate);
                //$scope.poExchangeRate = $scope.poExchangeRate != null ? $scope.poExchangeRate : $scope.projectplanningPurchaseOrderNew.ExchangeRate;
            }
            calculateTotalQuantity();
            calculateTotalAmount();
            $scope.getProjectPlanningPurchaseOrderDetail();
        })
    }
    /***Cbo***************/
    function CurrencyList() {
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.CurrencyList = result;
        });
    };
    CurrencyList();
    //*************Project Planning Detail************/
    $scope.projectPlanningMaterialMasterSearchPopup = function () {
        //$scope.getUomList();
        $scope.getMaterailMasterData();
        angular.element(document.querySelector('#projectPlanningRequisitionPopUp')).modal('show');
    };
    //getProjectPlanningPurchaseOrderDetail***********/
    $scope.ProjectPlanningPurchaseOrderDetailSavedList = [];
    $scope.getProjectPlanningPurchaseOrderDetail = function () {
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanningPurchaseOrder/GetProjectPlanningPurchaseOrderDetail?projectPlanningPurchaseOrderId=' + $scope.projectplanningPurchaseOrderNew.Id,
        }).then(function successCallback(response) {
            $scope.ProjectPlanningPurchaseOrderDetailSavedList = response.data;
            //calculatePODetailTotal();
            console.log('ProjectPlanningPurchaseOrderDetailSavedList', $scope.ProjectPlanningPurchaseOrderDetailSavedList);
        })
    }
    //*************ProjectPlanningMaster************/
    //$scope.searchbyMachineTypeMasterList = [
    //    {
    //        'name': 'User Name',
    //        'value': 'UserName'
    //    },
    //    {
    //        'name': 'Code',
    //        'value': 'Code'
    //    },
    //    {
    //        'name': 'Machine ClassName',
    //        'value': 'MachineClassName'
    //    }
    //]
    //$scope.projectplanningMasterListParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: 'asc',
    //    sort: 'UserName',
    //    searchBy: "UserName",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};
    //$scope.getProjectPlanningMasterData = function () {
    //    $scope.projectplanningMasterList = [];
    //    baseService.setCurrentPage('projectplanningMasterList');
    //    $scope.loadProjectPlanningMasterData = function (pageno) {
    //        //baseService.paginationBase('Projects/projectplanningmaster/getlistfordynamicpopup', pageno, $scope.projectplanningMasterListParameters)
    //        baseService.paginationBase('Machines/MachineType/GetMachineTypeList', pageno, $scope.projectplanningMasterListParameters)
    //            .then(function (result) {
    //                for (var i = 0; i < result.Rows.length; i++) {
    //                    result.Rows[i].ProjectPlanningPurchaseOrderDetailId = $scope.ProjectPlanningPurchaseOrderDetailId;
    //                }
    //                $scope.projectplanningMasterList = result.Rows;
    //                console.log('new', $scope.projectplanningMasterList);
    //                $scope.projectplanningMasterListParameters.total_count = result.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    }; $scope.loadProjectPlanningMasterData();
    //};
    //$scope.projectplanningMasterSearchPopup = function () {
    //    $scope.getProjectPlanningMasterData();
    //    angular.element(document.querySelector('#projectplanningMasterModal')).modal('show');
    //};
    //function checkProjectPlanningMasterExist(list, id) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].MachineTypeId === id) {
    //            return true;
    //        } else {
    //            return false;
    //        }
    //    }
    //    return false;
    //}
    //$scope.ProjectPlaningProjectPlanningListForSave = [];
    //function ProjectPlaningProjectPlanningListSave() {
    //    angular.forEach($scope.projectplanningMasterList, function (item) {
    //        if (item.Flag) {
    //            if (checkProjectPlanningMasterExist($scope.ProjectPlaningProjectPlanningListForSave, item.Id) === false) {
    //                $scope.ProjectPlaningProjectPlanningListForSave.push(
    //                    {
    //                        //ProjectPlanningMasterId: item.Id,
    //                        Id: null,
    //                        ProjectPlanningPurchaseOrderId: item.ProjectPlanningPurchaseOrderId,
    //                        ProjectPlanningPurchaseOrderDetailId: item.ProjectPlanningPurchaseOrderDetailId,
    //                        UserName: item.UserName,
    //                        Code: item.Code,
    //                        MachineClassName: item.MachineClassName,
    //                        MachineTypeId: item.Id,
    //                        MachineTypeName: item.UserName,
    //                        Quantity: null,
    //                    }
    //                );
    //            }

    //        }
    //    })
    //}
    //$scope.projectplanningMasterModalCloseListPopUp = function () {
    //    ProjectPlaningProjectPlanningListSave();
    //    calculateTotalQuantity();
    //    calculateTotalAmount();
    //    console.log('projectplanningMasterList', $scope.projectplanningMasterList);
    //    //$scope.ProjectPlaningProjectPlanning.ProjectPlanningMasterId = data.Id;
    //    angular.element(document.querySelector('#projectplanningMasterModal')).modal('hide');
    //}

    /*****Vendor***************/
    $scope.vendorData = [];
    $scope.searchbyVendorlist = [];
    $scope.vendorListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Description',
        searchBy: "Description",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getVendorData = function () {
        baseService.setCurrentPage('vendorData');
        $scope.loadVendorData = function (pageno) {//loadProcessData
            baseService.paginationBase('Parties/party/getvendorlist', pageno, $scope.vendorListParameters)
                .then(function (result) {
                    $scope.vendorData = result.Rows;
                    $scope.vendorListParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.searchbyVendorlist) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyVendorlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadVendorData();
    };
    //*************MaterialMaster************/
    $scope.searchbyMaterailMasterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'BaseUom',
            'value': 'BaseUom'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMaster'
        },
    ]
    $scope.materialMasterListParameters = {
        limit: 5,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 5,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getMaterailMasterData = function () {
        $scope.materialMasterList = [];
        baseService.setCurrentPage('projectPlanningMaterialMasterList');
        $scope.loadMaterialMasterData = function (pageno) {
            baseService.paginationBase('Projects/projectplanning/GetProjectplanningMaterialMasterSavedList?projectPlanningDetailId=' + $scope.ProjectPlanningDetailId, pageno, $scope.materialMasterListParameters)
                .then(function (result) {
                    for (var i = 0; i < result.Rows.length; i++) {
                        result.Rows[i].ProjectPlanningPurchaseOrderDetailId = $scope.ProjectPlanningPurchaseOrderDetailId;
                    }
                    $scope.projectPlanningMaterialMasterList = result.Rows;
                    $scope.materialMasterListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMaterialMasterData();
    };
    //$scope.projectPlanningMaterialMasterSearchPopup = function () {
    //    $scope.getUomList();
    //    $scope.getMaterailMasterData();
    //    angular.element(document.querySelector('#MaterialMasterModal')).modal('show');
    //};

    //----------------
    $scope.ProjectPlaningPOMaterialListForSave = [];
    function ProjectPlaningMaterialListSave() {
        angular.forEach($scope.projectPlanningMaterialMasterList, function (item) {
            if (item.Flag) {
                if (checkMaterialMasterExist($scope.ProjectPlaningPOMaterialListForSave, item.Id) === false) {
                    $scope.ProjectPlaningPOMaterialListForSave.push(
                        {
                            Id: null,
                            ProjectPlanningMaterialMasterId: item.Id,
                            MaterialMasterId: item.MaterialMasterId,
                            ProjectPlanningPurchaseOrderId: $scope.projectplanningPurchaseOrderNew.Id,
                            ProjectPlanningPurchaseOrderDetailId: item.ProjectPlanningPurchaseOrderDetailId,
                            Code: item.Code,
                            UserName: item.UserName,
                            PlanningCurrencyId: $scope.projectPlanningInfo.CurrencyId,
                            PlanningCurrencyName: $scope.projectPlanningInfo.CurrencyName,
                            //Title: $scope.projectPlanningInfo.RequisitionTitle,
                            BaseUom: item.BaseUom,
                            BaseUOMId: item.BaseUOMId,
                            PlanningUOMId: item.PlanningUOMId,
                            PlanningUOM: item.PlanningUOM,
                            alernativeUomLists: buildUomDropDown($scope.alterNativeUomList, item.MaterialMasterId),
                            AlternativeUomId: selectedDDL(buildUomDropDown($scope.alterNativeUomList, item.MaterialMasterId)),
                            PlanningQuantity: item.Quantity,
                            ReverseQuantity: 0,
                            ReverseTotalQuantity: item.ReverseTotalQuantity === null ? 0 : item.ReverseTotalQuantity,
                            //Quantity: null,
                            Rate: null,
                            Amount: null
                        });
                }
            }
        })
    }
    function selectedDDL(list) {
        try {
            var uomId = null;
            for (var i = 0; i < list.length; i++) {
                if (list[i].IsPo) {
                    uomId = list[i].Value;
                    return uomId;
                }
            }
            return uomId;
        } catch (e) {
        }
    }

    $scope.materialMasterModalCloseListPopUp = function () {
        ProjectPlaningMaterialListSave();
        //$scope.ProjectPlaningFixedAsset.FixedAssetMasterId = data.Id;
        console.log('$scope.ProjectPlaningPOMaterialListForSave', $scope.ProjectPlaningPOMaterialListForSave)
        angular.element(document.querySelector('#MaterialMasterModal')).modal('hide');
    }

    //------------------
    //*******Material MasterForm*****************//
    var finalUomDropDownList = [];
    function buildUomDropDown(list, id) {
        finalUomDropDownList = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                if (finalUomDropDownList.length > 0) {
                    if (checkExistUOM(list[i].UoMID) === false) {
                        finalUomDropDownList.push({
                            Text: list[i].UoM,
                            Value: list[i].UoMID,
                            Id: list[i].Id,
                            IsPo: setPo(list, list[i].Id, list[i].UoMID),
                        });
                    }
                } else {
                    finalUomDropDownList.push({
                        Text: list[i].UoM,
                        Value: list[i].UoMID,
                        Id: list[i].Id,
                        IsPo: setPo(list, list[i].Id, list[i].UoMID),
                    });
                }
            }
        }

        return finalUomDropDownList;
    }
    function checkExistUOM(uomId) {
        for (var i = 0; i < finalUomDropDownList.length; i++) {
            if (finalUomDropDownList[i].Value === uomId) {
                return true;
                break;
            }
        }
        return false;
    }
    function setPo(list, id, uomId) {
        try {
            var hasValue = false;
            for (var i = 0; i < list.length; i++) {
                if (list[i].Id === id && list[i].UoMID === uomId && list[i].IsPo) {
                    hasValue = true;
                }
            }
            return hasValue;
        } catch (e) {
        }
    }
    //$scope.materialMasterFormSearchPopup = function (data) {
    //    $scope.ProjectPlanningDetailId = data.ProjectPlanningDetailId;
    //    $scope.ProjectPlanningPurchaseOrderDetailId = data.Id;
    //    $scope.projectplanningConvertedUOmQuantity = [];
    //    $scope.getmaterialMasterSavedList();

    //    $scope.getCurrencyName();
    //    angular.element(document.querySelector('#materialMasterFormModal')).modal('show');
    //};

    //$scope.getmaterialMasterSavedList = function () {
    //    $scope.getUomList();
    //    $scope.materialMasterList = [];
    //    $http({
    //        method: 'GET',
    //        url: 'Projects/projectplanningpurchaseorder/GetProjectplanningPOMaterialMasterSavedList?projectPlanningPODetailId=' + $scope.ProjectPlanningPurchaseOrderDetailId,
    //    }).then(function successCallback(response) {
    //        $scope.ProjectPlaningPOMaterialListForSave = response.data.Rows;
    //        for (var i = 0; i < $scope.ProjectPlaningPOMaterialListForSave.length; i++) {
    //            $scope.ProjectPlaningPOMaterialListForSave[i].TempConvertQuantity = $scope.ProjectPlaningPOMaterialListForSave[i].ReverseQuantity;
    //            $scope.ProjectPlaningPOMaterialListForSave[i].alernativeUomLists = buildUomDropDown($scope.alterNativeUomList, $scope.ProjectPlaningPOMaterialListForSave[i].MaterialMasterId);
    //            $scope.ProjectPlaningPOMaterialListForSave[i].PlanningCurrencyId = $scope.projectPlanningInfo.CurrencyId;
    //            $scope.ProjectPlaningPOMaterialListForSave[i].PlanningCurrencyName = $scope.projectPlanningInfo.CurrencyName;
    //            //forAddingQuantityCount
    //            $scope.convertUOMQuantity(i, $scope.ProjectPlaningPOMaterialListForSave[i].PlanningUOMId, $scope.ProjectPlaningPOMaterialListForSave[i].AlternativeUomId, $scope.ProjectPlaningPOMaterialListForSave[i].Quantity)
    //        }
    //        calculateTotalQuantity();
    //        calculateTotalAmount();
    //        console.log('ProjectPlaningPOMaterialListForSave', $scope.ProjectPlaningPOMaterialListForSave);
    //    })
    //};
    //$scope.materialMasterFormModalCloseListPopUp = function () {
    //    angular.element(document.querySelector('#materialMasterFormModal')).modal('hide');
    //}
    $scope.getTotalResult = function () {
        calculateTotalQuantity();
        calculateTotalAmount();
    }
    $scope.RaisedQuantity = 0;
    function calculateTotalQuantity() {
        $scope.totalQuantity = 0;
        for (var i = 0; i < $scope.ProjectPlaningPOMaterialListForSave.length; i++) {
            $scope.totalQuantity += parseInt($scope.ProjectPlaningPOMaterialListForSave[i].Quantity);
            $scope.RaisedQuantity += parseInt($scope.ProjectPlaningPOMaterialListForSave[i].PlanningQuantity - $scope.ProjectPlaningPOMaterialListForSave[i].Quantity);
        }
    }
    function calculateTotalAmount() {
        $scope.totalAmount = 0;
        $scope.totalBdtAmount = 0;
        for (var i = 0; i < $scope.ProjectPlaningPOMaterialListForSave.length; i++) {
            $scope.totalAmount += $scope.ProjectPlaningPOMaterialListForSave[i].Quantity * ($scope.ProjectPlaningPOMaterialListForSave[i].Rate * $scope.projectplanningPurchaseOrderNew.ExchangeRate);
            $scope.totalBdtAmount += $scope.ProjectPlaningPOMaterialListForSave[i].Quantity * ($scope.ProjectPlaningPOMaterialListForSave[i].Rate);
        }
    }
    //function calculatePODetailTotal() {
    //    $scope.totalPODQuantity = 0;
    //    $scope.totalPODAmount = 0;
    //    $scope.totalPLAmount = 0;
    //    for (var i = 0; i < $scope.ProjectPlanningPurchaseOrderDetailSavedList.length; i++) {
    //        $scope.totalPODQuantity += $scope.ProjectPlanningPurchaseOrderDetailSavedList[i].TotalQuantity;
    //        $scope.totalPODAmount += $scope.ProjectPlanningPurchaseOrderDetailSavedList[i].TotalAmount * $scope.projectplanningPurchaseOrderNew.ExchangeRate;
    //        $scope.totalPLAmount += $scope.ProjectPlanningPurchaseOrderDetailSavedList[i].PlanningAmount;
    //    }

    //}
    //$scope.convertMaterialUOMQuantity = function (index, PlanningUOMId, AlternativeUomId, Quantity, checkingBalanceQuantity) {
    //    //var PlanningUOMId = data.PlanningUOMId;
    //    //var AlternativeUomId = data.AlternativeUomId;
    //    //var Quantity = data.Quantity;
    //    //var BalanceQuantity = data.PlanningQuantity - data.ReverseTotalQuantity
    //    //var checkingBalanceQuantity = BalanceQuantity - data.Quantity;

    //    if (checkingBalanceQuantity < 0) {
    //        $scope.ProjectPlaningPOMaterialListForSave[index].Quantity = parseInt(Quantity) - (-checkingBalanceQuantity);
    //        return ShowResult("Po Quantiy can not be greater than planning quantiy ", 'failure', "materialMasterFormModal");
    //    }
    //    if (PlanningUOMId === AlternativeUomId) {
    //        $scope.ProjectPlaningPOMaterialListForSave[index].ReverseQuantity = Quantity;
    //        // $scope.projectplanningConvertedUOmQuantity[index]['ConvertedQuantity'] = Quantity
    //    } else {
    //        $http({
    //            method: 'GET',
    //            url: 'Setups/UOMConversion/GetUOMValueConvert?fromUOMId=' + PlanningUOMId + '&toUOMId=' + AlternativeUomId + '&quantity=' + Quantity,
    //        }).then(function successCallback(response) {
    //            $scope.ProjectPlaningPOMaterialListForSave[index].ReverseQuantity = response.data[0].ReverseQuantity
    //        });
    //    }

    //}

    //#region ************* AssetItemUom***********
    //function getIsExistsgUOM(list, id) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].Value === id) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //var finalAssetItemUomDropDownList = [];
    //function buildAssetItemUomDropDown(list, id) {
    //    finalAssetItemUomDropDownList = [];
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].Id === id) {
    //            if (finalAssetItemUomDropDownList.length > 0) {
    //                if (getIsExistsgUOM(finalAssetItemUomDropDownList, list[i].UoMID) === false) {
    //                    finalAssetItemUomDropDownList.push({
    //                        Text: list[i].UoM,
    //                        Value: list[i].UoMID,
    //                        Id: list[i].Id,
    //                    });
    //                }

    //            } else {
    //                finalAssetItemUomDropDownList.push({
    //                    Text: list[i].UoM,
    //                    Value: list[i].UoMID,
    //                    Id: list[i].Id,
    //                });
    //            }
    //        }
    //    }

    //    return finalAssetItemUomDropDownList;
    //}
    //------------------
    //$scope.machineTypeMasterModalCloseListPopUp = function () {
    //    ProjectPlaningMachineTypeListSave();
    //    console.log('machineTypeMasterList', $scope.machineTypeMasterList);
    //    //$scope.ProjectPlaningFixedAsset.FixedAssetMasterId = data.Id;
    //    angular.element(document.querySelector('#machineTypeMasterModal')).modal('hide');
    //}
    ////------------------
    //*************MachineTypeMasterForm************/
    //$scope.machineTypeMasterFormSearchPopup = function (data) {
    //    $scope.ProjectPlanningPurchaseOrderDetailId = data.Id;
    //    $scope.ProjectPlanningDetailId = data.ProjectPlanningDetailId;
    //    $scope.projectplanningConvertedUOmQuantity = [];
    //    $scope.getMachineTypeMasterSavedList();
    //    angular.element(document.querySelector('#projectPlanningPOMachineTypeMasterFormModal')).modal('show');
    //};
    //$scope.getMachineTypeMasterSavedList = function () {
    //    $scope.getAssetItemUomList();
    //    $http({
    //        method: 'GET',
    //        url: 'Projects/projectplanningpurchaseorder/getprojectplanningPurchaseOrderMachineTypeMaster?projectPlanningPurchaseOrderDetailId=' + $scope.ProjectPlanningPurchaseOrderDetailId,
    //    }).then(function successCallback(response) {
    //        $scope.ProjectPlaningPurchaseOrderMachineTypeListForSave = response.data.Rows;
    //        for (var i = 0; i < $scope.ProjectPlaningPurchaseOrderMachineTypeListForSave.length; i++) {
    //            //forAddingQuantityCount
    //            $scope.convertUOMQuantity(i, $scope.ProjectPlaningPurchaseOrderMachineTypeListForSave[i].MachineTypeUomId, $scope.ProjectPlaningPurchaseOrderMachineTypeListForSave[i].POMachineTypeUomId, $scope.ProjectPlaningPurchaseOrderMachineTypeListForSave[i].Quantity)
    //            $scope.ProjectPlaningPurchaseOrderMachineTypeListForSave[i].AlernativeUomLists = buildAssetItemUomDropDown($scope.alterNativeAssetItemUomList, $scope.ProjectPlaningPurchaseOrderMachineTypeListForSave[i].AssetItemId);
    //        }
    //        console.log($scope.ProjectPlaningPurchaseOrderMachineTypeListForSave);
    //    })
    //}
    //$scope.projectPlanningPOMachineTypeMasterFormModalCloseListPopUp = function () {
    //    angular.element(document.querySelector('#projectPlanningPOMachineTypeMasterFormModal')).modal('hide');
    //}

    //Deleting Rows from BudgetSaveList
    //$scope.valuePassInBudgetSavedDelModal = function (index, Id) {
    //    $scope.ProjectPlanningPoDetailId = Id;
    //    $scope.ProjectPlanningPoDetailIdIndex = index;
    //    if (baseService.isUndefinedOrNull($scope.id))
    //        $scope.message_confirmation = 'Are you sure want to delete this data....';
    //    else
    //        $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.ProjectPlanningPoDetailId + ' ]';
    //    angular.element(document.querySelector('#confirmgenericPODetailPopUp')).modal('show');
    //};

    //$scope.DeletePODetailItem = function () {
    //    for (var i = 0; i < $scope.ProjectPlanningPurchaseOrderDetailSavedList.length; i++) {
    //        if ($scope.ProjectPlanningPurchaseOrderDetailSavedList[i].Id == $scope.ProjectPlanningPoDetailId) {
    //            $http({
    //                method: 'POST',
    //                url: 'Projects/ProjectPlanningPurchaseOrder/DeleteProjectPlanningPurchaseOrderDetail?id=' + $scope.ProjectPlanningPoDetailId,
    //            }).then(function successCallback(response) {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.ProjectPlanningPurchaseOrderDetailSavedList.splice($scope.ProjectPlanningPoDetailIdIndex, 1);
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //        }
    //    }
    //    $scope.ProjectPlanningPoDetailId = null;
    //    $scope.ProjectPlanningPoDetailIdIndex = null;
    //};
    //
    //Deleting Rows from MachineFormList
    //$scope.valuePassInMachineFormDelModal = function (index, Id) {
    //    $scope.MachineTypeId = Id;
    //    $scope.mIndex = index;
    //    if (baseService.isUndefinedOrNull($scope.MachineTypeId))
    //        $scope.message_confirmation = 'Are you sure want to delete this data....';
    //    else
    //        $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.MachineTypeId + ' ]';
    //    angular.element(document.querySelector('#confirmgenericPopUpForMachineForm')).modal('show');
    //};

    //$scope.DeleteMachineSavedItem = function () {
    //    if (baseService.isUndefinedOrNull($scope.MachineTypeId)) {
    //        $scope.ProjectPlaningProjectPlanningListForSave.splice($scope.mIndex, 1);
    //    } else {
    //        for (var i = 0; i < $scope.ProjectPlaningProjectPlanningListForSave.length; i++) {
    //            if ($scope.ProjectPlaningProjectPlanningListForSave[i].Id == $scope.MachineTypeId) {
    //                $http({
    //                    method: 'GET',
    //                    url: 'Projects/ProjectPlanningPurchaseOrder/DeleteProjectPlanningMachineType?id=' + $scope.MachineTypeId,
    //                }).then(function successCallback(response) {
    //                    ShowResult(response.data.Message, 'success');
    //                    $scope.ProjectPlaningPurchaseOrderMachineTypeListForSave.splice($scope.mIndex, 1);
    //                }, function () {
    //                    ShowResult(commonMessage.NetworkError, 'failure');
    //                }).finally(function () {
    //                })
    //            }
    //        }
    //    }
    //    $scope.MachineTypeId = null;
    //    $scope.mIndex = null;
    //};
    //
    //Deleting Rows from MaterialFormList
    //$scope.valuePassInMaterialFormDelModal = function (index, Id) {
    //    $scope.PPPOMaterialMasterId = Id;
    //    $scope.mTIndex = index;
    //    if (baseService.isUndefinedOrNull($scope.PPPOMaterialMasterId))
    //        $scope.message_confirmation = 'Are you sure want to delete this data....';
    //    else
    //        $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.PPPOMaterialMasterId + ' ]';
    //    angular.element(document.querySelector('#confirmgenericPopUpForMaterialForm')).modal('show');
    //};

    //$scope.DeleteMaterialSavedItem = function () {
    //    if (baseService.isUndefinedOrNull($scope.PPPOMaterialMasterId)) {
    //        $scope.ProjectPlaningPOMaterialListForSave.splice($scope.mTIndex, 1);
    //    } else {
    //        for (var i = 0; i < $scope.ProjectPlaningPOMaterialListForSave.length; i++) {
    //            if ($scope.ProjectPlaningPOMaterialListForSave[i].Id == $scope.PPPOMaterialMasterId) {
    //                $http({
    //                    method: 'POST',
    //                    url: 'Projects/ProjectPlanningPurchaseOrder/DeleteProjectPlanningPOMaterial?id=' + $scope.PPPOMaterialMasterId,
    //                }).then(function successCallback(response) {
    //                    ShowResult(response.data.Message, 'success');
    //                    $scope.ProjectPlaningPOMaterialListForSave.splice($scope.mTIndex, 1);
    //                }, function () {
    //                    ShowResult(commonMessage.NetworkError, 'failure');
    //                }).finally(function () {
    //                })
    //            }
    //        }
    //    }
    //    $scope.PPPOMaterialMasterId = null;
    //    $scope.mTIndex = null;
    //};
    //
    $scope.Get = function (id, index) {
        $scope.index = index;
        //$scope.projectplanningPurchaseOrder = $scope.projectplanningPurchaseOrders[$scope.index];
        $scope.projectplanningPurchaseOrderNew = Object.assign({}, $scope.projectplanningPurchaseOrder);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    //$scope.projectplanningConvertedUOmQuantity = [];
    //$scope.convertUOMQuantity = function (index, PlanningUOMId, AlternativeUomId, Quantity,checkingBalanceQuantity) {
    //    if (checkingBalanceQuantity < 0) {
    //        $scope.ProjectPlaningPOMaterialListForSave[index].Quantity = parseInt(Quantity) - (-checkingBalanceQuantity);
    //        return ShowResult("Po Quantiy can not be greater than planning quantiy ", 'failure',"projectPlanningPOMachineTypeMasterFormModal");
    //    }
    //    if (PlanningUOMId === AlternativeUomId) {
    //        $scope.ProjectPlaningPOMaterialListForSave[index].ReverseQuantity = Quantity;
    //        // $scope.projectplanningConvertedUOmQuantity[index]['ConvertedQuantity'] = Quantity
    //    } else {
    //        $http({
    //            method: 'GET',
    //            url: 'Setups/UOMConversion/GetUOMValueConvert?fromUOMId=' + PlanningUOMId + '&toUOMId=' + AlternativeUomId + '&quantity=' + Quantity,
    //        }).then(function successCallback(response) {
    //            //$scope.ProjectPlaningPOMaterialListForSave[index].ReverseQuantity = response.data[0].ReverseQuantity;
    //            $scope.ProjectPlaningPOMaterialListForSave[index].ReverseQuantity = response.data[0].ReverseQuantity
    //        });
    //    }
    //}

    //function checkPOMachineQuantity(list) {
    //    try {
    //        for (var i = 0; i < list.length; i++) {
    //            if (list[i].ReverseQuantity > (list[i].ProjectPlanningMachineTypeQuantity - list[i].ReverseTotalQuantity)) {
    //                throw " Quantity can't be greater than Balance!!"
    //            }
    //        }
    //    } catch (e) {
    //        throw e;
    //    }
    //}

    //$scope.ProjectPlanningPOMaterialMasterSave = function () {
    //    angular.copy($scope.projectplanningPurchaseOrderNew, $scope.projectplanningPurchaseOrder);
    //    try {
    //        $scope.$broadcast('show-errors-check-validity');
    //        if ($scope.projectPlanningPOmaterialMasterForm.$valid) {
    //            checkValidation();
    //            if ($scope.Action == "Save") {
    //                $http({
    //                    method: 'POST',
    //                    url: $scope.saveUrl,
    //                    data: { 'projectplanningPurchaseOrder': $scope.projectplanningPurchaseOrder, 'ProjectPlanningPurchaseOrderDetail': $scope.ProjectPlanningPurchaseOrderDetailSelectedList.length > 0 ? $scope.ProjectPlanningPurchaseOrderDetailSelectedList : $scope.ProjectPlanningPurchaseOrderDetailSavedList, 'projectPlanningPurchaseOrderMaterial': $scope.ProjectPlaningPOMaterialListForSave },
    //                    dataType: 'JSON'
    //                }).then(function successCallback(response) {
    //                    if (response.data.Error == true) {
    //                        ShowResult(response.data.Message, 'failure');
    //                    }
    //                    else {
    //                        ShowResult(response.data.Message, 'success');
    //                        getProjectPlanningPurchaseOrderById(response.data.ProjectPlanningPurchaseOrderId);
    //                        $scope.projectPlanningDetailFormCloseListPopUp();
    //                        $scope.ProjectPlanningPurchaseOrderMaterialSelectedList = [];
    //                        $scope.getProjectPlanningPurchaseOrderDetail();
    //                        $scope.materialMasterFormModalCloseListPopUp();

    //                    }
    //                }), function errorCallBack(response) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //            }
    //            else if ($scope.Action == "Update") {
    //                $http({
    //                    method: 'POST',
    //                    url: $scope.saveUrl,
    //                    data: $scope.projectplanningPurchaseOrder,
    //                    dataType: 'JSON'
    //                }).then(function successCallback(response) {
    //                    if (response.data.Error == true) {
    //                        ShowResult(response.data.Message, 'failure');
    //                    }
    //                    else {
    //                        ShowResult(response.data.Message, 'success');
    //                        if ($scope.index > -1) {
    //                            $scope.projectplanningPurchaseOrders[$scope.index] = $scope.projectplanningPurchaseOrder;
    //                            $scope.projectplanningPurchaseOrders = $filter('orderBy')($scope.projectplanningPurchaseOrders, 'Sequence');
    //                        }
    //                    }
    //                }, function errorCallBack(response) {
    //                    ShowResult(response.data.Message, 'failure');
    //                });
    //            }
    //        }
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}
    //$scope.ProjectPlanningPOMachineTypeSave = function () {
    //    try {
    //        angular.copy($scope.projectplanningPurchaseOrderNew, $scope.projectplanningPurchaseOrder);
    //        checkPOMachineQuantity($scope.ProjectPlaningPurchaseOrderMachineTypeListForSave);
    //        $scope.$broadcast('show-errors-check-validity');
    //        if ($scope.machineTypeMasterForm.$valid) {
    //            if ($scope.Action == "Save") {
    //                $http({
    //                    method: 'POST',
    //                    url: $scope.saveUrl,
    //                    data: { 'projectplanningPurchaseOrder': $scope.projectplanningPurchaseOrder, 'ProjectPlanningPurchaseOrderDetail': $scope.ProjectPlanningPurchaseOrderDetailSelectedList.length > 0 ? $scope.ProjectPlanningPurchaseOrderDetailSelectedList : $scope.ProjectPlanningPurchaseOrderDetailSavedList, 'projectPlanningPurchaseOrderMachineType': $scope.ProjectPlaningPurchaseOrderMachineTypeListForSave },
    //                    dataType: 'JSON'
    //                }).then(function successCallback(response) {
    //                    if (response.data.Error == true) {
    //                        ShowResult(response.data.Message, 'failure');
    //                    }
    //                    else {
    //                        ShowResult(response.data.Message, 'success');
    //                        getProjectPlanningPurchaseOrderById(response.data.ProjectPlanningPurchaseOrderId);
    //                        $scope.projectPlanningPOMachineTypeMasterFormModalCloseListPopUp();
    //                        $scope.budgetMasterSelectedList = [];
    //                    }
    //                }), function errorCallBack(response) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //            }
    //        }
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}
    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.projectplanningPurchaseOrderNew.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrl + $scope.projectplanningPurchaseOrderNew.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.projectplanningPurchaseOrders.splice($scope.index, 1);
    //                baseService.paginationRemove();
    //                ClearFields();
    //            }
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        });
    //    }
    //}
}
ProjectPlanningPurchaseOrderController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];