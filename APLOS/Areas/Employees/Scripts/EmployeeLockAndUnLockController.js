'use strict';
EmployeeLockAndUnLockController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function EmployeeLockAndUnLockController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,$window) {
    $rootScope.title = 'Employee Lock And UnLock';
    $scope.Action = 'Save';      
    $scope.path = 'employees/employeeinformation/';
    $scope.getListUrl = $scope.path + 'getemployeelist';
    $scope.saveLockUrl = $scope.path + 'CreateLockData';



    $scope.employees = [];
    $scope.customPara = {
        lockDate: null      
    };

   
  
   
   
    // #region Tab




    $scope.tab = 1;
    $scope.setTab1 = function (newTab) {
        $scope.tab = newTab;
      

    };
    $scope.isSet1 = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTab2 = function (newTab) {
        $scope.tab = newTab;
   

    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab === tabNum;
    };



    // #endregion

    // #endregion

    $scope.messageText = "";

    $scope.SaveLockData = function () {

        $.ajax({
            type: "POST",
            url: $scope.saveLockUrl,
            data:
            {               
                'lockDate': $scope.customPara.lockDate
            },
            dataType: "json",
            success: function (data) {
                $scope.ShowResultCustom(data.Message, "success");
             
            }

        });

    };

    $scope.SaveData = function () {

        $.ajax({
            type: "POST",
            url: $scope.OTConfirmationSaveUrl,
            data:
            {
                'employeeOTInformation': $scope.employees,
                'ProcDate': $scope.customPara.procdate
            },
            dataType: "json",
            success: function (data) {
                $scope.ShowResultCustom(data.Message, "success");
               
            }

        });

    };




    
}
