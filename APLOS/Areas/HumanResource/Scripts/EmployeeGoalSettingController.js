'use strict';
EmployeeGoalSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeGoalSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Goal Setting';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/EmployeeGoalSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateEGSParent';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    //Getting the MasterData
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEGList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
   // $scope.getData();

    // All Lists
    $scope.EGSChildList = [];

    
    // ALL GET FUNCTIONS
    
    $scope.PerformanceYearList = [];
    $scope.SelectPerformanceYearId = null;
    $scope.getPerformancePeriod = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getPerformancePeriod',
        }).then(function success(response) {
            $scope.PerformanceYearList = response.data;
            $scope.SelectPerformanceYearId = $scope.PerformanceYearList[0].Value;
        })
    }

    $scope.getPerformancePeriod();


    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmployee',

        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        })
    }

    $scope.getEmployee();

    $scope.PerformanceGroupList = [];
    $scope.getPMSMaster = function () {
        $http({
            method: "POST",
            url: $scope.path + 'getPMSMaster',
            
            dataType: 'JSON',
        }).then(function success(res) {
            $scope.PerformanceGroupList = res.data;
            //for (var i = 0; i <= $scope.PerformanceGroupList.length; i++) {
            //   // $scope.getEGSList($scope.PerformanceGroupList[i].PMSId)
            //}
            
        })
    }
    

    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    $scope.SelectedEmployeeId = null;
    $scope.SelEMployeeInfoList = [];
    $scope.EmployeeId = null;
    $scope.perfYear = null;
    $scope.selEmp = function (e) {
        $scope.SelectedEmployeeId = e.data.SystemId;
        $scope.EmployeeId = e.data.EmployeeId;
        $scope.SelEMployeeInfoList = e.data

        $scope.perfYear = document.getElementById("ddperfYear").value;
        
        if ($scope.perfYear != "?" && $scope.SelectedEmployeeId != null) {

            document.getElementById("PerformanceGroupList").style.cssText = "dispplay:block";
            $scope.getPMSMaster();
        } 
        
        
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

   
    /*
    $scope.getEGSList = function (e) {
        $http({
            method: 'POST',
            url: $scope.path + "getEGSList",
            data: {'Id':e},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EGSChildList = response.data;
        });
    }
    */
    
    // ALL GET FUNCTIONS CLOSED

    // SAVE FUNCTIONS
    $scope.ModelTemp = {
        SystemId: null,
        
        PerformanceYearId : null,
        ConfirmationStatus: true,
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ChildMasterID = null;
    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.ChildMasterID = args.data.Id;
        $scope.GetChildList();
    };

    $scope.SaveEGSParent = function () {

        $scope.$broadcast('show-errors-check-validity');
       if ($scope.ModelNewForm.$valid) {
           $http({
               method: 'POST',
               url: $scope.saveUrl,
               data: {
                   'datas': $scope.ModelNew,                   
                   "EmployeeId": $scope.EmployeeId,
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   // ClearFields(response.data.Sequence);
                    //$scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.SystemId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.SystemId,
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

    function ClearFields(seq) {
        $scope.Action = 'CreateEGSParent';
        $scope.ModelNew = {
            SystemId: null,
            PerformanceYearId: null,
            ConfirmationStatus: false,
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
       
            $scope.EmployeeList.isSelected = false;
    }

    

    // Open Popup for Performance group
    $scope.SaveEmployeeGoalSettingChild = function () {
        angular.element(document.querySelector('#PerfGrPop')).modal('show');
    }
    // SAVE FUNCTIONS CLOSED

    

    $scope.EnableDisable = function () {        
        $scope.result = $scope.ModelNew.CostSaving;
        if ($scope.result == 'yes') {
            $scope.ModelNew.Value.disabled = false;
        }
        else {
            $scope.ModelNew.Value.disabled = true;
        }
    }
    

    
}