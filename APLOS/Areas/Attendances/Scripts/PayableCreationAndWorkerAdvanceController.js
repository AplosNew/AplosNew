'use strict';
PayableCreationAndWorkerAdvanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function PayableCreationAndWorkerAdvanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Payable Creation & Worker Advance';
    $scope.WorkerAdvanceList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.saveUrl = $scope.path + 'CreateWorkerAdvance';
    $scope.UpdateUrl = $scope.path + 'UpdateGoodWorkDetailEdit';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    //$scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteChildUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.LoadEmpListUrl = $scope.path + 'LoadPCAACEmployeelist';
    $scope.Action = 'Save';
    $scope.passwordShow = true;
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    //***********************************Good Work ********************************************************//

    $scope.ModelTemp = {
        Id: null,
        FromDate: null,
        ToDate: null,
        UserRef: null,
        NoOfDays: 0,
        Percentage: 0,
        CheckedBy: null,
        ApprovedBy: null,
        PreparedBy: null,
        Remarks: null        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelWADTemp = {
        Id: null,
        WorkerAdvanceId: null,
        EmpSystemId: null,
        EmployeeCode: null,
        EmployeeName: null,
        PayDays: null,
        Amount: null 
    };
    $scope.ModelWADNew = Object.assign({}, $scope.ModelWADTemp);

    $scope.popUpDataList = [];
    $scope.showByWhomEmployeeListPopUp = function (index) {
        try {
            $scope.tempIndex = index;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'Attendances/GoodWork/GetAllActiveEmployeeData'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedById = arg.data.SystemId;
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedByCode = arg.data.EmployeeCode;
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedByName = arg.data.EmployeeName;
        $scope.closePopUp();
    }

    $scope.clearEmp = function () {
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedById = null;
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedByCode = null;
        $scope.EmployeeMainList[$scope.tempIndex].ApprovedByName = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }
 
    $scope.removeRow = function (data) {
        $scope.empSystemId = data.SystemId;
        $scope.Id = data.Id;
        if (baseService.isUndefinedOrNull(data.EmployeeName))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.EmployeeName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteRow = function () {
        if ($scope.Id == "") {
            var tempData = $scope.EmployeeMainList;
            for (var i = 0; i < tempData.length; i++) {
                if (tempData[i].SystemId === $scope.empSystemId) {
                    $scope.EmployeeMainList.splice(i, 1);
                }
            }
            $scope.Id = null;
            tempData = [];
        }
        else {
            $http({
                method: 'GET',
                url: 'Attendances/GoodWork/DeleteChildUrl?Id=' + $scope.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetWorkerAdvanceDetailCenter();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, 'workerAdvanceDetail': $scope.EmployeeMainList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.EmployeeMainList = [];
        return true;
    };


    $scope.getData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetWorkerAdvanceList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.WorkerAdvanceList = response.data;
        });
    }
    $scope.getData();


    $scope.GetDblClick = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetWorkerAdvanceDetailCenter();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.EmployeeMainList = [];
    $scope.GetWorkerAdvanceDetailCenter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetWorkerAdvanceDetailCenter?workAdvanceId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeMainList = resp.data;
        });
    }
 
    // UserGroup
    $scope.UserGroupList = [];
    $scope.selectUserGroup = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getUserGroupData',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.UserGroupList = resp.data;
        });
    }
    $scope.selectUserGroup();

    // UserGroup

    //***********************************Payable Creation And Worker Advance Start ********************************************************//
 
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.employeeDataList = [];
    $scope.employeeUrl = 'OrderManagements/masterorder/GetEmployeeListResponsible';
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;
            //baseService.setCurrentPage('employeeDataList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                //$scope.employeeParameters.plantId = $scope.fileNew.PlantId;
                //$scope.employeeParameters.partyAccountGroupId = $scope.fileNew.PartyAccountGroupId;
                //$scope.employeeParameters.partyId = $scope.fileNew.PartyId;
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeDataList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeDataList[$scope.employeeIndex];
            if ($scope.Name === 'PB') {
                $scope.ModelNew.PreparedById = employee.SystemId;
                $scope.ModelNew.PreparedBy = employee.EmployeeName;
            } else if ($scope.Name === 'AB') {
                $scope.ModelNew.ApprovedById = employee.SystemId;
                $scope.ModelNew.ApprovedBy = employee.EmployeeName;
            }
            else {
                $scope.ModelNew.CheckedById = employee.SystemId;
                $scope.ModelNew.CheckedBy = employee.EmployeeName;
            }
        }
        $scope.hideEmployeePopUp();
    };


    $scope.EmployeeList = [];
    $scope.EmployeeMainList = [];
    $scope.getEmploymeeList = function () {
        try {
            $http.get($scope.LoadEmpListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeList = response.data;
                        angular.element(document.querySelector("#dialogEmployeeInfo")).modal("show");
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmployeeInfoList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                $scope.EmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.GetSelectedEmployeeList = function () {
        try {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                if (checkItemExist($scope.EmployeeMainList, $scope.EmployeeList[i].SystemId) === false) {
                    if ($scope.EmployeeList[i].CheckBoxSelect === true) {
                        $scope.EmployeeList[i].FromTime = $scope.ModelNew.FromTime;
                        $scope.EmployeeList[i].ToTime = $scope.ModelNew.ToTime;
                        $scope.EmployeeList[i].CalculatedTime = $scope.ModelNew.CalculatedTime;
                        $scope.EmployeeMainList.push($scope.EmployeeList[i]);
                    }
                }
            }
            angular.element(document.querySelector("#dialogEmployeeInfo")).modal("hide");
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkItemExist(list, SystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemId === SystemId) {
                return true;
            }
        }
        return false;
    }

    $scope.getAmount = function () {
        try {
                $http({
                    method: 'POST',
                    url: 'Attendances/GoodWork/GetAmount',
                    data: { 'data': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.ModelWADNew.Amount = response.data;
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
          
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //***********************************Payable Creation And Worker Advance End********************************************************//
}