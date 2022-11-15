'use strict';
MaterialIssueController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function MaterialIssueController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    $rootScope.title = "Material Issue";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SOItemList = [];
    $scope.path = 'Materials/MaterialIssueControl/';
    $scope.saveUrl = $scope.path + 'CreateIssue';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['BOM'];
  
    $scope.ModelNew = { Id: null, POId: null, EntityId: null, MaterialStorageId: null, IssueDate: null, IssueType: 'Revenue', UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "Costing", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };

    $scope.modelList = [];
    $scope.GetData = function () {
        $scope.modelList = [];
        $http.get('Materials/MaterialIssueControl/GetApprovedData')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.modelList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
      
    };
    $scope.GetData();

    $scope.SOItemList = [];
    $scope.GetSavedSODetailData = function () {
        $scope.SOItemList = [];
        $http.get('Materials/MaterialIssueControl/GetSavedSODetailData?masterId='+$scope.ModelNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.QBOQCostingList = [];
    $scope.GetSavedDetailData = function () {
        $scope.QBOQCostingList = [];
        $http.get('Materials/MaterialIssueControl/GetSavedDetailData?masterId=' + $scope.ModelNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.QBOQCostingList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.Get = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        $scope.Action = 'Update';
        $scope.GetSavedSODetailData();
        $scope.GetSavedDetailData();
        $scope.IssueSlipGriddata('ForChecked', 'InventorySlip', $scope.ModelNew.POId);
        /*$scope.getdataInventoryIssue($scope.ModelNew.POId);*/
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getArticle = function (data) {
        $scope.SelectedMaterial = data;
        $scope.getArticleSearchList(data.MaterialMasterId);
    };
    $scope.selectarticle = function (ob) {
        try {

            $scope.SelectedMaterial.MaterialMasterId = ob.MaterialMasterId;
            $scope.SelectedMaterial.Material = ob.MaterialMasterName;
            $scope.SelectedMaterial.ArticleId = ob.Id;
            $scope.SelectedMaterial.Article = ob.StandardName;

            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
            if ($scope.ModelNew.Level == "Costing") {
                var gridObj = $("#CGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            } else {
                var gridObj = $("#BGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            }
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.popUpDataList = [];
    $scope.showEmployeeListPopUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmployeeData'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });

            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.ModelNew.ByWhomId = arg.data.SystemId;
        $scope.ModelNew.ByWhom = arg.data.EmployeeName;
        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.Clear = function () {
        $scope.ModelNew = { Id: null, POId: null, EntityId: null, MaterialStorageId: null, IssueDate: null, IssueType: 'Revenue', UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "Costing", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
        $scope.SOItemList = [];
        $scope.QBOQCostingList = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.QBOQCostingList = [];
    $scope.GetQBOQCostingData = function () {
        if ($scope.SOItemList.length > 0) {
            var uniqueMasterOrderId = removeDuplicates($scope.SOItemList, 'SOId');
            var wcEmpCode = "";
            if (uniqueMasterOrderId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueMasterOrderId, function (item) { return "'" + item.SOId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }


        $scope.QBOQCostingList = [];
        if ($scope.ModelNew.Level == "Costing") {
            $http({
                method: 'GET',
                url: 'Materials/MaterialIssueControl/GetCostingDataList?soId=' + $scope.sqlInStatement

            }).then(function successCallback(response) {
                $scope.QBOQCostingList = response.data;
            });
        }
        else {
            $http({
                method: 'GET',
                url: 'Materials/MaterialIssueControl/GetQBOQDataList?soId=' + $scope.sqlInStatement

            }).then(function successCallback(response) {
                $scope.QBOQCostingList = response.data;
            });
        }
    }

    $scope.Calculation = function (obj) {
        obj.data.PlanConsumption = (obj.data.TotalConsumption + obj.data.AdditionReduction) * $scope.ModelNew.PlanPercentage / 100;
        obj.data.TotaPlanlAmount = obj.data.PlanConsumption * obj.data.Rate;
        obj.data.ActualIssueAmount = obj.data.PlanConsumption * obj.data.StockRate;

        if ($scope.ModelNew.Level == "Costing") {
            var gridObj = $("#CGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        } else {
            var gridObj = $("#BGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        }
    }

    $scope.Getstorage = function () {
        $http({
            method: 'GET',
            url: 'Materials/MaterialStorage/getcbo'
        }).then(function (response) {
            $scope.storageList = response.data;
        });
    }
    $scope.Getstorage();

    $scope.NotificationSettingStatus = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/NotificationSetting',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
            $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
            if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
                $scope.labelCheckAndApproved = 'To be checked by';
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                $scope.labelCheckAndApproved = 'To be approved by';
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                $scope.labelCheckAndApproved = 'To be checked by';
            }
            //else {
            //    $scope.productNew.labelCheckAndApproved = 'To be checked/approved by';
            //}

        });
    }
    $scope.NotificationSettingStatus();
    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetSupervisorCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();
    $scope.CostCenterLoad = function () {
        cboService.getCostCenterCbo(function (result) {
            $scope.costCenterList = result;
        });
    }
    $scope.CostCenterLoad();

    $scope.IssueSlipListPopup = [];

    $scope.searchBySlipMaterial = "MaterialMasterName"; $scope.searchSlip = "";
    $scope.searchBySlipList = [{ value: 'MaterialMasterGroupName', name: "MaterialMasterGroupName" }, { value: 'MaterialType', name: "MaterialType" }, { value: 'MaterialMasterName', name: "Material Master" }, { value: 'StandardName', name: "Article" }
    ];

    $scope.materialStockList = [];
    $scope.specificStockList = [];
    $scope.newData = {};
    $scope.newDatum = {};
    $scope.getSpecificMaterialStock = function (data) {
        $scope.newDatum = data.data;
        $scope.selectedRowQty = data.data.TotalConsumption;
        $scope.newData.MaterialMasterId = data.data.MaterialMasterId;
        $scope.newData.ArticleId = data.data.ArticleId;
        $scope.newData.MaterialStorageId = $scope.ModelNew.MaterialStorageId;
        $scope.newData.TransactionUoMId = $scope.ModelNew.UoMId;
        $http({
            method: 'POST'
            , url: 'Products/GoodsReceiveNote/GetStockForMaterialIssue'
            , data: { materialMasterId: $scope.newData.MaterialMasterId, articleId: $scope.newData.ArticleId }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.materialStockList = response.data;

            //for (var i1 = 0; i1 < $scope.materialStockList.length; i1++) {
            //    $scope.materialStockList[i1].TrasactopmUomQty = $scope.materialStockList[i1].BalanceStock / $scope.newDatum.BaseUoMFactor;
            //    $scope.materialStockList[i1].IssueTransactionUoMId = data.TransactionUoMId;
            //    $scope.materialStockList[i1].IssueTransactionUoM = data.TransactionUoM;
            //    $scope.materialStockList[i1].TransactionUoMId = $scope.newDatum.UoMId;
            //    $scope.materialStockList[i1].BaseUoMFactor = $scope.newDatum.BaseUoMFactor;
            //}
            angular.element(document.querySelector('#stockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.closeStockPopUp = function () {
        angular.element(document.querySelector('#stockPopUp')).modal('hide');
    }

    $scope.CalTotalQty = function () {
        var totalQty = 0;
        for (var i = 0; i < $scope.materialStockList.length; i++) {
            totalQty += $scope.materialStockList[i].RequestedQty;
        }
        $scope.newDatum.RequestedQty = totalQty;
        $scope.newDatum.InventoryMaterialId = $scope.materialStockList[0].InventoryMaterialId;
        if ($scope.ModelNew.Level == "Costing") {
            var gridObj = $("#CGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        } else {
            var gridObj = $("#BGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        }
    }

    $scope.Action = 'Save';
    $scope.Save = function () {
        $scope.QBOQCostingListNew = [];
        for (var p = 0; p < $scope.QBOQCostingList.length; p++) {
            if ($scope.QBOQCostingList[p].RequestedQty > 0) {
                $scope.QBOQCostingList[p].TransactionUoMId = $scope.QBOQCostingList[p].UoMId;
                $scope.QBOQCostingList[p].BaseUoMId = $scope.QBOQCostingList[p].UoMId;
                $scope.QBOQCostingList[p].CostCenterId = $scope.ModelNew.CostCenterId;
                $scope.QBOQCostingListNew.push($scope.QBOQCostingList[p]);
            }
        }
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.POId)) {
                throw "Select Production Order.";
            }
            if (baseService.arrayLength($scope.SOItemList) === 0) {
                throw "Select SO Detail.";
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'model': $scope.ModelNew
                            , 'soList': $scope.SOItemList
                            , 'dataList': $scope.QBOQCostingListNew
                            , 'dataLists': $scope.QBOQCostingListNew
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //$scope.GridInventoryIssuedata = [];
    //$scope.getdataInventoryIssue = function (productionOrderId) {
    //    $http({
    //        method: "GET",
    //        dataType: 'JSON',
    //        url: 'Materials/MaterialIssueControl/GetInventoryIssueByProductionOrder?productionOrderId=' + productionOrderId,
    //    }).then(function successCallback(response) {
    //        $scope.GridInventoryIssuedata = response.data;
    //    });
    //};

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/GoodsReceiveNote/IssueRequestReport?issueId=" + data.Id;

    };
    $scope.IssueRequestReport = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.IssueRequestReportprint
        }
    }];

    //**********Expenses GL Budget Activity**************
    $scope.searchglByList = [
        {
            "name": "GL",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoName",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GLPopUp = function (data) {
        //debugger;
        $scope.customerInvoiceGLList = [];
        //baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetAllGLBudgetActivityPostingAutomaticOnly", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
        $scope.tempData = data;
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };
    $scope.setSelected = function (data) {
        $scope.tempData.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.tempData.BudgetMasterId = data.BudgetMasterId;
        $scope.tempData.BudgetName = data.BudgetName;
        $scope.tempData.ActivityName = data.ActivityName;
        $scope.tempData.ExpenseActivityId = data.ActivityId;
        var gridObj = $("#BGrid").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };
    //********** End Expenses GL Budget Activity**************

    $scope.IssueSlipList = [];
    $scope.IssueSlipHoldRejectList = []
    $scope.IssueSlipCheckedList = []
    $scope.IssueStatus = 'ForChecked';

    $scope.IssueSlipGriddata = function (issueStatus, issueSlipType,productionOrderId) {
        $scope.IssueSlipList = [];
        $scope.IssueSlipHoldRejectList = []
        $scope.IssueSlipCheckedList = []
        $scope.Status = 'InventorySlip';
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/IssueListDataByProudctionOrder?IssueStatus=' + issueStatus + '&IssueSlipType=' + issueSlipType + '&productionOrderId=' + productionOrderId
        }).then(function successCallback(response) {
            if (issueStatus == 'ForChecked') {
                $scope.IssueSlipList = response.data;
            }
        });
    }
    
}