'use strict';
purchaseorderRegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function purchaseorderRegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
	$rootScope.title = "Material Ledger / Report";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Materials/MaterialLedger/';
	$scope.path1 = 'Accounts/InventoryPayable/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	$scope.RowColor = "";
	$scope.isAlternative = -1;
	$scope.rowDataBound = function rowDataBound(e) {

		if ($scope.RowColor != e.data.Id ) {
			$scope.isAlternative = $scope.isAlternative * -1;
			$scope.RowColor = e.data.Id;
		}
		if ($scope.isAlternative > 0)
			e.row.css("background-color", '#D3D3D3');
		else
			e.row.css("background-color", '#ffffff');


	}
	$scope.Print = function () {
		
		var gridObj1 = $("#GridPO").data("ejGrid");
		var data1 = gridObj1.model.dataSource();
		$http({
			method: 'POST',
			url: $scope.exportgriddataUrl,
			//data: { 'data': data1 }
			data: JSON.stringify(data1)
		}).then(function successCallback(response) {
			if (response.data.Error == true) {
				// ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

			}
			else {

				location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
			}
		});
	}
	$scope.PrintPurchaseRegister = function () {
		
		var gridObj11 = $("#GridPrint").data("ejGrid");
		var data11 = gridObj11.model.dataSource();

		$http({

			method: "POST",
			url: $scope.exportgriddataUrl,
			data: { 'data': data11 }

		}).then(function successCallback(response) {
			if (response.data.Error == true) {
				// ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

			}
			else {

				location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
			}
		});

	}

	
	
    $scope.productNew = {
        IsClose: false,
        Type: null,
        WithStock: true,
        WithoutStock: false
	};
	$scope.changeSourceFrom = function (from) {
        debugger;
        if (from === 'AsOnDate') {
            $scope.report.FromDate = "";

			$scope.productNew.Type = 'Posted';

		}
        if (from === 'ForThePeriod') {
			$scope.productNew.Type = 'NonPosted';


		}
	};

	$scope.GriddataMaterialLedger = [];
	$scope.getaldataMaterialLedger = function () {
		
		$http({
			method: 'POST',
			//url: $scope.getSearchListUrl,
			url: 'Materials/MaterialLedger/GetMaterialLedger',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
				Type: $scope.productNew.Type
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.GriddataMaterialLedger = response.data;

			//entrydata = copy(searchdata);
		});
	};


	$scope.PurchaseRegisterLst = [];
	$scope.pivotTableFieldListID = [];
	$scope.GetPurchaseRegister = function () {
		debugger;
		
		if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		else if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		$http({
			method: 'POST',
			//url: $scope.getSearchListUrl,
            url: 'Materials/MaterialLedger/GetPurchaseOrderRegister',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
				Type: $scope.productNew.Type,
                isClose: $scope.productNew.IsClose
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.PurchaseRegisterLst = response.data;
			for (var i = 0; i < $scope.PurchaseRegisterLst.length; i++) {
				response.data[i].GRNEntryDate = new Date($scope.PurchaseRegisterLst[i].GRNEntryDate);
			}
		});
    };

	$scope.getPurchaseRegisterReport = function () {
		$scope.GetPurchaseRegister();
	}

    $scope.PurchaseOrderReportPdf = function () {

        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }

        var dataList = [];
        var g = $("#GridPrint").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.PurchaseRegisterLst;
        }

        $scope.fileName = 'Purchase Order Register';
       
        $http({
            method: 'POST',
            //url: $scope.path + "StockRegisterReport",
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.PurchaseOrderReportExcel = function () {
        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        var dataList = [];
        var g = $("#GridPrint").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.PurchaseRegisterLst;
        }
        try {
            $scope.fileName = 'Purchase Order Register.xlsx';

            $http({
                method: 'POST',
                url: $scope.path + "GetPurchaseOrderReport",
                data: {'data': dataList,'reportFileName': $scope.fileName,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {

        }
    }

    $scope.productNew.AsOnDate = 'AsOnDate';
    $scope.tab = 1;
    $scope.setTabPR = function (newTab) {
        $scope.tab = newTab;
        //$scope.GetGRN();
    };
    $scope.isSetPR = function (tabNum) {
        return $scope.tab === tabNum;
        //$scope.GRN = 0;
    };
    $scope.setTabPRP = function (newTab) {
        $scope.tab = newTab;
        //$scope.GetGRN();
    };
    $scope.isSetPRP = function (tabNum) {
        return $scope.tab === tabNum;
        //$scope.GRN = 0;
    };




    $scope.report.ToDate = $filter("dateFiltering")(Date.now());
    $scope.productNew.Qty = true;
    $scope.productNew.Inventory = true;
    $scope.change = function (e) {

        $scope.status = e;
        if ($scope.status === 'ForThePeriod') {
            var date = new Date(), y = date.getFullYear(), m = date.getMonth();
            var firstDay = new Date(y, m, 1);
            FromDate: $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1)),
                //$scope.report.FromDate = $filter("dateFiltering")(Date.now());

                $scope.report.FromDate = $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1));
            $scope.report.ToDate = $filter("dateFiltering")(Date.now());
            $scope.productNew.ForThePeriod = 'ForThePeriod';
            //$scope.productNew.Qty = true;
            //$scope.productNew.Amount = false;

        }
        if ($scope.status === 'AsOnDate') {

            $scope.productNew.RcptIssue = '';
            $scope.report.FromDate = '';
            $scope.productNew.AsOnDate = 'AsOnDate';
            //$scope.productNew.Qty = true;
            //$scope.productNew.Amount = false;



        }

	}

	




    //#region ServiceAcknowledgement Register Report


    $scope.ServiceAcknowledgementLst = [];
    //$scope.pivotTableFieldListID = [];
    $scope.GetServiceAckRegister = function () {
        debugger;

        if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        else if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        $http({
            method: 'POST',
            //url: $scope.getSearchListUrl,
            url: 'Materials/MaterialLedger/GetServiceAcknowledgementRegister',
            data: {
                fromDate: $scope.report.FromDate,
                toDate: $scope.report.ToDate,
                Type: $scope.productNew.Type
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ServiceAcknowledgementLst = response.data;

            for (var i = 0; i < $scope.ServiceAcknowledgementLst.length; i++) {
                response.data[i].GRNEntryDate = new Date($scope.ServiceAcknowledgementLst[i].GRNEntryDate);
            }
        });

    };

    $scope.getServiceAcknowledgementRegisterReport = function () {
        $scope.GetServiceAckRegister();
    }


    $scope.ServiceAcknowledgementRegisterReportPdf = function (id, reportFormat) {

        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/ServiceAcknowledgementRegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
    };

    $scope.ServiceAcknowledgementRegisterReportExcel = function (reportFormat) {
        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        try {
            var Excel;
            var file_src = 'Materials/MaterialLedger/ServiceAcknowledgementRegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.WithStock + '&Inventory=' + $scope.productNew.WithoutStock;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }


    //#endregion ServiceAcknowledgement Register Report





}

 

