'use strict';
hrCutOffDateOpeningBalanceController.$inject = ['$rootScope', '$scope', 'cboService', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function hrCutOffDateOpeningBalanceController($rootScope, $scope, cboService, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Opening Balance CutOff Date';
    $scope.Action = 'Save';
    $scope.ngShowTbl = false;
    $scope.index = -1;
    $scope.openingBalanceList = [];
    $scope.openingBalanceCutOffDates = [];

    $scope.openingBalanceCutOffDate = {
        Id: null,
        StandardName: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        IsEntityLevel: null,
        CutOffDate: null,
        Remarks: null
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getCboCompanyByCompanyGroup = function (companyGroupId) {
        cboService.getCboCompanyByCompanyGroup(companyGroupId, function (result) {
            $scope.companyList = result;
        });
    };

    $scope.getData = function (pageno) {
        $rootScope.parameters.companyGroupId = $scope.openingBalanceCutOffDate.CompanyGroupId;
        $rootScope.parameters.companyId = $scope.openingBalanceCutOffDate.CompanyId;
        baseService.init('accounts/OpeningBalance/GetHRCutOffDateList', null, null, null, 'PlantName');
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.openingBalanceList = result.Rows;
                for (var i = 0; i < $scope.openingBalanceList.length; i++) {
                    $scope.openingBalanceList[i].CutOffDate = $filter('dateFiltering')($scope.openingBalanceList[i].CutOffDate);
                    $scope.openingBalanceList[i].CompanyGroupId = $scope.openingBalanceCutOffDate.CompanyGroupId;
                    $scope.openingBalanceList[i].CompanyId = $scope.openingBalanceCutOffDate.CompanyId;
                }
                $scope.ngShowTbl = true;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $rootScope.searchByList = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Cut Off Date',
            'value': 'CutOffDate'
        }
    ];

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.comSRForm.$valid) {
            $http({
                method: 'POST',
                url: 'accounts/OpeningBalance/CreateHRCutOffDate',
                data: {
                    'openingBalanceCutOffDates': $scope.openingBalanceList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.openingBalanceList.push(response.data.OpeningBalanceCutOffDate);
                    $scope.openingBalanceCutOffDate.CompanyId = $scope.companyId;
                    baseService.paginationAdd();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.openingBalanceCutOffDate = {
            CompanyGroupId: $scope.openingBalanceCutOffDate.CompanyGroupId,
            CompanyId: $scope.openingBalanceCutOffDate.CompanyId
        };
        $scope.getData();
    }
}