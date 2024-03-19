'use strict';
finalSettlementNewController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function finalSettlementNewController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Final Settlement';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'Payrolls/FinalSettlement/';
    $scope.getSTListUrl = $scope.path + 'GetSeparationTypelist';
    //$scope.getSTSCUrl = $scope.path + 'SeparationTypeSelectedChangeNew';
    $scope.getSTSCUrl = $scope.path + 'SeparationTypeSelectedChange';
    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.saveUrl = $scope.path + 'SaveFinalSettlementNew';
    $scope.getFSListUrl = $scope.path + 'GetEmployeeFinalSettlementlist';
    $scope.getDataForEditUrl = $scope.path + 'GetDataForEdit';

    $scope.getETListUrl = $scope.path + 'GetEmploymentTypelist';
 
    $scope.getListUrl = $scope.path + 'GetSeparationTypelist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
   
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    $scope.salaryRuleGeneral = {
        FormulaDescription : null,
        FormulaIDDescription : null
    };
    //$scope.getData();
    $scope.btnSave = false;
    $scope.SeparationTypeList = [];
    $scope.getSeparationType = function () {
        try {
            $http.get($scope.getSTListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.SeparationTypeList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getSeparationType();  

    $scope.CustomPara = {
        SeparationTypeId: null,
        EmpSystemId: null
    };
    $scope.FinalSettlementUndisbursedEarningList = [];
    $scope.FinalSettlementEarningHeadList = [];
    $scope.FinalSettlementDeductionHeadList = [];
    $scope.SeparationTypeDetails = {};
    $scope.FinalSettlementModel = {};
    $scope.SeparationTypeSelectedChange = function () {
        try {
            $http.get($scope.getSTSCUrl + '?SeparationTypeId=' + $scope.CustomPara.SeparationTypeId + '&EmpSystemId=' + $scope.EmployeeModel.SystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                    
                        $scope.FinalSettlementModel = response.data[0];
                        $scope.FinalSettlementDeductionHeadList = response.FinalSettlementDeduction[0];
                        $scope.FinalSettlementEarningHeadList = response.FinalSettlementEarning[0];
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.FinalSettlementList = [];
    $scope.LoadAllFinalSettlementList = function () {
        try {
            $http.get($scope.getFSListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.FinalSettlementList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadAllFinalSettlementList();


    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FinalSettlementModel.FinalSettlementDate)) {
                throw "Please Enter Final Settlement Date";
            }

            if (new Date($scope.FinalSettlementModel.FinalSettlementDate) > new Date()) {
                throw "Please Enter valid Date";
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'FinalSettlementData': $scope.FinalSettlementModel
                    , 'DeductionData': $scope.FinalSettlementDeductionHeadList
                    , 'EarningData': $scope.FinalSettlementEarningHeadList
                    , 'FinalSettlementRetainedHead': $scope.FinalSettlementRetainedHeadList
                    , 'UndisbursedEarningList': $scope.FinalSettlementUndisbursedEarningList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadAllFinalSettlementList();
                    $scope.btnSave = false;

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }



       
    };
    $scope.EditFinalSettlement = function (obj) {
        var gridObj = $("#GridFinalSettlementList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.FinalSettlementNew = data;
        $scope.getDataForEdit(data.Id);

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        // $scope.getSalaryRuleESIC();
    };
    $scope.FinalSettlementNew = {};
    $scope.getDataForEdit = function (Id) {
        try {
            $http.get($scope.getDataForEditUrl + '?Id=' + Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.FinalSettlementModel = response.data.FinalSettlement[0];
                        $scope.EmployeeModel = response.data.EmployeeInfo[0];
                        $scope.btnSave = true;
                       
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };




    $scope.FinalSettlementRetainedHeadList = [];
    $scope.EmployeeInformationList = [];
    $scope.LoadEmployeeList = function () {
        try {
           
            //var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            //eDialog.open();
            angular.element(document.querySelector('#dialogEmployeeInfo')).modal('show');



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
    $scope.SelectEmployee = function (obj) {
        try {
           
           // var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
            $scope.EmployeeModel = obj.data;

            //var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            //eDialog.close();

            angular.element(document.querySelector('#dialogEmployeeInfo')).modal('hide');

            $http.get($scope.getSTSCUrl + '?EmpSystemId=' + $scope.EmployeeModel.SystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.FinalSettlementRetainedHeadList = [];
                        $scope.FinalSettlementEarningHeadList = [];
                        $scope.FinalSettlementDeductionHeadList = [];
                        $scope.FinalSettlementModel = {};
                        $scope.FinalSettlementUndisbursedEarningList = response.data.FinalSettlementUndisbursedEarning;
                        $scope.FinalSettlementModel = response.data.data;
                        $scope.FinalSettlementDeductionHeadList = response.data.FinalSettlementDeduction;
                        $scope.FinalSettlementEarningHeadList = response.data.FinalSettlementEarning;
                        $scope.FinalSettlementRetainedHeadList = response.data.FinalSettlementRetainedHead;
                        $scope.FinalSettlementModel.LastMonthNetPayAmount = 0;
                        if (baseService.arrayLength($scope.FinalSettlementUndisbursedEarningList)>0) {
                            for (var i = 0; i < $scope.FinalSettlementUndisbursedEarningList.length; i++) {
                                $scope.FinalSettlementModel.LastMonthNetPayAmount += $scope.FinalSettlementUndisbursedEarningList[i].DisbusmentAmount;
                            }
                        }
                        $scope.btnSave = true;
                        $scope.AddRetained();
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });




            //$http.get($scope.getEmployeeListUrl)
            //    .then(function successCallback(response) {
            //        if (response.data.Error === true) {
            //            ShowResult(response.data.Message, 'failure');
            //        }
            //        else {
            //            $scope.EmployeeInformationList = response.data;
            //        }
            //    },

            //        function errorCallBack(response) {
            //            ShowResult(response.data.Message, 'failure');
            //        });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.EmploymentTypeList = [];
    $scope.getEmploymentType = function () {
        try {
            $http.get($scope.getETListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmploymentTypeList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getEmploymentType();
    
    $scope.AddDeduction = function () {
        try {
            
            var total = 0;
            for (var i = 0; i < $scope.FinalSettlementDeductionHeadList.length; i++) { 
                if (!baseService.isUndefinedOrNull($scope.FinalSettlementDeductionHeadList[i].DeductionAmount)) {
                    total += parseInt($scope.FinalSettlementDeductionHeadList[i].DeductionAmount);
                }
               
            }
            $scope.FinalSettlementModel.DeductionAmount = total;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.AddEarning = function () {
        try {

            var total = 0;
            for (var i = 0; i < $scope.FinalSettlementEarningHeadList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.FinalSettlementEarningHeadList[i].EarningAmount)) {
                    total += parseInt($scope.FinalSettlementEarningHeadList[i].EarningAmount);
                }

            }
            $scope.FinalSettlementModel.EarningAmount = total;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.AddNoticePeriodAmount = function () {
        try {
            
           
            if ($scope.FinalSettlementModel.NoticePeriodType == 'Deduction') {
                $scope.FinalSettlementModel.TotalPayableAmount =$scope.FinalSettlementModel.EarningAmount +
                    $scope.FinalSettlementModel.LastMonthNetPayAmount +
                    $scope.FinalSettlementModel.LvEncashmentAmount +
                    $scope.FinalSettlementModel.FixedDayAmount +
                    $scope.FinalSettlementModel.SeparationTypeAmount;

                $scope.FinalSettlementModel.NetPayAmount =$scope.FinalSettlementModel.DeductionAmount + $scope.FinalSettlementModel.EarnLvDeductionAmount + $scope.FinalSettlementModel.NoticePeriodAmount;


            } else {
                $scope.FinalSettlementModel.TotalPayableAmount =$scope.FinalSettlementModel.EarningAmount +
                    $scope.FinalSettlementModel.LastMonthNetPayAmount +
                    $scope.FinalSettlementModel.LvEncashmentAmount +
                    $scope.FinalSettlementModel.FixedDayAmount +
                    $scope.FinalSettlementModel.SeparationTypeAmount +
                    $scope.FinalSettlementModel.NoticePeriodAmount;

                $scope.FinalSettlementModel.NetPayAmount =$scope.FinalSettlementModel.DeductionAmount + $scope.FinalSettlementModel.EarnLvDeductionAmount;
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.AddRetained = function () {
        try {

            var total = 0;
            for (var i = 0; i < $scope.FinalSettlementRetainedHeadList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.FinalSettlementRetainedHeadList[i].DisbusmentAmount)) {
                    total += parseInt($scope.FinalSettlementRetainedHeadList[i].DisbusmentAmount);
                }

            }
            $scope.FinalSettlementModel.TotalRetainedAmount = total;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.SeparationType = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        FormulaDes: null,
        FormulaDesID: null,
        IsGratuityApplicable: false,
        IsActive: true,
        AddedBy: null,
        AddedDate: new Date('dd-MMM-yyyy'),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.SeparationTypesList = [];
    $scope.getSeparationTypesList = function () {
        try {
            $http.get($scope.getListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.SeparationTypesList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getSeparationTypesList();



    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.SeparationType.Sequence = response.data[0].Sequence;
            });
    };

    $scope.CheckIdUse = function (id) {
        $http.get('accounts/SeparationType/checkiduse?id=' + id)
            .then(function (response) {
                $scope.checkIdUsedValue = response.data;
            });
    };

    $scope.GetSequence();

  

    
    

    $scope.EditSeparationType = function (obj) {
        var gridObj = $("#GridSeparationTypesList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.SeparationTypeNew = data;
        $scope.getDataForEdit(data.Id);

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        // $scope.getSalaryRuleESIC();
    };




    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.SeparationType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.SeparationType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SeparationTypes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
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