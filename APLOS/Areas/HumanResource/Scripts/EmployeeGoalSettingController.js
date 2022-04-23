'use strict';
EmployeeGoalSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeGoalSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Goal Setting';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/EmployeeGoalSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateEGSParent';
    $scope.saveChildUrl = $scope.path + 'CreateEGChild';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteChildUrl = $scope.path + 'deleteChild/';
    baseService.init($scope.getListUrl);

    $scope.checked = function (e) {
        if (e.checked != true) {
            document.querySelector('.glyphicon').classList.add('glyphicon-ok');
        }

    }

    //Getting the MasterData
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEGList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            //ClearFields(response.data);
            
        });
    }
    $scope.getData();

    // All Lists
    $scope.EGSChildList = [];
    $scope.PerformanceYearList = [];
    $scope.EmployeeList = [];
    $scope.PerformanceGroupList = [];
    $scope.SelEmployeeInfoList = [];

    // All declared variables with null
    $scope.SelectPerformanceYearId = null;
    $scope.SelectPMSId = null;
    $scope.SelectedEmployeeId = null;
    $scope.EmployeeId = null;
    $scope.perfYear = null;
    $scope.Employee = null;

    // ALL POP UPs
    // POP OPEN
    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    // Open Popup for Performance group
    $scope.SaveEmployeeGoalSettingChild = function () {
        angular.element(document.querySelector('#PerfGrPop')).modal('show');
    }

    // POP OPEN
    $scope.selectEmployeeGoal = function () {

        angular.element(document.querySelector('#EmployeeGoalPop')).modal('show');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeeGoalPop')).modal('hide');
    }


    // ALL GET FUNCTIONS
    
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

    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmployee',

        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        })
    }
    $scope.getEmployee();
 
    $scope.getPMSMaster = function () {
        
        $http({
            method: "POST",
            url: $scope.path + 'getPMSMaster',
            data: { "SystemId": $scope.SelectedEmployeeId},
            dataType: 'JSON',
        }).then(function success(res) {
            $scope.PerformanceGroupList = res.data;
            $scope.SelectPMSId = $scope.PerformanceGroupList[0].PMSId
            
        })
    }
   
   
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
   

    // SAVE FUNCTIONS

    $scope.ModelTemp = {
        SystemId: null,       
        PerformanceYearId: null,
        ConfirmationStatus: false,
        isApproved: false,
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ChildMasterID = null;
    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
       
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        if (baseService.isUndefinedOrNull($scope.SelectedEmployeeId)) {
            throw 'Employee is Required.';
            ShowResult('Employee is Required.', 'failure');
        }

        else {

            document.getElementById("PerformanceGroupList").style.cssText = "display:block";
            $scope.getPMSMaster();
        }
        
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
        $scope.ModelTempChild = {
            Id: null,
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
        
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNewChild = Object.assign({}, $scope.ModelTempChild);
       
        //$scope.SelectedEmployeeId.isselected = false;
    }

    
    
    //=================================================================================================================//

                                        /*
                                        ----EMPLOYEE GOAL SETTING CHILD-----
                                        */
    //Getting the MasterData
    $scope.ModelListChild = null;
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEGChild",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListChild = response.data;
            //ClearFields(response.data);

        });
    }
    $scope.getData();

    $scope.EnableDisable = function (e) {
        $scope.result = $scope.ModelNew.CostSaving;
        if ($scope.result === "Yes") {
            document.getElementById("txtValue").disabled = false;
        } else {
            document.getElementById("txtValue").disabled = true;
        }

    }

    $scope.SelectedEmpGoalId = null;
    $scope.selEmpGoal = function (e) {
        $scope.SelectedEmpGoalId = e.data.SystemId;
        angular.element(document.querySelector('#EmployeeGoalPop')).modal('hide');
    }


    $scope.ModelTempChild = {
        Id: null,
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

    $scope.GetChild = function (args) {

        $scope.ModelNewChild = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
       
    }
    
    $scope.SaveEGChild = function () {
        $http({
            method: 'POST',
            url: $scope.saveChildUrl,
            data: {
                'datas': $scope.ModelNewChild,
                'EGSetting': $scope.SelectedEmpGoalId,
                'PMSId':$scope.SelectPMSId
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

    $scope.DeleteChild = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewChild.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteChildUrl + $scope.ModelNewChild.Id,
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
    
}