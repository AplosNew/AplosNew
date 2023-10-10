'use strict';
function DepartmentController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Department';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.departments = [];
    $scope.path = 'Organizations/Department/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.departments = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.department = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null,
        DepartmentHeadCode: null,
        DepartmentHead: null,
        DepartmentHeadId:null
    };
    $scope.departmentNew = angular.copy($scope.department);

    $scope.language = {
        Id: null,
        LanguageId: null,
        LanguageName: null,
        Name: null
    };
    $scope.languageNew = Object.assign({}, $scope.language);

    $scope.GetSequence = function () {
        $http.get('Organizations/department/getautosequence')
            .then(function (response) {
                $scope.departmentNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.department = $scope.departments[$scope.index];
        $scope.departmentNew = Object.assign({}, $scope.department);
        $scope.languageData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.popUpDataList = [];
    $scope.showEmployeeListPopUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmpData'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });

            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.departmentNew.DepartmentHeadId = arg.data.SystemId;
        $scope.departmentNew.DepartmentHead = arg.data.EmployeeName;
        $scope.departmentNew.DepartmentHeadCode = arg.data.EmployeeCode;
        $scope.closePopUp();
    }


    $scope.clearEmp = function () {
        $scope.departmentNew.DepartmentHeadId = null;
        $scope.departmentNew.DepartmentHead = null;
        $scope.departmentNew.DepartmentHeadCode = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.departmentNewForm.$valid) {
            angular.copy($scope.departmentNew, $scope.department);
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/department/create',
                    data: {
                        'department': $scope.department
                        , 'localLanguages': $scope.languageDataList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.departments.push(response.data.Department);
                        $scope.departments = $filter('orderBy')($scope.departments, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: 'Organizations/department/edit',
                    data: {
                        'department': $scope.department
                        , 'localLanguages': $scope.languageDataList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.departments[$scope.index] = $scope.department;
                            $scope.departments = $filter('orderBy')($scope.departments, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.departmentNew.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/department/delete/' + $scope.departmentNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.departments.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
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

    //Local Language Part Start
    $scope._languageIndex = -1;
    $scope.languageTbl = false;
    $scope.LanguageCaption = 'Add Row';
    $scope.languageDataList = [];
    $scope.AddMultipleLanguage = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.languageNew.LanguageId)) {
                throw 'Please select your language.';
            }
            if (baseService.isUndefinedOrNull($scope.languageNew.Name)) {
                throw 'Please insert name.';
            }
            var isAvailable = false;
            var lng = document.getElementById("languageId").options[document.getElementById('languageId').selectedIndex].text;
            for (var i = 0; i < $scope.languageDataList.length; i++) {
                isAvailable = listValidation($scope.languageDataList[i].LanguageId, $scope.languageNew.LanguageId, i);
                if (isAvailable) {
                    throw 'This Language : [' + lng + '] has been already taken';
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
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    function listValidation(oldValue, newValue, index) {
        var isAvailable = false;
        // LanguageId
        if ($scope._languageIndex == -1) {
            if (oldValue == newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if ($scope._languageIndex != index) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        return isAvailable;
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
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetDepartmentLanguageList?departmentId=' + $scope.departmentNew.Id;
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
    }
    $scope.languageEdit = function (data, index) {
        $scope.language = $scope.languageDataList[index];
        $scope.languageNew = Object.assign({}, $scope.language);
        $scope._languageIndex = index;
        $scope.LanguageCaption = 'Update Row';
    }

    $scope.languageDelete = function (data, index) {
        $scope.message_confirmation = '';
        $scope._languageIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.LanguageName + ' ]';
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

    function clearLanguage() {
        $scope.language = {};
        $scope.languageNew = {};
        $scope._languageIndex = -1;
        $scope.LanguageCaption = 'Add Row';
    }
    //Local Language Part End

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        clearLanguage();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.department = {};
        $scope.departmentNew = {};
        $scope.departmentNew.Sequence = seq;
        $scope.departmentNew.Active = true;
        $scope.languageDataList = [];
        $scope.languageTbl = false;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}
DepartmentController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];