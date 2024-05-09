'use strict';
fullandfinalSettlementApproveController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function fullandfinalSettlementApproveController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Final Settlement Approve';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'Payrolls/FinalSettlement/';
   
    $scope.FinalSettlementList = [];
    $scope.LoadAllFinalSettlementList = function () {
        try {
            $http.get('Payrolls/FinalSettlement/GetFNFMasterDataForApprove')
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

    // #region checkbox all

    $scope.FinalSettlementModel = { Id: null, FinalSettlementName: null, FinalSettlementDate: null }

 
    $scope.Approve = function () {
        try {
           

            $http({
                method: 'POST',
                url: 'Payrolls/FinalSettlement/ApproveFNF',
                data: { 'data': $scope.FinalSettlementModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.FinalSettlementModel.Id = response.data.Data.Id;
                    $scope.SelectedEmployeeList = [];
                    $scope.LoadAllFinalSettlementList();
                    $scope.Clear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

   


    // #endregion

    $scope.EmpSystemId = null;
    $scope.FormulaList = [];
    $scope.FinalSettlementUndisbursedEarningList = [];
    $scope.GetEmployeeItems = function (obj) {
        $scope.FormulaList = [];
        $scope.EmpSystemId = obj.data.EmpSystemId;
        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetEmployeeSeperationItemFormulaData?EmpSystemId=' + $scope.EmpSystemId
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

    $scope.GetSavedEmployeeItems = function () {
        $scope.FormulaList = [];
        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetEmployeeSeperationItemFormulaData?EmpSystemId=' + $scope.EmpSystemId
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
        $scope.FinalSettlementModel = {};
        $scope.SelectedEmployeeList = [];
        $scope.FormulaList = [];
    }
};