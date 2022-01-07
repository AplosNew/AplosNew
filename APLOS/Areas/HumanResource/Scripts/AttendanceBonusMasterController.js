'use strict';
AttendanceBonusMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function AttendanceBonusMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Attendance Bonus Master';
    $scope.Action = 'Save';
    $scope.path = 'HumanResource/AttendanceBonusMaster/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

  
    // The Tab Switching Code    

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
       
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };  
     
     
    //*********************  Operations Staring for the Pages  ******************************\\

    //Getting the Master Grid
    $scope.masterList = [];
    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getMaster',
        }).then(function succ( resp ){
            $scope.masterList = [];
            $scope.masterList = resp.data;
        });
    }
    $scope.getMaster();

    $scope.getMasterDetails = function (e) {
        $scope.Master = e.data;
        $http({
            method: 'POST',
            url: $scope.path + 'getChildData',
            data: {'MasterId': $scope.Master.Id}
        }).then(function success(resp){

            $scope.Action = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        })
    }


    //*********************  Operations for the Master Tab  *************************\\
    $scope.saveMaster = function () {
        $scope.$broadcast('show-errors-check-validity');
        allValidations();
        if ($scope.MasterForm2.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveMaster',
                data: {'Master' : $scope.Master}
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.Child.MasterId = response.data.Data.Id;
                    $scope.Master.Id = response.data.Data.Id;
                    $scope.getMaster();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            }
        }
    }

    
    //For Deleting of A Master
    $scope.Delete = function () {
       //

        $http({
            method: 'POST',
            url: $scope.path + 'deleteMaster',
            data: { 'id': $scope.Master.Id }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMaster();
                $scope.Clear();
                if ($rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

    // ****************************************** Main Page Operations ******************************************** \\ 

    var j = document.getElementById("tab_show");
    j.style.display = "none";
    //Showing the Childs
    function showTabs() {
        if ($scope.Header.Id != null) {
            j.style.display = "block";
        }
        else {
            j.style.display = "none";
        }
    }   

    // Double Click the Main Header Grid
    $scope.getHeaderDetails = function (e) {
        $scope.Header = e.data;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        $scope.ClearRuleMaster();
        $scope.RuleMaster.HeaderId = e.data.Id;
        $scope.Child.HeaderId = e.data.Id;
        $scope.getRulesList();
        updateChild();
        showTabs();
        
    }


    /// ******************************* Header Operations ******************************* \\\
    $scope.Header = {
        Id: null,
        ShortName:null,
        StandardName:null,
        UserName: null,
        Sequence: 0,
        Remarks: null,
        Active:false,
    };

    $scope.HeaderList = [];

    // Operations to Get the Header
    $scope.getHeader = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getHeader',
        }).then(function succ(resp) {
            $scope.HeaderList = [];
            $scope.HeaderList = resp.data;
        });

    }

    $scope.getHeader();


    //Saving The Header
    $scope.saveHeader = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.HeaderForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveHeader',
                data: { 'Header': $scope.Header }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Header = response.data.Data;
                    $scope.DayChild.HeaderId = response.data.Data.Id;
                    $scope.Child.HeaderId = response.data.Data.Id;
                    showTabs();
                    LoadLeaveDayType();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    //Getting the Header Sequence
    $scope.GetSequenceHeader = function () {
        cboService.getSequence($scope.path +'GetAutoSequenceHeader', function (data) {
            $scope.Header.Sequence = data;
        });
    };

    $scope.GetSequenceHeader();

    //Clearing the Whole Header
    $scope.clearHeader = function () {
        $scope.Header = {
            Id: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Sequence: 0,
            Remarks: null,
            Active: false,
        };
        $scope.GetSequenceHeader();
        showTabs();

    }

    $scope.RuleMaster = {
        Id: null,
        HeaderId: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Remarks: null,
        Active: false,
        LeaveValue: 0,
        AbsentValue: 0,
        LateValue: 0,
        Amount:0
    };     


    $scope.SaveRuleMaster = function () {
        $scope.$broadcast('show-errors-check-validity');
        allValidations();
        if ($scope.RuleMaster.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveRuleMaster',
                data: { 'RuleMasterData': $scope.RuleMaster }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getRulesList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    // Validations for the Day Child Values
    function allValidations() {

        allZeros();

        if (parseFloat($scope.RuleMaster.LateValue) > 1 ) {
            ShowResult("Please check Late Value. It cannot be more than 1!", 'failure');
            throw ("Error");
        }

        if (parseFloat($scope.RuleMaster.AbsentValue) > 1) {
            ShowResult("Please check Absent Value. It cannot be more than 1!", 'failure');
            throw ("Error");
        }

        if (parseFloat($scope.RuleMaster.LeaveValue) > 1) {
            ShowResult("Please check Leave Value. It cannot be more than 1!", 'failure');
            throw ("Error");
        }        
    };

    function allZeros() {
        //Print 13 to 24
        for (var i = 13; i < 25; i++) {
            if (Object.values($scope.DayChild)[i] == '') {
                var jj = Object.keys($scope.DayChild)[i];
                $scope.DayChild[jj] = 0;
            }
        }
    };

    //Clearing The Rule Master
    $scope.ClearRuleMaster = function () {
        $scope.RuleMaster = {
            Id: null,
            HeaderId: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Remarks: null,
            Active: false,
            LeaveValue: 0,
            AbsentValue: 0,
            LateValue: 0,
            Amount:0
        };
        $scope.RuleMaster.HeaderId = $scope.Header.Id;
        
    }

    $scope.RulesList = [];
    //Getting the Day Type Child Grid
    $scope.getRulesList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getRulesList",
            data: { 'Id': $scope.Header.Id},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RulesList = [];
            $scope.RulesList = response.data;
        });
    }

    //Double Click the Day Type Child Grid
    $scope.getDayTypeChildDetails = function (e) {
        $scope.DayChild = e.data;
        for (var i = 0; i < $scope.EmployeesList.length; i++)
        {
            if ($scope.EmployeesList[i].SystemId == $scope.DayChild.ResponsiblePersonId) {
                $scope.RespPerson = $scope.EmployeesList[i].EmployeeName;
            }
        }
        LoadLeaveDayType();
        $scope.DayChild.OverStayLimit = String(e.data.OverStayLimit);
        $scope.DayChild.OTApplicable = String(e.data.OTApplicable);
        $scope.DayChild.GoodWorkApplicable = String(e.data.GoodWorkApplicable);
        $scope.DayChild.SandwichStatusFlag = String(e.data.SandwichStatusFlag);
        $scope.DayChild.OTCalculation = String(e.data.OTCalculation);
        $scope.DayChild.OTMultiplier = String(e.data.OTMultiplier);
        $scope.DayChild.OTHourLimit = String(e.data.OTHourLimit);
        $scope.DayChild.ApplicableWM = String(e.data.ApplicableWM);
        $scope.DayChild.OTConfirmation = String(e.data.OTConfirmation);
        $scope.DayChild.DisplayInOutTime = String(e.data.DisplayInOutTime);
        $scope.DayChild.OTCategory = String(e.data.OTCategory);
    }

    //Delete Day Status
    $scope.DeleteDT = function () {
        console.log("Delete DT");
    }
       
   
    // ********************************************** Plant EmpType Child

    $scope.Child = {
        Id: null,
        HeaderId: null,
        PlantId: null,
        EmpTypeId: null,
    };

   
    $scope.EmpTypeList = [];

    // Getting the Plants and the Company List

    $scope.PlantList = [];
    $scope.getPlants = function () {
        $http({
            method: 'GET',
            url: 'HumanResource/RosterPattern/getPlants',
            params: { 'cmp': $scope.Company }
        }).then(function success(response) {
            $scope.PlantList = response.data;
        })
    }


    $scope.Company = null;
    $scope.CompanyList = [];
    $scope.getCompany = function () {
        $http({
            method: 'GET',
            url: 'humanresource/RosterPattern/getCompany'
        }).then(function success(response) {
            $scope.CompanyList = response.data;
        })
    }

    $scope.getCompany();



    //Filling of the Employee Type List

    $scope.fillPlantsEmps = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmpType',

        }).then(function success(response) {
            $scope.EmpTypeList = [];
            $scope.EmpTypeList = response.data;
        })

    }

    $scope.fillPlantsEmps();

    // Refreshing The Child Table
    $scope.childDataList = [];
    function updateChild() {
        $http({
            method: 'POST',
            url: $scope.path + 'getChildData',
            data: { 'MasterId': $scope.Header.Id }
        }).then(function success(resp) {
            $scope.childDataList = [];
            $scope.childDataList = resp.data;
        });
    }


    //Deleting the Child Table
    $scope.DeleteChildData = [];
    $scope.confirmModal = function (data) {
        $scope.DeleteChildData = [];
        $scope.DeleteChildData = data;
        angular.element(document.querySelector('#confirmPOPUPD')).modal('show');
    }


    $scope.DeleteChild = function () {

        var obj = $scope.DeleteChildData;
        if (!baseService.isUndefinedOrNull(obj.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteChild',
                data: { 'id': obj.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    updateChild();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };



    //Save The Child Data

    $scope.saveChild = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ChildForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveChild',
                data: { 'Child': $scope.Child }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    console.log(response.data.Data);
                    updateChild();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

 }