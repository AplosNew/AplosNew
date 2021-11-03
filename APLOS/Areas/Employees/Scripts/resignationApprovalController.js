'use strict';
resignationApprovalController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function resignationApprovalController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Resignation Approval';
    $scope.jobDescriptionCategories = [];
    $scope.path = 'employees/resignation-approval/';

    $scope.Resignation = {
        Id: null,
        ResignationDate: null,
        Reason: null,
        AttachLetter: null,
        ApprovedDate: null,
        EffectiveDate: null,
        ApprovedEffectiveDate: null,
        EmpSystemId: null,
        PlantId: null,
        CompanyId: null,
        EmployeeName: null,
        EmployeeCode: null,
        Designation: null,
        EmployeeCategory: null,
        DOJ: null,
        DOC: null,
        Remarks: null,
        IsApproved: false
    };

    cboService.getCboPlant(function (result) {
        $scope.PlantList = result;
    });

    $scope.ApprovalList = [];
    cboService.getEnumCbo("enum/GetApprovalStatusCbo", function (result) {
        $scope.ApprovalList = result;
    });

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });

    //document.getElementById("uploadBtn").onchange = function () {
    //    var filename = document.getElementById("uploadFile").value = this.value;
    //    var res = filename.replace(/C:\\fakepath\\/i, '');
    //    document.getElementById("uploadFile").value = res;
    //};

    $scope.Save = function () {
        //angular.copy($scope.resignationNew, $scope.resignation);
        try {
            Validate();

            $scope.savedisable = true;

            var formData = new FormData();

            $http({
                method: 'POST',
                url: 'employees/resignation/create',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("Resignation", angular.toJson(data.Resignation));
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
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");

                    $scope.savedisable = false;
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
    };

    $scope.edit = function () {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeName',
            searchBy: "EmployeeName",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };

        $scope.popUpUrl = 'employees/resignation/getlist';
        baseService.setCurrentPage('dataList');
        $scope.popUpList = [];
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.getPopUpData();
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.Resignation.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.Resignation.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Resignations.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.AttachRemove = function () {
        // $scope.message_confirmation = 'Are you sure to remove this file?';
        // angular.element(document.querySelector('#confirmDelete')).modal('show');
        $scope.Resignation.AttachLetter = null;
    };

    $scope.removeResignation = function () {
        angular.element(document.querySelector('#confirmDelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.Resignation.EmpSystemId)) {
            document.getElementById('uploadBtn').value = '';
            $scope.filedata = '';
            $scope.Resignation.FileName = "";
            $scope.filedata = {};
            document.getElementById('uploadFile').value = "";
        }
        else {
            $scope.ClearTraining();
        }
    };

    $scope.popUpList = [];
    $scope.valueData = '';

    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeName',
        searchBy: "EmployeeName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.loadEmployee = function () {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeName',
            searchBy: "EmployeeName",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };

        $scope.popUpUrl = 'employees/resignationapproval/getresignationlist?plantId=' + $scope.Resignation.PlantId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            $scope.popUpList = [];
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    //if (baseService.arrayLength($scope.popUpList) == 0) {
                    baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    //}
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        //angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    function selectDoubleClick(data) {
        console.log(data.Id);
        $scope.Resignation.EmpSystemId = data.EmpSystemId;
        $scope.Resignation.EmployeeName = data.EmployeeName;
        $scope.Resignation.EmployeeCode = data.EmployeeCode;
        $scope.Resignation.Designation = data.Designation;
        $scope.Resignation.DOJ = data.DOJ;
        $scope.Resignation.DOC = data.DOC;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.PlantId = data.PlantId;
        $scope.Resignation.ResignationDate = data.ResignationDate;
        $scope.Resignation.EffectiveDate = data.EffectiveDate;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.Reason = data.Reason;
        $scope.Resignation.AttachLetter = data.AttachLetter;
        $scope.Resignation.Id = data.Id;
        $scope.Resignation.IsApproved = data.IsApproved;
        $scope.Resignation.Remarks = data.Remarks;
        $scope.Resignation.ApprovedEffectiveDate = data.ApprovedEffectiveDate;
        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
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
            CheckField($scope.Resignation.Remarks, "Remarks");
            if ($scope.Resignation.IsApproved) {
                CheckField($scope.Resignation.ApprovedEffectiveDate, "Approved Effective Date");
                var regdate = new Date($scope.Resignation.ResignationDate);
                var appeffdate = new Date($scope.Resignation.ApprovedEffectiveDate);

                //var _rd = $filter('dateFiltering')(RD, 'dd-MMM-yy');
                //var _ad = $filter('dateFiltering')(AD, 'dd-MMM-yy');
                if (regdate > appeffdate) {
                    throw 'Resignation date must be greater than Applied Effective date';
                }
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.Clear = function () {
        ClearOb($scope.Resignation);
    };

    function ClearOb(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }

    $scope.showSearch = function (flag) {
        try {
            $scope.search_flag = flag;
            switch (flag) {
                case 'REG':
                    $scope.edit();
                    break;
                case 'EMP':
                    CheckField($scope.Resignation.PlantId, "Plant");
                    $scope.loadEmployee();
                    break;
                default:
                    return ShowResult("Search Flag is not defined!!!", 'failure');
            }
            angular.element(document.querySelector('#search_popup')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getSearchObject = function (ob) {
        try {
            switch ($scope.search_flag) {
                case 'EMP':
                    selectDoubleClick(ob);
                    break;
                default:
                // $scope.getMaterialMasterSearchData();
            }
            $scope.search_flag = '';
            angular.element(document.querySelector('#search_popup')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.AttachLetter;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = $rootScope.ResignationLetter + data.Id + extention;
    };

    //$scope.resignationPopUp = function () {
    //    $scope.popUpUrl1 = 'employees/resignation/getlist';
    //    baseService.setCurrentPage('dataList');
    //    $scope.getPopUpData = function (pageno) {
    //        baseService.init($scope.popUpUrl1, null, null, null, 'EmpSystemId', 'EmpSystemId')
    //        //baseService.paginationBase($scope.popUpUrl1, pageno, $scope.resignationpopUpParameters)
    //            .then(function (result) {
    //                $scope.resignationpopUpDataList = result.Rows;
    //                $scope.resignationpopUpParameters.total_count = result.Total;
    //                if (baseService.arrayLength($scope.popUpList) == 0) {
    //                    baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure', 'resignationpopUpId');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#resignationpopUpId')).modal('show');
    //    $scope.getPopUpData();
    //}
}