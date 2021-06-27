'use strict';
BartackCodeController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];
function BartackCodeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $rootScope.title = "Bartack / Button Hole / Button Sew Code";
    $scope.Action = 'Save';
    $scope.path = 'IE/BartackCode/';

    $scope.VAS = {
        Id: '',
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: '',
        TMU: null,
        MCHand: 'M',
        CycleTime: null,
        Frequency: null,
        Activity: '',
        Element: ''
    };

    $scope.mcHandList = [];
    $scope.bartackCodeList = [];

    $scope.mcHandList = [{ name: 'M' }, { name: 'H' }];

    $scope.getAllData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllData'

        }).then(function successCallback(response) {
            $scope.bartackCodeList = response.data;
        });
    };
    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.PopulateSelectedDate($scope.RowId);
    };
    $scope.LoadSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.VAS.Id = $scope.selecteddata.Id;
        $scope.PopulateSelectedDate($scope.VAS.Id);
    };

    $scope.PopulateSelectedDate = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedData',
            data: {
                'Id': Id
            }
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.VAS.Id = response.data[0].Id;
                $scope.VAS.Code = response.data[0].Code;
                $scope.VAS.ShortName = response.data[0].ShortName;
                $scope.VAS.StandardName = response.data[0].StandardName;
                $scope.VAS.UserName = response.data[0].UserName;
                $scope.VAS.Description = response.data[0].Description;
                $scope.VAS.TMU = response.data[0].TMU;
                $scope.VAS.MCHand = response.data[0].MCHand;
                $scope.VAS.CycleTime = response.data[0].CycleTime;
                $scope.VAS.Frequency = response.data[0].Frequency;
                $scope.VAS.Activity = response.data[0].Activity;
                $scope.VAS.Element = response.data[0].Element;

                $scope.Action = 'Update';

                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            }
            else {
                ShowResult('No Data Found..!', 'failure');
            }
        });
    };
    $scope.SaveData = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.ElementCodeForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "SaveData",
                    data: { 'elementType': $scope.VAS },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.getAllData();
                        ShowResult(response.data.Message, 'success');
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.VAS.Id = $scope.selecteddata.Id;

        $scope.message_confirmation = 'Are you sure want to Remove?';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteSelectedData?Id=" + $scope.VAS.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');

                    $scope.getAllData();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getTMUValue = function () {
        var cycleTime = $scope.VAS.CycleTime;
        var frequency = $scope.VAS.Frequency;

        if (cycleTime === undefined || frequency === undefined)
            return false;

        if ($.isNumeric(cycleTime) && $.isNumeric(frequency) && cycleTime != "" && frequency != "") {
            var calculatedTMU = parseInt((cycleTime * 2000) * frequency);
            $scope.VAS.TMU = calculatedTMU;
            $scope.VAS.Code = "Z" + calculatedTMU;
        }
        else {
            $scope.VAS.TMU = "";
            $scope.VAS.Code = "";
            $scope.VAS.Frequency = "1";
        }
    };

    $scope.Cancel = function () {
        $scope.Clear();
        $rootScope.toggle();
    };

    $scope.getAllData();

    $scope.Clear = function () {
        $scope.VAS = {};
        $scope.VAS.Id = '';
        $scope.VAS.MCHand = 'M';
        $scope.VAS.Description = '';
        $scope.Action = 'Save';
    };
}