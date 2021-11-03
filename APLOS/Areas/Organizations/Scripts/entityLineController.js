'use strict';
entityLineController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function entityLineController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.companyLineList = [];
    $scope.path = 'Organizations/companyline/getlinelist';
    $scope.getLineMasterOnCompanyChange = function (entityId) {
        $scope.SelectedCompany = entityId;
        $http({
            method: 'GET',
            url: 'Organizations/entityline/getlist?entityId=' + entityId
        }).then(function successCallback(response) {
            $scope.companyLineList = response.data;
            if ($scope.companyLineList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    };

    $scope.getEntityMapData = function (id) {
        $scope.entityData = [];
        $scope.entitySearch = [];
        $http({
            method: 'GET',
            url: 'Organizations/entity/get?id=' + id
        }).then(function successCallback(response) {
            $scope.entityData = [];
            $scope.entityData.push(response.data);
            baseService.getDDLSearchColumn($scope.entityData, $scope.entitySearch);
        });
    };

    $scope.companyLine = {
        Id: null,
        EntityId: null,
        LineId: null,
        Active: true
    };

    $scope.companyList = [];
    cboService.getCboProductionEntityByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    //LineList for modal
    $scope.ShowLineList = function () {
        $scope.searchByList = [
            {
                'name': 'Code',
                'value': 'Code'
            },
            {
                'name': 'User Name',
                'value': 'UserName'
            }
        ];

        if ($scope.companyLine.EntityId === null) {
            return ShowResult('Please select Entity.', 'failure');
        }

        baseService.init('Organizations/line/GetLineList', null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.entityId = $scope.SelectedCompany;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.lineListWithCompanyWise = result.Rows;
                    console.log('lineListWithCompanyWise', $scope.lineListWithCompanyWise);
                    console.log('companyLineList', $scope.companyLineList);
                    angular.forEach($scope.companyLineList, function (item) {
                        for (var i = 0; i < $scope.lineListWithCompanyWise.length; i++) {
                            if ($scope.lineListWithCompanyWise[i]['LineId'] == item.LineId) {
                                $scope.lineListWithCompanyWise[i].Flag = getActive($scope.companyLineList, $scope.lineListWithCompanyWise[i].LineId);
                                $scope.lineListWithCompanyWise[i].Flag = true;

                                //$scope.lineListWithCompanyWise.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#linePopUp')).modal('show');
        $scope.getData();
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].LineId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.companyLineList, data.LineId) === false) {
                  
                    $scope.companyLineList.push(
                        {
                            Id: null,
                            EntityId: $scope.companyLine.EntityId,
                            LineId: data.LineId,
                            Code: data.Code,
                            UserName: data.UserName,
                            Flag: data.Flag,
                            Active: true
                        }
                    );
                }
            }
            else {
                for (var i = 0; i < $scope.companyLineList.length; i++) {
                    if ($scope.companyLineList[i].LineId === data.LineId) {
                        $scope.companyLineList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].LineId === Id) {
                return true;
            }
        }
        return false;
    }

    //End LineList for modal
    //Passing Data For Line List
    $scope.LineSelectdCloseListPopUp = function () {
        angular.forEach($scope.lineListWithCompanyWise, function (item) {
            if (checkExistTempList($scope.companyLineList, item.LineId) === false) {
                if (item.Flag) {
                    $scope.companyLineList.push(
                        {
                            Id: null,
                            EntityId: $scope.companyLine.EntityId,
                            LineId: item.LineId,
                            Code: item.Code,
                            UserName: item.UserName,
                            Flag: item.Flag,
                            Active: true
                        }
                    );
                }
            }
        });

        angular.element(document.querySelector('#linePopUp')).modal('hide');
        if ($scope.companyLineList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };

    //Save
    $scope.Save = function () {
        $scope.lineSelectedList = [];
        angular.forEach($scope.companyLineList, function (item) {
            if (item.Flag) {
                $scope.lineSelectedList.push(item);
            }
        });
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.Action === 'Save') {
            $http({
                method: 'POST',
                url: 'Organizations/entityline/create',
                data: { 'entityLine': $scope.companyLineList, 'entityId': $scope.companyLine.EntityId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getLineMasterOnCompanyChange($scope.companyLine.EntityId);
                }
            });
            return true;
        }
    };

    // Deleting Rows from CompanyDepartmentList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.id = data.Id;
        $scope.index = index;
        $scope.LineId = data.LineId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.UserName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteLineList = function () {
        $scope.companyLineList.splice($scope.index, 1);
        $scope.id = null;
        $scope.index = null;
        $scope.LineId = null;
        if ($scope.companyLineList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
}