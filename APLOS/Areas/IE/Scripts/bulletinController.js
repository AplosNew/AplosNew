'use strict';
BulletinController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$sce'];
function BulletinController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $scope.maxRow = 10;
    $rootScope.title = 'Bulletin';
    $scope.Action = 'Save';
    $scope.gridDetailGrid = false;
    $scope.btnDetailEntryPopup = false;
    $scope.btndeletemaster = true;
    $scope.isdeletedetail = false;
    $scope.message_confirmation = '';
    $scope.ActionDetail = 'Save';
    $scope.SaveDetailDisabled = false;
    $scope.btnPrintShow = false;
    $scope.btnSummaryShow = false;
    $scope.path = 'IE/bulletin/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrlMaster = $scope.path + 'createmaster';
    $scope.saveUrlDetail = $scope.path + 'createdetail';
    $scope.deleteUrlmaster = $scope.path + 'deletemaster';
    $scope.deleteUrlDetail = $scope.path + 'deletedetail';
    $scope.generateExcelUrl = $scope.path + 'GenerateExcel';

    ///list
    $scope.zoneList = [];
    $scope.componentList = [];
    $scope.operationActionList = [];
    $scope.searchbythirdpartyList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Time Measurement Unit',
            'value': 'TMU'
        },
        {
            'name': 'Type',
            'value': 'Type'
        },
        {
            'name': 'Grouping',
            'value': 'Grouping'
        }
    ];
    $scope.manpowerTypeList = [];
    $scope.bulletinmasterList = [];
    $scope.bulletindetailList = [];
    $scope.processData = [];
    $scope.buyerData = [];
    $scope.materialMasterData = [];
    $scope.machineTypeData = [];
    $scope.operationData = [];

    $scope.bulletinmaster = {
        Id: null
        , Description: null
        , WorkingHour: null
        , Sequence: null
    };
    $scope.bulletinmastermodal = {
        Id: null
        , MaterialMasterId: null
        , MaterialMasterName: null
        , MaterialMasterArticleId: null
        , ArticleName: null
        , Description: null
        , WorkingHour: null
        , Sequence: null
    };
    $scope.operaionclass = {
        Id: null,
        Code: null,
        StandardTime: 0,
        OperationTypeCode: null,
        IsMachineRequire: null,
        Sequence: null
    };
    ///other
    $scope.index = -1;
    $scope.masterindex = -1;
    $scope.detailindex = -1;
    ///declaration ends-----------------------------------------------------------------------------------------------------
    ///2.common function----------------------------------------------------------------------------------------------------

    ///**************************************************get data from database*********************************
    $scope.getData = function () {
        $scope.searchbyMasterlist = [];
        baseService.setCurrentPage('bulletinmasterList');
        baseService.init($scope.path + 'getlist', null, $scope.maxRow, null, 'Description', 'Description');
        $scope.loadMasterData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.bulletinmasterList = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMasterlist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMasterlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMasterData();
    }
    $scope.operationParameters = {
        limit: 2,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: 'UserName',
        pageSize: 2,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getOperationData = function () {
        $scope.popUpUrl = $scope.path + 'getoperationlist';
        $scope.loadOperationData = function (pageno) {
            $scope.operationParameters.processid = $scope.bulletindetailmodal.ProcessId;
            $scope.operationParameters.fgcomponentid = $scope.bulletindetailmodal.ComponentId;
            $scope.operationParameters.operationactionid = $scope.bulletindetailmodal.OperationActionId;
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.operationParameters)
                .then(function (result) {
                    $scope.operationData = result.Rows;
                    $scope.operationParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.loadOperationData();
    }
    $scope.getMasterData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getbulletinmasterlist?masterid=' + masterid,
        }).then(function successCallback(response) {
            $scope.bulletinmasterList = [];
            $scope.bulletinmasterList = response.data;
            if (baseService.arrayLength($scope.bulletinmasterList) > 0) {
                $scope.bulletinmaster = $scope.bulletinmasterList[0];
                //show add detail button
                if ($scope.bulletinmaster.Id != null && $scope.bulletinmaster.Id.length > 0) {//add edit
                    $scope.btnDetailEntryPopup = true;
                }//not null
            }//if length>0
        })//success
    }
    function getSummaryData() {
        var _totalsam = 0;
        var _taotalmanp = 0;
        try {
            var summaryData = {
                Sequence: null,
                Zone: null,
                Component: null,
                OperationDescription: null,
                UserDefinedSPT: null,
                AllotedManpower: null
            };
            $scope.summaryDataList = [];
            for (var i = 0; i < baseService.arrayLength($scope.bulletindetailList); i++) {
                var summaryData = angular.copy(summaryData);
                summaryData.Sequence = $scope.bulletindetailList[i].Sequence;
                summaryData.Zone = $scope.bulletindetailList[i].Zone;
                summaryData.Component = $scope.bulletindetailList[i].Component;
                summaryData.OperationDescription = $scope.bulletindetailList[i].OperationDescription;
                summaryData.UserDefinedSPT = $scope.bulletindetailList[i].UserDefinedSPT;
                summaryData.AllotedManpower = $scope.bulletindetailList[i].AllotedManpower;
                var sam = summaryData.UserDefinedSPT;
                var mp = summaryData.AllotedManpower;
                _totalsam += sam;
                _taotalmanp += mp;
                $scope.summaryDataList.push(summaryData);
            }
            $scope.TotalSAM = _totalsam.toFixed(2);
            $scope.TotalManpower = _taotalmanp;
        } catch (e) {
            throw e;
        }
    }

    ///**************************************************search ddl list*********************************
    $scope.searchbyMaterialMasterDatalist = [];
    $scope.searchbyBuyerDatalist = [];
    $scope.searchbyOperationDatalist = [
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
            'name': 'Operation Type',
            'value': 'OperationTypeCode'
        },
        {
            'name': 'Operation Category',
            'value': 'OperationCategoryName'
        }
    ];
    $scope.searchbyMachineTypelist = [];
    $scope.searchbyMasterlist = [];

    ///**************************************************grid row selected event function*********************************
    $scope.getOperationCode = function (data) {
        $scope.bulletindetailmodal.OperationId = data.Id;
        $scope.bulletindetailmodal.OperationDescription = data.UserName;
        $scope.bulletindetailmodal.OperationActionId = data.OperationActionId;
        $scope.bulletindetailmodal.OperationActionName = data.OperationActionName;
        $scope.bulletindetailmodal.OperationType = data.OperationTypeCode;
        $scope.bulletindetailmodal.MachineRequired = data.IsMachineRequired;
        $scope.bulletindetailmodal.MachineExecutiontype = data.IsMachineRequired;
        $scope.bulletindetailmodal.MaterialMasterMaterialMasterArticleId = data.MaterialMasterMaterialMasterArticleId;
        $scope.bulletindetailmodal.Machine = data.Machine;
        $scope.bulletindetailmodal.FixedAssetCategory = data.FixedAssetCategory;
        $scope.bulletindetailmodal.FixedAssetSubCategory = data.FixedAssetSubCategory;
        $scope.bulletindetailmodal.AssetItem = data.AssetItem;
        $scope.bulletindetailmodal.AssetType = data.AssetType;
        angular.element(document.querySelector('#operationmodal')).modal('hide');
    };
    $scope.clearOperationCode = function (id, code) {
        $scope.bulletindetailmodal.OperationId = null;
        $scope.bulletindetailmodal.OperationDescription = null;
        $scope.bulletindetailmodal.OperationActionId = null;
        $scope.bulletindetailmodal.OperationActionName = null;
        $scope.bulletindetailmodal.OperationType = null;
        $scope.bulletindetailmodal.MachineRequired = null;
        $scope.bulletindetailmodal.MachineExecutiontype = null;
        $scope.bulletindetailmodal.MaterialMasterMaterialMasterArticleId = null;
        $scope.bulletindetailmodal.Machine = null;
        $scope.bulletindetailmodal.FixedAssetCategory = null;
        $scope.bulletindetailmodal.FixedAssetSubCategory = null;
        $scope.bulletindetailmodal.AssetItem = null;
        $scope.bulletindetailmodal.AssetType = null;
    };
    $scope.GetMasterIndex = function (id, index) {
        $scope.masterindex = index;
        $scope.bulletinmaster = $scope.bulletinmasterList[$scope.masterindex];
        $scope.getDetailData($scope.bulletinmaster.Id);
        $scope.gridDetailGrid = true;
        $scope.btnDetailEntryPopup = true;
        $scope.btnDetailEntryPopup = true;
        $scope.btnDetailEntryPopup = true;
        $scope.btnPrintShow = true;
        $scope.btnSummaryShow = true;
        // $scope.bulletinmastermodal = $scope.bulletinmasterList[$scope.masterindex];
        angular.element(document.querySelector('#mastersearchpopup')).modal('hide');
    };

    ///**************************************************save function*********************************
    function ValidationMaster() {
        try {
            if ($scope.bulletinmastermodal.WorkingHour <= 0) throw 'WorkingHour must be greater than Zero.';
        } catch (e) {
            throw e;
        }
    }
    function HasDuplicate(zoneid, componentid, operationid) {
        for (var i = 0; i < baseService.arrayLength($scope.bulletindetailList); i++) {
            if ($scope.bulletindetailmodal.Id != $scope.bulletindetailList[i].Id) {
                if (zoneid == $scope.bulletindetailList[i].ZoneId) {
                    if (componentid == $scope.bulletindetailList[i].ComponentId) {
                        if (operationid == $scope.bulletindetailList[i].OperationId) {
                            throw 'Id:[' + $scope.bulletindetailList[i].Id + '] has already same Zone, Component and Operation !!!';
                        }
                    }//component
                }//zone
            }//id
        }//for
    }
    function ValidationDetail() {
        try {
            if ($scope.bulletindetailmodal.ZoneId == null || $scope.bulletindetailmodal.ZoneId.length == 0) {
                throw '[Zone] can not be blank...';
            }
            if ($scope.bulletindetailmodal.ComponentId == null || $scope.bulletindetailmodal.ComponentId.length == 0) {
                throw '[Component] can not be blank...';
            }
            if ($scope.bulletindetailmodal.ManpowerBudgetName == null || $scope.bulletindetailmodal.ManpowerBudgetName.length == 0) {
                throw '[Designation Group] can not be blank...';
            }
            if ($scope.bulletindetailmodal.Manpowertype == null || $scope.bulletindetailmodal.Manpowertype.length == 0) {
                throw '[Manpower Type] can not be blank...';
            }
            if ($scope.bulletindetailmodal.OperationId == null || $scope.bulletindetailmodal.OperationId.length == 0) {
                throw '[Operation] can not be blank...';
            }
            HasDuplicate($scope.bulletindetailmodal.ZoneId, $scope.bulletindetailmodal.ComponentId, $scope.bulletindetailmodal.OperationId);
            if ($scope.bulletindetailmodal.IsMachineRequire && baseService.isUndefinedOrNull($scope.bulletindetailmodal.MaterialMasterId)) {
                throw 'Machine is required';
            }
            if ($scope.bulletindetailmodal.MachineExecutiontype == null || $scope.bulletindetailmodal.MachineExecutiontype.length == 0) {
                throw '[Machine Execution Type] can not be blank...';
            }
            if ($scope.bulletindetailmodal.UserDefinedSAM == null || $scope.bulletindetailmodal.OperationId.UserDefinedSAM == 0) {
                throw '[User Defined SAM] can not be blank...';
            }
            if ($scope.bulletindetailmodal.AllotedWorkstation == null || $scope.bulletindetailmodal.AllotedWorkstation.length == 0) {
                throw '[Alloted Workstation] can not be blank...';
            }
            if ($scope.bulletindetailmodal.AllotedManpower == null || $scope.bulletindetailmodal.AllotedManpower.length == 0) {
                throw '[Alloted Manpower] can not be blank...';
            }
            if ($scope.bulletindetailmodal.Sequence == null || $scope.bulletindetailmodal.Sequence.length == 0) {
                throw '[Sequence] can not be blank...';
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.SaveMaster = function () {
        try {
            ValidationMaster();
            $scope.ModalToMainPage();
            $http({
                method: 'POST',
                url: $scope.saveUrlMaster,
                dataType: 'JSON',
                data: { 'master': $scope.bulletinmaster }
            }).then(function successCallback(response) {
                if (response.data.Error == true)
                    ShowResult(response.data.Message, 'Error');
                else {
                    try {
                        var vv = typeof (response.data.Message);
                        if (vv == 'undefined')
                            throw 'Url or Function in the controller is invalid...';
                        ShowResult(response.data.Message, 'success');
                        $scope.getMasterData(response.data.id)
                        angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                        if ($scope.Action != 'Save')
                            $scope.Action = 'Save';
                    } catch (e) {
                        ShowResult(e, 'Error');
                    }
                }//success
            }, function errorCallback(response) {
                throw status.Message;
            });
            return true;
        }
        catch (e) {
            ShowResult(e, 'Error', 'masteraddeditpopup');
        }
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        //var fdata = new FormData();
        //fdata.append('file', $scope.filedata);
        //fdata.append('fileName', '99');
        //fdata.append('model', angular.toJson(data.model));
        //fdata.append('Id', '20161');
        //data: { 'glGeneralInfo': $scope.glinfo, 'glCompanyInfo': $scope.newGlcominfo, 'glAccountType': $scope.glaccounttypies },
        if ($scope.Action == 'Save') {
            $http({
                method: 'POST',
                url: $scope.saveUrlMaster,
                withCredentials: true,
                processData: false,
                //headers: { 'Content-Type': undefined },
                //dataType: 'JSON',
                data: { 'file': $scope.filedata, 'cm': $scope.timecapture, 'detail': $scope.fromToTable },
                transformRequest: angular.identity
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //$scope.SaveChild();
                    //ShowResult(data.Message, 'success');
                    //$scope.timecaptureList.push(data.TimeCapture);
                    //baseService.paginationAdd();
                    //ClearFields(data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
        else if ($scope.Action == 'Update') {
            $http({
                method: 'POST',
                url: $scope.updateUrl,
                data: $scope.timeCapture,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    if ($scope.index > -1) {
                        $scope.timeCaptureList[$scope.index] = $scope.timecapture;
                    }
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
        //}
    }
    $scope.DeleteMaster = function () {
        $scope.bulletinmaster.Id = $scope.bulletinmastermodal.Id;
        $http({
            method: 'POST',
            url: $scope.deleteUrlmaster,
            dataType: 'JSON',
            data: { 'masterid': $scope.bulletinmaster.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                ClearAll();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    }

    $scope.showPK = function (id) {
        if (id != null && id != '') return true;
        else return false;
    }
    $scope.MainPageToModal = function () {
        angular.copy($scope.bulletinmaster, $scope.bulletinmastermodal);
        cboService.getBuyerStyleCboByBuyer($scope.bulletinmastermodal.BuyerId, function (result) {
            $scope.buyerStyleList = [];
            $scope.buyerStyleList = result;
        });
    }
    $scope.ClearMasterModal = function () {
        $scope.sprocessList = [];
        for (var i in $scope.bulletinmastermodal) {
            $scope.bulletinmastermodal[i] = null;
        }
        $scope.hasAttribute = false;
    }
    $scope.ClearDetailModal = function () {
        for (var i in $scope.bulletindetailmodal) {
            $scope.bulletindetailmodal[i] = null;
        }
    }
    $scope.ClearMaster = function () {
        $scope.sprocessList = [];
        for (var i in $scope.bulletinmaster) {
            $scope.bulletinmaster[i] = null;
        }
        $scope.hasAttribute = false;
    }
    $scope.ClearDetail = function () {
        for (var i in $scope.bulletindetail) {
            $scope.bulletindetail[i] = null;
        }
    }
    $scope.ModalToMainPage = function () {
        for (var i in $scope.bulletinmaster) {
            $scope.bulletinmaster[i] = $scope.bulletinmastermodal[i];
        }
    }
    $scope.CancelDetail = function () {
        angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
    }
    function ClearAll() {
        $scope.ClearMasterModal();
        $scope.ClearMaster();
        $scope.ClearDetail();
        $scope.ClearDetailModal();
        $scope.bulletindetailList = [];
        $scope.Action = 'Save';
        $scope.gridDetailGrid = false;
        $scope.btnDetailEntryPopup = false;
        $scope.btndeletemaster = true;
        $scope.message_confirmation = '';
        $scope.ActionDetail = 'Save';//SaveDetailDisabled
        $scope.SaveDetailDisabled = false;//DeleteMaster
    }

    $scope.summaryPopup = function () {
        getSummaryData();
        angular.element(document.querySelector('#summarymodal')).modal('show');
    }
    $scope.masterAddEditPopup = function (flag) {
        if (flag == 'NEW') {
            $scope.isdeletedetail = false;
            $scope.btndeletemaster = false;
            $scope.gridDetailGrid = false;
            $scope.btnDetailEntryPopup = false;
            $scope.btnPrintShow = false;
            $scope.btnSummaryShow = false;
            $scope.Action = 'Save';
            $scope.ClearMasterModal();
            $scope.ClearMaster();
            $scope.bulletindetailList = [];
        }
        else {
            $scope.isdeletedetail = true;
            $scope.btndeletemaster = true;
            $scope.Action = 'Update';
            $scope.MainPageToModal();
        }
        angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
    };
    $scope.masterSearchPopup = function () {
        $scope.getData();
        angular.element(document.querySelector('#mastersearchpopup')).modal('show');
    };

    $scope.showOperationModal = function () {
        if ($scope.bulletindetailmodal.ComponentId == null)
            return ShowResult('Please select component.......!', 'failure', 'detailentrypopup');
        $scope.getOperationData();
        angular.element(document.querySelector('#operationmodal')).modal('show');
    };
    $scope.viewOperation = function () {
        angular.element(document.querySelector('#viewOperationPop')).modal('show');
    };
    $scope.closeViewOperation = function () {
        angular.element(document.querySelector('#viewOperationPop')).modal('hide');
    };
    $scope.viewArticle = function () {
        angular.element(document.querySelector('#viewAriclePop')).modal('show');
    };
    $scope.closeViewAriclePop = function () {
        angular.element(document.querySelector('#viewAriclePop')).modal('hide');
    };

    $scope.clearOpMachine = function () {
        $scope.clearOperationCode();
    }
    $scope.deleteMaster = function () {
        var _id = $scope.bulletinmastermodal.Id;
        $scope.message_confirmation = 'Are you sure to delete [' + _id + '] ';
        //$rootScope.passValue(_id, $scope.masterindex);
    }
    $scope.removeMasterYes = function () {
        $scope.DeleteMaster();
    };

    $scope.deleteDetailInGrid = function (index) {
        $scope.message_confirmation = 'Are you sure to delete [' + $scope.bulletindetailmodal.Id + '] ';
        $rootScope.passValue($scope.bulletindetailmodal.Id, index);
    }
    //For Detail

    ///5.Report call******************************************************************************************************
    $scope.genExcel = function () {
        $http({
            method: 'POST',
            url: $scope.generateExcelUrl,
            data: { 'masterid': $scope.bulletinmaster.Id },
            responseType: 'arraybuffer',
            headers: {
                'Content-type': 'application/json',
                'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            }
        }).then(function successCallback(response) {
            var blob = new Blob([response.data], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            });
            var objectUrl = URL.createObjectURL(blob);
            window.open(objectUrl);
            //saveAs(blob, 'File_Name_With_Some_Unique_Id_Time' + '.xlsx');
        })
    }
    ///service
    baseService.init($scope.getListUrl, null, 25, null, 'Process', 'Process');

    // #region Process
    $rootScope.tempList = [];
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
        baseService.setCurrentPage('processList');
        $scope.getProcessData = function (pageno) {
            $scope.getProcessUrl = 'Processes/CompanyProcess/GetCompanyProductionProcessList?processIds=' + baseService.getColumnValueList($scope.sprocessList, 'ProcessId');
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
    $scope.SaveProcess = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            var list = [];
            angular.forEach($rootScope.tempList, function (a) {
                list.push({
                    ProcessId: a.Id,
                });
            });

            $http({
                method: 'POST',
                url: $scope.path + 'createprocess',
                data: {
                    'masterId': $scope.bulletinmaster.Id
                    , 'processList': list
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) return ShowResult(response.data.Message, 'failure', 'processPopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'processPopUp');
                    $scope.getDetailData($scope.bulletinmaster.Id);
                    $scope.closeProcess();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'processPopUp');
            }
        }
    };
    $scope.removeProcessRowModal = function (ob, index) {
        try {
            $scope.processId = ob.Id
            $scope.message_confirmation = "Are you sure want to permanent delete [" + ob.UserName + "].";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
            $scope.popUpIndex = index;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeProcessRow = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'deleteprocess?id=' + $scope.processId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) return ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success', 'processPopUp');
                $scope.sprocessList.splice($scope.popUpIndex, 1);
                $scope.popUpIndex = -1;
                $scope.closeProcess();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'processPopUp');
        }
    };
    $scope.closeProcess = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };
    $scope.getDetailData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getbulletinprocesslist?masterid=' + masterid,
        }).then(function successCallback(response) {
            $scope.sprocessList = [];
            $scope.sprocessList = response.data;
        })
    }
    // #endregion

    // #region Details
    $scope.loadDDL = function () {
        $http.get($scope.path + 'getzonecbo/')
            .then(function (response) {
                $scope.zoneList = [];
                $scope.zoneList = response.data;
            });
        $http.get($scope.path + 'getcomponentcbo/')
            .then(function (response) {
                $scope.componentList = [];
                $scope.componentList = response.data;
            });

        $http.get($scope.path + 'getoperationactioncbo/')
            .then(function (response) {
                //console.log(response);
                $scope.operationActionList = [];
                $scope.operationActionList = response.data;
            });
    };
    $scope.bulletindetail = {
        Id: null,
        BulletinMasterId: null,
        ProcessId: null,
        ZoneId: null,
        Zone: null,
        ComponentId: null,
        Component: null,
        ManpowerBudgetId: null,
        ManpowerBudgetName: null,
        Sequence: null,
        Manpowertype: null,
        OperationId: null,
        UserName: null,
        OperationNo: null,
        OperationType: null,
        OperationDescription: null,
        OperationSequence: null,
        MachineRequired: null,
        MachineExecutiontype: null,

        MaterialMasterMaterialMasterArticleId: null,
        Machine: null,
        AssetItem: null,
        FixedAssetCategory: null,
        FixedAssetSubCategory: null,
        AssetType: null,

        Active: null,
        UserDefinedSAM: null,
        OperationTargetPerHour: null,
        RequiredManpower: false,
        AllotedWorkstation: null,
        AllotedManpower: null,
        IsPrintable: null,
        IsLastOperation: null,
        IsDirect: null,
        Remark: null,
        Archive: false
    };
    $scope.bulletindetailmodal = Object.assign({}, $scope.bulletindetail)
    $scope.getDetails = function (data) {
        $scope.bulletindetailmodal.BulletinMasterId = data.BulletinId;
        $scope.bulletindetailmodal.ProcessId = data.ProcessId
        getDetailList();
        $scope.loadDDL();
        angular.element(document.querySelector('#detailentrypopup')).modal('show');
    };
    function getDetailList(data) {
        $http({
            method: 'GET',
            url: $scope.path + 'getbulletindetaillist?masterId=' + $scope.bulletindetailmodal.BulletinMasterId
            + '&processId=' + $scope.bulletindetailmodal.ProcessId,
        }).then(function successCallback(response) {
            $scope.bulletindetailList = [];
            $scope.bulletindetailList = response.data;
        });
        angular.element(document.querySelector('#detailentrypopup')).modal('show');
    };
    $scope.deleteDetail = function (index) {
        var _id = $scope.bulletindetailList[index].Id;
        $scope.message_confirmation = 'Are you sure to permanent delete [' + _id + '] ';
        $scope.bulletindetailmodal.Id = _id;
    }
    $scope.removeRowYes = function () {
        $scope.DeleteDetail();
    };
    $scope.getDetailRow = function (index) {
        $scope.ActionDetail = 'Update';
        angular.copy($scope.bulletindetailList[index], $scope.bulletindetail);
        angular.copy($scope.bulletindetail, $scope.bulletindetailmodal);
    }
    $scope.DeleteDetail = function () {
        $scope.bulletindetail.Id = $scope.bulletindetailmodal.Id;
        $http({
            method: 'POST',
            url: $scope.deleteUrlDetail,
            dataType: 'JSON',
            data: { 'detailid': $scope.bulletindetail.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                getDetailList();
                $scope.bulletindetailClear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    }
    $scope.SaveDetail = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.bulletinDetailForm.$valid) {
                angular.copy($scope.bulletindetailmodal, $scope.bulletindetail);
                $http({
                    method: 'POST',
                    url: $scope.saveUrlDetail,
                    dataType: 'JSON',
                    data: { 'detail': $scope.bulletindetail }
                }).then(function successCallback(response) {
                    if (response.data.Error == true)
                        return ShowResult(response.data.Message, 'Error', 'bulletinDetail');
                    else {
                        ShowResult(response.data.Message, 'success');
                        getDetailList();
                        $scope.bulletindetailClear();
                    }
                }, function errorCallback(response) {
                    $scope.SaveDetailDisabled = false;
                    throw response.status.Message;
                });

                return true;
            }//valid
        } catch (e) {
            $scope.SaveDetailDisabled = false;
            ShowResult(e, 'Error', 'bulletinDetail');
        };
    }
    $scope.bulletindetailClear = function () {
        $scope.ActionDetail = 'Save';
        $scope.bulletindetail = {};
        $scope.bulletindetailmodal = { BulletinMasterId: $scope.bulletindetailmodal.BulletinMasterId, ProcessId: $scope.bulletindetailmodal.ProcessId }
    }
    $scope.bulletindetailClose = function () {
        $scope.bulletindetail = {};
        $scope.bulletindetailmodal = {}
        angular.element(document.querySelector('#detailentrypopup')).modal('hide');
    }
    // #endregion

    // #region Manpower budget
    $scope.getManPowerModal = function () {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'Code',
            searchBy: 'PositionCode',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUpTitle = 'ManPowerBudget Profile';
        $scope.popUpUrl = 'Organizations/ManpowerBudget/GetForResponsiblePerson';
        $scope.popUpList = [];
        $scope.popUpDataList = [];
        baseService.setCurrentPage('dataList');
        $scope.getManPowerPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) == 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#manPowerPopUpId')).modal('show');
        $scope.getManPowerPopUpData();
    };
    $scope.selectManPower = function (data) {
        $scope.bulletindetailmodal.ManpowerBudgetId = data.Id;
        $scope.bulletindetailmodal.ManpowerBudgetName = data.Code;
        $scope.closeManPowerPopUp();
    }
    $scope.closeManPowerPopUp = function () {
        angular.element(document.querySelector('#manPowerPopUpId')).modal('hide');
    }
    // #endregion

    // #region Material
    $scope.hasAttribute = false;
    $scope.materialPopUp = function () {
        $scope.materialPopUpList = [];
        $scope.materialPopUpDataList = [];
        $scope.excluedColumnList = ['IsOurStyleRequired', 'IsProductMstRequired', 'OurStyle', 'ProductMaster', 'Buyer', 'WithSKU', 'HasAttribute'];
        $scope.materialPopUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'UserName',
            searchBy: 'UserName',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase('Productions/SalesOrderLinear/MM_FG_Article', pageno, $scope.materialPopUpParameters)
                .then(function (result) {
                    $scope.materialPopUpDataList = result.Rows;
                    $scope.materialPopUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.materialPopUpList) == 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.materialPopUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'materialPopup');
                }).finally(function () {
                });
        };
        $scope.getPopUpData();
        angular.element(document.querySelector('#materialPopup')).modal('show');
    }
    $scope.setMaterialMasterData = function (ob) {
        $scope.bulletinmastermodal.MaterialMasterId = ob.Id;
        $scope.bulletinmastermodal.MaterialMasterName = ob.UserName;
        $scope.bulletinmastermodal.MaterialMasterArticleId = null;
        $scope.bulletinmastermodal.ArticleName = null;
        $scope.hasAttribute = ob.HasAttribute;
        if ($scope.hasAttribute) $scope.getArticleList(ob.Id);
        angular.element(document.querySelector('#materialPopup')).modal('hide');
    };

    $scope.getArticleList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.artData = [];
            baseService.setCurrentPage('artData');
            baseService.init('Productions/SalesOrderLinear/GetArticlListByMaterialStyle', null, null, null, 'StandardName', 'StandardName');
            $scope.loadArtData = function (pageno) {
                $rootScope.parameters.materialMasterId = id;
                baseService.pagination(pageno)
                    .then(function (result) {
                        $scope.artData = result;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArtData();
            angular.element(document.querySelector('#articlePop')).modal('show');
        } catch (e) {
            ShowResult(e, '', 'masteraddeditpopup');
        }
    }
    $scope.selectarticle = function (ob) {
        try {
            $scope.bulletinmastermodal.MaterialMasterArticleId = ob.Id;
            $scope.bulletinmastermodal.ArticleName = ob.StandardName;
            angular.element(document.querySelector('#articlePop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articlePop');
        }
    };
    // #endregion
};