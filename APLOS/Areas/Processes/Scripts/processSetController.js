'use strict';
ProcessSetController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function ProcessSetController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = "Process Set";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.processSets = [];
    $scope.path = 'Processes/processset/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Entity", "Entity");

    $scope.processSet = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        CompanyId: null,
        EntityId: null,
        Entity: null,
        ProcessCategoryId: null,
        ProcessCategory: null,
        ProcessCriteriaId: null,
        ProcessCriteria: null,
        Code: null,
        RequiredTimeUnit: null,
        Description: null,
        PlantId: null
    };
    $scope.processSetNew = Object.assign({}, $scope.processSet);

    $scope.getData = function (pageno) {
        $rootScope.parameters.companyId = $scope.processSetNew.CompanyId;
        $rootScope.parameters.entityId = $scope.processSetNew.EntityId;
        $rootScope.parameters.plantId = $scope.processSetNew.PlantId;

        baseService.pagination(pageno)
            .then(function (result) {
                $scope.processSets = result.Rows;
                ClearFields();
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
            'name': 'Process Category',
            'value': 'ProcessCategory'
        },
        {
            'name': 'Process Criteria',
            'value': 'ProcessCriteria'
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

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.processSetNew.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };


    $scope.entityList = [];
    $scope.getEntity = function () {
        $http({
            method: 'POST',
            url: "Processes/EntityProcessTag/GetEntity?plantId=" + $scope.processSetNew.PlantId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    };

   

    $http({
        method: 'GET',
        url: 'Processes/processcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.processCategoryList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Processes/processcriteria/getcbo'
    }).then(function successCallback(response) {
        $scope.processCriteriaList = response.data;
    });

    cboService.getEnumCbo('enum/getenumrequiredtimeunitcbo', function (result) {
        $scope.requiredTimeUnitList = result;
    });

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
            url: 'Organizations/entity/get?id=' + $scope.processSetNew.EntityId
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
        $rootScope.plantId = $scope.processSetNew.PlantId;
        $scope.processSetNew = $scope.processSets[$scope.index];
        $scope.processSetNew = Object.assign({}, $scope.processSetNew);
        $scope.processSetNew.PlantId = $rootScope.plantId ;
        $scope.getDetails();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.processSetDetails)) {
                return ShowResult('Please at first select process.', 'failure');
            }
            //daysSortValidation($scope.processSetDetails);
            isJobWorkType($scope.processSetDetails);
            isEntityExistInGrid($scope.processSetDetails, $scope.processSetNew.EntityId);
            var isBaseProcess = false;
            for (var i = 0; i < baseService.arrayLength($scope.processSetDetails); i++) {
                if ($scope.processSetDetails[i].IsBaseProcess) {
                    isBaseProcess = true;
                    break;
                }
                isBaseProcess = false;
            }
            if (!isBaseProcess) throw 'Please select base process';
            $scope.processSetNew.CompanyGroupId = $window.companyGroupId;
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.processSetNewForm.$valid) {
                angular.copy($scope.processSetNew, $scope.process);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'processSet': $scope.processSetNew, 'processSetDetail': $scope.processSetDetails },
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
                        data: { 'processSet': $scope.processSetNew, 'processSetDetail': $scope.processSetDetails },
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
            else {
                $scope.setTab(1);
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.processSetNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.processSetNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.processSets.splice($scope.index, 1);
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
        $scope.processSet = {};
        $scope.processSetNew = {
            CompanyId: $scope.processSetNew.CompanyId
            , PlantId: $scope.processSetNew.PlantId
            , EntityId: $scope.processSetNew.EntityId
        };
        $scope.processSetDetailTblShow = false;
        $scope.processSetDetails = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    function isEntityExistInGrid(list, entityId) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].EntityIdWithinCompany === entityId || list[i].EntityIdWithinGroup === entityId) {
                    throw 'You can not take this entity in process set!';
                }
            }
        } catch (e) {
            throw e;
        }
    }

    //#region Process

    $scope.processSetDetailTblShow = false;
    $scope.processSetDetails = [];
    $scope.processSetDetailDataList = [];
    $scope.processSetDetailList = [];
    $scope.valueData = '';
    $scope.processSetDetailParameters = {
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
    $scope.processSetDetailPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.processSetNew.CompanyId))
            return ShowResult('Please at first select entity.', 'failure');
        if (baseService.isUndefinedOrNull($scope.processSetNew.RequiredTimeUnit))
            return ShowResult('Please at first select required time unit.', 'failure');
        $scope.processSetDetailUrl = 'Processes/Process/GetProductionProcessList';
        $scope.getProcessPopUpData = function (pageno) {
            baseService.paginationBase($scope.processSetDetailUrl, pageno, $scope.processSetDetailParameters)
                .then(function (result) {
                    $scope.processSetDetailDataList = result.Rows;
                    $scope.processSetDetailParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.processSetDetailList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.processSetDetailList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processSetDetailPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processSetDetailPopUp')).modal('show');
        $scope.getProcessPopUpData();
    };

    function isProcessIdExistInGrid(list) {
        $scope.processIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] === false) {
                    $scope.processIds.push(list[i]['ProcessId']);
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
            return ShowResult('Please at first select row', 'failure', 'processSetDetailPopUp');
        }
        $scope.selectPSDDoubleClick($scope.valueData);
        $scope.closePSDPopUp();
    };
    $scope.closePSDPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#processSetDetailPopUp')).modal('hide');
    };
    $scope.addProcessSetDetails = function (data) {
        $scope.processSetDetails.push({
            Id: $scope.pk()
            , ProcessSetId: $scope.processSetNew.Id
            , ProcessId: data.Id
            , ProcessName: data.UserName
            , Sequence: $scope.processSetDetails.length + 1
            , IsBaseProcess: false
            , Days: 0
            , Symbol: '+'
            , ProductionCycleTime: 1
            , JobWorkApplicable: false
            , JobWorkType: null
            , EntityOrVendorId: null
            , EntityOrVendorName: null
            , Archive: false
            , setDisable: true
            , class: 'new'
        });
        if (!$scope.processSetDetailTblShow)
            $scope.processSetDetailTblShow = true;
    };
    $scope.valuePassInDelModal = function (data, index) {
        $scope.message_confirmation = '';
        $scope.gridId = data.Id;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.ProcessName + ' ]';
        angular.element(document.querySelector('#confirmDelPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.processSetDetails.length; i++) {
            if ($scope.processSetDetails[i].Id === $scope.gridId) {
                $scope.processSetDetails.splice(i, 1);
            }
        }
        $scope.gridId = '';
        if ($scope.processSetDetails.length > 0)
            $scope.processSetDetailTblShow = true;
        else
            $scope.processSetDetailTblShow = false;
    };
    $scope.pk = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };
    $scope.setPlusOrMinus = function (event, index) {
        for (var i = 0; i <= $scope.processSetDetails.length - 1; i++) {
            if (i < index) {
                $scope.processSetDetails[i].Symbol = '-';
                $scope.processSetDetails[i].IsBaseProcess = false;
            }
            else if (i > index) {
                $scope.processSetDetails[i].Symbol = '+';
                $scope.processSetDetails[i].IsBaseProcess = false;
            }
            else if (i === index) {
                $scope.processSetDetails[i].Symbol = null;
                $scope.processSetDetails[i].Days = 0;
                $scope.processSetDetails[i].IsBaseProcess = true;
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
                if (list[i].JobWorkApplicable && list[i].JobWorkType === null
                    && (list[i].EntityIdWithinCompany === null
                        || list[i].EntityIdWithinGroup === null
                        || list[i].VendorId === null)
                ) {
                    throw 'Please select job work type and entity/vendor!';
                }
                if (!baseService.isUndefinedOrNull(list[i].JobWorkType)) {
                    if (baseService.isUndefinedOrNull(list[i].EntityOrVendorName)) {
                        throw 'Please insert entity/vendor.......!';
                    }
                }
            }
        } catch (e) {
            throw e;
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
    $scope.SetDisable = function (id) {
        for (var i = 0; i < $scope.processSetDetails.length; i++) {
            if ($scope.processSetDetails[i].Id === id) {
                if ($scope.processSetDetails[i].JobWorkApplicable) {
                    return $scope.processSetDetails[i].setDisable = false;
                }
                else {
                    return $scope.processSetDetails[i].setDisable = true;
                }
            }
        }
    };
    $scope.getDetails = function () {
        $http({
            method: 'GET',
            url: 'Processes/processset/getprocesssetdetaillist?processSetId=' + $scope.processSetNew.Id
        }).then(function successCallback(response) {
            $scope.processSetDetails = response.data;
            if ($scope.processSetDetails.length > 0)
                $scope.processSetDetailTblShow = true;
            else
                $scope.processSetDetailTblShow = false;
        });
    };

    var move = function (origin, destination) {
        var temp = $scope.processSetDetails[destination];
        var symbolIndex = null;
        $scope.processSetDetails[destination] = $scope.processSetDetails[origin];
        $scope.processSetDetails[origin] = temp;
        //$scope.processSetDetails[origin].Sequence = destination + 1;
        for (var i = 0; i < $scope.processSetDetails.length; i++) {
            $scope.processSetDetails[i].Sequence = i + 1;
            if ($scope.processSetDetails[i].IsBaseProcess) {
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

    //#region Job Work Type

    $scope.popUpTitle = '';
    $scope.valueData = '';
    $scope.popUp = function (id) {
        $scope.popUpList = [];
        $scope.popUpDataList = [];
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'Name',
            searchBy: "Name",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        if (isJobWorkApplicable($scope.processSetDetails, id))
            return ShowResult('Please select at first job work type!', 'failure');
        $scope.popUpUrl = typeCheckAndCreateUrl($scope.processSetDetails, id);
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
        valueSetInGrid($scope.processSetDetails, data, $scope.id);
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
                if (list[i].JobWorkType === 'EntityWithinCompany') {
                    $scope.popUpTitle = 'Entity within company';
                    return 'Organizations/entity/withincompany?companyId=' + $scope.processSetNew.CompanyId + '&entityId=' + $scope.processSetNew.EntityId;
                }
                else if (list[i].JobWorkType === 'EntityWithinGroup') {
                    $scope.popUpTitle = 'Entity in group';
                    return 'Organizations/entity/withingroup?companyGroupId=' + $window.companyGroupId + '&companyId=' + $scope.processSetNew.CompanyId + '&entityId=' + $scope.processSetNew.EntityId;
                }
                else {
                    $scope.popUpParameters.sort = 'PartyName';
                    $scope.popUpParameters.searchBy = 'PartyName';
                    $scope.popUpTitle = 'Vendor';
                    //return 'Parties/vendorcompanydata/getpartyfromvendor?companyGroupId=' + $window.companyGroupId + '&companyId=' + $scope.processSetNew.CompanyId;
                    //return 'Parties/party/GetCompanyPartyDataList?partyType=vendor'
                    return 'Parties/party/GetCompanyPartyDataListByPlantId?CompanyId=' + $scope.processSetNew.CompanyId + '&PlantId=' + $scope.processSetNew.PlantId + '&partyType=' + 'vendor';
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
                    list[i].PartyId = data.Id;
                    list[i].EntityOrVendorName = data.PartyName;
                }
                break;
            }
        }
    }

    //#endregion Job Work Type
}