'use strict';
resignationRecruitmentPlanningController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function resignationRecruitmentPlanningController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Resignation Recruitment Planning';
    $scope.message = '';
    $scope.tempList = [];

    $scope.resignationRecruitmentPlanning = {
        Id: null,
        PlantId: null,
        CompanyId: null,
        CompanyGroupId: null,
        ManpowerBudgetId: null,
        RecruitmentGroupId: null,
        RecruitmentPlanningId: null,
        OnBoardDate: null,
        PlanningType: null,
        Male: null,
        Female: null,
        TotalManpower: null,
        Remarks: null,
        Active: null,
        Archive: null
    };

    $scope.RecruitmentPlanningProcessSet = {
        Id: null,
        RecruitmentPlanningDetailId: null,
        RecruitmentProcessId: null,
        EmployeeId: null,
        Sequence: null,
        TargetDate: null
    };

    $scope.SelectionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'EmployeeName',
        searchBy: "EmployeeName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.searchByList = [
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Position',
            'value': 'position'
        },
        {
            'name': 'Budget Code',
            'value': 'BudgetCode'
        },
        {
            'name': 'Resignation Date',
            'value': 'ResignationDate'
        }

    ];

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getCboPlantByCompany = function (companyId) {
        cboService.getCboPlantByCompany(companyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.getListUrl1 = 'employees/resignationRecruitmentPlanning/getListRecPlanning';
    $scope.getSequence = function (pageno) {
        try {
            $scope.searchList = [];
            console.log($scope.tempList);
            baseService.init($scope.getListUrl1 + '?companyId=' + $scope.resignationRecruitmentPlanning.CompanyId + "&plantId=" + $scope.resignationRecruitmentPlanning.PlantId, null, null, null, 'EmployeeName', 'EmployeeName');
            $scope.LoadDataList = function (pageno) {
                baseService.pagination(pageno)
                    .then(function (data) {
                        if (data.Error) return $scope.message = data.Message;
                        $scope.resignationRecruitmentPlannings = data.Rows;
                        $scope.SelectionParameters.total_count = data.Total;
                        //$scope.message = data.Message === '' ? null : data.Message;
                        for (var i = 0; i < $scope.resignationRecruitmentPlannings.length; i++) {
                            $scope.resignationRecruitmentPlannings[i].Active = cacheActiveValue($scope.tempList, $scope.resignationRecruitmentPlannings[i].EmployeeId);
                            //if (baseService.arrayLength($scope.searchList) === 0)
                            //baseService.getDDLSearchColumn(data.Data.Rows, $scope.searchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.LoadDataList();
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.getListUrl2 = 'employees/resignationRecruitmentPlanning/getListRecPlanningByEmpId';
    $scope.loadRecruitmentPopUp = function (list, index) {
        $scope.getPopUpData = function (list) {
            cboService.getSequence($scope.getListUrl2 + '?companyId=' + $scope.resignationRecruitmentPlanning.CompanyId + "&plantId=" + list.PlantId + "&empId=" + list.EmployeeId,
                function (result) {
                    $scope.resignationRecruitmentPlannings1 = result.Rows;
                    //$scope.message = result.Message;

                    var onBoardDate = '';
                    var finisedDate1 = '';
                    if ($scope.resignationRecruitmentPlannings1.length > 1) {
                        for (var i = 0; i < $scope.resignationRecruitmentPlannings1.length; i++) {
                            if (i === 0) {
                                onBoardDate = $scope.resignationRecruitmentPlannings1[i].DOSep;
                            }
                            else {
                                onBoardDate = finisedDate1;
                            }
                            var requiredDays = $scope.resignationRecruitmentPlannings1[i].RequiredDays;
                            var d = new Date(onBoardDate);
                            d.setDate(d.getDate() + requiredDays);
                            var d1 = $filter('date')(d, 'dd-MMM-yy');
                            finisedDate1 = new Date(d1);
                            $scope.resignationRecruitmentPlannings1[i]['finisedDate1'] = d1;
                        }
                    }
                });
        };
        $scope.getPopUpData(list);
        console.log($scope.resignationRecruitmentPlannings1);
        angular.element(document.querySelector('#recruitmentPopUp')).modal('show');
    };

    $scope.closeFinalRemarkPopUp = function () {
        angular.element(document.querySelector('#recruitmentPopUp')).modal('hide');
    };

    $scope.Save = function () {
        try {
            Validate($scope.tempList);
            $scope.savedisable = true;
            $scope.RecruitmentPlanningProcessSet = [];
            console.log('1', $scope.recruitmentSelections);
            for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                var ob = $scope.tempList[i];
                $scope.RecruitmentPlanningProcessSet.push(ob);
            }

            $http({
                method: 'POST',
                url: 'employees/resignationRecruitmentPlanning/create',
                data: { 'RecruitmentPlanningProcessSet': $scope.RecruitmentPlanningProcessSet }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;

                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.tempList = [];
                    $scope.getSequence();
                    ShowResult(response.data.Message, "success");
                    $scope.savedisable = false;
                    //$scope.Clear();
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
                ShowResult(response.status.Message, "failure");
            });
            $scope.savedisable = false;
            return true;
        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    }

    function cacheActiveValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return true;
            }
        }
        return false;
    }

    function checkExistTempList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    function Validate(list) {
        try {
            var count = 0;
            for (var i = 0; i < baseService.arrayLength(list); i++) {
                if (list[i].Active) {
                    count++;
                    var ob = list[i];
                }//active
                else {
                    count++;
                    var ob = list[i];
                }
            }//for
            if (count === 0) {
                throw 'Please select an Employee'
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.EmployeeId) === false) {
                    $scope.tempList.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                        if ($scope.tempList[i].EmployeeId === data.EmployeeId) {
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }

                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                    if ($scope.tempList[i].EmployeeId === data.EmployeeId) {
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

    $scope.getDateColor = function (item) {
        var processFinishedDate = new Date(item.finisedDate1);
        var toDay = new Date();

        if ($filter('dateFiltering')(processFinishedDate, 'dd-MMM-yyyy') === $filter('dateFiltering')(toDay, 'dd-MMM-yyyy')) {
            return 'current';
        } else if (processFinishedDate < toDay) {
            return 'old';
        } else if (processFinishedDate > toDay) {
            return 'pending';
        }
    };

    $scope.showEntity = function () {
        $http.get('employees/resignationRecruitmentPlanning/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    };

    $scope.roleWiseMessage = function () {
        $http.get('employees/resignationRecruitmentPlanning/getEntity')
            .then(function successCallback(response) {
                if (!baseService.isUndefinedOrNull(response.data.Message)) {
                    $scope.message = response.data.Message;
                }
                else {
                    $scope.message = response.data;
                }
            }
            ), function errorCallBack(response) {
                showResult(response.data.Message, 'failure');
            }
    }
    $scope.roleWiseMessage();

    //$scope.getSequence = function (pageno) {
    //    $scope.searchList = [];
    //    baseService.init($scope.getListUrl1 + '?companyId=' + $scope.resignationRecruitmentPlanning.CompanyId + "&plantId=" + $scope.resignationRecruitmentPlanning.PlantId, null, null, null, 'EmployeeName', 'EmployeeName');
    //    baseService.pagination(pageno)
    //        .then(function (data) {
    //            if (data.Error) return $scope.message = data.Message;
    //            $scope.resignationRecruitmentPlannings = data.Data.Rows;
    //            $scope.SelectionParameters.total_count = data.Data.Total;
    //            $scope.message = data.Message === '' ? null : data.Message;
    //            for (var i = 0; i < $scope.resignationRecruitmentPlannings.length; i++) {
    //                $scope.resignationRecruitmentPlannings[i].Active = cacheActiveValue($scope.tempList, $scope.resignationRecruitmentPlannings[i].EmployeeId);
    //                if (baseService.arrayLength($scope.searchList) === 0)
    //                    baseService.getDDLSearchColumn(result.Data.Rows, $scope.searchList);
    //            }

    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};

    //$scope.getSequence = function (plantId) {
    //    cboService.getSequence($scope.getListUrl1 + '?companyId=' + $scope.resignationRecruitmentPlanning.CompanyId + "&plantId=" + plantId,
    //        function (result) {
    //            $scope.resignationRecruitmentPlannings = result.Data.Rows;
    //            $scope.message = result.Message;
    //            console.log($scope.resignationRecruitmentPlannings);
    //    });
    //};

    //$scope.getPopUpData = function (list) {
    //    cboService.getSequence($scope.getListUrl2 + '?companyId=' + $scope.resignationRecruitmentPlanning.CompanyId + "&plantId=" + list.PlantId + "&empId=" + list.EmployeeId,
    //        function (result) {
    //            $scope.resignationRecruitmentPlannings1 = result.Data.Rows;
    //            $scope.message = result.Message;

    //            var onBoardDate = '';
    //            var finisedDate1 = '';
    //            if ($scope.resignationRecruitmentPlannings1.length > 1) {
    //                for (var i = 0; i < $scope.resignationRecruitmentPlannings1.length; i++) {
    //                    if (i == 0) {
    //                        onBoardDate = $scope.resignationRecruitmentPlannings1[i].OnBoardDate1;
    //                    }
    //                    else {
    //                        onBoardDate = finisedDate1;
    //                    }
    //                    var requiredDays = $scope.resignationRecruitmentPlannings1[i].RequiredDays;
    //                    var d = new Date(onBoardDate);
    //                    d.setDate(d.getDate() + requiredDays);
    //                    var d1 = $filter('date')(d, 'dd-MMM-yy');
    //                    finisedDate1 = new Date(d1);
    //                    $scope.resignationRecruitmentPlannings1[i]['finisedDate1'] = d1;

    //                }
    //            }
    //        });
    //};
}