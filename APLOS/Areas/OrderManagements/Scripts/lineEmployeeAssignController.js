'use strict';
lineEmployeeAssignController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function lineEmployeeAssignController(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Line Operator Assign";
    $scope.Action = 'Save';
    $scope.lineEmployeeAssigns = [];
    $scope.employeeAssignList = [];
    $scope.path = 'OrderManagements/LineEmployeeAssign/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.totalOperationCount = 0;
    $scope.lineEmployeeAssign = {
        Id: null,
        EmployeeId: null,
        OperationDate: null,
        OperationId: null,
        LineId: null,
        Line: null,
        SalesOrderId: null,
        ShiftId: null,
        LineOperationBookingId: null,
        OperatorQty: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null
    };
    $scope.lineEmployeeAssignNew = Object.assign({}, $scope.lineEmployeeAssign);
    $scope.lineList = [];
    $scope.getLineCbo = function () {
        $scope.lineEmployeeAssigns = [];
        $scope.lineEmployeeAssignNew.LineId = null;
        $scope.lineEmployeeAssignNew.SalesOrderId = null;
        $scope.lineEmployeeAssignNew.ShiftId = null;
        cboService.getLineCbo($filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy'), function (result) {
            $scope.lineList = result;
        });
    }
    $scope.salesOrderCboList = [];
    $scope.getSalesOrderCbo = function () {
        $scope.lineEmployeeAssigns = [];
        $scope.lineEmployeeAssignNew.SalesOrderId = null;
        $scope.lineEmployeeAssignNew.ShiftId = null;
        cboService.getSalesOrderCbo($filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy'), document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text, function (result) {
            $scope.salesOrderCboList = result;
        });
    };
    $scope.operationCboList = [];
    $scope.getOperationList = function () {
        $scope.lineEmployeeAssigns = [];
        cboService.getOperationCbo($filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy'), document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text, function (result) {
            $scope.operationCboList = result;
        });
    };
    $scope.shiftCboList = [];
    $scope.getShiftCboList = function () {
        $scope.lineEmployeeAssigns = [];
        $scope.lineEmployeeAssignNew.ShiftId = null;
        cboService.getShiftCbo($filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy'), document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text, document.getElementById("SalesOrderId").options[document.getElementById('SalesOrderId').selectedIndex].text, function (result) {
            $scope.shiftCboList = result;
        });
    };
    $scope.salesOrderList = [];
    $scope.getSalesOrder = function () {
        $http({
            method: "GET",
            url: '/OrderManagements/LineEmployeeAssign/GetSalesOrder',
            params: {
                'date': $filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy'),
                'linename': document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text,
                'operationName': document.getElementById("OperationId").options[document.getElementById('OperationId').selectedIndex].text
            },
            dataType: "json"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.salesOrderList = response.data;
                if ($scope.salesOrderList.length > 1) {
                    $scope.employeeAddConfirmModal();
                } else {
                    addSalesOrderData();
                }
            }
        }), function errorCallBack(response) {
            showResult(response.data.Message, 'failure');
        };
    };
    $scope.salesOrderOb = {};
    $scope.getSoData = function (value) {
        $scope.salesOrderOb = $.grep($scope.salesOrderList, function (item) {
            return item.Value === value;
        })[0];
    };
    $scope.employeeWithTerget = [];
    $scope.getEmpAssignList = function () {
        $http({
            method: "GET",
            url: '/OrderManagements/LineEmployeeAssign/GetList',
            params: {
                'date': $filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy'),
                'line': document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text,
                'salesOrderName': document.getElementById("SalesOrderId").options[document.getElementById('SalesOrderId').selectedIndex].text,
                'shift': document.getElementById("ShiftId").options[document.getElementById('ShiftId').selectedIndex].text
            },
            dataType: "json"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.lineEmployeeAssigns = response.data;
                $scope.totalOperationCount = baseService.filterUnique(response.data, 'LineOperationBookingId').length;
            }
        }), function errorCallBack(response) {
            showResult(response.data.Message, 'failure');
        };
    };
    function setEmpTarget(list, index) {
        //angular.forEach(list, function (item) {
        //    item.AssignTarget = getEmpTarget(list, item.LineOperationBookingId);
        getEmpTarget(list, index);
        //})
        return list;
    }
    function getEmpTarget(list, id) {
        var list2 = $filter("filter")(list, { LineOperationBookingId: id });
        var rCount = list2.length;
        //return t / rCount;

        var result = 0;
        var count = rCount;
        var iterration = count;
        var value = list2[0].ProductionQty;
        var arr = [];
        for (var t = 0; t < count; t++) {
            if (result === 0) {
                arr.push(parseInt(parseInt(value) / parseInt(count)));
                result += parseInt(parseInt(value) / parseInt(count));
            }
            else {
                arr.push(parseInt(parseInt(value - result) / parseInt(iterration)));
                result += parseInt(parseInt(value - result) / parseInt(iterration));
            }
            iterration--;
        }
        var iii = 0;
        for (var a = 0; a < baseService.arrayLength(list); a++) {
            if (list[a].LineOperationBookingId === id) {
                list[a].AssignActualProduction = arr[iii];
                list[a].OperatorQty = arr[iii];
                iii++;
            }
        }
        // return result;
    }
    //#region Employee
    $scope.employeeAddConfirmModal = function () {
        $scope.message_confirmation = 'There are more than one salse order. Do you want to add employee for all?';
        angular.element(document.querySelector('#confirmgenericPopUpForEmployee')).modal('show');
    };
    $scope.confirmToShowEmployeeModal = function () {
        $scope.rowEmployeeSelect = false;
        $scope.popUp();
    };
    $scope.notConfirmToShowEmployeeModal = function () {
        addSalesOrderData();
    };
    function addSalesOrderData() {
        angular.forEach($scope.salesOrderList, function (item) {
            var ob = {};
            ob.Id = null;
            ob.EmployeeId = null;
            ob.EmployeeName = null;
            ob.EmployeeCode = null;
            ob.CompanyGroupId = $window.companyGroupId;
            ob.CompanyId = $window.companyId;
            ob.PlantId = $window.plantId;
            ob.Fabrication = item.Fabrication;
            ob.SalesOrder = item.SalesOrder;
            ob.Style = item.Style;
            ob.TotalQty = item.TotalQty;
            ob.LineOperationBookingId = item.LineOperationBookingId;
            ob.OperatorQty = null;
            $scope.lineEmployeeAssigns.push(ob);
        });
    }
    $scope.rowEmployeeSelect = false;
    $scope.addRowEmployee = function (index) {
        $scope.rowEmployeeSelect = true;
        $scope.lineEmpAssignsIndex = index;
        $scope.popUp();
    };
    $scope.employeeList = [];
    $scope.popUp = function () {
        try {
            $scope.employeeParameters = {
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
            $scope.searchEmployeeByList = [
                {
                    name: 'Employee Code',
                    value: 'EmployeeCode'
                },
                {
                    name: 'Employee Name',
                    value: 'EmployeeName'
                },
                {
                    name: 'Given Designation',
                    value: 'GivenDesignation'
                },
                {
                    name: 'Department',
                    value: 'Department'
                }
            ];

            $scope.popUpUrl = '';
            $scope.employeeParameters.sort = '';
            $scope.employeeParameters.searchBy = '';
            $scope.popUpTitle = 'Employee';
            $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatalist';
            $scope.employeeParameters.sort = 'EmployeeCode';
            $scope.employeeParameters.searchBy = 'EmployeeCode';

            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                        //getListForm($scope.employeeList);
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', '#employeePopUp');
                    }).finally(function () {
                    });
            };

            $scope.fieldName = name;
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.addEmployee = function (ob) {
        var data = Object.assign({}, ob);
        if (checkExisting(ob.SystemId) === true) {
            return ShowResult("This employee is already added in this line.", 'failure', 'employeePopUp');
        }
        if ($scope.rowEmployeeSelect & $scope.lineEmpAssignsIndex !== -1) {
            $scope.lineEmployeeAssigns[$scope.lineEmpAssignsIndex].EmployeeId = data.SystemId;
            $scope.lineEmployeeAssigns[$scope.lineEmpAssignsIndex].EmployeeName = data.EmployeeName;
            $scope.lineEmployeeAssigns[$scope.lineEmpAssignsIndex].EmployeeCode = data.EmployeeCode;
            $scope.lineEmployeeAssigns[$scope.lineEmpAssignsIndex].Employee = data.EmployeeName + ' - ' + data.EmployeeCode;
        } else {
            angular.forEach($scope.salesOrderList, function (item) {
                var ob = {};
                ob.Id = null;
                ob.CompanyGroupId = $window.companyGroupId;
                ob.CompanyId = $window.companyId;
                ob.PlantId = $window.plantId;
                ob.EmployeeId = data.SystemId;
                ob.EmployeeName = data.EmployeeName;
                ob.EmployeeCode = data.EmployeeCode;
                ob.Employee = data.EmployeeName + ' - ' + data.EmployeeCode;
                ob.Fabrication = item.Fabrication;
                ob.SalesOrder = item.SalesOrder;
                ob.Style = item.Style;
                ob.TotalQty = item.TotalQty;
                ob.LineOperationBookingId = item.LineOperationBookingId;
                ob.OperatorQty = null;
                $scope.lineEmployeeAssigns.push(ob);
            });
        }
        $scope.lineEmpAssignsIndex = -1;
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    function checkExisting(id) {
        for (var i = 0; i < $scope.lineEmployeeAssigns.length; i++) {
            var ob = $scope.lineEmployeeAssigns[i];
            if (baseService.isUndefinedOrNull(ob.EmployeeId) && !baseService.isUndefinedOrNull(ob.TempEmployeeId)) {
                if (ob.TempEmployeeId === id) {
                    return true;
                }
            } else {
                if (ob.EmployeeId === id) {
                    return true;
                }
            }
        }
        return false;
    }
    $scope.employeeValueClear = function (index) {
        $scope.lineEmployeeAssigns[index].EmployeeId = null;
        $scope.lineEmployeeAssigns[index].EmployeeName = null;
        $scope.lineEmployeeAssigns[index].Employee = null;
        $scope.lineEmployeeAssigns[index].EmployeeCode = null;
        $scope.$broadcast('angucomplete-alt:clearInput', 'autocomplete-' + index);
    };
    //#autocompleate
    $scope.autoList = [];
    $scope.getSearchAuto = function (str) {
        $http.get("Employees/approvalconfiguration/GetEmployeeDataWithEmployeeCode?employeeCode=" + str)
            .then(function (response) {
                $scope.autoList = response.data.results;
            });
    }
    $scope.SelectedAutoCompleateGroup = function (selected) {
        if (selected) {
            $scope.lineEmployeeAssigns[$scope.autoEmpIndex].EmployeeId = selected.originalObject.SystemId;
            $scope.lineEmployeeAssigns[$scope.autoEmpIndex].EmployeeCode = selected.originalObject.EmployeeCode;
            $scope.lineEmployeeAssigns[$scope.autoEmpIndex].EmployeeName = selected.originalObject.EmployeeName;
        }
    };
    $scope.autoEmpIndex = -1;
    $scope.inputChanged = function () {
        $scope.autoEmpIndex = this.$parent.$index;
    };
    //#endregion
    $scope.addNewRow = function (data, index) {
        var newObj = {};
        angular.copy(data, newObj);
        newObj.Id = null;
        newObj.EmployeeId = null;
        newObj.EmployeeName = null;
        newObj.EmployeeCode = null;
        newObj.TempEmployeeId = null;
        newObj.TempEmployeeName = null;
        newObj.TempEmployeeCode = null;
        newObj.Employee = null;
        $scope.lineEmployeeAssigns.splice(index + 1, 0, newObj);
        getEmpTarget($scope.lineEmployeeAssigns, data.LineOperationBookingId);
    };
    //#end region
    function getDataForSave() {
        $scope.lineEmployeeAssignsSaveList = [];
        $scope.tempLineEmpAssignList = [];
        angular.forEach($scope.lineEmployeeAssigns, function (item) {
            if (baseService.isUndefinedOrNull(item.EmployeeId) && !baseService.isUndefinedOrNull(item.TempEmployeeId)) {
                item.EmployeeId = item.TempEmployeeId;
                item.OperatorQty = item.AssignActualProduction === undefined ? item.ProductionQty : item.AssignActualProduction;
                item.PlantId = $window.plantId;
                if (baseService.valueCheckInList($scope.lineEmployeeAssignsSaveList, 'EmployeeId', item.EmployeeId)) {
                    throw item.EmployeeName + " found multiple";
                }
                $scope.lineEmployeeAssignsSaveList.push(item);
            }
            else {
                if (!baseService.isUndefinedOrNull(item.EmployeeId)) {
                    item.OperatorQty = item.AssignActualProduction === undefined ? item.ProductionQty : item.AssignActualProduction;
                    item.PlantId = $window.plantId;
                    if (baseService.valueCheckInList($scope.lineEmployeeAssignsSaveList, 'EmployeeId', item.EmployeeId)) {
                        throw item.EmployeeName + " found multiple";
                    }
                    $scope.lineEmployeeAssignsSaveList.push(item);
                }
            }
        });
        angular.forEach($scope.lineEmployeeAssignsSaveList, function (item) {
            if (!baseService.valueCheckInList($scope.tempLineEmpAssignList, 'LineOperationBookingId', item.LineOperationBookingId)) {
                $scope.tempLineEmpAssignList.push(item);
            }
        });
    }
    function ceckExist(list, value) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].LineOperationBookingId === value) {
                return true;
                break;
            }
        }
        return false;
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.lineEmployeeAssignNewForm.$valid) {
                getDataForSave();
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'lineEmployeeAssign': $scope.lineEmployeeAssignsSaveList
                        , 'tempLineEmployeeAssign': $scope.tempLineEmpAssignList
                        , 'date': $filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate)
                        , 'line': document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getEmpAssignList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //Deleting Rows from RetentionAllowanceList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempEmpOb = data;
        $scope.empIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id)) {
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.OperationName + ' ]';
        } else {
            $scope.message_confirmation = 'Are you sure want to parmenently delete [ ' + data.OperationName + ' ]';
        }
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id)) {
            $scope.lineEmployeeAssigns.splice($scope.empIndex, 1);
            getEmpTarget($scope.lineEmployeeAssigns, $scope.tempEmpOb.LineOperationBookingId);
            $scope.empIndex = -1;
            $scope.tempEmpOb.Id = null;
        } else {
            $scope.removeFromDb($scope.tempEmpOb.Id);
        }
        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
    };
    $scope.removeFromDb = function (id) {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/LineEmployeeAssign/Delete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.lineEmployeeAssigns.splice($scope.empIndex, 1);
                    getEmpTarget($scope.lineEmployeeAssigns, $scope.tempEmpOb.LineOperationBookingId);
                    $scope.empIndex = -1;
                    $scope.tempEmpOb.Id = null;
                    $scope.Save();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {
        $scope.lineEmployeeAssign = {};
        $scope.lineEmployeeAssignNew = {};
        $scope.lineEmployeeAssignHeadList = [];
        $scope.popUpList = [];
        $scope.valueData = [];
    }
    $scope.getReport = function () {
        location.href = '/OrderManagements/LineEmployeeAssign/ReportLineEmployeeAssign?date=' + $filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy') + '&line=' + document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text;
    };
    $scope.getEmpReport = function () {
        location.href = '/OrderManagements/LineEmployeeAssign/ReportEmployee?fromdate=' + $filter('dateFiltering')(new Date('01-Aug-2018'), 'dd-MM-yyyy') + '&todate=' + $filter('dateFiltering')(new Date('13-Aug-2018'), 'dd-MM-yyyy');
    };
}
