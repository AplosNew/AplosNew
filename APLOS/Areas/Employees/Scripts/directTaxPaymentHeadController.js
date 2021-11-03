'use strict';
directTaxPaymentHeadController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function directTaxPaymentHeadController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Direct Tax Payment Head';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.directTaxPaymentHeads = [];
    $scope.path = 'employees/directtaxpaymenthead/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.directTaxPaymentHeads = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.directTaxPaymentHead = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.directTaxPaymentHeadNew = Object.assign({}, $scope.directTaxPaymentHead);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.directTaxPaymentHeadNew.Sequence = data;
        })
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.directTaxPaymentHead = $scope.directTaxPaymentHeads[$scope.index];
        $scope.directTaxPaymentHeadNew = Object.assign({}, $scope.directTaxPaymentHead);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.directTaxPaymentHeadNew, $scope.directTaxPaymentHead);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.directTaxPaymentHeadNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.directTaxPaymentHead,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.directTaxPaymentHeads.push(response.data.DirectTaxPaymentHead);
                        $scope.directTaxPaymentHeads = $filter('orderBy')($scope.directTaxPaymentHeads, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.directTaxPaymentHead,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.directTaxPaymentHeads[$scope.index] = $scope.directTaxPaymentHead;
                            $scope.directTaxPaymentHeads = $filter('orderBy')($scope.directTaxPaymentHeads, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.directTaxPaymentHeadNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.directTaxPaymentHeadNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.directTaxPaymentHeads.splice($scope.index, 1);
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
        $scope.Action = 'Save';
        $scope.directTaxPaymentHead = {};
        $scope.directTaxPaymentHeadNew = {};
        $scope.directTaxPaymentHeadNew.Sequence = seq;
        $scope.directTaxPaymentHeadNew.Active = true;
    }
}