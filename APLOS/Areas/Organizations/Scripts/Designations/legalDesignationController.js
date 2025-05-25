'use strict';
LegalDesignationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function LegalDesignationController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = 'Legal Designation';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.legalDesignations = [];
    $scope.path = 'Organizations/legalDesignation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl + '?ids=' + null);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.legalDesignations = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList = [
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
            'name': 'User Define Name',
            'value': 'UserName'
        },
        {
            'name': 'User Define Reort Designation',
            'value': 'UserDefineReortDesignation'
        },
        {
            'name': 'Designation Master',
            'value': 'DesignationMaster'
        }
    ];

    $scope.legalDesignation = {
        Id: null
        , Sequence: 0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
        , IsGradeSpecific: false
        , UserDefineReortDesignation: null
        , Factor: 1
        , LegalUserCategoryId:null
    };
    $scope.legalDesignationNew = angular.copy($scope.legalDesignation);

    $scope.empTypeList = [];
    $http({
        method: 'GET',
        url: 'employees/employeecategory/getcbo'
    }).then(function successCallback(response) {
        $scope.empTypeList = response.data;
    });

    $scope.language = {
        Id: null
        , LanguageId: null
        , LanguageName: null
        , Name: null
    };
    $scope.languageNew = Object.assign({}, $scope.language);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.legalDesignationNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.LegalSalaryGradeList = [];
    $scope.GetLegalSalaryGradeCbo = function () {
        $http.get('HumanResource/LegalSalaryGrade/GetLegalSalaryGradeCbo')
            .then(function (response) {
                $scope.LegalSalaryGradeList = response.data;
            });
    }
    $scope.GetLegalSalaryGradeCbo();



    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    $scope.Get = function (index) {

        $scope.index = index;
        $scope.legalDesignation = $scope.legalDesignations[$scope.index];
        $scope.legalDesignation.AddedDate = $filter('dateFiltering')($scope.legalDesignation.AddedDate, 'dd-M-yyyy');
        $scope.legalDesignation.UpdatedDate = $filter('dateFiltering')($scope.legalDesignation.UpdatedDate, 'dd-M-yyyy');
        $scope.legalDesignationNew = angular.copy($scope.legalDesignation);
        clearLanguage();
        $scope.PlantList = [];
        $scope.languageDataList = [];
        $scope.languageData();
        $scope.getPlant();

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.PlantList = [];
    $scope.getPlant = function () {
        $scope.GetLegalSalaryGradeCbo();
        $scope.PlantList = [];
        $scope.legalSalaryGradeDesignationList = [];
        $http.get('HumanResource/LegalSalaryGradeDesignation/GetCompanyPlant?legalDesignationId=' + $scope.legalDesignationNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.PlantList = response.data;
                        for (var i = 0; i < $scope.PlantList.length; i++) {
                            if ($scope.PlantList[i].Flag) {
                                $scope.legalSalaryGradeDesignationList.push($scope.PlantList[i]);
                            }

                        }

                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getPlant();

    $scope.indexx = -1;
    $scope.setLGD = function (x, index) {
        $scope.legalSalaryGradeDesignationList = [];
        $scope.indexx = index;
        for (var i = 0; i < $scope.PlantList.length; i++) {
            if (i === $scope.indexx && $scope.PlantList[i].Flag) {
                $scope.PlantList[i].LegalSalaryGradeId = x;
            }
        }
        $scope.indexx = -1;
    }


    $scope.legalSalaryGradeDesignationList = [];

    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;
        for (var i = 0; i < $scope.PlantList.length; i++) {
            $scope.PlantList[i].Flag = _isselected;
        }

        for (var i = 0; i < baseService.arrayLength($scope.PlantList); i++) {
            if (_isselected)
                $scope.legalSalaryGradeDesignationList.push($scope.PlantList[i]);
            else
                for (var j = 0; j < $scope.legalSalaryGradeDesignationList.length; j++) {
                    if ($scope.legalSalaryGradeDesignationList[j].PlantId === $scope.PlantList[i].PlantId) {
                        $scope.legalSalaryGradeDesignationList.splice(j, 1);
                        break;
                    }
                }
        }
    };

    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.legalSalaryGradeDesignationList, data.PlantId) === false) {
                    $scope.legalSalaryGradeDesignationList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.legalSalaryGradeDesignationList.length; i++) {
                    if ($scope.legalSalaryGradeDesignationList[i].PlantId === data.PlantId) {
                        $scope.legalSalaryGradeDesignationList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, PlantId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PlantId === PlantId) {
                return true;
            }
        }
        return false;
    }

    function reDirectToRequiredTab() {
        if ($scope.legalDesignationNewForm.$invalid) {
            $scope.setTab(1);
        }

    }

    $scope.legalSalaryGradeDesignationList = [];
    $scope.Save = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.legalDesignationNew.Factor)) {
                if ($scope.legalDesignationNew.Factor < 0) {
                    throw "Factor should greater than 0.";
                }
            }
            if (baseService.arrayLength($scope.legalSalaryGradeDesignationList) === 0) {
                for (var i = 0; i < $scope.PlantList.length; i++) {
                    $scope.legalSalaryGradeDesignationList.push($scope.PlantList[i]);
                }
            }

            $scope.$broadcast('show-errors-check-validity');
            reDirectToRequiredTab();
            if ($scope.legalDesignationNewForm.$valid) {
                angular.copy($scope.legalDesignationNew, $scope.legalDesignation);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'legalDesignation': $scope.legalDesignation
                            , 'localLanguages': $scope.languageDataList
                            , 'legalSalaryGradeDesignation': $scope.legalSalaryGradeDesignationList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.legalDesignations.push(response.data.LegalDesignation);
                            $scope.legalDesignations = $filter('orderBy')($scope.legalDesignations, 'Sequence');
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                            $scope.setTab(1);
                            $scope.getPlant();
                            $scope.legalSalaryGradeDesignationList = [];
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'legalDesignation': $scope.legalDesignation
                            , 'localLanguages': $scope.languageDataList
                            , 'legalSalaryGradeDesignation': $scope.legalSalaryGradeDesignationList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.legalDesignations[$scope.index] = $scope.legalDesignation;
                                $scope.legalDesignations = $filter('orderBy')($scope.legalDesignations, 'Sequence');
                            }
                            ClearFields(response.data.Sequence);
                            $scope.setTab(1);
                            $scope.getPlant();
                            $scope.legalSalaryGradeDesignationList = [];
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.legalDesignationNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.legalDesignationNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.legalDesignations.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    //#region Local Language
    $scope._languageIndex = -1;
    $scope.languageTbl = false;
    $scope.LanguageCaption = 'Add Row';
    $scope.languageDataList = [];
    $scope.updateLanguage = function (languageId, languageName) {
        $scope.languageDataList[$scope._languageIndex].Name = languageName;
        $scope._languageIndex = -1;
        $scope.languageNew = {};
    };
    $scope.AddMultipleLanguage = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.languageNew.LanguageId))
                throw 'Please select your language';
            if (baseService.isUndefinedOrNull($scope.languageNew.Name))
                throw 'Please insert locally translated name';

            var isAvailable = false;
            var lng = document.getElementById("languageId").options[document.getElementById("languageId").selectedIndex]
                .text;
            for (var i = 0; i < $scope.languageDataList.length; i++) {
                isAvailable = listValidation($scope.languageDataList[i].LanguageId, $scope.languageNew.LanguageId, i);
                if (isAvailable) {
                    throw "This Language : [" + lng + "] has been already taken";
                }
            }
            angular.copy($scope.languageNew, $scope.language);
            if ($scope._languageIndex === -1) {
                $scope.languageDataList.push({
                    Id: null,
                    LanguageId: $scope.language.LanguageId,
                    LanguageName: lng,
                    Name: $scope.language.Name
                });
            }
            else {
                $scope.language.LanguageName = lng;
                $scope.languageDataList[$scope._languageIndex] = $scope.language;
            }
            if (!$scope.languageTbl) {
                $scope.languageTbl = true;
            }
            clearLanguage();
            } 
            catch (e) {
            ShowResult(e, 'failure');
        }
    };

    function listValidation(oldValue, newValue, index) {
        if ($scope._languageIndex === -1) {
            if (oldValue === newValue) {
                return true;
            }
        }
        else {
            if ($scope._languageIndex !== index) {
                if (oldValue === newValue) {
                    return true;
                }
            }
        }
        return false;
    }

    $scope.languageDataParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'LanguageName',
        searchBy: null,
        pageSize: 10,
        total_count: 0,
        search: 'LanguageName',
        serverPagination: true
    };

    $scope.languageData = function () {
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetLegalDesignationLanguageList?legalDesignationId=' + $scope.legalDesignationNew.Id;
        $scope.getlanguageData = function (pageno) {
            baseService.paginationBase($scope.languageDataUrl, pageno, $scope.languageDataParameters)
                .then(function (result) {
                    $scope.languageDataList = result.Rows;
                    $scope.languageDataParameters.total_count = result.Total;
                    if ($scope.languageDataList.length > 0) {
                        $scope.languageTbl = true;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'languageDataId');
                }).finally(function () {
                });
        };
        $scope.getlanguageData();
    };

    $scope.languageEdit = function (data, index) {
        $scope.language = $scope.languageDataList[index];
        $scope.languageNew = Object.assign({}, $scope.language);
        $scope._languageIndex = index;
        $scope.LanguageCaption = "Update Row";
    };

    $scope.languageDelete = function (data, index) {
        $scope.message_confirmation = '';
        $scope._languageIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.LanguageName + ' ]?';
        angular.element(document.querySelector('#confirmlngPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        $scope.languageDataList.splice($scope._languageIndex, 1);
        $scope._languageIndex = -1;
    };
    //#endregion Local Language

    function clearLanguage() {
        $scope.language = {};
        $scope.languageNew = {};
        $scope._languageIndex = -1;
        $scope.LanguageCaption = 'Add Row';
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.legalDesignation = {};
        $scope.legalDesignationNew = { Sequence: seq, Active: true, Factor: 1 };
        clearLanguage();
        $scope.languageDataList = [];
        $scope.getPlant();
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}