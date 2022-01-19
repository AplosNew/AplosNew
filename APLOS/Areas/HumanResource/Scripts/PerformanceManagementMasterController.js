'use strict';
PerformanceManagementMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PerformanceManagementMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Performance Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/PerformanceManagementMaster/';
    /*$scope.getListUrl = $scope.path + 'getlist';*/
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
  /*  baseService.init($scope.getListUrl);*/
    $scope.searchBy = null; $scope.search = null;
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

  
    $scope.ID = null;
    $scope.EmployeeList = [];
    $scope.EmployeeId = null;

    //$scope.getData = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + "GetList",
    //        data: { column: $scope.searchBy, value: $scope.search },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.ModelList = response.data;
    //        ClearFields(response.data.Sequence);
    //        $scope.GetSequence();
    //    });
    //}

    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployee",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
        });
    }

    $scope.getEmployee();
    $scope.selectEmployee = function () {
       
        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }


    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Category: null,
        SubCategory: null,
        StandardName: null,
        UserName: null,
        ShortName: null,
        Code: null,
        Active: null,
        Remarks:null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.SelEmpList = [];

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        var AllData = [];
        $http({
            method: 'POST',
            url: $scope.path + "Get",
            data: {'Id':args.data.Id},
            dataType: 'JSON'
        }).then(function successCallback(resp) {
            $scope.EmployeeIds = [];
            $scope.SelEmpList = [];
            AllData = resp.data.master;
            var child = resp.data.child;
            var ob = {};
            $scope.ModelNew = Object.assign({}, AllData[0]);
            for (var i = 0; i < child.length; i++) {
                ob[child[i].EmployeeId] = true;
                $scope.EmployeeIds.push(child[i].EmployeeId);
                
            }

            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                if ($scope.EmployeeList[i].Id in ob) {
                    $scope.EmployeeList[i].isSelected = true;
                    $scope.SelEmpList.push($scope.EmployeeList[i]);
                }
                else {
                    $scope.EmployeeList[i].isSelected = false;
                }
            }


        });

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
       $scope.$broadcast('show-errors-check-validity');

        //if (angular.isUndefinedOrNull($scope.ModelNew.EmployeeId)) {
        //    ShowResult('No EmployeeType Selected!!' , 'failure');
        //    throw ("Invalid");
        //}
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'datas': $scope.ModelNew, 'Employee' :$scope.EmployeeIds },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
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
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.EmployeeIds = [];
        $scope.SelEmpList = [];
    }

    // Addition of the Modal Operations for PMS Child
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    $scope.EmployeeIds = [];

    $scope.selectEmpDetail = function () {
        $scope.EmployeeIds = [];
        $scope.SelEmpList = [];
        for (var i = 0; i < $scope.EmployeeList.length; i++) {
            if ($scope.EmployeeList[i].isSelected == true) {
                $scope.EmployeeIds.push($scope.EmployeeList[i].Id);
                $scope.SelEmpList.push($scope.EmployeeList[i]);
            }
        }

        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }
}