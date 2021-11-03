 'use strict';
myAppCalendarController.$inject = ['uiCalendarConfig', '$uibModal', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$cookies'];
function myAppCalendarController(uiCalendarConfig, $uibModal,fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $cookies) {
    $scope.eventsTab = [];
    $scope.events = [$scope.eventsTab];

    $scope.EventObj = {};

    //Clear calendar  
    function clearCalendar() {
        if (uiCalendarConfig.calendars.Calendar != null) {
            uiCalendarConfig.calendars.Calendar.fullCalendar('removeEvents');
        }
    }

    //Gets all events from db  
    //function GetEvents() {

    //    clearCalendar();

    //    $http.get('/api/Calendar/GetEvents', {
    //        cache: false,
    //        params: {},
    //    }).then(function (response) {

    //        angular.forEach(response.data, function (value) {

    //            $scope.eventsTab.push({
    //                id: value.EventID,
    //                title: value.EventTitle,
    //                description: value.EventDescription,
    //                start: new Date(value.StartDate),
    //                end: new Date(value.EndDate),
    //                backgroundColor: "#f9a712",
    //                borderColor: "#8e8574"
    //            });

    //            console.log($scope.eventsTab);

    //        });
    //    });
    //}

    //GetEvents();


    //Configure Calendar  
    $scope.uiConfig = {
        calendar: {
            height: 450,
            editable: true,
            displayEventTime: true,
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,agendaWeek,agendaDay'
            },
            selectable: true,
            select: function (start, end) {
                var startDate = moment(start).format('YYYY/MM/DD');
                var endDate = moment(end).format('YYYY/MM/DD');

                $scope.EventObj = {
                    EventID: 0,
                    EventTitle: '',
                    EventDescription: '',
                    StartDate: startDate,
                    EndDate: endDate
                };

                $scope.ShowModal();

            },
            eventClick: function (event) {

                var startDate = moment(event.start).format('YYYY/MM/DD');
                var endDate = moment(event.end).format('YYYY/MM/DD');

                $scope.EventObj = {
                    EventID: event.id,
                    EventTitle: event.title,
                    EventDescription: event.description,
                    StartDate: startDate,
                    EndDate: endDate
                };

                $scope.ShowModal();
            }

        }

    };

    // Popup modal  
    $scope.ShowModal = function () {

        var modalInstance = $uibModal.open({
            templateUrl: 'modalPopUp.html',
            controller: 'modalCtrl',
            backdrop: 'static',
            resolve: {
                EventObj: function () {
                    return $scope.EventObj;
                }
            }
        });

        modalInstance.result.then(function (result) {

            switch (result.operation) {

                case 'AddOrUpdate':
                    $http({
                        method: 'POST',
                        url: '/api/Calendar/PostSaveOrUpdate',
                        data: $scope.EventObj
                    }).then(function (response) {
                        console.log("Added ^_^");
                        GetEvents();

                    }, function errorRollBak() {
                        console.log("Something Wrong !!");
                    });
                    break;

                case 'Delete':
                    $http({
                        method: 'DELETE',
                        url: '/api/Calendar/DeleteEvent/' + $scope.EventObj.EventID

                    }).then(function (response) {

                        GetEvents();

                    }, function errorRollBack() {

                        console.log("Something Wrong !!");

                    });


                    break;

                default:
                    break;
            }

        }, function () {
            // $log.info('modal-component dismissed at: ' + new Date());
        })

    }
}