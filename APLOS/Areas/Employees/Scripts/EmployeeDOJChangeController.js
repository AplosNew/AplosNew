'use strict';
EmployeeDOJChangeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeDOJChangeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee DOJ Change';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'employees/EmployeeDOJChange/';  
    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.saveUrl = $scope.path + 'SaveDOJ';
 

    $scope.DOJpastDays = 0;
    $scope.GetPlantWiseHRMSSetting = function () {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/GetPlantWiseHRMSSetting'
        }).then(function successCallback(response) {

            if (response.data[0].DOCBaseON === "Month") {
                $scope.showMonthInput = true;
                $scope.showDayInput = false;
                $scope.EmployeeModel.DOCIsMonth = true;
                $scope.EmployeeModel.DOCIsDay = false;
                $scope.EmployeeModel.DOCMonth = response.data[0].DOCCount;

                $scope.showEmpMonthInput = true;
                $scope.showEmpDayInput = false;
                $scope.EmployeeModel.DOCIsMonth = true;
                $scope.EmployeeModel.DOCIsDay = false;
                $scope.EmployeeModel.DOCMonth = response.data[0].DOCCount;

            }
            else {
                $scope.showMonthInput = false;
                $scope.showDayInput = true;
                $scope.EmployeeModel.DOCIsDay = true;
                $scope.EmployeeModel.DOCIsMonth = false;
                $scope.EmployeeModel.DOCDay = response.data[0].DOCCount;

            }
           
        })
    };


    $scope.SetDoc = function () {
        if ($scope.EmployeeModel.DOCIsMonth) {
            var dt = new Date($scope.NewDOJ);
            $scope.DOC = new Date(dt.setMonth(dt.getMonth() + $scope.EmployeeModel.DOCMonth));
            $scope.EmployeeModel.DOC = $filter('dateFiltering')(new Date($scope.DOC), 'dd-MM-yyyy');
        }
        if ($scope.EmployeeModel.DOCIsDay) {
            var dt = new Date($scope.NewDOJ);
            $scope.DOC = new Date(dt.setDate(dt.getDate() + $scope.EmployeeModel.DOCDay));
            $scope.EmployeeModel.DOC = $filter('dateFiltering')(new Date($scope.DOC), 'dd-MM-yyyy');
        }
    }

    $scope.NewDOJ = null;
    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.NewDOJ)) {
                throw "Please Enter New DOJ";
            }


            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'EmpId': $scope.EmployeeModel.SystemId, 'NewDOJ': $scope.NewDOJ,'DOC': $scope.EmployeeModel.DOC },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.NewDOJ = null;
                    $scope.EmployeeModel = null;
                   

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }



       
    };
    

    $scope.EmployeeInformationList = [];
    $scope.LoadEmployeeList = function () {
        try {
           
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();




            $http.get($scope.getEmployeeListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeInformationList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.EmployeeModel = {};
    $scope.SelectEmployee = function () {
        try {
           
            var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
             $scope.EmployeeModel = gridObj.getSelectedRecords()[0];        

            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.close();
            $scope.GetPlantWiseHRMSSetting();
            


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    

    
};