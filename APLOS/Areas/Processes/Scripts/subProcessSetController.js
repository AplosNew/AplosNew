'use strict';
SubProcessSetController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function SubProcessSetController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = "SubProcess Set";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.subProcessSets = [];
    $scope.path = 'Processes/subprocessset/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Entity", "Entity");

    $scope.subProcessSet = {
        Id: null
        , CompanyGroupId: $window.companyGroupId
        , CompanyId: null
        , EntityId: null
        , Entity: null
        , ProcessId: null
        , Process: null
        , ProcessTypeId: null
        , ProcessType: null
        , Code: null
        , RequiredTimeUnit: null
        , Description: null
    };
    $scope.subProcessSetNew = Object.assign({}, $scope.subProcessSet);

    $scope.getData = function (pageno) {
        $rootScope.parameters.entityId = $scope.subProcessSetNew.EntityId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.subProcessSets = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.searchFromProcessSetList = [
        {
            'name': 'Entity',
            'value': 'Entity'
        },
        {
            'name': 'Process',
            'value': 'Process'
        },
        {
            'name': 'Process Type',
            'value': 'ProcessType'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    ];

    // #region DDL

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    cboService.getEnumCbo('enum/getenumrequiredtimeunitcbo', function (result) {
        $scope.requiredTimeUnitList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.subProcessSetNew.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };


    $scope.entityList = [];
    $scope.getEntity = function () {
        $http({
            method: 'POST',
            url: "Processes/EntityProcessTag/GetEntity?plantId=" + $scope.subProcessSetNew.PlantId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    };


    //$scope.GetEntityList = function () {
    //    ClearFields();
    //    cboService.getCboProductionEntityByCompany(null, $scope.subProcessSetNew.CompanyId, function (result) {
    //        $scope.entityList = result;
    //    });
    //};

    cboService.getProductionProcessCbo(function (result) {
        $scope.processList = result;
    });

    $scope.getProcessTypeList = function () {
        $scope.processTypeList = [];
        cboService.getCboProcessTypeByProcess($scope.subProcessSetNew.ProcessId, function (result) {
            $scope.processTypeList = result;
        });
    }

    cboService.getEnumCbo("enum/GetJobWorkTypeListCbo", function (result) {
        $scope.jobWorkTypeList = result;
    });

    // #endregion

    // #region

    $scope.entities = [];
    $scope.getEntityMapData = function () {
        $scope.entities = [];
        $http({
            method: 'GET',
            url: 'Organizations/entity/get?id=' + $scope.subProcessSetNew.EntityId
        }).then(function successCallback(response) {
            if (baseService.arrayLength($scope.entities) === 0) {
                var localValue = [];
                localValue.push(response.data);
                baseService.getDDLSearchColumn(localValue, $scope.entities);
                $scope.entityValue = localValue;
            }
        });
    };

    // #endregion


    $scope.Get = function (id, index) {
        $scope.setTab(1);
        $scope.index = index;
        $scope.subProcessSet = $scope.subProcessSets[$scope.index];
        $scope.subProcessSetNew = Object.assign({}, $scope.subProcessSet);
        $scope.getProcessTypeList();
        $scope.getDetails();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.subProcessSetNew.CompanyId)) {
                return ShowResult('Please at first select company', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.subProcessSetNew.ProcessId)) {
                return ShowResult('Please select Process', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.subProcessSetDetails)) {
                return ShowResult('Please select sub process', 'failure');
            }
            //daysSortValidation($scope.subProcessSetDetails);
            isJobWorkType($scope.subProcessSetDetails);

            var isBaseProcess = false;
            for (var i = 0; i < baseService.arrayLength($scope.subProcessSetDetails); i++) {
                if ($scope.subProcessSetDetails[i].IsBaseProcess) {
                    isBaseProcess = true;
                    break;
                }
                isBaseProcess = false;
            }
            if (!isBaseProcess) throw 'Please select base process';


            $scope.subProcessSetNew.CompanyGroupId = $window.companyGroupId;
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.subProcessSetNewForm.$valid) {
                angular.copy($scope.subProcessSetNew, $scope.subprocess);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'subProcessSet': $scope.subProcessSetNew, 'subProcessSetDetail': $scope.subProcessSetDetails },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getData();
                            ClearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: { 'subProcessSet': $scope.subProcessSetNew, 'subProcessSetDetail': $scope.subProcessSetDetails },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");

                            $scope.getData();
                            ClearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
            else
                $scope.setTab(1);
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.subProcessSetNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.subProcessSetNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.subProcessSets.splice($scope.index, 1);
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
        $scope.Action = "Save";
        $scope.subProcessSet = {};
        $scope.subProcessSetNew = {
            CompanyId: $scope.subProcessSetNew.CompanyId
            , PlantId: $scope.subProcessSetNew.PlantId
            , EntityId: $scope.subProcessSetNew.EntityId
        };
        $scope.processSetDetailTblShow = false;
        $scope.subProcessSetDetails = [];
        $scope.processTypeList = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //#region Sub Process Set

    $scope.processSetDetailTblShow = false;
    $scope.subProcessSetDetails = [];
    $scope.valueData = '';
    $scope.subprocessSetDetailDataList = [];
    $scope.subprocessSetDetailList = [];
    $scope.subprocessSetDetailParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'Code'
        , searchBy: "SubProcessName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.subprocessTitle = 'Sub Process';
    $scope.subprocessSetDetailPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.subProcessSetNew.CompanyId)) {
            return ShowResult('Please at first select company', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.subProcessSetNew.ProcessId)) {
            return ShowResult('Please select Process', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.subProcessSetNew.RequiredTimeUnit)) {
            return ShowResult('Please at first select required time unit', 'failure');
        }

        $scope.subprocessSetDetailUrl = 'Processes/subprocess/GetSubProcessListByProductionProcess?processId=' + $scope.subProcessSetNew.ProcessId;

        $scope.getProcessPopUpData = function (pageno) {
            baseService.paginationBase($scope.subprocessSetDetailUrl, pageno, $scope.subprocessSetDetailParameters)
                .then(function (result) {
                    $scope.subprocessSetDetailDataList = result.Rows;
                    $scope.subprocessSetDetailParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.subprocessSetDetailList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.subprocessSetDetailList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processSetDetailPopUp');
                }).finally(function () {
                });
        };

        angular.element(document.querySelector('#subprocessSetDetailPopUp')).modal('show');
        $scope.getProcessPopUpData();
    };
    function isProcessIdExistInGrid(list) {
        $scope.processIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] === false) {
                    $scope.processIds.push(list[i]['SubProcessId']);
                }
            }
        }
        return JSON.stringify($scope.processIds);
    }

    $scope.selectPSDDoubleClick = function (data) {
        $scope.addProcessSetDetails(data);
        $scope.closePSDPopUp();
    };
    $scope.selectPSDSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.selectPSDByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'subprocessSetDetailPopUp');
        }
        $scope.selectPSDDoubleClick($scope.valueData);
        $scope.closePSDPopUp();
    };
    $scope.closePSDPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#subprocessSetDetailPopUp')).modal('hide');
    };

    $scope.addProcessSetDetails = function (data) {
        $scope.subProcessSetDetails.push({
            Id: $scope.pk(),
            SubProcessSetId: $scope.subProcessSetNew.Id,
            SubProcessId: data.SubProcessId,
            SubProcessName: data.SubProcessName,
            Sequence: $scope.subProcessSetDetails.length + 1,
            IsBaseProcess: false,
            Days: 0,
            Symbol: '+',
            ProductionCycleTime: 1,
            JobWorkApplicable: false,
            JobWorkType: null,
            EntityIdWithinCompany: null,
            EntityIdWithinGroup: null,
            VendorId: null,
            EntityOrVendorName: null,
            setDisable: true,
            class: 'new',
            Archive: false
        });
        if (!$scope.processSetDetailTblShow)
            $scope.processSetDetailTblShow = true;
    };
    $scope.valuePassInDelModal = function (data, index) {
        $scope.message_confirmation = '';
        $scope.gridId = data.Id;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.SubProcessName + ' ]';
        angular.element(document.querySelector('#confirmDelPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.subProcessSetDetails.length; i++) {
            if ($scope.subProcessSetDetails[i].Id === $scope.gridId) {
                $scope.subProcessSetDetails.splice(i, 1);
            }
        }
        $scope.gridId = '';
        if ($scope.subProcessSetDetails.length > 0)
            $scope.processSetDetailTblShow = true;
        else
            $scope.processSetDetailTblShow = false;
    };
    $scope.pk = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };

    $scope.setPlusOrMinus = function (event, index) {
        for (var i = 0; i <= $scope.subProcessSetDetails.length - 1; i++) {
            if (i < index) {
                $scope.subProcessSetDetails[i].Symbol = '-';
                $scope.subProcessSetDetails[i].IsBaseProcess = false;
            }
            else if (i > index) {
                $scope.subProcessSetDetails[i].Symbol = '+';
                $scope.subProcessSetDetails[i].IsBaseProcess = false;
            }
            else if (i === index) {
                $scope.subProcessSetDetails[i].Symbol = null;
                $scope.subProcessSetDetails[i].Days = 0;
                $scope.subProcessSetDetails[i].IsBaseProcess = true;
            }
        }
    };
    function daysSortValidation(list) {
        try {
            var seq = 0;
            var seqNeg = 0;
            var isNeg = true;
            if (list[0].Days === 0) {
                isNeg = false;
            } else {
                seqNeg = parseInt(list[0].Days);
                seqNeg += 1;
            }
            for (var i = 0; i < list.length; i++) {
                if (isNeg === false) {//0,1,2
                    if (list[i].Days >= seq) {
                        seq = list[i].Days;
                    }
                    else//0,1,3,2
                        throw "Lag days sequence is not valid.....!";
                }
                else //2,1,0,1,2 or2,1,0
                {
                    if (list[i].Days <= seqNeg) {//2,1,0
                        seqNeg = list[i].Days;
                        if (list[i].Days === 0) {
                            isNeg = false;
                            seq = 0;
                        }
                    }
                    else {
                        //2,3,1,0,1,2
                        throw "Lag days sequence is not valid.....!";
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }
    function isJobWorkType(list) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].JobWorkApplicable && baseService.isUndefinedOrNull(list[i].JobWorkType)
                    && (baseService.isUndefinedOrNull(list[i].EntityIdWithinCompany)
                        || baseService.isUndefinedOrNull(list[i].EntityIdWithinGroup)
                        || baseService.isUndefinedOrNull(list[i].VendorId))
                ) {
                    throw 'Please select job work type or entity/vendor.......!';
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.clearEntityOrVendor = function (list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id && !list[i].Archive) {
                list[i].EntityIdWithinCompany = null;
                list[i].EntityIdWithinGroup = null;
                list[i].VendorId = null;
                list[i].EntityOrVendorName = null;
                break;
            }
        }
    };
    $scope.SetDisable = function (id) {
        for (var i = 0; i < $scope.subProcessSetDetails.length; i++) {
            if ($scope.subProcessSetDetails[i].Id === id) {
                if ($scope.subProcessSetDetails[i].JobWorkApplicable) {
                    return $scope.subProcessSetDetails[i].setDisable = false;
                }
                else {
                    return $scope.subProcessSetDetails[i].setDisable = true;
                }
            }
        }
    };
    $scope.getDetails = function () {
        $http({
            method: 'GET',
            url: 'Processes/subprocessset/getsubprocesssetdetaillist?subprocessSetId=' + $scope.subProcessSetNew.Id
        }).then(function successCallback(response) {
            $scope.subProcessSetDetails = response.data;
            if ($scope.subProcessSetDetails.length > 0)
                $scope.processSetDetailTblShow = true;
            else
                $scope.processSetDetailTblShow = false;
        });
    };

    var move = function (origin, destination) {
        var temp = $scope.subProcessSetDetails[destination];
        var symbolIndex = null;
        $scope.subProcessSetDetails[destination] = $scope.subProcessSetDetails[origin];
        $scope.subProcessSetDetails[origin] = temp;
        //$scope.subProcessSetDetails[origin].Sequence = destination + 1;
        for (var i = 0; i < $scope.subProcessSetDetails.length; i++) {
            $scope.subProcessSetDetails[i].Sequence = i + 1;
            if ($scope.subProcessSetDetails[i].IsBaseProcess) {
                symbolIndex = i;
            }
        }
        $scope.setPlusOrMinus(null, symbolIndex);
    };
    $scope.moveUp = function (index) {
        move(index, index - 1);
    };
    $scope.moveDown = function (index) {
        move(index, index + 1);
    };
    //#endregion

    //#region

    $scope.valueData = '';

    $scope.popUp = function (id) {
        $scope.popUpList = [];
        $scope.popUpDataList = [];
        $scope.popUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Name'
            , searchBy: "Name"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        if (isJobWorkApplicable($scope.subProcessSetDetails, id))
            return ShowResult('Please select at first job work type..............!', 'failure');
        $scope.popUpUrl = typeCheckAndCreateUrl($scope.subProcessSetDetails, id);
        baseService.setCurrentPage('dataList');
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
        $scope.id = id;
        $scope.getPopUpData();
    };
    $scope.selectDoubleClick = function (data) {
        valueSetInGrid($scope.subProcessSetDetails, data, $scope.id);
        $scope.id = '';
        $scope.closePopUp();
    };
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    function typeCheckAndCreateUrl(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id && !list[i].Archive) {
                if (list[i].JobWorkType === 'EntityWithinCompany')
                    return 'organizations/entity/withincompany?companyId=' + $scope.subProcessSetNew.CompanyId;
                else if (list[i].JobWorkType === 'EntityWithinGroup')
                    return 'organizations/entity/withingroup?companyGroupId=' + $window.companyGroupId + '&companyId=' + $scope.subProcessSetNew.CompanyId;
                else {
                    $scope.popUpParameters.sort = 'PartyName';
                    $scope.popUpParameters.searchBy = 'PartyName';
                    $scope.popUpTitle = 'Vendor';
                    //return 'parties/vendorcompanydata/getpartyfromvendor?companyGroupId=' + $window.companyGroupId + '&companyId=' + $scope.subProcessSetNew.CompanyId;
                    return 'Parties/party/GetCompanyPartyDataList?partyType=vendor'
                }
            }
        }
    }
    function isJobWorkApplicable(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id && !list[i].Archive) {
                if (baseService.isUndefinedOrNull(list[i].JobWorkType))
                    return true;
                else
                    return false;
            }
        }
    }
    function valueSetInGrid(list, data, id) {
        $scope.clearEntityOrVendor(list, id);
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id && !list[i].Archive) {
                if (list[i].JobWorkType === 'EntityWithinCompany') {
                    list[i].EntityIdWithinCompany = data.Id;
                    list[i].EntityOrVendorName = data.Name;
                }
                else if (list[i].JobWorkType === 'EntityWithinGroup') {
                    list[i].EntityIdWithinGroup = data.Id;
                    list[i].EntityOrVendorName = data.Name;
                }
                else {
                    list[i].VendorId = data.Id;
                    list[i].EntityOrVendorName = data.Name;
                }
                break;
            }
        }
    }
    $scope.clearEntityOrVendor = function (list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id && !list[i].Archive) {
                list[i].EntityIdWithinCompany = null;
                list[i].EntityIdWithinGroup = null;
                list[i].VendorId = null;
                list[i].EntityOrVendorName = null;
                break;
            }
        }
    };
    $scope.clearJobType = function (list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                list[i].JobWorkType = null;
                break;
            }
        }
    };
    //#endregion
}