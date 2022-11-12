'use strict';
EmployerMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', '$window', 'baseService', '$routeParams', '$location', '$http', '$controller', '$filter'];
function EmployerMasterController(cboService, commonMessage, $scope, $rootScope, $window, baseService, $routeParams, $location, $http, $controller, $filter) {
    $rootScope.title = 'Medicine Receipt';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/EmployerMaster/';
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

    $scope.EmployerNameList = [
        {
            "Value": "Arvind Ltd.",
            "Text":"Arvind Ltd."
        },
        {
            "Value": "Vardhman Textiles Ltd.",
            "Text": "Vardhman Textiles Ltd."
        },
        {
            "Value": "Welspun India Ltd.",
            "Text": "Welspun India Ltd."
        },
        {
            "Value": "Raymond Ltd.",
            "Text": "Raymond Ltd."
        },
        {
            "Value": "Trident Ltd.",
            "Text": "Trident Ltd."
        },
        {
            "Value": "K P R Mill Ltd.",
            "Text": "K P R Mill Ltd."
        },
        {
            "Value": "Page Industries Ltd.",
            "Text": "Page Industries Ltd."
        },
        {
            "Value": "Nitin Spinners Ltd.",
            "Text": "Nitin Spinners Ltd."
        },
        {
            "Value": "Rupa & Company Ltd.",
            "Text": "Rupa & Company Ltd."
        },
        {
            "Value": "Himatsingka Seide Ltd.",
            "Text": "Himatsingka Seide Ltd."
        },
        {
            "Value": "Other",
            "Text": "Other"
        },
    ]
}