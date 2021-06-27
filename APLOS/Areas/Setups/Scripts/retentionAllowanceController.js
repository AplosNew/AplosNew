'use strict';
RetentionAllowanceController.$inject = ['cboService', 'commonMessage', "$window", '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function RetentionAllowanceController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.retentionAllowanceList = [];
    $scope.retentionAllowanceSelectedList = [];
    $scope.path = 'Setups/RetentionAllowance/GetList';
    $scope.retentionAllowanceOb = {
        Id: null,
        PlantId: null,
        EffectiveDate: null,
        IsAbsentismApplicable: true
    };
    $scope.plantList = [];
    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
    });
    $scope.legalSalaryGradeCboList = [];
    cboService.getCboLegalSalaryGrade(function (result) {
        $scope.legalSalaryGradeCboList = result;
    });



    $scope.getListData = function () {
        $scope.searchByList = [
            {
                name: 'EffectiveDate',
                value: 'EffectiveDate'
            },
            {
                name: 'Absentism Applicable',
                value: 'IsAbsentismApplicable'
            }
        ];
        baseService.init('Setups/RetentionAllowance/GetList?plantId=' + $scope.retentionAllowanceOb.PlantId, null, null, null, "EffectiveDate", "EffectiveDate");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.retentionAllowanceList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.getRetentionAllowanceDetailData = function () {
        if (!baseService.isUndefinedOrNull($scope.retentionAllowanceOb.PlantId) && !baseService.isUndefinedOrNull($scope.retentionAllowanceOb.EffectiveDate)) {
            $http.get("Setups/RetentionAllowance/GetDetailList?masterId=" + $scope.retentionAllowanceOb.Id)
                .then(
                function successCallback(response) {
                    $scope.retentionAllowanceSelectedList = response.data.Rows;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        }
    };
    //

    $scope.closeGLPopUp = function () {
        angular.element(document.querySelector('#CustomerInvoiceGLPopUp')).modal('hide');
    };
    $scope.addRow = function () {
        $scope.retentionAllowanceOb.LegalSalaryGradeName = document.getElementById("legalSalaryGrade").options[document.getElementById('legalSalaryGrade').selectedIndex].text;
        var ob = Object.assign({}, $scope.retentionAllowanceOb);
        $scope.retentionAllowanceSelectedList.push(ob);
        clear();
    };
    function clear() {
        $scope.retentionAllowanceOb.Id = null;
        $scope.retentionAllowanceOb.GLGeneralInfoName = null;
        $scope.retentionAllowanceOb.GLGeneralInfoId = null;
        $scope.retentionAllowanceOb.BudgetMasterId = null;
        $scope.retentionAllowanceOb.BudgetName = null;
        $scope.retentionAllowanceOb.ActivityId = null;
        $scope.retentionAllowanceOb.ActivityName = null;
        $scope.retentionAllowanceOb.OldGLId = null;
    }
    //Deleting Rows from RetentionAllowanceList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempRetentionAllowanceOb = data;
        $scope.glMappingIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempRetentionAllowanceOb.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.LegalSalaryGradeName + ' ]';
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
    };
    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempRetentionAllowanceOb.Id) === true) {
            $scope.retentionAllowanceSelectedList.splice($scope.glMappingIndex, 1);
        } else {
            $scope.removeFromDb($scope.tempRetentionAllowanceOb.Id, $scope.glMappingIndex);
        }
        $scope.glMappingIndex = -1;
        $scope.$scope.tempRetentionAllowanceOb.Id = null;
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('hide');
    };
    $scope.removeFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: 'Setups/RetentionAllowance/Delete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.retentionAllowanceSelectedList.splice($scope.glMappingIndex, 1);
                    $scope.glMappingIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    //function validateData() {
    //    angular.forEach($scope.retentionAllowanceSelectedList, function (item) {
    //        if (duplicateCheck($scope.retentionAllowanceSelectedList) === true) {
    //            throw "Duplicate experience span <b>("+ item.ExperienceSpan+ ")</b> has found!";
    //        }
    //    });
    //}
    function duplicateCheck(list) {
        for (var i = 0; i < list.length; i++) {
            for (var x = i + 1; x < list.length; x++) {
                if (list[i].ExperienceSpan === list[x].ExperienceSpan) {
                    throw "Duplicate experience span <b>(" + list[x].ExperienceSpan + ")</b> has found!";

                }
                break;
            }
        }
        //return false;
    }
    //Save
    $scope.Save = function () {
        try {
            duplicateCheck($scope.retentionAllowanceSelectedList);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Setups/RetentionAllowance/create',
                    data: { 'model': $scope.retentionAllowanceOb, 'entities': $scope.retentionAllowanceSelectedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getRetentionAllowanceDetailData();
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //
    $scope.Get = function (data) {
        var ob = data;
        $scope.retentionAllowanceOb = Object.assign({}, ob);
        $scope.getRetentionAllowanceDetailData();
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}