'use strict';
ShiftRosterCreationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ShiftRosterCreationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Roster Creation';
    $scope.Action = 'Save';

    //#region declaration
    $scope.path = 'Attendances/ShiftRosterCreation/';
    $scope.searchShiftUrl = $scope.path + 'SearchShift';
    $scope.loadRosterUrl = $scope.path + 'LoadRoster';
    $scope.loadRosterChildUrl = $scope.path + 'LoadRosterChild';

    $scope.saveUrl = $scope.path + 'Save';
    $scope.detail = [];
    $scope.master = {
        SystemId: null,
        ShiftRosterName: null,
        ShiftRosterDescription: null,
        ChangeAfterDayLength: 0,
        RosteringPattern: 'IndividualWeekOff',
        WeekDays: null,
        MultiDate: null,
        EffectiveDate: null
    };

    $scope.SelectedShift = function () {
        var gridObj = $("#GridSearchShiftList").data("ejGrid");
        var detail_ob = gridObj.getSelectedRecords()[0]

        // detail_ob.ShiftSequence = baseService.arrayLength($scope.detail) + 1;

        if (checkDoubleShift($scope.detail, detail_ob.ShiftDefinationID) === false) {
            $scope.detail.push(detail_ob);
        }

        var eDialog = $("#dialogShiftInfo").data("ejDialog");//dialogShiftInfo
        eDialog.close();

    }

    $scope.SelectedRoster = function () {
        var gridObj = $("#Griddetail").data("ejGrid");
        var detail_ob = gridObj.getSelectedRecords()[0]
        $scope.master = detail_ob;
        $scope.LoadRosterChild();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    function checkDoubleShift(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ShiftDefinationID === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.SearchShiftList = [];
    $scope.SearchShift = function () {
        try {

            var eDialog = $("#dialogShiftInfo").data("ejDialog");
            eDialog.open();

            $http.get($scope.searchShiftUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.SearchShiftList = response.data.LeaveInfo;
                        //eDialog.close();
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    //#endregion

    $scope.LoadRosterList = [];
    $scope.LoadRoster = function () {
        try {
            $http.get($scope.loadRosterUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LoadRosterList = response.data.Roster;
                        //eDialog.close();
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadRoster();

    $scope.LoadRosterChild = function () {
        try {
            $http.get($scope.loadRosterChildUrl + '?rosterid=' + $scope.master.SystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.detail = response.data.RosterChild;
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function combineBothList(list) {
        angular.forEach(list, function (item, key) {
            item.ShiftSequence = key + 1;

            $scope.savedetail.push(item);
        });
    }

    //#region Save
    $scope.Save = function () {
        try {
            $scope.savedetail = [];
            combineBothList($scope.detail);

            if (baseService.isUndefinedOrNull($scope.master.ShiftRosterName)) {
                throw "'Shift Roster Name' can not be blank...";
            }

            if ($scope.master.RosteringPattern == 'ChangeAfterDayLength') {
                if (baseService.isUndefinedOrNull($scope.master.ChangeAfterDayLength)) {
                    throw "Change After Day can not be blank...";
                }
                if ($scope.master.ChangeAfterDayLength <= 0) {
                    throw "Change After Day can not be 0 or below";
                }
                if (baseService.isUndefinedOrNull($scope.master.EffectiveDate)) {
                    throw "Effective Date can not be blank...";
                }
            }

            if ($scope.master.RosteringPattern == 'WeekDays') {
                if (baseService.isUndefinedOrNull($scope.master.WeekDays)) {
                    throw "Week Days can not be blank...";
                }
            }

            if ($scope.master.RosteringPattern == 'MultiDate') {
                if (baseService.isUndefinedOrNull($scope.master.MultiDate)) {
                    throw "Date not be blank...";
                }
            }

            if ($scope.master.RosteringPattern == 'IndividualWeekOff') {
                $scope.master.ChangeAfterDayLength = null,
                    $scope.master.WeekDays = null,
                    $scope.master.MultiDate = null,
                    $scope.master.EffectiveDate = null
            }
            if ($scope.master.RosteringPattern == 'ChangeAfterDayLength') {
                $scope.master.WeekDays = null,
                    $scope.master.MultiDate = null
            }
            if ($scope.master.RosteringPattern == 'WeekDays') {
                $scope.master.ChangeAfterDayLength = null,
                    $scope.master.EffectiveDate = null,
                $scope.master.MultiDate = null
            }
            if ($scope.master.RosteringPattern == 'MultiDate') {
                $scope.master.ChangeAfterDayLength = null,
                    $scope.master.WeekDays = null,
                    $scope.master.EffectiveDate = null
            }


            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'master': $scope.master, 'detail': $scope.savedetail },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.LoadRosterChild();
                    $scope.LoadRoster();
                    ShowResult(response.data.Message, 'success');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //#endregion

    //Deleting Rows from EarnignSalaryHeadSelectedList
    $scope.DeleteShift = function (data) {
        $scope.DeleteShiftSystemId = data.ShiftDefinationID;
        if (baseService.isUndefinedOrNull(data.ShiftDefinationName))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.ShiftDefinationName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.DeleteRow = function () {
        var tempData = $scope.detail;
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].ShiftDefinationID === $scope.DeleteShiftSystemId) {
                $scope.detail.splice(i, 1);
            }
        }
        $scope.DeleteShiftSystemId = null;
        tempData = [];
    };
    var move = function (origin, destination, list) {
        var temp = $scope[list][destination];
        $scope[list][destination] = $scope[list][origin];
        //$scope[list][ShiftSequence] = $scope[list][origin];
        $scope[list][origin] = temp;

    };
    $scope.moveUp = function (index, list) {
        move(index, index - 1, list);
    };
    $scope.moveDown = function (index, list) {
        move(index, index + 1, list);
    };
    //#region TBD
    $scope.Delete = function () {
        try {

            var gridObj = $("#GridLeaveEncashmentList").data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];

            if (data.Isdisburse == true) {
                throw "This Encashment had already been disbursed ";
            }

            $http({
                method: 'POST',
                url: $scope.deleteLvEncashmentUrl,
                data: { 'leaveEncashmentId': data.Id, 'EmpSystemId': data.EmpSystemId, 'EncashmentDate': data.EncashmentDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadLeaveEncashmentList();
                    $scope.btnSave = false;

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };
    $scope.message_confirmation = null;
    $scope.remove = function (obj) {
        $scope.message_confirmation = 'Are you sure to Delete This leave Encashmen ?';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    };

    $scope.Clear = function (obj) {
        ClearFields(obj);
        $scope.master.ChangeAfterDayLength = 7;
        $scope.detail = [];
        $scope.master = {
            SystemId: null,
            ShiftRosterName: null,
            ShiftRosterDescription: null,
            ChangeAfterDayLength: 0,
            RosteringPattern: 'IndividualWeekOff',
            WeekDays: null,
            MultiDate: null
        };
    };
    function ClearFields(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }

    $scope.ChangeAfterDay = function () {
        $scope.master.ChangeAfterDayLength = 7;
        $scope.master.WeekDays = null,
            $scope.master.MultiDate = null
    }

    $scope.Change = function () {
        $scope.master.ChangeAfterDayLength = null,
            $scope.master.EffectiveDate = null,
            $scope.master.WeekDays = null,
            $scope.master.MultiDate = null
    }

    $scope.changeWeeklyon = function () {
        $scope.master.ChangeAfterDayLength = null,
            $scope.master.EffectiveDate = null,
            $scope.master.MultiDate = null
    }

    $scope.changeSpecific = function () {
        $scope.master.ChangeAfterDayLength = null,
            $scope.master.EffectiveDate = null,
            $scope.master.WeekDays = null
    }

};