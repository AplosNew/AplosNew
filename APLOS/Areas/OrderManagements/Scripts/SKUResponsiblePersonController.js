'use strict';
SKUResponsiblePersonController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function SKUResponsiblePersonController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Order Monitors";
    $scope.Action = 'Save';
    $scope.OrderMonitorsList = [];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.getSavedEmpData = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetSKUResPersonList'
        }).then(function successCallback(response) {
            $scope.OrderMonitorsList = response.data;

        });
    };
    $scope.getSavedEmpData();

    $scope.ModelTemp = {
        Id: null,
        Status: 'Active',
        ResponsiblePersonId: null,
        TargetDate: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.processCboList = [];
    cboService.getProductionProcessCbo(function (response) {
        $scope.processCboList = response;
    });

    $scope.LevelList = [
        {
            'Text': 'Level1',
            'Value': 'Level1'
        },
        {
            'Text': 'Level2',
            'Value': 'Level2'
        },
        {
            'Text': 'Level3',
            'Value': 'Level3'
        }
    ];

    $scope.StatusList = [
        {
            'Text': 'Active',
            'Value': 'Active'
        },
        {
            'Text': 'Running',
            'Value': 'Running'
        },
        {
            'Text': 'Closed',
            'Value': 'Closed'
        }
    ];

    $scope.popUpDataList = [];
    $scope.getEmpPopUpData = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    // #region checkbox all

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPopUp").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    function MakeData() {
        for (var i = 0; i < $scope.popUpDataList.length; i++) {
            if ($scope.popUpDataList[i].Flag == true) {
                if (checkExists($scope.OrderMonitorsList, $scope.popUpDataList[i].SystemID) === false) {
                    var ob = {};
                    ob.Id = -(Math.floor(Math.random() * 1000000) + 1);
                    ob.EmpSystemId = $scope.popUpDataList[i].SystemId;
                    ob.EmployeeCode = $scope.popUpDataList[i].EmployeeCode;
                    ob.EmployeeName = $scope.popUpDataList[i].EmployeeName;
                    ob.LegalDesignation = $scope.popUpDataList[i].LegalDesignation;
                    ob.Department = $scope.popUpDataList[i].Department;
                    ob.Company = $scope.popUpDataList[i].Company;
                    ob.Plant = $scope.popUpDataList[i].Plant;
                    ob.Section = $scope.popUpDataList[i].Section;
                    ob.SubSection = $scope.popUpDataList[i].SubSection;
                    ob.Line = $scope.popUpDataList[i].Line;

                    $scope.OrderMonitorsList.push(ob);
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveEmployee = function () {
        try {
            if (baseService.arrayLength($scope.OrderMonitorsList) < 0) {
                throw "Select Employee.";
            }
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CreateSKUResPerson',
                data: { 'data': $scope.OrderMonitorsList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedEmpData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.closePopUp = function () {
        MakeData();
        $scope.SaveEmployee();
        angular.element(document.querySelector('#popUp')).modal('hide');
    };


    $scope.message_confirmation = "";
    $scope.removeEmp = function (data) {
        $scope.runobj = data.data;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.runobj.EmployeeCode + ' ]';
        angular.element(document.querySelector('#confirmRunDelPopUp')).modal('show');
    };
    $scope.DeleteEmployee = function () {
        if (!baseService.isUndefinedOrNull($scope.runobj.Id)) {
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/DeleteSKUResPerson?id=' + $scope.runobj.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedEmpData();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }

    };

    $scope.SaveRow = function (data) {
            $http({
                method: 'POST',
                url: "OrderManagements/MasterOrder/UpdateSKUResPerson",
                data: { 'data': data.data },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedEmpData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    };
  

    $scope.loadfilters = function () {
        try {
            $scope.filters = [];
            $http({
                method: 'GET',
                url:'OrderManagements/MasterOrder/getPOFilters',
                dataType: 'JSON'
            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.filters = response.data;
                    var columnList = [
                        { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
                        { field: 'ProductionStatus', width: 20, headerText: "PO Status", type: "string" },
                        { field: 'POId', width: 20, headerText: "POId", type: "string" }

                    ];
                    $("#filters").ejGrid({
                        dataSource: $scope.filters,
                        minWidth: 450, minHeight: 400,
                        allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                        filterSettings: { filterType: "excel" },
                        columns: columnList
                    });

                    var gridObj = $("#filters").data("ejGrid");
                    gridObj.refreshContent(true);
                    gridObj.refreshTemplate();
                    $("#filters").children('.e-pager.e-js.e-pager').hide();
                    $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
                    $("#filters").children('.e-gridcontent').hide();
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "CustomerId", "Value": getString(fl, "CustomerId") });
        parameters.push({ "Key": "ProductionStatusId", "Value": getString(fl, "ProductionStatusId") });
        parameters.push({ "Key": "POId", "Value": getString(fl, "POId") });
        $scope.parameters = parameters;

    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                /* var replace = data[i][column].replace(",", "','");*/
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.SalesOrderItemList = [];
    $scope.GetSalesOrder = function () {
        $scope.filterComplete();
        $http({
            method: 'POST',
            url: "OrderManagements/MasterOrder/GetSalesOrder",
            data: { 'parameters': $scope.parameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.SalesOrderItemList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.SOId = null;
    $scope.SKUItemList = [];
    $scope.GetSKUData = function (data) {
        $scope.SOId = data.data.SOId;
        $http({
            method: 'POST',
            url: "OrderManagements/MasterOrder/GetSKUData",
            data: { 'salesOrderId': $scope.SOId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.SKUItemList = response.data;
                angular.element(document.querySelector('#SKUPopUp')).modal('show');
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.ResponsiblePerson1CboList = [];
    $scope.GetRP1Data = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/MasterOrder/GetRP1Data'

            }).then(function successCallback(response) {
                $scope.ResponsiblePerson1CboList = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.GetRP1Data();

    $scope.ResponsiblePerson2CboList = [];
    $scope.GetRP2Data = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/MasterOrder/GetRP2Data'

            }).then(function successCallback(response) {
                $scope.ResponsiblePerson2CboList = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.GetRP2Data();

    $scope.ResponsiblePerson3CboList = [];
    $scope.GetRP3Data = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/MasterOrder/GetRP3Data'

            }).then(function successCallback(response) {
                $scope.ResponsiblePerson3CboList = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.GetRP3Data();

    $scope.SaveSKU = function () {
        try {
            if (baseService.arrayLength($scope.SKUItemList) > 0) {

            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/CreateSKUControl',
                data: { 'data': $scope.SKUItemList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedSKUData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetSavedSKUData = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/MasterOrder/GetSKUData",
            data: { 'salesOrderId': $scope.SOId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.SKUItemList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.closeSkuPopUp = function () {
        angular.element(document.querySelector('#SKUPopUp')).modal('hide');
    };

}

