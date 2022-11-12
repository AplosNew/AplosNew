'use strict';
ExperienceMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', '$window', 'baseService', '$routeParams', '$location', '$http', '$controller', '$filter'];
function ExperienceMasterController(cboService, commonMessage, $scope, $rootScope, $window, baseService, $routeParams, $location, $http, $controller, $filter) {
    $rootScope.title = 'Medicine Receipt';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/ExperienceMaster/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveUrlP = $scope.path + 'SavePurpose';
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);

    $scope.DepartmentList = [];
    $scope.GetDepartment = function () {
        $http.get('Materials/DetentionLogReport/GetDepartment')
            .then(
                function successCallback(response) {
                    $scope.DepartmentList = response.data;

                }
            )
    }
    $scope.GetDepartment();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        AreaOfExperience: null,
        Department: null,
        IsActive:true
    };
    $scope.ModalNew = Object.assign({}, $scope.ModelTemp);
}