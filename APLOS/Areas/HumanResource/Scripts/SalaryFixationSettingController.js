'use strict';
salaryFixationSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function salaryFixationSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'SalaryFixationSetting';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SalaryFixationSettings = [];
    $scope.path = 'HumanResource/salaryfixationsetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveMonthlyUrl = $scope.path + 'CreateDetails';
    $scope.saveAnnualCashUrl = $scope.path + 'CreateAnnualCashDetails';
    $scope.saveNonCashUrl = $scope.path + 'CreateNonCashDetails';
    $scope.saveLeaveTypeUrl = $scope.path + 'CreateLeaveTypeDetails';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'UserName');

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.SalaryFixationSettings = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.SalaryFixationSettingNew.Sequence = data;
        })
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.setTab(1);
        $rootScope.SalFixSetId = null;
        $scope.index = index;
        $scope.SalaryFixationSetting = $scope.SalaryFixationSettings[$scope.index];
        $scope.SalaryFixationSettingNew = Object.assign({}, $scope.SalaryFixationSetting);
        $rootScope.SalFixSetId = $scope.SalaryFixationSettingNew.Id;

        $scope.GetMasterWiseSavedChildData($rootScope.SalFixSetId);
        $scope.GetAnnualCashChildData($rootScope.SalFixSetId);
        $scope.GetNonCashDetailList($rootScope.SalFixSetId);
        $scope.GetLeaveTypeList($rootScope.SalFixSetId);

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SalaryFixationSetting = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Archive: false
    };

    $scope.SalaryFixationSettingNew = Object.assign({}, $scope.SalaryFixationSetting);

    $scope.SaveMaster = function () {
        angular.copy($scope.SalaryFixationSettingNew, $scope.SalaryFixationSetting);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SalaryFixationSettingNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.SalaryFixationSetting,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        console.log('m', response.data);
                        console.log('mob', response.data.SalaryFixationSetting);
                        console.log('mid', response.data.SalaryFixationSetting.Id);
                        $scope.SalaryFixationSettings.push(response.data.SalaryFixationSetting);
                        $rootScope.SalFixSetId = response.data.SalaryFixationSetting.Id;//SalaryFixationSetting
                        $scope.SalaryFixationSettings = $filter('orderBy')($scope.SalaryFixationSettings, 'Sequence');

                        $scope.SalaryFixationSettingNew.Id = response.data.SalaryFixationSetting.Id;
                        //$rootScope.SalFixSetId = $scope.SalaryFixationSettings.Id;
                        baseService.paginationAdd();
                        //ClearFields(response.data.Sequence);

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.SalaryFixationSetting,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        //if ($scope.index > -1) {
                        //    angular.copy($scope.SalaryFixationSetting, $scope.SalaryFixationSettings[$scope.index]);
                        //    $scope.SalaryFixationSettings = $filter('orderBy')($scope.SalaryFixationSetting, 'Sequence');
                        //}
                        //ClearFields(response.data.Sequence);
                        if ($scope.index > -1) {
                            $scope.SalaryFixationSettings[$scope.index] = $scope.SalaryFixationSetting;
                            $scope.SalaryFixationSettings = $filter('orderBy')($scope.SalaryFixationSettings, 'Sequence');
                        }
                        $rootScope.SalFixSetId = response.data.SalaryFixationSetting.Id;
                        //ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.SalaryFixationSettingNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.SalaryFixationSettingNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SalaryFixationSettings.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $rootScope.SalFixSetId = null;
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    // %%%%%%%%%%%% (SF) MONTHLY TABS WORK AREA (STARTS) %%%%%%%

    $scope.searchSalaryHeadList = [
        {
            'name': 'SalaryHead ID',
            'value': 'SalaryHeadID'
        },
        {
            'name': 'Salary Head',
            'value': 'SalaryHead'
        },
        {
            'name': 'Description',
            'value': 'Description'
        },
        {
            'name': 'Head Type',
            'value': 'HeadType'
        },
        {
            'name': 'Head Category',
            'value': 'HeadCategory'
        }];

    $scope.SalaryHeadPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'SalaryHeadID',
        searchBy: "SalaryHeadID",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.dataListSalaryHead = [];
    $scope.getSalaryHeadModalData = function (pageno) {
        baseService.paginationBase('humanresource/salaryfixationsetting/getsalaryheads', pageno, $scope.SalaryHeadPopUpParameters)
            .then(function (result) {
                $scope.dataListSalaryHead = result.Rows;
                for (var i = 0; i < $scope.dataListSalaryHead.length; i++) {
                    if (checkExistingSalaryHeads($scope.dataListSalaryHead[i].SalaryHeadID) === true) {
                        $scope.dataListSalaryHead[i].Flag = true;
                    } else {
                        $scope.dataListSalaryHead[i].Flag = false;
                    }
                }
                for (var i = 0; i < $scope.dataListSalaryHead.length; i++) {
                    if ($scope.dataListSalaryHead[i].Flag === false) {
                        $scope.dataListSalaryHead[i].Flag = getFirstActive($scope.tempPrimaryList, $scope.dataListSalaryHead[i].SalaryHeadID)
                    }
                }
                $scope.SalaryHeadPopUpParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        angular.element(document.querySelector('#PopUpSalaryHead')).modal('show');
    };

    $scope.tempPrimaryList = [];
    $scope.selectFirstCheckedValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistingTempCheckedList($scope.tempPrimaryList, data.SalaryHeadID) === false) {
                    $scope.tempPrimaryList.push(data);
                }
            } else {
                for (var i = 0; i < $scope.tempPrimaryList.length; i++) {
                    if ($scope.tempPrimaryList[i].SalaryHeadID === data.SalaryHeadID) {
                        $scope.tempPrimaryList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, 'failure');
        }
    }

    function checkExistingTempCheckedList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    function getFirstActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalaryHeadID === id) {
                return true;
                ///break
                break;
            }
        }
        return false;
    }

    function checkExistingSalaryHeads(Id) {
        for (var i = 0; i < $scope.selectedsalaryHead.length; i++) {
            if ($scope.selectedsalaryHead[i].SalaryHeadID === Id) {
                return true;
                break;
            }
        }
        return false;
    }

    function checkExisting(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalaryHeadID === id) {
                return true;
            }
        }
        return false;
    }

    $scope.selectedsalaryHead = [];
    $scope.salaryHeadToTable = function () {
        var sq = 1000;
        for (var i = 0; i < $scope.tempPrimaryList.length; i++) {//8
            var ob = $scope.tempPrimaryList[i];

            //check other tab list
            if (CheckTabData(ob.SalaryHeadID, $scope.selectedAnnCashSalaryHead) === false) {//1
                if (ob.Flag) {//5
                    sq++;
                    //check own tab list
                    if (checkExisting($scope.selectedsalaryHead, ob.SalaryHeadID) === false) {//6
                        $scope.selectedsalaryHead.push(
                            {
                                Id: null,
                                SequenceNo: sq,
                                SalFixSetId: $rootScope.SalFixSetId,
                                SalaryHeadID: ob.SalaryHeadID,
                                SalaryHead: ob.SalaryHead,
                                Description: ob.Description,
                                HeadType: ob.HeadType,
                                HeadCategory: ob.HeadCategory,
                                IsMonthly: true
                            }
                        );
                    }//6
                }//5
            }//1
        }//8
        angular.element(document.querySelector('#PopUpSalaryHead')).modal('hide');
    }

    function CheckTabData(clistid, tlist) {
        var r = false;
        if (checkExisting(tlist, clistid)) {
            r = true;
        }
        return r;
    }

    $scope.removeMonthlySalaryHeadRow = function (data, index) {
        $scope.SalaryHeadID = data.SalaryHeadID;
        $scope.Index = index;
        if (baseService.isUndefinedOrNull($scope.SalaryHeadID))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.SalaryHeadID + ' ]';
        angular.element(document.querySelector('#confirmMonthlyPopUp')).modal('show');
    };

    $scope.DeleteMonthlyTabRow = function () {
        var tempData = $scope.selectedsalaryHead;
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].SalaryHeadID === $scope.SalaryHeadID) {
                $scope.selectedsalaryHead.splice(i, 1);
                break;
            }
        }

        var tempData = $scope.tempPrimaryList;
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].SalaryHeadID === $scope.SalaryHeadID) {
                $scope.tempPrimaryList.splice(i, 1);
                break;
            }
        }
        $scope.Id = null;
        $scope.Index = -1;
        tempData = [];
    };

    $scope.SaveSFMonthly = function () {
        if ($scope.selectedsalaryHead.length > 0) {
            $http({
                method: 'POST',
                url: $scope.saveMonthlyUrl,
                data: { 'models': $scope.selectedsalaryHead, 'salFixSetId': $rootScope.SalFixSetId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SalaryHeadListTosave.push(response.data.selectedsalaryHead);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    $scope.removeSalaryHeadRow = function (data, index) {
        $scope.SalaryHeadID = data.SalaryHeadID;
        $scope.Index = index;
        if (baseService.isUndefinedOrNull($scope.SalaryHeadID))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.SalaryHead + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    // %%%%%%%%%%%%%% (SF) MONTHLY TABS WORK AREA E-N-D-S %%%%%%%%

    $scope.AnnCashSearchByList = [
        {
            'name': 'SalaryHead ID',
            'value': 'SalaryHeadID'
        },
        {
            'name': 'Salary Head',
            'value': 'SalaryHead'
        },
        {
            'name': 'Description',
            'value': 'Description'
        },
        {
            'name': 'Head Type',
            'value': 'HeadType'
        },
        {
            'name': 'Head Category',
            'value': 'HeadCategory'
        }];

    $scope.AnnCashPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'SalaryHeadID',
        searchBy: "SalaryHeadID",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.dataListAnnCashSalaryHead = [];
    $scope.getAnnCashSalaryHeadModalData = function (pageno) {
        baseService.paginationBase('humanresource/salaryfixationsetting/getsalaryheadsancash', pageno, $scope.AnnCashPopUpParameters)
            .then(function (result) {
                $scope.dataListAnnCashSalaryHead = result.Rows;
                for (var i = 0; i < $scope.dataListAnnCashSalaryHead.length; i++) {
                    if (checkAnnCashExistingSalaryHeads($scope.dataListAnnCashSalaryHead[i].SalaryHeadID) === true) {
                        $scope.dataListAnnCashSalaryHead[i].Flag = true;
                    } else {
                        $scope.dataListAnnCashSalaryHead[i].Flag = false;
                    }
                }
                for (var i = 0; i < $scope.dataListAnnCashSalaryHead.length; i++) {
                    if ($scope.dataListAnnCashSalaryHead[i].Flag === false) {
                        $scope.dataListAnnCashSalaryHead[i].Flag = getAnnCashFirstActive($scope.annCashTempPrimaryList, $scope.dataListAnnCashSalaryHead[i].SalaryHeadID)
                    }
                }
                $scope.AnnCashPopUpParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        angular.element(document.querySelector('#PopUpAnnualCashSalaryHead')).modal('show');
    };

    $scope.selectedAnnCashSalaryHead = [];
    $scope.annCashSalaryHeadToTable = function () {
        var sq = 2000;
        for (var i = 0; i < $scope.annCashTempPrimaryList.length; i++) {
            var ob = $scope.annCashTempPrimaryList[i];

            if (CheckTabData(ob.SalaryHeadID, $scope.selectedsalaryHead) === false) {
                if (ob.Flag) {
                    sq++;
                    if (checkAnnCashExisting($scope.selectedAnnCashSalaryHead, ob.SalaryHeadID) === false) {
                        $scope.selectedAnnCashSalaryHead.push(
                            {
                                Id: null,
                                SequenceNo: sq,
                                SalFixSetId: $rootScope.SalFixSetId,
                                SalaryHeadID: ob.SalaryHeadID,
                                SalaryHead: ob.SalaryHead,
                                Description: ob.Description,
                                HeadType: ob.HeadType,
                                HeadCategory: ob.HeadCategory,
                                IsAnnualCash: true
                            }
                        );
                    }
                }
            }
        }
        angular.element(document.querySelector('#PopUpAnnualCashSalaryHead')).modal('hide');
    }

    $scope.annCashTempPrimaryList = [];
    $scope.selectAnnCashFirstCheckedValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkAnnCashExisting($scope.annCashTempPrimaryList, data.SalaryHeadID) === false) {
                    $scope.annCashTempPrimaryList.push(data);
                }
            } else {
                for (var i = 0; i < $scope.annCashTempPrimaryList.length; i++) {
                    if ($scope.annCashTempPrimaryList[i].SalaryHeadID === data.SalaryHeadID) {
                        $scope.annCashTempPrimaryList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, 'failure');
        }
    }

    function getAnnCashFirstActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalaryHeadID === id) {
                return true;
                break;
            }
        }
        return false;
    }

    function checkAnnCashExistingSalaryHeads(Id) {
        for (var i = 0; i < $scope.selectedAnnCashSalaryHead.length; i++) {
            if ($scope.selectedAnnCashSalaryHead[i].SalaryHeadID === Id) {
                return true;
                break;
            }
        }
        return false;
    }

    function checkAnnCashExisting(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalaryHeadID === id) {
                return true;
            }
        }
        return false;
    }

    $scope.removeAnnualCashSalaryHeadRow = function (data, index) {
        $scope.SalaryHeadID = data.SalaryHeadID;
        $scope.Index = index;
        if (baseService.isUndefinedOrNull($scope.SalaryHeadID))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.SalaryHeadID + ' ]';
        angular.element(document.querySelector('#confirmAnnualCashPopUp')).modal('show');
    };

    $scope.DeleteAnnualCashTabRow = function () {
        var tempData = $scope.selectedAnnCashSalaryHead;//dataListAnnCashSalaryHead
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].SalaryHeadID === $scope.SalaryHeadID) {
                $scope.selectedAnnCashSalaryHead.splice(i, 1);
                break;
            }
        }

        var tempData = $scope.annCashTempPrimaryList;
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].SalaryHeadID === $scope.SalaryHeadID) {
                $scope.annCashTempPrimaryList.splice(i, 1);
                break;
            }
        }

        $scope.Id = null;
        $scope.Index = -1;
        tempData = [];
    };

    $scope.SaveSFAnnualCash = function () {
        if ($scope.selectedAnnualCashsalaryHead.length > 0) {
            $http({
                method: 'POST',
                url: $scope.saveAnnualCashUrl,
                data: { 'models': $scope.selectedAnnualCashsalaryHead, 'salFixSetId': $rootScope.SalFixSetId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SalaryHeadListTosave.push(response.data.selectedAnnualCashsalaryHead);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    // %%%% (SF) ANNUAL TABS WORK AREA ENDS %%%%%%%%%%%

    // !%%%%%%!! Annual Non Cash Work STARTS........%%%%%%%...
    $scope.searchNonCashList = [
        {
            'name': 'Id',
            'value': 'Id'
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
            'name': 'Description',
            'value': 'Description'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }];

    $scope.ANCPopUpParameters = {
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

    $scope.dataListAnnualNonCash = [];
    $scope.getAnnualNonCashModalData = function (pageno) {
        baseService.paginationBase('humanresource/salaryfixationsetting/getannualnoncash', pageno, $scope.ANCPopUpParameters)
            .then(function (result) {
                $scope.dataListAnnualNonCash = result.Rows;
                for (var i = 0; i < $scope.dataListAnnualNonCash.length; i++) {
                    if (checkExistingNonCashHeads($scope.dataListAnnualNonCash[i].AnnualNonCashId) === true) {
                        $scope.dataListAnnualNonCash[i].Flag = true;
                    } else {
                        $scope.dataListAnnualNonCash[i].Flag = false;
                    }
                }
                for (var i = 0; i < $scope.dataListAnnualNonCash.length; i++) {
                    if ($scope.dataListAnnualNonCash[i].Flag === false) {
                        $scope.dataListAnnualNonCash[i].Flag = getFirstActiveNonCash($scope.tempSelectedNonCashList, $scope.dataListAnnualNonCash[i].AnnualNonCashId)
                    }
                }
                $scope.ANCPopUpParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        angular.element(document.querySelector('#PopUpAnnualNonCash')).modal('show');
    };

    function checkExistingNonCashHeads(Id) {
        for (var i = 0; i < $scope.selectedAnnualNonCash.length; i++) {
            if ($scope.selectedAnnualNonCash[i].Id === Id) {
                return true;
                break;
            }
        }
        return false;
    }

    $scope.selectedAnnualNonCash = [];
    $scope.annualNonCashToTable = function () {
        var sq = 3000;
        for (var i = 0; i < $scope.dataListAnnualNonCash.length; i++) {
            var ob = $scope.dataListAnnualNonCash[i];

            if (ob.Flag) {
                if (chkExTmpCheckedNonCashList($scope.selectedAnnualNonCash, ob.AnnualNonCashId) === false) {
                    sq++;
                    $scope.selectedAnnualNonCash.push(
                        {
                            Id: null,
                            SequenceNo: sq,
                            AnnualNonCashId: ob.AnnualNonCashId,
                            Code: ob.Code,
                            ShortName: ob.ShortName,
                            UserName: ob.UserName,
                            Description: ob.Description,
                            Active: ob.Active,
                            SalFixSetId: $rootScope.SalFixSetId,
                            IsAnnualNonCash: true
                        }
                    );
                }
            }
        }
        angular.element(document.querySelector('#PopUpAnnualNonCash')).modal('hide');
    }

    $scope.removeNonCashRow = function (data, index) {
        $scope.Id = data.Id;
        $scope.Index = index;
        if (baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.Id + ' ]';
        angular.element(document.querySelector('#confirmNonCashPopUp')).modal('show');
    };

    $scope.DeleteNonCashTabRow = function () {//DeleteNonCashTabRow
        var tempDataNonCash = $scope.selectedAnnualNonCash;//selectedAnnualNonCash
        //console.log('hasan', tempDataNonCash);
        for (var i = 0; i < tempDataNonCash.length; i++) {
            if (tempDataNonCash[i].Id === $scope.Id) {
                $scope.selectedAnnualNonCash.splice(i, 1);
            }
        }

        var tempDataNonCash = $scope.tempSelectedNonCashList;
        for (var i = 0; i < tempDataNonCash.length; i++) {
            if (tempDataNonCash[i].Id === $scope.Id) {
                $scope.tempSelectedNonCashList.splice(i, 1);
                break;
            }
        }

        $scope.Id = null;
        $scope.Index = -1;
        tempDataNonCash = [];
    };

    $scope.tempSelectedNonCashList = [];
    $scope.selectFirstCheckedNonCashValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (chkExTmpCheckedNonCashList($scope.tempSelectedNonCashList, data.AnnualNonCashId) === false) {
                    $scope.tempSelectedNonCashList.push(data);
                }
            } else {
                for (var i = 0; i < $scope.tempSelectedNonCashList.length; i++) {
                    if ($scope.tempSelectedNonCashList[i].AnnualNonCashId === data.AnnualNonCashId) {
                        $scope.tempSelectedNonCashList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, 'failure');
        }
    }

    function chkExTmpCheckedNonCashList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    function getFirstActiveNonCash(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].AnnualNonCashId === id) {
                return true;
                break;
            }
        }
        return false;
    }

    function checkExistingNonCash(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveSFAnnualNonCash = function () {
        if ($scope.selectedAnnualNonCash.length > 0) {
            $http({
                method: 'POST',
                url: $scope.saveNonCashUrl,
                data: { 'models': $scope.selectedAnnualNonCash, 'salFixSetId': $rootScope.SalFixSetId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SalaryHeadListTosave.push(response.data.selectedAnnualNonCash)
                    $scope.GetNonCashDetailList(response.data.SalFixSetId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    // !!! Annual Non Cash Work ENDS...........

    $scope.searchLeaveTypeList = [
        {
            'name': 'LeaveTypeId',
            'value': 'LeaveTypeId'
        },
        {
            'name': 'Leave Type',
            'value': 'LeaveType'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }];

    $scope.LeaveTypePopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'LeaveTypeId',
        searchBy: 'LeaveTypeId',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getLeaveTypeModalData = function (pageno) {
        baseService.paginationBase('humanresource/salaryfixationsetting/getleavetypes', pageno, $scope.LeaveTypePopUpParameters)
            .then(function (result) {
                $scope.dataListLeaveType = result.Rows;

                $scope.dataListSalaryHead = result.Rows;
                for (var i = 0; i < $scope.dataListLeaveType.length; i++) {
                    if (checkExistLeave($scope.dataListLeaveType[i].LeaveTypeId) === true) {
                        $scope.dataListLeaveType[i].Flag = true;
                    } else {
                        $scope.dataListLeaveType[i].Flag = false;
                    }
                }

                $scope.LeaveTypePopUpParameters.total_count = result.Total;
                for (var i = 0; i < $scope.dataListLeaveType.length; i++) {
                    $scope.dataListLeaveType[i].Active = getActive($scope.tempList, $scope.dataListLeaveType[i].Id);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        angular.element(document.querySelector('#PopUpLeaveType')).modal('show');
    };

    function checkExistLeave(Id) {
        for (var i = 0; i < $scope.selectedLeaveTypes.length; i++) {
            if ($scope.selectedLeaveTypes[i].LeaveTypeId === Id) {
                return true;
                break;
            }
        }
        return false;
    }

    $scope.selectedLeaveTypes = [];
    $scope.leaveTypesToTable = function () {
        var sq = 4000;
        for (var i = 0; i < $scope.dataListLeaveType.length; i++) {
            var ob = $scope.dataListLeaveType[i];
            if (ob.Flag) {
                if (checkExistTempList($scope.selectedLeaveTypes, ob.LeaveTypeId) === false) {
                    sq++;
                    $scope.selectedLeaveTypes.push(
                        {
                            Id: null,
                            SequenceNo: sq,
                            LeaveTypeId: ob.LeaveTypeId,
                            LeaveType: ob.LeaveType,
                            Code: ob.Code,
                            UserName: ob.UserName,
                            Description: ob.Description,
                            SalFixSetId: $rootScope.SalFixSetId,
                            IsLeave: true
                        }
                    );
                }
            }
        }
        angular.element(document.querySelector('#PopUpLeaveType')).modal('hide');
    }

    $scope.removeLeaveTypeRow = function (data, index) {
        $scope.LeaveTypeId = data.LeaveTypeId;
        $scope.Index = index;
        if (baseService.isUndefinedOrNull($scope.LeaveTypeId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.LeaveTypeId + ' ]';
        angular.element(document.querySelector('#confirmLeavePopUp')).modal('show');
    };

    $scope.spliceLeaveTypeRow = function () {
        var tempDataLvType = $scope.selectedLeaveTypes;
        for (var i = 0; i < tempDataLvType.length; i++) {
            if (tempDataLvType[i].LeaveTypeId === $scope.LeaveTypeId) {
                $scope.selectedLeaveTypes.splice(i, 1);
            }
        }

        var tempDataLvType = $scope.tempList;
        for (var i = 0; i < tempDataLvType.length; i++) {
            if (tempDataLvType[i].SalaryHeadID === $scope.SalaryHeadID) {
                $scope.tempList.splice(i, 1);
                break;
            }
        }

        $scope.Id = null;
        $scope.Index = -1;
        tempDataLvType = [];
    };
    // ***** Save LeaveType with CHECKED items ****** /// START
    $scope.tempList = [];
    $scope.selectCheckedLeave = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.LeaveTypeId) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].LeaveTypeId === data.LeaveTypeId) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    function checkExistTempList(list, LeaveTypeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].LeaveTypeId === LeaveTypeId) {
                return true;
            }
        }
        return false;
    }

    function getActive(list, LeaveTypeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].LeaveTypeId === LeaveTypeId) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveLeaveType = function () {
        $http({
            method: 'POST',
            url: $scope.saveLeaveTypeUrl,
            data: { 'models': $scope.selectedLeaveTypes, 'salFixSetId': $rootScope.SalFixSetId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LeaveTypeListTosave.push(response.data.selectedLeaveType);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

    $scope.GetMasterWiseSavedChildData = function (id) {
        $http.get('humanresource/salaryfixationsetting/getchilddatamasterwise?salFixSetId=' + id)
            .then(function (response) {
                $scope.selectedsalaryHead = response.data;
            })
    }

    $scope.GetAnnualCashChildData = function (id) {
        $http.get('humanresource/salaryfixationsetting/getannualcashchilddatamasterwise?salFixSetId=' + id)
            .then(function (response) {
                $scope.selectedAnnCashSalaryHead = response.data;
            })
    }

    $scope.GetNonCashDetailList = function (id) {
        $http.get('humanresource/salaryfixationsetting/getnoncashdetaillist?salFixSetId=' + id)
            .then(function (response) {
                $scope.selectedAnnualNonCash = response.data;
            })
    }

    $scope.GetLeaveTypeList = function (id) {
        $http.get('humanresource/salaryfixationsetting/getsavedleavetypes?salFixSetId=' + id)
            .then(function (response) {
                $scope.selectedLeaveTypes = response.data;
            })
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.SalaryFixationSetting = {};
        $scope.SalaryFixationSettingNew = {};
        $scope.selectedsalaryHead = {};
        $scope.selectedAnnualCashsalaryHead = {};
        $scope.dataListLeaveType = {};
        $scope.SalaryFixationSettingNew.Sequence = seq;
        $scope.SalaryFixationSettingNew.Active = true;

        //all tab list
        $scope.selectedsalaryHead = [];
        $scope.selectedAnnCashSalaryHead = [];
        $scope.selectedAnnualNonCash = [];
        $scope.selectedLeaveTypes = [];
        $rootScope.SalFixSetId = null;
        //all search temp list
        $scope.annCashTempPrimaryList = [];//cash
        $scope.tempPrimaryList = [];//monthly
        $scope.tempSelectedNonCashList = [];//noncash
        $scope.tempList = [];//leave
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    var move = function (origin, destination, list) {
        var temp = $scope[list][destination];
        $scope[list][destination] = $scope[list][origin];
        $scope[list][origin] = temp;
    };
    $scope.moveUp = function (index, list) {
        move(index, index - 1, list);
    };
    $scope.moveDown = function (index, list) {
        move(index, index + 1, list);
    };

    $scope.SaveSalaryHead = function () {
        $http({
            method: 'POST',
            url: 'humanresource/salaryfixationsetting/createsalaryheaddetails',
            data: {
                'monthlyList': $scope.selectedsalaryHead,
                'annualCashList': $scope.selectedAnnCashSalaryHead,
                'annualCashNonList': $scope.selectedAnnualNonCash,
                'leaveTypeList': $scope.selectedLeaveTypes,
                'salFixSetId': $rootScope.SalFixSetId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetMasterWiseSavedChildData(response.data.SalaryFixationSetting[0].SalFixSetId);
                $scope.GetAnnualCashChildData(response.data.SalaryFixationSetting[0].SalFixSetId);
                $scope.GetNonCashDetailList(response.data.SalaryFixationSetting[0].SalFixSetId);
                $scope.GetLeaveTypeList(response.data.SalaryFixationSetting[0].SalFixSetId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }


}