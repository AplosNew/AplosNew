'use strict';
ComplianceDocumentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];
function ComplianceDocumentController(commonMessage, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Compliance Document Category';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.complianceDocuments = [];
    $scope.path = 'employees/complianceDocument/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    // #region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion

    $scope.searchByComplianceDocumentList = [
        {
            'name': 'Document Name',
            'value': 'UserName'
        },
        {
            'name': 'Compliance Document Category',
            'value': 'ComplianceDocumentCategoryName'
        },
        {
            'name': 'Compliance Document SubCategory',
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
            'name': 'Employment Stage',
            'value': 'EmploymentStage'
        }
    ];

    $scope.complianceDocumentListParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'Sequence, UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetDocData = function () {
        baseService.init($scope.getListUrl, null, null, null, 'Sequence, UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.type = $scope.complianceDocumentNew.Type;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.complianceDocuments = result.Rows;
                    //$scope.complianceDocumentListParameters.total_count = result.Total;
                    $rootScope.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.complianceDocument = {
        Id: null,
        ComplianceDocumentCategoryId: null,
        ComplianceDocumentSubCategoryId: null,
        Sequence: null,
        DocumentType: null,
        UserName: null,
        Importance: null,
        EmploymentStage: null,
        DocumentationBy: 'Self',
        DocumentExpirable: 'NonExpirable',
        OptionalOrMandatory: null,
        ResponsiblePersonId: null,
        ResponsiblePersonCode: null,
        DependateDate: null,
        EmpType: null,
        Remarks: null,
        Description: null,
        ProfileType: null,
        QualificationLevelId: null,
        LeadOrLagDays: null,
        IsSkillBased: null,
        IsGlobalDocument: null,
        IsRecurring: false,
        Active: true,
        ReNewUOM: null,
        ReNewAfterEvery: null,
        ReNewAble: false,
        Expirable: false,
        DaysBeforeExpiry: null,
        DocDateRequired: false,
        DocNumberRequired: false
    };
    $scope.complianceDocumentPositonCode = {
        Id: null,
        ComplianceDocumentId: null,
        PositionId: null,
        PositionName: null
    };
    $scope.complianceDocumentNew = Object.assign({}, $scope.complianceDocument);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.complianceDocumentNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    //*******MultiSelect*********/
    $scope.multiSelectSettings = {
        scrollableHeight: 'auto',
        smartButtonMaxItems: 3,
        scrollable: true,
        showCheckAll: false,
        showUncheckAll: false,
        enableSearch: false,
        dynamicTitle: true
    };
    $scope.postRecruitmentSelectedList = [];
    $scope.multi1events = {
        onItemSelect: function (item) {
            // $scope.postRecruitmentSelectedList.push(item);
        }
        //}, onItemDeselect: function (item) {
        //    $scope.cboCratetor($scope.fixedassetClassIds, 'FixedAssetClassId');
        //}
    };
    $scope.resetPostRecritment = function () {
        if ($scope.complianceDocumentNew.EmploymentStage !== 'PostRecruitment') {
            $scope.postRecruitmentSelectedList = [];
        } else {
            getDocumentPostRecruitmentCboList();
        }
    };
    //*******CBO***************
    cboService.getEnumCbo('Enum/GetEnumForDocumentType/', function (result) {
        $scope.documentTypeList = result;
    });
    cboService.getEnumCbo('Enum/GetEnumForImportance/', function (result) {
        $scope.importanceList = result;
    });
    cboService.getEnumCbo('Enum/GetEnumEmploymentStage/', function (result) {
        $scope.employmentStageList = result;
    });
    cboService.getEnumCbo('Enum/GetEnumPostRecruitment/', function (result) {
        $scope.postRecruitmentList = result;
    });
    cboService.getEnumCbo('Enum/GetEnumDependateDate/', function (result) {
        $scope.dependateDateList = result;
    });
    cboService.getCboComplianceDocumentCategory(function (result) {
        $scope.complianceDocumentCategoryList = result;
    });
    cboService.getCboComplianceDocumentSubCategory(function (result) {
        $scope.complianceDocumentSubCategoryList = result;
    });
    $scope.getProfileTypeList = [];
    cboService.getEnumCbo("Enum/GetProfileTypeEnumCbo", function (result) {
        $scope.getProfileTypeList = result;
    });
    cboService.getEnumCbo("Enum/getcompliancedocumentcategoryenumcbo", function (result) {
        $scope.typeList = result;
    });
    cboService.getEnumCbo("Enum/getdurationuomenumcbo", function (result) {
        $scope.durationUOMList = result;
    });
    $scope.qualificationLabelList = [];
    cboService.getCboQualificationLevel(function (result) {
        $scope.qualificationLabelList = result;
    });
    //*********END************
    //*********************** Position PopUp Start *************************************
    $scope.positionSearchList = [];
    $scope.positionDataList = [];
    $scope.positionSearch = [];
    $scope.positionUrl = 'Organizations/Position/GetList';
    $scope.positionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'UserName',
        searchBy: 'Id',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionPopUp = function () {
        $scope.getPositionData = function (pageno) {
            baseService.paginationBase($scope.positionUrl, pageno, $scope.positionParameters)
                .then(function (response) {
                    $scope.positionDataList = response.Rows;
                    $scope.positionParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.positionSearchList) === 0) {
                        $scope.positionSearchList.push(
                            {
                                'Text': 'Id',
                                'Value': 'Id'
                            });
                        baseService.getDDLSearchColumn($scope.positionDataList, $scope.positionSearchList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#positionPopUp')).modal('show');
        $scope.getPositionData();
    };

    $scope.closePositionPopUp = function () {
        $scope.entityId = '';
        $scope.EntityName = '';
        angular.element(document.querySelector('#positionPopUp')).modal('hide');
    };

    $scope.selectPositionPopUp = function (data) {
        $scope.selectedPositionId = data.Id;
        $scope.complianceDocumentPositonCode.PositionId = $scope.selectedPositionId;
        $scope.complianceDocumentPositonCode.PositionName = data.UserName;
        data.PositionId = data.Id;
        $scope.addPositionForSave(data);
        angular.element(document.querySelector('#positionPopUp')).modal('hide');
    };
    $scope.positionList = [];
    $scope.addPositionForSave = function (data) {
        if (checkPositoinExist($scope.positionList, data.Id) === false) {
            data.Id = null;
            data.PositionCode = data.Code;
            $scope.positionList.push(data);
        }
    };
    function checkPositoinExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PositionId === id) {
                return true;
            }
        }
        return false;
    }
    //function checkPositoinExist(list,id) {
    //    angular.forEach(list, function (item) {
    //        if (item.PositionId === id) {
    //            return true;
    //        }
    //    });
    //    return false;
    //}
    $scope.clearPosition = function () {
        $scope.selectedPositionId = null;
        $scope.complianceDocumentPositonCode.PositionId = null;
        $scope.complianceDocumentPositonCode.PositionName = null;
        $scope.positionData = [];
        $scope.positionSearch = [];
    };
    //removing Row
    $scope.documentPositionId = null;
    $scope.documentPositionIndex = -1;
    $scope.valuePassInDelModal = function (data, index) {
        $scope.documentPositionId = data.Id;
        $scope.documentPositionIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.UserName + ' ]?';
        angular.element(document.querySelector('#deleteRow')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.positionList.length; i++) {
            if ($scope.documentPositionId !== null) {
                if ($scope.positionList[i].Id === $scope.documentPositionId) {
                    $scope.removeFromDb($scope.documentPositionId, i);
                    break;
                }
            } else {
                $scope.positionList.splice($scope.documentPositionIndex, 1);
                $scope.documentPositionIndex = -1;
                break;
            }
        }
        $scope.documentPositionId = null;
        $scope.documentPositionIndex = -1;
    };
    $scope.removeFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + '/DeleteDocumentPosition',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.positionList.splice(index, 1);
                    $scope.documentPositionIndex = -1;
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
    //*********************** Position PopUp End *************************************
    // #region Get Employee
    $scope.employeeProfileList = [];
    $scope.employeeProfileDataList = [];
    $scope.employeeProfileParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'Code',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.fieldName = '';
    $scope.employeeProfilePopUp = function () {
        baseService.setCurrentPage('employeeProfileDataList');
        $scope.employeeProfileUrl = 'Organizations/ManpowerBudget/GetForResponsiblePerson';
        $scope.getemployeeProfileData = function (pageno) {
            baseService.paginationBase($scope.employeeProfileUrl, pageno, $scope.employeeProfileParameters)
                .then(function (result) {
                    $scope.employeeProfileDataList = result.Rows;
                    $scope.employeeProfileParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.employeeProfileList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.employeeProfileList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeeProfilePopUp')).modal('show');
        $scope.getemployeeProfileData();
    };

    $scope.selectEmployeedblClick = function (data) {
        $scope.complianceDocumentNew.ResponsiblePersonCode = data.Code;
        $scope.complianceDocumentNew.ResponsiblePersonId = data.Id;
        $scope.complianceDocumentNew.PositionName = data.PositionName;
        angular.element(document.querySelector('#employeeProfilePopUp')).modal('hide');
    };

    $scope.valueData = '';
    $scope.selectEmplyee = function (data) {
        $scope.complianceDocumentNew.ResponsiblePersonCode = data.Code;
        $scope.complianceDocumentNew.ResponsiblePersonId = data.Id;
    };

    $scope.SelectEmployeeByButton = function () {
        if ($scope.valueData === '') {
            ShowResult('Please at first select row.', 'failure', 'employeeProfilePopUp');
            return;
        }
        $scope.selectEmployeedblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#employeeProfilePopUp')).modal('hide');
    };

    $scope.closeEmployeeProfilePopUp = function () {
        $scope.employeeId = '';
        $scope.FullName = '';
        angular.element(document.querySelector('#employeeProfilePopUp')).modal('hide');
    };

    $scope.employeeProfileClear = function () {
        $scope.complianceDocumentNew.ResponsiblePersonCode = null;
        $scope.complianceDocumentNew.ResponsiblePersonId = null;
        $scope.complianceDocumentNew.PositionName = null;
    };

    function getDocumentPositionList() {
        $http.get('employees/ComplianceDocument/GetDocumentPositionList?complianceDocumentId=' + $scope.complianceDocumentNew.Id)
            .then(
            function successCallback(response) {
                $scope.positionList = response.data;
            });
    }
    function getDocumentPostRecruitmentCboList() {
        $http.get('employees/ComplianceDocument/GetDocumentPostRecruitmentCboList?complianceDocumentId=' + $scope.complianceDocumentNew.Id)
            .then(
            function successCallback(response) {
                $scope.postRecruitmentSelectedList = response.data;
            });
    }
    $scope.Get = function (id, index) {
        $scope.tempList = [];
        $scope.index = index;
        $scope.complianceDocument = $scope.complianceDocuments[$scope.index];
        $scope.complianceDocumentNew = Object.assign({}, $scope.complianceDocument);
        $scope.getDocumentProof(id);
        getDocumentPostRecruitmentCboList();
        getDocumentPositionList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    function getPostRecruitmentSelectedListForSave(list) {
        $scope.postRecruitmentSelectedListForSave = [];
        angular.forEach(list, function (item) {
            item.Id = null;
            item.PostRecruitment = item.Value;
            item.ComplianceDocumentId = $scope.complianceDocument.Id;
            $scope.postRecruitmentSelectedListForSave.push(item);
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
        try {
            angular.copy($scope.complianceDocumentNew, $scope.complianceDocument);
            if ($scope.complianceDocument.IsSkillBased && $scope.positionList.length < 1) {
                return ShowResult('Select Position', 'failure');
            }
            if ($scope.complianceDocument.IsSkillBased === false && $scope.positionList.length > 0) {
                return ShowResult('Select Skill Based ', 'failure');
            }
            getPostRecruitmentSelectedListForSave($scope.postRecruitmentSelectedList);
            getDocumentProofListForSave($scope.documentProofList);

            if ($scope.complianceDocument.Type === 'EmployeeRelated') {
                if (baseService.isUndefinedOrNull($scope.complianceDocument.DependateDate)) {
                    throw "Dependate Date is required.";
                }
                if (baseService.isUndefinedOrNull($scope.complianceDocument.EmpType)) {
                    throw "Emp Type is required.";
                }
                if (baseService.isUndefinedOrNull($scope.complianceDocument.LeadOrLagDays)) {
                    throw "Lead Or Lag Days is required.";
                }
                if (baseService.isUndefinedOrNull($scope.complianceDocument.EmploymentStage)) {
                    throw "Employment Stage is required.";
                }
            }
            if ($scope.complianceDocument.ReNewAble) {
                if (baseService.isUndefinedOrNull($scope.complianceDocument.ReNewAfterEvery)) {
                    throw "Renew After is required.";
                }
                if (baseService.isUndefinedOrNull($scope.complianceDocument.ReNewUOM)) {
                    throw "Period is required.";
                }
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.complianceDocumentNewForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'complianceDocument': $scope.complianceDocument, 'complianceDocumentPositon': $scope.positionList, 'complianceDocumentPostRecruitment': $scope.postRecruitmentSelectedListForSave, 'complianceDocumentProofTypeAssign': $scope.documentProofListForSave
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.complianceDocuments.push(response.data.ComplianceDocument);
                            $scope.complianceDocuments = $filter('orderBy')($scope.complianceDocuments, 'Sequence');
                            $scope.GetDocData();
                            baseService.paginationAdd();
                            $scope.positionList = [];
                            $scope.tempList = [];
                            ClearFields(response.data.Sequence);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'complianceDocument': $scope.complianceDocument, 'complianceDocumentPositon': $scope.positionList, 'complianceDocumentPostRecruitment': $scope.postRecruitmentSelectedListForSave, 'complianceDocumentProofTypeAssign': $scope.documentProofListForSave
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.GetDocData();
                            $scope.positionList = [];
                            $scope.tempList = [];
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.complianceDocumentNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.complianceDocumentNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.complianceDocuments.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.complianceDocument = { Type: $scope.complianceDocument.Type };
        $scope.complianceDocumentNew = { Type: $scope.complianceDocumentNew.Type };
        $scope.complianceDocumentNew.Active = true;
        $scope.complianceDocumentNew.DocumentationBy = 'Self';
        $scope.complianceDocumentNew.DocumentExpirable = 'NonExpirable';
        $scope.complianceDocumentNew.Sequence = seq;
        $scope.tempList = [];
        $scope.getDocumentProof();
        $scope.postRecruitmentSelectedList = [];
    }

    //************************** Pagination *********************************

    //$scope.documentProofList = [];
    //$scope.getDocumentProof = function(complianceDocumentId) {
    //    $http.get('employees/ComplianceDocument/GetComplianceDocumentProofTypeAssignList?complianceDocumentId=' + complianceDocumentId)
    //        .then(
    //        function successCallback(response) {
    //            $scope.documentProofList = response.data;
    //        });
    //}
    //$scope.getDocumentProof();

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
    $scope.documentProofList = [];
    $scope.getDocumentProof = function (complianceDocumentId) {
        $scope.Url = 'employees/ComplianceDocument/GetComplianceDocumentProofTypeAssignList?complianceDocumentId=' + complianceDocumentId;
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
    };
    function checkExistTempList(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ComplianceDocumentProofTypeId === id && list[i].Flag)
                return true;
        }
        return false;
    }
}