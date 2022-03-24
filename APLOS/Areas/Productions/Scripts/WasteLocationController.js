'use strict';
WasteLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WasteLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Waste Location';
    $scope.Action = 'Save';
    $scope.buyerStyles = [];
    $scope.path = 'Productions/WasteLocation/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.index = -1;
    $scope.showTbl = false;
   

    $scope.getDataList = function () {
        try {
            $http({
                method: 'Get',
                url: 'Productions/WasteLocation/GetList?companyId='+$scope.buyerStyleNew.CompanyId + '&plantId=' + $scope.buyerStyleNew.PlantId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.buyerStyles = response.data;
            });
        }
        catch (ex) {
            ShowResult(ex, "failure");
        }
    };

    $scope.Save = function () {

        try {
            var tempItem = [];
            for (var i = 0; i < $scope.buyerStyles.length; i++) {
                if ($scope.buyerStyles[i].IsWasteLocation) {
                    tempItem.push($scope.buyerStyles[i]);
                }
            }
            $scope.$broadcast('show-errors-check-validity');

                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': tempItem },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.buyerStyles();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    };

    $rootScope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.buyerStyleNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    function checkChangeWaste(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.WasteBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].IsWasteLocation = true;
            else
                row[0].IsWasteLocation = false;
        }

    }

    function headCheckChangeWaste(e) {
        if (e.model.checkState == "check") {

            var filtered = $("#GridWaste").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.buyerStyles.length; i++) {

                    $scope.buyerStyles[i].IsWasteLocation = true;
                }
            }
            else {
                for (var i = 0; i < $scope.buyerStyles.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.buyerStyles[i].Id == filtered[j].Id)
                            $scope.buyerStyles[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#GridWaste .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "change": checkChangeWaste });
            }
        }
        else {
            var filtered = $("#GridWaste").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.buyerStyles.length; i++) {
                    $scope.buyerStyles[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.buyerStyles.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.buyerStyles[i].Id == filtered[j].Id)
                            $scope.buyerStyles[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#GridWaste.rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "change": checkChangeWaste });
            }
        }
        //header level check
    }

    $scope.dataBoundWaste = function (args) {
        $("#GridWaste .rowCheckbox").ejCheckBox({ "change": checkChangeWaste });
        $("#headchk").ejCheckBox({ "change": headCheckChangeWaste });

    };
}