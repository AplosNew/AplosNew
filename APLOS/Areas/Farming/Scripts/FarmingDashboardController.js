'use strict';
FarmingDashboardController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$cookies', '$window'];
function FarmingDashboardController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $cookies, $window) {

    $rootScope.title = 'Farming Dashboard';
    $scope.returnData;
    $scope.TableData = [];
    $scope.ICSnumbersInGroups = [];

    $scope.cropType;
    $scope.land;
    $scope.crop;
    $scope.cropSubCategory;
    $scope.cropCategory;

    $scope.icsNumbers = [];
    $scope.uniNoClick = 0;

    $scope.Total = {
        number: 0,
        active: 0,
        inactive: 0,
        totarea: 0,
        planarea: 0
    };

    $scope.ColList = [];
    $scope.filterGroup = {};
    var columnsNames = [
        { 'columnName': 'ICS Group/Center', 'standardName': 'Group' },
        { 'columnName': 'ICS Name', 'standardName': 'Id' }
    ];


    $scope.farmingDropDown = {
        CropTypeId: null,
        CropCategoryId: null,
        CropSubCategoryId: null,
        LandId: null,
        CropId: null
    };

    ///$http({}).then(function successCallback(response) { });
    /*
     $http({
        method: 'GET',
        url:'',
        dataType: 'JSON'
     }).then(function successCallback(response) { });
     */

function DropDowns()
{
    $http({
        method: 'GET',
        url: 'Farming/FarmingDashboard/getCropType',
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.cropType = response.data;
    });


    $http({
        method: 'GET',
        url: 'Farming/FarmingDashboard/getCropCategory',
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.cropCategory = response.data;
    });


    $http({
        method: 'GET',
        url: 'Farming/FarmingDashboard/getCropSubCategory',
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.cropSubCategory = response.data;
    });


    $http({
        method: 'GET',
        url: 'Farming/FarmingDashboard/getLand',
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.land = response.data;
    });


    $http({
        method: 'GET',
        url: 'Farming/FarmingDashboard/getCrop',
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.crop = response.data;
    });


    $http({
        method: 'GET',
        url: 'Farming/FarmingDashboard/IcsPie',
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.icsNumbers = response.data;
        fillPie($scope.icsNumbers);
    });
}

    DropDowns();

    function app() {

        $scope.Total = {
            number: 0,
            active: 0,
            inactive: 0,
            totarea: 0,
            planarea: 0
        };
      
          

            $http({
                method: 'GET',
                url: 'Farming/FarmingDashboard/getFilterData',
                params: {
                    'landId': $scope.farmingDropDown.LandId,
                    'cropId': $scope.farmingDropDown.CropId,
                    'cropTypeId': $scope.farmingDropDown.CropTypeId,
                    'cropCategoryId':$scope.farmingDropDown.CropCategoryId ,
                    'cropSubCategoryId': $scope.farmingDropDown.CropSubCategoryId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.returnData = response.data;
                var InitialTable = {
                    Group: null,
                    ActiveFarmers: null,
                    InctiveFarmers: null,
                    TotalArea: null,
                    PlannedArea: null,
                    NumberOfIcs: null,
                    noClick: null,
                    columnName: null,
                    standardName: null
                }
                for (var i = 0; i < $scope.returnData.length; i++) {

                    InitialTable.Group = $scope.returnData[i].Group;
                    InitialTable.ActiveFarmers = $scope.returnData[i].Active;
                    InitialTable.InactiveFarmers = $scope.returnData[i].Inactive;
                    InitialTable.TotalArea = $scope.returnData[i].TotalArea;
                    InitialTable.PlannedArea = $scope.returnData[i].PlannedArea;
                    InitialTable.numberOfICS = $scope.returnData[i].numberICS
                    InitialTable.noClick = 0;
                    InitialTable.columnName = columnsNames[InitialTable.noClick].columnName;
                    InitialTable.standardName = columnsNames[InitialTable.noClick].standardName;

                    $scope.TableData[i] = InitialTable;
                    InitialTable = {};

                }

                for (var i = 0; i < $scope.TableData.length; i++) {
                    $scope.Total.number = $scope.Total.number + $scope.TableData[i].numberOfICS;
                    $scope.Total.active = $scope.Total.active + $scope.TableData[i].ActiveFarmers;
                    $scope.Total.inactive = $scope.Total.inactive + $scope.TableData[i].InactiveFarmers;
                    $scope.Total.totarea = $scope.Total.totarea + $scope.TableData[i].TotalArea;
                    $scope.Total.planarea = $scope.Total.planarea + $scope.TableData[i].PlannedArea;
                }
                $scope.uniNoClick = $scope.TableData[0].noClick;
                $scope.ColList.push($scope.TableData[0]);
                fillChart($scope.TableData);
            });

        
    };

    app();

    $scope.getDrillDownData = function (data) {
        getIcsDrillData(data);
    }


    //Getting the Drill Data with the help of noClicks
    function getIcsDrillData(data) {
        $scope.Total = {
            number: 0,
            active: 0,
            inactive: 0,
            totarea: 0,
            planarea: 0
        };

        $scope.filterGroup = data;
            
            $scope.TableData = [];
            $http({
                method: 'GET',
                url: 'Farming/FarmingDashboard/getFilterDrillData',
                params: {
                    'icsGroup': data.Group,
                    'landId': $scope.farmingDropDown.LandId,
                    'cropId': $scope.farmingDropDown.CropId,
                    'cropTypeId': $scope.farmingDropDown.CropTypeId,
                    'cropCategoryId': $scope.farmingDropDown.CropCategoryId,
                    'cropSubCategoryId': $scope.farmingDropDown.CropSubCategoryId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.returnData = response.data;
                var InitialTable = {
                    Group: null,
                    ActiveFarmers: null,
                    InctiveFarmers: null,
                    TotalArea: null,
                    PlannedArea: null,
                    noClick: null,
                    columnName: null,
                    standardName: null
                }
                for (var i = 0; i < $scope.returnData.length; i++) {

                    InitialTable.Group = $scope.returnData[i].Group;
                    InitialTable.ActiveFarmers = $scope.returnData[i].Active;
                    InitialTable.InactiveFarmers = $scope.returnData[i].Inactive;
                    InitialTable.TotalArea = $scope.returnData[i].TotalArea;
                    InitialTable.PlannedArea = $scope.returnData[i].PlannedArea;
                    InitialTable.noClick = data.noClick + 1;
                    InitialTable.columnName = columnsNames[InitialTable.noClick].columnName;
                    InitialTable.standardName = columnsNames[InitialTable.noClick].standardName;

                    $scope.TableData[i] = InitialTable;
                    InitialTable = {};

                }
                for (var i = 0; i < $scope.TableData.length; i++) {
                    $scope.Total.number = $scope.Total.number + $scope.TableData[i].numberOfICS;
                    $scope.Total.active = $scope.Total.active + $scope.TableData[i].ActiveFarmers;
                    $scope.Total.inactive = $scope.Total.inactive + $scope.TableData[i].InactiveFarmers;
                    $scope.Total.totarea = $scope.Total.totarea + $scope.TableData[i].TotalArea;
                    $scope.Total.planarea = $scope.Total.planarea + $scope.TableData[i].PlannedArea;
                }

                $scope.ColList.push($scope.TableData[0]);
                $scope.uniNoClick = $scope.TableData[0].noClick;
                fillChart($scope.TableData);
            });
        
    }


   

    ///The Nav Bar Click Function
    $scope.navClick = function (data) {

        if (data.noClick == 0) {
            $scope.TableData = [];
            $scope.ColList = [];
            app();
        }
        else {
            var data1 = $scope.ColList[data.noClick - 1];
            $scope.TableData = [];
            getIcsDrillData(data1);
        }
    }

    $scope.dropDown = function () {
        if ($scope.uniNoClick == 0) {
            app();
        }
        if ($scope.uniNoClick == 1) {
            getIcsDrillData($scope.filterGroup);
        }
    }

    $scope.reset = function () {

        $scope.farmingDropDown = {
            CropTypeId: null,
            CropCategoryId: null,
            CropSubCategoryId: null,
            LandId: null,
            CropId: null
        };
        $scope.TableData = [];
        $scope.ColList = [];
        app();
        
    }

   /* function LoadData(data) {
        $("#Grid2").ejGrid({

            dataSource: data, // data must be array of json
            allowPaging: true,
            allowFiltering: true,
            isResponsive: true,
            enableResponsiveRow: true,
            allowTextWrap: true,
            textWrapSettings: { wrapMode: "header" },
            cssClass: "filtered",
            filterSettings: {
                filterType: "excel"
            },
            allowScrolling: true,
            scrollSettings: { height: "5" },



            columns: [
                { headerText: "S. No.", field: "Snum", width: 10 },
                { headerText: "Farmer ID", field: "FarmerId", width: 30 },
                { headerText: "Tracenet Id", field: "TracenetId", width: 30 },
                { headerText: "Farmer Name", field: "FarmerName", width: 30 },
                { headerText: "Farmer Father Name", field: "FarmerFatherName", width: 30 },
                { headerText: "Total Area", field: "TotalArea", width: 30 },
                { headerText: "Total Plots", field: "TotalPlots", width: 30 },
                { headerText: "Registration Date", field: "RegistrationDate", width: 30 }
            ]


        });
        $("#Grid2").children('.e-pager.e-js.e-pager').hide();
        $("#Grid2").children('.e-gridcontent.e-droppable.e-js').hide();
        $("#Grid2").children('.e-gridcontent').hide();
        //$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

        $("#Grid2").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();
    }


    $scope.dFunction = function () {
        var obj = $("#Grid2").ejGrid("instance");
        var sd = obj.getFilteredRecords();
        
        if (sd.length == 0) {
            sd = obj.model.dataSource;
            $scope.FarmersModal = sd;
        }
        else {
            $scope.FarmersModal = sd;
        }
    }

    $scope.page = 1;
    var rows = 5;
    function pagination(tableData , page ) {
        var trimStart = (page - 1) * rows;
        var trimEnd = rows + trimStart;

        var trimedData = tableData.slice(trimStart, trimEnd);
        var pages = Math.ceil(tableData.length / rows);
        return {
            'tableData': trimedData,
            'pages' : pages
        }
    }

   function pageButtons(pages , objectData) {
        var wrapper = document.getElementById('tableBar');
        wrapper.innerHTML = '';
        for (var pa = 1; pa <= pages; pa++) {
            wrapper.innerHTML += `<button value=${pa} class = "page btn btn-sm btn-info" style="margin-right: 5px;">${pa}</button>`;
       }

       $('.page').on('click', function () {
          

           $scope.page = Number($(this).val())

           $scope.showModal(objectData);
       })
    }

    $scope.pageSet = function () {
        $scope.page = 1;
    }*/
    //------------------------------------------------------ Charts Section ------------------------------------------------------\\


    function fillPie(data) {
        var labelsChart = [];
        for (var i = 0; i < data.length; i++) {
            labelsChart.push(data[i].Group);
            
            $scope.ICSnumbersInGroups.push(data[i].numberIcs)
            
        }
        ICSNoChart.data.labels = labelsChart;
        ICSNoChart.data.datasets[0].data = $scope.ICSnumbersInGroups;
        $scope.ICSnumbersInGroups = [];
        ICSNoChart.update();
    }


    function fillChart(data) {
        var labelsChart = [];
        var datasetsDisp = [];
        var PArea = [];
        var TArea = [];
        for (var i = 0; i < data.length; i++) {
            labelsChart.push(data[i].Group);
            datasetsDisp.push(data[i].ActiveFarmers);
           
            PArea.push(data[i].PlannedArea);
            TArea.push(data[i].TotalArea);
        }

        FarmerChart.data.datasets[0].data = datasetsDisp;
        FarmerChart.data.labels = labelsChart;
        FarmerChart.update();

       

        AreaChart.data.labels = labelsChart;
        AreaChart.data.datasets[0].data = TArea;
        AreaChart.data.datasets[1].data = PArea;
        AreaChart.update();


    }

    //Total Number of Farmers Group WIse Chart(Bar Chart)
    var FarmersChart = document.getElementById('FarmersChart').getContext('2d');
    var FarmerChart = new Chart(FarmersChart, {
        type: 'bar',
        data: {
            labels: [],
            datasets: [{
                label: 'Number of Farmers',
                data: [],
                backgroundColor: 'rgba(255, 99, 132)',
                hoverBorderWidth: 2,
                hoverBorderColor: '#054564',
            }]
        },
        options: {
            scales: {
                xAxes: [{
                    categoryPercentage: 0.6,
                }]
            }
        },
    });



    //Total No if ICS in a group Chart (PIE Chart)
    var IcsNoChart = document.getElementById('ICSNoChart').getContext('2d');

    var ICSNoChart = new Chart(IcsNoChart, {
        type: 'pie',
        data:
        {
            labels: [],
            datasets: [{
                label: 'Points',
                data: [],
                backgroundColor: ['#01c5c4', '#b8de6f', '#f1e189', '#f39233', '#f39ab3', '#53a2c3'],
                hoverBorderWidth: 2,
                hoverBorderColor: '#027373',
            }]
        },
        options: {
            cutoutPercentage: 30,
            animation: {
                animateScale: true
            }
        }
    });


    //Areas Chart for Total and Planned Area of Farmers Group Wise
    var AreasChart = document.getElementById('AreaChart').getContext('2d');

    var AreaChart = new Chart(AreasChart, {
        type: 'bar',
        data: {
            labels: [],
            datasets: [{
                label: 'Total Area',
                data: [],
                backgroundColor: '#f39233',
                hoverBorderWidth: 2,
                hoverBorderColor: '#054564',
            },
            {
                label: 'Planned Area',
                data: [],
                backgroundColor: '#b8de6f',
                hoverBorderWidth: 2,

                hoverBorderColor: '#054564',
            }
            ]
        },
        options: {
            scales: {
                xAxes: [{
                    categoryPercentage: 0.6,
                    barPercentage: 1.0
                }]
            }
        },
    });




    //------------------------------------------------------------------------- Charts Section End  -------------------------------------------------------------------------\\

    //------------------------------------------------------------------------- Modals Section -------------------------------------------------------------------------\\


    $scope.ActiveFarmersModal = [];
    $scope.currentGroup;
   
    
    $scope.showActiveFarmerModal = function (data) {
        //$scope.dd = data;
        $http({
            method: 'GET',
            url: 'Farming/FarmingDashboard/getActiveFarmers',
            params: {
                'column': data.standardName, 'groups': data.Group,
                'landId': $scope.farmingDropDown.LandId,
                'cropId': $scope.farmingDropDown.CropId,
                'cropTypeId': $scope.farmingDropDown.CropTypeId,
                'cropCategoryId': $scope.farmingDropDown.CropCategoryId,
                'cropSubCategoryId': $scope.farmingDropDown.CropSubCategoryId
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            
            $scope.ActiveFarmersModal = response.data;
            $scope.currentGroup = data;
            //var data1 = pagination($scope.FarmersModal, $scope.page);
          //  $scope.FarmersModal = data1.tableData;
          // pageButtons(data1.pages , $scope.dd);
           // LoadData($scope.FarmersModal);
            
            
            angular.element(document.querySelector('#ShowActiveFarmers')).modal('show');
            
        });
    }

    $scope.InactiveFarmersModal = [];


    $scope.showInactiveFarmerModal = function (data) {
        //$scope.dd = data;
        $http({
            method: 'GET',
            url: 'Farming/FarmingDashboard/getInactiveFarmers',
            params: {
                'column': data.standardName, 'groups': data.Group,
                'landId': $scope.farmingDropDown.LandId,
                'cropId': $scope.farmingDropDown.CropId,
                'cropTypeId': $scope.farmingDropDown.CropTypeId,
                'cropCategoryId': $scope.farmingDropDown.CropCategoryId,
                'cropSubCategoryId': $scope.farmingDropDown.CropSubCategoryId
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {

            $scope.InactiveFarmersModal = response.data;
            $scope.currentGroup = data;
            //var data1 = pagination($scope.FarmersModal, $scope.page);
            //  $scope.FarmersModal = data1.tableData;
            // pageButtons(data1.pages , $scope.dd);
            // LoadData($scope.FarmersModal);


            angular.element(document.querySelector('#ShowInactiveFarmers')).modal('show');

        });
    }

    // Total Area Modal
    $scope.TotalAreaModal = [];
    $scope.showTotalArea = function (data) {
        //$scope.dd = data;
        $http({
            method: 'GET',
            url: 'Farming/FarmingDashboard/getTotalArea',
            params: {
                'column': data.standardName, 'groups': data.Group,
                'landId': $scope.farmingDropDown.LandId,
                'cropId': $scope.farmingDropDown.CropId,
                'cropTypeId': $scope.farmingDropDown.CropTypeId,
                'cropCategoryId': $scope.farmingDropDown.CropCategoryId,
                'cropSubCategoryId': $scope.farmingDropDown.CropSubCategoryId },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            
            $scope.TotalAreaModal = response.data;
            $scope.currentGroup = data;
           // var data1 = pagination($scope.FarmersModal, $scope.page);
           // $scope.FarmersModal = data1.tableData;
            //pageButtons(data1.pages, $scope.dd);
            //LoadData($scope.FarmersModal);
            angular.element(document.querySelector('#ShowTotalArea')).modal('show');
        });
    }

  

    $scope.PlannedAreaModal = [];
    $scope.showPlannedArea = function (data) {
        //$scope.dd = data;
        $http({
            method: 'GET',
            url: 'Farming/FarmingDashboard/getPlannedArea',
            params: {
                'column': data.standardName, 'groups': data.Group,
                'landId': $scope.farmingDropDown.LandId,
                'cropId': $scope.farmingDropDown.CropId,
                'cropTypeId': $scope.farmingDropDown.CropTypeId,
                'cropCategoryId': $scope.farmingDropDown.CropCategoryId,
                'cropSubCategoryId': $scope.farmingDropDown.CropSubCategoryId },
            dataType: 'JSON',
        }).then(function successCallback(response) {

            $scope.PlannedAreaModal = response.data;
            $scope.currentGroup = data;
            // var data1 = pagination($scope.FarmersModal, $scope.page);
            // $scope.FarmersModal = data1.tableData;
            //pageButtons(data1.pages, $scope.dd);
            //LoadData($scope.FarmersModal);
            angular.element(document.querySelector('#ShowPlannedArea')).modal('show');
        });
    }

    $scope.ConfirmPrintTab = function (Id) {
        $scope.FarmerMasterPrintTabId = Id;
        //     var data = args.data;
        var reportFormat = "Excel";
        try {
            window.open('Farming/FarmingDashboard/GetFarmerPrintReport?reportFormat=' + reportFormat + '&FarmerMasterPrintId=' + $scope.FarmerMasterPrintTabId, '_blank');


        } catch (e) {
            throw e;
        }
    }

    $scope.ActiveFarmersReport = function (Id) {
        $scope.FarmerMasterPrintTabId = Id;
        //     var data = args.data;
        var reportFormat = "Excel";
        try {
            
            window.open('Farming/FarmingDashboard/GetActiveFarmersPrintReport?reportFormat=' + reportFormat + '&column=' + Id.standardName + '&groups=' + Id.Group +
                '&landId=' + $scope.farmingDropDown.LandId +
                '&cropId=' + $scope.farmingDropDown.CropId +
                '&cropTypeId=' + $scope.farmingDropDown.CropTypeId +
                '&cropCategoryId=' + $scope.farmingDropDown.CropCategoryId +
                '&cropSubCategoryId=' + $scope.farmingDropDown.CropSubCategoryId, '_blank');

        } catch (e) {
            throw e;
        }
    }

    $scope.InactiveFarmersReport = function (Id) {
        //     var data = args.data;
        var reportFormat = "Excel";
        try {

            window.open('Farming/FarmingDashboard/GetInactiveFarmersPrintReport?reportFormat=' + reportFormat + '&column=' + Id.standardName + '&groups=' + Id.Group +
                '&landId=' + $scope.farmingDropDown.LandId +
                '&cropId=' + $scope.farmingDropDown.CropId +
                '&cropTypeId=' + $scope.farmingDropDown.CropTypeId +
                '&cropCategoryId=' + $scope.farmingDropDown.CropCategoryId +
                '&cropSubCategoryId=' + $scope.farmingDropDown.CropSubCategoryId, '_blank');

        } catch (e) {
            throw e;
        }
    }


    $scope.TotalAreaReport = function (Id) {
        //     var data = args.data;
        var reportFormat = "Excel";
        try {

            window.open('Farming/FarmingDashboard/GetTotalAreaReport?reportFormat=' + reportFormat + '&column=' + Id.standardName + '&groups=' + Id.Group +
                '&landId=' + $scope.farmingDropDown.LandId +
                '&cropId=' + $scope.farmingDropDown.CropId +
                '&cropTypeId=' + $scope.farmingDropDown.CropTypeId +
                '&cropCategoryId=' + $scope.farmingDropDown.CropCategoryId +
                '&cropSubCategoryId=' + $scope.farmingDropDown.CropSubCategoryId, '_blank');

        } catch (e) {
            throw e;
        }
    }


    $scope.PlannedAreaReport = function (Id) {
        //     var data = args.data;
        var reportFormat = "Excel";
        try {

            window.open('Farming/FarmingDashboard/GetPlannedAreaReport?reportFormat=' + reportFormat + '&column=' + Id.standardName + '&groups=' + Id.Group +
                '&landId=' + $scope.farmingDropDown.LandId +
                '&cropId=' + $scope.farmingDropDown.CropId +
                '&cropTypeId=' + $scope.farmingDropDown.CropTypeId +
                '&cropCategoryId=' + $scope.farmingDropDown.CropCategoryId +
                '&cropSubCategoryId=' + $scope.farmingDropDown.CropSubCategoryId, '_blank');

        } catch (e) {
            throw e;
        }
    }
   
}