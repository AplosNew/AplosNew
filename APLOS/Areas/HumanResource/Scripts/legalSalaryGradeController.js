'use strict';
LegalSalaryGradeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window','cboService'];
function LegalSalaryGradeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "LegalSalaryGrade";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.legalSalaryGrades = [];
    $scope.path = 'HumanResource/legalsalarygrade/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        $rootScope.parameters.offset = 0;
        $scope.legalSalaryGrades = [];
        $rootScope.parameters.plantId = $scope.legalSalaryGradeNew.PlantId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.legalSalaryGrades = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
   // $scope.getData();

    $scope.legalSalaryGrade = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        CurrencyRuleMasterId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.legalSalaryGradeNew = Object.assign({}, $scope.legalSalaryGrade);

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.legalSalaryGradeNew.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };


    $scope.GetSequence = function () {
        $http.get('HumanResource/legalsalarygrade/getautosequence?plantId=' + $scope.legalSalaryGradeNew.PlantId)
            .then(function (response) {
                $scope.legalSalaryGradeNew.Sequence = response.data;
            });
    };
   

    $scope.Get = function (index) {
        $rootScope.parameters.CompanyId = $scope.legalSalaryGradeNew.CompanyId;
        $scope.index = index;
        angular.copy($scope.legalSalaryGrades[$scope.index], $scope.legalSalaryGrade);
        angular.copy($scope.legalSalaryGrade, $scope.legalSalaryGradeNew);
        GetLegalSalaryHead();
        $scope.legalSalaryGradeNew.CompanyId = $rootScope.parameters.CompanyId;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.legalSalaryGradeNewForm.$valid) {
            reDirectToRequiredTab();
            if (baseService.arrayLength($scope.legalSalaryGradeHeadList) === 0)
                ShowResult('Please insert legal salary grade head');
            $scope.legalSalaryGrade = Object.assign({}, $scope.legalSalaryGradeNew);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        entity: $scope.legalSalaryGrade,
                        legalSalaryGradeHead: $scope.legalSalaryGradeHeadList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.legalSalaryGrades.push(response.data.LegalSalaryGrade);
                        $scope.legalSalaryGrades = $filter('orderBy')($scope.legalSalaryGrades, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        entity: $scope.legalSalaryGrade,
                        legalSalaryGradeHead: $scope.legalSalaryGradeHeadList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            angular.copy($scope.legalSalaryGrade, $scope.legalSalaryGrades[$scope.index]);
                            $scope.legalSalaryGrades = $filter('orderBy')($scope.legalSalaryGrades, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.legalSalaryGradeNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.legalSalaryGrades.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.legalSalaryGrade = {};
        $scope.legalSalaryGradeNew = { CompanyId: $scope.legalSalaryGradeNew.CompanyId, PlantId: $scope.legalSalaryGradeNew.PlantId};
        $scope.legalSalaryGradeNew.Sequence = seq;
        $scope.legalSalaryGradeNew.Active = true;
        $scope.legalSalaryGradeHeadList = [];
        $scope.popUpList = [];
        $scope.valueData = [];
    }

    //**********************************************Child**************************************************************//
    $scope.legalSalaryGradeHeadList = [];
    $scope.currencyRuleList = [];
    $scope.getCurrencyRuleList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getcurrencyrulecbo?companyGroupId=' + $window.companyGroupId + '&plantId=' + $scope.legalSalaryGradeNew.PlantId,
            contentType: "application/json; charset=utf-8",
            dataType: 'JSON'
        }).then(function (response) {
            $scope.currencyRuleList = response.data;
        });
    }

    function GetLegalSalaryHead() {
        $http({
            method: 'GET',
            url: $scope.path + 'legalsalarygradeheadlist?legalSalaryGradeId=' + $scope.legalSalaryGradeNew.Id,
            contentType: "application/json; charset=utf-8",
            dataType: 'JSON'
        }).then(function (response) {
            $scope.legalSalaryGradeHeadList = response.data;
        });
    }
    $scope.searchPopUpByList = [
        {
            'name': 'Salary Head',
            'value': 'SalaryHead'
        }
    ];
    $scope.popUpList = [];
    $scope.valueData = [];
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'SalaryHead',
        searchBy: "SalaryHead",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        if (baseService.isUndefinedOrNull($scope.legalSalaryGradeNew.CurrencyRuleMasterId))
            return ShowResult('Please select currency rule');
        $scope.popUpUrl = $scope.path + 'SalaryHeadList?companyGroupId=' + $window.companyGroupId + '&currencyRuleId=' + $scope.legalSalaryGradeNew.CurrencyRuleMasterId
            + '&salaryHeadIds=' + baseService.getColumnValueList($scope.legalSalaryGradeHeadList, 'SalaryHeadId');
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    for (var i = 0; i < baseService.arrayLength($scope.popUpDataList); i++) {
                        $scope.popUpDataList[i].Flag = baseService.valueCheckInList($scope.valueData, 'SalaryHeadId', $scope.popUpDataList[i].SalaryHeadId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };
    $scope.selectSalaryHead = function (data, event) {
        if (event.currentTarget.checked) {
            if (!baseService.valueCheckInList($scope.valueData, 'SalaryHeadId', data.SalaryHeadId))
                $scope.valueData.push(data);
        }
        else {
            if (baseService.valueCheckInList($scope.valueData, 'SalaryHeadId', data.SalaryHeadId)) {
                for (var i = 0; i < baseService.arrayLength($scope.valueData); i++) {
                    if (data.SalaryHeadId === $scope.valueData[i].SalaryHeadId)
                        $scope.valueData.splice(i, 1);
                }
            }
        }
    };
    $scope.selectByButton = function () {
        if (baseService.arrayLength($scope.valueData) < 1)
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        salaryHeadAdd($scope.valueData, $scope.legalSalaryGradeHeadList)
        CloseModalShowResult('popUpId');
        $scope.closePopUp();
    };
    function salaryHeadAdd(valueList, mainList) {
        for (var i = 0; i < baseService.arrayLength(valueList); i++) {
            if (baseService.arrayLength(mainList) === 0) {
                mainList.push({
                    Id: null
                    , LegalSalaryGradeId: $scope.legalSalaryGradeNew.Id
                    , Sequence: mainList.length + 1
                    , SalaryHeadId: valueList[i].SalaryHeadId
                    , SalaryHead: valueList[i].SalaryHead
                    , HeadCategory: valueList[i].HeadCategory
                    , EntryCurrency: valueList[i].EntryCurrency
                    , DefinitionCurrency: valueList[i].DefinitionCurrency
                    , DisbusmentCurrency: valueList[i].DisbusmentCurrency
                });
            }
            else {
                for (var t = 0; t < baseService.arrayLength(mainList); t++) {
                    if (valueList[i].SalaryHeadId !== mainList[t].SalaryHeadId) {
                        mainList.push({
                            Id: null
                            , LegalSalaryGradeId: $scope.legalSalaryGradeNew.Id
                            , Sequence: mainList.length + 1
                            , SalaryHeadId: valueList[i].SalaryHeadId
                            , SalaryHead: valueList[i].SalaryHead
                            , HeadCategory: valueList[i].HeadCategory
                            , EntryCurrency: valueList[i].EntryCurrency
                            , DefinitionCurrency: valueList[i].DefinitionCurrency
                            , DisbusmentCurrency: valueList[i].DisbusmentCurrency
                        });
                        break;
                    }
                }
            }
        }
        $scope.valueData = [];
    }
    $scope.closePopUp = function () {
        $scope.popUpList = [];
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
   

    $scope.rowIndex = -1;
    $scope.valuePassInDelModal = function (index, data) {
        $scope.message_confirmation = '';
        $scope.tempEmpOb = data;
        $scope.rowIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.confirmationMessage = 'Are you sure want to delete this data....';
        else
            $scope.confirmationMessage = 'Are you sure want to parmenently delete <b> [ ' + data.SalaryHead + ']</b>';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeFromHeadList = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id) === true) {
            $scope.legalSalaryGradeHeadList.splice($scope.rowIndex, 1);
            for (var i = 0; i < baseService.arrayLength($scope.legalSalaryGradeHeadList); i++) {
                $scope.legalSalaryGradeHeadList[i].Sequence = i + 1;
            }
            $scope.rowIndex = -1;
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
                url: 'HumanResource/LegalSalaryGrade/LegalSalaryGradeHeadDelete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.legalSalaryGradeHeadList.splice($scope.rowIndex, 1);
                    for (var i = 0; i < baseService.arrayLength($scope.legalSalaryGradeHeadList); i++) {
                        $scope.legalSalaryGradeHeadList[i].Sequence = i + 1;
                    }
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
    //**********************************************Child**************************************************************//

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    function reDirectToRequiredTab() {
        if ($scope.legalSalaryFormTab1.$invalid)
            $scope.setTab(1);
        else
            $scope.setTab(2);
    }
}