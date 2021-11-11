'use strict';
OTConfirmationProcessController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OTConfirmationProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'OT Confirmation Process';
    $scope.path = "humanresource/OTConfirmationProcess/";


    $scope.parameters = [];
    $scope.filters = [];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;

            var gridObj = $("#WeekList").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#WeekList").children('.e-pager.e-js.e-pager').hide();
            $("#WeekList").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#WeekList").children('.e-gridcontent').hide();
        });
    }
    $scope.loadfilters();


   



    //$scope.blackoutDates = [new Date()];
    //$scope.Changes = function()
    //{
    //    console.log('changed');
    //}

    $scope.ToDate = null;
    $scope.FromDate = null;
    $scope.Week = null;

    $scope.OTConfirmationValue = null;

    $scope.OTConfirmSelection = [
        {'Id':'0', 'Value': 'To Confirm'},
        { 'Id': '1', 'Value': 'Confirmed' },
        { 'Id': '2', 'Value': 'All' },
    ];

    //Process Filter
    $scope.Process = null;
    $scope.ProcessValue = 0.0;
    $scope.OTLimit = null;
    $scope.DSApp = null;

    //Day Status

    $scope.DayStatus

    $scope.DayTypesList = [];
    function getDayTypes() {
        $http({
            method: 'GET',
            url: $scope.path + 'getDayTypes',
        }).then(function succ(resp) {
            $scope.DayTypesList = resp.data;
        });
    };

    getDayTypes();

    $scope.selectCurrDayStatus = function () {
        angular.element(document.querySelector('#CurrDayStatusModal')).modal('show');
    }

    $scope.doubleCurrDayStatus = function (e) {
        $scope.DayStatus = e.data.DayType;
        angular.element(document.querySelector('#CurrDayStatusModal')).modal('hide');
    }

    $scope.getData = function () {

        var gridObj = $("#WeekList").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (filteredRecords.length == 0) {
            filteredRecords = $scope.filters;
        }

        var parameters = [];
        parameters.push({ "Key": "PlantId", "Value": getString(filteredRecords, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });


        console.log($scope.Process, ' ',
            $scope.ProcessValue, ' ',
            $scope.OTLimit, ' ',
            $scope.DSApp, ' ',
            $scope.DayStatus, ' ', $scope.OTConfirmationValue, ' ', $scope.ToDate, ' ', $scope.FromDate, ' ', $scope.Week , ' = ' , parameters);
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
}