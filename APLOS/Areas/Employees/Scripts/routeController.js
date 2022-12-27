'use strict';
routeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function routeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Route';
    $scope.Action = 'Save';
    $scope.ActionTransaction = 'Save';
    $scope.ActionRouteShd = 'Save';
    $scope.ActionRouteShdChild = 'Save';
    $scope.ActionStoppage = 'Save';
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
        Totalkm: 0,
        From: null,
        To: null,
    };
    $scope.routeNew = Object.assign({}, $scope.route);

    $scope.transport = {
        Id: null,
        TransportCategory: null,
        TransportUserName: null,
        TransportNo: null,
        TransportPort: null,
        Capacity: 0,
        PlanCapacity: 0,
        DriverName: null,
        SpeedPerkm: 0,
        Remarks: null
    };
    $scope.ModelChildNew = Object.assign({}, $scope.transport);

    $scope.schedule = {
        Id: null,
        TransportId: null,
        Transport: null,
        RouteId: null,
        Route: null,
        TripNo: null,
        UpDown: null,
        ShiftId: null,
        Shift: null,
        StartTime: null,
        EndTime: null,
        Remarks: null
    };
    $scope.ModelRouteSchedule = Object.assign({}, $scope.schedule);

    $scope.RouteShd = {
        Id: null,
        RouteScheduleId: null,
        StartTime: null,
        EndTime: null,
        UpDown: null,
        Remarks: null,
    };
    $scope.RouteSheduleChildModel = Object.assign({}, $scope.RouteShd);

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Save = function () {
        try {
            ValidationMasterRoute();

            var StopageList = [];
            var ob = {};
            for (var i = 0; i < $scope.StopageListNew.length; i++) {
                ob.Id = null;
                ob.RouteId = $scope.routeNew.Id;
                ob.StoppageId = $scope.StopageListNew[i].StopagePrimaryId;
                ob.UpDistanceFrom = $scope.StopageListNew[i].UpDistanceFrom;
                ob.DownDistanceFrom = $scope.StopageListNew[i].DownDistanceFrom;
                StopageList.push(ob);
                ob = {};
            }
            if (StopageList.length == 0) {
                throw "Please Select Stopage";
            }
            $scope.$broadcast('show-errors-check-validity');
            //if ($scope.routeNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.routeNew, 'StopageList': StopageList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Action = 'Update';
                        $scope.routeNew.Id = response.data.Route.Id;
                       $scope.getData();
                        $scope.getRouteStopageData();
                        //if ($rootScope.isCollapsed) {
                        //    $rootScope.toggle();
                        //}
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
            data: { 'data': $scope.ModelChildNew },
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
       
        $http({
            method: 'POST',
            url: $scope.saveRouteSchedule,
            data: { 'data': $scope.ModelRouteSchedule},
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
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    // --Start Route Shedule Child Details--

    $scope.GetRouteSheduleChildData = function (index) {
        $scope.RouteShdChId = index;
        $scope.RouteSheduleChildModel = Object.assign({}, $scope.RouteShd);
        $scope.GetRouteSheduleChild();
        //$scope.GetArticleAliasDatas();
        angular.element(document.querySelector('#RouteScheduleChilPopUp')).modal('show');
    };

    $scope.SaveRouteSheduleChild = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'CreateRouteSheduleChildDetails',
            data: { 'RouteShChild': $scope.RouteSheduleChildModel, 'RouteScheduleId': $scope.RouteShdChId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetRouteSheduleChild();
                $scope.RouteSheduleChildClear();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.RouteScheduleChildList = [];
    $scope.GetRouteSheduleChild = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetRouteScheduleChilddata?tripId=' + $scope.RouteShdChId ,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RouteScheduleChildList = response.data;

        });
    }
   

    $scope.GetRouteScheduleChilddbl = function (args) {

        $scope.RouteSheduleChildModel = Object.assign({}, args);
        $scope.GetRouteSheduleChild(args.Id);
        $scope.ActionRouteShdChild = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.RouteSheduleChildClear = function () {

        $scope.RouteSheduleChildModel = Object.assign({}, $scope.RouteShd);
        $scope.ActionRouteShdChild = 'Save';
    }

    $scope.deleteRouteScheduleChildList = function (RouteShdChId) {
        try {
            $scope.RouteShdChId = RouteShdChId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmRouteScheduleChildRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeRouteScheduleChildRow = function () {
        $http({
            method: 'POST',
            url: 'employees/route/RouteScheduleChildDelete',
            data: { 'Id': $scope.RouteShdChId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetRouteSheduleChild();
                $scope.RouteSheduleChildClear();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

     // --End Route Shedule Child Details--

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMasterRoute() {
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
            //if (baseService.isUndefinedOrNull($scope.routeNew.Id)) {
            //    throw "Select Route.";
            //}
            $http({
                method: 'GET',
                url: 'employees/route/GetStopageInformation'
            }).then(function successCallback(response) {
                $scope.StopageList = response.data;
            });
            angular.element(document.querySelector('#StoppageInfo')).modal('show');
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

            //var eDialog = $("#StoppageInfo").data("ejDialog");
            //eDialog.close();
            angular.element(document.querySelector('#StoppageInfo')).modal('hide');
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



    $scope.ClearMain = function () {
        $scope.routeNew = Object.assign({}, $scope.route);
        $scope.StopageListNew = [];
        $scope.Action = 'Save';
    };
   

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
            //data: { 'RouteId': $scope.routeNew.Id },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.transportDetailsList = resp.data;
        });
    }

    $scope.TransportDetailsdoubleclick = function (args) {
        $scope.ModelChildNew = Object.assign({}, args);
        $scope.ActionTransaction = 'Update';
    };

    $scope.ClearTransDetails = function () {
        $scope.ModelChildNew = Object.assign({}, $scope.transport);
        $scope.ActionTransaction = 'Save';
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
            url: 'employees/route/TransportDetailsDelete',
            data: { 'id': $scope.tempId},
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

    $scope.RouteList = [];
        $scope.selectRoute = function () {
            $http({
                method: 'GET',
                url: $scope.path + 'GetRoute',
                dataType: 'JSON'
            }).then(function succ(resp) {
                $scope.RouteList = resp.data;
            });
        }
        $scope.selectRoute();

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
            //data: { 'RouteId': $scope.routeNew.Id },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.routeScheduleList = resp.data;
        });
    }
    $scope.getRouteScheduleMaster();

    $scope.routeScheduledoubleclick = function (args) {
        $scope.ModelRouteSchedule = Object.assign({}, args);
        $scope.GetRouteScheduleTransport(args.Id);
        $scope.ActionRouteShd = 'Update';
    };
    $scope.ClearRouteSchedule = function () {
        $scope.ModelRouteSchedule = Object.assign({}, $scope.schedule);
        $scope.ActionRouteShd = 'Save';
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

    //Stoppage Start By om@r

    
    $scope.index = -1;
    $scope.path2 = 'employees/Stoppage/';
    $scope.getSeqUrl = 'employees/Stoppage/getautosequence';

    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.companyList = [];
    $scope.getCompany = function () {
        $http.get('employees/Stoppage/GetCompany')
            .then(function (response) {
                $scope.companyList = response.data;

            });
    };
    $scope.getCompany();

    $scope.cityList = [];
    $scope.getCityList = function () {
        $http.get('employees/Stoppage/GetCity?CompanyId=' + $scope.stoppageNew.CompanyId)
            .then(function (response) {
                $scope.cityList = response.data;
            });
    };


    $scope.ModelStoppageList = [];
    $scope.getData = function () {
        $scope.stoppageNew = Object.assign({}, $scope.stoppage);
        $scope.ModelStoppageList = [];
        $http.get('employees/Stoppage/getlist')
            .then(function (response) {
                $scope.ModelStoppageList = response.data;

            });
    };
    $scope.getData();

    $scope.recorddoubleclicks = function (args) {
        try {
            $scope.ActionStoppage = 'Update';
            $scope.stoppageNew = Object.assign({}, args.data);
            //var gridObj = $("#GridEdit").data("ejGrid");
            //$scope.stoppageNew = gridObj.getSelectedRecords()[0];
            $scope.getCityList();
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.stoppage = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        CityId: null,
        Remarks: null,
        Active: true
    };
    $scope.stoppageNew = Object.assign({}, $scope.stoppage);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.stoppageNew.Sequence = data;
        })
    };
    $scope.GetSequence();

    $scope.SaveStooppage = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.stoppageNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: 'employees/Stoppage/Save',
                    data: $scope.stoppageNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields($scope.GetSequence());
                        $scope.getData();
                        $scope.stoppageNew.Active = true;
                        //$scope.companyList = [];
                        $scope.cityList = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.removeStoppageModal = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmPopUps')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.DeleteStoppage = function () {
        if (!baseService.isUndefinedOrNull($scope.stoppageNew.Id)) {
            $http.get('employees/Stoppage/Delete?Id=' + $scope.stoppageNew.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.stoppageNew = Object.assign({}, $scope.stoppage);
                        ClearFields($scope.GetSequence());
                        $scope.getData();
                        $scope.stoppageNew.Active = true;
                        //$scope.companyList = [];
                        $scope.cityList = [];
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.stoppage = {};
        $scope.stoppageNew = {};
        $scope.stoppageNew.Sequence = seq;
        $scope.stoppageNew.Active = true;
    }

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
            CheckField("Code", $scope.stoppageNew.Code);
            CheckField("Short Name", $scope.stoppageNew.ShortName);
            CheckField("Standard Name", $scope.stoppageNew.StandardName);
            CheckField("User Name", $scope.stoppageNew.UserName);
            CheckField("City", $scope.stoppageNew.CityId);

        } catch (ex) {
            throw ex;
        }
    };

   //Stoppage End By om@r
}

