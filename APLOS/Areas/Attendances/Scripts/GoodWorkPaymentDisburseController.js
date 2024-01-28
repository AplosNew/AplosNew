'use strict';
GoodWorkPaymentDisburseController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function GoodWorkPaymentDisburseController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Good Work Payment Disburse';
    $rootScope.titleTab1 = 'Undisburse Data';
    $rootScope.titleTab2 = 'Disburse Data';

    $scope.WorkerAdvanceList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.saveUrl = $scope.path + 'CreateWorkerAdvance';
    $scope.savePCUrl = $scope.path + 'PayableCreationSave';
    $scope.UpdateUrl = $scope.path + 'UpdateGoodWorkDetailEdit';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    baseService.init($scope.getListUrl);
    //$scope.LoadEmpListUrl = $scope.path + 'LoadPCAACEmployeelist';
    $scope.Action = 'Save';
    $scope.PCAction = 'Save';
    $scope.PCOTAction = 'Save';
    $scope.passwordShow = true;
    $controller("employeeBaseController", { $scope: $scope, $http: $http });


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.PCEmployeeList = [];
    $scope.PCOTEmployeeUndisburseList = [];
    $scope.GetLoadEmployeeInformation = function () {
        $scope.TabName = 'PaymentDisburse';
        if ($scope.ToDate === "" || $scope.ToDate === null || $scope.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        if ($scope.FromDate === "" || $scope.FromDate === null || $scope.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        $http({
            method: 'POST',
            url: $scope.path + "LoadPCEmployeelist",
            data: { 'fromDate': $scope.FromDate, 'toDate': $scope.ToDate, 'tabName': $scope.TabName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCOTEmployeeUndisburseList = response.data;
            $scope.GetGoodWorkPaymentDisburseOTAdvisedetail();
        });
    }

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridChildEdit").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PCOTEmployeeUndisburseList.length; i++) {
                $scope.PCOTEmployeeUndisburseList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridChildEdit").data("ejGrid");
        gridObj.refreshContent();
    };

    var getString = function (data) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i]) == false) {
                string += ",'" + data[i] + "'";
                collection.push(data[i]);
            }
        }
        return string;
    }

    $scope.GoodWorkPaymentDisburseSave = function () {
        try {
            $scope.NewDisburseIds = [];
            for (var i = 0; i < $scope.PCOTEmployeeUndisburseList.length; i++) {
                if ($scope.PCOTEmployeeUndisburseList[i].isSelected == true) {
                    $scope.NewDisburseIds.push($scope.PCOTEmployeeUndisburseList[i].EmpSystemId);
                }
            }
            var disburseIds = getString($scope.NewDisburseIds);

            $http({
                method: 'POST',
                url: $scope.path + 'CreateGoodWorkPaymentDisburse',
                data: { 'Id': disburseIds },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetGoodWorkPaymentUnDisburseOTAdvisedetail();
                    $scope.GetGoodWorkPaymentDisburseOTAdvisedetail();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetGoodWorkPaymentUnDisburseOTAdvisedetail = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdviseUnDisburseOTDetailList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCOTEmployeeUndisburseList = response.data;
        });
    }

    $scope.PCOTEmployeedisburseList = [];
    $scope.GetGoodWorkPaymentDisburseOTAdvisedetail = function () {
        if ($scope.ToDate === "" || $scope.ToDate === null || $scope.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        if ($scope.FromDate === "" || $scope.FromDate === null || $scope.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        $scope.FD = $filter('dateFiltering')(new Date($scope.FromDate), 'dd-MM-yyyy');
        $scope.TD = $filter('dateFiltering')(new Date($scope.ToDate), 'dd-MM-yyyy');
        $http({
            method: 'Get',
            url: $scope.path + 'GetGoodWorkPaymentAdviseDisburseOTDetailList?fromDate=' + $scope.FD + '&toDate=' + $scope.TD,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCOTEmployeedisburseList = response.data;
        });
    }


    $scope.EmployeeMainList = [];
    $scope.GetWorkerAdvanceDetailCenter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetWorkerAdvanceDetailCenter?workAdvanceId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeMainList = resp.data;
        });
    }


    $scope.PCOTEmployeeUndisburseList = [];
    $scope.EmployeeMainList = [];
    $scope.getEmploymeeList = function () {

        if ($scope.ModelNew.FromDate === "" || $scope.ModelNew.FromDate === null || $scope.ModelNew.FromDate === undefined) {
            ShowResult('Select Work Date', 'failure');
            return false;
        }
        if ($scope.ModelNew.PayDaysType === "" || $scope.ModelNew.PayDaysType === null || $scope.ModelNew.PayDaysType === undefined) {
            ShowResult('Select From Pay Days', 'failure');
            return false;
        }
        $scope.FD = $filter('dateFiltering')(new Date($scope.ModelNew.FromDate), 'dd-MM-yyyy');
        $scope.TD = $filter('dateFiltering')(new Date($scope.ModelNew.ToDate), 'dd-MM-yyyy');

        $http({
            method: 'POST',
            url: $scope.path + "LoadPCAACEmployeelist",
            data: { 'fromDate': $scope.ModelNew.FromDate, 'toDate': $scope.ModelNew.ToDate, 'payDaysType': $scope.ModelNew.PayDaysType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCOTEmployeeUndisburseList = response.data;

            angular.element(document.querySelector("#dialogEmployeeInfo")).modal("show");
        });
    }

    $scope.getPayDaysAmount = function () {
        $http({
            method: 'POST',
            url: $scope.path + "LoadPCAACEmployeelist",
            data: { 'fromDate': $scope.ModelNew.FromDate, 'toDate': $scope.ModelNew.ToDate, 'payDaysType': $scope.ModelNew.PayDaysType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                for (var j = 0; j < $scope.EmployeeMainList.length; j++) {
                    if ($scope.EmployeeMainList[j].SystemId == $scope.EmployeeList[i].SystemId) {
                        $scope.EmployeeMainList[j].PayDays = $scope.EmployeeList[i].PayDays;
                    }
                }
            }
        });
    }


    //*********************************** Worker Advance End********************************************************//

    //***********************************Payable Creation Start*******************************************************//

    $scope.PayableCreationSave = function () {
        try {
            $scope.FD = $filter('dateFiltering')(new Date($scope.ModelNew.FromDate), 'dd-MM-yyyy');
            $scope.TD = $filter('dateFiltering')(new Date($scope.ModelNew.ToDate), 'dd-MM-yyyy');
            $http({
                method: 'POST',
                url: $scope.savePCUrl,
                data: { 'data': $scope.ModelPCNew, 'goodWorkPaymentDetail': $scope.PCEmployeeList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearPayableCreation();
                    $scope.GetGoodWorkPaymentData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GoodWorkPaymentList = [];
    $scope.GetGoodWorkPaymentData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentList?paymentSource=" + 'GoodWork',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GoodWorkPaymentList = response.data;
        });
    }
    $scope.GetGoodWorkPaymentData();

    $scope.GetGoodWorkPaymentAdvisedetail = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentAdviseDetailList?paymentAdviseId=" + $scope.ModelPCNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PCEmployeeList = response.data;
        });
    }



    //***********************************Payable Creation Extra OT Start********************************************************//

    $scope.GoodWorkOTPaymentList = [];
    $scope.GetGoodWorkOTPaymentData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkPaymentList?paymentSource=" + 'Attendance',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GoodWorkOTPaymentList = response.data;
        });
    }
    $scope.GetGoodWorkOTPaymentData();

}