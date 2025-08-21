'use strict';
OperationMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function OperationMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Skill Master';
    $scope.Action = 'Save';
    $scope.Action1 = 'Save';
    $scope.Action2 = 'Delete';
    $scope.OperationActivityList = [];
    $scope.OperationTypeList = [];
    $scope.OperationCategoryList = [];
    $scope.SkillList = [];
    $scope.MachineMasterList = [];
    $scope.ProcessList = [];
    $scope.legalDesignationList = [];
    $scope.SkillGroupingList = [];
    $scope.GetDataByMasterOrderIdList = [];
    $scope.EntityList = [];
    //$scope.LineList = [];
    $scope.PositionList = [];

    $scope.path = 'IE/OperationMaster/';//ControlerName
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'Edit';
    $scope.deleteUrl = $scope.path + 'Delete/';
    $scope.saveUrl1 = $scope.path + 'CreateManpower';
    $scope.updateUrl1 = $scope.path + 'EditManpower';
    $scope.deleteUrl1 = $scope.path + 'DeleteManpower/';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });



    $scope.model = {
        Id: null,
        CompanyGroupId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        OperationActivityId: null,
        OperationTypeId: null,
        OperationCategoryId: null,
        SkillId: null,
        Type: null,
        MachineMasterId: null,
        SkillGroupId: null,
        LegalDesignationId: null,
        ProcessId: null,
        ProposedSalary: null,
        Remarks: null,
        Active: null,
        DesignationGroupId: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.modelM = {
        Id: null,
        CompanyGroupId: null,
        Sequence: null,
        OperationMasterId: null,
        EntityId: null,
        ShiftId: null,
        LineId: null,
        PositionId: null,
        UserName: null,
        Caption: null,
        ManpowerBudget: null,
        Active: null
    };
    $scope.modelNewM = Object.assign({}, $scope.modelM);
    $(".searchableDDL").select2();
    $scope.modelNewM.Active = true;
    $scope.modelNew.Active = true;

    $scope.designationGroupList = [];
    $scope.GetDesignationGroupCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetDesignationGroupCbo'
        }).then(function successCallback(response) {
            $scope.designationGroupList = response.data;
        });
    };
    $scope.GetDesignationGroupCbo();


    // #region GET Display DTA ON GRID
    $scope.GriddataOperationMaster = [];
    $scope.getaldataOperationMaster = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'IE/OperationMaster/GetOperationMaster',
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
            url: 'IE/OperationMaster/GetOperationPositionMPBudget?id=' + $scope.modelNew.OperationMasterId,
        }).then(function successCallback(response) {
            $scope.GetOperationPositionMp = response.data;
            //entrydata = copy(searchdata);
        });
    };
    //$scope.GetOperationPositionMPBudget();


    //#endregion

    // #region Bind Data on DropdownList 

    $scope.EntityCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboEntity'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }
    $scope.EntityCbo();

    $scope.ShiftList = [];
    $scope.ShiftCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboShift'
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.ShiftCbo();


    //$scope.LineCbo = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'IE/OperationMaster/GetCboLine'
    //    }).then(function successCallback(response) {
    //        $scope.LineList = response.data;
    //    });
    //}
    //$scope.LineCbo();
    $scope.PositionCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboPosition'
        }).then(function successCallback(response) {
            $scope.PositionList = response.data;
            $scope.modelNewM.PositionId = $('#Position option:selected').val();

        });
    }
    $scope.PositionCbo();

    $scope.OperationActivityCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboOperationActivity'
        }).then(function successCallback(response) {
            $scope.OperationActivityList = response.data;
        });
    }
    $scope.OperationActivityCbo();

    $scope.GetCboOperationTypeCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboOperationType'
        }).then(function successCallback(response) {
            $scope.OperationTypeList = response.data;
        });
    }
    $scope.GetCboOperationTypeCbo();



    $scope.GetCboOperationCategoryCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboOperationCategory'
        }).then(function successCallback(response) {
            $scope.OperationCategoryList = response.data;
        });
    }
    $scope.GetCboOperationCategoryCbo();


    $scope.GetCboSkillCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboSkill'
        }).then(function successCallback(response) {
            $scope.SkillList = response.data;
        });
    }
    $scope.GetCboSkillCbo();

    $scope.GetCboMachineMasterCbo = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboMachineMaster'
        }).then(function successCallback(response) {
            $scope.MachineMasterList = response.data;
        });
    }
    $scope.GetCboMachineMasterCbo();

    $scope.GetCboSkillCboByMachine = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboSkillCboByMachine?Id=' + $scope.modelNew.MachineMasterId
        }).then(function successCallback(response) {
            //$scope.SkillList = response.data;
            //if (!baseService.isUndefinedOrNull(response.data))
            $scope.modelNew.SkillId = response.data[0].SkillId;
            //else
            //$scope.modelNew.SkillId = '';
            //$scope.SkillList = [];
        });
    }

    $scope.GetCboSkillGroupingCbo = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboSkillGrouping'
        }).then(function successCallback(response) {
            $scope.SkillGroupingList = response.data;
        });
    }
    $scope.GetCboSkillGroupingCbo();

    $scope.GetSkillGroupingCbo = function () {
        for (var i = 0; i < $scope.SkillList.length; i++) {
            if ($scope.modelNew.SkillId == $scope.SkillList[i].Value) {
                var sgId = $scope.SkillList[i].SkillGroupId;
                break;
            }
        }

        for (var i = 0; i < $scope.SkillGroupingList.length; i++) {
            if ($scope.SkillGroupingList[i].Value == sgId) {
                $scope.modelNew.SkillGroupId = $scope.SkillGroupingList[i].Value;
                break;
            }
        }
    }

    $scope.GetCbolegalDesignation = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCbolegalDesignation?designationGroupId=' + $scope.modelNew.DesignationGroupId
        }).then(function successCallback(response) {
            $scope.legalDesignationList = response.data;

            $scope.GetSkillMasterMachineData();
        });
    }

    $scope.GetCboProcess = function () {
        //debugger;
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
            url: 'IE/OperationMaster/GetAutoSequence'
        }).then(function successCallback(response) {
            $scope.modelNew.Sequence = response.data;
        });
    }
    $scope.GeneratSequenceNo();


    //#endregion AutoSequenceNo

    // #region For AutoSequenceNo For ManPower
    $scope.GetAutoSequenceForManPower = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetAutoSequenceForManPower?OMId=' + $scope.modelNew.OperationMasterId
        }).then(function successCallback(response) {
            $scope.modelNewM.Sequence = response.data;
        });
    }
    $scope.GetAutoSequenceForManPower();


    //#endregion AutoSequenceNo

    // #region Data Save Update and Delete


    $scope.Save = function () {
        //debugger;
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
                            //$scope.Clear();
                            $scope.modelNew.OperationMasterId = response.data.Id;

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
                    //ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }

        else
            ShowResult('First delete all line item.', 'failure');
    };
    $scope.valuePassInDelModal = function () {
        // $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };
    $scope.DeleteManpower = function () {

        if (!baseService.isUndefinedOrNull($scope.modelNewM.Id)) {

            // if ($scope.Action2 === 'Delete') {
            $http({
                method: 'POST',
                url: $scope.deleteUrl1 + $scope.modelNewM.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetOperationPositionMPBudget();
                    ClearFieldss();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
            //}
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
        // $scope.modelNew.Active = true;
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

        //debugger;       
        var x = $event;
        $scope.OMId = x.data.Id;
        $scope.modelNew = Object.assign({}, x.data);
        $scope.modelNew.OperationMasterId = x.data.Id;
        $scope.modelNew.SkillId = x.data.SkillId;
        $scope.GetDataByMasterOrderIdfn($scope.OMId);
        // $scope.GetDataByMasterOrderIdfnMP1($scope.OMId);
        $scope.GetOperationPositionMPBudget();
        $scope.GetAutoSequenceForManPower();
        $scope.GetCbolegalDesignation();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.recorddoubleclickMP = function ($event) {
        //debugger;
        var x = $event;
        $scope.OMId = x.data.Id;
        $scope.OperationMasterId = x.data.OperationMasterId;
        $scope.GetDataByMasterOrderIdfnMP($scope.OMId);
        $scope.modelNewM = x.data;
        $scope.Action1 = 'Update';

        $scope.modelNewM.PositionId = $('#Position option:selected').val();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.GetDataByMasterOrderIdfn = function (OMId) {
        //debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetDataByMasterOrderId?id=' + OMId
        }).then(function successCallback(response) {

            $scope.modelNew = response.data[0];
            $scope.modelNew.OperationMasterId = response.data[0].Id;

        });
    }

    $scope.GetDataByMasterOrderIdfnMP = function (OMId) {
        //debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetDataByMasterOrderIdMP?id=' + OMId
        }).then(function successCallback(response) {
            $scope.modelNewM = response.data[0];


        });
    }
    $scope.GetDataByMasterOrderIdfnMP1 = function (OMId) {
        //debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetDataByMasterOrderIdMP1?id=' + OMId
        }).then(function successCallback(response) {
            $scope.modelNewM = response.data[0];




        });
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.SaveManpower = function () {
        angular.copy($scope.modelNewM, $scope.modelM);
        $scope.modelM.OperationMasterId = $scope.modelNew.OperationMasterId;

        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.modelNewForm1.$valid) {
                if ($scope.Action1 === 'Save') {
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
                            //$scope.Action1 = 'Update';
                            $scope.GetOperationPositionMPBudget();
                            $scope.Clear1();

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');

                    };
                }
                else if ($scope.Action1 === 'Update') {
                    $scope.modelM.OperationMasterId = $scope.OperationMasterId;
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

    $scope.positionSearchList = [];
    $scope.positionDataList = [];
    $scope.positionSearch = [];
    $scope.positionUrl = "Organizations/Position/GetList";
    $scope.positionParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "UserName",
        searchBy: "Id",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionPopUp = function () {

        $scope.positionParameters.entityId = $scope.modelNewM.EntityId;
        $scope.getPositionData = function (pageno) {
            baseService.paginationBase($scope.positionUrl, pageno, $scope.positionParameters)
                .then(function (response) {
                    $scope.positionDataList = response.Rows;
                    $scope.positionParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.positionSearchList) === 0) {
                        $scope.positionSearchList.push(
                            {
                                "Text": "Id",
                                "Value": "Id"
                            });
                        baseService.getDDLSearchColumn($scope.positionDataList, $scope.positionSearchList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
            angular.element(document.querySelector("#positionPopUp")).modal("show");
        };
        $scope.getPositionData();
    };

    $scope.closePositionPopUp = function () {
        angular.element(document.querySelector("#positionPopUp")).modal("hide");
    };

    $scope.selectPositionPopUp = function (data) {
        $scope.selectedPositionId = data.Id;
        $scope.modelNewM.UserName = data.Id + ' - ' + data.UserName;
        $scope.modelNewM.PositionId = data.Id;
        $scope.modelNewM.PositionCode = $scope.selectedPositionId;
        $scope.closePositionPopUp();
    };

    $scope.Clear3 = function () {
        $scope.modelM.PositionCode = null;
    };
    //********************** Position PopUp End ************************************

    //$scope.Clear
    //********************** Opertation Master report  ************************************

    $scope.OpertationMasterReportPdf = function (id, reportFormat) {

        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        window.open('IE/OperationMaster/OperationMasterReports?reportFormat=' + reportFormat, '_blank');
    };
    $scope.OpertationMasterReportExcel = function (id, reportFormat) {

        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        window.open('IE/OperationMaster/OperationMasterReports?reportFormat=' + reportFormat, '_blank');
    };


    // #region SkillMasterMachine

    $scope.ActionSM = "Save";
    $scope.SkillMasterMachine = {
        Id: null,
        SkillMasterId: null,
        ArticleId: null
    }
    $scope.SkillMasterMachineNew = Object.assign({}, $scope.SkillMasterMachine);
    $scope.materialArticleList = [];

    $scope.materialType = 'MachineDefinition';
    $scope.getArticle = function () {
        $scope.getMaterialMasterWithCbxArticle();
    };

    // #region checkbox all

    $scope.refreshTemplatearticle = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllarticle });
    };

    function CheckBoxSelectAllarticle(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#MACGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.materialArticleList.length; i++) {
                $scope.materialArticleList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#MACGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.machineList = [];
    function MakeData() {

        for (var i = 0; i < $scope.materialArticleList.length; i++) {
            if ($scope.materialArticleList[i].Flag == true) {
                if (checkExists($scope.machineList, $scope.materialArticleList[i].Id) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.ArticleId = $scope.materialArticleList[i].Id;
                    ob.SkillMasterId = $scope.modelNew.OperationMasterId;

                    $scope.machineList.push(ob);
                }
                else {
                    throw "This Machine " + $scope.materialArticleList[i].StandardName + " is already taken.";
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ArticleId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseArticle = function () {
        try {
            MakeData();
            $scope.SaveSM();
            angular.element(document.querySelector('#materialarticleNewCbxPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    // #endregion Article

    $scope.SaveSM = function () {
        try {
            if (baseService.arrayLength($scope.machineList) < 0) {
                throw "Select Machine Master.";
            }

            $http({
                method: 'POST',
                url: 'IE/OperationMaster/SaveSkillMachine',
                data: { 'machineList': $scope.machineList, 'SkillMasterId': $scope.modelNew.OperationMasterId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSkillMasterMachineData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SkillMasterMachineList = [];
    $scope.GetSkillMasterMachineData = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetSkillMasterMachineData?OMId=' + $scope.modelNew.OperationMasterId
        }).then(function successCallback(response) {
            $scope.SkillMasterMachineList = response.data;
        });
    }

    $scope.ClearSM = function () {
        $scope.ActionSM = "Save";
        $scope.SkillMasterMachine = {
            Id: null,
            SkillMasterId: null,
            ArticleId: null,
            MaterialName: null
        }
        $scope.SkillMasterMachineNew = Object.assign({}, $scope.SkillMasterMachine);
    }
}