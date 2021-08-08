'use strict';
NewAttendanceDashboardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function NewAttendanceDashboardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'New Attendance Dashboard';
    $scope.path = "HumanResource/NewAttendanceDashboard/";

  

    //The Filters 
    $scope.filters=[];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var ob = {
                'OTApplicable': 'True'
            };
            var ob1 = {
                'OTApplicable': 'False'
            }
            Object.assign(response.data[0], ob);
            Object.assign(response.data[1], ob1);
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
        destroyGrid();
        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "PlantId", "Value": getString(fl, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "CustomerId", "Value": getString(fl, "CustomerId") });
        parameters.push({ "Key": "MResId", "Value": getString(fl, "MResId") });
        parameters.push({ "Key": "ERespId", "Value": getString(fl, "ERespId") });
        parameters.push({ "Key": "Status", "Value": getString(fl, "Status") });

        $scope.parameters = parameters;
        $scope.slabGrid();
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
    function destroyGrid() {
        var g = $("#slabGrid").data("ejGrid");
        g.destroy();
    }

    $scope.clearFilters = function () {

        var gridObj = $("#filters").data("ejGrid");
        gridObj.clearFiltering();
    }

    

    /// Charts Section

    Object.size = function (obj) {
        var size = 0, key;
        for (key in obj) {
            if (obj.hasOwnProperty(key)) size++;
        }
        return size;
    };
    function fillChart() {
        try {
            //var labels = [];
            //var active = [];
            //var pending = [];
            //var close = [];
            //var dispatch = [];
            //var prodcom = [];
            //var l = Object.size($scope.slabData[0]);
            //for (var i = 0; i < l; i++) {
            //    active[i] = 0;
            //    pending[i] = 0;
            //    close[i] = 0;
            //    dispatch[i] = 0;
            //    prodcom[i] = 0;
            //}
            var ini = 0;
            if ($scope.group == "Delivery") {
                slabChart.data.labels = ["", "", "<-30", "<-30 To -20","<-20 TO -10", "<-10 To -5", "<-5 TO 0", "= 0", "> 0 To 5", "> 5 TO 10", ">10 TO 15", ">15 To 20", ">20 To 30", ">30"];
                ini = 2;
            }
            else {
                slabChart.data.labels = [ "", "","<-30","<-30 To -20", "<-20 TO -10", "<-10 To -5", "<-5 TO 0", "= 0", "> 0 To 5", "> 5 TO 10", ">10 TO 15", ">15 To 20", ">20 To 30", ">30"];
                ini = 1;

            }
            //for (var i = 0; i < $scope.slabData.length; i++) {
            //    //var k = 0; 
            //    var kk = ini;
            //    for (var j = 2; j < l; j++) {
            //        if (Object.values($scope.slabData[i])[j] != '-') {
            //            active[ini] = active[ini] + Object.values($scope.slabData[i])[j];
            //            ini++;
            //        }
            //        else {
            //            ini++;
            //        }
            //        //k++;
            //    }
            //    ini = kk;
            //}
            ////labels = ["<-30", "<-20 TO -10", "<-10 To -5", "<-5 TO 0", "= 0", "> 0 To 5", "> 5 TO 10", ">10 TO 15", ">15 To 20", ">20 To 30", ">30"];
            
            ////slabChart.data.labels = $scope.labels;
            slabChart.data.datasets[0].data = $scope.Chart[1];//pending
            slabChart.data.datasets[1].data = $scope.Chart[2];//ToClose
            slabChart.data.datasets[2].data = $scope.Chart[3];//ToDispatch
            slabChart.data.datasets[3].data = $scope.Chart[4];//ProductionComplete
            slabChart.data.datasets[4].data = $scope.Chart[0]; // active
            
            slabChart.update();
        }
        catch (e) { }
    }

    var slabCharter = document.getElementById('slabChart').getContext('2d');

    var slabChart = new Chart(slabCharter, {
        type: 'bar',
        data: {
        labels:  [],
        datasets: [{
                label: 'Pending',
                data: [],
            backgroundColor: '#FFFF00',
        },

            {
                label: 'To Close',
                data: [],
                backgroundColor: '#FF6347', 
            },
            {
                label: 'To Ship',
                data: [],
                backgroundColor:'#FFA500',
            },
            {
                label: 'Production Complete',
                data: [],
                backgroundColor: '#00ff00',
            },
            {
                label: 'Active',
                data: [],
                backgroundColor: '#0E86D4',
            }
        ]
    },
        options: {
            scaleShowValues: true,
            title: {
                display: true,
                text: 'Slab Chart'
            },
            tooltips: {
                mode: 'index',
                intersect: false
            },
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                xAxes: [{
                    ticks: {
                        autoSkip: true,
                        padding: 10,
                        fontSize: 10
                    },
                    stacked: true,
                }],
                yAxes: [{
                    stacked: true
                }]
            }
        }
    });

}



   