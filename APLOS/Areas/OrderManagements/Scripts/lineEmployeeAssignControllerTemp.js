'use strict';
lineEmployeeAssignControllerTemp.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function lineEmployeeAssignControllerTemp(fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Line Operator Assign";
    $scope.Action = 'Save';
    $scope.lineEmployeeAssigns = [];
    $scope.employeeAssignList = [];
    $scope.path = 'OrderManagements/LineEmployeeAssign/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.lineEmployeeAssign = {
        Id: null,
        EmployeeId: null,
        OperationDate: null,
        OperationId: null,
        LineId: null,
        SalesOrderId: null,
        LineOperationBookingId: null,
        OperatorQty: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId:null
    };
    $scope.lineEmployeeAssignNew = Object.assign({}, $scope.lineEmployeeAssign);
    $scope.lineList = [];
    $scope.getLineCbo = function () {
        $scope.lineEmployeeAssigns = [];
        cboService.getLineCbo($filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy'), function (result) {
            $scope.lineList = result;
        });
    }
    $scope.operationList = [];
    $scope.getOperationList = function () {
        $scope.lineEmployeeAssigns = [];
        cboService.getOperationCbo($filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy'), document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text, function (result) {
            $scope.operationList = result;
        });
    }
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
                    $scope.employeeAddConfirmModal()
                } else {
                    addSalesOrderData();
                }
            }
        }), function errorCallBack(response) {
            showResult(response.data.Message, 'failure');
        }
    }
    $scope.salesOrderOb = {};
    $scope.getSoData = function (value) {
        $scope.salesOrderOb = $.grep($scope.salesOrderList, function (item) {
            return item.Value == value;
        })[0];
    };
    $scope.getEmpAssignList = function (id) {
        $http({
            method: "GET",
            url: $scope.getListUrl,
            params: {
                'date': $filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy'),
                'line': document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text,
                'operationName': document.getElementById("OperationId").options[document.getElementById('OperationId').selectedIndex].text
            },
            dataType: "json"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.lineEmployeeAssigns = response.data;
            }
        }), function errorCallBack(response) {
            showResult(response.data.Message, 'failure');
        }
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
                        getListForm($scope.employeeList);
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
    $scope.addEmployee = function (data) {
        if ($scope.rowEmployeeSelect & $scope.lineEmpAssignsIndex != -1) {
            $scope.lineEmployeeAssigns[$scope.lineEmpAssignsIndex].EmployeeId = data.SystemId;
            $scope.lineEmployeeAssigns[$scope.lineEmpAssignsIndex].EmployeeName = data.EmployeeName;
        } else {
            angular.forEach($scope.salesOrderList, function (item) {
                var ob = {};
                ob.Id = null;
                ob.CompanyGroupId = $window.companyGroupId;
                ob.CompanyId = $window.companyId;
                ob.PlantId = $window.plantId;
                ob.EmployeeId = data.SystemId;
                ob.EmployeeName = data.EmployeeName;
                ob.Fabrication = item.Fabrication;
                ob.SalesOrder = item.SalesOrder;
                ob.Style = item.Style;
                ob.TotalQty = item.TotalQty;
                ob.LineOperationBookingId = item.LineOperationBookingId;
                ob.OperatorQty = null;
                $scope.lineEmployeeAssigns.push(ob);
            })
        }
        $scope.lineEmpAssignsIndex = -1;
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    function checkExisting(id) {
        for (var i = 0; i < $scope.lineEmployeeAssigns.length; i++) {
            var ob = $scope.lineEmployeeAssigns[i];
            if (ob.EmployeeId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.employeeValueClear = function (index) {
        $scope.lineEmployeeAssigns[index].EmployeeId = null;
        $scope.lineEmployeeAssigns[index].EmployeeName = null;
    };

    //#end region
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.lineEmployeeAssignNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: $scope.lineEmployeeAssigns,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
    };
    //Deleting Rows from RetentionAllowanceList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempEmpOb = data;
        $scope.empIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.SalesOrder + ' ]';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id) === true) {
            $scope.lineEmployeeAssigns.splice($scope.empIndex, 1);
        } else {
            $scope.removeFromDb($scope.tempEmpOb.Id, $scope.empIndex);
        }
        $scope.empIndex = -1;
        $scope.tempEmpOb.Id = null;
        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
    };
    $scope.removeFromDb = function (id, index) {
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
                    $scope.empIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.Get = function (id, index) {
        $scope.getDetailList(id);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields(seq) {
        $scope.lineEmployeeAssign = {};
        $scope.lineEmployeeAssignNew = {};
        $scope.lineEmployeeAssignHeadList = [];
        $scope.popUpList = [];
        $scope.valueData = [];
    }


    //#region
    $("#upload").change(function () {
        $scope.filedata = this.files[0];
    });
    $scope.getFile = function () {
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {

            });
    };
    $scope.getExcelData = function () {
        var formData = new FormData();
        $http({
            method: 'POST',
            url: $scope.path + 'GetExcelData',
            headers: { 'Content-Type': undefined },
            transformRequest: function (data) {
                formData.append('file', data.file);
                return formData;
            },
            data: {
                'file': $scope.filedata
            }
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                document.getElementById("upload").value = '';
            }
        }, function errorCallback(response) {
            ShowResult(response.Message, 'failure');
        });
    };
    $scope.getReport = function () {
        location.href = '/OrderManagements/LineEmployeeAssign/ReportLineEmployeeAssign?date=' + $filter('dateFiltering')($scope.lineEmployeeAssignNew.OperationDate, 'dd-MM-yyyy') + '&line=' + document.getElementById("LineId").options[document.getElementById('LineId').selectedIndex].text;
    };
    //#endregion 
}
