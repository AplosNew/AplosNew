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
            ClearFields(response.data);
            
        });
    }
    $scope.getData();

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
            data: { "SystemId": $scope.SelectedEmployeeId},
            dataType: 'JSON',
        }).then(function success(res) {
            $scope.PerformanceGroupList = res.data;
            //$scope.SystemId = $scope.SelectedEmployeeId
            
        })
    }
   

    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    $scope.SelectedEmployeeId = null;
    $scope.SelEmployeeInfoList = [];
    $scope.EmployeeId = null;
    $scope.perfYear = null;
    $scope.Employee = null;

    $scope.selEmp = function (e) {
        $scope.SelectedEmployeeId = e.data.SystemId;
        $scope.EmployeeId = e.data.EmployeeId;
        $scope.SelEmployeeInfoList = e.data;
        $scope.Employee = e.data.EmployeeName;

        $scope.perfYear = document.getElementById("ddperfYear").value;

        if (baseService.isUndefinedOrNull($scope.SelectPerformanceYearId)) {
            throw 'Performance Year is Required.';
            ShowResult('Performance Year is Required.', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.SelectedEmployeeId)) {
            throw 'Employee is Required.';
            ShowResult('Employee is Required.', 'failure');
        }

        else {

            document.getElementById("PerformanceGroupList").style.cssText = "display:block";
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
        //$scope.ChildMasterID = args.data.Id;
        //$scope.GetChildList();
    };

    $scope.SaveEGSParent = function () {

        $scope.$broadcast('show-errors-check-validity');
       if ($scope.ModelNewForm.$valid) {
           $http({
               method: 'POST',
               url: $scope.saveUrl,
               data: {
                   'datas': $scope.ModelNew,                   
                   "SelectedEmployeeId": $scope.SelectedEmployeeId,
                },
                dataType: 'JSON',
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
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'CreateEGSParent';
        $scope.ModelNew = {
            SystemId: null,
            PerformanceYearId: null,
            ConfirmationStatus: false,
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
       
        $scope.SelectedEmployeeId.isselected = false;
    }

    

    // Open Popup for Performance group
    $scope.SaveEmployeeGoalSettingChild = function () {
        angular.element(document.querySelector('#PerfGrPop')).modal('show');
    }
    // SAVE FUNCTIONS CLOSED

    

    $scope.EnableDisable = function (e) {        
        $scope.result = $scope.ModelNew.CostSaving;
        if ($scope.result === "Yes") {
            document.getElementById("txtValue").disabled = false;
        } else {
            document.getElementById("txtValue").disabled = true;
        }
        
    }
    
    //
    //---- EMPLOYEE GOAL SETTING CHILD

    

    $scope.ModelTempChild = {
        ID: null,
        ObjectiveName: null,
        objectiveDetail: null,
        CostSaving: null,
        Value: null,
        Attachment: null,
        AssesmentDate: null,
        ObjNameClosingDate: null,
        MaxStoryPoints: null,
        Remarks: null,
    };
    $scope.ModelNewChild = Object.assign({}, $scope.ModelTempChild);

    $scope.SaveEGChild = function () {
        $http({
            method: 'POST',
            url: $scope.path + "CreateEGChild",
            data: {
                'data': $scope.ModelNewChild,
                
            },
            dataType: 'JSON',
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
    
}