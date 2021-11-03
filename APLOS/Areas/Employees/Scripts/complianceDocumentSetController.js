'use strict';
ComplianceDocumentSetController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ComplianceDocumentSetController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Compliance Document Category';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.complianceDocumentSets = [];
    $scope.path = 'employees/complianceDocumentSet/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    // #region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion


    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.complianceDocumentSets = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();



    $scope.complianceDocumentSet = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.complianceDocumentSetNew = Object.assign({}, $scope.complianceDocumentSet);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.complianceDocumentSetNew.Sequence = response.data;
            });
    };

    $scope.GetSequence();
    //*********************** ComplianceDocumentSet PopUp Start *************************************
    $scope.tempFirstList = [];
    $scope.selectFirstChValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempFirstList($scope.tempFirstList, data.Id) === false) {
                    $scope.tempFirstList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempFirstList.length; i++) {
                    if ($scope.tempFirstList[i].Id === data.Id) {
                        $scope.tempFirstList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, 'failure');
        }
    }
    function checkExistTempFirstList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    function getFirstActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }
    $scope.searchByComplianceDocumentList = [
        {
            'name': 'Document Name',
            'value': 'UserName'
        },
        {
            'name': 'Document Category',
            'value': 'ComplianceDocumentCategoryName'
        },
        {
            'name': 'Document SubCategory',
            'value': 'ComplianceDocumentSubCategoryName'
        },
        {
            'name': 'Document Type',
            'value': 'DocumentType'
        },
        {
            'name': 'Importance',
            'value': 'Importance'
        },
        {
            'name': 'EmploymentStage',
            'value': 'EmploymentStage'
        }
    ];
    $scope.complianceDocumentListParameters = {
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
    $scope.complianceDocuments = [];
    $scope.complianceDocumentPopUp = function () {
        $scope.tempFirstList = [];
        baseService.setCurrentPage('complianceDocuments');
        $scope.getComplianceDocumentData = function (pageno) {
            //baseService.paginationBase('employees/ComplianceDocument/GetList', pageno, $scope.complianceDocumentListParameters)
            baseService.paginationBase('employees/compliancedocumentset/getcompliancedocumentlist', pageno, $scope.complianceDocumentListParameters)
                .then(function (data) {
                    $scope.complianceDocuments = data.Rows;
                    $scope.complianceDocumentListParameters.total_count = data.Total;
                    for (var i = 0; i < $scope.complianceDocuments.length; i++) {
                        $scope.complianceDocuments[i].Flag = getFirstActive($scope.tempFirstList, $scope.complianceDocuments[i].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#complianceDocumentPopUp')).modal('show');
        $scope.getComplianceDocumentData();
    };

    $scope.selectComplianceDocument = function () {
        angular.forEach($scope.tempFirstList, function (item) {
            $scope.addComplianceDocumentSetForSave(item);
        })
        angular.element(document.querySelector('#complianceDocumentPopUp')).modal('hide');
    };
    $scope.complianceDocumentSetDetailList = [];
    $scope.addComplianceDocumentSetForSave = function (data) {
        if (checkComplianceDocumentExist($scope.complianceDocumentSetDetailList, data.Id) === false) {
            data.ComplianceDocumentId = data.Id;
            data.Id = null;
            $scope.complianceDocumentSetDetailList.push(data);
        }
    }
    function checkComplianceDocumentExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ComplianceDocumentId === id) {
                return true;
            }
        }
        return false;
    }
    //function checkPositoinExist(list,id) {
    //    angular.forEach(list, function (item) {
    //        if (item.ComplianceDocumentSetId === id) {
    //            return true;
    //        }
    //    });
    //    return false;
    //}
    $scope.clearComplianceDocumentSet = function () {
        $scope.selectedComplianceDocumentSetId = null;
        $scope.complianceDocumentPositonCode.ComplianceDocumentSetId = null;
        $scope.complianceDocumentPositonCode.ComplianceDocumentSetName = null;
        $scope.complianceDocumentSetData = [];
        $scope.complianceDocumentSetSearch = [];
    };
    //removing Row
    $scope.documentComplianceDocumentSetDetailId = null;
    $scope.documentComplianceDocumentSetDetailIndex = -1;
    $scope.valuePassInDelModal = function (data, index) {
        $scope.documentComplianceDocumentSetDetailId = data.Id;
        $scope.documentComplianceDocumentSetDetailIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.UserName + ' ]?';
        angular.element(document.querySelector('#deleteRow')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.complianceDocumentSetDetailList.length; i++) {
            if ($scope.documentComplianceDocumentSetDetailId !== null) {
                if ($scope.complianceDocumentSetDetailList[i].Id == $scope.documentComplianceDocumentSetDetailId) {
                    $scope.removeFromDb($scope.documentComplianceDocumentSetDetailId, i);
                    break;
                }
            } else {
                $scope.complianceDocumentSetDetailList.splice($scope.documentComplianceDocumentSetDetailIndex, 1);
                $scope.documentComplianceDocumentSetDetailIndex = -1;
                break;
            }

        }
        $scope.documentComplianceDocumentSetDetailId = null;
        $scope.documentComplianceDocumentSetDetailIndex = -1;
    };
    $scope.removeFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + '/DeleteDocumentSetDetail',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.complianceDocumentSetDetailList.splice(index, 1);
                    $scope.documentComplianceDocumentSetDetailIndex = -1;
                    //angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    //-
    function getDocumentSetDetailList() {
        $http.get('employees/ComplianceDocumentSet/GetDocumentSetDetailList?complianceDocumentSetId=' + $scope.complianceDocumentSetNew.Id)
            .then(
            function successCallback(response) {
                $scope.complianceDocumentSetDetailList = response.data;
            });
    }

    $scope.Get = function (id, index) {
        $scope.tempList = [];
        $scope.index = index;
        $scope.complianceDocumentSet = $scope.complianceDocumentSets[$scope.index];
        //$scope.complianceDocumentSetId =
        $scope.complianceDocumentSetNew = Object.assign({}, $scope.complianceDocumentSet);
        $scope.getDocumentProof(id);
        //getDocumentProof(id);
        getDocumentSetDetailList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.complianceDocumentSetDetailListForSave = [];
    function complianceDocumentSetDetailForSave(list) {
        angular.forEach(list, function (item) {
            $scope.complianceDocumentSetDetailListForSave.push(
                {
                    Id: item.Id,
                    ComplianceDocumentId: item.ComplianceDocumentId,
                    ComplianceDocumentSetId: $scope.complianceDocumentSetNew.Id,
                    OptionalOrMandatory: item.OptionalOrMandatory
                }
            );
        });
    }
    function getDocumentProofListForSave(list) {
        $scope.documentProofListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                $scope.documentProofListForSave.push(item);
            }
        });
    }
    $scope.Save = function () {
        angular.copy($scope.complianceDocumentSetNew, $scope.complianceDocumentSet);
        $scope.$broadcast('show-errors-check-validity');
        $scope.complianceDocumentSetDetailListForSave = [];
        complianceDocumentSetDetailForSave($scope.complianceDocumentSetDetailList);
        getDocumentProofListForSave($scope.documentProofList);
        if ($scope.complianceDocumentSetNewForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'complianceDocumentSet': $scope.complianceDocumentSet, 'complianceDocumentSetDetail': $scope.complianceDocumentSetDetailListForSave, 'complianceDocumentSetProofTypeAssign': $scope.documentProofListForSave
                        //'complianceDocumentSet': $scope.complianceDocumentSet, 'complianceDocumentSetDetail': $scope.complianceDocumentSetDetailListForSave, 'complianceDocumentSetProofTypeAssign': $scope.tempList

                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.complianceDocumentSets.push(response.data.ComplianceDocumentSet);
                        $scope.complianceDocumentSets = $filter('orderBy')($scope.complianceDocumentSets, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.ComplianceDocumentSet.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'complianceDocumentSet': $scope.complianceDocumentSet, 'complianceDocumentSetDetail': $scope.complianceDocumentSetDetailListForSave, 'complianceDocumentSetProofTypeAssign': $scope.documentProofListForSave
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.complianceDocumentSets[$scope.index] = $scope.complianceDocumentSet;
                            $scope.complianceDocumentSets = $filter('orderBy')($scope.complianceDocumentSets, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.complianceDocumentSetNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.complianceDocumentSetNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.complianceDocumentSets.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.complianceDocumentSet = {};
        $scope.complianceDocumentSetNew = {};
        $scope.complianceDocumentSetDetailList = [];
        $scope.complianceDocumentSetNew.Sequence = seq;
        $scope.complianceDocumentSetNew.Active = true;
        $scope.tempList = [];
        $scope.getDocumentProof();
    }

    $scope.paginationParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: 'Sequence',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.tempList = [];
    $scope.firstTemp = [];
    $scope.documentProofList = [];

    //$scope.getDocumentProof = function(complianceDocumentSetId) {
    //    $http.get('employees/ComplianceDocumentSet/GetComplianceDocumentSetProofTypeAssignList?complianceDocumentSetId=' + complianceDocumentSetId)
    //        .then(
    //        function successCallback(response) {
    //            $scope.documentProofList = response.data.Rows;
    //        });
    //}

    $scope.getDocumentProof = function (complianceDocumentSetId) {
        $scope.Url = 'employees/ComplianceDocumentSet/GetComplianceDocumentProofTypeAssignList?complianceDocumentSetId=' + complianceDocumentSetId;
        $scope.getTypeData = function (pageno) {
            baseService.paginationBase($scope.Url, pageno, $scope.paginationParameters)
                .then(function (result) {
                    $scope.documentProofList = result.Rows;
                    $scope.paginationParameters.total_count = result.Total;
                    getFirstTimeActive($scope.documentProofList);

                    for (var i = 0; i < baseService.arrayLength($scope.documentProofList); i++) {
                        $scope.documentProofList[i].Flag = checkExistTempList($scope.tempList, $scope.documentProofList[i].ComplianceDocumentProofTypeId);
                    }

                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getTypeData();
    };
    $scope.getDocumentProof();


    function getFirstTimeActive(list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Flag)
                pushIntempList($scope.tempList, list[i]);
        }
    }
    function pushIntempList(tempList, item) {
        if (baseService.arrayLength(tempList) > 0) {
            if (!baseService.valueCheckInList(tempList, 'ComplianceDocumentProofTypeId', item.ComplianceDocumentProofTypeId)) {
                tempList.push(item);
            }
        }
        else
            tempList.push(item);
    }
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (!baseService.valueCheckInList($scope.tempList, 'ComplianceDocumentProofTypeId', data.ComplianceDocumentProofTypeId))
                    $scope.tempList.push(data);
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                        if ($scope.tempList[i].ComplianceDocumentProofTypeId === data.ComplianceDocumentProofTypeId) {
                            $scope.tempList[i].Flag = true;
                            break;
                        }
                    }
                }
            }
            else {
                for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                    if ($scope.tempList[i].ComplianceDocumentProofTypeId === data.ComplianceDocumentProofTypeId) {
                        $scope.tempList[i].Flag = false;
                        break;
                    }
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    function checkExistTempList(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ComplianceDocumentProofTypeId === id && list[i].Flag)
                return true;
        }
        return false;
    }




}