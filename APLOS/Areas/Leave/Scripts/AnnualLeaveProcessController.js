'use strict';
AnnualLeaveProcessController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AnnualLeaveProcessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Annual Leave Process';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Leave/AnnualLeaveProcess/';  
      
    //#region The Paging System

    var x = document.getElementById("FDiv");
    var y = document.getElementById("SDiv");
    x.style.display = "block";
    y.style.display = "none";

    $scope.clickdde1 = function () {
        if (x.style.display === "none") {
            y.style.display = "none";
            x.style.display = "block";

        }
    };

    $scope.clickdde2 = function () {
        if (y.style.display === "none") {

            y.style.display = "block";
            x.style.display = "none";

        }
    };

    // #endregion

    // #region Get Plants List and Company List
    $scope.PlantList = [];
    $scope.getPlants = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getPlants',
            params: { 'cmp': $scope.Company }
        }).then(function success(response) {
            $scope.PlantList = response.data;
        })
    }

    $scope.Company = null;
    $scope.CompanyList = [];
    $scope.getCompany = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getCompany'
        }).then(function success(response) {
            $scope.CompanyList = response.data;
        })
    }
    $scope.getCompany();

    // #endregion

    // #region Opening Upload Tab Functions

    $scope.BudgetPlantId = null;
    $scope.fileData = [];
    $scope.GetSample = function () {
        var reportFormat = "Excel";

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select Plant ...", 'failure');
            throw ("Please First Select Plant ...");
        }

        var plantName = "";
        for (var i = 0; i < $scope.PlantList.length; i++) {
            if ($scope.PlantList[i].Value == $scope.BudgetPlantId) {
                plantName = $scope.PlantList[i].Text;
            }
        }

        try {
            window.open($scope.path + 'GetSampleReport?PlantId=' + $scope.BudgetPlantId + '&name=' + plantName + '&LvYearId=LY4&reportFormat=' + reportFormat, '_blank');

        } catch (e) {

        }
    }

    $scope.ImportData = function () {
        try {
            $scope.ExcelUploadData = [];
            $scope.msg = "";
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.fileData.length == 0) {

                throw ("Please Select A File!!");
            }
            if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
                ShowResult("Please First Select a Plant!!", 'failure');
                throw ("Please First Select a Plant!!");
            }

            var fileData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.fileData)) {
                $scope.ModelNew.FileName = $scope.fileData.name;
            }

            $http({
                method: 'POST',
                url: $scope.path + 'ImportData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    fileData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.fileData) === false) {
                        fileData.append('file', data.file);
                        fileData.append('plantId', $scope.BudgetPlantId);
                    }
                    return fileData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.fileData, 'plantId': $scope.BudgetPlantId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");

                }

                else {
                    try {
                        $scope.ExcelUploadData = response.data;
                    }

                    catch (e) {

                        ShowResult(e, "failure");
                    }

                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };


    // #endregion

}