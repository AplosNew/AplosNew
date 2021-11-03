'use strict';
PaymentTermController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];
function PaymentTermController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'PaymentTerm';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.paymentTerms = [];
    $scope.path = 'accounts/paymentterm/';
    $scope.getListUrl = $scope.path + 'getpaymenttermlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'UserName', null);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.paymentTerms = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.paymentTerm = {
        Id: null,
        Code: null,
        BaseLineDate: 'postingdate',
        PaymentModeId: null,
        UserName: null,
        IsCustomer: null,
        IsVendor: null,
        IsEmployee: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: new Date(),
        UpdatedFromIP: null
    };

    $scope.paymentTermDetail = {
        Id: null,
        Sequence: 1,
        PaymentTermId: null,
        Percentage: null,
        NoOfDay: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.paymentTermDetails = [];
    $scope.paymentTermDetailsGrid = function () {
        $scope.paymentTermDetails = [];
        for (var i = 1; i < 4; i++) {
            var obj = angular.copy($scope.paymentTermDetail);
            obj.Sequence = i;
            $scope.paymentTermDetails.push(obj);
        }
    };
    $scope.paymentTermDetailsGrid();

    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'BaseLine Date',
            'value': 'BaseLineDate'
        }
    ];

    $scope.paymentModeList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetPaymentModeEnumCbo/'
    }).then(function successCallback(response) {
        $scope.paymentModeList = response.data;
        });

    $scope.GetPaymentTermDetailList = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/paymentterm/getlist?id=' + id
        }).then(function (response) {
            $scope.paymentTermDetailsdb = response.data;
            $scope.paymentTermDetailsGrid();
            for (var i = 0; i < $scope.paymentTermDetailsdb.length; i++) {
                $scope.paymentTermDetails[i].Id = $scope.paymentTermDetailsdb[i].Id;
                $scope.paymentTermDetails[i].PaymentTermId = $scope.paymentTermDetailsdb[i].PaymentTermId;
                $scope.paymentTermDetails[i].Percentage = $scope.paymentTermDetailsdb[i].Percentage > 0 ? $scope.paymentTermDetailsdb[i].Percentage : '';
                $scope.paymentTermDetails[i].NoOfDay = $scope.paymentTermDetailsdb[i].NoOfDay;
                $scope.paymentTermDetails[i].Description = $scope.paymentTermDetailsdb[i].Description;
                $scope.paymentTermDetails[i].Remarks = $scope.paymentTermDetailsdb[i].Remarks;
                $scope.paymentTermDetails[i].Active = $scope.paymentTermDetailsdb[i].Active;
                $scope.paymentTermDetails[i].Sequence = $scope.paymentTermDetailsdb[i].Sequence;
            }
        });
    };

    function isfieldNull(value) {
        if (value === null && value === '') {
            return true;
        }
        else {
            return false;
        }
    }

    function validation() {
        try {
            if (isfieldNull($scope.paymentTermDetails[0].Percentage && $scope.paymentTermDetails[0].NoOfDay)) {
                throw 'Please give data in 3rd row !!!!';
            }
            if (($scope.paymentTermDetails[1].Percentage === null || $scope.paymentTermDetails[1].Percentage === '') &&
                ($scope.paymentTermDetails[1].NoOfDay === null || $scope.paymentTermDetails[1].NoOfDay === '') &&
                (($scope.paymentTermDetails[0].Percentage === null || $scope.paymentTermDetails[0].Percentage === '') &&
                    ($scope.paymentTermDetails[0].NoOfDay === null || $scope.paymentTermDetails[0].NoOfDay === ''))) {
            } else if (($scope.paymentTermDetails[1].Percentage !== null || $scope.paymentTermDetails[1].Percentage !== '') &&
                ($scope.paymentTermDetails[1].NoOfDay !== null || $scope.paymentTermDetails[1].NoOfDay !== '') &&
                ($scope.paymentTermDetails[0].Percentage === null || $scope.paymentTermDetails[0].Percentage === '') &&
                ($scope.paymentTermDetails[0].NoOfDay === null || $scope.paymentTermDetails[0].NoOfDay === '')) {
                throw 'Please give data in 1st row !!!!';
            }
            if ($scope.paymentTermDetails[1].Percentage !== null && $scope.paymentTermDetails[1].NoOfDay !== null
                && $scope.paymentTermDetails[1].NoOfDay !== '' && $scope.paymentTermDetails[0].Percentage !== null && $scope.paymentTermDetails[0].NoOfDay !== null) {
                if ($scope.paymentTermDetails[0].NoOfDay >= $scope.paymentTermDetails[1].NoOfDay) {
                    throw '2nd No Of Day must bigger than 1st No Of Day !!!!!';
                }
            }
            if ($scope.paymentTermDetails[0].Percentage !== null && $scope.paymentTermDetails[0].NoOfDay !== null && $scope.paymentTermDetails[2].NoOfDay === null) {
                throw 'Please give data in 3rd row !!!!';
            }
            if ($scope.paymentTermDetails[2].NoOfDay !== null) {
                if ($scope.paymentTermDetails[1].NoOfDay > 0) {
                    if ($scope.paymentTermDetails[1].NoOfDay < $scope.paymentTermDetails[2].NoOfDay) {
                    }
                    else {
                        throw '3rd No Of Day must bigger than 2nd No Of Day !!!!!';
                    }
                }
            }
            if ($scope.paymentTermDetails[2].NoOfDay !== null) {
                if ($scope.paymentTermDetails[0].NoOfDay > 0) {
                    if ($scope.paymentTermDetails[0].NoOfDay < $scope.paymentTermDetails[2].NoOfDay) {
                    }
                    else {
                        throw '3rd No Of Day must bigger than 1st No Of Day !!!!!';
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }

    function checkPercentage() {
        if (($scope.paymentTermDetails[1].NoOfDay === null || $scope.paymentTermDetails[1].NoOfDay === '') && ($scope.paymentTermDetails[1].Percentage === null || $scope.paymentTermDetails[1].Percentage === '')) {
        }
        else
            if (($scope.paymentTermDetails[1].NoOfDay !== null || $scope.paymentTermDetails[1].NoOfDay !== '') && ($scope.paymentTermDetails[1].Percentage === null || $scope.paymentTermDetails[1].Percentage === '')) {
                throw 'Please give data in 2nd row !!!!';
            }
        if (($scope.paymentTermDetails[1].NoOfDay == null || $scope.paymentTermDetails[1].NoOfDay === '') && ($scope.paymentTermDetails[1].Percentage === null || $scope.paymentTermDetails[1].Percentage === '')) {
        }
        else
            if (($scope.paymentTermDetails[1].NoOfDay == null || $scope.paymentTermDetails[1].NoOfDay === '') && ($scope.paymentTermDetails[1].Percentage !== null || $scope.paymentTermDetails[1].Percentage !== '')) {
                throw 'Please give data in 2nd row !!!!';
            }

        if (($scope.paymentTermDetails[0].NoOfDay === null || $scope.paymentTermDetails[0].NoOfDay === '') && ($scope.paymentTermDetails[0].Percentage === null || $scope.paymentTermDetails[0].Percentage === '')) {
        }
        else
            if (($scope.paymentTermDetails[0].NoOfDay !== null || $scope.paymentTermDetails[0].NoOfDay !== '') && ($scope.paymentTermDetails[0].Percentage === null || $scope.paymentTermDetails[0].Percentage === '')) {
                throw 'Please give data in 1st row !!!!';
            }

        if (($scope.paymentTermDetails[0].NoOfDay == null || $scope.paymentTermDetails[0].NoOfDay == '') && ($scope.paymentTermDetails[0].Percentage == null || $scope.paymentTermDetails[0].Percentage === '')) {
        }
        else
            if (($scope.paymentTermDetails[0].NoOfDay == null || $scope.paymentTermDetails[0].NoOfDay === '') && ($scope.paymentTermDetails[0].Percentage !== null || $scope.paymentTermDetails[0].Percentage !== '')) {
                throw 'Please give data in 1st row !!!!';
            }
    }

    function checkbox() {
        if ($scope.paymentTerm.IsCustomer || $scope.paymentTerm.IsVendor || $scope.paymentTerm.IsEmployee) {
        }
        else {
            throw 'Please Select Account Type !!!!';
        }
    }
    $scope.Save = function () {
        try {
            validation();
            $scope.$broadcast('show-errors-check-validity');
            checkbox();
            checkPercentage();
            if ($scope.paymentTermForm.$valid) {
                if ($scope.Action === 'Save') {
                    if ($scope.paymentTermDetails[2].NoOfDay === null) {
                        $http({
                            method: 'POST',
                            url: $scope.saveUrl,
                            data: $scope.paymentTerm,
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                            }
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.paymentTerms.push(response.data.PaymentTerm);
                                baseService.paginationAdd();
                                ClearFields();
                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        };
                    }
                    else {
                        $http({
                            method: 'POST',
                            url: $scope.saveUrl,
                            data: { 'paymentTerm': $scope.paymentTerm, 'paymentTermDetail': $scope.paymentTermDetails },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                            }
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.paymentTerms.push(response.data.PaymentTerm);
                                baseService.paginationAdd();
                                ClearFields();
                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        };
                    }
                }
                else if ($scope.Action === 'Update') {
                    if ($scope.paymentTermDetails[2].NoOfDay === null) {
                        $http({
                            method: 'POST',
                            url: $scope.updateUrl,
                            data: $scope.paymentTerm,
                            dataType: 'JSON'
                        }).then(function successCallBack(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                            }
                            else {
                                ShowResult(response.data.Message, 'success');
                                if ($scope.index > -1) {
                                    $scope.paymentTerms[$scope.index] = $scope.paymentTerm;
                                }
                                ClearFields();
                            }
                        }, function errorCallback(response) {
                            ShowResult(response.data.Message, 'failure');
                        });
                    }
                    else {
                        $http({
                            method: 'POST',
                            url: $scope.updateUrl,
                            data: { 'paymentTerm': $scope.paymentTerm, 'paymentTermDetail': $scope.paymentTermDetails },
                            dataType: 'JSON'
                        }).then(function successCallBack(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                            }
                            else {
                                ShowResult(response.data.Message, 'success');
                                if ($scope.index > -1) {
                                    $scope.paymentTerms[$scope.index] = $scope.paymentTerm;
                                }
                                ClearFields();
                            }
                        }, function errorCallback(response) {
                            ShowResult(response.data.Message, 'failure');
                        });
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'error');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.paymentTerm.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.paymentTerm.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.paymentTerms.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.paymentTerm = $scope.paymentTerms[$scope.index];
        $scope.paymentTerm.AddedDate = $filter('dateFilter')($scope.paymentTerm.AddedDate);
        $scope.paymentTerm.UpdatedDate = $filter('dateFilter')($scope.paymentTerm.UpdatedDate);
        $scope.GetPaymentTermDetailList($scope.paymentTerm.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.paymentTerm = {};
        $scope.paymentTerm.BaseLineDate = 'documentdate';
        $scope.paymentTermDetails = [];
        $scope.paymentTermDetailsGrid();
        $scope.paymentTerm.Active = true;
    }
}