'use strict';
recruitmentApprovalController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function recruitmentApprovalController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Recruitment Approval';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'employees/recruitmentapproval/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.preRecruitmentEmployee = {
        Id: null,
        Image: null,
        InterviewRankingId: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        PositionID: null,
        BudgetCode: null,
        IsDirect: true,
        FullName: null,
        Gender: null,
        NationalID: null,
        DOB: null,
        Phone: null,
        Email: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        NickName: null,
        EmployeeName: null,
        CivilStatus: null,
        EmpType: null,
        SelectionStatus: null,
        ConfirmationStatus: null,
        NatureOfEmployement: null,
        TIN: null,
        FatherName: null,
        MotherName: null,
        CitizenID: null,
        ReligionID: null,
        CivilStatusID: null,
        ResignationID: null,
        SpouseName: null,
        SpouseNationalID: null,
        SpouseOccupation: null,
        NoOfChildren: null,
        PresentAddress1: null,
        PresentAddress2: null,
        ParmanentAddress1: null,
        ParmanentAddress2: null,
        PresThanaID: null,
        ParmThanaID: null,
        PresPostOfficeID: null,
        ParmPostOfficeID: null,
        PresZipCode: null,
        ParmZipCode: null,
        PresDistrictID: null,
        ParmDistrictID: null,
        PresCityID: null,
        ParmCityID: null,
        PresCountryID: null,
        EmrCntPer1Name: null,
        EmrCntPer2Name: null,
        EmrCntPer1CellNo: null,
        EmrCntPer2CellNo: null,
        SubmitDateTime: null,
        SelectionDateTime: null,
        SelectedBy: null,
        Submitted: false,
        ReadyForCandidateAccess: false,
        GivenDesignationId: null,
        LegalDesignationId: null,
        Designation: null,
        Completed: false,
        ConfirmAfterDays: null
    };

    //cboService.getCboPlantByCompany(null, function (result) {
    //    $scope.plantList = result;
    //});

    cboService.getEnumCbo('enum/getconfirmationstatus', function (result) {
        $scope.confirmationStatusList = result;
    });

    $scope.showEntityPopUp = function () {
        $http.get('employees/recruitmentapproval/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    };

    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.Id) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].Id === data.Id) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, 'failure');
        }
    };

    function checkExistTempList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }

    function getSelected(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return list[i].ConfirmationStatus;
            }
        }
        return null;
    }

    function getlegal(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return list[i].LegalDesignationId;
            }
        }
        return null;
    }

    function isDesignationExists(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (id === list[i].Value) {
                return true;
            }
        }
        return false;
    }

    $scope.confirmParameters = {
        limit: 50,
        offset: 0,
        order: 'DESC',
        sort: 'FullName',
        searchBy: "FullName",
        pageSize: 50,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $rootScope.searchDataByList = [
        {
            'name': 'Candidate Id',
            'value': 'Id'
        },
        {
            'name': 'Full Name',
            'value': 'FullName'
        },
        {
            'name': 'Budget Id',
            'value': 'BudgetId'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Position',
            'value': 'PositionName'
        }
    ];

    $scope.preRecruitmentEmployees = [];
    $scope.message = '';

    $scope.getListUrl = 'employees/recruitmentapproval/getlist',
        baseService.init($scope.getListUrl, null, 50, null, 'FullName', 'FullName');
    $scope.LoadData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (data) {
                if (data.Error) {
                    return $scope.message = data.Message;
                } else {
                    $scope.preRecruitmentEmployees = data.Data.Rows;
                    //$rootScope.total_count = data.Data.Total;
                    $scope.confirmParameters.total_count = data.Data.Total;
                    $scope.message = data.Message;
                }
                legalDesignation($scope.preRecruitmentEmployees);
                //if ($scope.preRecruitmentEmployees.Active) {
                for (var i = 0; i < $scope.preRecruitmentEmployees.length; i++) {
                    $scope.preRecruitmentEmployees[i].Active = getActive($scope.tempList, $scope.preRecruitmentEmployees[i].Id);
                    $scope.preRecruitmentEmployees[i].ConfirmationStatus = getSelected($scope.tempList, $scope.preRecruitmentEmployees[i].Id)
                    $scope.preRecruitmentEmployees[i].LegalDesignationId = getlegal($scope.tempList, $scope.preRecruitmentEmployees[i].Id)

                    $scope.preRecruitmentEmployees[i].ConfirmationStatus = "Selected";
                }
                //}
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.LoadData();

    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.preRecruitmentEmployees.length; i++) {
            $scope.preRecruitmentEmployees[i].Active = _isselected;
        }

        for (var i = 0; i < baseService.arrayLength($scope.preRecruitmentEmployees); i++) {
            if (_isselected)
                $scope.tempList.push($scope.preRecruitmentEmployees[i]);
            else
                for (var j = 0; j < $scope.tempList.length; j++) {
                    if ($scope.tempList[j].Id === $scope.preRecruitmentEmployees[i].Id) {
                        $scope.tempList.splice(j, 1);
                        break;
                    }
                }
        }
    };

    //$scope.CheckDesignation= function () {
    //    try {
    //        for (var i = 0; i < $scope.tempList.length; i++) {
    //            if ($scope.tempList[i].Active) {
    //                if (isDesignationExists($scope.tempList[i].DesignationId, $scope.givenDesignationList)) {
    //                    $scope.tempList[i].GivenDesignationId = $scope.tempList[i].DesignationId;
    //                }
    //                else {
    //                    $scope.tempList[i].GivenDesignationId = null;
    //                    throw 'Given Designation  is not found in Designation for Employee Id : ' + $scope.tempList[i].Id + ' !!!';
    //                }
    //            }
    //        }
    //    } catch (e) {
    //        throw e;
    //    }
    //}

    cboService.getCboGivenDesignation(function (result) {
        $scope.givenDesignationList = result;
    });

    $scope.legalDesignationList = [];
    function legalDesignation(dataList) {
        cboService.getCboLegalDesignation(null, function (result) {
            $scope.legalDesignationList = result;
            try {
                for (var i = 0; i < dataList.length; i++) {
                    var DGid = dataList[i].DesignationGroupId;
                    var fld = getLD(DGid, $scope.legalDesignationList);
                    dataList[i].legalDesignationList = fld;
                }
            } catch (e) {
                ShowResult(e, 'failure');
            }
        });
    }

    function getLD(Id, legalDesignationList) {
        var NewList = [];
        for (var i = 0; i < baseService.arrayLength(legalDesignationList); i++) {
            if (Id === legalDesignationList[i].DesignationGroupId) {
                NewList.push(legalDesignationList[i]);
            }
        }
        return NewList;
    }

    $scope.RefreshBody = function () {
        $scope.LoadData($scope.preRecruitmentEmployee.PlantId);
    };

    $scope.pushValue = function (data) {
        angular.forEach($scope.tempList, function (item, i) {
            if (item.Id === data.Id) {
                $scope.tempList[i].ConfirmationStatus = data.ConfirmationStatus;
            }
        });
    };

    $scope.Validation = function (list) {
        try {
            var doj = $filter('dateFiltering')(new Date(), 'dd-MM-yyyy');
            // $scope.CheckDesignation();
            for (var i = 0; i < list.length; i++) {
                if (list[i].Active) {
                    if (!baseService.isUndefinedOrNull(list[i].EmployeeCode)) {
                        if (isNaN(list[i].EmployeeCode)) {
                            throw 'Employee code must be numaric value for Employee Id : ' + list[i].Id + '.';
                        }
                    }
                    if (baseService.isUndefinedOrNull(list[i].DOJ)) {
                        throw 'Date Of Join cann\'t be null for Employee Id : ' + list[i].Id + '.';
                    }
                    if (baseService.isUndefinedOrNull(list[i].ConfirmAfterDays)) {
                        throw 'Confirm After Days cann\'t be null for Employee Id : ' + list[i].Id + '.';
                    }
                    if (list[i].ConfirmAfterDays < 0) {
                        throw 'Confirm After Days cann\'t be less than 0 for Employee Id : ' + list[i].Id + '.';
                    }

                    //if (list[i].DOJ === '') {
                    //    throw 'Date Of Join cann't be empty for Employee Id : ' + list[i].Id + ' !!!';
                    //}

                    //if (list[i].DOJ.substring(2, 3) != '-' || list[i].DOJ.substring(6, 7) != '-') {
                    //    throw 'Invalid format of 'Date Of Join' for Employee Id : ' + list[i].Id + ' !!!';
                    //}

                    //if (list[i].DOJ.length != 11) {
                    //    throw 'Invalid format of 'Date Of Join' for Employee Id : ' + list[i].Id + ' !!!';
                    //}

                    if (new Date(list[i].DOJ) > new Date(doj)) {
                        throw 'Date Of Join cann\'t be greater than todays date for Employee Id : ' + list[i].Id + ' !!!';
                    }

                    if (baseService.isUndefinedOrNull(list[i].ConfirmationStatus)) {
                        throw 'Confirmation Status cann\'t be null for Employee Id : ' + list[i].Id + '.';
                    }

                    if (baseService.arrayLength(list[i].legalDesignationList) > 0 && baseService.isUndefinedOrNull(list[i].LegalDesignationId)) {
                        throw 'Legal Designation cann\'t be null for Employee Id : ' + list[i].Id + '.';
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    };

    // #region Save
    $scope.Save = function () {
        try {
            $scope.Validation($scope.tempList);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'preRecruitmentEmployees': $scope.tempList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.LoadData($scope.preRecruitmentEmployee.PlantId);
                        $scope.tempList = [];
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    // #endregion
}