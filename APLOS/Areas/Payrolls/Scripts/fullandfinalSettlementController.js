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


    $scope.FinalSettlementList = [];
    $scope.LoadAllFinalSettlementList = function () {
        try {
            $http.get('Payrolls/FinalSettlement/GetFNFMasterData')
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

    $scope.SelectedEmployeeList = [];
    $scope.GetEmployeeFNFMasterData = function () {
        $scope.SelectedEmployeeList = [];
        try {
            $http.get('Payrolls/FinalSettlement/GetEmployeeFNFDataByMaster?masterId=' + $scope.FinalSettlementModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.SelectedEmployeeList = response.data;
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.SelectEmpDetail = function (args) {
        $scope.FinalSettlementModel = Object.assign({}, args.data);
        $scope.FinalSettlementModel.FinalSettlementDate = $filter('dateFiltering')($scope.FinalSettlementModel.FinalSettlementDate, 'dd-M-yyyy');
        $scope.GetEmployeeFNFMasterData();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.approvedByList = [];
    $scope.GetApprovedByCboList = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetApprovedByCbo'
        }).then(function successCallback(response) {
            $scope.approvedByList = response.data;
            if (baseService.arrayLength($scope.approvedByList) == 1) {
                $scope.FinalSettlementModel.ApproveById = $scope.approvedByList[0].Value;
            }
        });
    }
    $scope.GetApprovedByCboList();

    $scope.FinalSettlementRetainedHeadList = [];
    $scope.EmployeeInformationList = [];
    $scope.LoadEmployeeList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FinalSettlementModel.FinalSettlementName)) {
                throw "Final Settlement Name is required.";
            }
            if (baseService.isUndefinedOrNull($scope.FinalSettlementModel.FinalSettlementDate)) {
                throw "Final Settlement Date is required.";
            }

            $http.get($scope.getEmployeeListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeInformationList = response.data;
                        angular.element(document.querySelector('#dialogEmployeeInfo')).modal('show');
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

    $scope.FinalSettlementModel = { Id: null, FinalSettlementName: null, FinalSettlementDate: null, ApproveById: null, IsApproved: false };

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
                        ob.State = true;
                        $scope.SelectedEmployeeList.push(ob);
                        ob = {};
                    }
                }
            }
            angular.element(document.querySelector('#dialogEmployeeInfo')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.message_confirmation = null;
    $scope.RemoveEmployee = function (obj) {
        if ($scope.FinalSettlementModel.IsApproved == false) {
            $scope.DG = obj.data;
            $scope.EmpSysId = $scope.DG.EmpSystemId;
            if (!baseService.isUndefinedOrNull($scope.DG.Id))
                $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.DG.EmployeeCode + ' ]';
            angular.element(document.querySelector('#confirmPopUp')).modal('show');
        }
        else {
            ShowResult("Final Settlement is Approved",'failure');
        }
    }

    $scope.DeleteEmp = function () {
        if (!baseService.isUndefinedOrNull($scope.DG.Id)) {
            $http({
                method: 'POST',
                url: 'Payrolls/FinalSettlement/DeleteEmp?empId=' + $scope.DG.EmpSystemId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetEmployeeFNFMasterData();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
        else {
            for (var i = 0; i < $scope.SelectedEmployeeList.length; i++) {
                if ($scope.SelectedEmployeeList[i].EmpSystemId == obj.data.EmpSystemId) {
                    if (baseService.isUndefinedOrNull(obj.data.Id)) {
                        $scope.SelectedEmployeeList.splice(i, 1);
                        break;
                    }
                }
            }
        }
    };

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.refreshTemplateEmp = function (args) {
        $("#headchkEmp").ejCheckBox({ "change": headCheckChangeEmp });
    };

    function headCheckChangeEmp(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSelectedEmp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SelectedEmployeeList.length; i++) {
                $scope.SelectedEmployeeList[i].State = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].State = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSelectedEmp").data("ejGrid");
        gridObj.refreshContent();
    };



    $scope.Process = function () {
        try {
            $scope.SelectedEmpList = [];
            for (var i = 0; i < $scope.SelectedEmployeeList.length; i++) {
                if ($scope.SelectedEmployeeList[i].State == true) {
                    $scope.SelectedEmpList.push($scope.SelectedEmployeeList[i]);
                }
            }

            if (baseService.arrayLength($scope.SelectedEmpList) < 0) {
                throw "Select Employee.";
            }

            $http({
                method: 'POST',
                url: 'Payrolls/FinalSettlement/Process',
                data: { 'data': $scope.FinalSettlementModel, 'datalist': $scope.SelectedEmpList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.FinalSettlementModel.Id = response.data.Data.Id;
                    $scope.GetEmployeeFNFMasterData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.UpdateItemData = function () {
        try {
            $http({
                method: 'POST',
                url: 'Payrolls/FinalSettlement/UpdateItemData',
                data: { 'datalist': $scope.FormulaList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.GetSavedEmployeeItems();
                }
            }, function errorCallback(response) {
                $scope.ShowResultCustom(response.status.Message, "failure");
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }


    // #endregion

    $scope.EmpSysId = null;
    $scope.FormulaList = [];
    $scope.FinalSettlementUndisbursedEarningList = [];
    $scope.GetEmployeeItems = function (obj) {
        $scope.FormulaList = [];
        $scope.EmpSysId = obj.data.EmpSystemId;
        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetEmployeeSeperationItemFormulaData?EmpSystemId=' + $scope.EmpSysId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.FormulaList = response.data.SeperationItem;
                $scope.FinalSettlementUndisbursedEarningList = response.data.FinalSettlementUndisbursedEarning;
                angular.element(document.querySelector('#FormulaInfo')).modal('show');
            }
        });
    }

    $scope.GetSavedEmployeeItems = function () {

        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetEmployeeSeperationItemFormulaData?EmpSystemId=' + $scope.EmpSysId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.FormulaList = response.data.SeperationItem;
                $scope.FinalSettlementUndisbursedEarningList = response.data.FinalSettlementUndisbursedEarning;
            }
        });
    }


    $scope.CloseFormulaPopUp = function () {
        angular.element(document.querySelector('#FormulaInfo')).modal('hide');
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.FinalSettlementModel = { Id: null, FinalSettlementName: null, FinalSettlementDate: null, ApproveById: null, IsApproved: false };
        $scope.SelectedEmployeeList = [];
        $scope.FormulaList = [];
        if (baseService.arrayLength($scope.approvedByList) == 1) {
            $scope.FinalSettlementModel.ApproveById = $scope.approvedByList[0].Value;
        }
    }


    $scope.PrintData = function (data) {
        try {
            $scope.fileName = "EmpSepItemReport.xls";


            //  $scope.ReportFormat = 'Excel';
            $scope.ReportFormat = 'Pdf';
            var url = 'Payrolls/FinalSettlement/GetEmpSepItemReportPdf?reportFormat=' + $scope.ReportFormat + '&empId=' + data.data.EmpSystemId;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


};