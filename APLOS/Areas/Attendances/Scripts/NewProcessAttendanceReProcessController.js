'use strict';
NewProcessAttendanceReProcessController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function NewProcessAttendanceReProcessController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Attendance Reprocess';
    $scope.path = 'Attendances/NewProcessAttendanceReProcess/';

    $scope.selectedValues = {
        FromDate: null,
        ToDate: null       
    };

 
    $scope.clearFliters = function () {
        $scope.selectedValues.FromDate = null;
        $scope.selectedValues.ToDate = null;      
    }

    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: $scope.path + "getFilters",
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;

            var index = 0;
            for (var i = 0; i < $scope.PlantList.length; i++) {
                if ($scope.PlantList[i].PlantId == $window.plantId) {
                    index = i;
                }
            }

            $('#CWPlant').ejDropDownList(
                {
                    dataSource: $scope.PlantList,
                    fields: { text: "PlantName", value: "PlantId" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 180
                });


        });
    }
    $scope.getPlant();

    /// ReProcess Function

    $scope.ReProcessFunction = function () {
        try {
            var PlantId = "";
            var DropDownListObj = $("#CWPlant").data("ejDropDownList");
            if (!baseService.isUndefinedOrNull(DropDownListObj)) {
                PlantId = DropDownListObj.getSelectedValue();

                if (baseService.isUndefinedOrNull(PlantId)) {
                    throw "Select Plant..";
                }
            }


            if (angular.isUndefinedOrNull($scope.selectedValues.FromDate)) {
                ShowResult("Select From Date", 'failure');
            }
            if (angular.isUndefinedOrNull($scope.selectedValues.ToDate)) {
                ShowResult("Select To Date", 'failure');
            }

            else {

                var parameters = {
                    'From': $scope.selectedValues.FromDate, 'To': $scope.selectedValues.ToDate,
                    'PlantId': PlantId
                };
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: $scope.path + '/ReProcessAttendance',
                    data: parameters
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');

                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

      
}