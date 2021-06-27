'use strict';
nationalFestivalController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function nationalFestivalController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Nation Festival';
    //$controller('employeeBaseController', { $scope: $scope, $http: $http });

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.NationlFestival = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        EmployeeId: null,
        ReportFormat: 'Excel',
        chkAdditionInfo:false
    };

    $scope.EmployeeList = [];
    $scope.GetEmployeeInformation = function () {
        
            $scope.searchbyonRoleEmpList = [];
            var parameters = { 'fromDate': $scope.NationlFestival.FromDate, 'toDate': $scope.NationlFestival.ToDate };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'HumanResource/AttendanceManagement/GetEmployeeInformation',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.EmployeeList = response.data;

                    if (baseService.arrayLength($scope.searchbyDetaillist) === 0) {
                        baseService.getDDLSearchColumn(response.data, $scope.searchbyonRoleEmpList);
                    }
                    var fieldList = [];
                    for (var i = 0; i < $scope.searchbyonRoleEmpList.length; i++) {
                        fieldList.push({ field: $scope.searchbyonRoleEmpList[i].Value, visible: true, width: "180px" });
                    }

                    $('#empInfoGrid').ejGrid({
                        dataSource: response.data,
                        allowPaging: true,
                        allowFiltering: true,
                        pageSettings: { pageSize: "10" },
                        allowKeyboardNavigation: true,
                        columns: fieldList,
                        filterSettings: { filterType: "excel" },
                        allowScrolling: true,
                        //scrollSettings: { width: 1200, height: 400 }
                        minWidth: 1000,
                        height: 300,
                        isResponsive: true,
                        actionComplete: $scope.actionCompleteSelected
                    });
                    $scope.dataGrid = "#empInfoGrid";
                }

                //angular.element(document.querySelector('#empInfo')).modal('show');
            });
    };

    var sqlInStatement = "";
    $scope.actionCompleteSelected = function (args) {
        try {
            var gridObj = $("#empInfoGrid").ejGrid("instance");

            if (args.requestType === "refresh") {
                var scrollerwidth = $("#empInfo").width();//Obtain the width of the container
                $("#Grid").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }

            if (args.requestType === "filtering") {
                var filtereddata = gridObj.getFilteredRecords();
                var uniqueEmpSystemId = removeDuplicates(filtereddata, 'EmpSystemId');
                var wcEmpCode = "";
                if (uniqueEmpSystemId.length > 0) {
                    wcEmpCode = "IN(";
                    wcEmpCode += Array.prototype.map.call(uniqueEmpSystemId, function (item) { return "'" + item.EmpSystemId + "'"; }).join(",") + ")";
                }
                sqlInStatement = wcEmpCode;
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }
    
    $scope.NationlFestivalLeave = function (obj) {
        try {
            var datum = obj.data;
                if (baseService.isUndefinedOrNull($scope.Id)) {
                    throw 'Please Select Year';
                }               
            var url = 'HumanResource/AttendanceManagement/GetNationalFestivalReport?reportFormat=' + $scope.NationlFestival.ReportFormat + "&CalanderYearId=" + $scope.Id + '&fromDate=' + $scope.FromDate + '&toDate=' + $scope.ToDate + '&EmpSystemId=' + datum.EmpSystemId;
                $rootScope.report(url);
                }
                   
             catch (e) {
                ShowResult(e, 'failure');

            }
    };
    $scope.tempList = [];
    
    $scope.ClanderYearModel = {
        Id: null,
        YearNo: null,
        FromDate: null,
        ToDate: null
    }
    $scope.Id = null;
    $scope.ClanderYear = [];
    $scope.GetClanderYear = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Employees/EmployeeInformation/GetClanderYear'

        }).then(function successCallback(response) {
            $scope.ClanderYear = response.data.data;

        });
    }
    $scope.GetClanderYear();

    $scope.getFromAndToDate = function () {
        $scope.OBJ = $filter("filter")($scope.ClanderYear, { Id: $scope.Id });
        $scope.ClanderYearModel = $scope.OBJ[0];
        $scope.FromDate = $scope.ClanderYearModel.FromDate;
        $scope.ToDate = $scope.ClanderYearModel.ToDate;
    };
}