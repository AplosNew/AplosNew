organizationsBaseController.$inject = ['$scope', '$http', '$filter', 'baseService', '$rootScope'];
function organizationsBaseController($scope, $http, $filter, baseService, $rootScope) {
    $scope.searchList = [];
    $scope.dataPlate = [];
    $scope.popUpUrl = null;

    // #region Section

    $scope.sectionPopUpDataList = function () {
        $scope.dataPlate = [];
        $scope.searchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.sectionPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.popUpUrl = 'Organizations/Section/GetSectionList?sectionIds=' + baseService.getColumnValueList($scope.sectionList, 'SectionId');
        angular.forEach($scope.sectionList, function (a) {
            $rootScope.tempList.push({
                Id: a.SectionId
                , Sequence: a.Sequence
                , Code: a.Code
                , UserName: a.SectionName
                , Active: a.Active
            });
        });
        baseService.setCurrentPage('dataPlate');
        $scope.getSectionDataList = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.sectionPopUpParameters)
                .then(function (result) {
                    $scope.dataPlate = result.Rows;
                    $scope.sectionPopUpParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.dataPlate); t++) {
                        $scope.dataPlate[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.dataPlate[t].Id);
                    }
                    if (baseService.arrayLength($scope.searchList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                    }
                    angular.element(document.querySelector('#sectionPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'sectionPopUp');
                }).finally(function () {
                });
        };
        $scope.getSectionDataList();
    };
    $scope.closeSectionPopUp = function () {
        $scope.popUpUrl = null;
        $scope.dataPlate = [];
        $scope.searchList = [];
        angular.element(document.querySelector('#sectionPopUp')).modal('hide');
    };

    // #endregion Section

}