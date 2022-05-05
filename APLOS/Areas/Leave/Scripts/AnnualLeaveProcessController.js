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
    $scope.LeaveTypeList = [];
    $scope.getLeaveYear = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getLeaveYear',
            params: { 'PlantId': $scope.BudgetPlantId }
        }).then(function success(response) {
            $scope.YearList = response.data;
        })

        $http({
            method: 'GET',
            url: $scope.path + 'GetLeaveType'
        }).then(function success(response) {
            $scope.LeaveTypeList = response.data;
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

    $scope.NewYearList = [];
    $scope.getNewLeaveYearData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getNewLeaveYear',
            params: {
                'PlantId': $scope.BudgetPlantId,
                'LvYearId': $scope.LeaveModel.CurrentLvYearId
            }
        }).then(function success(response) {
            $scope.NewYearList = response.data;
        })
    }

    $scope.EmpCatList = [];
    $scope.getEmpCategory = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetEmpCategory'
        }).then(function success(response) {
            $scope.EmpCatList = response.data;
        })
    }
    $scope.getEmpCategory();


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

    // #region Annual Process Functions 
        
    $scope.LeaveModel = {
        CurrentLvYearId: null,
        NewLvYearId: null
    };

    
    $scope.ClearFirstTabData = function () {
        $("#EmpCategoryDropdown").data("ejDropDownList").clearText();
        $("#LeaveTypeDropdown").data("ejDropDownList").clearText();
        $scope.LeaveModel = {
            CurrentLvYearId: null,
            NewLvYearId: null
        };
        $scope.BudgetPlantId = null;
        $scope.Company = null;
    };

    $scope.LoadedData = [];
    $scope.getGridData = function () {

        $scope.LeaveTypeString = "";

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select Plant ...", 'failure');
            throw ("Please First Select Plant ...");
        }

        if ($scope.LeaveModel.CurrentLvYearId == "" ||
            $scope.LeaveModel.CurrentLvYearId == undefined) {
            ShowResult("Please First Select Current Leave Year ...", 'failure');
            throw ("Please First Select Current Leave Year ...");
        }      

        var LeaveTypeObj = $("#LeaveTypeDropdown").data("ejDropDownList");
        $scope.LeaveTypeString = LeaveTypeObj.getSelectedValue().split(",");

        if ($scope.LeaveTypeString == "" ) {
            ShowResult("Please First Select Leave Type ...", 'failure');
            throw ("Please First Select Leave Type ...");
        }

        $http({
            method: 'GET',
            url: $scope.path + 'LoadData',
            params: {
                'PlantId': $scope.BudgetPlantId,
                'LvYearId': $scope.LeaveModel.CurrentLvYearId,
                'LvTypeId': $scope.LeaveTypeString
            }
        }).then(function success(response) {
            $scope.LoadedData = response.data;
        })
    }

    $scope.ProcArr = [];
    $scope.ProcessAll = function () {

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select Plant ...", 'failure');
            throw ("Please First Select Plant ...");
        }

        if ($scope.LeaveModel.CurrentLvYearId == "" ||
            $scope.LeaveModel.CurrentLvYearId == undefined) {
            ShowResult("Please First Select Current Leave Year ...", 'failure');
            throw ("Please First Select Current Leave Year ...");
        }


        $scope.ProcArr = [];
        for (var i = 0; i < $scope.LoadedData.length; i++) {
            $scope.ProcArr.push({
                'EmpId': $scope.LoadedData[i].EmpId,
                'LeaveTypeId': $scope.LoadedData[i].LeaveTypeId, 'Opening': $scope.LoadedData[i].Opening,
                'Earned': $scope.LoadedData[i].Earned, 'RegularEncashment': $scope.LoadedData[i].RegularEncashment,
                'Availed': $scope.LoadedData[i].Availed, 'Closing': $scope.LoadedData[i].Closing,
                'Adjustment': $scope.LoadedData[i].Adjustment
            });
        }


        $scope.Proc = JSON.stringify($scope.ProcArr);

        $http({
            method: 'POST',
            url: $scope.path + 'ProcessData',
            data: {
                'Data': $scope.Proc, 'PlantId': $scope.BudgetPlantId,
                'CurrentLvYearId': $scope.LeaveModel.CurrentLvYearId,
                'MaxCarryForward':5
            },
        }).then(function succ(resp) {
            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        })
    }

    // #endregion
}