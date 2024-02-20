'use strict';
WorkcenterWiseDetentionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WorkcenterWiseDetentionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Workcenter Wise Detention';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'IE/WorkcenterWiseDetention/';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.year = new Date().getFullYear().toString();

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    $scope.ModelTransaction = {
        Id: null,
        EntityId: null,
        Entity: null,
        DetentionId: null,
        FromTime: null,
        ToTime: null,
        Date: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        ProcessId: null,
        Process: null,
        ShiftId: null,
        Shift: null,
        //IfAssetApplicable: false,

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTransaction);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetWorkcenter();
        $scope.getMinute();

        //$scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.GetSavedWorkCenterForUpdate(args.data.EntityId, args.data.DetentionId, args.data.ProcessId, args.data.Date, args.data.ShiftId, args.data.Minute);
        }
    };

    $scope.MachineMasterDateForUpdate = [];
    $scope.GetSavedWorkCenterForUpdate = function (entityid, detentionid, processid, date, shiftid, minute) {
        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.path + 'GetSavedWorkCenterForUpdate',
            data: {
                'entityid': entityid,
                'detentionid': detentionid,
                'processid': processid,
                'date': date,
                'shiftid': shiftid,
                'minute': minute
            }
        }).then(function successCallback(response) {
            $scope.MachineMasterDateForUpdate = response.data;
        });

    }

    $scope.GriddataMachineMasterData = [];
    $scope.getData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.path + 'GetMachineMasterTransaction',
        }).then(function successCallback(response) {
            $scope.GriddataMachineMasterData = response.data;
        });
    };
    $scope.getData();

    // #region Shift
    $scope.selectShift = function () {
        $scope.getsS();
        angular.element(document.querySelector('#ShiftPop')).modal('show');
    }

    $scope.ShiftList = [];
    $scope.getsS = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getShift',
            data: {
                'processid': $scope.ModelNew.ProcessId
            },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }

    $scope.doubleShift = function (e) {
        $scope.ModelNew.ShiftId = e.data.ShiftId;
        $scope.ModelNew.Shift = e.data.ShiftDefination;
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }
    // #endregion Shift

    // #region Entity
    $scope.EntityList = [];
    $scope.selectEntity = function () {
        $http({
            method: 'POST',

            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity",
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });
        angular.element(document.querySelector('#EntityPop')).modal('show');
    }

    $scope.doubleEntity = function (e) {
        $scope.ModelNew.EntityId = e.data.Id;
        $scope.ModelNew.Entity = e.data.UserName;
        // $scope.GetworkcenterData();
        angular.element(document.querySelector('#EntityPop')).modal('hide');
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#EntityPop')).modal('hide');
    }
    // #endregion Entity

    // #region Process
    $scope.selectProcess = function () {
        $scope.getsP();
        angular.element(document.querySelector('#ProcessPop')).modal('show');
    }

    $scope.ProcessList = [];
    $scope.getsP = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getProcess',
            // data: { 'machineMasterId': $scope.ModelNew.MachineMasterId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;

        });
    }

    $scope.doubleProcess = function (e) {
        $scope.ModelNew.ProcessId = e.data.Id;
        $scope.ModelNew.Process = e.data.Process;
        //$scope.GetworkcenterData();
        angular.element(document.querySelector('#ProcessPop')).modal('hide');
        $scope.GetWorkcenter();
    }

    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#ProcessPop')).modal('hide');
    }
    // #endregion Process

    // #region Detention
    $scope.DetentionList = [];
    $scope.GetDetention = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetDetentionMaster',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.DetentionList = resp.data;

        });

    }
    $scope.GetDetention();

    $scope.SelectedDetentionInGrid = function () {
        for (var i = 0; i < $scope.WorkcenterList.length; i++) {
            for (var j = 0; j < $scope.DetentionList.length; j++) {
                if ($scope.DetentionList[j].Value == $scope.ModelNew.DetentionId) {
                    $scope.WorkcenterList[i].Detention = $scope.DetentionList[j].Text;
                }
            }
            // $scope.WorkcenterList[i].Detention = $scope.ModelNew.DetentionId;
        }
    }



    // #endregion Detention

    // #region CalcTime
    $scope.getMinute = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.ModelNew.FromTime) && !baseService.isUndefinedOrNull($scope.ModelNew.ToTime)) {
                $scope.MinuteUrl = 'IE/MachineMasterTransaction/GetMinute/'
                $http({
                    method: 'POST',
                    url: $scope.MinuteUrl,
                    data: { 'data': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {

                    for (var i = 0; i < $scope.WorkcenterList.length; i++) {
                        $scope.WorkcenterList[i].Minute = response.data;
                    }

                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    // #endregion CalcTime

    // #region Workcenter
    $scope.WorkcenterList = [];
    $scope.GetWorkcenter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetWorkcenter',
            data: {
                'entityid': $scope.ModelNew.EntityId,
                'processid': $scope.ModelNew.ProcessId,
                'headerid': $scope.ModelNew.Id,
                'detentionId': $scope.ModelNew.DetentionId,
                'date': $scope.ModelNew.Date

            },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.WorkcenterList = resp.data;

        });
    }
    //$scope.GetWorkcenter();
    // #endregion Workcenter

    // #region Employee popup
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

        $scope.ModelNew.ResponsiblePersonId = data.SystemId;
        $scope.ModelNew.ResponsiblePerson = data.EmployeeName;
        $scope.ModelNew.ResponsiblePersonCode = data.EmployeeCode;

        for (var i = 0; i < $scope.WorkcenterList.length; i++) {
            $scope.WorkcenterList[i].ResponsiblePerson = data.EmployeeName;
        }

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };

    // #endregion Employee popup

    // #region Save
    $scope.refreshTemplateWorkcenter = function (args) {
        $("#Workcenterheadchk").ejCheckBox({ "change": CheckBoxSelectAllWorkcenter });
    };

    function CheckBoxSelectAllWorkcenter(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#workcenterGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.WorkcenterList.length; i++) {
                $scope.WorkcenterList[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#workcenterGrid").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
    };

    $scope.CheckedDetentionWorkList = [];
    $scope.Save = function () {

        if ($scope.ModelNewForm.$valid) {
            for (var i = 0; i < $scope.WorkcenterList.length; i++) {

                if ($scope.WorkcenterList[i].isSelected) {
                    $scope.CheckedDetentionWorkList.push($scope.WorkcenterList[i]);
                    for (var j = 0; j < $scope.CheckedDetentionWorkList.length; j++) {
                        $scope.CheckedDetentionWorkList[j].EntityId = $scope.ModelNew.EntityId;
                        $scope.CheckedDetentionWorkList[j].DetentionId = $scope.ModelNew.DetentionId;
                        $scope.CheckedDetentionWorkList[j].FromTime = $scope.ModelNew.FromTime;
                        $scope.CheckedDetentionWorkList[j].ToTime = $scope.ModelNew.ToTime;
                        $scope.CheckedDetentionWorkList[j].Date = $scope.ModelNew.Date;
                        $scope.CheckedDetentionWorkList[j].ProcessId = $scope.ModelNew.ProcessId;
                        $scope.CheckedDetentionWorkList[j].ShiftId = $scope.ModelNew.ShiftId;
                        $scope.CheckedDetentionWorkList[j].CalculatedTime = $scope.ModelNew.CalculatedTime;
                        $scope.CheckedDetentionWorkList[j].ResponsiblePersonId = $scope.ModelNew.ResponsiblePersonId;
                        $scope.CheckedDetentionWorkList[j].Remark = $scope.ModelNew.Remark;
                    }

                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {

                    'data': $scope.CheckedDetentionWorkList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.Clear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            }
        }


    };

    // #endregion Save

    // #region Delete
    $scope.Delete = function () {
        //for (var i = 0; i < $scope.MachineMasterDateForUpdate.length; i++) {

        //    if ($scope.MachineMasterDateForUpdate[i].isSelected) {
        //        $scope.CheckedDetentionWorkList.push($scope.MachineMasterDateForUpdate[i]);
        //        for (var j = 0; j < $scope.CheckedDetentionWorkList.length; j++) {
        //            $scope.CheckedDetentionWorkList[j].Id = $scope.ModelNew.EntityId;
        //            $scope.CheckedDetentionWorkList[j].DetentionId = $scope.ModelNew.DetentionId;
        //            $scope.CheckedDetentionWorkList[j].FromTime = $scope.ModelNew.FromTime;
        //            $scope.CheckedDetentionWorkList[j].ToTime = $scope.ModelNew.ToTime;
        //            $scope.CheckedDetentionWorkList[j].Date = $scope.ModelNew.Date;
        //            $scope.CheckedDetentionWorkList[j].ProcessId = $scope.ModelNew.ProcessId;
        //            $scope.CheckedDetentionWorkList[j].ShiftId = $scope.ModelNew.ShiftId;
        //            $scope.CheckedDetentionWorkList[j].CalculatedTime = $scope.ModelNew.CalculatedTime;
        //            $scope.CheckedDetentionWorkList[j].ResponsiblePersonId = $scope.ModelNew.ResponsiblePersonId;
        //            $scope.CheckedDetentionWorkList[j].Remark = $scope.ModelNew.Remark;
        //        }

        //    }
        //}
        $http({
            method: 'POST',
            url: $scope.deleteUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
                $scope.Clear();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });

    };
    // #endregion Delete

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = {
            Id: null,
            EntityId: null,
            Entity: null,
            DetentionId: null,
            FromTime: null,
            ToTime: null,
            Date: null,
            ResponsiblePersonId: null,
            ResponsiblePerson: null,
            ProcessId: null,
            Process: null,
            ShiftId: null,
            Shift: null,
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTransaction);
        $scope.WorkcenterList = [];

    }

    $scope.DateValidation = function (ProductionDate) {
        try {
            var date = new Date();
            date.setDate(date.getDate() - 1);
            $scope.Yestarday = $filter('dateFiltering')(date);
            $scope.ProdDate = $filter('dateFiltering')(ProductionDate);
            if ($scope.ProdDate < $scope.Yestarday) {
                throw "Date must be allow only Yestarday's Date!";
            }
            if (new Date(ProductionDate) > new Date()) {
                throw "Date must be equal to current Date!";
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    };
}