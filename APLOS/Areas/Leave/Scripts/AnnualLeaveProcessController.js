'use strict';
AnnualLeaveProcessController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AnnualLeaveProcessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Annual Leave Process';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Leave/AnnualLeaveProcess/';  


    // #region The Tab Switching Code    

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion
        
    // #region Other Functions

    $scope.SelectedYearId = null;
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

    $scope.YearList = [];
    $scope.getLeaveYear = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getLeaveYear',
            params: { 'PlantId': $scope.BudgetPlantId }
        }).then(function success(response) {
            $scope.YearList = response.data;
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

    // #region Sample Report Download

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
            window.open($scope.path + 'GetSampleReport?PlantId=' + $scope.BudgetPlantId + '&name=' + plantName + '&LvYearId=' + $scope.SelectedYearId+ '&reportFormat=' + reportFormat, '_blank');

        } catch (e) {

        }
    }

    // #endregion

    $scope.currentList = [];
    $scope.getCurrentFileList = function () {

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select a Plant!!", 'failure');
            throw ("Invalid!!");
        }

        if ($scope.SelectedYearId == "" || $scope.SelectedYearId == undefined) {
            ShowResult("Please First Select Leave Year !!", 'failure');
            throw ("Invalid!!");
        }

        $http({
            method: 'GET',
            url: $scope.path + 'getCurrentList',
            params: { 'PlantId': $scope.BudgetPlantId, 'YearId': $scope.SelectedYearId }
        }).then(function success(response) {
            $scope.currentList = [];
            $scope.currentList = response.data;
        })
    }

    $("#uploadFile").change(function () {
        $scope.fileData = this.files[0];
    });
    $scope.ExcelUploadData = [];
    
    $scope.ModelNew = {
        FileName: null
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

            if ($scope.SelectedYearId == "" || $scope.SelectedYearId == undefined) {
                ShowResult("Please First Select Leave Year!!", 'failure');
                throw ("Please First Select Leave Year!!");
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
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.fileData }
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

    $scope.saveFileList = function () {

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select Plant!!", 'failure');
            throw ("Please First Select Plant!!");
        }

        if ($scope.SelectedYearId == "" || $scope.SelectedYearId == undefined) {
            ShowResult("Please First Select Leave Year!!", 'failure');
            throw ("Please First Select Leave Year!!");
        }

        $http({
            method: 'POST',
            url: $scope.path + 'SaveFileList',
            data: {
                'data': $scope.ExcelUploadData, 'PlantId': $scope.BudgetPlantId,
                'YearId': $scope.SelectedYearId
                }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                try {
                    if ($rootScope.isCollapsed == true) {
                        $rootScope.toggle();
                    }
                    $scope.getCurrentFileList();
                    ShowResult(response.data.Message, 'success')
                }
                catch (e) {

                    ShowResult(e, "failure");
                }
            }
        }, function errorCallback(response) {

        });
    }

    $scope.clearFileList = function () {

        $scope.Company = null;
        $scope.BudgetPlantId = null;
        $scope.SelectedYearId = null;
    }

    // #endregion

}