'use strict';
SalaryHeadWiseAmountTransactionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SalaryHeadWiseAmountTransactionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Salary Head Wise Amount Transaction';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'Payrolls/SalaryHeadWiseAmountTransaction/';
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

   



    $scope.monthList = [
        {
            Value: '01',
            Text: 'January'
        },
        {
            Value: '02',
            Text: 'February'
        },
        {
            Value: '03',
            Text: 'March'
        },
        {
            Value: '04',
            Text: 'April'
        },
        {
            Value: '05',
            Text: 'May'
        },
        {
            Value: '06',
            Text: 'June'
        },
        {
            Value: '07',
            Text: 'July'
        },
        {
            Value: '08',
            Text: 'August'
        },
        {
            Value: '09',
            Text: 'September'
        },
        {
            Value: '10',
            Text: 'October'
        },
        {
            Value: '11',
            Text: 'November'
        },
        {
            Value: '12',
            Text: 'December'
        }
    ];
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();


    $scope.yearList = [];
    //cboService.getCboLeaveYear(function (result) {
    //    $scope.yearList = result;
    //});

    GetYearList();

    function GetYearList() {
        var FromYear = 2017
        var ToYear = parseInt(new Date().getFullYear().toString());
        while (FromYear <= ToYear) {
            $scope.yearList.push(FromYear);
            FromYear++;
        }
    }



    $scope.GetSalaryHeadWiseAmountSettingDetails = function () {
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




                                        var gridObj = $("#GridSalaryHeadWiseAmountTransactionList").data("ejGrid");
                                        gridObj.refreshContent();

                                        if ($scope.SalaryHeadWiseAmountSettingDetails.DurationType === 'DateSpecific') {

                                            gridObj.hideColumns("YearNo");
                                            gridObj.hideColumns("MonthNo");
                                            gridObj.hideColumns("FromDate");
                                            gridObj.hideColumns("ToDate");
                                            gridObj.showColumns("WorkDate");
                                            gridObj.showColumns("Amount");
                                        }
                                        if ($scope.SalaryHeadWiseAmountSettingDetails.DurationType === 'Monthly') {


                                            gridObj.hideColumns("WorkDate");
                                            gridObj.hideColumns("FromDate");
                                            gridObj.hideColumns("ToDate");
                                            gridObj.showColumns("Amount");
                                            gridObj.showColumns("YearNo");
                                            gridObj.showColumns("MonthNo");
                                        }
                                        if ($scope.SalaryHeadWiseAmountSettingDetails.DurationType === 'Recurring') {
                                            gridObj.hideColumns("WorkDate");
                                            gridObj.hideColumns("YearNo");
                                            gridObj.hideColumns("MonthNo");
                                            gridObj.showColumns("FromDate");
                                            gridObj.showColumns("ToDate");
                                            gridObj.showColumns("Amount");
                                        }

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
        WorkDate: null,
        YearNo: new Date().getFullYear().toString(),
        MonthNo: null,
        Amount: null,
        ToDate: null,
        FromDate: null,
        DurationType:null
    };

    $scope.Save = function () {
        try {


          
            if (baseService.isUndefinedOrNull($scope.EmployeeModel.SystemId)) {
                throw "Please select employee";
            }
            if (baseService.isUndefinedOrNull($scope.CustomModel.SalaryHeadWiseAmountSettingId)) {
                throw "Please select Allowance Component";
            }

            $scope.SalaryHeadWiseAmountTransactionModel.DurationType = $scope.SalaryHeadWiseAmountSettingDetails.DurationType;
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'EmpSystemId': $scope.EmployeeModel.SystemId,
                    'SalaryHeadWiseAmountSettingId': $scope.CustomModel.SalaryHeadWiseAmountSettingId,
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
                        WorkDate: null,
                        YearNo: new Date().getFullYear().toString(),
                        MonthNo: null,
                        Amount: null,
                        ToDate: null,
                        FromDate: null,
                        DurationType: null
                    };
                    $scope.btnSave = false;

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }




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



    
};