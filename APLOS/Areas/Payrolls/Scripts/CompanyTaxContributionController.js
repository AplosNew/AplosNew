'use strict';
companyTaxContributionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function companyTaxContributionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Company Tax Contribution";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.companyTaxContributions = [];
    $scope.path = 'Payrolls/companytaxcontribution/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.companyTaxContributions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    // $scope.getData();
    $scope.companyTaxContribution = {
        Id: null,
        CompanyGroupId: null,
        PlantId: null,
        EmpSystemId: null,
        TaxYearId: null,
        IsFixed: 'Fixed',
        Amount: 0
    };

    $scope.companyTaxContributionNew = Object.assign({}, $scope.companyTaxContribution);

    cboService.getCboPlantByCompany(null, function (result) {
        $scope.plantList = result;
    });

    $scope.taxYearList = [];
    $scope.getTaxYear = function () {
        $http({
            method: 'GET',
            url: 'accounts/CompanyTaxYear/GetCompanyTaxYearCbo?id=' + window.companyId
        }).then(function successCallback(response) {
            $scope.taxYearList = response.data;
        });
    };
    $scope.getTaxYear();

    $scope.searchInEmployeesList = [{
        'name': 'Employee SystemId',
        'value': 'SystemId'
    },
    {
        'name': 'Employee Name',
        'value': 'EmployeeName'
    },
    {
        'name': 'Email',
        'value': 'EmailId'
    },
    {
        'name': 'Department',
        'value': 'Department'
    },
    {
        'name': 'Given Designation',
        'value': 'GivenDesignation'
    }];

    $scope.EmployeePopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'SystemId',
        searchBy: "SystemId",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getEmployeesInPopup = function (pageno, plantId) {
        try {
            if (baseService.isUndefinedOrNull($scope.companyTaxContributionNew.PlantId)) {
                throw 'Please Select Plant !!';
            }
            baseService.paginationBase('Payrolls/CompanyTaxContribution/GetAllEmployee?plantId=' + $scope.companyTaxContributionNew.PlantId, pageno, $scope.EmployeePopUpParameters)
                .then(function (result) {
                    $scope.dataListEmployee = result.Rows;
                    $scope.EmployeePopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            angular.element(document.querySelector('#PopUpEmployee')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure')
        }
    }

    $scope.selectedEmployee = function (dr) {
        $scope.companyTaxContributionNew.EmpSystemId = dr.SystemId;
        $scope.companyTaxContributionNew.EmployeeName = dr.EmployeeName;
        angular.element(document.querySelector('#PopUpEmployee')).modal('hide');
        $scope.GetEmpData();
    }

    $scope.Validation = function () {
        if (baseService.isUndefinedOrNull($scope.companyTaxContributionNew.EmpSystemId)) {
            throw 'Please Select Employee..!!!';
        }
        else if (baseService.isUndefinedOrNull($scope.companyTaxContributionNew.TaxYearId)) {
            throw 'Please Select Tax Year..!!!';
        }
        if ($scope.companyTaxContributionNew.IsFixed === 'Fixed' && $scope.companyTaxContributionNew.Amount <= 0) {
            throw 'Fixed Amount can\'t be Zero..!!!';
        }
        if ($scope.companyTaxContributionNew.IsFixed === 'Percentage' && ($scope.companyTaxContributionNew.Amount <= 0 || $scope.companyTaxContributionNew.Amount > 100)) {
            throw 'Insert Percentage Amount between (1 to 100).';
        }
    }

    $scope.Save = function () {
        try {
            $scope.companyTaxContributionNew.Amount = Math.abs($scope.companyTaxContributionNew.Amount);
            $scope.Validation();
            if ($scope.companyTaxContributionNewForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.companyTaxContributionNew,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        } else {
                            ShowResult(response.data.Message, 'success');
                            $scope.companyTaxContributions.push(response.data.companyTaxContributionNew);
                        }
                    }), function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.companyTaxContributionNew,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        } else {
                            ShowResult(response.data.Message, 'success');
                            $scope.companyTaxContributions.push(response.data.companyTaxContributionNew);
                        }
                    }), function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure')
        }
    }

    //$scope.GetEmpData = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'getemployeeData?empId=' + $scope.companyTaxContributionNew.EmpSystemId + '&plantId=' + $scope.companyTaxContributionNew.PlantId
    //    }).then(function successCallback(response) {
    //        if (response.data.Rows.length > 0) {
    //            $scope.Action = 'Update';
    //            $scope.companyTaxContributionNew.Id = response.data.Rows[0].Id;
    //            $scope.companyTaxContributionNew.TaxYearId = response.data.Rows[0].TaxYearId;
    //            $scope.companyTaxContributionNew.IsFixed = response.data.Rows[0].IsFixed;
    //            $scope.companyTaxContributionNew.Amount = response.data.Rows[0].Amount;

    //            $scope.companyTaxContributionNew.CompanyGroupId = response.data.Rows[0].CompanyGroupId;
    //            $scope.companyTaxContributionNew.PlantId = response.data.Rows[0].PlantId;
    //            $scope.companyTaxContributionNew.EmpSystemId = response.data.Rows[0].EmpSystemId;
    //        } else {
    //            $scope.Action = 'Save';
    //            $scope.companyTaxContributionNew.Id = null;
    //            $scope.companyTaxContributionNew.TaxYearId = null;
    //            $scope.companyTaxContributionNew.IsFixed = 'Fixed';
    //            $scope.companyTaxContributionNew.Amount = null;
    //        }
    //    });
    //}

    $scope.GetEmpData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getemployeeDataTaxYearly?empId=' + $scope.companyTaxContributionNew.EmpSystemId + '&plantId=' + $scope.companyTaxContributionNew.PlantId + '&taxYear=' + $scope.companyTaxContributionNew.TaxYearId
        }).then(function successCallback(response) {
            if (response.data.Rows.length > 0) {
                $scope.Action = 'Update';
                $scope.companyTaxContributionNew.Id = response.data.Rows[0].Id;
                $scope.companyTaxContributionNew.TaxYearId = response.data.Rows[0].TaxYearId;
                $scope.companyTaxContributionNew.IsFixed = response.data.Rows[0].IsFixed;
                $scope.companyTaxContributionNew.Amount = response.data.Rows[0].Amount;

                $scope.companyTaxContributionNew.CompanyGroupId = response.data.Rows[0].CompanyGroupId;
                $scope.companyTaxContributionNew.PlantId = response.data.Rows[0].PlantId;
                $scope.companyTaxContributionNew.EmpSystemId = response.data.Rows[0].EmpSystemId;
            } else {
                $scope.Action = 'Save';
                $scope.companyTaxContributionNew.Id = null;
                $scope.companyTaxContributionNew.TaxYearId = null;
                $scope.companyTaxContributionNew.IsFixed = 'Fixed';
                $scope.companyTaxContributionNew.Amount = null;
            }
        });
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.companyTaxContributionNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.companyTaxContributionNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.companyTaxContributionNew = {
                        IsFixed: $scope.companyTaxContributionNew.IsFixed
                    };
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
}