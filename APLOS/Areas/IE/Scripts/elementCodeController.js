'use strict';

ElementCodeController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];
function ElementCodeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $rootScope.title = "Element Code";
    $scope.Action = 'Save';
    $scope.path = 'IE/ElementCode/';

    $scope.VAS = {
        Id: '',
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: '',
        TMU: null,
        MCHand: 'M',
        CodeType: '',
        Activity: '',
        Element: ''
    };

    $scope.elementCodeList = [];
    $scope.mcHandList = [{ name: 'H' }, { name: 'M' }];

    $scope.getAllData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllData'

        }).then(function successCallback(response) {
            $scope.elementCodeList = response.data;
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

    $scope.Save = function () {
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
                        $scope.Clear();
                        $scope.getAllData();
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector("#modalElementCode")).modal("toggle");
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