'use strict';
exceptionEmployeeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function exceptionEmployeeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Exception Employee';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.rests = [];
    $scope.path = 'humanresource/rest/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.excEmpUrl = 'Employees/ExceptionEmployee/';
    $scope.saveUrl = $scope.excEmpUrl + 'create';
    $scope.updateUrl = $scope.excEmpUrl + 'edit';
    $scope.deleteUrl = $scope.excEmpUrl + 'delete/';

    $scope.restNew = {
        SectionId: null,
        SubSectionId: null,
        DepartmentId: null,
        IsOTEntitle: false
    };

    $scope.popUpList = [];
    $scope.popUpDataList = [];
    $scope.popUp = function (name) {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: '',
            searchBy: '',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        try {
            $scope.popUpUrl = '';
            $scope.popUpParameters.sort = '';
            $scope.popUpParameters.searchBy = '';

            if (baseService.isUndefinedOrNull($scope.restNew.IsOTEntitle)) {
                $scope.restNew.IsOTEntitle = false;
            }
            $scope.popUpUrl = 'Employees/ExceptionEmployee/getallemployeelist?sectionId=' + $scope.restNew.SectionId + '&subSectionId=' + $scope.restNew.SubSectionId + '&departmentId=' + $scope.restNew.DepartmentId + '&isOTEntitle=' + $scope.restNew.IsOTEntitle;
            $scope.popUpParameters.sort = 'EmployeeCode';
            $scope.popUpParameters.searchBy = 'EmployeeCode';

            if (name === 'EmployeeInfo') {
                $scope.popUpTitle = 'Employee Information';
            }

            $scope.popUpData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                    .then(function (result) {
                        $scope.popUpDataList = result.Rows;
                        if (baseService.arrayLength($scope.popUpDataList) > 0) {
                            if (baseService.arrayLength($scope.tempList) !== 0) {
                                for (var i = 0; i < $scope.popUpDataList.length; i++) {
                                    $scope.popUpDataList[i].Active = getActive($scope.tempList, $scope.popUpDataList[i].EmpSystemId);
                                }
                            }
                            $scope.popUpParameters.total_count = result.Total;
                            if (baseService.arrayLength($scope.popUpList) === 0) {
                                baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                            }
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };

            $scope.fieldName = name;
            angular.element(document.querySelector('#popUp')).modal('show');
            $scope.popUpData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.valueData = '';
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.SelectByButton = function () {
        if ($scope.valueData === '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.EmpSystemId) === false) {
                    $scope.tempList.push(data);
                }
            }
         
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].EmpSystemId === data.EmpSystemId) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, EmpSystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === EmpSystemId) {
                return true;
            }
        }
        return false;
    }

    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.popUpDataList.length; i++) {
            $scope.popUpDataList[i].Active = _isselected;
        }

        for (var j = 0; j < baseService.arrayLength($scope.popUpDataList); j++) {
            if (_isselected)
                $scope.tempList.push($scope.popUpDataList[j]);
            else
                for (var k = 0; k < $scope.tempList.length; k++) {
                    if ($scope.tempList[k].EmpSystemId === $scope.popUpDataList[j].EmpSystemId) {
                        $scope.tempList.splice(k, 1);
                        break;
                    }
                }
        }
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.Save = function () {
        try {

            if ($scope.tempList.length == 0) {
                throw 'Please Select Employee......';
            }
            for (var i = 0; i < $scope.tempList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.tempList[i].EffectiveDate)) {
                    throw "Effective Date is required for Employee" + $scope.tempList[i].EmployeeCode + "";
                }
            }

            angular.copy($scope.restNew, $scope.rest);
            $scope.$broadcast('show-errors-check-validity');           
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {  'empList': $scope.tempList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure','popUp');
                }
                else {
                    ShowResult(response.data.Message, 'success','popUp');
                    if ($rootScope.isCollapsed) $rootScope.toggle();
                    $scope.GetExceptionEmployee();
                    $scope.closePopUp();

                    var gridObj = $("#GridEmpWise").data("ej-grid");
                    gridObj.refreshContent(true);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'popUp');
            };


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetExceptionEmployeeList = [];
    $scope.GetExceptionEmployee = function () {
        try {
           
            $http({
                method: 'GET',
                url: "Employees/ExceptionEmployee/GetExceptionEmployeeList",
                //data: { 'empList': $scope.tempList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {
                    //ShowResult(response.data.Message, 'success');
                    $scope.GetExceptionEmployeeList = response.data;
                   
                }
            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetExceptionEmployee();


    $scope.employeeInformation = {};
    $scope.message_confirmation = null;
    $scope.commandExceptionEmployeeDelete = function (obj) {
       
        $scope.employeeInformation = obj.data;
        if (!baseService.isUndefinedOrNull($scope.employeeInformation.SystemID))
            $scope.message_confirmation = 'Are you sure to remove This Employee  [ ' + $scope.employeeInformation.EmployeeCode + ' ] Exception Employee list?';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    };


    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.employeeInformation.SystemID)) {
            $http({
                method: 'POST',
                url: "Employees/ExceptionEmployee/Delete?EmpId=" + $scope.employeeInformation.SystemID,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetExceptionEmployee();
                   
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.confirmDelete = function (Id, EmployeeCode, index) {
        $scope.index = index;
        $scope.deleteId = Id;
        $scope.message_confirmation = "Are you sure to permanently delete [" + EmployeeCode + "]? ";
    };

    $scope.DeleteDetail = function () {
        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            $scope.tempList.splice($scope.index, 1);
            $scope.index = -1;
        } else {
            $http({
                method: 'POST',
                url: 'humanresource/rest/deletedetail',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetRestDetailsData($scope.restid);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };
    $scope.messageText = "";

    $scope.ShowResultCustom = function (message, type) {
        $("#dialogMessage").ejDialog("setTitle", "Success");
        $scope.messageText = message;
        $scope.messageTitle = "Message";

        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };

}