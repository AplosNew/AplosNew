'use strict';
resignationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function resignationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Resignation';
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.filedata = '';
    $scope.message = null;
    $scope.imageSrc = null;
    $scope.Action = 'Save';
    $scope.maxDate = new Date().toDateString();
    $scope.SeparationTypeList = [];
    $scope.Resignation = {
        Id: null,
        ResignationDate: null,
        Reason: null,
        /*ResignationTypeId: null,*/
        Image: null,
        imageSrc: null,
        AttachLetter: null,
        ApprovedDate: null,
        EffectiveDate: null,
        ApprovedEffectiveDate: null,
        Remarks: null,
        EmployeeId: null,
        PlantId: null,
        CompanyId: null,
        EmployeeName: null,
        EmployeeCode: null,
        Designation: null,
        Picture: null,
        GivenDesignation: null,
        EmployeeCategory: null,
        DOJ: null,
        DOC: null,
        IsPastResignationAllowed: false,
        PastResignationDaysAllowed: null,
        EmpPicPath: null,
        ApprovalStatus: null,
        Entity: null,
        SeparationTypeId:null
    };

    $scope.appliedList = [];
    $scope.getPendingistData = function () {
        try {
            $scope.Url = 'employees/resignationapprovalmultiple/MultipleResignationAppliedList';
            $scope.LoadList = function (pageno) {

                $http({
                    method: 'GET',
                    url: $scope.Url,
                    params: {},
                    dataType: 'JSON'
                })
                    .then(function (response) {
                        $scope.appliedList = response.data;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.LoadList();
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.getPendingistData();


    $scope.Get = function (args) {
        $scope.Resignation = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getSeparationType = function () {
        $http({
            method: 'Get',
            url: 'employees/resignationapprovalmultiple/GetSeparationType/'

        }).then(function successCallback(response) {
            if (response.data.Error === true) {

                ShowResult(response.data.Message, "failure");
            }
            else {
                $scope.SeparationTypeList = response.data;

            }
        });
    }
    $scope.getSeparationType();
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });

    function setUserImage(data) {
        if (!baseService.isUndefinedOrNull(data.SystemId)) {
            //$scope.imageSrc = $rootScope.HRMSImage + data.EmpPicPath;
            //$scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;
            $scope.imageBtnDisable = true;
            $scope.employee.EmpPicPath = data.EmpPicPath;
        }
        else {
            $scope.imageBtnDisable = false;
            $scope.employee.EmpPicPath = null;
        }
    }

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };   

    $scope.Save = function () {
        try {
            $scope.url = 'employees/resignation/create';
            if ($scope.Action == 'Update') {
                $scope.url = 'employees/resignation/edit';
            }
            else {
                $scope.url = 'employees/resignation/create';
            }
            Validate();
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            //$scope.Resignation.AttachLetter = fileName;

            $scope.savedisable = true;

            var formData = new FormData();
            $http({
                method: 'POST',
                url: $scope.url,
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append('Resignation', angular.toJson(data.Resignation));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'Resignation': $scope.Resignation, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getPendingistData();

                    $scope.Clear();
                    $scope.filedata = {};
                    document.getElementById('abc').value = ''
                    $scope.savedisable = false;
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
                ShowResult(response.status.Message, 'failure');
            });
            $scope.savedisable = false;
            return true;
        } catch (e) {
            $scope.savedisable = false;

            ShowResult(e, 'failure');
        }
    };

    $scope.loadNewEmployee = function () {
        $scope.excluedEmpColumn = ['Email', 'Reason', 'position', 'ResignationDate', 'AttachLetter', 'ApprovalStatus', 'EffectiveDate', 'Picture', 'IsPastResignationAllowed', 'PastResignationDaysAllowed', 'EmployeeCategory'];
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeCode',
            searchBy: 'EmployeeCode',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUpUrl = 'employees/resignation/newList?plantId=' + $scope.Resignation.PlantId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                   
                    if (baseService.arrayLength($scope.popUpList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId1');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId1')).modal('show');
        $scope.getPopUpData();
    };
    $scope.loadPendingEmployee = function () {
        $scope.excluedEmpColumn = ['Email', 'Reason', 'position', 'ResignationDate', 'AttachLetter', 'ApprovalStatus', 'EffectiveDate', 'Picture', 'IsPastResignationAllowed', 'PastResignationDaysAllowed', 'EmployeeCategory'];
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeCode',
            searchBy: 'EmployeeCode',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUpUrl = 'employees/resignation/pendingList?plantId=' + $scope.Resignation.PlantId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.popUpList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId2');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId2')).modal('show');
        $scope.getPopUpData();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId1')).modal('hide');
    };
    $scope.closePopUp2 = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId2')).modal('hide');
    };

    function selectNewEmployee(data) {
        $scope.Resignation.Id = data.Id;
        $scope.imageSrc = virtualPath.EmployeePic + data.Picture;
        $scope.Resignation.EmployeeId = data.EmployeeId;
        $scope.Resignation.EmployeeName = data.EmployeeName;
        $scope.Resignation.EmployeeCode = data.EmployeeCode;
        $scope.Resignation.GivenDesignation = data.GivenDesignation;
        $scope.Resignation.Designation = data.Designation;
        $scope.Resignation.DOJ = data.DOJ;
        $scope.Resignation.DOC = data.DOC;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.PlantId = data.PlantId;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.Id = data.Id;
        $scope.Resignation.IsPastResignationAllowed = data.IsPastResignationAllowed;
        $scope.Resignation.PastResignationDaysAllowed = data.PastResignationDaysAllowed;
        $scope.Resignation.Entity = data.Entity;
        $scope.Resignation.AttachLetter = data.AttachLetter;
        $scope.closePopUp();
        $scope.Action = 'Save';
    }
    function selectPendingEmployee(data) {
        $scope.Resignation.Id = data.Id;
        $scope.imageSrc = virtualPath.EmployeePic + data.Picture;
        $scope.Resignation.EmployeeId = data.EmployeeId;
        $scope.Resignation.EmployeeName = data.EmployeeName;
        $scope.Resignation.EmployeeCode = data.EmployeeCode;
        $scope.Resignation.GivenDesignation = data.GivenDesignation;
        $scope.Resignation.Designation = data.Designation;
        $scope.Resignation.DOJ = data.DOJ;
        $scope.Resignation.DOC = data.DOC;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.PlantId = data.PlantId;
        $scope.Resignation.ResignationDate = data.ResignationDate;
        $scope.Resignation.EffectiveDate = data.EffectiveDate;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.Reason = data.Reason;
        $scope.Resignation.Id = data.Id;
        $scope.Resignation.IsPastResignationAllowed = data.IsPastResignationAllowed;
        $scope.Resignation.PastResignationDaysAllowed = data.PastResignationDaysAllowed;
        $scope.Resignation.Entity = data.Entity;
        $scope.Resignation.AttachLetter = data.AttachLetter;
        document.getElementById('abc').value = data.AttachLetter;
        $scope.closePopUp2();
        $scope.Action = 'Update';
    }
    $scope.loadResignationHistory = function (Id) {
        $http.get('employees/resignation/getResignationHistoryById?EmployeeId=' + $scope.Resignation.EmployeeId)
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#ResignationHistoryPopUp')).modal('show');
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }
    function Validate() {
        try {
            CheckField($scope.Resignation.PlantId, 'Plant');
            CheckField($scope.Resignation.SeparationTypeId, 'Separation Type');
            CheckField($scope.Resignation.EmployeeName, 'Employee Name');
            CheckField($scope.Resignation.ResignationDate, 'Resignation Submission Date');
            CheckField($scope.Resignation.EffectiveDate, 'Applied Effective Date');
            CheckField($scope.Resignation.Reason, 'Reason');
            CheckField($scope.Resignation.AttachLetter, 'Resignation Letter');
            var regDate = new Date($scope.Resignation.ResignationDate);
            var effDate = new Date($scope.Resignation.EffectiveDate);
            var dojDate = new Date($scope.Resignation.DOJ);

            if (dojDate > regDate) {
                throw 'Resignation date must be greater than Date of Join'
            }
            if (regDate > effDate) {
                throw 'Applied Effective date cannot be less than Resignation date'
            }

            var d = new Date();
            var d1 = $filter('date')(d, 'dd-MMM-yy');
            var d3 = $filter('date')(regDate, 'dd-MMM-yy');
            var resignationDate = new Date(d3);
            var today = new Date(d1);
            if (resignationDate > today) {
                throw 'Future Resignation date is not allowed';
            }

            var effDate2 = $filter('date')(effDate, 'dd-MMM-yy')
            var effectiveDate = new Date(effDate2);

            d.setDate(d.getDate() + 90);
            var d1 = $filter('date')(d, 'dd-MMM-yy');
            var d2 = new Date(d1);
            if (effDate > d2) {
                throw 'Applied Effective Date Cannot be Greater then [' + d1 + ']'
            }

            var allowDays = new Date();
            allowDays.setDate(d.getDate() - $scope.Resignation.PastResignationDaysAllowed);
            var d7 = $filter('date')(allowDays, 'dd-MMM-yy');
            var d8 = new Date(d7);
            if ($scope.Resignation.IsPastResignationAllowed === true) {
                if (d8 > regDate) {
                    throw 'Past Resignation date before [' + d7 + '] Days is not allowed';
                }
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.showSearch = function (flag) {
        try {
            $scope.search_flag = flag;
            switch (flag) {
                case 'PendingEMP':
                    CheckField($scope.Resignation.PlantId, 'Plant');
                    $scope.loadPendingEmployee();
                    break;
                case 'NewEMP':
                    CheckField($scope.Resignation.PlantId, 'Plant');
                    $scope.loadNewEmployee();
                    break;
                default:
                    return ShowResult('Search Flag is not defined!!!', 'failure');
            }
            //angular.element(document.querySelector('#popUpId')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.getSearchObject = function (ob) {
        try {
            switch ($scope.search_flag) {
                case 'PendingEMP':
                    selectPendingEmployee(ob);
                    break;
                case 'NewEMP':
                    selectNewEmployee(ob);
                    break;
                default:
            }
            $scope.search_flag = '';
            //angular.element(document.querySelector('#search_popup')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearOb($scope.Resignation);
        $scope.Action = 'Save';
        $scope.filedata = null;
        document.getElementById('abc').value = null;
        $scope.Resignation.AttachLetter = null;
        $scope.imageSrc = virtualPath.EmployeePic + '';
    };
    function ClearOb(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }
    $('#uploadBtn').change(function () {
        $scope.filedata = this.files[0];
        $scope.Resignation.AttachLetter = null;
        $scope.Resignation.AttachLetter = $scope.filedata.name;
        document.getElementById('abc').value = $scope.filedata.name;
    });
    $scope.AttachRemove = function () {
        // $scope.message_confirmation = 'Are you sure to remove this file?';
        // angular.element(document.querySelector('#confirmDelete')).modal('show');
        $scope.filedata = [];
        document.getElementById('uploadBtn').value = null;
        $scope.Resignation.AttachLetter = null;
        document.getElementById('abc').value = '';
    };

    $scope.LeaveTest = function () {
        try {
            $http({
                method: 'POST',
                url: 'employees/resignation/leaveSummary?CompanyGroupId=CG20181',
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ResignationTypeList = [];
    $scope.GetResignationType = function () {
        $http({
            method: 'GET',
            url: 'employees/resignation/GetResignationType',
            dataType: 'JSON'
        }).then(function succ(resp) {

            $scope.ResignationTypeList = resp.data;
        });
    }
    $scope.GetResignationType();
}