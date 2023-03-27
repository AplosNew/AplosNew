'use strict';
AnnualLeaveProcessController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function AnnualLeaveProcessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Annual Leave Process';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Leave/AnnualLeaveProcess/';
    $scope.exportgriddataUrlUpdate2 = 'GridReports/ExcelExportUpdate2';
    $scope.downloadgriddataUrl2 = 'GridReports/Download';


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
    $scope.RegYearList = [];
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
            url: $scope.path + 'getLeaveYear',
            params: { 'PlantId': $scope.BudgetPlantId }
        }).then(function success(response) {
            $scope.RegYearList = response.data;
            
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
            window.open($scope.path + 'GetSampleReport?PlantId=' + $scope.BudgetPlantId + '&name=' + plantName + '&LvYearId=' + $scope.SelectedYearId + '&reportFormat=' + reportFormat, '_blank');

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
        NewLvYearId: null,
        MaxCarryForward: null,
        MaxEncash: null,
        MaxLapse: null
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
        $scope.EmpCategoryString = "";

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

        if ($scope.LeaveTypeString == "") {
            ShowResult("Please First Select Leave Type ...", 'failure');
            throw ("Please First Select Leave Type ...");
        }

        var EmployeeTypeObj = $("#EmpCategoryDropdown").data("ejDropDownList");
        $scope.EmpCategoryString = EmployeeTypeObj.getSelectedValue().split(",");

        if ($scope.EmpCategoryString == "") {
            ShowResult("Please First Select Employee Type ...", 'failure');
            throw ("Please First Select Employee Category ...");
        }

        $http({
            method: 'GET',
            url: $scope.path + 'LoadData',
            params: {
                'PlantId': $scope.BudgetPlantId,
                'LvYearId': $scope.LeaveModel.CurrentLvYearId,
                'LvTypeId': $scope.LeaveTypeString,
                'EmpCategory': $scope.EmpCategoryString
            }
        }).then(function success(response) {
            $scope.LoadedData = response.data;
        })
    }

    $scope.ProcArr = [];
    $scope.ProcessAll = function () {

        // #region Validations

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select Plant ...", 'failure');
            throw ("Please First Select Plant ...");
        }

        if ($scope.LeaveModel.CurrentLvYearId == "" ||
            $scope.LeaveModel.CurrentLvYearId == undefined) {
            ShowResult("Please First Select Current Leave Year ...", 'failure');
            throw ("Please First Select Current Leave Year ...");
        }

        if ($scope.LeaveModel.NewLvYearId == "" ||
            $scope.LeaveModel.NewLvYearId == undefined) {
            ShowResult("Please First Select New Leave Year ...", 'failure');
            throw ("Please First Select New Leave Year ...");
        }

        if ($scope.LeaveModel.MaxCarryForward == "" || $scope.LeaveModel.MaxCarryForward == undefined) {
            ShowResult("Please Enter Max Carryforward ...", 'failure');
            throw ("Please Enter Max Carryforward ...");
        }

        if ($scope.LeaveModel.MaxEncash == "" || $scope.LeaveModel.MaxEncash == undefined) {
            ShowResult("Please Enter Max Encashment ...", 'failure');
            throw ("Please Enter Max Encashment ...");
        }

        if ($scope.LeaveModel.MaxLapse == "" || $scope.LeaveModel.MaxLapse == undefined) {
            ShowResult("Please Enter Max Lapse ...", 'failure');
            throw ("Please Enter Max Lapse ...");
        }

        // #endregion

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
                'MaxCarryForward': $scope.LeaveModel.MaxCarryForward,
                'MaxEncash': $scope.LeaveModel.MaxEncash,
                'MaxLapse': $scope.LeaveModel.MaxLapse,
                'LeaveTypeList': $scope.LeaveTypeString,
                'NewYear': $scope.LeaveModel.NewLvYearId
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

    // #region Regular Encashment Functions

    $scope.LeaveRegModel = {
        CurrentLvYearId: null,
        MaxEncash: null,
        From: null,
        To: null
    };

    $scope.ClearThirdTabData = function () {
        $("#LeaveRegTypeDropdown").data("ejDropDownList").clearText();
        $scope.LeaveRegModel = {
            CurrentLvYearId: null,
            MaxEncash: null
        };
        $scope.BudgetPlantId = null;
        $scope.Company = null;
    };

    $scope.EmployeeData = [];
    $scope.getEmployeeData = function () {
        try {

            $scope.LeaveTypexString = "";

            if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
                throw ("Please First Select Plant ...");
            }

            if ($scope.LeaveRegModel.CurrentLvYearId == "" ||
                $scope.LeaveRegModel.CurrentLvYearId == undefined) {
                throw ("Please First Select Leave Year ...");
            }

            var LeaveTypexObj = $("#LeaveRegTypeDropdown").data("ejDropDownList");
            $scope.LeaveTypexString = LeaveTypexObj.getSelectedValue().split(",");

            if ($scope.LeaveTypexString == "") {
                throw ("Please First Select Leave Type ...");
            }

            if (baseService.isUndefinedOrNull($scope.LeaveRegModel.From)) {
                throw ("Please Select From Date ...");
            }
            if (baseService.isUndefinedOrNull($scope.LeaveRegModel.To)) {
                throw ("Please Select To Date ...");
            }

            $http({
                method: 'GET',
                url: $scope.path + 'GetEmpInfo',
                params: {
                    'PlantId': $scope.BudgetPlantId,
                    'From': $scope.LeaveRegModel.From,
                    'To': $scope.LeaveRegModel.To,
                    'Year': $scope.LeaveRegModel.CurrentLvYearId
                }
            }).then(function success(response) {
                $scope.EmployeeData = response.data;
            })
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.ProcReg = [];
    $scope.ProcessRegEncash = function () {

        // #region Validations

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select Plant ...", 'failure');
            throw ("Please First Select Plant ...");
        }

        if ($scope.LeaveRegModel.CurrentLvYearId == "" ||
            $scope.LeaveRegModel.CurrentLvYearId == undefined) {
            ShowResult("Please First Select Leave Year ...", 'failure');
            throw ("Please First Select Leave Year ...");
        }

        if ($scope.LeaveRegModel.MaxEncash == "" || $scope.LeaveRegModel.MaxEncash == undefined) {
            ShowResult("Please Enter Max Encashment ...", 'failure');
            throw ("Please Enter Max Encashment ...");
        }


        // #endregion

        $scope.ProcReg = [];
        for (var i = 0; i < $scope.EmployeeData.length; i++) {
            $scope.ProcReg.push({
                'EmpId': $scope.EmployeeData[i].EmpId,
                'LeaveTypeId': $scope.EmployeeData[i].LeaveTypeId
            });
        }


        $scope.ProcData = JSON.stringify($scope.ProcReg);

        $http({
            method: 'POST',
            url: $scope.path + 'ProcessRegData',
            data: {
                'Data': $scope.ProcData, 'PlantId': $scope.BudgetPlantId,
                'CurrentLvYearId': $scope.LeaveRegModel.CurrentLvYearId,
                'MaxEncash': $scope.LeaveRegModel.MaxEncash,
                'LeaveTypeList': $scope.LeaveTypexString
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




    $scope.DownLoadEmpData = function () {
        var dataList = [];
        var g = $("#EmpDataGrid").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.EmployeeData;
        }
        $scope.fileName = 'EmpLeaveDataReport';
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpdate2,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrl2 + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.DownLoadData = function () {
        try {
            var dataList = [];
            var g = $("#LeaveDataGrid").data("ejGrid");
            dataList = g.getFilteredRecords();

            if (dataList.length == 0) {
                dataList = $scope.LoadedData;
            }

            if (dataList.length == 0) {
                throw "First click on Go button.";
            }

            $scope.fileName = "LeaveDataReport.xlsx";

            $http({
                method: 'POST',
                url: $scope.path + "LeaveDataReportXls",
                data: { 'reportFileName': $scope.fileName, 'data': dataList, 'plantId': $scope.BudgetPlantId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.LeaveRegModelNew = { empId: null, EmployeeCode: null, EmployeeName: null, CurrentLvYearId: null, ToDate: null, FromDate:null};

    $scope.popUpDataList = [];

    $scope.showEmployeeListPopUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmployeeData'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });

            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.LeaveRegModelNew.empId = arg.data.SystemId;
        $scope.LeaveRegModelNew.EmployeeCode = arg.data.EmployeeCode;
        $scope.LeaveRegModelNew.EmployeeName = arg.data.EmployeeName;
        $scope.closePopUp();
    }

    $scope.clearEmp = function () {
        $scope.LeaveRegModelNew.EmployeeId = null;
        $scope.LeaveRegModelNew.EmployeeCode = null;
        $scope.LeaveRegModelNew.EmployeeName = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.EmployeeYearEarnAvailDataList = [];
    $scope.GetEmpYearEarnAvailData = function () {
        $http.get('Leave/AnnualLeaveProcess/GetEmpYearEarnAvailData?fromdate=' + $scope.LeaveRegModelNew.FromDate + '&todate=' + $scope.LeaveRegModelNew.ToDate + '&empId=' + $scope.LeaveRegModelNew.empId)
            .then(function (response) {
                $scope.EmployeeYearEarnAvailDataList = [];
                $scope.EmployeeYearEarnAvailDataList = response.data;
            });
    };

    $scope.GetFromToDate = function () {
        for (var i = 0; i < $scope.RegYearList.length; i++) {
            if ($scope.LeaveRegModelNew.CurrentLvYearId == $scope.RegYearList[i].Value) {
                $scope.LeaveRegModelNew.FromDate = $scope.RegYearList[i].FromDate;
                $scope.LeaveRegModelNew.ToDate = $scope.RegYearList[i].ToDate;
            }
        }
    }

    $scope.DownLoadEmpYearEarnAvailData = function () {
        var dataList = [];
        var g = $("#EmpYEADataGrid").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.EmployeeYearEarnAvailDataList;
        }
        $scope.fileName = 'EmployeeYearEarnAvailDataReport';
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpdate2,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrl2 + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };


}