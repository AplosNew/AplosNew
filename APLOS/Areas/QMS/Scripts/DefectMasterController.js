'use strict';
DefectMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DefectMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Defect Master';
    $rootScope.titleDP = 'Defect Point';
    $rootScope.titleAQL = 'AQL Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'QMS/QualityProcess/';
    $scope.getSeqUrl = $scope.path + 'getdefectmasterautosequence';
    $scope.saveUrl = $scope.path + 'createdefectmaster';
    $scope.saveProcessUrl = $scope.path + 'SaveDefectProcess';
    $scope.deleteUrl = $scope.path + 'deletedefectmaster/';
    $scope.getDPSeqUrl = $scope.path + 'getdefectpointautosequence';
    $scope.saveDPUrl = $scope.path + 'createdefectpoint';
    $scope.deleteDPUrl = $scope.path + 'deletedefectpoint/';
    $scope.saveAQLUrl = $scope.path + 'CreateAQLMaster';
    $scope.deleteAQLUrl = $scope.path + 'DeleteAQLMaster/';
    $scope.searchBy = "DefectNames"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'DefectCode', name: "DefectCode" }, { value: 'DefectNames', name: "DefectNames" }, { value: 'Remarks', name: "Remarks" }];


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    //#region Defect Master

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDefectMasterList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        SrNo: null,
        DefectCategory: null,
        DefectCode: null,
        Remarks: null,
        DefectNames: null,
        DefectsLocalName: null,
        ProcessId: null,
        QualityProcessId: null,
        TypesOfDefects: null,
        Zone: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.processList = [];
    $http({
        method: 'GET',
        url: "QMS/QualityProcess/GetProcessCbo",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.processList = response.data;

    });

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.SrNo = data;
            $scope.ModelNew.SrNo = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        getDefectMasterProcessList();
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
        $scope.ModelNew.SrNo = seq;
        $scope.userProcessList = [];
    }

    //#endregion

    // #region  DP
    $scope.searchByDP = "UserName"; $scope.searchDP = "";
    $scope.searchByDPList = [{ value: 'Id', name: "Id" }, { value: 'UserName', name: "UserName" }, { value: 'Remarks', name: "Remarks" }];
    $scope.DPModelList = [];
    $scope.getDPData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDefectPointList",
            data: { column: $scope.searchByDP, value: $scope.searchDP },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DPModelList = response.data;
        });
    }
    $scope.getDPData();

    $scope.ModelDPTemp = {
        Id: null,
        SrNo: null,
        UserName: null,
        ZoneA: null,
        ZoneB: null,
        ZoneC: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.ModelNewDP = Object.assign({}, $scope.ModelDPTemp);

    $scope.GetDPSequence = function () {
        cboService.getSequence($scope.getDPSeqUrl, function (data) {
            $scope.ModelDPTemp.SrNo = data;
            $scope.ModelNewDP.SrNo = data;
        });
    };
    $scope.GetDPSequence();

    $scope.GetDP = function (args) {
        $scope.ModelNewDP = Object.assign({}, args.data);
        $scope.DPAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDP = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewDPForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveDPUrl,
                data: { 'data': $scope.ModelNewDP },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearDPFields(response.data.Sequence);
                    $scope.getDPData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.removeDP = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewDP.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently.';
        angular.element(document.querySelector('#confirmDelPopUp')).modal('show');
    }

    $scope.DeleteDP = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewDP.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteDPUrl + $scope.ModelNewDP.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearDPFields(response.data.Sequence);
                    $scope.getDPData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearDP = function () {
        ClearDPFields($scope.GetDPSequence());
        return true;
    };
    $scope.DPAction = 'Save';
    function ClearDPFields(seq) {
        $scope.DPAction = 'Save';
        $scope.ModelNewDP = Object.assign({}, $scope.ModelDPTemp);
        $scope.ModelNewDP.SrNo = seq;
    }
    //#endregion

    // #region  AQL
    $scope.searchByAQL = "FromLotSize"; $scope.searchDP = "";
    $scope.searchByAQLList = [{ value: 'Id', name: "Id" }, { value: 'FromLotSize', name: "FromLotSize" }, { value: 'SampleSize', name: "SampleSize" }];
    $scope.AQLModelList = [];
    $scope.getAQLData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetAQLMasterList",
            data: { column: $scope.searchByAQL, value: $scope.searchAQL },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.AQLModelList = response.data;
        });
    }
    $scope.getAQLData();

    $scope.ModelAQLTemp = {
        Id: 0,
        FromLotSize: 0,
        ToLotSize: 0,
        SampleSize: 0,
        AQLLevel: 0,
        Accept: 0,
        Reject: 0,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.ModelNewAQL = Object.assign({}, $scope.ModelAQLTemp);


    $scope.GetAQL = function (args) {
        $scope.ModelNewAQL = Object.assign({}, args.data);
        $scope.AQLAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveAQL = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewAQLForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveAQLUrl,
                data: { 'data': $scope.ModelNewAQL },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearAQLFields();
                    $scope.getAQLData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.removeAQL = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewAQL.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently.';
        angular.element(document.querySelector('#confirmDelAQLPopUp')).modal('show');
    }

    $scope.DeleteAQL = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewAQL.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteAQLUrl + $scope.ModelNewAQL.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearAQLFields();
                    $scope.getAQLData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearAQL = function () {
        ClearAQLFields();
        return true;
    };
    $scope.AQLAction = 'Save';
    function ClearAQLFields() {
        $scope.AQLAction = 'Save';
        $scope.ModelNewAQL = Object.assign({}, $scope.ModelAQLTemp);
    }
    //#endregion

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
                            Id: -(Math.floor(Math.random() * 100) + 1)
                            , ProcessId: a.Id
                            , DefectMasterId: $scope.ModelNew.Id
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
    $scope.userProcessList = [];
    function getDefectMasterProcessList() {
        $http({
            method: 'GET',
            url: 'QMS/QualityProcess/GetDefectMasterProcessList?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.userProcessList = response.data;
        });
    }

    $scope.SaveProcess = function () {
        $http({
            method: 'POST',
            url: $scope.saveProcessUrl,
            data: { 'data': $scope.userProcessList, 'masterId': $scope.ModelNew.Id  },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                getDefectMasterProcessList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope._SaveProcess = function () {
        try {
            if ($scope.userProcessList > 0) {
                $http({
                    method: 'POST',
                    url: "QMS/QualityProcess/SaveDefectProcess",
                    data: { 'data': $scope.userProcessList, 'masterId': $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getDefectMasterProcessList();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.message_confirmation = null;
    $scope.removeProcess = function (obj) {
        $scope.processobj = obj;
        if (!baseService.isUndefinedOrNull($scope.processobj.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [' + $scope.processobj.Process + ' ]';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    }

    $scope.DeleteProcess = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/DeleteDefectMasterProcess?id=' + $scope.processobj.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                getDefectMasterProcessList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // #endregion Process
}