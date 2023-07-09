'use strict';
balanceSheetSchedulingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function balanceSheetSchedulingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    //  #region Chart Account Setup -1
    $scope.ActionBalanceSheetScheduling = 'Save';
    $scope.indexBalanceSheetScheduling = -1;
    $scope.balanceSheetSchedulings = [];
    $scope.pathBalanceSheetScheduling = 'accounts/BalanceSheetScheduling/';
    $scope.getListUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'getchartofaccountlevel1list';
    $scope.getUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'get';
    $scope.getSeqUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'getautosequence';
    $scope.saveUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'create';
    $scope.updateUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'edit';
    $scope.deleteUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'delete/';
    baseService.init($scope.getListUrlBalanceSheetScheduling);

    $scope.getDataBalanceSheetScheduling = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.balanceSheetSchedulings = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getDataBalanceSheetScheduling();

    $scope.balanceSheetScheduling = {
        Id: 0,
        OptionNo: null,
        Type: null,
        Group: null,
        SubGroup: null,
        UserGroup: null,
        UserSubGroup: null,
        Item: null,
        ScheduleNo: null,
        ScheduleName: null,
        UserItem: null,
        UserScheduleName: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.GetSequenceBalanceSheetScheduling = function () {
        $http.get($scope.getSeqUrlBalanceSheetScheduling)
            .then(function (response) {
                $scope.balanceSheetScheduling.Sequence = response.data;
            });
    };

    $scope.CheckIdUseBalanceSheetScheduling = function (id) {
        $http.get('accounts/chartofaccountlevel1/checkiduse?id=' + id)
            .then(function (response) {
                $scope.checkIdUsedValue = response.data;
            });
    };

    $scope.GetSequenceBalanceSheetScheduling();

    $scope.GetBalanceSheetScheduling = function (id, index) {
        $scope.index = index;
        $scope.CheckIdUseBalanceSheetScheduling(id);
        $scope.balanceSheetScheduling = $scope.balanceSheetSchedulings[$scope.index];
        $scope.balanceSheetScheduling.AddedDate = $filter('dateFilter')($scope.balanceSheetScheduling.AddedDate);
        $scope.balanceSheetScheduling.UpdatedDate = $filter('dateFilter')($scope.balanceSheetScheduling.UpdatedDate);
        $scope.ActionBalanceSheetScheduling = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveBalanceSheetScheduling = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.balanceSheetSchedulingForm.$valid) {
            if ($scope.ActionBalanceSheetScheduling === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrlBalanceSheetScheduling,
                    data: $scope.balanceSheetScheduling,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.balanceSheetSchedulings.push(response.data.ChartOfAccountLevel1);
                        baseService.paginationAdd();
                        ClearFieldsBalanceSheetScheduling(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.ActionBalanceSheetScheduling === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrlBalanceSheetScheduling,
                    data: $scope.balanceSheetScheduling,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.balanceSheetSchedulings[$scope.index] = $scope.balanceSheetScheduling;
                        }
                        ClearFieldsBalanceSheetScheduling(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.DeleteBalanceSheetScheduling = function () {
        if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlBalanceSheetScheduling + $scope.balanceSheetScheduling.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.balanceSheetSchedulings.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFieldsBalanceSheetScheduling(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.ClearBalanceSheetScheduling = function () {
        ClearFieldsBalanceSheetScheduling($scope.GetSequenceBalanceSheetScheduling());
        return true;
    };

    function ClearFieldsBalanceSheetScheduling(seq) {
        $scope.Action = 'Save';
        $scope.balanceSheetScheduling = {};
        $scope.balanceSheetScheduling.Sequence = seq;
        $scope.balanceSheetScheduling.Active = true;
        $scope.checkIdUsedValue = false;
    }
    //  #endregion Chart Account Setup -1

    //  #region Chart Account Setup -2
    //$rootScope.titleLevel2s = 'Chart Of Account Level 2';
    //$scope.ActionLevel2s = 'Save';
    //$scope.index = -1;
    //$scope.chartOfAccountLevel2s = [];
    //$scope.pathLevel2s = 'accounts/chartofaccountlevel2/';
    //$scope.getListUrlLevel2s = $scope.pathLevel2s + 'getchartofaccountlevel2list';
    //$scope.getUrlLevel2s = $scope.pathLevel2s + 'get';
    //$scope.getSeqUrlLevel2s = $scope.pathLevel2s + 'getautosequence';
    //$scope.saveUrlLevel2s = $scope.pathLevel2s + 'create';
    //$scope.updateUrlLevel2s = $scope.pathLevel2s + 'edit';
    //$scope.deleteUrlLevel2s = $scope.pathLevel2s + 'delete/';
    //baseService.init($scope.getListUrlLevel2s);

    //$scope.getDataLevel2s = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.chartOfAccountLevel2s = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getDataLevel2s();

    //$scope.chartOfAccountLevel2 = {
    //    Id: null,
    //    Sequence: 0,
    //    Code: null,
    //    ShortName: null,
    //    StandardName: null,
    //    UserName: null,
    //    Description: null,
    //    Remarks: null,
    //    Active: true
    //};

    //$scope.GetSequenceLevel2s = function () {
    //    $http.get($scope.getSeqUrlLevel2s)
    //        .then(function (response) {
    //            $scope.chartOfAccountLevel2.Sequence = response.data;
    //        });
    //};

    //$scope.GetSequenceLevel2s();

    //$scope.CheckIdUseLevel2s = function (id) {
    //    $http.get('accounts/chartofaccountlevel2/checkiduse?id=' + id)
    //        .then(function (response) {
    //            $scope.checkIdUsedValue = response.data;
    //        });
    //};

    //$scope.GetLevel2s = function (id, index) {
    //    $scope.index = index;
    //    $scope.CheckIdUseLevel2s(id);
    //    $scope.chartOfAccountLevel2 = $scope.chartOfAccountLevel2s[$scope.index];
    //    $scope.ActionLevel2s = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    //$scope.SaveLevel2s = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.chartOfAccountLevel2Form.$valid) {
    //        if ($scope.ActionLevel2s == 'Save') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.saveUrlLevel2s,
    //                data: $scope.chartOfAccountLevel2,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error == true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    $scope.chartOfAccountLevel2s.push(response.data.ChartOfAccountLevel2);
    //                    baseService.paginationAdd();
    //                    ClearFieldsLevel2s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //        else if ($scope.ActionLevel2s == 'Update') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.updateUrlLevel2s,
    //                data: $scope.chartOfAccountLevel2,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error == true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    if ($scope.index > -1) {
    //                        $scope.chartOfAccountLevel2s[$scope.index] = $scope.chartOfAccountLevel2;
    //                    }
    //                    ClearFieldsLevel2s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //    }
    //};

    //$scope.DeleteLevel2s = function () {
    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel2.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrlLevel2s + $scope.chartOfAccountLevel2.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.chartOfAccountLevel2s.splice($scope.index, 1);
    //                baseService.paginationRemove();
    //                ClearFieldsLevel2s(response.data.Sequence);
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.status.Message, 'failure');
    //        });
    //    }
    //    else {
    //        ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
    //    }
    //    return true;
    //};

    //$scope.ClearLevel2s = function () {
    //    ClearFieldsLevel2s($scope.GetSequenceLevel2s());
    //    return true;
    //};

    //function ClearFieldsLevel2s(seq) {
    //    $scope.ActionLevel2s = 'Save';
    //    $scope.chartOfAccountLevel2 = {};
    //    $scope.chartOfAccountLevel2.Sequence = seq;
    //    $scope.chartOfAccountLevel2.Active = true;
    //    $scope.checkIdUsedValue = false;
    //}
    ////  #endregion Chart Account Setup -2

    ////  #region Chart Account Setup -3
    //$rootScope.titleLevel3s = 'Chart Of Account Level 3';
    //$scope.ActionLevel3s = 'Save';
    //$scope.index = -1;
    //$scope.chartOfAccountLevel3s = [];
    //$scope.pathLevel3s = 'accounts/chartofaccountlevel3/';
    //$scope.getListUrlLevel3s = $scope.pathLevel3s + 'getchartofaccountlevel3list';
    //$scope.getUrlLevel3s = $scope.pathLevel3s + 'get';
    //$scope.getSeqUrlLevel3s = $scope.pathLevel3s + 'getautosequence';
    //$scope.saveUrlLevel3s = $scope.pathLevel3s + 'create';
    //$scope.updateUrlLevel3s = $scope.pathLevel3s + 'edit';
    //$scope.deleteUrlLevel3s = $scope.pathLevel3s + 'delete/';
    //baseService.init($scope.getListUrlLevel3s);

    //$scope.getDataLevel3s = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.chartOfAccountLevel3s = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getDataLevel3s();

    //$scope.chartOfAccountLevel3 = {
    //    Id: null,
    //    Sequence: 0,
    //    Code: null,
    //    ShortName: null,
    //    StandardName: null,
    //    UserName: null,
    //    Description: null,
    //    Remarks: null,
    //    Active: true
    //};

    //$scope.GetSequenceLevel3s = function () {
    //    $http.get($scope.getSeqUrlLevel3s)
    //        .then(function (response) {
    //            $scope.chartOfAccountLevel3.Sequence = response.data;
    //        });
    //};
    //$scope.GetSequenceLevel3s();

    //$scope.CheckIdUseLevel3s = function (id) {
    //    $http.get('accounts/chartofaccountlevel3/checkiduse?id=' + id)
    //        .then(function (response) {
    //            $scope.checkIdUsedValue = response.data;
    //        });
    //};

    //$scope.GetLevel3s = function (id, index) {
    //    $scope.index = index;
    //    $scope.CheckIdUseLevel3s(id);
    //    $scope.chartOfAccountLevel3 = $scope.chartOfAccountLevel3s[$scope.index];
    //    $scope.ActionLevel3s = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    //$scope.SaveLevel3s = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.chartOfAccountLevel3Form.$valid) {
    //        if ($scope.ActionLevel3s === 'Save') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.saveUrlLevel3s,
    //                data: $scope.chartOfAccountLevel3,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error == true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    $scope.chartOfAccountLevel3s.push(response.data.ChartOfAccountLevel3);
    //                    baseService.paginationAdd();
    //                    ClearFieldsLevel3s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //        else if ($scope.ActionLevel3s === 'Update') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.updateUrlLevel3s,
    //                data: $scope.chartOfAccountLevel3,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error == true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    if ($scope.index > -1) {
    //                        $scope.chartOfAccountLevel3s[$scope.index] = $scope.chartOfAccountLevel3;
    //                    }
    //                    ClearFieldsLevel3s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //    }
    //};

    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel3.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrlLevel3s + $scope.chartOfAccountLevel3.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.chartOfAccountLevel3s.splice($scope.index, 1);
    //                baseService.paginationRemove();
    //                ClearFields(response.data.Sequence);
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.status.Message, 'failure');
    //        });
    //    }
    //    else {
    //        ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
    //    }
    //    return true;
    //};

    //$scope.ClearLevel3s = function () {
    //    ClearFieldsLevel3s($scope.GetSequenceLevel3s());
    //    return true;
    //};

    //function ClearFieldsLevel3s(seq) {
    //    $scope.ActionLevel3s = 'Save';
    //    $scope.chartOfAccountLevel3 = {};
    //    $scope.chartOfAccountLevel3.Sequence = seq;
    //    $scope.chartOfAccountLevel3.Active = true;
    //    $scope.checkIdUsedValue = false;
    //}
    ////  #endregion Chart Account Setup -3

    ////  #region Chart Account Setup -4
    //$rootScope.titleLevel4s = 'Chart Of Account Level 4';
    //$scope.ActionLevel4s = 'Save';
    //$scope.index = -1;
    //$scope.chartOfAccountLevel4s = [];
    //$scope.pathLevel4s = 'accounts/chartofaccountlevel4/';
    //$scope.getListUrlLevel4s = $scope.pathLevel4s + 'getchartofaccountlevel4list';
    //$scope.getUrlLevel4s = $scope.pathLevel4s + 'get';
    //$scope.getSeqUrlLevel4s = $scope.pathLevel4s + 'getautosequence';
    //$scope.saveUrlLevel4s = $scope.pathLevel4s + 'create';
    //$scope.updateUrlLevel4s = $scope.pathLevel4s + 'edit';
    //$scope.deleteUrlLevel4s = $scope.pathLevel4s + 'delete/';
    //baseService.init($scope.getListUrlLevel4s);

    //$scope.getDataLevel4s = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.chartOfAccountLevel4s = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getDataLevel4s();

    //$scope.chartOfAccountLevel4 = {
    //    Id: null,
    //    Sequence: 0,
    //    Code: null,
    //    ShortName: null,
    //    StandardName: null,
    //    UserName: null,
    //    Description: null,
    //    Remarks: null,
    //    Active: true
    //};

    //$scope.GetSequenceLevel4s = function () {
    //    $http.get($scope.getSeqUrlLevel4s)
    //        .then(function (response) {
    //            $scope.chartOfAccountLevel4.Sequence = response.data;
    //        });
    //};

    //$scope.GetSequenceLevel4s();

    //$scope.CheckIdUseLevel4s = function (id) {
    //    $http.get('accounts/chartofaccountlevel4/checkiduse?id=' + id)
    //        .then(function (response) {
    //            $scope.checkIdUsedValue = response.data;
    //        });
    //};

    //$scope.GetLevel4s = function (id, index) {
    //    $scope.index = index;
    //    $scope.CheckIdUseLevel4s(id);
    //    $scope.chartOfAccountLevel4 = $scope.chartOfAccountLevel4s[$scope.index];
    //    $scope.ActionLevel4s = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    //$scope.SaveLevel4s = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.chartOfAccountLevel4Form.$valid) {
    //        if ($scope.ActionLevel4s === 'Save') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.saveUrlLevel4s,
    //                data: $scope.chartOfAccountLevel4,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error == true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    $scope.chartOfAccountLevel4s.push(response.data.ChartOfAccountLevel4);
    //                    baseService.paginationAdd();
    //                    ClearFieldsLevel4s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //        else if ($scope.ActionLevel4s === 'Update') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.updateUrlLevel4s,
    //                data: $scope.chartOfAccountLevel4,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error == true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    if ($scope.index > -1) {
    //                        $scope.chartOfAccountLevel4s[$scope.index] = $scope.chartOfAccountLevel4;
    //                    }
    //                    ClearFieldsLevel4s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //    }
    //};

    //$scope.DeleteLevel4s = function () {
    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel4.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrlLevel4s + $scope.chartOfAccountLevel4.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.chartOfAccountLevel4s.splice($scope.index, 1);
    //                baseService.paginationRemove();
    //                ClearFieldsLevel4s(response.data.Sequence);
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.status.Message, 'failure');
    //        });
    //    }
    //    else {
    //        ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
    //    }
    //    return true;
    //};

    //$scope.ClearLevel4s = function () {
    //    ClearFieldsLevel4s($scope.GetSequenceLevel4s());
    //    return true;
    //};

    //function ClearFieldsLevel4s(seq) {
    //    $scope.Action = 'Save';
    //    $scope.chartOfAccountLevel4 = {};
    //    $scope.chartOfAccountLevel4.Sequence = seq;
    //    $scope.chartOfAccountLevel4.Active = true;
    //    $scope.checkIdUsedValue = false;
    //}
    ////  #endregion Chart Account Setup -4

    ////  #region Chart Account Setup -5
    //$rootScope.titleLevel5s = 'Chart Of Account Level 5';
    //$scope.ActionLevel5s = 'Save';
    //$scope.index = -1;
    //$scope.chartOfAccountLevel5s = [];
    //$scope.pathLevel5s = 'accounts/chartofaccountlevel5/';
    //$scope.getListUrlLevel5s = $scope.pathLevel5s + 'getchartofaccountlevel5list';
    //$scope.getUrlLevel5s = $scope.pathLevel5s + 'get';
    //$scope.getSeqUrlLevel5s = $scope.pathLevel5s + 'getautosequence';
    //$scope.saveUrlLevel5s = $scope.pathLevel5s + 'create';
    //$scope.updateUrlLevel5s = $scope.pathLevel5s + 'edit';
    //$scope.deleteUrlLevel5s = $scope.pathLevel5s + 'delete/';
    //baseService.init($scope.getListUrlLevel5s);

    //$scope.getDataLevel5s = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.chartOfAccountLevel5s = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getDataLevel5s();

    //$scope.chartOfAccountLevel5 = {
    //    Id: null,
    //    Sequence: 0,
    //    Code: null,
    //    ShortName: null,
    //    StandardName: null,
    //    UserName: null,
    //    Description: null,
    //    Remarks: null,
    //    Active: true
    //};

    //$scope.GetSequenceLevel5s = function () {
    //    $http.get($scope.getSeqUrlLevel5s)
    //        .then(function (response) {
    //            $scope.chartOfAccountLevel5.Sequence = response.data;
    //        });
    //};

    //$scope.GetSequenceLevel5s();

    //$scope.CheckIdUseLevel5s = function (id) {
    //    $http.get('accounts/chartofaccountlevel5/checkiduse?id=' + id)
    //        .then(function (response) {
    //            $scope.checkIdUsedValue = response.data;
    //        });
    //};

    //$scope.GetLevel5s = function (id, index) {
    //    $scope.index = index;
    //    $scope.CheckIdUseLevel5s(id);
    //    $scope.chartOfAccountLevel5 = $scope.chartOfAccountLevel5s[$scope.index];
    //    $scope.ActionLevel5s = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    //$scope.SaveLevel5s = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.chartOfAccountLevel5Form.$valid) {
    //        if ($scope.ActionLevel5s === 'Save') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.saveUrlLevel5s,
    //                data: $scope.chartOfAccountLevel5,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error == true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    $scope.chartOfAccountLevel5s.push(response.data.ChartOfAccountLevel5);
    //                    baseService.paginationAdd();
    //                    ClearFieldsLevel5s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //        else if ($scope.ActionLevel5s === 'Update') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.updateUrlLevel5s,
    //                data: $scope.chartOfAccountLevel5,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    if ($scope.index > -1) {
    //                        $scope.chartOfAccountLevel5s[$scope.index] = $scope.chartOfAccountLevel5;
    //                    }
    //                    ClearFieldsLevel5s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //    }
    //};

    //$scope.DeleteLevel5s = function () {
    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel5.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrlLevel5s + $scope.chartOfAccountLevel5.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.chartOfAccountLevel5s.splice($scope.index, 1);
    //                baseService.paginationRemove();
    //                ClearFieldsLevel5s(response.data.Sequence);
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.status.Message, 'failure');
    //        });
    //    }
    //    else {
    //        ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
    //    }
    //    return true;
    //};

    //$scope.ClearLevel5s = function () {
    //    ClearFieldsLevel5s($scope.GetSequenceLevel5s());
    //    return true;
    //};

    //function ClearFieldsLevel5s(seq) {
    //    $scope.ActionLevel5s = 'Save';
    //    $scope.chartOfAccountLevel5 = {};
    //    $scope.chartOfAccountLevel5.Sequence = seq;
    //    $scope.chartOfAccountLevel5.Active = true;
    //    $scope.checkIdUsedValue = false;
    //}
    ////  #endregion Chart Account Setup -5

    ////  #region Chart Account Setup -6
    //$rootScope.titleLevel6s = 'Chart Of Account Level 6';
    //$scope.ActionLevel6s = 'Save';
    //$scope.index = -1;
    //$scope.chartOfAccountLevel6s = [];
    //$scope.pathLevel6s = 'accounts/chartofaccountlevel6/';
    //$scope.getListUrlLevel6s = $scope.pathLevel6s + 'getchartofaccountlevel6list';
    //$scope.getUrlLevel6s = $scope.pathLevel6s + 'get';
    //$scope.getSeqUrlLevel6s = $scope.pathLevel6s + 'getautosequence';
    //$scope.saveUrlLevel6s = $scope.pathLevel6s + 'create';
    //$scope.updateUrlLevel6s = $scope.pathLevel6s + 'edit';
    //$scope.deleteUrlLevel6s = $scope.pathLevel6s + 'delete/';
    //baseService.init($scope.getListUrlLevel6s);
    //$scope.getDataLevel6s = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.chartOfAccountLevel6s = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getDataLevel6s();

    //$scope.chartOfAccountLevel6 = {
    //    Id: null,
    //    Sequence: 0,
    //    Code: null,
    //    ShortName: null,
    //    StandardName: null,
    //    UserName: null,
    //    Description: null,
    //    Remarks: null,
    //    Active: true
    //};

    //$scope.GetSequenceLevel6s = function () {
    //    $http.get($scope.getSeqUrlLevel6s)
    //        .then(function (response) {
    //            $scope.chartOfAccountLevel6.Sequence = response.data;
    //        });
    //};
    //$scope.GetSequenceLevel6s();

    //$scope.CheckIdUseLevel6s = function (id) {
    //    $http.get('accounts/chartofaccountlevel6/checkiduse?id=' + id)
    //        .then(function (response) {
    //            $scope.checkIdUsedValue = response.data;
    //        });
    //};

    //$scope.GetLevel6s = function (id, index) {
    //    $scope.index = index;
    //    $scope.CheckIdUseLevel6s(id);
    //    $scope.chartOfAccountLevel6 = $scope.chartOfAccountLevel6s[$scope.index];
    //    $scope.ActionLevel6s = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    //$scope.SaveLevel6s = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.chartOfAccountLevel6Form.$valid) {
    //        if ($scope.ActionLevel6s === 'Save') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.saveUrlLevel6s,
    //                data: $scope.chartOfAccountLevel6,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error == true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    $scope.chartOfAccountLevel6s.push(response.data.ChartOfAccountLevel6);
    //                    baseService.paginationAdd();
    //                    ClearFieldsLevel6s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //        else if ($scope.ActionLevel6s === 'Update') {
    //            $http({
    //                method: 'POST',
    //                url: $scope.updateUrlLevel6s,
    //                data: $scope.chartOfAccountLevel6,
    //                dataType: 'JSON'
    //            }).then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    if ($scope.index > -1) {
    //                        $scope.chartOfAccountLevel6s[$scope.index] = $scope.chartOfAccountLevel6;
    //                    }
    //                    ClearFieldsLevel6s(response.data.Sequence);
    //                }
    //            }, function errorCallback(response) {
    //                ShowResult(response.status.Message, 'failure');
    //            });
    //            return true;
    //        }
    //    }
    //};

    //$scope.DeleteLevel6s = function () {
    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel6.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrlLevel6s + $scope.chartOfAccountLevel6.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.chartOfAccountLevel6s.splice($scope.index, 1);
    //                baseService.paginationRemove();
    //                ClearFieldsLevel6s(response.data.Sequence);
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.status.Message, 'failure');
    //        });
    //    }
    //    else {
    //        ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
    //    }
    //    return true;
    //};

    //$scope.ClearLevel6s = function () {
    //    ClearFieldsLevel6s($scope.GetSequenceLevel6s());
    //    return true;
    //};

    //function ClearFieldsLevel6s(seq) {
    //    $scope.Action = 'Save';
    //    $scope.chartOfAccountLevel6 = {};
    //    $scope.chartOfAccountLevel6.Sequence = seq;
    //    $scope.chartOfAccountLevel6.Active = true;
    //    $scope.checkIdUsedValue = false;
    //}
    //  #endregion Chart Account Setup -6

    $scope.message_Detailconfirmation = null;
    $scope.RemoveBalanceSheetScheduling = function () {

        if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpLevel1')).modal('show');
    }

    //$scope.RemoveLevel2s = function () {

    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel2.Id))
    //        $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
    //    angular.element(document.querySelector('#confirmDetailPopUpLevel2')).modal('show');
    //}

    //$scope.RemoveLevel3s = function () {

    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel3.Id))
    //        $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
    //    angular.element(document.querySelector('#confirmDetailPopUpLevel3')).modal('show');
    //}

    //$scope.RemoveLevel4s = function () {

    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel4.Id))
    //        $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
    //    angular.element(document.querySelector('#confirmDetailPopUpLevel4')).modal('show');
    //}

    //$scope.RemoveLevel5s = function () {

    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel5.Id))
    //        $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
    //    angular.element(document.querySelector('#confirmDetailPopUpLevel5')).modal('show');
    //}

    //$scope.RemoveLevel6s = function () {

    //    if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel6.Id))
    //        $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
    //    angular.element(document.querySelector('#confirmDetailPopUpLevel6')).modal('show');
    //}
}