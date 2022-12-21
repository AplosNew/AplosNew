'use strict';
OutPassRegisterController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function OutPassRegisterController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

	// Written By Nitesh

	$scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

	$scope.fileName = "GatePass.xlsx";
	$scope.GateAgainstGatePassExl = function () {
		debugger
		$http({
			method: 'POST',
			url: 'Products/OutPassRegister/GateAgainstGatePassExl',
			dataType: 'JSON',
		})
			.then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {

					$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
				}
			}, function errorCallback(response) {
				ShowResult(response.data.Message, 'failure');
			});

	};
}