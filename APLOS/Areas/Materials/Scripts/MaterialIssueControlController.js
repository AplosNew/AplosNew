'use strict';
MaterialIssueControlController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function MaterialIssueControlController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    $rootScope.title = "Material Issue Control";
    $scope.Action = 'Save';
    $scope.index = -1;

    $scope.path = 'Materials/MaterialIssueControl/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateIssue';
    $scope.updateUrl = $scope.path + 'Update';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.modelFilterByList = [
        { 'name': 'Prod. Order#', 'value': 'Id' },
        { 'name': 'Prod. Status', 'value': 'ProductionStatus' },
        { 'name': 'Material', 'value': 'Material' },
        { 'name': 'Product', 'value': 'Product' },
        { 'name': 'Product Category', 'value': 'ProductCategory' },
        { 'name': 'Master Order No', 'value': 'MasterOrderId' },
        { 'name': 'Buyer Order#', 'value': 'BuyerRefNo' },
        { 'name': 'Own Order#', 'value': 'OwnRefNo' },
        { 'name': 'Buyer Item#', 'value': 'StyleNo' },
        { 'name': 'Own Item#', 'value': 'OwnStyleNo' },
        { 'name': 'SO No', 'value': 'SONo' },
        { 'name': 'SO Desc', 'value': 'SODesc' },
        { 'name': 'Buyer', 'value': 'buyer' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['BOM'];
    $scope.ModelNew = { Id: null, POId: null, IssueId: null, EntityId: null, MaterialStorageId: null, IssueDate: null, IssueType: 'Revenue', UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "QBOQ", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'Get',
            url: "Materials/MaterialIssueControl/EntityList"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.getAllEntities();

    $scope.GetEntityName = function () {
        for (var i = 0; i < $scope.entityList.length; i++) {
            if ($scope.entityList[i].Value == $scope.ModelNew.EntityId) {
                $scope.ModelNew.Entity = $scope.entityList[i].Text;
                break;
            }
        }

    }

    $scope.AllTabPrint = function (data) {
        location.href = "Materials/MaterialIssueControl/IssueRequestReport?mId=" + data.data.Id;
    };

    $scope.LevelList = [
        {
            'Value': 'Costing',
            'Text': 'Costing'
        },
        {
            'Value': 'QBOQ',
            'Text': 'QBOQ'
        }
    ];

    $scope.storageList = [];

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
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetIssueSlipCheckByCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();

    $scope.CostCenterLoad = function () {
        cboService.getCostCenterCbo(function (result) {
            $scope.costCenterList = result;
            for (var i = 0; i < $scope.costCenterList.length; i++) {
                if ($scope.costCenterList[i].Text == 'Mixing') {
                    $scope.ModelNew.CostCenterId = $scope.costCenterList[i].Value;
                    break;
                }
            }
        });
    }
    $scope.CostCenterLoad();

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.modelList = [];
    $scope.getData = function () {
        $scope.modelList = [];
        if (!baseService.isUndefinedOrNull($scope.ModelNew.EntityId)) {
            $http({
                method: 'POST',
                data: {
                    'entityid': $scope.ModelNew.EntityId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: $scope.getListUrl
            }).then(function successCallback(response) {
                $scope.modelList = response.data;
            });
        }
    };

    $scope.rowDataBound = function rowDataBound(e) {
        if (e.data.Balance != 0) {
            e.row.css("background-color", '#FFFF00')
        }

    }


    $scope.MCFilterByList = [
        { 'name': 'Prod. Order#', 'value': 'POId' },
        { 'name': 'SlipId', 'value': 'IssueId' },
    ];

    $scope.MCSearchColumn = 'POId';
    $scope.MCSearchValue = null;
    $scope.savedList = [];
    $scope.savedissueList = [];
    $scope.GetSavedData = function () {
        $scope.savedList = [];
        $scope.savedissueList = [];
        $http({
            method: 'POST',
            data: {
                'column': $scope.MCSearchColumn, 'value': $scope.MCSearchValue
            },
            url: 'Materials/MaterialIssueControl/GetApprovedData'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                for (var i = 0; i < response.data.length; i++) {
                    if (response.data[i].IssuedQty != 0) {
                        $scope.savedissueList.push(response.data[i]);
                    }
                    if (response.data[i].Balance != 0) {
                        $scope.savedList.push(response.data[i]);
                    }
                }
            }


        });
    };
    $scope.GetSavedData();

    $scope.SOItemList = [];
    $scope.Get = function (obj) {
        $scope.ModelNew.POId = obj.data.Id;

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.SearchSOItemList = [];
    $scope.AddSO = function () {
        $scope.itemList = [];
        $http.get('Materials/MaterialIssueControl/GetSOItemList?entityid=' + $scope.ModelNew.EntityId + '&ProductionOrderId=' + $scope.ModelNew.POId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SearchSOItemList = response.data;

                        if (baseService.arrayLength($scope.SOItemList) > 0) {
                            for (var i = 0; i < $scope.SOItemList.length; i++) {
                                for (var j = 0; j < $scope.SearchSOItemList.length; j++) {
                                    if ($scope.SOItemList[i].LineItemId == $scope.SearchSOItemList[j].LineItemId) {
                                        $scope.SearchSOItemList.splice(j, 1);
                                    }
                                }
                            }
                        }
                        var ob = { Value: null, Text: null };
                        for (var i = 0; i < $scope.SearchSOItemList.length; i++) {
                            ob.Value = $scope.SearchSOItemList[i].LineItemId;
                            ob.Text = $scope.SearchSOItemList[i].LineItemId;
                            $scope.itemList.push(ob);
                            ob = {};
                        }
                    }


                    angular.element(document.querySelector('#SOpopUp')).modal('show');
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.itemList = [];
    $scope.closeSOPopUp = function () {
        try {
            for (var i = 0; i < $scope.SearchSOItemList.length; i++) {

                if ($scope.SearchSOItemList[i].Flag) {
                    if (checkExistsItem($scope.SOItemList, $scope.SearchSOItemList[i].LineItemId)) {
                        $scope.SOItemList.push($scope.SearchSOItemList[i]);
                    }
                    else {
                        $scope.SOItemList = [];
                        throw "Select same Line Item";
                    }
                }
            }
            $scope.GetQBOQCostingData();
            angular.element(document.querySelector('#SOpopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SOId === id) {
                return true;
            }
        }
        return false;
    }

    function checkExistsItem(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].LineItemId !== id) {
                return false;
            }
        }
        return true;
    }


    // #region checkbox all

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#SOPOPGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SOItemList.length; i++) {
                $scope.SearchSOItemList[i].Flag = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }

        }
        var gridObj = $("#SOPOPGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.GetSaved = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        $scope.ModelNew.IssueDate = $filter('dateFiltering')($scope.ModelNew.IssueDate, 'dd-M-yyyy');
        $scope.Action = 'Update';
        $scope.GetSavedSODetailData();
        $scope.GetSavedDetailData();

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SOItemList = [];
    $scope.GetSavedSODetailData = function () {
        $scope.SOItemList = [];
        $http.get('Materials/MaterialIssueControl/GetSavedSODetailData?masterId=' + $scope.ModelNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                        var ob = { Value: null, Text: null };
                        for (var i = 0; i < $scope.SOItemList.length; i++) {
                            ob.Value = $scope.SOItemList[i].LineItemId;
                            ob.Text = $scope.SOItemList[i].LineItemId;
                            $scope.itemList.push(ob);
                            ob = {};
                        }

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
                        $scope.GetIssueRequestList()
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.IssueRequestList = [];
    $scope.GetIssueRequestList = function () {
        $http({
            method: 'GET',
            url: 'Materials/MaterialIssueControl/GetIssueRequestList?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.IssueRequestList = response.data;
            $scope.GetIssueRequestBOQMapList();
        });
    };


    $scope.IssueRequestBOQMapList = [];
    $scope.GetIssueRequestBOQMapList = function () {
        $http({
            method: 'GET',
            url: 'Materials/MaterialIssueControl/GetIssueRequestBOQMapList?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.IssueRequestBOQMapList = response.data;
        });
    };

    $scope.getArticle = function (data) {
        $scope.SelectedMaterial = data;
        $scope.getArticleSearchList(data.MaterialMasterId);
    };
    $scope.selectarticle = function (ob) {
        try {

            //$scope.SelectedMaterial.MaterialMasterId = ob.MaterialMasterId;
            //$scope.SelectedMaterial.Material = ob.MaterialMasterName;
            $scope.SelectedMaterial.ArticleId = ob.Id;
            $scope.SelectedMaterial.QBOQArticle = ob.StandardName;

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
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmpData'

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
        $scope.ModelNew.ByWhomEmployeeCode = arg.data.EmployeeCode;
        $scope.closePopUp();
    }


    $scope.clearEmp = function () {
        $scope.ModelNew.ByWhomId = null;
        $scope.ModelNew.ByWhom = null;
        $scope.ModelNew.ByWhomEmployeeCode = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }
    $scope.disbtn = false;
    $scope.Action = 'Save';
    $scope.Save = function () {
        $scope.QBOQCostingListNew = [];

        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                $scope.disbtn = true;
                if ($scope.Action === 'Save') {
                    if (baseService.arrayLength($scope.SOItemList) === 0) {
                        $scope.disbtn = false;
                        throw "Select SO Detail.";
                    }
                    else {
                        if (baseService.arrayLength($scope.SOItemList) > 1) {
                            for (var i = 0; i < $scope.SOItemList.length; i++) {
                                var firstLineId = $scope.SOItemList[0].LineItemId;
                                if ($scope.SOItemList[i].LineItemId != firstLineId) {
                                    throw "Please select same Line Item.";
                                }
                            }
                        }
                    }


                    if (baseService.arrayLength($scope.QBOQCostingList) > 0) {
                        for (var p = 0; p < $scope.QBOQCostingList.length; p++) {

                            $scope.QBOQCostingList[p].TransactionUoMId = $scope.QBOQCostingList[p].UoMId;
                            $scope.QBOQCostingList[p].BaseUoMId = $scope.QBOQCostingList[p].UoMId;
                            $scope.QBOQCostingList[p].CostCenterId = $scope.ModelNew.CostCenterId;
                            $scope.QBOQCostingList[p].RequestedQty = $scope.QBOQCostingList[p].PlanConsumption;
                            $scope.QBOQCostingListNew.push($scope.QBOQCostingList[p]);
                        }
                    }

                    if (baseService.isUndefinedOrNull($scope.ModelNew.POId)) {
                        $scope.disbtn = false;
                        throw "Select Production Order.";
                    }


                 
                    if (baseService.isUndefinedOrNull($scope.ModelNew.ByWhomId)) {
                        throw "Select By Whom Employee.";
                    }
                    $scope.disbtn = true;
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
                            $scope.disbtn = false;
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.disbtn = false;
                            ShowResult(response.data.Message, 'success');
                            $scope.Clear();
                            $scope.GetSavedData();
                        }
                    }), function errorCallBack(response) {
                        $scope.disbtn = false;
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else {

                    $scope.disbtn = true;
                    for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
                        for (var j = 0; j < $scope.IssueRequestList.length; j++) {
                            if ($scope.QBOQCostingList[i].Id == $scope.IssueRequestList[j].MaterialIssueControlDetailId) {
                                $scope.IssueRequestList[j].RequestedQty = $scope.QBOQCostingList[i].PlanConsumption;
                            }
                        }
                    }

                    for (var i = 0; i < $scope.IssueRequestList.length; i++) {
                        for (var j = 0; j < $scope.IssueRequestBOQMapList.length; j++) {
                            if ($scope.IssueRequestList[i].Id == $scope.IssueRequestBOQMapList[j].IssueRequestDetailId) {
                                $scope.IssueRequestBOQMapList[j].Qty = $scope.IssueRequestList[i].RequestedQty;
                            }
                        }
                    }


                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'model': $scope.ModelNew
                            , 'soList': $scope.SOItemList
                            , 'dataList': $scope.QBOQCostingList
                            , 'IssueRequestList': $scope.IssueRequestList
                            , 'BOQMapList': $scope.IssueRequestBOQMapList
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            $scope.disbtn = false;
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.disbtn = false;
                            ShowResult(response.data.Message, 'success');
                            $scope.Clear();
                            $scope.GetSavedData();
                        }
                    }), function errorCallBack(response) {
                        $scope.disbtn = false;
                        ShowResult(response.data.Message, 'failure');
                    };

                }
            }
          
        
        } catch (e) {
            $scope.disbtn = false;
            ShowResult(e, "failure");
        }
       
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                data: {
                    'id': $scope.ModelNew.Id
                    , 'issueId': $scope.ModelNew.IssueId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.GetSavedData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.disbtn = false;
        $scope.ModelNew = { Id: null, POId: null, EntityId: null, MaterialStorageId: null, IssueDate: null, IssueType: 'Revenue', UserCode: null, UserRef: null, PlanPercentage: null, ByWhomId: null, UserName: null, Level: "QBOQ", LotNo: null, IsApproved: 0, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
        $scope.SOItemList = [];
        $scope.QBOQCostingList = [];
        $scope.ModelNew.Level = "QBOQ";
        $scope.modelList = [];
        $rootScope.toggle();
        $scope.CostCenterLoad();
        $scope.Action = 'Save';
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.QBOQCostingList = [];

    $scope.sqlInStatementSO = null;
    $scope.sqlInStatement = null;
    $scope.GetQBOQCostingData = function () {
        if ($scope.SOItemList.length > 0) {
            var uniqueMasterOrderId = removeDuplicates($scope.SOItemList, 'LineItemId');
            var wcEmpCode = "";
            if (uniqueMasterOrderId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueMasterOrderId, function (item) { return "'" + item.LineItemId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;

            var uniquesoId = removeDuplicates($scope.SOItemList, 'SOId');
            var wcsoId = "";
            if (uniquesoId.length > 0) {
                wcsoId = "IN(";
                wcsoId += Array.prototype.map.call(uniquesoId, function (item) { return "'" + item.SOId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatementSO = wcsoId;
        }

        $scope.QBOQCostingList = [];
        if ($scope.ModelNew.Level == "Costing") {
            $http({
                method: 'GET',
                url: 'Materials/MaterialIssueControl/GetCostingDataList?LineItemId=' + $scope.sqlInStatement + '&soId=' + $scope.sqlInStatementSO

            }).then(function successCallback(response) {
                $scope.QBOQCostingList = response.data;
            });
        }
        else {
            $http({
                method: 'GET',
                url: 'Materials/MaterialIssueControl/GetQBOQDataList?LineItemId=' + $scope.sqlInStatement + '&soId=' + $scope.sqlInStatementSO

            }).then(function successCallback(response) {
                $scope.QBOQCostingList = response.data;
            });
        }
    }

    $scope.BoqCalculation = function (obj) {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.PlanPercentage) || $scope.ModelNew.PlanPercentage == 0 || $scope.ModelNew.PlanPercentage == 'NaN') {
                throw "Input Plan Percentage";
            }
            var totaPlanlAmount = 0;
           // obj.data.PlanConsumption = (obj.data.TotalConsumption + obj.data.AdditionReduction) * $scope.ModelNew.PlanPercentage / 100;

            obj.data.PlanConsumption = (obj.data.TotalConsumption + obj.data.AdditionReduction) / (100 - ($scope.ModelNew.PlanPercentage - 100)) * 100;

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

            for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
                totaPlanlAmount += $scope.QBOQCostingList[i].TotaPlanlAmount;
            }

            for (var i = 0; i < $scope.SOItemList.length; i++) {
                $scope.SOItemList[i].PlanRate = totaPlanlAmount / $scope.SOItemList[i].PlannedQty;
                $scope.SOItemList[i].PlantCost = $scope.SOItemList[i].PlanRate * $scope.SOItemList[i].PlannedQty;
                $scope.SOItemList[i].TotalSOCostVsTotalPlanCost = $scope.SOItemList[i].SOTotalMaterailCost - $scope.SOItemList[i].PlantCost;
            }
            var gridObj = $("#SOGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.CostCalculation = function (obj) {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.PlanPercentage) || $scope.ModelNew.PlanPercentage == 0 || $scope.ModelNew.PlanPercentage == 'NaN') {
                throw "Input Plan Percentage";
            }
            var totaPlanlAmount = 0;
           // obj.data.PlanConsumption = (obj.data.TotalConsumption + obj.data.AdditionReduction) * $scope.ModelNew.PlanPercentage / 100;

            obj.data.PlanConsumption = (obj.data.TotalConsumption + obj.data.AdditionReduction)/(100-($scope.ModelNew.PlanPercentage-100))* 100;

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

            for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
                totaPlanlAmount += $scope.QBOQCostingList[i].TotaPlanlAmount;
            }

            for (var i = 0; i < $scope.SOItemList.length; i++) {
                $scope.SOItemList[i].PlanRate = totaPlanlAmount / $scope.SOItemList[i].PlannedQty;
                $scope.SOItemList[i].PlantCost = $scope.SOItemList[i].PlanRate * $scope.SOItemList[i].PlannedQty;
                $scope.SOItemList[i].TotalSOCostVsTotalPlanCost = $scope.SOItemList[i].SOTotalMaterailCost - $scope.SOItemList[i].PlantCost;
            }
            var gridObj = $("#SOGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.CalculationByPlan = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.PlanPercentage) || $scope.ModelNew.PlanPercentage == 0 || $scope.ModelNew.PlanPercentage == 'NaN') {
                throw "Input Plan Percentage";
            }
            var totaPlanlAmount = 0;
            for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
                //$scope.QBOQCostingList[i].PlanConsumption = ($scope.QBOQCostingList[i].TotalConsumption + $scope.QBOQCostingList[i].AdditionReduction) * $scope.ModelNew.PlanPercentage / 100;
                                                           
                $scope.QBOQCostingList[i].PlanConsumption = ($scope.QBOQCostingList[i].TotalConsumption + $scope.QBOQCostingList[i].AdditionReduction)/(100-($scope.ModelNew.PlanPercentage-100))* 100;
                $scope.QBOQCostingList[i].TotaPlanlAmount = $scope.QBOQCostingList[i].PlanConsumption * $scope.QBOQCostingList[i].Rate;
                $scope.QBOQCostingList[i].ActualIssueAmount = $scope.QBOQCostingList[i].PlanConsumption * $scope.QBOQCostingList[i].StockRate;
            }


            if ($scope.ModelNew.Level == "Costing") {
                var gridObj = $("#CGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            } else {
                var gridObj = $("#BGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            }

            for (var i = 0; i < $scope.QBOQCostingList.length; i++) {
                totaPlanlAmount += $scope.QBOQCostingList[i].TotaPlanlAmount;
            }

            for (var i = 0; i < $scope.SOItemList.length; i++) {
                $scope.SOItemList[i].PlanRate = totaPlanlAmount / $scope.SOItemList[i].PlannedQty;
                $scope.SOItemList[i].PlantCost = $scope.SOItemList[i].PlanRate * $scope.SOItemList[i].PlannedQty;
                $scope.SOItemList[i].TotalSOCostVsTotalPlanCost = $scope.SOItemList[i].SOTotalMaterailCost - $scope.SOItemList[i].PlantCost;
            }
            var gridObj = $("#SOGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.PrintData = function (data) {
        try {
            $scope.fileName = "MaterialIssueReport.xls";


            //$scope.ReportFormat = 'Excel';
            $scope.ReportFormat = 'Pdf';
            var url = 'Materials/MaterialIssueControl/GetMaterialIssueReportPdf?reportFormat=' + $scope.ReportFormat + '&masterId=' + data.data.Id;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

}