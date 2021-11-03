'use strict';
VASElementTypeController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];
function VASElementTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $rootScope.title = "Element Type";
    $scope.Action = 'Save';
    $scope.path = 'IE/VASElementType/';

    $scope.VAS = {
        Id: '',
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: ''
    };
    $scope.elementTypeList = [];

    $scope.getElementType = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllElementType'

        }).then(function successCallback(response) {
            $scope.elementTypeList = response.data;
        });
    };
    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.PopulateSelectedData($scope.RowId);
    };

    $scope.LoadSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.VAS.Id = $scope.selecteddata.Id;
        $scope.PopulateSelectedData($scope.VAS.Id);
    };

    $scope.PopulateSelectedData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedElementType',
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
    $scope.SaveElementType = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.ElementTypeForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "SaveElementType",
                    data: { 'elementType': $scope.VAS },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.getElementType();
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
                url: $scope.path + "DeleteSelectedElementType?Id=" + $scope.VAS.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    
                    $scope.getElementType();
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

    $scope.getElementType();
    $scope.Cancel = function () {
        $scope.Clear();
        $rootScope.toggle();
    };

    $scope.Clear = function () {        
        $scope.VAS = {};
        $scope.VAS.Id = '';
        $scope.VAS.Description = '';
    };
}