salesManagementService.$inject = ['$http'];
function salesManagementService($http) {
    var service = {
        getBudgetMasterCboList: getBudgetMasterCboList
    };

    function base(url, callback) {
        $http.get(url)
            .then(function successCallback(response) {
                callback(response.data);
            }, function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }
    function getBudgetMasterCboList(glId, callback) {
        base('Accounts/BudgetMaster/GetBudgetMasterCboList?glId=' + glId, callback);
    }
    return service;
}