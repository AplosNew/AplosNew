'use strict';
PlantSalaryHeadSequenceController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];
function PlantSalaryHeadSequenceController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "PlantSalaryHeadSequence ";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.plantSalaryHeadSequenceList = [];
    $scope.plantSalaryHeadSequenceSelectedList = [];
    $scope.salaryHeadList = [];
    $scope.plantList = [];
    $scope.path = 'Organizations/plantSalaryHeadSequence/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.plantSalaryHeadSequence = {
        Id: null,
        PlantId: null,
        SalaryHeadId: null,
        CompanyId: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.plantSalaryHeadSequenceNew = Object.assign({}, $scope.plantSalaryHeadSequence);
    $scope.earningPlantSalaryHeadSequenceSelectedList = [];
    $scope.deductionPlantSalaryHeadSequenceSelectedList = [];
    $scope.getPlantSalaryHeadSequence = function () {
        $scope.earningPlantSalaryHeadSequenceSelectedList = [];
        $scope.deductionPlantSalaryHeadSequenceSelectedList = [];
        var url = 'Organizations/PlantSalaryHeadSequence/GetPlantSalaryHeadSequence?plantId=' + $scope.plantSalaryHeadSequenceNew.PlantId;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            //$scope.plantSalaryHeadSequenceSelectedList = response.data;
            angular.forEach(response.data, function (item) {
                if (item.HeadType === 'E') {
                    $scope.earningPlantSalaryHeadSequenceSelectedList.push(item);
                } else {
                    $scope.deductionPlantSalaryHeadSequenceSelectedList.push(item);
                }
            });
        });
    }
    /***Cbo***************/
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.plantSalaryHeadSequenceNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };
    //--------------
    //******************Salary Head**************/

    $scope.GetSalaryHeadList = function (headType) {
        $scope.assignSalaryType = headType;
        var url = 'Organizations/PlantSalaryHeadSequence/GetSalaryHead';
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(result) {
            $scope.plantSalaryHeadSequenceList = result.data;
        });
        angular.element(document.querySelector('#plantSalaryHeadSequencePopUp')).modal('show');
    };
    function checkExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalaryHeadId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.salaryHeadCloseListPopUp = function () {
        var slected = false;
        var tempSalaryHead = $scope.plantSalaryHeadSequenceList;
        for (var i = 0; i < tempSalaryHead.length; i++) {
            if (tempSalaryHead[i].Flag) {
                if (tempSalaryHead[i].HeadType === 'E') {
                    if (checkExist($scope.earningPlantSalaryHeadSequenceSelectedList, tempSalaryHead[i].SalaryHeadID) === false) {
                        $scope.earningPlantSalaryHeadSequenceSelectedList.push(
                            {
                                Id: null,
                                SalaryHeadId: tempSalaryHead[i].SalaryHeadID,
                                PlantId: $scope.plantSalaryHeadSequenceNew.PlantId,
                                SalaryHead: tempSalaryHead[i].SalaryHead,
                                Description: tempSalaryHead[i].Description,
                                HeadCategory: tempSalaryHead[i].HeadCategory,
                                HeadType: tempSalaryHead[i].HeadType,
                                Sequence: null,
                                Flag: tempSalaryHead[i].Flag
                            }
                        );
                        angular.element(document.querySelector('#plantSalaryHeadSequencePopUp')).modal('hide');
                    } else {
                        return ShowResult("This salary head already exist", 'failure', 'plantSalaryHeadSequencePopUp');
                        break;
                    }
                } else {
                    if (checkExist($scope.deductionPlantSalaryHeadSequenceSelectedList, tempSalaryHead[i].SalaryHeadID) === false) {
                        $scope.deductionPlantSalaryHeadSequenceSelectedList.push(
                            {
                                Id: null,
                                SalaryHeadId: tempSalaryHead[i].SalaryHeadID,
                                PlantId: $scope.plantSalaryHeadSequenceNew.PlantId,
                                SalaryHead: tempSalaryHead[i].SalaryHead,
                                Description: tempSalaryHead[i].Description,
                                HeadCategory: tempSalaryHead[i].HeadCategory,
                                HeadType: tempSalaryHead[i].HeadType,
                                Sequence: null,
                                Flag: tempSalaryHead[i].Flag
                            }
                        );
                        angular.element(document.querySelector('#plantSalaryHeadSequencePopUp')).modal('hide');
                    } else {
                        return ShowResult("This salary head already exist", 'failure', 'plantSalaryHeadSequencePopUp');
                        break;
                    }
                }
                slected = true;
            } //flag
        };
        if (slected === false) {
            return ShowResult("Please at first select row", 'failure', 'plantSalaryHeadSequencePopUp');
        }
    }
    //-----------------
    //Deleting Rows from EarnignSalaryHeadSelectedList
    $scope.valuePassInEarningDelModal = function (data, index) {
        $scope.salaryHeadRuleId = data.SalaryHeadId;
        $scope.salaryHeadRuleIndex = index;
        if (baseService.isUndefinedOrNull($scope.salaryHeadRuleId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.SalaryHead + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.DeleteRow = function () {
        var tempData = $scope.earningPlantSalaryHeadSequenceSelectedList;
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].SalaryHeadId === $scope.salaryHeadRuleId) {
                $scope.earningPlantSalaryHeadSequenceSelectedList.splice(i, 1);
            }
        }
        $scope.salaryHeadRuleId = null;
        $scope.salaryHeadRuleIndex = -1;
        tempData = [];
    };
    //
    //Deleting Rows from DeductionSalaryHeadSelectedList
    $scope.valuePassInDeductionDelModal = function (data, index) {
        $scope.salaryHeadRuleId = data.SalaryHeadId;
        $scope.salaryHeadRuleIndex = index;
        if (baseService.isUndefinedOrNull($scope.salaryHeadRuleId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.SalaryHead + ' ]';
        angular.element(document.querySelector('#confirmgenericDeductionPopUp')).modal('show');
    };
    $scope.DeleteDeductionRow = function () {
        var tempData = $scope.deductionPlantSalaryHeadSequenceSelectedList;
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].SalaryHeadId === $scope.salaryHeadRuleId) {
                $scope.deductionPlantSalaryHeadSequenceSelectedList.splice(i, 1);
            }
        }
        $scope.salaryHeadRuleId = null;
        $scope.salaryHeadRuleIndex = -1;
        tempData = [];
    };
    //
    //*****Short**/
    var move = function (origin, destination, list) {
        var temp = $scope[list][destination];
        $scope[list][destination] = $scope[list][origin];
        $scope[list][origin] = temp;
    };
    $scope.moveUp = function (index, list) {
        move(index, index - 1, list);
    };
    $scope.moveDown = function (index, list) {
        move(index, index + 1, list);
    };
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.plantSalaryHeadSequence = $scope.plantSalaryHeadSequences[$scope.index];
        $scope.plantSalaryHeadSequenceNew = Object.assign({}, $scope.plantSalaryHeadSequence);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.hasDuplicateSeq = function (data, type) {
        for (var i = 0; i < data.length; i++) {
            if (data[i].HeadType === type) {
                for (var x = i + 1; x < data.length; x++) {
                    if (data[i].Sequence == data[x].Sequence && data[x].HeadType === type) {
                        return true;
                    }
                }//childFor
            }
        }//ParentFor
        return false;
    };
    function checkIsListExistOnErning(list) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].HeadType === 'E' && !baseService.isUndefinedOrNull(list[i].SalaryHeadId)) {
                    return true;
                    break;
                }
            }
            return false;
        } catch (e) {
            throw e;
        }
    }
    $scope.plantSalaryHeadSequenceSelectedListForSave = [];
    function combineBothList(list) {
        angular.forEach(list, function (item, key) {
            item.Sequence = key + 1;
            $scope.plantSalaryHeadSequenceSelectedListForSave.push(item);
        });
    }
    $scope.Save = function () {
        try {
            $scope.plantSalaryHeadSequenceSelectedListForSave = [];
            combineBothList($scope.earningPlantSalaryHeadSequenceSelectedList);
            combineBothList($scope.deductionPlantSalaryHeadSequenceSelectedList);
            angular.forEach($scope.plantSalaryHeadSequenceSelectedList, function (item) {
                if (baseService.isUndefinedOrNull(item.Sequence)) {
                    throw "Secquence require";
                }
            });
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.plantSalaryHeadSequenceForm.$valid) {
                angular.copy($scope.plantSalaryHeadSequenceNew, $scope.plantSalaryHeadSequence);
                if (checkIsListExistOnErning($scope.earningPlantSalaryHeadSequenceSelectedList) === false) {
                    throw "Add at least one earning head sequence";
                }
                if ($scope.Action == "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'plantSalaryHeadSequence': $scope.plantSalaryHeadSequenceSelectedListForSave },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getPlantSalaryHeadSequence();
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    // #region SetTab

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.plantSalaryHeadSequence = { PlantId: $scope.plantSalaryHeadSequence.PlantId, CompanyId: $scope.plantSalaryHeadSequence.CompanyId };
        $scope.plantSalaryHeadSequenceNew = { PlantId: $scope.plantSalaryHeadSequenceNew.PlantId, CompanyId: $scope.plantSalaryHeadSequenceNew.CompanyId };
        $scope.plantSalaryHeadSequenceNew.Id = null;
        $scope.plantSalaryHeadSequenceNew.Active = true;
        $scope.tempList = [];
    }
}