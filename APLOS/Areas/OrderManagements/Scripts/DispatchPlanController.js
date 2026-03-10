'use strict';
DispatchPlanController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function DispatchPlanController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Dispatch Plan";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'OrderManagements/DispatchMaster/';
    $scope.getListUrl = $scope.path + 'getlist?ids=null';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'DispatchPlanInsert';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.dispatchPlanNew = {
        Id: null,
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        CloseDate: $filter("dateFiltering")(Date.now()),
        PlanNo: null,
        ResponsiblePersonId: null,
        CheckBy: null,
        ApproveBy: null,
        MonthNo: null,
        YearNo: null,
        RevisionNo: null,
        ByWhom: null,

    };

    //$scope.dispatchPlanNew = Object.assign({}, $scope.dispatchPlanVM);

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'Costings/QuickCostingMaster/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    //$scope.getPopUpData();

    $scope.setEmpData = function (obj) {
        $scope.dispatchPlanNew.ResponsiblePersonId = obj.data.SystemID;
        $scope.responsiblePerson = obj.data.EmployeeName;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.Save = function () {
        //$scope.$broadcast('show-errors-check-validity');
        //if ($scope.dispatchPlanNewForm.$valid) {
        if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'data': $scope.dispatchPlanNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.getData();
                    //$scope.serviceMasters = $filter('orderBy')($scope.serviceMasters, 'Sequence');
                    //baseService.paginationAdd();
                    //ClearFields(response.data.Sequence);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        //else if ($scope.Action == "Update") {
        //    $http({
        //        method: 'POST',
        //        url: $scope.updateUrl,
        //        data: $scope.serviceMaster,
        //        dataType: 'JSON'
        //    }).then(function successCallback(response) {
        //        if (response.data.Error == true)
        //            ShowResult(response.data.Message, 'failure');
        //        else {
        //            ShowResult(response.data.Message, 'success');
        //            $scope.getData();
        //            ClearFields(response.data.Sequence);
        //        }
        //    }, function errorCallBack(response) {
        //        ShowResult(response.data.Message, 'failure');
        //    });
        //}
        // }
    }

    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.path + 'GetSampleFile?reportFormat=' + ReportFormat;
    };
}

