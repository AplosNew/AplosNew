'use strict';
DetentionMasterController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function DetentionMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "DetentionMaster";
    $scope.Action = 'Save';
    $scope.path = 'Materials/DetentionMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';
    $scope.ProcesssaveUrl = $scope.path + 'CreateProcess';
    $scope.DepartmentSaveUrl = $scope.path + 'CreateDepartment';
    $scope.detention = {
        Id: null
        , DetentionCategory: null
        , DetentionSubCategory: null
        , DetentionStandaredName: null
        , DetentionUserName: null
        , DetentionType: null
        , DetentionCriticality: null
        , ResponsiblePersion: null
        , DetentionTarget: null
        , DetentionPlan: null
        , IsAvoidable: true
    };
    $scope.detentionNew = Object.assign({}, $scope.detention);

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
        //$scope.Detail.pop();
    }

    $scope.DetentionList = [];
    $scope.LoadDetentionList = function () {
        $http({

            method: 'Get',
            url: 'Materials/DetentionMaster/LoadDetentionList'
        }).then(function successCallback(response) {
            $scope.DetentionList = response.data;
        }
        )
    }
    $scope.LoadDetentionList();


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.DetentionMasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'DetentionData': $scope.detentionNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    /*ClearFields(response.data.Sequence);*/
                    $scope.LoadDetentionList();
                    DetentionClearFields();
                   /* $scope.GetDetails({ data: { Id: response.data.Data.Id } });*/
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.rackNew.Id)) {
    //        $http({
    //            method: 'POST'
    //            , url: $scope.path + 'Delete?Id=' + $scope.rackNew.Id
    //            , dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');                   
    //                ClearFields(response.data.Sequence);
    //                $scope.LoadRackList();
    //            }
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        });
    //    }
    //};
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.GetDetails = function (args) {
        $scope.DetentionMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'Materials/DetentionMaster/LoadEditData?DetentionID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.detentionNew = response.data.detention[0];
           
            $scope.getDetentionMasterProcess();
            $scope.getDetentionMasterDepartment();
            
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.recorddoubleclick = function ($event) {
        debugger;
        var x = $event;
        $scope.DetentionMasterId = x.data.Id;

        // $scope.modelNew.OperationMasterIdID = response.data.Id;  
        /* $scope.GetDataByMasterOrderIdfn($scope.DetentionMasterId);*/
        // $scope.GetDataByMasterOrderIdfnMP($scope.OMId);
        $scope.Action = 'Update';
        $scope.getDetentionMasterProcess();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.getDetentionMasterProcess = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getProcess',
            data: { 'DetentionMasterId': $scope.DetentionMasterId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.userProcessList = [];
            $scope.userProcessList = resp.data;
        });
    }
    $scope.getDetentionMasterDepartment = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getDepartment',
            data: { 'DetentionMasterId': $scope.DetentionMasterId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.userDepartMentList = [];
            $scope.userDepartMentList = resp.data;
        });
    }
    $scope.Clear = function () {
        DetentionClearFields();
        $scope.userDepartMentList = [];
        $scope.userProcessList = [];
    };
    function DetentionClearFields() {
        $scope.Action = "Save";
        $scope.detentionNew = Object.assign({}, $scope.detention);

    }
//    function ClearFields(seq) {
//        $scope.Action = "Save";
//        $scope.detentionNew = Object.assign({}, $scope.detention);
///*        $scope.rackNew.Sequence= seq;*/
//        $scope.binList =[];

//    }

    $scope.processPopUpDataList = function () {
        $scope.processDataList = [];
        $scope.processSearchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.processPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.processUrl = 'Processes/Process/GetList?processId=[]';
        baseService.setCurrentPage('processDataList');
        $scope.getProcessDataList = function (pageno) {
            baseService.paginationBase($scope.processUrl, pageno, $scope.processPopUpParameters)
                .then(function (result) {
                    $scope.processDataList = result.Rows;
                    $scope.processPopUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.userProcessList) > 0) {
                        for (var i = 0; i < $scope.userProcessList.length; i++) {
                            for (var j = 0; j < $scope.processDataList.length; j++) {
                                if ($scope.userProcessList[i].ProcessId === $scope.processDataList[j].Id) {
                                    $scope.processDataList[j].Flag = true;
                                }
                            }
                        }
                    }
                    if (baseService.arrayLength($scope.processSearchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.processSearchList);
                    angular.element(document.querySelector('#processPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processPopUp');
                }).finally(function () {
                });
        };
        $scope.getProcessDataList();
    };
    $scope.userProcessList = [];

    $scope.DepartmentPopUpList = function () {
        $http({
            method: 'GET',
            url: 'Materials/DetentionMaster/LoadDepartmentList'
        }).then(function successCallback(response) {
                $scope.DepartmentDataList = response.data;
            angular.element(document.querySelector('#departmentPopUp')).modal('show');
        });
    };

    $scope.userDepartMentList = [];

    $scope.closeProcessPopUp = function () {
        $scope.processUpUrl = null;
        $scope.processDataList = [];
        $scope.processSearchList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };
    $scope.processDataList = [];
    $scope.SaveProcess = function () {

        try {

            if (baseService.arrayLength($scope.processDataList) > 0) {
                angular.forEach($scope.processDataList, function (a) {
                    if (checkProcessExist($scope.userProcessList, a.Id) === false) {
                        if (a.Flag) {
                            var ob = {};
                            ob.Id = null;
                            ob.ProcessId = a.Id;
                            ob.Code = a.Code;
                            ob.Sequence = a.Sequence;
                            ob.ShortName = a.ShortName;
                            ob.StandardName = a.StandardName;
                            ob.ProcessName = a.UserName;
                            $scope.userProcessList.push(ob);
                            ob = {};
                        }
                    }

                });
            }

            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.ProcesssaveUrl,
                data: { 'data': $scope.userProcessList, 'DetentionMasterId': $scope.detentionNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.processDataList();
                    $scope.getDetentionMasterProcess();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        $scope.closeProcessPopUp();
    };
    $scope.DepartmentDataList = [];
    $scope.SaveDepartment = function () {

        try {

            if (baseService.arrayLength($scope.DepartmentDataList) > 0) {
                angular.forEach($scope.DepartmentDataList, function (a) {
                    if (checkProcessExist($scope.userDepartMentList, a.Id) === false) {
                        if (a.Flag) {
                            var ob = {};
                            ob.Id = null;
                            ob.DepartmentId = a.Id;
                            ob.Code = a.Code;
                            ob.Sequence = a.Sequence;
                            ob.ShortName = a.ShortName;
                            ob.StandardName = a.StandardName;
                            ob.ProcessName = a.UserName;
                            $scope.userDepartMentList.push(ob);
                            ob = {};
                        }
                    }

                });
            }

            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.DepartmentSaveUrl,
                data: { 'data': $scope.userDepartMentList, 'DetentionMasterId': $scope.detentionNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.processDataList();
                    $scope.getDetentionMasterDepartment();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        $scope.closeDeptPopUp();
    };

    function checkProcessExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.closeDeptPopUp = function () {
        angular.element(document.querySelector('#departmentPopUp')).modal('hide');
    };

    $scope.closeProcessPopUp = function () {
        $scope.processUpUrl = null;
        $scope.processDataList = [];
        $scope.processSearchList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure you want to delete [" + name + "] permanently ?";
            angular.element(document.querySelector('#confirmRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeDeptRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempDeptId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure you want to delete [" + name + "] permanently ?";
            angular.element(document.querySelector('#confirmRemoveDeptPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionMaster/ProcessDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetentionMasterProcess();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.removeDeptRow = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionMaster/DepartmentDelete?id=' + $scope.tempDeptId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetentionMasterDepartment();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };


}