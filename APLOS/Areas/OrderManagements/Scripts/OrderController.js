'use strict';
OrderController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OrderController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Order Status';
    $scope.path = "OrderManagements/Order/";

    $scope.fromDate = '';
    $scope.toDate = '';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

   //The Selection Criteria   
    $scope.Datelist = [
        { text: "Ex Factory Date", value: "ExFactoryD" },
        { text: "Shipment Date", value: "ShipmentD" },
        { text: "Commitment Date", value: "CommitmentD" }        
    ];
    $rootScope.dateC = "Deliver Date";
    $rootScope.dateCgroup = "ShipmentD";
    $rootScope.chartTypeG = "ProductionD";
    $scope.dateValueChange = function () {
        var obj = $('#dropdownDate').data("ejDropDownList");
        $rootScope.dateC = obj.option("text");
        $rootScope.dateCgroup = obj.option("value");
    }

    $scope.Report = function () {
        try {
            if (new Date($scope.fromDate) > new Date($scope.toDate)) {
                throw " From date can not be greater than To date.";

            }
            $scope.filterComplete();
            $scope.fileName = "OrderReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetOrderReport",
                data: { 'parameters': $scope.parameters, 'fromDate': $scope.fromDate, 'toDate': $scope.toDate, 'dateType': $rootScope.dateCgroup },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath

                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

 
    //The Filters 
    $scope.filters=[];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'Plant', width: 20, headerText: "Plant", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
                { field: 'Buyer', width: 20, headerText: "Buyer", type: "string" },
                { field: 'ResponsiblePerson', width: 20, headerText: "Responsible Person", type: "string" },
                      
                { field: 'ProductionStatus', width: 20, headerText: "PO Status", type: "string" },
                { field: 'SOStatusId', width: 20, headerText: "SO Status", type: "string" },
                { field: 'MOStatusId', width: 20, headerText: "MO Status", type: "string" },
                
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
    }
    $scope.loadfilters();

    // THe Generate Filters
    $scope.parameters = [];
    $scope.filterComplete = function () {
      
        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "PlantId", "Value": getString(fl, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "CustomerId", "Value": getString(fl, "CustomerId") });
        parameters.push({ "Key": "BuyerId", "Value": getString(fl, "BuyerId") });
        parameters.push({ "Key": "ResponsiblePersonId", "Value": getString(fl, "ResponsiblePersonId") });
      
        parameters.push({ "Key": "ProductionStatusId", "Value": getString(fl, "ProductionStatusId") });
        parameters.push({ "Key": "SOStatusId", "Value": getString(fl, "SOStatusId") });
        parameters.push({ "Key": "MOStatusId", "Value": getString(fl, "MOStatusId") });
      
        $scope.parameters = parameters;
     
    }


    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
               /* var replace = data[i][column].replace(",", "','");*/
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    //Destroy The Grid Before ReBuilding And Clearing of the Filters
    $scope.clearFilters = function () {

        var gridObj = $("#filters").data("ejGrid");
        gridObj.clearFiltering();
    }

  




    $scope.refreshPage = function (e) {
        if (e.requestType == "paging") {
            var gridObj = $("#slabGrid").data("ejGrid");
       gridObj.refreshContent(true);
        gridObj.refreshTemplate();
        }
        var k = 100;
    }



}



   