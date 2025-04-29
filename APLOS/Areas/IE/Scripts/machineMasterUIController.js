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
    $scope.AssetSaveUrl = $scope.path + 'CreateAsset';
    $scope.EntityCapacitySaveUrl = $scope.path + 'CreateEntityCapacity';
    $scope.updateUrl = $scope.path + 'Edit';
    //$scope.updateAssetUrl = $scope.path + 'CreateAsset';
    $scope.deleteUrl = $scope.path + 'Delete/';
    $scope.saveUrl1 = $scope.path + 'CreateManpower';
    $scope.updateUrl1 = $scope.path + 'EditManpower';
    $scope.deleteUrl1 = $scope.path + 'DeleteManpower/';

    $scope.getMGSeqUrl = $scope.path + 'GetMGAutoSequence';
    $scope.saveMGUrl = $scope.path + 'CreateMG';
    $scope.deleteMGUrl = $scope.path + 'DeleteMG';
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
        MaintanenceScheduleApplicable: true,
        Active: true,
        MachineMake: null,
        MachineModel: null,
        MachinePerticulars:null
    };
    $scope.modelNew = Object.assign({}, $scope.model);
    $scope.modelA = {
        Id: null,
        AssetName: null,
        EntityId: null,
        Entity: null,
        AssetDetail: null,
        AssetCode: null,
        AssetReference: null,
        IsOldCode: false,
        OldCode: null,
        TargetUtilization: null,
        PlanUtilization: null,
        AssetCategory: null,
        RepairAndMaintanenceBudget: null,
        ConsumableBudget: null,
        Remark: null
    }
    $scope.modelNewA = Object.assign({}, $scope.modelA);

    $scope.modelEntity = {
        Id: null,
        EntityId: null,
        Entity: null,
        NoofMachine: null,
        DailyHr: null,
        WeelkyHr: null,
        MonthlyHr: null,
        TargetUtilization: null,
        PlanUtilization: null
    }
    $scope.modelEntityCapacity = Object.assign({}, $scope.modelEntity);

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
        
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboMachineMaster'
        }).then(function successCallback(response) {
            $scope.MachineMasterList = response.data;
        });
    }
    $scope.GetCboMachineMasterCbo();


    $scope.GetCboSkillGroupingCbo = function () {
        
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboSkillGrouping'
        }).then(function successCallback(response) {
            $scope.SkillGroupingList = response.data;
        });
    }
    $scope.GetCboSkillGroupingCbo();

    $scope.GetCbolegalDesignation = function () {
        
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCbolegalDesignation'
        }).then(function successCallback(response) {
            $scope.legalDesignationList = response.data;
        });
    }
    $scope.GetCbolegalDesignation();


    $scope.GetCboProcess = function () {
        
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
       /* */
        angular.copy($scope.modelNew, $scope.model);
        $scope.$broadcast('show-errors-check-validity');
        try {
           /* if ($scope.modelNewForm.$valid) {*/
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
          /*  }*/
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



    $scope.userProcessList = [];
    $scope.getMachineMasterProcess = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProcess',
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
            $scope.message_confirmation = "Are you sure want to permanent delete? ";
            angular.element(document.querySelector('#confirmRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeRow = function () {
        $http({
            method: 'POST',
            url: 'IE/MachineMasterUI/ProcessDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMachineMasterProcess();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
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
        $scope.userProcessList = [];
        $scope.assetList = [];
        $scope.entityCapacityList = [];
    }


    //#endregion 


    $scope.recorddoubleclick = function ($event) {
        
        var x = $event;
        $scope.OMId = x.data.Id;

        // $scope.modelNew.OperationMasterIdID = response.data.Id;  
        $scope.GetDataByMasterOrderIdfn($scope.OMId);
        // $scope.GetDataByMasterOrderIdfnMP($scope.OMId);
        $scope.Action = 'Update';
        $scope.getMachineMasterProcess();
        $scope.getAssetMaster();
        $scope.getEntityCapacityMaster();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.recorddoubleclickMP = function ($event) {
        
        var x = $event;
        $scope.OMId = x.data.Id;
        $scope.OperationMasterId = x.data.OperationMasterId;
        $scope.GetDataByMasterOrderIdfnMP($scope.OMId);
        //$scope.getMachineMasterProcess();
        $scope.Action1 = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.GetDataByMasterOrderIdfn = function (OMId) {
        
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetDataByMasterOrderId?id=' + OMId
        }).then(function successCallback(response) {

            $scope.modelNew = response.data[0];
            $scope.modelNew.OperationMasterIdID = response.data[0].Id;

        });
    }

    $scope.GetDataByMasterOrderIdfnMP = function (OMId) {
        
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


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion Process

    $scope.selectEntity = function () {
        $scope.getsE();
        angular.element(document.querySelector('#EntityPop')).modal('show');
    }

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

    $scope.doubleEntity = function (e) {

        if ($scope.tab == 3) {
            $scope.modelNewA.EntityId = e.data.EntityId;
            $scope.modelNewA.Entity = e.data.EntityName;
            angular.element(document.querySelector('#EntityPop')).modal('hide');
        }
        if ($scope.tab == 4) {
            $scope.modelEntityCapacity.EntityId = e.data.EntityId;
            $scope.modelEntityCapacity.Entity = e.data.EntityName;
            angular.element(document.querySelector('#EntityPop')).modal('hide');
        }

    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#EntityPop')).modal('hide');
    }

    $scope.workCenterList = [];
    $scope.GetWorkCenterList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.modelNewA.EntityId)) {
                throw "Entity is required.";
            }
            $http({
                method: 'GET',
                url: 'IE/MachineMasterUI/GetWorkCenterList?entityId=' + $scope.modelNewA.EntityId
            }).then(function successCallback(res) {
                $scope.workCenterList = res.data;
            });

            var eDialog = $("#workCenterPopUp").data("ejDialog");
            eDialog.open();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SetworkCenter = function (data) {
        $scope.modelNewA.WorkCenterMaster = data.data.UserName;
        $scope.modelNewA.WorkCenterMasterId = data.data.WorkCenterMasterId;
        $scope.CloseWorkCenter();
    }

    $scope.CloseWorkCenter = function () {
        var eDialog = $("#workCenterPopUp").data("ejDialog");
        eDialog.close();
    }

    $scope.articleList = [];
    $scope.GetArticleList = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetArticleList'
        }).then(function successCallback(res) {
            $scope.articleList = res.data;
        });

        var eDialog = $("#ArticleListPopUp").data("ejDialog");
        eDialog.open();
    }

    $scope.SetArticle = function (data) {
        $scope.modelNewA.Article = data.data.StandardName;
        $scope.modelNewA.ArticleId = data.data.Id;
        $scope.closeArticle();
    }

    $scope.closeArticle = function () {
        var eDialog = $("#ArticleListPopUp").data("ejDialog");
        eDialog.close();
    }

    $scope.AssetSave = function () {
        
        angular.copy($scope.modelNewA, $scope.modelA);
        $scope.$broadcast('show-errors-check-validity');
        try {
            $http({
                method: 'POST',
                url: $scope.AssetSaveUrl,
                data: { 'data': $scope.modelNewA, 'machineMasterId': $scope.modelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    throw response.data.Message;
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                    $scope.getAssetMaster();
                    $scope.ClearAsset();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.assetList = [];
    $scope.getAssetMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetAsset',
            data: { 'machineMasterId': $scope.OMId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.assetList = [];
            $scope.assetList = resp.data;
        });
    }

    $scope.removeAssetRowModal = function (name, tempId) {
        try {
            //$scope.popUpIndex = index;
            //$scope.listName = listName;
            $scope.tempId = tempId;
            //$scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmAssetRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeAssetRow = function () {
        $http({
            method: 'POST',
            url: 'IE/MachineMasterUI/AssetDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getAssetMaster();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.ClearAsset = function () {
        $scope.modelNewA = {
            Id: null,
            AssetName: null,
            EntityId: null,
            Entity: null,
            AssetDetail: null,
            AssetCode: null,
            AssetReference: null,
            IsOldCode: false,
            OldCode: null,
            TargetUtilization: null,
            PlanUtilization: null,
            AssetCategory: null,
            RepairAndMaintanenceBudget: null,
            ConsumableBudget: null,
            Remark: null
        };
    };


    $scope.Assetdoubleclick = function (args) {
        $scope.modelNewA = Object.assign({}, args);
    };

    $scope.EntityCapacitySave = function () {
        
        angular.copy($scope.modelEntityCapacity, $scope.modelEntity);
        $scope.$broadcast('show-errors-check-validity');
        try {
            $http({
                method: 'POST',
                url: $scope.EntityCapacitySaveUrl,
                data: { 'data': $scope.modelEntityCapacity, 'machineMasterId': $scope.modelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    throw response.data.Message;
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                    $scope.getEntityCapacityMaster();
                    $scope.ClearEntityCapacity();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.entityCapacityList = [];
    $scope.getEntityCapacityMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEntityCapacity',
            data: { 'machineMasterId': $scope.OMId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.entityCapacityList = [];
            $scope.entityCapacityList = resp.data;
        });
    }

    $scope.removeEntityCapacityRowModal = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmEntityCapacityRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeEntityCapacityRow = function () {
        $http({
            method: 'POST',
            url: 'IE/MachineMasterUI/EntityCapacityDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getEntityCapacityMaster();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.ClearEntityCapacity = function () {
        $scope.modelEntityCapacity = {
            Id: 0,
            EntityId: null,
            Entity: null,
            NoofMachine: null,
            DailyHr: null,
            WeelkyHr: null,
            MonthlyHr: null,
            TargetUtilization: null,
            PlanUtilization: null
        };
    };

    $scope.EntityCapacitydoubleclick = function (args) {
        $scope.modelEntityCapacity = Object.assign({}, args);
    };

    $scope.changeAsset = function () {
        if ($scope.modelNewA.IsOldCode == false) {
            $scope.modelNewA.OldCode = null;
        }

    }

    //#region MG
    $scope.searchMGBy = "UserName"; $scope.searchMG = "";
    $scope.searchMGByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.ModelMGList = [];
    $scope.MachineGroupList = [];
    $scope.getMGData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetMGList",
            data: { column: $scope.searchMGBy, value: $scope.searchMG },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelMGList = response.data;
            $scope.MachineGroupList = response.data;
            ClearMGFields(response.data.Sequence);
            $scope.GetMGSequence();
        });
    }
    $scope.getMGData();

    $scope.ModelMGTemp = {
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
    $scope.MachineGroupNew = Object.assign({}, $scope.ModelMGTemp);

    $scope.GetMGSequence = function () {
        cboService.getSequence($scope.getMGSeqUrl, function (data) {
            $scope.ModelMGTemp.Sequence = data;
            $scope.MachineGroupNew.Sequence = data;
        });
    };
    $scope.GetMGSequence();

    $scope.GetMG = function (args) {

        $scope.MachineGroupNew = Object.assign({}, args.data);
        $scope.ActionMG = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveMachineGroup = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MachineGroupNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveMGUrl,
                data: { 'data': $scope.MachineGroupNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearMGFields(response.data.Sequence);
                    $scope.getMGData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteMG = function () {
        if (!baseService.isUndefinedOrNull($scope.MachineGroupNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteMGUrl + $scope.MachineGroupNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearMGFields(response.data.Sequence);
                    $scope.getMGData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearMG = function () {
        ClearMGFields($scope.GetMGSequence());
        return true;
    };

    function ClearMGFields(seq) {
        $scope.ActionMG = 'Save';
        $scope.MachineGroupNew = Object.assign({}, $scope.ModelMGTemp);
        $scope.MachineGroupNew.Sequence = seq;
    }
    //#endregion

}