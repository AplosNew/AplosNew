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

    // #region checkbox all

    $scope.refreshTemplateOperation = function (args) {
        $("#headchk").ejCheckBox({ "change": headCheckChangeOperation });
    };

    function headCheckChangeOperation(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeInformationList.length; i++) {
                $scope.EmployeeInformationList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmp").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SelectedEmployeeList = [];
    $scope.Close = function () {
        try {
            for (var i = 0; i < $scope.EmployeeInformationList.length; i++) {
                if ($scope.EmployeeInformationList[i].Flag == true) {
                    if (checkExists($scope.SelectedEmployeeList, $scope.EmployeeInformationList[i].SystemId) === false) {
                        var ob = {};
                        ob.Id = null;
                        ob.EmployeeCode = $scope.EmployeeInformationList[i].EmployeeCode;
                        ob.EmpSystemId = $scope.EmployeeInformationList[i].SystemId;
                        ob.EmployeeName = $scope.EmployeeInformationList[i].EmployeeName;
                        ob.DOJ = $scope.EmployeeInformationList[i].DOJ;
                        ob.DOS = $scope.EmployeeInformationList[i].DOS;
                        ob.LegalDesignation = $scope.EmployeeInformationList[i].LegalDesignation;
                        ob.Department = $scope.EmployeeInformationList[i].Department;
                        ob.EntityName = $scope.EmployeeInformationList[i].EntityName;

                        $scope.SelectedEmployeeList.push(ob);
                        ob = {};
                    }
                }
            }
            //$scope.SaveFNF();
            angular.element(document.querySelector('#dialogEmployeeInfo')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.Process = function () {
        try {
            if (baseService.arrayLength($scope.SelectedEmployeeList) < 0) {
                throw "Select Employee.";
            }

            $http({
                method: 'POST',
                url: 'Payrolls/FinalSettlement/Process',
                data: { 'data': $scope.FinalSettlementModel, 'datalist': $scope.SelectedEmployeeList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    // #endregion


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