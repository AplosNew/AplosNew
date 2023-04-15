'use strict';
WCWorkStationsControlMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WCWorkStationsControlMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'WC/Work Stations Control Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/WCWorkStationsControlMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlColumns = $scope.path + 'createColumns';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = null; $scope.search = null;
    $scope.processList = [];

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields();
        });
    }
    $scope.getData();


    $scope.getProcess = function () {
        $http({
            method: 'GET',
            url: $scope.path + "getProcess",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    }

    $scope.getProcess();




    $scope.ModelTemp = {
        Id: null,
        StandardName: null,
        UserName: null,
        ResponsiblePerson: null,
        ResponsiblePersonId: null,
        ProcessId: null,
        IsActive: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.selectResponsiblePerson = function () {
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.ModelNew.ResponsiblePersonId = e.data.SystemId;
        $scope.ModelNew.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }


    $scope.Get = function (args) {

        var AllData = [];
        $http({
            method: 'POST',
            url: $scope.path + "Get",
            data: { 'Id': args.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(resp) {

            AllData = resp.data.master;
            $scope.ModelNew = Object.assign({}, AllData[0]);
            $scope.LoadWSMColumnsDetails($scope.ModelNew.Id,$scope.ModelNew.ProcessId);
        });
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'datas': $scope.ModelNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.removeModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempmasterId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveMaster')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };


    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.tempmasterId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.tempmasterId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.WSMColumnsList = [];
    $scope.LoadWSMColumnsDetails = function (mid,pid) {
        $http({
            method: 'POST',
            url: 'Productions/WCWorkStationsControlMaster/LoadWSMColumnsDetails?MasterId=' + mid + '&ProcessId=' + pid
        }).then(function successCallback(response) {
            $scope.WSMColumnsList = response.data;
        }
        )
    }

    $scope.refreshTemplateColumns = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllColumns });
    };
    function CheckBoxSelectAllColumns(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridColumns").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.WSMColumnsList.length; i++) {
                $scope.WSMColumnsList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridColumns").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SaveColumnsInfo = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.WSMColumnsList.length; i++) {
                /*if ($scope.WSMColumnsList[i].Active == true) {*/
                    $scope.WSMColumnsList[i].WSMId = $scope.ModelNew.Id;
                    $scope.WSMColumnsList[i].ProcessId = $scope.ModelNew.ProcessId;
                    $scope.SaveList.push($scope.WSMColumnsList[i]);
               /* }*/
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlColumns,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadWSMColumnsDetails($scope.ModelNew.Id, $scope.ModelNew.ProcessId);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
}