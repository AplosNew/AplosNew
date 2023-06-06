'use strict';
maternityLeaveReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function maternityLeaveReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Maternity Leave Report';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    
    $scope.NationlFestival = {      
        EmployeeId: null,
        ReportFormat: 'Excel',
        LanguageId: null,
        LeaveTransactionId: null,
    };

    $scope.EmployeeList = [];
    $scope.GetEmployeeInformation = function () {
        
            $scope.searchbyonRoleEmpList = [];
            //var parameters = { 'fromDate': $scope.NationlFestival.FromDate, 'toDate': $scope.NationlFestival.ToDate };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'HumanResource/AttendanceManagement/GetMaternityLeaveInformation',
                //data: parameters
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
                     
                        minWidth: 1000,
                        height: 300,
                        isResponsive: true,
                        actionComplete: $scope.actionCompleteSelected
                    });
                    $scope.dataGrid = "#empInfoGrid";
                }

                
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
    
    $scope.MaternityLeaveReport = function (obj) {
        try {
            //var empLeaveId = $scope.NationlFestival.LeaveTransactionId;

            $scope.UserName = $("#Lan option:selected").text();
            if (baseService.isUndefinedOrNull($scope.NationlFestival.LanguageId)) {
                throw 'Please Select Language';
            }
            var datum = obj.data;                             
            var url = 'HumanResource/MaternityLeaveTransaction/MaternityLeaveReport?reportFormat=' + $scope.NationlFestival.ReportFormat + '&SystemId=' + datum.SystemId + '&LanguageId=' + $scope.NationlFestival.LanguageId + '&UserName=' + $scope.UserName + '&LeaveTransactionId=' + datum.LeaveTransactionId +'&fromDate='
                + datum.FromDate;
                $rootScope.report(url);
                }
                   
             catch (e) {
                ShowResult(e, 'failure');

            }
    };
    $scope.tempList = [];   

    $scope.maternityList = [];
    $http.get('Employees/EmployeeInformation/GetDefaultCbo')
        .then(function (response) {
            $scope.maternityList = response.data;
        });
}