'use strict';
machineController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function machineController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Machine";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.models = [];
    $scope.processList = [];
    $scope.skillProcessList = [];
    $rootScope.tempList = [];
    $scope.processIndex = -1;
    $scope.path = 'materials/materialmastermachineprocess/';
    //$scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.model = {
        Id: null
        , MaterialMasterId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , AssetMaster: null
        , UserName: null
        , BaseUom: null
        , SkillId: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $http.get('Machines/StitchCode/GetCbo')
        .then(function (response) {
            $scope.stitchCodeList = response.data;
        });

    $scope.searchModel = {
        Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUom: null
    };

    function getDetails() {
        $http.get($scope.path + 'getdetaillist?materialMasterId=' + $scope.modelNew.MaterialMasterId)
            .then(function (response) {
                $scope.skillProcessList = response.data;
                getSkillCbo();
            });
    }

    function getArticleList() {
        $http({
            method: 'GET'
            , url: $scope.path + 'GetArticleList?materialMasterId=' + $scope.modelNew.MaterialMasterId
            , contentType: "application/json; charset=utf-8"
        }).then(function successCallback(response) {
            $scope.modelList = response.data;
        });
    }

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.form.$valid) {
                angular.copy($scope.modelNew, $scope.model);
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: {
                        'materialMasterId': $scope.model.MaterialMasterId
                        , 'SkillId': $scope.model.SkillId
                        , 'entities': $scope.skillProcessList
                        , 'articleList': $scope.modelList
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, "failure");
                    else {
                        ShowResult(response.data.Message, "success");
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.MaterialMasterId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.MaterialMasterId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.buyerStyles.splice($scope.index, 1);
                    ClearFields();
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
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.model = {};
        $scope.modelNew = {};
        $scope.skillProcessList = [];
        $scope.skillList = [];
        $rootScope.tempList = [];
        $scope.modelList = [];
    }

    $scope.deleteModal = function (data, index) {
        $scope.deleteId = data.Id;
        $scope.message_confirmation = '';
        $scope.processIndex = index;
        $scope.message_confirmation = 'Are you sure want to permanently delete [ ' + data.UserName + ' ]';
        angular.element(document.querySelector('#confirmationPopUp')).modal('show');
    };
    $scope.removeRow = function () {

        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
                if ($rootScope.tempList[t].Id === $scope.skillProcessList[$scope.processIndex].Id)
                    $rootScope.tempList.splice(t, 1);
            }
            $scope.skillProcessList.splice($scope.processIndex, 1);
            $scope.processIndex = -1;
            $scope.modelNew.SkillId = null;
            getSkillCbo();
        } else {
            $http({
                method: 'POST',
                url: 'materials/materialmastermachineprocess/delete',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.skillProcessList.splice($scope.processIndex, 1);
                    $scope.processIndex = -1;
                    getSkillCbo();
                    if (baseService.arrayLength($scope.skillProcessList) == 0) {
                        $scope.model.SkillId = null;
                        $scope.modelNew.SkillId = null;

                        $scope.Save();
                    }
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }





        
       
    };

    // #region Process

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
    $scope.processPopUp = function () {
        angular.forEach($scope.skillProcessList, function (a) {
            $rootScope.tempList.push({
                Id: a.ProcessId
                , Sequence: a.Sequence
                , Code: a.Code
                , ShortName: a.ShortName
                , StandardName: a.StandardName
                , UserName: a.UserName
                //,SkillId: a.SkillId
                , Active: a.Active
                , Archive: false
            });
        });
        baseService.setCurrentPage('processList');
        $scope.getProcessData = function (pageno) {
            $scope.getProcessUrl = 'Processes/process/GetList?processid=[]'; //+ baseService.getColumnValueList($scope.skillProcessList, 'ProcessId');
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.processParameters)
                .then(function (result) {
                    $scope.processList = result.Rows;
                    $scope.processParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.processList.length; i++) {
                        $scope.processList[i].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.processList[i].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };
    $scope.CloseProcessPopUp = function () {
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };
    $rootScope.searchProcessByList = [

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
            'name': 'UserName',
            'value': 'UserName'
        }
    ];
    $scope.checkedOrUnchecked = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (baseService.valueCheckInList($rootScope.tempList, 'Id', data.Id) === false) {
                    $rootScope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $rootScope.tempList.length; i++) {
                    if ($rootScope.tempList[i].Id === data.Id) {
                        $rootScope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    // #endregion Process

    // #region add Skill

    $scope.addProcess = function () {
        $scope.modelNew.SkillId = null;
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.skillProcessList, 'ProcessId', a.Id)) {
                    $scope.skillProcessList.push({
                        Id: null
                        , MachineId: $scope.modelNew.Id
                        , ProcessId: a.Id
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , ShortName: a.ShortName
                        , StandardName: a.StandardName
                        , UserName: a.UserName
                        //, SkillId: null
                        , Active: a.Active
                    });
                }
            });
        }
        else
            $scope.skillProcessList = [];
        angular.forEach($scope.skillProcessList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.ProcessId))
                $scope.skillProcessList.splice(a, 1);
        });
        getSkillCbo();
        $scope.CloseProcessPopUp();
    };

    function getSkillCbo() {
        $http({
            method: 'GET'
            , url: 'skills/skill/GetCboByProcess?processIds=' + baseService.getColumnValueList($scope.skillProcessList, 'ProcessId')
        }).then(function successCallback(response) {
            $scope.skillList = response.data;
        });
    }

    // #endregion add Skill

    // #region MM

    $scope.materialModel = {
        materialTypeId: null
        , materialCategoryId: null
        , materialSubCategoryId: null
        , materialGroupMasterId: null
    };
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
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
    $scope.popUp = function () {
        $scope.popUpDataList = [];
        $scope.popUpUrl = $scope.path + 'getmaterialmasterlist';
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };
    $scope.selectDoubleClick = function (data) {
        $scope.modelNew = data;
        $scope.skillProcessList = [];
        $rootScope.tempList = [];
        getDetails();
        getArticleList();
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        $scope.materialModel = {};
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    // #endregion MM

    //#region  MachineMaster
    $scope.MachineMasterList = [];
    $scope.GetMachineMasterData = function (data) {
        try {
            $scope.ArticleData = data;
            $scope.MachineMasterList = [];
            $http({
                method: 'GET',
                url: 'Materials/MaterialMasterMachineProcess/GetMachineMasterData'
            }).then(function successCallback(response) {
                $scope.MachineMasterList = response.data;
            });
            angular.element(document.querySelector('#MachineMasterPopUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.SetMachineMasterData = function (obj) {
        var data = obj.data;
        $scope.ArticleData.MachineMasterId = data.Id;
        $scope.ArticleData.MachineMaster = data.UserName;
        angular.element(document.querySelector('#MachineMasterPopUp')).modal('hide');
    };

    $scope.CloseMachineMasterPopUp = function () {
        angular.element(document.querySelector('#MachineMasterPopUp')).modal('hide');
    }
    $scope.ClearMachineMasterData = function (data) {
        $scope.ArticleData = data;
        $scope.ArticleData.MachineMasterId = null;
        $scope.ArticleData.MachineMaster = null;
    };

    //#endregion  MachineMaster

}