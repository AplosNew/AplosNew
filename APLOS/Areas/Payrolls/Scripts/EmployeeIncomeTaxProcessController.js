'use strict';
EmployeeIncomeTaxProcessController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeIncomeTaxProcessController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Income Tax Process';
    $scope.path = 'Payrolls/EmployeeIncomeTaxProcess/';
    $scope.Diffpath = 'Payrolls/EmployeeIncomeTax/';

    // #region Page Load Function Call

    $scope.YearList = [];
    $scope.PlantList = [];
    $scope.TaxTypeList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.Diffpath+ 'GetTaxYear',
        }).then(function successCallback(response) {
            $scope.YearList = response.data;
        });

        $http({
            method: 'GET',
            url: $scope.Diffpath +'GetTaxType',
        }).then(function successCallback(response) {
            $scope.TaxTypeList = response.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetPlants',
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
        });

    }
    $scope.getData();

    // #endregion

    // #region Modals

    $scope.selectedValues = {
        TaxYearId: null,
        TaxTypeId: null,
        CityOfResidence: null,
        EarningAmount: null,
        PolicyId: null,
        PlantId: null,
        Gender: null,
        StartDate: null,
        EndDate: null
    };
     
    $scope.clearFliters = function () {
        $scope.selectedValues = {
            TaxYearId: null,
            TaxTypeId: null,
            CityOfResidence: null,
            EarningAmount: null,
            PolicyId: null,
            PlantId: null,
            Gender: null,
            StartDate: null,
            EndDate: null
        };
    }

    // #endregion

    // Policy Finding
    $scope.TaxPolicyList = [];
    $scope.GetTaxPolicyList = function () {

        if (angular.isUndefinedOrNull($scope.selectedValues.Gender)
            || angular.isUndefinedOrNull($scope.selectedValues.CityOfResidence)
            || angular.isUndefinedOrNull($scope.selectedValues.TaxYearId)
            || angular.isUndefinedOrNull($scope.selectedValues.PlantId)
            || angular.isUndefinedOrNull($scope.selectedValues.EarningAmount))
        {
            ShowResult("Please select Required Fields !", 'failure');
            throw ('Please select Required Fields !!');
        }

        $http({
            method: 'POST',
            url: $scope.Diffpath + "GetTaxPolicy",
            data: {
                'Residence': $scope.selectedValues.CityOfResidence,
                'YearId': $scope.selectedValues.TaxYearId,
                'Gender': $scope.selectedValues.Gender
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.TaxPolicyList = [];
            $scope.TaxPolicyList = response.data;
            if ($scope.TaxPolicyList.length>0)
            {
                $scope.selectedValues.PolicyId = response.data[0].PolicyHeaderId;
                $scope.selectedValues.StartDate = response.data[0].StartDate;
                $scope.selectedValues.EndDate = response.data[0].EndDate;
                $scope.loadGrid();
            }
        });
    }
 
    /// --- Grid Show
    $scope.MainData = [];
    $scope.loadGrid = function () {

        if (angular.isUndefinedOrNull($scope.selectedValues.PolicyId)) {
            ShowResult("Please First Configure the Policy !", 'failure');
            throw ('Please First Configure the Policy !!');
        }   

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MainData.length != 0) {
            $scope.destroy();
        }
        if ($scope.General.$valid) {

            var ColumnList = [
                { field: 'EmpInfoSystemID', width: 120, headerText: "EmployeeId", type: "string" },
                { field: 'EmployeeName', width: 150, headerText: "Employee Name", type: "string" },
                { field: 'Dept', width: 150, headerText: "Department", type: "string" },
                { field: 'Section', width: 150, headerText: "Section", type: "string" },
                { field: 'Unit', width: 150, headerText: "Unit", type: "string" },
                { field: 'SubSection', width: 150, headerText: "SubSection", type: "string" },
                { field: 'StructureEarning', width: 150, headerText: "Structure Salary", type: "string" }
            ];
           
            $http({
                method: 'POST',
                url: $scope.path + 'GetData',
                data: {
                    PolicyId: $scope.selectedValues.PolicyId, PlantId: $scope.selectedValues.PlantId,
                    Earning: $scope.selectedValues.EarningAmount
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.MainData = response.data.DATA;

                    $("#GridData").ejGrid({
                        dataSource: $scope.MainData,
                        minWidth: 450, minHeight: 400,
                        allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                        filterSettings: { filterType: "excel" },
                        columns: ColumnList
                    });


                    var gridObj = $("#GridData").data("ejGrid");
                    gridObj.refreshContent(true);
                    gridObj.refreshTemplate();

                    var x = document.getElementById("but");
                    if ($scope.MainData.length != 0) {
                        x.style.display = "block";
                    }
                    else {
                        x.style.display = "none";
                    }
                }
            });

        }

    }

    //Grid Destroy
    $scope.destroy = function () {
        var grid = $("#GridData").data("ejGrid");
        grid.destroy();
    }


    $scope.filteringData = function () {

        if (angular.isUndefinedOrNull($scope.selectedValues.PolicyId)) {
            ShowResult("Please First Configure the Policy !", 'failure');
            throw ('Please First Configure the Policy !!');
        }

        var gridobj = $("#GridData").data("ejGrid");
        var filteredRecords = gridobj.getFilteredRecords();

        if (filteredRecords.length == 0) {
            filteredRecords = $scope.MainData;
        }

        var parameters = [];
        parameters.push({ "Key": "EmpInfoSystemID", "Value": getString(filteredRecords, "EmpInfoSystemID") });
        applyFilters(parameters);
              
    } 

    function applyFilters(parameters) {

        $http({
            method: 'POST',
            url: $scope.path + 'ProcessFunction',
            data: {
                PolicyId: $scope.selectedValues.PolicyId, PlantId: $scope.selectedValues.PlantId,
                YearId: $scope.selectedValues.TaxYearId, TaxTypeId: $scope.selectedValues.TaxTypeId,
                EmpId: parameters[0].Value, StartDate: $scope.selectedValues.StartDate,
                EndDate: $scope.selectedValues.EndDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
        
    }

    var getString = function (data, column) {
        var kk = "";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                if (kk === "") {
                    kk += "'" + data[i][column] + "'";
                }
                else if (data[i][column] == null) {
                }
                else {
                    kk += ",'" + data[i][column] + "'";
                }
            }

        }
        return kk;
    } 


}