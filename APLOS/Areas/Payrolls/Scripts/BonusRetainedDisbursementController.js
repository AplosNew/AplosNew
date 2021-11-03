'use strict';
BonusRetainedDisbursementController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function BonusRetainedDisbursementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Bonus Retained Disbursement';
    $scope.path = 'Payrolls/BonusRetainedDisbursement/';
    $scope.GetBonusRetainedDataUrl = $scope.path + 'GetBonusRetainedData';
    $scope.GetBonusRetainedDetailsUrl = $scope.path + 'GetBonusRetainedDetails';
    $scope.GetBonusRetainedSavedDetailsUrl = $scope.path + 'GetBonusRetainedSavedDetails';

    $scope.GetBonusRetainedDisbursementMasterDataUrl = $scope.path + 'GetBonusRetainedDisbursementMasterData';

    $scope.SaveBonusRetainedDataUrl = $scope.path + 'SaveBonusRetainedData';
    $scope.GetSevedBonusRetainedDataUrl = $scope.path + 'GetSevedBonusRetainedData';

    $scope.DeleteBonusRetainedDisbursementUrl = $scope.path + 'DeleteBonusRetainedDisbursement';




    $scope.CustomPara = {
        DisbursementDate: null,
        Description: null

    };

    $scope.AddEmployee = function () {
        try {
            $scope.BonusRetainedList = [];
            //$scope.GetBonusRetainedData();
            if (baseService.isUndefinedOrNull($scope.CustomPara.DisbursementDate)) {
                throw "Enter Disbursement Date.";
            };
            $http.get($scope.GetBonusRetainedDataUrl + '?DisbursementDate=' + $scope.CustomPara.DisbursementDate)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.BonusRetainedList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');


                    });

            var eDialog = $("#dialogAddEmployee").data("ejDialog");
            eDialog.open();



        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.SelectEmployee = function () {
        try {

            for (var i = 0; i < $scope.BonusRetainedList.length; i++) {
                if ($scope.BonusRetainedList[i].CheckBoxSelect === true) {

                    var IsEmpty = $scope.BonusRetainedListForSaved.filter(function (hero) {
                        return hero.EmpInfoSystemID == $scope.BonusRetainedList[i].EmpInfoSystemID;
                    });

                    if (IsEmpty.length === 0) {
                        $scope.BonusRetainedListForSaved.push($scope.BonusRetainedList[i]);
                    };

                }
            }
            var gridObj = $("#GridBonusRetainedListForSaved").data("ejGrid");
            gridObj.refreshContent();

            var eDialog = $("#dialogAddEmployee").data("ejDialog");
            eDialog.close();


        } catch (e) {
            ShowResult(e, "failure");
        }
    };



    $scope.BonusRetainedListForSaved = [];
    $scope.BonusRetainedList = [];
    $scope.BonusRetainedDisbursementMasterList = [];
    $scope.GetBonusRetainedDisbursementMasterData = function () {
        try {
            $http.get($scope.GetBonusRetainedDisbursementMasterDataUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.BonusRetainedDisbursementMasterList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetBonusRetainedDisbursementMasterData();

    $scope.BonusRetainedList = [];
    $scope.GetBonusRetainedData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.CustomPara.DisbursementDate)) {
                throw "Enter Disbursement Date.";
            };
            $http.get($scope.GetBonusRetainedDataUrl + '?DisbursementDate=' + $scope.CustomPara.DisbursementDate)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.BonusRetainedList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridBonusRetainedList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.BonusRetainedList.length; i++) {
                $scope.BonusRetainedList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridBonusRetainedList").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.BonusRetainedDetailsList = [];

    $scope.details = function () {
        try {


            var eDialog = $("#dialogDetails").data("ejDialog");
            eDialog.open();
            //var gridObj = $("#GridBonusRetainedList").data("ejGrid");
            //var modeldata = gridObj.getSelectedRecords()[0];

            var gridObj = $("#GridBonusRetainedListForSaved").data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];
            $scope.employeeInformation = data;

            if (!baseService.isUndefinedOrNull($scope.employeeInformation.SystemID)) {
                $http.get($scope.GetBonusRetainedSavedDetailsUrl + '?DisbursementDate=' + $scope.CustomPara.DisbursementDate + '&EmployeeSystemId=' + $scope.employeeInformation.EmpInfoSystemID)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.Message, 'failure');
                        }
                        else {
                            $scope.BonusRetainedDetailsList = response.data;
                            var gridObj = $("#GridBonusRetainedDetails").data("ejGrid");
                            gridObj.clearFiltering();



                            $("#GridBonusRetainedDetails").ejGrid({
                                dataSource: $scope.BonusRetainedDetailsList,//CreateSummaryList($scope.BonusRetainedDetailsList, "DisbusmentAmount", "DisbusmentAmount"),
                                minWidth: 450, minHeight: 400,
                                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                                filterSettings: { filterType: "excel" },
                                //columns: ColumnList,
                                showSummary: true,
                                summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DisbusmentAmount", dataMember: "DisbusmentAmount", format: "{0:N0}" }] }]
                            });
                        }
                    },

                        function errorCallBack(response) {
                            ShowResult(response.Message, 'failure');
                        });
            }
            else {
                $http.get($scope.GetBonusRetainedDetailsUrl + '?DisbursementDate=' + $scope.CustomPara.DisbursementDate + '&EmployeeSystemId=' + $scope.employeeInformation.EmpInfoSystemID)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.Message, 'failure');
                        }
                        else {
                            $scope.BonusRetainedDetailsList = response.data;
                            var gridObj = $("#GridBonusRetainedDetails").data("ejGrid");
                            gridObj.clearFiltering();



                            $("#GridBonusRetainedDetails").ejGrid({
                                dataSource: $scope.BonusRetainedDetailsList,//CreateSummaryList($scope.BonusRetainedDetailsList, "DisbusmentAmount", "DisbusmentAmount"),
                                minWidth: 450, minHeight: 400,
                                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                                filterSettings: { filterType: "excel" },
                                //columns: ColumnList,
                                showSummary: true,
                                summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DisbusmentAmount", dataMember: "DisbusmentAmount", format: "{0:N0}" }] }]
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

    $scope.detailsSaved = function () {
        try {


            var eDialog = $("#dialogDetails").data("ejDialog");
            eDialog.open();
            var gridObj = $("#GridSevedBonusRetained").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];




            $http.get($scope.GetBonusRetainedSavedDetailsUrl + '?DisbursementDate=' + $scope.CustomPara.DisbursementDate + '&EmployeeSystemId=' + modeldata.EmpInfoSystemID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.BonusRetainedDetailsList = response.data;
                        var gridObj = $("#GridBonusRetainedDetails").data("ejGrid");
                        gridObj.clearFiltering();
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.BonusRetainedModel = {
        EmpInfoSystemID: null,
        DisbusmentAmount: null
    }


    $scope.SaveBonusRetainedData = function () {

        try {
            //if (baseService.isUndefinedOrNull($scope.DailyAllowanceType)) {
            //    throw "Enter Allowance.";
            //};
            if (baseService.isUndefinedOrNull($scope.CustomPara.DisbursementDate)) {
                throw "Enter Disbursement Date.";
            };
            if (baseService.isUndefinedOrNull($scope.CustomPara.Description)) {
                throw "Enter Description.";
            };
            var tempBonusRetainedList = [];

            for (var i = 0; i < $scope.BonusRetainedListForSaved.length; i++) {
                //if ($scope.BonusRetainedList[i].CheckBoxSelect === true) {
                if (baseService.isUndefinedOrNull($scope.BonusRetainedListForSaved[i].SystemID)) {
                    $scope.BonusRetainedModel = {
                        EmpInfoSystemID: null,
                        DisbusmentAmount: null
                    }
                    $scope.BonusRetainedModel.EmpInfoSystemID = $scope.BonusRetainedListForSaved[i].EmpInfoSystemID;
                    $scope.BonusRetainedModel.DisbusmentAmount = $scope.BonusRetainedListForSaved[i].DisbusmentAmount;
                    tempBonusRetainedList.push($scope.BonusRetainedModel);
                }
            }


            if (tempBonusRetainedList.length === 0) {
                throw "Please Select Employee";
            };


            $.ajax({
                type: "POST",
                url: $scope.SaveBonusRetainedDataUrl,
                data: { 'CustomPara': $scope.CustomPara, 'BonusRetainedList': tempBonusRetainedList },
                dataType: "json",
                success: function (data) {
                    if (data.Error === true) {
                        ShowResult(data.Message, "failure");
                    }
                    else {
                        ShowResult(data.Message, "success");
                        $scope.GetBonusRetainedDisbursementMasterData();
                        $scope.GetBonusRetainedData();
                        $scope.BonusRetainedList = [];
                        $scope.IsEdit = false;
                        var gridObj = $("#GridBonusRetainedListForSaved").data("ejGrid");
                        gridObj.refreshContent();

                    }

                }

            });



        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.IsEdit = false;
    $scope.SevedBonusRetainedDataList = [];
    $scope.Get = function (obj) {
        try {
            $scope.IsEdit = true
            $scope.BonusRetainedListForSaved = [];

            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
            $scope.CustomPara = {
                DisbursementDate: null,
                Description: null

            };
            //var gridObj = $("#GridBonusRetainedDisbursementMasterList").data("ejGrid");
            //var modeldata = gridObj.getSelectedRecords()[0];
            $scope.CustomPara.DisbursementDate = obj.data.DisbursementDate;
            $scope.CustomPara.Description = obj.data.Description;

            $http.get($scope.GetSevedBonusRetainedDataUrl + '?BonusRetainedDisbursementMasterId=' + obj.data.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.BonusRetainedListForSaved = [];
                        $scope.BonusRetainedListForSaved = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {

        $scope.CustomPara = {
            DisbursementDate: null,
            Description: null
        };
        $scope.BonusRetainedListForSaved = [];
        $scope.BonusRetainedList = [];
        $scope.SevedBonusRetainedDataList = [];
        var gridObj = $("#GridBonusRetainedListForSaved").data("ejGrid");
        gridObj.refreshContent();


        $scope.IsEdit = false;
    };


    $scope.employeeInformation = {};
    $scope.message_confirmation = null;
    $scope.remove = function (obj) {
        var gridObj = $("#GridBonusRetainedListForSaved").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.employeeInformation = data;
        if (!baseService.isUndefinedOrNull($scope.employeeInformation.EmpInfoSystemID))
            $scope.message_confirmation = 'Are you sure to remove This  ?';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    };

    $scope.Delete = function () {

        try {
            var temp = [];

            for (var i = 0; i < $scope.BonusRetainedListForSaved.length; i++) {
                if ($scope.BonusRetainedListForSaved[i].EmpInfoSystemID === $scope.employeeInformation.EmpInfoSystemID) {

                } else {
                    temp.push($scope.BonusRetainedListForSaved[i]);
                }
            }
            $scope.BonusRetainedListForSaved = [];
            $scope.BonusRetainedListForSaved = temp;

            var gridObj = $("#GridBonusRetainedListForSaved").data("ejGrid");
            gridObj.refreshContent();



            if (!baseService.isUndefinedOrNull($scope.employeeInformation.SystemID)) {

                $.ajax({
                    type: "POST",
                    url: $scope.DeleteBonusRetainedDisbursementUrl,
                    data:
                    {

                        'EmpInfoSystemID': $scope.employeeInformation.EmpInfoSystemID, 'SystemID': $scope.employeeInformation.SystemID
                    },
                    dataType: "json",
                    success: function (response) {
                        if (response.Error) {
                            ShowResult(response.Message, 'error');
                        } else {

                            ShowResult(response.Message, 'success');


                        }

                    }

                });
            }



        } catch (e) {
            ShowResult(e, 'error');
        }
    };






    //#region Tab




    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };


    // #endregion Tab




    //$scope.ShiftInfoList = [];
    //$scope.getShiftInfo = function () {
    //    try {
    //        $http.get($scope.getShiftInfoUrl)
    //            .then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.Message, 'failure');
    //                }
    //                else {
    //                    $scope.ShiftInfoList = response.data;
    //                }
    //            },

    //                function errorCallBack(response) {
    //                    ShowResult(response.Message, 'failure');
    //                });


    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};
    //$scope.getShiftInfo();

    //$scope.DailyAllowanceList = [];
    //$scope.getDailyAllowance = function () {
    //    try {
    //        $http.get($scope.getDailyAllowanceUrl)
    //            .then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.Message, 'failure');
    //                }
    //                else {
    //                    $scope.DailyAllowanceList = response.data;
    //                }
    //            },

    //                function errorCallBack(response) {
    //                    ShowResult(response.Message, 'failure');
    //                });


    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};
    //$scope.getDailyAllowance();




    //$scope.SaveDailyAllowanceData = function () {

    //    try {
    //        if (baseService.isUndefinedOrNull($scope.DailyAllowanceType)) {
    //            throw "Enter Allowance.";
    //        };
    //        for (var i = 0; i < $scope.ShiftInfoList.length; i++) {
    //            if ($scope.ShiftInfoList[i].CheckBoxSelect===true) {
    //                if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].EffectiveTime)) {
    //                    throw "Enter Effective Time.";
    //                };
    //                if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].FromDate)) {
    //                    throw "Enter From Date.";
    //                };
    //                if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].ToDate)) {
    //                    throw "Enter To Date.";
    //                };
    //            }
    //        }
    //        $.ajax({
    //            type: "POST",
    //            url: $scope.SaveDailyAllowanceUrl,
    //            data: { 'DailyAllowanceData': $scope.ShiftInfoList, 'DailyAllowanceType': $scope.DailyAllowanceType },
    //            dataType: "json",
    //            success: function (data) {
    //                if (data.Error === true) {
    //                    ShowResult(data.Message, "failure");
    //                }
    //                else {
    //                    ShowResult(data.Message, "success");
    //                    $scope.getDailyAllowance();
    //                    $scope.ShiftInfoList = [];
    //                    $scope.getShiftInfo();
    //                }

    //            }

    //        });



    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};





    //$scope.refreshTemplateemployee4 = function (args) {
    //    $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    //};

    //function CheckBoxSelectAllEmolyeeWise(e) {
    //    //console.log('ok');


    //    if (e.model.checkState === "check") {

    //        for (var i = 0; i < $scope.ShiftInfoList.length; i++) {

    //            $scope.ShiftInfoList[i].CheckBoxSelect = true;
    //        }
    //    }
    //    else {
    //        //console.log('co-ok');
    //        for (var i = 0; i < $scope.ShiftInfoList.length; i++) {

    //            $scope.ShiftInfoList[i].CheckBoxSelect = false;


    //        }
    //    }
    //    //var gridObj = $("#GridShiftInfo").data("ejGrid");
    //    //gridObj.refreshContent();
    //};

    //$scope.custompara = {};
    //$scope.message_confirmation = null;
    //$scope.remove = function (obj) {
    //    var gridObj = $("#GridShiftInfoShow").data("ejGrid");
    //    var data = gridObj.getSelectedRecords()[0];
    //    $scope.custompara = data.Id;
    //    //if (!baseService.isUndefinedOrNull($scope.employeeInformation.SystemId))
    //        $scope.message_confirmation = 'Are you sure to Delete This Setting ?';
    //    angular.element(document.querySelector('#confirmPopUp')).modal('show');
    //};

    //$scope.Delete = function () {

    //    $.ajax({
    //        type: "POST",
    //        url: $scope.deleteDailyAllowanceUrl,
    //        data:
    //        {

    //            'Id': $scope.custompara
    //        },
    //        dataType: "json",
    //        success: function (response) {
    //            //$scope.ShowResult(data.Message, "success");
    //            ShowResult(response.Message, 'success');
    //            $scope.getDailyAllowance();

    //        }

    //    });
    //};








}