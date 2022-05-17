'use strict';
routeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function routeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Route';
    $scope.Action = 'Save';
    $scope.path = 'employees/route/';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.saveChildUrl = $scope.path + 'CreateChild';
    $scope.saveRouteSchedule = $scope.path + 'CreateRouteSchedule';

    $scope.route = {
        Id: null,
        //DriverId: null,
        //AssetId: null,
        Code: null,
        UserName: null,
        StandardName: null,
        ShortName: null,
        Description: null,
        Remarks: null,
        UpDistanceFrom: null,
        DownDistanceFrom: null,
        Active: true,
        UpOrDown: 'Up',      
    };
    $scope.routeNew = Object.assign({}, $scope.route);

    $scope.transport = {
        Id: null,
        TransportCategory: null,
        TransportUserName: null,
        TransportNo: null,
        TransportPort: null,
        Capacity: 0,
        DriverId: null,
        DriverCode: null,
        DriverName: null,
        Remarks: null
    };
    $scope.ModelChildNew = Object.assign({}, $scope.transport);

    $scope.schedule = {
        Id: null,
        TransportId: null,
        TripNo: null,
        UpDown: null,
        ShiftId: null,
        Shift: null,
        StartTime: null,
        EndTime: null,
        From: null,
        To: null,
        Distance: null,
        DistancePerUnit: null,
        Remarks: null
    };
    $scope.ModelRouteSchedule = Object.assign({}, $scope.schedule);

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Save = function () {
        try {
            ValidationMaster();

            var StopageList = [];
            for (var i = 0; i < $scope.StopageListNew.length; i++) {
                StopageList.push($scope.StopageListNew[i]);
            }
            if (StopageList.length == 0) {
                throw "Please Select Stopage";
            }
            $scope.$broadcast('show-errors-check-validity');
            //if ($scope.routeNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'Route': $scope.routeNew, 'StopageList': StopageList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Action = 'Update';
                       $scope.getData();
                        $scope.getRouteStopageData();
                        if ($rootScope.isCollapsed) {
                            $rootScope.toggle();
                        }
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            //}
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveChild = function () {
        $http({
            method: 'POST',
            url: $scope.saveChildUrl,
            data: { 'data': $scope.ModelChildNew, 'RouteId': $scope.routeNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getTransportDetailsMaster();
                $scope.ClearTransDetails();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    $scope.SaveRouteSchedule = function () {
        var DropDownListObj = $("#transportList").data("ejDropDownList");
        var dayStatus = DropDownListObj.getSelectedValue();
        $scope.transportId = dayStatus;

        $http({
            method: 'POST',
            url: $scope.saveRouteSchedule,
            data: { 'data': $scope.ModelRouteSchedule, 'RouteId': $scope.routeNew.Id, 'transportId': $scope.transportId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getRouteScheduleMaster();
                $scope.ClearRouteSchedule();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            CheckField("Code", $scope.routeNew.Code);
            CheckField("Short Name", $scope.routeNew.ShortName);
            CheckField("Standard Name", $scope.routeNew.StandardName);
            CheckField("User Name", $scope.routeNew.UserName);           
        } catch (ex) {
            throw ex;
        }
    };

    $scope.ModelList = [];
    $scope.getData = function () {
       // $scope.routeNew = Object.assign({}, $scope.route);
        $scope.ModelList = [];
        $http.get('employees/route/getlist')
            .then(function (response) {
                $scope.ModelList = response.data;

            });
    };
    $scope.getData();

    $scope.StopageListNew = [];
    $scope.getRouteStopageData = function (args) {
        $scope.routeNew = Object.assign({}, args.data);
        $http.get('employees/route/getRouteStopage?RouteId=' + $scope.routeNew.Id)
            .then(function (response) {
                $scope.StopageListNew = response.data;
            });
    };

    $scope.recorddoubleclick = function (args) {
        $scope.routeNew = Object.assign({}, args.data);
        $scope.getTransportDetailsMaster();
        $scope.getRouteScheduleMaster();
        try {
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
            $scope.getRouteStopageData(args);
        } catch (e) {

        }
    };

    $scope.StopageList = [];
    $scope.GetStopageInformation = function () {
        try {
            var eDialog = $("#StoppageInfo").data("ejDialog");
            eDialog.open();

            $http({
                method: 'GET',
                url: 'employees/route/GetStopageInformation'
            }).then(function successCallback(response) {
                $scope.StopageList = response.data;
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    var move = function (origin, destination, list) {
        var temp = $scope[list][destination];
        $scope[list][destination] = $scope[list][origin];
        $scope[list][origin] = temp;

    };
    $scope.moveUp = function (index, list) {
        move(index, index - 1, list);
    };
    $scope.moveDown = function (index, list) {
        move(index, index + 1, list);
    };
    $scope.DeleteStopage = function (data) {
        $scope.StopagePrimaryId = data.StopagePrimaryId;
        if (baseService.isUndefinedOrNull(data.UserName))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.UserName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.DeleteRow = function () {
        var tempData = $scope.StopageListNew;
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].StopagePrimaryId === $scope.StopagePrimaryId) {
                $scope.StopageListNew.splice(i, 1);
            }
        }
        $scope.Id = null;
        tempData = [];
    };

    $scope.StopageListNew = [];
    $scope.OK = function () {
        try {

            for (var i = 0; i < $scope.StopageList.length; i++) {
                if ($scope.StopageList[i].CheckBoxSelect == true) {
                    if (checkDoubleStopageInformation($scope.StopageListNew, $scope.StopageList[i].StopagePrimaryId) === false) {
                        $scope.StopageListNew.push($scope.StopageList[i]);
                    }
                }
            }

            var eDialog = $("#StoppageInfo").data("ejDialog");
            eDialog.close();

            if ($rootScope.isCollapsed) {
                $rootScope.toggle();
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    function checkDoubleStopageInformation(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].StopagePrimaryId === Id) {
                return true;
            }
        }
        return false;
    }



    $scope.Clear = function (obj) {
        ClearFields();
    };
    function ClearFields() { 
        $scope.routeNew = Object.assign({}, $scope.route);
        $scope.StopageListNew = [];
        $scope.transportDetailsList = [];
        $scope.routeScheduleList = [];
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.routeNew.Id)) {
            $http.get('employees/route/Delete?Id=' + $scope.routeNew.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Clear();
                        $scope.getData();
                        $scope.getRouteStopageData();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.transportDetailsList = [];
    $scope.getTransportDetailsMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetTransportDetails',
            data: { 'RouteId': $scope.routeNew.Id },
            dataType: 'JSON'
        }).then(function succ(resp) {
            //$scope.transportDetailsList = [];
            $scope.transportDetailsList = resp.data;
        });
    }

    $scope.TransportDetailsdoubleclick = function (args) {
        $scope.ModelChildNew = Object.assign({}, args);
    };

    $scope.ClearTransDetails = function () {
        $scope.ModelChildNew = Object.assign({}, $scope.transport);
    }

    $scope.removeTransportDetailsRowModal = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmTransportDetailsRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeTransportDetailsRow = function () {
        $http({
            method: 'POST',
            url: 'employees/route/TransportDetailsDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getTransportDetailsMaster();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.selectShift = function () {
        $scope.getsS();
        angular.element(document.querySelector('#ShiftPop')).modal('show');
    }

    $scope.ShiftList = [];
    $scope.getsS = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getShift',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }

    $scope.doubleShift = function (e) {
        $scope.ModelRouteSchedule.ShiftId = e.data.ShiftId;
        $scope.ModelRouteSchedule.Shift = e.data.ShiftDefination;
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.transportList = [];
    $scope.getTransport = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetTransport',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.transportList = resp.data;
        });
    }
    $scope.getTransport();

    $scope.routeScheduleList = [];
    $scope.getRouteScheduleMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetRouteSchedule',
            data: { 'RouteId': $scope.routeNew.Id },
            dataType: 'JSON'
        }).then(function succ(resp) {
            //$scope.routeScheduleList = [];
            $scope.routeScheduleList = resp.data;
        });
    }
    $scope.getRouteScheduleMaster();

    $scope.routeScheduledoubleclick = function (args) {
        $scope.ModelRouteSchedule = Object.assign({}, args);
        var DropDownListObj = $("#transportList").data("ejDropDownList");
        DropDownListObj.uncheckAll();
        $scope.GetRouteScheduleTransport(args.Id);
        $scope.getDistance();
    };
    $scope.ClearRouteSchedule = function () {
        $scope.ModelRouteSchedule = Object.assign({}, $scope.schedule);
    }

    $scope.removeRouteScheduleRowModal = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmRouteScheduleRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeRouteScheduleRow = function () {
        $http({
            method: 'POST',
            url: 'employees/route/RouteScheduleDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ClearRouteSchedule();
                $scope.getRouteScheduleMaster();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

   
    $scope.routeScheduleTransportList = [];
    $scope.GetRouteScheduleTransport = function (routeScheduleId) {
        $scope.routeScheduleTransportList = [];
        $http.get("employees/route/GetRouteScheduleTransport?routeScheduleId=" + routeScheduleId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.routeScheduleTransportList = response.data;

                        var DropDownListObj = $("#transportList").data("ejDropDownList");
                        for (var j = 0; j < $scope.routeScheduleTransportList.length; j++) {
                            DropDownListObj.selectItemByValue($scope.routeScheduleTransportList[j].routeScheduleTransportId);
                        }

                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.getDistance = function () {
        try {
            $scope.DistanceUrl = 'employees/route/GetDistance/'
            $http({
                method: 'POST',
                url: $scope.DistanceUrl,
                data: { 'data': $scope.ModelRouteSchedule },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.ModelRouteSchedule.DistancePerUnit = response.data;
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}