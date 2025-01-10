'use strict';
FabricRollsController.$inject = ['commonMessage', '$controller', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function FabricRollsController(commonMessage, $controller, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Fabric Roll Master";
    $scope.Action = 'Save';
    $scope.fabricRollMasters = [];
    $scope.selectedGRNList = [];
    $scope.path = 'Materials/FabricRoll/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlCustomer = $scope.path + 'createCustomer';
    $scope.updateUrlFabricDetails = $scope.path + 'UpdateFabricDetails';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.showfromto = true;
    $scope.showgrndiv = false;

    $scope.clickGo = function () {
        $scope.showfromto = false;
        $scope.showgrndiv = true;
    }

    $scope.clickBack = function () {
        $scope.showfromto = true;
        $scope.showgrndiv = false;
    }

    $scope.fabricRollMaster = {
        CompanyGroupId: $window.companyGroupId,
        PlantId: $window.plantId,
        InventoryReceiveId: null,
        GRNId: null,
        PaidHours: null,
        EmployeeId: null,
        EmployeeCategoryId: null,
        EmployeeCategory: null,
        EmployeeCode: "",
        EmployeeName: "",
        GRNSplitQty: null
        , TransactionQty: 0
        , TransactionAmount: 0
        , CurrencyCode: null
        , POId: null
        , PODate: null
        , GRNNo: null
        , VendorRefNo: null
        , PurchaseLCNo: null
        , PINo: null
        , InvoiceNo: null
        , LCDate: null
        , OpeningBank: null
        , UserName: null
    };

    function containsSpecialChars(str) {
        const specialChars = /[`!@#$%^&*()_+\=\[\]{};':"\\|,.<>\/?~]/;
        return specialChars.test(str);
    }

    $scope.CheckSpecialCharecter = function () {
        try {
            if (containsSpecialChars($scope.fabricRollMaster.UserName)) {
                $scope.fabricRollMaster.UserName = $scope.fabricRollMaster.UserName.substring(0, $scope.fabricRollMaster.UserName.length - 1);
                throw "No special characters allowed for User Name.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.fabricRollSplitOb = {
        VendorWidth: null
    }
    $scope.fabricRollMasterNew = Object.assign({}, $scope.fabricRollMaster);


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //#region Fabric Roll Pop Up
    $scope.selectedGRNRow = {};
    $scope.fabDistributeQty = 0;
    $scope.fabricEdit = false;
    $scope.RowData = '';
    $scope.showFabricPop = function (data) {

        $scope.RowData = data;
        $scope.fabricRollSplitOb.VendorWidth = null;
        // $scope.fabricEdit = isEdit;
        $scope.fabricRollMasterNew.GRNSplitQty = null;
        $scope.fabricRollMasterList = [];
        $scope.selectedGRNRow = data;
        $scope.fabDistributeQty = data.TotalDistributeQty;
        $scope.LoadFabricRollList();

        angular.element(document.querySelector('#fabricRollPopUp')).modal('show');
    };

    $scope.splitGrnRow = function () {
        debugger;
        if (!baseService.isUndefinedOrNull($scope.fabricRollMasterNew.GRNSplitQty)) {
            var dbIncre = 0;
            $http({
                method: 'GET',
                url: 'Materials/FabricRollMaster/GetFabricIncrementValue'
            }).then(function successCallback(response) {
                dbIncre = response.data;
                if (!$scope.fabricEdit) {
                    for (var i = 0; i < $scope.fabricRollMasterNew.GRNSplitQty; i++) {
                        var ob = Object.assign({}, $scope.selectedGRNRow);
                        if (ob.FabRollPrefix === null) {
                            ShowResult('Plan Configuaration is not set for roll prefix!', 'failure', 'fabricRollPopUp');

                        }

                        else {
                            ob.InventoryReceiveDetailId = ob.Id;
                            ob.Id = null;
                            ob.RollNo = ob.FabRollPrefix + new Date().getFullYear().toString().substring(2) + (new Date().getMonth() + 1) + new Date().getDate() + getGenNo(dbIncre + i);
                            ob.VendorQty = parseFloat((ob.TransactionQty / $scope.fabricRollMasterNew.GRNSplitQty).toFixed(2));
                            ob.VendorWidth = $scope.fabricRollSplitOb.VendorWidth;
                            ob.VendorRollNo = null;
                            ob.VendorLotNo = null;
                            $scope.fabricRollMasterList.push(ob);
                        }
                    }
                } else {
                    var tempQ = $scope.fabricRollMasterList.length;
                    for (var a = 0; a < $scope.fabricRollMasterNew.GRNSplitQty; a++) {
                        var oba = Object.assign({}, $scope.selectedGRNRow);
                        oba.InventoryReceiveDetailId = oba.Id;
                        oba.Id = null;
                        oba.RollNo = oba.FilePrefix + new Date().getFullYear().toString().substring(2) + (new Date().getMonth() + 1) + new Date().getDate() + getGenNo(tempQ + a + dbIncre);
                        oba.VendorQty = 0.00;
                        oba.VendorWidth = $scope.fabricRollSplitOb.VendorWidth;
                        oba.VendorRollNo = null;
                        oba.VendorLotNo = null;
                        $scope.fabricRollMasterList.push(oba);
                    }
                    tempQ = 0;
                }
                var ftempOb = 0;
                angular.forEach($scope.fabricRollMasterList, function (item) {
                    ftempOb += item.VendorQty;
                });
                $scope.fabDistributeQty = ftempOb;
                ftempOb = 0;
            });
        }
    }
    $scope.saveRollList = [];

    $scope.saveRoll = function () {

        if (!baseService.isUndefinedOrNull($scope.fabricRollMasterNew.GRNSplitQty)) {

            $http({
                method: 'POST',
                url: $scope.path + "GetRoll",
                data: { 'NoofRolls': $scope.fabricRollMasterNew.GRNSplitQty, 'SelectedRow': $scope.selectedGRNRow, 'Width': $scope.fabricRollSplitOb.VendorWidth, 'PackingForm': $scope.PackingformSearchBy },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.LoadFabricRollList();


            });
        }
    }

    $scope.clearSplitRow = function () {
        $scope.fabricRollMasterNew.GRNSplitQty = null;
        $scope.fabricRollMasterList = [];
    }

    $scope.searchGRNByList = [
        {
            name: 'GRN No',
            value: 'GRNNo'
        },
        {
            name: 'GRNDate',
            value: 'GRNDate'
        },
        {
            name: 'Party',
            value: 'PartyName'
        }
    ];
    $scope.GRNsearchBy = "GRNNo";
    $scope.GRNsearch = "";

    $scope.obj = {};
    $scope.SelectOption = function (args) {

       // $("#CreateNewPopUp").data("ejDialog").open();

        //$scope.obj = Object.assign({}, args.data);

        $scope.fabricRollMaster = Object.assign({}, args.data);
        $scope.fabricRollMaster.GRNId = $scope.fabricRollMaster.GRNNo;
        $scope.LoadMaterialSearchList();
        $scope.getSaveMaster($scope.fabricRollMaster.GRNNo);

        angular.element(document.querySelector('#grnListPopUp')).modal('hide');
    };

    $scope.Go = function () {
        $scope.fabricRollMaster = Object.assign({}, $scope.obj);
        $scope.fabricRollMaster.GRNId = $scope.fabricRollMaster.GRNNo;
        $scope.LoadMaterialSearchList();
        $scope.getSaveMaster($scope.fabricRollMaster.GRNNo);
    }

 
    $scope.MasterFabricRollId = null;
    $scope.getSaveMaster = function (GRNNo) {
        $http({
            method: 'GET',
            url: $scope.path + "GetSavedList?GRNId=" + GRNNo,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.fabricRollMaster = Object.assign({}, response.data[0]);
                $scope.getSaveChildData($scope.fabricRollMaster.Id);
                $scope.MasterFabricRollId = $scope.fabricRollMaster.Id;
            }
        });
    }

    $scope.getSaveChildData = function (masterId) {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricRollChildList?FabricRollManagementMasterId=" + masterId,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.grnDetailList = response.data;
            }
        });
    }



    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }

    //#region Display Material by GRN ID
    $scope.closeGRNPopUp = function (args) {

        $scope.fabricRollMaster = Object.assign({}, args.data);
        $scope.getGRNDetail();
    };
    //#endregion Material

    //#region grnDetail
    $scope.grnDetailList = [];
    $scope.getGRNDetail = function () {
        try {
            $scope.popUpUrl = '';
            $scope.popUpUrl = 'Materials/FabricRollMaster/MaterialList?inventoryReceiveId=' + $scope.fabricRollMaster.InventoryReceiveId;
            $scope.getGRNDetailData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.grnDetailParameters)
                    .then(function (result) {
                        $scope.grnDetailList = result.Rows;
                        $scope.grnDetailParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.getGRNDetailData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
        angular.element(document.querySelector('#grnPopUp')).modal('show');
    };

    $scope.SummaryData = function () {
        $scope.getFiltersData();
        angular.element(document.querySelector('#grnSummaryListPopUp')).modal('show');
    }

    $scope.grnCustomerDataList = [];
    $scope.getCustomerData = function () {
        $scope.grnCustomerDataList = [];
        $http({
            method: 'POST',
            url: $scope.path + "GetCustomerDataList",
            data: { 'HeaderId': $scope.MasterFabricRollId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.grnCustomerDataList = response.data;
        });
        var gridObj = $("#GridCustomerData").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        angular.element(document.querySelector('#grnCustomerDataListPopUp')).modal('show');
    }

    $scope.closeGRNCustomerDataPopUp = function () {
        $scope.grnCustomerDataList = [];
        angular.element(document.querySelector('#grnCustomerDataListPopUp')).modal('hide');
    }

    $scope.grnSummaryList = [];
    $scope.GetSummaryList = function () {
        $scope.grnSummaryList = [];
        $scope.filterComplete();
        $http({
            //method: 'POST',
            //url: 'Materials/FabricRoll/GetSummaryList?GRNId=' + $scope.fabricRollMaster.GRNNo + '&GRNRowId=' + $scope.parameters
            method: 'POST',
            url: $scope.path + "GetSummaryList",
            data: { 'parameters': $scope.parameters[0].Value, 'GRNId': $scope.fabricRollMaster.GRNNo },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.grnSummaryList = response.data;
        });
        var gridObj = $("#GridFabricSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    }
 


    $scope.closeGRNSummaryPopUp = function () {
        $scope.grnSummaryList = [];
        $scope.filters = [];
        angular.element(document.querySelector('#grnSummaryListPopUp')).modal('hide');
    }

    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            $http({
                method: 'GET',
                url: 'Materials/FabricRoll/GetFilterList?GRNId='+ $scope.fabricRollMaster.GRNNo,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'GRNRowId', width: 20, headerText: "GRNRowId", type: "string" },
                    { field: 'Article', width: 20, headerText: "Article", type: "string" },
                    { field: 'Material', width: 20, headerText: "Material", type: "string" },
                    { field: 'Qty', width: 20, headerText: "Qty", type: "string" },
                    { field: 'NoOfRoll', width: 20, headerText: "NoOfRoll", type: "string" },
                    { field: 'NoOfPackage', width: 20, headerText: "NoOfPackage", type: "string" }
                ];
                $("#filters").ejGrid({
                    dataSource: $scope.filters,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filters").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filters").children('.e-pager.e-js.e-pager').hide();
                $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filters").children('.e-gridcontent').hide();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "GRNRowId", "Value": getString(fl, "GRNRowId") });
        $scope.parameters = parameters;
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.MarkerGrpValue = "";
    $scope.ShadeGrpValue = "";
    $scope.FabricGroupChange = function (data) {
        try {
            $scope.MarkerGrpValue = "";
            $scope.ShadeGrpValue = "";
            $scope.MarkerGrpValue = data.data.MarkerGroup;
            $scope.ShadeGrpValue = data.data.ShadeGroup;
            data.data.FabricGroup = $scope.MarkerGrpValue + '-' + $scope.ShadeGrpValue;
            var gridObj = $("#GridFabricSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
   
    $scope.MaterialsearchBy = "Material Master";
    $scope.Materialsearch = "";
    $scope.MaterialGridList = [];
    $scope.LoadMaterialSearchList = function () {
        $scope.MaterialGridList = [];
        try {

            $http({
                method: 'POST',
                url: $scope.path + "GetMaterialListData",
                data: { 'inventoryReceiveId': $scope.fabricRollMaster.GRNNo },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.MaterialGridList = [];
                $scope.MaterialGridList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
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

        var filtered = $("#MaterialGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MaterialGridList.length; i++) {
                $scope.MaterialGridList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#MaterialGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all



    //$scope.LoadGRNSearchList();
    $scope.showSingleFabricRollPop = function (data) {
        angular.element(document.querySelector('#SinglefabricRollPopUpargegrfd')).modal('show');
    };

    $scope.GetFabricRollList = [];
    $scope.LoadFabricRollList = function () {
        $scope.GetFabricRollList = [];
        try {

            $http({
                method: 'POST',
                url: $scope.path + "FabricRollList",
                data: { 'inventoryReceiveDetailId': $scope.selectedGRNRow.Id },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.GetFabricRollList = [];
                $scope.GetFabricRollList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.DownloadRollReport = function (data) {
        /* $scope.GetRollDataList = [];*/
        try {
            $scope.selectedGRNRow = data;
            $scope.Index = 0;
            $http({
                method: 'POST',
                url: $scope.path + "DownloadRollReport",
                data: { 'inventoryReceiveDetailId': $scope.selectedGRNRow.Id },
                dataType: 'JSON'

            }).then(function successCallback(response) {


            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //$scope.IRDId = $scope.selectedGRNRow.Id;
    $scope.RR = function (data) {

        try {

            var file_src = $scope.path + 'DownloadRollReport?inventoryReceiveDetailId=' + data.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.POPupReport = function () {

        try {

            var file_src = $scope.path + 'DownloadRollReport?inventoryReceiveDetailId=' + $scope.RowData.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.LoadFabricRollList();
    $scope.Index = 0;
    $scope.EditSingleFabricRoll = function (data) {
        $scope.GetFabricRollList = [];
        try {
            $scope.selectedGRNRow = data;
            $scope.Index = 0;
            $http({
                method: 'POST',
                url: $scope.path + "FabricRollList",
                data: { 'inventoryReceiveDetailId': $scope.selectedGRNRow.Id },
                dataType: 'JSON'

            }).then(function successCallback(response) {

                $scope.GetFabricRollList = [];
                $scope.GetFabricRollList = response.data;
                angular.element(document.querySelector('#SinglefabricRollPopUpargegrfd')).modal('show');

            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }
    function updateSingleData() {

        validFabric();
        var data = [];
        data.push($scope.GetFabricRollList[$scope.Index]);
        $http({
            method: 'POST',
            url: 'Materials/FabricRoll/Update',
            data: { 'FabricRollData': data, 'PackingForm': $scope.PackingformSearchBy },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.getGRNDetail();
                angular.element(document.querySelector('#SinglefabricRollPopUpargegrfd')).modal('show');
                $scope.LoadFabricRollList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }
    $scope.Close = function () {
        angular.element(document.querySelector('#SinglefabricRollPopUpargegrfd')).modal('hide');

    }
    function updateSingleDataAndClose() {

        validFabric();
        var data = [];
        data.push($scope.GetFabricRollList[$scope.Index]);
        $http({
            method: 'POST',
            url: 'Materials/FabricRoll/Update',
            data: { 'FabricRollData': data, 'PackingForm': $scope.PackingformSearchBy },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.getGRNDetail();
                angular.element(document.querySelector('#SinglefabricRollPopUpargegrfd')).modal('hide');
                $scope.LoadFabricRollList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }
    $scope.SaveAndClose = function () {
        updateSingleDataAndClose();

    }
    $scope.Next = function () {
        if ($scope.Index == $scope.GetFabricRollList.length - 1) {
            ShowResult("This is the last position", 'failure');
            return;
        }
        updateSingleData();
        $scope.Index++;
    }
    $scope.Previous = function () {

        if ($scope.Index == 0) {
            ShowResult("This is the first position", 'failure');
            return;
        }
        updateSingleData();
        $scope.Index--;
    }
    //#endregion

    function getGenNo(value) {
        var rvalue = "0";
        while ((rvalue + value).length < 6) {
            rvalue = "0" + rvalue;
        }
        return rvalue + value;
    }
    //#end region
    //#region Employee
    //#region Payroll Group
    $scope.getSavedPayRollGroupData = function () {
        if (!baseService.isUndefinedOrNull($scope.selectedGRNRow.Id)) {
            $http.get("Materials/FabricRollMaster/GetFABRollList?inventoryReceiveDetailId=" + $scope.selectedGRNRow.Id)
                .then(
                    function successCallback(response) {
                        $scope.fabricRollMasterList = response.data.Rows;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        }
    };
    //#end region


    function checkExisting(id) {
        for (var i = 0; i < $scope.selectedGRNList.length; i++) {
            var ob = $scope.selectedGRNList[i];
            if (ob.Id === id) {
                return true;
            }
        }
        return false;
    }

    //#end region

    function validFabric() {
        angular.forEach($scope.fabricRollMasterList, function (item) {
            if (duplicateVendorLotNo($scope.fabricRollMasterList, item.VendorRollNo) === true) {
                throw "Same Vandor Roll no is not allowed";
            }
            if (item.VendorQty === 0 || baseService.isUndefinedOrNull(item.VendorQty)) {
                throw "Vendor quantity can not be zero.";
            }
            if (getTotalSumValue($scope.fabricRollMasterList) > $scope.selectedGRNRow.TransactionQty) {
                throw "Total Vendor quantity can not be greater than item quantity.";
            }
        });
    }
    function duplicateVendorLotNo(list, value) {
        for (var i = 0; i < list.length; i++) {
            if (baseService.isUndefinedOrNull(value)) {
                for (var x = i + 1; x < list.length; x++) {
                    if (!baseService.isUndefinedOrNull(list[i].VendorRollNo) && list[i].VendorRollNo === list[x].VendorRollNo) {
                        return true;
                    }
                }
            }
        }
        return false;
    };
    function getTotalSumValue(list) {
        var tvalue = 0;
        angular.forEach(list, function (item) {
            tvalue += item.VendorQty;
        });
        return tvalue;
    }
    $scope.Update = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            /*if ($scope.fabricRollMasterNewForm.$valid) {*/
            validFabric();
            $http({
                method: 'POST',
                url: 'Materials/FabricRoll/Update',
                data: { 'FabricRollData': $scope.GetFabricRollList, 'PackingForm': $scope.PackingformSearchBy },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.getGRNDetail();
                    angular.element(document.querySelector('#fabricRollPopUp')).modal('hide');
                    $scope.LoadFabricRollList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
            /* }*/
        } catch (e) {
            ShowResult(e, 'failure', 'fabricRollPopUp');
        }
    };
    //Deleting Rows from RetentionAllowanceList
    $scope.message_detailconfirmation = null;
    $scope.removeRoll = function (obj) {

        $scope.Id = obj.data.Id;
        if (!baseService.isUndefinedOrNull($scope.Id))
            $scope.message_detailconfirmation = 'Are you sure you want to delete this Roll: [ ' + obj.data.RollNo + ' ] permanently?';
        angular.element(document.querySelector('#confirmRollDeletePopUp')).modal('show');
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: $scope.deleteUrl + $scope.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //Clear(response.data.Sequence);
                $scope.LoadFabricRollList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });

    };
    //$scope.removeFromDb = function (id, index) {
    //    try {
    //        $http({
    //            method: 'POST',
    //            url: 'Materials/FabricRollMaster/Delete',
    //            dataType: 'JSON',
    //            data: { 'id': id }
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.fabricRollMasters.splice($scope.empIndex, 1);
    //                $scope.paidHoursSavedDataCount--;
    //                $scope.empIndex = -1;
    //                $scope.tempEmpOb.Id = null;
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.status.Message, 'failure');
    //        });
    //        return true;
    //    } catch (e) {
    //        ShowResult(e, 'Error');
    //    }
    //};
    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields() {
        $scope.fabricRollMaster = {};
        $scope.fabricRollMasterNew = {};
        $scope.grnDetailList = [];
        $scope.MaterialGridList = [];
        $scope.valueData = [];
    }
    $scope.getpdff = function (inventoryReceiveDetailId) {
        getPdf(inventoryReceiveDetailId);
        angular.element(document.querySelector('#fabricRollPDFPopUp')).modal('show');
    }
    function getPdf(inventoryReceiveDetailId) {
        $http.get("Materials/FabricRollMaster/GetBarCideList?inventoryReceiveDetailId=" + inventoryReceiveDetailId)
            .then(
                function successCallback(response) {
                    var tttt = response.data;
                    var imgData = tttt;
                    var doc = new jsPDF();
                    var y = 10;
                    var th = 10;
                    var h = 12;
                    angular.forEach(imgData, function (item, i) {
                        //if ((i + 1) % 6 === 0) {
                        //    doc.addPage('1', 'a6');
                        //}
                        doc.setFontSize(15);
                        doc.text(item.GRNNo, y, th);
                        doc.text(item.RollNo, y, th + 10);
                        doc.addImage(item.barCode, 'JPEG', y, th + 12, 50, 10);
                        doc.setFontSize(10);
                        doc.text(item.MaterialName, y, th + 26);
                        doc.text(item.ArticleName, y, th + 30);
                        doc.setFontSize(12);
                        doc.text("Vendor:" + item.Party, y, th + 36);
                        doc.text("Vendor Lot:" + item.VendorLotNo, y, th + 40);
                        doc.text("Vendor Qty:" + item.VendorQty, y + 40, th + 40);
                        doc.text("Shrinkage:" + item.ShrinkagePercentageWidth, y, th + 45);
                        doc.setLineWidth(0.5);
                        doc.line(10, th + 50, y, th + 50); // horizontal line
                        y += 80;
                        if ((i + 1) % 2 === 0 && i !== 0) {
                            var tth = th;
                            th += (th + 10 + 8 + 10 + 30) - tth;
                            y = 10;

                        }

                    });
                    pdf_test_harness_init(doc, null);
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }

    $scope.grnDetailParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'MaterialMasterName',
        searchBy: 'MaterialMasterName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.searchGRNDetailByList = [
        {
            name: 'Material Master',
            value: 'MaterialMasterName'
        },
        {
            name: 'Party',
            value: 'PartyName'
        }
    ];

    $scope.PackingformSearchBy = "";
    $scope.Packingformsearch = "";
    $scope.PackingformList = [
        {
            name: 'Roll',
            value: 'Roll'
        },
        {
            name: 'Bale',
            value: 'Bale'
        }
    ];

    $scope.GetFromToDate = function () {
        $http({
            method: 'Get',
            url: 'Materials/FabricRoll/GetFromToDate'
        }).then(function (response) {
            $scope.FromDate = response.data[0].FromDate;
            $scope.ToDate = response.data[0].ToDate;
        });
    };
    $scope.GetFromToDate();

    $scope.GRNGridList = [];
    $scope.LoadGRNSearchList = function () {
        $scope.GRNGridList = [];
        try {

            $http({
                method: 'POST',
                url: $scope.path + "GRNList",
                data: { 'column': $scope.GRNsearchBy, 'value': $scope.GRNsearch, 'fromDate': $scope.FromDate, 'toDate': $scope.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.GRNGridList = [];
                $scope.GRNGridList = response.data;
            });
            angular.element(document.querySelector('#grnListPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //File Upload
    $scope.FabricRollFile = {
        Id: null,
        FileId: null,
        FileName: null,
        FileStatus: null,
        PlantId: $window.plantId,
    }

    $rootScope.title = 'Fabric Roll File Upload';

    $("#uploadRollData").change(function () {
        $scope.RollData = this.files[0];
    });

    $scope.saveRollFile = function () {
        try {
            if ($scope.RollData != null) {
                var RollData = new FormData();
                //if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'Materials/FabricRoll/CreateRollFile',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        RollData.append("FabricRollFile", angular.toJson(data.FabricRollFile));
                        if (baseService.isUndefinedOrNull($scope.RollData) === false) {
                            RollData.append('file', data.file);
                        }
                        return RollData;
                    },
                    data: {
                        'FabricRollFile': $scope.FabricRollFile,
                        'file': $scope.RollData
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getMaster();
                        document.getElementById("uploadRollData").value = '';
                    }
                }, function errorCallback(response) {
                    $scope.savedisable = false;
                    $scope.showdiv = false;
                });
                return true;
                //}
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    function GetShortList(list) {
        var list2 = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === null || list[i].Id === '' || list[i].Id === 'undefined') {

            }
            else {
                list2.push(list[i]);
            }
        }
        return list2;
    }
    $scope.buyerNew = {
        FileName: null
    }
    
    $scope.fileName = $scope.fabricRollMaster.GRNNo + '-' + "Fabric Roll Management Template.xlsx";
    $scope.ModelNew = { FileName: null };
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.MaterialGridTempList = [];
    $scope.GetSampleFile = function () {
        try {
            $scope.fileName = $scope.fabricRollMaster.GRNNo + '-' + "Fabric Roll Management Template.xlsx";

            $scope.MaterialGridTempList = [];
            for (var i = 0; i < $scope.MaterialGridList.length; i++) {
                if ($scope.MaterialGridList[i].Flag == true) {
                    var ob = {};
                    ob.Id = $scope.MaterialGridList[i].Id;
                    ob.RollNo = $scope.MaterialGridList[i].RollNo;
                    $scope.MaterialGridTempList.push(ob);
                    ob = {};
                }
            }

            if (baseService.arrayLength($scope.MaterialGridTempList) == 0) {
                throw "Please select data.";
            }

            for (var i = 0; i < $scope.MaterialGridTempList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.MaterialGridTempList[i].RollNo)) {
                    throw "Roll No is required.";
                }
            }


            var ReportFormat = 'Excel';
            $http({
                method: 'POST',
                url: 'Materials/FabricRoll/GetSampleFile',
                data: {
                    'reportFormat': ReportFormat, 'GridTempList': $scope.MaterialGridTempList, 'fabricRollMaster': $scope.fabricRollMaster
                },
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.grnDetailList = [];
    $scope.ImportData = function () {
        try {
            $scope.msg = "";

            var picData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.picdata)) {
                $scope.ModelNew.FileName = $scope.picdata.name;
            }


            $http({
                method: 'POST',
                url: 'Materials/FabricRoll/ImportData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    picData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                        picData.append('file', data.file);
                    }
                    return picData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");

                }
                else {
                    // $scope.grnDetailList = [];
                    var x = GetShortList(response.data);
                    //if (baseService.arrayLength($scope.grnDetailList) > 0) {
                    //    for (var i = 0; i < x.length; i++) {
                    //        x[i].Id = null;
                    //        $scope.grnDetailList.push(x[i]);
                    //    }
                    //} else {

                    //    $scope.grnDetailList = x;
                    //}
                    for (var i = 0; i < x.length; i++) {
                        x[i].Id = null;
                        $scope.grnDetailList.push(x[i]);
                    }
                    $scope.ShowSaveBtn = true;
                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.employee = [];
    $scope.name = null;
    $scope.getPopUpData = function (name) {
        $scope.employee = [];
        $scope.name = name;

        $http({
            method: 'GET',
            url: 'HumanResource/leaveApplicationNew/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.setEmpData = function (obj) {
        var data = obj.data;
        if ($scope.name == 'Preparedby') {
            $scope.fabricRollMaster.PreparedByCode = data.EmployeeCode;
            $scope.fabricRollMaster.PreparedById = data.SystemID;
            $scope.fabricRollMaster.PreparedByName = data.EmployeeName;
        }
        else {
            $scope.fabricRollMaster.CheckedByCode = data.EmployeeCode;
            $scope.fabricRollMaster.CheckedById = data.SystemID;
            $scope.fabricRollMaster.CheckedByName = data.EmployeeName;
        }
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');

    };

    $scope.ClearEmpdata = function (name) {
        $scope.name = name;
        if ($scope.name == 'Preparedby') {
            $scope.fabricRollMaster.PreparedByCode = null;
            $scope.fabricRollMaster.PreparedById = null;
            $scope.fabricRollMaster.PreparedByName = null;
        }
        else {
            $scope.fabricRollMaster.CheckedByCode = null;
            $scope.fabricRollMaster.CheckedById = null;
            $scope.fabricRollMaster.CheckedByName = null;
        }
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.modeldata = {
        Id: null, PlantId: null, GRNId: $scope.fabricRollMaster.GRNNo, GRNDate: $scope.fabricRollMaster.GRNDate, UserName: $scope.fabricRollMaster.UserName, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    }

    $scope.Action = "Save";
    $scope.SaveRollData = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.fabricRollMaster.GRNNo)) {
                throw "Please select GRN No.";
            }
            if (baseService.isUndefinedOrNull($scope.fabricRollMaster.PreparedById)) {
                throw "Prepared By is required.";
            }
            if (baseService.isUndefinedOrNull($scope.fabricRollMaster.CheckedById)) {
                throw "Checked By is required.";
            }
            if (baseService.isUndefinedOrNull($scope.fabricRollMaster.UserName)) {
                throw "User Reference is required.";
            }
            //$scope.modeldata.Id = $scope.fabricRollMaster.Id;
            //$scope.modeldata.GRNId = $scope.fabricRollMaster.GRNNo;
            //$scope.modeldata.GRNDate = $scope.fabricRollMaster.GRNDate;
            //$scope.modeldata.PreparedById = $scope.fabricRollMaster.PreparedById;
            //$scope.modeldata.CheckedById = $scope.fabricRollMaster.CheckedById;
            //$scope.modeldata.Remarks = $scope.fabricRollMaster.Remarks;
            //$scope.modeldata.Comment = $scope.fabricRollMaster.Comment;

            $scope.modeldata = Object.assign({}, $scope.fabricRollMaster);
            if (baseService.isUndefinedOrNull($scope.modeldata.Id)) {
                $scope.modeldata.Id = null;
            }

            if (baseService.arrayLength($scope.grnDetailList) == 0) {
                throw "Detail list is requird.";
            }

            $http({
                method: "POST",
                url: 'Materials/FabricRoll/CreateFabricRollManage',
                data: {
                    "data": $scope.modeldata
                    , "grnDetailList": $scope.grnDetailList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.fabricRollMaster.Id = response.data.Data.Id;
                    $scope.getSaveChildData($scope.fabricRollMaster.Id);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //#region FabricRollFile upload

    $scope.onBeginPBUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.fabricRollMaster.Id))
                throw 'Please select/save the Fabric Roll first'

            args.data = $scope.fabricRollMaster.Id;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadPBUrl = "Materials/FabricRoll/SaveFabricRollFileDefault";

    $scope.getFileList = function () {
        $http({
            method: 'POST', url: 'Materials/FabricRoll/GetFileInfo', dataType: 'JSON',
            data: { Id: $scope.fabricRollMaster.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                var str = response.data[0].PicFileName;
                var extention = str.substr(str.indexOf('.'));
                $scope.PicFileName = virtualPath.FabricRollFile + '/' + $scope.fabricRollMaster.Id + extention;
                //$scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }


    $scope.fileselect = function (e) {

    }
    $scope.errorPBPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.fabricRollMaster.Id))
            ShowResult('Please select/save GRN No first', 'Error');
        //else
        //    ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    //#endregion Meeting Points Picture upload

    $scope.UpdateFabricDetails = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.grnSummaryList.length; i++) {
                if ($scope.grnSummaryList[i].MarkerGroup !== null && $scope.grnSummaryList[i].FabricGroup !== null && $scope.grnSummaryList[i].ShadeGroup !== null) {
                    $scope.SaveList.push($scope.grnSummaryList[i]);
                }
                else
                {
                    throw "Please Update MarkerGroup,FabricGroup and ShadeGroup then proceed....";
                }
            }
            $http({
                method: 'POST',
                url: $scope.updateUrlFabricDetails,
                data: {
                    "DataList": $scope.SaveList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'updated');
                    $scope.GetSummaryList();
                    $scope.Action = 'Update';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.refreshTemplateCustomerData = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllCustomerData });
    };
    function CheckBoxSelectAllCustomerData(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridCustomerData").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.grnCustomerDataList.length; i++) {
                $scope.grnCustomerDataList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridCustomerData").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SaveCustomerData = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.grnCustomerDataList.length; i++) {
                if ($scope.grnCustomerDataList[i].Flag == true) {
                    $scope.grnCustomerDataList[i].HeaderId = $scope.MasterFabricRollId;
                    $scope.SaveList.push($scope.grnCustomerDataList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlCustomer,
                data: {
                    "DataList": $scope.SaveList,
                    "Pid": $scope.MasterFabricRollId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
}
