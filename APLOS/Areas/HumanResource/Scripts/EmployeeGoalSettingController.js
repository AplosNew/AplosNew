'use strict';
EmployeeGoalSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeGoalSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Goal Setting';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/EmployeeGoalSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Create';
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

    $scope.OpenPMSPopup = function () {
        angular.element(document.querySelector('#PMSPop')).modal('show');
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
            //$scope.SelectPMS = $scope.PerformanceGroupList[1].Username
            
        })
    }
   //$scope.getPMSMaster();
    $scope.SelectPMS = null;
    $scope.SelectPerFormanceGroup = null;
    $scope.selPMS = function (e) {
        $scope.SelectPMS = e.data.Username;
        $scope.SelectPerFormanceGroup = e.data.PerFormanceGroup;
        angular.element(document.querySelector('#PMSPop')).modal('hide');
    }
   
    $scope.selEmp = function (e) {
        $scope.SelectedEmployeeId = e.data.SystemId;
        $scope.EmployeeId = e.data.EmployeeId;
        $scope.SelEmployeeInfoList = e.data;
        $scope.Employee = e.data.EmployeeName;
        $scope.perfYear = document.getElementById("ddperfYear").value;
        $scope.getPMSMaster();
        $scope.displayEGChild();
        
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    $scope.displayEGChild = function () {
      
        if (baseService.isUndefinedOrNull($scope.SelectPerformanceYearId) && baseService.isUndefinedOrNull($scope.SelectedEmployeeId)) {
            throw 'Performance Year and Employee Id is Required.';
            ShowResult('Performance Year and Employee Id is Required.', 'failure');
        }
        //if (baseService.isUndefinedOrNull($scope.SelectedEmployeeId)) {
        //    throw 'Employee is Required.';
        //    ShowResult('Employee is Required.', 'failure');
        //}

        else {
            document.getElementById("PerformanceGroupList").style.cssText = "display:block";
            document.getElementById("EGSGrid").style.cssText = "display:block";
            
        }
    }
   
    //$scope.displayEGChild();

    // SAVE FUNCTIONS

    $scope.ModelTemp = {
        SystemId: null,       
        PerformanceYearId: null,
        ConfirmationStatus: false,
        isApproved: false,
        // ------------------------------------
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

    $scope.ChildMasterID = null;
    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';       
        if (baseService.isUndefinedOrNull($scope.SelectedEmployeeId)) {
            throw 'Employee is Required.';
            ShowResult('Employee is Required.', 'failure');
        }
        else {
            document.getElementById("PerformanceGroupList").style.cssText = "display:block";
            $scope.getPMSMaster();
           
            
        }        
    };

    $scope.SelectedEmpGoalId = null;
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
       if ($scope.ModelNewForm.$valid) {
           $http({
               method: 'POST',
               url: $scope.saveUrl,
               data: {
                   'datas': $scope.ModelNew,                   
                   'SelectedEmployeeId': $scope.SelectedEmployeeId,
                   'EGSetting': $scope.SelectedEmpGoalId,
                   'PMSId': $scope.SelectPMSId,
                },
                dataType: 'JSON',
           }).then(function successCallback(response) {
              
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');                   
                    //Object.assign($scope.ModelNew, response.data.Data);
                    $scope.getData();
                    $scope.getChildData();
                    //$scope.ModelListChild($scope.ModelNew.SystemId);
                    ClearFields();

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
        $scope.Action = 'Create';
        
        $scope.Employee = null;
        $scope.SelectPerFormanceGroup = null;
        $scope.SelectedEmpGoalId = null
        $scope.ModelTemp = {
            SystemId: null,
            PerformanceYearId: null,
            ConfirmationStatus: false,
            isApproved: false,
            // ------------------------------------
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
    }

    //=================================================================================================================//

                                        /*
                                        ----EMPLOYEE GOAL SETTING CHILD-----
                                        */
    //Getting the MasterData
    $scope.ModelListChild = [];
    $scope.getChildData = function () {
        $scope.getData();
        $http({
            method: 'POST',
            url: $scope.path + "GetEGChild",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListChild = response.data;
            
        });
    }
    $scope.getChildData();

    $scope.EnableDisable = function (e) {
        $scope.result = $scope.ModelNew.CostSaving;
        if ($scope.result === "Yes") {
            document.getElementById("txtValue").disabled = false;
        } else {
            document.getElementById("txtValue").disabled = true;
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