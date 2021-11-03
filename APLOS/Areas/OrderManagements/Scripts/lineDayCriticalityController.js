'use strict';
LineDayCriticalityController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "cboService"];
function LineDayCriticalityController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $scope.Action = 'Save';
    $scope.rowAction = 'Add';
    $scope.index = -1;
    $scope.criticalList = [];
    $scope.lineDayCriticalitySelectedList = [];
    $scope.getListUrl = 'OrderManagements/lineDayCriticality/getList';
    $scope.GetLineDayCriticalitySavedList = function () {
        $http.get('OrderManagements/LineDayCriticality/getList')
            .then(function (response) {
                $scope.lineDayCriticalitySelectedList = [];
                var rowList = [];
                var colList = [];
                angular.forEach(response.data.Rows, function (item) {
                    if (isExistWorkDay(item.WorkDay, rowList) === false) {
                        rowList.push(item.WorkDay);
                    }
                });
                angular.forEach(response.data.Rows, function (item) {
                    var ob = {
                        CriticalId: item.CriticalId,
                        WorkDay: item.WorkDay
                    };
                    ob[item.UserName] = item.Efficiency
                    ob.UserName = item.UserName
                    colList.push(ob);
                });
                angular.forEach(rowList, function (item) {
                    var ob = {};
                    angular.forEach(colList, function (y) {
                        if (item === y.WorkDay) {
                            ob.Id = y.Id;
                            ob.WorkDay = item;
                            ob[y.UserName] = y[y.UserName];
                        }
                    });
                    $scope.lineDayCriticalitySelectedList.push(ob);
                });
                //angular.forEach(response.data.rows, function (item) {
                //    var tob = {
                //        Id: item.Id
                //    }
                //    tob.WorkDay = item.WorkDay;
                //    tob.CriticalId = item.Id;
                //    tob.Efficiency = item.Efficiency;
                //    tob[item.UserName] = item.UserName;
                //    var ob = Object.assign({}, tob);
                //    $scope.lineDayCriticalitySavedList.push(ob);
                //});
            });
    }
    $scope.GetLineDayCriticalitySavedList();
    function isExistWorkDay(parameter, list) {
        for (var i = 0; i < list.length; i++) {
            var ob = list[i];
            if (ob === parameter) {
                return true;
                break;
            }
        }
        return false;
    }
    function isExist(parameter, key, list) {
        for (var i = 0; i < list.length; i++) {
            var ob = list[i];
            if (ob[key] === parameter) {
                return true;
                break;
            }
        }
        return false;
    }
    $scope.lineDayCriticality = {
        Id: null,
        WorkDay: null,
        CriticalId: null,
        Efficiency: null
    };

    $scope.lineDayCriticalityNew = Object.assign({}, $scope.lineDayCriticality);
    $scope.GetCriticalList = function () {
        $http.get('OrderManagements/critical/getList')
            .then(function (response) {
                angular.forEach(response.data.Rows, function (item) {
                    item.Value = null;
                })
                $scope.criticalList = response.data.Rows;
            });
    }
    $scope.GetCriticalList();
    $scope.workingDayList = [];
    $scope.getWorkingDaysCbo = function () {
        $scope.workingDayList = [];
        var totalDay = 30;
        for (var i = 1; i <= totalDay; i++) {
            var ob = {
                Value: i,
                Text: i
            }
            $scope.workingDayList.push(ob);
        }
    }
    $scope.getWorkingDaysCbo();
    $scope.addCrtical = function () {
        if ($scope.rowAction === 'Add') {
            angular.forEach($scope.criticalList, function (item) {
                var tob = {
                    Id: null
                }
                tob.WorkDay = $scope.lineDayCriticalityNew.WorkDay;
                tob.CriticalId = item.Id;
                tob.Efficiency = item.Value;
                tob[item.UserName] = item.UserName;
                var ob = Object.assign({}, tob);
                // $scope.lineDayCriticalityNew[item.UserName] = item.Value;
                //$scope.lineDayCriticalitySavedList.push(ob);
            });
            var uob = Object.assign({}, $scope.lineDayCriticalityNew);
            if (isExist(uob.WorkDay, "WorkDay", $scope.lineDayCriticalitySelectedList) === false) {
                $scope.lineDayCriticalitySelectedList.push(uob);
            } else {
                return ShowResult("This Working day already exist", 'failure');

            }
            angular.forEach($scope.criticalList, function (item) {
                item.Value = null;
            })
            $scope.lineDayCriticalityNew = Object.assign({}, $scope.lineDayCriticality);
        } else {
            $scope.lineDayCriticalitySelectedList[$scope.rowIndex].WorkDay=$scope.lineDayCriticalityNew.WorkDay
            angular.forEach($scope.criticalList, function (item) {
                $scope.lineDayCriticalitySelectedList[$scope.rowIndex][item.UserName] = $scope.lineDayCriticalityNew[item.UserName];
            });
            $scope.lineDayCriticalityNew = Object.assign({}, $scope.lineDayCriticality);
            $scope.rowAction ='Add';
        }
    }
    $scope.Get = function (data, index) {
        $scope.rowIndex = index;
        $scope.lineDayCriticalityNew = Object.assign({}, data);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.lineDayCriticalitySavedList = [];
    function getLineDayForSave() {
        angular.forEach($scope.lineDayCriticalitySelectedList, function (item) {
            angular.forEach($scope.criticalList, function (x) {
                var ob = {};
                ob.WorkDay = item.WorkDay;
                ob.CriticalId = x.Id;
                ob.Efficiency = item[x.UserName];
                $scope.lineDayCriticalitySavedList.push(ob);
            });
            // if (isExist(item.WorkDay, "WorkDay", $scope.lineDayCriticalitySavedList) === false) {
            // }
        });
    }
    function getDataForSave() {
        $scope.lineDayCriticalitySavedList = [];
        angular.forEach($scope.criticalList, function (item) {
            var tob = {
                Id: $scope.lineDayCriticalityNew.Id
            }
            tob.WorkDay = $scope.lineDayCriticalityNew.WorkDay;
            tob.CriticalId = item.Id;
            tob.Efficiency = $scope.lineDayCriticalityNew[item.UserName];
            //tob[item.UserName] = item.UserName;
            var ob = Object.assign({}, tob);
            // $scope.lineDayCriticalityNew[item.UserName] = item.Value;
            $scope.lineDayCriticalitySavedList.push(ob);
        });
        //var uob = Object.assign({}, $scope.lineDayCriticalityNew);
    }
    $scope.Save = function () {
       // getLineDayForSave();
        getDataForSave();
        if ($scope.Action === "Save") {
            $http({
                method: 'POST',
                url: "OrderManagements/lineDayCriticality/create",
                data: $scope.lineDayCriticalitySavedList,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetLineDayCriticalitySavedList();
                    ClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else if ($scope.Action === "Update") {
            $http({
                method: 'POST',
                url: "OrderManagements/lineDayCriticality/create",
                data: $scope.lineDayCriticalitySavedList,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetLineDayCriticalitySavedList();
                    ClearFields();
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.lineDayCriticalityNew.WorkDay)) {
            $http({
                method: 'POST',
                url: "OrderManagements/lineDayCriticality/delete?worKday=" + $scope.lineDayCriticalityNew.WorkDay,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.GetLineDayCriticalitySavedList();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.lineDayCriticality = {};
        $scope.lineDayCriticalityNew = {};
        $scope.lineDayCriticalityNew.Active = true;
        $scope.lineDayCriticalitySavedList = [];
    }
}