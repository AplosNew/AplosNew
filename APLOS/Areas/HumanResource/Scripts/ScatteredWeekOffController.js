'use strict';
ScatteredWeekOffController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function ScatteredWeekOffController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Scattered Week Off';
    $scope.path = 'HumanResource/ScatteredWeekOff/';

    //Tabs Changes
    function Tabs() {
        var bindAll = function () {
            var menuElements = document.querySelectorAll('[data-tab]');
            for (var i = 0; i < menuElements.length; i++) {
                menuElements[i].addEventListener('click', change, false);
            }
        }

        var clear = function () {
            var menuElements = document.querySelectorAll('[data-tab]');
            for (var i = 0; i < menuElements.length; i++) {
                menuElements[i].classList.remove('active');
                var id = menuElements[i].getAttribute('data-tab');
                document.getElementById(id).classList.remove('active');
            }
        }

        var change = function (e) {
            clear();
            e.target.classList.add('active');
            var id = e.currentTarget.getAttribute('data-tab');
            document.getElementById(id).classList.add('active');
            if ($rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }

        bindAll();
    }
    var connectTabs = new Tabs();
    // Tab Changes End

    //Variables 
    $scope.weekOffList = [];
    $scope.companyList = [];
    $scope.plantList = [];
    $scope.savedWeekList = [];
    $scope.MasterDetails = [];

    $scope.Action = "Save";

    $scope.companyId = null;
    $scope.plantId = null;

    $scope.WeekDef = {
        Monday: null,
        Tuesday: null,
        Wednesday: null,
        Thursday: null,
        Friday: null,
        Saturday: null,
        Sunday: null,
    };


    $scope.WeekMaster = {
        Id: null,
        PlantId: null,
        StandardName: null,
        UserName: null,
        MaxBudgetNumber: null,
        Remarks: null,
    };

    // First Tab and the Page **********************************************************************
    $scope.getFirstRuns = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getWeeksList',
        }).then(function succ(resp) {
            $scope.weekOffList = resp.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'getCurrentWeekDef',
        }).then(function succ(resp) {
            for (var i = 0; i < resp.data.length; i++) {
                var jj = resp.data[i].Day;
                $scope.WeekDef[jj] = resp.data[i].WOHeaderId;
            }
        });
    }
    $scope.getFirstRuns();
   

    $scope.checkEntryDef = function () {
        var kk = {};
        var l = Object.values($scope.WeekDef);
        for (var i = 0; i < l.length; i++) {
            if (l[i] in kk) {
                ShowResult('Cannot have duplicate Entries!!', 'failure');
                throw ('');
            }
            else if (angular.isUndefinedOrNull(l[i]))
            {
                ShowResult('Selection of a Value is Mandatory', 'failure');
                throw ('');
            }
            else {
                kk[l[i]] = true;
            }
        }
    }

    $scope.SaveAllDef = function () {
        $scope.checkEntryDef();
        $http({
            method: 'POST',
            url: $scope.path + 'SaveAllDef',
            data: { 'data': $scope.WeekDef },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getFirstRuns();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }


    };

        // First Tab and the Page Ends **********************************************************************
    //
    //
        // Second Tab and the Page **********************************************************************

    $http({
        method: 'GET',
        url: $scope.path + 'getCompany',
    }).then(function succ(resp) {
        $scope.companyList = resp.data;
    });

    $scope.getMasterData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getMasterData',
        }).then(function succ(resp) {
            $scope.MasterDetails = resp.data;
        });
    }
    $scope.getMasterData();

    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getPlants',
            params: { cmp: $scope.companyId}
        }).then(function succ(resp) {
            $scope.plantList = resp.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'getCurrWeeksList',
            params: {HeaderId : $scope.WeekMaster.Id}
        }).then(function succ(resp) {
            $scope.savedWeekList = resp.data;
            for (var i = 0; i < $scope.savedWeekList.length; i++) {
                if ($scope.savedWeekList[i].isApplicable == 1) {
                    $scope.savedWeekList[i].isApplicable = true;
                }
            }
        });

    }

     //Gettting the Double Click Master Detail
    $scope.GetMasterDetails = function (e) {
        Object.assign($scope.WeekMaster, e.data);
        $scope.companyId = e.data.CompanyId;
        $scope.getPlant();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    
    //Saving of the Master and Child Data
    $scope.SaveAll = function () {
        let arr = [];
        for (var i = 0; i < $scope.savedWeekList.length; i++) {
            if ($scope.savedWeekList[i].isApplicable == true) {
                arr.push($scope.savedWeekList[i]);
            }
        }

        if (arr.length <= 0) {
            ShowResult("No Data for child is selected!!", 'failure');
            throw ('');
        }

        $http({
            method: 'POST',
            url: $scope.path + 'Create',
            data: { 'masterData': $scope.WeekMaster , 'childData':arr },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMasterData();
                if ($rootScope.isCollapsed) {
                    $rootScope.toggle();
                }

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    }

    //Clearing the Master and child data
    $scope.Clear = function () {
        $scope.WeekMaster = {
            Id: null,
            PlantId: null,
            StandardName: null,
            UserName: null,
            MaxBudgetNumber: null,
            Remarks: null,
        };

        $scope.companyId = null;
        for (var i = 0; i < $scope.savedWeekList.length; i++) {
             $scope.savedWeekList[i].isApplicable = false;
        }
    }

    //Deleting the Master and Child
    $scope.DeleteAll = function () {
        angular.element(document.querySelector('#confirmPOPUPD')).modal('show');
    }

    $scope.DeleteSelected = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteChild',
            data: { 'id': $scope.WeekMaster.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMasterData();
                $scope.Clear();
                if ($rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

}