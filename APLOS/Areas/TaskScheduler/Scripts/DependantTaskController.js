'use strict';
DependantTaskController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DependantTaskController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Dependant Task';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'TaskScheduler/DependantTask/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChildUrl = $scope.path + 'CreateChild';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    //for tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetDependantTaskDetailData();
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
                data: { 'data': $scope.ModelNew },
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
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.ChildModelList = [];
    }

    // #region  Dynamic PopUp
    $scope.popUpList = [];
    $scope.popUpDataList = [];
    $scope.GetByWhomPopupData = function () {
        try {

            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#EmpPopUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.selectdblClick = function (obj) {
        var ob = obj.data;
        $scope.ModelChildNew.ResponsiblePersonId = ob.SystemId;
        $scope.ModelChildNew.ResponsiblePerson = ob.EmployeeName;
        $scope.ModelChildNew.ResponsiblePersonCode = ob.EmployeeCode;
        angular.element(document.querySelector('#EmpPopUp')).modal('hide');
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#EmpPopUp')).modal('hide');
    };

    
    // #endregion


    $scope.ModelChildTempNew = { Id: null, DependantTaskId: null, DependantDate: null, DependantTaskDetailId: null, LegDays:0, TaskDependantLegDays: 0, ResponsiblePersonId: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP:null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    $scope.ModelChildNew = Object.assign({}, $scope.ModelChildTempNew);

    $scope.DependantTaskList = [];
    $scope.getDependantTaskData = function () {
        $http({
            method: 'GET',
            url: 'TaskScheduler/DependantTask/GetDependantTaskDetailData?masterId=' + $scope.ModelNew.Id

        }).then(function successCallback(response) {
            $scope.DependantTaskList = response.data.master;
            angular.element(document.querySelector('#DependantPopUp')).modal('show');
        });
    }

    $scope.selectdblClickDependant = function (obj) {
        var ob = obj.data;
        $scope.ModelChildNew.DependantTaskDetailId = ob.Id;
        $scope.ModelChildNew.TaskName = ob.Task;
        angular.element(document.querySelector('#DependantPopUp')).modal('hide');
    };

    $scope.closeDependantPopUp = function () {
        angular.element(document.querySelector('#DependantPopUp')).modal('hide');
    };

    $scope.SaveChild = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewChildForm.$valid) {
            $scope.ModelChildNew.DependantTaskId = $scope.ModelNew.Id;
            $http({
                method: 'POST',
                url: $scope.saveChildUrl,
                data: { 'data': $scope.ModelChildNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearChild();
                    $scope.GetDependantTaskDetailData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.ClearChild = function () {
        $scope.ModelChildNew = Object.assign({}, $scope.ModelChildTempNew);
    };

    $scope.ChildModelList = [];
    $scope.GetDependantTaskDetailData = function () {
        try {
            $http({
                method: 'GET',
                url: 'TaskScheduler/DependantTask/GetDependantTaskDetailData?masterId=' + $scope.ModelNew.Id

            }).then(function successCallback(response) {
                $scope.ChildModelList = response.data.master;
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetChild = function (args) {
        $scope.ModelChildNew = Object.assign({}, args.data);
    };


    $scope.deleteChildUrl = 'TaskScheduler/DependantTask/DeleteDependantTaskDetail';
    $scope.DeleteChild = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelChildNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteChildUrl + $scope.ModelChildNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearChild();
                    $scope.GetDependantTaskDetailData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

}