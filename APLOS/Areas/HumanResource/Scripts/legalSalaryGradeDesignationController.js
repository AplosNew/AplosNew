'use strict';
LegalSalaryGradeDesignationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function LegalSalaryGradeDesignationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Legal Salary Grade Designation";
    $scope.legalSalaryGrades = [];
    $scope.path = 'HumanResource/legalsalarygradedesignation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.legalSalaryGrade = {
        CompanyId: null,
        PlantId: null,
        LegalSalaryGradeId: null
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
    }

    $scope.legalSalaryGradeList = [];
    $scope.getLegalSalaryGradeList = function () {
        cboService.getCboLegalSalaryGrade($scope.legalSalaryGradeNew.PlantId, function (result) {
            $scope.legalSalaryGradeList = result;
        });
    };

    //$scope.legalSalaryGradeList = [];
    //$http.get('HumanResource/LegalSalaryGrade/getcbo')
    //    .then(function (response) {
    //        $scope.legalSalaryGradeList = response.data;
    //    });

    $scope.getData = function () {
        $scope.legalSalaryGradeHeadList = [];

        if (baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.PlantId)) {
            $scope.legalSalaryGradeNew.LegalSalaryGradeId = null;
            return ShowResult('Please before select plant');
        }
        $http({
            method: 'GET',
            url: $scope.getListUrl + '?plantId=' + $scope.legalSalaryGradeNew.PlantId + '&legalSalaryGradeId=' + $scope.legalSalaryGradeNew.LegalSalaryGradeId,
            contentType: "application/json; charset=utf-8",
            dataType: 'JSON'
        }).then(function (response) {
            $scope.legalSalaryGradeHeadList = response.data;
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.legalSalaryGradeNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: $scope.legalSalaryGradeHeadList,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.PlantId) && !baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.LegalSalaryGradeId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + '?plantId=' + $scope.legalSalaryGradeNew.PlantId + '&legalSalaryGradeId=' + $scope.legalSalaryGradeNew.LegalSalaryGradeId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields(seq) {
        $scope.legalSalaryGrade = {};
        $scope.legalSalaryGradeNew = {};
        $scope.legalSalaryGradeHeadList = [];
        $scope.popUpList = [];
        $scope.valueData = [];
    }

    $scope.searchPopUpByList = [
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
            'value': 'Standard Name'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.popUpList = [];
    $scope.valueData = [];
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpUrl = 'HumanResource/LegalSalaryGradeDesignation/GetLegalDesignationGroupWithoutExistingId?plantId=' + $scope.legalSalaryGradeNew.PlantId;
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    //if (baseService.arrayLength($scope.legalSalaryGradeHeadList)>0) {
                    //    for (var i = 0; i < $scope.legalSalaryGradeHeadList.length; i++) {
                    //        for (var j = 0; j < $scope.popUpDataList.length; j++) {
                    //            if ($scope.popUpDataList[j].LegalSalaryGradeId === $scope.legalSalaryGradeHeadList[i].LegalSalaryGradeId) {
                    //                $scope.popUpDataList[j].Flag = true;
                    //            }
                    //        }
                    //    }
                    //}
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    }
    

    $scope.selectByButton = function () {
        if (baseService.arrayLength($scope.popUpDataList) > 0) {
            angular.forEach($scope.popUpDataList, function (a) {
                if (checkExist($scope.legalSalaryGradeHeadList, a.LegalDesignationId) === false) {
                    if (a.Flag) {
                        $scope.legalSalaryGradeHeadList.push({
                            Id: a.Id
                            , PlantId: $scope.legalSalaryGradeNew.PlantId
                            , LegalSalaryGradeId: $scope.legalSalaryGradeNew.LegalSalaryGradeId
                            , LegalDesignationId: a.LegalDesignationId
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                            , Active: a.Active
                            , IsGradeSpecific: a.IsGradeSpecific
                        });
                    }
                }
            });
        }
        
        CloseModalShowResult('popUpId')
        $scope.closePopUp();
    }

    function checkExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].LegalDesignationId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUpId')).modal('hide');
    }
    $scope.removeRowModal = function (data, index) {
        $scope.rowIndex = index;
        $scope.tempEmpOb = data;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.confirmationMessage = 'Are you sure want to parmenently delete <b> [ ' + data.UserName + ']</b>';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    }
    $scope.removeFromHeadList = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id) === true) {
            $scope.legalSalaryGradeHeadList.splice($scope.rowIndex, 1);
            $scope.rowIndex = -1;
            $scope.confirmationMessage = '';
            $scope.tempEmpOb.Id = null;
        } else {
            $scope.removeLegalSalaryGradeFromDb($scope.tempEmpOb.Id, $scope.rowIndex);
        }
        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');

    };
    $scope.removeLegalSalaryGradeFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: 'HumanResource/LegalSalaryGradeDesignation/DeleteLegalSalaryGrade',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.legalSalaryGradeHeadList.splice($scope.rowIndex, 1);
                    $scope.rowIndex = -1;
                    $scope.tempEmpOb.Id = null;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    //**********************************************Designation**************************************************************//
}