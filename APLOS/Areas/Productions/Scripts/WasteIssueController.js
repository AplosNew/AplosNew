'use strict';
WasteIssueController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WasteIssueController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Waste Issue';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/WasteIssue/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $scope.downloadgriddataUrlPath = 'Productions/WasteIssue/DownloadUsingFullPath';
    baseService.init($scope.getListUrl);
    $scope.searchBy = null; $scope.search = null;


    $scope.EntityList = [];

    $scope.getsE = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEntity',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });
    }



    $scope.ModelTemp = {
        Id: 0,
        WasteMasterId: null,
        /* IssueId: null,*/
        Entity: null,
        EntityId: null,
        PreparedById: null,
        PreparedByCode: null,
        PreparedBy: null,
        ApprovedById: null,
        ApprovedByCode: null,
        ApprovedBy: null,
        CheckedById: null,
        CheckedByCode: null,
        CheckedBy: null,
        Waste: null,
        Purpose: null,
        Date: null,
        Remarks: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    //$scope.ModelWaste = {
    //    Id: null,
    //    WasteTransactionDataId: null,
    //    WasteIssueId: null,
    //    IssueQty: null,
    //    Rate: null,
    //    IssueValue: null,
    //    ProcessId: null,
    //    Process: null,
    //    Remarks: null
    //};
    //$scope.ModelWasteIssue = Object.assign({}, $scope.ModelWaste);

    

    $scope.WasteDetailDataList = [];

    $scope.Save = function () {

        try {
            var tempItem = [];

            for (var i = 0; i < $scope.WasteDetailDataList.length; i++) {
                if ($scope.WasteDetailDataList[i].Active) {
                    tempItem.push($scope.WasteDetailDataList[i]);
                }
            }

            $scope.$broadcast('show-errors-check-validity');

            if ($scope.ModelNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.ModelNew, 'WasteData': tempItem },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ModelNew.Id = response.data.Id;
                        $scope.Get();
                        $scope.GetWasteMaster();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }

    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Get();
                    $scope.GetWasteMaster();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.ModelNew = {
            Id: 0,
            WasteMasterId: null,
            /* IssueId: null,*/
            Entity: null,
            EntityId: null,
            PreparedById: null,
            PreparedByCode: null,
            PreparedBy: null,
            ApprovedById: null,
            ApprovedByCode: null,
            ApprovedBy: null,
            CheckedById: null,
            CheckedByCode: null,
            CheckedBy: null,
            Waste: null,
            Purpose: null,
            Date: null,
            Remarks: null,
        };
        $scope.WasteDetailDataList = [];
        $scope.Action = 'Save';
    };

        
    // Addition of the Modal Operations for Budget Child
    $scope.closeBudPopUp = function () {
        angular.element(document.querySelector('#BudgetPop')).modal('hide');
    }



    $scope.selectEntity = function () {
        $scope.getsE();
        angular.element(document.querySelector('#EntityPop')).modal('show');
    }


    $scope.doubleEntity = function (e) {
        $scope.ModelNew.EntityId = e.data.EntityId;
        $scope.ModelNew.Entity = e.data.EntityName;
        angular.element(document.querySelector('#EntityPop')).modal('hide');
        $scope.Get();
    }

    $scope.WasteDetailDataList = [];
    $scope.Get = function () {
        try {
            $scope.WasteDetailDataList = [];

            $http({
                method: 'POST',
                url: $scope.path + "GetWaste",
                data: { 'entityId': $scope.ModelNew.EntityId, 'Id': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.WasteDetailDataList = response.data;

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#EntityPop')).modal('hide');
    }

    function CheckBoxSelectAllWasteWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#wasteInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.WasteDetailDataList.length; i++) {
                $scope.WasteDetailDataList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#wasteInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    function checkChangeWaste(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.WasteBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }

    function headCheckChangeWaste(e) {
        if (e.model.checkState == "check") {

            var filtered = $("#GridWaste").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.WasteDetailDataList.length; i++) {

                    $scope.WasteDetailDataList[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.WasteDetailDataList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.WasteDetailDataList[i].Id == filtered[j].Id)
                            // $scope.ModelList[i].isSelect = true;
                            $scope.WasteDetailDataList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#GridWaste .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "change": checkChangeWaste });
            }
        }
        else {
            var filtered = $("#GridWaste").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.WasteDetailDataList.length; i++) {
                    $scope.WasteDetailDataList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.WasteDetailDataList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.WasteDetailDataList[i].Id == filtered[j].Id)
                            $scope.WasteDetailDataList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#GridWaste.rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridWaste.rowCheckbox")[i]).ejCheckBox({ "change": checkChangeWaste });
            }
        }
        //header level check
    }

    $scope.dataBoundWaste = function (args) {
        $("#GridWaste .rowCheckbox").ejCheckBox({ "change": checkChangeWaste });
        $("#headchk").ejCheckBox({ "change": headCheckChangeMeeting });

    };

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

    $scope.Name = null;
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;

        if ($scope.Name == 'Prepared') {
            $scope.ModelNew.PreparedById = data.SystemId;
            $scope.ModelNew.PreparedBy = data.EmployeeName;
            $scope.ModelNew.PreparedByCode = data.EmployeeCode;
        }
        else if ($scope.Name == 'Approved') {
            $scope.ModelNew.ApprovedById = data.SystemId;
            $scope.ModelNew.ApprovedBy = data.EmployeeName;
            $scope.ModelNew.ApprovedByCode = data.EmployeeCode;
        }
        else {
            $scope.ModelNew.CheckedById = data.SystemId;
            $scope.ModelNew.CheckedBy = data.EmployeeName;
            $scope.ModelNew.CheckedByCode = data.EmployeeCode;
        }

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };

    $scope.processSearchList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Local Name',
            'value': 'LocalName'
        },
        {
            'name': 'Alias',
            'value': 'Alias'
        }
    ];

    $scope.processPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ShowpProcessPopUp = function (obj) {
        $scope.wasteDetailData = obj;
        $scope.popUpProcessUrl = 'Processes/Process/GetProductionProcessList';
        $scope.getProcessData = function (pageno) {
            baseService.paginationBase($scope.popUpProcessUrl, pageno, $scope.processPopUpParameters)
                .then(function (result) {
                    $scope.processPopUpDataList = result.Rows;
                    $scope.processPopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };

    $scope.processAdd = function (data) {
        var gridObj = $("#wasteInfoGrid").data("ejGrid");
        var Selecteddata = gridObj.getSelectedRecords()[0];
        Selecteddata.ProcessId = data.Id;
        Selecteddata.Process = data.UserName;
        gridObj.refreshContent();
        gridObj.refreshTemplate();
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    $scope.ShowpIssueQuantityCal = function (obj) {
        obj.data.BalanceStock = obj.data.StockQty - (parseFloat(obj.data.IssueQty) + obj.data.OtherQty);
        obj.data.IssueValue = obj.data.Rate * parseFloat(obj.data.IssueQty);
        obj.data.BalanceStkValue = obj.data.StdValue - obj.data.IssueValue;
    }

    $scope.GetWasteUpd = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
       
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Get();
    };


    $scope.GetWasteMaster = function () {
        try {
            $scope.ModelList = [];

            $http({
                method: 'POST',
                url: $scope.path + "GetWasteMasterData",
                //data: {'Id': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.ModelList = response.data;

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetWasteMaster();


    $scope.Report = function (obj) {
        try {
            $scope.fileName = "WasteReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetWasteReport",
                data: { 'Id': obj.data.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {

                if (response.data.Error == false) {
                    //$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }
}