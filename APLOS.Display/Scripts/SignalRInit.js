SignalRInit.$inject = ["$rootScope", "$cookies", "$window", "$location", "$filter", "baseService", "$http", '$window', '$timeout', 'signalR'];
function SignalRInit($rootScope, $cookies, $window, $location, $filter, baseService, $http, $window, $timeout, signalR) {
    //this piece of shi*t has been implemented by
    //tarek talukder
    //tarektalukder@gmail.com
    //if you have any query, please don't call me
    var factory = {};
    var UserId = $window.UserRoleUserId + $window.plantId;
    factory.connect = function () {

        
        $http({
            method: 'GET', url: 'UPanel/NotificationURL', dataType: 'JSON'
        }).then(function successCallback(response) {
            try {
                if (angular.isUndefinedOrNull(response.data))
                    throw 'Notification location not found';

                if (response.data.PlantId == '')
                    throw 'Plant not found';

                

                UserId = response.data.PlantId + response.data.UserId;
                signalR.connection.url = response.data.URL;
                signalR.connection.qs = { 'UserToken': UserId };
                signalR.Hub = signalR.connection.createHubProxy('aplosbroadcasthub')
                signalR.EmployeeID = UserId;

                signalR.Hub.on("GetProgressNotification", function (Message) {

                    $rootScope.NotificationMessage = Message;
                });

                signalR.StartSignalR().then(function () {
                    if (signalR.isInitialized) {

                    }
                    else {

                    }
                });
            } catch (e) {

            }

        }, function errorCallback(response) {
        });

        factory.IsConnectedToHub = false;
        factory.IsConnectedToHubCheck = function () {
            $timeout(function () {
                factory.IsConnectedToHub = signalR.isInitialized;
                factory.IsConnectedToHubCheck();
            }, 20000);
        }
        factory.IsConnectedToHubCheck();
    }
    return factory;
}