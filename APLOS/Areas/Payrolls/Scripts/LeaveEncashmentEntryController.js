'use strict';
LeaveEncashmentEntryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LeaveEncashmentEntryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Leave Encashment';
    $scope.Action = 'Save';
   
    $scope.path = 'Payrolls/LeaveEncashmentEntry/';
    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.getYearlyCalendarUrl = $scope.path + 'LoadYearlyCalendar';
    $scope.getLoadLeaveEncashmentTypesUrl = $scope.path + 'LoadLeaveEncashmentTypes';
    $scope.getLvEncashmentUrl = $scope.path + 'GetLeaveEncashmentData';
    $scope.getLvEncashmentListUrl = $scope.path + 'GetLeaveEncashmentlist';
    $scope.saveLvEncashmentUrl = $scope.path + 'SaveLeaveEncashment';
    $scope.deleteLvEncashmentUrl = $scope.path + 'DeleteLeaveEncashment';



   


    $scope.CustomPara = {
        YearlyCalendarId: null,       
        EncashmentDate: new Date(),
        LeaveEncashmentType: null      
    };
    $scope.SalaryInfo = [];
    $scope.YearlyCalendar = [];
    $scope.LeaveEncashmentTypeList = [];


    $scope.LoadYearlyCalendarList = function () {
        try {

           
            $http.get($scope.getYearlyCalendarUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.YearlyCalendar = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadYearlyCalendarList();

    $scope.LoadLeaveEncashmentTypes = function () {
        try {


            $http.get($scope.getLoadLeaveEncashmentTypesUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LeaveEncashmentTypeList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadLeaveEncashmentTypes();


    $scope.EmployeeInformationList = [];
    $scope.LoadEmployeeList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.CustomPara.YearlyCalendarId)) {
                throw "Please Select year";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.EncashmentDate)) {
                throw "Please Select Encashment Date";
            }
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


    $scope.LeaveEncashmentModel = {};
    $scope.SelectEmployee = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.CustomPara.YearlyCalendarId)) {
                throw "Please Select year";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.EncashmentDate)) {
                throw "Please Select Encashment Date";
            }

            var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
            $scope.EmployeeModel = gridObj.getSelectedRecords()[0];

            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.close();

            $http.get($scope.getLvEncashmentUrl + '?EmpSystemId=' + $scope.EmployeeModel.SystemId + '&YearNo=' + $scope.CustomPara.YearlyCalendarId + '&EffectiveDate=' + $scope.CustomPara.EncashmentDate)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LeaveEncashmentModel = response.data.LeaveInfo;                 

                       
                       
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



    $scope.LeaveEncashmentList = [];
    $scope.LoadLeaveEncashmentList = function () {
        try {


            $http.get($scope.getLvEncashmentListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LeaveEncashmentList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadLeaveEncashmentList();










    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.CustomPara.YearlyCalendarId)) {
                throw "Please Select year";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.EncashmentDate)) {
                throw "Please Select Encashment Date";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.LeaveEncashmentType)) {
                throw "Please Select Leave Encashment Type";
            }
            if ($scope.LeaveEncashmentModel.Days > $scope.LeaveEncashmentModel.Balance -  $scope.LeaveEncashmentModel.AvailedEncashment) {
                throw "Please Enter valid Leave Encashment amount.";
            }



            $scope.LeaveEncashmentModel.YearlyCalendarId = $scope.CustomPara.YearlyCalendarId;
            $scope.LeaveEncashmentModel.EncashmentDate = $scope.CustomPara.EncashmentDate;
            $scope.LeaveEncashmentModel.LeaveEncashmentType = $scope.CustomPara.LeaveEncashmentType;
            $http({
                method: 'POST',
                url: $scope.saveLvEncashmentUrl,
                data: { 'leaveEncashment': $scope.LeaveEncashmentModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadLeaveEncashmentList();
                    $scope.btnSave = false;

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }




    };




    $scope.Delete = function () {
        try {

            var gridObj = $("#GridLeaveEncashmentList").data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];


            //if (baseService.isUndefinedOrNull($scope.CustomPara.YearlyCalendarId)) {
            //    throw "Please Select year";
            //}
            //if (baseService.isUndefinedOrNull($scope.CustomPara.EncashmentDate)) {
            //    throw "Please Select Encashment Date";
            //}
            if (data.Isdisburse == true)
            {
                throw "This Encashment had already been disbursed ";
            }
          
            $http({
                method: 'POST',
                url: $scope.deleteLvEncashmentUrl,
                data: { 'leaveEncashmentId': data.Id, 'EmpSystemId': data.EmpSystemId, 'EncashmentDate': data.EncashmentDate},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadLeaveEncashmentList();
                    $scope.btnSave = false;

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }




    };



    
    $scope.message_confirmation = null;
    $scope.remove = function (obj) {
        $scope.message_confirmation = 'Are you sure to Delete This leave Encashmen ?';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    };

    


    

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.FinalSettlementModel = {};
        $scope.EmployeeModel = {};
      
        $scope.CreateTempList();
    }
};