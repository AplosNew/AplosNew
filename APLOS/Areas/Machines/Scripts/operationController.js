'use strict';
OperationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$window', '$filter'];
function OperationController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $window, $filter) {
    $rootScope.title = "Operation Master";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.popUpIndex = -1;
    $scope.operations = [];
    $scope.excluedList = [];
    $scope.path = 'Machines/operation/';
    $scope.getListUrl = 'Machines/operation/getlist?ids=null';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.operations = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchOerationByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Process',
            'value': 'Process'
        },
        {
            'name': 'Operation Type',
            'value': 'OperationTypeCode'
        },
        {
            'name': 'Operation Category',
            'value': 'OperationCategoryName'
        },
        {
            'name': 'Machine/Without Machine',
            'value': 'IsMachineRequired'
        }
    ];

    $scope.operation = {
        Id: null
        , OperationCategoryId: null
        , OperationCategoryName: null
        , OperationTypeId: null
        , OperationTypeCode: null
        , OperationActivityId: null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , IsMachineRequired: 'H'
        , BasicProcessTime: 0
        , AssociateProcessTime: 0
        , PersonalAllowance: 0
        , MachineAllowance: 0
        , AdditionalAllowance:0
        , OperationLength: 0
        , Frequency: 0
        , SPI: 0
        , ProductionSystemId: null
        , Remarks: null
        , Active: true

        , ArticleId: null
        , ArticleName: null
        , SkillId: null
        , SkillName: null
    };
    $scope.operationNew = Object.assign({}, $scope.operation);

    // #region DDL
    $scope.OperationVariationList = [];
    $scope.GetOperationVariationData = function (operationId) {
    $http({
            method: 'GET',
        url: 'Machines/operation/GetOperationVariationData?operationId=' + operationId
        }).then(function successCallback(response) {
            $scope.OperationVariationList = response.data;
            });
        angular.element(document.querySelector('#OperationVariationPopUp')).modal('show');
    }

    $scope.operationTypeList = [];
    $http({
        method: 'GET',
        url: 'Machines/operationtype/getcbo'
    }).then(function successCallback(response) {
        $scope.operationTypeList = response.data;
    });

    $scope.operationCategoryList = [];
    $http({
        method: 'GET',
        url: 'Machines/OperationCategory/GetCbo'
    }).then(function successCallback(response) {
        $scope.operationCategoryList = response.data;
    });

    $scope.operationActionList = [];
    $http({
        method: 'GET',
        url: 'Machines/OperationActivity/GetCbo'
    }).then(function successCallback(response) {
        $scope.operationActionList = response.data;
    });

    $scope.productionSystemList = [];
    $http({
        method: 'GET',
        url: 'Machines/productionSystem/GetCbo'
    }).then(function successCallback(response) {
        $scope.productionSystemList = response.data;
    });

    // #endregion

    $scope.GetSequence = function () {
        $http.get("Machines/operation/getautosequence")
            .then(function (response) {
                $scope.operationNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (index) {
        $scope.setTab(1);
        $scope.index = index;
        angular.copy($scope.operations[$scope.index], $scope.operation);
        angular.copy($scope.operation, $scope.operationNew);
        getOperationProcessList();
        $scope.getFgComponentData();
        getAttributeList();
        getValueList();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    function reDirectToRequiredTab() {
        if ($scope.AdvancedForm.$invalid) {
            $scope.setTab(2);
        }
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredTab();
        if ($scope.operationNewForm.$valid) {
            angular.copy($scope.operationNew, $scope.operation);
            $scope.operationTypeId = angular.element("#operationTypeId :selected").text();
            $scope.operationCategoryId = angular.element("#operationCategoryId :selected").text();

            for (var t = 0; t < baseService.arrayLength($scope.attributeList); t++) {
                if ($scope.attributeList[t].ValueAssignmentLevel === 'General') {
                    var filterList = $filter("filter")($scope.valueList, { OperationAttributeId: $scope.attributeList[t].Id });
                    if (baseService.arrayLength(filterList) === 0)
                        return ShowResult('Attribute [' + $scope.attributeList[t].UserName + '] value can\'t be null.');
                }
            }

            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "Machines/operation/create",
                    data: {
                        'operation': $scope.operation
                        , 'processList': $scope.sprocessList
                        , 'operationFgComponent': $scope.operationFgComponents
                        , 'attributeList': $scope.attributeList
                        , 'valueList': $scope.valueList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: "Machines/operation/edit",
                    data: {
                        'operation': $scope.operation
                        , 'processList': $scope.sprocessList
                        , 'operationFgComponent': $scope.operationFgComponents
                        , 'attributeList': $scope.attributeList
                        , 'valueList': $scope.valueList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.operationNew.Id)) {
            $http({
                method: 'POST',
                url: "Machines/operation/delete/" + $scope.operationNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.operations.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.operation = {};
        $scope.operationNew = {
            Sequence: seq
            , Active: true
            , IsMachineRequired: 'H'
            , BasicProcessTime: 0
            , AssociateProcessTime: 0
            , PersonalAllowance: 0
            , MachineAllowance: 0
            , OperationLength: 0
            , Frequency: 0
            , SPI: 0
            , AdditionalAllowance: 0
        };
        $scope.operationFgComponents = [];
        $scope.sprocessList = [];
        $scope.attributeList = [];
    }

    // #region FG Component
    $scope.operationFgComponents = [];
    $scope.fgParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.FgComponentPopUp = function () {
        angular.forEach($scope.operationFgComponents, function (a) {
            $rootScope.tempList.push({
                Id: a.FGComponentId,
                Sequence: a.Sequence,
                Code: a.Code,
                ShortName: a.ShortName,
                StandardName: a.StandardName,
                UserName: a.UserName,
                Active: a.Active,
                Class: 'new',
                Archive: false
            });
        });
        baseService.setCurrentPage('operationFgComponentList');
        $scope.getFgData = function (pageno) {
            $scope.getFgComponentUrl = 'Materials/fgcomponent/getfgcomponentlist?id=' + isProcessIdExistGrid($scope.operationFgComponents);
            baseService.paginationBase($scope.getFgComponentUrl, pageno, $scope.fgParameters)
                .then(function (result) {
                    $scope.operationFgComponentList = result.Rows;
                    $scope.fgParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.operationFgComponentList); t++) {
                        $scope.operationFgComponentList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.operationFgComponentList[t].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#fgComponentPopUp')).modal('show');
        $scope.getFgData();
    };
    $scope.CloseProcessPopUp = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#fgComponentPopUp')).modal('hide');
    };
    $rootScope.searchByFgList = [

        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    function isProcessIdExistGrid(list) {
        $scope.FGComponentIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] === false) {
                    $scope.FGComponentIds.push(list[i]['FGComponentId']);
                }
            }
        }
        return JSON.stringify($scope.FGComponentIds);
    }
    $scope.addFg = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.operationFgComponents, 'FGComponentId', a.Id)) {
                    $scope.operationFgComponents.push({
                        Id: null,
                        OperationId: $scope.operationNew.Id,
                        FGComponentId: a.Id,
                        Sequence: a.Sequence,
                        Code: a.Code,
                        ShortName: a.ShortName,
                        StandardName: a.StandardName,
                        UserName: a.UserName,
                        Active: a.Active,
                        Class: 'new',
                        Archive: false
                    });
                }
            });
        }
        else
            $scope.operationFgComponents = [];
        angular.forEach($scope.operationFgComponents, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.FGComponentId))
                $scope.operationFgComponents.splice(a, 1);
        });
        $scope.CloseProcessPopUp();
    };

    $scope.getFgComponentData = function () {
        $http({
            method: 'GET',
            url: 'Machines/operation/getoperationfgcomponentlist?operationId=' + $scope.operationNew.Id
        }).then(function successCallback(response) {
            $scope.operationFgComponents = response.data;
        });
    };
    // #endregion

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region Process

    $scope.sprocessList = [];
    $scope.processParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.searchProcessByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Define Name',
            'value': 'UserName'
        }
    ];
    $scope.processPopUp = function () {
        //angular.forEach($scope.sprocessList, function (a) {
        //    $rootScope.tempList.push({
        //        Id: a.ProcessId
        //        , Sequence: a.Sequence
        //        , Code: a.Code
        //        , ShortName: a.ShortName
        //        , StandardName: a.StandardName
        //        , UserName: a.UserName
        //        , MaterialType: a.MaterialType
        //        , Active: a.Active
        //        , SubProcesses: []
        //    });
        //});
        baseService.setCurrentPage('processList');
        $scope.getProcessData = function (pageno) {
            $scope.getProcessUrl = 'Processes/process/GetList?processid=' + baseService.getColumnValueList($scope.sprocessList, 'ProcessId');
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.processParameters)
                .then(function (result) {
                    $scope.processList = result.Rows;
                    $scope.processParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.processList); t++) {
                        $scope.processList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.processList[t].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };

    $scope.addProcess = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            var flag = false;
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.sprocessList, 'ProcessId', a.Id)) {
                    $scope.sprocessList.push({
                        Id: null
                        , ProcessId: a.Id
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , ShortName: a.ShortName
                        , StandardName: a.StandardName
                        , UserName: a.UserName
                        , MaterialType: a.MaterialType
                        , Active: a.Active
                        , SubProcesses: []
                    });
                    flag = true;
                }
                //if (flag) {
                //    $scope.operationNew.ArticleId = null;
                //    $scope.operationNew.ArticleName = null;
                //    $scope.operationNew.SkillId = null;
                //    $scope.operationNew.SkillName = null;
                //    $scope.operationNew.MachineAllowance = 0;
                //}
            });
        }
        $scope.closeProcess();
    };

    $scope.removeProcessRowModal = function (ob, index) {
        try {
            $scope.message_confirmation = "Are you sure want to permanent delete [" + ob.UserName + "].This will delete machine along with skill.";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
            $scope.popUpIndex = index;
            $scope.processId = ob.ProcessId;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeProcessRow = function () {
        $scope.sprocessList.splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };
    $scope.closeProcess = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    $scope.MachineRequired = true;
    function getOperationProcessList() {
        $http({
            method: 'GET',
            url: $scope.path + 'getoperationprocesslist?operationId=' + $scope.operationNew.Id
        }).then(function successCallback(response) {
            $scope.sprocessList = response.data;
            });
        if ($scope.operationNew.IsMachineRequired === true)
        {
            $scope.MachineRequired = true;
            $scope.operationNew.IsMachineRequired= "M";
        } else
        {
            $scope.MachineRequired = false;

            $scope.operationNew.IsMachineRequired = "H";
        }
    }

    // #endregion

    // #region Sub process
    $scope.searchSubProcessByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.subProcessList = [];
    $scope.valueData = '';
    $scope.subProcessPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.subProcessPoUp = function (processId, index) {
        $scope.popUpIndex = index;
        $scope.ProcessId = processId;
        $scope.subProcessList = [];
        $scope.subProcessList = $scope.sprocessList[$scope.popUpIndex].SubProcesses;
        angular.element(document.querySelector('#subProcessPop')).modal('show');
    };
    $scope.SubProcessListPopUp = function () {
        angular.forEach($scope.subProcessList, function (a) {
            $rootScope.tempList.push({
                Id: a.SubProcessId,
                Code: a.Code,
                UserName: a.SubProcessName,
                SubProcessCategoryName: a.SubProcessCategoryName,
                Class: 'new'
            });
        });
        $scope.popUpUrl = 'Processes/subprocess/getlistsubprocess/?companyId=' + $window.CompanyId + '&processId=' + $scope.ProcessId
            + '&subProcessIds=' + baseService.getColumnValueList($scope.subProcessList, 'SubProcessId');
        baseService.setCurrentPage('subProcesses');
        $scope.getSubProcessData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.subProcessPopUpParameters)
                .then(function (result) {
                    $scope.subProcesses = result.Rows;
                    $scope.subProcessPopUpParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.subProcesses); t++) {
                        $scope.subProcesses[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.subProcesses[t].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });

            angular.element(document.querySelector('#subProcessPopUp')).modal('show');
        };
        $scope.getSubProcessData();
    };
    $scope.AddSubProcess = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.subProcessList, 'SubProcessId', a.Id)) {
                    $scope.subProcessList.push({
                        Id: null
                        , OperationId: $scope.operationNew.Id
                        , OperationProcessId: null
                        , ProcessId: $scope.ProcessId
                        , SubProcessId: a.Id
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , SubProcessName: a.UserName
                        , SubProcessCategoryName: a.SubProcessCategoryName
                    });
                }
            });
        }
        else
            $scope.subProcessList = [];
        angular.forEach($scope.subProcessList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.SubProcessId))
                $scope.subProcessList.splice(a, 1);
        });
        $scope.closeSubProcessListPopUp();
    };
    $scope.closeSubProcessListPopUp = function () {
        $scope.ProcessId = null;
        $rootScope.tempList = [];
        angular.element(document.querySelector('#subProcessPopUp')).modal('hide');
    };
    $scope.closeSubProcessList = function () {
        $scope.sprocessList[$scope.popUpIndex].SubProcesses = [];
        $scope.sprocessList[$scope.popUpIndex].SubProcesses = $scope.subProcessList;
        $scope.popUpIndex = - 1;
        CloseModalShowResult();
        angular.element(document.querySelector('#subProcessPop')).modal('hide');
    };
    // #endregion

    // #region Material Master

    $scope.materialList = [];
    $scope.materialParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'MaterialMasterName'
        , searchBy: "MaterialMasterName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.materialPopUp = function (index) {
        $scope.popUpIndex = index;
        $scope.materialDataList = [];
        $scope.materialUrl = 'Materials/MaterialMaster/GetCommonMachineListByProcess?processIds=' + baseService.getColumnValueList($scope.sprocessList, 'ProcessId');
        baseService.setCurrentPage('materialDataList');
        $scope.getMaterialData = function (pageno) {
            baseService.paginationBase($scope.materialUrl, pageno, $scope.materialParameters)
                .then(function (result) {
                    $scope.materialDataList = result.Rows;
                    $scope.materialParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.materialList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.materialList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'materialId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#materialId')).modal('show');
        $scope.getMaterialData();
    };
    $scope.closeMaterial = function () {
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#materialId')).modal('hide');
    };

    // #endregion MM

    // #region Article

    $scope.articleList = [];
    $scope.articleParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'StandardName'
        , searchBy: "StandardName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.articlePopUp = function (materialMasterId, materialIndex) {
        try {
            var flag = false;
            var prosessIds = $scope.materialDataList[materialIndex].ProsessIds;
            if (!baseService.isUndefinedOrNull(prosessIds)) {
                var processAray = prosessIds.split(',');
                for (var i = 0; i < baseService.arrayLength(processAray); i++) {
                    if (baseService.valueCheckInList($scope.sprocessList, 'ProcessId', processAray[i])) {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag) throw 'operation process and machine process not match ';
            $scope.excluedList = ['SkillName', 'MachineAllowance'];
            $scope.articleDataList = [];
            $scope.articleUrl = $scope.path + 'GetArticleListByMaterialMaster?materialMasterId=' + materialMasterId;
            baseService.setCurrentPage('dataList');
            $scope.getarticleData = function (pageno) {
                baseService.paginationBase($scope.articleUrl, pageno, $scope.articleParameters)
                    .then(function (result) {
                        $scope.articleDataList = result.Rows;
                        $scope.articleParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.articleList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.articleList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', 'articleId');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#articleId')).modal('show');
            $scope.getarticleData();
        } catch (e) {
            ShowResult(e, '', 'materialId');
        }

    };
    $scope.selectArticle = function (data) {
        $scope.operationNew.ArticleId = data.Id;
        $scope.operationNew.ArticleName = data.StandardName;
        $scope.operationNew.SkillId = data.SkillId;
        $scope.operationNew.SkillName = data.SkillName;
        $scope.operationNew.MachineAllowance = data.MachineAllowance;
        $scope.closeArticle();
        $scope.closeMaterial();
    };
    $scope.closeArticle = function () {
        angular.element(document.querySelector('#articleId')).modal('hide');
    };

    // #endregion Article

    // #region Skill

    $scope.skillList = [];
    $scope.skillParameters = {
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
    $scope.skillPoUp = function () {
        $scope.excluedList = [];
        $scope.skillDataList = [];
        $scope.skillUrl = 'Skills/Skill/GetCommonSkillListByProcess?processIds=' + baseService.getColumnValueList($scope.sprocessList, 'ProcessId') +'&MachineRequired='+$scope.MachineRequired;
        baseService.setCurrentPage('dataList');
        $scope.getSkillData = function (pageno) {
            baseService.paginationBase($scope.skillUrl, pageno, $scope.skillParameters)
                .then(function (result) {
                    $scope.skillDataList = result.Rows;
                    $scope.skillParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.skillList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.skillList);
                    }
                    angular.element(document.querySelector('#skillId')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'skillId');
                }).finally(function () {
                });
        };
        $scope.getSkillData();
    };
    $scope.selectSkill = function (data) {
        //$scope.operationNew.ArticleId = null;
        //$scope.operationNew.ArticleName = null;
        $scope.operationNew.SkillId = data.SkillId;
        $scope.operationNew.SkillName = data.UserName;
        $scope.operationNew.MachineAllowance = 0;
        $scope.closeSkill();
    };
    $scope.closeSkill = function () {
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#skillId')).modal('hide');
    };

    // #endregion Skill

    // #region Attribute 

    $scope.attributeIndex = -1;
    $scope.attributeList = [];

    $scope.attributePropertyList = [];
    cboService.getEnumCbo("enum/GetAttributePropertiesCbo", function (result) {
        $scope.attributePropertyList = result;
    });
    //cboService.getEnumCbo("enum/GetValueAssignmentCbo", function (result) {
    //    $scope.valueAssignmentList = result;
    //});

    $scope.attributePopUp = function () {
        $scope.attributeAction = 'Add';
        $scope.attributeIndex = -1;
        $scope.oprationAttribute = {
            Id: null
            , OperationId: $scope.operationNew.Id
            , Sequence: 0
            , Code: null
            , ShortName: null
            , StandardName: null
            , UserName: null
            , ValueAssignmentLevel: 'General'
            , AttributeProperty: $scope.attributePropertyList[0].Value
            , NoOfCharacter: 0
            , IsFixedNoOfCharacter: false
            , IsFreeField: true
            , IsPreDefinedField: true
            , IsMandatory: true
        };
        $scope.oprationAttributeNew = Object.assign({}, $scope.oprationAttribute);
        $scope.GetAttributeSequence();
        angular.element(document.querySelector('#oprationAttributeEntryPopUp')).modal('show');
    };

    $scope.GetAttributeSequence = function () {
        if (baseService.arrayLength($scope.attributeList) === 0) {
            $http.get("Machines/operation/GetAttributeSequence?operationId=" + $scope.operationNew.Id)
                .then(function (response) {
                    $scope.oprationAttributeNew.Sequence = response.data;
                });
        }
        else
            $scope.oprationAttributeNew.Sequence = baseService.getMaxNumberFromList($scope.attributeList, 'Sequence');
    };

    $scope.addAttribute = function () {
        if ($scope.manualValidationAddRemove('div_Sequence', $scope.oprationAttributeNew, 'Sequence')) return;
        if ($scope.manualValidationAddRemove('div_Code', $scope.oprationAttributeNew, 'Code')) return;
        if ($scope.manualValidationAddRemove('div_ShortName', $scope.oprationAttributeNew, 'Short Name')) return;
        if ($scope.manualValidationAddRemove('div_StandardName', $scope.oprationAttributeNew, 'Standard Name')) return;
        if ($scope.manualValidationAddRemove('div_UserName', $scope.oprationAttributeNew, 'UserName', 'User Define Name')) return;

        for (var t = 0; t < baseService.arrayLength($scope.attributeList); t++) {
            if (baseService.isAvailableInList($scope.attributeList[t].Code, $scope.oprationAttributeNew.Code, t, $scope.attributeIndex))
                return manualValidation('div_Code', true, 'Code: ' + $scope.oprationAttributeNew.Code + ' already exist in this operation.');
            if (baseService.isAvailableInList($scope.attributeList[t].UserName, $scope.oprationAttributeNew.UserName, t, $scope.attributeIndex))
                return manualValidation('div_UserName', true, 'User Define Name: ' + $scope.oprationAttributeNew.UserName + ' already exist in this operation.');
        }
        if ($scope.attributeIndex === -1) {
            $scope.attributeList.push({
                Id: baseService.pk()
                , OperationId: $scope.oprationAttributeNew.OperationId
                , Sequence: $scope.oprationAttributeNew.Sequence
                , Code: $scope.oprationAttributeNew.Code
                , ShortName: $scope.oprationAttributeNew.ShortName
                , StandardName: $scope.oprationAttributeNew.StandardName
                , UserName: $scope.oprationAttributeNew.UserName
                , ValueAssignmentLevel: $scope.oprationAttributeNew.ValueAssignmentLevel
                , AttributeProperty: $scope.oprationAttributeNew.AttributeProperty
                , NoOfCharacter: $scope.oprationAttributeNew.NoOfCharacter
                , IsFixedNoOfCharacter: $scope.oprationAttributeNew.IsFixedNoOfCharacter
                , IsFreeField: $scope.oprationAttributeNew.IsFreeField
                , IsPreDefinedField: $scope.oprationAttributeNew.IsPreDefinedField
                , IsMandatory: $scope.oprationAttributeNew.IsMandatory
            });
        }
        else {
            $scope.attributeList[$scope.attributeIndex].Id = $scope.oprationAttributeNew.Id;
            $scope.attributeList[$scope.attributeIndex].OperationId = $scope.oprationAttributeNew.OperationId;
            $scope.attributeList[$scope.attributeIndex].Sequence = $scope.oprationAttributeNew.Sequence;
            $scope.attributeList[$scope.attributeIndex].Code = $scope.oprationAttributeNew.Code;
            $scope.attributeList[$scope.attributeIndex].ShortName = $scope.oprationAttributeNew.ShortName;
            $scope.attributeList[$scope.attributeIndex].StandardName = $scope.oprationAttributeNew.StandardName;
            $scope.attributeList[$scope.attributeIndex].UserName = $scope.oprationAttributeNew.UserName;
            $scope.attributeList[$scope.attributeIndex].ValueAssignmentLevel = $scope.oprationAttributeNew.ValueAssignmentLevel;
            $scope.attributeList[$scope.attributeIndex].AttributeProperty = $scope.oprationAttributeNew.AttributeProperty;
            $scope.attributeList[$scope.attributeIndex].NoOfCharacter = $scope.oprationAttributeNew.NoOfCharacter;
            $scope.attributeList[$scope.attributeIndex].IsFixedNoOfCharacter = $scope.oprationAttributeNew.IsFixedNoOfCharacter;
            $scope.attributeList[$scope.attributeIndex].IsFreeField = $scope.oprationAttributeNew.IsFreeField;
            $scope.attributeList[$scope.attributeIndex].IsPreDefinedField = $scope.oprationAttributeNew.IsPreDefinedField;
            $scope.attributeList[$scope.attributeIndex].IsMandatory = $scope.oprationAttributeNew.IsMandatory;
        }

        $scope.clearAttribute();
    };

    $scope.editAttribute = function (index) {
        $scope.attributeAction = 'Update';
        $scope.attributeIndex = index;
        angular.copy($scope.attributeList[index], $scope.oprationAttributeNew);
        angular.element(document.querySelector('#oprationAttributeEntryPopUp')).modal('show');
    };

    $scope.clearAttribute = function () {
        manualValidation('div_Sequence', false);
        manualValidation('div_Code', false);
        manualValidation('div_ShortName', false);
        manualValidation('div_StandardName', false);
        manualValidation('div_UserName', false);

        $scope.attributeAction = 'Add';
        $scope.attributeIndex = -1;
        $scope.oprationAttribute = {};
        $scope.oprationAttributeNew = {
            OperationId: $scope.operationNew.Id
            , ValueAssignmentLevel: 'General'
            , AttributeProperty: $scope.attributePropertyList[0].Value
            , NoOfCharacter: 0
            , IsFixedNoOfCharacter: false
            , IsFreeField: true
            , IsPreDefinedField: true
            , IsMandatory: true
        };
        $scope.GetAttributeSequence();
    };

    $scope.closeAttribute = function () {
        manualValidation('div_Sequence', false);
        manualValidation('div_Code', false);
        manualValidation('div_ShortName', false);
        manualValidation('div_StandardName', false);
        manualValidation('div_UserName', false);
        $scope.oprationAttribute = {};
        $scope.oprationAttributeNew = {};
        angular.element(document.querySelector('#oprationAttributeEntryPopUp')).modal('hide');
    };

    $scope.manualValidationAddRemove = function (divId, model, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull(model[str.replace(/\s/g, '')]))
            return manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };

    $scope.removeAttributeModal = function (index, list, childList, name, parentId) {
        $scope.vindex = index;
        $scope.chList = list;
        $scope.parentId = parentId;
        $scope.childList = childList;
        $scope.subMaterialMessage = 'Are you sure want to permanent delete ' + name + '.?';
        angular.element(document.querySelector('#delAttribute')).modal('show');
    };
    $scope.removeAttributeRow = function () {
        for (var t = baseService.arrayLength($scope[$scope.childList]) - 1; t >= 0; t--) {
            if ($scope[$scope.childList][t].OperationAttributeId === $scope.parentId)
                $scope[$scope.childList].splice(t, 1);
        }
        $scope[$scope.chList].splice($scope.vindex, 1);
        $scope.vindex = -1;
        $scope.chList = null;
    };

    function getAttributeList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetOperationAttribute?operationId=' + $scope.operationNew.Id
        }).then(function successCallback(response) {
            $scope.attributeList = response.data;
        });
    }

    // #endregion Attribute

    // #region Attribute Value

    $scope.valueIndex = -1;
    $scope.valueList = [];

    $scope.valuePopUp = function (attributeId, attributeName, index) {
        $scope.attributeIndex = index;
        $scope.valueAction = 'Add';
        $scope.oprationValue = {
            Id: null
            , OperationId: $scope.operationNew.Id
            , OperationAttributeId: attributeId
            , AttributeName: attributeName
            , Sequence: 0
            , Code: null
            , ShortName: null
            , StandardName: null
            , UserName: null
        };
        $scope.oprationValueNew = Object.assign({}, $scope.oprationValue);
        $scope.GetValueSequence();
        angular.element(document.querySelector('#operationAttributeValueEntryPopUp')).modal('show');
    };

    $scope.GetValueSequence = function () {
        if (baseService.arrayLength($scope.valueList) === 0) {
            $http.get($scope.path + '/GetValueSequence?operationAttributeId=' + $scope.oprationValueNew.OperationAttributeId)
                .then(function (response) {
                    $scope.oprationValueNew.Sequence = response.data;
                });
        }
        else
            $scope.oprationValueNew.Sequence = baseService.getMaxNumberFromList($filter("filter")($scope.valueList, { OperationAttributeId: $scope.oprationValueNew.OperationAttributeId }), 'Sequence');
    };

    $scope.addValue = function () {
        if ($scope.manualValidationAddRemove('value_Sequence', $scope.oprationValueNew, 'Sequence')) return;
        if ($scope.manualValidationAddRemove('value_Code', $scope.oprationValueNew, 'Code')) return;
        if ($scope.manualValidationAddRemove('value_ShortName', $scope.oprationValueNew, 'Short Name')) return;
        if ($scope.manualValidationAddRemove('value_StandardName', $scope.oprationValueNew, 'Standard Name')) return;
        if ($scope.manualValidationAddRemove('value_UserName', $scope.oprationValueNew, 'UserName', 'User Define Name')) return;

        var filterList = $filter("filter")($scope.valueList, { OperationAttributeId: $scope.oprationValueNew.OperationAttributeId });

        for (var t = 0; t < baseService.arrayLength(filterList); t++) {
            if (baseService.isAvailableInList(filterList[t].Code, $scope.oprationValueNew.Code, t, $scope.valueIndex))
                return manualValidation('value_Code', true, 'Code: ' + $scope.oprationValueNew.Code + ' already exist in this operation.');
            if (baseService.isAvailableInList(filterList[t].UserName, $scope.oprationValueNew.UserName, t, $scope.valueIndex))
                return manualValidation('value_UserName', true, 'User Define Name: ' + $scope.oprationValueNew.UserName + ' already exist in this operation.');
        }
        if (checkPropertiesAndCharLength($scope.attributeList[$scope.attributeIndex], $scope.oprationValueNew)) return;

        if ($scope.valueIndex === -1) {
            $scope.valueList.push({
                Id: $scope.oprationValueNew.Id
                , OperationId: $scope.oprationValueNew.OperationId
                , OperationAttributeId: $scope.oprationValueNew.OperationAttributeId
                , Sequence: $scope.oprationValueNew.Sequence
                , Code: $scope.oprationValueNew.Code
                , ShortName: $scope.oprationValueNew.ShortName
                , StandardName: $scope.oprationValueNew.StandardName
                , UserName: $scope.oprationValueNew.UserName
            });
        }
        else {
            $scope.valueList[$scope.valueIndex].Id = $scope.oprationValueNew.Id;
            $scope.valueList[$scope.valueIndex].OperationId = $scope.oprationValueNew.OperationId;
            $scope.valueList[$scope.valueIndex].OperationAttributeId = $scope.oprationValueNew.OperationAttributeId;
            $scope.valueList[$scope.valueIndex].Sequence = $scope.oprationValueNew.Sequence;
            $scope.valueList[$scope.valueIndex].Code = $scope.oprationValueNew.Code;
            $scope.valueList[$scope.valueIndex].ShortName = $scope.oprationValueNew.ShortName;
            $scope.valueList[$scope.valueIndex].StandardName = $scope.oprationValueNew.StandardName;
            $scope.valueList[$scope.valueIndex].UserName = $scope.oprationValueNew.UserName;
        }
        $scope.clearValue();
    };

    $scope.editvalue = function (data,index) {
        $scope.valueAction = 'Update';
        $scope.valueIndex = index;
        //angular.copy($scope.valueList[index], $scope.oprationValueNew);
        angular.copy(data, $scope.oprationValueNew);
        angular.element(document.querySelector('#operationAttributeValueEntryPopUp')).modal('show');
    };

    $scope.clearValue = function () {
        manualValidation('value_Sequence', false);
        manualValidation('value_Code', false);
        manualValidation('value_ShortName', false);
        manualValidation('value_StandardName', false);
        manualValidation('value_UserName', false);

        $scope.valueAction = 'Add';
        $scope.valueIndex = -1;
        $scope.oprationValue = {};
        $scope.oprationValueNew = {
            OperationId: $scope.oprationValueNew.OperationId
            , OperationAttributeId: $scope.oprationValueNew.OperationAttributeId
            , AttributeName: $scope.oprationValueNew.AttributeName
        };
        $scope.GetValueSequence();
    };

    $scope.closeValue = function () {
        manualValidation('value_Sequence', false);
        manualValidation('value_Code', false);
        manualValidation('value_ShortName', false);
        manualValidation('value_StandardName', false);
        manualValidation('value_UserName', false);
        $scope.oprationValue = {};
        $scope.oprationValueNew = {};
        angular.element(document.querySelector('#operationAttributeValueEntryPopUp')).modal('hide');
    };

    function checkPropertiesAndCharLength(parentList, model) {
        if (parentList.AttributeProperty === 'Integer') {
            if (!Number.isInteger(parseInt(model.UserName)))
                return manualValidation('value_UserName', true, 'User Define Name: ' + model.UserName + ' is not integer.');
        }
        else if (parentList.AttributeProperty === 'Decimal') {
            if (!baseService.checkDecimal(model.UserName))
                return manualValidation('value_UserName', true, 'User Define Name: ' + model.UserName + ' is not decimal.');
        }
        else {
            if (parentList.IsFixedNoOfCharacter) {
                var code = model.UserName;
                if (code.length !== parseInt(parentList.NoOfCharacter))
                    return manualValidation('value_UserName', true, 'Can not be greater than ' + parentList.NoOfCharacter);
            }
        }
    }
    $scope.operationNew = {};
    //var bool = true;
    //var IsMachineRequired = bool.toString();
    function getValueList() {
        $scope.valueList = [];
        if (baseService.isUndefinedOrNull($scope.operationNew.Id)) return;
        $http({
            method: 'GET'
            , url: $scope.path + 'GetOperationAttributeValueList?operationId=' + $scope.operationNew.Id
        }).then(function successCallback(response) {
            $scope.valueList = response.data;
        });
    }

    // #endregion Attribute Value

    $scope.removeRowModal = function (ob, index, list) {
        try {
            $scope.message_confirmation = "Are you sure want to permanent delete [" + ob.UserName + "] ";
            angular.element(document.querySelector('#confirmFgPopUp')).modal('show');
            $scope.popUpIndex = index;
            $scope.list = list;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        $scope[$scope.list].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmFgPopUp')).modal('hide');
    };

    $scope.changeMachineRequired = function () {
        $scope.operationNew.ArticleId = null;
        $scope.operationNew.ArticleName = null;
        $scope.operationNew.SkillId = null;
        $scope.operationNew.SkillName = null;
        $scope.operationNew.MachineAllowance = 0;
        $scope.operationNew.SPI = 0;
    };


    //Operation Report
    //$scope.GetIssueReportExcel = function () {
    //    var url = 'IssueTracker/IssueTransaction/GetIssueReportExcel?checkbox=' + $scope.WithSubCategory.CheckBox;
    //    $window.open(url, '_blank');
    //};

    $scope.GetOperationReportExcel = function () {
        var url = 'Machines/operation/GetOperationReportExcel';
        $window.open(url, '_blank');
    };

     

    $scope.onClickReportDownloadExcel = function (args) {
        //debugger;
        //var gridObj = $("#GridEdit").data("ejGrid");
        ////getting corresponding record 
        //var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        //$window.open($scope.path + 'FixedAssetsDisposePost?reportFormat=' + reportFormat + '&voucherId=' + data.Id, '_blank');


        try {
            var file_src = $scope.path + 'FixedAssetsDisposePost?reportFormat=' + reportFormat + '&disposedVoucherId=' + args.Id
            $rootScope.report(file_src/*, '_blank'*/);


            //var file_src = 'Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' +'202015848'
            //$rootScope.report(file_src, '_blank');

            // $window.open($scope.path + 'FixedAssetsDisposePost?reportFormat=' + reportFormat + '&disposedVoucherId=' + args.Id, '_blank');

            //$window.open('Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' + args.Id, '_blank');

            // ReportVendorInvoice ? reportFormat = Pdf & voucherId=202015865

        } catch (e) {

        }

    };
}

