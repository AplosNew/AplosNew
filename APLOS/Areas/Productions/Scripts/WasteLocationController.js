'use strict';
WasteLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WasteLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Waste Location';
    $scope.buyerStyles = [];
    $scope.path = 'Productions/WasteLocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.index = -1;
    $scope.showTbl = false;
   

    $scope.getDataList = function (buyerId) {
        baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.buyerStyleNew.CompanyId;
            $rootScope.parameters.plantId = $scope.buyerStyleNew.PlantId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerStyles = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        $scope.GetSequence();
    }
    $rootScope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.buyerStyleNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl = $scope.path + 'getautosequence?companyId=' + $scope.buyerStyleNew.CompanyId + '&plantId=' + $scope.buyerStyleNew.PlantId)
            .then(function (response) {
                $scope.buyerStyleNew.Sequence = response.data;
            });
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerStyle = $scope.buyerStyles[$scope.index];
        $scope.buyerStyleNew = $scope.buyerStyle;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
   
}