'use strict';
fullandfinalSettlementController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function fullandfinalSettlementController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Final Settlement';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'Payrolls/FinalSettlement/';
    $scope.getSTListUrl = $scope.path + 'GetSeparationTypelist';
    $scope.getSTSCUrl = $scope.path + 'SeparationTypeSelectedChange';
    $scope.getEmployeeListUrl = $scope.path + 'GetEmployeelist';
    $scope.saveUrl = $scope.path + 'SaveFinalSettlement';
    $scope.getFSListUrl = $scope.path + 'GetEmployeeFinalSettlementlist';
    $scope.getDataForEditUrl = $scope.path + 'GetDataForEdit';
    $scope.getETListUrl = $scope.path + 'GetEmploymentTypelist'; 
    $scope.getListUrl = $scope.path + 'GetSeparationTypelist';   
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
                data: { 'FinalSettlementData': $scope.FinalSettlementModel, 'DeductionData': $scope.FinalSettlementDeductionHeadList, 'EarningData': $scope.FinalSettlementEarningHeadList, 'FinalSettlementRetainedHead': $scope.FinalSettlementRetainedHeadList },
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
    $scope.FormulaList = [];
    $scope.SelectEmployee = function (obj) {
        try {
            $scope.EmployeeModel = obj.data;
            $http({
                method: 'GET',
                url: 'Payrolls/FinalSettlement/GetEmployeeSeperationItemFormulaData?EmpSystemId=' + $scope.EmployeeModel.SystemId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.FormulaList = response.data;

                    //for (var i = 0; i < $scope.FormulaList.length; i++) {
                    //    if ($scope.FormulaList[i].) {

                    //    }
                    //}
                    angular.element(document.querySelector('#dialogEmployeeInfo')).modal('hide');
                }
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
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