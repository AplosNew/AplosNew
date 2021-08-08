'use strict';
NewAttendanceDashboardController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function NewAttendanceDashboardController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'New Attendance Dashboard';
    $scope.path = "HumanResource/NewAttendanceDashboard/";

    $scope.Date = null;

    var x = document.getElementById("getGridData");
    x.style.display = 'none';

    //The Filters 
    $scope.filters=[];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            
            var ob = {
                'OTApplicable': 'True'
            };
            var ob1 = {
                'OTApplicable': 'False'
            }
            Object.assign(response.data[0], ob);
            Object.assign(response.data[1], ob1);
            $scope.filters = response.data;
            var columnList = [
                { field: 'Plant', width: 20, headerText: "Plant", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'Division', width: 20, headerText: "Division", type: "string" },
                { field: 'Department', width: 20, headerText: "Department", type: "string" },
                { field: 'Section', width: 20, headerText: "Section", type: "string" },
                { field: 'SubSection', width: 20, headerText: "Sub Section", type: "string" },
                { field: 'Shift', width: 20, headerText: "Shift", type: "string" },
                { field: 'WorkGroup', width: 20, headerText: "Work Group", type: "string" },
                { field: 'AttendGroup', width: 20, headerText: "Attendance Group", type: "string" },
                { field: 'ROBudgetCode', width: 20, headerText: "RO Code", type: "string" },
                { field: 'PRBudgetCode', width: 20, headerText: "PR Code", type: "string" },
                { field: 'ResponsiblePerson', width: 20, headerText: "Responsible Person", type: "string" },
                { field: 'OTApplicable', width: 20, headerText: "OT Applicable", type: "string" },
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

        $scope.parameters = parameters;
        $scope.GenerateData();
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

    //Destroy The Grid Before ReBuilding And Clearing of the Filters
    

    $scope.clearFilters = function () {

        var gridObj = $("#filters").data("ejGrid");
        gridObj.clearFiltering();
    }

    // Generating the Data
    $scope.GridData = [];
    $scope.GenerateData = function () {

       

        if (angular.isUndefinedOrNull($scope.Date)) {
            ShowResult("Please First Select the Date!", 'failure');
            throw ("Invalid");
        }
        if (x.style.display == 'none') {
            x.style.display = 'block';
        } 

        $http({
            method: 'GET',
            url: $scope.path + 'getGridData',
            params: { 'Date': $scope.Date, 'param' : $scope.parameters}
        }).then(function succ(resp) {
            $scope.GridData = [];
            $scope.GridData = resp.data;
        })
    }


}



   