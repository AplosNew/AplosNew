'use strict';
LegalSalaryStructureController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function LegalSalaryStructureController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = 'Legal Salary Structure';
    $scope.path = 'HumanResource/legalsalarystructure/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getHeadListUrl = $scope.path + 'getheadlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete?id=';
    $scope.index = -1;

    //$scope.legalSalaryGradeList = [];
    //$http.get('HumanResource/LegalSalaryGrade/getcbo')
    //    .then(function (response) {
    //        $scope.legalSalaryGradeList = response.data;
    //    });
    $scope.legalSalaryGrade = {
        Id: null,
        LegalSalaryGradeId: null,
        EffectiveDate: new Date(),
        EmployeeLocationId: null,
        CompanyId: null,
        PlantId: null,

    };
    $scope.legalSalaryGradeNew = Object.assign({}, $scope.legalSalaryGrade);

    $scope.companyList = [];
    $scope.legalSalaryGradeHeadList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.legalSalaryGradeHeadList = [];
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.legalSalaryGradeNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.legalSalaryGradeList = [];
    $scope.getLegalSalaryGradeList = function () {
        cboService.getCboLegalSalaryGrade($scope.legalSalaryGradeNew.PlantId, function (result) {
            $scope.legalSalaryGradeList = result;
        });
    };


    $scope.employmentLocationList = [];
    cboService.getEmployeeLocationCbo(function (result) {
        $scope.employmentLocationList = result;
    });
    $scope.legalSalaryList = [];
    $scope.searchByList = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Effective Date',
            'value': 'EffectiveDate'
        }
    ];
    baseService.init($scope.getListUrl, null, null, 'desc', 'EffectiveDate', 'EffectiveDate');
    $scope.getData = function (pageno) {
        $rootScope.parameters.CompanyId = $scope.legalSalaryGradeNew.CompanyId;
        $rootScope.parameters.PlantId = $scope.legalSalaryGradeNew.PlantId;
        $rootScope.parameters.legalSalaryGradeId = $scope.legalSalaryGradeNew.LegalSalaryGradeId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.legalSalaryGrades = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getHead = function () {
        $http({
            method: 'GET',
            url: $scope.getHeadListUrl + '?legalSalaryGradeId=' + $scope.legalSalaryGradeNew.LegalSalaryGradeId,
            dataType: 'JSON',
            contentType: 'application/json; charset=utf-8'
        }).then(function (response) {
            $scope.legalSalaryList = response.data;
        });
    };
    $scope.getHeadEdit = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetHeadEdit?id=' + $scope.legalSalaryGradeNew.Id,
            dataType: 'JSON',
            contentType: 'application/json; charset=utf-8'
        }).then(function (response) {
            $scope.legalSalaryList = response.data;
        });
    };
    $scope.load = function () {
        $scope.legalSalaryGradeNew.Id = null;
        $scope.index = -1;
        $scope.getData();
        $scope.getHead();
        //if ($rootScope.isCollapsed) {
        //    $rootScope.toggle();
        //}
    };
    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.legalSalaryGrades[$scope.index], $scope.legalSalaryGrade);
        angular.copy($scope.legalSalaryGrade, $scope.legalSalaryGradeNew);
        $scope.getHeadEdit();
        $scope.Action = 'Update';
        //if (!$rootScope.isCollapsed) {
        //    $rootScope.toggle();
        //}
    };
    $scope.Save = function () {
        if (baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.CompanyId))
            return ShowResult('Please select Company.');
        if (baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.PlantId))
            return ShowResult('Please select Plant');
        if (baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.LegalSalaryGradeId))
            return ShowResult('Please select legal salary grade');
        if (baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.EffectiveDate))
            return ShowResult('Please select effective date');
        if (baseService.arrayLength($scope.legalSalaryList) == 0)
            return ShowResult('Can not save with out salary head value.');
        var salaryHV = 0;
        for (var i = 0; i < baseService.arrayLength($scope.legalSalaryList); i++) {
            salaryHV = salaryHV + parseFloat(baseService.isUndefinedOrNull($scope.legalSalaryList[i].SalaryHeadValue) ? 0 : $scope.legalSalaryList[i].SalaryHeadValue);
        }
        if (salaryHV === 0)
            return ShowResult('Sum of value can not be 0 or null.');
        $scope.legalSalaryGrade = Object.assign({}, $scope.legalSalaryGradeNew);
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                entity: $scope.legalSalaryGrade,
                values: $scope.legalSalaryList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Clear();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.legalSalaryGradeNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.legalSalaryGrades.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        $scope.legalSalaryGrade = {};
        $scope.legalSalaryGradeNew = { EffectiveDate: new Date(), CompanyId: $scope.legalSalaryGradeNew.CompanyId, PlantId: $scope.legalSalaryGradeNew.PlantId,LegalSalaryGradeId: $scope.legalSalaryGradeNew.LegalSalaryGradeId };
        $scope.legalSalaryList = [];
        $scope.legalSalaryGrades = [];
        $scope.getHead();
        $scope.getData();
        $scope.index = -1;
    };
}
