'use strict';
function RecruitmentProcessSetController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Recruitment Process Set';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.recruitmentProcessSets = [];
    $scope.path = 'employees/recruitmentprocessset/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'Code', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.recruitmentProcessSets = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList = [

        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];

    $scope.recruitmentProcessSet = {
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
    $scope.recruitmentProcessSetNew = Object.assign({}, $scope.recruitmentProcessSet);

    $scope.recruitmentProcessSetDetail = {
        Id: null,
        RecruitmentProcessSetId: null,
        Sequence: null,
        RecruitmentProcessId: null,
        RecruitmentProcessName: null,
        RequiredDays: null,
        Active: true
    };
    $scope.recruitmentProcessSetDetailNew = Object.assign({}, $scope.recruitmentProcessSetDetail);

    $scope.recruitmentProcessList = [];
    cboService.getCboRecruitmentProcess(function (data) {
        $scope.recruitmentProcessList = data;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.recruitmentProcessSet = $scope.recruitmentProcessSets[$scope.index];
        $scope.recruitmentProcessSetNew = Object.assign({}, $scope.recruitmentProcessSet);
        $scope.recruitmentProcessSetDetailData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.recruitmentProcessSetForm.$valid) {
            $scope.recruitmentProcessSet = Object.assign({}, $scope.recruitmentProcessSetNew);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'employees/recruitmentprocessset/create',
                    data: {
                        'recruitmentProcessSet': $scope.recruitmentProcessSet
                        , 'recruitmentProcessSetDetails': $scope.recruitmentProcessSetDetailDataList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.recruitmentProcessSets.push(response.data.RecruitmentProcessSet);
                        //$scope.recruitmentProcessSets = $filter('orderBy')($scope.recruitmentProcessSets, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'employees/recruitmentprocessset/edit',
                    data: {
                        'recruitmentProcessSet': $scope.recruitmentProcessSet
                        , 'recruitmentProcessSetDetails': $scope.recruitmentProcessSetDetailDataList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.recruitmentProcessSets[$scope.index] = $scope.recruitmentProcessSet;
                            //$scope.recruitmentProcessSets = $filter('orderBy')($scope.recruitmentProcessSets, 'Sequence');
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.recruitmentProcessSetNew.Id)) {
            $http({
                method: 'POST',
                url: 'employees/recruitmentprocessset/delete/' + $scope.recruitmentProcessSetNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.recruitmentProcessSets.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message);
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    //Local recruitmentProcessSetDetail Part Start
    $scope._recruitmentProcessSetDetailIndex = -1;
    $scope.recruitmentProcessSetDetailTbl = false;
    $scope.recruitmentProcessSetDetailCaption = 'Add Row';
    $scope.recruitmentProcessSetDetailDataList = [];
    $scope.AddMultiplerecruitmentProcessSetDetail = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.recruitmentProcessSetDetailNew.RecruitmentProcessId)) {
                throw 'Please select your Recruitment Process.';
            }
            if (baseService.isUndefinedOrNull($scope.recruitmentProcessSetDetailNew.RequiredDays) ||
                $scope.recruitmentProcessSetDetailNew.RequiredDays === 0) {
                throw 'Please insert required days.';
            }
            var isAvailable = false;
            isSeqValid($scope.recruitmentProcessSetDetailDataList, $scope.recruitmentProcessSetDetailNew.Sequence, $scope._recruitmentProcessSetDetailIndex);
            var lng = document.getElementById("recruitmentProcessId").options[document.getElementById('recruitmentProcessId').selectedIndex].text;
            for (var i = 0; i < $scope.recruitmentProcessSetDetailDataList.length; i++) {
                isAvailable = listValidation($scope.recruitmentProcessSetDetailDataList[i].RecruitmentProcessId
                    , $scope.recruitmentProcessSetDetailNew.RecruitmentProcessId, i);
                if (isAvailable) {
                    throw 'This recruitmentProcessSetDetail : [' + lng + '] has been already taken';
                }
            }
            $scope.recruitmentProcessSetDetail = Object.assign({}, $scope.recruitmentProcessSetDetailNew);
            if ($scope._recruitmentProcessSetDetailIndex === -1) {
                checkSequence($scope.recruitmentProcessSetDetailDataList, parseInt($scope.recruitmentProcessSetDetailNew.Sequence));
                $scope.recruitmentProcessSetDetailDataList.push({
                    Id: null
                    , RecruitmentProcessSetId: $scope.recruitmentProcessSet.Id
                    , RecruitmentProcessId: $scope.recruitmentProcessSetDetail.RecruitmentProcessId
                    , RecruitmentProcessName: lng
                    , Sequence: $scope.recruitmentProcessSetDetail.Sequence
                    , RequiredDays: $scope.recruitmentProcessSetDetail.RequiredDays
                    , Active: $scope.recruitmentProcessSetDetail.Active
                });
            }
            else {
                $scope.recruitmentProcessSetDetail.RecruitmentProcessName = lng;
                $scope.recruitmentProcessSetDetailDataList[$scope._recruitmentProcessSetDetailIndex] = $scope.recruitmentProcessSetDetail;
            }
            if (!$scope.recruitmentProcessSetDetailTbl) {
                $scope.recruitmentProcessSetDetailTbl = true;
            }
            $scope.recruitmentProcessSetDetail = { Active: true };
            $scope.recruitmentProcessSetDetailNew = { Active: true };
            $scope._recruitmentProcessSetDetailIndex = -1;
            $scope.recruitmentProcessSetDetailCaption = 'Add Row';
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    function listValidation(oldValue, newValue, index) {
        var isAvailable = false;
        // recruitmentProcessSetDetailId
        if ($scope._recruitmentProcessSetDetailIndex == -1) {
            if (oldValue == newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if ($scope._recruitmentProcessSetDetailIndex !== index) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        return isAvailable;
    }
    function isSeqValid(list, newSeq, index) {
        try {
            if (index === -1) {
                for (var i = 0; i < list.length; i++) {
                    var seq = list[i].Sequence;
                    if (list[i].Sequence == newSeq) {
                        throw 'Duplicate Sequence [' + newSeq + '] found in grid';
                    }
                }
            }
            else {
                for (var i = 0; i < list.length; i++) {
                    var seq = list[i].Sequence;
                    if (list[i].Sequence == newSeq && i !== index) {
                        throw 'Duplicate Sequence [' + newSeq + '] found in grid';
                    }
                }
            }

        } catch (e) {
            throw e;
        }
    }
    function checkSequence(list, newSeq) {
        try {
            if (list.length !== 0) {
                if (parseInt(list[list.length - 1].Sequence) + 1 !== newSeq) {
                    throw 'Please input sequence in sequentially. EX: 1,2,3..';
                }
            }
            else {
                if (1 !== newSeq) {
                    throw 'Please input sequence 1!';
                }
            }

        } catch (e) {
            throw e;
        }
    }

    $scope.recruitmentProcessSetDetailDataParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'recruitmentProcessSetDetailName',
        searchBy: null,
        pageSize: 10,
        total_count: 0,
        search: 'recruitmentProcessSetDetailName',
        serverPagination: true
    };
    $scope.recruitmentProcessSetDetailData = function () {
        $scope.recruitmentProcessSetDetailDataUrl = 'employees/recruitmentProcessSet/getrecruitmentprocesssetdetaillist?recruitmentProcessSetId=' + $scope.recruitmentProcessSetNew.Id;
        $scope.getRecruitmentProcessSetDetailData = function (pageno) {
            baseService.paginationBase($scope.recruitmentProcessSetDetailDataUrl, pageno, $scope.recruitmentProcessSetDetailDataParameters)
                .then(function (result) {
                    $scope.recruitmentProcessSetDetailDataList = result;
                    $scope.recruitmentProcessSetDetailDataParameters.total_count = result.Total;
                    if ($scope.recruitmentProcessSetDetailDataList.length > 0) {
                        $scope.recruitmentProcessSetDetailTbl = true;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'recruitmentProcessSetDetailDataId');
                }).finally(function () {
                });
        };
        $scope.getRecruitmentProcessSetDetailData();
    }
    $scope.recruitmentProcessSetDetailEdit = function (data, index) {
        $scope.recruitmentProcessSetDetail = $scope.recruitmentProcessSetDetailDataList[index];
        $scope.recruitmentProcessSetDetailNew = Object.assign({}, $scope.recruitmentProcessSetDetail);
        $scope._recruitmentProcessSetDetailIndex = index;
        $scope.recruitmentProcessSetDetailCaption = 'Update Row';
    }

    $scope.recruitmentProcessSetDetailDelete = function (data, index) {
        $scope.message_confirmation = '';
        $scope._recruitmentProcessSetDetailIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.RecruitmentProcessName + ' ]?';
        angular.element(document.querySelector('#confirmlngPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        $scope.recruitmentProcessSetDetailDataList.splice($scope._recruitmentProcessSetDetailIndex, 1);
        if ($scope.recruitmentProcessSetDetailDataList.length > 0)
            $scope.recruitmentProcessSetDetailTbl = true;
        else
            $scope.recruitmentProcessSetDetailTbl = false;
        $scope._recruitmentProcessSetDetailIndex = -1;
    };

    function clearRecruitmentProcessSetDetail() {
        $scope.recruitmentProcessSetDetail = {};
        $scope.recruitmentProcessSetDetailNew = {};
        $scope.recruitmentProcessSetDetailNew.Active = true;
        $scope._recruitmentProcessSetDetailIndex = -1;
        $scope.recruitmentProcessSetDetailCaption = 'Add Row';
    }
    //Local recruitmentProcessSetDetail Part End

    $scope.Clear = function () {
        ClearFields();
        clearRecruitmentProcessSetDetail();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.recruitmentProcessSet = {};
        $scope.recruitmentProcessSetNew = {};
        //$scope.recruitmentProcessSetNew.Sequence = seq;
        clearRecruitmentProcessSetDetail();
        $scope.recruitmentProcessSetNew.Active = true;
        $scope.recruitmentProcessSetDetailDataList = [];
        $scope.recruitmentProcessSetDetailTbl = false;
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}
RecruitmentProcessSetController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
