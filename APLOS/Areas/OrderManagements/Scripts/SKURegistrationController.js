'use strict';
SKURegistrationController.$inject = ['cboService', '$window', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SKURegistrationController(cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "SKU Registration";
    $scope.Action = 'Save';
    $scope.path = 'OrderManagements/ProductionOrder/';

    $scope.getFiltersData = function () {
        try {
          
            $http({
                method: 'GET',
                url: 'OrderManagements/ProductionOrder/GetSalesOrderFilterData',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'POId', width: 20, headerText: "POId", type: "string" },
                    { field: 'SOId', width: 20, headerText: "SOId", type: "string" },
                    { field: 'PartyId', width: 20, headerText: "PartyId", type: "string" },
                    { field: 'Customer', width: 20, headerText: "Customer", type: "string" }
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
    $scope.getFiltersData();









}