'use strict';
DesignationSetupController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function DesignationSetupController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $scope.message_confirmation = '';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    //  #region Employee Category
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $rootScope.titleEC = 'Employee Category';
    $scope.ActionEC = 'Save';
    $scope.index = -1;
    $scope.employeeCategories = [];
    $scope.pathEC = 'employees/employeecategory/';
    $scope.saveUrlEC = $scope.pathEC + 'create';
    $scope.updateUrlEC = $scope.pathEC + 'edit';
    $scope.deleteUrlEC = $scope.pathEC + 'delete/';
    $scope.getListUrlEC = $scope.pathEC + 'GetList';
    //baseService.init($scope.getListUrlEC, null, null, null, 'Sequence', 'Sequence');

    $scope.getDataEC = function (pageno) {
        //baseService.pagination(pageno)
        //    .then(function (result) {
        //        $scope.employeeCategories = result.Rows;
        //    }, function () {
        //        ShowResult(commonMessage.NetworkError, 'failure');
        //    }).finally(function () {
        //    });
        $http({
            method: 'POST',
            url: "employees/employeecategory/GetECList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employeeCategories = response.data;
        });
    };
    $scope.getDataEC();
    $scope.employeeCategory = {
        Id: null
        , Sequence: 0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
        , WorkingDaysInAMonth: null
    };
    $scope.employeeCategoryNew = angular.copy($scope.employeeCategory);

    $scope.GetSequenceEC = function () {
        $http.get('employees/employeecategory/getautosequence')
            .then(function (response) {
                $scope.employeeCategoryNew.Sequence = response.data;
            });
    };
    $scope.GetSequenceEC();

    $scope.languageEC = {
        Id: null
        , LanguageId: null
        , LanguageName: null
        , Name: null
    };
    $scope.languageNewEC = Object.assign({}, $scope.language);

    $scope.languageListEC = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageListEC = data;
    });

    $scope.WorkingDaysInAMonthList = [];
    cboService.getEnumCbo("enum/GetWorkingDaysInAMonthEnumCbo", function (result) {
        $scope.WorkingDaysInAMonthList = result;
    });

    $scope.GetEC = function (args) {

        $scope.employeeCategoryNew = Object.assign({}, args.data);
        clearLanguage();
        $scope.languageDataListEC = [];
        $scope.languageData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.SaveEC = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.employeeCategoryForm.$valid) {
            angular.copy($scope.employeeCategoryNew, $scope.employeeCategory);
            if ($scope.ActionEC === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrlEC,
                    data: {
                        'EmployeeCategory': $scope.employeeCategory
                        , 'localLanguages': $scope.languageDataListEC
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.employeeCategories.push(response.data.EmployeeCategoryNew);
                        $scope.employeeCategories = $filter('orderBy')($scope.employeeCategories, 'Sequence');
                        baseService.paginationAdd();
                        ClearFieldsEC(response.data.Sequence);
                        $scope.getDataEC();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.ActionEC === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrlEC,
                    data: {
                        'EmployeeCategory': $scope.employeeCategory
                        , 'localLanguages': $scope.languageDataListEC
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.employeeCategories[$scope.index] = $scope.employeeCategoryNew;
                            $scope.employeeCategories = $filter('orderBy')($scope.employeeCategories, 'Sequence');
                            $scope.getDataEC();
                        }
                        ClearFieldsEC(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.DeleteEC = function () {
        if (!baseService.isUndefinedOrNull($scope.employeeCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlEC + $scope.employeeCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.employeeCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFieldsEC(response.data.Sequence);
                    $scope.getDataEC();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };


    $scope._languageIndex = -1;
    $scope.languageTbl = false;
    $scope.LanguageCaption = 'Add Row';
    $scope.languageDataListEC = [];
    $scope.AddMultipleLanguage = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.languageNewEC.LanguageId))
                throw 'Please select your language';
            if (baseService.isUndefinedOrNull($scope.languageNewEC.Name))
                throw 'Please insert locally translated name';
            var lng = document.getElementById("languageId").options[document.getElementById('languageId').selectedIndex].text;
            for (var i = 0; i < $scope.languageDataListEC.length; i++) {
                if (baseService.isAvailableInList($scope.languageDataListEC[i].LanguageId, $scope.languageNewEC.LanguageId, i, $scope._languageIndex))
                    throw 'This Language : [' + lng + '] has been already taken';
            }
            angular.copy($scope.languageNewEC, $scope.language);
            if ($scope._languageIndex === -1) {
                $scope.languageDataListEC.push({
                    Id: null,
                    LanguageId: $scope.language.LanguageId,
                    LanguageName: lng,
                    Name: $scope.language.Name
                });
            }
            else {
                $scope.language.LanguageName = lng;
                $scope.languageDataListEC[$scope._languageIndex] = $scope.language;
            }
            if (!$scope.languageTbl) {
                $scope.languageTbl = true;
            }
            clearLanguage();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.languageDataParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'LanguageName'
        , searchBy: null
        , pageSize: 10
        , total_count: 0
        , search: 'LanguageName'
        , serverPagination: true
    };

    $scope.languageData = function () {
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetEmployeeCategoryLanguageList?employeeCategoryId=' + $scope.employeeCategoryNew.Id;
        $scope.getlanguageData = function (pageno) {
            baseService.paginationBase($scope.languageDataUrl, pageno, $scope.languageDataParameters)
                .then(function (result) {
                    $scope.languageDataListEC = result.Rows;
                    $scope.languageDataParameters.total_count = result.Total;
                    if ($scope.languageDataListEC.length > 0) {
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
        $scope.languageNewEC = Object.assign({}, $scope.language);
        $scope._languageIndex = index;
        $scope.LanguageCaption = 'Update Row';
    };

    $scope.languageDelete = function (data, index) {
        $scope.message_confirmation = '';
        $scope._languageIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.LanguageName + ' ]?';
        angular.element(document.querySelector('#confirmlngPopUp')).modal('show');
    };

    $scope.removeRowEC = function () {
        $scope.languageDataList.splice($scope._languageIndex, 1);
        if ($scope.languageDataList.length > 0)
            $scope.languageTbl = true;
        else
            $scope.languageTbl = false;
        $scope._languageIndex = -1;
    };

    $scope.ClearEC = function () {
        ClearFieldsEC($scope.GetSequenceEC());
        clearLanguage();
        return true;
    };

    function clearLanguage() {
        $scope.language = {};
        $scope.languageNewEC = {};
        $scope._languageIndex = -1;
        $scope.LanguageCaption = 'Add Row';
    }

    function ClearFieldsEC(seq) {
        $scope.Action = 'Save';
        $scope.employeeCategoryNew = {};
        $scope.employeeCategoryNew = { Sequence: seq, Active: true };
        clearLanguage();
        $scope.languageDataListEC = [];
    }

    $scope.tabEC = 1;
    $scope.setTabEC = function (newTab) {
        $scope.tabEC = newTab;
    };
    $scope.isSetEC = function (tabNum) {
        return $scope.tabEC === tabNum;

    }
    //  #endregion  Employee Category

    //  #region Designatiion Group
    $rootScope.titleDG = 'Designation Group';
    $scope.ActionDG = 'Save';
    $scope.index = -1;
    $scope.designationgroups = [];
    $scope.pathDG = 'Organizations/designationgroup/';
    $scope.getListUrlDG = $scope.pathDG + 'getlist';
    $scope.getUrlDG = $scope.pathDG + 'get';
    $scope.getSeqUrlDG = $scope.pathDG + 'getautosequence';
    $scope.saveUrlDG = $scope.pathDG + 'create';
    $scope.updateUrlDG = $scope.pathDG + 'edit';
    $scope.deleteUrlDG = $scope.pathDG + 'delete/';
  //  baseService.init($scope.getListUrlDG);
    $scope.getDataDG = function (pageno) {
        //baseService.pagination(pageno)
        //    .then(function (result) {
        //        $scope.designationgroups = result.Rows;
        //    }, function () {
        //        ShowResult(commonMessage.NetworkError, 'failure');
        //    }).finally(function () {
        //    });

        $http({
            method: 'POST',
            url: "Organizations/designationgroup/GetDGList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.designationgroups = response.data;
        });
    };
    $scope.getDataDG();

    $scope.designationgroup = {
        Id: null
        , Sequence: 0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.designationgroupNew = angular.copy($scope.designationgroup);

    $scope.languageDG = {
        Id: null
        , LanguageId: null
        , LanguageName: null
        , Name: null
    };
    $scope.languageNewDG = Object.assign({}, $scope.languageDG);

    $scope.GetSequenceDG = function () {
        $http.get($scope.getSeqUrlDG)
            .then(function (response) {
                $scope.designationgroupNew.Sequence = response.data;
            });
    };
    $scope.GetSequenceDG();

    $scope.languageListDG = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageListDG = data;
    });


    $scope.GetDG = function (args) {
       
        $scope.designationgroupNew = Object.assign({}, args.data);
        clearLanguageDG();
        $scope.languageDataListDG = [];
        $scope.languageDataDG();
        $scope.ActionDG = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();


    };

    $scope.SaveDG = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.designationgroupNewForm.$valid) {
            angular.copy($scope.designationgroupNew, $scope.designationgroup);
            if ($scope.ActionDG === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrlDG,
                    data: {
                        'designationGroup': $scope.designationgroup
                        , 'localLanguages': $scope.languageDataListDG
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.designationgroups.push(response.data.Designationgroup);
                        $scope.designationgroups = $filter('orderBy')($scope.designationgroups, 'Sequence');
                        baseService.paginationAdd();
                        $scope.getDataDG();
                        ClearFieldsDG(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.ActionDG === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrlDG,
                    data: {
                        'designationGroup': $scope.designationgroup
                        , 'localLanguages': $scope.languageDataListDG
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.designationgroups[$scope.index] = $scope.designationgroup;
                            $scope.designationgroups = $filter('orderBy')($scope.designationgroups, 'Sequence');
                        }
                        ClearFieldsDG(response.data.Sequence);
                        $scope.getDataDG();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.RemoveDesignation = function () {
        $scope.message_confirmation = '';   
        if (!baseService.isUndefinedOrNull($scope.designationgroupNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete';
        angular.element(document.querySelector('#confirmDesgPopUp')).modal('show');
    }

    $scope.DeleteDG = function () {
        if (!baseService.isUndefinedOrNull($scope.designationgroupNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlDG + $scope.designationgroupNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.designationgroups.splice($scope.index, 1);
                    //baseService.paginationRemove();
                    $scope.getDataDG();
                    ClearFieldsDG(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    //#region Local Language
    $scope._languageIndex = -1;
    $scope.languageTbl = false;
    $scope.LanguageCaption = 'Add Row';
    $scope.languageDataListDG = [];
    $scope.AddMultipleLanguage = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.languageNewDG.LanguageId))
                throw 'Please select your language';
            if (baseService.isUndefinedOrNull($scope.languageNewDG.Name))
                throw 'Please insert locally translated name';
            var lng = document.getElementById("languageId").options[document.getElementById('languageId').selectedIndex].text;
            for (var i = 0; i < $scope.languageDataListDG.length; i++) {
                if (baseService.isAvailableInList($scope.languageDataListDG[i].LanguageId, $scope.languageNewDG.LanguageId, i, $scope._languageIndex))
                    throw 'This Language : [' + lng + '] has been already taken';
            }
            angular.copy($scope.languageNewDG, $scope.language);
            if ($scope._languageIndex === -1) {
                $scope.languageDataListDG.push({
                    Id: null,
                    LanguageId: $scope.language.LanguageId,
                    LanguageName: lng,
                    Name: $scope.language.Name
                });
            }
            else {
                $scope.language.LanguageName = lng;
                $scope.languageDataListDG[$scope._languageIndex] = $scope.language;
            }
            clearLanguage();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

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

    $scope.languageDataDG = function () {
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetDesignationGroupLanguageList?designationGroupId=' + $scope.designationgroupNew.Id;
        $scope.getlanguageData = function (pageno) {
            baseService.paginationBase($scope.languageDataUrl, pageno, $scope.languageDataParameters)
                .then(function (result) {
                    $scope.languageDataListDG = result.Rows;
                    $scope.languageDataParameters.total_count = result.Total;
                    if ($scope.languageDataListDG.length > 0) {
                        $scope.languageTbl = true;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'languageDataId');
                }).finally(function () {
                });
        };
        $scope.getlanguageData();
    };

    $scope.languageEdit = function (index) {
        $scope.language = $scope.languageDataList[index];
        $scope.languageNew = Object.assign({}, $scope.language);
        $scope._languageIndex = index;
        $scope.LanguageCaption = 'Update Row';
    };

    $scope.languageDelete = function (data, index) {
        $scope.message_confirmation = '';
        $scope._languageIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.LanguageName + ' ]?';
        angular.element(document.querySelector('#confirmlngPopUp')).modal('show');
    };

    $scope.removeRowDG = function () {
        $scope.languageDataList.splice($scope._languageIndex, 1);
        $scope._languageIndex = -1;
    };
    //#endregion Local Language

    $scope.ClearDG = function () {
        ClearFieldsDG($scope.GetSequenceDG());
        return true;
    };

    function clearLanguageDG() {
        $scope.language = {};
        $scope.languageNew = {};
        $scope._languageIndex = -1;
        $scope.LanguageCaption = 'Add Row';
    }

    function ClearFieldsDG(seq) {
        $scope.ActionDG = 'Save';
        $scope.designationgroup = {};
        $scope.designationgroupNew = { Sequence: seq, Active: true };
        clearLanguageDG();
        $scope.languageDataListDG = [];
    }

    $scope.tabDG = 1;
    $scope.setTabDG = function (newTab) {
        $scope.tabDG = newTab;
    };
    $scope.isSetDG = function (tabNum) {
        return $scope.tabDG === tabNum;
    };
    //  #endregion  Designatiion Group

    //  #region Designation
    $rootScope.title = 'Designation';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.designations = [];
    $scope.path = 'Organizations/designation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
//    baseService.init($scope.getListUrl);
    $scope.searchd = null;
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: "Organizations/designation/GetDList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.designations = response.data;
        });
    };
    $scope.getData();

    $scope.designation = {
        Id: null
        , Sequence: 0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.designationNew = angular.copy($scope.designation);

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
                $scope.designationNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    $scope.Get = function (args) {
        $scope.designationNew = Object.assign({}, args.data);

        clearLanguage();
        $scope.languageDataList = [];
        $scope.languageData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.designationNewForm.$valid) {
            angular.copy($scope.designationNew, $scope.designation);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'designation': $scope.designation
                        , 'localLanguages': $scope.languageDataList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.designations.push(response.data.Designation);
                        $scope.designations = $filter('orderBy')($scope.designations, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'designation': $scope.designation
                        , 'localLanguages': $scope.languageDataList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                        $scope.designations1 = [];
                        $scope.designations1 = $scope.designations;
                        $scope.designations = [];
                        $scope.designations = $scope.designations1;
                        $scope.designations1 = [];
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.designations[$scope.index] = $scope.designation;
                            $scope.designations = $filter('orderBy')($scope.designations, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Remove = function () {
        $scope.message_confirmation = '';
        if (!baseService.isUndefinedOrNull($scope.designationNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete';
        angular.element(document.querySelector('#confirmDsgPopUp')).modal('show');
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.designationNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.designationNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.designations.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope._languageIndex = -1;
    $scope.languageTbl = false;
    $scope.LanguageCaption = 'Add Row';
    $scope.languageDataList = [];
    $scope.AddMultipleLanguage = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.languageNew.LanguageId))
                throw 'Please select your language';
            if (baseService.isUndefinedOrNull($scope.languageNew.Name))
                throw 'Please insert locally translated name';
            var lng = document.getElementById("languageId").options[document.getElementById('languageId').selectedIndex].text;
            for (var i = 0; i < $scope.languageDataList.length; i++) {
                if (baseService.isAvailableInList($scope.languageDataList[i].LanguageId, $scope.languageNew.LanguageId, i, $scope._languageIndex))
                    throw 'This Language : [' + lng + '] has been already taken';
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
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

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
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetDesignationLanguageList?designationId=' + $scope.designationNew.Id;
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
        $scope.LanguageCaption = 'Update Row';
    };

    $scope.languageDelete = function (data, index) {
        $scope.message_confirmation = '';
        $scope._languageIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.LanguageName + ' ]?';
        angular.element(document.querySelector('#confirmlngPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        $scope.languageDataList.splice($scope._languageIndex, 1);
        if ($scope.languageDataList.length > 0)
            $scope.languageTbl = true;
        else
            $scope.languageTbl = false;
        $scope._languageIndex = -1;
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        clearLanguage();
        return true;
    };

    function clearLanguage() {
        $scope.language = {};
        $scope.languageNew = {};
        $scope._languageIndex = -1;
        $scope.LanguageCaption = 'Add Row';
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.designation = {};
        $scope.designationNew = { Sequence: seq, Active: true };
        clearLanguage();
        $scope.languageDataList = [];
    }

    $scope.tabD = 1;
    $scope.setTabD = function (newTab) {
        $scope.tabD = newTab;
    };
    $scope.isSetD = function (tabNum) {
        return $scope.tabD === tabNum;
    };
    //  #endregion  Designation
}