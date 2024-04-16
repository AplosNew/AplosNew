'use strict';
RequisitionStatusController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function RequisitionStatusController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
	$rootScope.title = "Requisition Status";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Materials/RequisitionStatus/';
	$scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
	$scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

	$scope.downloadgriddataUrl = 'GridReports/Download';
	$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	$scope.RowColor = "";
	$scope.isAlternative = -1;
	$controller("employeeBaseController", { $scope: $scope, $http: $http });

	$scope.Employee = "AllEmployee";

	$scope.showEmployeeListPopUp = function () {
		baseService.setCurrentPage('employeeList');
		$scope.getEmployeeData = function (pageno) {
			var url = null;
			if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
				url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
			}
			else {
				url = $scope.employeeUrl;
			}
			baseService.paginationBase(url, pageno, $scope.employeeParameters)
				.then(function (result) {
					$scope.employeeList = result.Rows;
					$scope.employeeParameters.total_count = result.Total;
				}, function () {
					ShowResult(commonMessage.NetworkError, 'failure');
				}).finally(function () {
				});
		};
		angular.element(document.querySelector('#employeePopUp')).modal('show');
		$scope.getEmployeeData();
	};
	
	$scope.EmployeeId = null,
	$scope.EmployeeName = null,

	$scope.closeEmployeePopUp = function () {
		if ($scope.employeeIndex !== -1) {
			var employee = $scope.employeeList[$scope.employeeIndex];
			$scope.EmployeeId = employee.SystemId;
			$scope.EmployeeName = employee.EmployeeName;
		}
		$scope.hideEmployeePopUp();
	};
	$scope.hideEmployeePopUp = function () {
		angular.element(document.querySelector("#employeePopUp")).modal("hide");
	};


	$scope.EmployeeId = null;
	$scope.RequisitionFromDate = null;
	$scope.RequisitionToDate = null;
	$scope.RequisitionStatus = null;
	$scope.RequisitionStatusList = [];
	$scope.GetRequisitionStatus = function () {
		
		if ($scope.RequisitionFromDate === null || $scope.RequisitionFromDate === "") {
			ShowResult('Select Requisition From Date', 'failure');
			return false;
		}
		if ($scope.RequisitionToDate === null || $scope.RequisitionToDate === "") {
			ShowResult('Select Requisition To Date', 'failure');
			return false;
		}
		$http({
			method: 'POST',
			url: 'Materials/StockRegister/RequisitionStatusData',
			data: {
				employeeId: $scope.EmployeeId,
				requisitionFromDate: $scope.RequisitionFromDate,
				requisitionToDate: $scope.RequisitionToDate,
				requisitionStatus: $scope.RequisitionStatus,
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.RequisitionStatusList = response.data.NewData;
		});
	};

	$scope.RequisitionStatusReportExcel = function () {
		

		var dataList = [];
		var g = $("#GridRequisitionStatus").data("ejGrid");
		dataList = g.getFilteredRecords();

		if (dataList.length == 0)
		{
			dataList = $scope.RequisitionStatusList;
        }

		$scope.fileName = 'Requisition Status';

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
	}

	$scope.clearAllEmployee = function () {
		$scope.RequisitionStatusList = [];
		$scope.EmployeeId = null;
		$scope.RequisitionBeforeDate = null;
		$scope.RequisitionStatus = null;
	};

	$scope.clearSingleEmployee = function () {
		$scope.RequisitionStatusList = [];
	};
}
   

