employeeController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', 'baseService'];
function employeeController($scope, $http, $location, $rootScope, $window, baseService) {
    $scope.title = 'My Information';

    $scope.errorHide = true;
    $scope.employee = [];
    $scope.savebtndisable = false;
    // #region Objects

    $scope.employee = {
        Id: null,
        CompanyId: null,
        Code: null,
        Name: null,
        FirstName: null,
        LastName: null,
        FatherName: null,
        MotherName: null,
        DOB: null,
        DOJ: null,
        SalutationId: null,
        InitialPIN: null,
        IsFirstlogin: false,
        NewPIN: null,
        Mobile: null,
        Email: null,
        Col1: null,
        Col2: null,
        Col3: null,
        Col4: null,
        Col5: null,
        Col6: null,
        Col7: null,
        Col8: null,
        Col9: null,
        Col10: null,
        Col11: null,
        Col12: null,
        Col13: null,
        Col14: null,
        Col15: null,
        Col16: null,
        Col17: null,
        Col18: null,
        Col19: null,
        Col20: null,
        Submit: false,
        TimesSend: null,
        ReportingOfficerId: null,
        ReportingOfficerName: null,
        BirthdayCelebrationDate: null
    };

    $scope.activity = {
        Id: null,
        EmployeeId: null,
        Code: null,
        Name: null,
        ActivityDetail: null,
        PurposeOfTheActivity: null,
        ActivityCategoryId: null,
        PeriodId: null,
        Frequency: 1,
        AverageTime: null,
        ActivityImportanceId: null,
        ValueInActivity: null,
        FinancialImpact: false,
        Documents: false,
        Remarks: null,
        KPI: false
    }
    $scope.activityNew = Object.assign({}, $scope.activity);

    $scope.documentActivity = {
        Id: null,
        ActivityId: null,
        Name: null,
        FileName: null,
        DataSourceCategoryId: null,
        DocumentFormateId: null,
        ApplicationName: null,
        PreparedBy: null,
        Remarks: null,
        PreparedByInCaseOfOther: null,
        PreparedByInCaseOfOtherName: null
    }
    $scope.documentActivityNew = Object.assign({}, $scope.documentActivity);

    $scope.kpi = {
        Id: null,
        ActivityId: null,
        Name: null,
        Remarks: null,
        KPIDetail: null
    }
    $scope.kpiNew = Object.assign({}, $scope.kpi);

    // #endregion

    // #region Tab

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion

    // #region login

    $scope.LogOff = function () {
        location.reload();
    }

    $scope.Load = function () {
        $http.get('employee/getlist?id=' + $scope.employee.Id)
            .then(function (response) {
                $scope.employee = response.data;
                $rootScope.LogoOrImage = response.data.LogoFileName;
                $rootScope.EmployeeId = response.data.Id;
                $rootScope.EmployeeName = response.data.Name;
                $rootScope.CompanyId = response.data.CompanyId;
                $rootScope.CompanyMobileLength = response.data.CompanyMobileLength;
                $rootScope.CompanyGroupId = response.data.CompanyGroupId;
                $rootScope.EmployeeDocument = response.data.DocumentFolderName;
                $scope.LoadData();
                $scope.activityData();
                $scope.getActivityList($scope.employee.Id);
                $scope.getKPICboList($scope.employee.Id);
                $scope.getSalutationList();
                $scope.errorHide = true;
                $scope.errorText = '';
            });
    };
    $scope.Login = function () {
        $http({
            method: 'POST',
            url: 'employee/Login?id=' + $scope.employee.Id + '&initialpin=' + encodeURIComponent($scope.employee.NewPIN),
            dataType: 'json'
            //'Content-Type': 'application/x-www-form-urlencoded'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                $scope.errorHide = false;
                $scope.errorText = response.data.Message;
            }
            else {
                $scope.errorHide = true;
                $scope.errorText = null;
                if (!response.data.IsFirstlogin) {
                    $scope.employee.InitialPIN = '';
                    $scope.employee.NewPIN = '';
                    document.getElementById("NewPIN").focus();
                    angular.element(document.querySelector('#Confirm')).modal('show');
                    angular.element(document.querySelector('#userId')).modal('hide');
                }
                else {
                    angular.element(document.querySelector('#userId')).modal('hide');
                    $scope.Load();
                }
            }
        }, function errorCallback(response) {
            $scope.errorHide = false;
            $scope.errorText = response.data.Message;
        });
    };
    angular.element(document.querySelector('#userId')).modal('show');

    $scope.LoadData = function () {
        $http.get('employee/getdatalist?id=' + $scope.employee.Id)
            .then(function (response) {
                $scope.employees = response.data;
                var obj = $scope.employees;
                $scope.LoadDynamicData(obj);
            })
    };

    $scope.LoadDynamicData = function (obj) {
        $scope.left = '';
        $scope.right = '';
        if (obj !== null) {
            angular.forEach(obj, function (obj, i) {
                var colData = null;
                var dynamicHtml = '';
                dynamicHtml = '<input type="text" ng-model="employee.' + obj.ColumnName + '"  class="form-control" disabled>';
                if (i % 2 === 0) {
                    $scope.left += '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">' + obj.AplosColumnName + '</label>' +
                        '<div class="col-sm-8">' + dynamicHtml + '</div></div>';
                }
                else {
                    $scope.right += '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">' + obj.AplosColumnName + '</label>' +
                        '<div class="col-sm-8">' + dynamicHtml + '</div></div>';
                }
            });
        }
    };

    // #endregion

    // #region All DropDown

    $scope.getSalutationList = function () {
        $http({
            method: 'GET',
            url: 'employee/getcbolist?companyGroupId=' + $rootScope.CompanyGroupId
        }).then(function (response) {
            $scope.salutaionList = response.data;
        });
    };

    $scope.getActivityList = function () {
        $http({
            method: 'GET',
            url: 'employee/getactivitycbolist?employeeId=' + $scope.employee.Id
        }).then(function (response) {
            $scope.activitydocumentList = response.data;
            $scope.documentActivityNew.ActivityId = $scope.activityId;
        });
    };
    $scope.getKPICboList = function () {
        $http({
            method: 'GET',
            url: 'employee/getkpicbolist?employeeId=' + $scope.employee.Id
        }).then(function (response) {
            $scope.activitykpiList = response.data;
            $scope.kpiNew.ActivityId = $scope.activityId;
        });
    };

    $http({
        method: 'GET',
        url: 'employee/getactivitycategorycbolist'
    }).then(function (response) {
        $scope.activityCategoryList = response.data;
    });

    $scope.checkActivityCategory = function () {
        $scope.activityNew.OtherActivityCategory = '';
        var text = angular.element("#ActivityCategoryId :selected").text();
        if (text === 'Other (please Specify)') {
            $scope.otherActivityCategory = true;
        }
        else {
            $scope.otherActivityCategory = false;
        }
    };

    $http({
        method: 'GET',
        url: 'employee/getactivityimportancecbolist'
    }).then(function (response) {
        $scope.activityImportanceList = response.data;
    });

    $http({
        method: 'GET',
        url: 'employee/getperiodcbolist'
    }).then(function (response) {
        $scope.periodList = response.data;
    });

    $http({
        method: 'GET',
        url: 'employee/getdocumentformatecbolist'
    }).then(function (response) {
        $scope.documentFormateList = response.data;
    });

    $http({
        method: 'GET',
        url: 'employee/getdatasourcecategorycbolist'
    }).then(function (response) {
        $scope.dataSourceCategoryList = response.data;
    });

    // #endregion

    // #region Employee

    $scope.Save = function () {
        try {
            ValidationEmployee();
            if ($scope.employee.Mobile.length !== $rootScope.CompanyMobileLength) {
                throw "Mobile No must be " + $rootScope.CompanyMobileLength + " character.";
            }
            $scope.savebtndisable = true;
            $http({
                method: "post",
                url: 'employee/update',
                data: $scope.employee,
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    //$scope.employee = response.data.employee;
                    $scope.savebtndisable = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Confirm = function () {
        try {
            $http({
                method: "post",
                url: 'employee/updatepin',
                data: $scope.employee,
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    angular.element(document.querySelector('#Confirm')).modal('hide');
                    angular.element(document.querySelector('#userId')).modal('show');
                    $scope.employee.InitialPIN = '';
                    $scope.errorHide = true;
                    $scope.errorText = '';
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.disableBtn = function () {
        if ($scope.employee.Submit === false) {
            return false;
        }
        else {
            $scope.savebtndisable = true;
            return true;
        }
    };

    $scope.confirmSubmit = function () {
        $scope.confirm = $scope.employee.Id;
        $scope.message_confirmation = "Are you sure you want to submit? You won’t be able to modify your data after this.";
        angular.element(document.querySelector('#confirmSubmit')).modal('show');
    };

    $scope.confirmCloseSubmit = function () {
        $scope.confirm = $scope.employee.Id;
        angular.element(document.querySelector('#confirmSubmit')).modal('hide');
    };

    $scope.Submit = function () {
        try {
            $http({
                method: "post",
                url: 'employee/updatesubmit',
                data: $scope.employee,
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.Load();
                    $scope.employee = response.data.employee;
                    $scope.employee.Submit = true;
                }
            }, function errorCallback(response) {
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    // #endregion

    // #region Activity

    $scope._activityIndex = -1;
    $scope.activityCaption = 'Add Row';
    $scope.activityDataList = [];

    $scope.activityId = '';
    $scope.ActivityAction = "Save Activity";
    $scope.SaveActivity = function () {
        try {
            ValidationActivity();
            if ($scope.activityNew.Name === '' || $scope.activityNew.Name === null) {
                throw 'Please insert name';
            }
            if ($scope.activityNew.Frequency <= 0 && $scope.activityNew.Frequency === '' && $scope.activityNew.Frequency === null) {
                throw 'Frequency must greater than 0';
            }

            if ($scope.activityNew.AverageTime <= 0) {
                throw 'Average time must greater than 0';
            }

            var otherCategory = angular.element("#ActivityCategoryId :selected").text();
            if (otherCategory === 'Other (please Specify)') {
                if (baseService.isUndefinedOrNull($scope.activityNew.OtherActivityCategory)) {
                    throw 'Other Activity Category is required';
                }
            }

            var strName = $scope.activityNew.Name;
            var strActivityDetail = $scope.activityNew.ActivityDetail;
            var strRemarks = $scope.activityNew.Remarks;
            var strPurposeOfTheActivity = $scope.activityNew.PurposeOfTheActivity;
            var strOtherActivityCategory = $scope.activityNew.OtherActivityCategory;

            if (!baseService.isUndefinedOrNull($scope.activityNew.Name))
                $scope.activityNew.Name = strName.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.activityNew.ActivityDetail))
                $scope.activityNew.ActivityDetail = strActivityDetail.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.activityNew.Remarks))
                $scope.activityNew.Remarks = strRemarks.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.activityNew.PurposeOfTheActivity))
                $scope.activityNew.PurposeOfTheActivity = strPurposeOfTheActivity.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.activityNew.OtherActivityCategory))
                $scope.activityNew.OtherActivityCategory = strOtherActivityCategory.replace(/\s+/g, ' ');

            $scope.activityNew.EmployeeId = $scope.employee.Id;
            angular.copy($scope.activityNew, $scope.activity);
            if ($scope.ActivityAction === "Save Activity") {
                $scope.savebtndisable = true;
                $http({
                    method: "post",
                    url: 'employee/saveactivity',
                    data: $scope.activity,
                    dataType: "json"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.savebtndisable = false;
                    }
                    else {
                        $scope.savebtndisable = false;
                        $scope.clearactivity();
                        $scope.activityNew.Id = response.data.Activity.Id;
                        $scope.conVariable = 'd';
                        confirmPopUp($scope.conVariable, 'any');
                        $scope.activityData();
                        $scope.getActivityList();
                        $scope.getKPICboList();
                    }
                }, function errorCallback(response) {
                    $scope.savebtndisable = false;
                });
                return true;
            }
            else if ($scope.ActivityAction === "Update Activity") {
                $scope.savebtndisable = true;
                $http({
                    method: "post",
                    url: 'employee/saveactivity',
                    data: $scope.activity,
                    dataType: "json"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.savebtndisable = false;
                    }
                    else {
                        $scope.savebtndisable = false;
                        ShowResult(response.data.Message, "success");
                        $scope.clearactivity();
                        $scope.activityData();
                        $scope.getActivityList();
                        $scope.getKPICboList();
                    }
                }, function errorCallback(response) {
                    $scope.savebtndisable = false;
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure");
            $scope.savebtndisable = false;
        }
    };

    $scope.UpdateActivity = function (fieldName) {
        $http({
            method: "post",
            url: 'employee/updateactivity',
            data: {
                'id': $scope.activityNew.Id,
                'fieldName': fieldName
            },
            dataType: "json"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.activityData();
                $scope.getActivityList();
                $scope.getKPICboList();
            }
        }, function errorCallback(response) {
        });
    };

    $scope.activityDataParameters = {
        limit: 50,
        offset: 0,
        order: 'asc',
        sort: 'Name',
        searchBy: null,
        pageSize: 50,
        total_count: 0,
        search: 'Name',
        serverPagination: true
    };

    $scope.activityData = function (pageno) {
        baseService.paginationBase('employee/getactivitylist?employeeId=' + $scope.employee.Id, pageno, $scope.activityDataParameters)
            .then(function (result) {
                $scope.activityDataList = result.Rows;
                $scope.activityDataParameters.total_count = result.total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.activityData();
    $scope.hidediv = false;
    $scope.activityEdit = function (data, index) {
        $scope.activityNew = Object.assign({}, data);
        $scope.activityNew.ActivityCategoryId = $scope.activityNew.ActivityCategoryId.toString();
        $scope.activityNew.PeriodId = $scope.activityNew.PeriodId.toString();
        $scope.activityNew.ActivityImportanceId = $scope.activityNew.ActivityImportanceId.toString();

        $scope.activityNew.ActivityCategoryId = data.ActivityCategoryId.toString();
        var text = $scope.activityNew.ActivityCategoryId;
        if (text === '6') {
            $scope.otherActivityCategory = true;
        }
        else {
            $scope.otherActivityCategory = false;
        }

        $scope._activityIndex = index;
        $scope.ActivityAction = "Update Activity";
        $scope.hidediv = true;
    };

    $scope.DeleteActivity = function () {
        try {
            if ($scope.activityNew.Id === null || $scope.activityNew.Id === '') {
                $scope.activityDataList.splice($scope._activityIndex, 1);
                $scope._activityIndex = -1;
            }
            else {
                $http({
                    method: 'POST',
                    url: 'employee/activitydelete',
                    dataType: 'JSON',
                    data: { 'id': $scope.activityNew.Id }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector('#confirmactivityDeletePopUp')).modal('hide');
                        deletDeleted($scope.activityNew.Id, $scope.activityDataList);
                        $scope.activityData();
                        $scope.getActivityList($scope.employee.Id);
                        $scope.getKPICboList($scope.employee.Id);
                        $scope.clearactivity();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.activityDelete = function (data, index) {
        $scope.activityNew.Id = data.Id;
        $scope._activityIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.Name + ' ]?';
        angular.element(document.querySelector('#confirmactivityDeletePopUp')).modal('show');
    };

    function deletDeleted(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id == id) {
                list.splice(i, 1);
            }
        }
    };

    $scope.removeActivityRow = function () {
        angular.element(document.querySelector('#confirmactivityDeletePopUp')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.activityNew.Id)) {
            deletDeleted($scope.activityNew.Id, $scope.activityDataList);
        }
        else {
            $scope.DeleteActivity();
        }
    };

    $scope.clearactivity = function () {
        $scope.activity = {};
        $scope.activityNew = {};
        $scope._activityIndex = -1;
        $scope.ActivityAction = "Save Activity";
        $scope.activityNew.Documents = false;
        $scope.activityNew.KPI = false;
        $scope.activityNew.Frequency = 1;
        $scope.hidediv = false;
        $scope.otherActivityCategory = false;
        $scope.kpiList = [];
        $scope.documentsDataList = [];
        $scope.savebtndisable = false;
    };

    $scope.confirmDocument = function () {
        $scope.message_confirmation = "Does this activity have any document?";
        angular.element(document.querySelector('#document')).modal('show');
    };

    $scope.confirmCloseactivityDelete = function () {
        angular.element(document.querySelector('#confirmactivityDeletePopUp')).modal('hide');
    };

    // #endregion

    // #region DocumentActivity

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = 'Documents/' + $rootScope.EmployeeDocument + '/' + data.FileId + extention;
    };

    $scope.preparedbyOther = function () {
        var text = angular.element("#PreparedById :selected").text();
        if (text === 'Other') {
            $scope.preparebyOther = true;
        }
        else {
            $scope.preparebyOther = false;
        }
    };

    $scope.preparedbyOtherpop = function () {
        var text = angular.element("#PreparedBy :selected").text();
        if (text === 'Other') {
            $scope.preparebyOtherpop = true;
        }
        else {
            $scope.preparebyOtherpop = false;
        }
    };

    $scope._documentIndex = -1;
    $scope.fileList = [];
    $scope.fileId = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };

    $scope.filedata = null;
    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });
    $("#uploadBtn2").change(function () {
        $scope.filedata = this.files[0];
    });

    $scope.documentData = function () {
        $http.get('employee/getdocumentactivitylist?activityId=' + $scope.documentActivityNew.ActivityId)
            .then(function (response) {
                $scope.documentsDataList = response.data;
            })
    };
    $scope.DocumentAction = 'Save Document';
    $scope.documentEdit = function (data, index) {
        $scope.documentActivityNew = Object.assign({}, data);
        $scope.documentActivityNew.ActivityId = $scope.documentActivityNew.ActivityId.toString();
        $scope.documentActivityNew.DocumentFormateId = $scope.documentActivityNew.DocumentFormateId.toString();
        $scope.documentActivityNew.DataSourceCategoryId = $scope.documentActivityNew.DataSourceCategoryId.toString();
        $scope.documentActivityNew.PreparedBy = data.PreparedBy;
        $scope.DocumentAction = 'Update Document';
        $scope.documentActivityNew.FileName = data.FileName;
        var filename = document.getElementById("uploadFile").value = data.FileName;

        if ($scope.documentActivityNew.PreparedBy === 'Other') {
            $scope.preparebyOther = true;
        }
        else {
            $scope.preparebyOther = false;
        }

        if (!baseService.isUndefinedOrNull(data.FileName))
            $scope.filedata.name = data.FileName;
        else
            $scope.filedata = null;

        $scope._documentIndex = index;
    };

    $scope.getFile = function () {
    };
    $scope.documentId = '';

    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };
    document.getElementById("uploadBtn2").onchange = function () {
        var filename = document.getElementById("uploadFile2").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile2").value = res;
    };

    $scope.SaveDocument = function () {
        try {
            var preparedBy = angular.element("#PreparedBy :selected").text();
            if (preparedBy === 'Other') {
                if (baseService.isUndefinedOrNull($scope.documentActivityNew.PreparedByInCaseOfOther)) {
                    throw "Prepared By InCaseOf Other is required."
                }
            }

            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = '';
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.documentActivityNew.FileName = fileName;

            $scope.documentActivityNew.FileId = $scope.fileId();
            ValidationDocument();
            $scope.documentActivityNew.ActivityId = $scope.activityNew.Id;
            $scope.documentActivityNew.EmployeeId = $scope.employee.Id;
            var formData = new FormData();
            $scope.savebtndisable = true;

            var strName = $scope.documentActivityNew.Name;
            var strRemarks = $scope.documentActivityNew.Remarks;
            var strApplicationName = $scope.documentActivityNew.ApplicationName;

            if (!baseService.isUndefinedOrNull($scope.documentActivityNew.Name))
                $scope.documentActivityNew.Name = strName.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.documentActivityNew.Remarks))
                $scope.documentActivityNew.Remarks = strRemarks.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.documentActivityNew.ApplicationName))
                $scope.documentActivityNew.ApplicationName = strApplicationName.replace(/\s+/g, ' ');

            $http({
                method: "post",
                url: 'employee/create',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("documentActivityNew", angular.toJson(data.documentActivityNew));
                    if (baseService.isUndefinedOrNull($scope.filedata) == false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'documentActivityNew': $scope.documentActivityNew, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure", 'documentPopUp');
                    $scope.savebtndisable = false;
                }
                else {
                    $scope.savebtndisable = false;
                    ShowResult(response.data.Message, "success", 'documentPopUp');
                    $scope.documentId = response.data.DocumentActivity.Id;
                    angular.element(document.querySelector('#documentPopUp')).modal('hide');
                    $scope.conVariable = 'd';
                    confirmPopUp($scope.conVariable, 'more');
                    $scope.activityData();
                    $scope.getActivityList();
                    $scope.documentActivityNew.ActivityId = $scope.activityNew.Id;
                    cleardocumentActivity();
                }
            }, function errorCallback(response) {
                $scope.savebtndisable = false;
            });
        } catch (e) {
            ShowResult(e, "failure", 'documentPopUp');
            $scope.savebtndisable = false;
        }
    };

    $scope.confirmMoreDocument = function () {
        $scope.message_confirmation = "Do you have more document for this activity?";
        angular.element(document.querySelector('#document')).modal('show');
    };

    $scope.DeleteDocument = function () {
        try {
            if ($scope.documentActivityNew.Id == null || $scope.documentActivityNew.Id == '') {
                $scope.documentsDataList.splice($scope._documentIndex, 1);
                $scope._documentIndex = -1;
            }
            else {
                $http({
                    method: 'POST',
                    url: 'employee/deletedocument',
                    dataType: 'JSON',
                    data: { 'id': $scope.documentActivityNew.Id }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
                        deleteDeleted($scope.documentActivityNew.Id, $scope.documentsDataList);
                        $scope.documentData();
                        $scope.cleardocumentActivitybody();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.documentDelete = function (data, index) {
        $scope.documentActivityNew.Id = data.Id;
        $scope._documentIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.Name + ' ]?';
        angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('show');
    };

    function deleteDeleted(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id == id) {
                list.splice(i, 1);
            }
        }
    };

    $scope.removeDocumentRow = function () {
        angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.documentActivityNew.Id)) {
            deleteDeleted($scope.documentActivityNew.Id, $scope.documentsDataList);
        }
        else {
            $scope.DeleteDocument();
        }
    };

    $scope.confirmClosedocumentDelete = function () {
        angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
    };

    $scope.documentRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmdocDelete')).modal('show');
    };

    $scope.removeDocument = function () {
        angular.element(document.querySelector('#confirmdocDelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.documentActivityNew.FileName)) {
            document.getElementById('uploadBtn').value = '';
            document.getElementById('uploadFile').value = "";
            $scope.filedata = null;
        }
        else {
            $scope.ClearDoc();
        }
    };

    $scope.confirmClosedocDelete = function () {
        angular.element(document.querySelector('#confirmdocDelete')).modal('hide');
    };

    $scope.ClearDoc = function () {
        document.getElementById('uploadBtn').value = '';
        $scope.filedata = '';
        $scope.documentActivityNew.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
        $scope.UpdateDoc();
    };

    $scope.UpdateDoc = function () {
        try {
            var preparedBy = angular.element("#PreparedBy :selected").text();
            if (preparedBy === 'Other') {
                if (baseService.isUndefinedOrNull($scope.documentActivityNew.PreparedByInCaseOfOther)) {
                    throw "Prepared By InCaseOf Other is required."
                }
            }

            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = '';
            if (!baseService.isUndefinedOrNull($scope.filedata)) {
                fileName = $scope.filedata.name;
                $scope.documentActivityNew.FileName = fileName;
            }
            $scope.documentActivityNew.EmployeeId = $scope.employee.Id;
            var formData = new FormData();
            $http({
                method: "post",
                url: 'employee/detachdocument',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("documentActivityNew", angular.toJson(data.documentActivityNew));
                    if (baseService.isUndefinedOrNull($scope.filedata) == false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'documentActivityNew': $scope.documentActivityNew, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.activityData();
                    $scope.documentData();
                }
            }, function errorCallback(response) {
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.UpdateDocument = function () {
        try {
            var preparedBy = angular.element("#PreparedBy :selected").text();
            if (preparedBy === 'Other') {
                if (baseService.isUndefinedOrNull($scope.documentActivityNew.PreparedByInCaseOfOther)) {
                    throw "Prepared By InCaseOf Other is required."
                }
            }

            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = '';
            if (!baseService.isUndefinedOrNull($scope.filedata)) {
                fileName = $scope.filedata.name;
                $scope.documentActivityNew.FileName = fileName;
            }
            ValidationUpdateDocument();
            $scope.documentActivityNew.EmployeeId = $scope.employee.Id;
            var formData = new FormData();
            $scope.savebtndisable = true;

            var strName = $scope.documentActivityNew.Name;
            var strRemarks = $scope.documentActivityNew.Remarks;
            var strApplicationName = $scope.documentActivityNew.ApplicationName;

            if (!baseService.isUndefinedOrNull($scope.documentActivityNew.Name))
                $scope.documentActivityNew.Name = strName.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.documentActivityNew.Remarks))
                $scope.documentActivityNew.Remarks = strRemarks.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.documentActivityNew.ApplicationName))
                $scope.documentActivityNew.ApplicationName = strApplicationName.replace(/\s+/g, ' ');

            $http({
                method: "post",
                url: 'employee/create',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("documentActivityNew", angular.toJson(data.documentActivityNew));
                    if (baseService.isUndefinedOrNull($scope.filedata) == false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'documentActivityNew': $scope.documentActivityNew, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                    $scope.savebtndisable = false;
                }
                else {
                    $scope.savebtndisable = false;
                    ShowResult(response.data.Message, "success");
                    $scope.cleardocumentActivitybody();
                    $scope.activityData();
                    $scope.documentData();
                    $scope.DocumentAction = 'Save Document';
                }
            }, function errorCallback(response) {
                $scope.savebtndisable = false;
            });
        } catch (e) {
            ShowResult(e, "failure");
            $scope.savebtndisable = false;
        }
    };

    $scope.cleardocumentActivitybody = function () {
        $scope.ActivityId = $scope.documentActivityNew.ActivityId;
        $scope.documentActivity = {};
        $scope.documentActivityNew = {};
        $scope.documentActivityNew.ActivityId = $scope.ActivityId;
        $scope._documentIndex = -1;
        $scope.preparebyOther = false;
        $scope.preparebyOtherpop = false;
        document.getElementById('uploadBtn').value = '';
        document.getElementById('uploadBtn2').value = '';
        $scope.filedata = '';
        $scope.documentActivityNew.FileName = "";
        document.getElementById('uploadFile').value = "";
        document.getElementById('uploadFile2').value = "";
        $scope.savebtndisable = false;
        $scope.DocumentAction = 'Save Document';
    };

    function cleardocumentActivity() {
        $scope.documentActivity = {};
        $scope.documentActivityNew = {};
        $scope._documentIndex = -1;
        $scope.documentData();
        ClearImage();
        $scope.savebtndisable = false;
    };

    function ClearImage() {
        document.getElementById('uploadBtn').value = '';
        document.getElementById('uploadBtn2').value = '';
        $scope.filedata = '';
        $scope.documentActivityNew.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
        document.getElementById('uploadFile2').value = "";
    };

    $scope.ClearFile = function () {
        document.getElementById('uploadBtn2').value = '';
        $scope.filedata = '';
        $scope.documentActivityNew.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile2').value = "";
    };

    // #endregion

    // #region KPI

    $scope._kpiIndex = -1;

    $scope.SaveKPI = function () {
        try {
            ValidationKPI();
            $scope.kpiNew.ActivityId = $scope.activityNew.Id;
            $scope.kpiNew.EmployeeId = $scope.employee.Id;
            $scope.savebtndisable = true;

            var strName = $scope.kpiNew.Name;
            var strRemarks = $scope.kpiNew.Remarks;
            var strKPIDetail = $scope.kpiNew.KPIDetail;

            if (!baseService.isUndefinedOrNull($scope.kpiNew.Name))
                $scope.kpiNew.Name = strName.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.kpiNew.Remarks))
                $scope.kpiNew.Remarks = strRemarks.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.kpiNew.KPIDetail))
                $scope.kpiNew.KPIDetail = strKPIDetail.replace(/\s+/g, ' ');

            $http({
                method: "post",
                url: 'employee/savekpi',
                data: $scope.kpiNew,
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure", 'kpiPopUp');
                    $scope.savebtndisable = false;
                }
                else {
                    $scope.savebtndisable = false;
                    ShowResult(response.data.Message, "success", 'kpiPopUp');
                    $scope.kpiList = [];
                    clearkpi();
                    angular.element(document.querySelector('#kpiPopUp')).modal('hide');
                    $scope.conVariable = 'k';
                    confirmPopUp($scope.conVariable, 'more');
                    $scope.getKPICboList();
                    $scope.kpiNew.ActivityId = $scope.activityNew.Id;
                    $scope.activityData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
                $scope.savebtndisable = false;
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure", 'kpiPopUp');
            $scope.savebtndisable = false;
        }
    };

    $scope.kpiData = function () {
        $http.get('employee/getkpilist?activityId=' + $scope.kpiNew.ActivityId)
            .then(function (response) {
                $scope.kpiList = response.data;
            })
    };

    $scope.KPIAction = "Save KPI";
    $scope.kpiEdit = function (data, index) {
        $scope.kpiNew = Object.assign({}, data);
        $scope._kpiIndex = index;
        $scope.KPIAction = "Update KPI";
    };

    $scope.confirmKpi = function () {
        if ($scope.conVariable === 'd') {
            $scope.conVariable = 'k';
            confirmPopUp($scope.conVariable, 'any');
        }
        else {
            $scope.conVariable = '';
            angular.element(document.querySelector('#document')).modal('hide');
            angular.element(document.querySelector('#documentPopUp')).modal('hide');
            angular.element(document.querySelector('#kpiPopUp')).modal('hide');
            $scope.documentId = null;
            $scope.activityNew.Id = null;
        }
    };

    function confirmPopUp(d, msg) {
        var message = '';
        if (d !== null || d !== undefined) {
            if (d === 'd') {
                if (!baseService.isUndefinedOrNull($scope.documentId))
                    message = 'Document Created : [' + $scope.documentId + ']<br />';
                $scope.message_confirmation = message + 'Does this activity have <b>' + msg + '</b> Document?';
            }
            else
                $scope.message_confirmation = 'Does this activity have <b>' + msg + '</b> KPI?';
            angular.element(document.querySelector('#documentPopUp')).modal('hide');
            angular.element(document.querySelector('#document')).modal('show');
        }
    };

    $scope.showDocument = function () {
        docOrKpiPopUp($scope.conVariable);
    };

    function docOrKpiPopUp(d) {
        if (d === 'd') {
            $scope.UpdateActivity('IsDocument')
            angular.element(document.querySelector('#documentPopUp')).modal('show');
        }
        else {
            $scope.UpdateActivity('IsKpi');
            angular.element(document.querySelector('#kpiPopUp')).modal('show');
        }
    };
    $scope.docOrKpi = '';

    $scope.showKPI = function () {
        $scope.docOrKpi = 'KPI';
        angular.element(document.querySelector('#kpiPopUp')).modal('show');
        angular.element(document.querySelector('#document')).modal('hide');
    };

    $scope.hideKPI = function () {
        $scope.docOrKpi = '';
        angular.element(document.querySelector('#kpiPopUp')).modal('hide');
    };

    $scope.confirmMoreKPI = function () {
        $scope.message_confirmation = "Do you have <b>more<b/> kpi for this activity?";
        angular.element(document.querySelector('#kpiPopUp')).modal('show');
        angular.element(document.querySelector('#document')).modal('hide');
    };

    $scope.closeKpi = function () {
        $scope.docOrKpi = 'KPI';
        angular.element(document.querySelector('#kpi')).modal('hide');
    };

    // #region DeleteKPI

    $scope.DeleteKpi = function () {
        try {
            if ($scope.kpiNew.Id == null || $scope.kpiNew.Id == '') {
                $scope.kpiList.splice($scope._kpiIndex, 1);
                $scope._kpiIndex = -1;
            }
            else {
                $http({
                    method: 'POST',
                    url: 'employee/deletekpi',
                    dataType: 'JSON',
                    data: { 'id': $scope.kpiNew.Id }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector('#confirmkpiDeletePopUp')).modal('hide');
                        deletekpiDeleted($scope.kpiNew.Id, $scope.kpiList);
                        $scope.kpiData();
                        $scope.clearkpibody();
                        $scope.kpiNew.Id == null;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.kpiDelete = function (data, index) {
        $scope.kpiNew.Id = data.Id;
        $scope._kpiIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.Name + ' ]?';
        angular.element(document.querySelector('#confirmkpiDeletePopUp')).modal('show');
    };

    function deletekpiDeleted(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id == id) {
                list.splice(i, 1);
            }
        }
    };

    $scope.removeRow = function () {
        angular.element(document.querySelector('#confirmkpiDeletePopUp')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.kpiNew.Id)) {
            deletekpiDeleted($scope.kpiNew.Id, $scope.kpiList);
        }
        else {
            $scope.DeleteKpi();
        }
    };

    $scope.confirmClosekpiDelete = function () {
        angular.element(document.querySelector('#confirmkpiDeletePopUp')).modal('hide');
    };
    // #endregion

    $scope.UpdateKPI = function () {
        $scope.kpiNew.EmployeeId = $scope.employee.Id;
        try {
            ValidationUpdateKPI();
            $scope.savebtndisable = true;

            var strName = $scope.kpiNew.Name;
            var strRemarks = $scope.kpiNew.Remarks;
            var strKPIDetail = $scope.kpiNew.KPIDetail;

            if (!baseService.isUndefinedOrNull($scope.kpiNew.Name))
                $scope.kpiNew.Name = strName.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.kpiNew.Remarks))
                $scope.kpiNew.Remarks = strRemarks.replace(/\s+/g, ' ');

            if (!baseService.isUndefinedOrNull($scope.kpiNew.KPIDetail))
                $scope.kpiNew.KPIDetail = strKPIDetail.replace(/\s+/g, ' ');

            $http({
                method: "post",
                url: 'employee/savekpi',
                data: $scope.kpiNew,
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                    $scope.savebtndisable = false;
                }
                else {
                    $scope.savebtndisable = false;
                    ShowResult(response.data.Message, "success");
                    $scope.kpiList = [];
                    $scope.clearkpibody();
                    //$scope.kpiNew.ActivityId = response.data.KPI.ActivityId;
                    $scope.activityData();
                    $scope.kpiData();
                    $scope.kpiNew.Id = null;
                    $scope.KPIAction = "Save KPI";
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
                $scope.savebtndisable = false;
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function clearkpi() {
        $scope.ActivityId = $scope.kpiNew.ActivityId;
        $scope.kpi = {};
        $scope.kpiNew = {};
        $scope.kpiNew.ActivityId = $scope.ActivityId;
        $scope._kpiIndex = -1;
        $scope.savebtndisable = false;
    };

    $scope.clearkpibody = function () {
        $scope.ActivityId = $scope.kpiNew.ActivityId;
        $scope.kpi = {};
        $scope.kpiNew = {};
        $scope.kpiNew.ActivityId = $scope.ActivityId;
        $scope._kpiIndex = -1;
        $scope.savebtndisable = false;
        $scope.KPIAction = "Save KPI";
    };

    // #endregion

    // #region Reporting Officer & Prepared Modal

    $scope.searchbyEmployeelist = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Full Name',
            'value': 'Name'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'Last Name',
            'value': 'LastName'
        }
    ];
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Name',
        searchBy: 'Name',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getEmployeeData = function () {
        try {
            baseService.setCurrentPage('employeeData');
            $scope.loadEmployeeData = function (pageno) {
                baseService.paginationBase('employee/getnamelist?companyGroupId=' + $rootScope.CompanyGroupId + '&id=' + $scope.employee.Id, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeData = result.Rows;
                        $scope.employeeParameters.total_count = result.total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeemodal')).modal('show');
            $scope.loadEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.getEmployeeCode = function (ob) {
        $scope.employee.ReportingOfficerId = ob.Id;
        $scope.employee.ReportingOfficerName = ob.Name;

        angular.element(document.querySelector('#employeemodal')).modal('hide');
    };
    $scope.clearEmployeeCode = function () {
        $scope.employee.ReportingOfficerId = null;
        $scope.employee.ReportingOfficerName = null;
    };

    $scope.preparedParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Name',
        searchBy: 'Name',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getPreparedData = function () {
        try {
            baseService.setCurrentPage('employeeDatas');
            $scope.loadPreparedData = function (pageno) {
                baseService.paginationBase('employee/getnamelist?companyGroupId=' + $rootScope.CompanyGroupId + '&id=' + $scope.employee.Id, pageno, $scope.preparedParameters)
                    .then(function (result) {
                        $scope.employeeDatas = result.Rows;
                        $scope.preparedParameters.total_count = result.total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#Prepared')).modal('show');
            $scope.loadPreparedData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getPreparedByCode = function (ob) {
        $scope.documentActivityNew.PreparedByInCaseOfOther = ob.Id;
        $scope.documentActivityNew.PreparedByInCaseOfOtherName = ob.Name;
        angular.element(document.querySelector('#Prepared')).modal('hide');
    };

    $scope.clearPreparedByCode = function () {
        $scope.documentActivityNew.PreparedByInCaseOfOther = null;
        $scope.documentActivityNew.PreparedByInCaseOfOtherName = null;
    };

    // #endregion

    // #region Clear

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue == null || fieldValue == '') {
                throw ('[' + fieldName + '] is required...')
            }
        } catch (e) {
            throw e;
        }
    };

    function ValidationEmployee() {
        try {
            CheckField($scope.employee.SalutationId, "Salutation");
            CheckField($scope.employee.FirstName, "First Name");
            CheckField($scope.employee.LastName, "Last Name");
            //CheckField($scope.employee.Email, "Email");
            CheckField($scope.employee.Mobile, "Mobile");
            CheckField($scope.employee.BirthdayCelebrationDate, "Birthday Celebration Date");
        } catch (e) {
            throw e;
        }
    };

    function ValidationActivity() {
        try {
            CheckField($scope.activityNew.Name, "Activity");
            CheckField($scope.activityNew.ActivityDetail, "Activity Detail");
            CheckField($scope.activityNew.ActivityCategoryId, "Activity Category");
            CheckField($scope.activityNew.ActivityImportanceId, "Activity Importance");
            CheckField($scope.activityNew.PeriodId, "Period");
            CheckField($scope.activityNew.Frequency, "Frequency");
            CheckField($scope.activityNew.AverageTime, "Average Time");
            CheckField($scope.activityNew.ValueInActivity, "Find Value In This Activity");
            CheckField($scope.activityNew.PurposeOfTheActivity, "Purpose of the activity");
        } catch (e) {
            throw e;
        }
    };

    function ValidationDocument() {
        try {
            CheckField($scope.documentActivityNew.Name, "Description");
            CheckField($scope.documentActivityNew.DocumentFormateId, "Document Format");
            CheckField($scope.documentActivityNew.DataSourceCategoryId, "Data Source Category");
            CheckField($scope.documentActivityNew.PreparedBy, "Prepared by");
        } catch (e) {
            throw e;
        }
    };

    function ValidationUpdateDocument() {
        try {
            CheckField($scope.documentActivityNew.ActivityId, "Activity ");
            CheckField($scope.documentActivityNew.DocumentFormateId, "Document Format");
            CheckField($scope.documentActivityNew.Name, "Description");
            CheckField($scope.documentActivityNew.DataSourceCategoryId, "Data Source Category");
            CheckField($scope.documentActivityNew.PreparedBy, "Prepared by");
        } catch (e) {
            throw e;
        }
    };

    function ValidationKPI() {
        try {
            CheckField($scope.kpiNew.Name, "Name");
            CheckField($scope.kpiNew.KPIDetail, "KPI Detail");
        } catch (e) {
            throw e;
        }
    };

    function ValidationUpdateKPI() {
        try {
            CheckField($scope.kpiNew.ActivityId, "Activity ");
            CheckField($scope.kpiNew.Name, "Name");
            CheckField($scope.kpiNew.KPIDetail, "KPI Detail");
        } catch (e) {
            throw e;
        }
    };

    // #endregion

    // #region Report
    $scope.ReportParam = {
        CompanyGroupId: null,
        EmployeeName: null,
        withoutactivity: false,
        notloggedin: false,
        Submitted: false,
        NotSubmitted: false
    };

    $scope.GetIndividualInfo = function () {
        try {
            //$cookies.get('CompanyGroupId')
            $scope.ReportParam.CompanyGroupId = $rootScope.CompanyGroupId;
            $scope.ReportParam.EmployeeName = $rootScope.EmployeeName;
            location.href = "report/IndividualStatus?cg=" + $scope.ReportParam.CompanyGroupId + "&un=" + $scope.ReportParam.EmployeeName + "&uid=" + $rootScope.EmployeeId + "";
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
};