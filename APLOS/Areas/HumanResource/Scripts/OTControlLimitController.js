'use strict';
OTControlLimitController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function OTControlLimitController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $scope.model = {
        Id: null, EffectiveDate: null, ByWhom: null, ApproveBy: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    }
    $scope.modelNew = Object.assign({}, $scope.model);

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
    $scope.employeeUrl ='OrderManagements/masterorder/GetEmployeeListResponsible';
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;
            //$scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        //$scope.employeeParameters.searchBy = 'EmployeeCode';
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
            var employee = $scope.employeeList[$scope.employeeIndex];
            if ($scope.Name === 'ByWhom') {
                $scope.modelNew.ByWhom = employee.SystemId;
                $scope.modelNew.ByWhomName = employee.EmployeeName;
            } 
            else {
                $scope.modelNew.ApproveBy = employee.SystemId;
                $scope.modelNew.ApproveByName = employee.EmployeeName;

            }
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.MaterialGridTempList = [];
    $scope.GetSampleFile = function () {
        try {
            $scope.fileName = $scope.fabricRollMaster.GRNNo + '-' + "Fabric Roll Management Template.xlsx";

            $scope.MaterialGridTempList = [];

            var ReportFormat = 'Excel';
            $http({
                method: 'POST',
                url: 'HumanResource/OTControlLimit/GetSampleFile',
                data: {
                    'reportFormat': ReportFormat, 'GridTempList': $scope.MaterialGridTempList, 'fabricRollMaster': $scope.fabricRollMaster
                },
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.grnDetailList = [];
    $scope.ImportData = function () {
        try {
            $scope.msg = "";

            var picData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.picdata)) {
                $scope.ModelNew.FileName = $scope.picdata.name;
            }


            $http({
                method: 'POST',
                url: 'Materials/FabricRoll/ImportData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    picData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                        picData.append('file', data.file);
                    }
                    return picData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");

                }
                else {
                    $scope.grnDetailList = [];
                    var x = GetShortList(response.data);
                    $scope.grnDetailList = x;
                    $scope.ShowSaveBtn = true;
                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

}