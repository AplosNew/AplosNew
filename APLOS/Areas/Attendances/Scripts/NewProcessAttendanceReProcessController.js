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


    $scope.parameters = [];
    $scope.filters = [];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;

            var gridObj = $("#PlantList").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#PlantList").children('.e-pager.e-js.e-pager').hide();
            $("#PlantList").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#PlantList").children('.e-gridcontent').hide();

        });
    }
    $scope.loadfilters();    

}