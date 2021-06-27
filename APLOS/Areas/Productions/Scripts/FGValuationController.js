'use strict';
FGValuationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function FGValuationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'FGValuation';
   
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.model = {
        //FromDate: $filter('dateFiltering')(firstDay),
        //ToDate: $filter('dateFiltering')(Date.now()),
        FromDate: null,
        ToDate: null
    };
    
    $scope.valuationList = [];
    $scope.LoadData = function () {
        try {
                        
            if (baseService.isUndefinedOrNull($scope.model.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.model.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }
            else if (new Date($scope.model.FromDate) > new Date($scope.model.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.model.ToDate) < new Date($scope.model.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else {
                $http({
                    method: 'GET',
                    url: 'Productions/FGValuation/GetValuationData?fromDate=' + $scope.model.FromDate + '&toDate=' + $scope.model.ToDate
                }).then(function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.valuationList = response.data;
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    
}