'use strict';
issueRefDetailController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function issueRefDetailController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'issueRefDetail';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.issueRefs = [];
    $scope.path = 'issueTracker/issueRefDetail/';
    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, "DESC", "Id", "Id");
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    //$scope.getData = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.issueRefs = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getData();

    baseService.init("issueTracker/issueTransaction/getlist", null, null, "DESC", "IssueDate", "Id");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.issueTransactions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();


    $scope.issueRefDetail = {
        Id: null,
        EmployeeId: null,
        IssueRefId: null
    };

    $scope.issueRefNew = Object.assign({}, $scope.issueRefDetail);

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.issueRefDetail = $scope.issueRefs[$scope.index];
        $scope.issueRefNew = Object.assign({}, $scope.issueRefDetail);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    
    
    $scope.Save = function () {
        angular.copy($scope.issueRefNew, $scope.issueRefDetail);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.issueRefDetailNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        "issueRefDetail": $scope.issueRefDetail,
                        "issueRefDetailList": $scope.issueRefDetailList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.issueRefs.push(response.data.issueRefDetail);
                        $scope.issueRefs = $filter('orderBy')($scope.issueRefs, 'Sequence');
                        baseService.paginationAdd();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.issueRefDetail,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.issueRefs[$scope.index] = $scope.issueRefDetail;
                            $scope.issueRefs = $filter('orderBy')($scope.issueRefs, 'Sequence');
                        }
                        $scope.Clear();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.issueRefNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.issueRefNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.issueRefs.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.issueRefDetailList = [];
    $scope.closeEmployeePopUp = function () {
        $scope.employeeinfo = {};
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.employeeinfo.EmployeeName = employee.EmployeeName;
            $scope.employeeinfo.EmployeeId = employee.SystemId;
            $scope.issueRefDetailList.push($scope.employeeinfo)
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.issueRefDetail = {};
        $scope.issueRefNew = {};
        $scope.taskTypeNew.Active = true;
        $scope.issueRefNew.Sequence = seq;
    }

    $scope.getIssueTransaction = function () {
        $http({
            method: "get",
            url: "IssueTracker/IssueTransaction/GetCbo"
        }).then(function successCallback(response) {
            $scope.issueTransactionlist = response.data;
        });
    }
    $scope.getIssueTransaction();
}