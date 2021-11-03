'use strict';
localLanguageSalaryHeadController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function localLanguageSalaryHeadController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Local Language';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.LanguageId = false;
    //$scope.salaryHeads = [];
    $scope.path = 'Setups/LocalLanguage/';
    $scope.getListUrl = $scope.path + 'GetSalaryHeadList';
    baseService.init($scope.getListUrl, null, null, null, "SalaryHead", "SalaryHead");
    $scope.salaryHeads = {
        SalaryHeadId: null
       ,SalaryHead: null
       ,HeadType: null
       ,HeadCategory: null
       ,LeaveType: null
       ,Id: null
       ,LocalLanguage: null 
       ,LLID: null
       ,LanguageId: null
       ,LegalSalaryGradeId: null
       ,LabelName: null
       ,LabelId: null
        , DesignationId: null
        , LineId: null
        , SectionId: null
        , DepartmentId: null
        , UnitId: null
        , DivisionId: null
        , SubDivisionId: null
        , PlantId: null
        , CompanyGroupId: null
        , CompanyGroup: null 
        , LegalDesignationId: null 
    };
  
    $scope.localLanguageList = [];
    cboService.getEnumCbo("enum/GetCboLabelNameInLocalLanguage", function (result) {
        $scope.localLanguageList = result;
    });

    $scope.SelectedType = {
        SelectedItem: null
    };

    $scope.show = false;
   
    $scope.getLanguage = function ()
    {
            $scope.salaryHeads = [];
            if (baseService.isUndefinedOrNull($scope.SelectedType.SelectedItem)) {
                return false;
            }
            $http({
                method: 'GET',
                url: 'Setups/LocalLanguage/GetLanguageTypeList?LanguageId=' + $scope.languageNew.LanguageId + '&flag=' + $scope.SelectedType.SelectedItem,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.salaryHeads = response.data;
                }
                $scope.show = true;

            });
        
    };

    $scope.salaryHead = {
        SalaryHeadId: null,
        SalaryHead: null,
        LeaveType: null,
        Id: null,
        LocalLanguage: null, LLID: null,
        LanguageId: null
    };
    $scope.salaryHeadNew = angular.copy($scope.salaryHead);

    $scope.language = {
        Id: null,
        LanguageId: null,
        LanguageName: null,
        Name: null,
        SalaryHeadId: null
    };
    $scope.languageNew = Object.assign({}, $scope.language);
    $scope.salaryHeadNew = Object.assign({}, $scope.salaryHead);


    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    $scope.Get = function (SalaryHeadId, index) {
        $scope.index = index;
        $scope.salaryHead = $scope.salaryHeads[$scope.index];
        $scope.salaryHeadNew = Object.assign({}, $scope.salaryHead);
        clearLanguage();
        $scope.languageDataList = [];
        $scope.languageData();
        $scope.Action = 'Save';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if (baseService.isUndefinedOrNull($scope.SelectedType.SelectedItem)) {
            ShowResult("Select Entry Type.", 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.languageNew.LanguageId)) {
            ShowResult("Select Language.", 'failure');
            return false;
        }
        if (baseService.arrayLength($scope.salaryHeads) > 0) {
            if ($scope.salaryHeadNewForm.$valid) {
                angular.copy($scope.salaryHeadNew, $scope.salaryHead);

                for (var i = 0; i < $scope.salaryHeads.length; i++) {
                    $scope.salaryHeads[i].LanguageId = $scope.languageNew.LanguageId;
                    if ($scope.SelectedType.SelectedItem === 'SalaryHead') {
                        $scope.salaryHeads[i].SalaryHeadId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Leave') {
                        $scope.salaryHeads[i].LeaveTypeId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Label') {
                        $scope.salaryHeads[i].LabelName = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'CompanyGroup') {
                        $scope.salaryHeads[i].CompanyGroupId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Company'){
                        $scope.salaryHeads[i].CompanyId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Designation') {
                        $scope.salaryHeads[i].DesignationId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'LegalDesignation') {
                        $scope.salaryHeads[i].LegalDesignationId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Department') {
                        $scope.salaryHeads[i].DepartmentId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'EmpGrade') {
                        $scope.salaryHeads[i].LegalSalaryGradeId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Line') {
                        $scope.salaryHeads[i].LineId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Section') {
                        $scope.salaryHeads[i].SectionId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'SubSection') {
                        $scope.salaryHeads[i].SubSectionId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Unit') {
                        $scope.salaryHeads[i].UnitId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Division') {
                        $scope.salaryHeads[i].DivisionId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'SubDivision') {
                        $scope.salaryHeads[i].SubDivisionId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'Plant')
                    {
                        $scope.salaryHeads[i].PlantId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'EmployeeWorkType') {
                        $scope.salaryHeads[i].EmployeeWorkTypeId = $scope.salaryHeads[i].Sid;
                    }
                    else if ($scope.SelectedType.SelectedItem === 'FinalSettlementHead') {
                        $scope.salaryHeads[i].FinalSettlementHeadId = $scope.salaryHeads[i].Sid;
                    }
                }

                if ($scope.Action == 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Setups/LocalLanguage/CreateSalaryHead',
                        data: {
                            'localLanguage': $scope.salaryHeads
                            //'flag':  $scope.SelectedType.SelectedItem
                        },
                        //data: $scope.salaryHead,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getLanguage();
                            //$scope.salaryHeads.push(response.data.SalaryHead);
                            //$scope.salaryHeads = $filter('orderBy')($scope.salaryHeads);
                            //baseService.paginationAdd();
                            //ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: 'Setups/LocalLanguage/EditSalaryHead',
                        data: {
                            'salaryHead': $scope.salaryHeadNew
                            , 'localLanguages': $scope.languageDataList
                        },
                        //data: $scope.salaryHead,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.salaryHeads[$scope.index] = $scope.salaryHead;
                                $scope.salaryHeads = $filter('orderBy')($scope.salaryHeads);
                                $scope.getLanguage();
                            }
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                }
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.salaryHeadNew.SalaryHeadId)) {
            $http({
                method: 'POST',
                url: 'Setups/LocalLanguage/DeleteSalaryHead/' + $scope.salaryHeadNew.SalaryHeadId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.salaryHeads.splice($scope.index, 1);
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
            //if (baseService.isUndefinedOrNull($scope.salaryHeadNew.SalaryHeadId)) {
            //    throw 'Please insert SalaryHead.';
            //}
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
                    SalaryHeadId: $scope.salaryHeadNew.SalaryHeadId,
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
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetSalaryHeadLanguageList?salaryHeadId=' + $scope.salaryHeadNew.SalaryHeadId;
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
        $scope.salaryHead = {};
        clearLanguage();
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

