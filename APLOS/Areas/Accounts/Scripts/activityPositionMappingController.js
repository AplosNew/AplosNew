"use strict";
activityPositionMappingController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http"];
function activityPositionMappingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http) {
    $rootScope.title = "Activity Position Mapping";
    $scope.Action = "Save";
    $scope.tableShow = false;
    $scope.responsiblePersonList = [];
    $scope.getListUrl = "employees/responsibleperson/getactivitypositionmappinglist";
    baseService.init($scope.getListUrl, null, null, null, "ActivityName");
    $scope.getData = function (pageno) {
        $scope.selectedPositionId = $scope.responsiblePerson.PositionId;
        $rootScope.parameters.PositionId = $scope.responsiblePerson.PositionId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.responsiblePersonList = result.Rows;
                if ($scope.responsiblePersonList.length > 0) {
                    $scope.tableShow = true;
                }
                else {
                    $scope.tableShow = false;
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $scope.responsiblePerson = {
        Id: null,
        PositionId: null,
        ManpowerBudgetId: null,
        EmployeeId: null,
        TaggingType: null,
        TaggingId: null,
        Remarks: null,
        Active: true,
        AddedDate: new Date(),
        UpdatedBy: null
    };

    $scope.positionList = [];
    cboService.getCboPositionByCompanyGroup(null, function (result) {
        $scope.positionList = result;
    });

    $scope.popUpParameters = {
        limit: 2,
        offset: 0,
        order: "asc",
        sort: "Code",
        searchBy: "UserName",
        pageSize: 2,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    // Popup modal
    $scope.popUpList = [];
    $scope.valueData = "";
    $scope.popUpParameters = {
        limit: 2,
        offset: 0,
        order: "asc",
        sort: "Code",
        searchBy: "UserName",
        pageSize: 2,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpUrl = "";
        $scope.getPopUpData = function (pageno) {
            $rootScope.popUpParameters.companyId = $scope.SelectedCompany;
            baseService.paginationBase("Organizations/Division/getlistdivisionwithcompnay", pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "popUpId");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#popUpId")).modal("show");
        $scope.getPopUpData();
    };

    //End DivisionList for modal
    //Passing Data For Department List
    $scope.DivisionSelectdCloseListPopUp = function () {
        angular.forEach($scope.divisionListWithCompanyWise, function (item) {
            if (item.Flag) {
                $scope.companyDivisionList.push(
                    {
                        Id: null,
                        CompanyId: $scope.companyDivision.CompanyId,
                        DivisionId: item.DivisionId,
                        Code: item.Code,
                        UserName: item.UserName,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector("#divisionPopUp")).modal("hide");
        if ($scope.companyDivisionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //Save
    $scope.Save = function () {
        $scope.divisionSelectedList = [];
        if ($scope.companyDivisionList.length > 0) {
            angular.forEach($scope.companyDivisionList, function (item) {
                if (item.Flag) {
                    $scope.divisionSelectedList.push(item);
                }
            });
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "Organizations/companydivision/create",
                    data: { "CompanyDivision": $scope.companyDivisionList },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getDivisionMasterOnCompanyChange($scope.companyDivision.CompanyId);
                    }
                });
                return true;
            }
        } else {
            ShowResult("You have not selected any Division.", "failure");
        }
        return true;
    };

    //Deleting Rows from CompanyDepartmentList
    $scope.valuePassInDelModal = function (index, DivisionId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.DivisionId = DivisionId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = "Are you sure want to delete this data....";
        else
            $scope.message_confirmation = "Are you sure want to delete [ " + id + " ]";
        angular.element(document.querySelector("#confirmgenericPopUp")).modal("show");
    };

    $scope.DeleteDivisionList = function () {
        for (var i = 0; i < $scope.companyDivisionList.length; i++) {
            if ($scope.companyDivisionList[i].Id === null && $scope.companyDivisionList[i].DivisionId === $scope.DivisionId) {
                $scope.companyDivisionList.splice($scope.index, 1);
            }
            else if ($scope.companyDivisionList[i].Id !== null && $scope.companyDivisionList[i].DivisionId === $scope.DivisionId)
                $scope.companyDivisionList[i].Archive = true;
        }
        if ($scope.companyDivisionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
        $scope.id = null;
        $scope.index = null;
        $scope.DivisionId = null;
    };
}