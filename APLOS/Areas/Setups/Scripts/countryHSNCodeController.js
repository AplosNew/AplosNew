'use strict';
CountryHSNCodeController.$inject = ["addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http'];
function CountryHSNCodeController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.countryHSNCodeList = [];
    $scope.hSNCodeSelectedList = [];
    $scope.getCountryHSNCodeOnCountryChange = function (countryId) {
        $http.get('Setups/countryHSNCode/GetListWithCountry?countryId=' + countryId)
            .then(
            function successCallback(response) {
                $scope.hSNCodeSelectedList = response.data.Rows;
                if ($scope.hSNCodeSelectedList.length > 0) {
                    $scope.tableShow = true;
                }
                else {
                    $scope.tableShow = false;
                }
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    $scope.countryHSNCode = {
        Id: null,
        CountryGroupId: null,
        CountryId: null,
        DepartmentId: null,
        Remarks: null,
        Active: true,
        AddedDate: new Date(),
        UpdatedBy: null,
        UpdatedDate: new Date()
    };

    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.CountryList = result;
    });

    $rootScope.tempList = [];
    $scope.hSNCodeSelectedList = [];
    $scope.hSNCodeList = [];
    $scope.searchByHSNCodeList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    ];

    $scope.hSNCodeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.hSNCodeSearchPopup = function () {
        if ($scope.countryHSNCode.CountryId == null)
            return ShowResult('Please at first select country.', 'failure');
        angular.forEach($scope.hSNCodeSelectedList, function (item) {
            $rootScope.tempList.push({
                Id: item.Id
                , HSNCodeId: item.HSNCodeId
                , Code: item.Code
                , Description: item.Description
            });
        });
        baseService.setCurrentPage('hSNCodeList');
        $scope.loadHSNCodeData = function (pageno) {
            baseService.paginationBase('Setups/HSNCode/GetHSNCodeUnSelectedList', pageno, $scope.hSNCodeListParameters)
                .then(function (result) {
                    $scope.hSNCodeList = result.Rows;
                    $scope.hSNCodeListParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.hSNCodeList); t++) {
                        $scope.hSNCodeList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'HSNCodeId', $scope.hSNCodeList[t].HSNCodeId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.loadHSNCodeData();
        angular.element(document.querySelector('#hSNCodeListPopUp')).modal('show');
    };

    $scope.hSNCodeCloseListPopUp = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (item) {
                if (!baseService.valueCheckInList($scope.hSNCodeSelectedList, 'HSNCodeId', item.HSNCodeId)) {
                    $scope.hSNCodeSelectedList.push({
                        Id: item.Id
                        , HSNCodeId: item.HSNCodeId
                        , CountryId: $scope.countryHSNCode.CountryId
                        , Code: item.Code
                        , Description: item.Description
                    });
                }
            });
        }
        else
            $scope.hSNCodeSelectedList = [];
        angular.forEach($scope.hSNCodeSelectedList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'HSNCodeId', a.HSNCodeId))
                $scope.hSNCodeSelectedList.splice(a, 1);
        });
        angular.element(document.querySelector('#hSNCodeListPopUp')).modal('hide');
    };

    $scope.pushList = function (data, event, list) {
        if (event.currentTarget.checked)
            $rootScope.tempList.push(data);
        else {
            for (var a = 0; a < arrayLength($rootScope.tempList); a++) {
                if ($rootScope.tempList[a].HSNCodeId === data.HSNCodeId)
                    return $rootScope.tempList.splice(a, 1);
            }
            for (var b = 0; b < arrayLength(list); b++) {
                if (list[b].HSNCodeId === data.HSNCodeId)
                    return list.splice(b, 1);
            }
        }
    };

    //End DepartmentList for modal

    //Save
    function hSNCodeCDeleteIdList(list) {
        $scope.hSNCodeCDeleteIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].isDelete) {
                    $scope.hSNCodeCDeleteIds.push(list[i]['Id']);
                }
            }
        }
        return JSON.stringify($scope.hSNCodeCDeleteIds);
    }
    $scope.Save = function () {
        $scope.departmentSelectedList = [];
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.Action === 'Save') {
            $http({
                method: 'POST',
                url: 'Setups/countryHSNCode/create',
                data: { 'CountryHSNCode': $scope.hSNCodeSelectedList, 'countryId': $scope.countryHSNCode.CountryId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getCountryHSNCodeOnCountryChange($scope.countryHSNCode.CountryId);
                }
            });
            return true;
        }
        return true;
    };

    $scope.valuePassInDelModal = function (index, HSNCodeId, id) {
        $scope.id = id;
        $scope.cIndex = index;
        $scope.HSNCodeId = HSNCodeId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteHSNCodeList = function () {
        for (var i = 0; i < $scope.hSNCodeSelectedList.length; i++) {
            if ($scope.hSNCodeSelectedList[i].Id == null && $scope.hSNCodeSelectedList[i].HSNCodeId === $scope.HSNCodeId) {
                $scope.hSNCodeSelectedList.splice($scope.cIndex, 1);
            }
            else if ($scope.hSNCodeSelectedList[i].Id != null && $scope.hSNCodeSelectedList[i].HSNCodeId === $scope.HSNCodeId) {
                $scope.hSNCodeSelectedList.splice($scope.cIndex, 1);
            }
        }
        $scope.id = null;
        $scope.cIndex = null;
        $scope.HSNCodeId = null;
        if ($scope.hSNCodeSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
}