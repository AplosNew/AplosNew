signalR.$inject = ['$http', '$window', '$timeout'];
function signalR($http, $window, $timeout) {
    //this piece of shi*t has been implemented by
    //tarek talukder
    //tarektalukder@gmail.com
    //if you have any query, please don't call me
    var factory = {};
    const states = {
        connecting: 0, connected: 1, reconnecting: 2, disconnected: 4
    }
    factory.EmployeeID = '';
    factory.tryingToReconnect = false;
    factory.isInitialized = false;
    factory.isConnecting = false;
    factory.connection = null;

    factory.connection = $.hubConnection('./signalr', {
        useDefaultPath: false

    });
    factory.connection.url = "";
    factory.connection.qs = { 'UserToken': '' };
    factory.Hub = null; 


    $(factory.connection).bind("onStateChanged", function (e, data) {
        if (data.newState == states.disconnected) {
            if (factory.isConnecting == false) {
                factory.isConnecting = true;
                factory.StartSignalR();
            }
        }
    });

    factory.ConnectUser = function () {
        try {
            factory.Hub.invoke("connect", factory.EmployeeID);
        } catch (e) {

        }


    }

    factory.DisconnectUser = function () {
        try {
            factory.Hub.invoke("disconnect", factory.EmployeeID);

        } catch (e) {

        }

    }


    factory.StartSignalR = function () {

        return new Promise(function (resolve, reject) {
            try {


                factory.DisconnectUser();
            } catch (e) {

            }

            try {


                factory.connection.start(/*{ transport: ['webSockets'] }*/)
                    .done(function () {

                        factory.isInitialized = true;
                        factory.isConnecting = false;
                        resolve();
                    })
                    .fail(function (errormessage) {

                        try {
                            factory.isInitialized = false;
                            factory.isConnecting = true;
                            $timeout(function () {
                                factory.StartSignalR();
                            }, 10000);

                            reject();
                        } catch (e) {

                        }
                       
                    });
            } catch (e) {

            }
        });
    }

    factory.StopSignalR = function () {
        try {

            factory.connection.hub.stop();

        } catch (e) {

        }
    }
    factory.Reconnect = function (args) {
        try {
            if (factory.connection.state == states.disconnected) {
                factory.StartSignalR().then(function () {
                    if (!signalR.isInitialized) {
                        factory.Reconnect();
                    }
                });
            }
        } catch (e) {

        }
    }


    return factory;
}