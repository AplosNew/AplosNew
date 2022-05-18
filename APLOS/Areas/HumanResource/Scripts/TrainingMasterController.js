'use strict';
TrainingMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TrainingMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Training Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/TrainingMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // Tab Change
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    // Enable Disable
    $scope.EnableDisable = function () {
        $scope.result = $scope.ModelNew.CostSaving;
        if ($scope.result == "Yes") {
            if (document.getElementById("RepeatedDays").disabled == true) {
                document.getElementById("RepeatedDays").disabled = false;
            }

        } else {
            if (document.getElementById("RepeatedDays").disabled == false) {
                document.getElementById("RepeatedDays").disabled = true;
            }
        }


    }

    // ALL GET FUNCTION

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    //$scope.getData();

    $scope.RepeatedDaysList = [];
    $scope.getRepeatedays = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getRepeatedays",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RepeatedDaysList = response.data;
        });

    }


    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
   // $scope.GetSequence();

    // Save Function
    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Category: null,
        SubCategory: null,
        Class: null,
        SubClass: null,
        Group: null,
        SubGroup: null,
        TrainingModule: null,
        Attachment: null,
        Repeated: null,
        RepeatedDays: null,
        
        StoryPoint: null,
        MO: null,
        EffectiveDate: null,
        OneTime:null,

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope / path + "save",
            data: {
                'data': $scope.ModelNew,
            },
            dataType:'JSON',

        }).then(function successCalback() {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
               // ClearFields(response.data.Sequence);
                //$scope.getData();

            }
        });
    }

    // For an Update
    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
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
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }
}