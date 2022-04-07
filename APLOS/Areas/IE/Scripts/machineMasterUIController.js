'use strict';
machineMasterUIController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function machineMasterUIController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Machine Master';
    $scope.Action = 'Save';
    $scope.Action1 = 'Save';
    //$scope.OperationActivityList = [];
    $scope.MachineCategoryList = [];
    $scope.MachineSuvCategoryList = [];
    $scope.OperationTypeList = [];
    $scope.OperationCategoryList = [];
    $scope.SkillList = [];
    $scope.MachineMasterList = [];
    $scope.ProcessList = [];
    $scope.legalDesignationList = [];
    $scope.SkillGroupingList = [];
    $scope.GetDataByMasterOrderIdList = [];
    $scope.EntityList = [];
    $scope.PositionList = [];


    $scope.path = 'IE/MachineMasterUI/';//ControlerName
    $scope.ProcesssaveUrl = $scope.path + 'CreateProcess';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'Edit'; 
    $scope.deleteUrl = $scope.path + 'Delete/';
    $scope.ProcessdeleteUrl = $scope.path + 'ProcessDelete/';
    $scope.saveUrl1 = $scope.path + 'CreateManpower';
    $scope.updateUrl1 = $scope.path + 'EditManpower';
    $scope.deleteUrl1 = $scope.path + 'DeleteManpower/';
    $scope.model = {
        Id: null,
        CompanyGroupId: null,
        MachineCategoryId: null,
        MachineSubCategoryId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        SkillId: null,
        ProductionMachineQty: null,
        SampleMachineQty: null,
        TrainingMachineQty: null,
        RentMachineQty: null,
        OtherMachineQty: null,
        ConnectedPower: null,
        RunningLoad: null,
        ConnectedSteam: null,
        RunningSteam: null,
        ConnectedAir: null,
        RunningAir: null,
        MaintanenceScheduleApplicable: false,
        Active: true
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.modelM = {
        Id: null,
        CompanyGroupId: null,
        Sequence: null,
        OperationMasterId: null,
        EntityId: null,
        PositionId: null,
        Caption: null,
        ManpowerBudget: null,
        Active: null
    };
    $scope.modelNewM = Object.assign({}, $scope.modelM);


    // #region GET Display DTA ON GRID
    $scope.GriddataOperationMaster = [];
    $scope.getaldataOperationMaster = function () {
        debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'IE/MachineMasterUI/GetMachineMaster',
        }).then(function successCallback(response) {
            $scope.GriddataOperationMaster = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.getaldataOperationMaster();

    $scope.GetOperationPositionMp = [];
    $scope.GetOperationPositionMPBudget = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'IE/OperationMaster/GetOperationPositionMPBudget',
        }).then(function successCallback(response) {
            $scope.GetOperationPositionMp = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.GetOperationPositionMPBudget();


    //#endregion


    $scope.MachineCategoryCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetCboMachineCategory'
        }).then(function successCallback(response) {
            $scope.MachineCategoryList = response.data;
        });
    }
    $scope.MachineCategoryCbo();



    $scope.MachineSubCategoryCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetCboMachineSubCategory'
        }).then(function successCallback(response) {
            $scope.MachineSuvCategoryList = response.data;
        });
    }
    $scope.MachineSubCategoryCbo();

    $scope.GetCboOperationTypeCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetCboOperationType'
        }).then(function successCallback(response) {
            $scope.OperationTypeList = response.data;
        });
    }
    $scope.GetCboOperationTypeCbo();



    //$scope.GetCboOperationCategoryCbo = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'IE/MachineMasterUI/GetCboOperationCategory'
    //    }).then(function successCallback(response) {
    //        $scope.OperationCategoryList = response.data;
    //    });
    //}
    //$scope.GetCboOperationCategoryCbo();


    $scope.GetCboSkillCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetCboSkill'
        }).then(function successCallback(response) {
            $scope.SkillList = response.data;
        });
    }
    $scope.GetCboSkillCbo();



    $scope.GetCboMachineMasterCbo = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboMachineMaster'
        }).then(function successCallback(response) {
            $scope.MachineMasterList = response.data;
        });
    }
    $scope.GetCboMachineMasterCbo();


    $scope.GetCboSkillGroupingCbo = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboSkillGrouping'
        }).then(function successCallback(response) {
            $scope.SkillGroupingList = response.data;
        });
    }
    $scope.GetCboSkillGroupingCbo();

    $scope.GetCbolegalDesignation = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCbolegalDesignation'
        }).then(function successCallback(response) {
            $scope.legalDesignationList = response.data;
        });
    }
    $scope.GetCbolegalDesignation();


    $scope.GetCboProcess = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboProcess'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    $scope.GetCboProcess();



    //#endregion


    // #region For AutoSequenceNo
    $scope.GeneratSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetAutoSequence'
        }).then(function successCallback(response) {
            $scope.modelNew.Sequence = response.data;
        });
    }
    $scope.GeneratSequenceNo();


    //#endregion AutoSequenceNo

    // #region For AutoSequenceNo For ManPower
    $scope.GetAutoSequenceForManPower = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetAutoSequenceForManPower'
        }).then(function successCallback(response) {
            $scope.modelNewM.Sequence = response.data;
        });
    }
    $scope.GetAutoSequenceForManPower();


    //#endregion AutoSequenceNo



    // #region Data Save Update and Delete


    $scope.Save = function () {
        debugger;
        angular.copy($scope.modelNew, $scope.model);
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.modelNewForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.model,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            //$scope.getData();
                            ShowResult(response.data.Message, 'failure');
                            throw response.data.Message;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Update';

                            $scope.getaldataOperationMaster();
                            $scope.Clear();
                            $scope.modelNew.OperationMasterIdID = response.data.Id;

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');

                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.model,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            //$scope.getData();
                            throw response.data.Message;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getaldataOperationMaster();
                        }
                    }, function errorCallBack(response) {
                        //$scope.getData();
                        //ShowResult(response.data.Message, 'failure');
                        throw response.data.Message;
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
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
                data: { 'data': $scope.userProcessList, 'machineMasterId': $scope.modelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.processDataList();
                    $scope.getMachineMasterProcess();
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

    $scope.ProcessDelete = function () {
        
            $http({
                method: 'POST',
                url: $scope.ProcessdeleteUrl + $scope.OMId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.getData();
                    $scope.userProcessList = resp.data;
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }    };


    $scope.userProcessList = [];
    $scope.getMachineMasterProcess = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getProcess',
            data: { 'machineMasterId': $scope.OMId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.userProcessList = [];
            $scope.userProcessList = resp.data;
        });
    }

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + name + "] ";
            angular.element(document.querySelector('#confirmRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmRemovePopUp')).modal('hide');
    };

    $scope.Delete = function () {

        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getaldataOperationMaster();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }

        else
            ShowResult('First delete all line item.', 'failure');
    };
    $scope.DeleteManpower = function () {

        if (!baseService.isUndefinedOrNull($scope.modelNewM.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl1 + $scope.modelNewM.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getaldataOperationMaster();
                    ClearFieldss();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }

        else
            ShowResult('First delete all line item.', 'failure');
    };
    $scope.Clear = function () {
        ClearFields($scope.GeneratSequenceNo());
        return true;
    };
    $scope.Clear1 = function () {
        ClearFieldss($scope.GetAutoSequenceForManPower());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.OperationMaster = {};
        $scope.modelNew = { Active: true };
        $scope.modelNew.Active = true;
        $scope.modelNew.Sequence = seq;
    }
    function ClearFieldss(seq) {
        $scope.Action1 = 'Save';
        $scope.OperationMaster = {};
        $scope.modelNewM = { Active: true };
        //$scope.modelNew.Active = true;
        $scope.modelNewM.Sequence = seq;
    }

    //#endregion 


    $scope.recorddoubleclick = function ($event) {
        debugger;
        var x = $event;
        $scope.OMId = x.data.Id;

        // $scope.modelNew.OperationMasterIdID = response.data.Id;  
        $scope.GetDataByMasterOrderIdfn($scope.OMId);
        // $scope.GetDataByMasterOrderIdfnMP($scope.OMId);
        $scope.Action = 'Update';
        $scope.getMachineMasterProcess();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.recorddoubleclickMP = function ($event) {
        debugger;
        var x = $event;
        $scope.OMId = x.data.Id;
        $scope.OperationMasterId = x.data.OperationMasterId;
        $scope.GetDataByMasterOrderIdfnMP($scope.OMId);
        //$scope.getMachineMasterProcess();
        $scope.Action1 = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.GetDataByMasterOrderIdfn = function (OMId) {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetDataByMasterOrderId?id=' + OMId
        }).then(function successCallback(response) {

            $scope.modelNew = response.data[0];
            $scope.modelNew.OperationMasterIdID = response.data[0].Id;

        });
    }

    $scope.GetDataByMasterOrderIdfnMP = function (OMId) {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetDataByMasterOrderIdMP?id=' + OMId
        }).then(function successCallback(response) {
            $scope.modelNewM = response.data[0];


        });
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
        $scope.getalldata1();

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.SaveManpower = function () {
        debugger;
        angular.copy($scope.modelNewM, $scope.modelM);
        $scope.modelM.OperationMasterId = $scope.modelNew.OperationMasterIdID;

        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.modelNewForm1.$valid) {
                if ($scope.Action1 === 'Save') {
                    if ($scope.modelM.PositionId === null) {
                        ShowResult('Please select Position');
                    }
                    else if ($scope.modelM.Caption === null) {
                        ShowResult('Please input Caption');
                    }
                    else if ($scope.modelM.ManpowerBudget === null) {
                        ShowResult('Please input Manpower Budget');
                    }
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl1,
                        data: $scope.modelM,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            //$scope.getData();
                            ShowResult(response.data.Message, 'failure');
                            throw response.data.Message;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action1 = 'Update';
                            $scope.GetOperationPositionMPBudget();
                            $scope.Clear();

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');

                    };
                }
                else if ($scope.Action1 === 'Update') {
                    $scope.modelM.OperationMasterId = $scope.OperationMasterId;
                    if ($scope.modelM.PositionId === null) {
                        ShowResult('Please select Position');
                    }
                    else if ($scope.modelM.Caption === null) {
                        ShowResult('Please input Caption');
                    }
                    else if ($scope.modelM.ManpowerBudget === null) {
                        ShowResult('Please input Manpower Budget');
                    }
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl1,
                        data: $scope.modelM,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            //$scope.getData();
                            throw response.data.Message;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.GetOperationPositionMPBudget();
                        }
                    }, function errorCallBack(response) {
                        //$scope.getData();
                        //ShowResult(response.data.Message, 'failure');
                        throw response.data.Message;
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    // #region Process

    $scope.userProcessList = [];

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

    $scope.addProcess = function () {
        if (baseService.arrayLength($scope.processDataList) > 0) {
            angular.forEach($scope.processDataList, function (a) {
                if (checkProcessExist($scope.userProcessList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.userProcessList.push({
                            Id: null
                            , ProcessId: a.Id
                            //, UserId: $scope.userNew.Id
                            , Code: a.Code
                            , Sequence: a.Sequence
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , ProcessName: a.UserName
                        });
                    }
                }

            });
        }
        //else
        //    $scope.userProcessList = [];
        //angular.forEach($scope.userProcessList, function (a) {
        //    if (!baseService.valueCheckInList($scope.processDataList, 'Id', a.ProcessId))
        //        $scope.userProcessList.splice(a, 1);
        //});
        $scope.closeProcessPopUp();
    };

    function checkProcessExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.closeProcessPopUp = function () {
        $scope.processUpUrl = null;
        $scope.processDataList = [];
        $scope.processSearchList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    //function getUserProcessList() {
    //    $http({
    //        method: 'GET',
    //        url: 'IE/MachineMasterUI/getUserProcessList'
    //    }).then(function successCallback(response) {
    //        $scope.userProcessList = response.data;
    //    });
    //}
   
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    // #endregion Process
}