'use strict';
FabricRollController.$inject = ['commonMessage', '$controller', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function FabricRollController(commonMessage, $controller, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Fabric Roll Master";
    $scope.Action = 'Save';
    $scope.fabricRollMasters = [];
    $scope.selectedGRNList = [];
    $scope.path = 'Materials/FabricRoll/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.fabricRollMaster = {
        CompanyGroupId: $window.companyGroupId,
        PlantId: $window.plantId,
        InventoryReceiveId: null,
        GRNNo: null,
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
        , LCDate: null
        , OpeningBank: null
    };

    $scope.fabricRollSplitOb = {
        VendorWidth: null
    }
    $scope.fabricRollMasterNew = Object.assign({}, $scope.fabricRollMaster);
    //#region Fabric Roll Pop Up
    $scope.selectedGRNRow = {};
    $scope.fabDistributeQty = 0;
    $scope.fabricEdit = false;
    $scope.showFabricPop = function (data) {
      
        $scope.fabricRollSplitOb.VendorWidth = null;
        //$scope.fabricEdit = isEdit;
        $scope.fabricRollMasterNew.GRNSplitQty = null;
        $scope.fabricRollMasterList = [];
        $scope.selectedGRNRow = data;
        $scope.fabDistributeQty = data.TotalDistributeQty;
        if ($scope.fabricEdit) {
            $scope.getSavedPayRollGroupData();
        }
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
                data: { 'InventoryReceiveDetailId': $scope.selectedGRNRow.Id, 'NoofRolls': $scope.fabricRollMasterNew.GRNSplitQty, 'Qty': $scope.selectedGRNRow.TransactionQty},
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.saveRollList = [];
                $scope.saveRollList = response.data;
               
           
            });
        }
    }
    $scope.clearSplitRow = function () {
        $scope.fabricRollMasterNew.GRNSplitQty = null;
        $scope.fabricRollMasterList = [];
    }
    //#region GRN Load
    //$scope.grnList = [];
    //$scope.getGRNDataList = function () {
    //    try {
    //        $scope.grnParameters = {
    //            limit: 10,
    //            offset: 0,
    //            order: 'asc',
    //            sort: 'GRNDate',
    //            searchBy: 'GRNDate',
    //            pageSize: 10,
    //            total_count: 0,
    //            search: null,
    //            serverPagination: true
    //        };
          

    //        $scope.popUpUrl = '';
    //        $scope.popUpUrl = 'Materials/FabricRollMaster/GetGRNList';
    //        $scope.getGRNData = function (pageno) {
    //            baseService.paginationBase($scope.popUpUrl, pageno, $scope.grnParameters)
    //                .then(function (result) {
    //                    $scope.grnList = result.Rows;
    //                    $scope.grnParameters.total_count = result.Total;
    //                }, function () {
    //                    ShowResult(commonMessage.NetworkError, 'failure', '#grnPopUp');
    //                }).finally(function () {
    //                });
    //        };

    //        $scope.fieldName = name;
    //        $scope.getGRNData();
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //};
    //$scope.getGRNDataList();
   // #endregion GRN Load

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
    $scope.GRNGridList = [];
    $scope.LoadGRNSearchList = function () {             
        $scope.GRNGridList = [];
                try {
                    //if ($scope.GRNsearch == '')
                    //    throw "Please insert search value.";
                    $http({
                        method: 'POST',
                        url: $scope.path + "GRNList",
                        data: { 'column': $scope.GRNsearchBy, 'value': $scope.GRNsearch },
                        dataType: 'JSON'

                    }).then(function successCallback(response) {
                        $scope.GRNGridList = [];
                        $scope.GRNGridList = response.data;
                    });
                }
                catch (e) {
                    ShowResult(e, 'failure');
                }
    }
    $scope.LoadGRNSearchList();

    $scope.Get = function (args) {

        $scope.fabricRollMaster = Object.assign({}, args.data);
        //$scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.LoadMaterialSearchList();
    };

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



    $scope.MaterialsearchBy = "Material Master";
    $scope.Materialsearch = "";
    $scope.MaterialGridList = [];
    $scope.LoadMaterialSearchList = function () {
        $scope.MaterialGridList = [];
        try {
         
            $http({
                method: 'POST',
                url: $scope.path + "MaterialList",
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
    //$scope.LoadGRNSearchList();




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
   function duplicateVendorLotNo (list, value) {
       for (var i = 0; i < list.length; i++) {
           if (baseService.isUndefinedOrNull(value)) {
               for (var x = i + 1; x < list.length; x++) {
                   if (!baseService.isUndefinedOrNull(list[i].VendorRollNo)&& list[i].VendorRollNo === list[x].VendorRollNo) {
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
    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            /*if ($scope.fabricRollMasterNewForm.$valid) {*/
                validFabric();
            $http({
                method: 'POST',
                url: 'Materials/FabricRoll/Save',
                data: { 'FabricRollData' : $scope.fabricRollMasterList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        //$scope.getGRNDetail();
                        angular.element(document.querySelector('#fabricRollPopUp')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
           /* }*/
        } catch (e) {
            ShowResult(e, 'failure','fabricRollPopUp');
        }
    };
    //Deleting Rows from RetentionAllowanceList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempFabOb = data;
        $scope.tempFabIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeRow = function () {
        $scope.fabricRollMasterList.splice($scope.tempFabIndex, 1);
        $scope.tempFabIndex = -1;
        $scope.tempFabOb.Id = null;
        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
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
    function ClearFields(seq) {
        $scope.fabricRollMaster = {};
        $scope.fabricRollMasterNew = {};
        $scope.fabricRollMasterHeadList = [];
        $scope.popUpList = [];
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
                    doc.text(item.RollNo, y, th+10);
                    doc.addImage(item.barCode, 'JPEG', y, th+12, 50, 10);
                    doc.setFontSize(10);
                    doc.text(item.MaterialName, y, th+26);
                    doc.text(item.ArticleName, y, th+30);
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

}
