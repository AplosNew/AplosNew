'use strict';
guestUserController.$inject = ['cboService', 'fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function guestUserController(cboService, fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'GuestUser';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.models = [];
    $scope.path = 'employees/GuestUser/';
    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';


    //$scope.SelectionParameters = {
    //    limit: 100,
    //    offset: 0,
    //    order: 'ASC',
    //    sort: 'EmployeeName',
    //    searchBy: "EmployeeName",
    //    pageSize: 100,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};

    //$rootScope.searchDataByList = [
    //    {
    //        'name': 'Name',
    //        'value': 'EmployeeName'
    //    },
    //    {
    //        'name': 'Email',
    //        'value': 'Email'
    //    },
    //    {
    //        'name': 'Division',
    //        'value': 'Division'
    //    },
    //    {
    //        'name': 'Department',
    //        'value': 'Department'
    //    },
    //    {
    //        'name': 'Section',
    //        'value': 'Section'
    //    },
    //    {
    //        'name': 'SubSection',
    //        'value': 'SubSection'
    //    },
    //    {
    //        'name': 'Designation',
    //        'value': 'Designation'
    //    }
    //];


   
    $scope.getData = function () {
        $http({
            method: 'Get',
            url: 'employees/GuestUser/GetList'
        }).then(function (response) {
            $scope.models = response.data;
        });
    };
    $scope.getData();

    $scope.employeeInformation = {
        SystemId: null,
        GroupID: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        EmployeeName: null,
        NickName: null,
        EmpPicPath: null,
        EmpType: null,
        GenderID: null,
        LegalDesignationId: null,
        IsAccessible: true,
        EmailId: null,
        TentativeExpiryDate:null
    };

    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    console.log('$scope.popUpDataList', $scope.popUpDataList);
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.selectDoubleClick = function (data) {
        $scope.employeeInformation.BudgetId = data.Id;
        $scope.employeeInformation.Code = data.Code;
        $scope.employeeInformation.EntityName = data.EntityName;
        $scope.employeeInformation.Designation = data.Designation;
        $scope.employeeInformation.PositionName = data.PositionName;
        $scope.employeeInformation.DesignationId = data.DesignationId;

        $scope.employeeInformation.GivenDesignationId = null;

        cboService.getCboLowerGivenDesignation($scope.employeeInformation.DesignationId, function (result) {
            $scope.givenDesignationList = result;
            $scope.employeeInformation.GivenDesignationId = $scope.employeeInformation.DesignationId;
            //modelNew.GivenDesignationId
        });

        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.clearCode = function () {
        $scope.employeeInformation.BudgetId = null;
        $scope.employeeInformation.EntityName = null;
        $scope.employeeInformation.Designation = null;
        $scope.employeeInformation.PositionName = null;
        $scope.employeeInformation.GivenDesignationId = null;
    };

    $scope.divisionList = [];
    cboService.getCboDivisionByCompanyGroup(null, function (result) {
        $scope.divisionList = result;
    });

    $scope.LegalDesignationList = [];

    $http.get('Employees/GuestUser/GetAllLegalDesignationCbo')
        .then(
            function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.LegalDesignationList = response.data;
                }
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });

    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });

    $scope.sectionList = [];
    $scope.changeSectionByDept = function () {
        cboService.getSectionCboByDepartmentId($scope.employeeInformation.DepartmentId, function (result) {
            $scope.sectionList = result;
        });
    }

    $scope.subSectionList = [];
    $scope.changeSubSectionBySection = function () {
        cboService.getSubSectionCboBySectionId($scope.employeeInformation.SectionId, function (result) {
            $scope.subSectionList = result;
        });
    }

    $scope.Get = function (obj) {
        $scope.employeeInformation = Object.assign({}, obj.data);
        $scope.imageSrc = virtualPath.EmployeePic + $scope.employeeInformation.EmpPicPath;
        $scope.changeSectionByDept();
        $scope.changeSubSectionBySection();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.picdata = null;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };


    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.employeeInformationForm.$valid) {
                var picData = new FormData();

                $http({
                    method: 'POST',
                    url: 'employees/guestuser/create',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("employeeInformation", angular.toJson(data.employeeInformation));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'employeeInformation': $scope.employeeInformation
                        , 'file': $scope.picdata
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.employeeInformation.SystemId = response.data.EmployeeInformation.SystemId;
                        $scope.getData();
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.message_detailconfirmation = null;
    $scope.confirmDelete = function (obj) {

        $scope.employeeInformation = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.employeeInformation.SystemId))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.employeeInformation.EmployeeName + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.employeeInformation.SystemId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.employeeInformation.SystemId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.employeeInformation = { IsAccessible: true };
        $scope.picData = null;
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    }
}