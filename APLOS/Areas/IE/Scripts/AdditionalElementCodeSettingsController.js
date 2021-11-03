'use strict';
AdditionalElementCodeSettingsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AdditionalElementCodeSettingsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Additional Element Code Settings';
    $scope.Action = 'Save';
    $scope.path = 'IE/AdditionalElementCodeSettings/';


    $scope.GeneralSettings = { Id: null, EachStartTMU: 8.5, EachStopTMU: 8.5 };
    $scope.StoppingAccuracy = [];
    $scope.HandlingFactor = [];

    $scope.StoppingAccuracyModel = { Id: null, Code: null, ValueInTMU: 0, Description: null };
    $scope.HandlingFactorModel = { Id: null, Code: null, DegreeOfDifficulty: 0, AdditionRate: 0, Description: null };



    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }
    $scope.openPopupAngular = function (popupName) {
        try {
            angular.element(document.querySelector("#" + popupName + "")).modal("show");
        } catch (e) {

        }

    }
    $scope.Attachmenttab = 1;
    $scope.AttachmentsetTab = function (newTab) {
        $scope.Attachmenttab = newTab;
    };
    $scope.AttachmentisSet = function (tabNum) {
        return $scope.Attachmenttab === tabNum;

    };

    $scope.SaveGeneralSettings = function () {

        $http({
            method: 'POST',
            url: $scope.path + 'SaveGeneralSettings',
            data: { data: $scope.GeneralSettings }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });

    }

    $scope.SaveStoppingAccuracy = function () {

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.FrmStoppingAccuracyModel.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveStoppingAccuracy',
                data: { data: $scope.StoppingAccuracyModel, Masterdata: $scope.GeneralSettings }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.getData();
                    document.getElementById("StoppingAccuracyCode").focus();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }

    }
    $scope.DeleteStoppingAccuracy = function (data) {

        $http({
            method: 'GET',
            url: $scope.path + 'DeleteStoppingAccuracy?Id=' + $scope.StoppingAccuracyModel.Id,
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });

    }
    $scope.ClearStoppingAccuracy = function () {
        $scope.StoppingAccuracyModel = { Id: null, Code: null, ValueInTMU: 0, Description: null };
    }
    $scope.SelectStoppingAccuracy = function (args) {
        $scope.StoppingAccuracyModel = args.data;
    }


    $scope.SaveHandlingFactor = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.FrmHandlingFactorModel.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveHandlingFactor',
                data: { data: $scope.HandlingFactorModel, Masterdata: $scope.GeneralSettings }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.getData();
                    document.getElementById("HandlingFactorCode").focus();

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
    }
    $scope.DeleteHandlingFactor = function (data) {

        $http({
            method: 'GET',
            url: $scope.path + 'DeleteHandlingFactor?Id=' + $scope.HandlingFactorModel.Id
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });

    }
    $scope.ClearHandlingFactor = function () {
        $scope.HandlingFactorModel = { Id: null, Code: null, DegreeOfDifficulty: 0, AdditionRate: 0, Description: null };

    }
    $scope.SelectHandlingFactor = function (args) {
        $scope.HandlingFactorModel = args.data;
    }


    $scope.getData = function () {
        $scope.ClearStoppingAccuracy();
        $scope.ClearHandlingFactor();

        $http({
            method: 'POST',
            url: $scope.path + 'getData',
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.GeneralSettings = { Id: null, EachStartTMU: 8.5, EachStopTMU: 8.5 };
                if (response.data.GeneralSettings.length > 0) {
                    $scope.GeneralSettings = response.data.GeneralSettings[0];
                }
                $scope.StoppingAccuracy = response.data.StoppingAccuracy;
                $scope.HandlingFactor = response.data.HandlingFactor;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
    };
    $scope.getData();
}