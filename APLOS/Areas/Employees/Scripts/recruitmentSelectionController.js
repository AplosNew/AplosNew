'use strict';
recruitmentSelectionController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function recruitmentSelectionController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Selection Employee';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'employees/recruitmentselection/';
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
        Completed: false,
        ReadyForCandidateAccess: false,
        BirthdayCelebrationDate: null,
        MarriagedayCelebrationDate: null,
        AgreedDOJ: null,
        TotalSalary: null,
        SpecialReviewDuration: null,
        SpecialReviewAmount: null,
        ExpiredDays: null
    };

    $scope.emailSetup = {
        CC: null
        , SenderName: null
        , SenderEmail: null
    };

    $scope.plantList = [];
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.plantList = result;
    });

    $scope.selectionstatusList = [];
    cboService.getEnumCbo('enum/getselectionstatus', function (result) {
        $scope.selectionstatusList = result;
        //$scope.preRecruitmentEmployee.SelectionStatus = $scope.selectionstatusList[1].Value;
    });

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
            ShowResult(e, "failure");
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
                return list[i].SelectionStatus;
            }
        }
        return null;
    }
    function getSelectedddd(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return list[i].ExpiredDays;
            }
        }
        return null;
    }
    $scope.SelectionParameters = {
        limit: 100,
        offset: 0,
        order: 'DESC',
        sort: 'AppAddedDateTime',
        searchBy: "FullName",
        pageSize: 100,
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
            'name': 'Email',
            'value': 'Email'
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

    $scope.LoadData = function () {
        try {
            $scope.GLUrl3 = 'employees/recruitmentselection/getlist?plantId=' + $scope.preRecruitmentEmployee.PlantId,
                $scope.LoadDataList = function (pageno) {
                    baseService.paginationBase($scope.GLUrl3, pageno, $scope.SelectionParameters)
                        .then(function (data) {
                            $scope.recruitmentSelections = data.Rows;
                            $scope.SelectionParameters.total_count = data.Total;

                            //if ($scope.recruitmentSelections.length === 0) {
                            //    ShowResult('No data found.', 'Error');
                            //    $scope.recruitmentSelections = data.Rows;
                            //}
                            $scope.SelectionParameters.search = null;
                            for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
                                $scope.recruitmentSelections[i].Active = getActive($scope.tempList, $scope.recruitmentSelections[i].Id);
                                //$scope.recruitmentSelections[i].SelectionStatus = getSelected($scope.tempList, $scope.recruitmentSelections[i].Id);
                                // $scope.recruitmentSelections[i].ExpiredDays = getSelectedddd($scope.tempInputValue, $scope.recruitmentSelections[i].Id);
                                $scope.recruitmentSelections[i].SelectionStatus = "Selected";
                            }
                        }, function () {
                            ShowResult(commonMessage.NetworkError, 'failure');
                        }).finally(function () {
                        });
                };
            $scope.LoadDataList();
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.SetExpiredDays = function () {
        for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
            if ($scope.recruitmentSelections[i].Active) {
                $scope.recruitmentSelections[i].ExpiredDays = preRecruitmentEmployee.ExpiredDays;
            }
        }
    };

    $scope.RefreshBody = function () {
        $scope.LoadData($scope.preRecruitmentEmployee.PlantId);
    };

    $scope.pushValue = function (data) {
        angular.forEach($scope.tempList, function (item, i) {
            if (item.Id === data.Id) {
                $scope.tempList[i].SelectionStatus = data.SelectionStatus;
            }
        });
    };
    $scope.tempInputValue = [];
    $scope.getInputValue = function () {
        for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
            //if (checkExistTempList($scope.tempList, data.Id) === false) {
            //    $scope.tempList.push(data);
            //}
            $scope.tempInputValue.push($scope.recruitmentSelections[i]);
        }
    };

    $scope.validation = function (list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Active) {
                if (baseService.isUndefinedOrNull(list[i].SelectionStatus)) {
                    throw "Status cann't be null.";
                }
            }
        }
        CheckField($scope.emailSetup.SenderName, "Sender Name");
        CheckField($scope.emailSetup.SenderEmail, "Sender Email");
        CheckField($scope.preRecruitmentEmployee.ExpiredDays, "Expired Days");
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required.';
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;

        //for (var i = 0; i < baseService.arrayLength($scope.recruitmentSelections); i++) {
        //    $scope.recruitmentSelections[i].Active = _isselected;
        //}
        for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
            $scope.recruitmentSelections[i].Active = _isselected;
        }

        for (var i = 0; i < baseService.arrayLength($scope.recruitmentSelections); i++) {
            if (_isselected)
                $scope.tempList.push($scope.recruitmentSelections[i]);
            else
                for (var j = 0; j < $scope.tempList.length; j++) {
                    if ($scope.tempList[j].Id === $scope.recruitmentSelections[i].Id) {
                        $scope.tempList.splice(j, 1);
                        break;
                    }
                }
        }
    };

    // #region Save
    $scope.Save = function () {
        try {
            for (var i = 0; i < $scope.tempList.length; i++) {
                if ($scope.tempList[i].Active) {
                    $scope.tempList[i].ExpiredDays = $scope.preRecruitmentEmployee.ExpiredDays;
                }
            }
            if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.emailSetup.SenderEmail)) {
                //valid
            } else {
                throw "You have entered an invalid email address.";
            }
            $scope.validation($scope.tempList);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'emailSetup': $scope.emailSetup,
                        'preRecruitmentEmployee': $scope.tempList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.LoadData($scope.preRecruitmentEmployee.PlantId);
                        $scope.tempList = [];
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    // #endregion
}