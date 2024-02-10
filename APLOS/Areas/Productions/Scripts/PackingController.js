'use strict';
PackingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PackingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Packing';
    $scope.path = 'Productions/Packing/';

    $scope.searchBy = "Customer"; $scope.search = "";
    $scope.searchByList = [{ value: 'PO', name: "PO" }, { value: 'Customer', name: "Customer" }, { value: 'Productcode', name: "Product Code" }];


    
    $scope.content = "Learn the fundamentals of Delphi to build a variety of solutions for many devices and platforms";
  




    var x = document.getElementById("FDiv");
    var z = document.getElementById("TDiv");
    var u = document.getElementById("UDiv");


    //var rpt = document.getElementById("reportBtn");
    //var searchLoc = document.getElementById("listLoc");
    //rpt.style.display = "none";
    //searchLoc.style.display = "none";

    var a = document.getElementById("Filters");
    var b = document.getElementById("Filters2");
    var c = document.getElementById("Date");
    x.style.display = "block";
    z.style.display = "none";
    u.style.display = "none";
    a.style.display = "block";
    b.style.display = "block";
    c.style.display = "block";
    $scope.clickdde1 = function () {
        if (x.style.display === "none") {
            z.style.display = "none";
            x.style.display = "block";
            u.style.display = "none";
            a.style.display = "block";
            b.style.display = "block";
            c.style.display = "block";
        }
    };
    
    $scope.clickdde3 = function () {
        if (z.style.display === "none") {

            z.style.display = "block";
            x.style.display = "none";
            u.style.display = "none";
            a.style.display = "none";
            b.style.display = "none";
            c.style.display = "none";
            
        }
    };
    $scope.clickdde4 = function () {
        if (u.style.display === "none") {
            
            $scope.getgridPacking();
            z.style.display = "none";
            x.style.display = "none";
            u.style.display = "block";
            a.style.display = "none";
            b.style.display = "none";
            c.style.display = "none";
            
        }
    };

    $scope.selectedValues = {
       
        FromDate: null,
        ToDate: $filter("date")(Date.now(), "dd-MMM-yyyy"),
    };

    $scope.list = [
        { text: "All", value: "All" },
        { text: "Assigned", value: "Assigned" },
        { text: "Unassigned", value: "Unassigned" },



    ];
    $scope.list1 = [
        { text: "With Stock", value: "WithStock" },
        { text: "Stock With SO", value: "SOStock" },
        { text: "Stock Without SO", value: "SONoStock" },
        { text: "ALL", value: "All" },
    ];

    $rootScope.typeText = "All";
    $rootScope.typeVal = "All";

    $rootScope.groupText = "With Stock";
    $rootScope.groupVal = "WithStock";
    $scope.change = function () {
        var obj = $('#dropdown1').data("ejDropDownList");
        $rootScope.typeText = obj.option("text");
        $rootScope.typeVal = obj.option("value");
    }
    $scope.change1 = function () {
        var obj = $('#dropdown2').data("ejDropDownList");
        $rootScope.groupText = obj.option("text");
        $rootScope.groupVal = obj.option("value");
    }

    $scope.PurposeList = [];
    $scope.GetPurpose = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getPurposeCategory',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.PurposeList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.GetPurpose();

    $scope.validations = function () {

        
        if (angular.isUndefinedOrNull($scope.selectedValues.FromDate) == true) {
            ShowResult("Please select From Date");
            throw ("Please select From Date");
        }
        if (angular.isUndefinedOrNull($scope.selectedValues.ToDate) == true) {
            ShowResult("Please select To Date");
            throw ("Please select To Date");
        }


    }

    //Filling the locations list

    $scope.LocList = [];

    function getLocations() {
        $http({
            method: 'GET',
            url: $scope.path + "getLocations"
        }).then(function succ(resp) {
            $scope.LocList = resp.data;
        })
    }
    getLocations();

    $rootScope.LocName = "All";
    $rootScope.LocId = "All";
    
    $scope.LocChange = function () {
        var obj = $('#listLoc').data("ejDropDownList");
        $rootScope.LocName = obj.option("text");
        $rootScope.LocId = obj.option("value");
    }

    //Getting the stock status grid

    $scope.getData = function () {

        $scope.validations();

        //if (x.style.display == "none") {

        //    $scope.getGridOne();
        //}
        //else {

            $http({
                method: 'POST',
                url: $scope.path + "GetList",
                data: {
                    'ToDate': $scope.selectedValues.ToDate, 'FromDate': $scope.selectedValues.FromDate,
                    'type': $rootScope.typeVal, 'group': $rootScope.groupVal, 'value': $scope.search, 'column': $scope.searchBy,
                    'Loc': $scope.LocId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.MainData = response.data.DATA;
                    var cols = response.data.Cols;

                    var ColumnList = [

                        { field: 'Assigned', width: 150, headerText: "Assigned", type: "string" },
                        { field: 'ProductCode', width: 150, headerText: "Product Code", type: "number" },
                        { field: 'PO', width: 120, headerText: "Production Order", type: "number" },
                        { field: 'LotNo', width: 150, headerText: "Lot No", type: "number" },
                    ];

                    //for (var i = 0; i < cols.length; i++) {
                    //    ColumnList.push({ field: cols[i], width: 150, headerText: cols[i], type: "string" });
                    //}


                    ColumnList.push({ field: 'fd', width: 150, headerText: "From Date", type: "number" });
                    ColumnList.push({ field: 'fp', width: 150, headerText: "From Period", type: "number" });
                    ColumnList.push({ field: 'ud', width: 150, headerText: "Upto Date", type: "number" });
                    ColumnList.push({ field: 'Despatch', width: 150, headerText: "Despatch Qty", type: "number" });
                    ColumnList.push({ field: 'PlannedQty', width: 150, headerText: "Planned Qty", type: "number" });
                    ColumnList.push({ field: 'BookedQty', width: 150, headerText: "Booked Qty", type: "number" });
                    ColumnList.push({ field: 'StockQty', width: 150, headerText: "Stock Qty", type: "number" });
                    ColumnList.push({ field: 'Available', width: 150, headerText: "Available", type: "number" });
                    ColumnList.push({ field: 'SoQty', width: 150, headerText: "Sales Order Qty", type: "number" });
                    ColumnList.push({ field: 'NoOfSo', width: 150, headerText: "NO Of Sales Order", type: "number" });
                    ColumnList.push({ field: 'ItemId', width: 120, headerText: "Item Id", type: "string" });
                    ColumnList.push({ field: 'ItemArticle', width: 150, headerText: "Item Article", type: "string" });
                    ColumnList.push({ field: 'Product', width: 150, headerText: "Product", type: "string" });
                    ColumnList.push({ field: 'MasterOrderNo', width: 150, headerText: "Master Order No", type: "string" });
                    ColumnList.push({ field: 'Customer', width: 150, headerText: "Customer", type: "string" });


                    $("#GridData").ejGrid({
                        dataSource: $scope.MainData,
                        minWidth: 450, minHeight: 400,
                        allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                        filterSettings: { filterType: "excel" },
                        recordDoubleClick: $scope.showSOData,
                        columns: ColumnList,
                    });


                    var gridObj = $("#GridData").data("ejGrid");
                    gridObj.refreshContent(true);
                    gridObj.refreshTemplate();


                    //rpt.style.display = "block";
                    //searchLoc.style.display = "block";


                }
            });
        
    }

    $scope.seeChecked = function () {
        for (var i = 0; i < $scope.MainData.length; i++) {
            if ($scope.MainData[i]["checked"] == true) {
                console.log($scope.MainData[i]);
            }
        }
    }

    $scope.checkBoxChange = function (ProductCode, PO) {
        console.log($scope.clickData);
    }


    $scope.clickData = [];
    $scope.showSOData = function (e) {
        $scope.clickData = [];

        var po = e.data["PO"];
        var prodCode = e.data["ProductCode"];
        $http({
            method: 'POST',
            url: $scope.path + "getClickData",
            data: {
                'poid': po, 'productCode': prodCode
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.clickData = response.data;
            var gridObj = $("#soClickGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            angular.element(document.querySelector('#packingModal')).modal('show');
        });




    }

    $scope.productCodes = "";
    //$scope.gridOne = [];
    //$scope.getGridOne = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "getSOfromProduct",
    //        params: { 'column': $scope.searchBy, 'value': $scope.search },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.gridOne = response.data;
    //        $scope.productCodes = getString($scope.gridOne, "ProductCode");

    //        $http({
    //            method: 'GET',
    //            url: $scope.path + "getpackingGridOne",
    //            params: { 'productCode': $scope.productCodes },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            $scope.gridDown = response.data;
    //        });

    //    });
    //}

    

    //$scope.gridDown = [];
    //$scope.getGridDown = function () {
    //    var pos = "''";
    //    for (var i = 0; i < $scope.gridOne.length; i++) {
    //        if ($scope.gridOne[i]["checked"] == true) {
    //            pos = pos + ',' + "'" + $scope.gridOne[i]["ProductCode"] + "'";
    //        }
    //    }
    //    if (pos == "''") {
    //        pos = $scope.productCodes;
    //    }
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "getpackingGridOne",
    //        params: { 'productCode': pos },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.gridDown = response.data;
    //    });

    //}

    //$scope.moWithCustomer = [];
    //$scope.getMoWithCustomer = function () {
    //    var cus = "''";
    //    for (var i = 0; i < $scope.gridOne.length; i++) {
    //        if ($scope.gridOne[i]["checked"] == true) {
    //            cus = cus + ',' + "'" + $scope.gridOne[i]["CustomerId"] + "'";
    //        }
    //    }
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "getMOwithCustomers",
    //        params: { 'Customers': cus },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.moWithCustomer = response.data;
    //        angular.element(document.querySelector('#moWithCustomerModal')).modal('show');
    //    });


    //}

    //$scope.clear = function () {
    //    for (var i = 0; i < $scope.gridOne.length; i++) {
    //        if ($scope.gridOne[i]["checked"] == true) {
    //            $scope.gridOne[i]["checked"] = false;
    //        }
    //    }
    //}

    //Start for the 3rd Screen

    $scope.Packing = {
        PackingId: null,
        Date: $filter("date")(Date.now(), "dd-MMM-yyyy"),
        ByWhom: null,
        InactiveDate: $filter("date")(Date.now(), "dd-MMM-yyyy")+1,
        Remarks: null,
        CustomerId: null,
        StorageLocId: null,
        EntityId: null,
        DispatchResponsiblePersonId: null,
    };

    $scope.loadInactiveDate = function () {
        var med = new Date().setDate(new Date().getDate() + 1);
        $scope.Packing.InactiveDate = $filter('dateFiltering')(new Date(med), 'dd-MM-yyyy');

    }
    $scope.loadInactiveDate()
    $scope.PackingLineItem = {
        PackingId: null,
        PackingLineItemId: null,
        SOId: null,
    };

    $scope.POLotRef = {
        Id: null,
        PackingLineItemId: null,
        ProductCode: null,
        PONo: null,
        LotNo: null,
        BookQty: null,
        Remarks: null,
        PlanQty: null,
        Available: null,
        Status: "Active",
    };


    $scope.lastIndex = 0;
    //The Selections for the Packing Table


    $scope.CustomersList = [];
    $scope.EmployeesList = [];
    $scope.ByWhomEmployeesList = [];
    $scope.EntityList = [];
    $scope.storageLocList = [];
    $http({
        method: 'GET',
        url: $scope.path + "getCustomers",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.CustomersList = response.data;
    });

    $http({
        method: 'GET',
        url: $scope.path + "getEmployees",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.EmployeesList = response.data;
    });

    $http({
        method: 'GET',
        url: $scope.path + "getByWhomEmployees",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.ByWhomEmployeesList = response.data;
    });

    $http({
        method: 'GET',
        url: $scope.path + "getEntity",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.EntityList = response.data;
    });
    $http({
        method: 'GET',
        url: $scope.path + "getStorageLoc",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.storageLocList = response.data;
    });

    //End of that


    $scope.byWhom = "";
    $scope.customer = "";
    $scope.dRespPerson = "";

    $scope.selectCustomer = function () {
        angular.element(document.querySelector('#customersModal')).modal('show');
    }

    $scope.selectDRespPerson = function () {
        angular.element(document.querySelector('#dRespPersonModal')).modal('show');
    }

    $scope.selectByWhom = function () {
        angular.element(document.querySelector('#byWhomModal')).modal('show');
    }

    //The Double Clicks and the PackingLineItemGrid
    
    $scope.PackingLineItemGrid = [];



    $scope.doubleCustomer = function (e) {
        $scope.Packing = {
            PackingId: null,
            Date: null,
            ByWhom: null,
            InactiveDate: null,
            Remarks: null,
            CustomerId: null,
            StorageLocId: null,
            EntityId: null,
            DispatchResponsiblePersonId: null,
        };
        $scope.loadInactiveDate();
        $scope.customer = e.data.username;
        $scope.Packing.CustomerId = e.data.id;
        angular.element(document.querySelector('#customersModal')).modal('hide');
        $scope.getSoFromCustomerList();
        $scope.lastIndex = 0;
    }
    $scope.doubleByWhom = function (e) {
        $scope.byWhom = e.data.EmployeeName;
        $scope.Packing.ByWhom = e.data.SystemId;
        angular.element(document.querySelector('#byWhomModal')).modal('hide');

    }
    $scope.doubleDRespPerson = function (e) {
        $scope.dRespPerson = e.data.EmployeeName;
        $scope.Packing.DispatchResponsiblePersonId = e.data.SystemId;
        angular.element(document.querySelector('#dRespPersonModal')).modal('hide');
    }

    //Filling of SO List
    $scope.getSoFromCustomerList = function () {
        $http({
            method: 'GET',
            url: $scope.path + "getSoFromCustomer",
            params: { 'customer': $scope.Packing.CustomerId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PackingLineItemGrid = response.data;
        });
    }



    //Filling of the PoLotRefGrid
    $scope.PoLotRefGrid = [];
    $scope.toDisp = 0;
    $scope.totalPlanned ="";
    $scope.getLotRef = function () {

        var counter = 0;
        for (var i = 0; i < $scope.PackingLineItemGrid.length; i++) {
            if ($scope.PackingLineItemGrid[i]["checked"] == true) {
                counter++;
            }
        }

        if (angular.isUndefinedOrNull($scope.selectedValues.ToDate) == true) {
            ShowResult("Please select To Date");
            throw ("Please select To Date");
        }

        if (counter == 0) {
            ShowResult("Please Select SO");
            throw ("Please Select SO");
        }
        if (counter > 1) {
            ShowResult("Please Select only 1 SO");
            throw ("Please Select only 1 SO");
        }

        $scope.PoLotRefGrid = [];
        $scope.cartonCollection = [];
        var cus = "";
        var toDisp = "";
        var po = "";
        for (var i = 0; i < $scope.PackingLineItemGrid.length; i++) {

            if ($scope.PackingLineItemGrid[i]["checked"] == true) {
                var cus = "";
                $scope.toDisp = "";
                $scope.totalPlanned = 0;
                var po = "";
                cus = $scope.PackingLineItemGrid[i]["ProductCode"];
                $scope.toDisp = $scope.PackingLineItemGrid[i]["toDespatch"];
                po = $scope.PackingLineItemGrid[i]["PO"];
            }
        }

        $http({
            method: 'GET',
            url: $scope.path + "getPoLotReference",
            params: { 'productCode': cus, 'toDispatch': toDisp, 'PO': po, 'FromDate': $scope.selectedValues.FromDate, 'ToDate': $scope.selectedValues.ToDate },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PoLotRefGrid = [];
            $scope.PoLotRefGrid = response.data;
            $scope.PoLoTCollection = [];
            $scope.cartonCollection = [];

            //$http({
            //    method: 'GET',
            //    url: $scope.path + "getCartonsDetails",
            //    params: { 'LotNo': $scope.PoLotRefGrid[0]["LotNo"], 'Qty': $scope.PoLotRefGrid[0]["quant"] },
            //    dataType: 'JSON'
            //}).then(function successCallback(response) {
            //    $scope.cartonDetail = response.data.Data;

            //    $scope.PoLotRefGrid[0]["quant"] = response.data.quant;

            //    $scope.cartonClose();
            //});
        });
    }
    // Filling the Qnty
    $scope.Percent = 0;
    $scope.calculateQty = function () {
        $scope.totalPlanned = 0;
        var j = 0;
        for (var i = 0; i < $scope.PoLotRefGrid.length; i++) {
            if ($scope.PoLotRefGrid[i]["checked"] == true) {
                j = j + $scope.PoLotRefGrid[i]["quant"];
            }
        }

        $scope.totalPlanned = j;
        var kk = 0;
        if ($scope.totalPlanned == 0) {
            kk = 0;
        }
        else {
            kk = ($scope.totalPlanned / $scope.toDisp ) * 100;
        }
         
        $scope.Percent = kk.toFixed(2);
    }

   
    //Double Click on The POLotRefGrid
    $scope.cartonDetail = [];
    $scope.inactiveCartons = [];
    $scope.showCartons = function (e) {
        try {
            //if (baseService.isUndefinedOrNull(e.data.QualityStatus) !=="Pass") {
            //    throw "This Item is  not pass the quality.";
            //}
            if (parseFloat(e.data.quant) <= 0) {
                throw "Please First Enter the Plan Qty";
            }
            if ($scope.cartonDetail.length > 0) {
                if ($scope.cartonDetail[0]["LotNo"] === e.data.LotNo) {
                    $scope.cartonDetal = $scope.cartonDetail;
                    $scope.inactiveCartons = $scope.inactiveCartons;
                    angular.element(document.querySelector('#cartonDetailModal')).modal('show');
                }
                else {
                    $http({
                        method: 'GET',
                        url: $scope.path + "getCartonsDetails",
                        params: { 'LotNo': e.data.LotNo, 'ProductCode': e.data.ProductCode, 'PO': e.data.PO },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        $scope.cartonDetail = response.data.Data;

                        $scope.inactiveCartons = response.data.Inactive;
                        fillCartons();
                        angular.element(document.querySelector('#cartonDetailModal')).modal('show');
                    });

                }
            }
            else {
                $http({
                    method: 'GET',
                    url: $scope.path + "getCartonsDetails",
                    params: { 'LotNo': e.data.LotNo, 'ProductCode': e.data.ProductCode, 'PO': e.data.PO },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.cartonDetail = response.data.Data;
                    $scope.inactiveCartons = response.data.Inactive;
                    angular.element(document.querySelector('#cartonDetailModal')).modal('show');
                });
            }
        } catch (e) {
            ShowResult(e,'failure');
        }



    }

    //Filling the Cartons in the Grid
    function fillCartons() {
        var j = -1;
        for (var i = 0; i < $scope.cartonCollection.length; i++) {
            if ($scope.cartonCollection[i]["LotNo"] == $scope.cartonDetail[0]["LotNo"] && $scope.cartonCollection[i]["ProductCode"] == $scope.cartonDetail[0]["ProductCode"] && $scope.cartonCollection[i]["PO"] == $scope.cartonDetail[0]["POId"]) {
                j = i;
                break;
            }
        }

        if (j != -1) {
            var k = j;
            for (var i = 0; i < $scope.cartonDetail.length; i++) {
                if ($scope.cartonCollection[k]["RefNo"] == $scope.cartonDetail[i]["RefNo"]) {
                    $scope.cartonDetail[i]["checked"] = true;
                    k++;
                }
            }
        }
    }


    //Closing of The Carton Details Modal
    $scope.cartonCollection = [];
    $scope.cartonClose = function () {

        var jj = parseInt(0.0);
        var j = $scope.cartonDetail.length - 1;
        if ($scope.cartonCollection.length > 0) {
            for (var i = $scope.cartonCollection.length - 1; i >= 0; i--) {
                if ($scope.cartonCollection[i]["LotNo"] == $scope.cartonDetail[j]["LotNo"] && $scope.cartonCollection[i]["ProductCode"] == $scope.cartonDetail[j]["ProductCode"] && $scope.cartonCollection[i]["PO"] == $scope.cartonDetail[j]["POId"]) {
                    $scope.cartonCollection.splice(i, 1);
                    j--;
                }
            }
        }


        for (var i = 0; i < $scope.cartonDetail.length; i++) {
            if ($scope.cartonDetail[i]["checked"] == true) {
                if ($scope.cartonCollection.includes($scope.cartonDetail[i]["RefNo"]) == false) {
                    $scope.cartonCollection.push({ ProductCode: $scope.cartonDetail[i]["ProductCode"] , PO: $scope.cartonDetail[i]["POId"],LotNo: $scope.cartonDetail[i]["LotNo"], RefNo: $scope.cartonDetail[i]["RefNo"] });
                    jj = jj + $scope.cartonDetail[i]["NetWeight"];
                }
            }
        }


        for (var i = 0; i < $scope.PoLotRefGrid.length; i++) {
            if ($scope.PoLotRefGrid[i]["LotNo"] == $scope.cartonDetail[0]["LotNo"] && $scope.PoLotRefGrid[i]["ProductCode"] == $scope.cartonDetail[0]["ProductCode"] && $scope.PoLotRefGrid[i]["PO"] == $scope.cartonDetail[0]["POId"]) {
                
                $scope.PoLotRefGrid[i]["bookQty"] = parseFloat(jj.toFixed(2));
            }
        }

        angular.element(document.querySelector('#cartonDetailModal')).modal('hide');
    }

    //Select All The Cartons
    $scope.selectAllCartons = function () {
        if ($scope.cartonDetail.length > 0) {
            for (var i = 0; i < $scope.cartonDetail.length; i++) {
                $scope.cartonDetail[i]["checked"] = true;
            }
        }
    }
    //Deselect All Cartons
    $scope.DeselectAllCartons = function () {
        if ($scope.cartonDetail.length > 0) {
            for (var i = 0; i < $scope.cartonDetail.length; i++) {
                $scope.cartonDetail[i]["checked"] = false;
            }
        }
    }

    $scope.PoLoTCollection = [];
    //Saving All The Data
    $scope.soqty = 0;
    $scope.saveAll = function () {
        try {
            var counter = 0;
            for (var i = 0; i < $scope.PackingLineItemGrid.length; i++) {
                if ($scope.PackingLineItemGrid[i]["checked"] == true) {
                    counter++;
                }
            }
            if (counter == 0) {
                ShowResult("Please Select SO");
                throw ("Please Select SO");
            }
            if (counter > 1) {
                ShowResult("Please Select only 1 SO");
                throw ("Please Select only 1 SO");
            }
            $scope.AllVals();
            var so = "";
            $scope.soqty = 0;
            $scope.todispatchqty = 0;
            for (var i = 0; i < $scope.PackingLineItemGrid.length; i++) {

                if ($scope.PackingLineItemGrid[i]["checked"] == true) {
                    so = $scope.PackingLineItemGrid[i]["SO"];
                    $scope.soqty = $scope.PackingLineItemGrid[i]["SoQty"];
                    $scope.todispatchqty = $scope.PackingLineItemGrid[i]["toDespatch"];
                }
            }

            $scope.PackingLineItem.SOId = so;

            $scope.PoLoTCollection = [];
            for (var i = 0; i < $scope.PoLotRefGrid.length; i++) {
                //if ($scope.PoLotRefGrid[i]["checked"] == true) {

                //    if ($scope.PoLoTCollection.length > 0) {
                //        for (var j = $scope.PoLoTCollection.length - 1; j >= 0; j--) {
                //            if ($scope.PoLoTCollection[j]["LotNo"] == $scope.PoLotRefGrid[i]["LotNo"]) {
                //                $scope.PoLoTCollection.splice(j, 1);
                //            }
                //        }
                //    }
                if ($scope.PoLotRefGrid[i]["checked"] == true) {
                    $scope.POLotRef = {
                        Id: null,
                        PackingLineItemId: null,
                        ProductCode: null,
                        PONo: null,
                        LotNo: null,
                        BookQty: null,
                        Remarks: null,
                        Available: null,
                        Status: "Active",
                    };

                    $scope.POLotRef.ProductCode = $scope.PoLotRefGrid[i]["ProductCode"];
                    $scope.POLotRef.PONo = $scope.PoLotRefGrid[i]["PO"];
                    $scope.POLotRef.LotNo = $scope.PoLotRefGrid[i]["LotNo"];
                    $scope.POLotRef.BookQty = $scope.PoLotRefGrid[i]["bookQty"];
                    $scope.POLotRef.Remarks = $scope.PoLotRefGrid[i]["comment"];
                    $scope.POLotRef.PlanQty = $scope.PoLotRefGrid[i]["quant"];
                    $scope.POLotRef.Available = $scope.PoLotRefGrid[i]["Available"];
                    $scope.PoLoTCollection.push($scope.POLotRef);
                }

                //}


            }

            var cartons = "''";
            var cartonsAllObj = [];
            if ($scope.cartonCollection.length > 0) {
                var k = $scope.cartonCollection[0]["LotNo"];
                var kk = $scope.cartonCollection[0]["ProductCode"];
                var kkk = $scope.cartonCollection[0]["PO"];
                for (var i = 0; i < $scope.cartonCollection.length; i++) {
                    var j = $scope.cartonCollection[i]["LotNo"];
                    var jj = $scope.cartonCollection[i]["ProductCode"];
                    var jjj = $scope.cartonCollection[i]["PO"];
                    if (j == k && jj == kk && jjj == kkk) {
                        cartons = cartons + ',' + "'" + $scope.cartonCollection[i]["RefNo"] + "'";
                        k = j; kk = jj; kkk = jjj;
                    }
                    else {
                        cartonsAllObj.push({ ProductCode: kk, PO: kkk, LotNo: k, RefNo: cartons });
                        cartons = "''," + "'" + $scope.cartonCollection[i]["RefNo"] + "'";
                        k = j;
                        kk = jj;
                        kkk = jjj;
                    }
                }

                cartonsAllObj.push({ ProductCode: kk, PO: kkk, LotNo: k, RefNo: cartons });
            }



            //for (var i = 0; i < $scope.cartonCollection.length; i++) {
            //    cartons = cartons + ',' + "'"+$scope.cartonCollection[i]["RefNo"]+"'";
            //}

            var cartonsList = cartonsAllObj;

            $scope.secondVals();

            if ($scope.POLotRef.PlanQty > $scope.soqty) {
                throw "Plan Qty can't greater than SO Qty.";
            }
            if ($scope.todispatchqty<=0) {
                throw "Qty is not available.";
            }

            $http({
                method: 'POST',
                url: $scope.path + "CreateAll",
                data: { 'Packingdata': $scope.Packing, 'PackingLineItemdata': $scope.PackingLineItem, 'POLotRefData': $scope.POLotRef, 'Cartons': cartonsList, 'POLotCollection': $scope.PoLoTCollection, 'lastIndex': $scope.lastIndex },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Packing = response.data.Data;
                    $scope.lastIndex = response.data.lastIndex;
                    $scope.ClearAll();
                    $scope.getSoFromCustomerList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //Clear the Selections
    $scope.ClearAll = function () {
        $scope.PackingLineItem = {
            PackingId: null,
            PackingLineItemId: null,
            SOId: null,
        };

        $scope.POLotRef = {
            Id: null,
            PackingLineItemId: null,
            ProductCode: null,
            PONo: null,
            LotNo: null,
            BookQty: null,
            Remarks: null,
            Status: "Active",
        };
        $scope.PackingLineItemGrid = [];
        $scope.PoLotRefGrid = [];
        $scope.cartonDetail = [];
        $scope.inactiveCartons = [];
        $scope.PoLoTCollection = [];
        $scope.toDisp = 0;
        $scope.totalPlanned = 0;
        $scope.Percent = 0;
    }

    $scope.clearPage = function () {
        $scope.Packing = {
            PackingId: null,
            Date: null,
            ByWhom: null,
            InactiveDate: null,
            Remarks: null,
            CustomerId: null,
            StorageLocId: null,
            EntityId: null,
            DispatchResponsiblePersonId: null,
        };

        $scope.PackingLineItem = {
            PackingId: null,
            PackingLineItemId: null,
            SOId: null,
        };

        $scope.POLotRef = {
            Id: null,
            PackingLineItemId: null,
            ProductCode: null,
            PONo: null,
            LotNo: null,
            BookQty: null,
            Remarks: null,
            PlanQty: null,
            Available: null,
            Status: "Active",
        };


        $scope.lastIndex = 0;
        $scope.byWhom = "";
        $scope.customer = "";
        $scope.dRespPerson = "";
        $scope.PackingLineItemGrid = [];
        $scope.PoLotRefGrid = [];
        $scope.inactiveCartons = [];
        $scope.cartonDetail = [];
        $scope.PoLoTCollection = [];
        $scope.cartonCollection = [];
        $scope.toDisp = 0;
        $scope.totalPlanned = 0;
        $scope.Percent = 0;
    }

    $scope.summaryRows = [{
        title: "Totals", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetWeight", dataMember: "NetWeight", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GWeight", dataMember: "GWeight", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

    $scope.AllVals = function () {
        const date1 = new Date();
        const date2 = new Date($scope.Packing.InactiveDate);
        const diffTime = date2 - date1;
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)); 

        if (diffDays < 0 || diffDays > 4) {
            ShowResult("Inactive can't be more than 4 days");
            throw ("Inactive can't be more than 4 days");
        }

        if ($scope.Packing.EntityId == null || $scope.Packing.ByWhom == null || $scope.Packing.DispatchResponsiblePersonId == null || $scope.Packing.StorageLocId == null || $scope.Packing.InactiveDate == null) {
            ShowResult("Please Fill all the Packing Plan Fields");
            throw ("Please Fill all the Packing Plan Fields");
        }

    }



    $scope.secondVals = function () {
        if ($scope.PoLoTCollection.length == 0) {
            ShowResult("Please select A PO Lot Reference");
            throw ("Please select A PO Lot Reference");
        }

        for (var i = 0; i < $scope.PoLoTCollection.length; i++) {
            if ( $scope.PoLoTCollection[i]["PlanQty"] == 0) {
                ShowResult("Plan Qty cannot be 0!");
                throw ("Plan Qty cannot be 0!");
            }

            if ($scope.PoLoTCollection[i]["PlanQty"] < $scope.PoLoTCollection[i]["BookQty"]) {
                ShowResult("Book Qty cannot be more than Plan Qty!!");
                throw ("Book Qty cannot be more than Plan Qty!!");
            }


            //if ($scope.PoLoTCollection[i]["BookQty"] > $scope.PoLoTCollection[i]["Available"] || $scope.PoLoTCollection[i]["PlanQty"] > $scope.PoLoTCollection[i]["Available"]) {
            //    ShowResult("The Quantities cannot be more than Available Quantity!");
            //    throw ("The Quantities cannot be more than Available Quantity!");
            //}
        }
    }

    //Packing List Page
    $scope.packingList = [];
    $scope.getPackingList = function () {
        $http({
            method: 'GET',
            url: $scope.path + "getPackingList",
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.packingList = [];
                $scope.packingList = response.data.Data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }


    /// The Part for main Packing Plan Modals and The list

    $scope.cellDetails = function (e) {
        if (e.data.Active < 0) {
            e.cell.bgColor = '#ED4848';
        }
    }

    $scope.gridPacking = [];
    $scope.getgridPacking = function () {
        $http({
            method: 'GET',
            url: $scope.path + "getgridPacking",
            
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.gridPacking = [];
            $scope.gridPacking = response.data;

           
        });
    }


    $scope.PackingListReport = function (obj) {
        try {
            var file_src = $scope.path + "PackingList?PackingId=" + obj.data.PackingId;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.PackingListPDFReport = function (obj) {
        try {
            var file_src = $scope.path + "PackingListPDFReport?reportFormat=" + 'Pdf' +'&PackingId=' + obj.data.PackingId;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.PackingListXLReport = function (obj) {
        try {
            var file_src = $scope.path + "PackingListXLReport?reportFormat=" + 'Excel' + '&PackingId=' + obj.data.PackingId;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }



    $scope.PackingIdForReport = "";
    $scope.plItems = [];
    $scope.openPackingLineItem = function (e) {
        $scope.PackingIdForReport = "";
        $scope.PackingIdForReport = e.data.PackingId;
        $http({
            method: 'GET',
            url: $scope.path + "openPackingLineItemModal",
            params: {'PackingId' : e.data.PackingId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.plItems = [];
            $scope.polItems = [];
            $scope.plItems = response.data;
            
            angular.element(document.querySelector('#packingLineModal')).modal('show');
        });
    }

    $scope.polItems = [];
    $scope.openPOLotRefGrid = function (e) {
        $http({
            method: 'GET',
            url: $scope.path + "openPOLotRefGridModal",
            params: { 'PackingLineItemId': e.data.PackingLineItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.polItems = [];
            $scope.polItems = response.data;
        });
    }

    $scope.summaryRowsModal = [{
        title: "Totals", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BookQty", dataMember: "BookQty", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "PlanQty", dataMember: "PlanQty", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.printPackingReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPrintReport',
            data: { 'PackingId': $scope.PackingIdForReport},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.getStocksReport = function () {
       
        $http({
            method: 'POST',
            url: $scope.path + "GetStockReport",
            data: {
                'ToDate': $scope.selectedValues.ToDate, 'FromDate': $scope.selectedValues.FromDate,
                'type': $rootScope.typeVal, 'group': $rootScope.groupVal, 'value': $scope.search, 'column': $scope.searchBy,
                'Loc': $scope.LocId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.getFinishedStocksReport = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetFinishedStocksReport",
            
            data: {
                //'ToDate': $scope.selectedValues.ToDate, 'FromDate': $scope.selectedValues.FromDate,
                //'type': $rootScope.typeVal, 'group': $rootScope.groupVal, 'value': $scope.search, 'column': $scope.searchBy,
                'ToDate': $scope.selectedValues.ToDate, 'FromDate': $scope.selectedValues.FromDate,
                'Loc': $scope.LocId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.ViewReport = function (data) {
        try {
            var url = 'QMS/LWQSummaryReport/GetCustomerLWQSummaryJobCardReport?ProductionOrderId=' + data.data.PO + '&LotNumber=' + data.data.LotNo;
            $rootScope.report(url);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}