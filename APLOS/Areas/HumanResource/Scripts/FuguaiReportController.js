'use strict';
FuguaiReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FuguaiReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Fuguai Report';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/FuguaiReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl);


    $scope.ModelTemp = {
        Id: null,
        FromDate: null,
        ToDate: null,      
        ByWhom: null,
        ZoneMasterId: null,
        ZoneCategory: null,        
        ResponsiblePersonId: null,
        FinalStatus: null,        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ObservedByList = [];
    $scope.getByWhom = function () {
        $http({
            method: 'POST',
            data: {
                'FromDate': $scope.ModelNew.FromDate,
                'ToDate': $scope.ModelNew.ToDate,
            },
            url: $scope.path + 'getByWhom',
        }).then(function success(response) {
            
            $scope.ObservedByList = response.data;
        });

    }
    $scope.getByWhom();

    $scope.ResponsibleList = [];
    $scope.getResponsiblePerson = function () {
        $http({
            method: 'POST',
            
            url: $scope.path + 'getResponsiblePerson',
        }).then(function success(response) {

            $scope.ResponsibleList = response.data;
        });

    }
    $scope.getResponsiblePerson();

    $scope.CategoryList = [];
    $scope.getCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getCategory',
        }).then(function success(response) {
            $scope.CategoryList = response.data;
        });
    }
   $scope.getCategory();

    $scope.TagList = [];
    $scope.getFuguai = function () {
        $http({
            method: 'POST',
            data: {
                'categoryText': $scope.ModelNew.ZoneCategory,
            },
            url: $scope.path + 'getFuguai',
        }).then(function success(response) {
            $scope.TagList = response.data;
        });
    }

    $scope.FinalStatusList = [];
    $scope.getFinalStatus = function () {
        $http({
            method: 'POST',
            data: {
                'categoryText': $scope.ModelNew.ZoneCategory,
            },
            url: $scope.path + 'getFinalStatus',
        }).then(function success(response) {
            $scope.FinalStatusList = response.data;
        });
    }

    $scope.FuguaiTransactionList = [];
    $scope.getFuguaiTransaction = function () {
        $http({
            method: 'POST',
            data: {
                'SystemId': $scope.ModelNew.ResponsiblePersonId,
                'ObservedById': $scope.ModelNew.ObservedById,
            },
            url: $scope.path + 'getFuguaiTransaction',
        }).then(function success(response) {
            $scope.FuguaiTransactionList = response.data;
        });

    }

    $scope.GetReport = function () {
        var reportFormat = "Excel";
        try {
            var file_src = 'HumanResource/FuguaiReport/GetFuguaiReport?FromDate=' + $scope.ModelNew.FromDate + '&ToDate=' + $scope.ModelNew.ToDate + '&FinalStatus=' + $scope.ModelNew.FinalStatus;
            $rootScope.report(file_src);
        } catch (e) {

        }
    };
}