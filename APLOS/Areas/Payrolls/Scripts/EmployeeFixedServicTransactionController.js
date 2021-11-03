'use strict';
EmployeeFixedServicTransactionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeFixedServicTransactionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Fixed Servic Transaction';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'Payrolls/EmployeeFixedServicTransaction/';
    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.GetSalaryHeadWiseAmountSettinglistUrl = $scope.path + 'GetSalaryHeadWiseAmountSettinglist';
    $scope.SalaryHeadWiseAmountTransactionUrl = $scope.path + 'GetSalaryHeadWiseAmountTransaction';
    $scope.GetSalaryHeadWiseAmountSettingDetailsUrl = $scope.path + 'GetSalaryHeadWiseAmountSettingDetails';
    $scope.saveUrl = $scope.path + 'SaveSalaryHeadWiseAmountTransaction';
    $scope.deleteUrl = $scope.path + 'DeleteSalaryHeadWiseAmountTransaction';


   



    $scope.CustomModel = {
        SalaryHeadWiseAmountSettingId: null,
        FormulaIDDescription: null
    };


    $scope.SalaryHeadWiseAmountSettinglist = [];
    $scope.GetSalaryHeadWiseAmountSettinglist = function () {
        try {
            $http.get($scope.GetSalaryHeadWiseAmountSettinglistUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.SalaryHeadWiseAmountSettinglist = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetSalaryHeadWiseAmountSettinglist();  

    $scope.SalaryHeadWiseAmountSettingDetails = {};

   


    $scope.xGetSalaryHeadWiseAmountSettingDetails = function () {
        try {
            $scope.SalaryHeadWiseAmountTransactionList = [];
            if (baseService.isUndefinedOrNull($scope.EmployeeModel.SystemId)) {
                throw "Please select employee";
            }
            if (!baseService.isUndefinedOrNull($scope.CustomModel.SalaryHeadWiseAmountSettingId)) {
                $http.get($scope.GetSalaryHeadWiseAmountSettingDetailsUrl + '?SalaryHeadWiseAmountSettingId=' + $scope.CustomModel.SalaryHeadWiseAmountSettingId)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.Message, 'failure');
                        }
                        else {
                            $scope.SalaryHeadWiseAmountSettingDetails = response.data[0];


                            var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
                            $scope.EmployeeModel = gridObj.getSelectedRecords()[0];



                            $http.get($scope.SalaryHeadWiseAmountTransactionUrl + '?EmpSystemId=' + $scope.EmployeeModel.SystemId +
                                '&SalaryHeadWiseAmountSettingId=' + $scope.CustomModel.SalaryHeadWiseAmountSettingId +
                                '&DurationType=' + $scope.SalaryHeadWiseAmountSettingDetails.DurationType)
                                .then(function successCallback(response) {
                                    if (response.data.Error === true) {
                                        ShowResult(response.data.Message, 'failure');
                                    }
                                    else {
                                        $scope.SalaryHeadWiseAmountTransactionList = response.data;




                                      

                                        $scope.btnSave = true;
                                    }
                                },

                                    function errorCallBack(response) {
                                        ShowResult(response.data.Message, 'failure');
                                    });

                        }
                    },

                        function errorCallBack(response) {
                            ShowResult(response.Message, 'failure');
                        });






                

            }




        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetSalaryHeadWiseAmountSettingDetails = function () {
        try {
            $scope.SalaryHeadWiseAmountTransactionList = [];
            $http.get($scope.SalaryHeadWiseAmountTransactionUrl + '?EmpSystemId=' + $scope.EmployeeModel.SystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.SalaryHeadWiseAmountTransactionList = response.data;






                        $scope.btnSave = true;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });




        } catch (e) {
            ShowResult(e, "failure");
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
    $scope.SalaryHeadWiseAmountTransactionList = [];
   
    $scope.SelectEmployee = function () {
        try {

            //if (baseService.isUndefinedOrNull($scope.CustomModel.SalaryHeadWiseAmountSettingId)) {
            //    throw "Please Select Allowance Component";
            //}



            var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
            $scope.EmployeeModel = gridObj.getSelectedRecords()[0];

            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.close();
            $scope.GetSalaryHeadWiseAmountSettingDetails();
            //$http.get($scope.SalaryHeadWiseAmountTransactionUrl + '?EmpSystemId=' + $scope.EmployeeModel.SystemId + '&SalaryHeadWiseAmountSettingId=' + $scope.CustomModel.SalaryHeadWiseAmountSettingId)
            //    .then(function successCallback(response) {
            //        if (response.data.Error === true) {
            //            ShowResult(response.data.Message, 'failure');
            //        }
            //        else {
            //            $scope.SalaryHeadWiseAmountTransactionList = response.data;
            //            $scope.btnSave = true;
            //        }
            //    },

            //        function errorCallBack(response) {
            //            ShowResult(response.data.Message, 'failure');
            //        });







        } catch (e) {
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.close();
            ShowResult(e, "failure");
        }
    };


    $scope.SalaryHeadWiseAmountTransactionModel = {
        EffectiveDate: null,
        EmpSystemId: null,
        EmployeeFixedServicId: null,
        Amount: null,
        Particulars: null,
        Remarks: null,
        Active:false
    };

    $scope.Save = function () {
        try {


          
            if (baseService.isUndefinedOrNull($scope.EmployeeModel.SystemId)) {
                throw "Please select employee";
            }
            if (baseService.isUndefinedOrNull($scope.SalaryHeadWiseAmountTransactionModel.EmployeeFixedServicId)) {
                throw "Please select Servic Component";
            }
            if (baseService.isUndefinedOrNull($scope.SalaryHeadWiseAmountTransactionModel.EffectiveDate)) {
                throw "Please enter Effective Date";
            }
            if (baseService.isUndefinedOrNull($scope.SalaryHeadWiseAmountTransactionModel.Amount)) {
                throw "Please enter Amount";
            }
            $scope.SalaryHeadWiseAmountTransactionModel.EmpSystemId = $scope.EmployeeModel.SystemId
            $scope.SalaryHeadWiseAmountTransactionModel.PlantId = $scope.EmployeeModel.PlantId
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    
                    'SalaryHeadWiseAmountTransactionData': $scope.SalaryHeadWiseAmountTransactionModel
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSalaryHeadWiseAmountSettingDetails();
                    $scope.SalaryHeadWiseAmountTransactionModel = {
                        EffectiveDate: null,
                        EmpSystemId: null,
                        EmployeeFixedServicId: null,
                        Amount: null,
                        Particulars: null,
                        Remarks: null,
                        Active: false
                    };
                    $scope.btnSave = false;
                    $scope.Action = 'Save';
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }




    };
    $scope.Edit = function (obj) {
        var gridObj = $("#GridSalaryHeadWiseAmountTransactionList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.SalaryHeadWiseAmountTransactionModel = data;
        $scope.Action = 'Update';
    };

    $scope.Id = null;
    $scope.message_confirmation = null;
    $scope.remove = function (obj) {
        var gridObj = $("#GridSalaryHeadWiseAmountTransactionList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.Id = data.Id;
        if (!baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure to delete This ?';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    };

    $scope.Delete = function () {

        try {
            $.ajax({
                type: "POST",
                url: $scope.deleteUrl,
                data:
                {

                    'Id': $scope.Id
                },
                dataType: "json",
                success: function (response) {
                    if (response.Error) {
                        ShowResult(response.Message, 'error');
                    } else {
                       
                        ShowResult(response.Message, 'success');
                        $scope.GetSalaryHeadWiseAmountSettingDetails();

                    }

                }

            });
        } catch (e) {
            ShowResult(e, 'error');
        }
    };

    $scope.Clear = function () {

        $scope.SalaryHeadWiseAmountTransactionModel.EffectiveDate = null; 
        $scope.SalaryHeadWiseAmountTransactionModel.EmployeeFixedServicId = null; 
        $scope.SalaryHeadWiseAmountTransactionModel.Amount = null; 
        $scope.SalaryHeadWiseAmountTransactionModel.Particulars = null; 
        $scope.SalaryHeadWiseAmountTransactionModel.Remarks = null; 
        $scope.SalaryHeadWiseAmountTransactionModel.Active = false; 
        $scope.Action = 'Save';  
    };

    
};