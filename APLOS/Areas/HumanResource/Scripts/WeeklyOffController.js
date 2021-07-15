'use strict';
WeeklyOffController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WeeklyOffController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Weekly Off';
    $rootScope.title1 = 'Weekly Off';
    $scope.Action = 'Save';
    var url = "humanresource/WeeklyOff/";

    $scope.Action = "Save";
    $scope.Action1 = "Save";

    var headerId = document.getElementById("WeekOffId");


    //The Day And Day Type
    $scope.DayList = { 0: "Monday" ,
     1: "Tuesday" ,
     2: "Wednesday" ,
     3: "Thursday",
     4: "Friday" ,
     5: "Saturday" ,
     6: "Sunday" };

    $scope.DayTypeList = ["WD (Working Day)","NWD (Non Working Day)"];

    //// The Region For Roster Processing (Not to be included in this code Just for Testing)
    $scope.FromDateF;
    $scope.ToDateF;
    $scope.Run = function () {
        $http({
            method: 'GET',
            url: url + 'run',
        }).then(function success(response) {
            console.log("Done!!");
        });
    }

    // The End Region


    //The Header Modal
    $scope.Header = {
        Id: null,
        StandardName: null,
        ShortName: null,
        Description: null,
        Remarks: null,
        Active: false,
        UserName: null,
    }


    //Get The Main Master Grid
    $scope.masterGrid = [];
    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: url + 'getMaster'
        }).then(function success(response) {
            $scope.masterGrid = response.data;
        })
    }

    $scope.getMaster();
    var restoreShiftsChild = [];
    //Double Click on Master
    $scope.fillUpdates = function (e) {
        $scope.Header = e.data;
        $http({
            method: 'GET',
            url: url + 'getChilds',
            params: { 'Id': e.data.Id },
        }).then(function success(response) {
            if ($rootScope.isCollapsed == false) {
                $rootScope.toggle();
            }
            headerId.style.display = "block";
            $scope.DatesList = response.data.Dates;
            $scope.Action = "Update";
            $scope.Action1 = "Update";

            $scope.WeekChildList = response.data.Days;
            restoreShiftsChild = $scope.WeekChildList.length;
            var ll = $scope.WeekChildList.length;
            $scope.Sequences = ll;
        })
    }



    //Shifts List Modal
    $scope.WeekChildList = [];
    $scope.AddDay = function () {
        if ($scope.Header.Id == null || $scope.Header.Id == undefined) {
            ShowResult("Please First Save the Master!!", 'failure');
            throw ("Invalid");
        }
        var obj = {
            Id: null,
            WOHeaderId: null,
            WOSequence: 0,
            Day: null,
            DayType: null
        };
        $scope.Sequences++;
        obj.WOSequence = $scope.Sequences;
        obj.WOHeaderId = $scope.Header.Id;
        $scope.WeekChildList.push(obj);
        //refresh();
    }

    //Double Click Inside Shift Modal
    $scope.WeekChildList = [];
    $scope.Sequences = 0;
    

    function refresh() {
        var gridObj = $("#WeekList").data("ejGrid");
        gridObj.dataSource($scope.WeekChildList);
    }
    //The Add Tile in the Shifts Child Grids
    $scope.AddTile = function (e) {
        console.log(e);
        var obj = {
            Id: null,
            WOHeaderId: null,
            WOSequence: 0,
            Day: null,
            DayType: null
        };

        $scope.Sequences++;
        obj.WOHeaderId = e.WOHeaderId;
        obj.WOSequence = $scope.Sequences;
        obj.Day = e.Day;
        obj.DayType = e.DayType;
        $scope.WeekChildList.push(obj);
    }


    

    //Delete Tile in the Shifts Child Grids
    $scope.DeleteTile = function (e) {
        for (var i = 0; i < $scope.WeekChildList.length; i++) {
            if ($scope.WeekChildList[i].WOSequence === e.WOSequence) {
                $scope.WeekChildList.splice(i, 1);
            }
        }


    }


    


    //Seletion of Executive Dates
    $scope.EffectiveDate;
    $scope.DatesList = [];
    $scope.AddDates = function () {
        var c = 0;
        for (var i = 0; i < $scope.DatesList.length; i++) {
            if ($scope.DatesList[i].EffectiveDate === $scope.EffectiveDate) {
                c++;
            }
        }
        if (c === 0) {
            if (($scope.EffectiveDate + '').length < 21 && ($scope.EffectiveDate + '').length > 5) {

                $scope.DatesList.push({ Id: null, WOHeaderId: null, EffectiveDate: $scope.EffectiveDate });
            }
        }
    }

    //Delete The Date
    $scope.DeleteDate = function (e) {
        for (var i = 0; i < $scope.DatesList.length; i++) {
            if ($scope.DatesList[i].EffectiveDate === e) {
                $scope.DatesList.splice(i, 1);
            }
        }
    }

    //Save Master Data and Effective Dates Child
    $scope.SaveMasters = function () {
        $scope.$broadcast('show-errors-check-validity');

        validationsMaster();

        $http({
            method: 'POST',
            url: url + "saveMasters",
            data: { 'Master': $scope.Header, 'Effective': $scope.DatesList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                console.log(response.data);
                $scope.Header.Id = response.data.ids;
                headerId.style.display = "block";
                $scope.Action = "Update";
                $scope.getMaster();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

    //Delete Master
    //$scope.Delete = function () {
    //    if (restoreShiftsChild > 0) {
    //        ShowResult("There are Child Data in this Master. First Delete Those!", 'failure');
    //        throw ("There are Child Data in this Master. First Delete Those! If Already Deleted then Update it!");
    //    }

    //    $http({
    //        method: 'POST',
    //        url: url + 'deleteMaster',
    //        data: { 'id': $scope.Header.Id }
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            $scope.getMaster();
    //            $scope.Clear();
    //            if ($rootScope.isCollapsed) {
    //                $rootScope.toggle();
    //            }
    //        }
    //        function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    });
    //}

    //Clear Masters
    $scope.ClearMasters = function () {
        $scope.Header = {
            Id: null,
            StandardName: null,
            ShortName: null,
            Description: null,
            Remarks: null,
            Active: false,
            UserName: null,
        };
        $scope.DatesList = [];
        if ($scope.WeekChildList.length > 0) {
            $scope.WeekChildList = [];
        }
        headerId.style.display = "none";
        $scope.Action = "Save";
        $scope.Action1 = "Save";
        $scope.Sequences = 0;
    }


    //Save Shifts Child with the RosterId

    $scope.checkChildList = function () {
        if ($scope.WeekChildList.length == 0) {
            angular.element(document.querySelector('#confirmPopUpChild')).modal('show');
        }
        else {
            $scope.SaveDays();
        }
    }

    $scope.SaveDays = function () {
        try {


            if ($scope.Header.Id === null || $scope.Header.Id === undefined || $scope.Header.Id.length < 3) {
                ShowResult("Please First Create A Week Off Master!!" , 'failure');
                throw ("Please First Create A Week Off Master!!")
            }

            daySeriesValidation();

            $http({
                method: 'POST',
                url: url + "SaveDays",
                data: { 'Week': $scope.WeekChildList , 'HeaderId' : $scope.Header.Id},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    if ($scope.Action1 == "Save") {
                        if ($rootScope.isCollapsed == true) {
                            $rootScope.toggle();
                        }
                        $scope.ClearMasters();
                        $scope.getMaster();
                    }
                    else {
                        $scope.getMaster();
                        $scope.restoreChildShifts();
                    }

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (e) {
            throw e;
        }
    }

    //restore Child Shifts
    $scope.restoreChildShifts = function () {
        $http({
            method: 'GET',
            url: url + 'getChilds',
            params: { 'Id': $scope.Header.Id },
        }).then(function success(response) {
            $scope.WeekChildList = response.data.Days;
            restoreShiftsChild = $scope.WeekChildList.length;
            var ll = $scope.WeekChildList.length;
            $scope.Sequences = ll;
        })
        angular.element(document.querySelector('#confirmPopUpChild')).modal('hide');
    }
    //Clear Shifts
    $scope.ClearDays = function () {
        if ($scope.WeekChildList.length > 0) {
            $scope.WeekChildList = [];
            $scope.Sequences = 0;
        }
    }


    //Refreshing Sequence
    $scope.refreshSequence = function () {
        var c = 0;
        if ($scope.WeekChildList.length > 0) {
            for (var i = 0; i < $scope.WeekChildList.length; i++) {
                c++;
                $scope.WeekChildList[i].WOSequence = c;
            }
        }
        refresh();
    }


    //Validations Section
    function validationsMaster() {
        if ($scope.Header.StandardName == null || $scope.Header.ShortName == null || $scope.Header.Description == null || $scope.Header.UserName == null ) {
            ShowResult("Please Fill All the necessary " , 'failure');
            throw ("Please Fill All the necessary ");
        }

        if ($scope.DatesList.length <= 0) {
            ShowResult("Please Select Atleast 1 Effective Date " , 'failure');
            throw ("Please Select Atleast 1 Effective Date ");
        }
    }

    function daySeriesValidation() {
        var Prev = 0;
        for (var i = 0; i < 7; i++) {
            if (Object.values($scope.DayList)[i] == $scope.WeekChildList[0].Day) {
                Prev = parseInt(Object.keys($scope.DayList)[i]);
            }
        }
        for (var i = 0; i < $scope.WeekChildList.length; i++) {
            if (Prev > 6) {
                Prev = 0;
            }
            if (Object.values($scope.DayList)[Prev] != $scope.WeekChildList[i].Day) {
                ShowResult("The Day is not in Order at Sequence - " + $scope.WeekChildList[i].WOSequence);
                throw ("Error");
            }
            Prev = Prev + 1;
        }
    }

}